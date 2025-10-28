using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;
using System.Web.Script.Serialization;

namespace RRCManagementSystem
{
    public partial class GetNotificationInspector : System.Web.UI.Page
    {
        private static readonly string Cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.ContentType = "application/json";

            if (Session["UserID"] == null)
            {
                Response.Write("{\"unread\":0,\"notifications\":[]}");
                HttpContext.Current.ApplicationInstance.CompleteRequest();
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            // Handle mark as read action
            if (Request.QueryString["action"] == "markread")
            {
                MarkAllAsRead(userId);
                Response.Write("{\"success\":true}");
                HttpContext.Current.ApplicationInstance.CompleteRequest();
                return;
            }

            // Return JSON notifications
            var result = new
            {
                unread = GetUnreadCount(userId),
                notifications = GetLatestNotifications(userId)
            };

            string json = new JavaScriptSerializer().Serialize(result);
            Response.Write(json);
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }
        private int GetUnreadCount(int userId)
        {
            try
            {
                using (var conn = new SqlConnection(Cs))
                using (var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Notifications WHERE UserID=@UserID AND (IsRead=0 OR IsRead IS NULL)", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    conn.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                }
            }
            catch
            {
                return 0;
            }
        }

        private List<object> GetLatestNotifications(int userId)
        {
            var list = new List<object>();
            try
            {
                using (var conn = new SqlConnection(Cs))
                using (var cmd = new SqlCommand(
                    @"SELECT TOP 10 NotificationID, Title, Body, Url, IsRead, CreatedAt
                      FROM Notifications 
                      WHERE UserID=@UserID 
                      ORDER BY CreatedAt DESC", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string rawUrl = reader["Url"] != DBNull.Value ? reader["Url"].ToString() : "";
                            if (!string.IsNullOrEmpty(rawUrl))
                            {
                                if (rawUrl.StartsWith("~/"))
                                {
                                    rawUrl = rawUrl.Replace("~/", "/");
                                }

                                if (!rawUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                                {
                                    rawUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + rawUrl;
                                }
                            }

                            list.Add(new
                            {
                                id = reader["NotificationID"],
                                title = reader["Title"].ToString(),
                                body = reader["Body"] != DBNull.Value ? reader["Body"].ToString() : "",
                                date = Convert.ToDateTime(reader["CreatedAt"]).ToString("MMM dd, yyyy hh:mm tt"),
                                isRead = reader["IsRead"] != DBNull.Value && Convert.ToBoolean(reader["IsRead"]),
                                url = !string.IsNullOrEmpty(rawUrl) ? rawUrl : "#"
                            });
                        }

                    }
                }
            }
            catch { }
            return list;
        }

        private void MarkAllAsRead(int userId)
        {
            try
            {
                using (var conn = new SqlConnection(Cs))
                using (var cmd = new SqlCommand(
                    "UPDATE Notifications SET IsRead=1 WHERE UserID=@UserID AND (IsRead=0 OR IsRead IS NULL)", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch { }
        }
    }
}
