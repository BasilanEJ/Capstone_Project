using System;
using System.Configuration;
using System.Data.SqlClient;

namespace RRCManagementSystem
{
    public partial class AddSupplier : System.Web.UI.Page
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

            // 🔐 Deny access for SuperAdmin and Inspector
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            // 🔐 Check CanAdd permission for ManageSupplier
            if (!HasAddPermission(userId, "ManageSupplier"))
            {
                Response.Redirect("~/Unauthorized.aspx");
                return;
            }

            if (!IsPostBack)
            {
                ddlStatus.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Select Status", ""));
                // ✅ Additional setup if needed
            }
        }


        private bool HasAddPermission(int adminId, string moduleName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT CanAdd FROM AdminPermissions WHERE UserID = @UserID AND ModuleName = @ModuleName";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", adminId);
                    cmd.Parameters.AddWithValue("@ModuleName", moduleName);

                    try
                    {
                        conn.Open();
                        object result = cmd.ExecuteScalar();
                        return result != null && result != DBNull.Value && Convert.ToBoolean(result);
                    }
                    catch (Exception ex)
                    {
                        DisplayMessage("❌ Permission check failed: " + ex.Message, System.Drawing.Color.Red);
                        return false;
                    }
                }
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            int adminId = Convert.ToInt32(Session["UserID"]);

            if (!HasAddPermission(adminId, "ManageSupplier"))
            {
                DisplayMessage("❌ You don't have permission to add suppliers.", System.Drawing.Color.Red);
                return;
            }

            string name = txtName.Text.Trim();
            string companyName = txtCompanyName.Text.Trim();
            string businessType = ddlBusinessType.SelectedValue;
            string address = txtAddress.Text.Trim();
            string contactNumber = txtContactNumber.Text.Trim();
            string email = txtEmail.Text.Trim();
            string status = ddlStatus.SelectedValue;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(address) || string.IsNullOrEmpty(contactNumber))
            {
                DisplayMessage("⚠ Please fill in all required fields (marked with *).", System.Drawing.Color.Red);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    INSERT INTO Supplier 
                    (Name, CompanyName, BusinessType, Address, ContactNumber, Email, Status, CreatedAt)
                    VALUES 
                    (@Name, @CompanyName, @BusinessType, @Address, @ContactNumber, @Email, @Status, GETDATE())";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@CompanyName", string.IsNullOrEmpty(companyName) ? (object)DBNull.Value : companyName);
                    cmd.Parameters.AddWithValue("@BusinessType", string.IsNullOrEmpty(businessType) ? (object)DBNull.Value : businessType);
                    cmd.Parameters.AddWithValue("@Address", address);
                    cmd.Parameters.AddWithValue("@ContactNumber", contactNumber);
                    cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email);
                    cmd.Parameters.AddWithValue("@Status", string.IsNullOrEmpty(status) ? (object)DBNull.Value : status);

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();

                        // ✅ Add Audit Log
                        AddAuditLog(adminId, $"Added new supplier: {name} ({companyName})");

                        DisplayMessage("✅ Supplier added successfully!", System.Drawing.Color.Green);
                        ClearForm();
                    }
                    catch (Exception ex)
                    {
                        DisplayMessage("⚠ Error adding supplier: " + ex.Message, System.Drawing.Color.Red);
                    }
                }
            }
        }

        private void DisplayMessage(string message, System.Drawing.Color color)
        {
            lblMessage.Text = message;
            lblMessage.ForeColor = color;
        }

        private void ClearForm()
        {
            txtName.Text = "";
            txtCompanyName.Text = "";
            txtAddress.Text = "";
            txtContactNumber.Text = "";
            txtEmail.Text = "";
            ddlStatus.SelectedIndex = 0;
            ddlBusinessType.SelectedIndex = 0;
        }

        // ✅ Add this Audit Logger Method
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
                        // Optional: silently handle logging failure
                    }
                }
            }
        }
    }
}