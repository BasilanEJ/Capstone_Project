using System;
using System.Configuration;
using System.Data.SqlClient;
using RRCManagementSystem.Helpers; // Ensure PasswordHelper.cs is in this namespace

namespace RRCManagementSystem
{
    public partial class SuperAdminProfile : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Ensure only logged-in SuperAdmin can access
               /* if (Session["SuperAdminID"] == null)
                {
                    Response.Redirect("~/Login.aspx");
                    return;
                } */

                LoadProfile();
            }
        }

        private void LoadProfile()
        {
            int superAdminId = Convert.ToInt32(Session["SuperAdminID"]);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT Name, Email FROM Users WHERE UserID = @UserID AND Role = 'SuperAdmin'";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", superAdminId);

                    try
                    {
                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            txtName.Text = reader["Name"].ToString();
                            txtEmail.Text = reader["Email"].ToString();
                        }
                        else
                        {
                            lblMessage.Text = "⚠ SuperAdmin not found.";
                        }
                    }
                    catch (Exception ex)
                    {
                        lblMessage.Text = "⚠ Error loading profile: " + ex.Message;
                    }
                }
            }
        }

        protected void btnSaveProfile_Click(object sender, EventArgs e)
        {
            int superAdminId = Convert.ToInt32(Session["SuperAdminID"]);
            string newName = txtName.Text.Trim();
            string newEmail = txtEmail.Text.Trim();
            string newPassword = txtNewPassword.Text.Trim();

            if (string.IsNullOrEmpty(newName) || string.IsNullOrEmpty(newEmail))
            {
                lblMessage.Text = "⚠ Name and Email cannot be empty.";
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query;

                if (!string.IsNullOrEmpty(newPassword))
                {
                    // 🔐 Hash new password using Argon2
                    string hashedPassword = PasswordHelper.HashPassword(newPassword);

                    query = @"UPDATE Users
                              SET Name = @Name,
                                  Email = @Email,
                                  PasswordHash = @PasswordHash
                              WHERE UserID = @UserID AND Role = 'SuperAdmin'";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", newName);
                        cmd.Parameters.AddWithValue("@Email", newEmail);
                        cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                        cmd.Parameters.AddWithValue("@UserID", superAdminId);

                        try
                        {
                            conn.Open();
                            cmd.ExecuteNonQuery();
                            lblMessage.CssClass = "alert success";
                            lblMessage.Text = "✅ Profile and password updated successfully!";
                        }
                        catch (Exception ex)
                        {
                            lblMessage.Text = "⚠ Error saving profile: " + ex.Message;
                        }
                    }
                }
                else
                {
                    // No password change
                    query = @"UPDATE Users
                              SET Name = @Name,
                                  Email = @Email
                              WHERE UserID = @UserID AND Role = 'SuperAdmin'";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", newName);
                        cmd.Parameters.AddWithValue("@Email", newEmail);
                        cmd.Parameters.AddWithValue("@UserID", superAdminId);

                        try
                        {
                            conn.Open();
                            cmd.ExecuteNonQuery();
                            lblMessage.CssClass = "alert success";
                            lblMessage.Text = "✅ Profile updated successfully!";
                        }
                        catch (Exception ex)
                        {
                            lblMessage.Text = "⚠ Error saving profile: " + ex.Message;
                        }
                    }
                }
            }
        }
    }
}
    