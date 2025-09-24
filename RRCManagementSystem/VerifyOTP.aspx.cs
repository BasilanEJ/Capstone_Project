using RRCManagementSystem.Helpers;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;

namespace RRCManagementSystem
{
    public partial class VerifyOTP : System.Web.UI.Page
    {
        private readonly string cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                lblMessage.Text = "";
            }
        }

        protected void btnVerifyOTP_Click(object sender, EventArgs e)
        {
            string enteredOTP = txtOTP.Text.Trim();
            string sessionOTP = Session["OTP"]?.ToString();
            DateTime? expiry = Session["OTP_Expiry"] as DateTime?;
            string email = Session["OTP_Email"] as string; // plain email from ForgotPassword

            // 1. Validate OTP entry
            if (string.IsNullOrWhiteSpace(enteredOTP))
            {
                lblMessage.Text = "⚠ Please enter the OTP.";
                return;
            }

            // 2. Ensure session values exist
            if (sessionOTP == null || expiry == null || string.IsNullOrWhiteSpace(email))
            {
                lblMessage.Text = "⚠ Session expired. Please request a new OTP.";
                Response.Redirect("ForgotPassword.aspx", endResponse: false);
                return;
            }

            // 3. Check OTP expiration
            if (DateTime.Now > expiry.Value)
            {
                lblMessage.Text = "⚠ OTP has expired. Please request a new one.";
                Response.Redirect("ForgotPassword.aspx", endResponse: false);
                return;
            }

            // 4. Check OTP match
            if (!string.Equals(enteredOTP, sessionOTP, StringComparison.Ordinal))
            {
                lblMessage.Text = "⚠ Invalid OTP. Please try again.";
                return;
            }

            // 5. OTP is correct → move to reset link generation
            try
            {
                string accountType = GetAccountTypeByEmail(email);

                if (accountType == null)
                {
                    lblMessage.Text = "⚠ Account not found for this email.";
                    return;
                }

                // OTP verified successfully
                Session["IsOTPVerified"] = true;

                // Clear OTP session data
                Session.Remove("OTP");
                Session.Remove("OTP_Expiry");
                Session.Remove("OTP_Email");

                // Generate reset token and URL
                string resetUrl = IssueResetTokenAndGetUrl(email, accountType);

                // Send reset link via email
                SendResetLinkEmail(email, resetUrl);

                // Redirect user to the reset page directly
                Response.Redirect(resetUrl, endResponse: false);
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠ Could not issue reset link. Please try again.<br/>" + ex.Message;
            }
        }

        // ============================================================
        // ACCOUNT LOOKUP
        // ============================================================
        private string GetAccountTypeByEmail(string email)
        {
            // Hashes used to find account in respective tables
            string userHash = AESHelper.ComputeSHA256(email.ToLowerInvariant());            // plain hash for Users
            string clientHash = AESHelper.ComputeSHA256WithPepper(email.ToLowerInvariant()); // peppered hash for Clients

            using (var con = new SqlConnection(cs))
            {
                con.Open();

                // Check Users table
                using (var cmd = new SqlCommand(
                    "IF EXISTS (SELECT 1 FROM dbo.Users WHERE EmailHash = @H) SELECT 1 ELSE SELECT 0", con))
                {
                    cmd.Parameters.AddWithValue("@H", userHash);
                    if ((int)cmd.ExecuteScalar() == 1)
                        return "User";
                }

                // Check Clients table
                using (var cmd = new SqlCommand(
                    "IF EXISTS (SELECT 1 FROM dbo.Clients WHERE EmailHash = @H) SELECT 1 ELSE SELECT 0", con))
                {
                    cmd.Parameters.AddWithValue("@H", clientHash);
                    if ((int)cmd.ExecuteScalar() == 1)
                        return "Client";
                }
            }

            return null; // No match found
        }

        // ============================================================
        // TOKEN GENERATION & RESET URL
        // ============================================================

        /// <summary>
        /// Generate a secure random token (~32 chars)
        /// </summary>
        private static string NewToken()
        {
            var bytes = new byte[24];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
        }

        /// <summary>
        /// Call stored procedure to store token and return reset URL.
        /// </summary>
        private string IssueResetTokenAndGetUrl(string email, string accountType)
        {
            string token = NewToken();
            DateTime expiry = DateTime.Now.AddMinutes(15);

            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(
                accountType == "User" ? "dbo.spResetToken_IssueUser" : "dbo.spResetToken_IssueClient", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                if (accountType == "User")
                {
                    // ✅ Users: plain SHA-256 hash of lowercase email
                    string userHash = AESHelper.ComputeSHA256(email.ToLowerInvariant());
                    cmd.Parameters.AddWithValue("@EmailHash", userHash);
                }
                else
                {
                    // ✅ Clients: SHA-256 with pepper
                    string clientHash = AESHelper.ComputeSHA256WithPepper(email.ToLowerInvariant());
                    cmd.Parameters.AddWithValue("@EmailHash", clientHash);
                }

                cmd.Parameters.AddWithValue("@Token", token);
                cmd.Parameters.AddWithValue("@Expiry", expiry);

                con.Open();
                int rows = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                if (rows == 0)
                    throw new Exception("Account not found to issue token.");
            }

            // Build full reset URL
            string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
            string path = accountType == "User" ? "~/ResetAdminPassword.aspx" : "~/ResetPassword.aspx";
            return baseUrl + ResolveUrl(path) + "?token=" + token;
        }

        // ============================================================
        // EMAIL SENDING
        // ============================================================
        private void SendResetLinkEmail(string recipientEmail, string resetUrl)
        {
            var body = $@"
<!DOCTYPE html>
<html>
<body style='font-family:Arial,sans-serif;background:#f6f7f9;padding:24px;'>
  <table width='100%' cellspacing='0' cellpadding='0'>
    <tr><td align='center'>
      <table width='520' cellspacing='0' cellpadding='0' 
             style='background:#fff;border-radius:8px;box-shadow:0 4px 12px rgba(0,0,0,.08);'>
        <tr>
          <td style='background:#007bff;color:#fff;padding:16px 20px;
                     border-radius:8px 8px 0 0;font-size:18px;font-weight:600;'>
            RRC Management System
          </td>
        </tr>
        <tr>
          <td style='padding:24px;color:#333'>
            <h3 style='margin:0 0 12px 0;'>Password Reset Link</h3>
            <p>Click the button below to reset your password. This link is valid for <b>15 minutes</b>.</p>
            <p style='margin:24px 0;text-align:center;'>
              <a href='{resetUrl}' style='display:inline-block;padding:10px 16px;
                 background:#007bff;color:#fff;text-decoration:none;border-radius:6px;'>Reset Password</a>
            </p>
            <p>If the button doesn't work, copy and paste this URL into your browser:<br>{resetUrl}</p>
          </td>
        </tr>
        <tr>
          <td style='background:#f3f4f6;color:#777;padding:12px 20px;
                     border-radius:0 0 8px 8px;font-size:12px;text-align:center'>
            &copy; 2025 RRC Management System
          </td>
        </tr>
      </table>  
    </td></tr>
  </table>
</body>
</html>";

            var mail = new MailMessage
            {
                From = new MailAddress("rrctermiteandpestcontrol@gmail.com", "RRC Management System"),
                Subject = "Password Reset Link",
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
    }
}
