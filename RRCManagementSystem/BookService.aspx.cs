    using System;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls; // RadioButtonList

    namespace RRCManagementSystem
    {
        public partial class BookService : System.Web.UI.Page
        {
            private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

            protected void Page_Load(object sender, EventArgs e)
            {
                if (!IsPostBack && Session["ClientID"] != null)
                {
                    LoadQuotation();
                }
                else if (Session["ClientID"] == null)
                {
                    Response.Redirect("~/Login.aspx");
                }
            }

            private void LoadQuotation()
            {
                int clientId = Convert.ToInt32(Session["ClientID"]);

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.usp_PendingQuotation_GetLatestByClient", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lblServices.Text = SafeGetString(reader, "ServiceNames", "N/A");
                            lblSQM.Text = SafeGetString(reader, "SQM", "0");

                            decimal price = 0m;
                            int priceIdx = SafeOrdinal(reader, "Price");
                            if (priceIdx >= 0 && !reader.IsDBNull(priceIdx))
                                price = Convert.ToDecimal(reader["Price"]);
                            lblPrice.Text = $"\u20B1{price:N2}";

                            // contract detection from either PQ.IsContract or ServiceType == 'Termite Control'
                            bool isContractCol = SafeGetBool(reader, "IsContract", false);
                            string serviceType = SafeGetString(reader, "ServiceType", null);
                            bool isTermiteType = string.Equals(serviceType, "Termite Control", StringComparison.OrdinalIgnoreCase);
                            bool isContractFinal = isTermiteType || isContractCol;

                            hfIsContract.Value = isContractFinal ? "True" : "False";
                        hfQuotationID.Value = SafeGetString(reader, "PendingQuotationID", null);


                        // For UI only
                        ViewState["ServiceIDs"] = SafeGetString(reader, "ServiceID", string.Empty);

                            lblInspector.Text = SafeGetString(reader, "InspectorName", "N/A");
                            btnBook.Enabled = true;

                            // Show/hide plan selector; preselect default for contracts
                            var rblPlan = FindControl("rblPlan") as RadioButtonList;
                            var planPanel = FindControl("pnlPlan"); // optional wrapper
                            if (rblPlan != null)
                            {
                                if (isContractFinal)
                                {
                                    rblPlan.Visible = true;
                                    if (planPanel != null) planPanel.Visible = true;

                                    // Preselect 50-25-25 by default if nothing selected yet
                                    if (rblPlan.SelectedIndex < 0)
                                    {
                                        var item = rblPlan.Items.FindByValue("50-25-25");
                                        if (item != null) item.Selected = true;
                                    }
                                }
                                else
                                {
                                    rblPlan.ClearSelection();
                                    rblPlan.Visible = false;
                                    if (planPanel != null) planPanel.Visible = false;
                                }
                            }
                        }
                        else
                        {
                            btnBook.Enabled = false;
                            ScriptManager.RegisterStartupScript(this, GetType(), "noQuote", @"
                                Swal.fire('No Quotation', 
                                          'Please wait for the inspector to create a quotation.', 
                                          'info');", true);
                        }
                    }
                }
            }

            protected void btnBook_Click(object sender, EventArgs e)
            {
                // Basic validation
                if (string.IsNullOrWhiteSpace(txtDate.Text) || string.IsNullOrWhiteSpace(txtTime.Text))
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "missing",
                        "Swal.fire('Missing Info', 'Please select a preferred date and time.', 'warning');", true);
                    return;
                }

                // Parse date/time flexibly
                DateTime selectedDateTime;
                if (!DateTime.TryParse($"{txtDate.Text} {txtTime.Text}", CultureInfo.CurrentCulture, DateTimeStyles.None, out selectedDateTime) &&
                    !DateTime.TryParse($"{txtDate.Text} {txtTime.Text}", CultureInfo.GetCultureInfo("en-PH"), DateTimeStyles.None, out selectedDateTime))
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "invalid",
                        "Swal.fire('Invalid Input', 'Invalid date or time format.', 'error');", true);
                    return;
                }

                if (selectedDateTime < DateTime.Now)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "pastDate",
                        "Swal.fire('Invalid Schedule', 'Please choose a future date and time.', 'error');", true);
                    return;
                }

                int quotationId;
                if (!int.TryParse(hfQuotationID.Value, out quotationId) || quotationId <= 0)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "noQuoteId",
                        "Swal.fire('Error', 'Missing quotation reference.', 'error');", true);
                    return;
                }

                if (Session["ClientID"] == null)
                {
                    Response.Redirect("~/Login.aspx");
                    return;
                }

                int clientId = Convert.ToInt32(Session["ClientID"]);
                string notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim();

                bool isContract = string.Equals(hfIsContract.Value, "True", StringComparison.OrdinalIgnoreCase);

                // Determine plan to send
                string planToSend = null;
                var rblPlan = FindControl("rblPlan") as RadioButtonList;

                if (isContract)
                {
                    if (rblPlan != null && rblPlan.Visible)
                    {
                        // Use selection; if none, fall back to default 50-25-25
                        planToSend = string.IsNullOrWhiteSpace(rblPlan.SelectedValue) ? "50-25-25" : rblPlan.SelectedValue;
                    }
                    else
                    {
                        // Control not on the page → default to 50-25-25 as requested
                        planToSend = "50-25-25";
                    }
                }
                // else: non-contract → planToSend remains null (proc will set 100%)

                int newBookingId = 0;
                string newBookingCode = null;

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.usp_BookService_ConfirmFromQuotation", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@PendingQuotationID", SqlDbType.Int).Value = quotationId;
                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                    cmd.Parameters.Add("@ScheduledDate", SqlDbType.Date).Value = selectedDateTime.Date;
                    cmd.Parameters.Add("@StartTime", SqlDbType.Time).Value = selectedDateTime.TimeOfDay;
                    cmd.Parameters.Add("@Notes", SqlDbType.NVarChar, 500).Value = (object)notes ?? DBNull.Value;

                    // Pass plan only for contract services; proc validates/forces 100% for non-contract
                    var pPlan = cmd.Parameters.Add("@PaymentPlan", SqlDbType.NVarChar, 20);
                    if (isContract)
                        pPlan.Value = planToSend;
                    else
                        pPlan.Value = DBNull.Value;

                    var pIdOut = cmd.Parameters.Add("@BookingID", SqlDbType.Int);
                    pIdOut.Direction = ParameterDirection.Output;

                    var pCodeOut = cmd.Parameters.Add("@OutBookingCode", SqlDbType.NVarChar, 16); // align with proc/DB
                    pCodeOut.Direction = ParameterDirection.Output;

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();

                        if (pIdOut.Value != DBNull.Value) newBookingId = Convert.ToInt32(pIdOut.Value);
                        if (pCodeOut.Value != DBNull.Value) newBookingCode = pCodeOut.Value as string;
                    }
                    catch (SqlException ex)
                    {
                        ScriptManager.RegisterStartupScript(this, GetType(), "sqlErr",
                            "Swal.fire('Error', 'Failed to create booking: " + ex.Message.Replace("'", "\\'") + "', 'error');", true);
                        return;
                    }
                }

                if (newBookingId > 0)
                {
                    string safeCode = string.IsNullOrWhiteSpace(newBookingCode) ? "" : $" (Code: {newBookingCode})";
                    ScriptManager.RegisterStartupScript(this, GetType(), "booked",
                        $"Swal.fire('Success', 'Your service has been booked!{safeCode}', 'success');", true);

                    // Reset form
                    txtDate.Text = "";
                    txtTime.Text = "";
                    txtNotes.Text = "";

                    // Optional: redirect to bookings
                    // Response.Redirect(\"~/MyBookings.aspx\");
                }
                else
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "fail",
                        "Swal.fire('Error', 'No booking was created. Please try again.', 'error');", true);
                }
            }

            // ---------- Safe readers ----------
            private static int SafeOrdinal(IDataRecord r, string column)
            {
                try { return r.GetOrdinal(column); } catch { return -1; }
            }

            private static string SafeGetString(IDataRecord r, string column, string fallback)
            {
                int i = SafeOrdinal(r, column);
                if (i < 0 || r.IsDBNull(i)) return fallback;
                return Convert.ToString(r[i]);
            }

            private static bool SafeGetBool(IDataRecord r, string column, bool fallback)
            {
                int i = SafeOrdinal(r, column);
                if (i < 0 || r.IsDBNull(i)) return fallback;
                object v = r[i];
                if (v is bool b) return b;
                if (v is int ii) return ii != 0;
                if (bool.TryParse(Convert.ToString(v), out bool parsed)) return parsed;
                return fallback;
            }
        }
    }
