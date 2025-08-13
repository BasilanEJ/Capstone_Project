using System;
using System.Security.Cryptography;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class GenerateHmacKey : Page
    {
        protected void Page_Load(object sender, EventArgs e) { }

        protected void btnGenerate_Click(object sender, EventArgs e)
        {
            byte[] key = new byte[32]; // 256 bits
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }
            string base64Key = Convert.ToBase64String(key);
            lblResult.Text = $"<strong>Generated BlockchainHmacKey:</strong><br />{base64Key}";
        }
    }
}
