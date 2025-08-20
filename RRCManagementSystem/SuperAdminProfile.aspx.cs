using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using RRCManagementSystem.Helpers; // PasswordHelper

namespace RRCManagementSystem
{
    public partial class SuperAdminProfile : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Only logged-in SuperAdmin can access
                if (Session["UserID"] == null || !string.Equals(Session["Role"]?.ToString(), "SuperAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    Response.Redirect("~/Login.aspx");
                    return;
                }

                LoadProfile();
            }
        }

        private void LoadProfile()
        {
            int superAdminId = Convert.ToInt32(Session["UserID"]);

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spSuperAdmin_GetProfile", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = superAdminId;

                try
                {
                    conn.Open();
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            txtName.Text = rdr["Name"].ToString();
                            txtEmail.Text = rdr["Email"].ToString();
                        }
                        else
                        {
                            lblMessage.Text = "⚠ SuperAdmin not found.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "⚠ Error loading profile: " + ex.Message;
                }
            }
        }

        protected void btnSaveProfile_Click(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || !string.Equals(Session["Role"]?.ToString(), "SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int superAdminId = Convert.ToInt32(Session["UserID"]);
            string newName = (txtName.Text ?? "").Trim();
            string newEmail = (txtEmail.Text ?? "").Trim();
            string newPw = (txtNewPassword.Text ?? "").Trim();

            if (string.IsNullOrEmpty(newName) || string.IsNullOrEmpty(newEmail))
            {
                lblMessage.Text = "⚠ Name and Email cannot be empty.";
                return;
            }

            // Only hash if a new password was entered
            string hashed = string.IsNullOrEmpty(newPw) ? null : PasswordHelper.HashPassword(newPw);

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spSuperAdmin_UpdateProfile", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = superAdminId;
                cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = newName;
                cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = newEmail;

                var pHash = cmd.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, -1);
                pHash.Value = (object)hashed ?? DBNull.Value;

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();

                    // Optional: log it
                    TryAudit(superAdminId, "SuperAdmin updated own profile.");

                    lblMessage.CssClass = "alert success";
                    lblMessage.Text = "✅ Profile updated successfully!";
                    // Clear password box
                    txtNewPassword.Text = string.Empty;
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "⚠ Error saving profile: " + ex.Message;
                }
            }
        }

        private void TryAudit(int userId, string action)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAudit_Insert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@AdminID", SqlDbType.Int).Value = userId;
                    cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 255).Value = action;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                // non-blocking: ignore audit failures
            }
        }
    }
}
