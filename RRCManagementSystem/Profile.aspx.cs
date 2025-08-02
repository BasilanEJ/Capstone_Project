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
            if (Session["Email"] == null)
            {
                lblMessage.Text = "⚠ Session expired. Please log in again.";
                return;
            }

            string email = Session["Email"].ToString();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT FirstName, MiddleName, LastName, Email, ContactNumber, 
                           StreetAndUnit, Barangay, City, Region, Country, ProfilePic 
                    FROM Clients 
                    WHERE Email = @Email";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Email", email);

                try
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        // Combine full name
                        string firstName = reader["FirstName"].ToString();
                        string middleName = reader["MiddleName"].ToString();
                        string lastName = reader["LastName"].ToString();

                        string fullName = $"{lastName}, {firstName}";
                        if (!string.IsNullOrWhiteSpace(middleName))
                        {
                            fullName += $" {middleName[0]}.";
                        }

                        // View Mode
                        txtFirstName.Text = firstName;
                        txtMiddleName.Text = middleName;
                        txtLastName.Text = lastName;
                        lblEmail.Text = reader["Email"].ToString();
                        lblContactNumber.Text = reader["ContactNumber"].ToString();
                        lblStreetAndUnit.Text = reader["StreetAndUnit"].ToString();
                        lblBarangay.Text = reader["Barangay"].ToString();
                        lblCity.Text = reader["City"].ToString();
                        lblRegion.Text = reader["Region"].ToString();
                        lblCountry.Text = reader["Country"].ToString();

                        // Edit Mode
                        txtFirstName.Text = firstName;
                        txtMiddleName.Text = middleName;
                        txtLastName.Text = lastName;
                        txtName.Text = fullName;
                        txtEmail.Text = reader["Email"].ToString();
                        txtContactNumber.Text = reader["ContactNumber"].ToString();
                        txtStreetAndUnit.Text = reader["StreetAndUnit"].ToString();
                        txtBarangay.Text = reader["Barangay"].ToString();
                        txtCity.Text = reader["City"].ToString();
                        txtRegion.Text = reader["Region"].ToString();
                        txtCountry.Text = reader["Country"].ToString();

                        // Profile Picture
                        string profilePic = reader["ProfilePic"] != DBNull.Value && !string.IsNullOrWhiteSpace(reader["ProfilePic"].ToString())
                            ? reader["ProfilePic"].ToString()
                            : "default-profile.png";
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
            string firstName = txtFirstName.Text.Trim();
            string middleName = txtMiddleName.Text.Trim();
            string lastName = txtLastName.Text.Trim();
            string contactNumber = txtContactNumber.Text.Trim();
            string streetAndUnit = txtStreetAndUnit.Text.Trim();
            string barangay = txtBarangay.Text.Trim();
            string city = txtCity.Text.Trim();
            string region = txtRegion.Text.Trim();
            string country = txtCountry.Text.Trim();

            string profilePicFileName = "";

            // Image upload logic
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

                profilePicFileName = Guid.NewGuid().ToString() + fileExtension;
                string folderPath = Server.MapPath("~/Uploads/");
                string fullPath = Path.Combine(folderPath, profilePicFileName);

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                fuProfilePic.SaveAs(fullPath);
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    UPDATE Clients
                    SET FirstName = @FirstName,
                        MiddleName = @MiddleName,
                        LastName = @LastName,
                        ContactNumber = @ContactNumber,
                        StreetAndUnit = @StreetAndUnit,
                        Barangay = @Barangay,
                        City = @City,
                        Region = @Region,
                        Country = @Country";

                if (!string.IsNullOrEmpty(profilePicFileName))
                {
                    query += ", ProfilePic = @ProfilePic";
                }

                query += " WHERE Email = @Email";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@FirstName", firstName);
                cmd.Parameters.AddWithValue("@MiddleName", middleName);
                cmd.Parameters.AddWithValue("@LastName", lastName);
                cmd.Parameters.AddWithValue("@ContactNumber", contactNumber);
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
