
using System;
using System.Configuration;
using System.Data.SqlClient;
using OtpNet; // Install OtpNet via NuGet
using QRCoder; // Install QRCoder via NuGet
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
                if (Session["AdminEmail"] == null)
                {
                    Response.Redirect("Login.aspx");
                    return;
                }

                GenerateQRCode();
            }
        }

        private void GenerateQRCode()
        {
            // Generate a random 160-bit (20-byte) secret key
            byte[] secretKey = KeyGeneration.GenerateRandomKey(20);
            string base32Secret = Base32Encoding.ToString(secretKey);

            // Temporarily store the secret in session for validation
            Session["2FA_Secret"] = base32Secret;

            string email = Session["AdminEmail"].ToString();
            string issuer = "RRCManagementSystem";

            // Generate otpauth URL
            string otpauthUrl = $"otpauth://totp/{issuer}:{email}?secret={base32Secret}&issuer={issuer}";

            // Generate QR code image
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(otpauthUrl, QRCodeGenerator.ECCLevel.Q))
            using (QRCode qrCode = new QRCode(qrCodeData))
            using (Bitmap qrBitmap = qrCode.GetGraphic(20))
            using (MemoryStream ms = new MemoryStream())
            {
                qrBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                string base64Image = Convert.ToBase64String(ms.ToArray());
                imgQRCode.ImageUrl = "data:image/png;base64," + base64Image;
            }
        }

        protected void btnVerify_Click(object sender, EventArgs e)
        {
            string code = txtCode.Text.Trim();
            string secret = Session["2FA_Secret"]?.ToString();
            string email = Session["AdminEmail"]?.ToString();

            if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(email))
            {
                lblMessage.Text = "❌ Session expired. Please reload the page.";
                return;
            }

            var totp = new Totp(Base32Encoding.ToBytes(secret));
            bool isValid = totp.VerifyTotp(code, out _, VerificationWindow.RfcSpecifiedNetworkDelay);

            if (isValid)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        UPDATE Users
                        SET TOTPSecret = @Secret,
                            TwoFactorEnabled = 1
                        WHERE Email = @Email AND Status = 'Active'";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Secret", secret);
                        cmd.Parameters.AddWithValue("@Email", email);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                lblMessage.ForeColor = System.Drawing.Color.Green;
                lblMessage.Text = "✅ 2FA enabled successfully! Redirecting...";

                // Optional redirect after enabling
                Response.AddHeader("REFRESH", "3;URL=Login.aspx");
            }
            else
            {
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblMessage.Text = "❌ Invalid code. Please try again.";
            }
        }
    }
}

