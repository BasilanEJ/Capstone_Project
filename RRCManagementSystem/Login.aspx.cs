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
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;
        private const int IpWindowMinutes = 10;  // rate-limit window for IP failures

        protected void Page_Load(object sender, EventArgs e)
        {
            // Prevent cached pages (back/forward bypass after logout)
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));

            if (!IsPostBack)
            {
                pnlCaptcha.Visible = false;
                lblMessage.Text = "";
            }

            // Auto-redirect only if fully authenticated (after 2FA)
            if (Session["IsAuthenticated"] as bool? == true &&
                Session["UserID"] != null && Session["Role"] != null)
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

            // Client already logged in
            if (Session["ClientID"] != null)
            {
                Response.Redirect("Home.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string ip = Request.UserHostAddress ?? "";

            // ======== IP rate-limit check ========
            if (GetFailedIPAttempts(ip, IpWindowMinutes) >= 5)
            {
                lblMessage.Text = "⏳ Too many failed attempts from this IP. Try again later.";
                return;
            }

            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text.Trim();

            // ======== Validation: Empty fields ========
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                lblMessage.Text = "⚠ Please enter both email and password.";
                return;
            }

            // ======== Validation: Email length ========
            if (email.Length > 100)
            {
                lblMessage.Text = "⚠ Email address is too long. Maximum 100 characters allowed.";
                return;
            }

            // ======== Validation: Email format ========
            bool isValidEmail = Regex.IsMatch(email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase);

            if (!isValidEmail)
            {
                lblMessage.Text = "⚠ Please enter a valid email address.";
                return;
            }

            // ======== Validation: Password length ========
            if (password.Length < 8 || password.Length > 64)
            {
                lblMessage.Text = "⚠ Password must be between 8 and 64 characters.";
                return;
            }

            // ======== Validation: Password complexity ========
            Regex strongPasswordRegex = new Regex(
                @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,64}$");

            if (!strongPasswordRegex.IsMatch(password))
            {
                lblMessage.Text = "⚠ Password must contain at least one uppercase, one lowercase, one number, and one special character.";
                return;
            }

            try
            {
                // ======== Compute SHA-256 hash for email lookup ========
                string emailHash = AESHelper.ComputeSHA256(email);

                // ========== 1) Try Users (admins/staff) ==========
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

                            // Decrypt the email for session usage or display
                            string decryptedEmail = string.Empty;
                            if (reader["Email"] != DBNull.Value)
                            {
                                try
                                {
                                    decryptedEmail = AESHelper.DecryptEmail(reader["Email"].ToString());
                                }
                                catch
                                {
                                    decryptedEmail = "[Decryption Error]";
                                }
                            }

                            /* ===============================
                               STATUS CHECK
                               =============================== */
                            if (status.Equals("Deleted", StringComparison.OrdinalIgnoreCase))
                            {
                                // Treat deleted account as not found
                                lblMessage.Text = "⚠ Account not found.";
                                return;
                            }

                            if (!status.Equals("Active", StringComparison.OrdinalIgnoreCase) &&
                                !status.Equals("Available", StringComparison.OrdinalIgnoreCase))
                            {
                                lblMessage.Text = "⚠ Your account is not active.";
                                return;
                            }

                            /* ===============================
                               LOCKOUT CHECK
                               =============================== */
                            if (lockoutObj != DBNull.Value && Convert.ToDateTime(lockoutObj) > DateTime.Now)
                            {
                                pnlCaptcha.Visible = true;
                                lblMessage.Text = $"⏳ Account locked. Try again after {Convert.ToDateTime(lockoutObj):hh:mm tt}.";
                                return;
                            }

                            // CAPTCHA after 5 failed attempts
                            if (failedAttempts >= 5)
                            {
                                pnlCaptcha.Visible = true;
                                if (!IsCaptchaValid())
                                {
                                    lblMessage.Text = "⚠ CAPTCHA verification failed.";
                                    return;
                                }
                            }

                            /* ===============================
                               PASSWORD CHECK USING ARGON2
                               =============================== */
                            if (!string.IsNullOrEmpty(hash) && PasswordHelper.VerifyPassword(hash, password))
                            {
                                ResetFailedLogin(userID);   // SP
                                LogIPAttempt(ip, true);     // SP
                                AddAuditLog(userID, $"{role} {userName} passed password; 2FA pending."); // SP

                                // Enforce 2FA for all roles except Inspector
                                if (!role.Equals("Inspector", StringComparison.OrdinalIgnoreCase))
                                {
                                    Session["Pending2FA_UserID"] = userID;
                                    Session["Pending2FA_Email"] = decryptedEmail;
                                    Session["Pending2FA_Name"] = userName;
                                    Session["Pending2FA_Role"] = role;

                                    // Clear any authenticated session values
                                    Session.Remove("IsAuthenticated");
                                    Session.Remove("UserID");
                                    Session.Remove("Role");
                                    Session.Remove("Name");
                                    Session.Remove("Email");

                                    if (!is2FAEnabled)
                                        Response.Redirect("Enable2FA.aspx", false);
                                    else
                                        Response.Redirect("VerifyTOTP.aspx", false);

                                    Context.ApplicationInstance.CompleteRequest();
                                    return;
                                }
                                else
                                {
                                    // Inspectors skip 2FA
                                    Session["UserID"] = userID;
                                    Session["Role"] = role;
                                    Session["Name"] = userName;
                                    Session["Email"] = decryptedEmail;
                                    Session["IsAuthenticated"] = true;

                                    Response.Redirect("~/InspectorDashboard.aspx", false);
                                    Context.ApplicationInstance.CompleteRequest();
                                    return;
                                }
                            }
                            else
                            {
                                HandleFailedLogin(userID); // SP
                                LogIPAttempt(ip, false);   // SP

                                int remaining = Math.Max(0, 4 - failedAttempts);
                                lblMessage.Text = $"⚠ Invalid credentials. {remaining} attempt(s) left.";
                                if (failedAttempts + 1 >= 5) pnlCaptcha.Visible = true;
                                return;
                            }
                        }
                    }
                }

                // ========== 2) Try Clients ==========
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAuth_GetClientByEmail", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Compute SHA-256 hash of the email for lookup with pepper
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

                            // Decrypt EmailEnc
                            string decryptedClientEmail = string.Empty;
                            if (reader["EmailEnc"] != DBNull.Value)
                            {
                                try
                                {
                                    decryptedClientEmail = AESHelper.DecryptEmail(reader["EmailEnc"].ToString());
                                }
                                catch
                                {
                                    decryptedClientEmail = "[Decryption Error]";
                                }
                            }

                            /* ===============================
                               CLIENT STATUS CHECK
                               =============================== */
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

                            /* ===============================
                               CLIENT PASSWORD CHECK
                               =============================== */
                            if (!string.IsNullOrEmpty(hash) && PasswordHelper.VerifyPassword(hash, password))
                            {
                                // ✅ Login success
                                Session["ClientID"] = clientId;
                                Session["ClientName"] = name;
                                Session["Email"] = decryptedClientEmail;

                                AddAuditLog(null, $"Client {name} logged in.");

                                Response.Redirect("Home.aspx", false);
                                Context.ApplicationInstance.CompleteRequest();
                                return;
                            }
                            else
                            {
                                lblMessage.Text = "⚠ Invalid credentials for client account.";
                                return;
                            }
                        }
                        else
                        {
                            lblMessage.Text = "⚠ Account not found.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠ Error during login: " + ex.Message;
            }
        }

        /* ======== CAPTCHA Validation ======== */
        private bool IsCaptchaValid()
        {
            string response = Request.Form["g-recaptcha-response"];
            if (string.IsNullOrEmpty(response)) return false;

            using (var client = new WebClient())
            {
                string secret = "6Ld6VrcrAAAAANJi4Djjr9vN7N5KIWoIoL_CCi_z"; // TODO: move to config
                string result = client.DownloadString($"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={response}");
                return result.Contains("\"success\": true");
            }
        }

        /* ======== IP Tracking ======== */
        private int GetFailedIPAttempts(string ip, int windowMinutes)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spLoginAttempt_CountRecentFailures", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IPAddress", SqlDbType.NVarChar, 50).Value = ip ?? "";
                cmd.Parameters.Add("@WindowMinutes", SqlDbType.Int).Value = windowMinutes;
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private void LogIPAttempt(string ip, bool success)
        {
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
