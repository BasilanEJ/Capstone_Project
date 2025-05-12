using System;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;

namespace RRCManagementSystem
{
    public partial class GetNotifications : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["ClientID"] == null)
            {
                Response.Clear();
                Response.Write("0|||<div class='notification-item'>⚠️ Session expired. Please login again.</div>");
                Response.End();
                return;
            }

            int clientId = Convert.ToInt32(Session["ClientID"]);
            string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

            // 🔵 Check if user clicked Bell to mark as read
            string action = Request.QueryString["action"];
            if (!string.IsNullOrEmpty(action) && action == "markread")
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string markReadQuery = @"UPDATE Notifications SET Status = 'Read' WHERE UserID = @ClientID AND Status = 'Sent'";
                    SqlCommand cmd = new SqlCommand(markReadQuery, con);
                    cmd.Parameters.AddWithValue("@ClientID", clientId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                Response.Clear();
                Response.Write("OK");
                Response.End();
                return;
            }

            // 🔵 Otherwise, fetch unread notifications
            int notificationCount = 0;
            StringBuilder notificationHtml = new StringBuilder();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // 1️⃣ Count total unread notifications
                string countQuery = @"SELECT COUNT(*) FROM Notifications WHERE UserID = @ClientID AND Status = 'Sent'";
                SqlCommand countCmd = new SqlCommand(countQuery, con);
                countCmd.Parameters.AddWithValue("@ClientID", clientId);

                con.Open();
                object result = countCmd.ExecuteScalar();
                notificationCount = (result != DBNull.Value) ? Convert.ToInt32(result) : 0;
                con.Close();
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // 2️⃣ Fetch unread notification messages
                string fetchQuery = @"SELECT TOP 5 Message, CreatedAt FROM Notifications WHERE UserID = @ClientID AND Status = 'Sent' ORDER BY CreatedAt DESC";
                SqlCommand fetchCmd = new SqlCommand(fetchQuery, con);
                fetchCmd.Parameters.AddWithValue("@ClientID", clientId);

                con.Open();
                SqlDataReader reader = fetchCmd.ExecuteReader();
                while (reader.Read())
                {
                    string message = reader["Message"].ToString();
                    DateTime createdAt = (reader["CreatedAt"] != DBNull.Value) ? Convert.ToDateTime(reader["CreatedAt"]) : DateTime.Now;

                    notificationHtml.Append($"<div class='notification-item'><i class='fas fa-bell'></i> {message} <br/><small class='text-muted'>{createdAt:MMM dd, yyyy hh:mm tt}</small></div>");
                }
                con.Close();
            }

            if (notificationCount == 0)
            {
                notificationHtml.Append("<div class='notification-item'>✅ No new notifications.</div>");
            }

            // 🔵 Final output for Ajax
            Response.Clear();
            Response.Write($"{notificationCount}|||{notificationHtml}");
            Response.End();
        }
    }
}