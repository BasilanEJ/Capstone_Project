using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using iTextSharp.text;
using iTextSharp.text.pdf;
using RRCManagementSystem.Helpers; // AESHelper

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
            // Windows ID for Manila time zone
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

                // Get the latest payable booking (Assigned or Approved)
                const string bookingQuery = @"
SELECT TOP 1 b.BookingID, b.Price
FROM Bookings b
WHERE b.ClientID = @ClientID
  AND b.Status IN ('Assigned','Approved')
ORDER BY b.CreatedAt DESC;";

                using (SqlCommand cmd = new SqlCommand(bookingQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@ClientID", ddlClients.SelectedValue);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            txtRemainingBalance.Text = "0.00"; // No payable booking found
                            return;
                        }

                        int bookingId = Convert.ToInt32(reader["BookingID"]);
                        decimal price = Convert.ToDecimal(reader["Price"]);
                        reader.Close(); // close before next command

                        // Sum valid payments for that booking
                        const string paymentQuery = @"
SELECT ISNULL(SUM(Amount), 0)
FROM Transactions
WHERE SaleID = @BookingID
  AND (Status IS NULL OR Status NOT IN ('Refunded','Voided'));";

                        using (SqlCommand payCmd = new SqlCommand(paymentQuery, conn))
                        {
                            payCmd.Parameters.AddWithValue("@BookingID", bookingId);
                            decimal totalPayments = Convert.ToDecimal(payCmd.ExecuteScalar());

                            decimal remaining = price - totalPayments;
                            if (remaining < 0) remaining = 0;

                            txtRemainingBalance.Text = remaining.ToString("N2");
                        }
                    }
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

        private int SavePaymentAndBlockchain(decimal amount, string performedBy)
        {
            int bookingId = GetLatestPayableBookingId(Convert.ToInt32(ddlClients.SelectedValue));
            if (bookingId == 0) throw new InvalidOperationException("No payable booking found for this client.");

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

            int transactionId;
            DateTime nowPht = NowPht();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();

                try
                {
                    // 1) Insert Transaction (PHT timestamp)
                    const string insertTransaction = @"
INSERT INTO Transactions
    (SaleID, Amount, TransactionDate, PaymentMethod, Status, PerformedBy, Remarks, Receipt)
OUTPUT INSERTED.TransactionID
VALUES
    (@SaleID, @Amount, @NowPht, @PaymentMethod, 'Manual Adjustment', @PerformedBy, @Remarks, @Receipt);";

                    using (SqlCommand cmd = new SqlCommand(insertTransaction, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@SaleID", bookingId);
                        cmd.Parameters.AddWithValue("@Amount", amount);
                        cmd.Parameters.AddWithValue("@NowPht", nowPht);
                        cmd.Parameters.AddWithValue("@PaymentMethod", ddlPaymentMethod.SelectedValue);
                        cmd.Parameters.AddWithValue("@PerformedBy", performedBy);
                        cmd.Parameters.AddWithValue("@Remarks", txtRemarks.Text.Trim());
                        cmd.Parameters.AddWithValue("@Receipt", "~/Receipts/" + receiptFileName);

                        transactionId = (int)cmd.ExecuteScalar();
                    }

                    // 2) Insert Blockchain (PHT timestamp)
                    string saleDataJson = $@"{{
  ""TransactionID"": ""{transactionId}"",
  ""SaleID"": ""{bookingId}"",
  ""Amount"": ""{amount}"",
  ""PaymentMethod"": ""{ddlPaymentMethod.SelectedValue}"",
  ""Status"": ""Manual Adjustment"",
  ""PerformedBy"": ""{performedBy}"",
  ""Remarks"": ""{txtRemarks.Text.Trim()}"",
  ""TransactionDate"": ""{nowPht:yyyy-MM-dd HH:mm:ss}""
}}";

                    string saleHash = GenerateSHA256Hash(saleDataJson);

                    const string insertBlockchain = @"
INSERT INTO BlockchainSalesLog (TransactionID, SaleHash, SaleDataJson, Timestamp)
VALUES (@TransactionID, @SaleHash, @SaleDataJson, @NowPht);";

                    using (SqlCommand cmd = new SqlCommand(insertBlockchain, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@TransactionID", transactionId);
                        cmd.Parameters.AddWithValue("@SaleHash", saleHash);
                        cmd.Parameters.AddWithValue("@SaleDataJson", saleDataJson);
                        cmd.Parameters.AddWithValue("@NowPht", nowPht);
                        cmd.ExecuteNonQuery();
                    }

                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }

            return transactionId;
        }

        // Use same PHT timestamp in the PDF
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

        // --- Utils --------------------------------------------------------

        private string GenerateSHA256Hash(string rawData)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

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
