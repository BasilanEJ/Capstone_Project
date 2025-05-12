using System;
using System.Configuration;
using System.Data.SqlClient;

namespace RRCManagementSystem
{
    public partial class InspectorDashboard : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadInspectionCounts();
            }
        }

        private void LoadInspectionCounts()
        {
            int inspectorId = Convert.ToInt32(Session["UserID"]);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Total Inspections
                SqlCommand totalCmd = new SqlCommand("SELECT COUNT(*) FROM Inspections WHERE InspectorID = @InspectorID", conn);
                totalCmd.Parameters.AddWithValue("@InspectorID", inspectorId);
                lblTotalInspections.Text = totalCmd.ExecuteScalar().ToString();

                // Today's Inspections
                SqlCommand todayCmd = new SqlCommand("SELECT COUNT(*) FROM Inspections WHERE InspectorID = @InspectorID AND CAST(ScheduledDate AS DATE) = CAST(GETDATE() AS DATE)", conn);
                todayCmd.Parameters.AddWithValue("@InspectorID", inspectorId);
                lblTodayInspections.Text = todayCmd.ExecuteScalar().ToString();
            }
        }
    }
}