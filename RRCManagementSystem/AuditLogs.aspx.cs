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
            // Parse dates from the HTML5 date inputs (yyyy-MM-dd)
            DateTime fromDate, toDate;
            bool hasFrom = DateTime.TryParse(txtFrom.Text.Trim(), out fromDate);
            bool hasTo = DateTime.TryParse(txtTo.Text.Trim(), out toDate);

            // Guard: if both provided, ensure range is valid
            if (hasFrom && hasTo && fromDate.Date > toDate.Date)
            {
                lblMessage.Text = "“From Date” must be earlier than or equal to “To Date”.";
                rptYears.Visible = false;
                lblNoData.Visible = true;
                lblNoData.Text = "⚠ Invalid date range.";
                return;
            }

            DataTable allLogs = new DataTable();

            // ✅ Call stored procedure instead of inline SQL
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spAuditLogs_List", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                var pFrom = cmd.Parameters.Add("@From", SqlDbType.Date);
                pFrom.Value = hasFrom ? (object)fromDate.Date : DBNull.Value;

                var pTo = cmd.Parameters.Add("@To", SqlDbType.Date);
                pTo.Value = hasTo ? (object)toDate.Date : DBNull.Value;

                da.Fill(allLogs);
            }

            // Group (same as before)
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

            var finalGroup = groupedLogs
                .GroupBy(x => x.Year)
                .Select(y => new { Year = y.Key, Months = y.ToList() })
                .Where(y => y.Months.Any(m => m.Logs != null && m.Logs.Rows.Count > 0))
                .ToList();

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

            // clear any old message
            lblMessage.Text = "";
        }

        protected void rptYears_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                dynamic yearGroup = e.Item.DataItem;
                var rptMonths = (Repeater)e.Item.FindControl("rptMonths");
                rptMonths.DataSource = yearGroup.Months;
                rptMonths.DataBind();
            }
        }

        protected void rptMonths_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                dynamic monthGroup = e.Item.DataItem;
                var gvLogs = (GridView)e.Item.FindControl("gvLogs");
                gvLogs.DataSource = monthGroup.Logs;
                gvLogs.DataBind();
            }
        }
    }
}
