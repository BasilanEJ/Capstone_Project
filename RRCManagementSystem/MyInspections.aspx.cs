using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class MyInspections : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Require inspector session
            if (Session["UserID"] == null || !string.Equals(Session["Role"]?.ToString(), "Inspector", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                // Optional: handle ?done=123 to mark an inspection as completed (ownership enforced in SP)
                if (Request.QueryString["done"] != null && int.TryParse(Request.QueryString["done"], out int id))
                {
                    int inspectorId = Convert.ToInt32(Session["UserID"]);
                    MarkInspectionAsDone(id, inspectorId);
                }

                LoadMyInspections();
            }
        }

        protected void ddlStatusFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadMyInspections();
        }

        private void LoadMyInspections()
        {
            int inspectorId = Convert.ToInt32(Session["UserID"]);
            string statusFilter = ddlStatusFilter.SelectedValue; // expect values like "All", "Scheduled", "Completed", etc.

            using (var conn = new SqlConnection(connectionString))
            using (var da = new SqlDataAdapter("dbo.usp_Inspections_ListByInspector", conn))
            {
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.Add("@InspectorID", SqlDbType.Int).Value = inspectorId;
                da.SelectCommand.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value =
                    string.IsNullOrWhiteSpace(statusFilter) ? "All" : statusFilter;

                var dt = new DataTable();
                da.Fill(dt);

                rptInspections.DataSource = dt;
                rptInspections.DataBind();
            }
        }

        private void MarkInspectionAsDone(int inspectionId, int inspectorId)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.usp_Inspection_MarkCompleted", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@InspectionID", SqlDbType.Int).Value = inspectionId;
                cmd.Parameters.Add("@InspectorID", SqlDbType.Int).Value = inspectorId; // enforce ownership

                var affectedParam = cmd.Parameters.Add("@RowsAffected", SqlDbType.Int);
                affectedParam.Direction = ParameterDirection.Output;

                conn.Open();
                cmd.ExecuteNonQuery();

                // Optional feedback (no UI elements shown here; add a label or toast if you want)
                // int rows = (affectedParam.Value == DBNull.Value) ? 0 : (int)affectedParam.Value;
            }
        }
    }
}
