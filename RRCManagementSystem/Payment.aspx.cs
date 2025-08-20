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
using System.Web.UI.WebControls;
using Newtonsoft.Json;

namespace RRCManagementSystem
{
    public partial class Payment : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;
        private readonly string payMongoSecretKey = ConfigurationManager.AppSettings["PayMongoSecretKey"];

        // ==== KPI backing fields (re-applied in OnPreRender to survive master.DataBind) ====
        private string _kpiNext = "₱0.00";
        private string _kpiTotal = "₱0.00";
        private string _kpiPaid = "₱0.00";
        private string _kpiRemain = "₱0.00";

        // ===== Lifecycle Guards =========================================================
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (Session["ClientID"] == null)
            {
                // Safe redirect (avoid ThreadAbortException)
                Response.Redirect("~/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        protected async void Page_Load(object sender, EventArgs e)
        {
            if (Session["ClientID"] == null) return;

            // Keep hidden field synced with dropdown
            hfSelectedPlan.Value = ddlPlanChoice.SelectedValue;

            // Non-blocking: make sure webhook exists (if key present)
            await EnsurePayMongoWebhookAsync();

            if (!IsPostBack)
            {
                int clientId = Convert.ToInt32(Session["ClientID"], CultureInfo.InvariantCulture);

                LoadClientInfo(clientId, hfSelectedPlan.Value);
                LoadPaymentHistory(clientId);

                // Handle PayMongo redirect messages
                var qs = Request.QueryString;
                if (qs["success"] == "1")
                {
                    ScriptManager.RegisterStartupScript(
                        this, GetType(), "pmOk",
                        "Swal.fire('Payment completed!', 'Thanks for your payment. Your balance has been updated.', 'success');",
                        true);
                }
                else if (qs["failed"] == "1")
                {
                    ScriptManager.RegisterStartupScript(
                        this, GetType(), "pmFail",
                        "Swal.fire('Payment not completed', 'You may try again or use a different method.', 'info');",
                        true);
                }

                // Initial render of PayPal + check PayMongo button
                ScriptManager.RegisterStartupScript(this, GetType(), "renderPPInit",
                    "renderPayPalButtons(); updatePayMongoButton();", true);
            }
        }

        // Re-apply KPI values late so a master page DataBind can't clear them
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            lblNextInstallment.Text = _kpiNext;
            lblTotalPrice.Text = _kpiTotal;
            lblAlreadyPaid.Text = _kpiPaid;
            lblRemaining.Text = _kpiRemain;
        }

        // ===== Events ==================================================================
        protected void ddlPlanChoice_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Session["ClientID"] == null) return;

            int clientId = Convert.ToInt32(Session["ClientID"], CultureInfo.InvariantCulture);
            string selectedPlan = ddlPlanChoice.SelectedValue;
            hfSelectedPlan.Value = selectedPlan;

            int bookingId = GetLatestAssignedBookingId(clientId);
            if (bookingId > 0)
                SaveSelectedPlanToDb(bookingId, selectedPlan);

            LoadClientInfo(clientId, selectedPlan);
            LoadPaymentHistory(clientId);

            // Re-render PayPal + re-check PayMongo link after partial update
            ScriptManager.RegisterStartupScript(this, GetType(), "rerenderPP",
                "renderPayPalButtons(); updatePayMongoButton();", true);
        }

        // ===== Data Loaders =============================================================
        private int GetLatestAssignedBookingId(int clientId)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.usp_Booking_GetLatestAssignedIdByClient", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                con.Open();
                object o = cmd.ExecuteScalar();
                return (o == null || o == DBNull.Value) ? 0 : Convert.ToInt32(o, CultureInfo.InvariantCulture);
            }
        }

        private void SaveSelectedPlanToDb(int bookingId, string plan)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.usp_Booking_UpdatePaymentPlan", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingId;
                cmd.Parameters.Add("@PaymentPlan", SqlDbType.NVarChar, 20).Value = (object)plan ?? DBNull.Value;
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void LoadClientInfo(int clientId, string selectedPlan)
        {
            // Reset UI & KPI defaults
            lblServiceName.Text = "";
            lblPaymentPlan.Text = "";
            lblPrice.Text = "";
            hiddenCheckoutURL.Value = "";
            hiddenReference.Value = "";

            _kpiNext = _kpiTotal = _kpiPaid = _kpiRemain = "₱0.00";
            lblNextInstallment.Text = lblTotalPrice.Text = lblAlreadyPaid.Text = lblRemaining.Text = "₱0.00";

            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.usp_Payment_ClientLatestAssignedInfo", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;

                con.Open();
                using (var reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!reader.Read())
                    {
                        lblPrice.Text = "No approved booking found.";
                        hfPayPalAmount.Value = "0.00";
                        hfPayPalBookingID.Value = "0";
                        hfPayPalClientID.Value = clientId.ToString(CultureInfo.InvariantCulture);
                        hiddenCheckoutURL.Value = string.Empty;
                        hiddenReference.Value = string.Empty;
                        return;
                    }

                    string serviceName = reader["ServiceName"]?.ToString() ?? string.Empty;
                    decimal fullPrice = reader["Price"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["Price"], CultureInfo.InvariantCulture);
                    string dbPlan = reader["PaymentPlan"] == DBNull.Value ? "" : reader["PaymentPlan"].ToString();
                    int bookingId = Convert.ToInt32(reader["BookingID"], CultureInfo.InvariantCulture);
                    bool isContract = reader["IsContract"] != DBNull.Value && Convert.ToBoolean(reader["IsContract"], CultureInfo.InvariantCulture);

                    lblServiceName.Text = serviceName;

                    // Decide effective plan (UI-selected > DB-stored > default)
                    string effectivePlan = !string.IsNullOrWhiteSpace(selectedPlan) ? selectedPlan : dbPlan;
                    if (string.IsNullOrWhiteSpace(effectivePlan))
                        effectivePlan = isContract ? "50-25-25" : "100";

                    if (ddlPlanChoice.Items.FindByValue(effectivePlan) != null)
                        ddlPlanChoice.SelectedValue = effectivePlan;

                    decimal totalPaid = GetTotalPaid(bookingId);
                    decimal nextAmount = CalculateNextInstallment(isContract, fullPrice, totalPaid, effectivePlan);
                    decimal remaining = Math.Max(0m, fullPrice - totalPaid);

                    lblPaymentPlan.Text = isContract
                        ? "Installment Plan (" + effectivePlan.Replace("-", "/") + ")"
                        : "One-time Pay (100%)";

                    // --- Update KPI fields + labels ---
                    _kpiNext = "₱" + nextAmount.ToString("N2");
                    _kpiTotal = "₱" + fullPrice.ToString("N2");
                    _kpiPaid = "₱" + totalPaid.ToString("N2");
                    _kpiRemain = "₱" + remaining.ToString("N2");

                    lblNextInstallment.Text = _kpiNext;
                    lblTotalPrice.Text = _kpiTotal;
                    lblAlreadyPaid.Text = _kpiPaid;
                    lblRemaining.Text = _kpiRemain;

                    if (remaining <= 0m)
                    {
                        // Fully paid state
                        lblPrice.Text = "Fully Paid";
                        hfPayPalAmount.Value = "0.00";
                        hfPayPalBookingID.Value = bookingId.ToString(CultureInfo.InvariantCulture);
                        hfPayPalClientID.Value = clientId.ToString(CultureInfo.InvariantCulture);

                        // Clear PayMongo link/refs so button disables
                        hiddenCheckoutURL.Value = string.Empty;
                        hiddenReference.Value = string.Empty;

                        // KPIs for fully-paid
                        _kpiNext = _kpiRemain = "₱0.00";
                        lblNextInstallment.Text = _kpiNext;
                        lblRemaining.Text = _kpiRemain;
                        return;
                    }

                    // Legacy combined label (renders with line breaks via .preline CSS)
                    lblPrice.Text =
                        "₱" + nextAmount.ToString("N2") + " (Next installment)\n" +
                        "Total Price: ₱" + fullPrice.ToString("N2") + "\n" +
                        "Already Paid: ₱" + totalPaid.ToString("N2") + "\n" +
                        "Remaining Balance: ₱" + remaining.ToString("N2");

                    // Hidden fields for PayPal & JS
                    hfPayPalBookingID.Value = bookingId.ToString(CultureInfo.InvariantCulture);
                    hfPayPalAmount.Value = nextAmount.ToString("0.00", CultureInfo.InvariantCulture);
                    hfPayPalClientID.Value = clientId.ToString(CultureInfo.InvariantCulture);

                    // Optional: enqueue a payment reminder (best-effort)
                    if (nextAmount > 0m)
                    {
                        try
                        {
                            using (var con2 = new SqlConnection(connectionString))
                            using (var cmd2 = new SqlCommand("dbo.usp_Notifications_Add", con2))
                            {
                                cmd2.CommandType = CommandType.StoredProcedure;
                                cmd2.Parameters.AddWithValue("@ClientID", clientId);
                                cmd2.Parameters.AddWithValue("@Type", "payment");
                                cmd2.Parameters.AddWithValue("@Title", "Payment Due");
                                cmd2.Parameters.AddWithValue("@Body",
                                    "Next installment: " + _kpiNext + ". Remaining: " + _kpiRemain + ".");
                                cmd2.Parameters.AddWithValue("@Url", "Payment.aspx");
                                cmd2.Parameters.AddWithValue("@DedupKey", "PAY-" + bookingId + "-" + effectivePlan);
                                con2.Open();
                                cmd2.ExecuteNonQuery();
                            }
                        }
                        catch { /* swallow non-blocking notification errors */ }
                    }

                    // Generate/refresh PayMongo Checkout URL (async fire-and-forget)
                    GenerateCheckoutURL(clientId, bookingId, isContract, fullPrice, totalPaid, effectivePlan);
                }
            }
        }

        private decimal GetTotalPaid(int bookingId)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.usp_Transactions_TotalPaidByBooking", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingId;
                con.Open();
                object o = cmd.ExecuteScalar();
                return (o == null || o == DBNull.Value) ? 0m : Convert.ToDecimal(o, CultureInfo.InvariantCulture);
            }
        }

        // Helper used by the Receipt TemplateField
        protected string GetReceiptLink(object receiptObj)
        {
            // Accepts NULL/DBNull/""
            var v = (receiptObj == null || receiptObj == DBNull.Value) ? "" : receiptObj.ToString();
            if (string.IsNullOrWhiteSpace(v)) return "No Receipt";

            // Only pass the file name to your decrypt page
            var file = System.IO.Path.GetFileName(v);
            var url = "DecryptReceipt.aspx?file=" + HttpUtility.UrlEncode(file);
            return $"<a href='{url}' target='_blank' rel='noopener'>View</a>";
        }

        // Optional: if your proc sometimes returns date as string, normalize here
        protected void gvPaymentHistory_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            // If the bound date arrived as string and not DateTime, format it
            // (Only needed if you still see unformatted dates)
            // Example:
            // var dateText = DataBinder.Eval(e.Row.DataItem, "TransactionDate") as string;
            // if (!string.IsNullOrEmpty(dateText) && DateTime.TryParse(dateText, out var dt))
            //     e.Row.Cells[0].Text = dt.ToString("yyyy-MM-dd");
        }

        private void LoadPaymentHistory(int clientId)
        {
            try
            {
                using (var con = new SqlConnection(connectionString))
                using (var da = new SqlDataAdapter("dbo.usp_Transactions_ListByClient", con))
                {
                    da.SelectCommand.CommandType = CommandType.StoredProcedure;
                    da.SelectCommand.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;

                    var dt = new DataTable();
                    da.Fill(dt);

                    gvPaymentHistory.DataSource = dt;
                    gvPaymentHistory.DataBind();
                }
            }
            catch (Exception ex)
            {
                // Show the error visibly on the page so it doesn't fail silently
                lblMessage.CssClass = "text-danger fw-semibold d-block mt-2";
                lblMessage.Text = "Payment history error: " + ex.Message;
            }
        }


        // ===== Business Logic ===========================================================
        private decimal CalculateNextInstallment(bool isContract, decimal fullPrice, decimal totalPaid, string selectedPlan)
        {
            if (!isContract)
                return Math.Round(fullPrice - totalPaid, 2, MidpointRounding.AwayFromZero);

            switch (selectedPlan)
            {
                case "50-25-25":
                    if (totalPaid == 0m) return Math.Round(fullPrice * 0.50m, 2, MidpointRounding.AwayFromZero);
                    if (totalPaid < Math.Round(fullPrice * 0.75m, 2, MidpointRounding.AwayFromZero))
                        return Math.Round(fullPrice * 0.25m, 2, MidpointRounding.AwayFromZero);
                    return Math.Round(fullPrice - totalPaid, 2, MidpointRounding.AwayFromZero);

                case "70-30":
                    return (totalPaid == 0m)
                        ? Math.Round(fullPrice * 0.70m, 2, MidpointRounding.AwayFromZero)
                        : Math.Round(fullPrice - totalPaid, 2, MidpointRounding.AwayFromZero);

                case "100":
                default:
                    return Math.Round(fullPrice - totalPaid, 2, MidpointRounding.AwayFromZero);
            }
        }

        // ===== PayMongo Helpers =========================================================
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
                string referenceNumber = "RRC-" + clientId + "-" + DateTime.UtcNow.Ticks;

                // Save RAW reference (no prefix) so webhook can find it 1:1
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
                                success = GetHttpsBaseUrl() + "/Payment.aspx?success=1&ref=" + HttpUtility.UrlEncode(referenceNumber),
                                failed = GetHttpsBaseUrl() + "/Payment.aspx?failed=1"
                            }
                        }
                    }
                };

                string json = JsonConvert.SerializeObject(payload);

                using (var client = new HttpClient())
                {
                    var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes(payMongoSecretKey + ":"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

                    using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
                    {
                        var resp = await client.PostAsync("https://api.paymongo.com/v1/checkout_sessions", content);
                        string res = await resp.Content.ReadAsStringAsync();

                        if (resp.IsSuccessStatusCode)
                        {
                            dynamic doc = JsonConvert.DeserializeObject(res);
                            string checkoutUrl = doc?.data?.attributes?.checkout_url;
                            hiddenCheckoutURL.Value = checkoutUrl ?? string.Empty;
                        }
                        else
                        {
                            hiddenCheckoutURL.Value = string.Empty;
                            lblMessage.Text = "❌ PayMongo Error: " + res;
                        }
                    }
                }

                // after creating link, update UI buttons
                ScriptManager.RegisterStartupScript(this, GetType(), "pmUpdateBtn", "updatePayMongoButton();", true);
            }
            catch (Exception ex)
            {
                hiddenCheckoutURL.Value = string.Empty;
                lblMessage.Text = "❌ Exception: " + ex.Message;
            }
        }

        private string GetServiceNameForBooking(int bookingId)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.usp_Booking_GetServiceName", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingId;
                con.Open();
                object o = cmd.ExecuteScalar();
                return (o == null || o == DBNull.Value) ? null : o.ToString();
            }
        }

        private void SavePayMongoReference(int bookingId, string referenceNumber)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.usp_Booking_SavePayMongoRef", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingId;
                // Save RAW reference only
                cmd.Parameters.Add("@Reference", SqlDbType.NVarChar, 200).Value = referenceNumber;
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private string GetHttpsBaseUrl()
        {
            string left = Request.Url.GetLeftPart(UriPartial.Authority);
            if (left.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                left = "https://" + left.Substring("http://".Length);
            return left;
        }

        private async Task EnsurePayMongoWebhookAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(payMongoSecretKey)) return;

                string webhookUrl = GetHttpsBaseUrl().TrimEnd('/') + "/PayMongoWebhook.ashx";

                using (var http = new HttpClient())
                {
                    var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(payMongoSecretKey + ":"));
                    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);

                    // 1) list existing
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

                    // 2) create if not found
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
                    await http.PostAsync(
                        "https://api.paymongo.com/v1/webhooks",
                        new StringContent(json, Encoding.UTF8, "application/json"));
                }
            }
            catch
            {
                // non-blocking: ignore failures
            }
        }

        // ===== No-op handlers (buttons are handled by JS) ===============================
        protected void btnPayWithPayPal_Click(object sender, EventArgs e) { }
        protected void btnConfirmPayment_Click(object sender, EventArgs e) { }
    }
}
