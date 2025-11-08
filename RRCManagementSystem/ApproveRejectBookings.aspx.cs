using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json.Linq;

namespace RRCManagementSystem
{
    public partial class ApproveRejectBookings : System.Web.UI.Page
    {
        private readonly string cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 🔐 Require login
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string role = Session["Role"].ToString();

            // 🔐 Block SuperAdmin and Inspector
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            // 🔐 Check CanEdit permission for ManageBooking
            if (!HasEditPermission(userId, "ManageBooking"))
            {
                lblMessage.Text = "❌ You do not have permission to approve or reject bookings.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                gvBookings.Visible = false;
                return;
            }

            if (!IsPostBack)
            {
                LoadPendingBookings();
            }
        }

        // ==================== Permission Check ====================
        private bool HasEditPermission(int adminId, string moduleName)
        {
            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spAdminPermission_Check", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = adminId;
                    cmd.Parameters.Add("@ModuleName", SqlDbType.NVarChar, 100).Value = moduleName;
                    cmd.Parameters.Add("@Permission", SqlDbType.NVarChar, 10).Value = "CanEdit";

                    con.Open();
                    object allowed = cmd.ExecuteScalar();
                    return allowed != null && allowed != DBNull.Value && Convert.ToBoolean(allowed);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HasEditPermission error: {ex.Message}");
                return false;
            }
        }

        // ==================== Load Pending Bookings ====================
        private void LoadPendingBookings()
        {
            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spBooking_ListPendingForApproval", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        gvBookings.DataSource = dt;
                        gvBookings.DataBind();

                        if (dt.Rows.Count == 0)
                        {
                            lblMessage.Text = "";
                        }
                        else
                        {
                            lblMessage.Text = $"<i class='fas fa-info-circle mr-2'></i>{dt.Rows.Count} pending booking(s) found";
                            lblMessage.ForeColor = System.Drawing.Color.FromArgb(59, 130, 246); // Blue
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠️ Error loading bookings: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
                System.Diagnostics.Debug.WriteLine($"LoadPendingBookings error: {ex.Message}");
            }
        }

        // ==================== GridView Row Data Bound ====================
        protected void gvBookings_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // ✅ Highlight rows without contracts
                bool hasContract = Convert.ToBoolean(DataBinder.Eval(e.Row.DataItem, "HasContract"));

                if (!hasContract)
                {
                    // Light yellow background for missing contracts
                    e.Row.BackColor = System.Drawing.Color.FromArgb(254, 252, 232);

                    // Add tooltip to the entire row
                    e.Row.Attributes["title"] = "⚠️ Client has no contract uploaded";
                    e.Row.Style["cursor"] = "help";
                }

                // ✅ Add visual warning icon to Client Name cell if no contract
                if (!hasContract)
                {
                    // Find the Client Name cell (adjust index if needed - currently assumes column 2)
                    TableCell clientCell = e.Row.Cells[2];
                    clientCell.Text += " <i class='fas fa-exclamation-triangle text-yellow-600 ml-2' " +
                                      "title='No contract uploaded' style='font-size: 0.875rem;'></i>";
                }
            }
        }

        // ==================== Format Service Details ====================
        /// <summary>
        /// Format service names from JSON or fallback to simple string
        /// </summary>
        protected string FormatServiceDetails(object serviceDetailsObj, object serviceNameObj)
        {
            string serviceDetails = serviceDetailsObj?.ToString();
            string serviceName = serviceNameObj?.ToString();

            // If we have JSON data, parse it
            if (!string.IsNullOrWhiteSpace(serviceDetails) && serviceDetails.TrimStart().StartsWith("["))
            {
                try
                {
                    var services = JArray.Parse(serviceDetails);
                    var html = new System.Text.StringBuilder();

                    foreach (var service in services)
                    {
                        string name = service["ServiceName"]?.ToString();
                        if (!string.IsNullOrEmpty(name))
                        {
                            html.Append("<div class='service-item-row'>");
                            html.Append("<i class='fas fa-check-circle text-green-500' style='font-size: 0.875rem;'></i>");
                            html.Append($"<span class='text-sm'>{System.Web.HttpUtility.HtmlEncode(name)}</span>");
                            html.Append("</div>");
                        }
                    }

                    return html.Length > 0 ? html.ToString() : "<span class='text-gray-400 text-sm'>N/A</span>";
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"FormatServiceDetails JSON parse error: {ex.Message}");
                    // Fall through to simple display
                }
            }

            // Fallback: display simple service name
            if (!string.IsNullOrWhiteSpace(serviceName))
            {
                return $"<div class='service-item-row'><i class='fas fa-tools text-blue-500' style='font-size: 0.875rem;'></i><span class='text-sm'>{System.Web.HttpUtility.HtmlEncode(serviceName)}</span></div>";
            }

            return "<span class='text-gray-400 text-sm'>N/A</span>";
        }

        // ==================== Format SQM Details ====================
        /// <summary>
        /// Format SQM values from JSON or fallback to total
        /// </summary>
        protected string FormatSQMDetails(object serviceDetailsObj, object totalSQMObj)
        {
            string serviceDetails = serviceDetailsObj?.ToString();

            // If we have JSON data, parse it
            if (!string.IsNullOrWhiteSpace(serviceDetails) && serviceDetails.TrimStart().StartsWith("["))
            {
                try
                {
                    var services = JArray.Parse(serviceDetails);
                    var html = new System.Text.StringBuilder();
                    int totalSQM = 0;

                    foreach (var service in services)
                    {
                        int sqm = Convert.ToInt32(service["SQM"] ?? 0);
                        totalSQM += sqm;

                        html.Append("<div class='service-item-row'>");
                        html.Append($"<span class='sqm-badge'>{sqm} m²</span>");
                        html.Append("</div>");
                    }

                    // Add total if multiple services
                    if (services.Count > 1)
                    {
                        html.Append("<div class='sqm-total'>");
                        html.Append($"<strong>Total:</strong> {totalSQM} m²");
                        html.Append("</div>");
                    }

                    return html.Length > 0 ? html.ToString() : "<span class='text-gray-400 text-sm'>0 m²</span>";
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"FormatSQMDetails JSON parse error: {ex.Message}");
                    // Fall through to simple display
                }
            }

            // Fallback: display total SQM
            if (totalSQMObj != null && totalSQMObj != DBNull.Value)
            {
                return $"<span class='sqm-badge'>{totalSQMObj} m²</span>";
            }

            return "<span class='text-gray-400 text-sm'>0 m²</span>";
        }

        // ==================== Helper Methods for Display ====================

        /// <summary>
        /// Generate payment plan badge HTML
        /// </summary>
        protected string GetPaymentBadge(string paymentPlan, bool isContract)
        {
            if (string.IsNullOrWhiteSpace(paymentPlan))
                paymentPlan = isContract ? "50-25-25" : "100";

            string badgeClass = isContract ? "payment-contract" : "payment-full";
            string icon = isContract ? "fa-calendar-alt" : "fa-money-bill-wave";

            return $"<span class='payment-badge {badgeClass}'><i class='fas {icon} mr-1'></i>{paymentPlan}</span>";
        }

        /// <summary>
        /// Generate contract status indicator HTML
        /// </summary>
        protected string GetContractIndicator(bool hasContract)
        {
            if (hasContract)
            {
                return "<span class='contract-indicator text-green-600'>" +
                       "<i class='fas fa-file-contract'></i> Yes</span>";
            }
            else
            {
                return "<span class='contract-indicator text-yellow-600'>" +
                       "<i class='fas fa-exclamation-triangle'></i> No</span>";
            }
        }

        /// <summary>
        /// Format time value for display
        /// </summary>
        protected string FormatTime(object timeValue)
        {
            if (timeValue == null || timeValue == DBNull.Value)
                return "—";

            try
            {
                // If it's already a DateTime
                if (timeValue is DateTime dt)
                {
                    return dt.ToString("hh:mm tt");
                }

                // If it's a TimeSpan
                if (timeValue is TimeSpan ts)
                {
                    return DateTime.Today.Add(ts).ToString("hh:mm tt");
                }

                // Try parsing as TimeSpan
                if (TimeSpan.TryParse(timeValue.ToString(), out TimeSpan parsedTime))
                {
                    return DateTime.Today.Add(parsedTime).ToString("hh:mm tt");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FormatTime error: {ex.Message}");
            }

            return "—";
        }

        // ==================== Row Command Handler ====================
        protected void gvBookings_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!int.TryParse(e.CommandArgument.ToString(), out int bookingID))
            {
                ShowErrorAlert("Invalid Booking ID");
                return;
            }

            int adminId = Convert.ToInt32(Session["UserID"]);
            string bookingCode = GetBookingCode(bookingID);

            if (string.IsNullOrEmpty(bookingCode))
            {
                ShowErrorAlert("Failed to retrieve booking information");
                return;
            }

            if (e.CommandName == "Approve")
            {
                HandleApproval(bookingID, bookingCode, adminId);
            }
            else if (e.CommandName == "Reject")
            {
                HandleRejection(bookingID, bookingCode, adminId);
            }
        }

        // ==================== Approval Handler ====================
        private void HandleApproval(int bookingID, string bookingCode, int adminId)
        {
            int clientId = GetClientIdFromBooking(bookingID);

            if (clientId == 0)
            {
                ShowErrorAlert("Failed to retrieve client information");
                return;
            }

            // 🔍 Check if client has contract first
            if (!HasClientContract(clientId))
            {
                // ⚠️ Show SweetAlert and redirect to ManageContract.aspx
                string js = $@"
                    Swal.fire({{
                        icon: 'warning',
                        title: 'No Contract Found',
                        html: 'Client does not have a contract yet.<br><strong>Booking: {EscapeJs(bookingCode)}</strong><br>Please upload one before approving.',
                        confirmButtonText: '<i class=""fas fa-upload mr-2""></i>Go to Contract Upload',
                        confirmButtonColor: '#2563eb',
                        showCancelButton: true,
                        cancelButtonText: 'Cancel'
                    }}).then((result) => {{
                        if (result.isConfirmed) {{
                            window.location.href = 'ManageContract.aspx?ClientID={clientId}';
                        }}
                    }});";
                ScriptManager.RegisterStartupScript(this, GetType(), "NoContractAlert", js, true);
                return;
            }

            // ✅ Get booking info
            var info = GetBookingBasics(bookingID);

            if (info.Price == 0 && info.SQM == 0)
            {
                ShowErrorAlert("Failed to retrieve booking details");
                return;
            }

            // ✅ Store booking info in session INCLUDING ServiceDetails
            Session["BookingID"] = bookingID;
            Session["BookingCode"] = bookingCode;
            Session["Price"] = info.Price;
            Session["SQM"] = info.SQM;
            Session["ServiceName"] = info.ServiceNames;
            Session["ServiceDetails"] = info.ServiceDetails; // ✅ NEW: Pass JSON to next page
            Session["ScheduledDate"] = info.ScheduledDate;
            Session["AdminID"] = adminId;

            // Show loading and redirect
            string redirectJs = $@"
                Swal.fire({{
                    icon: 'info',
                    title: 'Proceeding to Team Assignment',
                    html: 'Redirecting for booking <strong>{EscapeJs(bookingCode)}</strong>...',
                    timer: 1500,
                    timerProgressBar: true,
                    showConfirmButton: false,
                    allowOutsideClick: false
                }}).then(() => {{
                    window.location.href = 'AssignBooking.aspx?BookingID={bookingID}';
                }});";

            ScriptManager.RegisterStartupScript(this, GetType(), "RedirectToAssign", redirectJs, true);
        }

        // ==================== Rejection Handler ====================
        private void HandleRejection(int bookingID, string bookingCode, int adminId)
        {
            // ❌ IMMEDIATE REJECTION - Update status right away
            if (SetBookingStatus(bookingID, "Rejected"))
            {
                AddAuditLog(adminId, $"Rejected booking {bookingCode} (ID: {bookingID})");

                string js = $@"
                    Swal.fire({{
                        icon: 'success',
                        title: 'Booking Rejected',
                        html: 'Booking <strong>{EscapeJs(bookingCode)}</strong> has been rejected.',
                        confirmButtonColor: '#10b981',
                        confirmButtonText: '<i class=""fas fa-check mr-2""></i>OK'
                    }}).then((result) => {{
                        window.location.href = 'ApproveRejectBookings.aspx';
                    }});";

                ScriptManager.RegisterStartupScript(this, GetType(), "RejectOK", js, true);
            }
            else
            {
                ShowErrorAlert($"Failed to reject booking {bookingCode}");
            }
        }

        // ==================== Database Helper Methods ====================

        /// <summary>
        /// Check if client has contract
        /// </summary>
        private bool HasClientContract(int clientId)
        {
            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.ClientContracts WHERE ClientID = @ClientID", con))
                {
                    cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                    con.Open();
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HasClientContract error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get ClientID from booking
        /// </summary>
        private int GetClientIdFromBooking(int bookingID)
        {
            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("SELECT ClientID FROM dbo.Bookings WHERE BookingID = @BookingID", con))
                {
                    cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingID;
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetClientIdFromBooking error: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Get BookingCode from BookingID
        /// </summary>
        private string GetBookingCode(int bookingID)
        {
            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("SELECT BookingCode FROM dbo.Bookings WHERE BookingID = @BookingID", con))
                {
                    cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingID;
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    return result?.ToString() ?? null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetBookingCode error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Set booking status
        /// </summary>
        private bool SetBookingStatus(int bookingID, string status)
        {
            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spBooking_SetStatus", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingID;
                    cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = status;

                    con.Open();
                    object rows = cmd.ExecuteScalar();
                    return rows != null && rows != DBNull.Value && Convert.ToInt32(rows) > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SetBookingStatus error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get basic booking information - UPDATED to include ServiceDetails
        /// </summary>
        private (decimal Price, int SQM, string ServiceNames, DateTime ScheduledDate, string ServiceDetails) GetBookingBasics(int bookingID)
        {
            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spBooking_GetBasics", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingID;

                    con.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            decimal price = r["Price"] != DBNull.Value ? Convert.ToDecimal(r["Price"]) : 0m;
                            int sqm = r["SQM"] != DBNull.Value ? Convert.ToInt32(r["SQM"]) : 0;
                            string services = r["ServiceNames"]?.ToString() ?? "";
                            DateTime sched = r["ScheduledDate"] != DBNull.Value ? Convert.ToDateTime(r["ScheduledDate"]) : DateTime.MinValue;
                            string serviceDetails = r["ServiceDetails"]?.ToString() ?? ""; // ✅ NEW
                            return (price, sqm, services, sched, serviceDetails);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetBookingBasics error: {ex.Message}");
            }

            return (0m, 0, "", DateTime.MinValue, "");
        }

        /// <summary>
        /// Add audit log entry
        /// </summary>
        private void AddAuditLog(int? userID, string action)
        {
            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spAudit_Insert", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@AdminID", SqlDbType.Int).Value = (object)userID ?? DBNull.Value;
                    cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 255).Value = action;

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddAuditLog error: {ex.Message}");
            }
        }

        // ==================== Utility Methods ====================

        /// <summary>
        /// Escape JavaScript strings to prevent injection
        /// </summary>
        private string EscapeJs(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text.Replace("\\", "\\\\")
                      .Replace("'", "\\'")
                      .Replace("\"", "\\\"")
                      .Replace("\r", "")
                      .Replace("\n", " ");
        }

        /// <summary>
        /// Show error alert with SweetAlert2
        /// </summary>
        private void ShowErrorAlert(string message)
        {
            string js = $@"
                Swal.fire({{
                    icon: 'error',
                    title: 'Error',
                    text: '{EscapeJs(message)}',
                    confirmButtonColor: '#ef4444'
                }});";
            ScriptManager.RegisterStartupScript(this, GetType(), "ErrorAlert", js, true);
        }
    }
}