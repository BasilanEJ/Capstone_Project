using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.IO;
using System.Linq;
using System.Collections.Generic;

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
            }
        }

        protected void btnSubmitInquiry_Click(object sender, EventArgs e)
        {
            if (!chkTerms.Checked)
            {
                ShowSweetAlert("Terms Required", "Please agree to the terms and conditions before submitting.", "warning");
                return;
            }

            string email = (txtEmail.Text ?? "").Trim().ToLowerInvariant();
            string contact = (txtContactNumber.Text ?? "").Trim();
            string message = (txtMessage.Text ?? "").Trim();

            // ===== Domain allow-list guard (server-side) =====
            // Allowed consumer mailbox domains (case-insensitive)
            var allowedDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "gmail.com", "yahoo.com", "outlook.com", "hotmail.com", "live.com", "icloud.com"
            };

            // Extract domain
            int atIndex = email.IndexOf('@');
            if (atIndex < 0 || atIndex == email.Length - 1)
            {
                ShowSweetAlert("Invalid Email", "Email address must contain @ and a domain.", "error");
                return;
            }

            string domain = email.Substring(atIndex + 1);

            // ✅ Check direct allowed domains OR if it's a school/government PH domain
            bool isValid =
                allowedDomains.Contains(domain) ||
                domain.EndsWith(".edu.ph", StringComparison.OrdinalIgnoreCase) ||
                domain.EndsWith(".gov.ph", StringComparison.OrdinalIgnoreCase);

            if (!isValid)
            {
                ShowSweetAlert(
                    "Invalid Email",
                    "Only Gmail, Yahoo, Outlook/Hotmail/Live, iCloud, or school/government (.edu.ph / .gov.ph) emails are allowed.",
                    "error"
                );
                return;
            }
            // =================================================

            if (!System.Text.RegularExpressions.Regex.IsMatch(contact, @"^09\d{9}$"))
            {
                ShowSweetAlert("Invalid Contact", "Please enter a valid 11-digit contact number starting with 09.", "warning");
                return;
            }

            string photoPath = null;

            // ✅ Image validation & upload
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
                        ShowSweetAlert("Invalid File", "Only PNG or JPEG image files are allowed.", "warning");
                        return;
                    }

                    // ✅ Auto-create folder path
                    string folderRelativePath = "/UploadedPestPhotos/";
                    string folderPhysicalPath = Server.MapPath(folderRelativePath);

                    if (!Directory.Exists(folderPhysicalPath))
                    {
                        Directory.CreateDirectory(folderPhysicalPath); // ✅ Auto-create the folder
                    }

                    string filename = Guid.NewGuid().ToString("N") + extension;
                    string savePath = Path.Combine(folderPhysicalPath, filename);

                    fuPestPhoto.SaveAs(savePath);

                    // ✅ Save relative path for browser access
                    photoPath = folderRelativePath + filename;
                }
                catch (Exception ex)
                {
                    ShowSweetAlert("Upload Error", "Unable to save the uploaded photo. " + ex.Message, "error");
                    return;
                }
            }

            try
            {
                string generatedCode;

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("dbo.spInquirySimple_Insert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@ContactNumber", contact);
                    cmd.Parameters.AddWithValue("@Message", (object)message ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PhotoPath", string.IsNullOrEmpty(photoPath) ? (object)DBNull.Value : photoPath);

                    cmd.Parameters.AddWithValue("@LastName", "");
                    cmd.Parameters.AddWithValue("@FirstName", "");
                    cmd.Parameters.AddWithValue("@MiddleName", "");

                    cmd.Parameters.AddWithValue("@StreetAndUnit", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Barangay", DBNull.Value);
                    cmd.Parameters.AddWithValue("@City", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Region", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Country", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Landmark", DBNull.Value);

                    // OUTPUT params
                    var pCode = new SqlParameter("@GeneratedInquiryCode", SqlDbType.NVarChar, 25) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(pCode);

                    var pId = new SqlParameter("@NewInquiryID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(pId);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    generatedCode = Convert.ToString(pCode.Value ?? "");
                }

                SendConfirmationEmail(email, generatedCode);

                ShowSweetAlert(
                    "Submitted!",
                    $"Your inquiry was submitted successfully.\nReference Code: {generatedCode}",
                    "success"
                );
                ClearForm();
            }
            catch (Exception ex)
            {
                ShowSweetAlert("Error", "Something went wrong while saving: " + ex.Message, "error");
            }
        }

        private void SendConfirmationEmail(string toEmail, string inquiryCode)
        {
            // Gmail account used to send emails
            string fromEmail = ConfigurationManager.AppSettings["emailFrom"];       // Your Gmail address
            string appPassword = ConfigurationManager.AppSettings["emailPassword"]; // Gmail App Password

            // Use modern TLS only
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using (var mail = new MailMessage())
            {
                mail.From = new MailAddress(fromEmail, "RRC Management System", Encoding.UTF8);
                mail.To.Add(new MailAddress(toEmail));
                // Where replies should go (can be same Gmail or a support alias)
                mail.ReplyToList.Add(new MailAddress("rrctermiteandpestcontrol@gmail.com"));

                // Plain hyphen for broad compatibility
                mail.Subject = $"RRC Inquiry Received - Ref {inquiryCode}";
                mail.SubjectEncoding = Encoding.UTF8;

                mail.Body =
$@"Thank you for contacting R.R.C. Termite & Pest Control!

We received your inquiry. Your reference code is: {inquiryCode}
Please keep this code so we can quickly find your record.

We'll get back to you as soon as possible.

—
RRC Termite & Pest Control
rrctermiteandpestcontrol@gmail.com";
                mail.BodyEncoding = Encoding.UTF8;
                mail.IsBodyHtml = false;
                mail.HeadersEncoding = Encoding.UTF8;

                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(fromEmail, appPassword);
                    smtp.EnableSsl = true;                       // STARTTLS on 587
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Timeout = 20000;

                    try
                    {
                        smtp.Send(mail);
                    }
                    catch (SmtpFailedRecipientException ex)
                    {
                        ShowSweetAlert("Email Error",
                            $"Recipient rejected ({ex.FailedRecipient}).\nStatus: {ex.StatusCode}\nDetails: {ex.Message}",
                            "warning");
                    }
                    catch (SmtpException ex)
                    {
                        var more = ex.InnerException?.Message ?? "";
                        ShowSweetAlert("Email Error",
                            $"SMTP failed (Status {ex.StatusCode}): {ex.Message}" + (string.IsNullOrEmpty(more) ? "" : " / " + more),
                            "warning");
                    }
                    catch (Exception ex)
                    {
                        ShowSweetAlert("Email Error",
                            "We saved your inquiry but failed to send confirmation: " + ex.Message,
                            "warning");
                    }
                }
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
        confirmButtonColor: '#007bff'
      }});
    }} else {{
      alert('{Esc(title)}\n{Esc(message)}');
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
