using System;
using System.Configuration;
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

            string email = Session["Email"].ToString();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT FirstName, MiddleName, LastName, Email, ContactNumber,
                       StreetAndUnit, Barangay, City, Region, Country, ProfilePic
                FROM Clients
                WHERE Email = @Email;", conn))
            {
                cmd.Parameters.AddWithValue("@Email", email);

                try
                {
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            string first = r["FirstName"]?.ToString() ?? "";
                            string middle = r["MiddleName"]?.ToString() ?? "";
                            string last = r["LastName"]?.ToString() ?? "";

                            string fullName = $"{last}, {first}";
                            if (!string.IsNullOrWhiteSpace(middle))
                                fullName += $" {middle[0]}.";

                            // Read-only textboxes
                            txtFirstName.Text = first;
                            txtMiddleName.Text = middle;
                            txtLastName.Text = last;
                            txtName.Text = fullName;
                            txtEmail.Text = r["Email"]?.ToString() ?? "";
                            txtContactNumber.Text = r["ContactNumber"]?.ToString() ?? "";
                            txtStreetAndUnit.Text = r["StreetAndUnit"]?.ToString() ?? "";
                            txtBarangay.Text = r["Barangay"]?.ToString() ?? "";
                            txtCity.Text = r["City"]?.ToString() ?? "";
                            txtRegion.Text = r["Region"]?.ToString() ?? "";
                            txtCountry.Text = r["Country"]?.ToString() ?? "";

                            // (Hidden) labels kept for compatibility
                            lblEmail.Text = txtEmail.Text;
                            lblContactNumber.Text = txtContactNumber.Text;
                            lblStreetAndUnit.Text = txtStreetAndUnit.Text;
                            lblBarangay.Text = txtBarangay.Text;
                            lblCity.Text = txtCity.Text;
                            lblRegion.Text = txtRegion.Text;
                            lblCountry.Text = txtCountry.Text;

                            // Profile picture
                            string pic = (r["ProfilePic"] != DBNull.Value)
                                ? (r["ProfilePic"]?.ToString() ?? "")
                                : "";
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

            string ext = Path.GetExtension(fuProfilePic.FileName)?.ToLower() ?? "";
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

            // Save file
            string fileName = Guid.NewGuid().ToString("N") + ext;
            string folder = Server.MapPath("~/Uploads/");
            string fullPath = Path.Combine(folder, fileName);

            try
            {
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                fuProfilePic.SaveAs(fullPath);

                // Update DB
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE Clients SET ProfilePic = @Pic WHERE Email = @Email;", conn))
                {
                    cmd.Parameters.AddWithValue("@Pic", fileName);
                    cmd.Parameters.AddWithValue("@Email", Session["Email"].ToString());

                    conn.Open();
                    int n = cmd.ExecuteNonQuery();

                    if (n > 0)
                    {
                        lblMessage.Text = "✅ Profile photo updated!";
                        // Refresh UI to show new image
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
