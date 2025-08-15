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

                    // Ensure Sale exists for this booking
                    int saleId = GetOrCreateSaleId(bookingId);

                    decimal remaining = GetRemainingForSale(saleId);
                    if (remaining <= 0m)
                    {
                        context.Response.Write("⚠️ Already fully paid.");
                        return;
                    }

                    // Avoid duplicate manual insert (by SaleID + Amount + Ref)
                    if (ExistsDuplicateTx(saleId, remaining, reference))
                    {
                        context.Response.Write("ℹ️ Already recorded.");
                        return;
                    }

                    int txId = InsertTransaction(
                        saleId,
                        remaining,
                        method: "PayMongo",
                        status: "Completed",
                        remarks: "PayMongo Manual Ref: " + reference
                    );

                    int clientId = GetClientIdFromBooking(bookingId);

                    // Blockchain JSON (UTC timestamp stored; display in PHT as needed)
                    BlockchainLogger.AppendSaleLog(cs, txId, new
                    {
                        TransactionID = txId,
                        ClientID = clientId,
                        BookingID = bookingId,
                        SaleID = saleId,
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

        // Philippine local time helper (UTC+08:00)
        private static DateTime NowPh() => DateTime.UtcNow.AddHours(8);

        // ============= POST: Real PayMongo webhook =============
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

                var root = JObject.Parse(body);

                // Event type (accept main success types)
                string type =
                    root.SelectToken("data.attributes.type")?.ToString() ??
                    root.SelectToken("type")?.ToString() ?? "";

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

                // Amount (in centavos; try several paths)
                long amountCents =
                    root.SelectToken("data.attributes.data.attributes.amount")?.Value<long?>() ??
                    root.SelectToken("data.attributes.amount")?.Value<long?>() ??
                    root.SelectToken("data.attributes.data.attributes.line_items[0].amount")?.Value<long?>() ??
                    0;

                // Payment method (best-effort)
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

                // Ensure Sale exists for this booking
                int saleId = GetOrCreateSaleId(bookingId);

                // Dedup by (SaleID, Amount, Reference pattern in Remarks)
                if (ExistsDuplicateTx(saleId, amount, reference))
                {
                    context.Response.Write("ℹ️ Already recorded.");
                    return;
                }

                int txId = InsertTransaction(
                    saleId,
                    amount,
                    method: method ?? "PayMongo",
                    status: "Completed",
                    remarks: "PayMongo Webhook Ref: " + reference
                );

                int clientId = GetClientIdFromBooking(bookingId);

                // Blockchain JSON (UTC timestamp stored; display in PHT as needed)
                BlockchainLogger.AppendSaleLog(cs, txId, new
                {
                    TransactionID = txId,
                    ClientID = clientId,
                    BookingID = bookingId,
                    SaleID = saleId,
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

        // ============= DB helpers =============

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

        private static bool ExistsDuplicateTx(int saleId, decimal amount, string reference)
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
                // Match whatever pattern you save the reference in (adjust if needed)
                cmd.CommandText = "SELECT TOP 1 BookingID FROM Bookings WHERE Notes LIKE @ref;";
                cmd.Parameters.Add("@ref", SqlDbType.NVarChar, 255).Value = "%PayMongoRef: " + reference + "%";
                con.Open();
                var r = cmd.ExecuteScalar();
                return (r == null || r == DBNull.Value) ? 0 : Convert.ToInt32(r);
            }
        }

        /// <summary>
        /// Remaining balance for a Sale (Booking total - sum of Transactions on that Sale).
        /// </summary>
        private static decimal GetRemainingForSale(int saleId)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = @"
SELECT CAST(b.Price - ISNULL(t.TotalPaid,0) AS decimal(18,2)) AS Remaining
FROM Sales s
JOIN Bookings b ON b.BookingID = s.BookingID
OUTER APPLY (SELECT SUM(Amount) AS TotalPaid FROM Transactions WHERE SaleID = s.SaleID) t
WHERE s.SaleID = @SID;";
                cmd.Parameters.Add("@SID", SqlDbType.Int).Value = saleId;
                con.Open();
                var r = cmd.ExecuteScalar();
                return (r == null || r == DBNull.Value) ? 0m : Convert.ToDecimal(r, CultureInfo.InvariantCulture);
            }
        }
    }
}
