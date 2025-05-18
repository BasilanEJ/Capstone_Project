using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Mail;
using System.Net;
using System.IO;

namespace RRCManagementSystem
{
    public partial class Default : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Optional test for loading
            // ClientScript.RegisterStartupScript(this.GetType(), "Log", "<script>console.log('Page Loaded');</script>", false);
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
            if (fuPestPhoto.HasFile)
            {
                try
                {
                    string filename = Guid.NewGuid().ToString() + Path.GetExtension(fuPestPhoto.FileName);
                    string folderPath = Server.MapPath("~/UploadedPestPhotos/");

                    // ✅ Create folder if it doesn't exist
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string savePath = Path.Combine(folderPath, filename);
                    fuPestPhoto.SaveAs(savePath);
                    photoPath = "~/UploadedPestPhotos/" + filename;
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
                {
                    string query = @"
                INSERT INTO InquirySimple (Email, ContactNumber, Message, PhotoPath, SubmittedAt)
                VALUES (@Email, @ContactNumber, @Message, @PhotoPath, GETDATE());";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@ContactNumber", contact);
                        cmd.Parameters.AddWithValue("@Message", message);
                        cmd.Parameters.AddWithValue("@PhotoPath", string.IsNullOrEmpty(photoPath) ? DBNull.Value : (object)photoPath);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
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
            string password = ConfigurationManager.AppSettings["emailPassword"]; // Make sure this key exists in your Web.config

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(fromEmail, "RRC Management System");
            mail.To.Add(toEmail);
            mail.Subject = "RRC Inquiry Received";
            mail.Body = "Thank you for contacting us! We will get back to you as soon as possible.";

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
