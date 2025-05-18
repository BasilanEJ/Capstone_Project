using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Web;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class PaymentConfirmation : Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["ClientID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            // PayMongo flow (based on ?ref=)
            string reference = Request.QueryString["ref"];

            // PayPal flow (based on ?custom= which contains BookingID)
            string bookingIdStr = Request.QueryString["custom"];

            if (!string.IsNullOrEmpty(reference))
            {
                HandlePayMongo(reference);
            }
            else if (!string.IsNullOrEmpty(bookingIdStr))
            {
                if (int.TryParse(bookingIdStr, out int bookingId))
                {
                    HandlePayPal(bookingId);
                }
                else
                {
                    ShowMessage("❌ Invalid Booking ID received from PayPal.", isSuccess: false);
                }
            }
            else
            {
                ShowMessage("❌ No payment reference or booking ID received.", isSuccess: false);
            }
        }

        private void HandlePayMongo(string reference)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    string baseUrl = "   https://2118-136-158-39-124.ngrok-free.app";
                    string webhookUrl = $"{baseUrl}/PayMongoWebhook.ashx?ref={HttpUtility.UrlEncode(reference)}";

                    string result = client.DownloadString(webhookUrl);

                    ShowMessage("✅ PayMongo payment processed successfully.", isSuccess: true);
                    litResponse.Text = HttpUtility.HtmlEncode(result);
                }
            }
            catch (Exception ex)
            {
                ShowMessage("❌ Error confirming PayMongo payment: " + ex.Message, isSuccess: false);
            }
        }

        private void HandlePayPal(int bookingId)
        {
            string amountStr = Request.QueryString["amount"];
            string clientIdStr = Request.QueryString["client"];
            int expectedClientId = Convert.ToInt32(Session["ClientID"]);

            if (!decimal.TryParse(amountStr, out decimal amountPaid))
            {
                ShowMessage("❌ Invalid or missing amount.", false);
                return;
            }

            if (!int.TryParse(clientIdStr, out int passedClientId) || passedClientId != expectedClientId)
            {
                ShowMessage("❌ Client validation failed.", false);
                return;
            }

            try
            {
                // Insert the actual payment with full details
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string insertQuery = @"
INSERT INTO Transactions (SaleID, Amount, PaymentMethod, TransactionDate, Remarks)
VALUES (@BookingID, @Amount, 'PayPal', GETDATE(), 'Paid via PayPal - manual confirm')";

                    SqlCommand cmd = new SqlCommand(insertQuery, con);
                    cmd.Parameters.AddWithValue("@BookingID", bookingId);
                    cmd.Parameters.AddWithValue("@Amount", amountPaid);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                ShowMessage("✅ PayPal payment logged successfully.", true);
                litResponse.Text = $"Payment of ₱{amountPaid:N2} received for Booking ID {bookingId}.";
            }
            catch (Exception ex)
            {
                ShowMessage("❌ Error logging PayPal payment: " + ex.Message, false);
            }
        }


        private void ShowMessage(string message, bool isSuccess)
        {
            litStatus.Text = $"<h3 style='color:{(isSuccess ? "green" : "red")};'>{message}</h3>";
        }
    }
}
