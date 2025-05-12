using System;
using System.Net;
using System.Web;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class PaymentConfirmation : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["ClientID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string reference = Request.QueryString["ref"];
            if (string.IsNullOrEmpty(reference))
            {
                ShowMessage("❌ Missing payment reference number.", isSuccess: false);
                return;
            }

            try
            {
                using (WebClient client = new WebClient())
                {
                    // 👇 Use your ngrok base URL explicitly
                    string baseUrl = "https://448b-136-158-39-124.ngrok-free.app";
                    string webhookUrl = $"{baseUrl}/PayMongoWebhook.ashx?ref={HttpUtility.UrlEncode(reference)}";

                    string result = client.DownloadString(webhookUrl);

                    ShowMessage("✅ Payment processed successfully.", isSuccess: true);
                    litResponse.Text = HttpUtility.HtmlEncode(result);
                }
            }
            catch (Exception ex)
            {
                ShowMessage("❌ Error confirming payment: " + ex.Message, isSuccess: false);
            }
        }

        private void ShowMessage(string message, bool isSuccess)
        {
            litStatus.Text = $"<h3 style='color:{(isSuccess ? "green" : "red")};'>{message}</h3>";
        }
    }
}
