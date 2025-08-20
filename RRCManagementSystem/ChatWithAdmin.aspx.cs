using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.UI;
using RRCManagementSystem.Helpers;

namespace RRCManagementSystem
{
    public partial class ChatWithAdmin : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["ClientID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                if (Session["AdminID"] == null)
                {
                    int adminId = GetDefaultAdminID();
                    if (adminId > 0)
                        Session["AdminID"] = adminId;
                    else
                    {
                        lblInfo.Text = "❌ No active Admin available.";
                        return;
                    }
                }

                LoadMessages();
            }
        }

        private void LoadMessages()
        {
            int clientId = Convert.ToInt32(Session["ClientID"]);
            int adminId = Convert.ToInt32(Session["AdminID"]);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
SELECT MessageID, MessageText, SenderType, SentAt, AttachmentName, AttachmentType
FROM dbo.Messages
WHERE 
    (
        (SenderID = @AdminID AND SenderType = 'Admin' AND ReceiverID = @ClientID AND ReceiverType = 'Client')
        OR
        (SenderID = @ClientID AND SenderType = 'Client' AND ReceiverID = @AdminID AND ReceiverType = 'Admin')
    )
ORDER BY SentAt ASC;";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ClientID", clientId);
                cmd.Parameters.AddWithValue("@AdminID", adminId);

                DataTable dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);

                rptMessages.DataSource = dt;
                rptMessages.DataBind();
            }

            // ✅ As soon as the client views the chat, mark all Admin→Client messages as READ.
            // This clears your synthetic chat notification (and any DB chat notifs if you enabled them).
            try { MarkAdminMessagesAsRead(clientId); } catch { /* non-blocking */ }

            // (Optional) If you also inserted DB notifications with Type='chat', mark them read too.
            try { MarkChatNotificationsReadForClient(clientId); } catch { /* non-blocking */ }
        }

        protected void btnSend_Click(object sender, EventArgs e)
        {
            int clientId = Convert.ToInt32(Session["ClientID"]);
            int adminId = Convert.ToInt32(Session["AdminID"]);
            string messageText = (txtMessage.Text ?? string.Empty).Trim();
            byte[] encryptedAttachment = null;
            string attachmentName = null;
            string attachmentType = null;

            // 🔒 Handle file attachment
            if (fileAttachment.HasFile)
            {
                using (var ms = new MemoryStream())
                {
                    fileAttachment.PostedFile.InputStream.CopyTo(ms);
                    byte[] fileBytes = ms.ToArray();

                    encryptedAttachment = AESHelper.EncryptBytes(fileBytes);
                    attachmentName = fileAttachment.FileName;
                    attachmentType = fileAttachment.PostedFile.ContentType;
                }
            }

            // 🛑 Require at least a message or file
            if (string.IsNullOrWhiteSpace(messageText) && encryptedAttachment == null)
            {
                lblInfo.Text = "⚠️ Please type a message or attach a file.";
                return;
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
INSERT INTO dbo.Messages 
    (SenderID, ReceiverID, SenderType, ReceiverType, MessageText, Attachment, AttachmentName, AttachmentType, SentAt, Status)
VALUES 
    (@SenderID, @ReceiverID, 'Client', 'Admin', @MessageText, @Attachment, @AttachmentName, @AttachmentType, @SentAt, 'Sent');";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@SenderID", clientId);
                cmd.Parameters.AddWithValue("@ReceiverID", adminId);
                cmd.Parameters.AddWithValue("@MessageText", (object)messageText ?? DBNull.Value);

                // ✅ Explicit varbinary parameter to avoid implicit type issues
                SqlParameter attachmentParam = new SqlParameter("@Attachment", SqlDbType.VarBinary);
                attachmentParam.Value = (object)encryptedAttachment ?? DBNull.Value;
                cmd.Parameters.Add(attachmentParam);

                cmd.Parameters.AddWithValue("@AttachmentName", (object)attachmentName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@AttachmentType", (object)attachmentType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SentAt", DateTime.Now);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            lblInfo.ForeColor = System.Drawing.Color.Green;
            lblInfo.Text = "✅ Message sent.";
            txtMessage.Text = string.Empty;

            LoadMessages();
        }

        private int GetDefaultAdminID()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
SELECT TOP 1 UserID 
FROM dbo.Users 
WHERE Role = 'Admin' 
  AND (Status = 'Active' OR Status = 'Available')
ORDER BY UserID ASC;";

                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        /// <summary>
        /// Marks Admin → Client messages as 'Read' for the current client.
        /// This helps your GetNotifications synthetic chat badge to clear immediately.
        /// </summary>
        private void MarkAdminMessagesAsRead(int clientId)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"
UPDATE dbo.Messages
SET Status = 'Read'
WHERE ReceiverType = 'Client' 
  AND ReceiverID = @ClientID
  AND SenderType = 'Admin'
  AND (Status IS NULL OR Status <> 'Read');", con))
            {
                cmd.Parameters.AddWithValue("@ClientID", clientId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// OPTIONAL: If you also create real rows in dbo.Notifications with Type='chat',
        /// mark them as read when the user opens/chat is viewed.
        /// Safe no-op if the table/rows aren't there.
        /// </summary>
        private void MarkChatNotificationsReadForClient(int clientId)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"
UPDATE dbo.Notifications
SET IsRead = 1
WHERE ClientID = @ClientID
  AND [Type] = 'chat'
  AND IsRead = 0;", con))
            {
                cmd.Parameters.AddWithValue("@ClientID", clientId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Renders an attachment link or preview. Uses concatenation for older C# compatibility.
        /// </summary>
        public string GetAttachmentHtml(object nameObj, object typeObj, object idObj)
        {
            if (nameObj == DBNull.Value || typeObj == DBNull.Value || idObj == DBNull.Value)
                return "";

            string name = Convert.ToString(nameObj) ?? "";
            string type = Convert.ToString(typeObj) ?? "";
            string id = Convert.ToString(idObj) ?? "";

            // Basic sanitize
            string safeName = HttpUtility.HtmlEncode(name);

            if (type.ToLower().StartsWith("image/"))
            {
                return "<br/><a href='DownloadAttachment.aspx?ID=" + id + "' target='_blank'>" +
                       "<img src='DownloadAttachment.aspx?ID=" + id + "' alt='" + safeName + "' class='img-preview' /></a>";
            }
            else
            {
                return "<br/><a href='DownloadAttachment.aspx?ID=" + id + "' class='attachment-link'>📎 " + safeName + "</a>";
            }
        }
    }
}
