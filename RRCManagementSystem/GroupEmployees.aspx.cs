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
                ddlExistingTeams.Items.Insert(0, new ListItem("Select an existing team", ""));
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

                ddlAction.DataSource = dt;
                ddlAction.DataValueField = "TeamID";
                ddlAction.DataTextField = "GroupName";
                ddlAction.DataBind();
                ddlAction.Items.Insert(0, new ListItem("No Action", ""));
                ddlAction.Items.Add(new ListItem("Remove from team", "REMOVE"));
            }
        }

        protected void btnSaveChanges_Click(object sender, EventArgs e)
        {
            // Use UserID, not AdminID (to match your other pages)
            int adminId = Convert.ToInt32(Session["UserID"]);
            int changesCount = 0;
            var feedback = new StringBuilder();

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
                        feedback.AppendLine($"✅ Removed {employeeName} from their team.<br/>");
                        AddAudit(adminId, $"Removed technician '{employeeName}' (ID: {employeeId}) from their team.");
                        changesCount++;
                    }
                }
                else if (!string.IsNullOrEmpty(selected))
                {
                    int newTeamId = int.Parse(selected);
                    if (UpsertEmployeeTeam(employeeId, newTeamId))
                    {
                        feedback.AppendLine($"✅ {employeeName} reassigned to selected team.<br/>");
                        AddAudit(adminId, $"Assigned technician '{employeeName}' (ID: {employeeId}) to TeamID: {newTeamId}.");
                        changesCount++;
                    }
                }
                else if (!string.IsNullOrEmpty(ddlExistingTeams.SelectedValue))
                {
                    int fallbackTeamId = int.Parse(ddlExistingTeams.SelectedValue);
                    if (UpsertEmployeeTeam(employeeId, fallbackTeamId))
                    {
                        feedback.AppendLine($"✅ {employeeName} assigned to team from top dropdown.<br/>");
                        AddAudit(adminId, $"Assigned technician '{employeeName}' (ID: {employeeId}) to TeamID: {fallbackTeamId}.");
                        changesCount++;
                    }
                }
            }

            lblMessage.Text = changesCount > 0
                ? $"✅ {changesCount} changes successfully saved!<br/>{feedback}"
                : "⚠️ No changes made.";
            lblMessage.ForeColor = changesCount > 0 ? System.Drawing.Color.Green : System.Drawing.Color.OrangeRed;

            LoadTechnicians();
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
            int adminId = Convert.ToInt32(Session["UserID"]);

            if (string.IsNullOrEmpty(teamName))
            {
                lblMessage.Text = "⚠️ Please enter a team name.";
                lblMessage.ForeColor = System.Drawing.Color.OrangeRed;
                return;
            }

            try
            {
                int teamId = CreateTeam(teamName);
                if (teamId == 0)
                {
                    lblMessage.Text = $"⚠️ Team '{teamName}' already exists.";
                    lblMessage.ForeColor = System.Drawing.Color.OrangeRed;
                }
                else
                {
                    lblMessage.Text = $"✅ Team '{teamName}' created successfully.";
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                    txtModalTeamName.Text = "";
                    LoadExistingTeams();
                    AddAudit(adminId, $"Created team '{teamName}' (ID: {teamId}).");
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"❌ Error creating team: {ex.Message}";
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        private int CreateTeam(string name)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spTeam_Create", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@GroupName", SqlDbType.NVarChar, 50).Value = name;

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
