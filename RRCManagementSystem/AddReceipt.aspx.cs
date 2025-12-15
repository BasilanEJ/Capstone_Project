using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

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

            string role = Session["Role"].ToString();
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            // Check CanAdd permission for Sales&Transaction module
            if (!HasAddPermission(userId, "Sales&Transaction"))
            {
                ShowError("You do not have permission to add receipts.");
                btnSaveReal.Enabled = false;
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
                    btnSaveReal.Enabled = false;
                }
            }
        }

        private bool HasAddPermission(int userId, string moduleName)
        {
            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spAdminPermission_CanAdd", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                    cmd.Parameters.Add("@ModuleName", SqlDbType.NVarChar, 100).Value = moduleName;

                    con.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null && Convert.ToInt32(result) == 1;
                }
            }
            catch
            {
                return false;
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
                        btnSaveReal.Enabled = false;
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
                        // ✅ Use direct link (no DecryptReceipt.aspx)
                        string receiptUrl = ResolveUrl($"~/Receipts/{existing}");

                        var link = new Literal
                        {
                            Text = $"<a href='#' class='pill pill-view' onclick=\"viewReceipt('{receiptUrl}', '{existing}'); return false;\">View Current Receipt</a>"
                        };
                        phReceipt.Controls.Add(link);
                    }
                    else
                    {
                        var noReceipt = new Literal
                        {
                            Text = "<span class='pill pill-add'>No Receipt Yet</span>"
                        };
                        phReceipt.Controls.Add(noReceipt);
                    }
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            // ✅ CRITICAL: Check if user confirmed via SweetAlert
            if (hfConfirmed.Value != "true")
            {
                return; // User didn't confirm, stop processing
            }

            // Reset confirmation flag
            hfConfirmed.Value = "false";

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

                // ✅ Generate unique filename (matching webhook pattern)
                string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                string random = Guid.NewGuid().ToString("N").Substring(0, 8);
                string fileName = $"Receipt_tx{txId}_Manual_{timestamp}_{random}{ext}";
                string fullPath = Path.Combine(folder, fileName);

                // ✅ Save as PLAIN file (no encryption)
                fuReceipt.SaveAs(fullPath);

                // ✅ Store PLAIN filename in database
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.usp_Transactions_SetReceipt", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@TransactionID", SqlDbType.Int).Value = txId;
                    cmd.Parameters.Add("@Receipt", SqlDbType.NVarChar, 500).Value = fileName;  // ✅ Plain filename only
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                // ✅ Show success and redirect
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