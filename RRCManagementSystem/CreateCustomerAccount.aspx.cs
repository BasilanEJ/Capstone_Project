using RRCManagementSystem.Helpers;
using System;
using System.Collections.Generic;      // HashSet
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;   // email shape check
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class CreateCustomerAccount : Page
    {
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // ✅ Require login
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            // ✅ Restrict certain roles
            var role = Convert.ToString(Session["Role"]);
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);
            if (!HasPermission(userId, "CreateCustomerAccount", "CanView"))
            {
                Response.Redirect("~/Unauthorized.aspx");
                return;
            }

            if (!IsPostBack)
            {
                // Ensure the button and validator share the same group and the button causes validation
                // (You can omit this if you’ve already set these in the .aspx markup)
                btnCreate.ValidationGroup = "inq";
                btnCreate.CausesValidation = true;
                revEmail.ValidationGroup = "inq";

                LoadRegions();
                txtCountry.Text = "Philippines";

                // ===== Prefill from InspectedInquiry =====
                if (Request.QueryString["prefill"] == "1")
                {
                    // Basic Info
                    txtLastName.Text = (Session["Prefill_LastName"] as string) ?? "";
                    txtFirstName.Text = (Session["Prefill_FirstName"] as string) ?? "";
                    txtMiddleName.Text = (Session["Prefill_MiddleName"] as string) ?? "";
                    txtEmail.Text = (Session["Prefill_Email"] as string) ?? "";
                    txtContact.Text = (Session["Prefill_Contact"] as string) ?? "";

                  
                    var region = (Session["Prefill_Region"] as string) ?? "";
                    var city = (Session["Prefill_City"] as string) ?? "";

                    if (!string.IsNullOrWhiteSpace(region) && ddlRegion.Items.FindByValue(region) != null)
                    {
                        ddlRegion.ClearSelection();
                        ddlRegion.Items.FindByValue(region).Selected = true;

                        LoadCities(region);

                        if (!string.IsNullOrWhiteSpace(city) && ddlCity.Items.FindByValue(city) != null)
                        {
                            ddlCity.ClearSelection();
                            ddlCity.Items.FindByValue(city).Selected = true;
                        }
                    }

                    
                    txtCountry.Text = (Session["Prefill_Country"] as string) ?? "Philippines";
                    txtBarangay.Text = (Session["Prefill_Barangay"] as string) ?? "";
                    txtStreet.Text = (Session["Prefill_Street"] as string) ?? "";
                    txtLandmark.Text = (Session["Prefill_Landmark"] as string) ?? "";
                }
                
            }
        }


        protected void btnCreate_Click(object sender, EventArgs e)
        {
            // 0) Run client/server validators in the "inq" group (matches your ASPX)
            Page.Validate("inq");
            if (!Page.IsValid)
            {
                ShowSweetAlert("Invalid Input", "Please correct the highlighted fields.", "warning");
                return;
            }

            // 1) Gather inputs
            string lastName = txtLastName.Text.Trim();
            string firstName = txtFirstName.Text.Trim();
            string middleName = string.IsNullOrWhiteSpace(txtMiddleName.Text) ? null : txtMiddleName.Text.Trim();
            string email = (txtEmail.Text ?? "").Trim().ToLowerInvariant();
            string contact = txtContact.Text.Trim();
            string street = txtStreet.Text.Trim();
            string brgy = txtBarangay.Text.Trim();
            string city = ddlCity.SelectedValue;
            string region = ddlRegion.SelectedValue;
            string country = txtCountry.Text.Trim();
            string landmark = string.IsNullOrWhiteSpace(txtLandmark.Text) ? null : txtLandmark.Text.Trim();

            // 2) Required fields (defensive check even after Page.IsValid)
            if (string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(contact) ||
                string.IsNullOrWhiteSpace(street) || string.IsNullOrWhiteSpace(brgy) ||
                string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(region) ||
                string.IsNullOrWhiteSpace(country))
            {
                ShowSweetAlert("Error", "Please fill in all required fields.", "error");
                return;
            }

            // 3) Basic email shape (server-side)
            if (!IsValidEmail(email))
            {
                ShowSweetAlert("Invalid Email", "Please enter a valid email address.", "warning");
                return;
            }

            // 4) Must match your allow-list (same as ASPX ValidationExpression)
            if (!IsAllowedEmailDomain(email, out _))
            {
                ShowSweetAlert(
                    "Invalid Email",
                    "Only Gmail, Yahoo, Outlook/Hotmail/Live/MSN, iCloud/Me/Mac, Proton, Zoho, or school/government (*.edu.ph / *.gov.ph) emails are allowed.",
                    "warning"
                );
                return;
            }

            // 5) Contact number: enforce 11 digits (matches your front-end JS)
            if (!Regex.IsMatch(contact, @"^\d{11}$"))
            {
                ShowSweetAlert("Invalid Contact Number", "Contact Number must be exactly 11 digits.", "warning");
                return;
            }

            // 6) Duplicate email check
            if (EmailExists(email))
            {
                ShowSweetAlert("Error", "This email is already registered.", "warning");
                return;
            }

            // 7) Create reset token
            string token = Guid.NewGuid().ToString();
            DateTime expiry = DateTime.Now.AddHours(1);

            // 8) Insert client + reset and get ClientNumber
            var (clientId, clientNumber) = CreateClientWithReset(
                lastName, firstName, middleName, email, contact,
                street, brgy, city, region, country, landmark,
                token, expiry
            );

            if (clientId <= 0)
            {
                ShowSweetAlert("Error", "Failed to add client.", "error");
                return;
            }

            // 9) Save findings/history if available
            var findings = (Session["Prefill_Findings"] as string) ?? "";
            var inspectionIdTxt = (Session["Prefill_InspectionID"] as string) ?? "";
            var inquiryCode = (Session["Prefill_InquiryCode"] as string) ?? "";
            int inspectionIdForSave = 0;
            int.TryParse(inspectionIdTxt, out inspectionIdForSave);

            int userId = Convert.ToInt32(Session["UserID"]);

            if (inspectionIdForSave > 0)
            {
                try
                {
                    SaveClientFinding(clientId, inspectionIdForSave, inquiryCode, findings, userId);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("SaveClientFinding failed: " + ex);
                    // Non-fatal: continue
                }
            }

            // 10) Clear prefill sessions
            Session.Remove("Prefill_Findings");
            Session.Remove("Prefill_InspectionID");
            Session.Remove("Prefill_InquiryCode");

            // 11) Send reset email
            bool sent = SendResetEmail(email, token);

            // 12) Show success message with ClientNumber
            ShowSweetAlert(
                sent ? "Success" : "Partial Success",
                sent
                    ? $"Client added successfully!<br/><strong>Client Number: {clientNumber}</strong><br/>Email sent for password setup."
                    : $"Client added successfully!<br/><strong>Client Number: {clientNumber}</strong><br/>However, failed to send email.",
                sent ? "success" : "warning"
            );

            // 13) Clear form
            ClearForm();
        }


        /* ===================== EMAIL / VALIDATION HELPERS ===================== */

        private static bool IsValidEmail(string email)
            => Regex.IsMatch(email ?? "", @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        // exact-domain allow-list (matches your ASPX regex)
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

        // suffix allow-list for PH schools & government
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

        private bool SendResetEmail(string toEmail, string token)
        {
            try
            {
                // ✅ Fetch credentials from Web.config or App.config
                string fromEmail = ConfigurationManager.AppSettings["emailFrom"] ?? "rrctermiteandpestcontrol@gmail.com";
                string appPassword = ConfigurationManager.AppSettings["emailPassword"] ?? "";

                // ✅ Generate the reset link
                string resetLink = $"https://rrcmngmnt.com/ResetPassword.aspx?type=client&token={HttpUtility.UrlEncode(token)}";
                string subject = "Set Your Password - RRC Management System";

                // ✅ Simplified and clean HTML email template with full inline styles
                string body = $@"
<!DOCTYPE html>
<html>
<head>
<meta http-equiv='Content-Type' content='text/html; charset=UTF-8' />
<meta name='viewport' content='width=device-width, initial-scale=1.0' />
<title>Set Your Password - RRC Management System</title>
</head>
<body style='margin:0; padding:0; background-color:#f4f7fa; font-family: Arial, sans-serif;'>

<table width='100%' cellpadding='0' cellspacing='0' border='0'>
    <tr>
        <td align='center' style='padding:20px;'>
            <table width='600' cellpadding='0' cellspacing='0' border='0' style='max-width:600px; width:100%; background:#ffffff; border-radius:8px; box-shadow:0 4px 12px rgba(0,0,0,0.05);'>
                
                <!-- HEADER -->
                <tr>
                    <td align='center' style='padding:20px; background-color:#1a202c; border-top-left-radius:8px; border-top-right-radius:8px;'>
                        <h1 style='color:#ffffff; font-size:24px; margin:0;'>RRC Management System</h1>
                    </td>
                </tr>

                <!-- BODY -->
                <tr>
                    <td style='padding:30px;'>
                        <h2 style='color:#2d3748; font-size:22px; margin:0 0 15px;'>Welcome!</h2>
                        <p style='color:#4a5568; font-size:16px; line-height:1.6; margin:0 0 15px;'>Hello,</p>
                        <p style='color:#4a5568; font-size:16px; line-height:1.6; margin:0 0 15px;'>
                            You have been registered as a <strong>Client</strong>. 
                            To get started, you'll need to set your password.
                        </p>
                        <p style='color:#4a5568; font-size:16px; line-height:1.6; margin:0 0 25px;'>
                            Please click the button below to continue:
                        </p>

                        <!-- BUTTON -->
                        <p style='text-align:center; margin:0 0 30px;'>
                            <a href='{resetLink}' 
                               style='background-color:#2b6cb0; color:#ffffff; font-size:16px; font-weight:bold; padding:12px 24px; 
                                      border-radius:6px; display:inline-block; text-align:center; text-decoration:none;'>
                               Set Password
                            </a>
                        </p>

                        <!-- PLAIN URL -->
                        <p style='color:#4a5568; font-size:14px; line-height:1.6; margin:20px 0 10px; text-align:center;'>
                            If the button does not work, copy and paste this URL into your browser:
                        </p>
                        <p style='text-align:center; font-size:14px; word-break:break-word; margin:0;'>
                            <a href='{resetLink}' style='color:#2b6cb0; text-decoration:underline;'>{resetLink}</a>
                        </p>
                    </td>
                </tr>

                <!-- FOOTER -->
                <tr>
                    <td align='center' style='padding:15px; background:#f4f7fa; border-top:1px solid #e2e8f0;'>
                        <p style='font-size:12px; color:#718096; margin:0;'>
                            This link will expire in 1 hour. If you did not request this, you can safely ignore this email.
                            <br/><br/>
                            &copy; {DateTime.Now.Year} RRC Management System. All rights reserved.
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

                // ✅ Send the email
                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(fromEmail, "RRC Management System");
                    mail.To.Add(toEmail);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;

                    using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new NetworkCredential(fromEmail, appPassword);
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log for debugging
                System.Diagnostics.Debug.WriteLine("Email sending error: " + ex.Message);
                return false;
            }
        }




        private void ShowSweetAlert(string title, string message, string icon)
        {
            // Requires a <asp:Literal ID="ltScript" runat="server"></asp:Literal> in the .aspx
            ltScript.Text = $@"
<script>
Swal.fire({{
  title: '{title}',
  text: '{message}',
  icon: '{icon}',
  confirmButtonText: 'OK'
}});
</script>";
        }

        /* ===================== DB HELPERS ===================== */

        private bool HasPermission(int userId, string module, string permissionColumn)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spAdminPermission_Check", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@ModuleName", SqlDbType.NVarChar, 100).Value = module;
                cmd.Parameters.Add("@Permission", SqlDbType.NVarChar, 10).Value =
                    (permissionColumn == "CanView" || permissionColumn == "CanAdd" ||
                     permissionColumn == "CanEdit" || permissionColumn == "CanDelete")
                    ? permissionColumn : "CanView";

                con.Open();
                object val = cmd.ExecuteScalar();
                return val != null && Convert.ToBoolean(val);
            }
        }

        private bool EmailExists(string email)
        {
            string emailHash = AESHelper.ComputeSHA256WithPepper(email);

            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spClient_EmailExists", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@EmailHash", SqlDbType.Char, 64).Value = emailHash;

                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
            }
        }



        private (int clientId, string clientNumber) CreateClientWithReset(
     string lastName, string firstName, string middleName, string email, string contact,
     string street, string brgy, string city, string region, string country, string landmark,
     string token, DateTime expiry)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spClient_CreateWithReset", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // Plaintext names remain as-is
                cmd.Parameters.Add("@LastName", SqlDbType.NVarChar, 100).Value = lastName;
                cmd.Parameters.Add("@FirstName", SqlDbType.NVarChar, 100).Value = firstName;
                cmd.Parameters.Add("@MiddleName", SqlDbType.NVarChar, 100).Value =
                    string.IsNullOrWhiteSpace(middleName) ? (object)DBNull.Value : middleName;

                // Encrypt sensitive fields
                cmd.Parameters.Add("@EmailEnc", SqlDbType.NVarChar).Value = AESHelper.EncryptEmail(email);
                cmd.Parameters.Add("@ContactEnc", SqlDbType.NVarChar).Value = AESHelper.EncryptField(contact);
                cmd.Parameters.Add("@StreetEnc", SqlDbType.NVarChar).Value = AESHelper.EncryptField(street);
                cmd.Parameters.Add("@BarangayEnc", SqlDbType.NVarChar).Value = AESHelper.EncryptField(brgy);
                cmd.Parameters.Add("@CityEnc", SqlDbType.NVarChar).Value = AESHelper.EncryptField(city);
                cmd.Parameters.Add("@RegionEnc", SqlDbType.NVarChar).Value = AESHelper.EncryptField(region);
                cmd.Parameters.Add("@CountryEnc", SqlDbType.NVarChar).Value = AESHelper.EncryptField(country);
                cmd.Parameters.Add("@LandmarkEnc", SqlDbType.NVarChar).Value =
                    string.IsNullOrWhiteSpace(landmark) ? (object)DBNull.Value : AESHelper.EncryptField(landmark);

                // Compute SHA-256 email hash
                string emailHash = AESHelper.ComputeSHA256WithPepper(email);
                cmd.Parameters.Add("@EmailHash", SqlDbType.Char, 64).Value = emailHash;

                // Reset token
                cmd.Parameters.Add("@ResetToken", SqlDbType.NVarChar, 200).Value = token;
                cmd.Parameters.Add("@ResetTokenExpiry", SqlDbType.DateTime).Value = expiry;

                con.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int clientId = reader.GetInt32(reader.GetOrdinal("ClientID"));
                        string clientNumber = reader.GetString(reader.GetOrdinal("ClientNumber"));
                        return (clientId, clientNumber);
                    }
                }

                return (0, null);
            }
        }


        /// <summary>
        /// Inserts a history entry capturing the inspection findings for the newly created client.
        /// Requires: dbo.spClientHistory_AddFinding(@ClientID,@InspectionID,@InquiryCode,@Title,@Description,@CreatedBy)
        /// </summary>
        private void SaveClientFinding(int clientId, int inspectionId, string inquiryCode, string findings, int createdByUserId)
        {
            try
            {
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spClientHistory_AddFinding", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                    cmd.Parameters.Add("@InspectionID", SqlDbType.Int).Value = inspectionId;
                    cmd.Parameters.Add("@InquiryCode", SqlDbType.NVarChar, 50).Value = (object)inquiryCode ?? DBNull.Value;
                    cmd.Parameters.Add("@Title", SqlDbType.NVarChar, 200).Value = "Inspection Findings";
                    cmd.Parameters.Add("@Description", SqlDbType.NVarChar, -1).Value = (object)findings ?? DBNull.Value;
                    cmd.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = createdByUserId;

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                throw; // bubble up to caller
            }
        }

        /* ===================== UI HELPERS ===================== */

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
                ddlCity.Items.Add(new ListItem("Malabon", "Malabon"));
                ddlCity.Items.Add(new ListItem("Mandaluyong", "Mandaluyong"));
                ddlCity.Items.Add(new ListItem("Marikina", "Marikina"));
                ddlCity.Items.Add(new ListItem("Muntinlupa", "Muntinlupa"));
                ddlCity.Items.Add(new ListItem("Navotas", "Navotas"));
                ddlCity.Items.Add(new ListItem("San Juan", "San Juan"));
                ddlCity.Items.Add(new ListItem("Pateros", "Pateros"));
                ddlCity.Items.Add(new ListItem("Parañaque", "Parañaque"));
            }
            else if (selectedRegion == "Region I")
            {
                ddlCity.Items.Add(new ListItem("Alaminos", "Alaminos"));
                ddlCity.Items.Add(new ListItem("Batac", "Batac"));
                ddlCity.Items.Add(new ListItem("Candon", "Candon"));
                ddlCity.Items.Add(new ListItem("Laoag", "Laoag"));
                ddlCity.Items.Add(new ListItem("Vigan", "Vigan"));
                ddlCity.Items.Add(new ListItem("San Fernando", "San Fernando"));
                ddlCity.Items.Add(new ListItem("San Carlos", "San Carlos"));
                ddlCity.Items.Add(new ListItem("Dagupan", "Dagupan"));
                ddlCity.Items.Add(new ListItem("Urdaneta", "Urdaneta"));
            }
            else if (selectedRegion == "Region II")
            {
                ddlCity.Items.Add(new ListItem("Cauayan", "Cauayan"));
                ddlCity.Items.Add(new ListItem("Tuguegarao", "Tuguegarao"));
                ddlCity.Items.Add(new ListItem("Ilagan", "Ilagan"));
                ddlCity.Items.Add(new ListItem("Santiago", "Santiago"));
            }
            else if (selectedRegion == "Region III")
            {
                ddlCity.Items.Add(new ListItem("San Fernando", "San Fernando"));
                ddlCity.Items.Add(new ListItem("Angeles", "Angeles"));
                ddlCity.Items.Add(new ListItem("Olongapo", "Olongapo"));
                ddlCity.Items.Add(new ListItem("Balanga", "Balanga"));
                ddlCity.Items.Add(new ListItem("Baliwag", "Baliwag"));
                ddlCity.Items.Add(new ListItem("Cabanatuan", "Cabanatuan"));
                ddlCity.Items.Add(new ListItem("Gapan", "Gapan"));
                ddlCity.Items.Add(new ListItem("Mabalacat", "Mabalacat"));
                ddlCity.Items.Add(new ListItem("Malolos", "Malolos"));
                ddlCity.Items.Add(new ListItem("Meycauayan", "Meycauayan"));
                ddlCity.Items.Add(new ListItem("Muñoz", "Muñoz"));
                ddlCity.Items.Add(new ListItem("Palayan", "Palayan"));
                ddlCity.Items.Add(new ListItem("San Jose", "San Jose"));
                ddlCity.Items.Add(new ListItem("San Jose del Monte", "San Jose del Monte"));
                ddlCity.Items.Add(new ListItem("Tarlac City", "Tarlac City"));
            }
            else if (selectedRegion == "Region IV-A")
            {
                ddlCity.Items.Add(new ListItem("Cavite", "Cavite"));
                ddlCity.Items.Add(new ListItem("Batangas", "Batangas"));
                ddlCity.Items.Add(new ListItem("Lucena", "Lucena"));
                ddlCity.Items.Add(new ListItem("Antipolo", "Antipolo"));
                ddlCity.Items.Add(new ListItem("Bacoor", "Bacoor"));
                ddlCity.Items.Add(new ListItem("Biñan", "Biñan"));
                ddlCity.Items.Add(new ListItem("Cabuyao", "Cabuyao"));
                ddlCity.Items.Add(new ListItem("Calaca", "Calaca"));
                ddlCity.Items.Add(new ListItem("Calamba", "Calamba"));
                ddlCity.Items.Add(new ListItem("Carmona", "Carmona"));
                ddlCity.Items.Add(new ListItem("Dasmariñas", "Dasmariñas"));
                ddlCity.Items.Add(new ListItem("General Trias", "General Trias"));
                ddlCity.Items.Add(new ListItem("Imus", "Imus"));
                ddlCity.Items.Add(new ListItem("Lipa", "Lipa"));
                ddlCity.Items.Add(new ListItem("San Pablo", "San Pablo"));
                ddlCity.Items.Add(new ListItem("San Pedro", "San Pedro"));
                ddlCity.Items.Add(new ListItem("Santa Rosa", "Santa Rosa"));
                ddlCity.Items.Add(new ListItem("Santo Tomas", "Santo Tomas"));
                ddlCity.Items.Add(new ListItem("Tagaytay", "Tagaytay"));
                ddlCity.Items.Add(new ListItem("Tanauan", "Tanauan"));
                ddlCity.Items.Add(new ListItem("Tayabas", "Tayabas"));
                ddlCity.Items.Add(new ListItem("Trece Martires", "Trece Martires"));
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
                ddlCity.Items.Add(new ListItem("Iriga", "Iriga"));
                ddlCity.Items.Add(new ListItem("Ligao", "Ligao"));
                ddlCity.Items.Add(new ListItem("Masbate City", "Masbate City"));
                ddlCity.Items.Add(new ListItem("Sorsogon City", "Sorsogon City"));
                ddlCity.Items.Add(new ListItem("Tabaco", "Tabaco"));
            }
            else if (selectedRegion == "Region VI")
            {
                ddlCity.Items.Add(new ListItem("Iloilo City", "Iloilo City"));
                ddlCity.Items.Add(new ListItem("Passi", "Passi"));
                ddlCity.Items.Add(new ListItem("Bacolod", "Bacolod"));
                ddlCity.Items.Add(new ListItem("Roxas City", "Roxas City"));
            }
            else if (selectedRegion == "Region VII")
            {
                ddlCity.Items.Add(new ListItem("Cebu City", "Cebu City"));
                ddlCity.Items.Add(new ListItem("Dumaguete", "Dumaguete"));
                ddlCity.Items.Add(new ListItem("Lapu-Lapu City", "Lapu-Lapu City"));
                ddlCity.Items.Add(new ListItem("Mandaue", "Mandaue"));
                ddlCity.Items.Add(new ListItem("Bogo", "Bogo"));
                ddlCity.Items.Add(new ListItem("Carcar", "Carcar"));
                ddlCity.Items.Add(new ListItem("Danao", "Danao"));
                ddlCity.Items.Add(new ListItem("Naga", "Naga"));
                ddlCity.Items.Add(new ListItem("Tagbilaran", "Tagbilaran"));
                ddlCity.Items.Add(new ListItem("Talisay", "Talisay"));
                ddlCity.Items.Add(new ListItem("Toledo", "Toledo"));
            }
            else
            {
                ddlCity.Items.Add(new ListItem("No Cities Available", ""));
            }
        }


        private void ClearForm()
        {
            txtLastName.Text = "";
            txtFirstName.Text = "";
            txtMiddleName.Text = "";
            txtEmail.Text = "";
            txtContact.Text = "";
            txtStreet.Text = "";
            txtBarangay.Text = "";
            ddlCity.SelectedIndex = 0;
            ddlRegion.SelectedIndex = 0;
            txtCountry.Text = "Philippines";
            txtLandmark.Text = "";
        }
    }
}
