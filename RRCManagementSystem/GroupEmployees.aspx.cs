using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class GroupEmployees : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string role = Session["Role"].ToString();
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            if (!HasEditPermission(userId, "ManageEmployees"))
            {
                lblMessage.Text = "❌ You do not have permission to manage team assignments.";
                lblMessage.CssClass = "alert alert-danger alert-message auto-fade";
                btnSaveChanges.Enabled = false;
                gvTechnicians.Enabled = false;
                ddlExistingTeams.Enabled = false;
                btnOpenModal.Enabled = false;
                return;
            }

            if (!IsPostBack)
            {
                LoadExistingTeams();
                LoadTechnicians();
                LoadTeamLeaders(); // Load Head Technicians

                // Set default to Morning Shift
                rbMorningShift.Checked = true;
            }
        }

        private bool HasEditPermission(int userId, string module)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spAdminPermission_Check", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@ModuleName", SqlDbType.NVarChar, 100).Value = module;
                cmd.Parameters.Add("@Permission", SqlDbType.NVarChar, 10).Value = "CanEdit";

                con.Open();
                object result = cmd.ExecuteScalar();
                return result != null && Convert.ToBoolean(result);
            }
        }

        private void LoadExistingTeams()
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spTeams_ListAll", con))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                var dt = new DataTable();
                da.Fill(dt);

                ddlExistingTeams.DataSource = dt;
                ddlExistingTeams.DataValueField = "TeamID";
                ddlExistingTeams.DataTextField = "GroupName";
                ddlExistingTeams.DataBind();
                ddlExistingTeams.Items.Insert(0, new ListItem("-- Select a team to assign selected technicians --", ""));
            }
        }

        private void LoadTechnicians()
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spTechnicians_WithCurrentTeam", con))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                var dt = new DataTable();
                da.Fill(dt);

                gvTechnicians.DataSource = dt;
                gvTechnicians.DataBind();
            }
        }

        private void LoadTeamLeaders()
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("SELECT UserID, Name FROM dbo.Users WHERE Role = 'Headtechnician' AND Status = 'Active' ORDER BY Name", con))
            using (var da = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                da.Fill(dt);

                ddlTeamLeader.DataSource = dt;
                ddlTeamLeader.DataValueField = "UserID";
                ddlTeamLeader.DataTextField = "Name";
                ddlTeamLeader.DataBind();
                ddlTeamLeader.Items.Insert(0, new ListItem("-- Select a Head Technician --", ""));
            }
        }

        protected void gvTechnicians_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            var ddlAction = (DropDownList)e.Row.FindControl("ddlAction");
            if (ddlAction == null) return;

            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spTeams_ListAll", con))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                var dt = new DataTable();
                da.Fill(dt);

                // Add teams to dropdown
                foreach (DataRow row in dt.Rows)
                {
                    ddlAction.Items.Add(new ListItem(row["GroupName"].ToString(), row["TeamID"].ToString()));
                }

                // Insert default options
                ddlAction.Items.Insert(0, new ListItem("-- No Action --", ""));
                ddlAction.Items.Add(new ListItem("❌ Remove from team", "REMOVE"));
            }
        }

        protected void btnSaveChanges_Click(object sender, EventArgs e)
        {
            int adminId = Convert.ToInt32(Session["UserID"]);
            int changesCount = 0;
            var feedback = new StringBuilder();

            // Validate that at least one technician is selected
            bool anySelected = false;
            foreach (GridViewRow row in gvTechnicians.Rows)
            {
                var cbSelect = (CheckBox)row.FindControl("chkSelect");
                if (cbSelect?.Checked == true)
                {
                    anySelected = true;
                    break;
                }
            }

            if (!anySelected)
            {
                lblMessage.Text = "⚠️ Please select at least one technician.";
                lblMessage.CssClass = "alert alert-warning alert-message auto-fade";
                return;
            }

            foreach (GridViewRow row in gvTechnicians.Rows)
            {
                var cbSelect = (CheckBox)row.FindControl("chkSelect");
                var ddlAction = (DropDownList)row.FindControl("ddlAction");
                var hfEmployeeName = (HiddenField)row.FindControl("hfEmployeeName");

                if (cbSelect == null || !cbSelect.Checked) continue;

                int employeeId = Convert.ToInt32(gvTechnicians.DataKeys[row.RowIndex].Value);
                string employeeName = hfEmployeeName?.Value ?? $"Emp#{employeeId}";
                string selected = ddlAction?.SelectedValue;

                if (selected == "REMOVE")
                {
                    if (RemoveEmployeeFromTeam(employeeId))
                    {
                        feedback.AppendLine($"• Removed {employeeName} from their team.");
                        AddAudit(adminId, $"Removed technician '{employeeName}' (ID: {employeeId}) from their team.");
                        changesCount++;
                    }
                }
                else if (!string.IsNullOrEmpty(selected))
                {
                    int newTeamId = int.Parse(selected);
                    if (UpsertEmployeeTeam(employeeId, newTeamId))
                    {
                        string teamName = ddlAction.SelectedItem.Text;
                        feedback.AppendLine($"• {employeeName} assigned to {teamName}.");
                        AddAudit(adminId, $"Assigned technician '{employeeName}' (ID: {employeeId}) to team '{teamName}' (TeamID: {newTeamId}).");
                        changesCount++;
                    }
                }
                else if (!string.IsNullOrEmpty(ddlExistingTeams.SelectedValue))
                {
                    int fallbackTeamId = int.Parse(ddlExistingTeams.SelectedValue);
                    string fallbackTeamName = ddlExistingTeams.SelectedItem.Text;
                    if (UpsertEmployeeTeam(employeeId, fallbackTeamId))
                    {
                        feedback.AppendLine($"• {employeeName} assigned to {fallbackTeamName}.");
                        AddAudit(adminId, $"Assigned technician '{employeeName}' (ID: {employeeId}) to team '{fallbackTeamName}' (TeamID: {fallbackTeamId}).");
                        changesCount++;
                    }
                }
            }

            if (changesCount > 0)
            {
                // Build HTML for feedback
                string feedbackHtml = feedback.ToString().Replace(Environment.NewLine, "<br/>");

                // Show SweetAlert instead of inline message
                string script = $@"
                    Swal.fire({{
                        icon: 'success',
                        title: '<strong>{changesCount} Changes Saved!</strong>',
                        html: '{feedbackHtml.Replace("'", "\\'")}',
                        confirmButtonColor: '#10b981',
                        confirmButtonText: 'OK',
                        customClass: {{
                            popup: 'swal-wide'
                        }}
                    }});
                ";
                System.Web.UI.ScriptManager.RegisterStartupScript(this, GetType(), "SaveSuccess", script, true);

                LoadTechnicians();
            }
            else
            {
                lblMessage.Text = "<i class='fas fa-exclamation-triangle'></i> No changes made.";
                lblMessage.CssClass = "alert alert-warning alert-message auto-fade";
            }
        }

        private bool UpsertEmployeeTeam(int employeeId, int teamId)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spTeamMember_Upsert", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = employeeId;
                cmd.Parameters.Add("@TeamID", SqlDbType.Int).Value = teamId;

                con.Open();
                object result = cmd.ExecuteScalar();
                return Convert.ToInt32(result ?? 0) > 0;
            }
        }

        private bool RemoveEmployeeFromTeam(int employeeId)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spTeamMember_Remove", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = employeeId;

                con.Open();
                object result = cmd.ExecuteScalar();
                return Convert.ToInt32(result ?? 0) > 0;
            }
        }

        protected void btnCreateTeamModal_Click(object sender, EventArgs e)
        {
            string teamName = txtModalTeamName.Text.Trim();
            string shiftType = rbMorningShift.Checked ? "MorningShift" : (rbNightShift.Checked ? "NightShift" : "");
            string teamLeaderIdStr = ddlTeamLeader.SelectedValue;
            int adminId = Convert.ToInt32(Session["UserID"]);

            // Validation
            if (string.IsNullOrEmpty(teamName))
            {
                lblMessage.Text = "<i class='fas fa-exclamation-triangle'></i> Please enter a team name.";
                lblMessage.CssClass = "alert alert-warning alert-message auto-fade";
                return;
            }

            if (string.IsNullOrEmpty(shiftType))
            {
                lblMessage.Text = "<i class='fas fa-exclamation-triangle'></i> Please select a shift type.";
                lblMessage.CssClass = "alert alert-warning alert-message auto-fade";
                return;
            }

            if (string.IsNullOrEmpty(teamLeaderIdStr))
            {
                lblMessage.Text = "<i class='fas fa-exclamation-triangle'></i> Please select a team leader.";
                lblMessage.CssClass = "alert alert-warning alert-message auto-fade";
                return;
            }

            int teamLeaderId = int.Parse(teamLeaderIdStr);

            try
            {
                int teamId = CreateTeam(teamName, shiftType, teamLeaderId);
                if (teamId == 0)
                {
                    lblMessage.Text = $"<i class='fas fa-exclamation-circle'></i> Team '<strong>{teamName}</strong>' already exists.";
                    lblMessage.CssClass = "alert alert-warning alert-message auto-fade";
                }
                else
                {
                    string teamLeaderName = ddlTeamLeader.SelectedItem.Text;

                    // Clear form
                    txtModalTeamName.Text = "";
                    rbMorningShift.Checked = true;
                    rbNightShift.Checked = false;
                    ddlTeamLeader.SelectedIndex = 0;

                    // Reload dropdowns and gridview
                    LoadExistingTeams();
                    LoadTechnicians();

                    // Add audit
                    AddAudit(adminId, $"Created team '{teamName}' (ID: {teamId}) with shift type '{shiftType}' and team leader '{teamLeaderName}' (UserID: {teamLeaderId}).");

                    // Close modal and show centered SweetAlert
                    string script = $@"
                        if(createTeamModal) createTeamModal.hide();
                        Swal.fire({{
                            icon: 'success',
                            title: '<strong>Team Created!</strong>',
                            html: 'Team <strong>{teamName}</strong> has been created successfully with <strong>{teamLeaderName}</strong> as team leader.',
                            confirmButtonColor: '#10b981',
                            confirmButtonText: 'OK',
                            timer: 5000,
                            timerProgressBar: true
                        }});
                    ";
                    System.Web.UI.ScriptManager.RegisterStartupScript(this, GetType(), "CloseModal", script, true);
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"<i class='fas fa-times-circle'></i> Error creating team: {ex.Message}";
                lblMessage.CssClass = "alert alert-danger alert-message auto-fade";
            }
        }

        private int CreateTeam(string name, string shiftType, int teamLeaderId)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spTeam_Create", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@GroupName", SqlDbType.NVarChar, 50).Value = name;
                cmd.Parameters.Add("@ShiftType", SqlDbType.NVarChar, 20).Value = shiftType;
                cmd.Parameters.Add("@TeamLeaderID", SqlDbType.Int).Value = teamLeaderId;

                con.Open();
                object result = cmd.ExecuteScalar();
                return Convert.ToInt32(result ?? 0);
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
    }
}