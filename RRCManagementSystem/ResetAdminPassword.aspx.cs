using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using RRCManagementSystem.Helpers; // For AESHelper and PasswordHelper

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

                // 🔑 1) TOKEN path (preferred) - Direct reset link via email
                var token = Request.QueryString["token"];
                if (!string.IsNullOrWhiteSpace(token))
                {
                    HandleAdminTokenReset(); // Validates token, loads email and role into ViewState
                    return;
                }

                // 🔐 2) OTP/session fallback path
                if (!(Session["IsOTPVerified"] is bool ok && ok) || Session["Email"] == null)
                {
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    lblMessage.Text = "❌ Invalid or expired link. Please request a new reset.";
                    btnResetPassword.Enabled = false;
                    return;
                }

                // Confirm that email exists in Users table
                if (!IsEmailInUsers(Session["Email"].ToString()))
                {
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    lblMessage.Text = "❌ Account not found in Users.";
                    btnResetPassword.Enabled = false;
                    return;
                }

                // OTP path validated
                ViewState["Email"] = Session["Email"].ToString();
                ViewState["Role"] = "Admin";
                lblMessage.ForeColor = System.Drawing.Color.Green;
                lblMessage.Text = "✅ Verified. Please enter your new password.";
            }
        }

        /// <summary>
        /// Check if given email exists using SHA-256 hash (no plain text search)
        /// </summary>
        private bool IsEmailInUsers(string email)
        {
            string emailHash = AESHelper.ComputeSHA256(email);

            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("SELECT COUNT(1) FROM dbo.Users WHERE EmailHash = @EmailHash", con))
            {
                cmd.Parameters.AddWithValue("@EmailHash", emailHash);
                con.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        /// <summary>
        /// Validates token and retrieves associated encrypted email + expiry
        /// </summary>
        private void HandleAdminTokenReset()
        {
            string token = Request.QueryString["token"];

            if (string.IsNullOrEmpty(token))
            {
                lblMessage.Text = "❌ Missing reset token.";
                btnResetPassword.Enabled = false;
                return;
            }

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spResetToken_Validate", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ResetToken", SqlDbType.NVarChar, 100).Value = token;

                try
                {
                    conn.Open();
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            // Decrypt the email from DB
                            string encryptedEmail = rdr["Email"]?.ToString();
                            string decryptedEmail = string.Empty;

                            if (!string.IsNullOrEmpty(encryptedEmail))
                            {
                                try
                                {
                                    decryptedEmail = AESHelper.DecryptEmail(encryptedEmail);
                                }
                                catch
                                {
                                    decryptedEmail = "[Decryption Error]";
                                }
                            }

                            DateTime expiry = rdr["TokenExpiry"] != DBNull.Value
                                ? Convert.ToDateTime(rdr["TokenExpiry"])
                                : DateTime.MinValue;

                            if (expiry >= DateTime.Now)
                            {
                                ViewState["Email"] = decryptedEmail;
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
                catch (Exception ex)
                {
                    lblMessage.Text = "⚠ Error validating token: " + ex.Message;
                    btnResetPassword.Enabled = false;
                }
            }
        }

        /// <summary>
        /// Resets the password for admin or client accounts
        /// </summary>
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

            // Hash new password using Argon2
            string hashedPassword = PasswordHelper.HashPassword(newPassword);

            // Encrypt email for DB lookup
            string emailHash = AESHelper.ComputeSHA256(email);

            int rows = 0;
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(
                role == "Admin" ? "dbo.spPassword_ResetAdmin" : "dbo.spPassword_ResetClient", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                if (role == "Admin")
                {
                    cmd.Parameters.Add("@EmailHash", SqlDbType.Char, 64).Value = emailHash;
                    cmd.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, -1).Value = hashedPassword;
                }
                else
                {
                    cmd.Parameters.Add("@EmailHash", SqlDbType.Char, 64).Value = emailHash;
                    cmd.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 256).Value = hashedPassword;
                }

                try
                {
                    conn.Open();
                    object o = cmd.ExecuteScalar();
                    rows = (o == null || o == DBNull.Value) ? 0 : Convert.ToInt32(o);
                }
                catch (Exception ex)
                {
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    lblMessage.Text = "⚠ Error resetting password: " + ex.Message;
                    return;
                }
            }

            if (rows > 0)
            {
                lblMessage.ForeColor = System.Drawing.Color.Green;
                lblMessage.CssClass = "success-message";
                lblMessage.Text = "✅ Password reset successful! Redirecting to login...";

                // Clear OTP-related sessions if client
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
                lblMessage.Text = $"⚠ Failed to reset password. Email={email}, Role={role}.";
            }
        }
    }
}
