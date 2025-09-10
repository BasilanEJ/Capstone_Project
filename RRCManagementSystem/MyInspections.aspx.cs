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
            // Ensure only inspectors can access this page
            if (Session["UserID"] == null ||
                !string.Equals(Session["Role"]?.ToString(), "Inspector", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                // Handle the query string when inspector marks an inspection as done
                if (Request.QueryString["done"] != null && int.TryParse(Request.QueryString["done"], out int inspectionId))
                {
                    int inspectorId = Convert.ToInt32(Session["UserID"]);
                    string findings = Request.QueryString["findings"] == null
                        ? null
                        : Server.UrlDecode(Request.QueryString["findings"]).Trim();

                    // Validate findings before saving
                    if (string.IsNullOrWhiteSpace(findings))
                    {
                        // Redirect if no findings were provided
                        Response.Redirect("MyInspections.aspx?err=nofindings");
                        return;
                    }

                    try
                    {
                        // Save the inspection status and findings
                        MarkInspectionAsDone(inspectionId, inspectorId, findings);

                        // Redirect to clear query string and avoid duplicate submissions
                        Response.Redirect("MyInspections.aspx?marked=1");
                        return;
                    }
                    catch (Exception ex)
                    {
                        // Optionally log the error
                        Response.Redirect("MyInspections.aspx?err=save&msg=" + Server.UrlEncode(ex.Message));
                        return;
                    }
                }

                // Load the inspections when page first loads
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
            string statusFilter = ddlStatusFilter.SelectedValue; // "All", "Pending", "Completed"

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

        /// <summary>
        /// Marks an inspection as completed and saves the inspector's findings.
        /// </summary>
        private void MarkInspectionAsDone(int inspectionId, int inspectorId, string findings)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.usp_Inspection_MarkCompleted", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@InspectionID", SqlDbType.Int).Value = inspectionId;
                cmd.Parameters.Add("@InspectorID", SqlDbType.Int).Value = inspectorId;
                cmd.Parameters.Add("@Findings", SqlDbType.NVarChar, -1).Value = findings;

                var affectedParam = cmd.Parameters.Add("@RowsAffected", SqlDbType.Int);
                affectedParam.Direction = ParameterDirection.Output;

                conn.Open();
                cmd.ExecuteNonQuery();

                // Optional: check rows affected to verify update worked
                // int rows = (affectedParam.Value == DBNull.Value) ? 0 : (int)affectedParam.Value;
                // if (rows == 0) throw new InvalidOperationException("Update failed or unauthorized.");
            }
        }
    }
}
