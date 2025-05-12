using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;
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
    FROM Messages
    WHERE 
        (
            (SenderID = @AdminID AND SenderType = 'Admin' AND ReceiverID = @ClientID AND ReceiverType = 'Client')
            OR
            (SenderID = @ClientID AND SenderType = 'Client' AND ReceiverID = @AdminID AND ReceiverType = 'Admin')
        )
    ORDER BY SentAt ASC";


                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ClientID", clientId);
                cmd.Parameters.AddWithValue("@AdminID", adminId);

                DataTable dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);

                rptMessages.DataSource = dt;
                rptMessages.DataBind();
            }
        }

        protected void btnSend_Click(object sender, EventArgs e)
        {
            int clientId = Convert.ToInt32(Session["ClientID"]);
            int adminId = Convert.ToInt32(Session["AdminID"]);
            string messageText = txtMessage.Text.Trim();
            byte[] encryptedAttachment = null;
            string attachmentName = null;
            string attachmentType = null;

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

            if (string.IsNullOrWhiteSpace(messageText) && encryptedAttachment == null)
            {
                lblInfo.Text = "⚠️ Please type a message or attach a file.";
                return;
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                    INSERT INTO Messages 
                        (SenderID, ReceiverID, SenderType, ReceiverType, MessageText, Attachment, AttachmentName, AttachmentType, SentAt, Status)
                    VALUES 
                        (@SenderID, @ReceiverID, 'Client', 'Admin', @MessageText, @Attachment, @AttachmentName, @AttachmentType, @SentAt, 'Sent')";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@SenderID", clientId);
                cmd.Parameters.AddWithValue("@ReceiverID", adminId);
                cmd.Parameters.AddWithValue("@MessageText", messageText);
                cmd.Parameters.AddWithValue("@Attachment", (object)encryptedAttachment ?? DBNull.Value);
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
                string query = "SELECT TOP 1 UserID FROM Users WHERE Role = 'Admin' AND Status = 'Active' ORDER BY UserID ASC";
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        public string GetAttachmentHtml(object nameObj, object typeObj, object idObj)
        {
            if (nameObj == DBNull.Value || typeObj == DBNull.Value)
                return "";

            string name = nameObj.ToString();
            string type = typeObj.ToString().ToLower();
            string id = idObj.ToString();

            if (type.StartsWith("image/"))
            {
                return $"<br/><a href='DownloadAttachment.aspx?ID={id}' target='_blank'>" +
                       $"<img src='DownloadAttachment.aspx?ID={id}' alt='{name}' class='img-preview' /></a>";
            }
            else
            {
                return $"<br/><a href='DownloadAttachment.aspx?ID={id}' class='attachment-link'>📎 {name}</a>";
            }
        }
    }
}