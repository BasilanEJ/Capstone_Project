using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class AddRole : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string roleName = (txtRoleName.Text ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(roleName))
            {
                ShowSweetAlert("Warning", "Role name is required.", "warning");
                return;
            }

            // Optional: Title Case for display consistency
            TextInfo ti = new CultureInfo("en-US", false).TextInfo;
            roleName = ti.ToTitleCase(roleName.ToLower());

            try
            {
                int newRoleId;

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spRole_Add", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@RoleName", SqlDbType.NVarChar, 100).Value = roleName;

                    var outParam = new SqlParameter("@NewRoleID", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(outParam);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    newRoleId = (outParam.Value == DBNull.Value) ? 0 : Convert.ToInt32(outParam.Value);
                }

                if (newRoleId > 0)
                {
                    ShowSweetAlert("Success", "✅ Role added successfully!", "success");
                    txtRoleName.Text = string.Empty;
                }
                else
                {
                    // Either duplicate or not inserted for some reason
                    ShowSweetAlert("Duplicate", "This role already exists.", "error");
                }
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601) // unique violation
            {
                ShowSweetAlert("Duplicate", "This role already exists.", "error");
            }
            catch (Exception ex)
            {
                ShowSweetAlert("Error", "❌ " + ex.Message, "error");
            }
        }

        private void ShowSweetAlert(string title, string message, string icon)
        {
            string script = $@"
                <script>
                    Swal.fire({{
                        title: '{title}',
                        text: '{message}',
                        icon: '{icon}',
                        confirmButtonColor: '#1f2937'
                    }});
                </script>";

            ScriptManager.RegisterStartupScript(this, GetType(), "SweetAlert", script, false);
        }
    }
}
