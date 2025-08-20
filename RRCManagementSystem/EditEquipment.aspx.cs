using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls; // HtmlImage

namespace RRCManagementSystem
{
    public partial class EditEquipment : System.Web.UI.Page
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
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);
            if (!HasEditPermission(userId, "ManageEquipment"))
            {
                lblMessage.Text = "❌ You do not have permission to edit equipment.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                btnUpdate.Enabled = false;
                return;
            }

            if (!IsPostBack)
            {
                // Populate status dropdown if not in markup
                if (ddlStatus.Items.Count == 0)
                {
                    ddlStatus.Items.Add("Available");
                    ddlStatus.Items.Add("Unavailable");
                }

                if (!TryGetEquipmentId(out var eqId))
                {
                    lblMessage.Text = "Invalid Equipment ID.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    btnUpdate.Enabled = false;
                    return;
                }

                // (Optional) make ID read-only if it’s an input control
                txtEquipmentID.ReadOnly = true;
                txtEquipmentID.Text = eqId.ToString();

                LoadEquipment(eqId);
            }
        }

        private bool HasEditPermission(int userId, string moduleName)
        {
            try
            {
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAdminPermission_Check", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@ModuleName", moduleName);
                    cmd.Parameters.AddWithValue("@Permission", "CanEdit");

                    con.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null && result != DBNull.Value && Convert.ToBoolean(result);
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the EquipmentID from the query string (numeric or Base64-url encoded).
        /// Accepts ?id=, ?EquipmentID=, or ?eid=.
        /// </summary>
        private bool TryGetEquipmentId(out int eqId)
        {
            string raw = Request.QueryString["id"]
                      ?? Request.QueryString["EquipmentID"]
                      ?? Request.QueryString["eid"];

            if (string.IsNullOrWhiteSpace(raw))
            {
                eqId = 0;
                return false;
            }

            // Plain numeric?
            if (int.TryParse(raw, out eqId))
                return true;

            // Decode Base64-url → Base64
            try
            {
                string b64 = raw.Replace("-", "+").Replace("_", "/");
                switch (b64.Length % 4)
                {
                    case 2: b64 += "=="; break;
                    case 3: b64 += "="; break;
                }
                string decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(b64));
                return int.TryParse(decoded, out eqId);
            }
            catch
            {
                eqId = 0;
                return false;
            }
        }

        private void LoadEquipment(int equipmentId)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spEquipment_GetById", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EquipmentID", equipmentId);

                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        txtEquipmentName.Text = rdr["Name"].ToString();

                        var status = rdr["Status"].ToString();
                        if (ddlStatus.Items.FindByValue(status) != null)
                            ddlStatus.SelectedValue = status;

                        imagePreview.Src = rdr["ImagePath"] == DBNull.Value ? "" : rdr["ImagePath"].ToString();
                    }
                    else
                    {
                        lblMessage.Text = "Equipment not found.";
                        lblMessage.ForeColor = System.Drawing.Color.Red;
                        btnUpdate.Enabled = false;
                    }
                }
            }
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtEquipmentID.Text, out int id))
            {
                lblMessage.Text = "Invalid Equipment ID.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            string name = txtEquipmentName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                lblMessage.Text = "Please enter equipment name.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            string status = ddlStatus.SelectedValue;
            if (string.IsNullOrWhiteSpace(status))
            {
                lblMessage.Text = "Please select status.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            // Optional image upload
            string imagePath = imagePreview.Src;
            if (fuEquipmentImage.HasFile)
            {
                string ext = Path.GetExtension(fuEquipmentImage.FileName).ToLowerInvariant();
                if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                {
                    lblMessage.Text = "Only JPG, JPEG, and PNG are allowed.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                string folder = Server.MapPath("~/Uploads/Equipment/");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                string fileName = $"Equipment_{id}_{Guid.NewGuid():N}{ext}";
                fuEquipmentImage.SaveAs(Path.Combine(folder, fileName));
                imagePath = "~/Uploads/Equipment/" + fileName;
            }

            try
            {
                int affected = 0;
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spEquipment_Update", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EquipmentID", id);
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Status", status);
                    if (!string.IsNullOrWhiteSpace(imagePath))
                        cmd.Parameters.AddWithValue("@ImagePath", imagePath);
                    else
                        cmd.Parameters.AddWithValue("@ImagePath", DBNull.Value);

                    con.Open();
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                            affected = Convert.ToInt32(rdr["Affected"]);
                    }
                }

                if (affected == 1)
                {
                    InsertAudit(Convert.ToInt32(Session["UserID"]),
                        $"Updated equipment ID={id}: Name='{name}', Status='{status}'");

                    ScriptManager.RegisterStartupScript(this, GetType(), "ok", @"
                        Swal.fire({
                            icon:'success', title:'Updated!', text:'Equipment has been updated successfully.',
                            confirmButtonColor:'#007bff'
                        }).then(()=>{ window.location.href='ViewEquipment.aspx'; });
                    ", true);
                }
                else
                {
                    lblMessage.Text = "Update failed. Please try again.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Update error: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void InsertAudit(int? adminId, string action)
        {
            try
            {
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAudit_Insert", con))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AdminID", (object)adminId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Action", action);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch { /* optional logging */ }
        }
    }
}
