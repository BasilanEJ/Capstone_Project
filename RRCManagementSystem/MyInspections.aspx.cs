using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class MyInspections : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"]?.ToString() != "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                if (Request.QueryString["done"] != null && int.TryParse(Request.QueryString["done"], out int id))
                {
                    MarkInspectionAsDone(id);
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
            string statusFilter = ddlStatusFilter.SelectedValue;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
SELECT i.InspectionID, i.ScheduledDate, i.InspectionStatus, i.Remarks, i.CreatedAt,
       q.Name, q.StreetAndUnit, q.Barangay, q.City, q.Region, q.Country
FROM Inspections i
INNER JOIN InquirySimple q ON i.InquiryID = q.InquiryID
WHERE i.InspectorID = @InspectorID";

                if (statusFilter != "All")
                {
                    query += " AND i.InspectionStatus = @Status";
                }

                query += " ORDER BY i.ScheduledDate DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@InspectorID", inspectorId);
                if (statusFilter != "All")
                {
                    cmd.Parameters.AddWithValue("@Status", statusFilter);
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptInspections.DataSource = dt;
                rptInspections.DataBind();
            }
        }

        private void MarkInspectionAsDone(int inspectionId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "UPDATE Inspections SET InspectionStatus = 'Completed' WHERE InspectionID = @InspectionID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@InspectionID", inspectionId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}