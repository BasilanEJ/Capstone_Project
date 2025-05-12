using System;
using System.Configuration;
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
            if (Session["AdminID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int adminId = Convert.ToInt32(Session["AdminID"]);

            if (!HasEditPermission(adminId, "ManageEmployees"))
            {
                lblMessage.Text = "❌ You do not have permission to edit employees.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                btnSave.Enabled = false;
                return;
            }

            if (!IsPostBack)
            {
                if (Request.QueryString["EmployeeID"] != null && int.TryParse(Request.QueryString["EmployeeID"], out int employeeID))
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

        private bool HasEditPermission(int adminId, string moduleName)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT CanEdit FROM AdminPermissions WHERE UserID = @UserID AND ModuleName = @ModuleName";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserID", adminId);
                    cmd.Parameters.AddWithValue("@ModuleName", moduleName);

                    try
                    {
                        con.Open();
                        object result = cmd.ExecuteScalar();
                        return result != null && result != DBNull.Value && Convert.ToBoolean(result);
                    }
                    catch (Exception ex)
                    {
                        lblMessage.Text = $"❌ Permission check failed: {ex.Message}";
                        lblMessage.ForeColor = System.Drawing.Color.Red;
                        return false;
                    }
                }
            }
        }

        private void LoadEmployeeData(int employeeID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT FullName, Email, Phone, Position, Status, ProfileImage FROM Employees WHERE EmployeeID = @EmployeeID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@EmployeeID", employeeID);
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        txtFullName.Text = reader["FullName"].ToString();
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
                    reader.Close();
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int adminId = Convert.ToInt32(Session["AdminID"]);

            if (!HasEditPermission(adminId, "ManageEmployees"))
            {
                lblMessage.Text = "❌ You do not have permission to edit employees.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            string fullName = txtFullName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string position = ddlPosition.SelectedValue;
            string status = ddlStatus.SelectedValue;
            string profileImagePath = imgProfilePreview.ImageUrl;

            if (phone.Length != 11 || !System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\d{11}$"))
            {
                lblMessage.Text = "⚠ Please enter a valid 11-digit phone number.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            if (fuProfilePicture.HasFile)
            {
                string fileExtension = Path.GetExtension(fuProfilePicture.FileName).ToLower();
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };

                if (!Array.Exists(allowedExtensions, ext => ext == fileExtension))
                {
                    lblMessage.Text = "⚠ Only JPG, JPEG, and PNG files are allowed.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                string fileName = Guid.NewGuid().ToString() + fileExtension;
                string folderPath = Server.MapPath("~/uploads/");
                string fullPath = Path.Combine(folderPath, fileName);

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                fuProfilePicture.SaveAs(fullPath);
                profileImagePath = "~/uploads/" + fileName;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string updateQuery = @"
                    UPDATE Employees 
                    SET FullName = @FullName, Email = @Email, Phone = @Phone, 
                        Position = @Position, Status = @Status, ProfileImage = @ProfileImage
                    WHERE EmployeeID = @EmployeeID";

                using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                {
                    string employeeID = Request.QueryString["EmployeeID"];

                    cmd.Parameters.AddWithValue("@EmployeeID", employeeID);
                    cmd.Parameters.AddWithValue("@FullName", fullName);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Phone", phone);
                    cmd.Parameters.AddWithValue("@Position", position);
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@ProfileImage", profileImagePath);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    // ✅ Audit log after successful update
                    AddAuditLog(adminId, $"Updated employee (ID: {employeeID}) - Name: {fullName}, Position: {position}, Status: {status}");
                }
            }

            lblMessage.Text = "✅ Employee updated successfully!";
            lblMessage.ForeColor = System.Drawing.Color.Green;
        }

        // ✅ Audit Log Method
        private void AddAuditLog(int? userID, string action)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO AuditLogs (AdminID, Action, Timestamp) VALUES (@AdminID, @Action, GETDATE())";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@AdminID", (object)userID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Action", action);

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch
                    {
                        // Optionally handle or log failure
                    }
                }
            }
        }
    }
}