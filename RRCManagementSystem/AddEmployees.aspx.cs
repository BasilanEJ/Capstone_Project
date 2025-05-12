using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class AddEmployees : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["AdminID"] == null)
                {
                    Response.Redirect("~/Unauthorized.aspx");
                    return;
                }

                int adminId = Convert.ToInt32(Session["AdminID"]);

                if (!HasPermissionToAdd(adminId, "ManageEmployees"))
                {
                    Response.Redirect("~/Unauthorized.aspx");
                }

                ddlPosition.Items.Insert(0, new ListItem("Select Position", ""));
            }
        }

        private bool HasPermissionToAdd(int userId, string moduleName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT CanAdd 
                    FROM AdminPermissions
                    WHERE UserID = @UserID AND ModuleName = @ModuleName";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@ModuleName", moduleName);

                    try
                    {
                        conn.Open();
                        object result = cmd.ExecuteScalar();
                        return result != null && Convert.ToBoolean(result);
                    }
                    catch (Exception ex)
                    {
                        ShowMessage("❌ Error checking permissions: " + ex.Message, false);
                        return false;
                    }
                }
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            int adminId = Convert.ToInt32(Session["AdminID"]);

            if (!HasPermissionToAdd(adminId, "ManageEmployees"))
            {
                ShowMessage("❌ You do not have permission to add employees.", false);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                ShowMessage("⚠ Full Name is required.", false);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text) || !Regex.IsMatch(txtEmail.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ShowMessage("⚠ Please enter a valid Email.", false);
                return;
            }

            string phoneNumber = txtPhone.Text.Trim();
            if (!Regex.IsMatch(phoneNumber, @"^\d{11}$"))
            {
                ShowMessage("⚠ Please enter a valid 11-digit phone number.", false);
                return;
            }

            if (ddlPosition.SelectedIndex == 0)
            {
                ShowMessage("⚠ Please select a Position.", false);
                return;
            }

            string imagePath = string.Empty;
            if (fuProfilePicture.HasFile)
            {
                string fileExtension = Path.GetExtension(fuProfilePicture.FileName).ToLower();
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };

                if (!Array.Exists(allowedExtensions, ext => ext == fileExtension))
                {
                    ShowMessage("⚠ Only JPG, JPEG, and PNG files are allowed.", false);
                    return;
                }

                string fileName = Guid.NewGuid().ToString() + fileExtension;
                string folderPath = Server.MapPath("~/EmployeeImages/");
                string fullPath = Path.Combine(folderPath, fileName);

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                fuProfilePicture.SaveAs(fullPath);
                imagePath = "~/EmployeeImages/" + fileName;
            }
            else
            {
                ShowMessage("⚠ Please select a profile picture.", false);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        INSERT INTO Employees (FullName, Email, Phone, Position, ProfileImage) 
                        VALUES (@FullName, @Email, @Phone, @Position, @ProfileImage)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@FullName", txtFullName.Text.Trim());
                        cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@Phone", phoneNumber);
                        cmd.Parameters.AddWithValue("@Position", ddlPosition.SelectedValue);
                        cmd.Parameters.AddWithValue("@ProfileImage", imagePath);

                        conn.Open();
                        cmd.ExecuteNonQuery();

                        // ✅ Audit Log here
                        AddAuditLog(adminId, $"Added a new employee: {txtFullName.Text.Trim()}");
                    }
                }

                ShowMessage("✅ Employee added successfully!", true);

                txtFullName.Text = "";
                txtEmail.Text = "";
                txtPhone.Text = "";
                ddlPosition.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ShowMessage("❌ Error: " + ex.Message, false);
            }
        }

        private void ShowMessage(string message, bool isSuccess)
        {
            lblMessage.Text = message;
            lblMessage.ForeColor = isSuccess ? System.Drawing.Color.Green : System.Drawing.Color.Red;
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
                        // Optional: handle/log exception silently
                    }
                }
            }
        }
    }
}