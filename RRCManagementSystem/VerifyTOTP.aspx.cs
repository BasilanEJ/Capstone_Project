using System;
using System.Configuration;
using System.Data;
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
                    // ✅ Finalize real authentication
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

                    string redirect = role == "SuperAdmin" ? "SuperAdminDashboard.aspx"
                                     : role == "Inspector" ? "InspectorDashboard.aspx"
                                     : "Dashboard.aspx";
                    Response.Redirect(redirect, false);
                    Context.ApplicationInstance.CompleteRequest();
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

        /* =========================
           Stored-proc helpers
           ========================= */

        private string GetTOTPSecret(string email)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spAuth_GetTOTPSecretByEmail", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = email ?? string.Empty;

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
                string secret = "6Ld6VrcrAAAAANJi4Djjr9vN7N5KIWoIoL_CCi_z"; // move to config
                string result = client.DownloadString(
                    $"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={response}"
                );
                return result.Contains("\"success\": true");
            }
        }
    }
}
