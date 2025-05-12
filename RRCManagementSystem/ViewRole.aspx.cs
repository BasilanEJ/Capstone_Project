using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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

        protected void gvRoles_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DeleteRole")
            {
                int roleId = Convert.ToInt32(e.CommandArgument);

                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        // 1. Get the RoleName first
                        string getRoleNameQuery = "SELECT RoleName FROM Roles WHERE RoleID = @RoleID";
                        string roleName = "";

                        using (SqlCommand cmdGet = new SqlCommand(getRoleNameQuery, conn))
                        {
                            cmdGet.Parameters.AddWithValue("@RoleID", roleId);
                            object result = cmdGet.ExecuteScalar();
                            if (result != null)
                                roleName = result.ToString();
                        }

                        if (string.IsNullOrEmpty(roleName))
                        {
                            lblMessage.Text = "⚠ Role not found.";
                            lblMessage.ForeColor = System.Drawing.Color.Red;
                            return;
                        }

                        // 2. Check if any users are using this role
                        string checkQuery = "SELECT COUNT(*) FROM Users WHERE Role = @RoleName";
                        using (SqlCommand cmdCheck = new SqlCommand(checkQuery, conn))
                        {
                            cmdCheck.Parameters.AddWithValue("@RoleName", roleName);
                            int count = (int)cmdCheck.ExecuteScalar();

                            if (count > 0)
                            {
                                lblMessage.Text = "⚠ Cannot delete. There are users assigned to this role.";
                                lblMessage.ForeColor = System.Drawing.Color.Red;
                                return;
                            }
                        }

                        // 3. If no users found, proceed to delete
                        string deleteQuery = "DELETE FROM Roles WHERE RoleID = @RoleID";
                        using (SqlCommand cmdDelete = new SqlCommand(deleteQuery, conn))
                        {
                            cmdDelete.Parameters.AddWithValue("@RoleID", roleId);
                            cmdDelete.ExecuteNonQuery();
                        }
                    }

                    lblMessage.Text = "✅ Role deleted successfully.";
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                    LoadRoles();
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "❌ Error: " + ex.Message;
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
            }
        }

    }
}