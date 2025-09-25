using RRCManagementSystem.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class Default : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Restrict file picker to image files only
                fuPestPhoto.Attributes["accept"] = "image/png,image/jpeg,image/jpg";
                MaintainScrollPositionOnPostBack = true;
            }
        }

        protected void btnSubmitInquiry_Click(object sender, EventArgs e)
        {
            if (!chkTerms.Checked)
            {
                ScriptManager.RegisterStartupScript(
                    this,
                    GetType(),
                    "ShowSweetAlertAndModal",
                    @"
            Swal.fire({
                icon: 'warning',
                title: 'Terms Required',
                text: 'Please agree to the terms and conditions before submitting.',
                showConfirmButton: false,
                timer: 2000,
                position: 'center',
                backdrop: false
            });

            setTimeout(function() {
                var termsModal = new bootstrap.Modal(document.getElementById('termsModal'));
                termsModal.show();
            }, 2100);
            ",
                    true
                );
                return;
            }

            string email = (txtEmail.Text ?? "").Trim().ToLowerInvariant();
            string contact = (txtContactNumber.Text ?? "").Trim();
            string message = (txtMessage.Text ?? "").Trim();

            // Validate email
            int atIndex = email.IndexOf('@');
            if (atIndex < 0 || atIndex == email.Length - 1)
            {
                ShowSweetAlert("Invalid Email", "Email must contain @ and a domain.", "error");
                return;
            }

            string domain = email.Substring(atIndex + 1);
            var allowedDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "gmail.com", "yahoo.com", "outlook.com", "hotmail.com", "live.com", "icloud.com"
    };

            bool isValid = allowedDomains.Contains(domain) ||
                           domain.EndsWith(".edu.ph", StringComparison.OrdinalIgnoreCase) ||
                           domain.EndsWith(".gov.ph", StringComparison.OrdinalIgnoreCase);

            if (!isValid)
            {
                ShowSweetAlert("Invalid Email", "Only Gmail, Yahoo, Outlook, iCloud, or .edu.ph / .gov.ph emails allowed.", "error");
                return;
            }

            // Validate contact number
            if (!System.Text.RegularExpressions.Regex.IsMatch(contact, @"^09\d{9}$"))
            {
                ShowSweetAlert("Invalid Contact", "Contact number must be 11 digits starting with 09.", "warning");
                return;
            }

            // Handle photo upload
            string photoPath = null;
            if (fuPestPhoto.HasFile)
            {
                try
                {
                    string extension = Path.GetExtension(fuPestPhoto.FileName).ToLowerInvariant();
                    string contentType = (fuPestPhoto.PostedFile.ContentType ?? "").ToLowerInvariant();
                    string[] allowedExtensions = { ".png", ".jpg", ".jpeg" };
                    string[] allowedMimeTypes = { "image/png", "image/jpg", "image/jpeg" };

                    if (!allowedExtensions.Contains(extension) || !allowedMimeTypes.Contains(contentType))
                    {
                        ShowSweetAlert("Invalid File", "Only PNG or JPEG files are allowed.", "warning");
                        return;
                    }

                    string folderRelativePath = "/Uploads/InquiryPhotos/";
                    string folderPhysicalPath = Server.MapPath(folderRelativePath);

                    if (!Directory.Exists(folderPhysicalPath))
                        Directory.CreateDirectory(folderPhysicalPath);

                    string filename = Guid.NewGuid().ToString("N") + extension;
                    string savePath = Path.Combine(folderPhysicalPath, filename);

                    fuPestPhoto.SaveAs(savePath);
                    photoPath = folderRelativePath + filename;
                }
                catch (Exception ex)
                {
                    ShowSweetAlert("Upload Error", "Unable to save uploaded photo. " + ex.Message, "error");
                    return;
                }
            }

            try
            {
                // ================================================
                // Encryption + Hashing
                // ================================================
                string emailHash = AESHelper.ComputeSHA256WithPepper(email); // For search
                string emailEnc = AESHelper.EncryptEmail(email);
                string contactEnc = AESHelper.EncryptField(contact);

                // Address placeholders (client not entering these yet)
                string streetEnc = AESHelper.EncryptField("");
                string barangayEnc = AESHelper.EncryptField("");
                string cityEnc = AESHelper.EncryptField("");
                string regionEnc = AESHelper.EncryptField("");
                string countryEnc = AESHelper.EncryptField("");
                string landmarkEnc = AESHelper.EncryptField("");

                string generatedCode;

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("dbo.spInquirySimple_Insert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@EmailHash", emailHash);
                    cmd.Parameters.AddWithValue("@EmailEnc", emailEnc);
                    cmd.Parameters.AddWithValue("@ContactEnc", contactEnc);
                    cmd.Parameters.AddWithValue("@Message", string.IsNullOrEmpty(message) ? "N/A" : message);
                    cmd.Parameters.AddWithValue("@PhotoPath", string.IsNullOrEmpty(photoPath) ? (object)DBNull.Value : photoPath);

                    cmd.Parameters.AddWithValue("@LastName", "");
                    cmd.Parameters.AddWithValue("@FirstName", "");
                    cmd.Parameters.AddWithValue("@MiddleName", "");

                    cmd.Parameters.AddWithValue("@StreetEnc", streetEnc);
                    cmd.Parameters.AddWithValue("@BarangayEnc", barangayEnc);
                    cmd.Parameters.AddWithValue("@CityEnc", cityEnc);
                    cmd.Parameters.AddWithValue("@RegionEnc", regionEnc);
                    cmd.Parameters.AddWithValue("@CountryEnc", countryEnc);
                    cmd.Parameters.AddWithValue("@LandmarkEnc", landmarkEnc);

                    var pCode = new SqlParameter("@GeneratedInquiryCode", SqlDbType.NVarChar, 25) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(pCode);

                    var pId = new SqlParameter("@NewInquiryID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(pId);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    generatedCode = Convert.ToString(pCode.Value ?? "");
                }

                SendConfirmationEmail(email, generatedCode);

                ShowSweetAlert("Submitted!", $"Your inquiry was submitted successfully.\nReference Code: {generatedCode}", "success");
                ClearForm();
            }
            catch (Exception ex)
            {
                ShowSweetAlert("Error", "Something went wrong while saving: " + ex.Message, "error");
            }
        }


        private bool SendConfirmationEmail(string toEmail, string inquiryCode)
        {
            try
            {
                string fromEmail = ConfigurationManager.AppSettings["emailFrom"] ?? "rrctermiteandpestcontrol@gmail.com";
                string appPassword = ConfigurationManager.AppSettings["emailPassword"] ?? "";

                string subject = $"RRC Inquiry Received - Ref {inquiryCode}";

                // HTML email body
                string body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 30px auto; background: #fff; border-radius: 8px; padding: 20px;
                      box-shadow: 0 2px 8px rgba(0,0,0,0.05); }}
        .header {{ background: #2563eb; color: #fff; padding: 15px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content p {{ font-size: 16px; color: #333; line-height: 1.5; }}
        .code {{ font-size: 20px; font-weight: bold; color: #2563eb; text-align: center; padding: 10px 0; }}
        .footer {{ font-size: 12px; color: #777; text-align: center; padding-top: 15px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>RRC Management System</h2>
        </div>
        <div class='content'>
            <p>Hi,</p>
            <p>We have received your inquiry. Your reference code is:</p>
            <p class='code'>{inquiryCode}</p>
            <p>Please keep this code safe so we can quickly find your record.</p>
            <p>Thank you for reaching out to RRC Termite & Pest Control.</p>
        </div>
        <div class='footer'>
            <p>If you did not make this inquiry, you can safely ignore this email.</p>
            <p>© {DateTime.Now.Year} RRC Management System. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(fromEmail, "RRC Management System", Encoding.UTF8);
                    mail.To.Add(new MailAddress(toEmail));
                    mail.Subject = subject;
                    mail.SubjectEncoding = Encoding.UTF8;
                    mail.Body = body;
                    mail.BodyEncoding = Encoding.UTF8;
                    mail.IsBodyHtml = true;

                    using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new NetworkCredential(fromEmail, appPassword);
                        smtp.EnableSsl = true;
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                        try
                        {
                            smtp.Send(mail);
                            return true; // ✅ SUCCESS
                        }
                        catch (Exception ex)
                        {
                            // Log the error
                            System.Diagnostics.Debug.WriteLine("Email sending failed: " + ex.Message);
                            return false; // ❌ FAILED
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Critical error while sending email: " + ex.Message);
                return false; // ❌ FAILED
            }
        }


        private void ShowSweetAlert(string title, string message, string icon)
        {
            string Esc(string s) => (s ?? "")
                .Replace("\\", "\\\\")
                .Replace("'", "\\'")
                .Replace("\r", "")
                .Replace("\n", "\\n");

            var key = "swal_" + Guid.NewGuid().ToString("N");

            string script = $@"
    (function() {{
      function show() {{
        if (window.Swal && typeof Swal.fire === 'function') {{
          Swal.fire({{
            title: '{Esc(title)}',
            text: '{Esc(message)}',
            icon: '{Esc(icon)}',
            confirmButtonColor: '#007bff',
            focusConfirm: false,      // Don't focus button
            backdrop: false,          // No dimmed background
            position: 'center',       // Keep it inline
            allowOutsideClick: false, // Force user to acknowledge
            allowEscapeKey: false,
            didOpen: () => {{
                // Prevent scroll jump
                if (history.scrollRestoration) {{
                    history.scrollRestoration = 'manual';
                }}
            }}
          }}).then(() => {{
              // Restore scroll manually
              const y = sessionStorage.getItem('scrollPosition');
              if (y) window.scrollTo(0, parseInt(y));
          }});
        }} else {{
          alert('{Esc(title)}\\n{Esc(message)}');
        }}
      }}
      if (document.readyState === 'complete') {{
        show();
      }} else {{
        window.addEventListener('load', show);
      }}
    }})();";

            ScriptManager.RegisterStartupScript(this, GetType(), key, script, addScriptTags: true);
        }



        private void ClearForm()
        {
            txtEmail.Text = "";
            txtContactNumber.Text = "";
            txtMessage.Text = "";
            chkTerms.Checked = false;
        }
    }
}
