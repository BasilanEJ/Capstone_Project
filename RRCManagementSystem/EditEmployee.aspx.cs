using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class EditEmployee : Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string role = Session["Role"].ToString();
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            if (!HasEditPermission(userId, "ManageEmployees"))
            {
                lblMessage.Text = "❌ You do not have permission to edit employees.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                btnSave.Enabled = false;
                return;
            }

            if (!IsPostBack)
            {
                if (int.TryParse(Request.QueryString["EmployeeID"], out int employeeID))
                {
                    LoadEmployeeData(employeeID);
                }
                else
                {
                    lblMessage.Text = "❌ Invalid Employee ID.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
            }
        }

        private bool HasEditPermission(int userId, string moduleName)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spAdminPermission_Check", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@ModuleName", SqlDbType.NVarChar, 100).Value = moduleName;
                cmd.Parameters.Add("@Permission", SqlDbType.NVarChar, 10).Value = "CanEdit";

                con.Open();
                object result = cmd.ExecuteScalar();
                return result != null && Convert.ToBoolean(result);
            }
        }

        private void LoadEmployeeData(int employeeID)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spEmployee_GetById", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = employeeID;

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        txtLastName.Text = reader["LastName"].ToString();
                        txtFirstName.Text = reader["FirstName"].ToString();
                        txtMiddleName.Text = reader["MiddleName"].ToString();
                        txtEmail.Text = reader["Email"].ToString();
                        txtPhone.Text = reader["Phone"].ToString();
                        ddlPosition.SelectedValue = reader["Position"].ToString();
                        ddlStatus.SelectedValue = reader["Status"].ToString();

                        string profileImagePath = reader["ProfileImage"].ToString();
                        if (!string.IsNullOrEmpty(profileImagePath))
                        {
                            imgProfilePreview.ImageUrl = profileImagePath;
                            imgProfilePreview.Visible = true;
                        }
                    }
                    else
                    {
                        lblMessage.Text = "❌ Employee not found.";
                        lblMessage.ForeColor = System.Drawing.Color.Red;
                    }
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int adminId = Convert.ToInt32(Session["UserID"]);
            if (!HasEditPermission(adminId, "ManageEmployees"))
            {
                lblMessage.Text = "❌ You do not have permission to edit employees.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            if (!int.TryParse(Request.QueryString["EmployeeID"], out int employeeID))
            {
                lblMessage.Text = "❌ Invalid Employee ID.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            string phone = txtPhone.Text.Trim();
            if (phone.Length != 11 || !System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\d{11}$"))
            {
                lblMessage.Text = "⚠ Please enter a valid 11-digit phone number.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            // upload (optional)
            string profileImagePath = imgProfilePreview.ImageUrl;
            if (fuProfilePicture.HasFile)
            {
                string[] allowed = { ".jpg", ".jpeg", ".png" };
                string ext = Path.GetExtension(fuProfilePicture.FileName)?.ToLowerInvariant();
                if (Array.IndexOf(allowed, ext) < 0)
                {
                    lblMessage.Text = "⚠ Only JPG, JPEG, and PNG files are allowed.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                string fileName = Guid.NewGuid() + ext;
                string folder = Server.MapPath("~/uploads/");
                Directory.CreateDirectory(folder);
                string fullPath = Path.Combine(folder, fileName);
                fuProfilePicture.SaveAs(fullPath);

                profileImagePath = "~/uploads/" + fileName;
            }

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spEmployee_Update", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = employeeID;
                cmd.Parameters.Add("@LastName", SqlDbType.NVarChar, 100).Value = txtLastName.Text.Trim();
                cmd.Parameters.Add("@FirstName", SqlDbType.NVarChar, 100).Value = txtFirstName.Text.Trim();
                cmd.Parameters.Add("@MiddleName", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(txtMiddleName.Text) ? (object)DBNull.Value : txtMiddleName.Text.Trim();
                cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = txtEmail.Text.Trim();
                cmd.Parameters.Add("@Phone", SqlDbType.NVarChar, 15).Value = phone;
                cmd.Parameters.Add("@Position", SqlDbType.NVarChar, 50).Value = ddlPosition.SelectedValue;
                cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = ddlStatus.SelectedValue;
                cmd.Parameters.Add("@ProfileImage", SqlDbType.NVarChar, 255).Value = string.IsNullOrWhiteSpace(profileImagePath) ? (object)DBNull.Value : profileImagePath;

                conn.Open();
                int rows = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);

                // audit
                using (var audit = new SqlCommand("dbo.spAudit_Insert", conn))
                {
                    audit.CommandType = CommandType.StoredProcedure;
                    audit.Parameters.Add("@AdminID", SqlDbType.Int).Value = adminId;
                    string fullName = $"{txtLastName.Text.Trim()}, {txtFirstName.Text.Trim()}" +
                                      (string.IsNullOrWhiteSpace(txtMiddleName.Text) ? "" : $" {txtMiddleName.Text.Trim()}");
                    audit.Parameters.Add("@Action", SqlDbType.NVarChar, 255).Value =
                        $"Updated employee (ID: {employeeID}) - Name: {fullName}, Position: {ddlPosition.SelectedValue}, Status: {ddlStatus.SelectedValue}";
                    audit.ExecuteNonQuery();
                }

                if (rows > 0)
                {
                    // Show success message with SweetAlert and redirect
                    string script = @"
                        Swal.fire({
                            icon: 'success',
                            title: 'Employee Updated!',
                            text: 'The employee information has been successfully updated.',
                            confirmButtonColor: '#2563eb',
                            timer: 2000,
                            timerProgressBar: true
                        }).then(() => {
                            window.location.href = 'AllEmployee.aspx';
                        });
                    ";
                    ScriptManager.RegisterStartupScript(this, GetType(), "showSuccessAndRedirect", script, true);
                }
                else
                {
                    lblMessage.Text = "⚠ No changes saved.";
                    lblMessage.ForeColor = System.Drawing.Color.OrangeRed;
                }
            }
        }
    }
}