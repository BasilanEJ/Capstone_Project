using Newtonsoft.Json.Linq;
using RRCManagementSystem.Helpers;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Web;

namespace RRCManagementSystem
{
    /// <summary>
    /// PayMongoWebhook.ashx - Handles PayMongo webhooks with email confirmation and PDF receipt generation
    /// </summary>
    public class PayMongoWebhook : IHttpHandler
    {
        private static readonly string cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        public bool IsReusable => false;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.TrySkipIisCustomErrors = true;

            try
            {
                string method = context.Request.HttpMethod?.ToUpperInvariant() ?? "";
                if (method == "POST")
                {
                    HandlePostWebhook(context);
                }
                else if (method == "GET")
                {
                    HandleGetFallback(context);
                }
                else
                {
                    context.Response.StatusCode = 405;
                    context.Response.Write("❌ Unsupported HTTP method.");
                }
            }
            catch (Exception ex)
            {
                LogToFile($"❌ TOP-LEVEL EXCEPTION: {ex.Message}\n{ex.StackTrace}");
                context.Response.StatusCode = 500;
                context.Response.Write("❌ Top-level error: " + ex.Message);
            }
        }

        // ========================= GET fallback (?ref=RAW_REFERENCE) =========================
        private static void HandleGetFallback(HttpContext context)
        {
            string reference = context.Request.QueryString["ref"];
            if (string.IsNullOrWhiteSpace(reference))
            {
                context.Response.Write("❌ Missing ref.");
                return;
            }

            int bookingId = GetBookingIdFromReference(reference);
            if (bookingId <= 0)
            {
                context.Response.Write("❌ No booking matched for reference.");
                return;
            }

            int saleId = GetOrCreateSaleId(bookingId);
            decimal remaining = GetRemainingForSale(saleId);

            if (remaining <= 0m)
            {
                context.Response.Write("⚠️ Already fully paid.");
                return;
            }

            if (ExistsDuplicateRef(saleId, reference))
            {
                context.Response.Write("ℹ️ Already recorded.");
                return;
            }

            int clientId = GetClientIdFromBooking(bookingId);
            string clientEmail = GetClientEmail(clientId);
            string clientName = GetClientName(clientId);

            // ✅ FETCH DISPLAY CODES
            string bookingCode = GetBookingCodeForDisplay(bookingId);
            string clientNumber = GetClientNumberForDisplay(clientId);

            string receiptFileName = GenerateReceiptFileName(bookingId, "PayMongo");

            int txId = InsertTransaction(
                saleId,
                remaining,
                method: "Bank Transfer",
                status: "Completed",
                remarks: "PayMongo Manual Ref: " + reference,
                reference: reference,
                receipt: receiptFileName
            );

            if (txId <= 0)
            {
                LogToFile("❌ Failed to insert transaction");
                context.Response.Write("❌ Failed to record transaction.");
                return;
            }

            // ✅ PASS DISPLAY CODES
            string receiptPath = GenerateReceiptPDF(
                txId, clientId, bookingId, saleId,
                remaining, "Bank Transfer", reference,
                receiptFileName, clientName,
                clientNumber, bookingCode  // ✅ NEW PARAMETERS
            );

            if (!string.IsNullOrWhiteSpace(clientEmail))
            {
                try
                {
                    string decryptedEmail = AESHelper.DecryptEmail(clientEmail);
                    SendPaymentConfirmationEmail(
                        decryptedEmail, clientName, txId,
                        remaining, "Bank Transfer", reference,
                        receiptPath, bookingId, saleId,
                        clientNumber, bookingCode  // ✅ NEW PARAMETERS
                    );
                }
                catch (Exception emailEx)
                {
                    LogToFile($"⚠️ Email send failed: {emailEx.Message}");
                }
            }

            try
            {
                BlockchainLogger.AppendSaleLog(cs, txId, new
                {
                    TransactionID = txId,
                    ClientID = clientId,
                    BookingID = bookingId,
                    SaleID = saleId,
                    Amount = remaining,
                    Currency = "PHP",
                    Method = "Bank Transfer",
                    Status = "Completed",
                    PaidAtUtc = DateTime.UtcNow
                });
            }
            catch (Exception bcEx)
            {
                LogToFile($"⚠️ Blockchain logging failed: {bcEx.Message}");
            }

            context.Response.Write("✅ Manual webhook success. Email sent.");
        }

        // ========================= POST: Real PayMongo webhook =========================
        private static void HandlePostWebhook(HttpContext context)
        {
            string body;
            using (var reader = new StreamReader(context.Request.InputStream))
                body = reader.ReadToEnd();

            LogToFile($"=== NEW WEBHOOK ===\nRaw Body: {body}\n");

            try
            {
                if (string.IsNullOrWhiteSpace(body))
                {
                    context.Response.Write("❌ Empty body.");
                    return;
                }

                var root = JObject.Parse(body);

                string type =
                    root.SelectToken("data.attributes.type")?.ToString() ??
                    root.SelectToken("type")?.ToString() ?? "";

                bool isPaid =
                    string.Equals(type, "checkout_session.payment.paid", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(type, "payment.paid", StringComparison.OrdinalIgnoreCase);

                if (!isPaid)
                {
                    LogToFile($"Event ignored: {type}");
                    context.Response.Write("Ignored event: " + (type ?? "null"));
                    return;
                }

                string reference =
                    root.SelectToken("data.attributes.data.attributes.reference_number")?.ToString() ??
                    root.SelectToken("data.attributes.reference_number")?.ToString() ??
                    root.SelectToken("data.attributes.data.attributes.reference")?.ToString() ??
                    root.SelectToken("data.attributes.reference")?.ToString() ??
                    "";

                long? amountCents =
                    root.SelectToken("data.attributes.data.attributes.amount")?.Value<long?>() ??
                    root.SelectToken("data.attributes.amount")?.Value<long?>() ??
                    root.SelectToken("data.attributes.data.attributes.line_items[0].amount")?.Value<long?>();

                string method =
                    root.SelectToken("data.attributes.data.attributes.payments[0].data.attributes.payment_method.type")?.ToString() ??
                    root.SelectToken("data.attributes.payments[0].payment_method.type")?.ToString() ??
                    "Bank Transfer";

                if (string.IsNullOrWhiteSpace(reference) || !amountCents.HasValue || amountCents.Value <= 0)
                {
                    LogToFile($"❌ Missing data - Ref: {reference}, Amount: {amountCents}");
                    context.Response.Write("❌ Missing reference or amount.");
                    return;
                }

                decimal amount = amountCents.Value / 100m;
                LogToFile($"Parsed - Reference: {reference}, Amount: {amount}, Method: {method}");

                int bookingId = GetBookingIdFromReference(reference);
                if (bookingId <= 0)
                {
                    LogToFile($"❌ No booking found for reference: {reference}");
                    context.Response.Write("❌ No booking for reference.");
                    return;
                }

                LogToFile($"Found BookingID: {bookingId}");

                int saleId = GetOrCreateSaleId(bookingId);
                LogToFile($"SaleID: {saleId}");

                if (ExistsDuplicateRef(saleId, reference))
                {
                    LogToFile($"⚠️ Duplicate reference detected: {reference}");
                    context.Response.Write("ℹ️ Already recorded.");
                    return;
                }

                int clientId = GetClientIdFromBooking(bookingId);
                string clientEmail = GetClientEmail(clientId);
                string clientName = GetClientName(clientId);

                // ✅ FETCH DISPLAY CODES
                string bookingCode = GetBookingCodeForDisplay(bookingId);
                string clientNumber = GetClientNumberForDisplay(clientId);

                LogToFile($"ClientID: {clientId}, Name: {clientName}, Email: {(string.IsNullOrWhiteSpace(clientEmail) ? "MISSING" : "Found")}");
                LogToFile($"Display - ClientNumber: {clientNumber}, BookingCode: {bookingCode}");

                string receiptFileName = GenerateReceiptFileName(bookingId, "PayMongo");
                LogToFile($"Receipt filename: {receiptFileName}");

                int txId = InsertTransaction(
                    saleId,
                    amount,
                    method ?? "Bank Transfer",
                    "Completed",
                    "PayMongo Webhook Ref: " + reference,
                    reference,
                    receiptFileName
                );

                if (txId <= 0)
                {
                    LogToFile("❌ Failed to insert transaction");
                    context.Response.Write("❌ Failed to record transaction.");
                    return;
                }

                LogToFile($"✅ Transaction inserted - TxID: {txId}");

                LogToFile($"Starting PDF generation...");
                string receiptPath = null;

                try
                {
                    // ✅ PASS DISPLAY CODES
                    receiptPath = GenerateReceiptPDF(
                        txId, clientId, bookingId, saleId,
                        amount, method ?? "Bank Transfer", reference,
                        receiptFileName, clientName,
                        clientNumber, bookingCode  // ✅ NEW PARAMETERS
                    );

                    if (string.IsNullOrWhiteSpace(receiptPath))
                    {
                        LogToFile($"⚠️ PDF generation returned null");
                    }
                    else if (!File.Exists(receiptPath))
                    {
                        LogToFile($"⚠️ PDF file not found at: {receiptPath}");
                    }
                    else
                    {
                        LogToFile($"✅ PDF generated successfully at: {receiptPath}");
                    }
                }
                catch (Exception pdfEx)
                {
                    LogToFile($"❌ PDF generation exception: {pdfEx.Message}\n{pdfEx.StackTrace}");
                }

                if (!string.IsNullOrWhiteSpace(clientEmail))
                {
                    try
                    {
                        string decryptedEmail = AESHelper.DecryptEmail(clientEmail);
                        LogToFile($"Decrypted email: {decryptedEmail}");

                        SendPaymentConfirmationEmail(
                            decryptedEmail, clientName, txId,
                            amount, method ?? "Bank Transfer", reference,
                            receiptPath, bookingId, saleId,
                            clientNumber, bookingCode  // ✅ NEW PARAMETERS
                        );

                        LogToFile($"✅ Email sent successfully to: {decryptedEmail}");
                    }
                    catch (Exception emailEx)
                    {
                        LogToFile($"❌ Email exception: {emailEx.Message}\n{emailEx.StackTrace}");
                    }
                }
                else
                {
                    LogToFile($"⚠️ No client email - skipping email send");
                }

                try
                {
                    BlockchainLogger.AppendSaleLog(cs, txId, new
                    {
                        TransactionID = txId,
                        ClientID = clientId,
                        BookingID = bookingId,
                        SaleID = saleId,
                        Amount = amount,
                        Currency = "PHP",
                        Method = method ?? "Bank Transfer",
                        Status = "Completed",
                        PaidAtUtc = DateTime.UtcNow
                    });
                    LogToFile($"✅ Blockchain logged");
                }
                catch (Exception bcEx)
                {
                    LogToFile($"⚠️ Blockchain logging failed: {bcEx.Message}");
                }

                LogToFile($"=== WEBHOOK COMPLETED SUCCESSFULLY ===\n");
                context.Response.Write("✅ PayMongo webhook processed. Email sent.");
            }
            catch (Exception ex)
            {
                LogToFile($"❌ TOP-LEVEL ERROR: {ex.Message}\n{ex.StackTrace}");
                context.Response.StatusCode = 500;
                context.Response.Write("❌ Webhook error: " + ex.Message);
            }
        }

        // ✅ Helper method for logging
        private static void LogToFile(string message)
        {
            try
            {
                string logPath = HttpContext.Current.Server.MapPath("~/App_Data/PayMongoWebhook.log");
                string logDir = Path.GetDirectoryName(logPath);

                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);

                string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
                File.AppendAllText(logPath, $"[{timestamp}] {message}\n");
            }
            catch
            {
                // Ignore logging errors
            }
        }

        // ========================= RECEIPT GENERATION =========================

        private static string GenerateReceiptFileName(int bookingId, string method)
        {
            string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            string random = Guid.NewGuid().ToString("N").Substring(0, 8);
            return $"Receipt_{bookingId}_{method}_{timestamp}_{random}.pdf";
        }

        private static string GenerateReceiptPDF(
      int txId, int clientId, int bookingId, int saleId,
      decimal amount, string method, string reference,
      string fileName, string clientName,
      string clientNumber, string bookingCode)  // ✅ ADD THESE TWO
        {
            try
            {
                LogToFile($"PDF Gen - Getting payment info for Booking {bookingId}");
                var paymentInfo = GetPaymentInfo(bookingId, saleId, amount);
                LogToFile($"PDF Gen - Payment info retrieved");

                string receiptsDir = HttpContext.Current.Server.MapPath("~/Receipts");
                LogToFile($"PDF Gen - Receipts directory: {receiptsDir}");

                if (!Directory.Exists(receiptsDir))
                {
                    Directory.CreateDirectory(receiptsDir);
                    LogToFile($"PDF Gen - Created receipts directory");
                }

                string filePath = Path.Combine(receiptsDir, fileName);
                LogToFile($"PDF Gen - Full file path: {filePath}");

                LogToFile($"PDF Gen - Calling GeneratePDFReceipt...");
                GeneratePDFReceipt(
                    filePath, txId, clientName, clientId,
                    bookingId, saleId, amount, method,
                    reference, paymentInfo,
                    clientNumber, bookingCode  // ✅ PASS THEM HERE
                );

                LogToFile($"PDF Gen - Checking if file exists...");
                if (File.Exists(filePath))
                {
                    LogToFile($"PDF Gen - ✅ File created successfully, size: {new FileInfo(filePath).Length} bytes");
                    return filePath;
                }
                else
                {
                    LogToFile($"PDF Gen - ❌ File not found after generation");
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogToFile($"PDF Gen - ❌ EXCEPTION: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        private static void GeneratePDFReceipt(
      string filePath, int txId, string clientName, int clientId,
      int bookingId, int saleId, decimal amount, string method,
      string reference, PaymentInfo info,
      string clientNumber, string bookingCode)  // ✅ ADD THESE TWO
        {
            var converter = new SelectPdf.HtmlToPdf();

            converter.Options.WebPageFixedSize = false;
            converter.Options.WebPageWidth = 1024;
            converter.Options.WebPageHeight = 0;
            converter.Options.PdfPageSize = SelectPdf.PdfPageSize.A4;
            converter.Options.PdfPageOrientation = SelectPdf.PdfPageOrientation.Portrait;
            converter.Options.MarginTop = 20;
            converter.Options.MarginBottom = 20;
            converter.Options.MarginLeft = 20;
            converter.Options.MarginRight = 20;

            string html = GenerateReceiptHTML(
                txId, clientName, clientId, bookingId,
                saleId, amount, method, reference, info,
                clientNumber, bookingCode  // ✅ PASS THEM HERE
            );

            var doc = converter.ConvertHtmlString(html);
            doc.Save(filePath);
            doc.Close();
        }

        private static string GenerateReceiptHTML(
            int txId, string clientName, int clientId,
            int bookingId, int saleId, decimal amount,
            string method, string reference, PaymentInfo info,
            string clientNumber, string bookingCode)
        {
            var ph = new CultureInfo("en-PH");

            // Get Philippine time
            TimeZoneInfo phTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
            DateTime phTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, phTimeZone);
            string now = phTime.ToString("MMMM dd, yyyy hh:mm tt", ph);
            string generatedAt = phTime.ToString("yyyy-MM-dd hh:mm:ss tt", ph);

            string logoPath = HttpContext.Current.Server.MapPath("~/Images/logorrc.png");
            string logoBase64 = "";

            try
            {
                if (File.Exists(logoPath))
                {
                    byte[] imageBytes = File.ReadAllBytes(logoPath);
                    logoBase64 = Convert.ToBase64String(imageBytes);
                }
            }
            catch (Exception ex)
            {
                LogToFile($"⚠️ Logo loading failed: {ex.Message}");
            }

            string logoHtml = string.IsNullOrEmpty(logoBase64)
                ? "<div style='width:115px;height:115px;background:#e5e7eb;'></div>"
                : $"<img src='data:image/png;base64,{logoBase64}' alt='RRC Logo' class='company-logo' />";

            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
<meta charset='UTF-8'>
<meta name='viewport' content='width=device-width, initial-scale=1.0'>
<style>
    body {{
        font-family: 'Segoe UI', Arial, sans-serif;
        background: #f4f6f9;
        margin: 0;
        padding: 30px;
        color: #333;
        -webkit-font-smoothing: antialiased;
    }}

    .receipt-wrapper {{
        max-width: 800px;
        margin: 0 auto;
        background: #ffffff;
        border-radius: 12px;
        box-shadow: 0 4px 20px rgba(0,0,0,0.08);
        padding: 40px 45px;
    }}

    .header {{
        text-align: center;
        border-bottom: 3px solid #2563eb;
        padding-bottom: 25px;
        margin-bottom: 35px;
    }}

    .company-logo {{
        width: 115px;
        height: auto;
        margin-bottom: 12px;
    }}

    .company-name {{
        font-size: 30px;
        font-weight: 800;
        color: #1e3a8a;
        margin: 5px 0 10px;
        letter-spacing: 0.7px;
    }}

    .company-info {{
        font-size: 13px;
        color: #6b7280;
        line-height: 1.5;
    }}

    .receipt-title {{
        font-size: 26px;
        font-weight: 700;
        color: #047857;
        margin-top: 18px;
    }}

    .receipt-number {{
        font-size: 14px;
        color: #6b7280;
        margin-top: 5px;
    }}

    .section {{
        background: #f9fafb;
        padding: 20px 25px;
        border-radius: 10px;
        margin-bottom: 25px;
        border: 1px solid #e5e7eb;
    }}

    .section-title {{
        font-size: 18px;
        font-weight: 700;
        margin-bottom: 12px;
        color: #1e3a8a;
        padding-bottom: 8px;
        border-bottom: 2px solid #e5e7eb;
    }}

    .info-row {{
        display: flex;
        justify-content: space-between;
        padding: 8px 0;
        border-bottom: 1px solid #e5e7eb;
        font-size: 14px;
    }}

    .info-label {{
        font-weight: 600;
        color: #475569;
    }}

    .info-value {{
        color: #111827;
        font-weight: 500;
    }}

    .info-row:last-child {{
        border-bottom: none;
    }}

    .amount-box {{
        background: linear-gradient(135deg, #2563eb, #1e40af);
        color: white;
        padding: 25px;
        border-radius: 12px;
        text-align: center;
        margin: 30px 0;
        box-shadow: 0 4px 12px rgba(37,99,235,0.35);
    }}

    .amount-label {{
        font-size: 14px;
        opacity: 0.85;
        letter-spacing: 0.5px;
    }}

    .amount-value {{
        font-size: 38px;
        font-weight: 800;
        margin-top: 8px;
        letter-spacing: 1px;
    }}

    table {{
        width: 100%;
        border-collapse: collapse;
        margin-top: 10px;
        font-size: 14px;
    }}

    thead th {{
        background: #1e3a8a;
        color: white;
        padding: 12px;
        border: none;
        text-align: left;
        font-weight: 600;
        font-size: 14px;
    }}

    td {{
        padding: 11px 12px;
        border-bottom: 1px solid #e5e7eb;
    }}

    tr:nth-child(even) {{
        background: #f8fafc;
    }}

    .total-row {{
        background: #dbeafe !important;
        color: #1e3a8a;
        font-weight: 700;
    }}

    .highlight {{
        background: #fef3c7 !important;
        font-weight: 700;
    }}

    .status-badge {{
        background: #10b981;
        color: white;
        padding: 6px 18px;
        border-radius: 20px;
        font-size: 12px;
        font-weight: 700;
        display: inline-block;
    }}

    .thank-you {{
        text-align: center;
        font-size: 20px;
        font-weight: 700;
        color: #047857;
        margin: 30px 0 15px;
    }}

    .footer {{
        text-align: center;
        font-size: 11px;
        color: #6b7280;
        margin-top: 40px;
        line-height: 1.6;
        border-top: 1px solid #e5e7eb;
        padding-top: 20px;
    }}
</style>
</head>
<body>
<div class='receipt-wrapper'>
    <div class='header'>
        {logoHtml}
        <div class='company-name'>RRC TERMITE & PEST CONTROL</div>
        <div class='company-info'>
            Professional Pest Control & Termite Management Services<br>
            📞 +639924357834 &nbsp; | &nbsp; 📧 rrctermiteandpestcontrol@gmail.com<br>
            🌐 www.rrcpestcontrol.com
        </div>
        <div class='receipt-title'>OFFICIAL RECEIPT</div>
        <div class='receipt-number'>Receipt No: RRC-TX-{txId:D6}</div>
        <div class='receipt-number'>{now}</div>
    </div>

    <div class='section'>
        <div class='section-title'>Transaction Details</div>
        <div class='info-row'>
            <span class='info-label'>Transaction ID</span>
            <span class='info-value'>TX-{txId:D6}</span>
        </div>
        <div class='info-row'>
            <span class='info-label'>Reference Number</span>
            <span class='info-value'>{HttpUtility.HtmlEncode(reference)}</span>
        </div>
        <div class='info-row'>
            <span class='info-label'>Transaction Date</span>
            <span class='info-value'>{now}</span>
        </div>
        <div class='info-row'>
            <span class='info-label'>Payment Status</span>
            <span class='info-value'><span class='status-badge'>✓ COMPLETED</span></span>
        </div>
    </div>

    <div class='section'>
        <div class='section-title'>Customer Information</div>
        <div class='info-row'>
            <span class='info-label'>Client Name</span>
            <span class='info-value'>{HttpUtility.HtmlEncode(clientName)}</span>
        </div>
        <div class='info-row'>
            <span class='info-label'>Client Number</span>
            <span class='info-value'>{HttpUtility.HtmlEncode(clientNumber)}</span>
        </div>
        <div class='info-row'>
            <span class='info-label'>Booking Code</span>
            <span class='info-value'>{HttpUtility.HtmlEncode(bookingCode)}</span>
        </div>
    </div>

    <div class='section'>
        <div class='section-title'>Service Information</div>
        <div class='info-row'>
            <span class='info-label'>Service Name</span>
            <span class='info-value'>{HttpUtility.HtmlEncode(info.ServiceName)}</span>
        </div>
        <div class='info-row'>
            <span class='info-label'>Service Address</span>
            <span class='info-value'>{HttpUtility.HtmlEncode(info.ServiceAddress)}</span>
        </div>
        <div class='info-row'>
            <span class='info-label'>Payment Plan</span>
            <span class='info-value'>{HttpUtility.HtmlEncode(info.PaymentPlan)}</span>
        </div>
    </div>

    <div class='amount-box'>
        <div class='amount-label'>Amount Paid</div>
        <div class='amount-value'>₱{amount.ToString("N2", ph)}</div>
    </div>

    <div class='section'>
        <div class='section-title'>Payment Breakdown</div>
        <table>
            <thead>
                <tr>
                    <th>Description</th>
                    <th style='text-align:right;'>Amount</th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td>Base Service Price</td>
                    <td style='text-align:right;'>₱{info.BasePrice.ToString("N2", ph)}</td>
                </tr>
                <tr>
                    <td>Travel Expense</td>
                    <td style='text-align:right;'>₱{info.TravelExpense.ToString("N2", ph)}</td>
                </tr>
                <tr>
                    <td>Miscellaneous</td>
                    <td style='text-align:right;'>₱{info.Miscellaneous.ToString("N2", ph)}</td>
                </tr>
                <tr class='total-row'>
                    <td>Total Contract Price</td>
                    <td style='text-align:right;'>₱{info.TotalPrice.ToString("N2", ph)}</td>
                </tr>
                <tr>
                    <td>Previous Payments</td>
                    <td style='text-align:right;'>₱{info.PreviousPaid.ToString("N2", ph)}</td>
                </tr>
                <tr class='total-row'>
                    <td>Total Paid to Date</td>
                    <td style='text-align:right;'>₱{(info.PreviousPaid + amount).ToString("N2", ph)}</td>
                </tr>
                <tr class='highlight'>
                    <td>Remaining Balance</td>
                    <td style='text-align:right;'>₱{(info.TotalPrice - info.PreviousPaid - amount).ToString("N2", ph)}</td>
                </tr>
            </tbody>
        </table>
    </div>

    <div class='section'>
        <div class='section-title'>Payment Method</div>
        <div class='info-row'>
            <span class='info-label'>Payment Method</span>
            <span class='info-value'>{HttpUtility.HtmlEncode(method)}</span>
        </div>
        <div class='info-row'>
            <span class='info-label'>Gateway Reference</span>
            <span class='info-value'>{HttpUtility.HtmlEncode(reference)}</span>
        </div>
    </div>

    <div class='thank-you'>Thank you for your payment!</div>

    <div class='footer'>
        <strong>This is an official computer-generated receipt. No signature required.</strong><br><br>
        For inquiries:<br>
        rrctermiteandpestcontrol@gmail.com &nbsp; | &nbsp; +639924357834<br>
        🌐 www.rrcmngmnt.com<br><br>
        Generated at: {generatedAt} (Philippine Time)
    </div>
</div>
</body>
</html>";
        }
        // ========================= EMAIL SENDING =========================

        private static void SendPaymentConfirmationEmail(
            string toEmail, string clientName, int txId,
            decimal amount, string method, string reference,
            string receiptPath, int bookingId, int saleId,
            string clientNumber, string bookingCode)
        {
            try
            {
                var ph = new CultureInfo("en-PH");
                var paymentInfo = GetPaymentInfo(bookingId, saleId, amount);

                string subject = $"Payment Confirmation - Receipt #RRC-TX-{txId:D6}";

                string body = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
<meta charset='UTF-8'>
<meta name='viewport' content='width=device-width, initial-scale=1.0'>
<style>
    body {{
        font-family: 'Segoe UI', Arial, sans-serif;
        background-color: #f4f6f9;
        margin: 0;
        padding: 0;
        color: #333;
        -webkit-font-smoothing: antialiased;
    }}

    .email-wrapper {{
        max-width: 650px;
        margin: 30px auto;
        background: #ffffff;
        border-radius: 12px;
        box-shadow: 0 4px 20px rgba(0,0,0,0.06);
        overflow: hidden;
    }}

    .email-header {{
        background: linear-gradient(135deg, #2563eb, #1e40af);
        padding: 40px 30px 32px;
        text-align: center;
        color: #fff;
    }}

    .email-header h1 {{
        margin: 0;
        font-size: 26px;
        font-weight: 700;
        letter-spacing: 0.5px;
    }}
    
    .email-header p {{
        margin: 6px 0 0;
        opacity: 0.9;
        font-size: 14px;
    }}

    .email-content {{
        padding: 35px 30px;
    }}
    
    .email-content p {{
        font-size: 15px;
        line-height: 1.7;
        color: #444;
        margin: 12px 0;
    }}

    .amount-box {{
        background: #047857;
        color: #fff;
        padding: 18px;
        border-radius: 10px;
        text-align: center;
        font-size: 26px;
        font-weight: 700;
        margin: 25px 0;
        letter-spacing: 1px;
        box-shadow: 0 3px 10px rgba(4,120,87,0.25);
    }}

    .section-title {{
        margin-top: 25px;
        padding-bottom: 10px;
        border-bottom: 2px solid #e5e7eb;
        font-size: 18px;
        color: #1e3a8a;
        font-weight: 700;
    }}

    .details {{
        background: #f9fafb;
        border-left: 4px solid #2563eb;
        padding: 18px 20px;
        border-radius: 8px;
        margin-top: 18px;
    }}

    .detail-row {{
        display: flex;
        justify-content: space-between;
        padding: 7px 0;
        font-size: 14px;
        border-bottom: 1px solid #e5e7eb;
    }}

    .detail-row:last-child {{
        border-bottom: none;
    }}

    .detail-label {{
        font-weight: 600;
        color: #475569;
    }}

    .detail-value {{
        font-weight: 500;
        color: #111827;
        text-align: right;
    }}

    .btn-primary {{
        display: inline-block;
        text-decoration: none;
        background: #2563eb;
        color: #fff;
        padding: 12px 28px;
        font-size: 15px;
        font-weight: 600;
        border-radius: 6px;
        margin: 25px 0;
        transition: 0.2s;
    }}

    .btn-primary:hover {{
        background: #1d4ed8;
    }}

    .email-footer {{
        background: #f3f4f6;
        padding: 20px 30px;
        text-align: center;
        font-size: 12px;
        color: #6b7280;
        border-top: 1px solid #e5e7eb;
    }}
</style>
</head>
<body>
<div class='email-wrapper'>
    <div class='email-header'>
        <h1>Payment Confirmation</h1>
        <p>RRC Termite & Pest Control Services</p>
    </div>

    <div class='email-content'>
        <p>Dear <strong>{HttpUtility.HtmlEncode(clientName)}</strong>,</p>

        <p>We are pleased to inform you that your payment has been <strong>successfully received and processed</strong>. Below is a summary of your transaction.</p>

        <div class='amount-box'>
            ₱{amount.ToString("N2", ph)}
        </div>

        <div class='section-title'>Payment Details</div>

        <div class='details'>
            <div class='detail-row'>
                <span class='detail-label'>Transaction ID:</span>
                <span class='detail-value'>RRC-TX-{txId:D6}</span>
            </div>

            <div class='detail-row'>
                <span class='detail-label'>Client Number:</span>
                <span class='detail-value'>{HttpUtility.HtmlEncode(clientNumber)}</span>
            </div>

            <div class='detail-row'>
                <span class='detail-label'>Booking Code:</span>
                <span class='detail-value'>{HttpUtility.HtmlEncode(bookingCode)}</span>
            </div>

            <div class='detail-row'>
                <span class='detail-label'>Reference Number:</span>
                <span class='detail-value'>{HttpUtility.HtmlEncode(reference)}</span>
            </div>

            <div class='detail-row'>
                <span class='detail-label'>Payment Method:</span>
                <span class='detail-value'>{HttpUtility.HtmlEncode(method)}</span>
            </div>

            <div class='detail-row'>
                <span class='detail-label'>Date & Time:</span>
                <span class='detail-value'>{DateTime.Now.ToString("MMMM dd, yyyy • hh:mm tt", ph)}</span>
            </div>

            <div class='detail-row'>
                <span class='detail-label'>Status:</span>
                <span class='detail-value' style='color:#047857; font-weight:700;'>COMPLETED</span>
            </div>
        </div>

        <div class='section-title'>Balance Summary</div>

        <div class='details'>
            <div class='detail-row'>
                <span class='detail-label'>Total Contract Price:</span>
                <span class='detail-value'>₱{paymentInfo.TotalPrice.ToString("N2", ph)}</span>
            </div>

            <div class='detail-row'>
                <span class='detail-label'>Total Paid To Date:</span>
                <span class='detail-value' style='color:#047857; font-weight:700;'>₱{(paymentInfo.PreviousPaid + amount).ToString("N2", ph)}</span>
            </div>

            <div class='detail-row'>
                <span class='detail-label'>Remaining Balance:</span>
                <span class='detail-value' style='color:#b45309; font-weight:700;'>₱{(paymentInfo.TotalPrice - paymentInfo.PreviousPaid - amount).ToString("N2", ph)}</span>
            </div>
        </div>

        <p style='margin-top: 25px; font-size: 15px;'>Your official receipt is attached to this email for your reference.</p>

        <div style='text-align:center;'>
            <a href='https://rrcmngmnt.com/Payment.aspx' class='btn-primary'>
                View Payment History
            </a>
        </div>

        <hr style='margin: 40px 0; border: none; border-top: 1px solid #e5e7eb;'>

        <p style='font-size:14px;'>
            For inquiries or assistance, feel free to reach us at:<br>
            📧 rrctermiteandpestcontrol@gmail.com<br>
            📞 +63 9924357834<br>
            🌐 www.rrcmngmnt.com
        </p>

        <p style='margin-top: 25px;'>
            Thank you for trusting <strong>RRC Termite & Pest Control</strong>.<br>
            We appreciate your continued confidence in our services.
        </p>
    </div>

    <div class='email-footer'>
        This is an automated message. Please do not reply directly.<br>
        © {DateTime.Now.Year} RRC Termite & Pest Control Services. All rights reserved.
    </div>
</div>
</body>
</html>";

                using (var message = new System.Net.Mail.MailMessage())
                {
                    message.From = new System.Net.Mail.MailAddress(
                        ConfigurationManager.AppSettings["emailFrom"] ?? "rrctermiteandpestcontrol@gmail.com",
                        "RRC Pest Control"
                    );
                    message.To.Add(toEmail);
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = true;

                    // ✅ Attach PDF receipt if it exists
                    if (!string.IsNullOrWhiteSpace(receiptPath) && File.Exists(receiptPath))
                    {
                        var attachment = new System.Net.Mail.Attachment(receiptPath);
                        message.Attachments.Add(attachment);
                    }

                    using (var smtp = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new System.Net.NetworkCredential(
                            ConfigurationManager.AppSettings["emailFrom"] ?? "rrctermiteandpestcontrol@gmail.com",
                            ConfigurationManager.AppSettings["emailPassword"]
                        );
                        smtp.EnableSsl = true;
                        smtp.Timeout = 30000;
                        smtp.Send(message);
                    }
                }
            }
            catch (Exception ex)
            {
                LogToFile($"❌ Email Exception: {ex.Message}\n{ex.StackTrace}");
                System.Diagnostics.Debug.WriteLine($"Email sending error: {ex.Message}");
            }
        }

        // ========================= DB HELPERS =========================

        private static int GetOrCreateSaleId(int bookingId)
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

                return (pOut.Value == DBNull.Value) ? 0 : Convert.ToInt32(pOut.Value, CultureInfo.InvariantCulture);
            }
        }

        private static int InsertTransaction(int saleId, decimal amount, string method, string status, string remarks, string reference, string receipt = null)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.usp_Transactions_InsertWithReceipt", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@SaleID", SqlDbType.Int).Value = saleId;

                var pAmt = cmd.Parameters.Add("@Amount", SqlDbType.Decimal);
                pAmt.Precision = 18;
                pAmt.Scale = 2;
                pAmt.Value = amount;

                cmd.Parameters.Add("@PaymentMethod", SqlDbType.NVarChar, 50).Value = method ?? "Bank Transfer";
                cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = status ?? "Completed";
                cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, 255).Value = remarks ?? "";
                cmd.Parameters.Add("@Reference", SqlDbType.NVarChar, 200).Value = (object)reference ?? DBNull.Value;
                cmd.Parameters.Add("@Receipt", SqlDbType.NVarChar, 500).Value = (object)receipt ?? DBNull.Value;

                var pTx = cmd.Parameters.Add("@TransactionID", SqlDbType.Int);
                pTx.Direction = ParameterDirection.Output;

                con.Open();
                cmd.ExecuteNonQuery();

                return (pTx.Value == DBNull.Value) ? 0 : Convert.ToInt32(pTx.Value, CultureInfo.InvariantCulture);
            }
        }

        private static bool ExistsDuplicateRef(int saleId, string referenceToken)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.usp_Transactions_ExistsDuplicateByRef", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@SaleID", SqlDbType.Int).Value = saleId;
                cmd.Parameters.Add("@Reference", SqlDbType.NVarChar, 200).Value = referenceToken ?? "";
                con.Open();
                object o = cmd.ExecuteScalar();
                return (o != null && o != DBNull.Value && Convert.ToInt32(o, CultureInfo.InvariantCulture) == 1);
            }
        }

        private static int GetClientIdFromBooking(int bookingId)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.usp_Bookings_GetClientID", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingId;

                con.Open();
                object o = cmd.ExecuteScalar();
                return (o == null || o == DBNull.Value) ? 0 : Convert.ToInt32(o, CultureInfo.InvariantCulture);
            }
        }

        private static int GetBookingIdFromReference(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference)) return 0;

            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.usp_Bookings_GetByPayMongoReference", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Reference", SqlDbType.NVarChar, 200).Value = reference;

                con.Open();
                object o = cmd.ExecuteScalar();
                return (o == null || o == DBNull.Value) ? 0 : Convert.ToInt32(o, CultureInfo.InvariantCulture);
            }
        }

        private static decimal GetRemainingForSale(int saleId)
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

        private static string GetClientEmail(int clientId)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("SELECT EmailEnc FROM Clients WHERE ClientID=@C", con))
            {
                cmd.Parameters.Add("@C", SqlDbType.Int).Value = clientId;
                con.Open();
                object o = cmd.ExecuteScalar();
                return (o == null || o == DBNull.Value) ? "" : o.ToString();
            }
        }

        private static string GetClientName(int clientId)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("SELECT CONCAT(FirstName, ' ', LastName) FROM Clients WHERE ClientID=@C", con))
            {
                cmd.Parameters.Add("@C", SqlDbType.Int).Value = clientId;
                con.Open();
                object o = cmd.ExecuteScalar();
                return (o == null || o == DBNull.Value) ? "Valued Customer" : o.ToString();
            }
        }

        private static PaymentInfo GetPaymentInfo(int bookingId, int saleId, decimal currentPayment)
        {
            var info = new PaymentInfo();

            using (var con = new SqlConnection(cs))
            {
                con.Open();

                using (var cmd = new SqlCommand(@"
                    SELECT 
                        b.ServiceNames,
                        b.Price,
                        ISNULL(b.TravelExpense, 0) AS TravelExpense,
                        ISNULL(b.Miscellaneous, 0) AS Miscellaneous,
                        b.PaymentPlan,
                        c.StreetEnc,
                        c.BarangayEnc,
                        c.CityEnc
                    FROM Bookings b
                    LEFT JOIN Clients c ON b.ClientID = c.ClientID
                    WHERE b.BookingID = @BookingID", con))
                {
                    cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingId;

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            info.ServiceName = reader["ServiceNames"]?.ToString() ?? "Service";
                            info.TotalPrice = reader["Price"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["Price"]);
                            info.TravelExpense = reader["TravelExpense"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["TravelExpense"]);
                            info.Miscellaneous = reader["Miscellaneous"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["Miscellaneous"]);
                            info.PaymentPlan = reader["PaymentPlan"]?.ToString() ?? "100%";

                            // Decrypt address fields
                            string street = AESHelper.DecryptField(reader["StreetEnc"]?.ToString() ?? "");
                            string barangay = AESHelper.DecryptField(reader["BarangayEnc"]?.ToString() ?? "");
                            string city = AESHelper.DecryptField(reader["CityEnc"]?.ToString() ?? "");

                            info.ServiceAddress = $"{street}, {barangay}, {city}".Trim(new[] { ',', ' ' });
                            if (string.IsNullOrWhiteSpace(info.ServiceAddress))
                                info.ServiceAddress = "N/A";

                            info.BasePrice = info.TotalPrice - (info.TravelExpense + info.Miscellaneous);
                            if (info.BasePrice < 0) info.BasePrice = 0;
                        }
                    }
                }

                using (var cmd = new SqlCommand(@"
                    SELECT ISNULL(SUM(Amount), 0) AS PreviousPaid
                    FROM Transactions
                    WHERE SaleID = @SaleID
                      AND Status IN ('Completed', 'Succeeded', 'Success', 'Paid', 'Manual Adjustment')", con))
                {
                    cmd.Parameters.Add("@SaleID", SqlDbType.Int).Value = saleId;
                    object o = cmd.ExecuteScalar();
                    info.PreviousPaid = (o == null || o == DBNull.Value) ? 0m : Convert.ToDecimal(o);
                    info.PreviousPaid = Math.Max(0, info.PreviousPaid - currentPayment);
                }
            }

            return info;
        }

        private static string GetBookingCodeForDisplay(int bookingId)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("SELECT BookingCode FROM Bookings WHERE BookingID = @BookingID", con))
            {
                cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingId;
                con.Open();
                object o = cmd.ExecuteScalar();
                return (o == null || o == DBNull.Value) ? $"BK-{bookingId:D6}" : o.ToString();
            }
        }

        private static string GetClientNumberForDisplay(int clientId)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("SELECT ClientNumber FROM Clients WHERE ClientID = @ClientID", con))
            {
                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                con.Open();
                object o = cmd.ExecuteScalar();
                return (o == null || o == DBNull.Value) ? $"C-{clientId:D5}" : o.ToString();
            }
        }

        private class PaymentInfo
        {
            public string ServiceName { get; set; }
            public string ServiceAddress { get; set; }
            public string PaymentPlan { get; set; }
            public decimal TotalPrice { get; set; }
            public decimal BasePrice { get; set; }
            public decimal TravelExpense { get; set; }
            public decimal Miscellaneous { get; set; }
            public decimal PreviousPaid { get; set; }
        }
    }
}