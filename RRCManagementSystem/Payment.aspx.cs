using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
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
            if (!IsPostBack)
            {
                if (Session["ClientID"] == null)
                {
                    Response.Redirect("~/Login.aspx");
                    return;
                }

                int clientId = Convert.ToInt32(Session["ClientID"]);
                LoadClientInfo(clientId);
                LoadPaymentHistory(clientId);
                GenerateCheckoutURL(clientId);
            }
        }
        private void LoadClientInfo(int clientId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
SELECT TOP 1 
    ISNULL(s.Name, b.ServiceNames) AS ServiceName, 
    b.Price, b.PaymentPlan, b.BookingID, 
    ISNULL(s.IsContract, 0) AS IsContract, 
    s.ServiceType, b.CreatedAt
FROM Bookings b
LEFT JOIN BookingServices bs ON b.BookingID = bs.BookingID
LEFT JOIN Services s ON bs.ServiceID = s.ServiceID
WHERE b.ClientID = @ClientID AND b.Status = 'Approved'
ORDER BY b.CreatedAt DESC";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ClientID", clientId);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    lblServiceName.Text = reader["ServiceName"]?.ToString() ?? "";

                    bool isContract = reader["IsContract"] != DBNull.Value && Convert.ToBoolean(reader["IsContract"]);
                    lblPaymentPlan.Text = isContract ? "Staggered" : "One-time Pay";

                    decimal price = reader["Price"] != DBNull.Value ? Convert.ToDecimal(reader["Price"]) : 0;
                    int bookingId = reader["BookingID"] != DBNull.Value ? Convert.ToInt32(reader["BookingID"]) : 0;
                    DateTime createdAt = reader["CreatedAt"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedAt"]) : DateTime.MinValue;

                    decimal totalPaid = GetTotalPaid(bookingId);
                    decimal remaining = price - totalPaid;

                    if (remaining <= 0)
                    {
                        lblPrice.Text = "Fully Paid";
                        btnPayNow.Visible = false;
                        btnPayWithPayPal.Visible = false;
                        hiddenCheckoutURL.Value = "";
                        hfPayPalAmount.Value = "";

                        ScriptManager.RegisterStartupScript(this, GetType(), "swalPaid", @"
                    Swal.fire({
                        icon: 'success',
                        title: 'Payment Complete',
                        text: 'You have fully paid your service!',
                        confirmButtonColor: '#3085d6'
                    });", true);
                    }
                    else
                    {
                        decimal amountToPay = CalculateNextInstallment(isContract, price, totalPaid);
                        lblPrice.Text = $@"
₱{amountToPay:N2} (Next installment)<br/>
Total Price: ₱{price:N2}<br/>
Already Paid: ₱{totalPaid:N2}<br/>
Remaining Balance: ₱{remaining:N2}";

                        if (isContract)
                        {
                            int monthsElapsed = ((DateTime.Now.Year - createdAt.Year) * 12) + DateTime.Now.Month - createdAt.Month;

                            if (monthsElapsed == 0 && totalPaid < price * 0.5m)
                                lblReminder.Text = "🔔 First installment (50%) is due this month.";
                            else if (monthsElapsed == 1 && totalPaid < price * 0.75m)
                                lblReminder.Text = "🔔 Second installment (25%) is due this month.";
                            else if (monthsElapsed == 2 && totalPaid < price)
                                lblReminder.Text = "🔔 Final installment (25%) is due this month.";
                        }

                        hfPayPalBookingID.Value = bookingId.ToString();
                        hfPayPalAmount.Value = amountToPay.ToString("F2");
                        hfPayPalClientID.Value = clientId.ToString();
                    }
                }
                else
                {
                    lblMessage.Text = "❌ No approved bookings found.";
                    btnPayNow.Visible = false;
                    btnPayWithPayPal.Visible = false;

                    ScriptManager.RegisterStartupScript(this, GetType(), "swalNone", @"
                Swal.fire({
                    icon: 'info',
                    title: 'No Bookings Found',
                    text: 'You currently have no approved bookings.',
                    confirmButtonColor: '#3085d6'
                });", true);
                }
            }
        }




        private decimal GetTotalPaid(int bookingId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT ISNULL(SUM(Amount), 0) FROM Transactions WHERE SaleID = @BookingID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@BookingID", bookingId);
                con.Open();
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }

        private decimal CalculateNextInstallment(bool isContract, decimal fullPrice, decimal totalPaid)
        {
            if (isContract)
            {
                if (totalPaid == 0)
                    return fullPrice * 0.5m;
                else if (totalPaid < fullPrice * 0.75m)
                    return fullPrice * 0.25m;
                else
                    return fullPrice - totalPaid;
            }
            else
            {
                return fullPrice;
            }
        }

        private void LoadPaymentHistory(int clientId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"SELECT TransactionDate, Amount, PaymentMethod, Remarks, Receipt
                                 FROM Transactions
                                 WHERE SaleID IN (SELECT BookingID FROM Bookings WHERE ClientID = @ClientID)
                                 ORDER BY TransactionDate DESC";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ClientID", clientId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvPaymentHistory.DataSource = dt;
                gvPaymentHistory.DataBind();
            }
        }

            private async void GenerateCheckoutURL(int clientId)
            {
                decimal amount = 0;
                string serviceName = "RRC Service Payment";
                int bookingId = 0;
                string referenceNumber = "RRC-" + clientId + "-" + DateTime.Now.Ticks;

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                string query = @"
SELECT TOP 1 
    ISNULL(s.Name, b.ServiceNames) AS ServiceName, 
    b.Price, b.PaymentPlan, b.BookingID, 
    ISNULL(s.IsContract, 0) AS IsContract, 
    s.ServiceType
FROM Bookings b
LEFT JOIN BookingServices bs ON b.BookingID = bs.BookingID
LEFT JOIN Services s ON bs.ServiceID = s.ServiceID
WHERE b.ClientID = @ClientID AND b.Status = 'Approved'
ORDER BY b.CreatedAt DESC";


                SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@ClientID", clientId);
                    con.Open();

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        string dbServiceName = reader["ServiceName"].ToString();
                        if (!string.IsNullOrWhiteSpace(dbServiceName))
                            serviceName = dbServiceName;

                        decimal price = Convert.ToDecimal(reader["Price"]);
                        string plan = reader["PaymentPlan"].ToString();
                        bookingId = Convert.ToInt32(reader["BookingID"]);
                        bool isContract = reader["IsContract"] != DBNull.Value && Convert.ToBoolean(reader["IsContract"]);
                        decimal totalPaid = GetTotalPaid(bookingId);
                        amount = CalculateNextInstallment(isContract, price, totalPaid);

                        if (amount <= 0)
                            return;
                    }
                    else return;
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
                                    amount = (int)(amount * 100),
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
                                success = $"https://2118-136-158-39-124.ngrok-free.app/PaymentConfirmation.aspx?ref={referenceNumber}",
                                failed = $"https://2118-136-158-39-124.ngrok-free.app/Payment.aspx"
                            }
                        }
                    }
                };

                var json = JsonConvert.SerializeObject(payload, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include
                });

                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{payMongoSecretKey}:"));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        HttpResponseMessage response = await client.PostAsync("https://api.paymongo.com/v1/checkout_sessions", content);
                        string result = await response.Content.ReadAsStringAsync();

                        if (!response.IsSuccessStatusCode)
                        {
                            lblMessage.Text = "❌ PayMongo Error: " + result;
                            return;
                        }

                        dynamic jsonResult = JsonConvert.DeserializeObject(result);
                        string checkoutUrl = jsonResult?.data?.attributes?.checkout_url;

                        if (!string.IsNullOrEmpty(checkoutUrl))
                        {
                            hiddenCheckoutURL.Value = checkoutUrl;
                            lblMessage.ForeColor = System.Drawing.Color.Green;
                            lblMessage.Text = $"🔗 <a href='{checkoutUrl}' target='_blank'>Pay Now</a>";
                        }
                        else
                        {
                            lblMessage.Text = "❌ PayMongo Error: Invalid checkout URL.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "❌ PayMongo Exception: " + ex.Message;
                }
            }

        private void SavePayMongoReference(int bookingId, string referenceNumber)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "UPDATE Bookings SET Notes = @ref WHERE BookingID = @bookingId";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ref", "PayMongoRef: " + referenceNumber);
                cmd.Parameters.AddWithValue("@bookingId", bookingId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }


        protected void btnConfirmPayment_Click(object sender, EventArgs e)
                {
                  /*  string refNumber = hiddenReference.Value;
                    if (!string.IsNullOrEmpty(refNumber))
                    {
                        try
                        {
                            using (var client = new System.Net.WebClient())
                            {
                                string webhookUrl = $"{Request.Url.GetLeftPart(UriPartial.Authority)}/PayMongoWebhook.ashx?ref=" + HttpUtility.UrlEncode(refNumber);
                                string result = client.DownloadString(webhookUrl);  
                                ScriptManager.RegisterStartupScript(this, GetType(), "swalConfirmed", $@"
                                    Swal.fire({{
                                        icon: 'success',
                                        title: 'Payment Confirmed',
                                        text: '{HttpUtility.JavaScriptStringEncode(result)}'
                                    }});", true);
                                LoadClientInfo(Convert.ToInt32(Session["ClientID"]));
                                LoadPaymentHistory(Convert.ToInt32(Session["ClientID"]));
                            }
                        }
                        catch (Exception ex)
                        {
                            ScriptManager.RegisterStartupScript(this, GetType(), "swalErr", $@"
                                Swal.fire({{
                                    icon: 'error',
                                    title: 'Error confirming payment',
                                    text: '{HttpUtility.JavaScriptStringEncode(ex.Message)}'
                                }});", true);
                        }
                    } */
                }

            protected void btnPayWithPayPal_Click(object sender, EventArgs e)
            {
                // Deprecated in favor of Smart Button - no logic needed here now
            }




        }
    }
