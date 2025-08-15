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

        public bool IsReusable => false;

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
                    // Fallback for Smart Buttons: /PayPalWebhook.ashx?custom=<bookingId>&amount=123.45&client=<clientId>
                    string bookingIdStr = context.Request.QueryString["custom"];
                    string amountStr = context.Request.QueryString["amount"];
                    string clientIdStr = context.Request.QueryString["client"]; // optional

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
                    int.TryParse(clientIdStr, out int clientId); // optional

                    // Ensure Sale exists for this booking
                    int saleId = GetOrCreateSaleId(bookingId);

                    // Use a simple reference token for dedupe in GET flow
                    string refToken = $"GET:{bookingId}:{amount.ToString("0.00", CultureInfo.InvariantCulture)}";

                    if (ExistsDuplicateTx(saleId, amount, refToken))
                    {
                        context.Response.Write("ℹ️ Already recorded.");
                        return;
                    }

                    int txId = InsertTransaction(
                        saleId,
                        amount,
                        method: "PayPal",
                        status: "Completed",
                        remarks: "PayPal Smart Buttons (GET) Ref: " + refToken
                    );

                    // 🔗 Blockchain (UTC time; show PHT on read)
                    BlockchainLogger.AppendSaleLog(cs, txId, new
                    {
                        TransactionID = txId,
                        ClientID = (clientId > 0 ? clientId : GetClientIdFromBooking(bookingId)),
                        BookingID = bookingId,
                        SaleID = saleId,
                        Amount = amount,
                        Currency = "PHP",
                        Method = "PayPal",
                        Status = "Completed",
                        PaidAtUtc = DateTime.UtcNow
                    });

                    context.Response.Write("✅ DB updated & blockchain logged (GET).");
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

        // Philippine local time helper (UTC+08:00).
        private static DateTime NowPh() => DateTime.UtcNow.AddHours(8);

        // ========================= Webhook handler (POST) =========================
        private static void HandlePostWebhook(HttpContext context)
        {
            string body;
            using (var reader = new StreamReader(context.Request.InputStream))
                body = reader.ReadToEnd();

            try
            {
                if (string.IsNullOrWhiteSpace(body))
                {
                    context.Response.Write("❌ Empty body.");
                    return;
                }

                var payload = JObject.Parse(body);
                var eventType = payload["event_type"]?.ToString();

                // Only accept completed captures
                if (!string.Equals(eventType, "PAYMENT.CAPTURE.COMPLETED", StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.Write("Ignored event: " + (eventType ?? "null"));
                    return;
                }

                // Common PayPal fields
                string bookingIdStr = payload["resource"]?["custom_id"]?.ToString(); // set when you create the order on the client
                string amountStr = payload["resource"]?["amount"]?["value"]?.ToString();
                string payerEmail = payload["resource"]?["payer"]?["email_address"]?.ToString();
                string captureId = payload["resource"]?["id"]?.ToString(); // unique capture id – great for dedupe

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

                // Ensure Sale exists for this booking
                int saleId = GetOrCreateSaleId(bookingId);

                // Dedupe using captureId if available; fall back to a token
                string refToken = !string.IsNullOrWhiteSpace(captureId)
                    ? $"CAPTURE:{captureId}"
                    : $"POST:{bookingId}:{amount.ToString("0.00", CultureInfo.InvariantCulture)}";

                if (ExistsDuplicateTx(saleId, amount, refToken))
                {
                    context.Response.Write("ℹ️ Already recorded.");
                    return;
                }

                int txId = InsertTransaction(
                    saleId,
                    amount,
                    method: "PayPal",
                    status: "Completed",
                    remarks: string.IsNullOrEmpty(payerEmail)
                        ? ("PayPal Webhook Ref: " + refToken)
                        : ($"PayPal Webhook from: {payerEmail} | Ref: {refToken}")
                );

                int clientId = GetClientIdFromBooking(bookingId);

                // 🔗 Blockchain JSON (UTC stored)
                BlockchainLogger.AppendSaleLog(cs, txId, new
                {
                    TransactionID = txId,
                    ClientID = clientId,
                    BookingID = bookingId,
                    SaleID = saleId,
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

        // ========================= Core DB ops =========================

        /// <summary>
        /// Returns existing SaleID for BookingID, or creates a new Sales row using Bookings.ClientID.
        /// </summary>
        private static int GetOrCreateSaleId(int bookingId)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = con.CreateCommand())
            {
                con.Open();
                using (var tx = con.BeginTransaction())
                {
                    cmd.Transaction = tx;

                    // 1) Find existing Sale
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

                    // 3) Create Sale row
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

        private static int InsertTransaction(int saleId, decimal amount, string method, string status, string remarks)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = @"
INSERT INTO Transactions (SaleID, Amount, PaymentMethod, Status, TransactionDate, Remarks)
VALUES (@SaleID, @Amount, @Method, @Status, DATEADD(HOUR, 8, GETUTCDATE()), @Remarks);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

                cmd.Parameters.Add("@SaleID", SqlDbType.Int).Value = saleId;

                var pAmount = cmd.Parameters.Add("@Amount", SqlDbType.Decimal);
                pAmount.Precision = 18;
                pAmount.Scale = 2;
                pAmount.Value = amount;

                cmd.Parameters.Add("@Method", SqlDbType.NVarChar, 50).Value = method;
                cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = status;
                cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, 255).Value = remarks ?? "";

                con.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        private static bool ExistsDuplicateTx(int saleId, decimal amount, string referenceToken)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = @"
SELECT 1
FROM Transactions
WHERE SaleID = @SID
  AND ABS(Amount - @Amt) < 0.005
  AND Remarks LIKE @Ref;";
                cmd.Parameters.Add("@SID", SqlDbType.Int).Value = saleId;

                var pAmt = cmd.Parameters.Add("@Amt", SqlDbType.Decimal);
                pAmt.Precision = 18;
                pAmt.Scale = 2;
                pAmt.Value = amount;

                cmd.Parameters.Add("@Ref", SqlDbType.NVarChar, 255).Value = "%" + referenceToken + "%";

                con.Open();
                var o = cmd.ExecuteScalar();
                return o != null;
            }
        }

        // Helper: get ClientID for a booking (for blockchain JSON)
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
    }
}
