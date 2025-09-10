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
                        // Date part
                        DateTime schedDate = Convert.ToDateTime(reader["ScheduledDate"]);

                        // Time part (SQL time -> .NET TimeSpan)
                        TimeSpan startTime = reader.IsDBNull(reader.GetOrdinal("StartTime"))
                            ? TimeSpan.Zero
                            : reader.GetTimeSpan(reader.GetOrdinal("StartTime"));

                        // For display (12-hour with AM/PM)
                        string time = DateTime.Today.Add(startTime).ToString("h:mm tt");

                        DayOfWeek day = schedDate.DayOfWeek;

                        string client = $"{reader["LastName"]}, {reader["FirstName"]}";
                        string address = $"{reader["StreetAndUnit"]}, {reader["Barangay"]}, {reader["City"]}";
                        string groupName = reader["GroupName"] == DBNull.Value ? "Unassigned" : reader["GroupName"].ToString();

                        string modalContent = $"{client}<br/>{time}<br/>{address}<br/><strong>Team:</strong> {groupName}"
                            .Replace("'", "\\'");
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
            using (var con = new SqlConnection(cs))
            {
                con.Open();

                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Clients", con))
                    lblTotalClients.Text = Convert.ToInt32(cmd.ExecuteScalar()).ToString("N0");

                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Employees", con))
                    lblTotalWorkers.Text = Convert.ToInt32(cmd.ExecuteScalar()).ToString("N0");
            }

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




        // ==================== BLOCKCHAIN VERIFY (CANONICAL) ====================
        protected void btnVerifyBlockchain_Click(object sender, EventArgs e)
        {
            int checkedCount = 0, tamperedCount = 0;
            bool allValid = true;

            // --- Optional HMAC key (base64 in web.config appSettings) ---
            byte[] hmacKey = null;
            try
            {
                var keyB64 = ConfigurationManager.AppSettings["BlockchainHmacKey"];
                if (!string.IsNullOrWhiteSpace(keyB64))
                    hmacKey = Convert.FromBase64String(keyB64);
            }
            catch { hmacKey = null; }

            // --- 1) Load all blockchain rows ---
            var rows = new List<BlockRow>();
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.spBlockchain_Log_ListForVerify", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        rows.Add(new BlockRow
                        {
                            LogID = (int)r["LogID"],
                            SaleJson = r["SaleDataJson"] as string ?? "",
                            SaleHashDb = r["SaleHash"] as string ?? "",
                            PrevHashDb = r["PrevHash"] as string ?? "",
                            ChainHashDb = r["ChainHash"] as string ?? "",
                            TxId = (int)r["TransactionID"],
                            Timestamp = (DateTime)r["Timestamp"],
                            AuthTag = r["AuthTag"] as byte[]
                        });
                    }
                }
            }

            rows.Sort((a, b) => a.LogID.CompareTo(b.LogID)); // oldest -> newest

            // --- 2) Verify forward ---
            string previousChain = null; // null on first row
            foreach (var row in rows)
            {
                checkedCount++;

                string saleHash = Sha256Utf8Lower(row.SaleJson);
                bool okSale = row.SaleHashDb.Equals(saleHash, StringComparison.OrdinalIgnoreCase);

                bool isFirst = (previousChain == null);
                bool prevOkFirst = row.PrevHashDb.Equals(saleHash, StringComparison.OrdinalIgnoreCase)
                                   || string.IsNullOrWhiteSpace(row.PrevHashDb)
                                   || IsAllZeros64(row.PrevHashDb);

                bool okPrev = isFirst ? prevOkFirst : row.PrevHashDb.Equals(previousChain, StringComparison.OrdinalIgnoreCase);

                string material = (previousChain == null ? saleHash : (previousChain + "." + saleHash));
                string chainHash = Sha256Utf8Lower(material);
                bool okChain = row.ChainHashDb.Equals(chainHash, StringComparison.OrdinalIgnoreCase);

                previousChain = row.ChainHashDb;

                bool okHmac = true;
                if (hmacKey != null && row.AuthTag?.Length > 0)
                {
                    string paidAtIso = row.Timestamp.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
                    string macMaterial = row.SaleJson + "|" + row.TxId + "|" + paidAtIso;
                    using (var h = new HMACSHA256(hmacKey))
                    {
                        byte[] expected = h.ComputeHash(Encoding.UTF8.GetBytes(macMaterial));
                        okHmac = Enumerable.SequenceEqual(row.AuthTag, expected);
                    }
                }

                if (!(okSale && okPrev && okChain && okHmac))
                {
                    allValid = false;
                    tamperedCount++;
                }
            }

            // --- 3) Display result only ---
            if (checkedCount == 0)
            {
                lblVerificationResult.ForeColor = System.Drawing.Color.Gray;
                lblVerificationResult.Text = "ℹ No blockchain records found.";
            }
            else if (allValid)
            {
                lblVerificationResult.ForeColor = System.Drawing.Color.Green;
                lblVerificationResult.Text = "✅ Verification complete. All records are consistent. No tampering detected.";
            }
            else
            {
                lblVerificationResult.ForeColor = System.Drawing.Color.Red;
                lblVerificationResult.Text = $"❌ {tamperedCount} of {checkedCount} record(s) appear altered or inconsistent.";
            }

            LogAudit("Performed blockchain verification.");
            LoadWeeklyBookingCalendar();
        }


        // Helper row type (C# 7.3-friendly)
        // Treat UI date pickers as PHT and convert to UTC range for SQL
        private static DateTime? PhtRangeStartUtc(DateTime? localDate)
        {
            if (!localDate.HasValue) return null;
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time"); // UTC+08
            return TimeZoneInfo.ConvertTimeToUtc(localDate.Value.Date, tz);
        }
        private static DateTime? PhtRangeEndUtcExclusive(DateTime? localDate)
        {
            if (!localDate.HasValue) return null;
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
            return TimeZoneInfo.ConvertTimeToUtc(localDate.Value.Date.AddDays(1), tz);
        }

        // Row holder (C# 7.3 friendly)
        private sealed class BlockRow
        {
            public int LogID;
            public string SaleJson;
            public string SaleHashDb;
            public string PrevHashDb;
            public string ChainHashDb;
            public int TxId;
            public DateTime Timestamp;
            public byte[] AuthTag;
        }

        // Hash helpers
        private static string Sha256Utf8Lower(string s)
        {
            using (var sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(s ?? string.Empty);
                byte[] hash = sha.ComputeHash(bytes);
                var sb = new StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++) sb.Append(hash[i].ToString("x2"));
                return sb.ToString();
            }
        }
        private static bool IsAllZeros64(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return true;
            var t = s.Trim();
            if (t.Length != 64) return false;
            for (int i = 0; i < 64; i++) if (t[i] != '0') return false;
            return true;
        }



        // JSON popup (unchanged)
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

        protected string FormatLocalPH(object tsObj)
        {
            if (tsObj == null || tsObj == DBNull.Value) return "—";

            DateTime utc;
            if (tsObj is DateTime dt)
            {
                utc = (dt.Kind == DateTimeKind.Utc)
                      ? dt
                      : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            }
            else
            {
                if (!DateTime.TryParse(tsObj.ToString(), out var parsed)) return tsObj.ToString();
                utc = DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
            }

            var tz = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
            var local = TimeZoneInfo.ConvertTimeFromUtc(utc, tz);
            return local.ToString("yyyy-MM-dd h:mm tt");
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

        // ==================== OPTIONAL ONE-TIME REPAIR ====================
        // Add a temporary button on the page:
        // <asp:Button ID="btnRecomputeChain" runat="server" Text="Recompute Chain (once)" OnClick="btnRecomputeChain_Click" />
        protected void btnRecomputeChain_Click(object sender, EventArgs e)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(@"SELECT LogID, SaleDataJson FROM dbo.BlockchainSalesLog ORDER BY LogID", con))
            {
                con.Open();

                var rows = new List<(int LogID, string Json)>();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        rows.Add(((int)r["LogID"], r["SaleDataJson"] as string ?? ""));
                }

                string prevChain = null;
                foreach (var row in rows)
                {
                    string saleHash = Sha256Utf8Lower(row.Json);

                    // chain = SHA256(prevChain + "." + saleHash), but for the first row use just saleHash
                    string material = (prevChain == null ? saleHash : (prevChain + "." + saleHash));
                    string chain = Sha256Utf8Lower(material);

                    // IMPORTANT: satisfy CK_BSL_Chain_Pair by keeping PrevHash non-NULL on first row too
                    string prevToStore = (prevChain == null) ? saleHash : prevChain;

                    using (var up = new SqlCommand(@"
UPDATE dbo.BlockchainSalesLog
SET SaleHash = @sale,
    PrevHash = @prev,
    ChainHash = @chain
WHERE LogID = @id;", con))
                    {
                        up.Parameters.Add("@sale", SqlDbType.Char, 64).Value = saleHash;
                        up.Parameters.Add("@prev", SqlDbType.Char, 64).Value = prevToStore; // never NULL now
                        up.Parameters.Add("@chain", SqlDbType.Char, 64).Value = chain;
                        up.Parameters.Add("@id", SqlDbType.Int).Value = row.LogID;
                        up.ExecuteNonQuery();
                    }

                    prevChain = chain;
                }

                // refresh today's anchor
                if (prevChain != null)
                {
                    using (var anchor = new SqlCommand(@"
MERGE dbo.BlockchainAnchors AS t
USING (SELECT CONVERT(date, SYSUTCDATETIME()) AS AnchorDate) AS s
ON (t.AnchorDate = s.AnchorDate)
WHEN MATCHED THEN
  UPDATE SET LastLogID = (SELECT MAX(LogID) FROM dbo.BlockchainSalesLog), ChainHash = @chain, CreatedAt = SYSUTCDATETIME()
WHEN NOT MATCHED THEN
  INSERT(AnchorDate, LastLogID, ChainHash, CreatedAt)
  VALUES(s.AnchorDate, (SELECT MAX(LogID) FROM dbo.BlockchainSalesLog), @chain, SYSUTCDATETIME());", con))
                    {
                        anchor.Parameters.Add("@chain", SqlDbType.Char, 64).Value = prevChain;
                        anchor.ExecuteNonQuery();
                    }
                }
            }

            lblVerificationResult.ForeColor = System.Drawing.Color.DarkGoldenrod;
            lblVerificationResult.Text = "🔧 Chain was recomputed to canonical format. Re-run Verify.";
        }

    }
}
