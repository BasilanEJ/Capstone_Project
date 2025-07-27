    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Configuration;
    using System.Web.UI.WebControls;
    using System.IO;
    using RRCManagementSystem.Helpers;

    namespace RRCManagementSystem
    {
        public partial class ChatWithClient : System.Web.UI.Page
        {
            private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 🔐 Check session validity
            if (Session["UserID"] == null || Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int adminId = Convert.ToInt32(Session["UserID"]);

            if (!IsPostBack)
            {
                LoadClients(); 
            }
        }

        private void LoadClients()
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT ClientID, Name FROM Clients WHERE Status = 'Approved'";
                    SqlCommand cmd = new SqlCommand(query, con);
                    con.Open();
                    ddlClients.DataSource = cmd.ExecuteReader();
                    ddlClients.DataTextField = "Name";
                    ddlClients.DataValueField = "ClientID";
                    ddlClients.DataBind();
                    ddlClients.Items.Insert(0, new ListItem("-- Select Client --", ""));
                }
            }

            protected void ddlClients_SelectedIndexChanged(object sender, EventArgs e)
            {
                LoadMessages();
            }

            private void LoadMessages()
            {
                if (string.IsNullOrEmpty(ddlClients.SelectedValue)) return;

                int adminId = Convert.ToInt32(Session["AdminID"]);
                int clientId = Convert.ToInt32(ddlClients.SelectedValue);

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
                    cmd.Parameters.AddWithValue("@AdminID", adminId);
                    cmd.Parameters.AddWithValue("@ClientID", clientId);

                    try
                    {
                        con.Open();
                        DataTable dt = new DataTable();
                        new SqlDataAdapter(cmd).Fill(dt);

                        rptMessages.DataSource = dt;
                        rptMessages.DataBind();
                    }
                    catch (Exception ex)
                    {
                        lblInfo.Text = "❌ Error loading messages: " + ex.Message;
                    }
                }
            }

            protected void btnSend_Click(object sender, EventArgs e)
            {
                if (string.IsNullOrEmpty(ddlClients.SelectedValue)) return;

                int adminId = Convert.ToInt32(Session["AdminID"]);
                int clientId = Convert.ToInt32(ddlClients.SelectedValue);
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

                        encryptedAttachment = AESHelper.EncryptBytes(fileBytes); // AES-256 encryption
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
                            (@SenderID, @ReceiverID, 'Admin', 'Client', @MessageText, @Attachment, @AttachmentName, @AttachmentType, @SentAt, 'Sent')";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@SenderID", adminId);
                    cmd.Parameters.AddWithValue("@ReceiverID", clientId);
                    cmd.Parameters.AddWithValue("@MessageText", messageText);
                    cmd.Parameters.AddWithValue("@Attachment", (object)encryptedAttachment ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AttachmentName", (object)attachmentName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AttachmentType", (object)attachmentType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SentAt", DateTime.Now);

                    con.Open();
                    cmd.ExecuteNonQuery();

                    AddAuditLog(adminId, $"Sent a message to Client ID {clientId}: \"{messageText}\"");
                    txtMessage.Text = string.Empty;
                    lblInfo.Text = "✅ Message sent.";
                    LoadMessages();
                }
            }

            // ADD TO BOTTOM OF ChatWithClient CLASS
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


            // 🔐 Audit Log
            private void AddAuditLog(int? userID, string action)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "INSERT INTO AuditLogs (AdminID, Action, Timestamp) VALUES (@AdminID, @Action, GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@AdminID", (object)userID ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Action", action);

                        try
                        {
                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                        catch
                        {
                            // Optional: handle logging errors
                        }
                    }
                }
            }
        }
    }