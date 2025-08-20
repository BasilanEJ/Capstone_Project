using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class Reports : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // dropdown items are in .aspx
            }
        }

        protected void btnGenerate_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "";
            gvReports.DataSource = null;
            gvReports.DataBind();

            string selected = ddlModule.SelectedValue;
            if (string.IsNullOrEmpty(selected))
            {
                lblMessage.Text = "Please select a module.";
                return;
            }

            // Parse yyyy-MM-dd (HTML5 date inputs)
            DateTime fromDate, toDate;
            bool hasFrom = DateTime.TryParseExact(txtDateFrom.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDate);
            bool hasTo = DateTime.TryParseExact(txtDateTo.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out toDate);

            // Optional guard
            if (hasFrom && hasTo && fromDate.Date > toDate.Date)
            {
                lblMessage.Text = "“Date From” must be earlier than or equal to “Date To”.";
                return;
            }

            // Debug (optional)
            lblDebug.Text =
                $"Raw From: {txtDateFrom.Text} | Parsed: {(hasFrom ? fromDate.ToString("yyyy-MM-dd") : "INVALID")}<br/>" +
                $"Raw To: {txtDateTo.Text} | Parsed: {(hasTo ? toDate.ToString("yyyy-MM-dd") : "INVALID")}";

            string procName;
            bool sendDates = true;

            switch (selected)
            {
                case "Admins":
                    procName = "dbo.spReport_Admins";
                    break;
                case "ArchivedAdmins":
                    procName = "dbo.spReport_ArchivedAdmins";
                    break;
                case "Roles":
                    procName = "dbo.spReport_Roles";
                    sendDates = false; // this proc has no date params
                    break;
                case "AuditLogs":
                    procName = "dbo.spReport_AuditLogs";
                    break;
                case "SystemChanges":
                    procName = "dbo.spReport_SystemChanges";
                    break;
                default:
                    lblMessage.Text = "Invalid module selected.";
                    return;
            }

            DataTable dt = new DataTable();

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(procName, conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                if (sendDates)
                {
                    var pFrom = cmd.Parameters.Add("@From", SqlDbType.Date);
                    pFrom.Value = hasFrom ? (object)fromDate.Date : DBNull.Value;

                    var pTo = cmd.Parameters.Add("@To", SqlDbType.Date);
                    pTo.Value = hasTo ? (object)toDate.Date : DBNull.Value;
                }

                try
                {
                    da.Fill(dt);
                    gvReports.DataSource = dt;
                    gvReports.DataBind();

                    if (dt.Rows.Count == 0)
                        lblMessage.Text = "No data found for the selected module and date range.";
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "Error generating report: " + ex.Message;
                    return;
                }
            }

            // ✅ Log the report generation (non-blocking of the main result)
            try
            {
                int userId = 0;
                if (Session["UserID"] != null)
                {
                    int.TryParse(Session["UserID"].ToString(), out userId);
                }

                LogReport(selected,
                          userId,
                          hasFrom ? fromDate.Date : (DateTime?)null,
                          hasTo ? toDate.Date : (DateTime?)null,
                          $"{dt.Rows.Count} row(s) returned");
            }
            catch
            {
                // swallow logging errors by design; don't break the report
            }
        }

        private void LogReport(string reportType, int generatedBy, DateTime? from, DateTime? to, string remarks)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spReportsLog_Insert", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@ReportType", SqlDbType.NVarChar, 100).Value = (object)reportType ?? DBNull.Value;
                cmd.Parameters.Add("@GeneratedBy", SqlDbType.Int).Value = generatedBy; // 0 is OK if not logged-in context
                cmd.Parameters.Add("@FromDate", SqlDbType.Date).Value = from.HasValue ? (object)from.Value : DBNull.Value;
                cmd.Parameters.Add("@ToDate", SqlDbType.Date).Value = to.HasValue ? (object)to.Value : DBNull.Value;
                cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, -1).Value = string.IsNullOrWhiteSpace(remarks) ? (object)DBNull.Value : remarks;

                var pOut = new SqlParameter("@NewReportID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(pOut);

                conn.Open();
                cmd.ExecuteNonQuery();
                // int newId = (pOut.Value == DBNull.Value) ? 0 : Convert.ToInt32(pOut.Value); // use if needed
            }
        }
    }
}
    