using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

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

            string eventTarget = Request["__EVENTTARGET"];
            if (eventTarget == "DeleteRole")
            {
                int roleId = int.Parse(hfRoleIDToDelete.Value);
                DeleteRole(roleId);
            }
        }

        private void LoadRoles()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT RoleID, RoleName, CreatedAt FROM Roles ORDER BY CreatedAt DESC";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvRoles.DataSource = dt;
                gvRoles.DataBind();
            }
        }

        private void DeleteRole(int roleId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string roleName = "";
                    using (SqlCommand cmdGet = new SqlCommand("SELECT RoleName FROM Roles WHERE RoleID = @RoleID", conn))
                    {
                        cmdGet.Parameters.AddWithValue("@RoleID", roleId);
                        object result = cmdGet.ExecuteScalar();
                        if (result != null) roleName = result.ToString();
                    }

                    if (string.IsNullOrEmpty(roleName))
                    {
                        ShowAlert("⚠ Role not found.", "error");
                        return;
                    }

                    using (SqlCommand cmdCheck = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Role = @RoleName", conn))
                    {
                        cmdCheck.Parameters.AddWithValue("@RoleName", roleName);
                        int count = (int)cmdCheck.ExecuteScalar();
                        if (count > 0)
                        {
                            ShowAlert("⚠ Cannot delete. Users are assigned to this role.", "warning");
                            return;
                        }
                    }

                    using (SqlCommand cmdDelete = new SqlCommand("DELETE FROM Roles WHERE RoleID = @RoleID", conn))
                    {
                        cmdDelete.Parameters.AddWithValue("@RoleID", roleId);
                        cmdDelete.ExecuteNonQuery();
                    }
                }

                ShowAlert("✅ Role deleted successfully.", "success");
                LoadRoles();
            }
            catch (Exception ex)
            {
                ShowAlert("❌ Error: " + ex.Message, "error");
            }
        }

        private void ShowAlert(string message, string icon)
        {
            string script = $"Swal.fire({{ icon: '{icon}', text: '{message.Replace("'", "\\'")}', showConfirmButton: true }});";
            ScriptManager.RegisterStartupScript(this, GetType(), "swalMessage", script, true);
        }

        protected void gvRoles_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // not used anymore since we use __doPostBack for deletion
        }
    }
}
