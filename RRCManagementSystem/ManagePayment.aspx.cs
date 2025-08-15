using System;
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

// Avoid ListItem ambiguity with iTextSharp
using WebListItem = System.Web.UI.WebControls.ListItem;

namespace RRCManagementSystem
{
    public partial class ManagePayment : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;
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
            }
        }

        // --- Time helpers (Philippine Time) ------------------------------
        private static DateTime NowPht()
        {
            // Windows TZ ID for Manila (same offset): "Singapore Standard Time"
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        }

        // --- UI/Data ------------------------------------------------------
        private void LoadClients()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                const string query = @"
SELECT 
    ClientID,
    (LastName + ', ' + FirstName + ' ' + ISNULL(MiddleName, '')) AS Name 
FROM Clients 
WHERE Status = 'Approved'
ORDER BY LastName, FirstName;";

                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                ddlClients.DataSource = reader;
                ddlClients.DataTextField = "Name";
                ddlClients.DataValueField = "ClientID";
                ddlClients.DataBind();
                ddlClients.Items.Insert(0, new WebListItem("-- Select Client --", ""));
            }
        }

        protected void ddlClients_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtRemainingBalance.Text = "0.00";

            if (string.IsNullOrEmpty(ddlClients.SelectedValue))
                return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // 1) Get the latest payable booking (Assigned or Approved)
                int clientId = Convert.ToInt32(ddlClients.SelectedValue);
                int bookingId = 0;
                decimal price = 0m;

                const string bookingQuery = @"
SELECT TOP 1 b.BookingID, b.Price
FROM Bookings b
WHERE b.ClientID = @ClientID
  AND b.Status IN ('Assigned','Approved')
ORDER BY b.CreatedAt DESC;";

                using (SqlCommand cmd = new SqlCommand(bookingQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@ClientID", clientId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            txtRemainingBalance.Text = "0.00"; // No payable booking found
                            return;
                        }

                        bookingId = Convert.ToInt32(reader["BookingID"]);
                        price = Convert.ToDecimal(reader["Price"]);
                    }
                }

                // 2) Ensure a Sales row exists and get SaleID
                int saleId = GetOrCreateSaleId(bookingId);

                // 3) Sum valid payments for that SaleID
                const string paymentQuery = @"
SELECT ISNULL(SUM(Amount), 0)
FROM Transactions
WHERE SaleID = @SaleID
  AND (Status IS NULL OR Status NOT IN ('Refunded','Voided'));";

                using (SqlCommand payCmd = new SqlCommand(paymentQuery, conn))
                {
                    payCmd.Parameters.AddWithValue("@SaleID", saleId);
                    decimal totalPayments = Convert.ToDecimal(payCmd.ExecuteScalar());

                    decimal remaining = price - totalPayments;
                    if (remaining < 0) remaining = 0;

                    txtRemainingBalance.Text = remaining.ToString("N2");
                }
            }
        }

        // --- Save ---------------------------------------------------------
        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ddlClients.SelectedValue) || string.IsNullOrEmpty(ddlPaymentMethod.SelectedValue))
            {
                ShowError("❌ Please complete all fields.");
                return;
            }

            if (!decimal.TryParse(txtNewBalance.Text.Trim(), out decimal newBalance) ||
                !decimal.TryParse(txtRemainingBalance.Text.Trim(), out decimal remainingBalance))
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

            string ext = Path.GetExtension(fuReceipt.FileName).ToLower();
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
Swal.fire({
    icon: 'success',
    title: 'Payment Saved',
    text: 'The payment has been recorded successfully!',
    confirmButtonColor: '#004085'
});", true);

                    lblMessage.CssClass = "message success";
                    btnPrintReceipt.Visible = true;
                    btnDownloadReceipt.Visible = true;
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                ShowError("❌ Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Inserts the manual/over-the-counter payment into Transactions (PHT timestamp),
        /// then appends a signed, chained blockchain log using the shared helper.
        /// </summary>
        private int SavePaymentAndBlockchain(decimal amount, string performedBy)
        {
            int clientId = Convert.ToInt32(ddlClients.SelectedValue);
            int bookingId = GetLatestPayableBookingId(clientId);
            if (bookingId == 0) throw new InvalidOperationException("No payable booking found for this client.");

            // Ensure a Sales row exists for this booking
            int saleId = GetOrCreateSaleId(bookingId);

            // Save encrypted receipt image
            string receiptFolder = Server.MapPath("~/Receipts/");
            if (!Directory.Exists(receiptFolder))
                Directory.CreateDirectory(receiptFolder);

            string receiptFileName = Guid.NewGuid() + Path.GetExtension(fuReceipt.FileName);
            string savePath = Path.Combine(receiptFolder, receiptFileName);

            using (var ms = new MemoryStream())
            {
                fuReceipt.PostedFile.InputStream.CopyTo(ms);
                byte[] original = ms.ToArray();
                byte[] encrypted = AESHelper.Encrypt(original);
                File.WriteAllBytes(savePath, encrypted);
            }

            // Optional: clamp overly long remarks to avoid exceeding column length (e.g., nvarchar(255))
            string remarks = (txtRemarks.Text ?? string.Empty).Trim();
            if (remarks.Length > 255) remarks = remarks.Substring(0, 255);

            int transactionId;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();

                try
                {
                    // 1) Insert Transaction — PHT timestamp from DB (no app-server clock drift)
                    const string insertTransaction = @"
INSERT INTO Transactions
    (SaleID, Amount, TransactionDate, PaymentMethod, Status, PerformedBy, Remarks, Receipt)
OUTPUT INSERTED.TransactionID
VALUES
    (@SaleID, @Amount, DATEADD(HOUR, 8, GETUTCDATE()), @PaymentMethod, 'Manual Adjustment', @PerformedBy, @Remarks, @Receipt);";

                    using (SqlCommand cmd = new SqlCommand(insertTransaction, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@SaleID", saleId);

                        // Explicit precision/scale for decimal amount
                        var pAmt = cmd.Parameters.Add("@Amount", SqlDbType.Decimal);
                        pAmt.Precision = 18;
                        pAmt.Scale = 2;
                        pAmt.Value = amount;

                        cmd.Parameters.AddWithValue("@PaymentMethod", ddlPaymentMethod.SelectedValue);
                        cmd.Parameters.AddWithValue("@PerformedBy", performedBy);
                        cmd.Parameters.AddWithValue("@Remarks", remarks);
                        cmd.Parameters.AddWithValue("@Receipt", "~/Receipts/" + receiptFileName);

                        transactionId = (int)cmd.ExecuteScalar();
                    }

                    // Commit DB work before writing blockchain
                    trans.Commit();
                }
                catch
                {
                    try { trans.Rollback(); } catch { /* ignore */ }
                    throw;
                }
            }

            // 2) Append to blockchain (canonical JSON + HMAC + chaining) using UTC for consistency
            try
            {
                BlockchainLogger.AppendSaleLog(connectionString, transactionId, new
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
                // Payment is saved; surface that blockchain failed
                throw new ApplicationException("Payment saved but blockchain logging failed: " + ex.Message, ex);
            }

            return transactionId;
        }

        // Use same PHT timestamp in the PDF (display layer only)
        private string GenerateReceiptPDF(int transactionId, string performedBy)
        {
            string folderPath = Server.MapPath("~/ReceiptsPDF/");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

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

        private int GetLatestPayableBookingId(int clientId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                const string query = @"
SELECT TOP 1 BookingID
FROM Bookings
WHERE ClientID = @ClientID
  AND Status IN ('Assigned','Approved')
ORDER BY CreatedAt DESC;";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ClientID", clientId);
                conn.Open();
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        /// <summary>
        /// Returns existing SaleID for BookingID, or creates a new Sales row using Bookings.ClientID.
        /// </summary>
        private int GetOrCreateSaleId(int bookingId)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = con.CreateCommand())
            {
                con.Open();
                using (var tx = con.BeginTransaction())
                {
                    cmd.Transaction = tx;

                    // 1) Try to find existing Sale for this Booking
                    cmd.CommandText = "SELECT SaleID FROM Sales WHERE BookingID = @BookingID";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@BookingID", bookingId);
                    var existing = cmd.ExecuteScalar();
                    if (existing != null && existing != DBNull.Value)
                    {
                        tx.Commit();
                        return Convert.ToInt32(existing);
                    }

                    // 2) Get ClientID from Bookings
                    cmd.CommandText = "SELECT ClientID FROM Bookings WHERE BookingID = @BookingID";
                    var clientIdObj = cmd.ExecuteScalar();
                    if (clientIdObj == null || clientIdObj == DBNull.Value)
                        throw new InvalidOperationException("Booking has no ClientID.");

                    int clientId = Convert.ToInt32(clientIdObj);

                    // 3) Create the Sale row
                    cmd.CommandText = @"
INSERT INTO Sales (BookingID, ClientID)
VALUES (@BookingID, @ClientID);
SELECT CAST(SCOPE_IDENTITY() AS INT);";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@BookingID", bookingId);
                    cmd.Parameters.AddWithValue("@ClientID", clientId);

                    int saleId = Convert.ToInt32(cmd.ExecuteScalar());
                    tx.Commit();
                    return saleId;
                }
            }
        }

        // --- Utils --------------------------------------------------------
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

            byte[] encryptedBytes = File.ReadAllBytes(lastGeneratedEncryptedPDF);
            byte[] decryptedBytes = AESHelper.Decrypt(encryptedBytes);

            Response.BinaryWrite(decryptedBytes);
            Response.End();
        }

        protected void btnDownloadReceipt_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lastGeneratedEncryptedPDF)) return;

            Response.Clear();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment; filename=Receipt.pdf");

            byte[] encryptedBytes = File.ReadAllBytes(lastGeneratedEncryptedPDF);
            byte[] decryptedBytes = AESHelper.Decrypt(encryptedBytes);

            Response.BinaryWrite(decryptedBytes);
            Response.End();
        }
    }
}
