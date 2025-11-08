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
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT IR.ReportID, IR.QuotationCode, I.InquiryCode,
                       U.Name AS InspectorName, IR.TotalEstimatedCost, IR.UpdatedAt
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
            }
        }

        // ==============================
        // HANDLE GRIDVIEW COMMANDS
        // ==============================
        protected void gvArchived_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Restore")
            {
                int reportId = Convert.ToInt32(e.CommandArgument);
                UpdateStatus(reportId, "Submitted", "Report restored successfully!");
            }
            else if (e.CommandName == "DeleteReport")
            {
                int reportId = Convert.ToInt32(e.CommandArgument);
                DeleteReport(reportId);
            }
        }

        // ==============================
        // RESTORE REPORT STATUS
        // ==============================
        private void UpdateStatus(int reportId, string newStatus, string message)
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

        // ==============================
        // DELETE REPORT PERMANENTLY
        // ==============================
        private void DeleteReport(int reportId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("DELETE FROM dbo.InspectionReports WHERE ReportID = @ReportID", conn))
            {
                cmd.Parameters.AddWithValue("@ReportID", reportId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            ShowAlert("success", "Deleted!", "The report has been permanently removed.");
            LoadArchivedReports();
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
                    text: '{text}',
                    confirmButtonColor: '#3b82f6'
                }});
            ";
            ScriptManager.RegisterStartupScript(this, GetType(), "Alert", script, true);
        }
    }
}
