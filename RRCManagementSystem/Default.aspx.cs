using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using Isopoh.Cryptography.Argon2;
using System.Net.Mail;
using System.Net;
using System.IO;
using System.Xml.Linq;

namespace RRCManagementSystem
{
    public partial class Default : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
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

            // ✅ Validate 11-digit numeric contact number
            if (!System.Text.RegularExpressions.Regex.IsMatch(contact, @"^\d{11}$"))
            {
                ShowSweetAlert("Invalid Contact", "Please enter a valid 11-digit contact number.", "warning");
                return;
            }

            string photoPath = null;
            if (fuPestPhoto.HasFile)
            {
                try
                {
                    string filename = Guid.NewGuid().ToString() + Path.GetExtension(fuPestPhoto.FileName);
                    string savePath = Server.MapPath("~/UploadedPestPhotos/") + filename;
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
                ShowSweetAlert("Database Error", "Something went wrong while saving: " + ex.Message, "error");
            }
        }


        private void SendConfirmationEmail(string toEmail)
        {
            string fromEmail = ConfigurationManager.AppSettings["edgarjosephbasilan@gmail.com"];
            string password = ConfigurationManager.AppSettings["fbryvkhttqobssjy"];

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
            string script = $@"<script src='https://cdn.jsdelivr.net/npm/sweetalert2@11'></script>
                <script>Swal.fire({{title: '{title}', text: '{message}', icon: '{icon}', confirmButtonColor: '#007bff'}});</script>";
            ClientScript.RegisterStartupScript(this.GetType(), "SweetAlert", script);
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
