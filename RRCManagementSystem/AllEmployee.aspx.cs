using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class AllEmployee : Page
    {
        private readonly string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

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

        private void LoadEmployees()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        EmployeeID, 
                        LastName, 
                        FirstName, 
                        MiddleName, 
                        Email, 
                        Position, 
                        Phone, 
                        ProfileImage 
                    FROM Employees
                    WHERE Status = 'Active'";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
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
                Response.Redirect("EditEmployee.aspx?EmployeeID=" + id); // <— changed "id" to "EmployeeID"
            }
        }



        private bool IsEmployeeOnAnyTeam(int employeeId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT COUNT(*) FROM TeamMembers WHERE EmployeeID = @EmployeeID", conn))
            {
                cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
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
                if (IsEmployeeOnAnyTeam(id))
                {
                    ClientScript.RegisterStartupScript(this.GetType(), "archiveBlocked", @"
                Swal.fire({ icon: 'error', title: 'Cannot Archive',
                            text: 'This employee is still assigned to a team. Please remove them from the team first.',
                            confirmButtonText: 'OK' });", true);
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE Employees SET Status = 'Inactive' WHERE EmployeeID = @EmployeeID", conn))
                {
                    cmd.Parameters.AddWithValue("@EmployeeID", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                // ✅ Go straight to Archive page so you see the record there
                Response.Redirect("ArchiveEmployee.aspx?archived=1", false);
                Context.ApplicationInstance.CompleteRequest();
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
