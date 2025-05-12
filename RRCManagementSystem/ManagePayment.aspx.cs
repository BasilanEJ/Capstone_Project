using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using iTextSharp.text;
using iTextSharp.text.pdf;
using RRCManagementSystem.Helpers; // For AESHelper

namespace RRCManagementSystem
{
    public partial class ManagePayment : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;
        private static string lastGeneratedEncryptedPDF = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadClients();
            }
        }

        private void LoadClients()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT ClientID, Name FROM Clients WHERE Status = 'Approved'";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                ddlClients.DataSource = reader;
                ddlClients.DataTextField = "Name";
                ddlClients.DataValueField = "ClientID";
                ddlClients.DataBind();
                ddlClients.Items.Insert(0, new System.Web.UI.WebControls.ListItem("-- Select Client --", ""));

            }
        }

        protected void ddlClients_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlClients.SelectedValue))
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string bookingQuery = @"
                    SELECT TOP 1 b.BookingID, b.Price
                    FROM Bookings b
                    WHERE b.ClientID = @ClientID AND b.Status = 'Approved'
                    ORDER BY b.CreatedAt DESC";

                    using (SqlCommand cmd = new SqlCommand(bookingQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@ClientID", ddlClients.SelectedValue);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int bookingId = Convert.ToInt32(reader["BookingID"]);
                                decimal price = Convert.ToDecimal(reader["Price"]);
                                reader.Close();

                                string paymentQuery = "SELECT ISNULL(SUM(Amount), 0) FROM Transactions WHERE SaleID = @BookingID";
                                using (SqlCommand payCmd = new SqlCommand(paymentQuery, conn))
                                {
                                    payCmd.Parameters.AddWithValue("@BookingID", bookingId);
                                    decimal totalPayments = Convert.ToDecimal(payCmd.ExecuteScalar());

                                    decimal remaining = price - totalPayments;
                                    txtRemainingBalance.Text = remaining.ToString("N2");
                                }
                            }
                            else
                            {
                                txtRemainingBalance.Text = "0.00"; // No booking
                            }
                        }
                    }
                }
            }
        }

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

            string fileExtension = Path.GetExtension(fuReceipt.FileName).ToLower();
            if (fileExtension != ".jpg" && fileExtension != ".jpeg" && fileExtension != ".png")
            {
                ShowError("❌ Only JPG and PNG files are allowed.");
                return;
            }

            try
            {
                int clientId = Convert.ToInt32(ddlClients.SelectedValue);
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

        private int SavePaymentAndBlockchain(decimal newBalance, string performedBy)
        {
            int bookingId = GetLatestApprovedBookingId(Convert.ToInt32(ddlClients.SelectedValue));
            int transactionId = 0;

            string receiptFolder = Server.MapPath("~/Receipts/");
            if (!Directory.Exists(receiptFolder))
                Directory.CreateDirectory(receiptFolder);

            // 🔵 Save the encrypted receipt image
            string receiptFileName = Guid.NewGuid() + Path.GetExtension(fuReceipt.FileName);
            string savePath = Path.Combine(receiptFolder, receiptFileName);

            using (var ms = new MemoryStream())
            {
                fuReceipt.PostedFile.InputStream.CopyTo(ms);
                byte[] originalImageBytes = ms.ToArray();

                byte[] encryptedImageBytes = AESHelper.Encrypt(originalImageBytes); // 🔐 Encrypt image

                File.WriteAllBytes(savePath, encryptedImageBytes);
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();

                try
                {
                    // 1. Insert Transaction
                    string insertTransaction = @"
                INSERT INTO Transactions (SaleID, Amount, TransactionDate, PaymentMethod, Status, PerformedBy, Remarks, Receipt)
                OUTPUT INSERTED.TransactionID
                VALUES (@SaleID, @Amount, GETDATE(), @PaymentMethod, 'Manual Adjustment', @PerformedBy, @Remarks, @Receipt)";

                    using (SqlCommand cmd = new SqlCommand(insertTransaction, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@SaleID", bookingId);
                        cmd.Parameters.AddWithValue("@Amount", newBalance);
                        cmd.Parameters.AddWithValue("@PaymentMethod", ddlPaymentMethod.SelectedValue);
                        cmd.Parameters.AddWithValue("@PerformedBy", performedBy);
                        cmd.Parameters.AddWithValue("@Remarks", txtRemarks.Text.Trim());
                        cmd.Parameters.AddWithValue("@Receipt", "~/Receipts/" + receiptFileName);

                        transactionId = (int)cmd.ExecuteScalar();
                    }

                    // 2. Insert Blockchain
                    string saleDataJson = $@"{{
                ""TransactionID"": ""{transactionId}"",
                ""SaleID"": ""{bookingId}"",
                ""Amount"": ""{newBalance}"",
                ""PaymentMethod"": ""{ddlPaymentMethod.SelectedValue}"",
                ""Status"": ""Manual Adjustment"",
                ""PerformedBy"": ""{performedBy}"",
                ""Remarks"": ""{txtRemarks.Text.Trim()}"",
                ""TransactionDate"": ""{DateTime.Now:yyyy-MM-dd HH:mm:ss}""
            }}";

                    string saleHash = GenerateSHA256Hash(saleDataJson);

                    string insertBlockchain = @"
                INSERT INTO BlockchainSalesLog (TransactionID, SaleHash, SaleDataJson, Timestamp)
                VALUES (@TransactionID, @SaleHash, @SaleDataJson, GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(insertBlockchain, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@TransactionID", transactionId);
                        cmd.Parameters.AddWithValue("@SaleHash", saleHash);
                        cmd.Parameters.AddWithValue("@SaleDataJson", saleDataJson);
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



        private string GenerateReceiptPDF(int transactionId, string performedBy)
        {
            string folderPath = Server.MapPath("~/ReceiptsPDF/");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string plainPDF = Path.Combine(folderPath, $"Receipt_{transactionId}.pdf");

            using (Document doc = new Document())
            {
                PdfWriter.GetInstance(doc, new FileStream(plainPDF, FileMode.Create));
                doc.Open();

                string logoPath = Server.MapPath("~/Images/logorrc.png");
                if (File.Exists(logoPath))
                {
                    iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(logoPath);
                    logo.ScaleAbsolute(100, 100);
                    logo.Alignment = Element.ALIGN_CENTER;
                    doc.Add(logo);
                }

                doc.Add(new Paragraph($"Receipt #{transactionId}"));
                doc.Add(new Paragraph("Date: " + DateTime.Now.ToString("yyyy-MM-dd")));
                doc.Add(new Paragraph("Client: " + ddlClients.SelectedItem.Text));
                doc.Add(new Paragraph("Amount Paid: ₱ " + txtNewBalance.Text.Trim()));
                doc.Add(new Paragraph("Payment Method: " + ddlPaymentMethod.SelectedItem.Text));
                doc.Add(new Paragraph("Remarks: " + txtRemarks.Text.Trim()));
                doc.Add(new Paragraph("Handled By: " + performedBy));

                doc.Close();
            }

            // Encrypt the PDF
            byte[] pdfBytes = File.ReadAllBytes(plainPDF);
            byte[] encrypted = AESHelper.Encrypt(pdfBytes);
            string encryptedPath = Path.Combine(folderPath, $"Receipt_{transactionId}_encrypted.pdf");
            File.WriteAllBytes(encryptedPath, encrypted);

            return encryptedPath;
        }

        private int GetLatestApprovedBookingId(int clientId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT TOP 1 BookingID FROM Bookings WHERE ClientID = @ClientID AND Status = 'Approved' ORDER BY CreatedAt DESC";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ClientID", clientId);
                conn.Open();
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        private string GenerateSHA256Hash(string rawData)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private void ShowError(string message)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = "message error";
        }

        private void ClearForm()
        {
            ddlClients.SelectedIndex = 0;
            ddlPaymentMethod.SelectedIndex = 0;
            txtRemainingBalance.Text = "";
            txtNewBalance.Text = "";
            txtRemarks.Text = "";
        }

        protected void btnPrintReceipt_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lastGeneratedEncryptedPDF))
            {
                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "inline; filename=Receipt.pdf");

                byte[] encryptedBytes = File.ReadAllBytes(lastGeneratedEncryptedPDF);
                byte[] decryptedBytes = AESHelper.Decrypt(encryptedBytes);

                Response.BinaryWrite(decryptedBytes);
                Response.End();
            }
        }

        protected void btnDownloadReceipt_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lastGeneratedEncryptedPDF))
            {
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
}