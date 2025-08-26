using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class AddAdmin : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadRoles();
            }
        }

        /* =========================
           STORED PROCEDURE CALLS
           ========================= */

        private void LoadRoles()
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spRoles_List", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        ddlRole.Items.Clear();
                        ddlRole.Items.Add(new ListItem("Select Role", ""));

                        while (reader.Read())
                        {
                            string roleName = reader["RoleName"].ToString();
                            ddlRole.Items.Add(new ListItem(roleName, roleName)); // keeping Users.Role as NVARCHAR(RoleName)
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("❌ Error loading roles: " + ex.Message);
            }
        }

        private bool EmailExists(string email)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spUser_EmailExists", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = email;
                conn.Open();
                var existsFlag = cmd.ExecuteScalar();
                return existsFlag != null && Convert.ToInt32(existsFlag) == 1;
            }
        }

        private int CreateAdminUser(string name, string email, string role, string resetToken, DateTime tokenExpiry)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spUser_CreateAdmin", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = name;
                cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = email;
                cmd.Parameters.Add("@Role", SqlDbType.NVarChar, 100).Value = role;
                cmd.Parameters.Add("@ResetToken", SqlDbType.NVarChar, 100).Value = resetToken;
                cmd.Parameters.Add("@TokenExpiry", SqlDbType.DateTime).Value = tokenExpiry;

                var pOut = cmd.Parameters.Add("@NewUserID", SqlDbType.Int);
                pOut.Direction = ParameterDirection.Output;

                conn.Open();
                cmd.ExecuteNonQuery();
                return (int)pOut.Value;
            }
        }

        private void SavePermissionsBulk(int userId)
        {
            // Build a DataTable that matches dbo.AdminPermissionTVP
            var tvp = new DataTable();
            tvp.Columns.Add("ModuleName", typeof(string));
            tvp.Columns.Add("CanView", typeof(bool));
            tvp.Columns.Add("CanAdd", typeof(bool));
            tvp.Columns.Add("CanEdit", typeof(bool));
            tvp.Columns.Add("CanDelete", typeof(bool));

            foreach (RepeaterItem item in rptPermissions.Items)
            {
                string module = ((HiddenField)item.FindControl("hfModuleName")).Value;
                bool canView = ((CheckBox)item.FindControl("chkView")).Checked;
                bool canAdd = ((CheckBox)item.FindControl("chkAdd")).Checked;
                bool canEdit = ((CheckBox)item.FindControl("chkEdit")).Checked;
                bool canDelete = ((CheckBox)item.FindControl("chkDelete")).Checked;

                tvp.Rows.Add(module, canView, canAdd, canEdit, canDelete);
            }

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spAdminPermissions_BulkReplace", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;

                var p = cmd.Parameters.AddWithValue("@Perms", tvp);
                p.SqlDbType = SqlDbType.Structured;
                p.TypeName = "dbo.AdminPermissionTVP";

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        /* =========================
           UI / EVENTS
           ========================= */

        protected void ddlRole_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedRole = ddlRole.SelectedValue;

            if (!string.IsNullOrEmpty(selectedRole))
            {
                LoadDefaultPermissions(0, selectedRole); // 0 = not yet created
            }
            else
            {
                rptPermissions.Visible = false;
                rptPermissions.DataSource = null;
                rptPermissions.DataBind();
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            Page.Validate();
            if (!Page.IsValid)
            {
                ShowError("⚠ Please fix the highlighted errors.");
                return;
            }

            string name = (txtName.Text ?? "").Trim();
            string email = (txtEmail.Text ?? "").Trim().ToLowerInvariant();
            string role = ddlRole.SelectedValue;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role))
            {
                ShowError("⚠ Please fill in all required fields.");
                return;
            }

            if (!IsValidName(name))
            {
                ShowError("⚠ Name can only contain letters, spaces, hyphen (-), and apostrophe (').");
                return;
            }

            if (!IsAllowedEmailDomain(email))
            {
                ShowError("⚠ Email must be Gmail, Yahoo, or Outlook.");
                return;
            }

            if (EmailExists(email))
            {
                ShowError("⚠ That email is already in use. Please use a different email.");
                return;
            }

            string resetToken = Guid.NewGuid().ToString();
            DateTime tokenExpiry = DateTime.Now.AddHours(1);

            try
            {
                // 1) Create user via SP (returns new UserID)
                int newUserId = CreateAdminUser(name, email, role, resetToken, tokenExpiry);

                // 2) Save permissions in one go via TVP
                SavePermissionsBulk(newUserId);

                // 3) Email invite
                bool emailSent = SendResetEmail(email, resetToken, role);

                if (emailSent)
                    ShowSuccess("User account created and email sent successfully!", true);
                else
                    ShowWarning("User account created, but failed to send email.");
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                ShowError("⚠ That email is already in use. Please use a different email.");
            }
            catch (Exception ex)
            {
                ShowError("⚠ Error creating user: " + ex.Message);
            }
        }

        private void LoadDefaultPermissions(int userId, string role)
        {
            string[] modules = {
                "Dashboard",
                "ManageInquiry",
                "ClientApproval",
                "CreateCustomerAccount",
                "ManageEmployees",
                "ManageItem",
                "ManageEquipment",
                "ManageClient",
                "ManageBooking",
                "Sales&Transaction",
                "ManageSupplier",
                "ManageServices",
                "AdminReports",
                "AdminGuide"
            };

            DataTable dt = new DataTable();
            dt.Columns.Add("ModuleName");
            dt.Columns.Add("CanView", typeof(bool));
            dt.Columns.Add("CanAdd", typeof(bool));
            dt.Columns.Add("CanEdit", typeof(bool));
            dt.Columns.Add("CanDelete", typeof(bool));

            bool fullPermission = role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

            foreach (string module in modules)
            {
                var row = dt.NewRow();
                row["ModuleName"] = module;
                row["CanView"] = fullPermission;
                row["CanAdd"] = fullPermission;
                row["CanEdit"] = fullPermission;
                row["CanDelete"] = fullPermission;
                dt.Rows.Add(row);
            }

            rptPermissions.DataSource = dt;
            rptPermissions.DataBind();
            rptPermissions.Visible = true;
        }

        /* =========================
           HELPERS
           ========================= */

        private static bool IsValidName(string name)
        {
            return Regex.IsMatch(name, @"^[A-Za-zÀ-ÖØ-öø-ÿ\s'\-]+$");
        }

        private static bool IsAllowedEmailDomain(string email)
        {
            return email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase)
                || email.EndsWith("@yahoo.com", StringComparison.OrdinalIgnoreCase)
                || email.EndsWith("@outlook.com", StringComparison.OrdinalIgnoreCase);
        }

        private bool SendResetEmail(string toEmail, string token, string role)
        {
            try
            {
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                string formattedRole = textInfo.ToTitleCase((role ?? "").ToLower());
                string resetLink = $"https://rrcmanagement-bcfgfpa5hzaafhdy.eastasia-01.azurewebsites.net/ResetAdminPassword.aspx?type=admin&token={token}";
                string subject = "Set Your Password - RRC Management System";

                string body = $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='UTF-8'>
  <style>
    body {{
      background-color: #f9f9f9;
      font-family: Arial, sans-serif;
      color: #333;
      line-height: 1.6;
      margin: 0;
      padding: 0;
    }}
    .container {{
      max-width: 600px;
      margin: 30px auto;
      background: #ffffff;
      border-radius: 8px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.05);
      padding: 20px 30px;
    }}
    h3 {{ color: #2a4fa7; margin-bottom: 10px; }}
    p {{ margin: 10px 0; }}
    .button {{
      display: inline-block;
      padding: 12px 20px;
      background-color: #add8e6;
      color: #000000;
      text-decoration: none;
      border-radius: 5px;
      font-weight: bold;
      margin-top: 15px;
    }}
    .footer {{
      font-size: 12px;
      color: #777;
      margin-top: 25px;
      border-top: 1px solid #eee;
      padding-top: 10px;
    }}
  </style>
</head>
<body>
  <div class='container'>
    <h3>Welcome to RRC Management System</h3>
    <p>You have been invited as a <strong>{formattedRole}</strong>.</p>
    <p>Click the button below to set your password:</p>
    <p><a href='{resetLink}' class='button'>Set Password</a></p>
    <p class='footer'>This link will expire in 1 hour. If you did not request this, you can ignore this email.</p>
  </div>
</body>
</html>";

                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress("rrctermiteandpestcontrol@gmail.com", "RRC Management System");
                    mail.To.Add(toEmail);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;

                    using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new NetworkCredential(
                            "rrctermiteandpestcontrol@gmail.com",
                            "pktz jwzp tbvx qheq" // move to Web.config/appSettings
                        );
                        smtp.EnableSsl = true;
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

        private void ShowSuccess(string message, bool redirect)
        {
            string script = $@"
                Swal.fire({{
                    icon: 'success',
                    title: 'Success',
                    text: '{message}',
                    confirmButtonColor: '#3085d6'
                }})";

            if (redirect)
            {
                script += @".then((result) => {
                    if (result.isConfirmed) {
                        window.location.href = 'ViewAdmin.aspx';
                    }
                });";
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "successMessage", script, true);
        }

        private void ShowWarning(string message)
        {
            ScriptManager.RegisterStartupScript(this, this.GetType(), "warningMessage", $@"
                Swal.fire({{
                    icon: 'warning',
                    title: 'Warning',
                    text: '{message}',
                    confirmButtonColor: '#f27474'
                }});
            ", true);
        }

        private void ShowError(string message)
        {
            lblMessage.Visible = true;
            lblMessage.ForeColor = System.Drawing.Color.Red;
            lblMessage.Text = message;
        }
    }
}
