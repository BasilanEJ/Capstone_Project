using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class EditEquipment : Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;
        private string equipmentID;

        protected void Page_Load(object sender, EventArgs e)
        {
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

            if (!HasEditPermission(userId, "ManageEquipment"))
            {
                Response.Redirect("~/Unauthorized.aspx");
                return;
            }

            string encodedId = Request.QueryString["EquipmentID"];
            equipmentID = DecodeID(encodedId);

            if (string.IsNullOrEmpty(equipmentID))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "InvalidID", "Swal.fire('Error', 'Invalid Equipment ID.', 'error')", true);
                btnUpdate.Enabled = false;
                return;
            }

            if (!IsPostBack)
            {
                LoadEquipment();
            }
        }

        private void LoadEquipment()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM EquipmentStatus WHERE EquipmentID = @EquipmentID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@EquipmentID", equipmentID);
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        txtEquipmentID.Text = reader["EquipmentID"].ToString();
                        txtEquipmentName.Text = reader["Name"].ToString();
                        ddlStatus.SelectedValue = reader["Status"].ToString();

                        if (reader["ImagePath"] != DBNull.Value)
                        {
                            imgPreview.ImageUrl = reader["ImagePath"].ToString();
                            imgPreview.Visible = true;
                        }
                    }
                    else
                    {
                        ScriptManager.RegisterStartupScript(this, GetType(), "NotFound", "Swal.fire('Error', 'Equipment not found.', 'error')", true);
                        btnUpdate.Enabled = false;
                    }
                }
            }
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            int adminId = Convert.ToInt32(Session["UserID"]);

            string name = txtEquipmentName.Text.Trim();
            string status = ddlStatus.SelectedValue;
            string imagePath = imgPreview.ImageUrl;

            if (string.IsNullOrEmpty(name))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "ErrorName", "Swal.fire('Warning', 'Please enter Equipment Name.', 'warning')", true);
                return;
            }

            if (string.IsNullOrEmpty(status))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "ErrorStatus", "Swal.fire('Warning', 'Please select Equipment Status.', 'warning')", true);
                return;
            }

            if (fuEquipmentImage.HasFile)
            {
                string fileExt = Path.GetExtension(fuEquipmentImage.FileName).ToLower();
                string[] allowed = { ".jpg", ".jpeg", ".png" };

                if (Array.IndexOf(allowed, fileExt) < 0)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "InvalidFile", "Swal.fire('Error', 'Only JPG, JPEG, PNG files allowed.', 'error')", true);
                    return;
                }

                string fileName = Guid.NewGuid() + fileExt;
                string folder = Server.MapPath("~/EquipmentImages/");
                string fullPath = Path.Combine(folder, fileName);

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                fuEquipmentImage.SaveAs(fullPath);
                imagePath = "~/EquipmentImages/" + fileName;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string updateQuery = @"
                    UPDATE EquipmentStatus 
                    SET Name = @Name, Status = @Status, ImagePath = @ImagePath
                    WHERE EquipmentID = @EquipmentID";

                using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@ImagePath", (object)imagePath ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EquipmentID", equipmentID);

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();

                        AddAuditLog(adminId, $"Edited equipment: {equipmentID} - {name}");

                        // ✅ SweetAlert Success + Redirect
                        ScriptManager.RegisterStartupScript(this, GetType(), "SuccessAlert", @"
                            Swal.fire({
                                icon: 'success',
                                title: 'Updated!',
                                text: 'Equipment updated successfully.',
                                confirmButtonColor: '#007bff'
                            }).then(() => {
                                window.location.href = 'ViewEquipment.aspx';
                            });", true);
                    }
                    catch (Exception ex)
                    {
                        ScriptManager.RegisterStartupScript(this, GetType(), "UpdateError", $"Swal.fire('Error', 'Failed to update equipment: {ex.Message}', 'error')", true);
                    }
                }
            }
        }

        // 🔒 Decode Base64 EquipmentID
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
            catch
            {
                return null;
            }
        }

        private bool HasEditPermission(int userId, string moduleName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT CanEdit FROM AdminPermissions WHERE UserID = @UserID AND ModuleName = @ModuleName";
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
                    catch
                    {
                        return false;
                    }
                }
            }
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
                        // Ignore audit failure
                    }
                }
            }
        }
    }
}
