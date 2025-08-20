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
    /// <summary>
    /// /PayMongoWebhook.ashx
    /// Handles PayMongo webhooks (POST) and manual fallback (GET ?ref=RAW_REFERENCE).
    /// Stored procedures expected:
    ///   - dbo.usp_Sales_GetOrCreateByBooking(@BookingID, @SaleID OUTPUT)
    ///   - dbo.usp_Transactions_ExistsDuplicateByRef(@SaleID, @Reference)   <-- NEW
    ///   - dbo.usp_Transactions_Insert(@SaleID, @Amount, @PaymentMethod, @Status, @Remarks, @Reference, @TransactionID OUTPUT)
    ///   - dbo.usp_Bookings_GetClientID(@BookingID)
    ///   - dbo.usp_Bookings_GetByPayMongoReference(@Reference)
    ///   - dbo.usp_Sales_GetRemaining(@SaleID)
    /// </summary>
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
                context.Response.StatusCode = 500;
                context.Response.Write("❌ Top-level error: " + ex.Message);
            }
        }

        // ========================= GET fallback (?ref=RAW_REFERENCE) =========================
        private static void HandleGetFallback(HttpContext context)
        {
            // Manual confirmation: /PayMongoWebhook.ashx?ref=<raw reference_number>
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

            // 🔐 Dedupe strictly by the unique reference (NOT by amount)
            if (ExistsDuplicateRef(saleId, reference))
            {
                context.Response.Write("ℹ️ Already recorded.");
                return;
            }

            int txId = InsertTransaction(
                saleId,
                remaining,
                method: "PayMongo",
                status: "Completed",
                remarks: "PayMongo Manual Ref: " + reference,
                reference: reference
            );

            int clientId = GetClientIdFromBooking(bookingId);

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

        // ========================= POST: Real PayMongo webhook =========================
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

                // Accept only the "paid" events
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

                // RAW reference_number you saved from Payment.aspx.cs
                string reference =
                    root.SelectToken("data.attributes.data.attributes.reference_number")?.ToString() ??
                    root.SelectToken("data.attributes.reference_number")?.ToString() ??
                    root.SelectToken("data.attributes.data.attributes.reference")?.ToString() ??
                    root.SelectToken("data.attributes.reference")?.ToString() ??
                    "";

                // Amount in centavos
                long? amountCents =
                    root.SelectToken("data.attributes.data.attributes.amount")?.Value<long?>() ??
                    root.SelectToken("data.attributes.amount")?.Value<long?>() ??
                    root.SelectToken("data.attributes.data.attributes.line_items[0].amount")?.Value<long?>();

                // Method (best-effort)
                string method =
                    root.SelectToken("data.attributes.data.attributes.payments[0].data.attributes.payment_method.type")?.ToString() ??
                    root.SelectToken("data.attributes.payments[0].payment_method.type")?.ToString() ??
                    "PayMongo";

                if (string.IsNullOrWhiteSpace(reference) || !amountCents.HasValue || amountCents.Value <= 0)
                {
                    context.Response.Write("❌ Missing reference or amount.");
                    return;
                }

                decimal amount = amountCents.Value / 100m;

                // Map reference -> booking
                int bookingId = GetBookingIdFromReference(reference);
                if (bookingId <= 0)
                {
                    context.Response.Write("❌ No booking for reference.");
                    return;
                }

                int saleId = GetOrCreateSaleId(bookingId);

                // 🔐 Dedupe strictly by reference
                if (ExistsDuplicateRef(saleId, reference))
                {
                    context.Response.Write("ℹ️ Already recorded.");
                    return;
                }

                int txId = InsertTransaction(
                    saleId,
                    amount,
                    method ?? "PayMongo",
                    "Completed",
                    "PayMongo Webhook Ref: " + reference,
                    reference
                );

                int clientId = GetClientIdFromBooking(bookingId);

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

        // ========================= DB helpers (via stored procedures) =========================

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

        private static int InsertTransaction(int saleId, decimal amount, string method, string status, string remarks, string reference)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.usp_Transactions_Insert", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@SaleID", SqlDbType.Int).Value = saleId;

                var pAmt = cmd.Parameters.Add("@Amount", SqlDbType.Decimal);
                pAmt.Precision = 18; pAmt.Scale = 2; pAmt.Value = amount;

                cmd.Parameters.Add("@PaymentMethod", SqlDbType.NVarChar, 50).Value = method ?? "PayMongo";
                cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = status ?? "Completed";
                cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, 255).Value = remarks ?? "";
                cmd.Parameters.Add("@Reference", SqlDbType.NVarChar, 200).Value = (object)reference ?? DBNull.Value;

                var pTx = cmd.Parameters.Add("@TransactionID", SqlDbType.Int);
                pTx.Direction = ParameterDirection.Output;

                con.Open();
                cmd.ExecuteNonQuery();
                return (pTx.Value == DBNull.Value) ? 0 : Convert.ToInt32(pTx.Value, CultureInfo.InvariantCulture);
            }
        }

        // Legacy amount+reference check (kept for compatibility, not used here)
        private static bool ExistsDuplicateTx(int saleId, decimal amount, string reference)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.usp_Transactions_ExistsDuplicate", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@SaleID", SqlDbType.Int).Value = saleId;

                var pAmt = cmd.Parameters.Add("@Amount", SqlDbType.Decimal);
                pAmt.Precision = 18; pAmt.Scale = 2; pAmt.Value = amount;

                cmd.Parameters.Add("@Reference", SqlDbType.NVarChar, 200).Value = reference ?? "";

                con.Open();
                object o = cmd.ExecuteScalar();
                return (o != null && o != DBNull.Value && Convert.ToInt32(o, CultureInfo.InvariantCulture) == 1);
            }
        }

        // NEW: reference-only dedupe
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
    }
}
