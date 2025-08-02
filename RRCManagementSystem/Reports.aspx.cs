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
                // Dropdown is already populated in .aspx
            }
        }

        protected void btnGenerate_Click(object sender, EventArgs e)
        {
            string selected = ddlModule.SelectedValue;

            if (string.IsNullOrEmpty(selected))
            {
                lblMessage.Text = "Please select a module.";
                gvReports.DataSource = null;
                gvReports.DataBind();
                return;
            }

            // Parse dates exactly from yyyy-MM-dd (HTML5 date format)
            DateTime fromDate;
            DateTime toDate;
            bool hasFrom = DateTime.TryParseExact(txtDateFrom.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDate);
            bool hasTo = DateTime.TryParseExact(txtDateTo.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out toDate);

            // 🐛 Debug: show what was parsed
            lblDebug.Text = $"Raw From: {txtDateFrom.Text} | Parsed From: {(hasFrom ? fromDate.ToString("yyyy-MM-dd") : "INVALID")}<br/>" +
                            $"Raw To: {txtDateTo.Text} | Parsed To: {(hasTo ? toDate.ToString("yyyy-MM-dd") : "INVALID")}";

            string query = "";

            switch (selected)
            {
                case "Admins":
                    query = @"
                SELECT UserID, Name, Email, Role, CreatedAt 
                FROM Users 
                WHERE Status = 'Active' AND Role <> 'SuperAdmin'
                AND (@From IS NULL OR CreatedAt >= @From)
                AND (@To IS NULL OR CreatedAt < DATEADD(DAY, 1, @To))";
                    break;

                case "ArchivedAdmins":
                    query = @"
                SELECT UserID, Name, Email, Role, CreatedAt 
                FROM Users 
                WHERE Status = 'Archived' AND Role <> 'SuperAdmin'
                AND (@From IS NULL OR CAST(CreatedAt AS DATE) >= @From)
                AND (@To IS NULL OR CAST(CreatedAt AS DATE) <= @To)";
                    break;

                case "Roles":
                    query = "SELECT RoleID, RoleName FROM Roles";
                    break;

                case "AuditLogs":
                    query = @"
                SELECT a.LogID, u.Name AS AdminName, a.Action, a.Timestamp
                FROM AuditLogs a
                LEFT JOIN Users u ON a.AdminID = u.UserID
                WHERE (@From IS NULL OR a.Timestamp >= @From)
                AND (@To IS NULL OR a.Timestamp <= @To)
                ORDER BY a.Timestamp DESC";
                    break;

                case "SystemChanges":
                    query = @"
                SELECT SettingID, SettingName, SettingValue, UpdatedAt
                FROM SystemSettings
                WHERE (@From IS NULL OR UpdatedAt >= @From)
                AND (@To IS NULL OR UpdatedAt <= @To)
                ORDER BY UpdatedAt DESC";
                    break;

                default:
                    lblMessage.Text = "Invalid module selected.";
                    return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (query.Contains("@From"))
                    cmd.Parameters.AddWithValue("@From", hasFrom ? (object)fromDate : DBNull.Value);

                if (query.Contains("@To"))
                    cmd.Parameters.AddWithValue("@To", hasTo ? (object)toDate : DBNull.Value);

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                gvReports.DataSource = dt;
                gvReports.DataBind();

                lblMessage.Text = (dt.Rows.Count == 0)
                    ? "No data found for the selected module and date range."
                    : "";
            }
        }

    }
}
