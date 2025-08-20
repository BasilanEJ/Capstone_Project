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
                int resultCode;

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spRole_DeleteSafe", conn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add("@RoleID", SqlDbType.Int).Value = roleId;

                    var pOut = new SqlParameter("@ResultCode", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(pOut);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    resultCode = (pOut.Value == DBNull.Value) ? -99 : Convert.ToInt32(pOut.Value);
                }

                switch (resultCode)
                {
                    case 1:
                        ShowAlert("✅ Role deleted successfully.", "success");
                        LoadRoles();
                        break;
                    case 0:
                        ShowAlert("⚠ Cannot delete. Users are assigned to this role.", "warning");
                        break;
                    case -1:
                        ShowAlert("⚠ Role not found.", "warning");
                        break;
                    default:
                        ShowAlert("❌ Unknown result while deleting role.", "error");
                        break;
                }
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

        // Not used because deletion uses __doPostBack() from JS
        protected void gvRoles_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e) { }
    }
}
