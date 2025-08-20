using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.UI;
using RRCManagementSystem.Helpers; // AESHelper

namespace RRCManagementSystem
{
    public partial class OurContract : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // IMPORTANT: rebuild panel state each request so events are wired
            if (!IsPostBack)
            {
                LoadContract();
            }
        }

        private void LoadContract()
        {
            lblMessage.Text = "";
            pnlContract.Visible = false;
            pnlPreview.Visible = false;

            if (Session["ClientID"] == null || !int.TryParse(Session["ClientID"].ToString(), out int clientId))
            {
                lblMessage.Text = "❌ Session expired. Please log in again.";
                return;
            }

            string filePath = null;

            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.usp_ClientContract_GetLatest", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;

                    conn.Open();
                    using (var reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (reader.Read())
                        {
                            if (reader["StartDate"] != DBNull.Value)
                                lblStartDate.Text = Convert.ToDateTime(reader["StartDate"]).ToString("yyyy-MM-dd");

                            if (reader["EndDate"] != DBNull.Value)
                                lblEndDate.Text = Convert.ToDateTime(reader["EndDate"]).ToString("yyyy-MM-dd");

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

                            lblRemarks.Text = reader["Remarks"] as string ?? string.Empty;
                            filePath = reader["FilePath"] as string;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "❌ Error loading contract: " + ex.Message;
                return;
            }

            if (!string.IsNullOrWhiteSpace(filePath))
            {
                // Optional: ensure the resolved path stays under an expected base folder for extra safety
                ViewState["ContractPath"] = filePath;
                pnlContract.Visible = true;
            }
            else
            {
                lblMessage.Text = "❌ No contract found for your account.";
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
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "❌ Failed to download contract: " + ex.Message;
            }
        }
    }
}
