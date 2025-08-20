using System;
using System.Configuration;
using System.Data;
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
            // auth & role checks
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
            if (!HasPermissionToAdd(userId, "ManageEmployees"))
            {
                Response.Redirect("~/Unauthorized.aspx");
                return;
            }

            if (!IsPostBack)
            {
                ddlPosition.Items.Insert(0, new ListItem("Select Position", ""));
            }
        }

        private bool HasPermissionToAdd(int userId, string moduleName)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spAdminPermission_CanAdd", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@ModuleName", SqlDbType.NVarChar, 100).Value = moduleName;

                conn.Open();
                object result = cmd.ExecuteScalar();
                return result != null && Convert.ToBoolean(result);
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            int adminId = Convert.ToInt32(Session["UserID"]);
            if (!HasPermissionToAdd(adminId, "ManageEmployees"))
            {
                ShowMessage("❌ You do not have permission to add employees.", false);
                return;
            }

            // basic validation
            if (string.IsNullOrWhiteSpace(txtLastName.Text) || string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                ShowMessage("⚠ Last Name and First Name are required.", false);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtEmail.Text) ||
                !Regex.IsMatch(txtEmail.Text.Trim(), @"^[a-zA-Z0-9._%+-]+@(gmail|yahoo|outlook)\.com$", RegexOptions.IgnoreCase))
            {
                ShowMessage("⚠ Please enter a valid Gmail, Yahoo, or Outlook email address.", false);
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

            // upload
            string imagePath = string.Empty;
            if (fuProfilePicture.HasFile)
            {
                string ext = Path.GetExtension(fuProfilePicture.FileName).ToLowerInvariant();
                string[] allowed = { ".jpg", ".jpeg", ".png" };
                if (Array.IndexOf(allowed, ext) < 0)
                {
                    ShowMessage("⚠ Only JPG, JPEG, and PNG files are allowed.", false);
                    return;
                }

                string fileName = Guid.NewGuid().ToString("N") + ext;
                string folder = Server.MapPath("~/EmployeeImages/");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                fuProfilePicture.SaveAs(Path.Combine(folder, fileName));
                imagePath = "~/EmployeeImages/" + fileName;
            }
            else
            {
                ShowMessage("⚠ Please select a profile picture.", false);
                return;
            }

            try
            {
                // optional: ensure email unique
                if (IsEmployeeEmailTaken(txtEmail.Text.Trim()))
                {
                    ShowMessage("⚠ This email is already used by another employee.", false);
                    return;
                }

                int newId = InsertEmployee(
                    txtLastName.Text.Trim(),
                    txtFirstName.Text.Trim(),
                    string.IsNullOrWhiteSpace(txtMiddleName.Text) ? null : txtMiddleName.Text.Trim(),
                    txtEmail.Text.Trim(),
                    phoneNumber,
                    ddlPosition.SelectedValue,
                    imagePath);

                if (newId > 0)
                {
                    string fullName = $"{txtLastName.Text.Trim()}, {txtFirstName.Text.Trim()}"
                                      + (string.IsNullOrWhiteSpace(txtMiddleName.Text) ? "" : $" {txtMiddleName.Text.Trim()}");
                    AddAuditLog(adminId, $"Added a new employee: {fullName}");

                    ShowMessage("✅ Employee added successfully!", true);

                    // reset form
                    txtLastName.Text = txtFirstName.Text = txtMiddleName.Text = txtEmail.Text = txtPhone.Text = "";
                    ddlPosition.SelectedIndex = 0;
                }
                else
                {
                    ShowMessage("❌ Failed to add employee. Please try again.", false);
                }
            }
            catch (Exception ex)
            {
                ShowMessage("❌ Error: " + ex.Message, false);
            }
        }

        private bool IsEmployeeEmailTaken(string email)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spEmployees_EmailExists", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = email;

                conn.Open();
                object result = cmd.ExecuteScalar();
                return result != null && Convert.ToBoolean(result);
            }
        }

        private int InsertEmployee(string lastName, string firstName, string middleName,
                                   string email, string phone, string position, string profileImage)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spEmployee_Insert", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@LastName", SqlDbType.NVarChar, 100).Value = lastName;
                cmd.Parameters.Add("@FirstName", SqlDbType.NVarChar, 100).Value = firstName;
                cmd.Parameters.Add("@MiddleName", SqlDbType.NVarChar, 100).Value = (object)middleName ?? DBNull.Value;
                cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = email;
                cmd.Parameters.Add("@Phone", SqlDbType.NVarChar, 15).Value = phone;
                cmd.Parameters.Add("@Position", SqlDbType.NVarChar, 50).Value = position;
                cmd.Parameters.Add("@ProfileImage", SqlDbType.NVarChar, 255).Value = profileImage;
                cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = "Active";

                conn.Open();
                object result = cmd.ExecuteScalar(); // NewEmployeeID from SP
                return (result != null && int.TryParse(result.ToString(), out int id)) ? id : 0;
            }
        }

        private void AddAuditLog(int? userID, string action)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spAudit_Insert", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@AdminID", SqlDbType.Int).Value = (object)userID ?? DBNull.Value;
                cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 255).Value = action;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void ShowMessage(string message, bool isSuccess)
        {
            lblMessage.Text = message;
            lblMessage.ForeColor = isSuccess ? System.Drawing.Color.Green : System.Drawing.Color.Red;
        }
    }
}
