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
    /// /PayPalWebhook.ashx
    /// Handles PayPal webhooks (POST) and GET fallback from Smart Buttons.
    /// All DB operations are via stored procedures created earlier:
    ///   - dbo.usp_Sales_GetOrCreateByBooking(@BookingID, @SaleID OUTPUT)
    ///   - dbo.usp_Transactions_ExistsDuplicate(@SaleID, @Amount, @Reference)
    ///   - dbo.usp_Transactions_Insert(@SaleID, @Amount, @PaymentMethod, @Status, @Remarks, @Reference, @TransactionID OUTPUT)
    ///   - dbo.usp_Bookings_GetClientID(@BookingID)
    /// </summary>
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

        // ========================= GET fallback (Smart Buttons) =========================
        private static void HandleGetFallback(HttpContext context)
        {
            string bookingIdStr = context.Request.QueryString["custom"];
            string amountStr = context.Request.QueryString["amount"];
            string clientIdStr = context.Request.QueryString["client"]; // optional
            string refFromUi = context.Request.QueryString["ref"];    // NEW: capture/order id from UI

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
            int.TryParse(clientIdStr, out int clientId);

            int saleId = GetOrCreateSaleId(bookingId);

            // Build a reference token that is UNIQUE per PayPal payment
            string refToken = !string.IsNullOrWhiteSpace(refFromUi)
                ? $"GET:{refFromUi}"                          // preferred: unique capture/order id
                : $"GET:{bookingId}:{Guid.NewGuid():N}";      // fallback: random nonce

            // 🔐 Dedupe strictly by reference (NOT by amount)
            if (ExistsDuplicateRef(saleId, refToken))
            {
                context.Response.Write("ℹ️ Already recorded.");
                return;
            }

            int txId = InsertTransaction(
                saleId,
                amount,
                method: "PayPal",
                status: "Completed",
                remarks: "PayPal Smart Buttons (GET) Ref: " + refToken,
                reference: refToken
            );

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
                // event type like "PAYMENT.CAPTURE.COMPLETED"
                var eventType = payload["event_type"]?.ToString();

                // Only accept completed captures
                if (!string.Equals(eventType, "PAYMENT.CAPTURE.COMPLETED", StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.Write("Ignored event: " + (eventType ?? "null"));
                    return;
                }

                // Common PayPal fields (Smart Buttons should set custom_id when creating order)
                string bookingIdStr = payload["resource"]?["custom_id"]?.ToString();
                string amountStr = payload["resource"]?["amount"]?["value"]?.ToString();
                string payerEmail = payload["resource"]?["payer"]?["email_address"]?.ToString();
                string captureId = payload["resource"]?["id"]?.ToString(); // unique capture id – perfect for dedupe

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

                int saleId = GetOrCreateSaleId(bookingId);

                // Use PayPal capture id primarily; fall back to deterministic token
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
                        : ($"PayPal Webhook from: {payerEmail} | Ref: {refToken}"),
                    reference: refToken
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

                cmd.Parameters.Add("@PaymentMethod", SqlDbType.NVarChar, 50).Value = method ?? "PayPal";
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

        private static bool ExistsDuplicateTx(int saleId, decimal amount, string referenceToken)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.usp_Transactions_ExistsDuplicate", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@SaleID", SqlDbType.Int).Value = saleId;

                var pAmt = cmd.Parameters.Add("@Amount", SqlDbType.Decimal);
                pAmt.Precision = 18; pAmt.Scale = 2; pAmt.Value = amount;

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
    }
}
