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
    public class PayMongoWebhook : IHttpHandler
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
                    // Manual fallback: /PayMongoWebhook.ashx?ref=RRC-123-...
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

                    decimal remaining = GetRemainingForBooking(bookingId);
                    if (remaining <= 0m)
                    {
                        context.Response.Write("⚠️ Already fully paid.");
                        return;
                    }

                    // Avoid duplicate manual insert
                    if (ExistsDuplicateTx(bookingId, remaining, reference))
                    {
                        context.Response.Write("ℹ️ Already recorded.");
                        return;
                    }

                    int txId = InsertTransaction(bookingId, remaining, "PayMongo", "Completed", "PayMongo Manual Ref: " + reference);

                    // optional client id for blockchain JSON
                    int clientId = GetClientIdFromBooking(bookingId);
                    BlockchainLogger.AppendSaleLog(cs, txId, new
                    {
                        TransactionID = txId,
                        ClientID = clientId,
                        BookingID = bookingId,
                        Amount = remaining,
                        Currency = "PHP",
                        Method = "PayMongo",
                        Status = "Completed",
                        PaidAtUtc = DateTime.UtcNow
                    });

                    context.Response.Write("✅ Manual webhook success.");
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

        // ============= POST: Real PayMongo webhook =============
        private static void HandlePostWebhook(HttpContext context)
        {
            string body;
            using (var reader = new StreamReader(context.Request.InputStream))
                body = reader.ReadToEnd();

            try
            {
                var root = JObject.Parse(body);

                // Event type
                string type =
                    root.SelectToken("data.attributes.type")?.ToString() ??
                    root.SelectToken("type")?.ToString() ?? "";

                // We care about successful payment events
                bool isPaid =
                    string.Equals(type, "checkout_session.payment.paid", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(type, "payment.paid", StringComparison.OrdinalIgnoreCase);

                if (!isPaid)
                {
                    context.Response.Write("Ignored event: " + (type ?? "null"));
                    return;
                }

                // Reference number (Checkout uses reference_number)
                string reference =
                    root.SelectToken("data.attributes.data.attributes.reference_number")?.ToString() ??
                    root.SelectToken("data.attributes.reference_number")?.ToString() ?? "";

                // Amount in centavos (varies by payload)
                long amountCents =
                    root.SelectToken("data.attributes.data.attributes.amount")?.Value<long?>() ??
                    root.SelectToken("data.attributes.amount")?.Value<long?>() ??
                    root.SelectToken("data.attributes.data.attributes.line_items[0].amount")?.Value<long?>() ??
                    0;

                // Payment method
                string method =
                    root.SelectToken("data.attributes.data.attributes.payments[0].data.attributes.payment_method.type")?.ToString() ??
                    root.SelectToken("data.attributes.payments[0].payment_method.type")?.ToString() ??
                    "PayMongo";

                if (string.IsNullOrWhiteSpace(reference) || amountCents <= 0)
                {
                    context.Response.Write("❌ Missing reference or amount.");
                    return;
                }

                decimal amount = amountCents / 100m;

                // Map reference -> BookingID
                int bookingId = GetBookingIdFromReference(reference);
                if (bookingId <= 0)
                {
                    context.Response.Write("❌ No booking for reference.");
                    return;
                }

                // Dedup by (BookingID, Amount, Reference)
                if (ExistsDuplicateTx(bookingId, amount, reference))
                {
                    context.Response.Write("ℹ️ Already recorded.");
                    return;
                }

                int txId = InsertTransaction(bookingId, amount, "PayMongo", "Completed", "PayMongo Webhook Ref: " + reference);

                int clientId = GetClientIdFromBooking(bookingId);
                BlockchainLogger.AppendSaleLog(cs, txId, new
                {
                    TransactionID = txId,
                    ClientID = clientId,
                    BookingID = bookingId,
                    Amount = amount,
                    Currency = "PHP",
                    Method = method ?? "PayMongo",
                    Status = "Completed",
                    PaidAtUtc = DateTime.UtcNow
                });

                context.Response.Write("✅ PayMongo webhook processed.");
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.Write("❌ Webhook error: " + ex.Message);
            }
        }

        // ============= Shared DB helpers (mirror your PayPal code) =============
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
                pAmount.Precision = 18;
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

        private static bool ExistsDuplicateTx(int bookingId, decimal amount, string reference)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = @"
SELECT 1
FROM Transactions
WHERE SaleID=@BID
  AND ABS(Amount - @Amt) < 0.005
  AND Remarks LIKE @Ref;";
                cmd.Parameters.Add("@BID", SqlDbType.Int).Value = bookingId;

                var pAmt = cmd.Parameters.Add("@Amt", SqlDbType.Decimal);
                pAmt.Precision = 18;
                pAmt.Scale = 2;
                pAmt.Value = amount;

                cmd.Parameters.Add("@Ref", SqlDbType.NVarChar, 255).Value = "%" + reference + "%";

                con.Open();
                var o = cmd.ExecuteScalar();
                return o != null;
            }
        }

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

        private static int GetBookingIdFromReference(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference)) return 0;
            using (var con = new SqlConnection(cs))
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = "SELECT TOP 1 BookingID FROM Bookings WHERE Notes LIKE @ref;";
                cmd.Parameters.Add("@ref", SqlDbType.NVarChar, 255).Value = "%PayMongoRef: " + reference + "%";
                con.Open();
                var r = cmd.ExecuteScalar();
                return (r == null || r == DBNull.Value) ? 0 : Convert.ToInt32(r);
            }
        }

        private static decimal GetRemainingForBooking(int bookingId)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = @"
SELECT CAST(b.Price - ISNULL(t.TotalPaid,0) AS decimal(18,2)) AS Remaining
FROM Bookings b
OUTER APPLY (SELECT SUM(Amount) AS TotalPaid FROM Transactions WHERE SaleID=b.BookingID) t
WHERE b.BookingID=@B;";
                cmd.Parameters.Add("@B", SqlDbType.Int).Value = bookingId;
                con.Open();
                var r = cmd.ExecuteScalar();
                return (r == null || r == DBNull.Value) ? 0m : Convert.ToDecimal(r, CultureInfo.InvariantCulture);
            }
        }

        public bool IsReusable => false;
    }
}
