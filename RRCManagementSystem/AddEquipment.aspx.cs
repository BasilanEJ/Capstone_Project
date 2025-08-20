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
                lblMessage.Text = "❌ You do not have permission to add equipment.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            // ✅ Validate inputs
            if (!int.TryParse(txtEquipmentID.Text.Trim(), out int equipmentID) || equipmentID <= 0)
            {
                lblMessage.Text = "⚠ Please enter a valid numeric Equipment ID.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            string equipmentName = txtEquipmentName.Text.Trim();
            if (string.IsNullOrWhiteSpace(equipmentName))
            {
                lblMessage.Text = "⚠ Please enter Equipment Name.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            string status = ddlStatus.SelectedValue;
            if (string.IsNullOrEmpty(status))
            {
                lblMessage.Text = "⚠ Please select Equipment Status.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
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
                    lblMessage.Text = "⚠ Only JPG, JPEG, and PNG files are allowed.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
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

                    lblMessage.Text = "✅ Equipment added successfully!";
                    lblMessage.ForeColor = System.Drawing.Color.Green;

                    txtEquipmentID.Text = "";
                    txtEquipmentName.Text = "";
                    ddlStatus.SelectedIndex = 0;
                }
                else
                {
                    lblMessage.Text = (reason == "Duplicate")
                        ? "⚠ Equipment ID already exists."
                        : "❌ Insert failed.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "❌ Error adding equipment: " + ex.Message;
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
                // optional logging
            }
        }
    }
}
    