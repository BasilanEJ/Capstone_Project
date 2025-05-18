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
                if (Session["UserID"] == null || Session["Role"]?.ToString() != "SuperAdmin")
                {
                    Response.Redirect("~/Login.aspx");
                    return;
                }

                LoadDashboardStats();
                LoadRecentAuditLogs();
                CheckFailedLoginThreshold();
            }
        }

        private void LoadDashboardStats()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // ✅ Total Admin Accounts
                    string totalAdminsQuery = "SELECT COUNT(*) FROM Users WHERE LOWER(Role) = 'admin' AND Status = 'Active'";
                    using (SqlCommand cmdAdmins = new SqlCommand(totalAdminsQuery, conn))
                    {
                        object result = cmdAdmins.ExecuteScalar();
                        lblTotalAdmins.Text = result != null ? result.ToString() : "0";
                    }

                    // ✅ Total Audit Logs
                    string totalLogsQuery = "SELECT COUNT(*) FROM AuditLogs";
                    using (SqlCommand cmdLogs = new SqlCommand(totalLogsQuery, conn))
                    {
                        object result = cmdLogs.ExecuteScalar();
                        lblAuditLogs.Text = result != null ? result.ToString() : "0";
                    }
                }
                catch (Exception ex)
                {
                    lblTotalAdmins.Text = "0";
                    lblAuditLogs.Text = "0";
                    LogError("LoadDashboardStats", ex);
                }
            }
        }

        private void LoadRecentAuditLogs()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
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
                catch (Exception ex)
                {
                    LogError("LoadRecentAuditLogs", ex);
                }
            }
        }

        private void CheckFailedLoginThreshold()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string query = @"
                        SELECT COUNT(*) 
                        FROM LoginAttempts 
                        WHERE IsSuccess = 0 
                          AND AttemptTime > DATEADD(MINUTE, -10, GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        int failedAttempts = Convert.ToInt32(cmd.ExecuteScalar());
                        if (failedAttempts >= 50)
                        {
                            hfShowModal.Value = "1"; // Trigger SweetAlert
                        }
                        else
                        {
                            hfShowModal.Value = "0";
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError("CheckFailedLoginThreshold", ex);
                    hfShowModal.Value = "0";
                }
            }
        }

        private void LogError(string context, Exception ex)
        {
            try
            {
                string logPath = Server.MapPath("~/Logs/ErrorLog.txt");
                string message = $"{DateTime.Now}: [{context}] {ex.Message}{Environment.NewLine}";

                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(logPath));
                System.IO.File.AppendAllText(logPath, message);
            }
            catch
            {
                // Ignore logging failures
            }
        }
    }
}
