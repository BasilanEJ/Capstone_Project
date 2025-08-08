using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using OtpNet;

namespace RRCManagementSystem
{
    public partial class VerifyTOTP : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                lblMessage.Text = "";
                pnlCaptcha.Visible = false;

                if (Session["Pending2FA_Email"] == null || Session["Pending2FA_UserID"] == null)
                {
                    Response.Redirect("Login.aspx");
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

        protected void btnVerifyTOTP_Click(object sender, EventArgs e)
        {
            int userID = Convert.ToInt32(Session["Pending2FA_UserID"]);
            if (pnlCaptcha.Visible && !IsCaptchaValid())
            {
                lblMessage.Text = "⚠ CAPTCHA verification failed.";
                return;
            }

            string userInputCode = txtTOTP.Value.Trim();
            string email = Session["Pending2FA_Email"]?.ToString();
            string role = Session["Pending2FA_Role"]?.ToString();
            string name = Session["Pending2FA_Name"]?.ToString();

            if (string.IsNullOrEmpty(userInputCode) || userInputCode.Length != 6)
            {
                lblMessage.Text = "⚠ Please enter a valid 6-digit code.";
                return;
            }

            string totpSecret = GetTOTPSecret(email);
            if (string.IsNullOrEmpty(totpSecret))
            {
                lblMessage.Text = "⚠ 2FA is not enabled for this account.";
                return;
            }

            try
            {
                var totp = new Totp(Base32Encoding.ToBytes(totpSecret));
                bool isValid = totp.VerifyTotp(userInputCode, out _, VerificationWindow.RfcSpecifiedNetworkDelay);

                if (isValid)
                {
                    Session["UserID"] = userID;
                    Session["Role"] = role;
                    Session["Name"] = name;
                    Session["Email"] = email;

                    Session.Remove("Pending2FA_UserID");
                    Session.Remove("Pending2FA_Email");
                    Session.Remove("Pending2FA_Name");
                    Session.Remove("Pending2FA_Role");

                    AddAuditLog(userID, $"{role} {name} completed 2FA verification.");

                    string redirect = role == "SuperAdmin" ? "SuperAdminDashboard.aspx" :
                                      role == "Inspector" ? "InspectorDashboard.aspx" :
                                      "Dashboard.aspx";
                    Response.Redirect(redirect);
                }
                else
                {
                    lblMessage.Text = "⚠ Invalid code. Please try again.";
                    pnlCaptcha.Visible = true;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"❌ Error: {ex.Message}";
            }
        }

        private bool IsCaptchaValid()
        {
            string response = Request.Form["g-recaptcha-response"];
            if (string.IsNullOrEmpty(response)) return false;

            using (var client = new WebClient())
            {
                string secret = "6Lfu6JMrAAAAAEy4fBkw0jNrp7mXfvqy03Tlz1i7"; // Replace with your actual secret
                string result = client.DownloadString($"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={response}");
                return result.Contains("\"success\": true");
            }
        }

        private string GetTOTPSecret(string email)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT TOTPSecret FROM Users WHERE Email = @Email AND TwoFactorEnabled = 1 AND Status IN ('Active', 'Available')";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result?.ToString();
                }
            }
        }

        private bool IsLockedOut(int userId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT LockoutUntil FROM Users WHERE UserID = @UserID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserID", userId);
                conn.Open();
                object result = cmd.ExecuteScalar();
                if (result != DBNull.Value && result != null)
                {
                    DateTime lockoutTime = Convert.ToDateTime(result);
                    return lockoutTime > DateTime.Now;
                }
                return false;
            }
        }

        private void AddAuditLog(int userID, string action)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO AuditLogs (AdminID, Action, Timestamp) VALUES (@UserID, @Action, GETDATE())";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userID);
                    cmd.Parameters.AddWithValue("@Action", action);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
