using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using Newtonsoft.Json;

namespace RRCManagementSystem
{
    public partial class Payment : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;
        private readonly string payMongoSecretKey = ConfigurationManager.AppSettings["PayMongoSecretKey"];

        protected async void Page_Load(object sender, EventArgs e)
        {
            if (Session["ClientID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            // Keep hidden field in sync with current dropdown selection
            hfSelectedPlan.Value = ddlPlanChoice.SelectedValue;

            // (Optional, safe) Ensure webhook exists for this site (no duplicates)
            await EnsurePayMongoWebhookAsync();

            if (!IsPostBack)
            {
                int clientId = Convert.ToInt32(Session["ClientID"]);
                LoadClientInfo(clientId, hfSelectedPlan.Value);
                LoadPaymentHistory(clientId);

                // Re-render PayPal after first bind
                ScriptManager.RegisterStartupScript(this, GetType(), "renderPPInit", "renderPayPalButtons(); updatePayMongoButton();", true);
            }
        }

        protected void ddlPlanChoice_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Session["ClientID"] == null) return;

            int clientId = Convert.ToInt32(Session["ClientID"]);
            string selectedPlan = ddlPlanChoice.SelectedValue;
            hfSelectedPlan.Value = selectedPlan;

            // Get the latest assigned booking ID so we can save the selected plan in DB
            int bookingId = 0;
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT TOP 1 BookingID FROM Bookings WHERE ClientID=@C AND Status='Assigned' ORDER BY CreatedAt DESC", con))
            {
                cmd.Parameters.AddWithValue("@C", clientId);
                con.Open();
                var o = cmd.ExecuteScalar();
                if (o != null && o != DBNull.Value) bookingId = Convert.ToInt32(o);
            }
            if (bookingId > 0) SaveSelectedPlanToDb(bookingId, selectedPlan);

            LoadClientInfo(clientId, selectedPlan);
            LoadPaymentHistory(clientId);

            ScriptManager.RegisterStartupScript(this, GetType(), "rerenderPP", "renderPayPalButtons(); updatePayMongoButton();", true);
        }

        private void SaveSelectedPlanToDb(int bookingId, string plan)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("UPDATE Bookings SET PaymentPlan = @Plan WHERE BookingID = @Id", con))
            {
                cmd.Parameters.AddWithValue("@Plan", (object)plan ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Id", bookingId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void LoadClientInfo(int clientId, string selectedPlan)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
SELECT TOP 1 
       ISNULL(s.Name, b.ServiceNames) AS ServiceName, 
       b.Price, 
       b.PaymentPlan,        -- stored plan (if any)
       b.BookingID, 
       ISNULL(s.IsContract, 0) AS IsContract
FROM Bookings b
LEFT JOIN BookingServices bs ON b.BookingID = bs.BookingID
LEFT JOIN Services s ON bs.ServiceID = s.ServiceID
WHERE b.ClientID = @ClientID AND b.Status = 'Assigned'
ORDER BY b.CreatedAt DESC;";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ClientID", clientId);
                con.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        // No approved/assigned bookings
                        lblServiceName.Text = "";
                        lblPaymentPlan.Text = "";
                        lblPrice.Text = "No approved booking found.";
                        hfPayPalAmount.Value = "0.00";
                        hfPayPalBookingID.Value = "0";
                        hfPayPalClientID.Value = clientId.ToString(CultureInfo.InvariantCulture);
                        hiddenCheckoutURL.Value = string.Empty;
                        hiddenReference.Value = string.Empty;
                        return;
                    }

                    lblServiceName.Text = reader["ServiceName"]?.ToString() ?? string.Empty;

                    bool isContract = Convert.ToBoolean(reader["IsContract"]);
                    decimal fullPrice = Convert.ToDecimal(reader["Price"]);
                    int bookingId = Convert.ToInt32(reader["BookingID"]);
                    string dbPlan = reader["PaymentPlan"] == DBNull.Value ? "" : reader["PaymentPlan"].ToString();

                    // Decide which plan to use (UI-selected > DB-stored > default)
                    string effectivePlan = !string.IsNullOrWhiteSpace(selectedPlan) ? selectedPlan : dbPlan;
                    if (string.IsNullOrWhiteSpace(effectivePlan))
                        effectivePlan = isContract ? "50-25-25" : "100";

                    // Keep dropdown in sync
                    if (ddlPlanChoice.Items.FindByValue(effectivePlan) != null)
                        ddlPlanChoice.SelectedValue = effectivePlan;

                    decimal totalPaid = GetTotalPaid(bookingId);
                    decimal nextAmount = CalculateNextInstallment(isContract, fullPrice, totalPaid, effectivePlan);
                    decimal remaining = Math.Max(0m, fullPrice - totalPaid);

                    lblPaymentPlan.Text = isContract
                        ? $"Installment Plan ({effectivePlan.Replace("-", "/")})"
                        : "One-time Pay (100%)";

                    if (remaining <= 0m)
                    {
                        lblPrice.Text = "Fully Paid";
                        // Hide/disable payment areas via JS
                        hfPayPalAmount.Value = "0.00";
                        hfPayPalBookingID.Value = bookingId.ToString(CultureInfo.InvariantCulture);
                        hfPayPalClientID.Value = clientId.ToString(CultureInfo.InvariantCulture);
                        hiddenCheckoutURL.Value = string.Empty;
                        hiddenReference.Value = string.Empty;
                        return;
                    }

                    lblPrice.Text = $@"₱{nextAmount:N2} (Next installment)<br/>
Total Price: ₱{fullPrice:N2}<br/>
Already Paid: ₱{totalPaid:N2}<br/>
Remaining Balance: ₱{remaining:N2}";

                    // Hidden fields used by PayPal & PayMongo JS
                    hfPayPalBookingID.Value = bookingId.ToString(CultureInfo.InvariantCulture);
                    hfPayPalAmount.Value = nextAmount.ToString("0.00", CultureInfo.InvariantCulture);
                    hfPayPalClientID.Value = clientId.ToString(CultureInfo.InvariantCulture);

                    // Generate/refresh PayMongo Checkout URL for the *current nextAmount*
                    GenerateCheckoutURL(clientId, bookingId, isContract, fullPrice, totalPaid, effectivePlan);
                }
            }
        }

        private decimal GetTotalPaid(int bookingId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("SELECT ISNULL(SUM(Amount), 0) FROM Transactions WHERE SaleID = @BID;", con))
            {
                cmd.Parameters.AddWithValue("@BID", bookingId);
                con.Open();
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }

        private decimal CalculateNextInstallment(bool isContract, decimal fullPrice, decimal totalPaid, string selectedPlan)
        {
            if (!isContract) return fullPrice - totalPaid;

            switch (selectedPlan)
            {
                case "50-25-25":
                    if (totalPaid == 0m) return Math.Round(fullPrice * 0.50m, 2, MidpointRounding.AwayFromZero);      // 50%
                    if (totalPaid < Math.Round(fullPrice * 0.75m, 2, MidpointRounding.AwayFromZero))
                        return Math.Round(fullPrice * 0.25m, 2, MidpointRounding.AwayFromZero);                         // next 25%
                    return Math.Round(fullPrice - totalPaid, 2, MidpointRounding.AwayFromZero);                         // last 25%

                case "70-30":
                    return (totalPaid == 0m)
                        ? Math.Round(fullPrice * 0.70m, 2, MidpointRounding.AwayFromZero)
                        : Math.Round(fullPrice - totalPaid, 2, MidpointRounding.AwayFromZero);

                case "100":
                default:
                    return Math.Round(fullPrice - totalPaid, 2, MidpointRounding.AwayFromZero);
            }
        }

        private void LoadPaymentHistory(int clientId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
SELECT TransactionDate, Amount, PaymentMethod, Remarks, Receipt
FROM Transactions
WHERE SaleID IN (SELECT BookingID FROM Bookings WHERE ClientID = @C)
ORDER BY TransactionDate DESC;";

                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@C", clientId);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        gvPaymentHistory.DataSource = dt;
                        gvPaymentHistory.DataBind();
                    }
                }
            }
        }

        /// <summary>
        /// Creates a PayMongo Checkout Session for the computed next installment,
        /// saves the reference in Bookings.Notes as "PayMongoRef: {reference}".
        /// Exposes checkout_url to JS via hiddenCheckoutURL.
        /// </summary>
        private async void GenerateCheckoutURL(int clientId, int bookingId, bool isContract, decimal fullPrice, decimal totalPaid, string selectedPlan)
        {
            try
            {
                decimal amount = CalculateNextInstallment(isContract, fullPrice, totalPaid, selectedPlan);
                if (amount <= 0m)
                {
                    hiddenCheckoutURL.Value = string.Empty;
                    hiddenReference.Value = string.Empty;
                    return;
                }

                string serviceName = GetServiceNameForBooking(bookingId) ?? "RRC Service Payment";
                // human-friendly reference your webhook will match in Bookings.Notes
                string referenceNumber = "RRC-" + clientId + "-" + DateTime.UtcNow.Ticks;

                SavePayMongoReference(bookingId, referenceNumber);
                hiddenReference.Value = referenceNumber;

                // Build payload
                var payload = new
                {
                    data = new
                    {
                        attributes = new
                        {
                            description = serviceName,
                            billing = new { name = "Client #" + clientId },
                            line_items = new[] {
                                new {
                                    amount = (int)Math.Round(amount * 100m, 0, MidpointRounding.AwayFromZero), // centavos
                                    currency = "PHP",
                                    description = serviceName,
                                    name = "RRC Service",
                                    quantity = 1
                                }
                            },
                            payment_method_types = new[] { "gcash", "card", "paymaya" },
                            reference_number = referenceNumber,
                            send_email_receipt = false,
                            show_description = true,
                            show_line_items = true,
                            redirect = new
                            {
                                success = $"{GetHttpsBaseUrl()}/PaymentConfirmation.aspx?ref={HttpUtility.UrlEncode(referenceNumber)}",
                                failed = $"{GetHttpsBaseUrl()}/Payment.aspx"
                            }
                        }
                    }
                };

                string json = JsonConvert.SerializeObject(payload);

                using (HttpClient client = new HttpClient())
                {
                    var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{payMongoSecretKey}:"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage resp = await client.PostAsync("https://api.paymongo.com/v1/checkout_sessions", content);
                    string result = await resp.Content.ReadAsStringAsync();

                    if (resp.IsSuccessStatusCode)
                    {
                        dynamic doc = JsonConvert.DeserializeObject(result);
                        string checkoutUrl = doc?.data?.attributes?.checkout_url;
                        hiddenCheckoutURL.Value = checkoutUrl ?? string.Empty;
                    }
                    else
                    {
                        hiddenCheckoutURL.Value = string.Empty;
                        lblMessage.Text = "❌ PayMongo Error: " + result;
                    }
                }
            }
            catch (Exception ex)
            {
                hiddenCheckoutURL.Value = string.Empty;
                lblMessage.Text = "❌ Exception: " + ex.Message;
            }
        }

        private string GetServiceNameForBooking(int bookingId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"
SELECT TOP 1 ISNULL(s.Name, b.ServiceNames) AS ServiceName
FROM Bookings b
LEFT JOIN BookingServices bs ON b.BookingID = bs.BookingID
LEFT JOIN Services s ON bs.ServiceID = s.ServiceID
WHERE b.BookingID=@BID;", con))
            {
                cmd.Parameters.AddWithValue("@BID", bookingId);
                con.Open();
                var o = cmd.ExecuteScalar();
                return (o == null || o == DBNull.Value) ? null : o.ToString();
            }
        }

        private void SavePayMongoReference(int bookingId, string referenceNumber)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("UPDATE Bookings SET Notes = @ref WHERE BookingID = @BID;", con))
            {
                cmd.Parameters.AddWithValue("@ref", "PayMongoRef: " + referenceNumber);
                cmd.Parameters.AddWithValue("@BID", bookingId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private string GetHttpsBaseUrl()
        {
            // Force HTTPS base for PayMongo redirect/webhook-friendly URL
            string left = Request.Url.GetLeftPart(UriPartial.Authority);
            if (left.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                left = "https://" + left.Substring("http://".Length);
            return left;
        }

        /// <summary>
        /// Create the webhook once (idempotent). Safe to run on page load.
        /// </summary>
        private async Task EnsurePayMongoWebhookAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(payMongoSecretKey)) return;

                string webhookUrl = $"{GetHttpsBaseUrl().TrimEnd('/')}/PayMongoWebhook.ashx";

                using (var http = new HttpClient())
                {
                    var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(payMongoSecretKey + ":"));
                    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);

                    // 1) Check existing
                    var list = await http.GetAsync("https://api.paymongo.com/v1/webhooks");
                    var listJson = await list.Content.ReadAsStringAsync();

                    bool exists = false;
                    if (list.IsSuccessStatusCode && !string.IsNullOrEmpty(listJson))
                    {
                        dynamic doc = JsonConvert.DeserializeObject(listJson);
                        if (doc?.data != null)
                        {
                            foreach (var w in doc.data)
                            {
                                string url = (string)(w?.attributes?.url ?? "");
                                if (string.Equals(url, webhookUrl, StringComparison.OrdinalIgnoreCase))
                                {
                                    exists = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (exists) return;

                    // 2) Create
                    var payload = new
                    {
                        data = new
                        {
                            attributes = new
                            {
                                url = webhookUrl,
                                events = new[] { "payment.paid", "payment.failed", "checkout_session.payment.paid" }
                            }
                        }
                    };
                    var json = JsonConvert.SerializeObject(payload);
                    var resp = await http.PostAsync(
                        "https://api.paymongo.com/v1/webhooks",
                        new StringContent(json, Encoding.UTF8, "application/json"));
                    // ignore resp content; if it fails we just don't block the page
                }
            }
            catch
            {
                // Do not block the page if webhook ensure fails.
            }
        }

        protected void btnPayWithPayPal_Click(object sender, EventArgs e)
        {
            // Not used (Smart Buttons are JS-based)
        }

        protected void btnConfirmPayment_Click(object sender, EventArgs e)
        {
            // Legacy manual confirm (intentionally blank)
        }
    }
}
