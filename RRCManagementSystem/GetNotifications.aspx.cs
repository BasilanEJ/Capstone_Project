using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class GetNotifications : Page
    {
        private readonly string cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // === Setup response headers ===
            Response.Clear();
            Response.ContentType = "text/plain";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Expires = -1;

            try
            {
                // === Step 1: Validate session ===
                if (Session["ClientID"] == null)
                {
                    WriteAndEnd("0|||<div class='text-muted small p-2'>Not signed in.</div>");
                    return;
                }

                int clientId = Convert.ToInt32(Session["ClientID"], CultureInfo.InvariantCulture);

                // === Step 2: Check for 'action' query string ===
                string action = (Request.QueryString["action"] ?? "").Trim().ToLowerInvariant();

                if (action == "markread")
                {
                    // Mark notifications as read
                    MarkAllRead(clientId);

                    // Immediately return after marking
                    WriteAndEnd("0|||");
                    return;
                }

                // === Step 3: Default behavior - Get notifications ===
                int unread = 0;
                string html = BuildListHtml(clientId, ref unread);

                // === Step 4: Include unread chat count (optional) ===
                int unreadChat = GetUnreadChatCount(clientId);
                if (unreadChat > 0)
                {
                    unread += 1; // one grouped chat notification

                    string chatHtml =
                        "<a href='ChatWithAdmin.aspx' class='text-decoration-none d-block'>" +
                        "  <div class='d-flex gap-2 p-2 border-bottom'>" +
                        "    <i class='fa-solid fa-message mt-1'></i>" +
                        "    <div class='flex-grow-1'>" +
                        "      <div class='fw-semibold'>New message" + (unreadChat > 1 ? "s" : "") + " from Admin</div>" +
                        "      <div class='small text-muted'>" + unreadChat + " unread</div>" +
                        "    </div>" +
                        "  </div>" +
                        "</a>";

                    // Prepend chat notification at the top
                    html = chatHtml + html;
                }

                // === Step 5: Return unread count and HTML ===
                WriteAndEnd(unread.ToString(CultureInfo.InvariantCulture) + "|||" + html);
            }
            catch (Exception ex)
            {
                string safe = HttpUtility.HtmlEncode(ex.Message);
                WriteAndEnd("0|||<div class='text-danger small p-2'>Notifications error: " + safe + "</div>");
            }
        }

        // ===== Helper: Mark all notifications as read =====
        private void MarkAllRead(int clientId)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.usp_Notifications_MarkAllRead", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ===== Helper: Build notification list HTML =====
        private string BuildListHtml(int clientId, ref int unread)
        {
            using (var con = new SqlConnection(cs))
            {
                con.Open();

                // === 1) Get unread count ===
                using (var cmdCount = new SqlCommand(@"
                    SELECT COUNT(*) 
                    FROM dbo.Notifications 
                    WHERE ClientID = @ID AND (IsRead = 0 OR IsRead IS NULL);", con))
                {
                    cmdCount.Parameters.Add("@ID", SqlDbType.Int).Value = clientId;
                    object o = cmdCount.ExecuteScalar();
                    unread = (o == null || o == DBNull.Value) ? 0 : Convert.ToInt32(o, CultureInfo.InvariantCulture);
                }

                // === 2) Get latest 10 notifications ===
                using (var cmd = new SqlCommand(@"
                    SELECT TOP 10
                           COALESCE(NULLIF(LTRIM(RTRIM([Title])), ''), [Message]) AS DisplayTitle,
                           [Body],
                           [Type],
                           [Url],
                           [CreatedAt],
                           CASE WHEN IsRead IS NULL THEN 0 ELSE IsRead END AS IsRead
                    FROM dbo.Notifications
                    WHERE ClientID = @ID
                    ORDER BY CreatedAt DESC;", con))
                {
                    cmd.Parameters.Add("@ID", SqlDbType.Int).Value = clientId;

                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.HasRows)
                            return "<div class='text-muted small p-2'>No notifications.</div>";

                        var sb = new StringBuilder();
                        while (r.Read())
                        {
                            string title = HttpUtility.HtmlEncode(r["DisplayTitle"] as string ?? "");
                            string body = HttpUtility.HtmlEncode(r["Body"] as string ?? "");
                            string type = (r["Type"] as string ?? "").Trim().ToLowerInvariant();
                            string url = HttpUtility.HtmlEncode(r["Url"] as string ?? "#");
                            bool isRead = r["IsRead"] != DBNull.Value && Convert.ToBoolean(r["IsRead"], CultureInfo.InvariantCulture);

                            DateTime created = (r["CreatedAt"] == DBNull.Value)
                                ? DateTime.UtcNow
                                : Convert.ToDateTime(r["CreatedAt"], CultureInfo.InvariantCulture);

                            // Icon mapping
                            string icon = "fa-circle-info";
                            if (type == "booking") icon = "fa-calendar-check";
                            else if (type == "payment") icon = "fa-credit-card";
                            else if (type == "quotation" || type == "quote") icon = "fa-file-invoice";

                            // Apply CSS for read/unread
                            string readCls = isRead ? "opacity-75" : "fw-semibold";

                            string when = created.ToLocalTime()
                                .ToString("MMM dd, yyyy hh:mm tt", CultureInfo.InvariantCulture);

                            // Build each notification row
                            sb.Append(
                                "<a href='" + url + "' class='text-decoration-none d-block'>" +
                                "  <div class='d-flex gap-2 p-2 border-bottom'>" +
                                "    <i class='fa-solid " + icon + " mt-1'></i>" +
                                "    <div class='flex-grow-1'>" +
                                "      <div class='" + readCls + "'>" + title + "</div>" +
                                (string.IsNullOrEmpty(body) ? "" : "      <div class='small text-muted'>" + body + "</div>") +
                                "      <div class='small text-muted'>" + when + "</div>" +
                                "    </div>" +
                                "  </div>" +
                                "</a>"
                            );
                        }

                        return sb.ToString();
                    }
                }
            }
        }

        private int GetUnreadChatCount(int clientId)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(@"
                SELECT COUNT(*) 
                FROM dbo.Messages
                WHERE ReceiverType = 'Client' AND ReceiverID = @ClientID
                  AND SenderType = 'Admin'
                  AND (Status IS NULL OR Status <> 'Read');", con))
            {
                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                con.Open();
                object o = cmd.ExecuteScalar();
                return (o == null || o == DBNull.Value) ? 0 : Convert.ToInt32(o, CultureInfo.InvariantCulture);
            }
        }

        // ===== Helper: Write and end response safely =====
        private void WriteAndEnd(string payload)
        {
            Response.Write(payload ?? "");
            try { Response.End(); } catch { /* expected ThreadAbort */ }
        }
    }
}
