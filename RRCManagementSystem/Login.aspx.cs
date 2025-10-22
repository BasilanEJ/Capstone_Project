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
            string ipSubnet = GetIPSubnet(ip);
            string userAgent = Request.UserAgent ?? "";

            EnhancedDeviceInfo deviceInfo = ParseEnhancedDeviceInfo();

            if (string.IsNullOrEmpty(deviceInfo.DeviceFingerprint))
            {
                lblMessage.Text = "**System Error.** Unable to identify your device. Please refresh the page and try again.";
                return;
            }

            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text.Trim();

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
                lblMessage.Text = "**Password Policy.** Your password must contain at least one uppercase letter, one lowercase letter, one number, and one special character (e.g., @$!%*?&.).";
                return;
            }

            string emailHash = AESHelper.ComputeSHA256(email);

            // Check lockout status
            EnhancedLockoutInfo lockoutInfo = CheckAllLockoutLayers(
                deviceInfo.DeviceFingerprint,
                deviceInfo.CanvasFingerprint,
                emailHash,
                ipSubnet,
                deviceInfo
            );

            // FIX: Handle expired lockouts properly
            if (lockoutInfo.IsLocked)
            {
                // Check if the lockout has actually expired
                if (lockoutInfo.LockedUntil.HasValue && lockoutInfo.LockedUntil.Value <= DateTime.Now)
                {
                    // Lockout has expired - reset it and allow login attempt to continue
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
                    // Lockout is still active - block the attempt
                    pnlCaptcha.Visible = true;
                    string lockoutMessage = lockoutInfo.LockoutReason == "Device"
                        ? "**Device Blocked.** Too many failed attempts from this device."
                        : lockoutInfo.LockoutReason == "Email"
                        ? "**Account Locked.** Too many failed attempts for this email address."
                        : "**Network Blocked.** Too many failed attempts from this network.";

                    lblMessage.Text = $"{lockoutMessage} Please try again after {lockoutInfo.LockedUntil.Value:hh:mm tt}.";
                    return;
                }
            }

            // Check IP-based lockout
            if (GetFailedIPAttempts(ip, IpWindowMinutes) >= 5)
            {
                DateTime ipLockoutExpires = DateTime.Now.AddMinutes(IpWindowMinutes);
                lblMessage.Text = $"**Access Denied.** Too many failed login attempts from this IP address. Please try again after {ipLockoutExpires:hh:mm tt}.";
                return;
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

                            if (lockoutObj != DBNull.Value && Convert.ToDateTime(lockoutObj) > DateTime.Now)
                            {
                                pnlCaptcha.Visible = true;
                                lblMessage.Text = $"**Account Locked.** This account is temporarily locked. Please try again after {Convert.ToDateTime(lockoutObj):hh:mm tt}.";
                                goto FailureCleanup;
                            }

                            if (failedAttempts >= 5)
                            {
                                pnlCaptcha.Visible = true;
                                string captchaResponse = Request.Form["g-recaptcha-response"];

                                if (string.IsNullOrEmpty(captchaResponse) || !IsCaptchaValid())
                                {
                                    lblMessage.Text = "**Security Check.** Due to multiple failed attempts, please complete the CAPTCHA to continue.";
                                    goto FailureCleanup;
                                }
                            }

                            if (!string.IsNullOrEmpty(hash) && PasswordHelper.VerifyPassword(hash, password))
                            {
                                // Successful login
                                loginSuccessful = true;
                                ResetFailedLogin(userID);

                                // Reset all device/IP lockouts
                                ResetAllLockoutLayers(
                                    deviceInfo.DeviceFingerprint,
                                    deviceInfo.CanvasFingerprint,
                                    emailHash,
                                    ipSubnet,
                                    deviceInfo.HardwareID
                                );

                                LogIPAttempt(ip, true);
                                AddAuditLog(userID, $"{role} {userName} passed password; 2FA pending.");

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
                                // Wrong password
                                HandleFailedLogin(userID);

                                int remaining = Math.Max(0, MaxAttempts - (failedAttempts + 1));
                                lblMessage.Text = $"**Login Failed.** Invalid email address or password. You have {remaining} attempt(s) remaining before your account is locked.";

                                if (failedAttempts + 1 >= 5)
                                    pnlCaptcha.Visible = true;

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
                goto FailureCleanup;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
                lblMessage.Text = "**System Error.** An unexpected error occurred. Please try again later. If the problem persists, contact support.";
                goto FailureCleanup;
            }

        FailureCleanup:
            LogIPAttempt(ip, false);
            RecordFailedAttemptAllLayers(deviceInfo, emailHash, ipSubnet, ip, userAgent);
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

                            // FIX: Check if lockout has expired before blocking
                            if (lockoutObj != null && lockoutObj != DBNull.Value)
                            {
                                DateTime lockoutTime = Convert.ToDateTime(lockoutObj);

                                if (lockoutTime > DateTime.Now)
                                {
                                    // Still locked
                                    pnlCaptcha.Visible = true;
                                    lblMessage.Text = $"**Account Locked.** This client account is temporarily locked. Please try again after {lockoutTime:hh:mm tt}.";
                                    goto ClientFailureCleanup;
                                }
                                // If lockout has expired, continue with login attempt (don't block)
                            }

                            if (failedAttempts >= 5)
                            {
                                pnlCaptcha.Visible = true;
                                string captchaResponse = Request.Form["g-recaptcha-response"];
                                if (string.IsNullOrEmpty(captchaResponse) || !IsCaptchaValid())
                                {
                                    lblMessage.Text = "**Security Check.** Due to multiple failed attempts, please complete the CAPTCHA to continue.";
                                    goto ClientFailureCleanup;
                                }
                            }

                            if (!string.IsNullOrEmpty(hash) && PasswordHelper.VerifyPassword(hash, password))
                            {
                                // Successful login
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

                                AddAuditLog(clientId, $"Client {name} logged in (single session started).");

                                Response.Redirect("Home.aspx", false);
                                Context.ApplicationInstance.CompleteRequest();
                                return;
                            }
                            else
                            {
                                // Wrong password
                                HandleClientFailedLogin(clientId);

                                int remaining = Math.Max(0, MaxAttempts - (failedAttempts + 1));
                                lblMessage.Text = $"**Login Failed.** Invalid email address or password. You have {remaining} attempt(s) remaining before your account is locked.";

                                if (failedAttempts + 1 >= 5)
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

        // =============================================
        // Existing Helper Methods (Legacy Support)
        // =============================================

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