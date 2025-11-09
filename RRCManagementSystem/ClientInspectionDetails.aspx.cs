using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;
using System.Web.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RRCManagementSystem
{
    public partial class ClientInspectionDetails : Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Client authentication check
            if (Session["ClientID"] == null)
            {
                Response.Redirect("~/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                // Validate ReportID properly
                string reportIdStr = Request.QueryString["ReportID"];
                if (string.IsNullOrWhiteSpace(reportIdStr) || !int.TryParse(reportIdStr, out int reportId))
                {
                    ShowAlert("error", "Invalid Request", "No valid Report ID specified.");
                    RedirectToMyInquiries();
                    return;
                }

                // Verify client owns this inquiry
                int clientId = Convert.ToInt32(Session["ClientID"]);
                if (!VerifyClientOwnership(reportId, clientId))
                {
                    ShowAlert("error", "Access Denied", "You don't have permission to view this report.");
                    RedirectToMyInquiries();
                    return;
                }

                LoadInspectionDetails(reportId);

                // ✅ NEW: Check if booking already exists for this report
                CheckExistingBooking(reportId, clientId);
            }
        }

        // ==============================
        // ✅ NEW: CHECK IF BOOKING EXISTS
        // ==============================
        private void CheckExistingBooking(int reportId, int clientId)
        {
            const string sql = @"
                SELECT TOP 1 
                    B.BookingID,
                    B.BookingCode,
                    B.Status,
                    B.CreatedAt
                FROM dbo.Bookings B
                WHERE B.ReportID = @ReportID 
                  AND B.ClientID = @ClientID
                ORDER BY B.CreatedAt DESC";

            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@ReportID", SqlDbType.Int) { Value = reportId });
                    cmd.Parameters.Add(new SqlParameter("@ClientID", SqlDbType.Int) { Value = clientId });

                    conn.Open();
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            // ✅ Booking exists - hide button and show message
                            btnBookService.Visible = false;

                            string bookingCode = dr["BookingCode"]?.ToString() ?? "N/A";
                            string status = dr["Status"]?.ToString() ?? "Unknown";
                            DateTime createdAt = dr["CreatedAt"] != DBNull.Value
                                ? Convert.ToDateTime(dr["CreatedAt"])
                                : DateTime.Now;

                            // Show info message that booking already exists
                            string infoScript = $@"
                                Swal.fire({{
                                    icon: 'info',
                                    title: 'Already Booked',
                                    html: '<p>You have already booked this service.</p><p><strong>Booking Code:</strong> {bookingCode}</p><p><strong>Status:</strong> {status}</p><p><strong>Booked On:</strong> {createdAt:MMM dd, yyyy}</p>',
                                    confirmButtonColor: '#1e40af',
                                    confirmButtonText: 'View My Bookings'
                                }}).then((result) => {{
                                    if (result.isConfirmed) {{
                                        window.location.href = 'MyBookings.aspx';
                                    }}
                                }});
                            ";
                            ScriptManager.RegisterStartupScript(this, GetType(), "AlreadyBooked", infoScript, true);
                        }
                        else
                        {
                            // ✅ No booking exists - show button
                            btnBookService.Visible = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("CheckExistingBooking error: " + ex.Message);
                // If error occurs, keep button visible (fail-safe)
                btnBookService.Visible = true;
            }
        }

        // ==============================
        // VERIFY CLIENT OWNERSHIP
        // ==============================
        private bool VerifyClientOwnership(int reportId, int clientId)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM dbo.InspectionReports IR
                INNER JOIN dbo.Inquiries I ON IR.InquiryID = I.InquiryID
                WHERE IR.ReportID = @ReportID 
                  AND I.ClientID = @ClientID
                  AND IR.Status = 'Approved'";

            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@ReportID", SqlDbType.Int) { Value = reportId });
                    cmd.Parameters.Add(new SqlParameter("@ClientID", SqlDbType.Int) { Value = clientId });

                    conn.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("VerifyClientOwnership error: " + ex.Message);
                return false;
            }
        }

        // ==============================
        // LOAD INSPECTION DETAILS (SAFE)
        // ==============================
        private void LoadInspectionDetails(int reportId)
        {
            const string sql = @"
                SELECT 
                    IR.*, 
                    I.InquiryNumber, 
                    U.Name AS InspectorName,
                    I.ClientID
                FROM dbo.InspectionReports IR
                LEFT JOIN dbo.Inquiries I ON IR.InquiryID = I.InquiryID
                LEFT JOIN dbo.Users U ON IR.InspectorID = U.UserID
                WHERE IR.ReportID = @ReportID
                  AND IR.Status = 'Approved'
                  AND ISNULL(U.Role, '') = @InspectorRole
            ";

            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    // Explicit parameter types
                    cmd.Parameters.Add(new SqlParameter("@ReportID", SqlDbType.Int) { Value = reportId });
                    cmd.Parameters.Add(new SqlParameter("@InspectorRole", SqlDbType.NVarChar, 50) { Value = "Inspector" });

                    conn.Open();
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (!dr.HasRows)
                        {
                            ShowAlert("info", "No Report Found", "This report is not available or still being processed.");
                            RedirectToMyInquiries();
                            return;
                        }

                        if (dr.Read())
                        {
                            // Verify ownership again
                            int clientIdFromReport = SafeGetInt(dr, "ClientID");
                            int currentClientId = Convert.ToInt32(Session["ClientID"]);

                            if (clientIdFromReport != currentClientId)
                            {
                                ShowAlert("error", "Access Denied", "You don't have permission to view this report.");
                                RedirectToMyInquiries();
                                return;
                            }

                            // Basic Information
                            lblQuotationCode.Text = SafeGetString(dr, "QuotationCode");
                            lblInquiryCode.Text = SafeGetString(dr, "InquiryNumber");
                            lblInspectorName.Text = SafeGetString(dr, "InspectorName");

                            // Findings
                            lblInfestationLevel.Text = SafeGetString(dr, "InfestationLevel");
                            lblFindings.Text = SafeGetString(dr, "FindingsDescription");

                            // Format affected areas from JSON array
                            string affectedAreasJson = SafeGetString(dr, "AffectedAreas");
                            lblAffectedAreas.Text = FormatAffectedAreas(affectedAreasJson);

                            lblNotes.Text = SafeGetString(dr, "AdditionalNotes");

                            // Costs
                            lblTravelCost.Text = SafeGetDecimal(dr, "TravelCost").ToString("N2");
                            lblMiscTotal.Text = SafeGetDecimal(dr, "MiscellaneousTotal").ToString("N2");
                            lblGrandTotal.Text = SafeGetDecimal(dr, "TotalEstimatedCost").ToString("N2");

                            // Follow-up
                            lblFollowUpRequired.Text = SafeGetBool(dr, "FollowupRequired") ? "Yes" : "No";
                            lblFollowUpDate.Text = dr["FollowupDate"] != DBNull.Value
                                ? Convert.ToDateTime(dr["FollowupDate"]).ToShortDateString()
                                : "N/A";
                            lblFollowUpReason.Text = SafeGetString(dr, "FollowupReason");

                            // Parse and bind services with proper structure
                            try
                            {
                                string servicesJson = SafeGetString(dr, "SelectedServices");
                                if (!string.IsNullOrEmpty(servicesJson))
                                {
                                    var services = JArray.Parse(servicesJson);
                                    var dtServices = new DataTable();
                                    dtServices.Columns.Add("ServiceName", typeof(string));
                                    dtServices.Columns.Add("SQM", typeof(int));
                                    dtServices.Columns.Add("FlatPrice", typeof(decimal));

                                    foreach (var svc in services)
                                    {
                                        dtServices.Rows.Add(
                                            svc["ServiceName"]?.ToString() ?? "Unknown Service",
                                            Convert.ToInt32(svc["SQM"] ?? 0),
                                            Convert.ToDecimal(svc["FlatPrice"] ?? 0)
                                        );
                                    }

                                    gvServices.DataSource = dtServices;
                                    gvServices.DataBind();
                                }
                            }
                            catch (Exception jsEx)
                            {
                                System.Diagnostics.Debug.WriteLine("Services JSON parse error: " + jsEx.Message);
                            }

                            // Parse and bind misc expenses
                            try
                            {
                                string miscJson = SafeGetString(dr, "MiscellaneousExpenses");
                                if (!string.IsNullOrEmpty(miscJson))
                                {
                                    var miscItems = JArray.Parse(miscJson);
                                    var dtMisc = new DataTable();
                                    dtMisc.Columns.Add("description", typeof(string));
                                    dtMisc.Columns.Add("amount", typeof(decimal));

                                    foreach (var item in miscItems)
                                    {
                                        dtMisc.Rows.Add(
                                            item["description"]?.ToString() ?? "Miscellaneous Item",
                                            Convert.ToDecimal(item["amount"] ?? 0)
                                        );
                                    }

                                    rptMisc.DataSource = dtMisc;
                                    rptMisc.DataBind();
                                }
                            }
                            catch (Exception jsEx)
                            {
                                System.Diagnostics.Debug.WriteLine("Misc JSON parse error: " + jsEx.Message);
                            }

                            // Parse and bind photos (comma-separated paths)
                            // Parse and bind photos (comma-separated paths)
                            string photos = SafeGetString(dr, "InspectionPhotosPath");
                            if (!string.IsNullOrEmpty(photos))
                            {
                                var dtPhotos = new DataTable();
                                dtPhotos.Columns.Add("PhotoPath", typeof(string));
                                foreach (var path in photos.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    string resolvedPath = ResolveUrl(path.Trim());
                                    dtPhotos.Rows.Add(resolvedPath);
                                }

                                rptPhotos.DataSource = dtPhotos;
                                rptPhotos.DataBind();
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadInspectionDetails error: " + ex.Message);
                ShowAlert("error", "Error", "An error occurred while loading the report.");
                RedirectToMyInquiries();
            }
        }

        // ==============================
        // FORMAT AFFECTED AREAS FROM JSON
        // ==============================
        private string FormatAffectedAreas(string json)
        {
            try
            {
                if (string.IsNullOrEmpty(json)) return "No areas specified";

                var areas = JArray.Parse(json);
                var areaList = new System.Collections.Generic.List<string>();

                foreach (var area in areas)
                {
                    areaList.Add(area.ToString());
                }

                return string.Join(", ", areaList);
            }
            catch
            {
                // If not JSON, return as-is
                return json;
            }
        }

        // ==============================
        // HELPER: Safe retrieval from reader
        // ==============================
        private string SafeGetString(SqlDataReader dr, string columnName)
        {
            try
            {
                int idx = dr.GetOrdinal(columnName);
                if (!dr.IsDBNull(idx)) return dr.GetString(idx);
            }
            catch { /* ignore */ }
            return string.Empty;
        }

        private decimal SafeGetDecimal(SqlDataReader dr, string columnName)
        {
            try
            {
                int idx = dr.GetOrdinal(columnName);
                if (!dr.IsDBNull(idx)) return Convert.ToDecimal(dr.GetValue(idx));
            }
            catch { /* ignore */ }
            return 0m;
        }

        private bool SafeGetBool(SqlDataReader dr, string columnName)
        {
            try
            {
                int idx = dr.GetOrdinal(columnName);
                if (!dr.IsDBNull(idx)) return Convert.ToBoolean(dr.GetValue(idx));
            }
            catch { /* ignore */ }
            return false;
        }

        private int SafeGetInt(SqlDataReader dr, string columnName)
        {
            try
            {
                int idx = dr.GetOrdinal(columnName);
                if (!dr.IsDBNull(idx)) return Convert.ToInt32(dr.GetValue(idx));
            }
            catch { /* ignore */ }
            return 0;
        }

        // ==============================
        // REDIRECT TO MY INQUIRIES
        // ==============================
        private void RedirectToMyInquiries()
        {
            string script = @"
                setTimeout(function() {
                    window.location.href = 'MyInquiries.aspx';
                }, 2000);
            ";
            ScriptManager.RegisterStartupScript(this, GetType(), "RedirectScript", script, true);
        }

        // ==============================
        // BOOK SERVICE BUTTON CLICK
        // ==============================
        protected void btnBookService_Click(object sender, EventArgs e)
        {
            try
            {
                string reportIdStr = Request.QueryString["ReportID"];
                if (!string.IsNullOrWhiteSpace(reportIdStr) && int.TryParse(reportIdStr, out int reportId))
                {
                    // ✅ Double-check before redirect - prevent race conditions
                    int clientId = Convert.ToInt32(Session["ClientID"]);
                    if (HasExistingBooking(reportId, clientId))
                    {
                        ShowAlert("warning", "Already Booked", "You have already booked this service.");
                        return;
                    }

                    // Redirect to BookService with ReportID parameter
                    Response.Redirect($"BookService.aspx?ReportID={reportId}", false);
                    Context.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    ShowAlert("error", "Error", "Invalid Report ID.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("btnBookService_Click error: " + ex.Message);
                ShowAlert("error", "Error", "Unable to proceed to booking.");
            }
        }

        // ==============================
        // ✅ NEW: QUICK CHECK FOR EXISTING BOOKING
        // ==============================
        private bool HasExistingBooking(int reportId, int clientId)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM dbo.Bookings
                WHERE ReportID = @ReportID 
                  AND ClientID = @ClientID";

            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@ReportID", SqlDbType.Int) { Value = reportId });
                    cmd.Parameters.Add(new SqlParameter("@ClientID", SqlDbType.Int) { Value = clientId });

                    conn.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("HasExistingBooking error: " + ex.Message);
                return false;
            }
        }

        // ==============================
        // SWEET ALERT HELPER (safe JS encoding)
        // ==============================
        private void ShowAlert(string icon, string title, string text)
        {
            string safeTitle = HttpUtility.JavaScriptStringEncode(title);
            string safeText = HttpUtility.JavaScriptStringEncode(text);

            string script = $@"
                Swal.fire({{
                    icon: '{HttpUtility.JavaScriptStringEncode(icon)}',
                    title: '{safeTitle}',
                    text: '{safeText}',
                    confirmButtonColor: '#1e40af'
                }});
            ";
            ScriptManager.RegisterStartupScript(this, GetType(), "Alert", script, true);
        }
    }
}