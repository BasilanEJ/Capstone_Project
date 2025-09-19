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
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

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

            // 🔐 View permission check for ManageEmployees module (via SP)
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

        private bool HasViewPermission(int userId, string moduleName)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAdminPermission_Check", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@ModuleName", moduleName);
                    cmd.Parameters.AddWithValue("@Permission", "CanView");

                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null && result != DBNull.Value && Convert.ToBoolean(result);
                }
            }
            catch
            {
                return false;
            }
        }

        protected void btnFilterDate_Click(object sender, EventArgs e)
        {
            if (DateTime.TryParse(txtDate.Text, out DateTime selectedDate))
            {
                LoadTeamsAndMembers(selectedDate.Date);
            }
            else
            {
                ShowMessage("⚠️ Please select a valid date!", "text-red-600");
            }
        }

        private void LoadTeamsAndMembers(DateTime targetDate)
        {
            try
            {
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.sp_ViewTeams_LoadForDate", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ScheduledDate", SqlDbType.Date).Value = targetDate.Date;

                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        // RS1: Teams
                        var dtTeams = new DataTable();
                        dtTeams.Load(reader);

                        // If no teams are found, display a message and stop.
                        if (dtTeams.Rows.Count == 0)
                        {
                            rptTeams.DataSource = null;
                            rptTeams.DataBind();
                            ShowMessage("No teams found for the selected date.", "text-gray-500");
                            return;
                        }

                        // RS2: Members
                        var dtMembers = new DataTable();
                        dtMembers.Load(reader);

                        // RS3: Assigned counts
                        var dtAssigned = new DataTable();
                        dtAssigned.Load(reader);

                        var assignedCounts = dtAssigned.AsEnumerable()
                            .ToDictionary(r => r.Field<int>("TeamID"),
                                            r => r.Field<int>("AssignmentsCount"));

                        // Project: add derived "Status" and attach members table
                        var teamsWithMembers = dtTeams.AsEnumerable()
                            .Select(team => new
                            {
                                TeamID = team.Field<int>("TeamID"),
                                GroupName = team.Field<string>("GroupName"),
                                Status = (assignedCounts.ContainsKey(team.Field<int>("TeamID")) &&
                                          assignedCounts[team.Field<int>("TeamID")] >= 2)
                                             ? "Unavailable"
                                             : "Available",
                                Employees = dtMembers.AsEnumerable()
                                    .Where(m => m.Field<int>("TeamID") == team.Field<int>("TeamID"))
                                    .CopyToDataTableOrNull() // Bind to a DataTable
                            })
                            .ToList();

                        rptTeams.DataSource = teamsWithMembers;
                        rptTeams.DataBind();

                        ShowMessage($"✅ Teams loaded for {targetDate:yyyy-MM-dd}.", "text-green-600");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"❌ Error loading teams: {ex.Message}", "text-red-600");
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

        private void ShowMessage(string message, string cssClass)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = $"block text-center text-xl font-bold mt-8 {cssClass}";
            lblMessage.Visible = true;
        }
    }

    // Helper to safely turn an IEnumerable<DataRow> into a DataTable
    public static class DataTableExtensions
    {
        public static DataTable CopyToDataTableOrNull(this IEnumerable<DataRow> rows)
        {
            if (rows != null && rows.Any())
                return rows.CopyToDataTable();
            return new DataTable();
        }
    }
}