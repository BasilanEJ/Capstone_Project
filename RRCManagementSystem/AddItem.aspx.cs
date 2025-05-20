using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class AddItem : Page
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

            // 🔐 Check CanAdd permission for ManageItem
            if (!HasPermissionToAdd(userId, "ManageItem"))
            {
                Response.Redirect("~/Unauthorized.aspx");
                return;
            } 

            if (!IsPostBack)
            {
                // ✅ Page logic here (e.g., populate fields, setup UI)
            }
        }


        private bool HasPermissionToAdd(int userId, string moduleName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT CanAdd FROM AdminPermissions WHERE UserID = @UserID AND ModuleName = @ModuleName";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@ModuleName", moduleName);

                try
                {
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null && result != DBNull.Value && Convert.ToBoolean(result);
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "❌ Error checking permissions: " + ex.Message;
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    return false;
                }
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            int currentUserId = Convert.ToInt32(Session["UserID"]);
            if (!HasPermissionToAdd(currentUserId, "ManageItem"))
            {
                lblMessage.Text = "❌ You do not have permission to add items.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            string itemName = txtItemName.Text.Trim();
            string itemType = ddlType.SelectedValue;
            int quantity;
            decimal excessML = 0;
            DateTime? expirationDate = null;
            string imagePath = null;

            if (!int.TryParse(txtQuantity.Text.Trim(), out quantity) || quantity <= 0)
            {
                lblMessage.Text = "⚠ Please enter a valid quantity.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            if (!string.IsNullOrEmpty(txtExcessML.Text))
            {
                if (!decimal.TryParse(txtExcessML.Text.Trim(), out excessML) || excessML < 0)
                {
                    lblMessage.Text = "⚠ Please enter a valid excess value in mL.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    return;
                }
            }

            if (!string.IsNullOrEmpty(txtExpirationDate.Text))
            {
                if (!DateTime.TryParse(txtExpirationDate.Text, out DateTime parsedDate))
                {
                    lblMessage.Text = "⚠ Invalid expiration date.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    return;
                }
                expirationDate = parsedDate;
            }

            if (fuItemImage.HasFile)
            {
                string fileExtension = Path.GetExtension(fuItemImage.FileName).ToLower();
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };

                if (!Array.Exists(allowedExtensions, ext => ext == fileExtension))
                {
                    lblMessage.Text = "⚠ Only JPG, JPEG, and PNG files are allowed.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                string fileName = Guid.NewGuid().ToString() + fileExtension;
                string folderPath = Server.MapPath("~/ItemImages/");
                string fullPath = Path.Combine(folderPath, fileName);

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                fuItemImage.SaveAs(fullPath);
                imagePath = "~/ItemImages/" + fileName;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    INSERT INTO Inventory (Name, Type, Quantity, ExpirationDate, CreatedAt, ImagePath, ExcessML)
                    VALUES (@Name, @Type, @Quantity, @ExpirationDate, GETDATE(), @ImagePath, @ExcessML)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Name", itemName);
                cmd.Parameters.AddWithValue("@Type", itemType);
                cmd.Parameters.AddWithValue("@Quantity", quantity);
                cmd.Parameters.AddWithValue("@ExpirationDate", (object)expirationDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ImagePath", (object)imagePath ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ExcessML", excessML);

                conn.Open();
                cmd.ExecuteNonQuery();

                // ✅ Insert audit log
                AddAuditLog(currentUserId, $"Added new item: {itemName} ({itemType})");
            }

            lblMessage.Text = "✅ Item added successfully!";
            lblMessage.ForeColor = System.Drawing.Color.Green;

            // Clear fields
            txtItemName.Text = "";
            ddlType.SelectedIndex = 0;
            txtQuantity.Text = "";
            txtExpirationDate.Text = "";
            txtExcessML.Text = "";
        }

        // ✅ Manual Audit Log Method
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
                        // Optional: handle/log silently
                    }
                }
            }
        }
    }
}