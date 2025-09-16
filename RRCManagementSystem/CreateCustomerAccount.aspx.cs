using RRCManagementSystem.Helpers;
using System;
using System.Collections.Generic;      // HashSet
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;   // email shape check
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

            // 8) Insert client + reset
            int clientId = CreateClientWithReset(
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

            ShowSweetAlert(
                sent ? "Success" : "Partial Success",
                sent
                    ? "Client added successfully. Email sent for password setup."
                    : "Client added but failed to send email.",
                sent ? "success" : "warning"
            );

            // 12) Clear form
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
                string fromEmail = ConfigurationManager.AppSettings["emailFrom"] ?? "rrctermiteandpestcontrol@gmail.com";
                string appPassword = ConfigurationManager.AppSettings["emailPassword"] ?? "";
                string resetLink = $"https://localhost:44341/ResetPassword.aspx?type=client&token={token}";
                string subject = "Set Your Password - RRC Management System";

                string body = $@"<!DOCTYPE html><html><head><meta charset='UTF-8'>
<style>body{{background:#f9f9f9;font-family:Arial}}.container{{max-width:600px;margin:30px auto;background:#fff;border-radius:8px;box-shadow:0 2px 8px rgba(0,0,0,.05);padding:20px 30px}}
h3{{color:#2a4fa7}}.button{{display:inline-block;padding:12px 20px;background:#add8e6;color:#000;text-decoration:none;border-radius:5px;font-weight:bold}}</style>
</head><body><div class='container'>
<h3>Welcome to RRC Management System</h3>
<p>You have been registered as a <strong>Client</strong>.</p>
<p>Click the button below to set your password (valid for 1 hour):</p>
<p><a href='{resetLink}' class='button'>Set Password</a></p>
</div></body></html>";

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

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
                        smtp.EnableSsl = true; // STARTTLS
                        smtp.Send(mail);
                    }
                }
                return true;
            }
            catch
            {
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



        private int CreateClientWithReset(
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
                object id = cmd.ExecuteScalar();
                return (id != null && int.TryParse(id.ToString(), out int clientId)) ? clientId : 0;
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
