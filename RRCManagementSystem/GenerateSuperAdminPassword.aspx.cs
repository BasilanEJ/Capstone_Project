using System;
using System.Web.UI;
using Isopoh.Cryptography.Argon2;

namespace RRCManagementSystem
{
    public partial class GenerateSuperAdminPassword : System.Web.UI.Page
    {
        protected void btnGenerate_Click(object sender, EventArgs e)
        {
            string plainPassword = txtPlainPassword.Text.Trim();

            if (string.IsNullOrEmpty(plainPassword))
            {
                lblResult.Text = "⚠️ Enter a password to hash.";
                return;
            }

            try
            {
                // ✅ Generate Argon2 hash (includes salt and parameters)
                string hash = Argon2.Hash(plainPassword);

                lblResult.Text = $@"
                    <strong>Argon2 Hashed Password:</strong><br/>{hash}<br/><br/>
                    ✅ Copy this into your SQL insert's <code>PasswordHash</code> column.<br/>
                    No separate salt is required.
                ";
            }
            catch (Exception ex)
            {
                lblResult.Text = $"⚠️ Error: {ex.Message}";
            }
        }
    }
}


