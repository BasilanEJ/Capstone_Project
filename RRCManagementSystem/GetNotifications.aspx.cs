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
            // Return "text/plain" and short-circuit normal rendering
            Response.Clear();
            Response.ContentType = "text/plain";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Expires = -1;

            try
            {
                if (Session["ClientID"] == null)
                {
                    WriteAndEnd("0|||<div class='text-muted small p-2'>Not signed in.</div>");
                    return;
                }

                int clientId = Convert.ToInt32(Session["ClientID"]);
                string action = (Request.QueryString["action"] ?? "").Trim().ToLowerInvariant();

                if (action == "markread")
                {
                    MarkAllRead(clientId);
                    WriteAndEnd("0|||");
                    return;
                }

                int unread = 0;
                string html = BuildListHtml(clientId, ref unread);

                // 🔔 Add a synthetic Chat notification if there are unread admin→client messages
                int unreadChat = GetUnreadChatCount(clientId);
                if (unreadChat > 0)
                {
                    // bump the badge count by 1 “grouped” chat card
                    unread += 1;

                    // prepend a chat row
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

                    html = chatHtml + html;
                }

                WriteAndEnd(unread.ToString(CultureInfo.InvariantCulture) + "|||" + html);
            }
            catch (Exception ex)
            {
                string safe = HttpUtility.HtmlEncode(ex.Message);
                WriteAndEnd("0|||<div class='text-danger small p-2'>Notifications error: " + safe + "</div>");
            }
        }

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

        private string BuildListHtml(int clientId, ref int unread)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.usp_Notifications_ListForClient", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                cmd.Parameters.Add("@TopN", SqlDbType.Int).Value = 30;

                con.Open();

                using (var r = cmd.ExecuteReader())
                {
                    // Result set 1: unread count
                    if (r.Read())
                        unread = Convert.ToInt32(r["UnreadCount"]);

                    // Result set 2: rows
                    if (!r.NextResult() || !r.HasRows)
                        return "<div class='text-muted small p-2'>No notifications.</div>";

                    var sb = new StringBuilder();

                    while (r.Read())
                    {
                        string type = (r["Type"] as string ?? "").ToLowerInvariant();
                        string title = HttpUtility.HtmlEncode(r["Title"] as string ?? "");
                        string body = HttpUtility.HtmlEncode(r["Body"] as string ?? "");
                        string url = r["Url"] as string ?? "";
                        bool isRead = r["IsRead"] != DBNull.Value && (bool)r["IsRead"];
                        DateTime created = (DateTime)r["CreatedAt"];

                        string icon = "fa-circle-info";
                        if (type == "chat") icon = "fa-message";
                        else if (type == "quotation") icon = "fa-file-invoice";
                        else if (type == "booking") icon = "fa-calendar-check";
                        else if (type == "payment") icon = "fa-peso-sign";

                        string readCls = isRead ? "opacity-75" : "fw-semibold";
                        string when = created.ToLocalTime().ToString("MMM dd, yyyy hh:mm tt", CultureInfo.InvariantCulture);

                        sb.Append(
                            "<a href='" + (string.IsNullOrWhiteSpace(url) ? "#" : url) + "' class='text-decoration-none d-block'>" +
                            "  <div class='d-flex gap-2 p-2 border-bottom'>" +
                            "    <i class='fa-solid " + icon + " mt-1'></i>" +
                            "    <div class='flex-grow-1'>" +
                            "      <div class='" + readCls + "'>" + title + "</div>" +
                            (string.IsNullOrWhiteSpace(body) ? "" : "<div class='small text-muted'>" + body + "</div>") +
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

        // 🔎 Count unread Admin→Client messages to show a chat badge/row
        private int GetUnreadChatCount(int clientId)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(@"
                SELECT COUNT(*) 
                FROM dbo.Messages
                WHERE ReceiverType='Client' AND ReceiverID=@ClientID
                  AND SenderType='Admin'
                  AND (Status IS NULL OR Status <> 'Read');", con))
            {
                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                con.Open();
                object o = cmd.ExecuteScalar();
                return (o == null || o == DBNull.Value) ? 0 : Convert.ToInt32(o);
            }
        }

        private void WriteAndEnd(string payload)
        {
            Response.Write(payload ?? "");
            try { Response.End(); } catch { /* ThreadAbort expected */ }
        }
    }
}
