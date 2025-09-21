using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI.WebControls;
using System.Collections.Generic;

namespace RRCManagementSystem
{
    // =========================
    // Serializable Class Here
    // =========================
    [Serializable]
    public class AuditLogEntry
    {
        public int LogID { get; set; }
        public string AdminName { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }
    }

    // =========================
    // Main Page Code Behind
    // =========================
    public partial class AuditLogs : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Initial data load on first page visit
                LoadAuditLogs();
            }
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            // Reloads data and rebinds the GridView when the Filter button is clicked
            LoadAuditLogs();
        }

        private void LoadAuditLogs()
        {
            DateTime fromDate, toDate;
            bool hasFrom = DateTime.TryParse(txtFrom.Text.Trim(), out fromDate);
            bool hasTo = DateTime.TryParse(txtTo.Text.Trim(), out toDate);

            if (hasFrom && hasTo && fromDate.Date > toDate.Date)
            {
                lblMessage.Text = "“From Date” must be earlier than or equal to “To Date”.";
                gvLogs.Visible = false;
                lblNoData.Visible = true;
                lblNoData.Text = "⚠ Invalid date range.";
                return;
            }

            DataTable allLogs = new DataTable();

            // Fetch logs from DB using the stored procedure
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spAuditLogs_List", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@From", hasFrom ? (object)fromDate.Date : DBNull.Value);
                cmd.Parameters.AddWithValue("@To", hasTo ? (object)toDate.Date : DBNull.Value);
                da.Fill(allLogs);
            }

            // Convert DataTable to a list of objects
            List<AuditLogEntry> logEntries = allLogs.AsEnumerable().Select(r => new AuditLogEntry
            {
                LogID = r.Field<int>("LogID"),
                AdminName = r.Field<string>("AdminName"),
                Action = r.Field<string>("Action"),
                Timestamp = r.Field<DateTime>("Timestamp")
            }).ToList();

            // Store the data in ViewState for subsequent postbacks (like paging)
            ViewState["AuditLogsData"] = logEntries;

            // Bind the data directly to the GridView
            if (logEntries.Count == 0)
            {
                gvLogs.Visible = false;
                lblNoData.Visible = true;
                lblNoData.Text = "⚠ No audit logs found for the selected date range.";
            }
            else
            {
                gvLogs.Visible = true;
                lblNoData.Visible = false;
                gvLogs.DataSource = logEntries;
                gvLogs.DataBind();
            }

            lblMessage.Text = "";
        }

        protected void gvLogs_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            // Set the new page index for the GridView
            gvLogs.PageIndex = e.NewPageIndex;

            // Retrieve the data from ViewState and rebind the GridView
            var logEntries = ViewState["AuditLogsData"] as List<AuditLogEntry>;
            if (logEntries != null)
            {
                gvLogs.DataSource = logEntries;
                gvLogs.DataBind();
            }
        }

        // The repeater-related methods are removed as they are no longer needed.
        // protected void rptYears_ItemDataBound(...)
        // protected void rptMonths_ItemDataBound(...)
    }
}