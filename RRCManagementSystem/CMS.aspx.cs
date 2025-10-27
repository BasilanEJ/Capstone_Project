using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using RRCManagementSystem.Helpers;

namespace RRCManagementSystem
{
    public partial class CMS : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        // Whitelist of allowed column names to prevent SQL injection
        private static readonly HashSet<string> AllowedColumns = new HashSet<string>
        {
            "HeroBannerPath",
            "AboutImagePath",
            "VideoThumbnailPath",
            "VideoUrl",
            "VideoType",
            "COImagePath",
            "Blog1ImagePath",
            "Blog2ImagePath",
            "Blog3ImagePath"
        };

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
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Hero Banner
                            SetImagePreview(imgHeroBannerPreview, reader["HeroBannerPath"]?.ToString(), "/images/rrc1.png");

                            // About Section
                            if (imgAboutPreview != null)
                            {
                                SetImagePreview(imgAboutPreview, reader["AboutImagePath"]?.ToString(), "/images/about-default.png");
                            }

                            // Video
                            SetImagePreview(imgVideoThumbPreview, reader["VideoThumbnailPath"]?.ToString(), "/images/banner tv.jpg");
                            txtVideoUrl.Text = reader["VideoUrl"]?.ToString() ?? "https://www.youtube.com/watch?v=dQw4w9WgXcQ";

                            string videoType = reader["VideoType"]?.ToString() ?? "YouTube";
                            ddlVideoType.SelectedValue = videoType;

                            // C&O and Blogs
                            SetImagePreview(imgCOPreview, reader["COImagePath"]?.ToString(), "/images/c&o.png");
                            SetImagePreview(imgBlog1Preview, reader["Blog1ImagePath"]?.ToString(), "/Images/DIY.jpg");
                            SetImagePreview(imgBlog2Preview, reader["Blog2ImagePath"]?.ToString(), "/Images/blog2.jpg");
                            SetImagePreview(imgBlog3Preview, reader["Blog3ImagePath"]?.ToString(), "/Images/blog3.jpg");
                        }
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
            if (imgControl == null) return;

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
                string path = SaveUploadedImageWithResize(
                    fuHeroBanner,
                    "HeroBanner",
                    ImageHelper.RecommendedDimensions.HeroBanner.Width,
                    ImageHelper.RecommendedDimensions.HeroBanner.Height
                );

                if (!string.IsNullOrEmpty(path))
                {
                    UpdateDatabase("HeroBannerPath", path);
                    ShowAlert("Success", "Hero banner updated successfully! Image optimized to 1920x600px", "success");
                    LoadCurrentImages();
                }
            }
            else
            {
                ShowAlert("Warning", "Please select an image file first.", "warning");
            }
        }

        protected void btnUpdateAbout_Click(object sender, EventArgs e)
        {
            if (fuAbout.HasFile)
            {
                string path = SaveUploadedImageWithResize(
                    fuAbout,
                    "About",
                    ImageHelper.RecommendedDimensions.About.Width,
                    ImageHelper.RecommendedDimensions.About.Height
                );

                if (!string.IsNullOrEmpty(path))
                {
                    UpdateDatabase("AboutImagePath", path);
                    ShowAlert("Success", "About section image updated successfully! Image optimized to 600x800px", "success");
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

            if (fuVideoThumbnail.HasFile)
            {
                string path = SaveUploadedImageWithResize(
                    fuVideoThumbnail,
                    "VideoThumbnail",
                    ImageHelper.RecommendedDimensions.VideoThumbnail.Width,
                    ImageHelper.RecommendedDimensions.VideoThumbnail.Height
                );

                if (!string.IsNullOrEmpty(path))
                {
                    UpdateDatabase("VideoThumbnailPath", path);
                    updated = true;
                }
            }

            string videoUrl = txtVideoUrl.Text.Trim();
            string videoType = ddlVideoType.SelectedValue;

            if (!string.IsNullOrEmpty(videoUrl))
            {
                if (IsValidVideoUrl(videoUrl, videoType))
                {
                    UpdateVideoSettings(videoUrl, videoType);
                    updated = true;
                }
                else
                {
                    ShowAlert("Invalid URL", $"Please enter a valid {videoType} URL.", "warning");
                    return;
                }
            }

            if (updated)
            {
                ShowAlert("Success", "Video section updated successfully!", "success");
                LoadCurrentImages();
            }
            else
            {
                ShowAlert("Warning", "Please provide an image or video URL to update.", "warning");
            }
        }

        protected void btnUpdateBlogs_Click(object sender, EventArgs e)
        {
            bool updated = false;
            var blogSize = ImageHelper.RecommendedDimensions.BlogCard;

            if (fuBlog1.HasFile)
            {
                string path = SaveUploadedImageWithResize(fuBlog1, "Blog1", blogSize.Width, blogSize.Height);
                if (!string.IsNullOrEmpty(path))
                {
                    UpdateDatabase("Blog1ImagePath", path);
                    updated = true;
                }
            }

            if (fuBlog2.HasFile)
            {
                string path = SaveUploadedImageWithResize(fuBlog2, "Blog2", blogSize.Width, blogSize.Height);
                if (!string.IsNullOrEmpty(path))
                {
                    UpdateDatabase("Blog2ImagePath", path);
                    updated = true;
                }
            }

            if (fuBlog3.HasFile)
            {
                string path = SaveUploadedImageWithResize(fuBlog3, "Blog3", blogSize.Width, blogSize.Height);
                if (!string.IsNullOrEmpty(path))
                {
                    UpdateDatabase("Blog3ImagePath", path);
                    updated = true;
                }
            }

            if (updated)
            {
                ShowAlert("Success", "Blog images updated successfully! All images optimized to 640x400px", "success");
                LoadCurrentImages();
            }
            else
            {
                ShowAlert("Warning", "Please select at least one image to update.", "warning");
            }
        }

        protected void btnUpdateCO_Click(object sender, EventArgs e)
        {
            if (fuCO.HasFile)
            {
                string path = SaveUploadedImageWithResize(
                    fuCO,
                    "CO",
                    ImageHelper.RecommendedDimensions.COBanner.Width,
                    ImageHelper.RecommendedDimensions.COBanner.Height
                );

                if (!string.IsNullOrEmpty(path))
                {
                    UpdateDatabase("COImagePath", path);
                    ShowAlert("Success", "C&O image updated successfully! Image optimized to 1200x400px", "success");
                    LoadCurrentImages();
                }
            }
            else
            {
                ShowAlert("Warning", "Please select an image file first.", "warning");
            }
        }

        private string SaveUploadedImageWithResize(FileUpload fileUpload, string prefix, int targetWidth, int targetHeight)
        {
            try
            {
                string errorMessage;
                if (!ImageHelper.ValidateImage(fileUpload.PostedFile.InputStream, out errorMessage))
                {
                    ShowAlert("Invalid Image", errorMessage, "warning");
                    return null;
                }

                fileUpload.PostedFile.InputStream.Position = 0;

                string extension = Path.GetExtension(fileUpload.FileName).ToLowerInvariant();
                string folderPath = Server.MapPath("~/Uploads/CMS/");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string filename = prefix + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                string savePath = Path.Combine(folderPath, filename);

                ImageHelper.ResizeAndSaveImage(
                    fileUpload.PostedFile.InputStream,
                    savePath,
                    targetWidth,
                    targetHeight,
                    maintainAspect: true
                );

                return "/Uploads/CMS/" + filename;
            }
            catch (Exception ex)
            {
                ShowAlert("Upload Error", "Failed to process image: " + ex.Message, "error");
                return null;
            }
        }

        private bool IsValidVideoUrl(string url, string videoType)
        {
            if (string.IsNullOrEmpty(url)) return false;

            try
            {
                if (videoType == "YouTube")
                {
                    return url.Contains("youtube.com/watch?v=") ||
                           url.Contains("youtu.be/") ||
                           url.Contains("youtube.com/embed/");
                }
                else if (videoType == "Vimeo")
                {
                    return url.Contains("vimeo.com/");
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private void UpdateVideoSettings(string videoUrl, string videoType)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE CMSContent SET VideoUrl = @VideoUrl, VideoType = @VideoType, LastUpdated = GETDATE() WHERE ID = 1",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@VideoUrl", videoUrl);
                    cmd.Parameters.AddWithValue("@VideoType", videoType);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Database Error", "Failed to update video settings: " + ex.Message, "error");
            }
        }

        private void UpdateDatabase(string columnName, string value)
        {
            try
            {
                // Validate column name against whitelist to prevent SQL injection
                if (!AllowedColumns.Contains(columnName))
                {
                    throw new ArgumentException($"Invalid column name: {columnName}");
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Use parameterized query with validated column name
                    string query = $@"
                        IF EXISTS (SELECT 1 FROM CMSContent WHERE ID = 1) 
                            UPDATE CMSContent SET [{columnName}] = @Value, LastUpdated = GETDATE() WHERE ID = 1 
                        ELSE 
                            INSERT INTO CMSContent (ID, [{columnName}], LastUpdated) VALUES (1, @Value, GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Value", value ?? (object)DBNull.Value);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Database Error", "Failed to update database: " + ex.Message, "error");
            }
        }

        private void ShowAlert(string title, string message, string icon)
        {
            // Escape single quotes and sanitize input for JavaScript
            string sanitizedTitle = System.Web.HttpUtility.JavaScriptStringEncode(title);
            string sanitizedMessage = System.Web.HttpUtility.JavaScriptStringEncode(message);
            string sanitizedIcon = System.Web.HttpUtility.JavaScriptStringEncode(icon);

            string script = $@"
                Swal.fire({{
                    title: '{sanitizedTitle}',
                    text: '{sanitizedMessage}',
                    icon: '{sanitizedIcon}',
                    confirmButtonColor: '#2563eb'
                }});
            ";
            ScriptManager.RegisterStartupScript(this, GetType(), "alert", script, true);
        }
    }
}