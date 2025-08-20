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
                // 🔐 Require SuperAdmin
                if (Session["UserID"] == null || Session["Role"]?.ToString() != "SuperAdmin")
                {
                    Response.Redirect("~/Login.aspx");
                    return;
                }

                LoadDashboardStats();        // via SP
                LoadRecentAuditLogs(10);     // via SP
                CheckFailedLoginThreshold(); // via SP
            }
        }

        private void LoadDashboardStats()
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spDashboard_GetStats", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                try
                {
                    conn.Open();
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            lblTotalAdmins.Text = Convert.ToString(rdr["TotalUsers"] ?? "0");
                            lblAuditLogs.Text = Convert.ToString(rdr["TotalAuditLogs"] ?? "0");
                        }
                        else
                        {
                            lblTotalAdmins.Text = "0";
                            lblAuditLogs.Text = "0";
                        }
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

        private void LoadRecentAuditLogs(int topN)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spAuditLogs_GetRecent", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Top", SqlDbType.Int).Value = topN;

                var dt = new DataTable();
                try
                {
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
            const int windowMinutes = 10;
            const int threshold = 50;

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spLoginAttempt_CountRecentFailures", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                // Pass NULL for @IPAddress to count ALL failed attempts in the window
                cmd.Parameters.Add("@IPAddress", SqlDbType.NVarChar, 50).Value = DBNull.Value;
                cmd.Parameters.Add("@WindowMinutes", SqlDbType.Int).Value = windowMinutes;

                try
                {
                    conn.Open();
                    var result = cmd.ExecuteScalar();
                    int failedCount = (result == null || result == DBNull.Value) ? 0 : Convert.ToInt32(result);

                    hfShowModal.Value = failedCount >= threshold ? "1" : "0";
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
                string message = $"{DateTime.Now:u}: [{context}] {ex}{Environment.NewLine}";
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(logPath));
                System.IO.File.AppendAllText(logPath, message);
            }
            catch
            {
                // swallow logging failures
            }
        }
    }
}
