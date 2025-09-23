using RRCManagementSystem.Helpers; // For AESHelper and PasswordHelper
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

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
                if (Session["UserID"] == null ||
                    !string.Equals(Session["Role"]?.ToString(), "SuperAdmin", StringComparison.OrdinalIgnoreCase))
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

                            // Decrypt email before showing it in UI
                            string encryptedEmail = rdr["Email"].ToString();
                            txtEmail.Text = AESHelper.DecryptEmail(encryptedEmail);
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
            if (Session["UserID"] == null ||
                !string.Equals(Session["Role"]?.ToString(), "SuperAdmin", StringComparison.OrdinalIgnoreCase))
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
                ShowSweetAlert("Error", "Name and Email cannot be empty.", "error");
                return;
            }

            // Encrypt and hash email
            string encryptedEmail = AESHelper.EncryptEmail(newEmail);
            string emailHash = AESHelper.ComputeSHA256(newEmail);

            // Hash password only if a new password was entered
            string hashedPassword = string.IsNullOrEmpty(newPw) ? null : PasswordHelper.HashPassword(newPw);

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spSuperAdmin_UpdateProfile", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = superAdminId;
                cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = newName;
                cmd.Parameters.Add("@Email", SqlDbType.NVarChar, -1).Value = encryptedEmail;
                cmd.Parameters.Add("@EmailHash", SqlDbType.Char, 64).Value = emailHash;

                var pHash = cmd.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, -1);
                pHash.Value = (object)hashedPassword ?? DBNull.Value;

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();

                    // Log the update
                    TryAudit(superAdminId, "SuperAdmin updated own profile.");

                    // Clear password box
                    txtNewPassword.Text = string.Empty;

                    // ✅ Success popup
                    ShowSweetAlert("Profile Updated", "Your profile has been updated successfully!", "success");
                }
                catch (Exception ex)
                {
                    // ❌ Error popup
                    ShowSweetAlert("Error Saving Profile", ex.Message, "error");
                }
            }
        }

        private void ShowSweetAlert(string title, string message, string icon)
        {
            string script = $@"
        Swal.fire({{
            title: '{title}',
            text: '{message}',
            icon: '{icon}',
            confirmButtonColor: '#3085d6',
            confirmButtonText: 'OK'
        }});
    ";

            ScriptManager.RegisterStartupScript(this, GetType(), "SweetAlert", script, true);
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
