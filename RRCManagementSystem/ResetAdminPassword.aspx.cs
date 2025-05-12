using System;
using System.Configuration;
using System.Data.SqlClient;

namespace RRCManagementSystem
{
    public partial class ResetAdminPassword : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                lblMessage.Text = "";

                string type = Request.QueryString["type"]?.ToLower();

                if (type == "admin")
                {
                    HandleAdminTokenReset();
                }
                else if (type == "client")
                {
                    HandleClientOtpReset();
                }
                else
                {
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    lblMessage.Text = "❌ Invalid reset request. Missing or incorrect type.";
                    btnResetPassword.Enabled = false;
                }
            }
        }

        private void HandleAdminTokenReset()
        {
            string token = Request.QueryString["token"];

            if (string.IsNullOrEmpty(token))
            {
                lblMessage.Text = "❌ Missing reset token.";
                btnResetPassword.Enabled = false;
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = "SELECT Email, TokenExpiry FROM Users WHERE ResetToken = @ResetToken AND Status = 'Active';";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ResetToken", token);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            DateTime expiry = reader.GetDateTime(1);

                            if (expiry >= DateTime.Now)
                            {
                                ViewState["Email"] = reader.GetString(0);
                                ViewState["Role"] = "Admin";

                                lblMessage.ForeColor = System.Drawing.Color.Green;
                                lblMessage.Text = "✅ Token validated. Please enter your new password.";
                            }
                            else
                            {
                                lblMessage.Text = "❌ This reset link has expired.";
                                btnResetPassword.Enabled = false;
                            }
                        }
                        else
                        {
                            lblMessage.Text = "❌ Invalid reset token.";
                            btnResetPassword.Enabled = false;
                        }
                    }
                }
            }
        }

        private void HandleClientOtpReset()
        {
            if (Session["IsOTPVerified"] == null || !(bool)Session["IsOTPVerified"])
            {
                lblMessage.Text = "❌ OTP not verified. Please try again.";
                Response.Redirect("ForgotPassword.aspx");
                return;
            }

            string email = Session["Email"]?.ToString();

            if (string.IsNullOrEmpty(email))
            {
                lblMessage.Text = "❌ Missing client session data.";
                Response.Redirect("ForgotPassword.aspx");
                return;
            }

            ViewState["Email"] = email;
            ViewState["Role"] = "Client";

            lblMessage.ForeColor = System.Drawing.Color.Green;
            lblMessage.Text = "✅ OTP validated. Please enter your new password.";
        }

        protected void btnResetPassword_Click(object sender, EventArgs e)
        {
            string newPassword = txtNewPassword.Text.Trim();
            string confirmPassword = txtConfirmPassword.Text.Trim();

            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                lblMessage.Text = "⚠ Please fill in all password fields.";
                return;
            }

            if (newPassword != confirmPassword)
            {
                lblMessage.Text = "⚠ Passwords do not match.";
                return;
            }

            string email = ViewState["Email"]?.ToString();
            string role = ViewState["Role"]?.ToString();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role))
            {
                lblMessage.Text = "❌ Invalid session or reset data.";
                btnResetPassword.Enabled = false;
                return;
            }

            // ✅ Generate Argon2 hash
            string hashedPassword = PasswordHelper.HashPassword(newPassword);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = role == "Admin"
                    ? @"UPDATE Users
                        SET PasswordHash = @PasswordHash,
                            PasswordSalt = NULL,
                            ResetToken = NULL,
                            TokenExpiry = NULL
                        WHERE Email = @Email AND Status = 'Active';"
                    : @"UPDATE Clients
                        SET PasswordHash = @PasswordHash,
                            PasswordSalt = NULL
                        WHERE Email = @Email AND Status = 'Approved';";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                    cmd.Parameters.AddWithValue("@Email", email);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        lblMessage.ForeColor = System.Drawing.Color.Green;
                        lblMessage.CssClass = "success-message";
                        lblMessage.Text = "✅ Password reset successful! Redirecting to login...";

                        if (role == "Client")
                        {
                            Session["OTP"] = null;
                            Session["IsOTPVerified"] = null;
                            Session["Email"] = null;
                        }

                        Response.AddHeader("REFRESH", "3;URL=Login.aspx");
                    }
                    else
                    {
                        lblMessage.ForeColor = System.Drawing.Color.Red;
                        lblMessage.Text = $"⚠ Failed to reset password. Email={email}, Role={role}. Please try again.";
                    }
                }
            }
        }
    }
}