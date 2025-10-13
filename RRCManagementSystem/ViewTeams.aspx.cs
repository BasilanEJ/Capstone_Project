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
                ShowSweetAlert("Please select a valid date!", "warning");
            }
        }

        protected void btnDeleteTeam_Click(object sender, EventArgs e)
        {
            // Check if user confirmed the delete action
            if (hdnConfirmDelete.Value != "true")
            {
                return; // User didn't confirm, exit
            }

            // Reset the confirmation flag
            hdnConfirmDelete.Value = "false";

            // Get the team ID from the button's CommandArgument
            var deleteButton = (Button)sender;
            if (!int.TryParse(deleteButton.CommandArgument, out int teamId))
            {
                ShowSweetAlert("Invalid team ID!", "error");
                return;
            }

            try
            {
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spTeam_DeleteEmpty", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TeamID", teamId);

                    // Add output parameter to get return value
                    var returnParam = cmd.Parameters.Add("@ReturnValue", SqlDbType.Int);
                    returnParam.Direction = ParameterDirection.ReturnValue;

                    con.Open();
                    cmd.ExecuteNonQuery();

                    int returnValue = (int)returnParam.Value;

                    if (returnValue > 0)
                    {
                        ShowSweetAlert($"Team ID {teamId} has been successfully deleted!", "success");
                    }
                    else
                    {
                        ShowSweetAlert($"Could not delete Team ID {teamId}. It might have members or active bookings.", "warning");
                    }
                }

                // Reload the list
                if (DateTime.TryParse(txtDate.Text, out DateTime selectedDate))
                {
                    LoadTeamsAndMembers(selectedDate.Date);
                }
            }
            catch (SqlException ex)
            {
                ShowSweetAlert(ex.Message, "error");
            }
            catch (Exception ex)
            {
                ShowSweetAlert($"An error occurred: {ex.Message}", "error");
            }
        }

        private void LoadTeamsAndMembers(DateTime targetDate)
        {
            try
            {
                DataSet ds = new DataSet();

                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (var cmd = new SqlCommand("dbo.sp_ViewTeams_LoadForDate", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@ScheduledDate", SqlDbType.Date).Value = targetDate.Date;

                        using (var adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(ds);
                        }
                    }
                }

                // Verify we have the expected result sets
                if (ds.Tables.Count < 3)
                {
                    ShowSweetAlert($"Unexpected database response. Expected 3 result sets, got {ds.Tables.Count}.", "error");
                    return;
                }

                DataTable dtTeams = ds.Tables[0];
                DataTable dtMembers = ds.Tables[1];
                DataTable dtAssigned = ds.Tables[2];

                // Check if no teams found
                if (dtTeams.Rows.Count == 0)
                {
                    rptTeams.DataSource = null;
                    rptTeams.DataBind();
                    ShowSweetAlert("No teams found for the selected date.", "info");
                    return;
                }

                // Build assignment counts dictionary
                var assignedCounts = new Dictionary<int, int>();
                foreach (DataRow row in dtAssigned.Rows)
                {
                    assignedCounts[row.Field<int>("TeamID")] = row.Field<int>("AssignmentsCount");
                }

                // Build teams with status and members
                var teamsWithMembers = dtTeams.AsEnumerable()
                    .Select(team =>
                    {
                        int teamId = team.Field<int>("TeamID");
                        int assignmentCount = assignedCounts.ContainsKey(teamId) ? assignedCounts[teamId] : 0;

                        var teamEmployees = dtMembers.AsEnumerable()
                            .Where(m => m.Field<int>("TeamID") == teamId);

                        return new
                        {
                            TeamID = teamId,
                            GroupName = team.Field<string>("GroupName"),
                            Status = assignmentCount >= 2 ? "Unavailable" : "Available",
                            Employees = teamEmployees.Any() ? teamEmployees.CopyToDataTable() : new DataTable()
                        };
                    })
                    .ToList();

                rptTeams.DataSource = teamsWithMembers;
                rptTeams.DataBind();

                // Hide the label message when data loads successfully
                lblMessage.Visible = false;
            }
            catch (Exception ex)
            {
                ShowSweetAlert($"Error loading teams: {ex.Message}", "error");
            }
        }

        protected void rptTeams_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var rptEmployees = (Repeater)e.Item.FindControl("rptEmployees");
                var phNoMembers = (PlaceHolder)e.Item.FindControl("phNoMembers");
                var phDeleteTeam = (PlaceHolder)e.Item.FindControl("phDeleteTeam");

                var employees = DataBinder.Eval(e.Item.DataItem, "Employees") as DataTable;

                if (employees != null && employees.Rows.Count > 0)
                {
                    rptEmployees.DataSource = employees;
                    rptEmployees.DataBind();
                    phNoMembers.Visible = false;
                    phDeleteTeam.Visible = false;
                }
                else
                {
                    phNoMembers.Visible = true;
                    phDeleteTeam.Visible = true;
                }
            }
        }

        /// <summary>
        /// Shows a SweetAlert message to the user
        /// </summary>
        /// <param name="message">The message to display</param>
        /// <param name="type">The alert type: success, error, warning, info</param>
        private void ShowSweetAlert(string message, string type)
        {
            string script = "";

            switch (type.ToLower())
            {
                case "success":
                    script = $"showSuccessAlert('{EscapeJavaScript(message)}');";
                    break;
                case "error":
                    script = $"showErrorAlert('{EscapeJavaScript(message)}');";
                    break;
                case "warning":
                    script = $"showWarningAlert('{EscapeJavaScript(message)}');";
                    break;
                case "info":
                    script = $"showInfoAlert('{EscapeJavaScript(message)}');";
                    break;
                default:
                    script = $"showInfoAlert('{EscapeJavaScript(message)}');";
                    break;
            }

            ScriptManager.RegisterStartupScript(this, GetType(), "SweetAlert", script, true);
        }

        /// <summary>
        /// Escapes special characters in JavaScript strings to prevent injection
        /// </summary>
        private string EscapeJavaScript(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return text.Replace("\\", "\\\\")
                       .Replace("'", "\\'")
                       .Replace("\"", "\\\"")
                       .Replace("\n", "\\n")
                       .Replace("\r", "\\r")
                       .Replace("\t", "\\t");
        }
    }

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