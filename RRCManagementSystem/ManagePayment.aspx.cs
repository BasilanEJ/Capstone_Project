using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using iTextSharp.text;
using iTextSharp.text.pdf;
using RRCManagementSystem.Helpers; // AESHelper, BlockchainLogger
using WebListItem = System.Web.UI.WebControls.ListItem;

namespace RRCManagementSystem
{
    public partial class ManagePayment : System.Web.UI.Page
    {
        private readonly string cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;
        private static string lastGeneratedEncryptedPDF = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            // 🔐 Require login
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string role = Session["Role"].ToString();
            // 🔐 Deny access for SuperAdmin and Inspector
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadClients();
                LoadPaymentMethods();
            }
        }

        // --- Time helper (Philippine Time) --------------------------------
        private static DateTime NowPht()
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        }

        // --- UI/Data -------------------------------------------------------
        private void LoadClients()
        {
            try
            {
                ddlClients.Items.Clear();
                ddlClients.Items.Add(new WebListItem("-- Select Client --", ""));

                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spClients_ListApproved", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            int id = Convert.ToInt32(r["ClientID"]);
                            string ln = r["LastName"] as string ?? "";
                            string fn = r["FirstName"] as string ?? "";
                            string mn = r["MiddleName"] as string ?? "";

                            string display = string.IsNullOrWhiteSpace(mn)
                                ? $"{ln}, {fn}"
                                : $"{ln}, {fn} {mn}";

                            ddlClients.Items.Add(new WebListItem(display, id.ToString()));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("❌ Failed to load clients: " + ex.Message);
            }
        }


        private void LoadPaymentMethods()
        {
            try
            {
                ddlPaymentMethod.Items.Clear();

                // Always-available basics first
                var basics = new[] { "-- Select Payment Method --", "Cash", "Bank Transfer" };
                var values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var label in basics)
                {
                    string val = label == "-- Select Payment Method --" ? "" : label;
                    ddlPaymentMethod.Items.Add(new WebListItem(label, val));
                    values.Add(val);
                }

                // Add methods seen in transaction history (e.g., PayMongo, PayPal, GCash, Card)
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spTransactionHistory_PaymentMethods", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            var pm = (r["PaymentMethod"] ?? "").ToString().Trim();
                            if (pm.Length == 0) continue;

                            // Normalize label (optional)
                            var label = pm; // or: CultureInfo.CurrentCulture.TextInfo.ToTitleCase(pm.ToLowerInvariant())

                            if (values.Add(pm)) // only add if not already present
                                ddlPaymentMethod.Items.Add(new WebListItem(label, pm));
                        }
                    }
                }
            }
            catch
            {
                // Fallback if the SP fails
                ddlPaymentMethod.Items.Clear();
                ddlPaymentMethod.Items.Add(new WebListItem("-- Select Payment Method --", ""));
                ddlPaymentMethod.Items.Add(new WebListItem("Cash", "Cash"));
                ddlPaymentMethod.Items.Add(new WebListItem("Bank Transfer", "Bank Transfer"));
                ddlPaymentMethod.Items.Add(new WebListItem("PayMongo", "PayMongo"));
                ddlPaymentMethod.Items.Add(new WebListItem("PayPal", "PayPal"));
            }
        }


        protected void ddlClients_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtRemainingBalance.Text = "0.00";
            if (string.IsNullOrEmpty(ddlClients.SelectedValue)) return;

            try
            {
                int clientId = Convert.ToInt32(ddlClients.SelectedValue);

                // 1) Latest payable booking (Assigned/Approved) -> BookingID, Price
                int bookingId = 0;
                decimal price = 0m;
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spBookings_GetLatestPayableByClient", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                    con.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) { txtRemainingBalance.Text = "0.00"; return; }
                        bookingId = Convert.ToInt32(r["BookingID"]);
                        price = Convert.ToDecimal(r["Price"]);
                    }
                }

                // 2) Ensure/return SaleID for that Booking
                int saleId = 0;
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spSales_GetOrCreateByBooking", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingId;
                    con.Open();
                    saleId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // 3) Sum valid payments for SaleID
                decimal totalPayments = 0m;
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spTransactions_SumValidBySale", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@SaleID", SqlDbType.Int).Value = saleId;
                    con.Open();
                    object o = cmd.ExecuteScalar();
                    totalPayments = (o == null || o == DBNull.Value) ? 0m : Convert.ToDecimal(o);
                }

                var remaining = price - totalPayments;
                if (remaining < 0) remaining = 0;
                txtRemainingBalance.Text = remaining.ToString("N2");

                // 🧾 Audit
                LogAudit($"Checked remaining balance for ClientID {clientId}, BookingID {bookingId}. Remaining ₱{remaining:N2}");
            }
            catch (Exception ex)
            {
                ShowError("❌ Failed to compute remaining balance: " + ex.Message);
            }
        }

        // --- Save ----------------------------------------------------------
        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ddlClients.SelectedValue) ||
                string.IsNullOrEmpty(ddlPaymentMethod.SelectedValue))
            {
                ShowError("❌ Please complete all fields.");
                return;
            }

            if (!decimal.TryParse(txtNewBalance.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal newBalance) ||
                !decimal.TryParse(txtRemainingBalance.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal remainingBalance))
            {
                ShowError("❌ Invalid amount format.");
                return;
            }

            if (newBalance > remainingBalance)
            {
                ShowError("❌ Amount paid exceeds remaining balance.");
                return;
            }

            if (!fuReceipt.HasFile)
            {
                ShowError("❌ Please upload a receipt image.");
                return;
            }

            string ext = Path.GetExtension(fuReceipt.FileName)?.ToLowerInvariant() ?? "";
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
            {
                ShowError("❌ Only JPG and PNG files are allowed.");
                return;
            }

            try
            {
                string performedBy = Session["AdminName"]?.ToString() ?? "Admin";
                int transactionId = SavePaymentAndBlockchain(newBalance, performedBy);

                if (transactionId > 0)
                {
                    string pdfPath = GenerateReceiptPDF(transactionId, performedBy);
                    lastGeneratedEncryptedPDF = pdfPath;

                    lblMessage.Text = "✅ Payment saved and blockchain recorded successfully!";
                    ScriptManager.RegisterStartupScript(this, GetType(), "swalSuccess", @"
Swal.fire({ icon:'success', title:'Payment Saved', text:'The payment has been recorded successfully!', confirmButtonColor:'#004085' });", true);

                    lblMessage.CssClass = "message success";
                    btnPrintReceipt.Visible = true;
                    btnDownloadReceipt.Visible = true;

                    // 🧾 Audit
                    LogAudit($"Recorded payment TxID {transactionId} | Client '{ddlClients.SelectedItem?.Text}' | Method {ddlPaymentMethod.SelectedItem?.Text} | Amount ₱{newBalance:N2}");

                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                ShowError("❌ Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Inserts the payment via stored procedure (PHT timestamp in SQL),
        /// then appends a blockchain log.
        /// </summary>
        private int SavePaymentAndBlockchain(decimal amount, string performedBy)
        {
            int clientId = Convert.ToInt32(ddlClients.SelectedValue);
            int bookingId;

            // Latest payable booking
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.spBookings_GetLatestPayableByClient", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read()) throw new InvalidOperationException("No payable booking found for this client.");
                    bookingId = Convert.ToInt32(r["BookingID"]);
                }
            }

            // Ensure SaleID
            int saleId;
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.spSales_GetOrCreateByBooking", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingId;
                con.Open();
                saleId = Convert.ToInt32(cmd.ExecuteScalar());
            }

            // Save encrypted receipt image to disk
            string receiptFolder = Server.MapPath("~/Receipts/");
            if (!Directory.Exists(receiptFolder)) Directory.CreateDirectory(receiptFolder);
            string receiptFileName = Guid.NewGuid() + Path.GetExtension(fuReceipt.FileName);
            string savePath = Path.Combine(receiptFolder, receiptFileName);
            using (var ms = new MemoryStream())
            {
                fuReceipt.PostedFile.InputStream.CopyTo(ms);
                byte[] original = ms.ToArray();
                byte[] encrypted = AESHelper.Encrypt(original);
                File.WriteAllBytes(savePath, encrypted);
            }
            string receiptDbPath = "~/Receipts/" + receiptFileName;

            // Remarks clamp
            string remarks = (txtRemarks.Text ?? string.Empty).Trim();
            if (remarks.Length > 255) remarks = remarks.Substring(0, 255);

            // Insert transaction via SP
            int transactionId;
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.spTransactions_InsertManual", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@SaleID", SqlDbType.Int).Value = saleId;

                var pAmt = cmd.Parameters.Add("@Amount", SqlDbType.Decimal);
                pAmt.Precision = 18; pAmt.Scale = 2; pAmt.Value = amount;

                cmd.Parameters.Add("@PaymentMethod", SqlDbType.NVarChar, 50).Value = ddlPaymentMethod.SelectedValue;
                cmd.Parameters.Add("@PerformedBy", SqlDbType.NVarChar, 100).Value = performedBy;
                cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, 255).Value = remarks;
                cmd.Parameters.Add("@Receipt", SqlDbType.NVarChar, 260).Value = receiptDbPath;

                con.Open();
                transactionId = Convert.ToInt32(cmd.ExecuteScalar());
            }

            // Append to blockchain (post-DB)
            try
            {
                BlockchainLogger.AppendSaleLog(cs, transactionId, new
                {
                    TransactionID = transactionId,
                    ClientID = clientId,
                    BookingID = bookingId,
                    SaleID = saleId,
                    Amount = amount,
                    Currency = "PHP",
                    Method = ddlPaymentMethod.SelectedValue,
                    Status = "Manual Adjustment",
                    PaidAtUtc = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Payment saved but blockchain logging failed: " + ex.Message, ex);
            }

            return transactionId;
        }

        // Use same PHT timestamp in the PDF (display layer only)
        private string GenerateReceiptPDF(int transactionId, string performedBy)
        {
            string folderPath = Server.MapPath("~/ReceiptsPDF/");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string plainPDF = Path.Combine(folderPath, $"Receipt_{transactionId}.pdf");
            DateTime nowPht = NowPht();

            using (Document doc = new Document())
            {
                PdfWriter.GetInstance(doc, new FileStream(plainPDF, FileMode.Create));
                doc.Open();

                string logoPath = Server.MapPath("~/Images/logorrc.png");
                if (File.Exists(logoPath))
                {
                    var logo = iTextSharp.text.Image.GetInstance(logoPath);
                    logo.ScaleAbsolute(100, 100);
                    logo.Alignment = Element.ALIGN_CENTER;
                    doc.Add(logo);
                }

                doc.Add(new Paragraph($"Receipt #{transactionId}"));
                doc.Add(new Paragraph("Date: " + nowPht.ToString("yyyy-MM-dd HH:mm:ss")));
                doc.Add(new Paragraph("Client: " + ddlClients.SelectedItem.Text));
                doc.Add(new Paragraph("Amount Paid: ₱ " + txtNewBalance.Text.Trim()));
                doc.Add(new Paragraph("Payment Method: " + ddlPaymentMethod.SelectedItem.Text));
                doc.Add(new Paragraph("Remarks: " + txtRemarks.Text.Trim()));
                doc.Add(new Paragraph("Handled By: " + performedBy));
                doc.Close();
            }

            // Encrypt the PDF on disk
            byte[] pdfBytes = File.ReadAllBytes(plainPDF);
            byte[] encrypted = AESHelper.Encrypt(pdfBytes);
            string encryptedPath = Path.Combine(folderPath, $"Receipt_{transactionId}_encrypted.pdf");
            File.WriteAllBytes(encryptedPath, encrypted);

            return encryptedPath;
        }

        // --- Utils ---------------------------------------------------------
        private void ShowError(string message)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = "message error";
        }

        private void ClearForm()
        {
            if (ddlClients.Items.Count > 0) ddlClients.SelectedIndex = 0;
            if (ddlPaymentMethod.Items.Count > 0) ddlPaymentMethod.SelectedIndex = 0;
            txtRemainingBalance.Text = "";
            txtNewBalance.Text = "";
            txtRemarks.Text = "";
        }

        protected void btnPrintReceipt_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lastGeneratedEncryptedPDF)) return;
            Response.Clear();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "inline; filename=Receipt.pdf");
            byte[] enc = File.ReadAllBytes(lastGeneratedEncryptedPDF);
            byte[] dec = AESHelper.Decrypt(enc);
            Response.BinaryWrite(dec);
            Response.End();
        }

        protected void btnDownloadReceipt_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lastGeneratedEncryptedPDF)) return;
            Response.Clear();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment; filename=Receipt.pdf");
            byte[] enc = File.ReadAllBytes(lastGeneratedEncryptedPDF);
            byte[] dec = AESHelper.Decrypt(enc);
            Response.BinaryWrite(dec);
            Response.End();
        }

        private void LogAudit(string action)
        {
            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spAudit_Insert", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@AdminID", SqlDbType.Int).Value = Convert.ToInt32(Session["UserID"]);
                    cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 255).Value = action;
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch { /* audit must not break UX */ }
        }
    }
}
