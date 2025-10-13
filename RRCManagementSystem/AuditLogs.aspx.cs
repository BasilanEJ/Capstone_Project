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
    // Serializable Class
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
                LoadAuditLogs();
            }
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
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

            // Store the data in ViewState for paging
            ViewState["AuditLogsData"] = logEntries;

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
            gvLogs.PageIndex = e.NewPageIndex;

            var logEntries = ViewState["AuditLogsData"] as List<AuditLogEntry>;
            if (logEntries != null)
            {
                gvLogs.DataSource = logEntries;
                gvLogs.DataBind();
            }
        }

        // =========================
        // CUSTOM NUMERIC PAGER METHODS
        // =========================
        protected void gvLogs_DataBound(object sender, EventArgs e)
        {
            Repeater rptPages = (Repeater)gvLogs.BottomPagerRow?.FindControl("rptPages");
            if (rptPages != null)
            {
                int totalPages = gvLogs.PageCount;
                if (totalPages > 0)
                {
                    List<int> pages = Enumerable.Range(1, totalPages).ToList();
                    rptPages.DataSource = pages;
                    rptPages.DataBind();

                    // Disable buttons when necessary
                    LinkButton btnFirst = (LinkButton)gvLogs.BottomPagerRow.FindControl("btnFirst");
                    LinkButton btnPrev = (LinkButton)gvLogs.BottomPagerRow.FindControl("btnPrev");
                    LinkButton btnNext = (LinkButton)gvLogs.BottomPagerRow.FindControl("btnNext");
                    LinkButton btnLast = (LinkButton)gvLogs.BottomPagerRow.FindControl("btnLast");

                    if (btnFirst != null) btnFirst.Enabled = gvLogs.PageIndex > 0;
                    if (btnPrev != null) btnPrev.Enabled = gvLogs.PageIndex > 0;
                    if (btnNext != null) btnNext.Enabled = gvLogs.PageIndex < totalPages - 1;
                    if (btnLast != null) btnLast.Enabled = gvLogs.PageIndex < totalPages - 1;
                }
            }
        }

        protected void rptPages_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Page")
            {
                gvLogs.PageIndex = Convert.ToInt32(e.CommandArgument) - 1;

                var logEntries = ViewState["AuditLogsData"] as List<AuditLogEntry>;
                if (logEntries != null)
                {
                    gvLogs.DataSource = logEntries;
                    gvLogs.DataBind();
                }
            }
        }

        protected void rptPages_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                LinkButton lnkPage = (LinkButton)e.Item.FindControl("lnkPage");
                if (lnkPage != null && lnkPage.CommandArgument == (gvLogs.PageIndex + 1).ToString())
                {
                    lnkPage.CssClass = "selected-page";
                    lnkPage.Enabled = false;
                }
            }
        }
    }
}
