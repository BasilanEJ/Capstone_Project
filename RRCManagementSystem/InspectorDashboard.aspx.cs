using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace RRCManagementSystem
{
    public partial class InspectorDashboard : System.Web.UI.Page
    {
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadInspectionCounts();
            }
        }

        private void LoadInspectionCounts()
        {
            // Default UI state
            lblTotalInspections.Text = "0";
            lblTodayInspections.Text = "0";

            // Make sure we actually have a logged-in inspector
            if (Session["UserID"] == null || !int.TryParse(Session["UserID"].ToString(), out int inspectorId))
            {
                // Optionally: redirect to login or show a message
                return;
            }

            try
            {
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.usp_InspectorDashboard_Counts", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@InspectorID", SqlDbType.Int).Value = inspectorId;

                    con.Open();
                    using (var rdr = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (rdr.Read())
                        {
                            // Both columns are INT from the proc
                            int total = rdr["TotalInspections"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["TotalInspections"]);
                            int today = rdr["TodayInspections"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["TodayInspections"]);

                            lblTotalInspections.Text = total.ToString();
                            lblTodayInspections.Text = today.ToString();
                        }
                    }
                }
            }
            catch
            {
                // Keep labels at "0" on any error (or log/show a friendly message)
                lblTotalInspections.Text = "0";
                lblTodayInspections.Text = "0";
            }
        }
    }
}
