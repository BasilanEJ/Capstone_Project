using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Web;
using Newtonsoft.Json.Linq;
using RRCManagementSystem.Helpers;  // BlockchainLogger

namespace RRCManagementSystem
{
    public class PayPalWebhook : IHttpHandler
    {
        private static readonly string cs =
            ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.TrySkipIisCustomErrors = true;

            try
            {
                if (context.Request.HttpMethod == "POST")
                {
                    HandlePostWebhook(context);
                }
                else if (context.Request.HttpMethod == "GET")
                {
                    // Fallback from your PayPal Smart Buttons fetch(...?custom=&amount=&client=)
                    string bookingIdStr = context.Request.QueryString["custom"];
                    string amountStr = context.Request.QueryString["amount"];
                    string clientIdStr = context.Request.QueryString["client"];

                    if (!int.TryParse(bookingIdStr, out int bookingId))
                    {
                        context.Response.Write("❌ Invalid booking id.");
                        return;
                    }
                    if (!decimal.TryParse(amountStr, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount) || amount <= 0m)
                    {
                        context.Response.Write("❌ Invalid amount.");
                        return;
                    }
                    int clientId = 0;
                    int.TryParse(clientIdStr, out clientId); // optional

                    int txId = InsertTransaction(bookingId, amount, "PayPal", "Completed", "PayPal Smart Buttons");

                    // 🔗 Write blockchain using the central helper (HMAC + chain)
                    BlockchainLogger.AppendSaleLog(cs, txId, new
                    {
                        TransactionID = txId,
                        ClientID = clientId,
                        BookingID = bookingId,
                        Amount = amount,
                        Currency = "PHP",
                        Method = "PayPal",
                        Status = "Completed",
                        PaidAtUtc = DateTime.UtcNow
                    });

                    context.Response.Write("DB updated & blockchain logged.");
                }
                else
                {
                    context.Response.StatusCode = 405;
                    context.Response.Write("❌ Unsupported HTTP method.");
                }
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.Write("❌ Top-level error: " + ex.Message);
            }
        }

        // ========================= Core DB ops =========================

        private static int InsertTransaction(int bookingId, decimal amount, string method, string status, string remarks)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = @"
INSERT INTO Transactions (SaleID, Amount, PaymentMethod, Status, TransactionDate, Remarks)
VALUES (@SaleID, @Amount, @Method, @Status, DATEADD(HOUR, 8, GETUTCDATE()), @Remarks);
SELECT CAST(SCOPE_IDENTITY() AS INT);";


                cmd.Parameters.Add("@SaleID", SqlDbType.Int).Value = bookingId;

                var pAmount = cmd.Parameters.Add("@Amount", SqlDbType.Decimal);
                pAmount.Precision = 18;      // adjust if your column differs
                pAmount.Scale = 2;
                pAmount.Value = amount;

                cmd.Parameters.Add("@Method", SqlDbType.NVarChar, 50).Value = method;
                cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = status;
                cmd.Parameters.Add("@NowUtc", SqlDbType.DateTime2).Value = DateTime.UtcNow;
                cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, 255).Value = remarks ?? "";

                con.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        // ========================= Webhook handler (POST) =========================

        private static void HandlePostWebhook(HttpContext context)
        {
            string body;
            using (var reader = new StreamReader(context.Request.InputStream))
                body = reader.ReadToEnd();

            try
            {
                var payload = JObject.Parse(body);
                var eventType = payload["event_type"]?.ToString();

                // Accept only final capture events
                if (!string.Equals(eventType, "PAYMENT.CAPTURE.COMPLETED", StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.Write("Ignored event: " + (eventType ?? "null"));
                    return;
                }

                string bookingIdStr = payload["resource"]?["custom_id"]?.ToString();
                string amountStr = payload["resource"]?["amount"]?["value"]?.ToString();
                string payerEmail = payload["resource"]?["payer"]?["email_address"]?.ToString();

                if (!int.TryParse(bookingIdStr, out int bookingId))
                {
                    context.Response.Write("❌ Invalid booking id in webhook.");
                    return;
                }
                if (!decimal.TryParse(amountStr, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount) || amount <= 0m)
                {
                    context.Response.Write("❌ Invalid amount in webhook.");
                    return;
                }

                // optional: look up ClientID from booking for better JSON
                int clientId = GetClientIdFromBooking(bookingId);

                int txId = InsertTransaction(
                    bookingId,
                    amount,
                    "PayPal",
                    "Completed",
                    string.IsNullOrEmpty(payerEmail) ? "PayPal Webhook" : ("PayPal Webhook from: " + payerEmail));

                // 🔗 Write blockchain using the helper
                BlockchainLogger.AppendSaleLog(cs, txId, new
                {
                    TransactionID = txId,
                    ClientID = clientId,
                    BookingID = bookingId,
                    Amount = amount,
                    Currency = "PHP",
                    Method = "PayPal",
                    Status = "Completed",
                    PaidAtUtc = DateTime.UtcNow
                });

                context.Response.Write("✅ PayPal webhook processed.");
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.Write("❌ Webhook error: " + ex.Message);
            }
        }

        // Helper: get ClientID for a booking (if you want it in the JSON)
        private static int GetClientIdFromBooking(int bookingId)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = "SELECT TOP 1 ClientID FROM Bookings WHERE BookingID=@B;";
                cmd.Parameters.Add("@B", SqlDbType.Int).Value = bookingId;
                con.Open();
                var r = cmd.ExecuteScalar();
                return (r == null || r == DBNull.Value) ? 0 : Convert.ToInt32(r);
            }
        }

        public bool IsReusable => false;
    }
}
