using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace RRCManagementSystem
{
    public partial class SuperAdminDashboard : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Session check to make sure SuperAdmin is logged in
                //if (Session["SuperAdminID"] == null)
                // {
                //     Response.Redirect("~/Login.aspx");
                // }

                LoadDashboardStats();
                LoadRecentAuditLogs();
            }
        }

        private void LoadDashboardStats()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // ✅ Total Admins (Case-insensitive check)
                    string totalAdminsQuery = "SELECT COUNT(*) FROM Users WHERE LOWER(Role) = 'admin' AND Status = 'Active'";

                    using (SqlCommand cmdAdmins = new SqlCommand(totalAdminsQuery, conn))
                    {
                        object result = cmdAdmins.ExecuteScalar();
                        lblTotalAdmins.Text = result != null ? result.ToString() : "0";
                    }

                    // ✅ Total Audit Logs
                    string totalLogsQuery = "IF OBJECT_ID('dbo.AuditLogs', 'U') IS NOT NULL SELECT COUNT(*) FROM AuditLogs ELSE SELECT 0";
                    using (SqlCommand cmdLogs = new SqlCommand(totalLogsQuery, conn))
                    {
                        object result = cmdLogs.ExecuteScalar();
                        lblAuditLogs.Text = result != null ? result.ToString() : "0";
                    }

                }

                // ✅ Total Reports (Optional - check if table exists)

                catch (Exception ex)
                {
                    // Show zero values in case of error
                    lblTotalAdmins.Text = "0";
                    lblAuditLogs.Text = "0";


                    // Optional: Log the actual error for troubleshooting
                    LogError("Dashboard Stats Load Error", ex);
                }
            }
        }



        private void LogError(string context, Exception ex)
        {
            try
            {
                string logPath = Server.MapPath("~/Logs/ErrorLog.txt");
                string message = $"{DateTime.Now}: {context} - {ex.Message}{Environment.NewLine}";

                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(logPath)); // Ensure directory exists
                System.IO.File.AppendAllText(logPath, message);
            }
            catch
            {
                // Suppress logging errors to avoid breaking the app
            }
        }


        private void LoadRecentAuditLogs()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT TOP 10
                        LogID,
                        (SELECT Name FROM Users WHERE Users.UserID = AuditLogs.AdminID) AS AdminName,
                        Action,
                        Timestamp
                    FROM AuditLogs
                    ORDER BY Timestamp DESC";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();

                da.Fill(dt);

                gvAuditLogs.DataSource = dt;
                gvAuditLogs.DataBind();
            }
        }
    }
}