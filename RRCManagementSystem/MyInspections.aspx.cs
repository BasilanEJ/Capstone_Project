using RRCManagementSystem.Helpers;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class MyInspections : System.Web.UI.Page
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Check authentication
            if (Session["UserID"] == null || Session["Role"]?.ToString() != "Inspector")
            {
                Response.Redirect("~/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                LoadInspections("All", "");
            }
            else
            {
                // Handle search postback
                if (Request.Form["__EVENTTARGET"] == txtSearch.UniqueID)
                {
                    string currentFilter = GetCurrentFilter();
                    string searchTerm = txtSearch.Text.Trim();
                    LoadInspections(currentFilter, searchTerm);
                }
            }
        }

        #region Load Inspections

        /// <summary>
        /// Load inspections with their report status and search functionality
        /// </summary>
        private void LoadInspections(string filter, string searchTerm)
        {
            try
            {
                int inspectorId = Convert.ToInt32(Session["UserID"]);

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(@"
                    SELECT 
                        i.InquiryID,
                        i.InquiryNumber,
                        i.PestType,
                        i.Status,
                        i.Urgency,
                        i.InspectionDate,
                        i.InspectionTime,
                        i.ProblemDescription,
                        i.InspectionReportPath,
                        i.AssignedAt,
                        i.AddressEnc,
                        i.BarangayEnc,
                        i.CityEnc,
                        i.RegionEnc,
                        i.LandmarkEnc,
                        c.FirstName AS ClientFirstName,
                        c.MiddleName AS ClientMiddleName,
                        c.LastName AS ClientLastName,
                        c.EmailEnc AS ClientEmailEnc,
                        c.ContactEnc AS ClientContactEnc,
                        ir.ReportID,
                        ir.QuotationCode,
                        ir.Status AS ReportStatus,
                        ir.CreatedAt AS ReportCreatedAt
                    FROM dbo.Inquiries i
                    INNER JOIN dbo.Clients c ON i.ClientID = c.ClientID
                    LEFT JOIN dbo.InspectionReports ir ON i.InquiryID = ir.InquiryID
                    WHERE i.AssignedInspectorID = @InspectorID
                        AND i.IsDeleted = 0
                        AND (@Filter = 'All' 
                            OR (@Filter = 'Assigned' AND i.Status = 'Assigned')
                            OR (@Filter = 'In Progress' AND i.Status = 'In Progress')
                            OR (@Filter = 'Inspected' AND i.Status = 'Inspected'))
                    ORDER BY 
                        CASE 
                            WHEN i.Urgency = 'Emergency' THEN 1
                            WHEN i.Urgency = 'High' THEN 2
                            WHEN i.Urgency = 'Medium' THEN 3
                            ELSE 4
                        END,
                        i.InspectionDate ASC
                ", conn))
                {
                    cmd.Parameters.AddWithValue("@InspectorID", inspectorId);
                    cmd.Parameters.AddWithValue("@Filter", filter);

                    var dt = new DataTable();
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }

                    // Decrypt sensitive fields
                    DecryptDataTable(dt);

                    // Add computed columns
                    AddComputedColumns(dt);

                    // Apply search filter if provided
                    if (!string.IsNullOrWhiteSpace(searchTerm))
                    {
                        dt = FilterDataTableBySearch(dt, searchTerm);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        rptInspections.DataSource = dt;
                        rptInspections.DataBind();
                        pnlEmptyState.Visible = false;
                    }
                    else
                    {
                        rptInspections.DataSource = null;
                        rptInspections.DataBind();
                        pnlEmptyState.Visible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadInspections Error: {ex.Message}");
                ShowError("Error loading inspections.");
            }
        }

        /// <summary>
        /// Filter DataTable by search term
        /// </summary>
        private DataTable FilterDataTableBySearch(DataTable dt, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return dt;

            searchTerm = searchTerm.ToLower();
            DataTable filteredDt = dt.Clone();

            foreach (DataRow row in dt.Rows)
            {
                bool matchFound = false;

                // Search in Inquiry Number
                if (row["InquiryNumber"]?.ToString().ToLower().Contains(searchTerm) == true)
                    matchFound = true;

                // Search in Client Name
                if (row["ClientName"]?.ToString().ToLower().Contains(searchTerm) == true)
                    matchFound = true;

                // Search in Pest Type
                if (row["PestType"]?.ToString().ToLower().Contains(searchTerm) == true)
                    matchFound = true;

                // Search in Full Address
                if (row["FullAddress"]?.ToString().ToLower().Contains(searchTerm) == true)
                    matchFound = true;

                // Search in Problem Description
                if (row["ProblemDescription"]?.ToString().ToLower().Contains(searchTerm) == true)
                    matchFound = true;

                // Search in Client Email
                if (row["ClientEmail"]?.ToString().ToLower().Contains(searchTerm) == true)
                    matchFound = true;

                // Search in Client Contact
                if (row["ClientContact"]?.ToString().ToLower().Contains(searchTerm) == true)
                    matchFound = true;

                // Search in Quotation Code
                if (row["QuotationCode"]?.ToString().ToLower().Contains(searchTerm) == true)
                    matchFound = true;

                if (matchFound)
                {
                    filteredDt.ImportRow(row);
                }
            }

            return filteredDt;
        }

        /// <summary>
        /// Get current active filter
        /// </summary>
        private string GetCurrentFilter()
        {
            if (btnFilterAssigned.CssClass.Contains("active"))
                return "Assigned";
            if (btnFilterInProgress.CssClass.Contains("active"))
                return "In Progress";
            if (btnFilterInspected.CssClass.Contains("active"))
                return "Inspected";

            return "All";
        }

        /// <summary>
        /// Decrypt sensitive data in DataTable
        /// </summary>
        private void DecryptDataTable(DataTable dt)
        {
            foreach (DataRow row in dt.Rows)
            {
                // Decrypt client email
                if (row["ClientEmailEnc"] != DBNull.Value)
                {
                    row["ClientEmailEnc"] = DecryptField(row["ClientEmailEnc"]);
                }

                // Decrypt client contact
                if (row["ClientContactEnc"] != DBNull.Value)
                {
                    row["ClientContactEnc"] = DecryptField(row["ClientContactEnc"]);
                }

                // Decrypt address fields
                if (row["AddressEnc"] != DBNull.Value)
                {
                    row["AddressEnc"] = DecryptField(row["AddressEnc"]);
                }

                if (row["BarangayEnc"] != DBNull.Value)
                {
                    row["BarangayEnc"] = DecryptField(row["BarangayEnc"]);
                }

                if (row["CityEnc"] != DBNull.Value)
                {
                    row["CityEnc"] = DecryptField(row["CityEnc"]);
                }

                if (row["RegionEnc"] != DBNull.Value)
                {
                    row["RegionEnc"] = DecryptField(row["RegionEnc"]);
                }

                if (row["LandmarkEnc"] != DBNull.Value)
                {
                    row["LandmarkEnc"] = DecryptField(row["LandmarkEnc"]);
                }
            }
        }

        /// <summary>
        /// Add computed columns to DataTable
        /// </summary>
        private void AddComputedColumns(DataTable dt)
        {
            dt.Columns.Add("ClientName", typeof(string));
            dt.Columns.Add("ClientEmail", typeof(string));
            dt.Columns.Add("ClientContact", typeof(string));
            dt.Columns.Add("FullAddress", typeof(string));

            foreach (DataRow row in dt.Rows)
            {
                // Build client name
                string firstName = row["ClientFirstName"]?.ToString() ?? "";
                string middleName = row["ClientMiddleName"]?.ToString() ?? "";
                string lastName = row["ClientLastName"]?.ToString() ?? "";
                row["ClientName"] = $"{firstName} {middleName} {lastName}".Trim();

                // Set decrypted values
                row["ClientEmail"] = row["ClientEmailEnc"];
                row["ClientContact"] = row["ClientContactEnc"];

                // Build full address
                string street = row["AddressEnc"]?.ToString() ?? "";
                string barangay = row["BarangayEnc"]?.ToString() ?? "";
                string city = row["CityEnc"]?.ToString() ?? "";
                string region = row["RegionEnc"]?.ToString() ?? "";
                string landmark = row["LandmarkEnc"]?.ToString() ?? "";

                var addressParts = new System.Collections.Generic.List<string>();
                if (!string.IsNullOrEmpty(street)) addressParts.Add(street);
                if (!string.IsNullOrEmpty(barangay)) addressParts.Add(barangay);
                if (!string.IsNullOrEmpty(city)) addressParts.Add(city);
                if (!string.IsNullOrEmpty(region)) addressParts.Add(region);

                row["FullAddress"] = string.Join(", ", addressParts);

                if (!string.IsNullOrEmpty(landmark))
                {
                    row["FullAddress"] += $" (Near: {landmark})";
                }
            }
        }

        #endregion

        #region Filter Buttons

        /// <summary>
        /// Handle filter button clicks
        /// </summary>
        protected void FilterInspections(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            string filter = "All";

            // Reset all tabs
            btnFilterAll.CssClass = "filter-tab";
            btnFilterAssigned.CssClass = "filter-tab";
            btnFilterInProgress.CssClass = "filter-tab";
            btnFilterInspected.CssClass = "filter-tab";

            // Set active tab and filter
            if (btn.ID == "btnFilterAssigned")
            {
                filter = "Assigned";
                btnFilterAssigned.CssClass = "filter-tab active";
            }
            else if (btn.ID == "btnFilterInProgress")
            {
                filter = "In Progress";
                btnFilterInProgress.CssClass = "filter-tab active";
            }
            else if (btn.ID == "btnFilterInspected")
            {
                filter = "Inspected";
                btnFilterInspected.CssClass = "filter-tab active";
            }
            else
            {
                btnFilterAll.CssClass = "filter-tab active";
            }

            // Get current search term
            string searchTerm = txtSearch.Text.Trim();
            LoadInspections(filter, searchTerm);
        }

        #endregion

        #region Repeater Events

        /// <summary>
        /// Handle repeater item commands
        /// </summary>
        protected void rptInspections_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            try
            {
                if (e.CommandName == "StartInspection")
                {
                    int inquiryId = Convert.ToInt32(e.CommandArgument);
                    StartInspection(inquiryId);
                }
                else if (e.CommandName == "CreateReport")
                {
                    int inquiryId = Convert.ToInt32(e.CommandArgument);
                    Response.Redirect($"InspectorReport.aspx?id={inquiryId}", false);
                }
                else if (e.CommandName == "ViewReport")
                {
                    string[] args = e.CommandArgument.ToString().Split('|');
                    int inquiryId = Convert.ToInt32(args[0]);
                    int reportId = Convert.ToInt32(args[1]);
                    Response.Redirect($"InspectorViewReport.aspx?id={inquiryId}&reportId={reportId}", false);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ItemCommand Error: {ex.Message}");
                ShowError("Error processing request.");
            }
        }

        #endregion

        #region Actions

        /// <summary>
        /// Start inspection (change status to In Progress)
        /// </summary>
        private void StartInspection(int inquiryId)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(@"
                    UPDATE dbo.Inquiries
                    SET Status = 'In Progress',
                        UpdatedAt = GETDATE()
                    WHERE InquiryID = @InquiryID
                ", conn))
                {
                    cmd.Parameters.AddWithValue("@InquiryID", inquiryId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                ShowSuccess("Inspection started! You can now create your report.");

                // Reload with current filter and search
                string currentFilter = GetCurrentFilter();
                string searchTerm = txtSearch.Text.Trim();
                LoadInspections(currentFilter, searchTerm);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StartInspection Error: {ex.Message}");
                ShowError("Failed to start inspection.");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Decrypt encrypted field
        /// </summary>
        private string DecryptField(object value)
        {
            if (value == null || value == DBNull.Value)
                return string.Empty;

            string encrypted = value.ToString();
            if (string.IsNullOrEmpty(encrypted))
                return string.Empty;

            try
            {
                return AESHelper.DecryptField(encrypted);
            }
            catch
            {
                return "[Decryption Error]";
            }
        }

        /// <summary>
        /// Get urgency CSS class
        /// </summary>
        public string GetUrgencyClass(object urgency)
        {
            if (urgency == null || urgency == DBNull.Value)
                return "";

            string urgencyStr = urgency.ToString().ToLower();

            if (urgencyStr == "emergency")
                return "urgent";
            if (urgencyStr == "high")
                return "high";

            return "";
        }

        /// <summary>
        /// Check if report is draft
        /// </summary>
        public bool IsReportDraft(object reportStatus)
        {
            if (reportStatus == null || reportStatus == DBNull.Value)
                return false;

            return reportStatus.ToString() == "Draft";
        }

        /// <summary>
        /// Render status badges dynamically
        /// </summary>
        public string RenderStatusBadges(object inquiryStatus, object reportStatus, object quotationCode)
        {
            var sb = new StringBuilder();

            // Inquiry Status Badge
            string status = inquiryStatus?.ToString() ?? "Assigned";
            string statusClass = GetStatusClass(inquiryStatus);

            sb.Append($"<span class='status-badge status-{statusClass}'>");
            sb.Append($"<i class='fas fa-circle'></i> {status}");
            sb.Append("</span>");

            // Report Status Badge (if exists)
            if (reportStatus != null && reportStatus != DBNull.Value)
            {
                string repStatus = reportStatus.ToString();
                string code = quotationCode?.ToString() ?? "";

                if (repStatus == "Inspected")
                {
                    sb.Append(" <span class='status-badge status-inspected'>");
                    sb.Append($"<i class='fas fa-check-circle'></i> SUBMITTED");
                    if (!string.IsNullOrEmpty(code))
                    {
                        sb.Append($" ({code})");
                    }
                    sb.Append("</span>");
                }
                else if (repStatus == "Approved")
                {
                    sb.Append(" <span class='status-badge status-completed'>");
                    sb.Append($"<i class='fas fa-check-circle'></i> APPROVED");
                    if (!string.IsNullOrEmpty(code))
                    {
                        sb.Append($" ({code})");
                    }
                    sb.Append("</span>");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Get status CSS class
        /// </summary>
        public string GetStatusClass(object status)
        {
            if (status == null || status == DBNull.Value)
                return "assigned";

            string statusStr = status.ToString().ToLower().Replace(" ", "-");

            switch (statusStr)
            {
                case "assigned":
                    return "assigned";
                case "in-progress":
                    return "in-progress";
                case "inspected":
                    return "inspected";
                case "completed":
                    return "completed";
                default:
                    return "assigned";
            }
        }

        /// <summary>
        /// Render client uploaded photos
        /// </summary>
        public string RenderClientPhotos(object imagePaths)
        {
            if (imagePaths == null || imagePaths == DBNull.Value || string.IsNullOrEmpty(imagePaths.ToString()))
                return "";

            var paths = imagePaths.ToString().Split(',');
            var sb = new StringBuilder();

            sb.Append("<div class='mt-4'>");
            sb.Append("<h4 class='text-sm font-bold text-gray-700 mb-2'>");
            sb.Append("<i class='fas fa-images'></i> Client Photos</h4>");
            sb.Append("<div class='images-preview'>");

            foreach (var path in paths)
            {
                if (!string.IsNullOrWhiteSpace(path))
                {
                    string imgUrl = ResolveUrl(path.Trim());
                    sb.Append($"<img src='{imgUrl}' alt='Client Photo' onclick='window.open(\"{imgUrl}\", \"_blank\")' />");
                }
            }

            sb.Append("</div></div>");

            return sb.ToString();
        }

        #endregion

        #region UI Messages

        /// <summary>
        /// Show error message
        /// </summary>
        private void ShowError(string message)
        {
            string script = $@"
                Swal.fire({{
                    icon: 'error',
                    title: 'Error',
                    text: '{message.Replace("'", "\\'")}',
                    confirmButtonColor: '#ef4444'
                }});
            ";
            ScriptManager.RegisterStartupScript(this, GetType(), "ShowError", script, true);
        }

        /// <summary>
        /// Show success message
        /// </summary>
        private void ShowSuccess(string message)
        {
            string script = $@"
                Swal.fire({{
                    icon: 'success',
                    title: 'Success!',
                    text: '{message.Replace("'", "\\'")}',
                    confirmButtonColor: '#10b981'
                }});
            ";
            ScriptManager.RegisterStartupScript(this, GetType(), "ShowSuccess", script, true);
        }

        #endregion
    }
}