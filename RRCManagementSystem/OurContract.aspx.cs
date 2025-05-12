using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using RRCManagementSystem.Helpers;

namespace RRCManagementSystem
{
    public partial class OurContract : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadContract();
            }
        }

        private void LoadContract()
        {
            if (Session["ClientID"] == null)
            {
                lblMessage.Text = "❌ Session expired. Please log in again.";
                return;
            }

            int clientId = Convert.ToInt32(Session["ClientID"]);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT TOP 1 * FROM ClientContracts WHERE ClientID = @ClientID ORDER BY UploadedAt DESC";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ClientID", clientId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    lblStartDate.Text = Convert.ToDateTime(reader["StartDate"]).ToString("yyyy-MM-dd");
                    lblEndDate.Text = Convert.ToDateTime(reader["EndDate"]).ToString("yyyy-MM-dd");
                    lblUploaded.Text = Convert.ToDateTime(reader["UploadedAt"]).ToString("yyyy-MM-dd hh:mm tt");
                    lblRemarks.Text = reader["Remarks"].ToString();

                    // This is already a full physical path (e.g., D:\EncryptedContracts\...)
                    ViewState["ContractPath"] = reader["FilePath"].ToString();
                    pnlContract.Visible = true;
                }
                else
                {
                    lblMessage.Text = "❌ No contract found for your account.";
                    pnlContract.Visible = false;
                }
            }
        }

        protected void btnDownload_Click(object sender, EventArgs e)
        {
            string filePath = ViewState["ContractPath"]?.ToString();

            if (!string.IsNullOrEmpty(filePath))
            {
                filePath = Server.MapPath(filePath); // 🔵 Map virtual to physical
            }

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                lblMessage.Text = "❌ Contract file not found on the server.";
                return;
            }

            try
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                byte[] decryptedData = AESHelper.Decrypt(encryptedData);

                // ✅ Create dynamic filename (ClientName_Contract.pdf)
                string clientName = Session["ClientName"]?.ToString() ?? "Client";
                string safeClientName = clientName.Replace(" ", "_").Replace(",", "").Replace(".", "");
                string downloadFileName = $"{safeClientName}_Contract.pdf";

                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("Content-Disposition", $"attachment; filename={downloadFileName}");
                Response.BinaryWrite(decryptedData);
                Response.End();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "❌ Failed to download contract: " + ex.Message;
            }
        }



    }
}