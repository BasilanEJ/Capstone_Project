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
        private string currentFilter = "All";

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
                LoadInspections("All");
            }
        }

        #region Load Inspections

        /// <summary>
        /// Load inspections based on filter
        /// </summary>
        private void LoadInspections(string filter)
        {
            try
            {
                int inspectorId = Convert.ToInt32(Session["UserID"]);

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spInspector_GetMyInspections", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@InspectorID", inspectorId);
                    cmd.Parameters.AddWithValue("@StatusFilter", filter);

                    var dt = new DataTable();
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }

                    // Decrypt sensitive fields
                    DecryptDataTable(dt);

                    // Add computed columns
                    AddComputedColumns(dt);

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
            btnFilterCompleted.CssClass = "filter-tab";

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
            else if (btn.ID == "btnFilterCompleted")
            {
                filter = "Completed";
                btnFilterCompleted.CssClass = "filter-tab active";
            }
            else
            {
                btnFilterAll.CssClass = "filter-tab active";
            }

            LoadInspections(filter);
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
                int inquiryId = Convert.ToInt32(e.CommandArgument);

                if (e.CommandName == "StartInspection")
                {
                    StartInspection(inquiryId);
                }
                else if (e.CommandName == "InputReport")
                {
                    Response.Redirect($"InspectorReport.aspx?id={inquiryId}", false);
                }
                else if (e.CommandName == "ViewDetails")
                {
                    Response.Redirect($"InspectionDetails.aspx?id={inquiryId}", false);
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

                ShowSuccess("Inspection started! You can now input your report.");
                LoadInspections("All");
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