using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace RRCManagementSystem
{
    public partial class Profile : System.Web.UI.Page
    {
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
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

            var email = Session["Email"].ToString();

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.usp_ClientProfile_GetByEmail", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 320).Value = email;

                try
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (r.Read())
                        {
                            string first = r["FirstName"] as string ?? "";
                            string middle = r["MiddleName"] as string ?? "";
                            string last = r["LastName"] as string ?? "";

                            string fullName = $"{last}, {first}";
                            if (!string.IsNullOrWhiteSpace(middle))
                                fullName += $" {middle[0]}.";

                            // Read-only textboxes
                            txtFirstName.Text = first;
                            txtMiddleName.Text = middle;
                            txtLastName.Text = last;
                            txtName.Text = fullName;
                            txtEmail.Text = r["Email"] as string ?? "";
                            txtContactNumber.Text = r["ContactNumber"] as string ?? "";
                            txtStreetAndUnit.Text = r["StreetAndUnit"] as string ?? "";
                            txtBarangay.Text = r["Barangay"] as string ?? "";
                            txtCity.Text = r["City"] as string ?? "";
                            txtRegion.Text = r["Region"] as string ?? "";
                            txtCountry.Text = r["Country"] as string ?? "";

                            // (Hidden) labels kept for compatibility
                            lblEmail.Text = txtEmail.Text;
                            lblContactNumber.Text = txtContactNumber.Text;
                            lblStreetAndUnit.Text = txtStreetAndUnit.Text;
                            lblBarangay.Text = txtBarangay.Text;
                            lblCity.Text = txtCity.Text;
                            lblRegion.Text = txtRegion.Text;
                            lblCountry.Text = txtCountry.Text;

                            // Profile picture
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

        // (Visible=false in ASPX, kept only to avoid orphaned handler scenarios)
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

            // Only handle photo upload
            if (!fuProfilePic.HasFile)
            {
                lblMessage.Text = "Please choose a photo to upload.";
                return;
            }

            string ext = (Path.GetExtension(fuProfilePic.FileName) ?? "").ToLowerInvariant();
            string[] allowed = { ".jpg", ".jpeg", ".png" };
            if (Array.IndexOf(allowed, ext) < 0)
            {
                lblMessage.Text = "⚠ Only JPG, JPEG, and PNG files are allowed.";
                return;
            }

            // (Optional) 5 MB limit
            const int maxBytes = 5 * 1024 * 1024;
            if (fuProfilePic.PostedFile.ContentLength > maxBytes)
            {
                lblMessage.Text = "⚠ File too large. Max size is 5 MB.";
                return;
            }

            // Save file with a GUID name (no user-controlled path)
            string fileName = Guid.NewGuid().ToString("N") + ext;
            string folder = Server.MapPath("~/Uploads/");
            string fullPath = Path.Combine(folder, fileName);

            try
            {
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                fuProfilePic.SaveAs(fullPath);

                // Update via stored procedure
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.usp_ClientProfile_UpdateProfilePic", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 320).Value = Session["Email"].ToString();
                    cmd.Parameters.Add("@ProfilePic", SqlDbType.NVarChar, 255).Value = fileName;

                    conn.Open();
                    int n = cmd.ExecuteNonQuery();

                    if (n > 0)
                    {
                        lblMessage.Text = "✅ Profile photo updated!";
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
