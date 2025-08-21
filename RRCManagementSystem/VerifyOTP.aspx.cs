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
            string email = Session["OTP_Email"] as string;  // set earlier during Forgot Password

            if (string.IsNullOrWhiteSpace(enteredOTP))
            {
                lblMessage.Text = "⚠ Please enter the OTP.";
                return;
            }

            if (sessionOTP == null || expiry == null || string.IsNullOrWhiteSpace(email))
            {
                lblMessage.Text = "⚠ Session expired. Please request a new OTP.";
                Response.Redirect("ForgotPassword.aspx", endResponse: false);
                return;
            }

            if (DateTime.Now > expiry)
            {
                lblMessage.Text = "⚠ OTP has expired. Please request a new one.";
                Response.Redirect("ForgotPassword.aspx", endResponse: false);
                return;
            }

            if (!string.Equals(enteredOTP, sessionOTP, StringComparison.Ordinal))
            {
                lblMessage.Text = "⚠ Invalid OTP. Please try again.";
                return;
            }

            // ✅ OTP verified: issue a reset token, email the link, and redirect with ?token=...
            try
            {
                string accountType = GetAccountTypeByEmail(email); // "User" | "Client" | null
                if (accountType == null)
                {
                    lblMessage.Text = "⚠ Account not found for this email.";
                    return;
                }

                // Optional flag you can check on the reset pages
                Session["IsOTPVerified"] = true;

                // Invalidate the OTP so it can't be reused
                Session.Remove("OTP");
                Session.Remove("OTP_Expiry");
                Session.Remove("OTP_Email");

                // 1) Issue a new reset token in DB and get the URL to the right page
                string resetUrl = IssueResetTokenAndGetUrl(email, accountType);

                // 2) Email the reset link (also shows up in user inbox if they open it later)
                SendResetLinkEmail(email, resetUrl);

                // 3) Redirect immediately so the reset page receives ?token=...
                Response.Redirect(resetUrl, endResponse: false);
            }
            catch (Exception)
            {
                lblMessage.Text = "⚠ Could not issue reset link. Please try again.";
            }
        }

        // ---------- Helpers ----------

        /// <summary>
        /// Returns "User" if found in Users table, "Client" if found in Clients table, otherwise null.
        /// If the email exists in both, Users wins (adjust if you prefer otherwise).
        /// </summary>
        private string GetAccountTypeByEmail(string email)
        {
            using (var con = new SqlConnection(cs))
            {
                con.Open();

                // Check Users first (admins/staff)
                using (var cmd = new SqlCommand(
                    "IF EXISTS (SELECT 1 FROM dbo.Users WHERE Email = @Email) SELECT 1 ELSE SELECT 0", con))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    int inUsers = (int)cmd.ExecuteScalar();
                    if (inUsers == 1) return "User";
                }

                // Then check Clients
                using (var cmd = new SqlCommand(
                    "IF EXISTS (SELECT 1 FROM dbo.Clients WHERE Email = @Email) SELECT 1 ELSE SELECT 0", con))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    int inClients = (int)cmd.ExecuteScalar();
                    if (inClients == 1) return "Client";
                }
            }
            return null;
        }

        /// <summary>Create a URL-safe random token (length ~32 chars when base64url).</summary>
        /// <summary>Create a URL-safe random token (~32 chars when base64url).</summary>
        private static string NewToken()
        {
            var bytes = new byte[24]; // 192 bits
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);   // <- works on .NET Framework
            }

            // base64url without padding
            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
        }


        /// <summary>
        /// Writes the token to DB (via SP) and returns the full reset-page URL containing ?token=...
        /// </summary>
        private string IssueResetTokenAndGetUrl(string email, string accountType)
        {
            string token = NewToken();
            DateTime expiry = DateTime.Now.AddMinutes(15); // adjust if you want

            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(
                accountType == "User" ? "dbo.spResetToken_IssueUser" : "dbo.spResetToken_IssueClient", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Token", token);
                cmd.Parameters.AddWithValue("@Expiry", expiry);

                con.Open();
                int rows = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                if (rows == 0)
                    throw new Exception("Account not found to issue token.");
            }

            string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
            string path = accountType == "User" ? "~/ResetAdminPassword.aspx" : "~/ResetPassword.aspx";
            string url = baseUrl + ResolveUrl(path) + "?token=" + token;
            return url;
        }

        /// <summary>Email the reset link to the user.</summary>
        private void SendResetLinkEmail(string recipientEmail, string resetUrl)
        {
            var body = $@"
<!DOCTYPE html>
<html><body style='font-family:Arial,sans-serif;background:#f6f7f9;padding:24px;'>
  <table width='100%' cellspacing='0' cellpadding='0'>
    <tr><td align='center'>
      <table width='520' cellspacing='0' cellpadding='0' style='background:#fff;border-radius:8px;box-shadow:0 4px 12px rgba(0,0,0,.08);'>
        <tr>
          <td style='background:#007bff;color:#fff;padding:16px 20px;border-radius:8px 8px 0 0;font-size:18px;font-weight:600;'>
            RRC Management System
          </td>
        </tr>
        <tr>
          <td style='padding:24px;color:#333'>
            <h3 style='margin:0 0 12px 0;'>Password reset link</h3>
            <p>Click the button below to reset your password. This link is valid for <b>15 minutes</b>.</p>
            <p style='margin:24px 0;'>
              <a href='{resetUrl}' style='display:inline-block;padding:10px 16px;background:#007bff;color:#fff;
                 text-decoration:none;border-radius:6px;'>Reset Password</a>
            </p>
            <p>If the button doesn't work, copy and paste this URL into your browser:<br>{resetUrl}</p>
          </td>
        </tr>
        <tr>
          <td style='background:#f3f4f6;color:#777;padding:12px 20px;border-radius:0 0 8px 8px;font-size:12px;text-align:center'>
            &copy; 2025 RRC Management System
          </td>
        </tr>
      </table>
    </td></tr>
  </table>
</body></html>";

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
                Credentials = new NetworkCredential("rrctermiteandpestcontrol@gmail.com", "pktz jwzp tbvx qheq"), // move to web.config/AppSettings
                EnableSsl = true
            };

            smtp.Send(mail);
        }
    }
}
