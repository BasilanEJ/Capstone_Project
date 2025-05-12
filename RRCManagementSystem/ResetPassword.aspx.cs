using System;
using System.Configuration;
using System.Data.SqlClient;

namespace RRCManagementSystem
{
    public partial class ResetPassword : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;
        private string token;
        private string email;

        protected void Page_Load(object sender, EventArgs e)
        {
            token = Request.QueryString["token"];
            if (!IsPostBack)
            {
                if (string.IsNullOrEmpty(token))
                {
                    ShowSweetAlert("Invalid Link", "Reset token is missing.", "error", false);
                    btnResetPassword.Enabled = false;
                    return;
                }

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT Email FROM Clients WHERE ResetToken = @Token AND ResetTokenExpiry > GETDATE()";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Token", token);
                    con.Open();

                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        email = result.ToString();
                        ViewState["ClientEmail"] = email;
                    }
                    else
                    {
                        ShowSweetAlert("Link Expired", "Reset token is invalid or has expired.", "warning", false);
                        btnResetPassword.Enabled = false;
                    }
                }
            }
        }

        protected void btnResetPassword_Click(object sender, EventArgs e)
        {
            string newPassword = txtNewPassword.Text.Trim();
            string confirmPassword = txtConfirmPassword.Text.Trim();

            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                ShowSweetAlert("Missing Fields", "Please enter and confirm your new password.", "info", true);
                return;
            }

            if (newPassword != confirmPassword)
            {
                ShowSweetAlert("Mismatch", "Passwords do not match.", "error", true);
                return;
            }

            string email = ViewState["ClientEmail"]?.ToString();
            if (string.IsNullOrEmpty(email))
            {
                ShowSweetAlert("Error", "Client email not found or session expired.", "error", true);
                return;
            }

            string hashedPassword = PasswordHelper.HashPassword(newPassword);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
UPDATE Clients
SET PasswordHash = @PasswordHash, ResetToken = NULL, ResetTokenExpiry = NULL, Status = 'Approved'
WHERE Email = @Email;";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                cmd.Parameters.AddWithValue("@Email", email);

                try
                {
                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        ShowSweetAlert("Success", "Password has been set! You may now log in.", "success", true, "Login.aspx");
                    }
                    else
                    {
                        ShowSweetAlert("Failed", "Could not reset password. Try again later.", "error", true);
                    }
                }
                catch (Exception ex)
                {
                    ShowSweetAlert("Server Error", "Error: " + ex.Message, "error", true);
                }
            }
        }

        private void ShowSweetAlert(string title, string message, string icon, bool showButton, string redirectUrl = "")
        {
            ltScript.Text = $@"
<script>
    Swal.fire({{
        title: '{title}',
        text: '{message}',
        icon: '{icon}',
        {(showButton ? "confirmButtonText: 'OK'," : "showConfirmButton: false,")}
        timer: 3000,
        timerProgressBar: true
    }}).then(() => {{
        {(string.IsNullOrEmpty(redirectUrl) ? "" : $"window.location.href = '{redirectUrl}';")}
    }});
</script>";
        }
    }
}