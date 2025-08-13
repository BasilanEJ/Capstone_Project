
using System;
using System.Configuration;
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
            if (!IsPostBack)
            {
                lblMessage.Text = "";
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();

            if (string.IsNullOrEmpty(email))
            {
                lblMessage.Text = "⚠ Please enter your email address.";
                return;
            }

            // ✅ Check if the email exists (Clients only here)
            bool emailExists = CheckClientEmailExists(email);

            if (!emailExists)
            {
                lblMessage.Text = "⚠ Email not found or not approved.";
                return;
            }

            // ✅ Generate OTP
            string otp = GenerateOTP();

            // ✅ Store OTP and email in session
            Session["OTP"] = otp;
            Session["OTP_Expiry"] = DateTime.Now.AddMinutes(2); // 2 minutes expiry
            Session["OTP_Email"] = email;

            // ✅ Send OTP via email
            string emailBody = GenerateOtpEmailBody(otp);
            bool emailSent = SendOtpEmail(email, emailBody);

            if (emailSent)
            {
                Response.Redirect("VerifyOTP.aspx");
            }
            else
            {
                lblMessage.Text = "⚠ Failed to send OTP email. Please try again later.";
            }
        }

        private bool CheckClientEmailExists(string email)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM Clients WHERE Email = @Email AND Status = 'Approved';";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);

                    try
                    {
                        conn.Open();
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                    catch (Exception ex)
                    {
                        lblMessage.Text = "⚠ Error checking email: " + ex.Message;
                        return false;
                    }
                }
            }
        }

        private string GenerateOTP()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] data = new byte[4];
                rng.GetBytes(data);

                int generatedValue = Math.Abs(BitConverter.ToInt32(data, 0));
                int otp = generatedValue % 900000 + 100000;

                return otp.ToString();
            }
        }

        private string GenerateOtpEmailBody(string otp)
        {
            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
  <meta charset='UTF-8'>
  <title>Your OTP Code</title>
</head>
<body style='font-family: Arial, sans-serif; background-color: #f3f4f6; margin: 0; padding: 20px;'>
  <table width='100%' cellpadding='0' cellspacing='0' border='0'>
    <tr>
      <td align='center'>
        <table width='500' cellpadding='0' cellspacing='0' border='0' style='background-color: #ffffff; border-radius: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
          <tr>
            <td style='background-color: #007bff; color: #ffffff; text-align: center; padding: 20px;'>
              <h2 style='margin: 0;'>RRC Management System</h2>
            </td>
          </tr>
          <tr>
            <td style='padding: 30px; color: #333333;'>
              <h3 style='margin-top: 0; text-align: center;'>One-Time Password (OTP)</h3>
              <p>Hello,</p>
              <p>Use the OTP code below to reset your password:</p>
              <div style='text-align: center; margin: 30px 0;'>
                <span style='display: inline-block; background-color: #f3f4f6; color: #333; padding: 15px 30px; font-size: 28px; font-weight: bold; border-radius: 6px; letter-spacing: 4px; border: 1px solid #ddd;'>
                  {otp}
                </span>
              </div>
              <p>This code is valid for <strong>2 minutes</strong>.</p>
              <p>If you didn’t request a password reset, please ignore this message.</p>
            </td>
          </tr>
          <tr>
            <td style='background-color: #f9f9f9; color: #999999; text-align: center; padding: 15px; font-size: 12px;'>
              &copy; 2025 RRC Management System
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
        }

        private bool SendOtpEmail(string recipientEmail, string emailBody)
        {
            try
            {
                MailMessage mail = new MailMessage
                {
                    From = new MailAddress("rrctermiteandpestcontrol@gmail.com", "RRC Management System"),
                    Subject = "Your OTP Code for Password Reset",
                    Body = emailBody,
                    IsBodyHtml = true
                };
                mail.To.Add(recipientEmail);

                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("rrctermiteandpestcontrol@gmail.com", "pktz jwzp tbvx qheq"), // App Password
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
    }
}

