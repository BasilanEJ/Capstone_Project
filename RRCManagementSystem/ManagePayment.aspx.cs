using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using RRCManagementSystem.Helpers; // AESHelper, BlockchainLogger
using WebListItem = System.Web.UI.WebControls.ListItem;

namespace RRCManagementSystem
{
    public partial class ManagePayment : Page
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

            // Optional: block roles that shouldn't access
            var role = Session["Role"].ToString();
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                BindClients();
                BindPaymentMethods();
                txtRemainingBalance.Text = "0.00";
            }
        }

        // =============== Data Binders ===================================================

        private void BindClients()
        {
            ddlClients.Items.Clear();
            ddlClients.Items.Add(new WebListItem("-- Select Client --", ""));

            const string sql = @"
        SELECT c.ClientID,
               LTRIM(RTRIM(c.LastName)) AS LastName,
               LTRIM(RTRIM(c.FirstName)) AS FirstName,
               LTRIM(RTRIM(ISNULL(c.MiddleName, ''))) AS MiddleName
        FROM dbo.Clients c
        WHERE EXISTS (SELECT 1 FROM dbo.Bookings b WHERE b.ClientID = c.ClientID)
        ORDER BY c.LastName, c.FirstName;";

            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand(sql, con))
                {
                    con.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            var id = r.GetInt32(0);
                            var ln = r["LastName"]?.ToString() ?? "";
                            var fn = r["FirstName"]?.ToString() ?? "";
                            var mn = r["MiddleName"]?.ToString() ?? "";

                            var display = string.IsNullOrWhiteSpace(mn) ? $"{ln}, {fn}" : $"{ln}, {fn} {mn}";
                            ddlClients.Items.Add(new WebListItem(display, id.ToString(CultureInfo.InvariantCulture)));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("❌ Failed to load clients: " + ex.Message);
            }
        }

        private void BindPaymentMethods()
        {
            ddlPaymentMethod.Items.Clear();
            ddlPaymentMethod.Items.Add(new WebListItem("-- Select Method --", ""));
            ddlPaymentMethod.Items.Add(new WebListItem("Cash", "Cash"));
            ddlPaymentMethod.Items.Add(new WebListItem("Bank Transfer", "Bank Transfer"));
            ddlPaymentMethod.Items.Add(new WebListItem("Online Payment", "Online Payment"));
            // Optional: reflect historical methods (PayPal/PayMongo) if you want
            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand(
                    "SELECT DISTINCT PaymentMethod FROM dbo.Transactions WHERE PaymentMethod IS NOT NULL AND PaymentMethod<>'' ORDER BY PaymentMethod", con))
                {
                    con.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            var pm = r["PaymentMethod"]?.ToString();
                            if (!string.IsNullOrWhiteSpace(pm) && ddlPaymentMethod.Items.FindByValue(pm) == null)
                                ddlPaymentMethod.Items.Add(new WebListItem(pm, pm));
                        }
                    }
                }
            }
            catch { /* non-blocking */ }
        }

        // =============== Events =========================================================

        protected void ddlClients_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblMessage.Text = "";
            txtRemainingBalance.Text = "0.00";
            hfBookingId.Value = "";

            if (string.IsNullOrEmpty(ddlClients.SelectedValue))
                return;

            try
            {
                RefreshRemainingForSelectedClient();
            }
            catch (Exception ex)
            {
                ShowError("Failed to compute remaining: " + ex.Message);
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "";

            // Basic validations
            if (string.IsNullOrEmpty(ddlClients.SelectedValue))
            {
                ShowError("Please select a client.");
                return;
            }
            if (string.IsNullOrEmpty(ddlPaymentMethod.SelectedValue))
            {
                ShowError("Please select a payment method.");
                return;
            }
            if (!decimal.TryParse(txtNewBalance.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount) || amount <= 0)
            {
                ShowError("Enter a valid amount.");
                return;
            }
            if (!decimal.TryParse(txtRemainingBalance.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal remaining))
            {
                ShowError("Remaining balance is invalid.");
                return;
            }
            if (amount > remaining)
            {
                ShowError("Amount exceeds remaining balance.");
                return;
            }
            if (!fuReceipt.HasFile)
            {
                ShowError("Please upload a receipt image (JPG/PNG).");
                return;
            }
            var ext = (Path.GetExtension(fuReceipt.FileName) ?? "").ToLowerInvariant();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
            {
                ShowError("Only JPG and PNG are allowed.");
                return;
            }

            try
            {
                var performedBy = Session["AdminName"]?.ToString() ?? "Admin";

                // 1) Locate latest payable booking + price
                int clientId = int.Parse(ddlClients.SelectedValue, CultureInfo.InvariantCulture);
                var (bookingId, price) = GetLatestPayableBooking(clientId);
                if (bookingId <= 0)
                {
                    ShowError("No payable booking found for this client.");
                    return;
                }
                hfBookingId.Value = bookingId.ToString(CultureInfo.InvariantCulture);

                // 2) Ensure/return SaleID for that booking
                int saleId = EnsureSaleForBooking(bookingId);

                // 3) Save receipt file (encrypted bytes)
                string receiptDbPath = SaveEncryptedReceiptFile(bookingId);

                // 4) Insert transaction via stored procedure
                //    NOTE: We pass Status="Manual Adjustment" (the SP will normalize to 'Completed')
                int txId = InsertTransaction(saleId, amount,
                                             ddlPaymentMethod.SelectedValue,
                                             "Manual Adjustment",
                                             (txtRemarks.Text ?? "").Trim(),
                                             MakeManualReference(bookingId),
                                             receiptDbPath);

                // 5) Log to blockchain
                BlockchainLogger.AppendSaleLog(cs, txId, new
                {
                    TransactionID = txId,
                    ClientID = clientId,
                    BookingID = bookingId,
                    SaleID = saleId,
                    Amount = amount,
                    Currency = "PHP",
                    Method = ddlPaymentMethod.SelectedValue,
                    Status = "Completed",
                    PaidAtUtc = DateTime.UtcNow
                });

                // 6) Recompute & display remaining
                var newRemaining = GetRemainingBySale(saleId);
                txtRemainingBalance.Text = newRemaining.ToString("N2", new CultureInfo("en-PH"));

                // 7) Generate printable receipt PDF (encrypted on disk)
                lastGeneratedEncryptedPDF = GenerateReceiptPDF(txId, clientId, bookingId, amount, ddlPaymentMethod.SelectedValue, txtRemarks.Text ?? "", performedBy);

                // 8) UX
                lblMessage.Text = "✅ Payment recorded and balance updated.";
                lblMessage.CssClass = "message success";
                btnPrintReceipt.Visible = true;
                btnDownloadReceipt.Visible = true;

                // Optional: clear amount entry
                txtNewBalance.Text = "";
            }
            catch (Exception ex)
            {
                ShowError("Error saving payment: " + ex.Message);
            }
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

        // =============== Core helpers ===================================================

        private (int bookingId, decimal price) GetLatestPayableBooking(int clientId)
        {
            // Your rule: latest booking with status Assigned/Confirmed/Approved
            const string sql = @"
                SELECT TOP(1) b.BookingID, ISNULL(b.Price,0) AS Price
                FROM dbo.Bookings b
                WHERE b.ClientID=@ClientID AND b.Status IN ('Assigned','Confirmed','Approved')
                ORDER BY b.BookingID DESC;";

            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                con.Open();
                using (var r = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!r.Read()) return (0, 0m);
                    return (Convert.ToInt32(r["BookingID"], CultureInfo.InvariantCulture),
                            Convert.ToDecimal(r["Price"], CultureInfo.InvariantCulture));
                }
            }
        }

        private int EnsureSaleForBooking(int bookingId)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.usp_Sales_GetOrCreateByBooking", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingId;

                var pOut = cmd.Parameters.Add("@SaleID", SqlDbType.Int);
                pOut.Direction = ParameterDirection.Output;

                con.Open();
                cmd.ExecuteNonQuery();

                return Convert.ToInt32(pOut.Value, CultureInfo.InvariantCulture);
            }
        }

        private string SaveEncryptedReceiptFile(int bookingId)
        {
            string folder = Server.MapPath("~/Receipts/");
            Directory.CreateDirectory(folder);
            string fileName = $"receipt_{bookingId}_{DateTime.UtcNow.Ticks}{Path.GetExtension(fuReceipt.FileName)}";
            string fullPath = Path.Combine(folder, fileName);

            using (var ms = new MemoryStream())
            {
                fuReceipt.PostedFile.InputStream.CopyTo(ms);
                byte[] original = ms.ToArray();
                byte[] encrypted = AESHelper.Encrypt(original);
                File.WriteAllBytes(fullPath, encrypted);
            }

            // store relative path in DB, keep on disk encrypted
            return "~/Receipts/" + fileName;
        }

        private static string MakeManualReference(int bookingId) =>
            $"MANUAL:{bookingId}:{Guid.NewGuid():N}";

        private int InsertTransaction(int saleId,
                                      decimal amount,
                                      string method,
                                      string status,
                                      string remarks,
                                      string reference,
                                      string receiptPathOrNull)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.usp_Transactions_Insert", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@SaleID", SqlDbType.Int).Value = saleId;

                var pAmt = cmd.Parameters.Add("@Amount", SqlDbType.Decimal);
                pAmt.Precision = 18; pAmt.Scale = 2; pAmt.Value = amount;

                cmd.Parameters.Add("@PaymentMethod", SqlDbType.NVarChar, 50).Value = method ?? "Cash";
                cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = status ?? "Completed";
                cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, 255).Value = (remarks ?? "").Trim();
                cmd.Parameters.Add("@Reference", SqlDbType.NVarChar, 200).Value = (object)reference ?? DBNull.Value;

                // Optional parameter in SP (backward-compatible). If your SP doesn't have it,
                // either add @Receipt NVARCHAR(255)=NULL, or remove the next line.
                cmd.Parameters.Add("@Receipt", SqlDbType.NVarChar, 255).Value =
                    string.IsNullOrWhiteSpace(receiptPathOrNull) ? (object)DBNull.Value : receiptPathOrNull;

                var pTx = cmd.Parameters.Add("@TransactionID", SqlDbType.Int);
                pTx.Direction = ParameterDirection.Output;

                con.Open();
                cmd.ExecuteNonQuery();
                return Convert.ToInt32(pTx.Value, CultureInfo.InvariantCulture);
            }
        }

        private decimal GetRemainingBySale(int saleId)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.usp_Sales_GetRemaining", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@SaleID", SqlDbType.Int).Value = saleId;
                con.Open();
                object o = cmd.ExecuteScalar();
                return (o == null || o == DBNull.Value) ? 0m : Convert.ToDecimal(o, CultureInfo.InvariantCulture);
            }
        }

        private string GenerateReceiptPDF(int transactionId, int clientId, int bookingId, decimal amount, string method, string remarks, string performedBy)
        {
            string folderPath = Server.MapPath("~/ReceiptsPDF/");
            Directory.CreateDirectory(folderPath);

            string plainPDF = Path.Combine(folderPath, $"Receipt_{transactionId}.pdf");

            using (var fs = new FileStream(plainPDF, FileMode.Create, FileAccess.Write))
            {
                var doc = new iTextSharp.text.Document();
                iTextSharp.text.pdf.PdfWriter.GetInstance(doc, fs);

                doc.Open();
                doc.Add(new iTextSharp.text.Paragraph($"Receipt #{transactionId}"));
                doc.Add(new iTextSharp.text.Paragraph($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}"));
                doc.Add(new iTextSharp.text.Paragraph($"Client ID: {clientId}"));
                doc.Add(new iTextSharp.text.Paragraph($"Booking ID: {bookingId}"));
                doc.Add(new iTextSharp.text.Paragraph($"Amount Paid: ₱ {amount:N2}"));
                doc.Add(new iTextSharp.text.Paragraph($"Method: {method}"));
                if (!string.IsNullOrWhiteSpace(remarks))
                    doc.Add(new iTextSharp.text.Paragraph($"Remarks: {remarks}"));
                doc.Add(new iTextSharp.text.Paragraph($"Handled By: {performedBy}"));
                doc.Close();
            }

            // Encrypt PDF bytes on disk using your AESHelper
            byte[] pdfBytes = File.ReadAllBytes(plainPDF);
            byte[] encrypted = AESHelper.Encrypt(pdfBytes);
            string encryptedPath = Path.Combine(folderPath, $"Receipt_{transactionId}_encrypted.pdf");
            File.WriteAllBytes(encryptedPath, encrypted);

            return encryptedPath;
        }


        // =============== UI helpers =====================================================

        private void RefreshRemainingForSelectedClient()
        {
            int clientId = int.Parse(ddlClients.SelectedValue, CultureInfo.InvariantCulture);

            var (bookingId, price) = GetLatestPayableBooking(clientId);
            if (bookingId <= 0)
            {
                txtRemainingBalance.Text = "0.00";
                hfBookingId.Value = "";
                return;
            }

            hfBookingId.Value = bookingId.ToString(CultureInfo.InvariantCulture);

            int saleId = EnsureSaleForBooking(bookingId);
            decimal remaining = GetRemainingBySale(saleId);
            if (remaining < 0) remaining = 0;

            txtRemainingBalance.Text = remaining.ToString("N2", new CultureInfo("en-PH"));
        }

        private void ShowError(string message)
        {
            lblMessage.Text = "❌ " + message;
            lblMessage.CssClass = "message error";
        }
    }
}
