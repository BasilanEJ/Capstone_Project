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
                    // ✅ Use null checks to avoid InvalidCastException
                    if (reader["StartDate"] != DBNull.Value)
                        lblStartDate.Text = Convert.ToDateTime(reader["StartDate"]).ToString("yyyy-MM-dd");

                    if (reader["EndDate"] != DBNull.Value)
                        lblEndDate.Text = Convert.ToDateTime(reader["EndDate"]).ToString("yyyy-MM-dd");

                    if (reader["UploadedAt"] != DBNull.Value)
                        lblUploaded.Text = Convert.ToDateTime(reader["UploadedAt"]).ToString("yyyy-MM-dd hh:mm tt");

                    lblRemarks.Text = reader["Remarks"] != DBNull.Value ? reader["Remarks"].ToString() : "";

                    string filePath = reader["FilePath"] != DBNull.Value ? reader["FilePath"].ToString() : "";

                    if (!string.IsNullOrEmpty(filePath))
                    {
                        ViewState["ContractPath"] = filePath;
                        pnlContract.Visible = true;

                        // ✅ Show PDF preview
                        ShowPDFPreview(filePath, clientId);
                    }
                    else
                    {
                        lblMessage.Text = "❌ No file path found for the contract.";
                        pnlContract.Visible = false;
                        pnlPreview.Visible = false;
                    }
                }
                else
                {
                    lblMessage.Text = "❌ No contract found for your account.";
                    pnlContract.Visible = false;
                    pnlPreview.Visible = false;
                }
            }
        }


        private void ShowPDFPreview(string encryptedPath, int clientId)
        {
            try
            {
                string absoluteEncryptedPath = Server.MapPath(encryptedPath);
                if (!File.Exists(absoluteEncryptedPath))
                {
                    lblMessage.Text = "❌ Contract file not found on the server.";
                    return;
                }

                // Decrypt file for preview
                byte[] encryptedData = File.ReadAllBytes(absoluteEncryptedPath);
                byte[] decryptedData = AESHelper.Decrypt(encryptedData);

                string previewsDir = Server.MapPath("~/Previews/");
                if (!Directory.Exists(previewsDir))
                    Directory.CreateDirectory(previewsDir);

                string previewFilename = $"Client_{clientId}_ContractPreview.pdf";
                string previewPath = Path.Combine(previewsDir, previewFilename);
                File.WriteAllBytes(previewPath, decryptedData);

                pdfViewer.Attributes["src"] = ResolveUrl($"~/Previews/{previewFilename}");
                pnlPreview.Visible = true;
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠️ Unable to preview contract: " + ex.Message;
                pnlPreview.Visible = false;
            }
        }

        protected void btnDownload_Click(object sender, EventArgs e)
        {
            string filePath = ViewState["ContractPath"]?.ToString();

            if (!string.IsNullOrEmpty(filePath))
            {
                filePath = Server.MapPath(filePath); // Convert to physical path
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

                // 🔵 Auto-generate Previews folder
                string previewsDir = Server.MapPath("~/Previews/");
                if (!Directory.Exists(previewsDir))
                {
                    Directory.CreateDirectory(previewsDir);
                }

                // 🔵 Generate unique preview file
                string clientName = Session["ClientName"]?.ToString() ?? "Client";
                string safeClientName = clientName.Replace(" ", "_").Replace(",", "").Replace(".", "");
                string fileName = $"{safeClientName}_Preview_{DateTime.Now.Ticks}.pdf";
                string previewPath = Path.Combine(previewsDir, fileName);

                File.WriteAllBytes(previewPath, decryptedData);

                // 🔵 Store preview path in ViewState
                ViewState["PreviewFilePath"] = $"~/Previews/{fileName}";

                // 🔵 Show preview iframe
                pdfViewer.Attributes["src"] = ViewState["PreviewFilePath"].ToString();
                pnlPreview.Visible = true;
                lblMessage.Text = "";
            }
            catch (Exception ex)
            {
                lblMessage.Text = "❌ Failed to load contract preview: " + ex.Message;
            }
        }

    }
}
