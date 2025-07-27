using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class GroupEmployees : System.Web.UI.Page
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

            // 🔐 Check Edit permission for ManageEmployees
            if (!HasEditPermission(userId, "ManageEmployees"))
            {
                lblMessage.Text = "❌ You do not have permission to manage team assignments.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                btnSaveChanges.Enabled = false;
                gvTechnicians.Enabled = false;
                ddlExistingTeams.Enabled = false;
                return;
            }

            if (!IsPostBack)
            {
                LoadExistingTeams();
                LoadTechnicians();
            }
        }


        private bool HasEditPermission(int adminId, string moduleName)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT CanEdit FROM AdminPermissions WHERE UserID = @UserID AND ModuleName = @ModuleName";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserID", adminId);
                    cmd.Parameters.AddWithValue("@ModuleName", moduleName);

                    try
                    {
                        con.Open();
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

        private void LoadExistingTeams()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT TeamID, GroupName FROM Teams ORDER BY GroupName";

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dtTeams = new DataTable();
                da.Fill(dtTeams);

                ddlExistingTeams.DataSource = dtTeams;
                ddlExistingTeams.DataValueField = "TeamID";
                ddlExistingTeams.DataTextField = "GroupName";
                ddlExistingTeams.DataBind();

                ddlExistingTeams.Items.Insert(0, new ListItem("Select an existing team", ""));
            }
        }

        private void LoadTechnicians()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT e.EmployeeID, e.FullName, 
                           ISNULL(t.GroupName, 'Not Assigned') AS CurrentTeam
                    FROM Employees e
                    LEFT JOIN TeamMembers tm ON e.EmployeeID = tm.EmployeeID
                    LEFT JOIN Teams t ON tm.TeamID = t.TeamID
                    WHERE e.Position = 'Technician'
                    ORDER BY e.FullName";

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvTechnicians.DataSource = dt;
                gvTechnicians.DataBind();
            }
        }

        protected void gvTechnicians_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DropDownList ddlAction = (DropDownList)e.Row.FindControl("ddlAction");

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT TeamID, GroupName FROM Teams ORDER BY GroupName";
                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    ddlAction.DataSource = dt;
                    ddlAction.DataValueField = "TeamID";
                    ddlAction.DataTextField = "GroupName";
                    ddlAction.DataBind();

                    ddlAction.Items.Insert(0, new ListItem("No Action", ""));
                    ddlAction.Items.Add(new ListItem("Remove from team", "REMOVE"));
                }
            }
        }

        protected void btnSaveChanges_Click(object sender, EventArgs e)
        {
            int adminId = Convert.ToInt32(Session["AdminID"]);
            int changesCount = 0;
            StringBuilder feedback = new StringBuilder();

            foreach (GridViewRow row in gvTechnicians.Rows)
            {
                CheckBox cbSelect = (CheckBox)row.FindControl("chkSelect");
                DropDownList ddlAction = (DropDownList)row.FindControl("ddlAction");
                HiddenField hfEmployeeName = (HiddenField)row.FindControl("hfEmployeeName");

                int employeeId = int.Parse(gvTechnicians.DataKeys[row.RowIndex].Value.ToString());
                string employeeName = hfEmployeeName?.Value;
                string selectedAction = ddlAction?.SelectedValue;

                // 🟢 Proceed only if checkbox is checked
                if (cbSelect != null && cbSelect.Checked)
                {
                    // Case 1: Remove from team
                    if (selectedAction == "REMOVE")
                    {
                        if (RemoveEmployeeFromTeam(employeeId))
                        {
                            feedback.AppendLine($"✅ Removed {employeeName} from their team.<br/>");
                            AddAuditLog(adminId, $"Removed technician '{employeeName}' (ID: {employeeId}) from their team.");
                            changesCount++;
                        }
                    }
                    // Case 2: Assign to team via row action dropdown
                    else if (!string.IsNullOrEmpty(selectedAction))
                    {
                        int newTeamId = int.Parse(selectedAction);
                        if (AssignOrUpdateEmployeeTeam(employeeId, newTeamId))
                        {
                            feedback.AppendLine($"✅ {employeeName} reassigned to selected team.<br/>");
                            AddAuditLog(adminId, $"Assigned technician '{employeeName}' (ID: {employeeId}) to TeamID: {newTeamId}.");
                            changesCount++;
                        }
                    }
                    // ✅ Case 3: Use top dropdown team if row action is No Action
                    else if (!string.IsNullOrEmpty(ddlExistingTeams.SelectedValue))
                    {
                        int fallbackTeamId = int.Parse(ddlExistingTeams.SelectedValue);
                        if (AssignOrUpdateEmployeeTeam(employeeId, fallbackTeamId))
                        {
                            feedback.AppendLine($"✅ {employeeName} assigned to team from top dropdown.<br/>");
                            AddAuditLog(adminId, $"Assigned technician '{employeeName}' (ID: {employeeId}) to TeamID: {fallbackTeamId}.");
                            changesCount++;
                        }
                    }
                }
            }

            lblMessage.Text = changesCount > 0
                ? $"✅ {changesCount} changes successfully saved!<br/>{feedback}"
                : "⚠️ No changes made.";
            lblMessage.ForeColor = changesCount > 0 ? System.Drawing.Color.Green : System.Drawing.Color.OrangeRed;

            LoadTechnicians();
        }

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                CheckBox chk = (CheckBox)e.Row.FindControl("chkSelect");
                if (chk != null)
                {
                    chk.InputAttributes.Add("style", "transform: scale(3); cursor: pointer;");
                }
            }
        }


        private bool AssignOrUpdateEmployeeTeam(int employeeId, int teamId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string checkExist = "SELECT COUNT(*) FROM TeamMembers WHERE EmployeeID = @EmployeeID";
                SqlCommand cmdCheck = new SqlCommand(checkExist, con);
                cmdCheck.Parameters.AddWithValue("@EmployeeID", employeeId);

                int count = (int)cmdCheck.ExecuteScalar();

                string query = count > 0
                    ? "UPDATE TeamMembers SET TeamID = @TeamID WHERE EmployeeID = @EmployeeID"
                    : "INSERT INTO TeamMembers (EmployeeID, TeamID) VALUES (@EmployeeID, @TeamID)";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
                cmd.Parameters.AddWithValue("@TeamID", teamId);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        private bool RemoveEmployeeFromTeam(int employeeId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM TeamMembers WHERE EmployeeID = @EmployeeID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@EmployeeID", employeeId);

                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        protected void btnCreateTeamModal_Click(object sender, EventArgs e)
        {
            string teamName = txtModalTeamName.Text.Trim();
            int adminId = Convert.ToInt32(Session["AdminID"]);

            if (string.IsNullOrEmpty(teamName))
            {
                lblMessage.Text = "⚠️ Please enter a team name.";
                lblMessage.ForeColor = System.Drawing.Color.OrangeRed;
                return;
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string checkQuery = "SELECT COUNT(*) FROM Teams WHERE GroupName = @GroupName";
                SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@GroupName", teamName);

                string insertQuery = "INSERT INTO Teams (GroupName) VALUES (@GroupName)";
                SqlCommand insertCmd = new SqlCommand(insertQuery, con);
                insertCmd.Parameters.AddWithValue("@GroupName", teamName);

                try
                {
                    con.Open();
                    int exists = (int)checkCmd.ExecuteScalar();

                    if (exists > 0)
                    {
                        lblMessage.Text = $"⚠️ Team '{teamName}' already exists.";
                        lblMessage.ForeColor = System.Drawing.Color.OrangeRed;
                    }
                    else
                    {
                        insertCmd.ExecuteNonQuery();
                        lblMessage.Text = $"✅ Team '{teamName}' created successfully.";
                        lblMessage.ForeColor = System.Drawing.Color.Green;

                        txtModalTeamName.Text = "";
                        LoadExistingTeams();
                    }
                }
                catch (Exception ex)
                {
                    lblMessage.Text = $"❌ Error creating team: {ex.Message}";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
            }
        }

        private void AddAuditLog(int? userID, string action)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO AuditLogs (AdminID, Action, Timestamp) VALUES (@AdminID, @Action, GETDATE())";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@AdminID", (object)userID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Action", action);

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch
                    {
                        // Silent fail
                    }
                }
            }
        }
    }
}