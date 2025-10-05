using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using RRCManagementSystem.Helpers; // AESHelper for encryption/hashing
using System.Web.Script.Serialization;

namespace RRCManagementSystem
{
    public partial class Login : Page
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;
        private const int IpWindowMinutes = 5;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Prevent caching of sensitive pages
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));

            if (!IsPostBack)
            {
                pnlCaptcha.Visible = false;
                lblMessage.Text = "";
            }

            // Redirect already logged-in admins
            if (Session["IsAuthenticated"] as bool? == true && Session["UserID"] != null && Session["Role"] != null)
            {
                string role = Session["Role"].ToString();
                Response.Redirect(
                    role == "RootAdmin" ? "~/RootDashboard.aspx" :
                    role == "SuperAdmin" ? "~/SuperAdminDashboard.aspx" :
                    role == "Inspector" ? "~/InspectorDashboard.aspx" :
                    "~/Dashboard.aspx",
                    false
                );
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            // Redirect logged-in client
            if (Session["ClientID"] != null)
            {
                Response.Redirect("Home.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            lblMessage.Text = ""; // Clear previous messages

            string ip = Request.UserHostAddress ?? "";

            // Step 1: IP-based throttling check
            if (GetFailedIPAttempts(ip, IpWindowMinutes) >= 5)
            {
                lblMessage.Text = $"⏳ Too many failed attempts from this IP. Please wait {IpWindowMinutes} minutes before trying again.";
                return;
            }


            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text.Trim();

            // Step 2: Basic input validation
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

            Regex strongPasswordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&.])[A-Za-z\d@$!%*?&.]{8,64}$");
            if (!strongPasswordRegex.IsMatch(password))
            {
                lblMessage.Text = "⚠ Password must contain at least one uppercase, one lowercase, one number, and one special character.";
                return;
            }

            try
            {
                string emailHash = AESHelper.ComputeSHA256(email);

                // Step 3: Try Admin/SuperAdmin/Inspector login
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
                            string decryptedEmail = "";

                            if (reader["Email"] != DBNull.Value)
                            {
                                try { decryptedEmail = AESHelper.DecryptEmail(reader["Email"].ToString()); }
                                catch { decryptedEmail = "[Decryption Error]"; }
                            }

                            // Step 4: Account status checks
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

                            // Step 5: Lockout handling
                            if (lockoutObj != DBNull.Value && Convert.ToDateTime(lockoutObj) > DateTime.Now)
                            {
                                pnlCaptcha.Visible = true;
                                lblMessage.Text = $"⏳ Account locked. Try again after {Convert.ToDateTime(lockoutObj):hh:mm tt}.";
                                return;
                            }

                            // Step 6: CAPTCHA logic (only after 5 failed attempts)
                            if (failedAttempts >= 5)
                            {
                                pnlCaptcha.Visible = true;
                                string captchaResponse = Request.Form["g-recaptcha-response"];

                                if (string.IsNullOrEmpty(captchaResponse))
                                {
                                    lblMessage.Text = "⚠ Please complete the CAPTCHA.";
                                    return;
                                }

                                if (!IsCaptchaValid())
                                {
                                    lblMessage.Text = "⚠ CAPTCHA verification failed.";
                                    return;
                                }
                            }

                            // Step 7: Password verification
                            if (!string.IsNullOrEmpty(hash) && PasswordHelper.VerifyPassword(hash, password))
                            {
                                // Reset failed attempts on success
                                ResetFailedLogin(userID);
                                LogIPAttempt(ip, true);
                                AddAuditLog(userID, $"{role} {userName} passed password; 2FA pending.");

                                // If not inspector, go to 2FA flow
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

                                    Guid newSessionID = Guid.NewGuid();

                                    // === Use a NEW connection for the update ===
                                    using (var updateConn = new SqlConnection(connectionString))
                                    using (var updateCmd = new SqlCommand(@"
    UPDATE dbo.Users
    SET CurrentSessionID = @SessionID,
        CurrentSessionAt = GETDATE()
    WHERE UserID = @UserID", updateConn))
                                    {
                                        updateCmd.Parameters.AddWithValue("@SessionID", newSessionID);
                                        updateCmd.Parameters.AddWithValue("@UserID", userID);
                                        updateConn.Open();
                                        updateCmd.ExecuteNonQuery();
                                    }


                                    // === Save to ASP.NET Session ===
                                    Session["UserID"] = userID;
                                    Session["Role"] = role;
                                    Session["Name"] = userName;
                                    Session["Email"] = decryptedEmail;
                                    Session["IsAuthenticated"] = true;
                                    Session["SessionID"] = newSessionID;

                                    AddAuditLog(userID, $"Inspector {userName} logged in (single session started).");

                                    Response.Redirect("~/InspectorDashboard.aspx", false);
                                    Context.ApplicationInstance.CompleteRequest();
                                    return;
                                }

                            }
                            else
                            {
                                // Incorrect password
                                HandleFailedLogin(userID);
                                LogIPAttempt(ip, false);
                                const int MaxAttempts = 5;
                                int remaining = Math.Max(0, MaxAttempts - (failedAttempts + 1));
                                lblMessage.Text = $"⚠ Invalid credentials. {remaining} attempt(s) left.";

                                if (failedAttempts + 1 >= 5)
                                    pnlCaptcha.Visible = true;

                                return;
                            }
                        }
                    }
                }

                // Step 8: Try client login if not found in admin table
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
                            string decryptedClientEmail = "";

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
                                Guid newSessionID = Guid.NewGuid();

                                using (var updateConn = new SqlConnection(connectionString))
                                using (var updateCmd = new SqlCommand(@"
    UPDATE dbo.Clients
    SET CurrentSessionID = @SessionID,
        CurrentSessionAt = GETDATE()
    WHERE ClientID = @ClientID", updateConn))
                                {
                                    updateCmd.Parameters.AddWithValue("@SessionID", newSessionID);
                                    updateCmd.Parameters.AddWithValue("@ClientID", clientId);
                                    updateConn.Open();
                                    updateCmd.ExecuteNonQuery();
                                }

                                // Save to ASP.NET Session
                                Session["ClientID"] = clientId;
                                Session["ClientName"] = name;
                                Session["Email"] = decryptedClientEmail;
                                Session["Role"] = "Client";             // VERY IMPORTANT
                                Session["SessionID"] = newSessionID;    // Used for Global.asax check


                                LogIPAttempt(ip, true);

                                AddAuditLog(clientId, $"Client {name} logged in (single session started).");

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

        // ========== HELPER METHODS ==========

        private bool IsCaptchaValid()
        {
            string response = Request.Form["g-recaptcha-response"];
            if (string.IsNullOrEmpty(response)) return false;

            using (var client = new WebClient())
            {
                string secret = ConfigurationManager.AppSettings["RecaptchaSecretKey"];
                string googleResponse = client.DownloadString(
                    $"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={response}"
                );

                var js = new JavaScriptSerializer();
                dynamic jsonData = js.Deserialize<dynamic>(googleResponse);

                return jsonData["success"] == true;
            }
        }

        private int GetFailedIPAttempts(string ip, int windowMinutes)
        {
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
