using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
        public partial class Dashboard : System.Web.UI.Page
        {
            protected string salesDataJson = "{}";

            private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // ✅ Handle AJAX chart data requests first (prevent full page load)
            if (Request.QueryString["type"] != null)
            {
                string type = Request.QueryString["type"];

                if (type == "custom")
                {
                    string from = Request.QueryString["from"];
                    string to = Request.QueryString["to"];
                    string chartType = Request.QueryString["chart"];
                    LoadCustomSalesChart(from, to, chartType);
                }
                else
                {
                    LoadSalesChart(type);
                }

                Response.Clear();
                Response.ContentType = "application/json";
                Response.Write(salesDataJson);
                Response.End();
                return;
            }

            // ✅ Page load for regular dashboard view
            if (!IsPostBack)
            {
                lblWelcome.Text = "Welcome back, " + (Session["AdminName"]?.ToString() ?? "Admin") + "!";
                LoadTotalCounts();
                LoadBlockchainLog();
                LoadSalesChart("monthly"); // Optional: preload chart for server-side rendering
                LoadWeeklyBookingCalendar();
            }
        }

        private void LoadWeeklyBookingCalendar()
        {
            DataTable calendarTable = new DataTable();
            for (int i = 0; i < 7; i++)
                calendarTable.Columns.Add(((DayOfWeek)i).ToString());

            DateTime today = DateTime.Today;
            DateTime sunday = today.AddDays(-(int)today.DayOfWeek);

            Dictionary<DayOfWeek, List<string>> calendarData = new Dictionary<DayOfWeek, List<string>>();
            for (int i = 0; i < 7; i++)
                calendarData[(DayOfWeek)i] = new List<string>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
SELECT 
    b.ScheduledDate,
    c.FirstName,
    c.LastName,
    c.StreetAndUnit,
    c.Barangay,
    c.City,
    t.GroupName
FROM Bookings b
INNER JOIN Clients c ON b.ClientID = c.ClientID
LEFT JOIN Teams t ON b.TeamID = t.TeamID
WHERE 
    CAST(b.ScheduledDate AS DATE) BETWEEN @Sunday AND @Saturday
    AND b.Status NOT IN ('Cancelled')";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Sunday", sunday.Date);
                cmd.Parameters.AddWithValue("@Saturday", sunday.AddDays(6).Date);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    DateTime sched = Convert.ToDateTime(reader["ScheduledDate"]);
                    DayOfWeek day = sched.DayOfWeek;

                    string client = $"{reader["LastName"]}, {reader["FirstName"]}";
                    string address = $"{reader["StreetAndUnit"]}, {reader["Barangay"]}, {reader["City"]}";
                    string time = sched.ToString("hh:mm tt");
                    string groupName = reader["GroupName"] != DBNull.Value ? reader["GroupName"].ToString() : "Unassigned";

                    // Inject full content into modal
                    string modalContent = $"{client}<br/>{time}<br/>{address}<br/><strong>Team:</strong> {groupName}".Replace("'", "\\'");
                    string clickableDiv = $@"
                <div onclick=""showBookingDetails('{modalContent}')""
                     style='cursor:pointer; padding: 5px; border-radius:5px; transition:0.2s;'
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

            TableRow row = new TableRow();
            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                TableCell cell = new TableCell();
                cell.Text = $"<strong>{day}</strong><hr style='margin:5px;' />";
                cell.CssClass = "align-top";

                foreach (var entry in calendarData[day])
                {
                    cell.Text += entry + "<hr style='margin:5px 0;' />";
                }

                row.Cells.Add(cell);
            }

            tblCalendar.Rows.Add(row);
        }



        private void LoadTotalCounts()
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Clients", conn))
                    {
                        lblTotalClients.Text = cmd.ExecuteScalar().ToString();
                    }

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Employees", conn))
                    {
                        lblTotalWorkers.Text = cmd.ExecuteScalar().ToString();
                    }

                    // 🔵 Total Sales Today
                    using (SqlCommand cmd = new SqlCommand(@"
            SELECT ISNULL(SUM(Amount), 0) 
            FROM Transactions 
            WHERE CAST(TransactionDate AS DATE) = CAST(GETDATE() AS DATE)", conn))
                    {
                        lblTodaySales.Text = "₱" + Convert.ToDecimal(cmd.ExecuteScalar()).ToString("N2");
                    }

                    // 🔵 Total Sales This Month
                    using (SqlCommand cmd = new SqlCommand(@"
            SELECT ISNULL(SUM(Amount), 0) 
            FROM Transactions 
            WHERE MONTH(TransactionDate) = MONTH(GETDATE()) 
              AND YEAR(TransactionDate) = YEAR(GETDATE())", conn))
                    {
                        lblMonthSales.Text = "₱" + Convert.ToDecimal(cmd.ExecuteScalar()).ToString("N2");
                    }
                }
            }


            private void LoadBlockchainLog()
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    string query = "SELECT LogID, TransactionID, SaleHash, Timestamp FROM BlockchainSalesLog";
                    List<SqlParameter> parameters = new List<SqlParameter>();

                    // If filtering by date
                    if (!string.IsNullOrEmpty(txtFromDate.Text) && !string.IsNullOrEmpty(txtToDate.Text))
                    {
                        query += " WHERE CAST(Timestamp AS DATE) BETWEEN @FromDate AND @ToDate";
                        parameters.Add(new SqlParameter("@FromDate", txtFromDate.Text));
                        parameters.Add(new SqlParameter("@ToDate", txtToDate.Text));
                    }

                    query += " ORDER BY Timestamp DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (parameters.Count > 0)
                            cmd.Parameters.AddRange(parameters.ToArray());

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        rptBlockchainLog.DataSource = dt;
                        rptBlockchainLog.DataBind();
                    }
                }
            }
            private void LoadSalesChart(string type)
            {
                List<string> labels = new List<string>();
                List<decimal> data = new List<decimal>();

                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = "";

                    if (type == "daily")
                        query = "SELECT FORMAT(TransactionDate, 'yyyy-MM-dd') AS Period, SUM(Amount) AS Total FROM Transactions GROUP BY FORMAT(TransactionDate, 'yyyy-MM-dd') ORDER BY Period";
                    else if (type == "weekly")
                        query = "SELECT DATEPART(YEAR, TransactionDate) AS Year, DATEPART(WEEK, TransactionDate) AS Week, SUM(Amount) AS Total FROM Transactions GROUP BY DATEPART(YEAR, TransactionDate), DATEPART(WEEK, TransactionDate) ORDER BY Year, Week";
                    else if (type == "monthly")
                        query = "SELECT FORMAT(TransactionDate, 'yyyy-MM') AS Period, SUM(Amount) AS Total FROM Transactions GROUP BY FORMAT(TransactionDate, 'yyyy-MM') ORDER BY Period";
                    else if (type == "yearly")
                        query = "SELECT YEAR(TransactionDate) AS Period, SUM(Amount) AS Total FROM Transactions GROUP BY YEAR(TransactionDate) ORDER BY Period";

                    Dictionary<string, decimal> salesDict = new Dictionary<string, decimal>();

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (type == "weekly")
                            {
                                string year = reader["Year"].ToString();
                                string week = reader["Week"].ToString();
                                string formatted = $"{year}-Wk{week.PadLeft(2, '0')}";
                                salesDict[formatted] = Convert.ToDecimal(reader["Total"]);
                            }
                            else
                            {
                                string period = reader["Period"].ToString();
                                salesDict[period] = Convert.ToDecimal(reader["Total"]);
                            }
                        }
                    }

                    // 🔵 Now handle missing weeks/months automatically
                    if (salesDict.Count > 0)
                    {
                        if (type == "daily" || type == "monthly" || type == "yearly")
                        {
                            // Daily / Monthly / Yearly: just fill missing dates
                            foreach (var key in salesDict.Keys)
                                labels.Add(key);
                        }
                        else if (type == "weekly")
                        {
                            // Weekly: Sort properly by year+week
                            var sortedWeeks = new SortedSet<string>(salesDict.Keys, StringComparer.Ordinal);

                            foreach (var week in sortedWeeks)
                                labels.Add(week);
                        }

                        foreach (var label in labels)
                        {
                            if (salesDict.ContainsKey(label))
                                data.Add(salesDict[label]);
                            else
                                data.Add(0); // 🔵 Missing week/month/year = 0 sales
                        }
                    }

                    var salesData = new { labels = labels, data = data };
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    salesDataJson = js.Serialize(salesData);
                }
            }

            private void LoadCustomSalesChart(string fromDate, string toDate, string chartType)
            {
                List<string> labels = new List<string>();
                List<decimal> data = new List<decimal>();

                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = @"
            SELECT FORMAT(TransactionDate, 'yyyy-MM-dd') AS Period, 
                   SUM(Amount) AS Total
            FROM Transactions
            WHERE CAST(TransactionDate AS DATE) BETWEEN @FromDate AND @ToDate
            GROUP BY FORMAT(TransactionDate, 'yyyy-MM-dd')
            ORDER BY Period";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@FromDate", fromDate);
                        cmd.Parameters.AddWithValue("@ToDate", toDate);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                labels.Add(reader["Period"].ToString());
                                data.Add(Convert.ToDecimal(reader["Total"]));
                            }
                        }
                    }
                }

                var salesData = new
                {
                    labels = labels,
                    data = data,
                    chartType = chartType
                };

                JavaScriptSerializer js = new JavaScriptSerializer();
                salesDataJson = js.Serialize(salesData);
            }



            protected void btnFilterBlockchain_Click(object sender, EventArgs e)
            {
                LoadBlockchainLog();
            }

            protected void btnVerifyBlockchain_Click(object sender, EventArgs e)
            {
                bool allValid = true;
                int tamperedCount = 0;

                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT SaleHash, SaleDataJson FROM BlockchainSalesLog";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string storedHash = reader["SaleHash"].ToString();
                            string saleDataJson = reader["SaleDataJson"].ToString();
                            string recomputedHash = GenerateSHA256Hash(saleDataJson);

                            if (!string.Equals(storedHash, recomputedHash, StringComparison.OrdinalIgnoreCase))
                            {
                                allValid = false;
                                tamperedCount++;
                            }
                        }
                    }
                }

                if (allValid)
                {
                    lblVerificationResult.ForeColor = System.Drawing.Color.Green;
                    lblVerificationResult.Text = "✅ All blockchain records are valid and untampered!";
                }
                else
                {
                    lblVerificationResult.ForeColor = System.Drawing.Color.Red;
                    lblVerificationResult.Text = $"❌ Warning! {tamperedCount} blockchain record(s) were tampered!";
                }

                // 🔥 Add blockchain verification to Audit Logs
                AddAuditLog(Convert.ToInt32(Session["AdminID"]), "Performed blockchain verification.");
            }

            protected void rptBlockchainLog_ItemCommand(object source, RepeaterCommandEventArgs e)
            {
                if (e.CommandName == "ViewJson")
                {
                    string transactionId = e.CommandArgument.ToString();

                    using (SqlConnection conn = DatabaseHelper.GetConnection())
                    {
                        conn.Open();
                        string query = "SELECT SaleDataJson FROM BlockchainSalesLog WHERE TransactionID = @TransactionID";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@TransactionID", transactionId);
                            object result = cmd.ExecuteScalar();

                            if (result != null)
                            {
                                string saleDataJson = result.ToString();
                                ScriptManager.RegisterStartupScript(this, GetType(), "PopupJson", $"openModal(`{saleDataJson}`);", true);
                            }
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
                    {
                        builder.Append(b.ToString("x2"));
                    }
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
                            // Optional: log or ignore
                        }
                    }
                }
            }
        }
    }