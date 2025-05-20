using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class AddEquipment : Page
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

            // 🔐 Check CanAdd permission for ManageEquipment
            if (!HasPermissionToAdd(userId, "ManageEquipment"))
            {
                Response.Redirect("~/Unauthorized.aspx");
                return;
            }

            if (!IsPostBack)
            {
                // ✅ Populate status dropdown
                ddlStatus.Items.Clear();
                ddlStatus.Items.Insert(0, new ListItem("Select Status", ""));
                ddlStatus.Items.Insert(1, new ListItem("Available", "Available"));
                ddlStatus.Items.Insert(2, new ListItem("Unavailable", "Unavailable"));
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
                        lblMessage.Text = "❌ Error checking permissions: " + ex.Message;
                        lblMessage.ForeColor = System.Drawing.Color.Red;
                        return false;
                    }
                }
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            int adminId = Convert.ToInt32(Session["UserID"]);

            if (!HasPermissionToAdd(adminId, "ManageEquipment"))
            {
                lblMessage.Text = "❌ You do not have permission to add equipment.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            string equipmentID = txtEquipmentID.Text.Trim();
            string equipmentName = txtEquipmentName.Text.Trim();
            string status = ddlStatus.SelectedValue;
            string imagePath = "";

            if (string.IsNullOrEmpty(equipmentID))
            {
                lblMessage.Text = "⚠ Please enter an Equipment ID.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            if (string.IsNullOrEmpty(equipmentName))
            {
                lblMessage.Text = "⚠ Please enter Equipment Name.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            if (string.IsNullOrEmpty(status))
            {
                lblMessage.Text = "⚠ Please select Equipment Status.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            if (fuEquipmentImage.HasFile)
            {
                string fileExtension = Path.GetExtension(fuEquipmentImage.FileName).ToLower();
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };

                if (!Array.Exists(allowedExtensions, ext => ext == fileExtension))
                {
                    lblMessage.Text = "⚠ Only JPG, JPEG, and PNG files are allowed.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                string fileName = Guid.NewGuid().ToString() + fileExtension;
                string folderPath = Server.MapPath("~/EquipmentImages/");
                string fullPath = Path.Combine(folderPath, fileName);

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                fuEquipmentImage.SaveAs(fullPath);
                imagePath = "~/EquipmentImages/" + fileName;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    INSERT INTO EquipmentStatus 
                    (EquipmentID, Name, Status, ImagePath, CreatedAt)
                    VALUES 
                    (@EquipmentID, @Name, @Status, @ImagePath, GETDATE())";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@EquipmentID", equipmentID);
                    cmd.Parameters.AddWithValue("@Name", equipmentName);
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@ImagePath", (object)imagePath ?? DBNull.Value);

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();

                        lblMessage.Text = "✅ Equipment added successfully!";
                        lblMessage.ForeColor = System.Drawing.Color.Green;

                        // ✅ Add Audit Log manually
                        AddAuditLog(adminId, $"Added new equipment: {equipmentID} - {equipmentName}");

                        // ✅ Clear fields
                        txtEquipmentID.Text = "";
                        txtEquipmentName.Text = "";
                        ddlStatus.SelectedIndex = 0;
                    }
                    catch (Exception ex)
                    {
                        lblMessage.Text = "❌ Error adding equipment: " + ex.Message;
                        lblMessage.ForeColor = System.Drawing.Color.Red;
                    }
                }
            }
        }

        // ✅ Manual Audit Logger
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
                        // You can optionally log or suppress errors here
                    }
                }
            }
        }
    }
}