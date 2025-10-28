using RRCManagementSystem.Helpers;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class ClientVerifyOTP : Page
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Prevent caching
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));

            if (!IsPostBack)
            {
                // Check if user has pending OTP verification session
                if (Session["PendingClientOTP_ClientID"] == null)
                {
                    // No pending verification, redirect to login
                    Response.Redirect("Login.aspx", false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }

                // Load OTP expiration time
                LoadOTPExpirationTime();
            }

            // Check if already authenticated (shouldn't happen, but just in case)
            if (Session["ClientID"] != null)
            {
                Response.Redirect("Home.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
        }

        private void LoadOTPExpirationTime()
        {
            try
            {
                int? clientId = Session["PendingClientOTP_ClientID"] as int?;
                string deviceFingerprint = Session["PendingClientOTP_DeviceFingerprint"] as string;

                if (clientId.HasValue && !string.IsNullOrEmpty(deviceFingerprint))
                {
                    using (var conn = new SqlConnection(connectionString))
                    using (var cmd = new SqlCommand("dbo.spClientOTP_GetStatus", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId.Value;
                        cmd.Parameters.Add("@DeviceFingerprint", SqlDbType.NVarChar, 500).Value = deviceFingerprint;

                        conn.Open();
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                DateTime expiresAt = Convert.ToDateTime(reader["ExpiresAt"]);
                                hiddenExpiresAt.Value = expiresAt.ToString("o"); // ISO 8601 format
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadOTPExpirationTime Error: {ex.Message}");
            }
        }

        protected void btnVerify_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "";
            string otpCode = txtOTPCode.Text.Trim();

            // Validate input
            if (string.IsNullOrEmpty(otpCode))
            {
                lblMessage.Text = "**Required Field.** Please enter the verification code.";
                return;
            }

            if (otpCode.Length != 6 || !System.Text.RegularExpressions.Regex.IsMatch(otpCode, @"^\d{6}$"))
            {
                lblMessage.Text = "**Invalid Code.** Please enter a valid 6-digit code.";
                return;
            }

            // Get session data
            int? clientId = Session["PendingClientOTP_ClientID"] as int?;
            string email = Session["PendingClientOTP_Email"] as string;
            string name = Session["PendingClientOTP_Name"] as string;
            string deviceFingerprint = Session["PendingClientOTP_DeviceFingerprint"] as string;
            string canvasFingerprint = Session["PendingClientOTP_CanvasFingerprint"] as string;
            string hardwareID = Session["PendingClientOTP_HardwareID"] as string;

            if (!clientId.HasValue || string.IsNullOrEmpty(deviceFingerprint))
            {
                lblMessage.Text = "**Session Error.** Your session has expired. Please log in again.";
                return;
            }

            try
            {
                // Verify OTP code
                bool isValid = false;
                string message = "";

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spClientOTP_Verify", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId.Value;
                    cmd.Parameters.Add("@OTPCode", SqlDbType.Char, 6).Value = otpCode;
                    cmd.Parameters.Add("@DeviceFingerprint", SqlDbType.NVarChar, 500).Value = deviceFingerprint;
                    cmd.Parameters.Add("@MaxAttempts", SqlDbType.Int).Value = 5;

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            isValid = reader["IsValid"] != DBNull.Value && Convert.ToBoolean(reader["IsValid"]);
                            message = reader["Message"]?.ToString() ?? "";
                        }
                    }
                }

                if (isValid)
                {
                    // ✅ OTP VERIFIED - Add device as trusted and complete login
                    AddTrustedDevice(clientId.Value, deviceFingerprint, canvasFingerprint, hardwareID);

                    // Create session
                    Guid newSessionID = Guid.NewGuid();
                    using (var conn = new SqlConnection(connectionString))
                    using (var cmd = new SqlCommand(@"
                        UPDATE dbo.Clients
                        SET CurrentSessionID = @SessionID,
                            CurrentSessionAt = GETDATE()
                        WHERE ClientID = @ClientID", conn))
                    {
                        cmd.Parameters.AddWithValue("@SessionID", newSessionID);
                        cmd.Parameters.AddWithValue("@ClientID", clientId.Value);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }

                    // Set session variables
                    Session["ClientID"] = clientId.Value;
                    Session["ClientName"] = name;
                    Session["Email"] = email;
                    Session["Role"] = "Client";
                    Session["SessionID"] = newSessionID;

                    // Clear pending OTP session data
                    Session.Remove("PendingClientOTP_ClientID");
                    Session.Remove("PendingClientOTP_Email");
                    Session.Remove("PendingClientOTP_Name");
                    Session.Remove("PendingClientOTP_DeviceFingerprint");
                    Session.Remove("PendingClientOTP_CanvasFingerprint");
                    Session.Remove("PendingClientOTP_HardwareID");


                    // Redirect to home
                    Response.Redirect("Home.aspx", false);
                    Context.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    // ❌ VERIFICATION FAILED
                    lblMessage.Text = $"**Verification Failed.** {message}";
                    txtOTPCode.Text = ""; // Clear input
                    txtOTPCode.Focus();
                }
            }
            catch (SqlException sqlEx)
            {
                System.Diagnostics.Debug.WriteLine($"OTP Verification SQL Error: {sqlEx.Message}");
                lblMessage.Text = "**System Error.** An unexpected database error occurred. Please try again.";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OTP Verification Error: {ex.Message}");
                lblMessage.Text = "**System Error.** An unexpected error occurred. Please try again.";
            }
        }

        protected void btnResend_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "";

            // Get session data
            int? clientId = Session["PendingClientOTP_ClientID"] as int?;
            string email = Session["PendingClientOTP_Email"] as string;
            string deviceFingerprint = Session["PendingClientOTP_DeviceFingerprint"] as string;
            string canvasFingerprint = Session["PendingClientOTP_CanvasFingerprint"] as string;
            string hardwareID = Session["PendingClientOTP_HardwareID"] as string;

            if (!clientId.HasValue || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(deviceFingerprint))
            {
                lblMessage.Text = "**Session Error.** Your session has expired. Please log in again.";
                return;
            }

            try
            {
                string ip = Request.UserHostAddress ?? "";
                string userAgent = Request.UserAgent ?? "";

                // Request new OTP with rate limiting
                bool success = false;
                string message = "";
                string newOTPCode = "";
                DateTime expiresAt = DateTime.Now;

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spClientOTP_Resend", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId.Value;
                    cmd.Parameters.Add("@DeviceFingerprint", SqlDbType.NVarChar, 500).Value = deviceFingerprint;
                    cmd.Parameters.Add("@CanvasFingerprint", SqlDbType.NVarChar, 500).Value = (object)canvasFingerprint ?? DBNull.Value;
                    cmd.Parameters.Add("@HardwareID", SqlDbType.NVarChar, 64).Value = (object)hardwareID ?? DBNull.Value;
                    cmd.Parameters.Add("@IPAddress", SqlDbType.NVarChar, 50).Value = ip;
                    cmd.Parameters.Add("@UserAgent", SqlDbType.NVarChar, 500).Value = userAgent;
                    cmd.Parameters.Add("@ValidityMinutes", SqlDbType.Int).Value = 10;
                    cmd.Parameters.Add("@MinResendIntervalSeconds", SqlDbType.Int).Value = 60;

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (reader["Success"] != DBNull.Value)
                            {
                                success = Convert.ToBoolean(reader["Success"]);
                            }
                            else
                            {
                                // Success not returned means OTP was generated
                                success = true;
                            }

                            message = reader["Message"]?.ToString() ?? "";

                            if (reader["OTPCode"] != DBNull.Value)
                            {
                                newOTPCode = reader["OTPCode"].ToString();
                            }

                            if (reader["ExpiresAt"] != DBNull.Value)
                            {
                                expiresAt = Convert.ToDateTime(reader["ExpiresAt"]);
                            }
                        }
                    }
                }

                if (success && !string.IsNullOrEmpty(newOTPCode))
                {
                    // Send new OTP email
                    SendResendOTPEmail(email, newOTPCode, expiresAt);

                    // Update expiration time on page
                    hiddenExpiresAt.Value = expiresAt.ToString("o");

                    lblMessage.Text = "**Code Resent.** A new verification code has been sent to your email address.";
                    txtOTPCode.Text = "";
                    txtOTPCode.Focus();
                }
                else
                {
                    lblMessage.Text = $"**Resend Failed.** {message}";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Resend OTP Error: {ex.Message}");
                lblMessage.Text = "**System Error.** Unable to resend code. Please try again.";
            }
        }

        private void AddTrustedDevice(int clientId, string deviceFingerprint, string canvasFingerprint, string hardwareID)
        {
            try
            {
                string ip = Request.UserHostAddress ?? "";
                string userAgent = Request.UserAgent ?? "";

                // Get device info from User-Agent
                string browserName = GetBrowserName(userAgent);
                string osInfo = GetOSInfo(userAgent);
                string platform = GetPlatform(userAgent);
                string deviceName = $"{browserName} on {osInfo}";

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spClientTrustedDevice_Add", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                    cmd.Parameters.Add("@DeviceFingerprint", SqlDbType.NVarChar, 500).Value = deviceFingerprint;
                    cmd.Parameters.Add("@CanvasFingerprint", SqlDbType.NVarChar, 500).Value = (object)canvasFingerprint ?? DBNull.Value;
                    cmd.Parameters.Add("@HardwareID", SqlDbType.NVarChar, 64).Value = (object)hardwareID ?? DBNull.Value;
                    cmd.Parameters.Add("@DeviceName", SqlDbType.NVarChar, 200).Value = deviceName;
                    cmd.Parameters.Add("@OSInfo", SqlDbType.NVarChar, 150).Value = osInfo;
                    cmd.Parameters.Add("@BrowserName", SqlDbType.NVarChar, 100).Value = browserName;
                    cmd.Parameters.Add("@Platform", SqlDbType.NVarChar, 100).Value = platform;
                    cmd.Parameters.Add("@IPAddress", SqlDbType.NVarChar, 50).Value = ip;
                    cmd.Parameters.Add("@UserAgent", SqlDbType.NVarChar, 500).Value = userAgent;
                    cmd.Parameters.Add("@TrustDurationDays", SqlDbType.Int).Value = 30;
                    cmd.Parameters.Add("@MaxDevicesPerClient", SqlDbType.Int).Value = 2;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                System.Diagnostics.Debug.WriteLine($"✅ Device trusted for ClientID {clientId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddTrustedDevice Error: {ex.Message}");
            }
        }

        private void SendResendOTPEmail(string email, string otpCode, DateTime expiresAt)
        {
            try
            {
                string subject = "New Device Verification Code - RRC Management System";

                string body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: 'Poppins', Arial, sans-serif; line-height: 1.6; color: #333; background: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 20px auto; background: white; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 20px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #28a745 0%, #20803a 100%); color: white; padding: 30px; text-align: center; }}
        .header h1 {{ margin: 0; font-size: 24px; font-weight: 600; }}
        .content {{ padding: 40px 30px; }}
        .otp-box {{ background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%); border: 2px solid #28a745; border-radius: 10px; padding: 30px; text-align: center; margin: 25px 0; }}
        .otp-code {{ font-size: 36px; font-weight: 700; color: #28a745; letter-spacing: 8px; margin: 10px 0; font-family: 'Courier New', monospace; }}
        .validity {{ color: #666; font-size: 14px; margin-top: 10px; }}
        .info-box {{ background: #d1ecf1; border-left: 4px solid #17a2b8; padding: 15px; margin: 20px 0; border-radius: 4px; }}
        .footer {{ background: #f1f1f1; padding: 20px; text-align: center; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔄 New Verification Code</h1>
        </div>
        <div class='content'>
            <p>Hello,</p>
            <p>You requested a new verification code. Here's your new code:</p>
            
            <div class='otp-box'>
                <div style='font-size: 14px; color: #666; text-transform: uppercase; letter-spacing: 1px; font-weight: 600;'>Your New Verification Code</div>
                <div class='otp-code'>{otpCode}</div>
                <div class='validity'>⏱️ Valid for 10 minutes (until {expiresAt:hh:mm tt})</div>
            </div>

            <div class='info-box'>
                <strong>📌 Note:</strong> This code replaces your previous verification code.
            </div>

            <p>If you didn't request this code, please ignore this email and contact support if you're concerned about your account security.</p>
        </div>
        <div class='footer'>
            <p>This is an automated message from <strong>RRC Management System</strong></p>
            <p>Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";

                SendEmail(email, subject, body, true, MailPriority.High);
                System.Diagnostics.Debug.WriteLine($"✅ Resend OTP email sent to: {email}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SendResendOTPEmail Error: {ex.Message}");
            }
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

   

        // Helper methods to parse User-Agent string
        private string GetBrowserName(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return "Unknown";

            if (userAgent.Contains("Edg/")) return "Edge";
            if (userAgent.Contains("Chrome/") && !userAgent.Contains("Edg/")) return "Chrome";
            if (userAgent.Contains("Firefox/")) return "Firefox";
            if (userAgent.Contains("Safari/") && !userAgent.Contains("Chrome")) return "Safari";
            if (userAgent.Contains("Opera") || userAgent.Contains("OPR/")) return "Opera";
            if (userAgent.Contains("MSIE") || userAgent.Contains("Trident/")) return "Internet Explorer";

            return "Other";
        }

        private string GetOSInfo(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return "Unknown";

            if (userAgent.Contains("Windows NT 10.0")) return "Windows 10/11";
            if (userAgent.Contains("Windows NT 6.3")) return "Windows 8.1";
            if (userAgent.Contains("Windows NT 6.2")) return "Windows 8";
            if (userAgent.Contains("Windows NT 6.1")) return "Windows 7";
            if (userAgent.Contains("Windows")) return "Windows";

            if (userAgent.Contains("Mac OS X")) return "macOS";
            if (userAgent.Contains("Macintosh")) return "Mac";

            if (userAgent.Contains("Android")) return "Android";
            if (userAgent.Contains("iPhone") || userAgent.Contains("iPad")) return "iOS";

            if (userAgent.Contains("Linux")) return "Linux";
            if (userAgent.Contains("Ubuntu")) return "Ubuntu";

            return "Other";
        }

        private string GetPlatform(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return "Unknown";

            if (userAgent.Contains("Mobile") || userAgent.Contains("Android") ||
                userAgent.Contains("iPhone") || userAgent.Contains("iPad"))
                return "Mobile";

            if (userAgent.Contains("Tablet"))
                return "Tablet";

            return "Desktop";
        }
    }
}