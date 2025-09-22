using System;
using System.Collections.Generic;
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
                Response.Redirect("~/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        protected async void Page_Load(object sender, EventArgs e)
        {
            if (Session["ClientID"] == null) return;

            // Keep hidden field synced with dropdown
            hfSelectedPlan.Value = ddlPlanChoice.SelectedValue;

            // Non-blocking: ensure webhook exists (if key present)
            await EnsurePayMongoWebhookAsync();

            if (!IsPostBack)
            {
                int clientId = Convert.ToInt32(Session["ClientID"], CultureInfo.InvariantCulture);

                LoadClientInfo(clientId, hfSelectedPlan.Value);
                LoadPaymentHistory(clientId);

                // Handle PayMongo redirect messages (from success_url/cancel_url)
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


        // --- DUE DATE HELPERS --------------------------------------------------------
        private DateTime GetBookingApprovalDate(int bookingId)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("SELECT CreatedAt FROM Bookings WHERE BookingID=@B", con))
            {
                cmd.Parameters.Add("@B", SqlDbType.Int).Value = bookingId;
                con.Open();
                var o = cmd.ExecuteScalar();
                if (o != null && o != DBNull.Value)
                {
                    var dt = Convert.ToDateTime(o, CultureInfo.InvariantCulture);
                    return dt.Date;
                }
            }
            return DateTime.UtcNow.Date; // fallback if no date
        }


        private List<DateTime> GetSuccessfulTransactionDatesBySale(int saleId)
        {
            var list = new List<DateTime>();
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(
                "SELECT TransactionDate FROM Transactions WHERE SaleID=@S AND Status='Completed' ORDER BY TransactionDate ASC", con))
            {
                cmd.Parameters.Add("@S", SqlDbType.Int).Value = saleId;
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        if (r["TransactionDate"] != DBNull.Value)
                            list.Add(Convert.ToDateTime(r["TransactionDate"], CultureInfo.InvariantCulture).Date);
                    }
                }
            }
            return list;
        }

        private DateTime ComputeNextDueDate(string plan, DateTime approvalDate, List<DateTime> paidDates)
        {
            // Rule:
            //  - 50-25-25: 50% due +3 days from approval; 2nd 25% due +1 month from 1st txn; last 25% due +1 month from 2nd txn
            //  - 70-30   : 70% due +3 days from approval; 30% due +1 month from 1st txn
            //  - 100     : one-time due +3 days from approval
            plan = (plan ?? "").Trim();
            if (string.Equals(plan, "70-30", StringComparison.OrdinalIgnoreCase))
            {
                if (paidDates.Count == 0) return approvalDate.AddDays(3);
                if (paidDates.Count == 1) return paidDates[0].AddMonths(1);
                return DateTime.MaxValue; // all done
            }
            if (string.Equals(plan, "50-25-25", StringComparison.OrdinalIgnoreCase))
            {
                if (paidDates.Count == 0) return approvalDate.AddDays(3);
                if (paidDates.Count == 1) return paidDates[0].AddMonths(1);
                if (paidDates.Count == 2) return paidDates[1].AddMonths(1);
                return DateTime.MaxValue; // all done
            }
            // default (100%):
            return approvalDate.AddDays(3);
        }

        private string FormatDueLabel(DateTime dueDateUtcOrLocal)
        {
            if (dueDateUtcOrLocal == DateTime.MaxValue) return "All installments paid";

            // Use local (PH) calendar feel; compare by date
            var today = DateTime.UtcNow.Date;
            var dd = (dueDateUtcOrLocal.Date - today).Days;

            if (dd <= 0)
            {
                // If it’s past due
                if (dd < 0) return $"Overdue by {Math.Abs(dd)} day(s)";
                return "Due now";
            }
            if (dd == 1) return "Due tomorrow";
            return $"Due in {dd} days";
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

            // 🔔 After saving plan, also generate installments
            GenerateInstallments(bookingId, plan);
        }

        private void GenerateInstallments(int bookingId, string plan)
        {
            int clientId = Convert.ToInt32(Session["ClientID"], CultureInfo.InvariantCulture);
            decimal fullPrice = 0m;

            // get full price from DB
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("SELECT Price FROM Bookings WHERE BookingID=@B", con))
            {
                cmd.Parameters.Add("@B", SqlDbType.Int).Value = bookingId;
                con.Open();
                object o = cmd.ExecuteScalar();
                if (o != null && o != DBNull.Value)
                    fullPrice = Convert.ToDecimal(o, CultureInfo.InvariantCulture);
            }

            // get booking approval date (or use today if not tracked)
            DateTime startDate = DateTime.UtcNow;

            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.usp_GenerateInstallments", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingId;
                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                cmd.Parameters.Add("@FullPrice", SqlDbType.Decimal).Value = fullPrice;
                cmd.Parameters.Add("@Plan", SqlDbType.NVarChar, 20).Value = plan;
                cmd.Parameters.Add("@StartDate", SqlDbType.Date).Value = startDate.Date;
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }


        private void LoadClientInfo(int clientId, string selectedPlan)
        {
            // Reset UI & KPI defaults
            lblServiceName.Text = "";
            lblPaymentPlan.Text = "";
            hiddenCheckoutURL.Value = "";
            hiddenReference.Value = "";
            litNextDue.Text = "—";

            _kpiNext = _kpiTotal = _kpiPaid = _kpiRemain = "₱0.00";
            lblNextInstallment.Text = lblTotalPrice.Text = lblAlreadyPaid.Text = lblRemaining.Text = "₱0.00";

            // Reset breakdown labels
            lblBasePrice.Text = "₱0.00";
            lblTravelExpense.Text = "₱0.00";
            lblMiscellaneous.Text = "₱0.00";
            Label1.Text = "₱0.00"; // Total Price

            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.usp_Payment_ClientLatestAssignedInfo", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;

                con.Open();
                using (var reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    // ---------------- NO BOOKING FOUND ----------------
                    if (!reader.Read())
                    {
                        lblMessage.Text = "No approved booking found.";
                        lblMessage.CssClass = "text-gray-600";

                        hfPayPalAmount.Value = "0.00";
                        hfPayPalBookingID.Value = "0";
                        hfPayPalClientID.Value = clientId.ToString(CultureInfo.InvariantCulture);
                        hiddenCheckoutURL.Value = string.Empty;
                        hiddenReference.Value = string.Empty;

                        hfMinRequired.Value = "0.00";

                        paymentPlanContainer.Visible = false;
                        ddlPlanChoice.Enabled = true;
                        return;
                    }

                    // ---------------- READ BOOKING INFO ----------------
                    string serviceName = reader["ServiceName"]?.ToString() ?? "";
                    decimal fullPrice = reader["Price"] == DBNull.Value ? 0m :
                        Convert.ToDecimal(reader["Price"], CultureInfo.InvariantCulture);

                    // TravelExpense and Miscellaneous (safe defaults)
                    decimal travelExpense = reader["TravelExpense"] == DBNull.Value ? 0m :
                        Convert.ToDecimal(reader["TravelExpense"], CultureInfo.InvariantCulture);

                    decimal miscellaneous = reader["Miscellaneous"] == DBNull.Value ? 0m :
                        Convert.ToDecimal(reader["Miscellaneous"], CultureInfo.InvariantCulture);

                    string dbPlanRaw = reader["PaymentPlan"]?.ToString() ?? "";
                    int bookingId = Convert.ToInt32(reader["BookingID"], CultureInfo.InvariantCulture);

                    bool isContractDb = reader["IsContract"] != DBNull.Value &&
                                        Convert.ToBoolean(reader["IsContract"], CultureInfo.InvariantCulture);

                    // Normalize plan
                    string NormalizePlan(string p)
                    {
                        if (string.IsNullOrWhiteSpace(p)) return "";
                        p = p.Trim();
                        return string.Equals(p, "100%", StringComparison.OrdinalIgnoreCase) ? "100" : p;
                    }

                    string dbPlan = NormalizePlan(dbPlanRaw);
                    string uiPlan = NormalizePlan(selectedPlan);

                    lblServiceName.Text = serviceName;

                    // Determine effective plan: UI > DB > Default
                    string effectivePlan = !string.IsNullOrWhiteSpace(uiPlan) ? uiPlan : dbPlan;
                    if (string.IsNullOrWhiteSpace(effectivePlan))
                        effectivePlan = isContractDb ? "50-25-25" : "100";

                    bool isContract = isContractDb || !string.Equals(effectivePlan, "100", StringComparison.OrdinalIgnoreCase);

                    // Persist plan to DB if missing
                    if (string.IsNullOrWhiteSpace(dbPlan))
                        SaveSelectedPlanToDb(bookingId, effectivePlan);

                    // Sync dropdown + hidden field
                    var item = ddlPlanChoice.Items.FindByValue(effectivePlan);
                    if (item != null) ddlPlanChoice.SelectedValue = effectivePlan;
                    hfSelectedPlan.Value = effectivePlan;

                    // Show/hide plan selector for contract
                    paymentPlanContainer.Visible = isContract;

                    // ---------------- CALCULATE TOTALS ----------------
                    int saleId = GetSaleIdByBooking(bookingId);
                    decimal totalPaid = saleId > 0 ? GetTotalPaidBySaleId(saleId) : 0m;

                    // Lock plan after first payment
                    ddlPlanChoice.Enabled = (totalPaid == 0m);

                    // Next installment and remaining balance
                    decimal nextAmount = CalculateNextInstallment(isContract, fullPrice, totalPaid, effectivePlan);
                    decimal remaining = Math.Max(0m, fullPrice - totalPaid);

                    lblPaymentPlan.Text = isContract
                        ? "Installment Plan (" + effectivePlan.Replace("-", "/") + ")"
                        : "One-time Pay (100%)";

                    _kpiNext = "₱" + nextAmount.ToString("N2");
                    _kpiTotal = "₱" + fullPrice.ToString("N2");
                    _kpiPaid = "₱" + totalPaid.ToString("N2");
                    _kpiRemain = "₱" + remaining.ToString("N2");

                    lblNextInstallment.Text = _kpiNext;
                    lblTotalPrice.Text = _kpiTotal;
                    lblAlreadyPaid.Text = _kpiPaid;
                    lblRemaining.Text = _kpiRemain;

                    // ---------------- DUE DATE CALCULATION ----------------
                    var paidDates = saleId > 0 ? GetSuccessfulTransactionDatesBySale(saleId) : new List<DateTime>();
                    var approvalDate = GetBookingApprovalDate(bookingId);
                    var nextDue = ComputeNextDueDate(effectivePlan, approvalDate, paidDates);

                    if (remaining <= 0m)
                    {
                        litNextDue.Text = "All installments paid";

                        hfPayPalAmount.Value = "0.00";
                        hfPayPalBookingID.Value = bookingId.ToString(CultureInfo.InvariantCulture);
                        hfPayPalClientID.Value = clientId.ToString(CultureInfo.InvariantCulture);
                        hfMinRequired.Value = "0.00";

                        hiddenCheckoutURL.Value = string.Empty;
                        hiddenReference.Value = string.Empty;

                        _kpiNext = _kpiRemain = "₱0.00";
                        lblNextInstallment.Text = _kpiNext;
                        lblRemaining.Text = _kpiRemain;

                        ddlPlanChoice.Enabled = false;
                        return;
                    }
                    else
                    {
                        litNextDue.Text = FormatDueLabel(nextDue);
                    }

                    // ---------------- PRICING BREAKDOWN ----------------
                    decimal baseServicePrice = fullPrice - (travelExpense + miscellaneous);
                    if (baseServicePrice < 0) baseServicePrice = 0; // Prevent negative

                    lblBasePrice.Text = $"₱{baseServicePrice:N2}";
                    lblTravelExpense.Text = $"₱{travelExpense:N2}";
                    lblMiscellaneous.Text = $"₱{miscellaneous:N2}";
                    Label1.Text = $"₱{fullPrice:N2}"; // Total Price

                    // ---------------- PAYPAL CONFIGURATION ----------------
                    hfPayPalBookingID.Value = bookingId.ToString(CultureInfo.InvariantCulture);

                    // By default, the minimum required amount is the next installment
                    hfPayPalAmount.Value = nextAmount.ToString("0.00", CultureInfo.InvariantCulture);
                    hfMinRequired.Value = nextAmount.ToString("0.00", CultureInfo.InvariantCulture);

                    hfPayPalClientID.Value = clientId.ToString(CultureInfo.InvariantCulture);

                    // Update the front-end label dynamically for minimum required
                    ScriptManager.RegisterStartupScript(this, GetType(), "updateMinReq",
                        $"document.getElementById('minRequiredAmount').innerText = '₱{nextAmount:N2}';", true);

                    // ---------------- NOTIFICATION ----------------
                    EnqueuePaymentDueNotification(
                        clientId: clientId,
                        bookingId: bookingId,
                        isContract: isContract,
                        plan: effectivePlan,
                        fullPrice: fullPrice,
                        totalPaid: totalPaid,
                        nextAmount: nextAmount
                    );

                    // ---------------- PAYMONGO CHECKOUT URL ----------------
                    // Grab custom amount from the TextBox if provided
                    decimal customAmountFromTextbox = 0m;
                    if (!string.IsNullOrWhiteSpace(txtCustomAmount.Text))
                    {
                        decimal.TryParse(txtCustomAmount.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out customAmountFromTextbox);
                    }

                    // Pass the custom amount to GenerateCheckoutURL
                    GenerateCheckoutURL(clientId, bookingId, isContract, fullPrice, totalPaid, effectivePlan, customAmountFromTextbox);
                }
            }
        }




        // ======= Canonical total via SaleID =============================================
        private int GetSaleIdByBooking(int bookingId)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(
                "SELECT TOP (1) SaleID FROM dbo.Sales WHERE BookingID=@B", con))
            {
                cmd.Parameters.Add("@B", SqlDbType.Int).Value = bookingId;
                con.Open();
                var o = cmd.ExecuteScalar();
                return (o == null || o == DBNull.Value) ? 0 : Convert.ToInt32(o, CultureInfo.InvariantCulture);
            }
        }

        private decimal GetTotalPaidBySaleId(int saleId)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.usp_TotalPaidBySale", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@SaleID", SqlDbType.Int).Value = saleId;
                con.Open();
                var o = cmd.ExecuteScalar();
                return (o == null || o == DBNull.Value) ? 0m :
                       Convert.ToDecimal(o, CultureInfo.InvariantCulture);
            }
        }

        private void EnqueuePaymentDueNotification(
            int clientId, int bookingId,
            bool isContract, string plan,
            decimal fullPrice, decimal totalPaid, decimal nextAmount)
        {
            if (nextAmount <= 0m) return;

            string stageKey;
            string stageLabel;

            if (!isContract || plan == "100")
            {
                stageKey = $"BOOK-{bookingId}-ONE-TIME";
                stageLabel = "Payment Due";
            }
            else if (string.Equals(plan, "50-25-25", StringComparison.OrdinalIgnoreCase))
            {
                var seventyFive = Math.Round(fullPrice * 0.75m, 2, MidpointRounding.AwayFromZero);

                if (totalPaid <= 0m)
                {
                    stageKey = $"BOOK-{bookingId}-STAGE-1-50";
                    stageLabel = "1st Installment (50%) Due";
                }
                else if (totalPaid < seventyFive)
                {
                    stageKey = $"BOOK-{bookingId}-STAGE-2-25";
                    stageLabel = "2nd Installment (25%) Due";
                }
                else
                {
                    stageKey = $"BOOK-{bookingId}-STAGE-3-25";
                    stageLabel = "Final Installment (25%) Due";
                }
            }
            else if (string.Equals(plan, "70-30", StringComparison.OrdinalIgnoreCase))
            {
                if (totalPaid <= 0m)
                {
                    stageKey = $"BOOK-{bookingId}-STAGE-1-70";
                    stageLabel = "1st Installment (70%) Due";
                }
                else
                {
                    stageKey = $"BOOK-{bookingId}-STAGE-2-30";
                    stageLabel = "Final Installment (30%) Due";
                }
            }
            else
            {
                stageKey = $"BOOK-{bookingId}-ONE-TIME";
                stageLabel = "Payment Due";
            }

            var ph = new CultureInfo("en-PH");
            string nextTxt = string.Format(ph, "{0:C}", nextAmount);
            decimal remaining = Math.Max(0m, fullPrice - totalPaid);
            string remainTxt = string.Format(ph, "{0:C}", remaining);

            try
            {
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.usp_Notifications_Add", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ClientID", clientId);
                    cmd.Parameters.AddWithValue("@Type", "payment");
                    cmd.Parameters.AddWithValue("@Title", stageLabel);
                    cmd.Parameters.AddWithValue("@Body", $"Next installment: {nextTxt}. Remaining: {remainTxt}.");
                    cmd.Parameters.AddWithValue("@Url", "Payment.aspx");
                    cmd.Parameters.AddWithValue("@DedupKey", stageKey);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch { /* best-effort */ }
        }

        // ===== History =================================================================
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

                    if (!dt.Columns.Contains("Receipt"))
                        dt.Columns.Add("Receipt", typeof(string));

                    gvPaymentHistory.DataSource = dt;
                    gvPaymentHistory.DataBind();
                }
            }
            catch (Exception ex)
            {
                lblMessage.CssClass = "text-danger fw-semibold d-block mt-2";
                lblMessage.Text = "Payment history error: " + ex.Message;
            }
        }

        // Helper used by the Receipt TemplateField
        protected string GetReceiptLink(object receiptObj)
        {
            var v = (receiptObj == null || receiptObj == DBNull.Value) ? "" : receiptObj.ToString();
            if (string.IsNullOrWhiteSpace(v)) return "No Receipt";
            var file = System.IO.Path.GetFileName(v);
            var url = "DecryptReceipt.aspx?file=" + HttpUtility.UrlEncode(file);
            return $"<a href='{url}' target='_blank' rel='noopener'>View</a>";
        }

        protected void gvPaymentHistory_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;
        }

        // ===== Business Logic ===========================================================
        private decimal CalculateNextInstallment(bool isContract, decimal fullPrice, decimal totalPaid, string selectedPlan)
        {
            if (!isContract)
                return Math.Round(fullPrice - totalPaid, 2, MidpointRounding.AwayFromZero);

            switch (selectedPlan)
            {
                case "50-25-25":
                    decimal firstThreshold = Math.Round(fullPrice * 0.50m, 2, MidpointRounding.AwayFromZero); // 50% stage
                    decimal secondThreshold = Math.Round(fullPrice * 0.75m, 2, MidpointRounding.AwayFromZero); // 75% stage

                    // If nothing has been paid yet, first installment is 50%
                    if (totalPaid == 0m)
                        return firstThreshold;

                    // If total paid is between first and second threshold
                    if (totalPaid < secondThreshold)
                    {
                        // Calculate how much beyond the first payment the client has already paid
                        decimal alreadyPaidBeyondFirst = totalPaid - firstThreshold;

                        // Standard second payment is 25%
                        decimal requiredSecondPayment = Math.Round(fullPrice * 0.25m, 2, MidpointRounding.AwayFromZero);

                        // Deduct extra paid amount from the second required payment
                        decimal remainingSecondPayment = requiredSecondPayment - alreadyPaidBeyondFirst;

                        // Make sure it never goes negative
                        return remainingSecondPayment < 0 ? 0 : remainingSecondPayment;
                    }

                    // Final stage: total remaining balance
                    return Math.Round(fullPrice - totalPaid, 2, MidpointRounding.AwayFromZero);

                case "70-30":
                    decimal firstPayment70 = Math.Round(fullPrice * 0.70m, 2, MidpointRounding.AwayFromZero);

                    if (totalPaid == 0m)
                        return firstPayment70;

                    // Second payment is just whatever remains
                    return Math.Round(fullPrice - totalPaid, 2, MidpointRounding.AwayFromZero);

                case "100":
                default:
                    // One-time full payment plan
                    return Math.Round(fullPrice - totalPaid, 2, MidpointRounding.AwayFromZero);
            }
        }


        // ===== PayMongo Helpers =========================================================
        private async void GenerateCheckoutURL(
      int clientId,
      int bookingId,
      bool isContract,
      decimal fullPrice,
      decimal totalPaid,
      string selectedPlan,
      decimal? customAmountOverride = null) // <-- Added this
        {
            try
            {
                // 1️⃣ Calculate the default system-required amount (next installment)
                decimal defaultAmount = CalculateNextInstallment(isContract, fullPrice, totalPaid, selectedPlan);

                // 2️⃣ Determine the custom amount:
                decimal finalCustomAmount = 0m;

                // If a customAmountOverride was passed, use it first
                if (customAmountOverride.HasValue && customAmountOverride.Value > 0)
                {
                    finalCustomAmount = customAmountOverride.Value;
                }
                else if (!string.IsNullOrWhiteSpace(txtCustomAmount.Text))
                {
                    // Fallback: read directly from textbox
                    decimal.TryParse(txtCustomAmount.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out finalCustomAmount);
                }

           
                // Always prefer custom amount if greater than 0, otherwise default
                decimal amountPhp = finalCustomAmount > 0 ? finalCustomAmount : defaultAmount;


                // 4️⃣ If the final amount is zero or less, no need to create a checkout session
                if (amountPhp <= 0m)
                {
                    hiddenCheckoutURL.Value = string.Empty;
                    hiddenReference.Value = string.Empty;
                    lblMessage.Text += " | Skipped checkout: Amount is zero or negative.";
                    return;
                }

                // 5️⃣ Build the PayMongo Checkout session
                string serviceName = GetServiceNameForBooking(bookingId) ?? "RRC Service Payment";

                // RAW reference for webhook mapping
                string referenceNumber = "RRC-" + clientId + "-" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
                SavePayMongoReference(bookingId, referenceNumber);
                hiddenReference.Value = referenceNumber;

                // Redirect back to THIS PAGE
                string baseUrl = GetHttpsBaseUrl().TrimEnd('/');
                string successUrl = $"{baseUrl}/Payment.aspx?success=1&ref={HttpUtility.UrlEncode(referenceNumber)}";
                string cancelUrl = $"{baseUrl}/Payment.aspx?failed=1";

                // Convert PHP to centavos
                long amountCentavos = (long)Math.Round(amountPhp * 100m, MidpointRounding.AwayFromZero);

                // ✅ Checkout Session payload
                var payload = new
                {
                    data = new
                    {
                        attributes = new
                        {
                            currency = "PHP",
                            description = serviceName,
                            payment_method_types = new[] { "gcash", "card", "grab_pay", "paymaya" },
                            reference_number = referenceNumber,
                            success_url = successUrl,
                            cancel_url = cancelUrl,
                            line_items = new[]
                            {
                        new {
                            name = "RRC Service",
                            description = serviceName,
                            amount = amountCentavos,
                            currency = "PHP",
                            quantity = 1
                        }
                    },
                            send_email_receipt = false,
                            show_line_items = true,
                            show_description = true
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
                            lblMessage.Text += " | ❌ PayMongo Error: " + res;
                        }
                    }
                }

                // Update PayMongo button on UI
                ScriptManager.RegisterStartupScript(this, GetType(), "pmUpdateBtn", "updatePayMongoButton();", true);
            }
            catch (Exception ex)
            {
                hiddenCheckoutURL.Value = string.Empty;
                lblMessage.Text = "❌ Exception: " + ex.Message;
            }
        }

        protected void txtCustomAmount_TextChanged(object sender, EventArgs e)
        {
            if (Session["ClientID"] == null) return;

            int clientId = Convert.ToInt32(Session["ClientID"]);
            int bookingId = GetLatestAssignedBookingId(clientId);

            decimal minRequired = 0m;
            decimal.TryParse(hfMinRequired.Value, out minRequired);

            string rawInput = txtCustomAmount.Text.Trim();
            bool hasValue = !string.IsNullOrWhiteSpace(rawInput);

            decimal enteredAmount = 0m;
            if (hasValue && !decimal.TryParse(rawInput, out enteredAmount))
            {
                lblCustomAmountError.Text = "Please enter a valid number.";
                hiddenCheckoutURL.Value = string.Empty;
                return;
            }

            if (hasValue && enteredAmount < minRequired)
            {
                lblCustomAmountError.Text = $"Amount cannot be less than ₱{minRequired:N2}";
                hiddenCheckoutURL.Value = string.Empty;
                return;
            }

            lblCustomAmountError.Text = "";

            decimal finalAmount = hasValue ? enteredAmount : minRequired;

            // ✅ Reload KPI and hidden fields
            LoadClientInfo(clientId, hfSelectedPlan.Value);

            // ✅ Generate PayMongo checkout link
            GenerateCheckoutURL(clientId, bookingId, true, 0, 0, hfSelectedPlan.Value, finalAmount);

            // ✅ Update PayPal dynamically
            ScriptManager.RegisterStartupScript(this, GetType(), "refreshPayPal",
                "renderPayPalButtons();", true);
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
                cmd.Parameters.Add("@Reference", SqlDbType.NVarChar, 200).Value = referenceNumber; // RAW, no prefixes
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private string GetHttpsBaseUrl()
        {
            // e.g., https://rrcmngmnt.com
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
