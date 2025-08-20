using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Mail;
using System.Net;
using System.IO;
using System.Linq;

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

            string email = txtEmail.Text.Trim();
            string contact = txtContactNumber.Text.Trim();
            string message = txtMessage.Text.Trim();

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
                    string extension = Path.GetExtension(fuPestPhoto.FileName).ToLower();
                    string contentType = fuPestPhoto.PostedFile.ContentType.ToLower();

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

                    string filename = Guid.NewGuid().ToString() + extension;
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
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("dbo.spInquirySimple_Insert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@ContactNumber", contact);
                    cmd.Parameters.AddWithValue("@Message", (object)message ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PhotoPath", string.IsNullOrEmpty(photoPath) ? (object)DBNull.Value : photoPath);

                    // If you don’t have name/address inputs on this form, send safe defaults:
                    cmd.Parameters.AddWithValue("@LastName", "");   // or pull from a textbox if you add one
                    cmd.Parameters.AddWithValue("@FirstName", "");
                    cmd.Parameters.AddWithValue("@MiddleName", "");

                    cmd.Parameters.AddWithValue("@StreetAndUnit", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Barangay", DBNull.Value);
                    cmd.Parameters.AddWithValue("@City", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Region", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Country", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Landmark", DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                SendConfirmationEmail(email);
                ShowSweetAlert("Submitted!", "Your inquiry was submitted successfully.", "success");
                ClearForm();
            }
            catch (Exception ex)
            {
                ShowSweetAlert("Error", "Something went wrong while saving: " + ex.Message, "error");
            }
        }

        private void SendConfirmationEmail(string toEmail)
        {
            string fromEmail = ConfigurationManager.AppSettings["emailFrom"];
            string password = ConfigurationManager.AppSettings["emailPassword"]; // Set in Web.config

            MailMessage mail = new MailMessage
            {
                From = new MailAddress(fromEmail, "RRC Management System"),
                Subject = "RRC Inquiry Received",
                Body = "Thank you for contacting us! We will get back to you as soon as possible."
            };
            mail.To.Add(toEmail);

            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true
            };

            try
            {
                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                ShowSweetAlert("Email Error", "We saved your inquiry but failed to send confirmation: " + ex.Message, "warning");
            }
        }

        private void ShowSweetAlert(string title, string message, string icon)
        {
            string script = $@"
<script>
    Swal.fire({{
        title: '{title}',
        text: '{message}',
        icon: '{icon}',
        confirmButtonColor: '#007bff'
    }});
</script>";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "SweetAlert", script, false);
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
