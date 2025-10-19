using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class AddEquipment : Page
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

            // 🔐 Check CanAdd permission for ManageEquipment (via SP)
            if (!HasPermissionToAdd(userId, "ManageEquipment"))
            {
                Response.Redirect("~/Unauthorized.aspx");
                return;
            }

            if (!IsPostBack)
            {
                ddlStatus.Items.Clear();
                ddlStatus.Items.Insert(0, new ListItem("Select Status", ""));
                ddlStatus.Items.Insert(1, new ListItem("Available", "Available"));
                ddlStatus.Items.Insert(2, new ListItem("Unavailable", "Unavailable"));
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
            int adminId = Convert.ToInt32(Session["UserID"]);

            if (!HasPermissionToAdd(adminId, "ManageEquipment"))
            {
                ShowMessage("error", "Permission Denied", "You do not have permission to add equipment.");
                return;
            }

            // ✅ Validate inputs
            if (!int.TryParse(txtEquipmentID.Text.Trim(), out int equipmentID) || equipmentID <= 0)
            {
                ShowMessage("warning", "Invalid Input", "Please enter a valid numeric Equipment ID.");
                return;
            }

            string equipmentName = txtEquipmentName.Text.Trim();
            if (string.IsNullOrWhiteSpace(equipmentName))
            {
                ShowMessage("warning", "Invalid Input", "Please enter Equipment Name.");
                return;
            }

            string status = ddlStatus.SelectedValue;
            if (string.IsNullOrEmpty(status))
            {
                ShowMessage("warning", "Invalid Input", "Please select Equipment Status.");
                return;
            }

            // 📷 Optional image upload
            string imagePath = null;
            if (fuEquipmentImage.HasFile)
            {
                string ext = Path.GetExtension(fuEquipmentImage.FileName).ToLowerInvariant();
                string[] allowed = { ".jpg", ".jpeg", ".png" };
                if (Array.IndexOf(allowed, ext) < 0)
                {
                    ShowMessage("error", "Invalid File", "Only JPG, JPEG, and PNG files are allowed.");
                    return;
                }

                string folder = Server.MapPath("~/EquipmentImages/");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                string fileName = Guid.NewGuid() + ext;
                fuEquipmentImage.SaveAs(Path.Combine(folder, fileName));
                imagePath = "~/EquipmentImages/" + fileName;
            }

            // 💾 Save via stored procedure
            try
            {
                int affected = 0;
                string reason = "";

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spEquipment_Add", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EquipmentID", equipmentID);
                    cmd.Parameters.AddWithValue("@Name", equipmentName);
                    cmd.Parameters.AddWithValue("@Status", status);
                    if (!string.IsNullOrWhiteSpace(imagePath))
                        cmd.Parameters.AddWithValue("@ImagePath", imagePath);
                    else
                        cmd.Parameters.AddWithValue("@ImagePath", DBNull.Value);

                    conn.Open();
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            affected = Convert.ToInt32(rdr["Affected"]);
                            reason = rdr["Reason"].ToString();
                        }
                    }
                }

                if (affected == 1)
                {
                    // 🧾 Audit
                    InsertAudit(adminId, $"Added new equipment: {equipmentID} - {equipmentName}");

                    // Clear form fields
                    txtEquipmentID.Text = "";
                    txtEquipmentName.Text = "";
                    ddlStatus.SelectedIndex = 0;

                    // Clear image preview via JavaScript
                    ScriptManager.RegisterStartupScript(this, GetType(), "clearImage",
                        "document.getElementById('imagePreview').classList.add('hidden'); " +
                        "document.getElementById('imagePreview').classList.remove('block');", true);

                    ShowMessage("success", "Success!", "Equipment added successfully.");
                }
                else
                {
                    if (reason == "Duplicate")
                        ShowMessage("error", "Duplicate Entry", "Equipment ID already exists.");
                    else
                        ShowMessage("error", "Failed", "Insert operation failed.");
                }
            }
            catch (Exception ex)
            {
                ShowMessage("error", "Error", $"Error adding equipment: {ex.Message}");
            }
        }

        private void ShowMessage(string icon, string title, string text)
        {
            string script = $@"
                Swal.fire({{
                    icon: '{icon}',
                    title: '{title}',
                    text: '{text.Replace("'", "\\'")}',
                    showConfirmButton: true,
                    confirmButtonColor: '#007bff'
                }});";

            ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(), script, true);
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
                // optional logging
            }
        }
    }
}