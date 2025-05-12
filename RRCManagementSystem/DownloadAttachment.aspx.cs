using System;
using System.Data.SqlClient;
using System.Configuration;
using RRCManagementSystem.Helpers;

namespace RRCManagementSystem
{
    public partial class DownloadAttachment : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["ClientID"] == null && Session["AdminID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                if (int.TryParse(Request.QueryString["ID"], out int messageId))
                {
                    DownloadFile(messageId);
                }
            }
        }

        private void DownloadFile(int messageId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT Attachment, AttachmentName, AttachmentType FROM Messages WHERE MessageID = @MessageID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@MessageID", messageId);

                try
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read() && reader["Attachment"] != DBNull.Value)
                    {
                        byte[] encryptedData = (byte[])reader["Attachment"];
                        string fileName = reader["AttachmentName"].ToString();
                        string fileType = reader["AttachmentType"].ToString();

                        // 🔒 Decrypt the file using AES-256
                        byte[] decryptedData = AESHelper.DecryptBytes(encryptedData);

                        Response.Clear();
                        Response.ContentType = fileType;

                        // Show image inline, download others
                        bool isImage = fileType.StartsWith("image/");
                        string disposition = isImage ? "inline" : "attachment";
                        Response.AddHeader("Content-Disposition", $"{disposition}; filename=\"{fileName}\"");

                        Response.BinaryWrite(decryptedData);
                        Response.End();
                    }
                }
                catch (Exception ex)
                {
                    Response.Clear();
                    Response.Write("❌ Error downloading file: " + ex.Message);
                    Response.End();
                }
            }
        }
    }
}