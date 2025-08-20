using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace RRCManagementSystem
{
    public partial class EditServices : System.Web.UI.Page
    {
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        private int ServiceID
        {
            get
            {
                int id;
                return int.TryParse(Request.QueryString["ServiceID"], out id) ? id : 0;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // 🔐 Require login
            if (Session["UserID"] == null || Session["Role"] == null)
            { Response.Redirect("~/Login.aspx"); return; }

            string role = Convert.ToString(Session["Role"]);
            // 🔐 Deny SuperAdmin & Inspector
            if (role == "SuperAdmin" || role == "Inspector")
            { Response.Redirect("~/Login.aspx"); return; }

            int userId = Convert.ToInt32(Session["UserID"]);
            if (!HasEditPermission(userId, "ManageServices"))
            {
                lblMessage.Text = "❌ You do not have permission to edit services.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                btnUpdate.Enabled = false;
                return;
            }

            if (!IsPostBack)
            {
                if (ServiceID > 0)
                {
                    LoadService(ServiceID);
                }
                else
                {
                    lblMessage.Text = "⚠ Invalid or missing Service ID.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    btnUpdate.Enabled = false;
                }
            }
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

        private void LoadService(int serviceID)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spService_GetById", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ServiceID", serviceID);

                conn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        txtName.Text = Convert.ToString(rdr["Name"]);
                        txtDescription.Text = rdr["Description"] as string ?? "";
                    }
                    else
                    {
                        lblMessage.Text = "⚠ Service not found.";
                        lblMessage.ForeColor = System.Drawing.Color.Red;
                        btnUpdate.Enabled = false;
                    }
                }
            }
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            int adminId = Convert.ToInt32(Session["UserID"]);
            if (!HasEditPermission(adminId, "ManageServices"))
            {
                lblMessage.Text = "❌ You do not have permission to edit services.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            if (ServiceID <= 0)
            {
                lblMessage.Text = "⚠ Invalid Service ID.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            string name = txtName.Text.Trim();
            string description = txtDescription.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                lblMessage.Text = "⚠ Service name is required.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            int affected = 0;
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spService_Update", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ServiceID", ServiceID);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description);

                conn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                        affected = Convert.ToInt32(rdr["Affected"]);
                }
            }

            if (affected == 1)
            {
                // Audit
                using (var conn = new SqlConnection(connectionString))
                using (var a = new SqlCommand("dbo.spAudit_Insert", conn))
                {
                    a.CommandType = CommandType.StoredProcedure;
                    a.Parameters.AddWithValue("@AdminID", (object)adminId ?? DBNull.Value);
                    a.Parameters.AddWithValue("@Action", $"Edited service (ID: {ServiceID}) - Name: {name}");
                    conn.Open();
                    a.ExecuteNonQuery();
                }

                lblMessage.Text = "✅ Service updated successfully.";
                lblMessage.ForeColor = System.Drawing.Color.Green;
            }
            else
            {
                lblMessage.Text = "⚠ Update failed. Service not found or no changes.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }
    }
}
