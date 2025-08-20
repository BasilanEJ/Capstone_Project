using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class SystemAdmin : System.Web.UI.Page
    {
        private readonly string cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // We do NOT disable the page even if a SuperAdmin exists.
                lblMessage.Text = "";
            }
        }

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            Page.Validate();
            if (!Page.IsValid) return;

            var email = (txtEmail.Text ?? "").Trim().ToLowerInvariant();
            var password = txtPassword.Text ?? "";
            var confirm = txtConfirm.Text ?? "";

            if (password != confirm)
            {
                ShowSweetAlert("error", "Password Mismatch", "Passwords do not match.");
                return;
            }

            if (EmailExists(email))
            {
                ShowSweetAlert("error", "Duplicate Email", "That email is already used.");
                return;
            }

            try
            {
                // Argon2id hash
                string hash = PasswordHelper.HashPassword(password);

                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spCreateSuperAdmin", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 256).Value = email;
                    cmd.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, -1).Value = hash;
                    cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 150).Value = "Super Admin";

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                // 🎉 Success with SweetAlert and redirect
                string script = @"
            Swal.fire({
                icon: 'success',
                title: 'SuperAdmin Created!',
                text: 'You will be redirected to Login.',
                confirmButtonText: 'OK'
            }).then(function() {
                window.location = 'Login.aspx';
            });";

                ScriptManager.RegisterStartupScript(this, GetType(), "SweetAlertSuccess", script, true);
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                ShowSweetAlert("error", "Duplicate Email", "That email already exists.");
            }
            catch (Exception ex)
            {
                ShowSweetAlert("error", "Error", "Error creating SuperAdmin: " + ex.Message);
            }
        }

        private void ShowSweetAlert(string icon, string title, string text)
        {
            string script = $@"
        Swal.fire({{
            icon: '{icon}',
            title: '{title}',
            text: '{text}'
        }});";
            ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(), script, true);
        }


        private bool EmailExists(string email)
        {
            const string sql = "SELECT TOP 1 1 FROM dbo.Users WHERE Email = @e;";
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@e", email);
                con.Open();
                var o = cmd.ExecuteScalar();
                return o != null;
            }
        }
    }
}
