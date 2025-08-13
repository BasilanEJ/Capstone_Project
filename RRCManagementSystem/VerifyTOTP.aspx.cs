using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Web;
using OtpNet;

namespace RRCManagementSystem
{
    public partial class VerifyTOTP : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Prevent cached back/forward navigation from reusing this page
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));

            if (!IsPostBack)
            {
                lblMessage.Text = "";
                pnlCaptcha.Visible = false;

                // Extra hardening: ensure no authenticated session is present yet
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

        protected void btnVerifyTOTP_Click(object sender, EventArgs e)
        {
            // Still require pending identity
            if (Session["Pending2FA_Email"] == null || Session["Pending2FA_UserID"] == null)
            {
                Response.Redirect("Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            int userID = Convert.ToInt32(Session["Pending2FA_UserID"]);
            string email = Session["Pending2FA_Email"]?.ToString();
            string role = Session["Pending2FA_Role"]?.ToString();
            string name = Session["Pending2FA_Name"]?.ToString();

            if (pnlCaptcha.Visible && !IsCaptchaValid())
            {
                lblMessage.Text = "⚠ CAPTCHA verification failed.";
                return;
            }

            string userInputCode = txtTOTP.Value?.Trim() ?? "";
            if (userInputCode.Length != 6)
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
                // Accept code with standard network delay window
                bool isValid = totp.VerifyTotp(userInputCode, out _, VerificationWindow.RfcSpecifiedNetworkDelay);

                if (isValid)
                {
                    // ✅ Now finalize real authentication
                    Session["UserID"] = userID;
                    Session["Role"] = role;
                    Session["Name"] = name;
                    Session["Email"] = email;
                    Session["IsAuthenticated"] = true;

                    // Clear pending state
                    Session.Remove("Pending2FA_UserID");
                    Session.Remove("Pending2FA_Email");
                    Session.Remove("Pending2FA_Name");
                    Session.Remove("Pending2FA_Role");

                    AddAuditLog(userID, $"{role} {name} completed 2FA verification.");

                    string redirect = role == "SuperAdmin" ? "SuperAdminDashboard.aspx" :
                                      role == "Inspector" ? "InspectorDashboard.aspx" :
                                                             "Dashboard.aspx";
                    Response.Redirect(redirect, false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
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
                string secret = "6LcIAqErAAAAAD3HQP8r8XkyIp9tJVFGSZSr0ozd"; // TODO: move to config
                string result = client.DownloadString(
                    $"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={response}"
                );
                return result.Contains("\"success\": true");
            }
        }

        private string GetTOTPSecret(string email)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                const string query = @"
SELECT TOTPSecret 
FROM Users 
WHERE Email = @Email 
  AND TwoFactorEnabled = 1 
  AND Status IN ('Active','Available')";
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
                const string query = "SELECT LockoutUntil FROM Users WHERE UserID = @UserID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
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
        }

        private void AddAuditLog(int userID, string action)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                const string query = "INSERT INTO AuditLogs (AdminID, Action, Timestamp) VALUES (@UserID, @Action, GETDATE())";
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
