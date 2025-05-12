using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;

namespace RRCManagementSystem
{
    public partial class Profile : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["Email"] == null)
                {
                    Response.Redirect("~/Login.aspx");
                    return;
                }

                LoadProfileData();
            }
        }

        private void LoadProfileData()
        {
            string email = Session["Email"].ToString();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT Name, Email, ContactNumber, StreetAndUnit, Barangay, City, Region, Country, ProfilePic 
                                 FROM Clients WHERE Email = @Email";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Email", email);

                try
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        // View Mode
                        lblName.Text = reader["Name"].ToString();
                        lblEmail.Text = reader["Email"].ToString();
                        lblContactNumber.Text = reader["ContactNumber"].ToString();
                        lblStreetAndUnit.Text = reader["StreetAndUnit"].ToString();
                        lblBarangay.Text = reader["Barangay"].ToString();
                        lblCity.Text = reader["City"].ToString();
                        lblRegion.Text = reader["Region"].ToString();
                        lblCountry.Text = reader["Country"].ToString();

                        // Edit Mode
                        txtName.Text = reader["Name"].ToString();
                        txtEmail.Text = reader["Email"].ToString();
                        txtContactNumber.Text = reader["ContactNumber"].ToString();
                        txtStreetAndUnit.Text = reader["StreetAndUnit"].ToString();
                        txtBarangay.Text = reader["Barangay"].ToString();
                        txtCity.Text = reader["City"].ToString();
                        txtRegion.Text = reader["Region"].ToString();
                        txtCountry.Text = reader["Country"].ToString();

                        // Profile Picture
                        string profilePic = reader["ProfilePic"] != DBNull.Value ? reader["ProfilePic"].ToString() : "default-profile.png";
                        imgProfilePic.ImageUrl = "~/Uploads/" + profilePic;
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "⚠ Error loading profile: " + ex.Message;
                }
            }
        }

        protected void btnEditProfile_Click(object sender, EventArgs e)
        {
            pnlViewMode.Visible = false;
            pnlEditMode.Visible = true;
        }

        protected void btnCancelEdit_Click(object sender, EventArgs e)
        {
            pnlViewMode.Visible = true;
            pnlEditMode.Visible = false;
        }

        protected void btnSaveProfile_Click(object sender, EventArgs e)
        {
            if (Session["Email"] == null)
            {
                lblMessage.Text = "⚠ Session expired. Please log in again.";
                Response.Redirect("~/Login.aspx");
                return;
            }

            string email = Session["Email"].ToString();
            string newName = txtName.Text.Trim();
            string newContact = txtContactNumber.Text.Trim();
            string streetAndUnit = txtStreetAndUnit.Text.Trim();
            string barangay = txtBarangay.Text.Trim();
            string city = txtCity.Text.Trim();
            string region = txtRegion.Text.Trim();
            string country = txtCountry.Text.Trim();

            string profilePicFileName = ""; // Will be filled if new file is uploaded

            // ✅ Handle image upload (strict validation)
            if (fuProfilePic.HasFile)
            {
                string fileExtension = Path.GetExtension(fuProfilePic.FileName).ToLower();
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };

                if (!Array.Exists(allowedExtensions, ext => ext == fileExtension))
                {
                    lblMessage.Text = "⚠ Only JPG, JPEG, and PNG files are allowed.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                // Generate a unique file name
                profilePicFileName = Guid.NewGuid().ToString() + fileExtension;
                string folderPath = Server.MapPath("~/Uploads/");
                string fullPath = Path.Combine(folderPath, profilePicFileName);

                // Ensure directory exists
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Save the file
                fuProfilePic.SaveAs(fullPath);
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"UPDATE Clients
                                 SET Name = @Name,
                                     ContactNumber = @ContactNumber,
                                     StreetAndUnit = @StreetAndUnit,
                                     Barangay = @Barangay,
                                     City = @City,
                                     Region = @Region,
                                     Country = @Country";

                // If a new profile pic is uploaded, update it too
                if (!string.IsNullOrEmpty(profilePicFileName))
                {
                    query += ", ProfilePic = @ProfilePic";
                }

                query += " WHERE Email = @Email";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Name", newName);
                cmd.Parameters.AddWithValue("@ContactNumber", newContact);
                cmd.Parameters.AddWithValue("@StreetAndUnit", streetAndUnit);
                cmd.Parameters.AddWithValue("@Barangay", barangay);
                cmd.Parameters.AddWithValue("@City", city);
                cmd.Parameters.AddWithValue("@Region", region);
                cmd.Parameters.AddWithValue("@Country", country);
                cmd.Parameters.AddWithValue("@Email", email);

                if (!string.IsNullOrEmpty(profilePicFileName))
                {
                    cmd.Parameters.AddWithValue("@ProfilePic", profilePicFileName);
                }

                try
                {
                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        lblMessage.Text = "✅ Profile updated successfully!";
                        pnlViewMode.Visible = true;
                        pnlEditMode.Visible = false;
                        LoadProfileData();
                    }
                    else
                    {
                        lblMessage.Text = "⚠ Update failed. Please try again.";
                    }
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "⚠ Error updating profile: " + ex.Message;
                }
            }
        }
    }
}

