using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace RRCManagementSystem
{
    public partial class Reports : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                lblMessage.Text = "";

                // Set calendar limits
                string minDate = "2018-01-01";
                string todayDate = DateTime.Now.ToString("yyyy-MM-dd");

                txtDateFrom.Attributes["min"] = minDate;
                txtDateFrom.Attributes["max"] = todayDate;

                txtDateTo.Attributes["min"] = minDate;
                txtDateTo.Attributes["max"] = todayDate;

                // Optional default values
                txtDateFrom.Text = minDate;
                txtDateTo.Text = todayDate;
            }
        }


        protected void ddlReportType_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblMessage.Text = "";
            gvReports.DataSource = null;
            gvReports.DataBind();
        }

        protected void btnGenerate_Click(object sender, EventArgs e)
        {
            string reportType = ddlReportType.SelectedValue;
            DateTime dateFrom, dateTo;

            // Validate report type selection
            if (string.IsNullOrEmpty(reportType))
            {
                lblMessage.Text = "⚠ Please select a report type.";
                return;
            }

            // Parse the provided dates
            DateTime minDateAllowed = new DateTime(2018, 1, 1); // Set the minimum allowed date (2018)

            if (!DateTime.TryParse(txtDateFrom.Text, out dateFrom) || dateFrom < minDateAllowed)
            {
                dateFrom = minDateAllowed;
            }

            if (!DateTime.TryParse(txtDateTo.Text, out dateTo))
            {
                dateTo = DateTime.Now; // Default to now if no input
            }

            // Ensure dateTo is not earlier than dateFrom
            if (dateTo < dateFrom)
            {
                lblMessage.Text = "⚠ Date To cannot be earlier than Date From.";
                return;
            }

            LoadReport(reportType, dateFrom, dateTo);
        }


        private void LoadReport(string reportType, DateTime dateFrom, DateTime dateTo)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "";

                switch (reportType)
                {
                    case "AdminActivity":
                        query = @"SELECT UserID, Name, Email, Role, CreatedAt
                                  FROM Users
                                  WHERE Role = 'Admin' AND CreatedAt BETWEEN @DateFrom AND @DateTo";
                        break;

                    case "Sales":
                        query = @"SELECT SaleID, ClientID, Amount, PaymentStatus, TransactionDate
                                  FROM Sales
                                  WHERE TransactionDate BETWEEN @DateFrom AND @DateTo";
                        break;

                    case "AuditLogs":
                        query = @"SELECT a.LogID, u.Name AS AdminName, a.Action, a.Timestamp
                                  FROM AuditLogs a
                                  INNER JOIN Users u ON a.AdminID = u.UserID
                                  WHERE a.Timestamp BETWEEN @DateFrom AND @DateTo";
                        break;

                    case "WorkOrders":
                        query = @"SELECT WorkOrderID, ClientID, Status, ScheduledDate, CompletionDate
                                  FROM WorkOrders
                                  WHERE ScheduledDate BETWEEN @DateFrom AND @DateTo";
                        break;

                    default:
                        lblMessage.Text = "⚠ Report type not recognized.";
                        return;
                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DateFrom", dateFrom);
                    cmd.Parameters.AddWithValue("@DateTo", dateTo);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    try
                    {
                        conn.Open();
                        da.Fill(dt);

                        gvReports.DataSource = dt;
                        gvReports.DataBind();

                        lblMessage.Text = dt.Rows.Count > 0 ? $"✅ {dt.Rows.Count} records found." : "⚠ No records found for the selected criteria.";
                    }
                    catch (Exception ex)
                    {
                        lblMessage.Text = "⚠ Error loading report: " + ex.Message;
                    }
                }
            }
        }
    }
}
