using RRCManagementSystem.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;

namespace RRCManagementSystem
{
    public class EnhancedLockoutInfo
    {
        public bool IsLocked { get; set; }
        public DateTime? LockedUntil { get; set; }
        public string LockoutReason { get; set; }
        public int FailedAttempts { get; set; }
    }

    public class EnhancedDeviceInfo
    {
        public string DeviceFingerprint { get; set; }
        public string CanvasFingerprint { get; set; }
        public string HardwareID { get; set; }
        public string OSInfo { get; set; }
        public string BrowserName { get; set; }
        public string ScreenResolution { get; set; }
        public string TimezoneOffset { get; set; }
        public string Language { get; set; }
        public int? HardwareConcurrency { get; set; }
        public int? ColorDepth { get; set; }
        public int? DeviceMemory { get; set; }
        public int? MaxTouchPoints { get; set; }
        public string Platform { get; set; }
    }

    public partial class Login : Page
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;
        private const int IpWindowMinutes = 15;
        private const int DeviceLockoutMinutes = 15;
        private const int MaxAttempts = 5;
        private const int CaptchaThreshold = 3;
        private const int UserIPMaxAttempts = 5;
        private const int UserIPLockoutMinutes = 15;
        private const int MultiUserThreshold = 3;
        private const int IPFullLockoutMinutes = 60;
        private const int AttackWindowMinutes = 15;

        private EnhancedDeviceInfo ParseEnhancedDeviceInfo()
        {
            return new EnhancedDeviceInfo
            {
                DeviceFingerprint = hiddenFingerprint.Value?.Trim() ?? "",
                CanvasFingerprint = hiddenCanvasFingerprint.Value?.Trim() ?? "",
                HardwareID = hiddenHardwareID.Value?.Trim() ?? "",
                OSInfo = hiddenOSInfo.Value?.Trim() ?? "",
                BrowserName = hiddenBrowserName.Value?.Trim() ?? "",
                ScreenResolution = hiddenScreenResolution.Value?.Trim() ?? "",
                TimezoneOffset = hiddenTimezoneOffset.Value?.Trim() ?? "",
                Language = hiddenLanguage.Value?.Trim() ?? "",
                HardwareConcurrency = TryParseInt(hiddenHardwareConcurrency.Value),
                ColorDepth = TryParseInt(hiddenColorDepth.Value),
                DeviceMemory = TryParseInt(hiddenDeviceMemory.Value),
                MaxTouchPoints = TryParseInt(hiddenMaxTouchPoints.Value),
                Platform = hiddenPlatform.Value?.Trim() ?? ""
            };
        }

        // ============================================
        // NEW METHODS FOR SMART IP LOCKOUT
        // ============================================

        /// <summary>
        /// Check if the entire IP address is locked (attack mode)
        /// </summary>
        private bool IsIPFullyLocked(string ip, out DateTime? lockedUntil, out string reason)
        {
            lockedUntil = null;
            reason = null;

            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spIPLockout_CheckFullIPLock", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@IPAddress", SqlDbType.NVarChar, 50).Value = ip ?? "";

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bool isLocked = reader["IsLocked"] != DBNull.Value && Convert.ToBoolean(reader["IsLocked"]);

                            if (isLocked)
                            {
                                lockedUntil = reader["LockedUntil"] != DBNull.Value
                                    ? Convert.ToDateTime(reader["LockedUntil"])
                                    : (DateTime?)null;
                                reason = reader["LockoutReason"]?.ToString();
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IsIPFullyLocked Error: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Check if specific User+IP combination is locked
        /// </summary>
        private bool IsUserIPLocked(string ip, string emailHash, out DateTime? lockedUntil, out int failedAttempts)
        {
            lockedUntil = null;
            failedAttempts = 0;

            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spIPLockout_CheckUserIPLock", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@IPAddress", SqlDbType.NVarChar, 50).Value = ip ?? "";
                    cmd.Parameters.Add("@EmailHash", SqlDbType.NVarChar, 64).Value = emailHash ?? "";

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bool isLocked = reader["IsLocked"] != DBNull.Value && Convert.ToBoolean(reader["IsLocked"]);

                            if (isLocked)
                            {
                                lockedUntil = reader["LockedUntil"] != DBNull.Value
                                    ? Convert.ToDateTime(reader["LockedUntil"])
                                    : (DateTime?)null;
                                failedAttempts = reader["FailedAttempts"] != DBNull.Value
                                    ? Convert.ToInt32(reader["FailedAttempts"])
                                    : 0;
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IsUserIPLocked Error: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Record login attempt for User+IP combination
        /// </summary>
        private void LogUserIPAttempt(string ip, string emailHash, bool success, string userAgent)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spIPLockout_RecordUserIPAttempt", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@IPAddress", SqlDbType.NVarChar, 50).Value = ip ?? "";
                    cmd.Parameters.Add("@EmailHash", SqlDbType.NVarChar, 64).Value = emailHash ?? "";
                    cmd.Parameters.Add("@IsSuccess", SqlDbType.Bit).Value = success;
                    cmd.Parameters.Add("@UserAgent", SqlDbType.NVarChar, 500).Value = userAgent ?? "";
                    cmd.Parameters.Add("@MaxAttempts", SqlDbType.Int).Value = UserIPMaxAttempts;
                    cmd.Parameters.Add("@LockoutMinutes", SqlDbType.Int).Value = UserIPLockoutMinutes;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LogUserIPAttempt Error: {ex.Message}");
            }
        }

        private bool DetectMultiUserAttack(string ip, out int distinctUsers)
        {
            distinctUsers = 0;

            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spIPLockout_DetectMultiUserAttack", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@IPAddress", SqlDbType.NVarChar, 50).Value = ip ?? "";
                    cmd.Parameters.Add("@WindowMinutes", SqlDbType.Int).Value = AttackWindowMinutes;
                    cmd.Parameters.Add("@DistinctUserThreshold", SqlDbType.Int).Value = MultiUserThreshold;

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            distinctUsers = reader["DistinctUsersFailed"] != DBNull.Value
                                ? Convert.ToInt32(reader["DistinctUsersFailed"])
                                : 0;

                            bool isAttack = reader["IsAttackDetected"] != DBNull.Value
                                && Convert.ToBoolean(reader["IsAttackDetected"]);

                            return isAttack;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DetectMultiUserAttack Error: {ex.Message}");
            }

            return false;
        }

        private void LockFullIPAddress(string ip, string reason, int distinctUsers)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spIPLockout_LockFullIP", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@IPAddress", SqlDbType.NVarChar, 50).Value = ip ?? "";
                    cmd.Parameters.Add("@LockoutMinutes", SqlDbType.Int).Value = IPFullLockoutMinutes;
                    cmd.Parameters.Add("@LockoutReason", SqlDbType.NVarChar, 255).Value = reason ?? "Multiple user attack detected";
                    cmd.Parameters.Add("@DistinctUsersAttempted", SqlDbType.Int).Value = distinctUsers;

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    System.Diagnostics.Debug.WriteLine($"🚨 SECURITY ALERT: Full IP lockout applied to {ip}. Reason: {reason}. Distinct users: {distinctUsers}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LockFullIPAddress Error: {ex.Message}");
            }
        }

        private void ResetUserIPLockout(string ip, string emailHash)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spIPLockout_ResetUserIP", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@IPAddress", SqlDbType.NVarChar, 50).Value = ip ?? "";
                    cmd.Parameters.Add("@EmailHash", SqlDbType.NVarChar, 64).Value = emailHash ?? "";

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ResetUserIPLockout Error: {ex.Message}");
            }
        }

        private int? TryParseInt(string value)
        {
            if (int.TryParse(value, out int result))
                return result;
            return null;
        }

        private string GetIPSubnet(string ip)
        {
            if (string.IsNullOrEmpty(ip)) return "";

            try
            {
                var parts = ip.Split('.');
                if (parts.Length >= 3)
                {
                    return $"{parts[0]}.{parts[1]}.{parts[2]}";
                }
            }
            catch { }

            return ip;
        }

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
        role == "Headtechnician" ? "~/TechDashboard.aspx" :
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
            string ipSubnet = null;  // Keep as null to disable subnet lockout
            string userAgent = Request.UserAgent ?? "";

            EnhancedDeviceInfo deviceInfo = ParseEnhancedDeviceInfo();

            if (string.IsNullOrEmpty(deviceInfo.DeviceFingerprint))
            {
                lblMessage.Text = "**System Error.** Unable to identify your device. Please refresh the page and try again.";
                return;  // ✅ NO FAILURE RECORDED - Not a login attempt
            }

            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                lblMessage.Text = "**Required Fields.** Please enter both your email address and password.";
                return;  // ✅ NO FAILURE RECORDED - Not a login attempt
            }

            if (email.Length > 100)
            {
                lblMessage.Text = "**Input Error.** The email address entered is too long. The maximum allowed is 100 characters.";
                return;  // ✅ NO FAILURE RECORDED - Validation error
            }

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase))
            {
                lblMessage.Text = "**Input Error.** Please enter a valid email address format.";
                return;  // ✅ NO FAILURE RECORDED - Validation error
            }

            if (password.Length < 8 || password.Length > 64)
            {
                lblMessage.Text = "**Password Policy.** Your password must be between 8 and 64 characters in length.";
                return;  // ✅ NO FAILURE RECORDED - Validation error
            }

            Regex strongPasswordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&.])[A-Za-z\d@$!%*?&.]{8,64}$");
            if (!strongPasswordRegex.IsMatch(password))
            {
                lblMessage.Text = "**Password Policy.** Your password must contain at least one uppercase letter, one lowercase letter, one number, and one special character (e.g., @$!%*?&.).";
                return;  // ✅ NO FAILURE RECORDED - Validation error
            }

            string emailHash = AESHelper.ComputeSHA256(email);

            // ============================================
            // ⭐ TIER 1: Check if entire IP is locked (attack mode)
            // ============================================
            DateTime? ipLockedUntil;
            string ipLockReason;
            if (IsIPFullyLocked(ip, out ipLockedUntil, out ipLockReason))
            {
                string timeRemaining = ipLockedUntil.HasValue
                    ? ipLockedUntil.Value.ToString("hh:mm tt")
                    : "some time";

                lblMessage.Text = $"**Security Alert.** This network has been temporarily blocked due to suspicious activity. Please try again after {timeRemaining}.";
                return;  // ✅ NO FAILURE RECORDED - Already locked
            }

            // ============================================
            // ⭐ TIER 2: Check if User+IP combination is locked
            // ============================================
            DateTime? userIpLockedUntil;
            int userIpFailedAttempts;
            if (IsUserIPLocked(ip, emailHash, out userIpLockedUntil, out userIpFailedAttempts))
            {
                string timeRemaining = userIpLockedUntil.HasValue
                    ? userIpLockedUntil.Value.ToString("hh:mm tt")
                    : "15 minutes";

                lblMessage.Text = $"**Account Temporarily Locked.** Too many failed login attempts for this account from your location. Please try again after {timeRemaining}.";
                return;  // ✅ NO FAILURE RECORDED - Already locked
            }

            // ============================================
            // TIER 3: Check device lockout status (existing)
            // ============================================
            EnhancedLockoutInfo lockoutInfo = CheckAllLockoutLayers(
                deviceInfo.DeviceFingerprint,
                deviceInfo.CanvasFingerprint,
                emailHash,
                ipSubnet,  // null = subnet lockout disabled
                deviceInfo
            );

            // Handle expired lockouts properly
            if (lockoutInfo.IsLocked)
            {
                if (lockoutInfo.LockedUntil.HasValue && lockoutInfo.LockedUntil.Value <= DateTime.Now)
                {
                    // Lockout expired, reset it
                    ResetAllLockoutLayers(
                        deviceInfo.DeviceFingerprint,
                        deviceInfo.CanvasFingerprint,
                        emailHash,
                        ipSubnet,
                        deviceInfo.HardwareID
                    );
                }
                else
                {
                    // Still locked
                    pnlCaptcha.Visible = true;
                    string lockoutMessage = lockoutInfo.LockoutReason == "Device"
                        ? "**Device Blocked.** Too many failed attempts from this device."
                        : lockoutInfo.LockoutReason == "Email"
                        ? "**Account Locked.** Too many failed attempts for this email address."
                        : "**Network Blocked.** Too many failed attempts from this network.";

                    lblMessage.Text = $"{lockoutMessage} Please try again after {lockoutInfo.LockedUntil.Value:hh:mm tt}.";
                    return;  // ✅ NO FAILURE RECORDED - Already locked
                }
            }

            // ⭐ UPDATED CAPTCHA ENFORCEMENT - Check both device and user+IP attempts
            if (lockoutInfo.FailedAttempts >= CaptchaThreshold || userIpFailedAttempts >= CaptchaThreshold)
            {
                pnlCaptcha.Visible = true;

                string captchaResponse = Request.Form["g-recaptcha-response"];
                if (string.IsNullOrEmpty(captchaResponse) || !IsCaptchaValid())
                {
                    lblMessage.Text = "**Security Verification Required.** Please complete the CAPTCHA to continue.";
                    return;  // ✅ NO FAILURE RECORDED - CAPTCHA not completed
                }
            }

            try
            {
                bool loginSuccessful = false;

                // Try to authenticate user (admin/staff)
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

                            // ✅ Check account status BEFORE checking password
                            if (status.Equals("Deleted", StringComparison.OrdinalIgnoreCase))
                            {
                                lblMessage.Text = "**Login Failed.** Invalid email address or password. Please try again.";
                                goto FailureCleanup;
                            }

                            if (!status.Equals("Active", StringComparison.OrdinalIgnoreCase) && !status.Equals("Available", StringComparison.OrdinalIgnoreCase))
                            {
                                lblMessage.Text = "**Account Status.** Your account is currently not active. Please contact support for assistance.";
                                goto FailureCleanup;
                            }

                            // ✅ Check if account is locked BEFORE checking password
                            if (lockoutObj != DBNull.Value && Convert.ToDateTime(lockoutObj) > DateTime.Now)
                            {
                                pnlCaptcha.Visible = true;
                                lblMessage.Text = $"**Account Locked.** This account is temporarily locked. Please try again after {Convert.ToDateTime(lockoutObj):hh:mm tt}.";
                                return;  // ✅ NO FAILURE RECORDED - Already locked
                            }

                            if (failedAttempts >= 3)
                            {
                                pnlCaptcha.Visible = true;
                            }

                            // ✅ NOW CHECK PASSWORD - Using PasswordHelper as in working code
                            if (!string.IsNullOrEmpty(hash) && PasswordHelper.VerifyPassword(hash, password))
                            {
                                // ✅ SUCCESS - Reset all lockouts
                                ResetFailedLogin(userID);
                                ResetAllLockoutLayers(
                                    deviceInfo.DeviceFingerprint,
                                    deviceInfo.CanvasFingerprint,
                                    emailHash,
                                    ipSubnet,
                                    deviceInfo.HardwareID
                                );

                                // ⭐ Reset User+IP lockout
                                ResetUserIPLockout(ip, emailHash);

                                // Log successful attempts
                                LogIPAttempt(ip, true);
                                LogUserIPAttempt(ip, emailHash, true, userAgent);

                                // ✅ FIXED: Proper 2FA handling like the old working code
                                AddAuditLog(userID, $"{role} {userName} passed password; 2FA pending.");

                                Session["Pending2FA_UserID"] = userID;
                                Session["Pending2FA_Email"] = decryptedEmail;
                                Session["Pending2FA_Name"] = userName;
                                Session["Pending2FA_Role"] = role;

                                Response.Redirect(is2FAEnabled ? "VerifyTOTP.aspx" : "Enable2FA.aspx", false);
                                Context.ApplicationInstance.CompleteRequest();
                                loginSuccessful = true;
                                return;
                            }
                            else
                            {
                                // ❌ WRONG PASSWORD - THIS is where we record failures
                                HandleFailedLogin(userID);

                                int newFailedAttempts = failedAttempts + 1;
                                int remaining = Math.Max(0, MaxAttempts - newFailedAttempts);

                                // Send security alerts
                                if (newFailedAttempts == CaptchaThreshold)
                                {
                                    SendSecurityAlertToUser(decryptedEmail, newFailedAttempts, ip, deviceInfo, DateTime.Now);
                                    SendSecurityAlertToSuperAdmin(decryptedEmail, newFailedAttempts, ip, deviceInfo, DateTime.Now);
                                }

                                if (newFailedAttempts >= MaxAttempts)
                                {
                                    SendLockoutAlertToSuperAdmin(decryptedEmail, ip, deviceInfo, DateTime.Now, newFailedAttempts);
                                    SendLockoutAlertToUser(decryptedEmail, ip, deviceInfo, DateTime.Now);
                                }

                                lblMessage.Text = $"**Login Failed.** Invalid email address or password. You have {remaining} attempt(s) remaining before your account is locked.";

                                if (newFailedAttempts >= CaptchaThreshold)
                                    pnlCaptcha.Visible = true;

                                // ✅ ONLY record failure for wrong password
                                goto FailureCleanup;
                            }
                        }
                    }
                }

                // If user not found, try client login
                if (!loginSuccessful)
                {
                    TryClientLogin(email, password, emailHash, ip, ipSubnet, deviceInfo);
                    return;
                }
            }
            catch (SqlException sqlEx)
            {
                System.Diagnostics.Debug.WriteLine($"SQL Error: {sqlEx.Message}");
                System.Diagnostics.Debug.WriteLine($"Procedure: {sqlEx.Procedure}, Line: {sqlEx.LineNumber}");
                lblMessage.Text = "**System Error.** An unexpected database error occurred. Please try again later. If the problem persists, contact support.";
                return;  // ✅ NO FAILURE RECORDED - System error, not user fault
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
                lblMessage.Text = "**System Error.** An unexpected error occurred. Please try again later. If the problem persists, contact support.";
                return;  // ✅ NO FAILURE RECORDED - System error, not user fault
            }

        FailureCleanup:
            // ✅ ONLY REACHED FOR ACTUAL AUTHENTICATION FAILURES (wrong password)
            LogIPAttempt(ip, false);
            LogUserIPAttempt(ip, emailHash, false, userAgent);

            // Record the failed attempt
            RecordFailedAttemptAllLayers(
                deviceInfo,
                emailHash,
                ipSubnet,
                ip,
                userAgent
            );

            // ⭐ Check for multi-user attack
            int distinctUsers;
            if (DetectMultiUserAttack(ip, out distinctUsers))
            {
                LockFullIPAddress(ip, $"Multiple account breach attempt detected ({distinctUsers} different accounts)", distinctUsers);

                lblMessage.Text = "**Security Alert.** Suspicious activity detected from your network. Access has been temporarily restricted.";
                return;
            }

            // GET UPDATED LOCKOUT INFO AFTER RECORDING FAILURE
            EnhancedLockoutInfo updatedLockoutInfo = CheckAllLockoutLayers(
                deviceInfo.DeviceFingerprint,
                deviceInfo.CanvasFingerprint,
                emailHash,
                ipSubnet,
                deviceInfo
            );

            int currentAttempts = updatedLockoutInfo.FailedAttempts;

            if (currentAttempts == CaptchaThreshold)
            {
                SendSecurityAlertToUser(email, currentAttempts, ip, deviceInfo, DateTime.Now);
                SendSecurityAlertToSuperAdmin(email, currentAttempts, ip, deviceInfo, DateTime.Now);
            }

            // Send CRITICAL lockout notifications when account is locked (5 attempts)
            if (updatedLockoutInfo.IsLocked && currentAttempts >= MaxAttempts)
            {
                SendLockoutAlertToSuperAdmin(email, ip, deviceInfo, DateTime.Now, currentAttempts);
                SendLockoutAlertToUser(email, ip, deviceInfo, DateTime.Now);

                // Update the error message to reflect lockout
                lblMessage.Text = $"**Account Temporarily Locked.** Too many failed login attempts. Please try again in {DeviceLockoutMinutes} minutes.";
            }

            // Show CAPTCHA for next attempt if threshold reached
            if (currentAttempts >= CaptchaThreshold)
            {
                pnlCaptcha.Visible = true;
            }

            return;
        }

        private void TryClientLogin(string email, string password, string emailHash, string ip, string ipSubnet, EnhancedDeviceInfo deviceInfo)
        {
            string userAgent = Request.UserAgent ?? "";
            bool loginSuccessful = false;

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

                            int failedAttempts = 0;
                            if (reader.HasColumn("FailedAttempts") && reader["FailedAttempts"] != DBNull.Value)
                            {
                                failedAttempts = Convert.ToInt32(reader["FailedAttempts"]);
                            }

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

                            // Account status checks
                            if (status.Equals("Deleted", StringComparison.OrdinalIgnoreCase))
                            {
                                lblMessage.Text = "**Login Failed.** Invalid email address or password. Please try again.";
                                goto ClientFailureCleanup;
                            }

                            if (!status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                            {
                                lblMessage.Text = "**Account Status.** Your client account is not yet approved. Please wait for an administrator to approve your registration.";
                                goto ClientFailureCleanup;
                            }

                            // Check if lockout has expired
                            if (lockoutObj != null && lockoutObj != DBNull.Value)
                            {
                                DateTime lockoutTime = Convert.ToDateTime(lockoutObj);

                                if (lockoutTime > DateTime.Now)
                                {
                                    pnlCaptcha.Visible = true;
                                    lblMessage.Text = $"**Account Locked.** This client account is temporarily locked. Please try again after {lockoutTime:hh:mm tt}.";
                                    goto ClientFailureCleanup;
                                }
                            }

                            // CAPTCHA check for multiple failures
                            if (failedAttempts >= 3)
                            {
                                pnlCaptcha.Visible = true;
                                string captchaResponse = Request.Form["g-recaptcha-response"];
                                if (string.IsNullOrEmpty(captchaResponse) || !IsCaptchaValid())
                                {
                                    lblMessage.Text = "**Security Check.** Due to multiple failed attempts, please complete the CAPTCHA to continue.";
                                    goto ClientFailureCleanup;
                                }
                            }

                            // ✅ PASSWORD VERIFICATION
                            if (!string.IsNullOrEmpty(hash) && PasswordHelper.VerifyPassword(hash, password))
                            {
                                // ✅ Password is correct - now check device trust
                                reader.Close();

                                // ⭐ NEW: Check if device is trusted
                                bool isDeviceTrusted = IsClientDeviceTrusted(clientId, deviceInfo);

                                if (isDeviceTrusted)
                                {
                                    // ✅ TRUSTED DEVICE - Allow direct login
                                    loginSuccessful = true;
                                    ResetClientFailedLogin(clientId);
                                    ResetAllLockoutLayers(
                                        deviceInfo.DeviceFingerprint,
                                        deviceInfo.CanvasFingerprint,
                                        clientEmailHash,
                                        ipSubnet,
                                        deviceInfo.HardwareID
                                    );

                                    LogIPAttempt(ip, true);

                                    // Update last used timestamp for trusted device
                                    UpdateTrustedDeviceLastUsed(clientId, deviceInfo.DeviceFingerprint);

                                    // Create session
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
                                    Response.Redirect("Home.aspx", false);
                                    Context.ApplicationInstance.CompleteRequest();
                                    return;
                                }
                                else
                                {
                                    // ⭐ NEW DEVICE - Require OTP verification
                                    ResetClientFailedLogin(clientId);
                                    ResetAllLockoutLayers(
                                        deviceInfo.DeviceFingerprint,
                                        deviceInfo.CanvasFingerprint,
                                        clientEmailHash,
                                        ipSubnet,
                                        deviceInfo.HardwareID
                                    );

                                    LogIPAttempt(ip, true);

                                    // Generate and send OTP
                                    string otpCode = GenerateAndSendClientOTP(clientId, decryptedClientEmail, deviceInfo, ip, userAgent);

                                    if (!string.IsNullOrEmpty(otpCode))
                                    {
                                        // Store pending verification info in session
                                        Session["PendingClientOTP_ClientID"] = clientId;
                                        Session["PendingClientOTP_Email"] = decryptedClientEmail;
                                        Session["PendingClientOTP_Name"] = name;
                                        Session["PendingClientOTP_DeviceFingerprint"] = deviceInfo.DeviceFingerprint;
                                        Session["PendingClientOTP_CanvasFingerprint"] = deviceInfo.CanvasFingerprint;
                                        Session["PendingClientOTP_HardwareID"] = deviceInfo.HardwareID;
                                        // Redirect to OTP verification page
                                        Response.Redirect("ClientVerifyOTP.aspx", false);
                                        Context.ApplicationInstance.CompleteRequest();
                                        return;
                                    }
                                    else
                                    {
                                        lblMessage.Text = "**System Error.** Unable to send verification code. Please try again.";
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                // ❌ WRONG PASSWORD
                                HandleClientFailedLogin(clientId);

                                int newFailedAttempts = failedAttempts + 1;
                                int remaining = Math.Max(0, MaxAttempts - newFailedAttempts);

                                if (newFailedAttempts == CaptchaThreshold)
                                {
                                    SendSecurityAlertToUser(email, newFailedAttempts, ip, deviceInfo, DateTime.Now);
                                    SendSecurityAlertToSuperAdmin(email, newFailedAttempts, ip, deviceInfo, DateTime.Now);
                                }
                                if (newFailedAttempts >= MaxAttempts)
                                {
                                    SendLockoutAlertToSuperAdmin(email, ip, deviceInfo, DateTime.Now, newFailedAttempts);
                                    SendLockoutAlertToUser(email, ip, deviceInfo, DateTime.Now);
                                }

                                lblMessage.Text = $"**Login Failed.** Invalid email address or password. You have {remaining} attempt(s) remaining before your account is locked.";

                                if (newFailedAttempts >= CaptchaThreshold)
                                    pnlCaptcha.Visible = true;

                                goto ClientFailureCleanup;
                            }
                        }
                        else
                        {
                            lblMessage.Text = "**Login Failed.** Invalid email address or password. Please try again.";
                            goto ClientFailureCleanup;
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                System.Diagnostics.Debug.WriteLine($"Client Login SQL Error: {sqlEx.Message}");
                lblMessage.Text = "**System Error.** An unexpected database error occurred. Please try again later.";
                goto ClientFailureCleanup;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Client Login Error: {ex.Message}");
                lblMessage.Text = "**System Error.** An unexpected error occurred. Please try again later.";
                goto ClientFailureCleanup;
            }

        ClientFailureCleanup:
            LogIPAttempt(ip, false);
            RecordFailedAttemptAllLayers(deviceInfo, emailHash, ipSubnet, ip, userAgent);
            return;
        }


        private bool IsClientDeviceTrusted(int clientId, EnhancedDeviceInfo deviceInfo)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spClientTrustedDevice_Check", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                    cmd.Parameters.Add("@DeviceFingerprint", SqlDbType.NVarChar, 500).Value = deviceInfo.DeviceFingerprint;
                    cmd.Parameters.Add("@CanvasFingerprint", SqlDbType.NVarChar, 500).Value = (object)deviceInfo.CanvasFingerprint ?? DBNull.Value;
                    cmd.Parameters.Add("@HardwareID", SqlDbType.NVarChar, 64).Value = (object)deviceInfo.HardwareID ?? DBNull.Value;

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bool isTrusted = reader["IsTrusted"] != DBNull.Value && Convert.ToBoolean(reader["IsTrusted"]);
                            return isTrusted;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IsClientDeviceTrusted Error: {ex.Message}");
            }

            return false; // Default to not trusted
        }

        private void UpdateTrustedDeviceLastUsed(int clientId, string deviceFingerprint)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spClientTrustedDevice_UpdateLastUsed", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                    cmd.Parameters.Add("@DeviceFingerprint", SqlDbType.NVarChar, 500).Value = deviceFingerprint;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateTrustedDeviceLastUsed Error: {ex.Message}");
            }
        }

        private string GenerateAndSendClientOTP(int clientId, string email, EnhancedDeviceInfo deviceInfo, string ip, string userAgent)
        {
            try
            {
                string otpCode = null;
                DateTime expiresAt = DateTime.Now;

                // Generate OTP code
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spClientOTP_Generate", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                    cmd.Parameters.Add("@DeviceFingerprint", SqlDbType.NVarChar, 500).Value = deviceInfo.DeviceFingerprint;
                    cmd.Parameters.Add("@CanvasFingerprint", SqlDbType.NVarChar, 500).Value = (object)deviceInfo.CanvasFingerprint ?? DBNull.Value;
                    cmd.Parameters.Add("@HardwareID", SqlDbType.NVarChar, 64).Value = (object)deviceInfo.HardwareID ?? DBNull.Value;
                    cmd.Parameters.Add("@IPAddress", SqlDbType.NVarChar, 50).Value = ip;
                    cmd.Parameters.Add("@UserAgent", SqlDbType.NVarChar, 500).Value = userAgent;
                    cmd.Parameters.Add("@ValidityMinutes", SqlDbType.Int).Value = 10;

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            otpCode = reader["OTPCode"]?.ToString();
                            expiresAt = Convert.ToDateTime(reader["ExpiresAt"]);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(otpCode))
                {
                    // Send OTP email
                    SendClientOTPEmail(email, otpCode, expiresAt, deviceInfo);
                    return otpCode;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GenerateAndSendClientOTP Error: {ex.Message}");
            }

            return null;
        }


        private void SendClientOTPEmail(string email, string otpCode, DateTime expiresAt, EnhancedDeviceInfo deviceInfo)
        {
            try
            {
                string subject = "Device Verification Code - RRC Management System";

                string body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: 'Poppins', Arial, sans-serif; line-height: 1.6; color: #333; background: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 20px auto; background: white; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 20px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #007bff 0%, #0056b3 100%); color: white; padding: 30px; text-align: center; }}
        .header h1 {{ margin: 0; font-size: 24px; font-weight: 600; }}
        .content {{ padding: 40px 30px; }}
        .otp-box {{ background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%); border: 2px solid #007bff; border-radius: 10px; padding: 30px; text-align: center; margin: 25px 0; }}
        .otp-code {{ font-size: 36px; font-weight: 700; color: #007bff; letter-spacing: 8px; margin: 10px 0; font-family: 'Courier New', monospace; }}
        .validity {{ color: #666; font-size: 14px; margin-top: 10px; }}
        .info-box {{ background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; border-radius: 4px; }}
        .device-info {{ background: #f8f9fa; padding: 15px; border-radius: 8px; margin: 20px 0; }}
        .device-info table {{ width: 100%; }}
        .device-info td {{ padding: 8px 5px; font-size: 14px; }}
        .device-info td:first-child {{ font-weight: 600; color: #666; width: 40%; }}
        .footer {{ background: #f1f1f1; padding: 20px; text-align: center; font-size: 12px; color: #666; }}
        .warning {{ color: #d32f2f; font-weight: 600; }}
        ul {{ padding-left: 20px; }}
        li {{ margin: 8px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔐 Device Verification Required</h1>
        </div>
        <div class='content'>
            <p>Hello,</p>
            <p>You're signing in to your RRC account from a new device. Please use the verification code below to complete your login:</p>
            
            <div class='otp-box'>
                <div style='font-size: 14px; color: #666; text-transform: uppercase; letter-spacing: 1px; font-weight: 600;'>Your Verification Code</div>
                <div class='otp-code'>{otpCode}</div>
                <div class='validity'>⏱️ Valid for 10 minutes (until {expiresAt:hh:mm tt})</div>
            </div>

            <div class='info-box'>
                <strong>⚠️ Important:</strong><br/>
                • This code expires in 10 minutes<br/>
                • Never share this code with anyone<br/>
                • If you didn't attempt to log in, please ignore this email and consider changing your password
            </div>

            <div class='device-info'>
                <h3 style='margin-top: 0; color: #333; font-size: 16px;'>📱 Device Information:</h3>
                <table>
                    <tr>
                        <td>Browser:</td>
                        <td>{deviceInfo.BrowserName}</td>
                    </tr>
                    <tr>
                        <td>Operating System:</td>
                        <td>{deviceInfo.OSInfo}</td>
                    </tr>
                    <tr>
                        <td>Platform:</td>
                        <td>{deviceInfo.Platform}</td>
                    </tr>
                </table>
            </div>

            <p><strong>What happens next?</strong></p>
            <ul>
                <li>Enter the code on the verification page</li>
                <li>This device will be remembered for 30 days</li>
                <li>You won't need to verify again on this device during that time</li>
            </ul>
        </div>
        <div class='footer'>
            <p>This is an automated security message from <strong>RRC Management System</strong></p>
            <p>If you have questions or concerns, please contact our support team.</p>
        </div>
    </div>
</body>
</html>";

                SendEmail(email, subject, body, true, MailPriority.High);
                System.Diagnostics.Debug.WriteLine($"✅ OTP email sent to: {email}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SendClientOTPEmail Error: {ex.Message}");
            }
        }

        private void SendSecurityAlertToUser(string email, int attemptCount, string ip, EnhancedDeviceInfo deviceInfo, DateTime attemptTime)
        {
            try
            {
                string subject = "Security Alert: Unusual Login Activity Detected";

                string body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #d32f2f; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background: #f9f9f9; padding: 20px; border: 1px solid #ddd; }}
        .alert-box {{ background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 15px 0; }}
        .details {{ background: white; padding: 15px; border-radius: 5px; margin: 15px 0; }}
        .details table {{ width: 100%; border-collapse: collapse; }}
        .details td {{ padding: 8px; border-bottom: 1px solid #eee; }}
        .details td:first-child {{ font-weight: bold; width: 40%; }}
        .footer {{ background: #f1f1f1; padding: 15px; text-align: center; font-size: 12px; color: #666; border-radius: 0 0 5px 5px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>🔒 Security Alert</h2>
        </div>
        <div class='content'>
            <p>Hello,</p>
            
            <div class='alert-box'>
                <strong>⚠️ Suspicious Activity Detected</strong><br/>
                We detected {attemptCount} failed login attempts on your RRC Management System account.
            </div>
            
            <p>If this was you, you can safely ignore this email. However, if you did not attempt to log in, your account may be at risk.</p>
            
            <div class='details'>
                <h3>Attempt Details:</h3>
                <table>
                    <tr>
                        <td>Time:</td>
                        <td>{attemptTime.ToString("MMMM dd, yyyy 'at' hh:mm:ss tt")}</td>
                    </tr>
                    <tr>
                        <td>IP Address:</td>
                        <td>{ip}</td>
                    </tr>
                    <tr>
                        <td>Browser:</td>
                        <td>{deviceInfo.BrowserName}</td>
                    </tr>
                    <tr>
                        <td>Operating System:</td>
                        <td>{deviceInfo.OSInfo}</td>
                    </tr>
                    <tr>
                        <td>Location:</td>
                        <td>{deviceInfo.TimezoneOffset} (Timezone)</td>
                    </tr>
                </table>
            </div>
            
            <h3>What should you do?</h3>
            <ul>
                <li><strong>If this was you:</strong> No action needed. You may continue trying to log in.</li>
                <li><strong>If this wasn't you:</strong> Someone may be trying to access your account. We recommend changing your password immediately.</li>
            </ul>
        </div>
        <div class='footer'>
            <p>This is an automated security alert from RRC Management System.</p>
            <p>If you have questions, please contact our support team.</p>
        </div>
    </div>
</body>
</html>";

                SendEmail(email, subject, body, true, MailPriority.High);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SendSecurityAlertToUser Error: {ex.Message}");
            }
        }

        private void SendSecurityAlertToSuperAdmin(string targetEmail, int attemptCount, string ip, EnhancedDeviceInfo deviceInfo, DateTime attemptTime)
        {
            try
            {
                var superAdminEmails = GetSuperAdminEmails();

                if (superAdminEmails.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No SuperAdmin emails found for security alert");
                    return;
                }

                string subject = $"🚨 Security Alert: Failed Login Attempts on {targetEmail}";

                string body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 700px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #d32f2f; color: white; padding: 20px; text-align: center; }}
        .content {{ background: #f9f9f9; padding: 20px; }}
        .alert-box {{ background: #ffebee; border-left: 4px solid #d32f2f; padding: 15px; margin: 15px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>🚨 Security Incident Alert</h2>
        </div>
        <div class='content'>
            <p>Dear System Admin,</p>
            <div class='alert-box'>
                <strong>Multiple failed login attempts detected</strong><br/>
                Account: {targetEmail}<br/>
                Attempts: {attemptCount}<br/>
                IP: {ip}<br/>
                Time: {attemptTime.ToString("MMMM dd, yyyy 'at' hh:mm:ss tt")}
            </div>
        </div>
    </div>
</body>
</html>";

                foreach (string adminEmail in superAdminEmails)
                {
                    SendEmail(adminEmail, subject, body, true, MailPriority.High);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SendSecurityAlertToSuperAdmin Error: {ex.Message}");
            }
        }

        private void SendLockoutAlertToSuperAdmin(string targetEmail, string ip, EnhancedDeviceInfo deviceInfo, DateTime lockoutTime, int totalAttempts)
        {
            try
            {
                var superAdminEmails = GetSuperAdminEmails();

                if (superAdminEmails.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No SuperAdmin emails found for lockout alert");
                    return;
                }

                string subject = $"🚨 CRITICAL: Account LOCKED - {targetEmail}";

                string body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; color: #333; }}
        .container {{ max-width: 700px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #b71c1c; color: white; padding: 20px; text-align: center; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>🔒 ACCOUNT LOCKOUT ALERT</h2>
        </div>
        <div style='padding: 20px;'>
            <p>Account {targetEmail} has been LOCKED after {totalAttempts} failed attempts.</p>
            <p>IP: {ip}</p>
            <p>Time: {lockoutTime.ToString("MMMM dd, yyyy 'at' hh:mm:ss tt")}</p>
        </div>
    </div>
</body>
</html>";

                foreach (string adminEmail in superAdminEmails)
                {
                    SendEmail(adminEmail, subject, body, true, MailPriority.High);
                }

                System.Diagnostics.Debug.WriteLine($"CRITICAL lockout alert sent to SuperAdmins for account: {targetEmail}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SendLockoutAlertToSuperAdmin Error: {ex.Message}");
            }
        }

        private void SendLockoutAlertToUser(string email, string ip, EnhancedDeviceInfo deviceInfo, DateTime lockoutTime)
        {
            try
            {
                string subject = "🔒 Your Account Has Been Temporarily Locked";

                string body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #d32f2f; color: white; padding: 20px; text-align: center; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>🔒 Account Locked</h2>
        </div>
        <div style='padding: 20px;'>
            <p>Your account has been temporarily locked due to multiple failed login attempts.</p>
            <p>Unlock time: {lockoutTime.AddMinutes(DeviceLockoutMinutes).ToString("hh:mm tt")}</p>
            <p>If this wasn't you, please reset your password immediately.</p>
        </div>
    </div>
</body>
</html>";

                SendEmail(email, subject, body, true, MailPriority.High);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SendLockoutAlertToUser Error: {ex.Message}");
            }
        }

        private List<string> GetSuperAdminEmails()
        {
            var emails = new List<string>();

            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("SELECT Email FROM Users WHERE Role = 'SuperAdmin' AND Status IN ('Active', 'Available')", conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string emailEncrypted = reader["Email"].ToString();
                            if (!string.IsNullOrEmpty(emailEncrypted))
                            {
                                try
                                {
                                    string decrypted = AESHelper.DecryptEmail(emailEncrypted);
                                    emails.Add(decrypted);
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"GetSuperAdminEmails - Decryption Error: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetSuperAdminEmails Error: {ex.Message}");
            }

            return emails;
        }

        private void SendEmail(string toEmail, string subject, string body, bool isHtml = false, MailPriority priority = MailPriority.Normal)
        {
            try
            {
                using (var message = new MailMessage())
                {
                    message.From = new MailAddress("rrctermiteandpestcontrol@gmail.com", "RRC Security System");
                    message.To.Add(toEmail);
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = isHtml;
                    message.Priority = priority;

                    using (var smtp = new SmtpClient())
                    {
                        smtp.Timeout = 10000;
                        smtp.Send(message);
                    }
                }

                System.Diagnostics.Debug.WriteLine($"✅ Email sent successfully to: {toEmail}");
            }
            catch (SmtpException smtpEx)
            {
                System.Diagnostics.Debug.WriteLine($"❌ SMTP Error: {smtpEx.StatusCode} - {smtpEx.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ SendEmail Error: {ex.Message}");
            }
        }

        private EnhancedLockoutInfo CheckAllLockoutLayers(
            string deviceFingerprint,
            string canvasFingerprint,
            string emailHash,
            string ipSubnet,
            EnhancedDeviceInfo deviceInfo)
        {
            EnhancedLockoutInfo info = new EnhancedLockoutInfo();

            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spDeviceLockout_CheckAllLayers", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@DeviceFingerprint", SqlDbType.NVarChar, 500).Value = deviceFingerprint;
                    cmd.Parameters.Add("@CanvasFingerprint", SqlDbType.NVarChar, 500).Value = (object)canvasFingerprint ?? DBNull.Value;
                    cmd.Parameters.Add("@HardwareID", SqlDbType.NVarChar, 64).Value = (object)deviceInfo.HardwareID ?? DBNull.Value;
                    cmd.Parameters.Add("@EmailHash", SqlDbType.NVarChar, 64).Value = (object)emailHash ?? DBNull.Value;
                    cmd.Parameters.Add("@IPSubnet", SqlDbType.NVarChar, 20).Value = (object)ipSubnet ?? DBNull.Value;
                    cmd.Parameters.Add("@ScreenResolution", SqlDbType.NVarChar, 50).Value = (object)deviceInfo.ScreenResolution ?? DBNull.Value;
                    cmd.Parameters.Add("@Platform", SqlDbType.NVarChar, 100).Value = (object)deviceInfo.Platform ?? DBNull.Value;
                    cmd.Parameters.Add("@HardwareConcurrency", SqlDbType.Int).Value = (object)deviceInfo.HardwareConcurrency ?? DBNull.Value;

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            info.IsLocked = reader["IsLocked"] != DBNull.Value && Convert.ToBoolean(reader["IsLocked"]);
                            info.LockedUntil = reader["LockedUntil"] != DBNull.Value ? (DateTime?)reader["LockedUntil"] : null;
                            info.LockoutReason = reader["LockoutReason"]?.ToString() ?? "";
                            info.FailedAttempts = reader["FailedAttempts"] != DBNull.Value ? Convert.ToInt32(reader["FailedAttempts"]) : 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CheckAllLockoutLayers Error: {ex.Message}");
            }

            return info;
        }

        private void RecordFailedAttemptAllLayers(EnhancedDeviceInfo deviceInfo, string emailHash, string ipSubnet, string ip, string userAgent)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spDeviceLockout_RecordFailure", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@DeviceFingerprint", SqlDbType.NVarChar, 500).Value = deviceInfo.DeviceFingerprint;
                    cmd.Parameters.Add("@CanvasFingerprint", SqlDbType.NVarChar, 500).Value = (object)deviceInfo.CanvasFingerprint ?? DBNull.Value;
                    cmd.Parameters.Add("@HardwareID", SqlDbType.NVarChar, 64).Value = (object)deviceInfo.HardwareID ?? DBNull.Value;
                    cmd.Parameters.Add("@EmailHash", SqlDbType.NVarChar, 64).Value = (object)emailHash ?? DBNull.Value;
                    cmd.Parameters.Add("@IPSubnet", SqlDbType.NVarChar, 20).Value = (object)ipSubnet ?? DBNull.Value;
                    cmd.Parameters.Add("@LockoutMinutes", SqlDbType.Int).Value = DeviceLockoutMinutes;
                    cmd.Parameters.Add("@MaxAttempts", SqlDbType.Int).Value = MaxAttempts;

                    cmd.Parameters.Add("@LastIPAddress", SqlDbType.NVarChar, 50).Value = ip;
                    cmd.Parameters.Add("@UserAgent", SqlDbType.NVarChar, 500).Value = userAgent.Length > 500 ? userAgent.Substring(0, 500) : userAgent;
                    cmd.Parameters.Add("@OSInfo", SqlDbType.NVarChar, 150).Value = deviceInfo.OSInfo;
                    cmd.Parameters.Add("@BrowserName", SqlDbType.NVarChar, 100).Value = deviceInfo.BrowserName;
                    cmd.Parameters.Add("@ScreenResolution", SqlDbType.NVarChar, 50).Value = deviceInfo.ScreenResolution;
                    cmd.Parameters.Add("@TimezoneOffset", SqlDbType.NVarChar, 10).Value = deviceInfo.TimezoneOffset;
                    cmd.Parameters.Add("@Language", SqlDbType.NVarChar, 50).Value = deviceInfo.Language;
                    cmd.Parameters.Add("@HardwareConcurrency", SqlDbType.Int).Value = (object)deviceInfo.HardwareConcurrency ?? DBNull.Value;
                    cmd.Parameters.Add("@ColorDepth", SqlDbType.Int).Value = (object)deviceInfo.ColorDepth ?? DBNull.Value;
                    cmd.Parameters.Add("@DeviceMemory", SqlDbType.Int).Value = (object)deviceInfo.DeviceMemory ?? DBNull.Value;
                    cmd.Parameters.Add("@MaxTouchPoints", SqlDbType.Int).Value = (object)deviceInfo.MaxTouchPoints ?? DBNull.Value;
                    cmd.Parameters.Add("@Platform", SqlDbType.NVarChar, 100).Value = deviceInfo.Platform;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RecordFailedAttemptAllLayers Error: {ex.Message}");
            }
        }

        private void ResetAllLockoutLayers(string deviceFingerprint, string canvasFingerprint, string emailHash, string ipSubnet, string hardwareID)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spDeviceLockout_ResetAllLayers", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@DeviceFingerprint", SqlDbType.NVarChar, 500).Value = deviceFingerprint;
                    cmd.Parameters.Add("@CanvasFingerprint", SqlDbType.NVarChar, 500).Value = (object)canvasFingerprint ?? DBNull.Value;
                    cmd.Parameters.Add("@HardwareID", SqlDbType.NVarChar, 64).Value = (object)hardwareID ?? DBNull.Value;
                    cmd.Parameters.Add("@EmailHash", SqlDbType.NVarChar, 64).Value = (object)emailHash ?? DBNull.Value;
                    cmd.Parameters.Add("@IPSubnet", SqlDbType.NVarChar, 20).Value = (object)ipSubnet ?? DBNull.Value;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ResetAllLockoutLayers Error: {ex.Message}");
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
    }

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