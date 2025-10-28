using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using RRCManagementSystem.Helpers; // AESHelper

namespace RRCManagementSystem
{
    public partial class AddReceipt : Page
    {
        private readonly string cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                if (int.TryParse(Request.QueryString["tx"], out int txId))
                {
                    hfTransactionID.Value = txId.ToString(CultureInfo.InvariantCulture);
                    LoadTransaction(txId);
                }
                else
                {
                    ShowError("Invalid transaction.");
                    btnSave.Enabled = false;
                }
            }
        }

        private void LoadTransaction(int txId)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.spTransaction_ForReceipt", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@TransactionID", SqlDbType.Int).Value = txId;
                con.Open();

                using (var r = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!r.Read())
                    {
                        ShowError("Transaction not found.");
                        btnSave.Enabled = false;
                        return;
                    }

                    txtTxId.Text = Convert.ToInt32(r["TransactionID"]).ToString();
                    txtSaleId.Text = (r["SaleID"] == DBNull.Value) ? "-" : r["SaleID"].ToString();
                    txtAmount.Text = "₱" + Convert.ToDecimal(r["Amount"]).ToString("N2", new CultureInfo("en-PH"));
                    txtMethod.Text = (r["PaymentMethod"] == DBNull.Value) ? "-" : r["PaymentMethod"].ToString();

                    var dt = r["TransactionDate"];
                    txtDate.Text = (dt == DBNull.Value) ? "-" : Convert.ToDateTime(dt).ToString("yyyy-MM-dd HH:mm");

                    phReceipt.Controls.Clear();
                    string existing = r["Receipt"] as string;
                    if (!string.IsNullOrWhiteSpace(existing))
                    {
                        var link = new HyperLink
                        {
                            Text = "View Receipt",
                            CssClass = "pill pill-view",
                            NavigateUrl = "ReceiptViewer.aspx?tx=" + txId,
                            Target = "_blank"
                        };
                        phReceipt.Controls.Add(link);
                    }
                    else
                    {
                        var add = new HyperLink
                        {
                            Text = "Add Receipt",
                            CssClass = "pill pill-add",
                            NavigateUrl = "#"
                        };
                        phReceipt.Controls.Add(add);
                    }
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            // Check if user confirmed via SweetAlert
            if (hfConfirmed.Value != "true")
            {
                return; // User hasn't confirmed yet
            }

            if (string.IsNullOrEmpty(hfTransactionID.Value))
            {
                ShowError("Missing transaction ID.");
                return;
            }
            if (!fuReceipt.HasFile)
            {
                ShowError("Please select a receipt file.");
                return;
            }

            int txId = int.Parse(hfTransactionID.Value, CultureInfo.InvariantCulture);

            try
            {
                string folder = Server.MapPath("~/Receipts/");
                Directory.CreateDirectory(folder);

                string ext = (Path.GetExtension(fuReceipt.FileName) ?? "").ToLowerInvariant();
                if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".pdf")
                {
                    ShowError("Only JPG, PNG, or PDF allowed.");
                    return;
                }

                string fileName = $"receipt_tx{txId}_{DateTime.UtcNow.Ticks}{ext}";
                string fullPath = Path.Combine(folder, fileName);

                // Encrypt before saving
                using (var ms = new MemoryStream())
                {
                    fuReceipt.PostedFile.InputStream.CopyTo(ms);
                    byte[] original = ms.ToArray();
                    byte[] encrypted = AESHelper.Encrypt(original);
                    File.WriteAllBytes(fullPath, encrypted);
                }

                string dbPath = "~/Receipts/" + fileName;

                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.usp_Transactions_SetReceipt", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@TransactionID", SqlDbType.Int).Value = txId;
                    cmd.Parameters.Add("@Receipt", SqlDbType.NVarChar, 255).Value = dbPath;
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                // Register success script with SweetAlert
                string script = @"
                    Swal.fire({
                        icon: 'success',
                        title: 'Success!',
                        text: 'Receipt uploaded successfully. Redirecting...',
                        timer: 2000,
                        showConfirmButton: false
                    }).then(() => {
                        window.location.href = 'TransactionHistory.aspx';
                    });
                ";
                ScriptManager.RegisterStartupScript(this, GetType(), "successAlert", script, true);
            }
            catch (Exception ex)
            {
                ShowError("Error saving receipt: " + ex.Message);
            }
        }

        private void ShowOk(string msg)
        {
            lblMessage.Text = "✅ " + msg;
            lblMessage.CssClass = "message-label msg-ok";
        }
        private void ShowError(string msg)
        {
            lblMessage.Text = "❌ " + msg;
            lblMessage.CssClass = "message-label msg-err";
        }
    }
}