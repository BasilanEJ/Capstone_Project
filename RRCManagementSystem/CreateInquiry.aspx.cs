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
using RRCManagementSystem.Helpers; // AESHelper for encryption

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

                // Load regions only once on initial page load
                LoadRegions();
            }
        }

        /* ========================= SAVE BUTTON CLICK ========================= */
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

            // Validate email format
            if (!IsValidEmail(email))
            {
                ShowSwal("Invalid Email", "Please enter a valid email address.", "warning");
                return;
            }

            // Validate email domain
            if (!IsAllowedEmailDomain(email, out string domain))
            {
                ShowSwal("Invalid Email",
                    "Only Gmail, Yahoo, Outlook/Hotmail/Live/MSN, iCloud/Me/Mac, Proton, Zoho, or school/government (*.edu.ph / *.gov.ph) emails are allowed.",
                    "warning");
                return;
            }

            // Validate contact format
            if (!Regex.IsMatch(contact, @"^\d{11}$"))
            {
                ShowSwal("Invalid Contact Number", "Contact Number must be exactly 11 digits.", "warning");
                return;
            }

            // Optional photo upload
            string photoPath = null;
            try
            {
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

                // =========================================
                // ENCRYPTION + HASHING BEFORE SAVING
                // =========================================
                string street = (txtStreet.Text ?? "").Trim();
                string barangay = (txtBarangay.Text ?? "").Trim();
                string city = (ddlCity.SelectedValue ?? "").Trim();
                string region = (ddlRegion.SelectedValue ?? "").Trim();
                string country = (txtCountry.Text ?? "").Trim();
                string landmark = (txtLandmark.Text ?? "").Trim();

                // Generate SHA-256 hash for email uniqueness/search
                string emailHash = AESHelper.ComputeSHA256WithPepper(email);

                // Encrypt sensitive fields
                string emailEnc = AESHelper.EncryptEmail(email);
                string contactEnc = AESHelper.EncryptField(contact);
                string streetEnc = AESHelper.EncryptField(street);
                string barangayEnc = AESHelper.EncryptField(barangay);
                string cityEnc = AESHelper.EncryptField(city);
                string regionEnc = AESHelper.EncryptField(region);
                string countryEnc = AESHelper.EncryptField(country);
                string landmarkEnc = AESHelper.EncryptField(landmark);

                // =========================================
                // SAVE TO DATABASE
                // =========================================
                string genCode;
                int newId = InsertInquiry_SP(
                    emailHash, emailEnc, contactEnc, message, photoPath,
                    (txtFirstName.Text ?? "").Trim(),
                    (txtMiddleName.Text ?? "").Trim(),
                    (txtLastName.Text ?? "").Trim(),
                    streetEnc, barangayEnc, cityEnc, regionEnc, countryEnc, landmarkEnc,
                    out genCode
                );

                if (newId > 0)
                {
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

        /* ========================= SQL SAVE ========================= */
        private int InsertInquiry_SP(
            string emailHash, string emailEnc, string contactEnc, string message, string photoPath,
            string firstName, string middleName, string lastName,
            string streetEnc, string barangayEnc, string cityEnc, string regionEnc, string countryEnc, string landmarkEnc,
            out string generatedCode)
        {
            generatedCode = null;

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spInquiry_Create", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@EmailHash", SqlDbType.Char, 64).Value = emailHash;
                cmd.Parameters.Add("@EmailEnc", SqlDbType.NVarChar).Value = emailEnc;
                cmd.Parameters.Add("@ContactEnc", SqlDbType.NVarChar).Value = contactEnc;
                cmd.Parameters.Add("@Message", SqlDbType.NVarChar).Value = (object)message ?? DBNull.Value;
                cmd.Parameters.Add("@PhotoPath", SqlDbType.NVarChar, 255).Value = (object)photoPath ?? DBNull.Value;

                cmd.Parameters.Add("@FirstName", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(firstName) ? (object)DBNull.Value : firstName;
                cmd.Parameters.Add("@MiddleName", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(middleName) ? (object)DBNull.Value : middleName;
                cmd.Parameters.Add("@LastName", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(lastName) ? (object)DBNull.Value : lastName;

                cmd.Parameters.Add("@StreetEnc", SqlDbType.NVarChar).Value = string.IsNullOrWhiteSpace(streetEnc) ? (object)DBNull.Value : streetEnc;
                cmd.Parameters.Add("@BarangayEnc", SqlDbType.NVarChar).Value = string.IsNullOrWhiteSpace(barangayEnc) ? (object)DBNull.Value : barangayEnc;
                cmd.Parameters.Add("@CityEnc", SqlDbType.NVarChar).Value = string.IsNullOrWhiteSpace(cityEnc) ? (object)DBNull.Value : cityEnc;
                cmd.Parameters.Add("@RegionEnc", SqlDbType.NVarChar).Value = string.IsNullOrWhiteSpace(regionEnc) ? (object)DBNull.Value : regionEnc;
                cmd.Parameters.Add("@CountryEnc", SqlDbType.NVarChar).Value = string.IsNullOrWhiteSpace(countryEnc) ? (object)DBNull.Value : countryEnc;
                cmd.Parameters.Add("@LandmarkEnc", SqlDbType.NVarChar).Value = string.IsNullOrWhiteSpace(landmarkEnc) ? (object)DBNull.Value : landmarkEnc;

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

        /* ========================= REGION & CITY ========================= */
        protected void ddlRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadCities(ddlRegion.SelectedValue);
        }

        private void LoadRegions()
        {
            ddlRegion.Items.Clear();
            ddlRegion.Items.Add(new ListItem("-- Select Region --", ""));
            ddlRegion.Items.Add(new ListItem("NCR - National Capital Region", "NCR"));
            ddlRegion.Items.Add(new ListItem("Region I - Ilocos Region", "Region I"));
            ddlRegion.Items.Add(new ListItem("Region II - Cagayan Valley", "Region II"));
            ddlRegion.Items.Add(new ListItem("Region III - Central Luzon", "Region III"));
            ddlRegion.Items.Add(new ListItem("Region IV-A - CALABARZON", "Region IV-A"));
            ddlRegion.Items.Add(new ListItem("Region IV-B - MIMAROPA", "Region IV-B"));
            ddlRegion.Items.Add(new ListItem("Region V - Bicol Region", "Region V"));
            ddlRegion.Items.Add(new ListItem("Region VI - Western Visayas", "Region VI"));
            ddlRegion.Items.Add(new ListItem("Region VII - Central Visayas", "Region VII"));
        }

        private void LoadCities(string selectedRegion)
        {
            ddlCity.Items.Clear();
            ddlCity.Items.Add(new ListItem("-- Select City --", ""));
            if (selectedRegion == "NCR")
            {
                ddlCity.Items.Add(new ListItem("Quezon City", "Quezon City"));
                ddlCity.Items.Add(new ListItem("Manila", "Manila"));
                ddlCity.Items.Add(new ListItem("Makati", "Makati"));
                ddlCity.Items.Add(new ListItem("Caloocan", "Caloocan"));
                ddlCity.Items.Add(new ListItem("Las Piñas", "Las Piñas"));
                ddlCity.Items.Add(new ListItem("Pasig", "Pasig"));
                ddlCity.Items.Add(new ListItem("Taguig", "Taguig"));
                ddlCity.Items.Add(new ListItem("Valenzuela", "Valenzuela"));
                ddlCity.Items.Add(new ListItem("Pasay", "Pasay"));
                ddlCity.Items.Add(new ListItem("Marikina", "Marikina"));
                ddlCity.Items.Add(new ListItem("Muntinlupa", "Muntinlupa"));
                ddlCity.Items.Add(new ListItem("Navotas", "Navotas"));
                ddlCity.Items.Add(new ListItem("San Juan", "San Juan"));
                ddlCity.Items.Add(new ListItem("Pateros", "Pateros"));
            }
            else if (selectedRegion == "Region I")
            {
                ddlCity.Items.Add(new ListItem("Vigan", "Vigan"));
                ddlCity.Items.Add(new ListItem("San Fernando", "San Fernando"));
                ddlCity.Items.Add(new ListItem("Dagupan", "Dagupan"));
            }
            else if (selectedRegion == "Region II")
            {
                ddlCity.Items.Add(new ListItem("Tuguegarao", "Tuguegarao"));
                ddlCity.Items.Add(new ListItem("Ilagan", "Ilagan"));
            }
            else if (selectedRegion == "Region III")
            {
                ddlCity.Items.Add(new ListItem("San Fernando", "San Fernando"));
                ddlCity.Items.Add(new ListItem("Angeles", "Angeles"));
            }
            else if (selectedRegion == "Region IV-A")
            {
                ddlCity.Items.Add(new ListItem("Cavite", "Cavite"));
                ddlCity.Items.Add(new ListItem("Batangas", "Batangas"));
            }
            else if (selectedRegion == "Region IV-B")
            {
                ddlCity.Items.Add(new ListItem("Puerto Princesa", "Puerto Princesa"));
                ddlCity.Items.Add(new ListItem("Calapan", "Calapan"));
            }
            else if (selectedRegion == "Region V")
            {
                ddlCity.Items.Add(new ListItem("Legazpi", "Legazpi"));
                ddlCity.Items.Add(new ListItem("Naga", "Naga"));
            }
            else if (selectedRegion == "Region VI")
            {
                ddlCity.Items.Add(new ListItem("Iloilo City", "Iloilo City"));
                ddlCity.Items.Add(new ListItem("Bacolod", "Bacolod"));
            }
            else if (selectedRegion == "Region VII")
            {
                ddlCity.Items.Add(new ListItem("Cebu City", "Cebu City"));
                ddlCity.Items.Add(new ListItem("Dumaguete", "Dumaguete"));
            }
            else
            {
                ddlCity.Items.Add(new ListItem("No Cities Available", ""));
            }
        }

        /* ========================= VALIDATION HELPERS ========================= */
        private static readonly HashSet<string> AllowedExactDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "gmail.com",
            "yahoo.com", "ymail.com", "rocketmail.com",
            "outlook.com", "hotmail.com", "live.com", "msn.com",
            "icloud.com", "me.com", "mac.com",
            "protonmail.com", "proton.me",
            "zoho.com", "zohomail.com"
        };

        private static readonly string[] AllowedSuffixes = new[] { ".edu.ph", ".gov.ph" };

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

        private static bool IsValidEmail(string email)
            => Regex.IsMatch(email ?? "", @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        /* ========================= EMAIL CONFIRMATION ========================= */
        private bool SendConfirmationEmail(string toEmail, string inquiryCode)
        {
            try
            {
                // Fetch credentials from App.config or Web.config for improved security
                string fromEmail = ConfigurationManager.AppSettings["emailFrom"] ?? "rrctermiteandpestcontrol@gmail.com";
                string appPassword = ConfigurationManager.AppSettings["emailPassword"] ?? "";

                string subject = $"RRC Inquiry Received - Ref {inquiryCode}";

                // Professional HTML body for the inquiry confirmation email
                string body = $@"
<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>Inquiry Received - RRC Management System</title>
    <style type=""text/css"">
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif, 'Apple Color Emoji', 'Segoe UI Emoji', 'Segoe UI Symbol'; margin: 0; padding: 0; background-color: #f4f7fa; }}
        table {{ border-collapse: collapse; }}
        a {{ text-decoration: none; }}
        .inquiry-code {{ font-size: 24px; font-weight: bold; color: #2b6cb0; word-break: break-all; }}
        .content-box {{ background-color: #ffffff; border-radius: 8px; box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05); padding: 30px; }}
        .footer {{ font-size: 12px; color: #718096; margin-top: 25px; border-top: 1px solid #e2e8f0; padding-top: 20px; text-align: center; }}
    </style>
</head>
<body style=""margin: 0; padding: 0; background-color: #f4f7fa;"">
    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
        <tr>
            <td align=""center"" style=""padding: 20px;"">
                <table role=""presentation"" width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""max-width: 600px; width: 100%;"">
                    <tr>
                        <td align=""center"" style=""padding-bottom: 20px;"">
                            <h1 style=""color: #2b6cb0; font-size: 28px; margin: 0;"">RRC Management System</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 30px; background-color: #ffffff; border-radius: 8px; box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);"">
                            <h2 style=""color: #2d3748; font-size: 24px; margin: 0 0 15px;"">Inquiry Received</h2>
                            <p style=""color: #4a5568; font-size: 16px; line-height: 1.6; margin: 0 0 15px;"">Thank you for contacting R.R.C. Termite & Pest Control!</p>
                            <p style=""color: #4a5568; font-size: 16px; line-height: 1.6; margin: 0 0 15px;"">We have received your inquiry. Your reference code is:</p>
                            <p align=""center"" style=""margin: 0; text-align: center; font-size: 24px; font-weight: bold; color: #2b6cb0; padding: 10px 0; word-break: break-all;"">{inquiryCode}</p>
                            <p style=""color: #4a5568; font-size: 16px; line-height: 1.6; margin: 0 0 15px;"">Please keep this code so we can quickly find your record. We will get back to you as soon as possible.</p>
                            <p style=""color: #4a5568; font-size: 16px; line-height: 1.6; margin: 25px 0 0;"">—</p>
                            <p style=""color: #4a5568; font-size: 16px; line-height: 1.6; margin: 0;"">RRC Termite & Pest Control</p>
                        </td>
                    </tr>
                    <tr>
                        <td align=""center"" style=""padding-top: 20px;"">
                            <p style=""font-size: 12px; color: #718096; margin: 0; text-align: center;"">
                                If you did not make this inquiry, you can safely ignore this email.
                                <br/><br/>
                                &copy; 2024 RRC Management System. All rights reserved.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(fromEmail, "RRC Management System", Encoding.UTF8);
                    mail.To.Add(new MailAddress(toEmail));
                    mail.ReplyToList.Add(new MailAddress("rrctermiteandpestcontrol@gmail.com"));

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
                        smtp.Timeout = 20000;
                        smtp.Send(mail);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Email error: " + ex.Message);
                return false;
            }
        }


        /* ========================= MISC HELPERS ========================= */
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
