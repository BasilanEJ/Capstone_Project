using RRCManagementSystem.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class ClientSignup : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadRegions();
                txtCountry.Text = "Philippines";
            }
        }

        #region Region/City Dropdowns

        /// <summary>
        /// Loads all Philippine regions into the dropdown
        /// </summary>
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

            // Initialize city dropdown
            ddlCity.Items.Clear();
            ddlCity.Items.Add(new ListItem("-- Select City --", ""));
        }

        /// <summary>
        /// Event handler when region selection changes - loads corresponding cities
        /// </summary>
        protected void ddlRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadCities(ddlRegion.SelectedValue);
        }

        /// <summary>
        /// Loads cities based on selected region
        /// </summary>
        private void LoadCities(string selectedRegion)
        {
            ddlCity.Items.Clear();
            ddlCity.Items.Add(new ListItem("-- Select City --", ""));

            if (string.IsNullOrEmpty(selectedRegion))
                return;

            // Dictionary of regions and their cities
            var regionCities = new Dictionary<string, List<string>>
            {
                {
                    "NCR", new List<string>
                    {
                        "Quezon City", "Manila", "Makati", "Caloocan", "Las Piñas", "Pasig",
                        "Taguig", "Valenzuela", "Pasay", "Malabon", "Mandaluyong", "Marikina",
                        "Muntinlupa", "Navotas", "San Juan", "Pateros", "Parañaque"
                    }
                },
                {
                    "Region I", new List<string>
                    {
                        "Alaminos", "Batac", "Candon", "Laoag", "Vigan", "San Fernando",
                        "San Carlos", "Dagupan", "Urdaneta"
                    }
                },
                {
                    "Region II", new List<string>
                    {
                        "Cauayan", "Tuguegarao", "Ilagan", "Santiago"
                    }
                },
                {
                    "Region III", new List<string>
                    {
                        "San Fernando", "Angeles", "Olongapo", "Balanga", "Baliwag", "Cabanatuan",
                        "Gapan", "Mabalacat", "Malolos", "Meycauayan", "Muñoz", "Palayan",
                        "San Jose", "San Jose del Monte", "Tarlac City"
                    }
                },
                {
                    "Region IV-A", new List<string>
                    {
                        "Cavite", "Batangas", "Lucena", "Antipolo", "Bacoor", "Biñan", "Cabuyao",
                        "Calaca", "Calamba", "Carmona", "Dasmariñas", "General Trias", "Imus",
                        "Lipa", "San Pablo", "San Pedro", "Santa Rosa", "Santo Tomas", "Tagaytay",
                        "Tanauan", "Tayabas", "Trece Martires"
                    }
                },
                {
                    "Region IV-B", new List<string>
                    {
                        "Puerto Princesa", "Calapan"
                    }
                },
                {
                    "Region V", new List<string>
                    {
                        "Legazpi", "Naga", "Iriga", "Ligao", "Masbate City", "Sorsogon City", "Tabaco"
                    }
                },
                {
                    "Region VI", new List<string>
                    {
                        "Iloilo City", "Passi", "Bacolod", "Roxas City"
                    }
                },
                {
                    "Region VII", new List<string>
                    {
                        "Cebu City", "Dumaguete", "Lapu-Lapu City", "Mandaue", "Bogo", "Carcar",
                        "Danao", "Naga", "Tagbilaran", "Talisay", "Toledo"
                    }
                }
            };

            if (regionCities.ContainsKey(selectedRegion))
            {
                foreach (var city in regionCities[selectedRegion])
                {
                    ddlCity.Items.Add(new ListItem(city, city));
                }
            }
            else
            {
                ddlCity.Items.Add(new ListItem("No Cities Available", ""));
            }
        }

        #endregion

        #region Sign Up Button Click Event

        /// <summary>
        /// Main signup button click handler
        /// </summary>
        protected void btnSignup_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Gather and trim all inputs
                string lastName = txtLastName.Text.Trim();
                string firstName = txtFirstName.Text.Trim();
                string middleName = string.IsNullOrWhiteSpace(txtMiddleName.Text) ? null : txtMiddleName.Text.Trim();
                string email = txtEmail.Text.Trim().ToLowerInvariant();
                string contact = txtContact.Text.Trim();
                string street = txtStreet.Text.Trim();
                string barangay = txtBarangay.Text.Trim();
                string city = ddlCity.SelectedValue;
                string region = ddlRegion.SelectedValue;
                string country = txtCountry.Text.Trim();
                string landmark = string.IsNullOrWhiteSpace(txtLandmark.Text) ? null : txtLandmark.Text.Trim();
                string password = txtPassword.Text;
                string confirmPassword = txtConfirmPassword.Text;

                // 2. Server-side validation
                if (!ValidateInputs(lastName, firstName, email, contact, street, barangay, city, region, country, password, confirmPassword))
                {
                    return;
                }

                // 3. Check if email already exists
                if (EmailExists(email))
                {
                    ShowSweetAlert("Email Already Registered", "This email is already associated with an account. Please use a different email or try logging in.", "warning");
                    return;
                }

                // ✅ 4. Hash the password using Argon2id (no separate salt needed)
                string passwordHash = PasswordHelper.HashPassword(password);

                // ✅ 5. Create client account (pass empty string for salt since Argon2 includes it)
                var (clientId, clientNumber) = CreateClientAccount(
                    lastName, firstName, middleName, email, contact,
                    street, barangay, city, region, country, landmark,
                    passwordHash, "" // PasswordSalt is empty for Argon2
                );

                if (clientId <= 0)
                {
                    ShowSweetAlert("Registration Failed", "Unable to create your account. Please try again later.", "error");
                    return;
                }

                // 6. Send welcome email
                bool emailSent = SendWelcomeEmail(email, firstName, clientNumber);

                // 7. Show success message
                string successMessage = emailSent
                    ? $"Account created successfully!<br/><strong>Client Number: {clientNumber}</strong><br/>A welcome email has been sent to your inbox."
                    : $"Account created successfully!<br/><strong>Client Number: {clientNumber}</strong><br/>However, we couldn't send the confirmation email.";

                ShowSweetAlertWithRedirect(
                    "Welcome to RRC!",
                    successMessage,
                    "success",
                    "Login.aspx"
                );

                // 8. Clear form
                ClearForm();
            }
            catch (Exception ex)
            {
                // Log the error (in production, use proper logging)
                System.Diagnostics.Debug.WriteLine($"Signup Error: {ex.Message}");
                ShowSweetAlert("Registration Error", "An unexpected error occurred. Please try again.", "error");
            }
        }

        #endregion

        #region Validation Methods

        /// <summary>
        /// Validates all user inputs
        /// </summary>
        private bool ValidateInputs(string lastName, string firstName, string email, string contact,
            string street, string barangay, string city, string region, string country,
            string password, string confirmPassword)
        {
            // Check required fields
            if (string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(contact) ||
                string.IsNullOrWhiteSpace(street) || string.IsNullOrWhiteSpace(barangay) ||
                string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(region) ||
                string.IsNullOrWhiteSpace(country) || string.IsNullOrWhiteSpace(password))
            {
                ShowSweetAlert("Missing Information", "Please fill in all required fields.", "warning");
                return false;
            }

            // Validate name fields (no numbers)
            if (ContainsDigits(lastName) || ContainsDigits(firstName))
            {
                ShowSweetAlert("Invalid Name", "Names cannot contain numbers.", "warning");
                return false;
            }

            // Validate email format
            if (!IsValidEmail(email))
            {
                ShowSweetAlert("Invalid Email", "Please enter a valid email address.", "warning");
                return false;
            }

            // Validate email domain
            if (!IsAllowedEmailDomain(email))
            {
                ShowSweetAlert("Invalid Email Domain", "Only Gmail, Yahoo, Outlook, iCloud, Proton, Zoho, or school/government (.edu.ph / .gov.ph) emails are allowed.", "warning");
                return false;
            }

            // Validate contact number
            if (!Regex.IsMatch(contact, @"^09\d{9}$"))
            {
                ShowSweetAlert("Invalid Contact Number", "Contact number must be exactly 11 digits starting with 09.", "warning");
                return false;
            }

            // Validate password
            if (password.Length < 8)
            {
                ShowSweetAlert("Weak Password", "Password must be at least 8 characters long.", "warning");
                return false;
            }

            // Validate password match
            if (password != confirmPassword)
            {
                ShowSweetAlert("Password Mismatch", "Passwords do not match.", "warning");
                return false;
            }

            // Validate terms acceptance
            if (!chkTerms.Checked)
            {
                ShowSweetAlert("Terms Required", "Please accept the Terms & Conditions to continue.", "warning");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if string contains any digits
        /// </summary>
        private bool ContainsDigits(string input)
        {
            return !string.IsNullOrEmpty(input) && Regex.IsMatch(input, @"\d");
        }

        /// <summary>
        /// Validates email format
        /// </summary>
        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email ?? "", @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        /// <summary>
        /// Checks if email domain is in the allowed list
        /// </summary>
        private bool IsAllowedEmailDomain(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            int atIndex = email.IndexOf('@');
            if (atIndex < 0 || atIndex == email.Length - 1)
                return false;

            string domain = email.Substring(atIndex + 1).ToLowerInvariant();

            // Allowed exact domains
            var allowedDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "gmail.com", "yahoo.com", "ymail.com", "rocketmail.com",
                "outlook.com", "hotmail.com", "live.com", "msn.com",
                "icloud.com", "me.com", "mac.com",
                "protonmail.com", "proton.me",
                "zoho.com", "zohomail.com"
            };

            if (allowedDomains.Contains(domain))
                return true;

            // Check for school/government domains
            if (domain.EndsWith(".edu.ph", StringComparison.OrdinalIgnoreCase) ||
                domain.EndsWith(".gov.ph", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        #endregion

        #region Database Operations

        /// <summary>
        /// Checks if email already exists in database
        /// </summary>
        private bool EmailExists(string email)
        {
            string emailHash = AESHelper.ComputeSHA256WithPepper(email);

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spClient_EmailExists", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@EmailHash", SqlDbType.Char, 64).Value = emailHash;

                conn.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        /// <summary>
        /// Creates a new client account in the database
        /// </summary>
        /// <summary>
        /// Creates a new client account in the database
        /// </summary>
        private (int clientId, string clientNumber) CreateClientAccount(
            string lastName, string firstName, string middleName, string email, string contact,
            string street, string barangay, string city, string region, string country, string landmark,
            string passwordHash, string passwordSalt)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spClient_Register", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // Plaintext names
                cmd.Parameters.Add("@LastName", SqlDbType.NVarChar, 100).Value = lastName;
                cmd.Parameters.Add("@FirstName", SqlDbType.NVarChar, 100).Value = firstName;
                cmd.Parameters.Add("@MiddleName", SqlDbType.NVarChar, 100).Value =
                    string.IsNullOrWhiteSpace(middleName) ? (object)DBNull.Value : middleName;

                // Encrypt sensitive fields
                cmd.Parameters.Add("@EmailEnc", SqlDbType.NVarChar).Value = AESHelper.EncryptEmail(email);
                cmd.Parameters.Add("@ContactEnc", SqlDbType.NVarChar).Value = AESHelper.EncryptField(contact);
                cmd.Parameters.Add("@StreetEnc", SqlDbType.NVarChar).Value = AESHelper.EncryptField(street);
                cmd.Parameters.Add("@BarangayEnc", SqlDbType.NVarChar).Value = AESHelper.EncryptField(barangay);
                cmd.Parameters.Add("@CityEnc", SqlDbType.NVarChar).Value = AESHelper.EncryptField(city);
                cmd.Parameters.Add("@RegionEnc", SqlDbType.NVarChar).Value = AESHelper.EncryptField(region);
                cmd.Parameters.Add("@CountryEnc", SqlDbType.NVarChar).Value = AESHelper.EncryptField(country);
                cmd.Parameters.Add("@LandmarkEnc", SqlDbType.NVarChar).Value =
                    string.IsNullOrWhiteSpace(landmark) ? (object)DBNull.Value : AESHelper.EncryptField(landmark);

                // Email hash for searching
                string emailHash = AESHelper.ComputeSHA256WithPepper(email);
                cmd.Parameters.Add("@EmailHash", SqlDbType.Char, 64).Value = emailHash;

                // ✅ Argon2id hashed password (no separate salt needed)
                cmd.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 256).Value = passwordHash;

                // ✅ PasswordSalt is empty string since Argon2 includes salt in the hash
                cmd.Parameters.Add("@PasswordSalt", SqlDbType.NVarChar, 256).Value = "";

                // Output parameters
                var pClientId = new SqlParameter("@ClientID", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(pClientId);

                var pClientNumber = new SqlParameter("@ClientNumber", SqlDbType.NVarChar, 20)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(pClientNumber);

                conn.Open();
                cmd.ExecuteNonQuery();

                int clientId = pClientId.Value != DBNull.Value ? Convert.ToInt32(pClientId.Value) : 0;
                string clientNumber = pClientNumber.Value != DBNull.Value ? pClientNumber.Value.ToString() : "";

                return (clientId, clientNumber);
            }
        }

        #endregion

        #region Email Notification

        /// <summary>
        /// Sends a welcome email to the newly registered client
        /// </summary>
        private bool SendWelcomeEmail(string toEmail, string firstName, string clientNumber)
        {
            try
            {
                string fromEmail = ConfigurationManager.AppSettings["emailFrom"] ?? "rrctermiteandpestcontrol@gmail.com";
                string appPassword = ConfigurationManager.AppSettings["emailPassword"] ?? "";

                string subject = "Welcome to RRC Termite & Pest Control!";

                string body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Welcome to RRC</title>
</head>
<body style='margin:0; padding:0; background-color:#f4f7fa; font-family: Arial, sans-serif;'>
    <table width='100%' cellpadding='0' cellspacing='0' border='0'>
        <tr>
            <td align='center' style='padding:20px;'>
                <table width='600' cellpadding='0' cellspacing='0' border='0' style='max-width:600px; width:100%; background:#ffffff; border-radius:8px; box-shadow:0 4px 12px rgba(0,0,0,0.05);'>
                    
                    <!-- Header -->
                    <tr>
                        <td align='center' style='padding:20px; background-color:#007bff; border-top-left-radius:8px; border-top-right-radius:8px;'>
                            <h1 style='color:#ffffff; font-size:24px; margin:0;'>Welcome to RRC!</h1>
                        </td>
                    </tr>

                    <!-- Body -->
                    <tr>
                        <td style='padding:30px;'>
                            <h2 style='color:#2d3748; font-size:22px; margin:0 0 15px;'>Hello {HttpUtility.HtmlEncode(firstName)}!</h2>
                            <p style='color:#4a5568; font-size:16px; line-height:1.6; margin:0 0 15px;'>
                                Thank you for creating an account with RRC Termite & Pest Control. We're excited to have you join our community!
                            </p>
                            <p style='color:#4a5568; font-size:16px; line-height:1.6; margin:0 0 15px;'>
                                Your account has been successfully created with the following details:
                            </p>
                            
                            <!-- Client Info Box -->
                            <table width='100%' cellpadding='15' cellspacing='0' border='0' style='background:#f8f9fa; border-radius:8px; margin:20px 0;'>
                                <tr>
                                    <td>
                                        <p style='margin:0; color:#2d3748; font-size:14px;'><strong>Client Number:</strong></p>
                                        <p style='margin:5px 0 0 0; color:#007bff; font-size:20px; font-weight:bold;'>{HttpUtility.HtmlEncode(clientNumber)}</p>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <p style='margin:15px 0 0 0; color:#2d3748; font-size:14px;'><strong>Email:</strong></p>
                                        <p style='margin:5px 0 0 0; color:#4a5568; font-size:16px;'>{HttpUtility.HtmlEncode(toEmail)}</p>
                                    </td>
                                </tr>
                            </table>

                            <p style='color:#4a5568; font-size:16px; line-height:1.6; margin:20px 0 15px;'>
                                You can now log in to your account and submit inquiries for our pest control services.
                            </p>

                            <!-- Login Button -->
                            <p style='text-align:center; margin:30px 0;'>
                                <a href='https://rrcmngmnt.com/Login.aspx' 
                                   style='background-color:#007bff; color:#ffffff; font-size:16px; font-weight:bold; padding:12px 24px; 
                                          border-radius:6px; display:inline-block; text-align:center; text-decoration:none;'>
                                   Login to Your Account
                                </a>
                            </p>

                            <p style='color:#4a5568; font-size:14px; line-height:1.6; margin:20px 0 0;'>
                                If you have any questions or need assistance, feel free to contact us at 
                                <a href='mailto:rrctermiteandpestcontrol@gmail.com' style='color:#007bff; text-decoration:none;'>rrctermiteandpestcontrol@gmail.com</a>
                            </p>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td align='center' style='padding:15px; background:#f4f7fa; border-top:1px solid #e2e8f0;'>
                            <p style='font-size:12px; color:#718096; margin:0;'>
                                This is an automated message. Please do not reply to this email.
                                <br/><br/>
                                &copy; {DateTime.Now.Year} RRC Termite & Pest Control. All rights reserved.
                                <br/>
                                #33 Kaligatasan Street, Brgy. Holy Spirit, Quezon City, Philippines
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
                System.Diagnostics.Debug.WriteLine($"Email Error: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Shows SweetAlert message
        /// </summary>
        private void ShowSweetAlert(string title, string message, string icon)
        {
            string script = $@"
                Swal.fire({{
                    icon: '{icon}',
                    title: '{title.Replace("'", "\\'")}',
                    html: '{message.Replace("'", "\\'")}',
                    confirmButtonColor: '#007bff'
                }});
            ";
            ScriptManager.RegisterStartupScript(this, GetType(), "SweetAlert", script, true);
        }

        /// <summary>
        /// Shows SweetAlert with redirect after closing
        /// </summary>
        private void ShowSweetAlertWithRedirect(string title, string message, string icon, string redirectUrl)
        {
            string script = $@"
                Swal.fire({{
                    icon: '{icon}',
                    title: '{title.Replace("'", "\\'")}',
                    html: '{message.Replace("'", "\\'")}',
                    confirmButtonColor: '#007bff',
                    allowOutsideClick: false
                }}).then((result) => {{
                    if (result.isConfirmed) {{
                        window.location.href = '{redirectUrl}';
                    }}
                }});
            ";
            ScriptManager.RegisterStartupScript(this, GetType(), "SweetAlertRedirect", script, true);
        }

        /// <summary>
        /// Clears all form fields
        /// </summary>
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
            txtPassword.Text = "";
            txtConfirmPassword.Text = "";
            chkTerms.Checked = false;
        }

        #endregion
    }
}