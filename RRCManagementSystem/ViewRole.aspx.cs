using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class ViewRole : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadRoles();
            }

            // Handle __doPostBack from the SweetAlert confirmDelete()
            string eventTarget = Request["__EVENTTARGET"];
            if (eventTarget == "DeleteRole")
            {
                if (int.TryParse(hfRoleIDToDelete.Value, out int roleId))
                {
                    DeleteRole(roleId);
                }
                else
                {
                    ShowAlert("⚠ Invalid role selected.", "warning");
                }
            }
        }

        private void LoadRoles()
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spRoles_ListAll", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                var dt = new DataTable();
                try
                {
                    da.Fill(dt);
                    gvRoles.DataSource = dt;
                    gvRoles.DataBind();
                }
                catch (Exception ex)
                {
                    ShowAlert("❌ Error loading roles: " + ex.Message, "error");
                }
            }
        }

        private void DeleteRole(int roleId)
        {
            try
            {
                int resultCode = -99;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // ✅ Step 1: Check if the role exists
                    string checkRole = "SELECT COUNT(*) FROM Roles WHERE RoleID = @RoleID";
                    using (SqlCommand cmd = new SqlCommand(checkRole, conn))
                    {
                        cmd.Parameters.AddWithValue("@RoleID", roleId);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        if (count == 0)
                        {
                            resultCode = -1; // Role not found
                        }
                    }

                    // ✅ Step 2: Check if users are assigned to this role
                    if (resultCode != -1)
                    {
                        string checkUsers = "SELECT COUNT(*) FROM Users WHERE RoleID = @RoleID";
                        using (SqlCommand cmd = new SqlCommand(checkUsers, conn))
                        {
                            cmd.Parameters.AddWithValue("@RoleID", roleId);
                            int userCount = Convert.ToInt32(cmd.ExecuteScalar());
                            if (userCount > 0)
                            {
                                resultCode = 0; // Cannot delete (users assigned)
                            }
                        }
                    }

                    // ✅ Step 3: If safe to delete, remove permissions and role
                    if (resultCode != 0 && resultCode != -1)
                    {
                        // Delete RolePermissions first
                        string deletePermissions = "DELETE FROM RolePermissions WHERE RoleID = @RoleID";
                        using (SqlCommand cmd = new SqlCommand(deletePermissions, conn))
                        {
                            cmd.Parameters.AddWithValue("@RoleID", roleId);
                            cmd.ExecuteNonQuery();
                        }

                        // Then delete the Role
                        string deleteRole = "DELETE FROM Roles WHERE RoleID = @RoleID";
                        using (SqlCommand cmd = new SqlCommand(deleteRole, conn))
                        {
                            cmd.Parameters.AddWithValue("@RoleID", roleId);
                            cmd.ExecuteNonQuery();
                        }

                        resultCode = 1; // Success
                    }
                }

                // ✅ Step 4: Handle result messages
                switch (resultCode)
                {
                    case 1:
                        LoadRoles();
                        ScriptManager.RegisterStartupScript(this, GetType(), "deleteSuccess",
                            "Swal.fire('Deleted!', 'The role and its permissions have been successfully deleted.', 'success');", true);
                        break;

                    case 0:
                        ScriptManager.RegisterStartupScript(this, GetType(), "cannotDelete",
                            "Swal.fire('Cannot Delete', 'Users are still assigned to this role.', 'warning');", true);
                        break;

                    case -1:
                        ScriptManager.RegisterStartupScript(this, GetType(), "notFound",
                            "Swal.fire('Not Found', 'The specified role does not exist.', 'info');", true);
                        break;

                    default:
                        ScriptManager.RegisterStartupScript(this, GetType(), "unknownError",
                            "Swal.fire('Error', 'An unknown error occurred while deleting the role.', 'error');", true);
                        break;
                }
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "deleteError",
                    $"Swal.fire('Error', 'An error occurred: {ex.Message.Replace("'", "\\'")}', 'error');", true);
            }
        }


        private void ShowAlert(string message, string icon)
        {
            string script = $"Swal.fire({{ icon: '{icon}', text: '{message.Replace("'", "\\'")}', showConfirmButton: true }});";
            ScriptManager.RegisterStartupScript(this, GetType(), "swalMessage", script, true);
        }

        // Not used because deletion uses __doPostBack() from JS
        protected void gvRoles_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e) { }
    }
}
