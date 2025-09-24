using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using RRCManagementSystem.Helpers; // For AESHelper

namespace RRCManagementSystem
{
    public partial class Login : Page
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;
        private const int IpWindowMinutes = 10;

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));

            if (!IsPostBack)
            {
                pnlCaptcha.Visible = false;
                lblMessage.Text = "";
            }

            // (Your existing redirection logic remains unchanged)
            if (Session["IsAuthenticated"] as bool? == true && Session["UserID"] != null && Session["Role"] != null)
            {
                string role = Session["Role"].ToString();
                Response.Redirect(
                    role == "RootAdmin" ? "~/RootDashboard.aspx"
                  : role == "SuperAdmin" ? "~/SuperAdminDashboard.aspx"
                  : role == "Inspector" ? "~/InspectorDashboard.aspx"
                                        : "~/Dashboard.aspx",
                    false
                );
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (Session["ClientID"] != null)
            {
                Response.Redirect("Home.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            // Reset the message text on every click to clear old messages
            lblMessage.Text = "";

            // ... (Your existing validation and login logic below) ...
            string ip = Request.UserHostAddress ?? "";

            if (GetFailedIPAttempts(ip, IpWindowMinutes) >= 5)
            {
                lblMessage.Text = "⏳ Too many failed attempts from this IP. Try again later.";
                return;
            }

            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                lblMessage.Text = "⚠ Please enter both email and password.";
                return;
            }

            if (email.Length > 100)
            {
                lblMessage.Text = "⚠ Email address is too long. Maximum 100 characters allowed.";
                return;
            }

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase))
            {
                lblMessage.Text = "⚠ Please enter a valid email address.";
                return;
            }

            if (password.Length < 8 || password.Length > 64)
            {
                lblMessage.Text = "⚠ Password must be between 8 and 64 characters.";
                return;
            }

            Regex strongPasswordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,64}$");
            if (!strongPasswordRegex.IsMatch(password))
            {
                lblMessage.Text = "⚠ Password must contain at least one uppercase, one lowercase, one number, and one special character.";
                return;
            }

            try
            {
                string emailHash = AESHelper.ComputeSHA256(email);
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAuth_GetUserByEmail", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@EmailHash", SqlDbType.Char, 64).Value = emailHash;
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string status = reader["Status"]?.ToString() ?? "";
                            string role = reader["Role"]?.ToString() ?? "";
                            string hash = reader["PasswordHash"]?.ToString();
                            bool is2FAEnabled = reader["TwoFactorEnabled"] != DBNull.Value && Convert.ToBoolean(reader["TwoFactorEnabled"]);
                            int failedAttempts = reader["FailedAttempts"] != DBNull.Value ? Convert.ToInt32(reader["FailedAttempts"]) : 0;
                            object lockoutObj = reader["LockoutUntil"];
                            int userID = Convert.ToInt32(reader["UserID"]);
                            string userName = reader["Name"]?.ToString() ?? "";
                            string decryptedEmail = string.Empty;

                            if (reader["Email"] != DBNull.Value)
                            {
                                try { decryptedEmail = AESHelper.DecryptEmail(reader["Email"].ToString()); }
                                catch { decryptedEmail = "[Decryption Error]"; }
                            }

                            if (status.Equals("Deleted", StringComparison.OrdinalIgnoreCase))
                            {
                                lblMessage.Text = "⚠ Account not found.";
                                return;
                            }
                            if (!status.Equals("Active", StringComparison.OrdinalIgnoreCase) && !status.Equals("Available", StringComparison.OrdinalIgnoreCase))
                            {
                                lblMessage.Text = "⚠ Your account is not active.";
                                return;
                            }

                            if (lockoutObj != DBNull.Value && Convert.ToDateTime(lockoutObj) > DateTime.Now)
                            {
                                pnlCaptcha.Visible = true;
                                lblMessage.Text = $"⏳ Account locked. Try again after {Convert.ToDateTime(lockoutObj):hh:mm tt}.";
                                return;
                            }

                            if (failedAttempts >= 5)
                            {
                                pnlCaptcha.Visible = true;
                                if (!IsCaptchaValid())
                                {
                                    lblMessage.Text = "⚠ CAPTCHA verification failed.";
                                    return;
                                }
                            }

                            if (!string.IsNullOrEmpty(hash) && PasswordHelper.VerifyPassword(hash, password))
                            {
                                ResetFailedLogin(userID);
                                LogIPAttempt(ip, true);
                                AddAuditLog(userID, $"{role} {userName} passed password; 2FA pending.");

                                if (!role.Equals("Inspector", StringComparison.OrdinalIgnoreCase))
                                {
                                    Session["Pending2FA_UserID"] = userID;
                                    Session["Pending2FA_Email"] = decryptedEmail;
                                    Session["Pending2FA_Name"] = userName;
                                    Session["Pending2FA_Role"] = role;

                                    Response.Redirect(is2FAEnabled ? "VerifyTOTP.aspx" : "Enable2FA.aspx", false);
                                    Context.ApplicationInstance.CompleteRequest();
                                    return;
                                }
                                else
                                {
                                    Session["UserID"] = userID;
                                    Session["Role"] = role;
                                    Session["Name"] = userName;
                                    Session["Email"] = decryptedEmail;
                                    Session["IsAuthenticated"] = true;
                                    AddAuditLog(userID, $"Inspector {userName} logged in.");
                                    Response.Redirect("~/InspectorDashboard.aspx", false);
                                    Context.ApplicationInstance.CompleteRequest();
                                    return;
                                }
                            }
                            else
                            {
                                HandleFailedLogin(userID);
                                LogIPAttempt(ip, false);
                                int remaining = Math.Max(0, 4 - (failedAttempts + 1));
                                lblMessage.Text = $"⚠ Invalid credentials. {remaining} attempt(s) left.";
                                if (failedAttempts + 1 >= 5) pnlCaptcha.Visible = true;
                                return;
                            }
                        }
                    }
                }

                // If no user was found, try Clients
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAuth_GetClientByEmail", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    string clientEmailHash = AESHelper.ComputeSHA256WithPepper(email);
                    cmd.Parameters.Add("@EmailHash", SqlDbType.Char, 64).Value = clientEmailHash;

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string status = reader["Status"]?.ToString() ?? "";
                            string hash = reader["PasswordHash"]?.ToString();
                            int clientId = Convert.ToInt32(reader["ClientID"]);
                            string name = reader["Name"]?.ToString() ?? "";
                            string decryptedClientEmail = string.Empty;

                            if (reader["EmailEnc"] != DBNull.Value)
                            {
                                try { decryptedClientEmail = AESHelper.DecryptEmail(reader["EmailEnc"].ToString()); }
                                catch { decryptedClientEmail = "[Decryption Error]"; }
                            }

                            if (status.Equals("Deleted", StringComparison.OrdinalIgnoreCase))
                            {
                                lblMessage.Text = "⚠ Account not found.";
                                return;
                            }
                            if (!status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                            {
                                lblMessage.Text = "⚠ Your account is not approved yet.";
                                return;
                            }

                            if (!string.IsNullOrEmpty(hash) && PasswordHelper.VerifyPassword(hash, password))
                            {
                                Session["ClientID"] = clientId;
                                Session["ClientName"] = name;
                                Session["Email"] = decryptedClientEmail;

                                LogIPAttempt(ip, true);
                                Response.Redirect("Home.aspx", false);
                                Context.ApplicationInstance.CompleteRequest();
                                return;
                            }
                            else
                            {
                                lblMessage.Text = "⚠ Invalid credentials for client account.";
                                LogIPAttempt(ip, false);
                                return;
                            }
                        }
                        else
                        {
                            lblMessage.Text = "⚠ Account not found.";
                            LogIPAttempt(ip, false);
                        }
                    }
                }
            }
            catch (SqlException)
            {
                lblMessage.Text = "⚠ An error occurred. Please try again later.";
            }
            catch (Exception)
            {
                lblMessage.Text = "⚠ An unexpected error occurred. Please try again later.";
            }
        }

        // (The rest of your helper methods like IsCaptchaValid, GetFailedIPAttempts, etc., are unchanged)
        private bool IsCaptchaValid()
        {
            // ... (rest of the code is unchanged)
            string response = Request.Form["g-recaptcha-response"];
            if (string.IsNullOrEmpty(response)) return false;

            using (var client = new WebClient())
            {
                string secret = ConfigurationManager.AppSettings["RecaptchaSecretKey"];
                string result = client.DownloadString($"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={response}");
                return result.Contains("\"success\": true");
            }
        }

        private int GetFailedIPAttempts(string ip, int windowMinutes)
        {
            // ... (rest of the code is unchanged)
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spLoginAttempt_CountRecentFailures", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IPAddress", SqlDbType.NVarChar, 50).Value = ip ?? "";
                cmd.Parameters.Add("@WindowMinutes", SqlDbType.Int).Value = windowMinutes;
                conn.Open();
                var result = cmd.ExecuteScalar();
                return result == DBNull.Value ? 0 : Convert.ToInt32(result);
            }
        }

        private void LogIPAttempt(string ip, bool success)
        {
            // ... (rest of the code is unchanged)
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spLoginAttempt_Insert", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IPAddress", SqlDbType.NVarChar, 50).Value = ip ?? "";
                cmd.Parameters.Add("@IsSuccess", SqlDbType.Bit).Value = success;
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void HandleFailedLogin(int userId)
        {
            // ... (rest of the code is unchanged)
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spAuth_FailAndMaybeLock", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void ResetFailedLogin(int userId)
        {
            // ... (rest of the code is unchanged)
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spAuth_ResetFailures", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void AddAuditLog(int? userID, string action)
        {
            // ... (rest of the code is unchanged)
            if (userID == null) return;

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spAudit_Insert", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@AdminID", SqlDbType.Int).Value = userID.Value;
                cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 255).Value = action ?? "";
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}