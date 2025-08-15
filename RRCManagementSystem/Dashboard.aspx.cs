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
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

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

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static object GetSalesData(string type)
        {
            var labels = new List<string>();
            var data = new List<decimal>();

            string cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;
            DateTime today = DateTime.Today;

            using (var conn = new SqlConnection(cs))
            {
                conn.Open();

                if (type == "weekly")
                {
                    // Current month, always 4 buckets. Extra days -> Week 4
                    DateTime start = new DateTime(today.Year, today.Month, 1);
                    DateTime end = start.AddMonths(1); // exclusive

                    // Labels: Week 1..Week 4
                    for (int w = 1; w <= 4; w++) labels.Add("Week " + w);
                    var totals = new decimal[4];

                    string q = @"SELECT TransactionDate, Amount
                         FROM Transactions
                         WHERE TransactionDate >= @start AND TransactionDate < @end";
                    using (var cmd = new SqlCommand(q, conn))
                    {
                        cmd.Parameters.AddWithValue("@start", start);
                        cmd.Parameters.AddWithValue("@end", end);

                        using (var rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                DateTime d = Convert.ToDateTime(rdr["TransactionDate"]);
                                int idx = (d.Day - 1) / 7;   // 0..4 (potentially)
                                if (idx < 0) idx = 0;
                                if (idx > 3) idx = 3;       // roll week 5 days into Week 4
                                totals[idx] += Convert.ToDecimal(rdr["Amount"]);
                            }
                        }
                    }

                    data.AddRange(totals);
                }
                else if (type == "monthly")
                {
                    // Current year: Jan..Dec
                    string[] monthNames = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
                    labels.AddRange(monthNames);
                    var totals = new decimal[12];

                    string q = @"SELECT MONTH(TransactionDate) AS M, SUM(Amount) AS Total
                         FROM Transactions
                         WHERE YEAR(TransactionDate) = @yr
                         GROUP BY MONTH(TransactionDate)";
                    using (var cmd = new SqlCommand(q, conn))
                    {
                        cmd.Parameters.AddWithValue("@yr", today.Year);
                        using (var rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                int m = Convert.ToInt32(rdr["M"]); // 1..12
                                totals[m - 1] = Convert.ToDecimal(rdr["Total"]);
                            }
                        }
                    }

                    data.AddRange(totals);
                }
                else if (type == "yearly")
                {
                    // Last 6 years including current (e.g., 2020..2025)
                    int startYear = today.Year - 5;
                    var map = new Dictionary<int, decimal>();
                    for (int y = startYear; y <= today.Year; y++)
                    {
                        labels.Add(y.ToString());
                        map[y] = 0m;
                    }

                    string q = @"SELECT YEAR(TransactionDate) AS Y, SUM(Amount) AS Total
                         FROM Transactions
                         WHERE YEAR(TransactionDate) BETWEEN @y1 AND @y2
                         GROUP BY YEAR(TransactionDate)";
                    using (var cmd = new SqlCommand(q, conn))
                    {
                        cmd.Parameters.AddWithValue("@y1", startYear);
                        cmd.Parameters.AddWithValue("@y2", today.Year);

                        using (var rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                int y = Convert.ToInt32(rdr["Y"]);
                                map[y] = Convert.ToDecimal(rdr["Total"]);
                            }
                        }
                    }

                    foreach (var lab in labels) data.Add(map[int.Parse(lab)]);
                }
                else
                {
                    // Daily for current month: 1..last day
                    DateTime start = new DateTime(today.Year, today.Month, 1);
                    DateTime end = start.AddMonths(1);
                    int daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);

                    var totals = new decimal[daysInMonth];
                    for (int d = 1; d <= daysInMonth; d++) labels.Add(d.ToString());

                    string q = @"SELECT DAY(TransactionDate) AS D, SUM(Amount) AS Total
                         FROM Transactions
                         WHERE TransactionDate >= @start AND TransactionDate < @end
                         GROUP BY DAY(TransactionDate)";
                    using (var cmd = new SqlCommand(q, conn))
                    {
                        cmd.Parameters.AddWithValue("@start", start);
                        cmd.Parameters.AddWithValue("@end", end);

                        using (var rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                int d = Convert.ToInt32(rdr["D"]); // 1..daysInMonth
                                totals[d - 1] = Convert.ToDecimal(rdr["Total"]);
                            }
                        }
                    }

                    data.AddRange(totals);
                }
            }

            return new { labels, data };
        }


        private void LoadWeeklyBookingCalendar()
        {
            // Always start fresh
            tblCalendar.Rows.Clear();

            // Sunday-start week
            DateTime today = DateTime.Today;
            DateTime sunday = today.AddDays(-(int)today.DayOfWeek);
            DateTime saturday = sunday.AddDays(6);

            // Prepare buckets per day
            var calendarData = new Dictionary<DayOfWeek, List<string>>();
            foreach (DayOfWeek d in Enum.GetValues(typeof(DayOfWeek)))
                calendarData[d] = new List<string>();

            // Pull bookings for the week (ok if none)
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(@"
        SELECT 
            b.ScheduledDate,
            c.FirstName, c.LastName,
            c.StreetAndUnit, c.Barangay, c.City,
            t.GroupName
        FROM Bookings b
        INNER JOIN Clients c ON b.ClientID = c.ClientID
        LEFT JOIN Teams t ON b.TeamID = t.TeamID
        WHERE CAST(b.ScheduledDate AS DATE) BETWEEN @Sunday AND @Saturday
          AND b.Status NOT IN ('Cancelled')
        ORDER BY b.ScheduledDate", con))
            {
                cmd.Parameters.AddWithValue("@Sunday", sunday.Date);
                cmd.Parameters.AddWithValue("@Saturday", saturday.Date);

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

            // Optional: header row with dates (Sun–Sat)
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

            // Body row with cells for each day (show placeholder if empty)
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
                    var sb = new System.Text.StringBuilder();
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


        // ==================== METRICS ====================
        private void LoadTotalCounts()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Simple counts
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Clients", conn))
                    lblTotalClients.Text = (Convert.ToInt32(cmd.ExecuteScalar())).ToString("N0");

                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Employees", conn))
                    lblTotalWorkers.Text = (Convert.ToInt32(cmd.ExecuteScalar())).ToString("N0");

                // --- Build PH time boundaries in SQL once, reuse them (robust + fast) ---
                // We compute:
                //   @phtTodayStart = today 00:00 PHT
                //   @phtTomorrowStart = tomorrow 00:00 PHT  (exclusive)
                //   @phtMonthStart = 1st day of this month 00:00 PHT
                //   @phtNextMonthStart = 1st day of next month 00:00 PHT (exclusive)
                string timeCte = @"
;WITH tz AS (
    SELECT 
        CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'Singapore Standard Time' AS datetimeoffset) AS NowPht
),
bounds AS (
    SELECT
        CAST(CAST(NowPht AS date) AS datetime)       AS PhtTodayStart,      -- today 00:00 PHT
        DATEADD(day, 1, CAST(CAST(NowPht AS date) AS datetime)) AS PhtTomorrowStart,
        CAST(DATEFROMPARTS(YEAR(NowPht), MONTH(NowPht), 1) AS datetime) AS PhtMonthStart,
        CAST(DATEADD(month, 1, DATEFROMPARTS(YEAR(NowPht), MONTH(NowPht), 1)) AS datetime) AS PhtNextMonthStart
    FROM tz
)
";

                // TODAY (PHT): sum Amount where TransactionDate shifted to +08:00 is in [today, tomorrow)
                using (var cmd = new SqlCommand(timeCte + @"
SELECT ISNULL(SUM(t.Amount), 0)
FROM Transactions t
CROSS JOIN bounds b
WHERE SWITCHOFFSET(CONVERT(datetimeoffset, t.TransactionDate), '+08:00') >= b.PhtTodayStart
  AND SWITCHOFFSET(CONVERT(datetimeoffset, t.TransactionDate), '+08:00') <  b.PhtTomorrowStart
", conn))
                {
                    lblTodaySales.Text = "₱" + Convert.ToDecimal(cmd.ExecuteScalar() ?? 0m).ToString("N2");
                }

                // THIS MONTH (PHT): sum Amount where shifted timestamp is in [monthStart, nextMonthStart)
                using (var cmd = new SqlCommand(timeCte + @"
SELECT ISNULL(SUM(t.Amount), 0)
FROM Transactions t
CROSS JOIN bounds b
WHERE SWITCHOFFSET(CONVERT(datetimeoffset, t.TransactionDate), '+08:00') >= b.PhtMonthStart
  AND SWITCHOFFSET(CONVERT(datetimeoffset, t.TransactionDate), '+08:00') <  b.PhtNextMonthStart
", conn))
                {
                    lblMonthSales.Text = "₱" + Convert.ToDecimal(cmd.ExecuteScalar() ?? 0m).ToString("N2");
                }
            }
        }


        // ==================== BLOCKCHAIN LOG ====================
        private void LoadBlockchainLog()
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = conn.CreateCommand())
            {
                // Parse dates if provided
                DateTime fromDate, toDate;
                bool hasFrom = DateTime.TryParse(txtFromDate.Text, out fromDate);
                bool hasTo = DateTime.TryParse(txtToDate.Text, out toDate);

                // Make 'to' an exclusive upper bound so the full end date is included
                if (hasTo) toDate = toDate.Date.AddDays(1);

                cmd.CommandText = @"
            SELECT LogID, TransactionID, SaleHash, Timestamp
            FROM BlockchainSalesLog
            WHERE (@From IS NULL OR Timestamp >= @From)
              AND (@To   IS NULL OR Timestamp <  @To)
            ORDER BY Timestamp DESC;";

                cmd.Parameters.Add("@From", SqlDbType.DateTime).Value = hasFrom ? (object)fromDate.Date : DBNull.Value;
                cmd.Parameters.Add("@To", SqlDbType.DateTime).Value = hasTo ? (object)toDate : DBNull.Value;

                var dt = new DataTable();
                using (var da = new SqlDataAdapter(cmd))
                {
                    conn.Open();
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

        protected void btnVerifyBlockchain_Click(object sender, EventArgs e)
        {
            int checkedCount = 0, tamperedCount = 0;
            bool allValid = true;

            DateTime fromDate, toDate;
            bool hasFrom = DateTime.TryParse(txtFromDate.Text, out fromDate);
            bool hasTo = DateTime.TryParse(txtToDate.Text, out toDate);
            if (hasTo) toDate = toDate.Date.AddDays(1);

            string expectedPrev = new string('0', 64);

            // Try to read HMAC key; if missing, we'll skip HMAC checks
            byte[] hmacKey = null;
            try
            {
                var keyB64 = ConfigurationManager.AppSettings["BlockchainHmacKey"];
                if (!string.IsNullOrWhiteSpace(keyB64))
                    hmacKey = Convert.FromBase64String(keyB64);
            }
            catch { hmacKey = null; }

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
SELECT LogID, TransactionID, SaleHash, SaleDataJson, PrevHash, ChainHash, Timestamp, AuthTag
FROM BlockchainSalesLog
WHERE (@From IS NULL OR Timestamp >= @From)
  AND (@To   IS NULL OR Timestamp <  @To)
ORDER BY LogID ASC;";
                cmd.Parameters.Add("@From", SqlDbType.DateTime).Value = hasFrom ? (object)fromDate.Date : DBNull.Value;
                cmd.Parameters.Add("@To", SqlDbType.DateTime).Value = hasTo ? (object)toDate : DBNull.Value;

                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    bool isFirstRowInRange = true;

                    while (r.Read())
                    {
                        checkedCount++;

                        string saleJson = r["SaleDataJson"] as string ?? "";
                        string saleHashDb = r["SaleHash"] as string ?? "";
                        string prevHash = r["PrevHash"] as string ?? "";
                        string chainHash = r["ChainHash"] as string ?? "";
                        int txId = (int)r["TransactionID"];
                        DateTime ts = (DateTime)r["Timestamp"];
                        byte[] tag = r["AuthTag"] as byte[]; // may be null for legacy

                        // 1) JSON hash
                        string recomputedSaleHash = GenerateSHA256Hash(saleJson);
                        bool okSale = string.Equals(saleHashDb, recomputedSaleHash, StringComparison.OrdinalIgnoreCase);

                        // Detect whether this row has chain fields
                        bool hasChain = !string.IsNullOrWhiteSpace(prevHash) && prevHash.Length == 64
                                     && !string.IsNullOrWhiteSpace(chainHash) && chainHash.Length == 64;

                        bool okPrev = true, okChain = true;

                        if (hasChain)
                        {
                            if (isFirstRowInRange)
                            {
                                // For the first row in a filtered set, accept its PrevHash as the starting point.
                                expectedPrev = prevHash;
                            }
                            // 2) prev link (now we can compare)
                            okPrev = string.Equals(prevHash, expectedPrev, StringComparison.OrdinalIgnoreCase);

                            // 3) chain hash
                            string material = $"{prevHash}|{recomputedSaleHash}|{txId}|{ts.ToUniversalTime():O}";
                            string recomputedChain = GenerateSHA256Hash(material);
                            okChain = string.Equals(chainHash, recomputedChain, StringComparison.OrdinalIgnoreCase);

                            // advance expected link only when we have chain
                            expectedPrev = chainHash;
                        }

                        // 4) HMAC (optional if key/tag missing)
                        bool okHmac = true;
                        if (hmacKey != null)
                        {
                            if (tag != null && tag.Length > 0)
                            {
                                string paidAtIso = ts.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
                                string macMaterial = $"{saleJson}|{txId}|{paidAtIso}";
                                byte[] expected;
                                using (var h = new HMACSHA256(hmacKey))
                                    expected = h.ComputeHash(Encoding.UTF8.GetBytes(macMaterial));
                                okHmac = tag.SequenceEqual(expected);
                            }
                            // else: legacy row without tag -> treat as OK
                        }

                        if (!(okSale && okPrev && okChain && okHmac))
                        {
                            allValid = false;
                            tamperedCount++;
                        }

                        isFirstRowInRange = false;
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
                lblVerificationResult.Text = "✅ Verification complete.<br/>" +
     "Your records match what was originally saved.<br/>" +
     "The sequence of records is correct.<br/>" +
     "No signs of changes or tampering.";

            }
            else
            {
                lblVerificationResult.ForeColor = System.Drawing.Color.Red;
                lblVerificationResult.Text = $"❌ {tamperedCount} out of {checkedCount} record(s) appear to have been altered or are inconsistent.";

            }

            AddAuditLog(Convert.ToInt32(Session["AdminID"]), "Performed blockchain verification.");
        }



        protected void rptBlockchainLog_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "ViewJson")
            {
                var arg = e.CommandArgument?.ToString();
                if (!int.TryParse(arg, out var transactionId))
                    return;

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT TOP 1 SaleDataJson FROM BlockchainSalesLog WHERE TransactionID = @TransactionID ORDER BY Timestamp DESC;";
                    cmd.Parameters.Add("@TransactionID", SqlDbType.Int).Value = transactionId;

                    conn.Open();
                    var result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        string saleDataJson = result.ToString();
                        string safeJson = HttpUtility.JavaScriptStringEncode(saleDataJson);
                        // openModal expects a string; pass a JS-safe version
                        ScriptManager.RegisterStartupScript(this, GetType(), "PopupJson", $"openModal('{safeJson}');", true);
                    }
                }
            }
        }

        private string GenerateSHA256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }

        private void AddAuditLog(int? userID, string action)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO AuditLogs (AdminID, Action, Timestamp) VALUES (@AdminID, @Action, GETDATE())";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@AdminID", (object)userID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Action", action);

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch
                    {
                        // swallow
                    }
                }
            }
        }
    }
}
