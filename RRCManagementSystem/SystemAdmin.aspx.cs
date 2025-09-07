using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using Isopoh.Cryptography.Argon2;

namespace RRCManagementSystem
{
    public partial class SystemAdmin : System.Web.UI.Page
    {
        private readonly string cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (RootAdminExists())
                {
                    DisableForm();
                    ShowSweetAlert("info", "Setup Completed", "A Root Admin account already exists. This page is disabled.");
                }
                else
                {
                    lblMessage.Text = "";
                }
            }
        }

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            Page.Validate();
            if (!Page.IsValid) return;

            string email = txtEmail.Text.Trim().ToLowerInvariant();
            string password = txtPassword.Text.Trim();
            string confirm = txtConfirm.Text.Trim();

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
                string passwordHash = PasswordHelper.HashPassword(password); // Use Argon2 or your helper

                using (SqlConnection con = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand("dbo.spCreateRootAdmin", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = "Root Admin";
                    cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = email;
                    cmd.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, -1).Value = passwordHash;
                    cmd.Parameters.Add("@RoleName", SqlDbType.NVarChar, 100).Value = "RootAdmin";

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                ShowSweetAlert("success", "Root Admin Created!", "You will be redirected to Login.", "Login.aspx");

                ClearForm();
                DisableForm();
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                ShowSweetAlert("error", "Duplicate Email", "That email already exists.");
            }
            catch (Exception ex)
            {
                ShowSweetAlert("error", "Error", "Error creating RootAdmin: " + ex.Message);
            }
        }

        private void ClearForm()
        {
            txtEmail.Text = string.Empty;
            txtPassword.Text = string.Empty;
            txtConfirm.Text = string.Empty;

            rfvEmail.IsValid = true;
            revEmail.IsValid = true;
            rfvPass.IsValid = true;
            revPass.IsValid = true;
            rfvConfirm.IsValid = true;
            cmpPass.IsValid = true;
        }

        private void ShowSweetAlert(string icon, string title, string text, string redirectUrl = null)
        {
            string script;
            if (redirectUrl != null)
            {
                script = $@"
                    Swal.fire({{
                        icon: '{icon}',
                        title: '{title}',
                        text: '{text}',
                        confirmButtonText: 'OK'
                    }}).then(function() {{
                        window.location = '{redirectUrl}';
                    }});";
            }
            else
            {
                script = $@"
                    Swal.fire({{
                        icon: '{icon}',
                        title: '{title}',
                        text: '{text}'
                    }});";
            }
            ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(), script, true);
        }

        private bool EmailExists(string email)
        {
            const string sql = "SELECT TOP 1 1 FROM dbo.Users WHERE Email = @e;";
            using (SqlConnection con = new SqlConnection(cs))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@e", email);
                con.Open();
                return cmd.ExecuteScalar() != null;
            }
        }

        private bool RootAdminExists()
        {
            const string sql = @"
                SELECT TOP 1 1 
                FROM dbo.Users U
                INNER JOIN dbo.Roles R ON U.RoleID = R.RoleID
                WHERE R.RoleName = 'RootAdmin';";

            using (SqlConnection con = new SqlConnection(cs))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                con.Open();
                return cmd.ExecuteScalar() != null;
            }
        }

        private void DisableForm()
        {
            txtEmail.Enabled = false;
            txtPassword.Enabled = false;
            txtConfirm.Enabled = false;
            btnCreate.Enabled = false;
        }
    }
}
