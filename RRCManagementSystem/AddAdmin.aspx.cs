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
            // Compute hash of the plain email
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

        private int CreateAdminUser(string name, string plainEmail, string role, string resetToken, DateTime tokenExpiry)
        {
            // Encrypt and hash the email
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

                var pOut = cmd.Parameters.Add("@NewUserID", SqlDbType.Int);
                pOut.Direction = ParameterDirection.Output;

                conn.Open();
                cmd.ExecuteNonQuery();
                return (int)pOut.Value;
            }
        }

        private void SavePermissionsBulk(int userId)
        {
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
                LoadDefaultPermissions(0, selectedRole);
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
                // 1) Create user via SP
                int newUserId = CreateAdminUser(name, email, role, resetToken, tokenExpiry);

                // 2) Save permissions via TVP
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
                // Use CultureInfo to correctly format the role for a professional appearance.
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                string formattedRole = textInfo.ToTitleCase((role ?? "").ToLower());
                string resetLink = $"https://rrcmngmnt.com/ResetAdminPassword.aspx?type=admin&token={token}";
                string subject = "Set Your Password - RRC Management System";

                // Updated HTML body with a professional, table-based layout and inline CSS for email client compatibility.
                string body = $@"
<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>Set Your Password - RRC Management System</title>
    <style type=""text/css"">
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif, 'Apple Color Emoji', 'Segoe UI Emoji', 'Segoe UI Symbol'; margin: 0; padding: 0; background-color: #f4f7fa; }}
        table {{ border-collapse: collapse; }}
        a {{ text-decoration: none; }}
        .button {{
            background-color: #2b6cb0;
            color: #ffffff;
            font-size: 16px;
            font-weight: bold;
            padding: 12px 24px;
            border-radius: 6px;
            display: inline-block;
        }}
        .link-text {{ color: #2b6cb0; text-decoration: underline; word-break: break-all; }}
        .content-box {{ background-color: #ffffff; border-radius: 8px; box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05); padding: 30px; }}
        .header {{ background-color: #1a202c; padding: 20px 0; }}
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
                            <h2 style=""color: #2d3748; font-size: 24px; margin: 0 0 15px;"">Welcome to RRC Management System</h2>
                            <p style=""color: #4a5568; font-size: 16px; line-height: 1.6; margin: 0 0 15px;"">Hello,</p>
                            <p style=""color: #4a5568; font-size: 16px; line-height: 1.6; margin: 0 0 15px;"">You have been invited to join the RRC Management System as a <strong>{formattedRole}</strong>. To get started, you'll need to set your password.</p>
                            <p style=""color: #4a5568; font-size: 16px; line-height: 1.6; margin: 0 0 30px;"">Please click the button below to continue:</p>
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
                                This link will expire in 1 hour. If you did not request this, you can safely ignore this email.
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

                using (var mail = new MailMessage())
                {
                    // NOTE: Using a professional domain email (e.g., support@rrcmngmnt.com)
                    // is highly recommended to improve deliverability and avoid spam filters.
                    mail.From = new MailAddress("rrctermiteandpestcontrol@gmail.com", "RRC Management System");
                    mail.To.Add(toEmail);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;

                    using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        // NOTE: The credentials should be stored in a secure location,
                        // like a configuration file (as you noted in your comment),
                        // and not hardcoded here.
                        smtp.Credentials = new NetworkCredential(
                            "rrctermiteandpestcontrol@gmail.com",
                            "pktz jwzp tbvx qheq"
                        );
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                // Logging the full exception in a real application is a better practice
                // to help with debugging.
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
