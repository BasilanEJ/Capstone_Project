using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using OtpNet;          // OtpNet via NuGet
using QRCoder;        // QRCoder via NuGet
using System.Drawing;
using System.IO;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class Enable2FA : Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Must come from the password step
                if (Session["Pending2FA_Email"] == null ||
                    Session["Pending2FA_UserID"] == null)
                {
                    Response.Redirect("Login.aspx");
                    return;
                }

                GenerateQRCode();
            }
        }

        private void GenerateQRCode()
        {
            // 1) Generate a 20-byte secret and keep it only in session until verified
            byte[] secretKey = KeyGeneration.GenerateRandomKey(20);
            string base32Secret = Base32Encoding.ToString(secretKey);
            Session["2FA_Secret"] = base32Secret;

            // 2) Check if user is Inspector - only show manual entry link for Inspectors
            string role = Session["Pending2FA_Role"]?.ToString() ?? "";
            bool isInspector = string.Equals(role, "Inspector", StringComparison.OrdinalIgnoreCase);

            // Show "Can't scan?" link only for Inspectors
            pnlManualEntryLink.Visible = isInspector;

            // 3) Build otpauth URI
            string email = Session["Pending2FA_Email"].ToString();
            string issuer = "RRCManagementSystem";
            string otpauthUrl = $"otpauth://totp/{issuer}:{email}?secret={base32Secret}&issuer={issuer}";

            // 4) Render QR Code (shown to all users)
            using (var qrGen = new QRCodeGenerator())
            using (var data = qrGen.CreateQrCode(otpauthUrl, QRCodeGenerator.ECCLevel.Q))
            using (var qr = new QRCode(data))
            using (var bmp = qr.GetGraphic(20))
            using (var ms = new MemoryStream())
            {
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                imgQRCode.ImageUrl = "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
            }

            // 5) Prepare secret key for manual entry (in modal for Inspectors)
            lblSecretKey.Text = FormatSecretKey(base32Secret);
            hdnSecretKey.Value = base32Secret; // Store clean version without spaces for copying
        }

        /// <summary>
        /// Formats the secret key with spaces for better readability
        /// Example: JBSW Y3DP EHPK 3PXP
        /// </summary>
        private string FormatSecretKey(string secret)
        {
            if (string.IsNullOrEmpty(secret)) return "";

            // Insert a space every 4 characters for readability
            string formatted = "";
            for (int i = 0; i < secret.Length; i++)
            {
                if (i > 0 && i % 4 == 0)
                    formatted += " ";
                formatted += secret[i];
            }
            return formatted;
        }

        protected void btnVerify_Click(object sender, EventArgs e)
        {
            string code = txtCode.Value?.Trim() ?? "";
            string secret = Session["2FA_Secret"]?.ToString();
            string email = Session["Pending2FA_Email"]?.ToString();
            string name = Session["Pending2FA_Name"]?.ToString();
            string role = Session["Pending2FA_Role"]?.ToString();

            if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(email))
            {
                lblMessage.Text = "❌ Session expired. Please reload the page.";
                return;
            }

            if (code.Length != 6)
            {
                lblMessage.Text = "❌ Enter the 6-digit code.";
                return;
            }

            // Verify TOTP
            var totp = new Totp(Base32Encoding.ToBytes(secret));
            bool isValid = totp.VerifyTotp(code, out _, VerificationWindow.RfcSpecifiedNetworkDelay);

            if (!isValid)
            {
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblMessage.Text = "❌ Invalid code. Please try again.";
                return;
            }

            // Persist the secret & enable flag via stored procedure
            int userId = Convert.ToInt32(Session["Pending2FA_UserID"]);
            int rows;
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.sp2FA_Enable", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@TOTPSecret", SqlDbType.NVarChar, 100).Value = secret;
                conn.Open();

                // sp returns @@ROWCOUNT AS RowsAffected
                object o = cmd.ExecuteScalar();
                rows = (o == null || o == DBNull.Value) ? 0 : Convert.ToInt32(o);
            }

            if (rows == 0)
            {
                lblMessage.Text = "❌ Could not enable 2FA (account not active/available).";
                return;
            }

            // Best-effort audit
            TryAudit(userId, $"{role} {name} enabled 2FA.");

            // Finalize login session now that 2FA is enabled
            Session["UserID"] = userId;
            Session["Email"] = email;
            Session["Name"] = name;
            Session["Role"] = role;
            Session["IsAuthenticated"] = true;

            // Cleanup pending state
            Session.Remove("Pending2FA_Email");
            Session.Remove("Pending2FA_Name");
            Session.Remove("Pending2FA_Role");
            Session.Remove("Pending2FA_UserID");
            Session.Remove("2FA_Secret");

            // Redirect to appropriate dashboard based on role
            string redirect = "~/Dashboard.aspx";
            if (string.Equals(role, "RootAdmin", StringComparison.OrdinalIgnoreCase))
                redirect = "~/RootDashboard.aspx";
            else if (string.Equals(role, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
                redirect = "~/SuperAdminDashboard.aspx";
            else if (string.Equals(role, "Inspector", StringComparison.OrdinalIgnoreCase))
                redirect = "~/InspectorDashboard.aspx";

            Response.Redirect(redirect, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private void TryAudit(int userId, string action)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAudit_Insert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@AdminID", SqlDbType.Int).Value = userId;
                    cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 255).Value = action ?? "";
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                // swallow — audit is best-effort
            }
        }
    }
}