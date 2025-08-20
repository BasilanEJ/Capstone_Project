using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class AddItem : Page
    {
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

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

            // 🔐 Check CanAdd permission for ManageItem (via SP)
            if (!HasPermissionToAdd(userId, "ManageItem"))
            {
                Response.Redirect("~/Unauthorized.aspx");
                return;
            }

            if (!IsPostBack)
            {
                // init UI if needed
            }
        }

        private bool HasPermissionToAdd(int userId, string moduleName)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAdminPermission_Check", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@ModuleName", moduleName);
                    cmd.Parameters.AddWithValue("@Permission", "CanAdd");
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null && result != DBNull.Value && Convert.ToBoolean(result);
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "❌ Error checking permissions: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return false;
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
            if (string.IsNullOrWhiteSpace(itemName))
            {
                lblMessage.Text = "⚠ Item name is required.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            if (!int.TryParse(txtQuantity.Text.Trim(), out int quantity) || quantity <= 0)
            {
                lblMessage.Text = "⚠ Please enter a valid quantity.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            decimal excessML = 0;
            if (!string.IsNullOrWhiteSpace(txtExcessML.Text))
            {
                if (!decimal.TryParse(txtExcessML.Text.Trim(), out excessML) || excessML < 0)
                {
                    lblMessage.Text = "⚠ Please enter a valid excess value in mL.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    return;
                }
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

            // 🔎 Optional: enforce expiration requirement for chemicals
            // if ((itemType == "Bottled Chemical" || itemType == "Sachet Pack Chemical") && expirationDate == null)
            // {
            //     lblMessage.Text = "⚠ Please provide an expiration date for chemicals.";
            //     lblMessage.ForeColor = System.Drawing.Color.Red;
            //     return;
            // }

            // 📷 Save image if provided
            string imagePath = null;
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

                string fileName = Guid.NewGuid() + ext;
                string folder = Server.MapPath("~/ItemImages/");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                string full = Path.Combine(folder, fileName);
                fuItemImage.SaveAs(full);
                imagePath = "~/ItemImages/" + fileName;
            }

            // 💾 Insert via stored procedure
            int newItemId = 0;
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spInventory_Add", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

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

                    var pExcess = cmd.Parameters.Add("@ExcessML", SqlDbType.Decimal);
                    pExcess.Precision = 10;
                    pExcess.Scale = 2;
                    pExcess.Value = excessML;

                    var pOut = cmd.Parameters.Add("@NewItemID", SqlDbType.Int);
                    pOut.Direction = ParameterDirection.Output;

                    conn.Open();
                    // Option 1: rely on OUTPUT param
                    cmd.ExecuteNonQuery();
                    newItemId = (pOut.Value == DBNull.Value) ? 0 : Convert.ToInt32(pOut.Value);
                }

                // 🧾 Audit
                InsertAudit(currentUserId, $"Added new item: {itemName} ({itemType}), ID={newItemId}");

                lblMessage.Text = "✅ Item added successfully!";
                lblMessage.ForeColor = System.Drawing.Color.Green;

                // Clear fields
                txtItemName.Text = "";
                ddlType.SelectedIndex = 0;
                txtQuantity.Text = "";
                txtExpirationDate.Text = "";
                txtExcessML.Text = "";
            }
            catch (Exception ex)
            {
                lblMessage.Text = "❌ Failed to add item: " + ex.Message;
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
                // swallow/log as desired
            }
        }
    }
}
