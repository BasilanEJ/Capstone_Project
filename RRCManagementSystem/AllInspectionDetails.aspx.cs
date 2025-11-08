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
            if (!IsPostBack)
                LoadInspectionReports();
        }

        // ==============================
        // LOAD ALL SUBMITTED REPORTS
        // ==============================
        private void LoadInspectionReports()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT IR.ReportID, IR.QuotationCode, I.InquiryNumber, 
                       U.Name AS InspectorName, IR.TotalEstimatedCost, IR.UpdatedAt
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

        protected void gvReports_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewDetails")
            {
                int reportId = Convert.ToInt32(e.CommandArgument);
                Response.Redirect($"InspectionDetails.aspx?ReportID={reportId}");
            }
        }
    }
}
