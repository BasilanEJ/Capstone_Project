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
            // 🔐 Require login
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string role = Session["Role"].ToString();

            // 🔐 Deny access for SuperAdmin and Inspector
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            // 🔐 Check CanView permission for ManageEmployees
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
                        return false; // Default to deny access on error
                    }
                }
            }
        }

        private void LoadEmployees()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT EmployeeID, FullName, Email, Position, Phone, ProfileImage FROM Employees";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvEmployees.DataSource = dt;
                gvEmployees.DataBind();
            }
        }

        protected void gvEmployees_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditEmployee")
            {
                int employeeID = Convert.ToInt32(e.CommandArgument);
                Response.Redirect($"EditEmployee.aspx?EmployeeID={employeeID}");
            }
            else if (e.CommandName == "DeleteEmployee")
            {
                int employeeID = Convert.ToInt32(e.CommandArgument);
                ArchiveEmployee(employeeID);
            }
        }

        private void ArchiveEmployee(int employeeID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string archiveQuery = @"
                    INSERT INTO EmployeesArchive (EmployeeID, FullName, Email, Phone, Position, Status, ProfileImage, ArchivedAt)
                    SELECT EmployeeID, FullName, Email, Phone, Position, 'Archived', ProfileImage, GETDATE()
                    FROM Employees
                    WHERE EmployeeID = @EmployeeID;

                    DELETE FROM Employees WHERE EmployeeID = @EmployeeID;
                ";

                using (SqlCommand cmd = new SqlCommand(archiveQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@EmployeeID", employeeID);

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        LoadEmployees(); // Refresh the grid after archiving
                    }
                    catch (Exception ex)
                    {
                        // Optional: handle/log the error
                    }
                }
            }
        }
    }
}