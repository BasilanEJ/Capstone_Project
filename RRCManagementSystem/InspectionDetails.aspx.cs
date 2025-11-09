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
    public partial class InspectionDetails : Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Validate ReportID properly
                string reportIdStr = Request.QueryString["ReportID"];
                if (string.IsNullOrWhiteSpace(reportIdStr) || !int.TryParse(reportIdStr, out int reportId))
                {
                    ShowAlert("error", "Invalid Request", "No valid Report ID specified.");
                    return;
                }

                LoadInspectionDetails(reportId);
            }
        }

        // ==============================
        // LOAD INSPECTION DETAILS (SAFE)
        // ==============================
        private void LoadInspectionDetails(int reportId)
        {
            const string sql = @"
                SELECT IR.*, I.InquiryNumber, U.Name AS InspectorName
                FROM dbo.InspectionReports IR
                LEFT JOIN dbo.Inquiries I ON IR.InquiryID = I.InquiryID
                LEFT JOIN dbo.Users U ON IR.InspectorID = U.UserID
                WHERE IR.ReportID = @ReportID
                  AND IR.Status = @InspectedStatus
                  AND ISNULL(U.Role, '') = @InspectorRole
            ";

            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    // Explicit parameter types
                    cmd.Parameters.Add(new SqlParameter("@ReportID", SqlDbType.Int) { Value = reportId });
                    cmd.Parameters.Add(new SqlParameter("@InspectedStatus", SqlDbType.NVarChar, 50) { Value = "Inspected" });
                    cmd.Parameters.Add(new SqlParameter("@InspectorRole", SqlDbType.NVarChar, 50) { Value = "Inspector" });

                    conn.Open();
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (!dr.HasRows)
                        {
                            ShowAlert("info", "No Report Found", "This report may still be in draft or the inspector is invalid.");
                            return;
                        }

                        if (dr.Read())
                        {
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
        // APPROVE BUTTON (SAFE)
        // ==============================
        protected void btnApprove_Click(object sender, EventArgs e)
        {
            if (!TryGetReportId(out int reportId)) return;

            if (!UserIsAuthorizedToManageReports())
            {
                ShowAlert("error", "Unauthorized", "You do not have permission to approve reports.");
                return;
            }

            UpdateReportStatus(reportId, "Approved", "The inspection report has been approved successfully.");
        }

        // ==============================
        // ARCHIVE BUTTON (SAFE)
        // ==============================
        protected void btnArchive_Click(object sender, EventArgs e)
        {
            if (!TryGetReportId(out int reportId)) return;

            if (!UserIsAuthorizedToManageReports())
            {
                ShowAlert("error", "Unauthorized", "You do not have permission to archive reports.");
                return;
            }

            UpdateReportStatus(reportId, "Archived", "The inspection report has been archived successfully.");
        }

        // ==============================
        // STATUS UPDATE HELPER (SAFE + AUDIT)
        // ==============================
        private void UpdateReportStatus(int reportId, string newStatus, string successMessage)
        {
            const string updateSql = @"
                UPDATE dbo.InspectionReports
                SET Status = @Status, UpdatedAt = GETDATE()
                WHERE ReportID = @ReportID
            ";

            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(updateSql, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, 50) { Value = newStatus });
                    cmd.Parameters.Add(new SqlParameter("@ReportID", SqlDbType.Int) { Value = reportId });

                    conn.Open();
                    int rows = cmd.ExecuteNonQuery();

                    if (rows > 0)
                    {
                        // Record audit - use session user id if available
                        int? userId = null;
                        if (Session["UserID"] != null && int.TryParse(Session["UserID"].ToString(), out int uid))
                            userId = uid;

                        AddAuditLog(userId, $"ReportID {reportId} status changed to '{newStatus}' by user {userId ?? 0}");

                        // Success alert with safe encoding of redirect text
                        string redirectUrl = newStatus == "Archived"
                            ? "ArchivedInspectionDetails.aspx"
                            : "AllInspectionDetails.aspx";

                        string safeMessage = HttpUtility.JavaScriptStringEncode(successMessage);
                        string safeRedirect = HttpUtility.JavaScriptStringEncode(redirectUrl);

                        string alertScript = $@"
                            Swal.fire({{
                                icon: 'success',
                                title: 'Success!',
                                text: '{safeMessage}',
                                confirmButtonColor: '#1e40af'
                            }}).then(function() {{
                                window.location.href = '{safeRedirect}';
                            }});
                        ";

                        ScriptManager.RegisterStartupScript(this, GetType(), "StatusUpdate", alertScript, true);
                    }
                    else
                    {
                        ShowAlert("error", "Not Updated", "No report was updated. It may no longer exist or status was already changed.");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("UpdateReportStatus error: " + ex.Message);
                ShowAlert("error", "Error", "Failed to update report status.");
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

        // ==============================
        // AUTHZ: Only Admin / SuperAdmin can change status
        // ==============================
        private bool UserIsAuthorizedToManageReports()
        {
            if (Session["Role"] == null) return false;
            string role = Session["Role"].ToString();
            return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, "SuperAdmin", StringComparison.OrdinalIgnoreCase);
        }

        // ==============================
        // Try parse ReportID from QueryString
        // ==============================
        private bool TryGetReportId(out int reportId)
        {
            reportId = 0;
            string reportIdStr = Request.QueryString["ReportID"];
            if (string.IsNullOrWhiteSpace(reportIdStr) || !int.TryParse(reportIdStr, out reportId))
            {
                ShowAlert("error", "Invalid Request", "No valid Report ID specified.");
                return false;
            }
            return true;
        }

        // ==============================
        // AUDIT LOG (parameterized, safe)
        // ==============================
        private void AddAuditLog(int? userID, string action)
        {
            try
            {
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAudit_Insert", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    var pAdmin = new SqlParameter("@AdminID", SqlDbType.Int);
                    if (userID.HasValue)
                        pAdmin.Value = userID.Value;
                    else
                        pAdmin.Value = DBNull.Value;
                    cmd.Parameters.Add(pAdmin);

                    var pAction = new SqlParameter("@Action", SqlDbType.NVarChar, 255)
                    {
                        Value = action ?? string.Empty
                    };
                    cmd.Parameters.Add(pAction);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // Do not throw - audit failures should not block the user flow.
                System.Diagnostics.Debug.WriteLine("AddAuditLog error: " + ex.Message);
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