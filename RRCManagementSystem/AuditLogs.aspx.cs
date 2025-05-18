using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace RRCManagementSystem
{
    public partial class AuditLogs : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DeleteOldLogs(); // ✅ Auto-delete logs older than 30 days
                LoadAuditLogs(); // Load current logs
            }
        }

        private void DeleteOldLogs()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string deleteQuery = @"
                    DELETE FROM AuditLogs
                    WHERE Timestamp < DATEADD(DAY, -30, GETDATE());";

                using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        // Optionally log how many rows were deleted if needed
                    }
                    catch (Exception ex)
                    {
                        LogError("DeleteOldLogs Error", ex);
                    }
                }
            }
        }

        private void LoadAuditLogs()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
    SELECT 
        al.LogID,
        u.Name AS AdminName,
        al.Action,
        al.Timestamp
    FROM AuditLogs al
    INNER JOIN Users u ON al.AdminID = u.UserID
    ORDER BY al.Timestamp DESC;";


                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();

                try
                {
                    da.Fill(dt);
                    gvAuditLogs.DataSource = dt;
                    gvAuditLogs.DataBind();
                    lblMessage.Text = dt.Rows.Count == 0 ? "⚠ No audit logs found." : "";
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "⚠ Error loading audit logs: " + ex.Message;
                    LogError("LoadAuditLogs Error", ex);
                }
            }
        }

        public void LogAction(int userId, string actionDescription)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string insertQuery = @"
                    INSERT INTO AuditLogs (AdminID, Action, Timestamp)
                    VALUES (@UserID, @Action, @Timestamp);";

                using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@Action", actionDescription);
                    cmd.Parameters.AddWithValue("@Timestamp", DateTime.Now);

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        LogError("LogAction Error", ex);
                    }
                }
            }
        }

        private void LogError(string context, Exception ex)
        {
            try
            {
                string message = $"{DateTime.Now}: {context} - {ex.Message}{Environment.NewLine}";
                string logFilePath = Server.MapPath("~/Logs/ErrorLog.txt");

                System.IO.File.AppendAllText(logFilePath, message);
            }
            catch
            {
                // Prevent recursive logging error
            }
        }
    }
}
