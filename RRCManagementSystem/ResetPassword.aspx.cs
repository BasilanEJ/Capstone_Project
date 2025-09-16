using RRCManagementSystem.Helpers;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace RRCManagementSystem
{
    public partial class ResetPassword : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;
        private string token;

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

                // ✅ Validate token via stored procedure (returns email if token is valid & not expired)
                string emailFromToken = GetEmailByValidToken(token);
                if (!string.IsNullOrEmpty(emailFromToken))
                {
                    ViewState["ClientEmail"] = emailFromToken;
                    // keep button enabled
                }
                else
                {
                    ShowSweetAlert("Link Expired", "Reset token is invalid or has expired.", "warning", false);
                    btnResetPassword.Enabled = false;
                }
            }
        }

        private string GetEmailByValidToken(string resetToken)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spClient_ResetToken_Validate", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Token", SqlDbType.NVarChar, 200).Value = resetToken;

                try
                {
                    con.Open();
                    object result = cmd.ExecuteScalar();

                    // Decrypt the returned EmailEnc
                    return result != null ? AESHelper.DecryptEmail(result.ToString()) : null;
                }
                catch (Exception ex)
                {
                    ShowSweetAlert("Server Error", "Error validating token: " + ex.Message, "error", true);
                    return null;
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

            // ✅ Compute email hash
            string emailHash = AESHelper.ComputeSHA256WithPepper(email);

            // ✅ Hash password (Argon2 or your preferred hashing method)
            string hashedPassword = PasswordHelper.HashPassword(newPassword);

            int rows = 0;
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spClient_ResetPassword", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@EmailHash", SqlDbType.Char, 64).Value = emailHash;
                cmd.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 256).Value = hashedPassword;

                try
                {
                    con.Open();
                    object o = cmd.ExecuteScalar();
                    rows = (o == null || o == DBNull.Value) ? 0 : Convert.ToInt32(o);
                }
                catch (Exception ex)
                {
                    ShowSweetAlert("Server Error", "Error: " + ex.Message, "error", true);
                    return;
                }
            }

            if (rows > 0)
            {
                ShowSweetAlert("Success", "Password has been set! You may now log in.", "success", true, "Login.aspx");
            }
            else
            {
                ShowSweetAlert("Failed", "Could not reset password. Try again later.", "error", true);
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
