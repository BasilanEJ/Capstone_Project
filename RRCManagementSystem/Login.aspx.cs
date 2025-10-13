using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using RRCManagementSystem.Helpers;
using System.Web.Script.Serialization;

namespace RRCManagementSystem
{
    public partial class Login : Page
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;
        private const int IpWindowMinutes = 5;

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

            if (Session["ClientID"] != null)
            {
                Response.Redirect("Home.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "";
            string ip = Request.UserHostAddress ?? "";

            // --- Security Check: IP Rate Limiting ---
            if (GetFailedIPAttempts(ip, IpWindowMinutes) >= 5)
            {
                lblMessage.Text = $"**Access Denied.** Too many failed login attempts from this network address. Please wait {IpWindowMinutes} minutes before trying again.";
                return;
            }

            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text.Trim();

            // --- Input Validation Checks ---
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                lblMessage.Text = "**Required Fields.** Please enter both your email address and password.";
                return;
            }

            if (email.Length > 100)
            {
                lblMessage.Text = "**Input Error.** The email address entered is too long. The maximum allowed is 100 characters.";
                return;
            }

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase))
            {
                lblMessage.Text = "**Input Error.** Please enter a valid email address format.";
                return;
            }

            if (password.Length < 8 || password.Length > 64)
            {
                lblMessage.Text = "**Password Policy.** Your password must be between 8 and 64 characters in length.";
                return;
            }

            Regex strongPasswordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&.])[A-Za-z\d@$!%*?&.]{8,64}$");
            if (!strongPasswordRegex.IsMatch(password))
            {
                // Note: We show this error *before* DB lookup, as it pertains to the *password field itself*, not credentials.
                lblMessage.Text = "**Password Policy.** Your password must contain at least one uppercase letter, one lowercase letter, one number, and one special character (e.g., @$!%*?&.).";
                return;
            }

            try
            {
                string emailHash = AESHelper.ComputeSHA256(email);

                // Try Admin/SuperAdmin/Inspector login
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

                            // --- Account Status Checks ---
                            if (status.Equals("Deleted", StringComparison.OrdinalIgnoreCase))
                            {
                                // IMPORTANT SECURITY: Use generic message for "not found" or "deleted" accounts
                                lblMessage.Text = "**Login Failed.** Invalid email address or password. Please try again.";
                                return;
                            }
                            if (!status.Equals("Active", StringComparison.OrdinalIgnoreCase) && !status.Equals("Available", StringComparison.OrdinalIgnoreCase))
                            {
                                lblMessage.Text = "**Account Status.** Your account is currently not active. Please contact support for assistance.";
                                return;
                            }

                            // --- Account Lockout Check ---
                            if (lockoutObj != DBNull.Value && Convert.ToDateTime(lockoutObj) > DateTime.Now)
                            {
                                pnlCaptcha.Visible = true;
                                lblMessage.Text = $"**Account Locked.** This account is temporarily locked. Please try again after {Convert.ToDateTime(lockoutObj):hh:mm tt}.";
                                return;
                            }

                            // --- CAPTCHA Requirement Check (for high failure rate on this user) ---
                            if (failedAttempts >= 5)
                            {
                                pnlCaptcha.Visible = true;
                                string captchaResponse = Request.Form["g-recaptcha-response"];

                                if (string.IsNullOrEmpty(captchaResponse))
                                {
                                    lblMessage.Text = "**Security Check.** Due to multiple failed attempts, please complete the CAPTCHA to continue.";
                                    return;
                                }

                                if (!IsCaptchaValid())
                                {
                                    lblMessage.Text = "**Security Check Failed.** CAPTCHA verification failed. Please try again.";
                                    return;
                                }
                            }

                            // --- Password Verification ---
                            if (!string.IsNullOrEmpty(hash) && PasswordHelper.VerifyPassword(hash, password))
                            {
                                ResetFailedLogin(userID);
                                LogIPAttempt(ip, true);
                                AddAuditLog(userID, $"{role} {userName} passed password; 2FA pending.");

                                // All users (including Inspector) now go through 2FA flow
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
                                // --- Failed Password / Credential Check ---
                                HandleFailedLogin(userID);
                                LogIPAttempt(ip, false);
                                const int MaxAttempts = 5;
                                int remaining = Math.Max(0, MaxAttempts - (failedAttempts + 1));

                                // IMPORTANT SECURITY: Generic failure message
                                lblMessage.Text = $"**Login Failed.** Invalid email address or password. You have {remaining} attempt(s) remaining before your account is locked.";

                                if (failedAttempts + 1 >= 5)
                                    pnlCaptcha.Visible = true;
                                return;
                            }
                        }
                    }
                }

                // Try client login if no staff/admin user was found
                TryClientLogin(email, password, ip);
            }
            // --- Catch Blocks (Generic User-Facing Error) ---
            catch (SqlException sqlEx)
            {
                System.Diagnostics.Debug.WriteLine($"SQL Error: {sqlEx.Message}");
                System.Diagnostics.Debug.WriteLine($"Procedure: {sqlEx.Procedure}, Line: {sqlEx.LineNumber}");
                lblMessage.Text = "**System Error.** An unexpected database error occurred. Please try again later. If the problem persists, contact support.";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
                lblMessage.Text = "**System Error.** An unexpected error occurred. Please try again later. If the problem persists, contact support.";
            }
        }

        private void TryClientLogin(string email, string password, string ip)
        {
            try
            {
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
                            int clientId = Convert.ToInt32(reader["ClientID"]);
                            string status = reader["Status"]?.ToString() ?? "";
                            string hash = reader["PasswordHash"]?.ToString();
                            string name = reader["Name"]?.ToString() ?? "";
                            string decryptedClientEmail = "";

                            // Safely read FailedAttempts
                            int failedAttempts = 0;
                            if (reader.HasColumn("FailedAttempts") && reader["FailedAttempts"] != DBNull.Value)
                            {
                                failedAttempts = Convert.ToInt32(reader["FailedAttempts"]);
                            }

                            // Safely read LockoutUntil
                            object lockoutObj = null;
                            if (reader.HasColumn("LockoutUntil"))
                            {
                                lockoutObj = reader["LockoutUntil"];
                            }

                            if (reader["EmailEnc"] != DBNull.Value)
                            {
                                try { decryptedClientEmail = AESHelper.DecryptEmail(reader["EmailEnc"].ToString()); }
                                catch { decryptedClientEmail = "[Decryption Error]"; }
                            }

                            // --- Client Account Status Checks ---
                            if (status.Equals("Deleted", StringComparison.OrdinalIgnoreCase))
                            {
                                // IMPORTANT SECURITY: Use generic message for "not found" or "deleted" accounts
                                lblMessage.Text = "**Login Failed.** Invalid email address or password. Please try again.";
                                return;
                            }
                            if (!status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                            {
                                lblMessage.Text = "**Account Status.** Your client account is not yet approved. Please wait for an administrator to approve your registration.";
                                return;
                            }

                            // --- Client Account Lockout Check ---
                            if (lockoutObj != null && lockoutObj != DBNull.Value && Convert.ToDateTime(lockoutObj) > DateTime.Now)
                            {
                                pnlCaptcha.Visible = true;
                                lblMessage.Text = $"**Account Locked.** This client account is temporarily locked. Please try again after {Convert.ToDateTime(lockoutObj):hh:mm tt}.";
                                return;
                            }

                            // --- CAPTCHA Requirement Check (for high failure rate on this client user) ---
                            if (failedAttempts >= 5)
                            {
                                pnlCaptcha.Visible = true;
                                string captchaResponse = Request.Form["g-recaptcha-response"];
                                if (string.IsNullOrEmpty(captchaResponse))
                                {
                                    lblMessage.Text = "**Security Check.** Due to multiple failed attempts, please complete the CAPTCHA to continue.";
                                    return;
                                }
                                if (!IsCaptchaValid())
                                {
                                    lblMessage.Text = "**Security Check Failed.** CAPTCHA verification failed. Please try again.";
                                    return;
                                }
                            }

                            // --- Client Password Verification ---
                            if (!string.IsNullOrEmpty(hash) && PasswordHelper.VerifyPassword(hash, password))
                            {
                                ResetClientFailedLogin(clientId);

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

                                Session["ClientID"] = clientId;
                                Session["ClientName"] = name;
                                Session["Email"] = decryptedClientEmail;
                                Session["Role"] = "Client";
                                Session["SessionID"] = newSessionID;

                                LogIPAttempt(ip, true);
                                AddAuditLog(clientId, $"Client {name} logged in (single session started).");

                                Response.Redirect("Home.aspx", false);
                                Context.ApplicationInstance.CompleteRequest();
                                return;
                            }
                            else
                            {
                                // --- Failed Client Password / Credential Check ---
                                HandleClientFailedLogin(clientId);
                                LogIPAttempt(ip, false);
                                const int MaxAttempts = 5;
                                int remaining = Math.Max(0, MaxAttempts - (failedAttempts + 1));

                                // IMPORTANT SECURITY: Generic failure message
                                lblMessage.Text = $"**Login Failed.** Invalid email address or password. You have {remaining} attempt(s) remaining before your account is locked.";

                                if (failedAttempts + 1 >= 5)
                                    pnlCaptcha.Visible = true;

                                return;
                            }
                        }
                        else
                        {
                            // --- Account Not Found Check ---
                            lblMessage.Text = "**Login Failed.** Invalid email address or password. Please try again.";
                            LogIPAttempt(ip, false);
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                System.Diagnostics.Debug.WriteLine($"Client Login SQL Error: {sqlEx.Message}");
                System.Diagnostics.Debug.WriteLine($"Procedure: {sqlEx.Procedure}, Line: {sqlEx.LineNumber}");
                throw; // Re-throw to be caught by the main btnLogin_Click handler
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Client Login Error: {ex.Message}");
                throw; // Re-throw to be caught by the main btnLogin_Click handler
            }
        }

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
            try
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LogIPAttempt Error: {ex.Message}");
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

        private void HandleClientFailedLogin(int clientId)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spClientAuth_FailAndMaybeLock", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException sqlEx)
            {
                System.Diagnostics.Debug.WriteLine($"HandleClientFailedLogin Error: {sqlEx.Message}");
                throw;
            }
        }

        private void ResetClientFailedLogin(int clientId)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spClientAuth_ResetFailures", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException sqlEx)
            {
                System.Diagnostics.Debug.WriteLine($"ResetClientFailedLogin Error: {sqlEx.Message}");
                throw;
            }
        }

        private void AddAuditLog(int? userID, string action)
        {
            if (userID == null) return;

            try
            {
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddAuditLog Error: {ex.Message}");
            }
        }
    }

    // Helper extension method
    public static class DataReaderExtensions
    {
        public static bool HasColumn(this IDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}