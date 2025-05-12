using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

public class PayMongoWebhook : IHttpHandler
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
                string reference = context.Request.QueryString["ref"];
                if (!string.IsNullOrEmpty(reference))
                {
                    HandleManualWebhook(reference, context);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Write("❌ Missing 'ref' in GET request.");
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

    private void HandleManualWebhook(string referenceNumber, HttpContext context)
    {
        try
        {
            string paymentMethod = "gcash";
            decimal amount = 0;

            int bookingId = GetBookingIdByReference(referenceNumber);
            if (bookingId == 0)
            {
                context.Response.Write("❌ No booking matched for reference: " + referenceNumber);
                return;
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string sql = @"
                    SELECT Price - ISNULL((SELECT SUM(Amount) FROM Transactions WHERE SaleID = b.BookingID), 0)
                    FROM Bookings b WHERE BookingID = @BookingID";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@BookingID", bookingId);
                con.Open();
                object result = cmd.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                {
                    context.Response.Write("❌ Could not retrieve remaining balance.");
                    return;
                }

                amount = Convert.ToDecimal(result);
            }

            if (amount <= 0)
            {
                context.Response.Write("⚠️ Already fully paid or invalid amount: " + amount);
                return;
            }

            int transactionId = LogTransaction(bookingId, paymentMethod, amount, referenceNumber);
            LogToBlockchain(transactionId, bookingId, amount, paymentMethod);

            context.Response.Write("✅ Manual webhook success: Transaction recorded.");
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            context.Response.Write("❌ Manual webhook error: " + ex.Message + "<br/>" + ex.StackTrace);
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
            var serializer = new JavaScriptSerializer();
            dynamic data = serializer.Deserialize<dynamic>(json);

            string eventType = data["data"]["attributes"]["type"];
            if (eventType == "checkout.session_paid")
            {
                string referenceNumber = data["data"]["attributes"]["reference_number"];
                int amountInCents = Convert.ToInt32(data["data"]["attributes"]["line_items"][0]["amount"]);
                string paymentMethod = data["data"]["attributes"]["payments"][0]["payment_method"]["type"];
                decimal amount = amountInCents / 100m;

                ProcessPayment(referenceNumber, paymentMethod, amount);
            }

            context.Response.StatusCode = 200;
            context.Response.Write("✅ Webhook processed successfully.");
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            context.Response.Write("❌ Webhook error: " + ex.Message);
        }
    }


    private int GetBookingIdByReference(string reference)
    {
        using (SqlConnection con = new SqlConnection(connectionString))
        {
            string query = @"SELECT TOP 1 BookingID FROM Bookings WHERE Notes LIKE @ref";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@ref", "%PayMongoRef: " + reference + "%");
            con.Open();
            object result = cmd.ExecuteScalar();
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }
    }

    private int LogTransaction(int bookingId, string method, decimal amount, string refNum)
    {
        using (SqlConnection con = new SqlConnection(connectionString))
        {
            string insert = @"
                INSERT INTO Transactions (SaleID, PaymentMethod, Amount, Status, TransactionDate, PerformedBy, Remarks)
                OUTPUT INSERTED.TransactionID
                VALUES (@SaleID, @Method, @Amount, 'Completed', GETDATE(), 'System - PayMongo', @Remarks)";

            SqlCommand cmd = new SqlCommand(insert, con);
            cmd.Parameters.AddWithValue("@SaleID", bookingId);
            cmd.Parameters.AddWithValue("@Method", method);
            cmd.Parameters.AddWithValue("@Amount", amount);
            cmd.Parameters.AddWithValue("@Remarks", "✅ PayMongoRef: " + refNum);
            con.Open();
            return (int)cmd.ExecuteScalar();
        }
    }

    private void LogToBlockchain(int transactionId, int bookingId, decimal amount, string paymentMethod)
    {
        string saleDataJson = $@"{{
            ""TransactionID"": ""{transactionId}"",
            ""BookingID"": ""{bookingId}"",
            ""Amount"": ""{amount}"",
            ""PaymentMethod"": ""{paymentMethod}"",
            ""Status"": ""Completed"",
            ""PerformedBy"": ""System - PayMongo"",
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
    private void ProcessPayment(string referenceNumber, string paymentMethod, decimal amount)
    {
        int bookingId = GetBookingIdByReference(referenceNumber);
        if (bookingId > 0)
        {
            int transactionId = LogTransaction(bookingId, paymentMethod, amount, referenceNumber);
            LogToBlockchain(transactionId, bookingId, amount, paymentMethod);
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
