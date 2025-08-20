using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web.Services;
using System.Web.Script.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web;
using System.Linq;
using System.Globalization;

namespace RRCManagementSystem
{
    public partial class Dashboard : System.Web.UI.Page
    {
        protected string salesDataJson = "{}";
        private readonly string cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                lblWelcome.Text = "Welcome back, " + (Session["AdminName"]?.ToString() ?? "Admin") + "!";
                LoadTotalCounts();
                LoadBlockchainLog();
                LoadWeeklyBookingCalendar();
            }
        }

        // ==================== SALES CHART (AJAX) ====================
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static object GetSalesData(string type)
        {
            var labels = new List<string>();
            var data = new List<decimal>();

            string cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;
            string mode = (type ?? "").ToLowerInvariant();

            // Map modes: "", "daily", "weekly", "monthly", "yearly"
            if (mode != "weekly" && mode != "monthly" && mode != "yearly" && mode != "daily")
                mode = "daily";

            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.spDashboard_Sales_Aggregate", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Mode", SqlDbType.NVarChar, 10).Value = mode;
                con.Open();

                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        labels.Add(Convert.ToString(r["BucketLabel"]));
                        data.Add(Convert.ToDecimal(r["Total"]));
                    }
                }
            }

            return new { labels, data };
        }

        // ==================== WEEKLY CALENDAR ====================
        private void LoadWeeklyBookingCalendar()
        {
            tblCalendar.Rows.Clear();

            DateTime today = DateTime.Today;
            DateTime sunday = today.AddDays(-(int)today.DayOfWeek);
            DateTime saturday = sunday.AddDays(6);

            var calendarData = new Dictionary<DayOfWeek, List<string>>();
            foreach (DayOfWeek d in Enum.GetValues(typeof(DayOfWeek)))
                calendarData[d] = new List<string>();

            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.spDashboard_WeeklyCalendar", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Sunday", SqlDbType.Date).Value = sunday.Date;
                cmd.Parameters.Add("@Saturday", SqlDbType.Date).Value = saturday.Date;

                con.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime sched = Convert.ToDateTime(reader["ScheduledDate"]);
                        DayOfWeek day = sched.DayOfWeek;

                        string client = $"{reader["LastName"]}, {reader["FirstName"]}";
                        string address = $"{reader["StreetAndUnit"]}, {reader["Barangay"]}, {reader["City"]}";
                        string time = sched.ToString("hh:mm tt");
                        string groupName = reader["GroupName"] == DBNull.Value ? "Unassigned" : reader["GroupName"].ToString();

                        string modalContent = $"{client}<br/>{time}<br/>{address}<br/><strong>Team:</strong> {groupName}".Replace("'", "\\'");
                        string clickableDiv = $@"
<div onclick=""showBookingDetails('{modalContent}')""
     style='cursor:pointer; padding:6px; border-radius:6px; transition:0.2s;'
     onmouseover=""this.style.backgroundColor='#e2e6ea'""
     onmouseout=""this.style.backgroundColor='transparent'"">
    <strong>{client}</strong><br/>
    <small>{time}</small><br/>
    <small>{address}</small><br/>
    <span class='badge bg-info'>{groupName}</span>
</div>";

                        calendarData[day].Add(clickableDiv);
                    }
                }
            }

            var header = new TableHeaderRow();
            for (int i = 0; i < 7; i++)
            {
                DateTime d = sunday.AddDays(i);
                var th = new TableHeaderCell
                {
                    Text = $"<div><strong>{d:dddd}</strong><br/><small>{d:MMM dd, yyyy}</small></div>",
                    CssClass = "align-top"
                };
                header.Cells.Add(th);
            }
            tblCalendar.Rows.Add(header);

            var row = new TableRow();
            for (int i = 0; i < 7; i++)
            {
                DayOfWeek day = (DayOfWeek)i;
                var cell = new TableCell { CssClass = "align-top" };

                if (calendarData[day].Count == 0)
                {
                    cell.Text = "<div class='text-muted' style='padding:6px'><em>No bookings</em></div>";
                }
                else
                {
                    var sb = new StringBuilder();
                    foreach (var entry in calendarData[day])
                    {
                        sb.Append(entry);
                        sb.Append("<hr style='margin:6px 0'/>");
                    }
                    cell.Text = sb.ToString();
                }

                row.Cells.Add(cell);
            }
            tblCalendar.Rows.Add(row);
        }

        // ==================== METRICS (PHT TODAY / THIS MONTH) ====================
        private void LoadTotalCounts()
        {
            // simple totals
            using (var con = new SqlConnection(cs))
            {
                con.Open();

                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Clients", con))
                    lblTotalClients.Text = Convert.ToInt32(cmd.ExecuteScalar()).ToString("N0");

                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Employees", con))
                    lblTotalWorkers.Text = Convert.ToInt32(cmd.ExecuteScalar()).ToString("N0");
            }

            // PHT sales totals via SP (returns two scalars)
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.spDashboard_TotalsPHT", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        decimal today = Convert.ToDecimal(r["TodayTotal"]);
                        decimal month = Convert.ToDecimal(r["MonthTotal"]);
                        lblTodaySales.Text = "₱" + today.ToString("N2");
                        lblMonthSales.Text = "₱" + month.ToString("N2");
                    }
                }
            }
        }

        // ==================== BLOCKCHAIN LOG LIST/FILTER ====================
        private void LoadBlockchainLog()
        {
            DateTime fromDate, toDate;
            bool hasFrom = DateTime.TryParse(txtFromDate.Text, out fromDate);
            bool hasTo = DateTime.TryParse(txtToDate.Text, out toDate);

            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.spBlockchain_Log_List", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@From", SqlDbType.DateTime).Value = hasFrom ? (object)fromDate.Date : DBNull.Value;
                cmd.Parameters.Add("@To", SqlDbType.DateTime).Value = hasTo ? (object)toDate.Date.AddDays(1) : DBNull.Value; // exclusive

                var dt = new DataTable();
                using (var da = new SqlDataAdapter(cmd))
                {
                    con.Open();
                    da.Fill(dt);
                }

                rptBlockchainLog.DataSource = dt;
                rptBlockchainLog.DataBind();
            }
        }

        protected void btnFilterBlockchain_Click(object sender, EventArgs e)
        {
            LoadBlockchainLog();
        }

        // ==================== BLOCKCHAIN VERIFY ====================
        protected void btnVerifyBlockchain_Click(object sender, EventArgs e)
        {
            int checkedCount = 0, tamperedCount = 0;
            bool allValid = true;

            DateTime fromDate, toDate;
            bool hasFrom = DateTime.TryParse(txtFromDate.Text, out fromDate);
            bool hasTo = DateTime.TryParse(txtToDate.Text, out toDate);
            if (hasTo) toDate = toDate.Date.AddDays(1);

            string expectedPrev = new string('0', 64);

            // Optional HMAC key
            byte[] hmacKey = null;
            try
            {
                var keyB64 = ConfigurationManager.AppSettings["BlockchainHmacKey"];
                if (!string.IsNullOrWhiteSpace(keyB64))
                    hmacKey = Convert.FromBase64String(keyB64);
            }
            catch { hmacKey = null; }

            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.spBlockchain_Log_ListForVerify", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@From", SqlDbType.DateTime).Value = hasFrom ? (object)fromDate.Date : DBNull.Value;
                cmd.Parameters.Add("@To", SqlDbType.DateTime).Value = hasTo ? (object)toDate : DBNull.Value; // exclusive

                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    bool isFirst = true;
                    while (r.Read())
                    {
                        checkedCount++;

                        string saleJson = r["SaleDataJson"] as string ?? "";
                        string saleHashDb = r["SaleHash"] as string ?? "";
                        string prevHash = r["PrevHash"] as string ?? "";
                        string chainHash = r["ChainHash"] as string ?? "";
                        int txId = (int)r["TransactionID"];
                        DateTime ts = (DateTime)r["Timestamp"];
                        byte[] tag = r["AuthTag"] as byte[];

                        // JSON hash
                        string recomputedSaleHash = GenerateSHA256Hash(saleJson);
                        bool okSale = saleHashDb.Equals(recomputedSaleHash, StringComparison.OrdinalIgnoreCase);

                        // Chain checks (if present)
                        bool hasChain = !string.IsNullOrWhiteSpace(prevHash) && prevHash.Length == 64
                                     && !string.IsNullOrWhiteSpace(chainHash) && chainHash.Length == 64;

                        bool okPrev = true, okChain = true;

                        if (hasChain)
                        {
                            if (isFirst) expectedPrev = prevHash; // accept starting point for filtered range
                            okPrev = prevHash.Equals(expectedPrev, StringComparison.OrdinalIgnoreCase);

                            string material = $"{prevHash}|{recomputedSaleHash}|{txId}|{ts.ToUniversalTime():O}";
                            string recomputedChain = GenerateSHA256Hash(material);
                            okChain = chainHash.Equals(recomputedChain, StringComparison.OrdinalIgnoreCase);

                            expectedPrev = chainHash;
                        }

                        // HMAC (optional)
                        bool okHmac = true;
                        if (hmacKey != null && tag != null && tag.Length > 0)
                        {
                            string paidAtIso = ts.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
                            string macMaterial = $"{saleJson}|{txId}|{paidAtIso}";
                            byte[] expected;
                            using (var h = new HMACSHA256(hmacKey))
                                expected = h.ComputeHash(Encoding.UTF8.GetBytes(macMaterial));
                            okHmac = tag.SequenceEqual(expected);
                        }

                        if (!(okSale && okPrev && okChain && okHmac))
                        {
                            allValid = false;
                            tamperedCount++;
                        }

                        isFirst = false;
                    }
                }
            }

            if (checkedCount == 0)
            {
                lblVerificationResult.ForeColor = System.Drawing.Color.Gray;
                lblVerificationResult.Text = "ℹ No blockchain records in the selected range.";
            }
            else if (allValid)
            {
                lblVerificationResult.ForeColor = System.Drawing.Color.Green;
                lblVerificationResult.Text = "✅ Verification complete.<br/>Your records match what was originally saved.<br/>The sequence is correct.<br/>No signs of tampering.";
            }
            else
            {
                lblVerificationResult.ForeColor = System.Drawing.Color.Red;
                lblVerificationResult.Text = $"❌ {tamperedCount} of {checkedCount} record(s) appear altered or inconsistent.";
            }

            LogAudit("Performed blockchain verification.");
        }

        // JSON popup
        protected void rptBlockchainLog_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "ViewJson")
            {
                if (!int.TryParse(e.CommandArgument?.ToString(), out var transactionId))
                    return;

                string saleDataJson = null;
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spBlockchain_GetLatestJsonByTransaction", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@TransactionID", SqlDbType.Int).Value = transactionId;
                    con.Open();
                    saleDataJson = Convert.ToString(cmd.ExecuteScalar());
                }

                if (saleDataJson != null)
                {
                    string safeJson = HttpUtility.JavaScriptStringEncode(saleDataJson);
                    ScriptManager.RegisterStartupScript(this, GetType(), "PopupJson", $"openModal('{safeJson}');", true);
                }
            }
        }

        private string GenerateSHA256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                var sb = new StringBuilder();
                foreach (byte b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        protected string FormatLocalPH(object tsObj)
        {
            if (tsObj == null || tsObj == DBNull.Value) return "—";

            DateTime utc;
            if (tsObj is DateTime dt)
            {
                // SQL DateTime2 usually arrives as Kind=Unspecified; treat as UTC because we store UTC
                utc = (dt.Kind == DateTimeKind.Utc)
                      ? dt
                      : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            }
            else
            {
                // If the provider gives a string
                if (!DateTime.TryParse(tsObj.ToString(), out var parsed)) return tsObj.ToString();
                utc = DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
            }

            // PH time zone on Windows = "Singapore Standard Time" (UTC+08:00)
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
            var local = TimeZoneInfo.ConvertTimeFromUtc(utc, tz);

            // format however you like
            return local.ToString("yyyy-MM-dd h:mm tt"); // e.g., 2025-08-20 10:20 AM
        }


        private void LogAudit(string action)
        {
            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spAudit_Insert", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@AdminID", SqlDbType.Int).Value = Convert.ToInt32(Session["UserID"]);
                    cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 255).Value = action;
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch { /* do not break UX */ }
        }
    }
}
