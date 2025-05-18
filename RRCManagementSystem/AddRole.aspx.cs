using System;
using System.Configuration;
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
            string roleName = txtRoleName.Text.Trim();

            if (string.IsNullOrEmpty(roleName))
            {
                ShowSweetAlert("Warning", "Role name is required.", "warning");
                return;
            }

            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            roleName = textInfo.ToTitleCase(roleName.ToLower());

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string checkQuery = "SELECT COUNT(*) FROM Roles WHERE LOWER(RoleName) = LOWER(@RoleName)";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@RoleName", roleName);
                        int count = (int)checkCmd.ExecuteScalar();

                        if (count > 0)
                        {
                            ShowSweetAlert("Duplicate", "This role already exists.", "error");
                            return;
                        }
                    }

                    string insertQuery = "INSERT INTO Roles (RoleName, CreatedAt) VALUES (@RoleName, GETDATE())";
                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@RoleName", roleName);
                        insertCmd.ExecuteNonQuery();
                    }
                }

                ShowSweetAlert("Success", "✅ Role added successfully!", "success");
                txtRoleName.Text = string.Empty;
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
