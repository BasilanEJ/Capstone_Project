using System;
using System.Configuration;
using System.Data.SqlClient;
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

                if (Session["Pending2FA_Email"] == null || Session["Pending2FA_UserID"] == null)
                {
                    Response.Redirect("Login.aspx");
                    return;
                }
            }
        }

        protected void btnVerifyTOTP_Click(object sender, EventArgs e)
        {
            string userInputCode = txtTOTP.Value.Trim();

            string email = Session["Pending2FA_Email"]?.ToString();
            string role = Session["Pending2FA_Role"]?.ToString();
            string name = Session["Pending2FA_Name"]?.ToString();
            int userID = Convert.ToInt32(Session["Pending2FA_UserID"]);

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
                byte[] secretBytes = Base32Encoding.ToBytes(totpSecret);
                var totp = new Totp(secretBytes);
                bool isValid = totp.VerifyTotp(userInputCode, out _, VerificationWindow.RfcSpecifiedNetworkDelay);

                if (isValid)
                {
                    Session["AdminID"] = userID;
                    Session["AdminName"] = name;
                    Session["AdminEmail"] = email;
                    Session["UserRole"] = role;

                    Session.Remove("Pending2FA_UserID");
                    Session.Remove("Pending2FA_Email");
                    Session.Remove("Pending2FA_Name");
                    Session.Remove("Pending2FA_Role");

                    AddAuditLog(userID, $"{role} {name} completed 2FA verification.");

                    if (role == "SuperAdmin")
                        Response.Redirect("SuperAdminDashboard.aspx");
                    else
                        Response.Redirect("Dashboard.aspx");
                }
                else
                {
                    lblMessage.Text = "⚠ Invalid code. Please try again.";
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"❌ Error: {ex.Message}";
            }
        }

        private string GetTOTPSecret(string email)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT TOTPSecret FROM Users WHERE Email = @Email AND TwoFactorEnabled = 1 AND Status = 'Active'";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result?.ToString();
                }
            }
        }

        private void AddAuditLog(int userID, string action)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO AuditLogs (AdminID, Action, Timestamp) VALUES (@AdminID, @Action, GETDATE())";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@AdminID", userID);
                    cmd.Parameters.AddWithValue("@Action", action);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}