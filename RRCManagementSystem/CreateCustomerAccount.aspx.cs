using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class CreateCustomerAccount : Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 🔐 Require login
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string role = Session["Role"].ToString();

            // 🔐 Deny access for SuperAdmin and Inspector only
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            // 🔐 Check CanView permission for CreateCustomerAccount
            if (!HasPermission(userId, "CreateCustomerAccount"))
            {
                Response.Redirect("~/Unauthorized.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadRegions();
                txtCountry.Text = "Philippines";
            }
        }



        protected void btnCreate_Click(object sender, EventArgs e)
        {
            string lastName = txtLastName.Text.Trim();
            string firstName = txtFirstName.Text.Trim();
            string middleName = txtMiddleName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string contact = txtContact.Text.Trim();
            string street = txtStreet.Text.Trim();
            string brgy = txtBarangay.Text.Trim();
            string city = ddlCity.SelectedValue;
            string region = ddlRegion.SelectedValue;
            string country = txtCountry.Text.Trim();
            string landmark = txtLandmark.Text.Trim();

            if (string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(contact) ||
                string.IsNullOrWhiteSpace(street) || string.IsNullOrWhiteSpace(brgy) ||
                string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(region) ||
                string.IsNullOrWhiteSpace(country))
            {
                ShowSweetAlert("Error", "Please fill in all required fields.", "error");
                return;
            }

            string token = Guid.NewGuid().ToString();
            DateTime expiry = DateTime.Now.AddHours(1);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string query = @"
INSERT INTO Clients
(LastName, FirstName, MiddleName, Email, ContactNumber, StreetAndUnit, Barangay, City, Region, Country, Landmark,
 Status, CreatedAt, UserRole, PasswordHash, PasswordSalt, TermsAccepted, ResetToken, ResetTokenExpiry)
VALUES
(@LastName, @FirstName, @MiddleName, @Email, @ContactNumber, @StreetAndUnit, @Barangay, @City, @Region, @Country, @Landmark,
 'Approved', GETDATE(), 'Client', '', '', 0, @ResetToken, @ResetTokenExpiry);";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@LastName", lastName);
                cmd.Parameters.AddWithValue("@FirstName", firstName);
                cmd.Parameters.AddWithValue("@MiddleName", string.IsNullOrEmpty(middleName) ? (object)DBNull.Value : middleName);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@ContactNumber", contact);
                cmd.Parameters.AddWithValue("@StreetAndUnit", street);
                cmd.Parameters.AddWithValue("@Barangay", brgy);
                cmd.Parameters.AddWithValue("@City", city);
                cmd.Parameters.AddWithValue("@Region", region);
                cmd.Parameters.AddWithValue("@Country", country);
                cmd.Parameters.AddWithValue("@Landmark", landmark);
                cmd.Parameters.AddWithValue("@ResetToken", token);
                cmd.Parameters.AddWithValue("@ResetTokenExpiry", expiry);
                cmd.ExecuteNonQuery();
            }

            if (SendResetEmail(email, token))
                ShowSweetAlert("Success", "Client added successfully. Email sent for password setup.", "success");
            else
                ShowSweetAlert("Error", "Client added but failed to send email.", "error");
        }

        private bool SendResetEmail(string toEmail, string token)
        {
            try
            {
                string resetLink = $"https://localhost:44341/ResetPassword.aspx?token={token}";
                string subject = "Set Your Password - RRC Management System";
                string body = $@"
<h3>Welcome to RRC Management System</h3>
<p>You have been registered as a <strong>Client</strong>.</p>
<p>Click below to set your password:</p>
<p><a href='{resetLink}'>Set Password</a></p>
<p>This link will expire in 1 hour.</p>";

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress("edgarjosephbasilan@gmail.com", "RRC Management System");
                    mail.To.Add(toEmail);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new NetworkCredential("edgarjosephbasilan@gmail.com", "fbryvkhttqobssjy");
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Email Error: " + ex.Message);
                return false;
            }
        }

        private void ShowSweetAlert(string title, string message, string icon)
        {
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

        protected void ddlRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedRegion = ddlRegion.SelectedValue;
            LoadCities(selectedRegion);
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

            if (string.IsNullOrEmpty(selectedRegion))
            {
                ddlCity.Items.Add(new ListItem("No Cities Available", ""));
                return;
            }

            switch (selectedRegion)
            {
                case "NCR":
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
                    break;
                default:
                    ddlCity.Items.Add(new ListItem("No Cities Available", ""));
                    break;
            }
        }

        private bool HasPermission(int userId, string moduleName)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM AdminPermissions WHERE UserID = @UserID AND ModuleName = @ModuleName AND CanView = 1";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@ModuleName", moduleName);
                    con.Open();
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }


    }
}