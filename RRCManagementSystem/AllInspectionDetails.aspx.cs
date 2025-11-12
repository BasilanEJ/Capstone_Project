using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class AllInspectionDetails : Page
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
                LoadInspectionReports();
            }
        }

        // ==============================
        // LOAD ALL SUBMITTED REPORTS
        // ==============================
        private void LoadInspectionReports()
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
                    WHERE IR.Status = @Status
                    ORDER BY IR.UpdatedAt DESC
                ", conn))
                {
                    cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = "Inspected";
                    conn.Open();

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        gvReports.DataSource = dt;
                        gvReports.DataBind();
                        lblNoData.Visible = false;
                    }
                    else
                    {
                        gvReports.DataSource = null;
                        gvReports.DataBind();
                        lblNoData.Visible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadInspectionReports Error: {ex.Message}");
                lblNoData.Text = $"<div class='text-red-500'><i class='fas fa-exclamation-triangle'></i> Error loading reports: {ex.Message}</div>";
                lblNoData.Visible = true;
            }
        }

        // ==============================
        // HANDLE GRIDVIEW ROW DATABOUND
        // ==============================
        protected void gvReports_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            // Add any additional row customization here if needed
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // You can add hover effects or additional styling here
            }
        }

        // ==============================
        // HANDLE GRIDVIEW COMMANDS
        // ==============================
        protected void gvReports_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                if (e.CommandName == "ViewDetails")
                {
                    int reportId = Convert.ToInt32(e.CommandArgument);
                    Response.Redirect($"InspectionDetails.aspx?ReportID={reportId}", false);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RowCommand Error: {ex.Message}");
            }
        }
    }
}