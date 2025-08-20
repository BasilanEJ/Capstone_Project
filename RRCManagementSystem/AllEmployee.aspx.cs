using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class AllEmployee : Page
    {
        private readonly string connectionString =
            System.Configuration.ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

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
            if (!HasViewPermission(userId, "ManageEmployees"))
            {
                Response.Redirect("~/Unauthorized.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadEmployees();
            }
        }

        private bool HasViewPermission(int adminId, string moduleName)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spAdminPermission_CanView", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = adminId;
                cmd.Parameters.Add("@ModuleName", SqlDbType.NVarChar, 100).Value = moduleName;

                conn.Open();
                object result = cmd.ExecuteScalar();
                return result != null && Convert.ToBoolean(result);
            }
        }

        private void LoadEmployees()
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spEmployees_ListActive", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                var dt = new DataTable();
                da.Fill(dt);

                gvEmployees.DataSource = dt;
                gvEmployees.DataBind();
            }
        }

        protected void gvEmployees_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvEmployees.PageIndex = e.NewPageIndex;
            LoadEmployees();
        }

        protected void gvEmployees_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditEmployee")
            {
                int id = Convert.ToInt32(e.CommandArgument);
                Response.Redirect("EditEmployee.aspx?EmployeeID=" + id);
            }
        }

        // Optional: keep this if you still want a pre-check (not required if you call ArchiveIfNoTeam)
        private bool IsEmployeeOnAnyTeam(int employeeId)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spTeamMembers_CountByEmployee", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = employeeId;

                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        protected void btnHiddenArchive_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(hfEmployeeToArchive.Value, out int id))
                return;

            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spEmployee_ArchiveIfNoTeam", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = id;

                    conn.Open();
                    object result = cmd.ExecuteScalar(); // RowsAffected
                    int rows = Convert.ToInt32(result ?? 0);

                    if (rows > 0)
                    {
                        // go to archive list
                        Response.Redirect("ArchiveEmployee.aspx?archived=1", false);
                        Context.ApplicationInstance.CompleteRequest();
                        return;
                    }

                    // not found or already inactive
                    ClientScript.RegisterStartupScript(this.GetType(), "archiveWarn",
                        "Swal.fire({icon:'warning',title:'Not Found',text:'Employee not found or already inactive.'});", true);
                }
            }
            catch (SqlException sqlEx) when (sqlEx.Message.Contains("EMPLOYEE_HAS_TEAM"))
            {
                ClientScript.RegisterStartupScript(this.GetType(), "archiveBlocked", @"
Swal.fire({
  icon: 'error',
  title: 'Cannot Archive',
  text: 'This employee is still assigned to a team. Remove them from the team first.',
  confirmButtonText: 'OK'
});", true);
            }
            catch (Exception ex)
            {
                string err = $@"Swal.fire({{
  icon: 'error',
  title: 'Error',
  text: 'Failed to archive employee: {ex.Message.Replace("'", "\\'")}',
  confirmButtonText: 'OK'
}});";
                ClientScript.RegisterStartupScript(this.GetType(), "archiveError", err, true);
            }
        }
    }
}
