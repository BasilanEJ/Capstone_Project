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
                LoadAuditLogs();
            }
        }


        private void LoadAuditLogs()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // ✅ SQL Query: Join AuditLogs with Users to get the Admin Name
                string query = @"
                    SELECT 
                        al.LogID,
                        u.Name AS AdminName,
                        al.Action,
                        al.Timestamp
                    FROM AuditLogs al
                    INNER JOIN Users u ON al.AdminID = u.UserID
                    WHERE u.Role = 'Admin'
                    ORDER BY al.Timestamp DESC;";

                // ✅ DataAdapter + DataTable for binding
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();

                try
                {
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        gvAuditLogs.DataSource = dt;
                        gvAuditLogs.DataBind();
                        lblMessage.Text = "";
                    }
                    else
                    {
                        gvAuditLogs.DataSource = null;
                        gvAuditLogs.DataBind();
                        lblMessage.Text = "⚠ No audit logs found.";
                    }
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "⚠ Error loading audit logs: " + ex.Message;
                    // Optionally log the error to a file or database
                    // LogError("LoadAuditLogs Error", ex);
                }
            }
        }

        /// <summary>
        /// Logs an action into the AuditLogs table.
        /// Call this method after login, edit, delete, etc.
        /// </summary>
        /// <param name="userId">Admin's UserID</param>
        /// <param name="actionDescription">Description of the action</param>
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
                        // Optional error logging
                        // LogError("LogAction Error", ex);
                    }
                }
            }
        }

        /// <summary>
        /// Optional error logging helper method (to file or database)
        /// </summary>
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
                // You can safely ignore errors in error logging to prevent infinite loops.
            }
        }
    }
}
