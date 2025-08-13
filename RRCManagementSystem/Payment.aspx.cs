using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.UI;
using Newtonsoft.Json;

namespace RRCManagementSystem
{
    public partial class Payment : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;
        private readonly string payMongoSecretKey = ConfigurationManager.AppSettings["PayMongoSecretKey"];

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["ClientID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            // Keep hidden field in sync with current dropdown selection
            hfSelectedPlan.Value = ddlPlanChoice.SelectedValue;

            if (!IsPostBack)
            {
                int clientId = Convert.ToInt32(Session["ClientID"]);
                LoadClientInfo(clientId, hfSelectedPlan.Value);
                LoadPaymentHistory(clientId);
            }
        }
        private void SaveSelectedPlanToDb(int bookingId, string plan)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                const string q = "UPDATE Bookings SET PaymentPlan = @Plan WHERE BookingID = @Id";
                using (var cmd = new SqlCommand(q, con))
                {
                    cmd.Parameters.AddWithValue("@Plan", plan ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id", bookingId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }


        protected void ddlPlanChoice_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Session["ClientID"] == null) return;

            int clientId = Convert.ToInt32(Session["ClientID"]);
            string selectedPlan = ddlPlanChoice.SelectedValue;
            hfSelectedPlan.Value = selectedPlan;

            // Get the latest assigned booking ID so we can save the plan
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

            // Make the PayPal button re-read hfPayPalAmount
            ScriptManager.RegisterStartupScript(this, GetType(), "rerenderPP", "renderPayPalButtons();", true);
        }


        private void LoadClientInfo(int clientId, string selectedPlan)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
SELECT TOP 1 
       ISNULL(s.Name, b.ServiceNames) AS ServiceName, 
       b.Price, 
       b.PaymentPlan,        -- <- read what's in DB
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
                        lblMessage.Text = "❌ No approved bookings found.";
                        btnPayNow.Visible = false;
                        btnPayWithPayPal.Visible = false;
                        hfPayPalAmount.Value = "0.00";
                        hiddenCheckoutURL.Value = string.Empty;
                        return;
                    }

                    lblServiceName.Text = reader["ServiceName"]?.ToString() ?? string.Empty;

                    bool isContract = Convert.ToBoolean(reader["IsContract"]);
                    decimal price = Convert.ToDecimal(reader["Price"]);
                    int bookingId = Convert.ToInt32(reader["BookingID"]);
                    string dbPlan = (reader["PaymentPlan"] == DBNull.Value) ? "" : reader["PaymentPlan"].ToString();

                    // Decide which plan to use
                    string effectivePlan = !string.IsNullOrWhiteSpace(selectedPlan)
                                           ? selectedPlan
                                           : dbPlan;

                    // Reasonable defaults
                    if (string.IsNullOrWhiteSpace(effectivePlan))
                        effectivePlan = isContract ? "50-25-25" : "100";

                    // Keep dropdown in sync with effective plan
                    if (ddlPlanChoice.Items.FindByValue(effectivePlan) != null)
                        ddlPlanChoice.SelectedValue = effectivePlan;

                    decimal totalPaid = GetTotalPaid(bookingId);
                    decimal amountToPay = CalculateNextInstallment(isContract, price, totalPaid, effectivePlan);
                    decimal remaining = price - totalPaid;

                    // Show the plan clearly
                    lblPaymentPlan.Text = isContract
                        ? $"Installment Plan ({effectivePlan.Replace("-", "/")})"
                        : "One-time Pay (100%)";

                    if (remaining <= 0m)
                    {
                        lblPrice.Text = "Fully Paid";
                        btnPayNow.Visible = false;
                        btnPayWithPayPal.Visible = false;
                        hfPayPalAmount.Value = "0.00";
                        hiddenCheckoutURL.Value = string.Empty;
                        return;
                    }

                    lblPrice.Text = $@"₱{amountToPay:N2} (Next installment)<br/>
Total Price: ₱{price:N2}<br/>
Already Paid: ₱{totalPaid:N2}<br/>
Remaining Balance: ₱{remaining:N2}";

                    // Hidden fields for PayPal
                    hfPayPalBookingID.Value = bookingId.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    hfPayPalAmount.Value = amountToPay.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
                    hfPayPalClientID.Value = clientId.ToString(System.Globalization.CultureInfo.InvariantCulture);

                    // Optional: regenerate PayMongo checkout link for the same amount
                    GenerateCheckoutURL(clientId, effectivePlan);

                    lblReminder.Text = $"🔍 Plan: {effectivePlan}, ToPay: ₱{amountToPay:N2}";
                    btnPayNow.Visible = true;
                    btnPayWithPayPal.Visible = true;
                }
            }
        }

        private decimal GetTotalPaid(int bookingId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                const string query = "SELECT ISNULL(SUM(Amount), 0) FROM Transactions WHERE SaleID = @BookingID;";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@BookingID", bookingId);
                con.Open();
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }

        private decimal CalculateNextInstallment(bool isContract, decimal fullPrice, decimal totalPaid, string selectedPlan)
        {
            if (!isContract) return fullPrice;

            switch (selectedPlan)
            {
                case "50-25-25":
                    if (totalPaid == 0m) return fullPrice * 0.50m;                 // 50%
                    if (totalPaid < fullPrice * 0.75m) return fullPrice * 0.25m;   // next 25%
                    return fullPrice - totalPaid;                                   // last 25% (or whatever remains)

                case "70-30":
                    return (totalPaid == 0m) ? (fullPrice * 0.70m) : (fullPrice - totalPaid);

                case "100":
                default:
                    return fullPrice - totalPaid;
            }
        }

        private void LoadPaymentHistory(int clientId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
SELECT TransactionDate, Amount, PaymentMethod, Remarks, Receipt
FROM Transactions
WHERE SaleID IN (SELECT BookingID FROM Bookings WHERE ClientID = @ClientID)
ORDER BY TransactionDate DESC;";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ClientID", clientId);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvPaymentHistory.DataSource = dt;
                gvPaymentHistory.DataBind();
            }
        }

        // Generates a PayMongo checkout session for the *current plan installment amount*
        // Note: async void is acceptable here since we only need to populate a hidden field.
        private async void GenerateCheckoutURL(int clientId, string selectedPlan)
        {
            decimal amount = 0m;
            int bookingId = 0;
            string serviceName = "RRC Service Payment";
            string referenceNumber = "RRC-" + clientId + "-" + DateTime.Now.Ticks;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
SELECT TOP 1 
       ISNULL(s.Name, b.ServiceNames) AS ServiceName, 
       b.Price, 
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
                    if (reader.Read())
                    {
                        serviceName = reader["ServiceName"].ToString();
                        decimal price = Convert.ToDecimal(reader["Price"]);
                        bookingId = Convert.ToInt32(reader["BookingID"]);
                        bool isContract = Convert.ToBoolean(reader["IsContract"]);
                        decimal totalPaid = GetTotalPaid(bookingId);

                        amount = CalculateNextInstallment(isContract, price, totalPaid, selectedPlan);
                    }
                    else
                    {
                        hiddenCheckoutURL.Value = string.Empty;
                        return;
                    }
                }
            }

            if (amount <= 0m)
            {
                hiddenCheckoutURL.Value = string.Empty;
                return;
            }

            SavePayMongoReference(bookingId, referenceNumber);
            hiddenReference.Value = referenceNumber;

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
                                amount = (int)Math.Round(amount * 100m, 0, MidpointRounding.AwayFromZero),
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
                            success = $"{Request.Url.GetLeftPart(UriPartial.Authority)}/PaymentConfirmation.aspx?ref={referenceNumber}",
                            failed = $"{Request.Url.GetLeftPart(UriPartial.Authority)}/Payment.aspx"
                        }
                    }
                }
            };

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string json = JsonConvert.SerializeObject(payload);
                    var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{payMongoSecretKey}:"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync("https://api.paymongo.com/v1/checkout_sessions", content);
                    string result = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        dynamic jsonResult = JsonConvert.DeserializeObject(result);
                        string checkoutUrl = jsonResult?.data?.attributes?.checkout_url;
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

        private void SavePayMongoReference(int bookingId, string referenceNumber)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                const string query = "UPDATE Bookings SET Notes = @ref WHERE BookingID = @bookingId;";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ref", "PayMongoRef: " + referenceNumber);
                cmd.Parameters.AddWithValue("@bookingId", bookingId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        protected void btnConfirmPayment_Click(object sender, EventArgs e)
        {
            // (Optional legacy manual confirm for PayMongo)
            // Left intentionally disabled.
        }

        protected void btnPayWithPayPal_Click(object sender, EventArgs e)
        {
            // Deprecated in favor of Smart Button – nothing to do here.
        }
    }
}
