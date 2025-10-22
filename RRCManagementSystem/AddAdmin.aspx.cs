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
using RRCManagementSystem.Helpers; // AESHelper

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
                            ddlRole.Items.Add(new ListItem(roleName, roleName));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("❌ Error loading roles: " + ex.Message);
            }
        }

        private bool EmailExists(string plainEmail)
        {
            string emailHash = AESHelper.ComputeSHA256(plainEmail);

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spUser_EmailExists", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@EmailHash", SqlDbType.Char, 64).Value = emailHash;
                conn.Open();
                var existsFlag = cmd.ExecuteScalar();
                return existsFlag != null && Convert.ToInt32(existsFlag) == 1;
            }
        }

        private (int userId, string employeeId) CreateAdminUser(string name, string plainEmail, string role, string resetToken, DateTime tokenExpiry)
        {
            string encryptedEmail = AESHelper.EncryptEmail(plainEmail);
            string emailHash = AESHelper.ComputeSHA256(plainEmail);

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spUser_CreateAdmin", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = name;
                cmd.Parameters.Add("@Email", SqlDbType.NVarChar, -1).Value = encryptedEmail;
                cmd.Parameters.Add("@EmailHash", SqlDbType.Char, 64).Value = emailHash;
                cmd.Parameters.Add("@Role", SqlDbType.NVarChar, 100).Value = role;
                cmd.Parameters.Add("@ResetToken", SqlDbType.NVarChar, 100).Value = resetToken;
                cmd.Parameters.Add("@TokenExpiry", SqlDbType.DateTime).Value = tokenExpiry;

                var pOutUserId = cmd.Parameters.Add("@NewUserID", SqlDbType.Int);
                pOutUserId.Direction = ParameterDirection.Output;

                var pOutEmployeeId = cmd.Parameters.Add("@NewEmployeeID", SqlDbType.VarChar, 20);
                pOutEmployeeId.Direction = ParameterDirection.Output;

                conn.Open();
                cmd.ExecuteNonQuery();

                int userId = (int)pOutUserId.Value;
                string employeeId = pOutEmployeeId.Value.ToString();

                return (userId, employeeId);
            }
        }

        /// <summary>
        /// Copy role permissions from RolePermissions to AdminPermissions for the new user
        /// </summary>
        private void CopyRolePermissionsToUser(int userId, string roleName)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spAdminPermissions_CopyFromRole", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@RoleName", SqlDbType.NVarChar, 100).Value = roleName;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        /* =========================
           FORM SUBMISSION
           ========================= */

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
            DateTime tokenExpiry = DateTime.Now.AddHours(24);

            try
            {
                // Step 1: Create user account
                var (newUserId, employeeId) = CreateAdminUser(name, email, role, resetToken, tokenExpiry);

                // Step 2: Copy role permissions to AdminPermissions for this user
                CopyRolePermissionsToUser(newUserId, role);

                // Step 3: Send invitation email
                bool emailSent = SendResetEmail(email, resetToken, role, employeeId);

                if (emailSent)
                    ShowSuccess($"✅ User account created successfully!<br/>Employee ID: <strong>{employeeId}</strong><br/>Role: <strong>{role}</strong><br/>Permissions have been assigned based on the role.", true);
                else
                    ShowWarning($"User account created (Employee ID: {employeeId}), but failed to send email.");
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

        private bool SendResetEmail(string toEmail, string token, string role, string employeeId)
        {
            try
            {
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                string formattedRole = textInfo.ToTitleCase((role ?? "").ToLower());
                string resetLink = $"https://rrcmngmnt.com/ResetAdminPassword.aspx?type=admin&token={token}";
                string subject = "Set Your Password - RRC Management System";

                string body = $@"
<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>Set Your Password - RRC Management System</title>
    <style type=""text/css"">
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; margin: 0; padding: 0; background-color: #f4f7fa; }}
        table {{ border-collapse: collapse; }}
        a {{ text-decoration: none; }}
        .button {{ background-color: #2b6cb0; color: #ffffff; font-size: 16px; font-weight: bold; padding: 12px 24px; border-radius: 6px; display: inline-block; }}
        .link-text {{ color: #2b6cb0; text-decoration: underline; word-break: break-all; }}
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
                            <h2 style=""color: #2d3748; font-size: 24px; margin: 0 0 15px;"">Welcome to RRC Management System</h2>
                            <p style=""color: #4a5568; font-size: 16px; line-height: 1.6; margin: 0 0 15px;"">Hello,</p>
                            <p style=""color: #4a5568; font-size: 16px; line-height: 1.6; margin: 0 0 15px;"">You have been invited to join the RRC Management System as a <strong>{formattedRole}</strong>. To get started, you'll need to set your password.</p>
                            
                            <div style=""background-color: #edf2f7; border-left: 4px solid #2b6cb0; padding: 15px; margin: 20px 0; border-radius: 4px;"">
                                <p style=""color: #2d3748; font-size: 14px; margin: 0 0 8px;""><strong>Your Account Details:</strong></p>
                                <p style=""color: #4a5568; font-size: 14px; margin: 0;"">Employee ID: <strong>{employeeId}</strong></p>
                                <p style=""color: #4a5568; font-size: 14px; margin: 5px 0 0;"">Role: <strong>{formattedRole}</strong></p>
                            </div>

                            <p style=""color: #4a5568; font-size: 16px; line-height: 1.6; margin: 0 0 30px;"">Please click the button below to set your password and activate your account:</p>
                            <p align=""center"" style=""margin: 0; text-align: center;"">
                                <a href=""{resetLink}"" class=""button"" style=""background-color: #2b6cb0; color: #ffffff; font-size: 16px; font-weight: bold; padding: 12px 24px; border-radius: 6px; display: inline-block;"">Set Password</a>
                            </p>
                            <p style=""color: #4a5568; font-size: 14px; line-height: 1.6; margin: 30px 0 15px; text-align: center;"">If the button does not work, you can copy and paste the following URL into your browser:</p>
                            <p style=""text-align: center; font-size: 14px; margin: 0;""><a href=""{resetLink}"" class=""link-text"" style=""color: #2b6cb0; text-decoration: underline; word-break: break-all;"">{resetLink}</a></p>
                        </td>
                    </tr>
                    <tr>
                        <td align=""center"" style=""padding-top: 20px;"">
                            <p style=""font-size: 12px; color: #718096; margin: 0; text-align: center;"">
                                This link will expire in 24 hours. If you did not request this, you can safely ignore this email.
                                <br/><br/>
                                &copy; 2025 RRC Management System. All rights reserved.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";

                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(
                        ConfigurationManager.AppSettings["SmtpEmail"] ?? "rrctermiteandpestcontrol@gmail.com",
                        "RRC Management System"
                    );
                    mail.To.Add(toEmail);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;

                    using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new NetworkCredential(
                            ConfigurationManager.AppSettings["SmtpEmail"] ?? "rrctermiteandpestcontrol@gmail.com",
                            ConfigurationManager.AppSettings["SmtpPassword"] ?? "pktz jwzp tbvx qheq"
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
                    html: '{message}',
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
                    html: '{message}',
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