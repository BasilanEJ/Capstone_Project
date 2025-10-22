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
            if (Page.Form != null)
            {
                Page.Form.Enctype = "multipart/form-data";
            }

            if (!IsPostBack)
            {

                LoadCMSContent();


                ApplyLazyLoadingToImages();

                fuPestPhoto.Attributes["accept"] = "image/png,image/jpeg,image/jpg";

                MaintainScrollPositionOnPostBack = true;
            }
        }

        // Helper method to apply lazy loading to all images
        private void ApplyLazyLoadingToImages()
        {
            // Hero and About images
            SetLazyLoadImage(imgHeroBanner);
            SetLazyLoadImage(imgAbout);
            SetLazyLoadImage(imgVideoThumbnail);

            // Service card images (Termite Control)
            SetLazyLoadImage(imgBaiting);
            SetLazyLoadImage(imgTermitePrevention);
            SetLazyLoadImage(imgSoil);
            SetLazyLoadImage(imgReticulation);
            SetLazyLoadImage(imgMound);

            // Service card images (General Pest Control)
            SetLazyLoadImage(imgGeneralPest);
            SetLazyLoadImage(imgTickFleas);
            SetLazyLoadImage(imgBedbugs);
            SetLazyLoadImage(imgRats);

            // Modal images (Termite Control)
            SetLazyLoadImage(imgModalBaiting);
            SetLazyLoadImage(imgModalTermitePrevention);
            SetLazyLoadImage(imgModalSoil);
            SetLazyLoadImage(imgModalReticulation);
            SetLazyLoadImage(imgModalMound);

            // Modal images (General Pest Control)
            SetLazyLoadImage(imgModalGeneralPest);
            SetLazyLoadImage(imgModalTickFleas);
            SetLazyLoadImage(imgModalBedbugs);
            SetLazyLoadImage(imgModalRats);

            // Blog images
            SetLazyLoadImage(imgBlog1);
            SetLazyLoadImage(imgBlog2);
            SetLazyLoadImage(imgBlog3);

            // Certifications & Organizations
            SetLazyLoadImage(imgCO);
        }

        // Simplified helper method - reads current ImageUrl value
        private void SetLazyLoadImage(System.Web.UI.WebControls.Image imgControl)
        {
            if (imgControl != null && !string.IsNullOrEmpty(imgControl.ImageUrl))
            {
                // Store the actual image path in data-src attribute
                imgControl.Attributes["data-src"] = ResolveUrl(imgControl.ImageUrl);

                // Set a transparent SVG placeholder as the initial src
                imgControl.ImageUrl = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1 1'%3E%3C/svg%3E";

                // Add loading attribute for native lazy loading support (modern browsers)
                imgControl.Attributes["loading"] = "lazy";
            }
        }

        // ADD THESE METHODS TO YOUR Default.aspx.cs

        private void LoadCMSContent()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM CMSContent WHERE ID = 1", conn))
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        // Hero Banner
                        imgHeroBanner.ImageUrl = GetImagePath(reader, "HeroBannerPath", "/images/rrc1.png");

                        // Service Images (for cards)
                        imgBaiting.ImageUrl = GetImagePath(reader, "BaitingImagePath", "/images/service-baiting.jpg");
                        imgTermitePrevention.ImageUrl = GetImagePath(reader, "TermitePreventionImagePath", "/images/service-termite-prevention.jpg");
                        imgSoil.ImageUrl = GetImagePath(reader, "SoilImagePath", "/images/service-soil.jpg");
                        imgReticulation.ImageUrl = GetImagePath(reader, "ReticulationImagePath", "/images/services-reticulations.jpg");
                        imgMound.ImageUrl = GetImagePath(reader, "MoundImagePath", "/images/service-mound.jpg");
                        imgGeneralPest.ImageUrl = GetImagePath(reader, "GeneralPestImagePath", "/images/service-general-pest.jpg");
                        imgTickFleas.ImageUrl = GetImagePath(reader, "TickFleasImagePath", "/images/service-tick-fleas.jpg");
                        imgBedbugs.ImageUrl = GetImagePath(reader, "BedbugsImagePath", "/images/service-bedbugs.jpg");
                        imgRats.ImageUrl = GetImagePath(reader, "RatsImagePath", "/images/service-rats.jpg");

                        // Modal Images (same as card images)
                        imgModalBaiting.ImageUrl = imgBaiting.ImageUrl;
                        imgModalTermitePrevention.ImageUrl = imgTermitePrevention.ImageUrl;
                        imgModalSoil.ImageUrl = imgSoil.ImageUrl;
                        imgModalReticulation.ImageUrl = imgReticulation.ImageUrl;
                        imgModalMound.ImageUrl = imgMound.ImageUrl;
                        imgModalGeneralPest.ImageUrl = imgGeneralPest.ImageUrl;
                        imgModalTickFleas.ImageUrl = imgTickFleas.ImageUrl;
                        imgModalBedbugs.ImageUrl = imgBedbugs.ImageUrl;
                        imgModalRats.ImageUrl = imgRats.ImageUrl;

                        // About Section
                        imgAbout.ImageUrl = GetImagePath(reader, "AboutImagePath", "/images/ppe.png");

                        // Video Section
                        imgVideoThumbnail.ImageUrl = GetImagePath(reader, "VideoThumbnailPath", "/images/banner tv.jpg");
                        string videoUrl = reader["VideoUrl"]?.ToString() ?? "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
                        string videoType = reader["VideoType"]?.ToString() ?? "YouTube";
                        string videoId = ExtractVideoId(videoUrl, videoType);
                        hfVimeoVideoId.Value = videoId;
                        hfVideoType.Value = videoType;

                        // C&O Section
                        imgCO.ImageUrl = GetImagePath(reader, "COImagePath", "/images/c&o.png");

                        // Blog Images
                        imgBlog1.ImageUrl = GetImagePath(reader, "Blog1ImagePath", "/Images/DIY.jpg");
                        imgBlog2.ImageUrl = GetImagePath(reader, "Blog2ImagePath", "/Images/blog2.jpg");
                        imgBlog3.ImageUrl = GetImagePath(reader, "Blog3ImagePath", "/Images/blog3.jpg");

                        // Load Service Modal Content
                        LoadServiceModalContent(reader);
                    }
                    else
                    {
                        SetDefaultImages();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("CMS Load Error: " + ex.Message);
                SetDefaultImages();
            }
        }

        private void LoadServiceModalContent(SqlDataReader reader)
        {
            // Baiting System
            litBaitingTitle.Text = reader["BaitingTitle"]?.ToString() ?? "🛡️ Baiting System";
            litBaitingDescription.Text = FormatDescription(reader["BaitingDescription"]?.ToString());
            litBaitingBullets.Text = FormatBulletPoints(reader["BaitingBulletPoints"]?.ToString());

            // Termite Prevention
            litTermitePreventionTitle.Text = reader["TermitePreventionTitle"]?.ToString() ?? "🔰 Termite Prevention";
            litTermitePreventionDescription.Text = FormatDescription(reader["TermitePreventionDescription"]?.ToString());
            litTermitePreventionBullets.Text = FormatBulletPoints(reader["TermitePreventionBulletPoints"]?.ToString());

            // Soil Poisoning
            litSoilTitle.Text = reader["SoilTitle"]?.ToString() ?? "🏗️ Soil Poisoning Treatment";
            litSoilDescription.Text = FormatDescription(reader["SoilDescription"]?.ToString());
            litSoilBullets.Text = FormatBulletPoints(reader["SoilBulletPoints"]?.ToString());

            // Reticulation
            litReticulationTitle.Text = reader["ReticulationTitle"]?.ToString() ?? "⚙️ Reticulation System";
            litReticulationDescription.Text = FormatDescription(reader["ReticulationDescription"]?.ToString());
            litReticulationBullets.Text = FormatBulletPoints(reader["ReticulationBulletPoints"]?.ToString());

            // Mound Demolition
            litMoundTitle.Text = reader["MoundTitle"]?.ToString() ?? "🎯 Mound Demolition";
            litMoundDescription.Text = FormatDescription(reader["MoundDescription"]?.ToString());
            litMoundBullets.Text = FormatBulletPoints(reader["MoundBulletPoints"]?.ToString());

            // General Pest Control
            litGeneralPestTitle.Text = reader["GeneralPestTitle"]?.ToString() ?? "🐜 General Pest Control";
            litGeneralPestDescription.Text = FormatDescription(reader["GeneralPestDescription"]?.ToString());
            litGeneralPestBullets.Text = FormatBulletPoints(reader["GeneralPestBulletPoints"]?.ToString());

            // Tick & Fleas
            litTickFleasTitle.Text = reader["TickFleasTitle"]?.ToString() ?? "🐕 Tick & Fleas Control";
            litTickFleasDescription.Text = FormatDescription(reader["TickFleasDescription"]?.ToString());
            litTickFleasBullets.Text = FormatBulletPoints(reader["TickFleasBulletPoints"]?.ToString());

            // Bedbugs
            litBedbugsTitle.Text = reader["BedbugsTitle"]?.ToString() ?? "🛏️ Bedbugs Control";
            litBedbugsDescription.Text = FormatDescription(reader["BedbugsDescription"]?.ToString());
            litBedbugsBullets.Text = FormatBulletPoints(reader["BedbugsBulletPoints"]?.ToString());

            // Rats & Rodents
            litRatsTitle.Text = reader["RatsTitle"]?.ToString() ?? "🐀 Rat & Rodents Control";
            litRatsDescription.Text = FormatDescription(reader["RatsDescription"]?.ToString());
            litRatsBullets.Text = FormatBulletPoints(reader["RatsBulletPoints"]?.ToString());
        }


        private string FormatDescription(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "<p>No description available.</p>";

            // Split by double line breaks (paragraph separators)
            var paragraphs = text.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);

            // Wrap each paragraph in <p> tags
            var formattedParagraphs = paragraphs
                .Select(p => $"<p>{Server.HtmlEncode(p.Trim())}</p>");

            return string.Join("", formattedParagraphs);
        }

        /// <summary>
        /// Formats bullet points separated by pipe character into list items
        /// </summary>
        private string FormatBulletPoints(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "<li>No information available</li>";

            // Split by pipe character
            var bullets = text.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            // Wrap each bullet in <li> tags
            var formattedBullets = bullets
                .Select(b => $"<li>{Server.HtmlEncode(b.Trim())}</li>");

            return string.Join("", formattedBullets);
        }

        // Replace the ExtractVideoId method in Default.aspx.cs with this improved version:

        private string ExtractVideoId(string url, string videoType)
        {
            if (string.IsNullOrEmpty(url)) return "";

            try
            {
                if (videoType == "YouTube")
                {
                    // Handle various YouTube URL formats
                    if (url.Contains("youtube.com/watch?v="))
                    {
                        var uri = new Uri(url);
                        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                        return query["v"] ?? "";
                    }
                    else if (url.Contains("youtu.be/"))
                    {
                        var uri = new Uri(url);
                        return uri.AbsolutePath.TrimStart('/').Split('?')[0];
                    }
                    else if (url.Contains("youtube.com/embed/"))
                    {
                        var uri = new Uri(url);
                        string[] segments = uri.AbsolutePath.Split('/');
                        return segments.Length > 2 ? segments[2] : "";
                    }
                }
                else if (videoType == "Vimeo")
                {
                    // Handle various Vimeo URL formats
                    var uri = new Uri(url);

                    // Handle player.vimeo.com/video/ID format
                    if (url.Contains("player.vimeo.com/video/"))
                    {
                        string[] segments = uri.AbsolutePath.Split('/');
                        // segments will be: ["", "video", "1009218555"]
                        for (int i = 0; i < segments.Length; i++)
                        {
                            if (segments[i] == "video" && i + 1 < segments.Length)
                            {
                                return segments[i + 1].Split('?')[0]; // Remove query params if any
                            }
                        }
                    }
                    // Handle standard vimeo.com/ID format
                    else if (url.Contains("vimeo.com/"))
                    {
                        return uri.AbsolutePath.TrimStart('/').Split('/')[0].Split('?')[0];
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error extracting video ID: {ex.Message}");
                return "";
            }

            return "";
        }

        private string GetImagePath(SqlDataReader reader, string columnName, string defaultPath)
        {
            string path = reader[columnName]?.ToString();
            return string.IsNullOrEmpty(path) ? defaultPath : path;
        }


        private void SetDefaultImages()
        {
            imgHeroBanner.ImageUrl = "/images/rrc1.png";
            imgBaiting.ImageUrl = "/images/service-baiting.jpg";
            imgTermitePrevention.ImageUrl = "/images/service-termite-prevention.jpg";
            imgSoil.ImageUrl = "/images/service-soil.jpg";
            imgReticulation.ImageUrl = "/images/services-reticulations.jpg";
            imgMound.ImageUrl = "/images/service-mound.jpg";
            imgGeneralPest.ImageUrl = "/images/service-general-pest.jpg";
            imgTickFleas.ImageUrl = "/images/service-tick-fleas.jpg";
            imgBedbugs.ImageUrl = "/images/service-bedbugs.jpg";
            imgRats.ImageUrl = "/images/service-rats.jpg";
            imgAbout.ImageUrl = "/images/ppe.png";
            imgVideoThumbnail.ImageUrl = "/images/banner tv.jpg";
            hfVimeoVideoId.Value = "dQw4w9WgXcQ";
            hfVideoType.Value = "YouTube";
            imgCO.ImageUrl = "/images/c&o.png";

            // Blog Images - ADD THESE LINES
            imgBlog1.ImageUrl = "/Images/DIY.jpg";
            imgBlog2.ImageUrl = "/Images/blog2.jpg";
            imgBlog3.ImageUrl = "/Images/blog3.jpg";
        }



        protected void btnSubmitInquiry_Click(object sender, EventArgs e)
        {
            // ================================
            // 1. Validate Terms & Conditions
            // ================================
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

            // ================================
            // 2. Gather Form Data
            // ================================
            string email = (txtEmail.Text ?? "").Trim().ToLowerInvariant();
            string contact = (txtContactNumber.Text ?? "").Trim();
            string message = (txtMessage.Text ?? "").Trim();

            // ================================
            // 3. Validate Email
            // ================================
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


            if (!System.Text.RegularExpressions.Regex.IsMatch(contact, @"^09\d{9}$"))
            {
                ShowSweetAlert("Invalid Contact", "Contact number must be 11 digits starting with 09.", "warning");
                return;
            }


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


                    string folderPhysicalPath = Server.MapPath("~/Uploads/InquiryPhotos/");
                    if (!Directory.Exists(folderPhysicalPath))
                        Directory.CreateDirectory(folderPhysicalPath);

                    // Generate unique filename
                    string filename = Guid.NewGuid().ToString("N") + extension;
                    string savePath = Path.Combine(folderPhysicalPath, filename);

                    // Save the file
                    fuPestPhoto.SaveAs(savePath);

                    // ✅ Correct path for database with ~ prefix
                    photoPath = "/Uploads/InquiryPhotos/" + filename;
                }
                catch (Exception ex)
                {
                    ShowSweetAlert("Upload Error", "Unable to save uploaded photo. " + ex.Message, "error");
                    return;
                }
            }


            try
            {

                string emailHash = AESHelper.ComputeSHA256WithPepper(email); // For search
                string emailEnc = AESHelper.EncryptEmail(email);
                string contactEnc = AESHelper.EncryptField(contact);


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

                    // ✅ Ensure correct path is stored in DB
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

                    var pCode = new SqlParameter("@GeneratedInquiryCode", SqlDbType.NVarChar, 25)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(pCode);

                    var pId = new SqlParameter("@NewInquiryID", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(pId);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    generatedCode = Convert.ToString(pCode.Value ?? "");
                }

                // Send confirmation email
                SendConfirmationEmail(email, generatedCode);

                // Success alert
                ShowSweetAlert("Submitted!", $"Your inquiry was submitted successfully.\\nReference Code: {generatedCode}", "success");

                // Clear form fields
                ClearForm();
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Message.Contains("Please wait 24 hours"))
                {
                    ShowSweetAlert(
                        "Wait Before Submitting",
                        "You have already submitted an inquiry recently. Please wait 24 hours before submitting another one.",
                        "warning"
                    );
                }
                else
                {
                    // Any other SQL error
                    ShowSweetAlert("Database Error", sqlEx.Message, "error");
                }
            }
            catch (Exception ex)
            {
                // Handle non-SQL exceptions (e.g., network issues, file save problems)
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
                            return true;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("Email sending failed: " + ex.Message);
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Critical error while sending email: " + ex.Message);
                return false;
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
            focusConfirm: false,
            backdrop: false,
            position: 'center',
            allowOutsideClick: false,
            allowEscapeKey: false,
            didOpen: () => {{
                if (history.scrollRestoration) {{
                    history.scrollRestoration = 'manual';
                }}
            }}
          }}).then(() => {{
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