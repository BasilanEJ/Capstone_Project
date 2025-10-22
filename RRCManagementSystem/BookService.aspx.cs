using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
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

        // -------------------- Load Quotation --------------------
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
                        // Quotation Code
                        lblQuotationCode.Text = SafeGetString(reader, "QuotationCode", "N/A");

                        // Service & SQM
                        lblServices.Text = SafeGetString(reader, "ServiceNames", "N/A");
                        lblSQM.Text = SafeGetString(reader, "SQM", "0");

                        // Extract pricing values
                        decimal totalPrice = SafeGetDecimal(reader, "Price");
                        decimal travel = SafeGetDecimal(reader, "TravelExpense");
                        decimal misc = SafeGetDecimal(reader, "Miscellaneous");

                        // Calculate Base Service Price
                        decimal baseServicePrice = totalPrice - (travel + misc);
                        if (baseServicePrice < 0) baseServicePrice = 0;

                        // Assign values to UI
                        lblBasePrice.Text = $"₱{baseServicePrice:N2}";
                        lblTravelExpense.Text = $"₱{travel:N2}";
                        lblMiscellaneous.Text = $"₱{misc:N2}";
                        lblTotalPrice.Text = $"₱{totalPrice:N2}";

                        // NEW: Load and display miscellaneous details
                        string miscDetails = SafeGetString(reader, "MiscellaneousDetails", null);
                        if (!string.IsNullOrWhiteSpace(miscDetails))
                        {
                            pnlMiscDetails.Visible = true;
                            litMiscDetails.Text = FormatMiscellaneousDetails(miscDetails);
                        }
                        else
                        {
                            pnlMiscDetails.Visible = false;
                        }

                        // Contract detection
                        bool isContractCol = SafeGetBool(reader, "IsContract", false);
                        string serviceType = SafeGetString(reader, "ServiceType", null);
                        bool isTermiteType = string.Equals(serviceType, "Termite Control", StringComparison.OrdinalIgnoreCase);
                        bool isContractFinal = isTermiteType || isContractCol;

                        hfIsContract.Value = isContractFinal ? "True" : "False";
                        hfQuotationID.Value = SafeGetString(reader, "PendingQuotationID", null);

                        // Inspector
                        lblInspector.Text = SafeGetString(reader, "InspectorName", "N/A");
                        btnBook.Enabled = true;
                    }
                    else
                    {
                        btnBook.Enabled = false;
                        ScriptManager.RegisterStartupScript(this, GetType(), "noQuote", @"
                    Swal.fire({
                        icon: 'info',
                        title: 'No Quotation Available', 
                        text: 'Please wait for the inspector to create a quotation.',
                        confirmButtonColor: '#2563eb'
                    });", true);
                    }
                }
            }
        }

        // NEW: Helper method to format miscellaneous details for display
        private string FormatMiscellaneousDetails(string miscDetails)
        {
            if (string.IsNullOrWhiteSpace(miscDetails))
                return string.Empty;

            // Split by semicolon (e.g., "Food Allowance: ₱300.00; Parking Fee: ₱50.00")
            var items = miscDetails.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            var html = new System.Text.StringBuilder();
            foreach (var item in items)
            {
                var trimmedItem = item.Trim();
                if (!string.IsNullOrEmpty(trimmedItem))
                {
                    html.Append($"<div class='text-sm text-gray-600 flex justify-between'>");
                    html.Append($"<span>• {System.Web.HttpUtility.HtmlEncode(trimmedItem)}</span>");
                    html.Append("</div>");
                }
            }

            return html.ToString();
        }

        // -------------------- Booking Submission --------------------
        protected void btnBook_Click(object sender, EventArgs e)
        {
            // Validate selected date/time
            if (string.IsNullOrWhiteSpace(txtDate.Text) || string.IsNullOrWhiteSpace(txtTime.Text))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "missing",
                    @"Swal.fire({
                        icon: 'warning',
                        title: 'Missing Information',
                        text: 'Please select a preferred date and time.',
                        confirmButtonColor: '#2563eb'
                    });", true);
                return;
            }

            // Parse selected date & time
            DateTime selectedDateTime;
            if (!DateTime.TryParse($"{txtDate.Text} {txtTime.Text}", CultureInfo.CurrentCulture, DateTimeStyles.None, out selectedDateTime) &&
                !DateTime.TryParse($"{txtDate.Text} {txtTime.Text}", CultureInfo.GetCultureInfo("en-PH"), DateTimeStyles.None, out selectedDateTime))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "invalid",
                    @"Swal.fire({
                        icon: 'error',
                        title: 'Invalid Input',
                        text: 'Invalid date or time format.',
                        confirmButtonColor: '#dc2626'
                    });", true);
                return;
            }

            // Ensure date/time is at least 1 hour in the future
            DateTime oneHourFromNow = DateTime.Now.AddHours(1);
            if (selectedDateTime < oneHourFromNow)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "tooSoon",
                    @"Swal.fire({
                        icon: 'error',
                        title: 'Invalid Schedule',
                        text: 'Please choose a date and time at least 1 hour from now.',
                        confirmButtonColor: '#dc2626'
                    });", true);
                return;
            }

            // Validate quotation ID
            int quotationId;
            if (!int.TryParse(hfQuotationID.Value, out quotationId) || quotationId <= 0)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "noQuoteId",
                    @"Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: 'Missing quotation reference.',
                        confirmButtonColor: '#dc2626'
                    });", true);
                return;
            }

            // Ensure session is valid
            if (Session["ClientID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int clientId = Convert.ToInt32(Session["ClientID"]);
            string notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim();

            // Output values from SP
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

                // Determine if it's a contract or non-contract and set PaymentPlan accordingly
                bool isContract = hfIsContract.Value == "True";
                if (isContract)
                {
                    // Contract services can have 50-25-25, 70-30, or 100. Default to 50-25-25
                    cmd.Parameters.Add("@PaymentPlan", SqlDbType.NVarChar, 20).Value = "50-25-25";
                }
                else
                {
                    // Non-contract services must be "100"
                    cmd.Parameters.Add("@PaymentPlan", SqlDbType.NVarChar, 20).Value = "100";
                }

                // OUTPUT Parameters
                var pIdOut = cmd.Parameters.Add("@BookingID", SqlDbType.Int);
                pIdOut.Direction = ParameterDirection.Output;

                var pCodeOut = cmd.Parameters.Add("@OutBookingCode", SqlDbType.NVarChar, 16);
                pCodeOut.Direction = ParameterDirection.Output;

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();

                    // Get output values
                    if (pIdOut.Value != DBNull.Value) newBookingId = Convert.ToInt32(pIdOut.Value);
                    if (pCodeOut.Value != DBNull.Value) newBookingCode = pCodeOut.Value as string;

                    // Debug logs
                    System.Diagnostics.Debug.WriteLine("BookingID OUT: " + newBookingId);
                    System.Diagnostics.Debug.WriteLine("BookingCode OUT: " + newBookingCode);
                }
                catch (SqlException ex)
                {
                    // Sanitize error message for SweetAlert
                    string safeMessage = ex.Message
                        .Replace("'", "\\'")
                        .Replace("\"", "\\\"")
                        .Replace("\r", "")
                        .Replace("\n", " ");

                    ScriptManager.RegisterStartupScript(this, GetType(), "sqlErr",
                        $@"Swal.fire({{
                            icon: 'error',
                            title: 'Booking Failed',
                            text: 'Failed to create booking: {safeMessage}',
                            confirmButtonColor: '#dc2626'
                        }});", true);
                    return;
                }
            }

            // Final validation: Was the booking successfully created?
            if (newBookingId > 0)
            {
                // Success - redirect to Home.aspx with success message
                string successScript = $@"
                    Swal.fire({{
                        icon: 'success',
                        title: 'Booking Confirmed!',
                        html: 'Your service has been successfully booked.<br><strong>Booking Code: {newBookingCode}</strong>',
                        confirmButtonColor: '#16a34a',
                        confirmButtonText: 'Go to Home'
                    }}).then((result) => {{
                        window.location.href = 'Home.aspx';
                    }});";

                ScriptManager.RegisterStartupScript(this, GetType(), "booked", successScript, true);
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "fail",
                    @"Swal.fire({
                        icon: 'error',
                        title: 'Booking Failed',
                        text: 'No booking was created. Please try again.',
                        confirmButtonColor: '#dc2626'
                    });", true);
            }
        }

        // -------------------- Helper Methods --------------------
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

        private static decimal SafeGetDecimal(IDataRecord r, string column)
        {
            int i = SafeOrdinal(r, column);
            if (i < 0 || r.IsDBNull(i)) return 0m;
            return Convert.ToDecimal(r[i]);
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