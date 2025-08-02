using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class AuditLogs : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadAuditLogs();
            }
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            LoadAuditLogs();
        }
        private void LoadAuditLogs()
        {
            DateTime? fromDate = null, toDate = null;

            if (DateTime.TryParse(txtFrom.Text.Trim(), out DateTime fDate))
                fromDate = fDate;

            if (DateTime.TryParse(txtTo.Text.Trim(), out DateTime tDate))
                toDate = tDate;

            DataTable allLogs = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
        SELECT 
            a.LogID, 
            u.Name AS AdminName, 
            a.Action, 
            a.Timestamp
        FROM AuditLogs a
        LEFT JOIN Users u ON a.AdminID = u.UserID
        ORDER BY a.Timestamp DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(allLogs);
                }
            }

            // Filter by date
            if (fromDate.HasValue && toDate.HasValue)
            {
                var filteredRows = allLogs.AsEnumerable()
                    .Where(row =>
                    {
                        DateTime ts = row.Field<DateTime>("Timestamp");
                        return ts >= fromDate.Value && ts <= toDate.Value;
                    });

                allLogs = filteredRows.Any() ? filteredRows.CopyToDataTable() : allLogs.Clone();
            }

            // Group logs
            var groupedLogs = allLogs.AsEnumerable()
                .GroupBy(r => new { Year = r.Field<DateTime>("Timestamp").Year, Month = r.Field<DateTime>("Timestamp").Month })
                .Select(g =>
                {
                    DataTable subTable = g.Any() ? g.CopyToDataTable() : allLogs.Clone();
                    return new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM"),
                        Logs = subTable
                    };
                })
                .ToList();

            // Group by year
            var finalGroup = groupedLogs
                .GroupBy(x => x.Year)
                .Select(y => new
                {
                    Year = y.Key,
                    Months = y.ToList()
                })
                // Only keep years with at least one month with logs
                .Where(y => y.Months.Any(m => m.Logs != null && m.Logs.Rows.Count > 0))
                .ToList();

            // 🔴 SHOW OR HIDE lblNoData
            if (finalGroup.Count == 0)
            {
                rptYears.Visible = false;
                lblNoData.Visible = true;
                lblNoData.Text = "⚠ No audit logs found for the selected date range.";
            }
            else
            {
                rptYears.Visible = true;
                lblNoData.Visible = false;
                rptYears.DataSource = finalGroup;
                rptYears.DataBind();
            }
        }




        protected void rptYears_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                dynamic yearGroup = e.Item.DataItem;
                Repeater rptMonths = (Repeater)e.Item.FindControl("rptMonths");
                rptMonths.DataSource = yearGroup.Months;
                rptMonths.DataBind();
            }
        }

        protected void rptMonths_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                dynamic monthGroup = e.Item.DataItem;
                GridView gvLogs = (GridView)e.Item.FindControl("gvLogs");
                gvLogs.DataSource = monthGroup.Logs;
                gvLogs.DataBind();
            }
        }
    }
}
