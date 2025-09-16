using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using RRCManagementSystem.Helpers; // For AESHelper
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class AddSuperUser : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Only RootAdmin can access this page
                if (Session["Role"] == null || !string.Equals(Session["Role"].ToString(), "RootAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    SafeRedirect("~/Unauthorized.aspx");
                }
            }
        }

        private void SafeRedirect(string url)
        {
            Response.Redirect(url, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            string plainEmail = txtEmail.Text.Trim().ToLowerInvariant();
            string name = txtName.Text.Trim();
            string password = txtPassword.Text;
            string confirmPassword = txtConfirm.Text;

            if (password != confirmPassword)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "SweetAlert",
                    "showError('Passwords do not match.');", true);
                return;
            }

            // Hash password using Argon2
            string hashedPassword = PasswordHelper.HashPassword(password);

            // Encrypt the email and create hash
            string encryptedEmail = AESHelper.EncryptEmail(plainEmail);
            string emailHash = AESHelper.ComputeSHA256(plainEmail);

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("spCreateSuperAdmin", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Email", encryptedEmail);
                    cmd.Parameters.AddWithValue("@EmailHash", emailHash);
                    cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                    cmd.Parameters.AddWithValue("@Name", name);

                    con.Open();
                    cmd.ExecuteNonQuery();

                    // If execution reaches here, user was created successfully
                    ClientScript.RegisterStartupScript(this.GetType(), "SweetAlert",
                        "showSuccess('Super Admin created successfully!');", true);
                }
            }
            catch (SqlException ex)
            {
                string message;

                // Handle RAISERROR messages from stored procedure
                if (ex.Number == 50000) // Custom errors
                {
                    message = ex.Message; // e.g., "Email already exists."
                }
                else
                {
                    message = "An error occurred while creating the user.";
                }

                ClientScript.RegisterStartupScript(this.GetType(), "SweetAlert",
                    $"showError('{message}');", true);
            }
            catch (Exception ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "SweetAlert",
                    $"showError('Unexpected error: {ex.Message}');", true);
            }
        }
    }
}
