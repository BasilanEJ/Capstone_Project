using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;

namespace RRCManagementSystem
{
    public partial class AddRole : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Nothing needed here for now
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string roleName = txtRoleName.Text.Trim();

            if (string.IsNullOrEmpty(roleName))
            {
                lblMessage.Text = "⚠️ Role name is required.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            // 🔵 Format role name to Title Case
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            roleName = textInfo.ToTitleCase(roleName.ToLower());

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // 🔵 Check if role already exists (case-insensitive)
                    string checkQuery = "SELECT COUNT(*) FROM Roles WHERE LOWER(RoleName) = LOWER(@RoleName)";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@RoleName", roleName);
                        int count = (int)checkCmd.ExecuteScalar();

                        if (count > 0)
                        {
                            lblMessage.Text = "❌ This role already exists.";
                            lblMessage.ForeColor = System.Drawing.Color.Red;
                            return;
                        }
                    }

                    // 🔵 Insert new role
                    string insertQuery = "INSERT INTO Roles (RoleName, CreatedAt) VALUES (@RoleName, GETDATE())";
                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@RoleName", roleName);
                        insertCmd.ExecuteNonQuery();
                    }
                }

                lblMessage.Text = "✅ New role added successfully.";
                lblMessage.ForeColor = System.Drawing.Color.Green;
                txtRoleName.Text = string.Empty;
            }
            catch (Exception ex)
            {
                lblMessage.Text = "❌ Error: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }
    }
}