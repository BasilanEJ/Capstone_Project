using System;
using System.Configuration;
using System.Data;
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
            // require login
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

            int userId = Convert.ToInt32(Session["UserID"]);
            if (!HasPermission(userId, "CreateCustomerAccount", "CanView"))
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

            if (EmailExists(email))
            {
                ShowSweetAlert("Error", "This email is already registered.", "warning");
                return;
            }

            string token = Guid.NewGuid().ToString();
            DateTime expiry = DateTime.Now.AddHours(1);

            int clientId = CreateClientWithReset(
                lastName, firstName, middleName, email, contact, street, brgy, city, region, country, landmark, token, expiry
            );

            if (clientId <= 0)
            {
                ShowSweetAlert("Error", "Failed to add client.", "error");
                return;
            }

            // NOTE: your ResetPassword page expects type=client (not admin)
            bool sent = SendResetEmail(email, token);
            ShowSweetAlert(sent ? "Success" : "Partial Success",
                sent ? "Client added successfully. Email sent for password setup."
                     : "Client added but failed to send email.",
                sent ? "success" : "warning");
        }

        /* ===== DB helpers using stored procedures ===== */

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
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spClient_EmailExists", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = email;
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

                cmd.Parameters.Add("@LastName", SqlDbType.NVarChar, 100).Value = lastName;
                cmd.Parameters.Add("@FirstName", SqlDbType.NVarChar, 100).Value = firstName;
                cmd.Parameters.Add("@MiddleName", SqlDbType.NVarChar, 100).Value = (object)middleName ?? DBNull.Value;
                cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = email;
                cmd.Parameters.Add("@ContactNumber", SqlDbType.NVarChar, 50).Value = contact;
                cmd.Parameters.Add("@StreetAndUnit", SqlDbType.NVarChar, 255).Value = street;
                cmd.Parameters.Add("@Barangay", SqlDbType.NVarChar, 100).Value = brgy;
                cmd.Parameters.Add("@City", SqlDbType.NVarChar, 100).Value = city;
                cmd.Parameters.Add("@Region", SqlDbType.NVarChar, 100).Value = region;
                cmd.Parameters.Add("@Country", SqlDbType.NVarChar, 100).Value = country;
                cmd.Parameters.Add("@Landmark", SqlDbType.NVarChar, 255).Value = (object)landmark ?? DBNull.Value;
                cmd.Parameters.Add("@ResetToken", SqlDbType.NVarChar, 200).Value = token;
                cmd.Parameters.Add("@ResetTokenExpiry", SqlDbType.DateTime).Value = expiry;

                con.Open();
                object id = cmd.ExecuteScalar();
                return (id != null && int.TryParse(id.ToString(), out int clientId)) ? clientId : 0;
            }
        }

        /* ===== UI helpers (unchanged) ===== */

        private bool SendResetEmail(string toEmail, string token)
        {
            try
            {
                // for clients, make the link explicit:
                string resetLink = $"https://rrcmngmnt.com/ResetPassword.aspx?type=client&token={token}";
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

                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress("rrctermiteandpestcontrol@gmail.com", "RRC Management System");
                    mail.To.Add(toEmail);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;

                    using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new NetworkCredential("rrctermiteandpestcontrol@gmail.com", "pktz jwzp tbvx qheq");
                        smtp.EnableSsl = true;
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
            else
            {
                ddlCity.Items.Add(new ListItem("No Cities Available", ""));
            }
        }
    }
}
