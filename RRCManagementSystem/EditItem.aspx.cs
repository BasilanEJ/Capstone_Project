using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class EditItem : Page
    {
        private readonly string connectionString =
            System.Configuration.ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        private int itemId;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 🔐 Require login
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string role = Session["Role"].ToString();

            // 🔐 Block SuperAdmin and Inspector
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            // ✅ Check Edit Permission (via SP)
            if (!HasEditPermission(userId, "ManageItem"))
            {
                lblMessage.Text = "❌ You do not have permission to edit items.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                btnSave.Enabled = false;
                return;
            }

            // ✅ Decode ItemID from query
            string encodedId = Request.QueryString["ItemID"];
            if (string.IsNullOrWhiteSpace(encodedId))
            {
                lblMessage.Text = "⚠ Missing Item ID.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                btnSave.Enabled = false;
                return;
            }

            string decodedId = DecodeID(encodedId);
            if (!int.TryParse(decodedId, out itemId))
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

        private string DecodeID(string encoded)
        {
            try
            {
                string padded = encoded.Replace("-", "+").Replace("_", "/");
                switch (padded.Length % 4)
                {
                    case 2: padded += "=="; break;
                    case 3: padded += "="; break;
                }
                byte[] data = Convert.FromBase64String(padded);
                return System.Text.Encoding.UTF8.GetString(data);
            }
            catch { return null; }
        }

        private bool HasEditPermission(int adminId, string moduleName)
        {
            try
            {
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAdminPermission_Check", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", adminId);
                    cmd.Parameters.AddWithValue("@ModuleName", moduleName);
                    cmd.Parameters.AddWithValue("@Permission", "CanEdit");

                    con.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null && result != DBNull.Value && Convert.ToBoolean(result);
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"❌ Permission check failed: {ex.Message}";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return false;
            }
        }

        private void LoadItemDetails()
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spInventory_GetById", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ItemID", itemId);

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        txtItemName.Text = reader["Name"].ToString();
                        ddlType.SelectedValue = reader["Type"].ToString();
                        txtQuantity.Text = reader["Quantity"].ToString();

                        if (reader["ExpirationDate"] != DBNull.Value)
                        {
                            DateTime exp = Convert.ToDateTime(reader["ExpirationDate"]);
                            txtExpirationDate.Text = exp.ToString("yyyy-MM-dd");
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
            int adminId = Convert.ToInt32(Session["UserID"]);

            if (!HasEditPermission(adminId, "ManageItem"))
            {
                lblMessage.Text = "❌ You do not have permission to edit items.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            string itemName = txtItemName.Text.Trim();
            string itemType = ddlType.SelectedValue;

            if (!int.TryParse(txtQuantity.Text.Trim(), out int quantity) || quantity <= 0)
            {
                lblMessage.Text = "⚠ Please enter a valid quantity.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            DateTime? expirationDate = null;
            if (!string.IsNullOrWhiteSpace(txtExpirationDate.Text))
            {
                if (!DateTime.TryParse(txtExpirationDate.Text, out DateTime parsed))
                {
                    lblMessage.Text = "⚠ Invalid expiration date.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    return;
                }
                expirationDate = parsed;
            }

            // 📷 Image upload (optional)
            string imagePath = imgPreview.ImageUrl;
            if (fuItemImage.HasFile)
            {
                string ext = Path.GetExtension(fuItemImage.FileName).ToLowerInvariant();
                string[] allowed = { ".jpg", ".jpeg", ".png" };
                if (Array.IndexOf(allowed, ext) < 0)
                {
                    lblMessage.Text = "⚠ Only JPG, JPEG, and PNG files are allowed.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                string folder = Server.MapPath("~/ItemImages/");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                string fileName = Guid.NewGuid() + ext;
                string full = Path.Combine(folder, fileName);
                fuItemImage.SaveAs(full);
                imagePath = "~/ItemImages/" + fileName;
            }

            try
            {
                int affected = 0;
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spInventory_Update", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@ItemID", SqlDbType.Int).Value = itemId;
                    cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = itemName;
                    cmd.Parameters.Add("@Type", SqlDbType.NVarChar, 50).Value = itemType;
                    cmd.Parameters.Add("@Quantity", SqlDbType.Int).Value = quantity;

                    if (expirationDate.HasValue)
                        cmd.Parameters.Add("@ExpirationDate", SqlDbType.DateTime).Value = expirationDate.Value;
                    else
                        cmd.Parameters.Add("@ExpirationDate", SqlDbType.DateTime).Value = DBNull.Value;

                    if (!string.IsNullOrWhiteSpace(imagePath))
                        cmd.Parameters.Add("@ImagePath", SqlDbType.NVarChar, 255).Value = imagePath;
                    else
                        cmd.Parameters.Add("@ImagePath", SqlDbType.NVarChar, 255).Value = DBNull.Value;

                    conn.Open();
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                            affected = Convert.ToInt32(rdr["Affected"]);
                    }
                }

                if (affected == 1)
                {
                    // 🧾 Audit
                    InsertAudit(adminId, $"Edited item (ID: {itemId}) - Name: {itemName}, Type: {itemType}, Qty: {quantity}");
                    lblMessage.Text = "✅ Item updated successfully!";
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                }
                else
                {
                    lblMessage.Text = "❌ Update failed or item not found.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "❌ Update error: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void InsertAudit(int? adminId, string action)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAudit_Insert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AdminID", (object)adminId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Action", action);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                // swallow or log
            }
        }
    }
}
