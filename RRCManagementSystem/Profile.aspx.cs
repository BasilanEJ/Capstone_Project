using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using RRCManagementSystem.Helpers;

namespace RRCManagementSystem
{
    public partial class Profile : System.Web.UI.Page
    {
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Require login
            if (Session["Email"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadProfileData();
            }
        }

        private void LoadProfileData()
        {
            lblMessage.Text = string.Empty;

            string email = Session["Email"].ToString();
            string emailHash = AESHelper.ComputeSHA256WithPepper(email);

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.usp_ClientProfile_GetByEmailHash", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@EmailHash", SqlDbType.Char, 64).Value = emailHash;

                try
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (r.Read())
                        {
                            // ✅ Load Client Number
                            txtClientNumber.Text = r["ClientNumber"]?.ToString() ?? "N/A";

                            string first = r["FirstName"]?.ToString() ?? "";
                            string middle = r["MiddleName"]?.ToString() ?? "";
                            string last = r["LastName"]?.ToString() ?? "";

                            // Build full name
                            string fullName = $"{last}, {first}";
                            if (!string.IsNullOrWhiteSpace(middle))
                                fullName += $" {middle[0]}.";

                            // Plaintext values
                            txtFirstName.Text = first;
                            txtMiddleName.Text = middle;
                            txtLastName.Text = last;
                            txtName.Text = fullName;

                            // 🔹 Decrypt sensitive fields
                            txtEmail.Text = r["EmailEnc"] != DBNull.Value ? AESHelper.DecryptEmail(r["EmailEnc"].ToString()) : "";
                            txtContactNumber.Text = r["ContactEnc"] != DBNull.Value ? AESHelper.DecryptField(r["ContactEnc"].ToString()) : "";
                            txtStreetAndUnit.Text = r["StreetEnc"] != DBNull.Value ? AESHelper.DecryptField(r["StreetEnc"].ToString()) : "";
                            txtBarangay.Text = r["BarangayEnc"] != DBNull.Value ? AESHelper.DecryptField(r["BarangayEnc"].ToString()) : "";
                            txtCity.Text = r["CityEnc"] != DBNull.Value ? AESHelper.DecryptField(r["CityEnc"].ToString()) : "";
                            txtRegion.Text = r["RegionEnc"] != DBNull.Value ? AESHelper.DecryptField(r["RegionEnc"].ToString()) : "";
                            txtCountry.Text = r["CountryEnc"] != DBNull.Value ? AESHelper.DecryptField(r["CountryEnc"].ToString()) : "";

                            // Hidden labels for compatibility
                            lblEmail.Text = txtEmail.Text;
                            lblContactNumber.Text = txtContactNumber.Text;
                            lblStreetAndUnit.Text = txtStreetAndUnit.Text;
                            lblBarangay.Text = txtBarangay.Text;
                            lblCity.Text = txtCity.Text;
                            lblRegion.Text = txtRegion.Text;
                            lblCountry.Text = txtCountry.Text;

                            // Profile Picture
                            var pic = r["ProfilePic"] as string;
                            if (string.IsNullOrWhiteSpace(pic))
                                pic = "default-profile.png";

                            imgProfilePic.ImageUrl = "~/Uploads/" + pic;
                        }
                        else
                        {
                            lblMessage.Text = "⚠ Profile not found.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "⚠ Error loading profile: " + ex.Message;
                }
            }
        }

        // Refresh profile if cancelled
        protected void btnCancelEdit_Click(object sender, EventArgs e)
        {
            LoadProfileData();
        }

        protected void btnSaveProfile_Click(object sender, EventArgs e)
        {
            if (Session["Email"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            // Validate file selection
            if (!fuProfilePic.HasFile)
            {
                lblMessage.Text = "⚠ Please choose a photo to upload.";
                return;
            }

            // Validate extension
            string ext = (Path.GetExtension(fuProfilePic.FileName) ?? "").ToLowerInvariant();
            string[] allowed = { ".jpg", ".jpeg", ".png" };
            if (Array.IndexOf(allowed, ext) < 0)
            {
                lblMessage.Text = "⚠ Only JPG, JPEG, and PNG files are allowed.";
                return;
            }

            // Optional: 5 MB limit
            const int maxBytes = 5 * 1024 * 1024;
            if (fuProfilePic.PostedFile.ContentLength > maxBytes)
            {
                lblMessage.Text = "⚠ File too large. Max size is 5 MB.";
                return;
            }

            // Generate secure file name
            string fileName = Guid.NewGuid().ToString("N") + ext;
            string folder = Server.MapPath("~/Uploads/");
            string fullPath = Path.Combine(folder, fileName);

            try
            {
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                // Save file
                fuProfilePic.SaveAs(fullPath);

                // Update in database
                string email = Session["Email"].ToString();
                string emailHash = AESHelper.ComputeSHA256WithPepper(email);

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.usp_ClientProfile_UpdateProfilePic", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@EmailHash", SqlDbType.Char, 64).Value = emailHash;
                    cmd.Parameters.Add("@ProfilePic", SqlDbType.NVarChar, 255).Value = fileName;

                    conn.Open();
                    int n = cmd.ExecuteNonQuery();

                    if (n > 0)
                    {
                        lblMessage.Text = "✅ Profile photo updated!";
                        lblMessage.CssClass = "text-success";
                        imgProfilePic.ImageUrl = "~/Uploads/" + fileName;
                    }
                    else
                    {
                        lblMessage.Text = "⚠ Update failed. Please try again.";
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠ Error updating profile: " + ex.Message;
            }
        }
    }
}