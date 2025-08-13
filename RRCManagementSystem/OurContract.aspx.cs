using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using RRCManagementSystem.Helpers; // AESHelper

namespace RRCManagementSystem
{
    public partial class OurContract : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // IMPORTANT: Rebuild the panel state on every request so the button events are wired.
            LoadContract();
        }

        private void LoadContract()
        {
            lblMessage.Text = "";

            if (Session["ClientID"] == null || !int.TryParse(Session["ClientID"].ToString(), out int clientId))
            {
                lblMessage.Text = "❌ Session expired. Please log in again.";
                pnlContract.Visible = false;
                pnlPreview.Visible = false;
                return;
            }

            string filePath = null;

            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(@"
            SELECT TOP 1 
                cc.StartDate,
                cc.EndDate,
                cc.UploadedAt,
                cc.Remarks,
                cc.FilePath,
                u.Name AS UploadedByName
            FROM ClientContracts cc
            INNER JOIN Users u ON cc.UploadedBy = u.UserID   -- ← join to Users
            WHERE cc.ClientID = @ClientID
            ORDER BY cc.UploadedAt DESC;", conn))
                {
                    cmd.Parameters.AddWithValue("@ClientID", clientId);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (reader["StartDate"] != DBNull.Value)
                                lblStartDate.Text = Convert.ToDateTime(reader["StartDate"]).ToString("yyyy-MM-dd");

                            if (reader["EndDate"] != DBNull.Value)
                                lblEndDate.Text = Convert.ToDateTime(reader["EndDate"]).ToString("yyyy-MM-dd");

                            // show datetime + admin name
                            string byName = reader["UploadedByName"] as string ?? "Unknown";
                            if (reader["UploadedAt"] != DBNull.Value)
                            {
                                string when = Convert.ToDateTime(reader["UploadedAt"]).ToString("yyyy-MM-dd hh:mm tt");
                                lblUploaded.Text = $"{when} by {byName}";
                            }
                            else
                            {
                                lblUploaded.Text = $"by {byName}";
                            }

                            lblRemarks.Text = reader["Remarks"] != DBNull.Value ? reader["Remarks"].ToString() : string.Empty;
                            filePath = reader["FilePath"] != DBNull.Value ? reader["FilePath"].ToString() : null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "❌ Error loading contract: " + ex.Message;
                pnlContract.Visible = false;
                pnlPreview.Visible = false;
                return;
            }

            if (!string.IsNullOrWhiteSpace(filePath))
            {
                ViewState["ContractPath"] = filePath;
                pnlContract.Visible = true;
            }
            else
            {
                lblMessage.Text = "❌ No contract found for your account.";
                pnlContract.Visible = false;
                pnlPreview.Visible = false;
            }
        }



        protected void btnPreview_Click(object sender, EventArgs e)
        {
            string relPath = ViewState["ContractPath"] as string;
            if (string.IsNullOrEmpty(relPath))
            {
                lblMessage.Text = "❌ Contract file not found on the server.";
                pnlPreview.Visible = false;
                return;
            }

            string absPath = Server.MapPath(relPath);
            if (!File.Exists(absPath))
            {
                lblMessage.Text = "❌ Contract file not found on the server.";
                pnlPreview.Visible = false;
                return;
            }

            try
            {
                // Decrypt to a temp preview file
                byte[] encrypted = File.ReadAllBytes(absPath);
                byte[] decrypted = AESHelper.Decrypt(encrypted);

                string previewsDir = Server.MapPath("~/Previews/");
                if (!Directory.Exists(previewsDir))
                    Directory.CreateDirectory(previewsDir);

                string clientName = (Session["ClientName"] as string ?? "Client")
                    .Replace(" ", "_").Replace(",", "").Replace(".", "");
                string fileName = $"{clientName}_Preview_{DateTime.Now:yyyyMMddHHmmssfff}.pdf";
                string previewPath = Path.Combine(previewsDir, fileName);

                File.WriteAllBytes(previewPath, decrypted);

                // Set iframe src
                pdfViewer.Attributes["src"] = ResolveUrl("~/Previews/" + fileName);
                pnlPreview.Visible = true;
                lblMessage.Text = "";
            }
            catch (Exception ex)
            {
                pnlPreview.Visible = false;
                lblMessage.Text = "⚠️ Unable to preview contract: " + ex.Message;
            }
        }

        protected void btnDownload_Click(object sender, EventArgs e)
        {
            string relPath = ViewState["ContractPath"] as string;
            if (string.IsNullOrEmpty(relPath))
            {
                lblMessage.Text = "❌ Contract file not found on the server.";
                return;
            }

            string absPath = Server.MapPath(relPath);
            if (!File.Exists(absPath))
            {
                lblMessage.Text = "❌ Contract file not found on the server.";
                return;
            }

            try
            {
                byte[] encrypted = File.ReadAllBytes(absPath);
                byte[] decrypted = AESHelper.Decrypt(encrypted);

                string clientName = (Session["ClientName"] as string ?? "Client")
                    .Replace(" ", "_").Replace(",", "").Replace(".", "");
                string downloadName = $"{clientName}_Contract.pdf";

                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("Content-Disposition", $"attachment; filename=\"{downloadName}\"");
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.BinaryWrite(decrypted);
                Response.Flush();

                // Avoid ThreadAbortException from Response.End()
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "❌ Failed to download contract: " + ex.Message;
            }
        }
    }
}
