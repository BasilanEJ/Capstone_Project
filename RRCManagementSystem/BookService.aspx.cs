using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Services;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class BookService : System.Web.UI.Page
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Check authentication
            if (Session["ClientID"] == null)
            {
                Response.Redirect("~/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                // Check if ReportID is in query string
                string reportIdStr = Request.QueryString["ReportID"];
                if (!string.IsNullOrWhiteSpace(reportIdStr) && int.TryParse(reportIdStr, out int reportId))
                {
                    // ✅ NEW WAY: Load from InspectionReport
                    LoadInspectionReport(reportId);
                }
                else
                {
                    // ❌ OLD WAY: Load from PendingQuotation (fallback)
                    LoadQuotation();
                }
            }
        }

        // ==================== NEW METHOD: Load from InspectionReport ====================
        private void LoadInspectionReport(int reportId)
        {
            int clientId = Convert.ToInt32(Session["ClientID"]);

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spInspectionReport_GetForBooking", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ReportID", SqlDbType.Int).Value = reportId;
                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Store ReportID in hidden field
                        hfQuotationID.Value = reportId.ToString();

                        // Quotation Code
                        lblQuotationCode.Text = SafeGetString(reader, "QuotationCode", "N/A");

                        // Parse Services from JSON
                        string servicesJson = SafeGetString(reader, "SelectedServices", "[]");
                        var serviceInfo = ParseServicesFromJson(servicesJson);

                        // Display formatted services with individual SQM
                        lblServices.Text = serviceInfo.ServiceDetailsFormatted;

                        // Display total SQM
                        lblSQM.Text = $"{serviceInfo.TotalSQM} m²";

                        // ✅ Store SQM in hidden field for JavaScript
                        hfSQM.Value = serviceInfo.TotalSQM.ToString();
                        Session["SQM"] = serviceInfo.TotalSQM; // Also store in session

                        // Store service names for database (without SQM)
                        ViewState["ServiceNames"] = serviceInfo.ServiceNames;

                        // Get pricing
                        decimal totalPrice = SafeGetDecimal(reader, "TotalEstimatedCost");
                        decimal travel = SafeGetDecimal(reader, "TravelCost");
                        decimal misc = SafeGetDecimal(reader, "MiscellaneousTotal");

                        // Calculate base service price
                        decimal baseServicePrice = totalPrice - (travel + misc);
                        if (baseServicePrice < 0) baseServicePrice = 0;

                        lblBasePrice.Text = $"₱{baseServicePrice:N2}";
                        lblTravelExpense.Text = $"₱{travel:N2}";
                        lblMiscellaneous.Text = $"₱{misc:N2}";
                        lblTotalPrice.Text = $"₱{totalPrice:N2}";

                        // Load miscellaneous details
                        string miscExpensesJson = SafeGetString(reader, "MiscellaneousExpenses", null);
                        if (!string.IsNullOrWhiteSpace(miscExpensesJson))
                        {
                            pnlMiscDetails.Visible = true;
                            litMiscDetails.Text = FormatMiscellaneousDetailsFromJson(miscExpensesJson);
                        }
                        else
                        {
                            pnlMiscDetails.Visible = false;
                        }

                        // Determine if contract based on services
                        bool isContract = serviceInfo.HasContractService;
                        hfIsContract.Value = isContract ? "True" : "False";

                        // ✅ UPDATED: Show/hide payment plan dropdown based on contract status
                        if (isContract)
                        {
                            pnlPaymentPlan.Visible = true;
                            ddlPaymentPlan.SelectedValue = "50-25-25"; // Default for contracts
                        }
                        else
                        {
                            pnlPaymentPlan.Visible = false;
                            ddlPaymentPlan.SelectedValue = "100"; // Force full payment for non-contracts
                        }

                        // Inspector info
                        lblInspector.Text = SafeGetString(reader, "InspectorName", "N/A");

                        // Store InspectorID for booking
                        ViewState["InspectorID"] = SafeGetInt(reader, "InspectorID", 0);

                        btnBook.Enabled = true;
                    }
                    else
                    {
                        btnBook.Enabled = false;
                        ScriptManager.RegisterStartupScript(this, GetType(), "noReport", @"
                            Swal.fire({
                                icon: 'error',
                                title: 'Report Not Found',
                                text: 'Unable to load inspection report details.',
                                confirmButtonColor: '#2563eb'
                            });", true);
                    }
                }
            }
        }

        // ==================== Parse Services from JSON ====================
        private ServiceInfo ParseServicesFromJson(string servicesJson)
        {
            var serviceInfo = new ServiceInfo();

            try
            {
                var services = JArray.Parse(servicesJson);
                var serviceDetails = new System.Collections.Generic.List<string>();
                var serviceNames = new System.Collections.Generic.List<string>();
                int totalSQM = 0;
                bool hasContract = false;

                foreach (var service in services)
                {
                    string serviceName = service["ServiceName"]?.ToString();
                    int sqm = Convert.ToInt32(service["SQM"] ?? 0);

                    if (!string.IsNullOrEmpty(serviceName))
                    {
                        // Add to simple list (for database storage)
                        serviceNames.Add(serviceName);

                        // Format: HTML service item with SQM badge
                        string formattedService = $@"
                            <div class='service-item'>
                                <span class='service-name'>
                                    <i class='fas fa-check-circle' style='color: #10b981; margin-right: 8px;'></i>
                                    {System.Web.HttpUtility.HtmlEncode(serviceName)}
                                </span>
                                <span class='service-sqm'>{sqm} m²</span>
                            </div>";

                        serviceDetails.Add(formattedService);
                        totalSQM += sqm;

                        // ✅ UPDATED: Check IsContract from JSON first, then fallback to database
                        bool serviceIsContract = false;

                        if (service["IsContract"] != null)
                        {
                            // Use IsContract from JSON if it exists
                            serviceIsContract = Convert.ToBoolean(service["IsContract"]);
                        }
                        else
                        {
                            // Fallback: Get IsContract from Services table
                            serviceIsContract = GetServiceIsContractFromDB(serviceName);
                        }

                        if (serviceIsContract)
                        {
                            hasContract = true;
                        }
                    }
                }

                // Store formatted HTML for display
                serviceInfo.ServiceDetailsFormatted = string.Join("", serviceDetails);

                // Store simple comma-separated list for database
                serviceInfo.ServiceNames = string.Join(", ", serviceNames);

                serviceInfo.TotalSQM = totalSQM;
                serviceInfo.HasContractService = hasContract;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ParseServicesFromJson error: " + ex.Message);
                serviceInfo.ServiceDetailsFormatted = "<div class='text-red-600'>Error parsing services</div>";
                serviceInfo.ServiceNames = "Error parsing services";
            }

            return serviceInfo;
        }

        // ✅ NEW: Get IsContract status from database
        private bool GetServiceIsContractFromDB(string serviceName)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("SELECT ISNULL(IsContract, 0) FROM dbo.Services WHERE Name = @ServiceName", conn))
                {
                    cmd.Parameters.Add("@ServiceName", SqlDbType.NVarChar, 100).Value = serviceName;
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null && Convert.ToBoolean(result);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetServiceIsContractFromDB error: {ex.Message}");

                // Fallback: Use name-based detection as last resort
                return serviceName.IndexOf("Termite", StringComparison.OrdinalIgnoreCase) >= 0;
            }
        }

        // Helper class for service info
        private class ServiceInfo
        {
            public string ServiceNames { get; set; } = ""; // Simple list for database
            public string ServiceDetailsFormatted { get; set; } = ""; // HTML formatted list with SQM
            public int TotalSQM { get; set; } = 0;
            public bool HasContractService { get; set; } = false;
        }

        // ==================== OLD METHOD: Load from PendingQuotation (Fallback) ====================
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
                        lblQuotationCode.Text = SafeGetString(reader, "QuotationCode", "N/A");

                        // For old quotation format, just display as text
                        string services = SafeGetString(reader, "ServiceNames", "N/A");
                        lblServices.Text = $"<div class='service-item'><span class='service-name'>{System.Web.HttpUtility.HtmlEncode(services)}</span></div>";

                        string sqmText = SafeGetString(reader, "SQM", "0");
                        lblSQM.Text = sqmText + " m²";

                        // ✅ Store SQM in hidden field for JavaScript
                        if (int.TryParse(sqmText, out int sqmValue))
                        {
                            hfSQM.Value = sqmValue.ToString();
                            Session["SQM"] = sqmValue;
                        }

                        // Store for booking
                        ViewState["ServiceNames"] = services;

                        decimal totalPrice = SafeGetDecimal(reader, "Price");
                        decimal travel = SafeGetDecimal(reader, "TravelExpense");
                        decimal misc = SafeGetDecimal(reader, "Miscellaneous");

                        decimal baseServicePrice = totalPrice - (travel + misc);
                        if (baseServicePrice < 0) baseServicePrice = 0;

                        lblBasePrice.Text = $"₱{baseServicePrice:N2}";
                        lblTravelExpense.Text = $"₱{travel:N2}";
                        lblMiscellaneous.Text = $"₱{misc:N2}";
                        lblTotalPrice.Text = $"₱{totalPrice:N2}";

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

                        bool isContractCol = SafeGetBool(reader, "IsContract", false);
                        string serviceType = SafeGetString(reader, "ServiceType", null);
                        bool isTermiteType = string.Equals(serviceType, "Termite Control", StringComparison.OrdinalIgnoreCase);
                        bool isContractFinal = isTermiteType || isContractCol;

                        hfIsContract.Value = isContractFinal ? "True" : "False";

                        // ✅ UPDATED: Show/hide payment plan dropdown for old quotations too
                        if (isContractFinal)
                        {
                            pnlPaymentPlan.Visible = true;
                            ddlPaymentPlan.SelectedValue = "50-25-25";
                        }
                        else
                        {
                            pnlPaymentPlan.Visible = false;
                            ddlPaymentPlan.SelectedValue = "100";
                        }

                        hfQuotationID.Value = SafeGetString(reader, "PendingQuotationID", null);

                        lblInspector.Text = SafeGetString(reader, "InspectorName", "N/A");

                        // Store inspector ID if available
                        ViewState["InspectorID"] = SafeGetInt(reader, "InspectorID", 0);

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

        // ==================== WEBMETHOD: Check Date Availability ====================
        [WebMethod]
        public static object CheckDateAvailability(string date, int sqm)
        {
            try
            {
                if (!DateTime.TryParse(date, out DateTime serviceDate))
                {
                    return new { Success = false, Message = "Invalid date format" };
                }

                var timeSlots = new System.Collections.Generic.List<object>();

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spTeam_CheckDateAvailabilityBySlot", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ServiceDate", SqlDbType.Date).Value = serviceDate;
                    cmd.Parameters.Add("@SQM", SqlDbType.Int).Value = sqm;

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            timeSlots.Add(new
                            {
                                TimeSlotID = Convert.ToInt32(reader["TimeSlotID"]),
                                TimeSlotName = reader["TimeSlotName"].ToString(),
                                TotalTeams = Convert.ToInt32(reader["TotalTeams"]),
                                BookedCount = Convert.ToInt32(reader["BookedCount"]),
                                AvailableTeams = Convert.ToInt32(reader["AvailableTeams"]),
                                IsAvailable = Convert.ToBoolean(reader["IsAvailable"])
                            });
                        }
                    }
                }

                return new { Success = true, TimeSlots = timeSlots };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CheckDateAvailability Error: {ex.Message}");
                return new { Success = false, Message = "Error checking availability" };
            }
        }

        // ==================== WEBMETHOD: Check Time Slot Availability ====================
        [WebMethod]
        public static object CheckTimeSlotAvailability(string date, int timeSlotId, int sqm)
        {
            try
            {
                if (!DateTime.TryParse(date, out DateTime serviceDate))
                {
                    return new { IsAvailable = false, Message = "Invalid date format" };
                }

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spTeam_CheckTimeSlotAvailability", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ServiceDate", SqlDbType.Date).Value = serviceDate;
                    cmd.Parameters.Add("@TimeSlotID", SqlDbType.Int).Value = timeSlotId;
                    cmd.Parameters.Add("@SQM", SqlDbType.Int).Value = sqm;

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int availableTeams = Convert.ToInt32(reader["AvailableTeams"]);
                            int totalTeams = Convert.ToInt32(reader["TotalTeams"]);

                            return new
                            {
                                IsAvailable = Convert.ToBoolean(reader["IsAvailable"]),
                                TotalTeams = totalTeams,
                                BookedCount = Convert.ToInt32(reader["BookedCount"]),
                                AvailableTeams = availableTeams,
                                Message = availableTeams > 0
                                    ? $"{availableTeams} team(s) available"
                                    : "This time slot is fully booked"
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CheckTimeSlotAvailability Error: {ex.Message}");
            }

            return new { IsAvailable = false, Message = "Error checking availability" };
        }

        // ==================== Format Miscellaneous from JSON ====================
        private string FormatMiscellaneousDetailsFromJson(string miscJson)
        {
            if (string.IsNullOrWhiteSpace(miscJson))
                return string.Empty;

            try
            {
                var miscItems = JArray.Parse(miscJson);
                var html = new System.Text.StringBuilder();

                foreach (var item in miscItems)
                {
                    string description = item["description"]?.ToString();
                    decimal amount = Convert.ToDecimal(item["amount"] ?? 0);

                    if (!string.IsNullOrEmpty(description))
                    {
                        html.Append($"<div class='text-sm text-gray-600 flex justify-between'>");
                        html.Append($"<span>• {System.Web.HttpUtility.HtmlEncode(description)}</span>");
                        html.Append($"<span class='font-semibold'>₱{amount:N2}</span>");
                        html.Append("</div>");
                    }
                }

                return html.ToString();
            }
            catch
            {
                return "<div class='text-sm text-gray-600'>Error parsing miscellaneous details</div>";
            }
        }

        // Format miscellaneous details (old format)
        private string FormatMiscellaneousDetails(string miscDetails)
        {
            if (string.IsNullOrWhiteSpace(miscDetails))
                return string.Empty;

            var items = miscDetails.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var html = new System.Text.StringBuilder();

            foreach (var item in items)
            {
                var trimmedItem = item.Trim();
                if (!string.IsNullOrEmpty(trimmedItem))
                {
                    html.Append($"<div class='text-sm text-gray-600'>");
                    html.Append($"<span>• {System.Web.HttpUtility.HtmlEncode(trimmedItem)}</span>");
                    html.Append("</div>");
                }
            }

            return html.ToString();
        }

        // ==================== BOOKING SUBMISSION ====================
        protected void btnBook_Click(object sender, EventArgs e)
        {
            // ✅ Validate date and time slot
            if (string.IsNullOrWhiteSpace(txtDate.Value) || string.IsNullOrWhiteSpace(txtTimeSlot.Value))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "missing",
                    @"Swal.fire({
                        icon: 'warning',
                        title: 'Missing Information',
                        text: 'Please select a preferred date and time slot.',
                        confirmButtonColor: '#2563eb'
                    });", true);
                return;
            }

            // Parse selected date
            DateTime selectedDate;
            if (!DateTime.TryParse(txtDate.Value, out selectedDate))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "invalid",
                    @"Swal.fire({
                        icon: 'error',
                        title: 'Invalid Input',
                        text: 'Invalid date format.',
                        confirmButtonColor: '#dc2626'
                    });", true);
                return;
            }

            // Validate not in the past
            if (selectedDate.Date < DateTime.Today)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "pastDate",
                    @"Swal.fire({
                        icon: 'error',
                        title: 'Invalid Date',
                        text: 'Please choose a future date.',
                        confirmButtonColor: '#dc2626'
                    });", true);
                return;
            }

            // Parse time slot ID
            if (!int.TryParse(txtTimeSlot.Value, out int timeSlotId) || timeSlotId < 1 || timeSlotId > 4)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "invalidSlot",
                    @"Swal.fire({
                        icon: 'error',
                        title: 'Invalid Time Slot',
                        text: 'Please select a valid time slot.',
                        confirmButtonColor: '#dc2626'
                    });", true);
                return;
            }

            // ✅ Convert time slot ID to StartTime
            TimeSpan startTime = GetStartTimeFromSlot(timeSlotId);

            // Get client ID
            int clientId = Convert.ToInt32(Session["ClientID"]);

            // Get inspector ID from ViewState
            int inspectorId = ViewState["InspectorID"] != null ? Convert.ToInt32(ViewState["InspectorID"]) : 0;

            // Get report ID
            int reportId;
            if (!int.TryParse(hfQuotationID.Value, out reportId) || reportId <= 0)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "noReportId",
                    @"Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: 'Missing report reference.',
                        confirmButtonColor: '#dc2626'
                    });", true);
                return;
            }

            // ✅ UPDATED: Get payment plan based on contract status (using dropdown)
            bool isContract = hfIsContract.Value == "True";
            string paymentPlan;

            if (isContract)
            {
                // Contract booking - use client's selection from dropdown
                paymentPlan = ddlPaymentPlan.SelectedValue;

                // Validation: Make sure they selected something
                if (string.IsNullOrWhiteSpace(paymentPlan))
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "noPlan",
                        @"Swal.fire({
                            icon: 'warning',
                            title: 'Payment Plan Required',
                            text: 'Please select a payment plan for your contract booking.',
                            confirmButtonColor: '#2563eb'
                        });", true);
                    return;
                }
            }
            else
            {
                // Non-contract booking - force 100% payment
                paymentPlan = "100";
            }

            // Get service names from ViewState
            string serviceNames = ViewState["ServiceNames"] != null ? ViewState["ServiceNames"].ToString() : lblServices.Text;

            // Parse SQM from label (remove " m²" suffix)
            string sqmText = lblSQM.Text.Replace(" m²", "").Trim();
            int sqm = int.TryParse(sqmText, out int s) ? s : 0;

            decimal totalPrice = ParseCurrency(lblTotalPrice.Text);
            decimal travel = ParseCurrency(lblTravelExpense.Text);
            decimal misc = ParseCurrency(lblMiscellaneous.Text);
            string notes = txtNotes.Text.Trim();

            // Output parameters
            int newBookingId = 0;
            string newBookingCode = null;

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spBooking_CreateFromInspectionReport", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ReportID", SqlDbType.Int).Value = reportId;
                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                cmd.Parameters.Add("@InspectorID", SqlDbType.Int).Value = inspectorId;
                cmd.Parameters.Add("@ScheduledDate", SqlDbType.Date).Value = selectedDate.Date;
                cmd.Parameters.Add("@StartTime", SqlDbType.Time).Value = startTime;
                cmd.Parameters.Add("@ServiceNames", SqlDbType.NVarChar, -1).Value = serviceNames;
                cmd.Parameters.Add("@SQM", SqlDbType.Int).Value = sqm;
                cmd.Parameters.Add("@Price", SqlDbType.Decimal).Value = totalPrice;
                cmd.Parameters.Add("@TravelExpense", SqlDbType.Decimal).Value = travel;
                cmd.Parameters.Add("@Miscellaneous", SqlDbType.Decimal).Value = misc;
                cmd.Parameters.Add("@PaymentPlan", SqlDbType.NVarChar, 100).Value = paymentPlan; // ✅ Now uses dropdown selection
                cmd.Parameters.Add("@Notes", SqlDbType.NVarChar, -1).Value = string.IsNullOrWhiteSpace(notes) ? (object)DBNull.Value : notes;

                // Output parameters
                var pIdOut = cmd.Parameters.Add("@BookingID", SqlDbType.Int);
                pIdOut.Direction = ParameterDirection.Output;

                var pCodeOut = cmd.Parameters.Add("@BookingCode", SqlDbType.NVarChar, 16);
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
                    string safeMessage = ex.Message.Replace("'", "\\'").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", " ");
                    ScriptManager.RegisterStartupScript(this, GetType(), "sqlErr",
                        $@"Swal.fire({{
                            icon: 'error',
                            title: 'Booking Failed',
                            text: '{safeMessage}',
                            confirmButtonColor: '#dc2626'
                        }});", true);
                    return;
                }
            }

            // Success
            if (newBookingId > 0)
            {
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

        // ==================== HELPER: Convert Time Slot ID to StartTime ====================
        private TimeSpan GetStartTimeFromSlot(int timeSlotId)
        {
            switch (timeSlotId)
            {
                case 1: return new TimeSpan(8, 0, 0);   // 8:00 AM
                case 2: return new TimeSpan(12, 0, 0);  // 12:00 PM
                case 3: return new TimeSpan(16, 0, 0);  // 4:00 PM
                case 4: return new TimeSpan(20, 0, 0);  // 8:00 PM
                default: return new TimeSpan(8, 0, 0);  // Default to 8 AM
            }
        }

        private decimal ParseCurrency(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0m;

            // Remove currency symbol and commas
            value = value.Replace("₱", "").Replace(",", "").Trim();

            if (decimal.TryParse(value, out decimal result))
                return result;

            return 0m;
        }

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

        private static int SafeGetInt(IDataRecord r, string column, int fallback)
        {
            int i = SafeOrdinal(r, column);
            if (i < 0 || r.IsDBNull(i)) return fallback;
            return Convert.ToInt32(r[i]);
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