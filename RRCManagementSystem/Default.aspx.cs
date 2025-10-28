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
                LoadServicesFromDatabase(); // Load dynamic services
                ApplyLazyLoadingToImages();
                LoadReviews();
                LoadBlogs();



                fuPestPhoto.Attributes["accept"] = "image/png,image/jpeg,image/jpg";

                MaintainScrollPositionOnPostBack = true;
            }
        }


        // Add this method in your Page_Load (inside if (!IsPostBack))
        private void LoadBlogs()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                SELECT BlogID, BlogTitle, BlogDescription, BlogContent, 
                       ImagePath, DisplayOrder
                FROM BlogsCMS
                WHERE IsActive = 1
                ORDER BY DisplayOrder, BlogID DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        rptBlogs.DataSource = dt;
                        rptBlogs.DataBind();

                        rptBlogModals.DataSource = dt;
                        rptBlogModals.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Blog Load Error: " + ex.Message);
            }
        }

        // Add this helper method to format blog content
        protected string FormatBlogContent(string content)
        {
            if (string.IsNullOrEmpty(content))
                return "";

            // Convert line breaks to HTML
            content = content.Replace("\r\n", "<br>");
            content = content.Replace("\n", "<br>");

            // Convert bullet points
            content = System.Text.RegularExpressions.Regex.Replace(
                content,
                @"^\s*[-•]\s*(.+)$",
                "<li>$1</li>",
                System.Text.RegularExpressions.RegexOptions.Multiline
            );

            // Wrap lists in <ul> tags
            if (content.Contains("<li>"))
            {
                content = System.Text.RegularExpressions.Regex.Replace(
                    content,
                    @"(<li>.*?</li>(\s*<br>)*)+",
                    match => "<ul style='margin-left:20px; margin-bottom:20px;'>" +
                             match.Value.Replace("<br>", "") + "</ul>"
                );
            }

            return content;
        }


        private void LoadReviews()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                SELECT ReviewID, CustomerName, ReviewText, Rating, Recommends
                FROM Reviews
                WHERE IsActive = 1
                ORDER BY DisplayOrder, ReviewID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        rptReviews.DataSource = dt;
                        rptReviews.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Review Load Error: " + ex.Message);
            }
        }

        // 3. ADD THIS HELPER METHOD to display star ratings
        protected string GetStarRatingForReview(int rating)
        {
            string stars = "";
            for (int i = 0; i < rating; i++)
            {
                stars += "⭐";
            }
            return stars;
        }


        private void LoadServicesFromDatabase()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT ServiceID, ServiceType, ServiceTitle, ServiceDescription, 
                               BulletPoints, ImagePath, DisplayOrder
                        FROM ServicesCMS
                        WHERE IsActive = 1
                        ORDER BY ServiceType, DisplayOrder";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        // Filter and bind Termite Control services
                        DataView dvTermite = new DataView(dt);
                        dvTermite.RowFilter = "ServiceType = 'Termite Control'";
                        rptTermiteServices.DataSource = dvTermite;
                        rptTermiteServices.DataBind();
                        rptTermiteModals.DataSource = dvTermite;
                        rptTermiteModals.DataBind();

                        // Filter and bind General Pest Control services
                        DataView dvPest = new DataView(dt);
                        dvPest.RowFilter = "ServiceType = 'General Pest Control'";
                        rptPestServices.DataSource = dvPest;
                        rptPestServices.DataBind();
                        rptPestModals.DataSource = dvPest;
                        rptPestModals.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Service Load Error: " + ex.Message);
            }
        }

        protected string GetShortDescription(string description)
        {
            if (string.IsNullOrEmpty(description))
                return "";

            // Return first 80 characters or until first period
            int maxLength = 80;
            if (description.Length <= maxLength)
                return description;

            // Try to cut at a sentence
            int periodIndex = description.IndexOf('.', 0, Math.Min(description.Length, maxLength));
            if (periodIndex > 0)
                return description.Substring(0, periodIndex + 1);

            // Otherwise just cut at maxLength
            return description.Substring(0, maxLength) + "...";
        }

        protected string FormatBulletPoints(string bulletPoints)
        {
            if (string.IsNullOrEmpty(bulletPoints))
                return "";

            var lines = bulletPoints.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var formatted = new System.Text.StringBuilder();

            foreach (var line in lines)
            {
                string trimmed = line.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    // Remove leading dash or bullet if present
                    if (trimmed.StartsWith("-") || trimmed.StartsWith("•"))
                        trimmed = trimmed.Substring(1).Trim();

                    formatted.AppendLine($"<li>{trimmed}</li>");
                }
            }

            return formatted.ToString();
        }



        private void ApplyLazyLoadingToImages()
        {
            // Hero and About images
            SetLazyLoadImage(imgHeroBanner);
            SetLazyLoadImage(imgAbout);
            SetLazyLoadImage(imgVideoThumbnail);
            SetLazyLoadImage(imgCO);


        }

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

        private string GetImagePath(SqlDataReader reader, string columnName, string defaultPath)
        {
            try
            {
                string path = reader[columnName]?.ToString();
                return string.IsNullOrEmpty(path) ? defaultPath : path;
            }
            catch
            {
                return defaultPath;
            }
        }

        private void SetDefaultImages()
        {
            imgHeroBanner.ImageUrl = "/images/rrc1.png";
            imgAbout.ImageUrl = "/images/ppe.png";
            imgVideoThumbnail.ImageUrl = "/images/banner tv.jpg";
            imgCO.ImageUrl = "/images/c&o.png";
        }

        private string ExtractVideoId(string url, string videoType)
        {
            if (string.IsNullOrEmpty(url)) return "";

            try
            {
                if (videoType == "YouTube")
                {
                    if (url.Contains("watch?v="))
                    {
                        var uri = new Uri(url);
                        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                        return query["v"] ?? "";
                    }
                    else if (url.Contains("youtu.be/"))
                    {
                        return url.Split('/').Last();
                    }
                    else if (url.Contains("embed/"))
                    {
                        return url.Split('/').Last();
                    }
                }
                else if (videoType == "Vimeo")
                {
                    var parts = url.Split('/');
                    return parts.Last();
                }
            }
            catch
            {
                return "";
            }

            return "";
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

            // ================================
            // 4. Validate Contact Number
            // ================================
            if (!System.Text.RegularExpressions.Regex.IsMatch(contact, @"^09\d{9}$"))
            {
                ShowSweetAlert("Invalid Contact", "Contact number must be 11 digits starting with 09.", "warning");
                return;
            }

            // ================================
            // 5. Handle File Upload
            // ================================
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

                    // Path for database
                    photoPath = "/Uploads/InquiryPhotos/" + filename;
                }
                catch (Exception ex)
                {
                    // Log the full error: Logger.LogError(ex, "File upload failed");
                    ShowSweetAlert("Upload Error", "Unable to save uploaded photo.", "error");
                    return;
                }
            }

            // ================================
            // 6. Database Operation
            // ================================
            try
            {
                string emailHash = AESHelper.ComputeSHA256WithPepper(email); // For search
                string emailEnc = AESHelper.EncryptEmail(email);
                string contactEnc = AESHelper.EncryptField(contact);

                // Set empty values for fields not on this form
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

                SendConfirmationEmail(email, generatedCode);

                ShowSweetAlert("Submitted!", $"Your inquiry was submitted successfully.<br>Reference Code: {generatedCode}", "success");

                ClearForm();
            } // End of try block
            catch (SqlException sqlEx)
            {

                if (sqlEx.Number == 50001)
                {
                    ShowSweetAlert(
                        "Inquiry Already Received", // <-- User-friendly title
                        "You have already submitted an inquiry recently. Please wait 24 hours before submitting another one.",
                        "info" // <-- Use "info" (blue) or "warning" (yellow)
                    );
                }
                // Check for a "Unique Constraint" violation
                else if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                {
                    ShowSweetAlert(
                        "Inquiry Already Received",
                        "We've already received your inquiry. A representative will contact you soon.",
                        "info"
                    );
                }
                else
                {

                    ShowSweetAlert(
                        "Submission Failed", // <-- User-friendly title
                        "Sorry, a system error occurred. Please try again later.", // <-- Generic, safe message
                        "error"
                    );
                }
            } 
            catch (Exception ex) // <-- **FIX:** This is now its own separate catch block
            {
            
                ShowSweetAlert(
                    "Submission Failed",
                    "Sorry, something went wrong. Please try again.", // <-- Generic, safe message
                    "error"
                );
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