using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class ViewTeams : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 🔐 Require login
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string role = Session["Role"].ToString();

            // 🔐 Deny SuperAdmin and Inspector
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            // 🔐 View permission check for ManageEmployees module
            if (!HasViewPermission(userId, "ManageEmployees"))
            {
                Response.Redirect("~/Unauthorized.aspx");
                return;
            }

            if (!IsPostBack)
            {
                txtDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
                LoadTeamsAndMembers(DateTime.Today);
            }
        }


        private bool HasViewPermission(int adminId, string moduleName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT CanView FROM AdminPermissions WHERE UserID = @UserID AND ModuleName = @ModuleName";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", adminId);
                    cmd.Parameters.AddWithValue("@ModuleName", moduleName);

                    try
                    {
                        conn.Open();
                        object result = cmd.ExecuteScalar();
                        return result != null && result != DBNull.Value && Convert.ToBoolean(result);
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        }

        protected void btnFilterDate_Click(object sender, EventArgs e)
        {
            if (DateTime.TryParse(txtDate.Text, out DateTime selectedDate))
            {
                LoadTeamsAndMembers(selectedDate);
            }
            else
            {
                lblMessage.Text = "⚠️ Please select a valid date!";
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void LoadTeamsAndMembers(DateTime targetDate)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string queryTeams = "SELECT TeamID, GroupName FROM Teams ORDER BY GroupName";
                    SqlDataAdapter daTeams = new SqlDataAdapter(queryTeams, con);
                    DataTable dtTeams = new DataTable();
                    daTeams.Fill(dtTeams);

                    string queryMembers = @"
    SELECT tm.TeamID, e.EmployeeID, e.LastName, e.FirstName, e.MiddleName, e.Department
    FROM TeamMembers tm
    INNER JOIN Employees e ON tm.EmployeeID = e.EmployeeID";

                    SqlDataAdapter daMembers = new SqlDataAdapter(queryMembers, con);
                    DataTable dtMembers = new DataTable();
                    daMembers.Fill(dtMembers);

                    // 🔵 CHANGE: Count how many times each team is assigned on that date
                    string queryAssignedTeams = @"
                SELECT bt.TeamID, COUNT(*) AS AssignmentsCount
                FROM BookingTeams bt
                INNER JOIN Bookings b ON bt.BookingID = b.BookingID
                WHERE CAST(b.ScheduledDate AS DATE) = @ScheduledDate
                GROUP BY bt.TeamID";
                    SqlCommand cmdAssignedTeams = new SqlCommand(queryAssignedTeams, con);
                    cmdAssignedTeams.Parameters.AddWithValue("@ScheduledDate", targetDate);

                    SqlDataAdapter daAssignedTeams = new SqlDataAdapter(cmdAssignedTeams);
                    DataTable dtAssignedTeams = new DataTable();
                    daAssignedTeams.Fill(dtAssignedTeams);

                    var assignedTeamsCount = dtAssignedTeams.AsEnumerable()
                        .ToDictionary(
                            r => r.Field<int>("TeamID"),
                            r => r.Field<int>("AssignmentsCount")
                        );

                    var teamsWithMembers = dtTeams.AsEnumerable().Select(team => new
                    {
                        TeamID = team.Field<int>("TeamID"),
                        GroupName = team.Field<string>("GroupName"),
                        Status = assignedTeamsCount.ContainsKey(team.Field<int>("TeamID")) && assignedTeamsCount[team.Field<int>("TeamID")] >= 2
                            ? "Unavailable" : "Available",
                        Employees = dtMembers.AsEnumerable()
                            .Where(m => m.Field<int>("TeamID") == team.Field<int>("TeamID"))
                            .CopyToDataTableOrNull()
                    }).ToList();

                    rptTeams.DataSource = teamsWithMembers;
                    rptTeams.DataBind();

                    lblMessage.Text = $"✅ Teams loaded for {targetDate:yyyy-MM-dd}.";
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"❌ Error loading teams: {ex.Message}";
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }


        protected void rptTeams_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var rptEmployees = (Repeater)e.Item.FindControl("rptEmployees");
                var phNoMembers = (PlaceHolder)e.Item.FindControl("phNoMembers");

                var employees = DataBinder.Eval(e.Item.DataItem, "Employees") as DataTable;

                if (employees != null && employees.Rows.Count > 0)
                {
                    rptEmployees.DataSource = employees;
                    rptEmployees.DataBind();
                    phNoMembers.Visible = false;
                }
                else
                {
                    phNoMembers.Visible = true;
                }
            }
        }
    }

    // ✅ Extension to safely handle empty DataTable generation
    public static class Extensions
    {
        public static DataTable CopyToDataTableOrNull(this IEnumerable<DataRow> rows)
        {
            if (rows != null && rows.Any())
            {
                return rows.CopyToDataTable();
            }
            return new DataTable();
        }
    }
}