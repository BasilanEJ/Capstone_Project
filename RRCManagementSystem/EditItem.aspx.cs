using System;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class EditItem : Page
    {
        private readonly string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;
        private int itemId;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["AdminID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int adminId = Convert.ToInt32(Session["AdminID"]);

            // ✅ Check Edit Permission
            if (!HasEditPermission(adminId, "ManageItem"))
            {
                lblMessage.Text = "❌ You do not have permission to edit items.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                btnSave.Enabled = false;
                return;
            }

            if (!int.TryParse(Request.QueryString["ItemID"], out itemId))
            {
                lblMessage.Text = "⚠ Invalid Item ID.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                btnSave.Enabled = false;
                return;
            }

            if (!IsPostBack)
            {
                LoadItemDetails();
            }
        }

        private bool HasEditPermission(int adminId, string moduleName)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT CanEdit FROM AdminPermissions WHERE UserID = @AdminID AND ModuleName = @ModuleName";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@AdminID", adminId);
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

        private void LoadItemDetails()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Inventory WHERE ItemID = @ItemID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ItemID", itemId);
                    conn.Open();

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        txtItemName.Text = reader["Name"].ToString();
                        ddlType.SelectedValue = reader["Type"].ToString();
                        txtQuantity.Text = reader["Quantity"].ToString();

                        if (reader["ExpirationDate"] != DBNull.Value)
                        {
                            DateTime expDate = Convert.ToDateTime(reader["ExpirationDate"]);
                            txtExpirationDate.Text = expDate.ToString("yyyy-MM-dd");
                        }

                        if (reader["ImagePath"] != DBNull.Value)
                        {
                            imgPreview.ImageUrl = reader["ImagePath"].ToString();
                            imgPreview.Visible = true;
                        }
                    }
                    else
                    {
                        lblMessage.Text = "⚠ Item not found.";
                        lblMessage.ForeColor = System.Drawing.Color.Red;
                        btnSave.Enabled = false;
                    }
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int adminId = Convert.ToInt32(Session["AdminID"]);

            if (!HasEditPermission(adminId, "ManageItem"))
            {
                lblMessage.Text = "❌ You do not have permission to edit items.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            string itemName = txtItemName.Text.Trim();
            string itemType = ddlType.SelectedValue;
            int quantity;
            DateTime? expirationDate = null;
            string imagePath = imgPreview.ImageUrl;

            if (!int.TryParse(txtQuantity.Text.Trim(), out quantity) || quantity <= 0)
            {
                lblMessage.Text = "⚠ Please enter a valid quantity.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
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

            // ✅ Handle Image Upload
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
                    UPDATE Inventory
                    SET Name = @Name,
                        Type = @Type,
                        Quantity = @Quantity,
                        ExpirationDate = @ExpirationDate,
                        ImagePath = @ImagePath
                    WHERE ItemID = @ItemID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", itemName);
                    cmd.Parameters.AddWithValue("@Type", itemType);
                    cmd.Parameters.AddWithValue("@Quantity", quantity);
                    cmd.Parameters.AddWithValue("@ExpirationDate", (object)expirationDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ImagePath", (object)imagePath ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ItemID", itemId);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    AddAuditLog(adminId, $"Edited item (ID: {itemId}) - Name: {itemName}, Type: {itemType}, Quantity: {quantity}");
                }
            }

            lblMessage.Text = "✅ Item updated successfully!";
            lblMessage.ForeColor = System.Drawing.Color.Green;
        }

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
                        // Fail silently or handle logging errors here
                    }
                }
            }
        }
    }
}