using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Net;
using System.Net.Mail;
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

        private void LoadRoles()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT RoleName FROM Roles ORDER BY RoleName ASC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        ddlRole.Items.Clear();
                        ddlRole.Items.Add(new ListItem("Select Role", ""));

                        while (reader.Read())
                        {
                            string role = reader["RoleName"].ToString();
                            ddlRole.Items.Add(new ListItem(role, role));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("❌ Error loading roles: " + ex.Message);
            }
        }

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
            string name = txtName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string role = ddlRole.SelectedValue;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role))
            {
                ShowError("⚠ Please fill in all required fields.");
                return;
            }

            string resetToken = Guid.NewGuid().ToString();
            DateTime tokenExpiry = DateTime.Now.AddHours(1);

            try
            {
                int newUserId;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string insertQuery = @"
                        INSERT INTO Users (Name, Email, Role, ResetToken, TokenExpiry, CreatedAt, Status)
                        OUTPUT INSERTED.UserID
                        VALUES (@Name, @Email, @Role, @ResetToken, @TokenExpiry, GETDATE(), 'Active')";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", name);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Role", role);
                        cmd.Parameters.AddWithValue("@ResetToken", resetToken);
                        cmd.Parameters.AddWithValue("@TokenExpiry", tokenExpiry);

                        conn.Open();
                        newUserId = (int)cmd.ExecuteScalar();
                    }
                }

                SavePermissions(newUserId);
                bool emailSent = SendResetEmail(email, resetToken, role);

                if (emailSent)
                    ShowSuccess("✅ Admin account created and email sent successfully!", true);
                else
                    ShowWarning("⚠ Admin account created, but failed to send email.");
            }
            catch (Exception ex)
            {
                ShowError("⚠ Error creating admin: " + ex.Message);
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
                DataRow row = dt.NewRow();
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

        private void SavePermissions(int userId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                foreach (RepeaterItem item in rptPermissions.Items)
                {
                    string module = ((HiddenField)item.FindControl("hfModuleName")).Value;
                    bool canView = ((CheckBox)item.FindControl("chkView")).Checked;
                    bool canAdd = ((CheckBox)item.FindControl("chkAdd")).Checked;
                    bool canEdit = ((CheckBox)item.FindControl("chkEdit")).Checked;
                    bool canDelete = ((CheckBox)item.FindControl("chkDelete")).Checked;

                    string insert = @"
                        INSERT INTO AdminPermissions (UserID, ModuleName, CanView, CanAdd, CanEdit, CanDelete)
                        VALUES (@UserID, @ModuleName, @CanView, @CanAdd, @CanEdit, @CanDelete)";

                    using (SqlCommand cmd = new SqlCommand(insert, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.Parameters.AddWithValue("@ModuleName", module);
                        cmd.Parameters.AddWithValue("@CanView", canView);
                        cmd.Parameters.AddWithValue("@CanAdd", canAdd);
                        cmd.Parameters.AddWithValue("@CanEdit", canEdit);
                        cmd.Parameters.AddWithValue("@CanDelete", canDelete);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private bool SendResetEmail(string toEmail, string token, string role)
        {
            try
            {
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                string formattedRole = textInfo.ToTitleCase(role.ToLower());
                string resetLink = $"https://localhost:44341/ResetAdminPassword.aspx?type=admin&token={token}";
                string subject = "Set Your Password - RRC Management System";
                string body = $@"
                    <h3>Welcome to RRC Management System</h3>
                    <p>You have been invited as a <strong>{formattedRole}</strong>.</p>
                    <p>Please click the link below to set your password:</p>
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