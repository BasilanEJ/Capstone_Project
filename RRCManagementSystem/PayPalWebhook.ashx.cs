using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;

namespace RRCManagementSystem
{
    public class PayPalWebhook : IHttpHandler
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                if (context.Request.HttpMethod == "POST")
                {
                    HandlePostWebhook(context);
                }
                else if (context.Request.HttpMethod == "GET")
                {
                    string bookingIdStr = context.Request.QueryString["custom"];
                    string amountStr = context.Request.QueryString["amount"];
                    string clientIdStr = context.Request.QueryString["client"];

                    if (int.TryParse(bookingIdStr, out int bookingId) && decimal.TryParse(amountStr, out decimal amount))
                    {
                        string remarks = "Manual PayPal confirmation";
                        int transactionId = LogTransaction(bookingId, "PayPal", amount, remarks);
                        LogToBlockchain(transactionId, bookingId, amount, "PayPal");
                        context.Response.Write("✅ Manual PayPal webhook success: Transaction recorded.");
                    }
                    else
                    {
                        context.Response.Write("❌ Invalid query parameters.");
                    }
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
                context.Response.Write("❌ Top-level error: " + ex.Message + "<br/>" + ex.StackTrace);
            }
        }

        private void HandlePostWebhook(HttpContext context)
        {
            string json;
            using (var reader = new StreamReader(context.Request.InputStream))
            {
                json = reader.ReadToEnd();
            }

            try
            {
                JObject payload = JObject.Parse(json);
                string eventType = payload["event_type"]?.ToString();

                if (eventType != "PAYMENT.CAPTURE.COMPLETED")
                {
                    context.Response.Write("Ignored event type: " + eventType);
                    return;
                }

                string bookingIdStr = payload["resource"]?["custom_id"]?.ToString();
                string amountStr = payload["resource"]?["amount"]?["value"]?.ToString();
                string payerEmail = payload["resource"]?["payer"]?["email_address"]?.ToString();

                if (!int.TryParse(bookingIdStr, out int bookingId) || !decimal.TryParse(amountStr, out decimal amount))
                {
                    context.Response.Write("❌ Invalid booking ID or amount in webhook.");
                    return;
                }

                string remarks = "✅ PayPal Webhook from: " + payerEmail;
                int transactionId = LogTransaction(bookingId, "PayPal", amount, remarks);
                LogToBlockchain(transactionId, bookingId, amount, "PayPal");
                context.Response.Write("✅ PayPal webhook processed.");
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.Write("❌ Webhook error: " + ex.Message);
            }
        }

        private int LogTransaction(int bookingId, string method, decimal amount, string refNum)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string insert = @"
                    INSERT INTO Transactions (SaleID, PaymentMethod, Amount, Status, TransactionDate, PerformedBy, Remarks)
                    OUTPUT INSERTED.TransactionID
                    VALUES (@SaleID, @Method, @Amount, 'Completed', GETDATE(), 'System - PayPal', @Remarks)";

                SqlCommand cmd = new SqlCommand(insert, con);
                cmd.Parameters.AddWithValue("@SaleID", bookingId);
                cmd.Parameters.AddWithValue("@Method", method);
                cmd.Parameters.AddWithValue("@Amount", amount);
                cmd.Parameters.AddWithValue("@Remarks", refNum);
                con.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        private void LogToBlockchain(int transactionId, int bookingId, decimal amount, string paymentMethod)
        {
            string saleDataJson = $@"
{{
    ""TransactionID"": ""{transactionId}"",
    ""BookingID"": ""{bookingId}"",
    ""Amount"": ""{amount}"",
    ""PaymentMethod"": ""{paymentMethod}"",
    ""Status"": ""Completed"",
    ""PerformedBy"": ""System - PayPal"",
    ""TransactionDate"": ""{DateTime.Now:yyyy-MM-dd HH:mm:ss}""
}}";

            string saleHash = GenerateSHA256Hash(saleDataJson);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string insert = @"INSERT INTO BlockchainSalesLog (TransactionID, SaleHash, SaleDataJson, Timestamp)
                                  VALUES (@TransactionID, @SaleHash, @SaleDataJson, GETDATE())";

                SqlCommand cmd = new SqlCommand(insert, con);
                cmd.Parameters.AddWithValue("@TransactionID", transactionId);
                cmd.Parameters.AddWithValue("@SaleHash", saleHash);
                cmd.Parameters.AddWithValue("@SaleDataJson", saleDataJson);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private string GenerateSHA256Hash(string rawData)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }

        public bool IsReusable => false;
    }
}
