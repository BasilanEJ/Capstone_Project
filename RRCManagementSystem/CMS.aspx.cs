using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class CMS : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadCurrentImages();
            }
        }

        private void LoadCurrentImages()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM CMSContent WHERE ID = 1", conn))
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        // Hero Banner
                        SetImagePreview(imgHeroBannerPreview, reader["HeroBannerPath"]?.ToString(), "/images/rrc1.png");

                        // Services
                        SetImagePreview(imgBaitingPreview, reader["BaitingImagePath"]?.ToString(), "/images/service-baiting.jpg");
                        SetImagePreview(imgTermitePreventionPreview, reader["TermitePreventionImagePath"]?.ToString(), "/images/service-termite-prevention.jpg");
                        SetImagePreview(imgSoilPreview, reader["SoilImagePath"]?.ToString(), "/images/service-soil.jpg");
                        SetImagePreview(imgReticulationPreview, reader["ReticulationImagePath"]?.ToString(), "/images/services-reticulations.jpg");
                        SetImagePreview(imgMoundPreview, reader["MoundImagePath"]?.ToString(), "/images/service-mound.jpg");
                        SetImagePreview(imgGeneralPestPreview, reader["GeneralPestImagePath"]?.ToString(), "/images/service-general-pest.jpg");
                        SetImagePreview(imgTickFleasPreview, reader["TickFleasImagePath"]?.ToString(), "/images/service-tick-fleas.jpg");
                        SetImagePreview(imgBedbugsPreview, reader["BedbugsImagePath"]?.ToString(), "/images/service-bedbugs.jpg");
                        SetImagePreview(imgRatsPreview, reader["RatsImagePath"]?.ToString(), "/images/service-rats.jpg");

                        // About
                        SetImagePreview(imgAboutPreview, reader["AboutImagePath"]?.ToString(), "/images/ppe.png");

                        // Video
                        SetImagePreview(imgVideoThumbPreview, reader["VideoThumbnailPath"]?.ToString(), "/images/banner tv.jpg");
                        txtVimeoVideoId.Text = reader["VimeoVideoId"]?.ToString() ?? "1009218555";

                        // C&O
                        SetImagePreview(imgCOPreview, reader["COImagePath"]?.ToString(), "/images/c&o.png");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to load current images: " + ex.Message, "error");
            }
        }

        private void SetImagePreview(Image imgControl, string dbPath, string defaultPath)
        {
            if (!string.IsNullOrEmpty(dbPath))
            {
                imgControl.ImageUrl = dbPath;
            }
            else
            {
                imgControl.ImageUrl = defaultPath;
            }
        }

        protected void btnUpdateHeroBanner_Click(object sender, EventArgs e)
        {
            if (fuHeroBanner.HasFile)
            {
                string path = SaveUploadedImage(fuHeroBanner, "HeroBanner");
                if (!string.IsNullOrEmpty(path))
                {
                    UpdateDatabase("HeroBannerPath", path);
                    ShowAlert("Success", "Hero banner updated successfully!", "success");
                    LoadCurrentImages();
                }
            }
            else
            {
                ShowAlert("Warning", "Please select an image file first.", "warning");
            }
        }

        protected void btnUpdateServices_Click(object sender, EventArgs e)
        {
            bool updated = false;

            if (fuBaiting.HasFile)
            {
                string path = SaveUploadedImage(fuBaiting, "Baiting");
                if (!string.IsNullOrEmpty(path))
                {
                    UpdateDatabase("BaitingImagePath", path);
                    updated = true;
                }
            }

            if (fuTermitePrevention.HasFile)
            {
                string path = SaveUploadedImage(fuTermitePrevention, "TermitePrevention");
                if (!string.IsNullOrEmpty(path))
                {
                    UpdateDatabase("TermitePreventionImagePath", path);
                    updated = true;
                }
            }

            if (fuSoil.HasFile)
            {
                string path = SaveUploadedImage(fuSoil, "Soil");
                if (!string.IsNullOrEmpty(path))
                {
                    UpdateDatabase("SoilImagePath", path);
                    updated = true;
                }
            }

            if (fuReticulation.HasFile)
            {
                string path = SaveUploadedImage(fuReticulation, "Reticulation");
                if (!string.IsNullOrEmpty(path))
                {
                    UpdateDatabase("ReticulationImagePath", path);
                    updated = true;
                }
            }

            if (fuMound.HasFile)
            {
                string path = SaveUploadedImage(fuMound, "Mound");
                if (!string.IsNullOrEmpty(path))
                {
                    UpdateDatabase("MoundImagePath", path);
                    updated = true;
                }
            }

            if (fuGeneralPest.HasFile)
            {
                string path = SaveUploadedImage(fuGeneralPest, "GeneralPest");
                if (!string.IsNullOrEmpty(path))
                {
                    UpdateDatabase("GeneralPestImagePath", path);
                    updated = true;
                }
            }

            if (fuTickFleas.HasFile)
            {
                string path = SaveUploadedImage(fuTickFleas, "TickFleas");
                if (!string.IsNullOrEmpty(path))
                {
                    UpdateDatabase("TickFleasImagePath", path);
                    updated = true;
                }
            }

            if (fuBedbugs.HasFile)
            {
                string path = SaveUploadedImage(fuBedbugs, "Bedbugs");
                if (!string.IsNullOrEmpty(path))
                {
                    UpdateDatabase("BedbugsImagePath", path);
                    updated = true;
                }
            }

            if (fuRats.HasFile)
            {
                string path = SaveUploadedImage(fuRats, "Rats");
                if (!string.IsNullOrEmpty(path))
                {
                    UpdateDatabase("RatsImagePath", path);
                    updated = true;
                }
            }

            if (updated)
            {
                ShowAlert("Success", "Service images updated successfully!", "success");
                LoadCurrentImages();
            }
            else
            {
                ShowAlert("Warning", "Please select at least one image to update.", "warning");
            }
        }

        protected void btnUpdateAbout_Click(object sender, EventArgs e)
        {
            if (fuAbout.HasFile)
            {
                string path = SaveUploadedImage(fuAbout, "About");
                if (!string.IsNullOrEmpty(path))
                {
                    UpdateDatabase("AboutImagePath", path);
                    ShowAlert("Success", "About section image updated successfully!", "success");
                    LoadCurrentImages();
                }
            }
            else
            {
                ShowAlert("Warning", "Please select an image file first.", "warning");
            }
        }

        protected void btnUpdateVideo_Click(object sender, EventArgs e)
        {
            bool updated = false;

            // Update thumbnail if uploaded
            if (fuVideoThumbnail.HasFile)
            {
                string path = SaveUploadedImage(fuVideoThumbnail, "VideoThumbnail");
                if (!string.IsNullOrEmpty(path))
                {
                    UpdateDatabase("VideoThumbnailPath", path);
                    updated = true;
                }
            }

            // Update Vimeo ID if changed
            if (!string.IsNullOrEmpty(txtVimeoVideoId.Text.Trim()))
            {
                UpdateDatabase("VimeoVideoId", txtVimeoVideoId.Text.Trim());
                updated = true;
            }

            if (updated)
            {
                ShowAlert("Success", "Video section updated successfully!", "success");
                LoadCurrentImages();
            }
            else
            {
                ShowAlert("Warning", "Please provide an image or video ID to update.", "warning");
            }
        }

        protected void btnUpdateCO_Click(object sender, EventArgs e)
        {
            if (fuCO.HasFile)
            {
                string path = SaveUploadedImage(fuCO, "CO");
                if (!string.IsNullOrEmpty(path))
                {
                    UpdateDatabase("COImagePath", path);
                    ShowAlert("Success", "C&O image updated successfully!", "success");
                    LoadCurrentImages();
                }
            }
            else
            {
                ShowAlert("Warning", "Please select an image file first.", "warning");
            }
        }

        private string SaveUploadedImage(FileUpload fileUpload, string prefix)
        {
            try
            {
                string extension = Path.GetExtension(fileUpload.FileName).ToLowerInvariant();
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

                if (Array.IndexOf(allowedExtensions, extension) == -1)
                {
                    ShowAlert("Invalid File", "Only JPG, PNG, and GIF files are allowed.", "warning");
                    return null;
                }

                // Check file size (5MB max)
                if (fileUpload.PostedFile.ContentLength > 5 * 1024 * 1024)
                {
                    ShowAlert("File Too Large", "Maximum file size is 5MB.", "warning");
                    return null;
                }

                // Create folder if not exists
                string folderPath = Server.MapPath("~/Uploads/CMS/");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Generate unique filename
                string filename = prefix + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                string savePath = Path.Combine(folderPath, filename);

                // Save file
                fileUpload.SaveAs(savePath);

                // Return web path
                return "/Uploads/CMS/" + filename;
            }
            catch (Exception ex)
            {
                ShowAlert("Upload Error", "Failed to save image: " + ex.Message, "error");
                return null;
            }
        }

        private void UpdateDatabase(string columnName, string value)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(
                    $"IF EXISTS (SELECT 1 FROM CMSContent WHERE ID = 1) " +
                    $"UPDATE CMSContent SET {columnName} = @Value, LastUpdated = GETDATE() WHERE ID = 1 " +
                    $"ELSE INSERT INTO CMSContent (ID, {columnName}, LastUpdated) VALUES (1, @Value, GETDATE())", conn))
                {
                    cmd.Parameters.AddWithValue("@Value", value);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Database Error", "Failed to update database: " + ex.Message, "error");
            }
        }

        private void ShowAlert(string title, string message, string icon)
        {
            string script = $@"
                Swal.fire({{
                    title: '{title.Replace("'", "\\'")}',
                    text: '{message.Replace("'", "\\'")}',
                    icon: '{icon}',
                    confirmButtonColor: '#2563eb'
                }});
            ";
            ScriptManager.RegisterStartupScript(this, GetType(), "alert", script, true);
        }
    }
}