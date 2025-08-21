using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;

namespace RRCManagementSystem
{
    public partial class ForgotPassword : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) lblMessage.Text = "";
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();

            if (string.IsNullOrEmpty(email))
            {
                lblMessage.Text = "⚠ Please enter your email address.";
                return;
            }

            try
            {
                // 1) USERS first (Admins/Staff) — allow only Active/Available
                if (TryGetEligibleUser(email, out int userId, out string userName, out string userRole))
                {
                    string otp = GenerateOTP();

                    // Save OTP context (2 minutes)
                    Session["OTP"] = otp;
                    Session["OTP_Expiry"] = DateTime.Now.AddMinutes(2);
                    Session["OTP_Email"] = email;
                    Session["OTP_AccountType"] = "User";
                    Session["OTP_UserID"] = userId;
                    Session.Remove("OTP_ClientID");

                    TryAudit($"Password reset OTP requested for USER {userRole} ({userName}) email {email}.");

                    string emailBody = GenerateOtpEmailBody(otp);
                    if (SendOtpEmail(email, emailBody))
                    {
                        Response.Redirect("VerifyOTP.aspx");
                        return;
                    }
                    lblMessage.Text = "⚠ Failed to send OTP email. Please try again later.";
                    return;
                }

                // 2) CLIENTS next — allow only Approved
                if (TryGetApprovedClient(email, out int clientId, out string clientName))
                {
                    string otp = GenerateOTP();

                    Session["OTP"] = otp;
                    Session["OTP_Expiry"] = DateTime.Now.AddMinutes(2);
                    Session["OTP_Email"] = email;
                    Session["OTP_AccountType"] = "Client";
                    Session["OTP_ClientID"] = clientId;
                    Session.Remove("OTP_UserID");

                    TryAudit($"Password reset OTP requested for CLIENT ({clientName}) email {email}.");

                    string emailBody = GenerateOtpEmailBody(otp);
                    if (SendOtpEmail(email, emailBody))
                    {
                        Response.Redirect("VerifyOTP.aspx");
                        return;
                    }
                    lblMessage.Text = "⚠ Failed to send OTP email. Please try again later.";
                    return;
                }

                // 3) Not eligible
                lblMessage.Text = "⚠ Email not found or not approved/active.";
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠ Error: " + ex.Message;
            }
        }

        /// <summary>
        /// Query Users via spAuth_GetUserByEmail; allow only Status = Active or Available.
        /// Expects columns: UserID, Name, Role, Status.
        /// </summary>
        private bool TryGetEligibleUser(string email, out int userId, out string name, out string role)
        {
            userId = 0; name = ""; role = "";

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spAuth_GetUserByEmail", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = email;

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return false;

                    string status = reader["Status"]?.ToString() ?? "";
                    if (!status.Equals("Active", StringComparison.OrdinalIgnoreCase) &&
                        !status.Equals("Available", StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }

                    userId = Convert.ToInt32(reader["UserID"]);
                    name = reader["Name"]?.ToString() ?? "";
                    role = reader["Role"]?.ToString() ?? "";
                    return true;
                }
            }
        }

        /// <summary>
        /// Query Clients via spAuth_GetClientByEmail; allow only Status = Approved.
        /// Expects columns: ClientID, Name, Status.
        /// </summary>
        private bool TryGetApprovedClient(string email, out int clientId, out string name)
        {
            clientId = 0; name = "";

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spAuth_GetClientByEmail", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = email;

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return false;

                    string status = reader["Status"]?.ToString() ?? "";
                    if (!status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                        return false;

                    clientId = Convert.ToInt32(reader["ClientID"]);
                    name = reader["Name"]?.ToString() ?? "";
                    return true;
                }
            }
        }

        private string GenerateOTP()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var data = new byte[4];
                rng.GetBytes(data);
                int value = Math.Abs(BitConverter.ToInt32(data, 0));
                int otp = value % 900000 + 100000; // 6 digits
                return otp.ToString();
            }
        }

        private string GenerateOtpEmailBody(string otp)
        {
            return $@"
<!DOCTYPE html>
<html lang='en'>
<head><meta charset='UTF-8'><title>Your OTP Code</title></head>
<body style='font-family: Arial, sans-serif; background-color: #f3f4f6; margin: 0; padding: 20px;'>
  <table width='100%' cellpadding='0' cellspacing='0' border='0'>
    <tr><td align='center'>
      <table width='500' cellpadding='0' cellspacing='0' border='0' style='background-color: #ffffff; border-radius: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
        <tr><td style='background-color: #007bff; color: #ffffff; text-align: center; padding: 20px;'>
          <h2 style='margin: 0;'>RRC Management System</h2>
        </td></tr>
        <tr><td style='padding: 30px; color: #333333;'>
          <h3 style='margin-top: 0; text-align: center;'>One-Time Password (OTP)</h3>
          <p>Hello,</p>
          <p>Use the OTP code below to reset your password:</p>
          <div style='text-align: center; margin: 30px 0;'>
            <span style='display: inline-block; background-color: #f3f4f6; color: #333; padding: 15px 30px; font-size: 28px; font-weight: bold; border-radius: 6px; letter-spacing: 4px; border: 1px solid #ddd;'>{otp}</span>
          </div>
          <p>This code is valid for <strong>2 minutes</strong>.</p>
          <p>If you didn’t request a password reset, please ignore this message.</p>
        </td></tr>
        <tr><td style='background-color: #f9f9f9; color: #999; text-align: center; padding: 15px; font-size: 12px;'>&copy; 2025 RRC Management System</td></tr>
      </table>
    </td></tr>
  </table>
</body>
</html>";
        }

        private bool SendOtpEmail(string recipientEmail, string emailBody)
        {
            try
            {
                var mail = new MailMessage
                {
                    From = new MailAddress("rrctermiteandpestcontrol@gmail.com", "RRC Management System"),
                    Subject = "Your OTP Code for Password Reset",
                    Body = emailBody,
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
                return true;
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠ Failed to send email: " + ex.Message;
                return false;
            }
        }

        private void TryAudit(string action)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAudit_Insert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@AdminID", SqlDbType.Int).Value = 0; // public action
                    cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 255).Value = action ?? "";
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch { /* ignore */ }
        }
    }
}
