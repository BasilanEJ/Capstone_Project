using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

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
                        lblServices.Text = reader["ServiceNames"].ToString();
                        lblSQM.Text = reader["SQM"] == DBNull.Value ? "0" : reader["SQM"].ToString();

                        decimal price = reader["Price"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["Price"]);
                        lblPrice.Text = $"\u20B1{price:N2}";

                        hfIsContract.Value = reader["IsContract"] != DBNull.Value && (bool)reader["IsContract"] ? "True" : "False";
                        hfQuotationID.Value = reader["QuotationID"].ToString();

                        // We keep this for your UI logic, but booking will read ServiceIDs from the quotation inside SQL.
                        ViewState["ServiceIDs"] = reader["ServiceID"]?.ToString() ?? string.Empty;

                        lblInspector.Text = reader["InspectorName"]?.ToString() ?? "N/A";
                    }
                    else
                    {
                        ScriptManager.RegisterStartupScript(this, GetType(), "noQuote", @"
                            Swal.fire('No Quotation', 
                                      'Please wait for the inspector to create a quotation.', 
                                      'info');", true);
                        btnBook.Enabled = false;
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

            DateTime selectedDateTime;
            if (!DateTime.TryParse(txtDate.Text + " " + txtTime.Text, out selectedDateTime))
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

            int clientId = Convert.ToInt32(Session["ClientID"]);
            string notes = (txtNotes.Text == null) ? null : txtNotes.Text.Trim();

            // Call single, atomic stored procedure that:
            // - Validates the quotation belongs to this client
            // - Creates Bookings row
            // - Inserts BookingServices from the quotation's ServiceID CSV
            // - Deletes the quotation
            int newBookingId = 0;

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.usp_BookService_ConfirmFromQuotation", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@QuotationID", SqlDbType.Int).Value = quotationId;
                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                cmd.Parameters.Add("@ScheduledDate", SqlDbType.Date).Value = selectedDateTime.Date;
                cmd.Parameters.Add("@StartTime", SqlDbType.Time).Value = selectedDateTime.TimeOfDay;
                cmd.Parameters.Add("@Notes", SqlDbType.NVarChar, 500).Value = (object)notes ?? DBNull.Value;

                var pOut = cmd.Parameters.Add("@BookingID", SqlDbType.Int);
                pOut.Direction = ParameterDirection.Output;

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    if (pOut.Value != DBNull.Value)
                    {
                        newBookingId = Convert.ToInt32(pOut.Value);
                    }
                }
                catch (SqlException ex)
                {
                    // Show a friendly error
                    ScriptManager.RegisterStartupScript(this, GetType(), "sqlErr",
                        "Swal.fire('Error', 'Failed to create booking: " + ex.Message.Replace("'", "\\'") + "', 'error');", true);
                    return;
                }
            }

            if (newBookingId > 0)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "booked",
                    "Swal.fire('Success', 'Your service has been booked!', 'success');", true);

                // 🔔 Insert notification (non-blocking)
                try
                {
                    using (var con = new SqlConnection(connectionString))
                    using (var cmd = new SqlCommand("dbo.usp_Notifications_Add", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ClientID", clientId);
                        cmd.Parameters.AddWithValue("@Type", "booking");
                        cmd.Parameters.AddWithValue("@Title", "Booking Submitted");
                        cmd.Parameters.AddWithValue("@Body", "We’ve received your booking request.");
                        cmd.Parameters.AddWithValue("@Url", "MyBookings.aspx");
                        cmd.Parameters.AddWithValue("@DedupKey", "BOOK-" + newBookingId + "-SUBMITTED");
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                catch
                {
                    // Swallow errors so a notification failure doesn't affect booking UX
                }

                // Reset form
                txtDate.Text = "";
                txtTime.Text = "";
                txtNotes.Text = "";

                // Optional: redirect so user immediately sees the new booking
                // Response.Redirect("~/MyBookings.aspx");
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "fail",
                    "Swal.fire('Error', 'No booking was created. Please try again.', 'error');", true);
            }
        }
    
    }
}
