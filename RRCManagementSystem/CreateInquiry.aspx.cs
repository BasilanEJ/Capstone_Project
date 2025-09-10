using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Collections.Generic;

namespace RRCManagementSystem
{
    public partial class CreateInquiry : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;
        private const string UploadVirtualFolder = "~/Uploads/InquiryPhotos/";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            var role = Session["Role"].ToString();
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (Page.Form != null) Page.Form.Enctype = "multipart/form-data";

            if (!IsPostBack)
            {
                int userId = Convert.ToInt32(Session["UserID"]);
                if (!HasPermission(userId, "ManageInquiry", "CanView"))
                {
                    Response.Redirect("~/Unauthorized.aspx");
                    return;
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int userId = Convert.ToInt32(Session["UserID"]);
            if (!HasPermission(userId, "ManageInquiry", "CanAdd"))
            {
                ShowSwal("Permission Denied", "You do not have permission to add inquiries.", "error");
                return;
            }

            string email = (txtEmail.Text ?? "").Trim().ToLowerInvariant();
            string contact = (txtContact.Text ?? "").Trim();
            string message = string.IsNullOrWhiteSpace(txtMessage.Text) ? "N/A" : txtMessage.Text.Trim();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(contact))
            {
                ShowSwal("Missing Fields", "Please fill in Email and Contact Number.", "warning");
                return;
            }

            // Basic shape validation first
            if (!IsValidEmail(email))
            {
                ShowSwal("Invalid Email", "Please enter a valid email address.", "warning");
                return;
            }

            // Strict allow-list: consumer domains or any *.edu.ph / *.gov.ph
            if (!IsAllowedEmailDomain(email, out string domain))
            {
                ShowSwal("Invalid Email",
                    "Only Gmail, Yahoo, Outlook/Hotmail/Live/MSN, iCloud/Me/Mac, Proton, Zoho, or school/government (*.edu.ph / *.gov.ph) emails are allowed.",
                    "warning");
                return;
            }

            if (!Regex.IsMatch(contact, @"^\d{11}$"))
            {
                ShowSwal("Invalid Contact Number", "Contact Number must be exactly 11 digits.", "warning");
                return;
            }

            string photoPath = null;

            try
            {
                // optional upload
                if (fuPhoto.HasFile)
                {
                    if (!ValidateUpload(fuPhoto))
                    {
                        ShowSwal("Invalid File", "Only .jpg, .jpeg, .png up to 5 MB are allowed.", "warning");
                        return;
                    }

                    string uploadsPhysicalPath = Server.MapPath(UploadVirtualFolder);
                    if (!Directory.Exists(uploadsPhysicalPath))
                        Directory.CreateDirectory(uploadsPhysicalPath);

                    string ext = Path.GetExtension(fuPhoto.FileName);
                    string uniqueName = $"inq_{Guid.NewGuid():N}{ext}";
                    fuPhoto.SaveAs(Path.Combine(uploadsPhysicalPath, uniqueName));
                    photoPath = UploadVirtualFolder + uniqueName;
                }

                string genCode;
                int newId = InsertInquiry_SP(
                    email, contact, message, photoPath,
                    (txtFirstName.Text ?? "").Trim(),
                    (txtMiddleName.Text ?? "").Trim(),
                    (txtLastName.Text ?? "").Trim(),
                    (txtStreet.Text ?? "").Trim(),
                    (txtBarangay.Text ?? "").Trim(),
                    (txtCity.Text ?? "").Trim(),
                    (txtRegion.Text ?? "").Trim(),
                    (txtCountry.Text ?? "").Trim(),
                    (txtLandmark.Text ?? "").Trim(),
                    out genCode
                );

                if (newId > 0)
                {
                    // Try to email; show a softer warning if it fails
                    try
                    {
                        SendConfirmationEmail(email, genCode);
                    }
                    catch (Exception exMail)
                    {
                        ShowSwal("Email Warning",
                            "Inquiry saved, but failed to send confirmation email: " + HttpUtility.HtmlEncode(exMail.Message),
                            "warning");
                    }

                    // Show code, then go back to list
                    string script = $@"
                        setTimeout(function(){{
                            Swal.fire({{
                                icon: 'success',
                                title: 'Inquiry Saved',
                                text: 'Reference Code: {genCode}'
                            }}).then(() => {{
                                window.location = 'AllInquiry.aspx';
                            }});
                        }}, 0);";
                    ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString("N"), script, true);
                }
                else
                {
                    ShowSwal("Error", "Failed to save inquiry. Please try again.", "error");
                }

            }
            catch (Exception ex)
            {
                ShowSwal("Error", "An error occurred: " + HttpUtility.HtmlEncode(ex.Message), "error");
            }
        }

        private int InsertInquiry_SP(
            string email, string contact, string message, string photoPath,
            string firstName, string middleName, string lastName,
            string street, string barangay, string city, string region, string country, string landmark,
            out string generatedCode)
        {
            generatedCode = null;

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spInquiry_Create", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = email;
                cmd.Parameters.Add("@ContactNumber", SqlDbType.NVarChar, 20).Value = contact;
                cmd.Parameters.Add("@Message", SqlDbType.NVarChar).Value = (object)message ?? DBNull.Value;
                cmd.Parameters.Add("@PhotoPath", SqlDbType.NVarChar, 255).Value = (object)photoPath ?? DBNull.Value;

                cmd.Parameters.Add("@FirstName", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(firstName) ? (object)DBNull.Value : firstName;
                cmd.Parameters.Add("@MiddleName", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(middleName) ? (object)DBNull.Value : middleName;
                cmd.Parameters.Add("@LastName", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(lastName) ? (object)DBNull.Value : lastName;

                cmd.Parameters.Add("@StreetAndUnit", SqlDbType.NVarChar, 255).Value = string.IsNullOrWhiteSpace(street) ? (object)DBNull.Value : street;
                cmd.Parameters.Add("@Barangay", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(barangay) ? (object)DBNull.Value : barangay;
                cmd.Parameters.Add("@City", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(city) ? (object)DBNull.Value : city;
                cmd.Parameters.Add("@Region", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(region) ? (object)DBNull.Value : region;
                cmd.Parameters.Add("@Country", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(country) ? (object)DBNull.Value : country;
                cmd.Parameters.Add("@Landmark", SqlDbType.NVarChar, 255).Value = string.IsNullOrWhiteSpace(landmark) ? (object)DBNull.Value : landmark;

                // OUTPUT params
                var pCode = new SqlParameter("@GeneratedInquiryCode", SqlDbType.NVarChar, 25)
                { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(pCode);

                var pId = new SqlParameter("@NewInquiryID", SqlDbType.Int)
                { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(pId);

                conn.Open();
                cmd.ExecuteNonQuery();

                generatedCode = Convert.ToString(pCode.Value ?? "");
                return (pId.Value == DBNull.Value) ? 0 : Convert.ToInt32(pId.Value);
            }
        }

        // ===== Domain allow-list (exact + suffix) =====
        private static readonly HashSet<string> AllowedExactDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Gmail family
            "gmail.com",

            // Yahoo family
            "yahoo.com", "ymail.com", "rocketmail.com",

            // Microsoft family
            "outlook.com", "hotmail.com", "live.com", "msn.com",

            // Apple family
            "icloud.com", "me.com", "mac.com",

            // Proton & Zoho
            "protonmail.com", "proton.me",
            "zoho.com", "zohomail.com"
        };

        private static readonly string[] AllowedSuffixes = new[]
        {
            ".edu.ph",
            ".gov.ph"
        };

        private static bool IsAllowedEmailDomain(string email, out string domain)
        {
            domain = "";
            if (string.IsNullOrWhiteSpace(email)) return false;

            int at = email.IndexOf('@');
            if (at < 0 || at == email.Length - 1) return false;

            domain = email.Substring(at + 1).Trim().ToLowerInvariant();

            if (AllowedExactDomains.Contains(domain))
                return true;

            foreach (var sfx in AllowedSuffixes)
                if (domain.EndsWith(sfx, StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }

        // === EMAIL SENDER (Gmail) ===
        private void SendConfirmationEmail(string toEmail, string inquiryCode)
        {
            string fromEmail = ConfigurationManager.AppSettings["emailFrom"];
            string appPassword = ConfigurationManager.AppSettings["emailPassword"];

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using (var mail = new MailMessage())
            {
                mail.From = new MailAddress(fromEmail, "RRC Management System", Encoding.UTF8);
                mail.To.Add(new MailAddress(toEmail));
                mail.ReplyToList.Add(new MailAddress("rrctermiteandpestcontrol@gmail.com"));

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

                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(fromEmail, appPassword);
                    smtp.EnableSsl = true; // STARTTLS
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Timeout = 20000;
                    smtp.Send(mail);
                }
            }
        }

        private bool HasPermission(int userId, string moduleName, string permissionColumn)
        {
            string perm = (permissionColumn == "CanView" || permissionColumn == "CanAdd" ||
                           permissionColumn == "CanEdit" || permissionColumn == "CanDelete")
                          ? permissionColumn : "CanView";

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spAdminPermission_Check", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@ModuleName", SqlDbType.NVarChar, 100).Value = moduleName;
                cmd.Parameters.Add("@Permission", SqlDbType.NVarChar, 10).Value = perm;

                conn.Open();
                object val = cmd.ExecuteScalar();
                return val != null && Convert.ToBoolean(val);
            }
        }

        private static bool ValidateUpload(FileUpload fu)
        {
            const int MAX = 5 * 1024 * 1024;
            if (fu.PostedFile.ContentLength <= 0 || fu.PostedFile.ContentLength > MAX) return false;

            string ext = (Path.GetExtension(fu.FileName) ?? "").ToLowerInvariant();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png") return false;

            string mime = (fu.PostedFile.ContentType ?? "").ToLowerInvariant();
            if (!mime.StartsWith("image/")) return false;

            return true;
        }

        private static bool IsValidEmail(string email)
            => Regex.IsMatch(email ?? "", @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        private void ShowSwal(string title, string text, string icon)
        {
            string script = $@"
                setTimeout(function(){{
                    Swal.fire({{
                        title: {ToJsString(title)},
                        text: {ToJsString(text)},
                        icon: '{icon}'
                    }});
                }}, 0);";
            ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString("N"), script, true);
        }

        private static string ToJsString(string s)
        {
            if (s == null) return "''";
            return "'" + s.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\r", "").Replace("\n", "\\n") + "'";
        }
    }
}
