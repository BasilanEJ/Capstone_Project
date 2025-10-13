using OtpNet;
using RRCManagementSystem.Helpers; // AESHelper for hashing
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Script.Serialization;

namespace RRCManagementSystem
{
    public partial class VerifyTOTP : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Prevent cached back/forward navigation
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));

            if (!IsPostBack)
            {
                lblMessage.Text = "";
                lblInfo.Text = "";
                pnlCaptcha.Visible = false;

                // Clear any authenticated session values
                Session.Remove("IsAuthenticated");
                Session.Remove("UserID");
                Session.Remove("Role");
                Session.Remove("Name");
                Session.Remove("Email");

                // Must come from password step
                if (Session["Pending2FA_Email"] == null || Session["Pending2FA_UserID"] == null)
                {
                    Response.Redirect("Login.aspx", false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }

                int userID = Convert.ToInt32(Session["Pending2FA_UserID"]);
                if (IsLockedOut(userID))
                {
                    pnlCaptcha.Visible = true;
                    lblMessage.Text = "⏳ You've been locked out. Please solve CAPTCHA to continue.";
                }
            }
        }

        /* ==============================================================
           VERIFY TOTP OR EMAIL CODE
           ============================================================== */
        protected void btnVerifyTOTP_Click(object sender, EventArgs e)
        {
            if (Session["Pending2FA_Email"] == null || Session["Pending2FA_UserID"] == null)
            {
                Response.Redirect("Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            int userID = Convert.ToInt32(Session["Pending2FA_UserID"]);
            string email = Session["Pending2FA_Email"].ToString();
            string role = Session["Pending2FA_Role"].ToString();
            string name = Session["Pending2FA_Name"].ToString();

            if (pnlCaptcha.Visible && !IsCaptchaValid())
            {
                lblMessage.Text = "⚠ CAPTCHA verification failed.";
                return;
            }

            string enteredCode = txtTOTP.Value?.Trim() ?? "";
            if (enteredCode.Length != 6)
            {
                lblMessage.Text = "⚠ Please enter a valid 6-digit code.";
                return;
            }

            // Compute SHA-256 of email for lookup
            string emailHash = AESHelper.ComputeSHA256(email);
            string totpSecret = GetTOTPSecret(emailHash);

            /* 1. Check Google Authenticator Code (TOTP) */
            if (!string.IsNullOrEmpty(totpSecret))
            {
                var totp = new Totp(Base32Encoding.ToBytes(totpSecret));
                bool isValidTOTP = totp.VerifyTotp(enteredCode, out _, VerificationWindow.RfcSpecifiedNetworkDelay);

                if (isValidTOTP)
                {
                    CompleteAuthentication(userID, role, name, email, "TOTP");
                    return;
                }
            }

            /* 2. If TOTP fails, check Email Code */
            if (VerifyEmailCode(userID, enteredCode))
            {
                CompleteAuthentication(userID, role, name, email, "Email Code");
                return;
            }

            lblMessage.Text = "❌ Invalid code. Please try again.";
            pnlCaptcha.Visible = true;
        }

        /* ==============================================================
           SEND EMAIL VERIFICATION CODE
           ============================================================== */
        protected void btnSendEmailCode_Click(object sender, EventArgs e)
        {
            if (Session["Pending2FA_UserID"] == null || Session["Pending2FA_Email"] == null)
            {
                Response.Redirect("Login.aspx", false);
                return;
            }

            int userId = Convert.ToInt32(Session["Pending2FA_UserID"]);
            string email = Session["Pending2FA_Email"].ToString();

            // Generate random 6-digit code
            Random rand = new Random();
            string code = rand.Next(100000, 999999).ToString();

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Clear any previous unused codes
                using (var deleteCmd = new SqlCommand("DELETE FROM EmailVerificationCodes WHERE UserID = @UserID", conn))
                {
                    deleteCmd.Parameters.AddWithValue("@UserID", userId);
                    deleteCmd.ExecuteNonQuery();
                }

                // Insert new code
                using (var cmd = new SqlCommand(@"
                    INSERT INTO EmailVerificationCodes (UserID, Code, ExpiresAt, IsUsed, CreatedAt)
                    VALUES (@UserID, @Code, @ExpiresAt, 0, GETDATE())", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@Code", code);
                    cmd.Parameters.AddWithValue("@ExpiresAt", DateTime.Now.AddMinutes(10)); // 10 min expiration
                    cmd.ExecuteNonQuery();
                }
            }

            try
            {
                SendVerificationCodeEmail(email, code);
                lblInfo.Text = "📧 A verification code has been sent to your email.";
                lblMessage.Text = "";
            }
            catch (Exception ex)
            {
                lblMessage.Text = "❌ Failed to send email: " + ex.Message;
            }
        }

        /* ==============================================================
           COMPLETE AUTHENTICATION (UPDATED FOR SINGLE-SESSION)
           ============================================================== */
        private void CompleteAuthentication(int userID, string role, string name, string email, string method)
        {
            Guid newSessionID = Guid.NewGuid();

            // ✅ Update the DB with the new session ID
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"
                UPDATE dbo.Users
                SET CurrentSessionID = @SessionID,
                    CurrentSessionAt = GETDATE()
                WHERE UserID = @UserID", conn))
            {
                cmd.Parameters.AddWithValue("@SessionID", newSessionID);
                cmd.Parameters.AddWithValue("@UserID", userID);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            // ✅ Set final session values
            Session["UserID"] = userID;
            Session["Role"] = role;
            Session["Name"] = name;
            Session["Email"] = email;
            Session["IsAuthenticated"] = true;
            Session["SessionID"] = newSessionID;  // <-- IMPORTANT for Global.asax check

            // ✅ Clear pending 2FA session values
            Session.Remove("Pending2FA_UserID");
            Session.Remove("Pending2FA_Email");
            Session.Remove("Pending2FA_Name");
            Session.Remove("Pending2FA_Role");

            // ✅ Add audit log
            AddAuditLog(userID, $"{role} {name} completed 2FA verification via {method}.");

            // ✅ Determine redirect based on role
            string redirect;
            if (role.Equals("RootAdmin", StringComparison.OrdinalIgnoreCase))
            {
                redirect = "RootDashboard.aspx";
            }
            else if (role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                redirect = "SuperAdminDashboard.aspx";
            }
            else if (role.Equals("Inspector", StringComparison.OrdinalIgnoreCase))
            {
                redirect = "InspectorDashboard.aspx";
            }
            else
            {
                redirect = "Dashboard.aspx"; // Default for Admin and others
            }

            // ✅ Redirect safely
            Response.Redirect(redirect, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        /* ==============================================================
           VERIFY EMAIL CODE
           ============================================================== */
        private bool VerifyEmailCode(int userId, string enteredCode)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"
                SELECT TOP 1 ID 
                FROM EmailVerificationCodes
                WHERE UserID = @UserID 
                  AND Code = @Code 
                  AND IsUsed = 0 
                  AND ExpiresAt > GETDATE()
                ORDER BY CreatedAt DESC", conn))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Code", enteredCode);

                conn.Open();
                var result = cmd.ExecuteScalar();

                if (result != null)
                {
                    // Mark as used
                    using (var updateCmd = new SqlCommand("UPDATE EmailVerificationCodes SET IsUsed = 1 WHERE ID = @ID", conn))
                    {
                        updateCmd.Parameters.AddWithValue("@ID", (int)result);
                        updateCmd.ExecuteNonQuery();
                    }
                    return true;
                }
            }
            return false;
        }

        /* ==============================================================
           SEND EMAIL (HTML TEMPLATE)
           ============================================================== */
        private void SendVerificationCodeEmail(string recipientEmail, string code)
        {
            var body = $@"
<!DOCTYPE html>
<html>
<body style='font-family: Arial, sans-serif; background: #f6f7f9; padding: 24px;'>
    <div style='max-width: 520px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08); background: #ffffff;'>
        <div style='background: #007bff; color: #ffffff; padding: 16px 20px; border-radius: 8px 8px 0 0; font-size: 18px; font-weight: 600;'>
            RRC Management System
        </div>
        <div style='padding: 24px; color: #333333;'>
            <h3 style='margin: 0 0 12px 0; font-size: 20px;'>Your Verification Code</h3>
            <p style='margin: 0 0 20px 0; line-height: 1.5;'>Use the code below to complete your login. This code is valid for <b>10 minutes</b>.</p>
            <p style='font-size: 32px; font-weight: bold; color: #007bff; text-align: center; margin: 20px 0;'>{code}</p>
            <p style='margin: 20px 0; line-height: 1.5;'>If you did not request this code, please contact support immediately.</p>
        </div>
        <div style='background: #f3f4f6; color: #777777; padding: 12px 20px; border-radius: 0 0 8px 8px; font-size: 12px; text-align: center;'>
            &copy; 2025 RRC Management System
        </div>
    </div>
</body>
</html>";

            var mail = new MailMessage
            {
                From = new MailAddress("rrctermiteandpestcontrol@gmail.com", "RRC Management System"),
                Subject = "Your RRC Verification Code",
                Body = body,
                IsBodyHtml = true
            };
            mail.To.Add(recipientEmail);

            var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("rrctermiteandpestcontrol@gmail.com", "pktz jwzp tbvx qheq"), // move to web.config
                EnableSsl = true
            };

            smtp.Send(mail);
        }

        /* ==============================================================
           HELPER METHODS
           ============================================================== */
        private string GetTOTPSecret(string emailHash)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spAuth_GetTOTPSecretByEmail", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@EmailHash", SqlDbType.Char, 64).Value = emailHash ?? string.Empty;

                conn.Open();
                object result = cmd.ExecuteScalar();
                return result == null || result == DBNull.Value ? null : result.ToString();
            }
        }

        private bool IsLockedOut(int userId)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spAuth_GetLockoutUntil", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;

                conn.Open();
                object result = cmd.ExecuteScalar();
                if (result == null || result == DBNull.Value) return false;

                DateTime lockoutUntil = Convert.ToDateTime(result);
                return lockoutUntil > DateTime.Now;
            }
        }

        private void AddAuditLog(int userID, string action)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spAudit_Insert", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@AdminID", SqlDbType.Int).Value = userID;
                cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 255).Value = action ?? "";
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        /* =========================
           CAPTCHA
           ========================= */
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
}