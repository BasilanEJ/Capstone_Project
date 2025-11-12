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
                Response.Redirect("~/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            string role = Session["Role"].ToString();

            // 🔐 Deny SuperAdmin and Inspector
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            // 🔐 View permission check for ManageEmployees module (via SP)
            if (!HasViewPermission(userId, "ManageEmployees"))
            {
                Response.Redirect("~/Unauthorized.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                txtDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
                LoadTeamLeaders(); // Load team leaders for edit modal
                LoadTeamsAndMembers(DateTime.Today, "");
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

        private void LoadTeamLeaders()
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"
        SELECT UserID, Name 
        FROM dbo.Users 
        WHERE Role = 'Headtechnician' 
          AND Status IN ('Active', 'Available')
        ORDER BY Name", con))
            using (var da = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                da.Fill(dt);

                ddlEditTeamLeader.DataSource = dt;
                ddlEditTeamLeader.DataValueField = "UserID";
                ddlEditTeamLeader.DataTextField = "Name";
                ddlEditTeamLeader.DataBind();
                ddlEditTeamLeader.Items.Insert(0, new ListItem("-- Select a Head Technician --", ""));
            }
        }


        protected void btnFilterDate_Click(object sender, EventArgs e)
        {
            if (DateTime.TryParse(txtDate.Text, out DateTime selectedDate))
            {
                string shiftFilter = ddlShiftFilter.SelectedValue;
                LoadTeamsAndMembers(selectedDate.Date, shiftFilter);
            }
            else
            {
                ShowSweetAlert("Please select a valid date!", "warning");
            }
        }

        protected void btnEditTeam_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            string[] args = button.CommandArgument.Split(',');

            if (args.Length >= 4)
            {
                hdnEditTeamID.Value = args[0]; // TeamID
                txtEditTeamName.Text = args[1]; // GroupName
                string shiftType = args[2]; // ShiftType
                string teamLeaderID = args[3]; // TeamLeaderID

                // Set shift type radio buttons
                if (shiftType == "MorningShift")
                {
                    rbEditMorning.Checked = true;
                    rbEditNight.Checked = false;
                }
                else
                {
                    rbEditMorning.Checked = false;
                    rbEditNight.Checked = true;
                }

                // Set team leader dropdown
                LoadTeamLeaders(); // Reload to ensure data is fresh
                if (ddlEditTeamLeader.Items.FindByValue(teamLeaderID) != null)
                {
                    ddlEditTeamLeader.SelectedValue = teamLeaderID;
                }

                // ✅ BEST FIX: Show modal after page loads
                string script = @"
                    window.addEventListener('load', function() {
                        var modalEl = document.getElementById('editTeamModal');
                        if (modalEl) {
                            var modal = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
                            modal.show();
                        }
                    });";

                ClientScript.RegisterStartupScript(this.GetType(), "ShowEditModal", script, true);
            }
        }

        protected void btnSaveEdit_Click(object sender, EventArgs e)
        {
            int teamId = Convert.ToInt32(hdnEditTeamID.Value);
            string teamName = txtEditTeamName.Text.Trim();
            string shiftType = rbEditMorning.Checked ? "MorningShift" : "NightShift";
            int teamLeaderId = Convert.ToInt32(ddlEditTeamLeader.SelectedValue);
            int adminId = Convert.ToInt32(Session["UserID"]);

            try
            {
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spTeam_Update", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@TeamID", SqlDbType.Int).Value = teamId;
                    cmd.Parameters.Add("@GroupName", SqlDbType.NVarChar, 50).Value = teamName;
                    cmd.Parameters.Add("@ShiftType", SqlDbType.NVarChar, 20).Value = shiftType;
                    cmd.Parameters.Add("@TeamLeaderID", SqlDbType.Int).Value = teamLeaderId;

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                // Add audit log
                string teamLeaderName = ddlEditTeamLeader.SelectedItem.Text;
                AddAudit(adminId, $"Updated team '{teamName}' (ID: {teamId}) - Shift: {shiftType}, Leader: {teamLeaderName} (ID: {teamLeaderId})");

                // Reload the page
                if (DateTime.TryParse(txtDate.Text, out DateTime selectedDate))
                {
                    LoadTeamsAndMembers(selectedDate.Date, ddlShiftFilter.SelectedValue);
                }

                // Show success message
                ShowSweetAlert("Team updated successfully!", "success");
            }
            catch (Exception ex)
            {
                ShowSweetAlert($"Error updating team: {ex.Message}", "error");
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
                        int adminId = Convert.ToInt32(Session["UserID"]);
                        AddAudit(adminId, $"Deleted empty team (ID: {teamId})");
                        ShowSweetAlert($"Team has been successfully deleted!", "success");
                    }
                    else
                    {
                        ShowSweetAlert($"Could not delete team. It might have members or active bookings.", "warning");
                    }
                }

                // Reload the list
                if (DateTime.TryParse(txtDate.Text, out DateTime selectedDate))
                {
                    LoadTeamsAndMembers(selectedDate.Date, ddlShiftFilter.SelectedValue);
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

        private void LoadTeamsAndMembers(DateTime targetDate, string shiftFilter)
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
                        cmd.Parameters.Add("@ShiftType", SqlDbType.NVarChar, 20).Value = string.IsNullOrEmpty(shiftFilter) ? (object)DBNull.Value : shiftFilter;

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
                    ShowSweetAlert("No teams found for the selected date and filter.", "info");
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
                            ShiftType = team.Field<string>("ShiftType") ?? "MorningShift",
                            TeamLeaderID = team.Field<int?>("TeamLeaderID") ?? 0,
                            TeamLeaderName = team.Field<string>("TeamLeaderName") ?? "Not Assigned",
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

        private void AddAudit(int adminId, string action)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spAudit_Insert", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@AdminID", SqlDbType.Int).Value = adminId;
                cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 255).Value = action;

                con.Open();
                cmd.ExecuteNonQuery();
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

            ClientScript.RegisterStartupScript(this.GetType(), "SweetAlert", script, true);
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