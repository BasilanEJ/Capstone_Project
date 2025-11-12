using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class ArchivedInspectionDetails : Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Check if admin is logged in
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                Response.Redirect("Login.aspx", false);
                return;
            }

            if (!IsPostBack)
            {
                LoadArchivedReports();
            }
        }

        // ==============================
        // LOAD ARCHIVED REPORTS
        // ==============================
        private void LoadArchivedReports()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        IR.ReportID, 
                        IR.QuotationCode, 
                        I.InquiryNumber,
                        U.Name AS InspectorName, 
                        IR.TotalEstimatedCost, 
                        IR.UpdatedAt
                    FROM dbo.InspectionReports IR
                    LEFT JOIN dbo.Inquiries I ON IR.InquiryID = I.InquiryID
                    LEFT JOIN dbo.Users U ON IR.InspectorID = U.UserID
                    WHERE IR.Status = 'Archived'
                    ORDER BY IR.UpdatedAt DESC
                ", conn))
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvArchived.DataSource = dt;
                    gvArchived.DataBind();

                    lblNoData.Visible = (dt.Rows.Count == 0);
                    gvArchived.Visible = (dt.Rows.Count > 0);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadArchivedReports Error: {ex.Message}");
                ShowAlert("error", "Error", "Failed to load archived reports: " + ex.Message);
            }
        }

        // ==============================
        // HANDLE GRIDVIEW COMMANDS
        // ==============================
        protected void gvArchived_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                int reportId = Convert.ToInt32(e.CommandArgument);

                // Check if this is a confirmed action from SweetAlert
                GridViewRow row = ((Control)e.CommandSource).NamingContainer as GridViewRow;
                if (row != null)
                {
                    var hdnConfirmAction = row.FindControl("hdnConfirmAction") as HiddenField;

                    if (hdnConfirmAction != null && !string.IsNullOrEmpty(hdnConfirmAction.Value))
                    {
                        if (hdnConfirmAction.Value == "Restore")
                        {
                            UpdateStatus(reportId, "Inspected", "Report restored successfully!");  
                        }
                        else if (hdnConfirmAction.Value == "Delete")
                        {
                            DeleteReport(reportId);
                        }

                        // Clear the confirmation flag
                        hdnConfirmAction.Value = "";
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RowCommand Error: {ex.Message}");
                ShowAlert("error", "Error", "Failed to process request: " + ex.Message);
            }
        }

        // ==============================
        // RESTORE REPORT STATUS
        // ==============================
        private void UpdateStatus(int reportId, string newStatus, string message)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(@"
                    UPDATE dbo.InspectionReports
                    SET Status = @Status, UpdatedAt = GETDATE()
                    WHERE ReportID = @ReportID
                ", conn))
                {
                    cmd.Parameters.AddWithValue("@Status", newStatus);
                    cmd.Parameters.AddWithValue("@ReportID", reportId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                ShowAlert("success", "Success!", message);
                LoadArchivedReports();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateStatus Error: {ex.Message}");
                ShowAlert("error", "Error", "Failed to restore report: " + ex.Message);
            }
        }

        // ==============================
        // DELETE REPORT PERMANENTLY
        // ==============================
        private void DeleteReport(int reportId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(@"
                    DELETE FROM dbo.InspectionReports 
                    WHERE ReportID = @ReportID
                ", conn))
                {
                    cmd.Parameters.AddWithValue("@ReportID", reportId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                ShowAlert("success", "Deleted!", "The report has been permanently removed.");
                LoadArchivedReports();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DeleteReport Error: {ex.Message}");
                ShowAlert("error", "Error", "Failed to delete report: " + ex.Message);
            }
        }

        // ==============================
        // SWEET ALERT HELPER
        // ==============================
        private void ShowAlert(string icon, string title, string text)
        {
            string script = $@"
                Swal.fire({{
                    icon: '{icon}',
                    title: '{title}',
                    text: '{text.Replace("'", "\\'")}',
                    confirmButtonColor: '#3b82f6'
                }});
            ";
            ScriptManager.RegisterStartupScript(this, GetType(), "Alert", script, true);
        }
    }
}