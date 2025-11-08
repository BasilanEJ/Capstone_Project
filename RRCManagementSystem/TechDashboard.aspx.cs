using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace RRCManagementSystem
{
    public partial class TechDashboard : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Verify Team Leader role safely
            string role = (Session["Role"] as string)?.Trim().Replace(" ", "");

            if (Session["UserID"] == null ||
                string.IsNullOrEmpty(role) ||
                !string.Equals(role, "Headtechnician", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                LoadBookingCounts();
            }
        }


        /// <summary>
        /// Load booking statistics for the team leader's team
        /// </summary>
        private void LoadBookingCounts()
        {
            // Default UI state
            lblTotalBookings.Text = "0";
            lblTodayBookings.Text = "0";
            lblInProgress.Text = "0";
            lblCompleted.Text = "0";

            // Make sure we have a logged-in team leader
            if (Session["UserID"] == null || !int.TryParse(Session["UserID"].ToString(), out int teamLeaderID))
            {
                return;
            }

            try
            {
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spTeamLeader_DashboardCounts", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@TeamLeaderID", SqlDbType.Int).Value = teamLeaderID;

                    con.Open();
                    using (var rdr = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (rdr.Read())
                        {
                            // Get counts from stored procedure
                            int total = rdr["TotalBookings"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["TotalBookings"]);
                            int today = rdr["TodayBookings"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["TodayBookings"]);
                            int inProgress = rdr["InProgress"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["InProgress"]);
                            int completed = rdr["Completed"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["Completed"]);

                            lblTotalBookings.Text = total.ToString();
                            lblTodayBookings.Text = today.ToString();
                            lblInProgress.Text = inProgress.ToString();
                            lblCompleted.Text = completed.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Keep labels at "0" on any error
                System.Diagnostics.Debug.WriteLine($"LoadBookingCounts error: {ex.Message}");
                
                lblTotalBookings.Text = "0";
                lblTodayBookings.Text = "0";
                lblInProgress.Text = "0";
                lblCompleted.Text = "0";
            }
        }
    }
}