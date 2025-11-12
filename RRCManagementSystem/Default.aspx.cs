using RRCManagementSystem.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class Default : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadCMSContent();
                LoadServicesFromDatabase();
                ApplyLazyLoadingToImages();
                LoadReviews();
                LoadBlogs();

                MaintainScrollPositionOnPostBack = true;
            }
        }

        #region Blog Methods

        /// <summary>
        /// Load blogs from database
        /// </summary>
        private void LoadBlogs()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== LoadBlogs: Starting ===");

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Use full table name with schema
                    string query = @"
                SELECT TOP (10)
                    [BlogID],
                    [BlogTitle],
                    [BlogDescription],
                    [BlogContent],
                    [ImagePath],
                    [DisplayOrder],
                    [IsActive],
                    [CreatedDate],
                    [ModifiedDate]
                FROM [EJBasilan_RRCDB].[EJBasilan_admin].[BlogsCMS]
                WHERE [IsActive] = 1
                ORDER BY [DisplayOrder], [BlogID] DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        System.Diagnostics.Debug.WriteLine("✅ Database connection opened");

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        System.Diagnostics.Debug.WriteLine($"✅ Blogs loaded: {dt.Rows.Count}");

                        if (dt.Rows.Count > 0)
                        {
                            // Log each blog
                            foreach (DataRow row in dt.Rows)
                            {
                                System.Diagnostics.Debug.WriteLine($"  - Blog {row["BlogID"]}: {row["BlogTitle"]}");
                            }

                            // Bind to repeaters
                            rptBlogs.DataSource = dt;
                            rptBlogs.DataBind();
                            System.Diagnostics.Debug.WriteLine("✅ rptBlogs DataBound");

                            rptBlogModals.DataSource = dt;
                            rptBlogModals.DataBind();
                            System.Diagnostics.Debug.WriteLine("✅ rptBlogModals DataBound");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("⚠️ No blogs found in database");
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                System.Diagnostics.Debug.WriteLine("❌ SQL Error: " + sqlEx.Message);
                System.Diagnostics.Debug.WriteLine("Error Number: " + sqlEx.Number);
                System.Diagnostics.Debug.WriteLine("Stack Trace: " + sqlEx.StackTrace);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("❌ Blog Load Error: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Stack Trace: " + ex.StackTrace);
            }
        }

        protected string FormatBlogContent(string content)
        {
            if (string.IsNullOrEmpty(content))
                return "";

            // Convert line breaks to HTML
            content = content.Replace("\r\n", "<br>");
            content = content.Replace("\n", "<br>");

            // Convert bullet points
            content = System.Text.RegularExpressions.Regex.Replace(
                content,
                @"^\s*[-•]\s*(.+)$",
                "<li>$1</li>",
                System.Text.RegularExpressions.RegexOptions.Multiline
            );

            // Wrap lists in <ul> tags
            if (content.Contains("<li>"))
            {
                content = System.Text.RegularExpressions.Regex.Replace(
                    content,
                    @"(<li>.*?</li>(\s*<br>)*)+",
                    match => "<ul style='margin-left:20px; margin-bottom:20px;'>" +
                             match.Value.Replace("<br>", "") + "</ul>"
                );
            }

            return content;
        }

        #endregion

        #region Review Methods
        // Add this method to your Default.aspx.cs

        /// <summary>
        /// Get opening HTML for carousel slide (every 3 reviews)
        /// </summary>
        protected string GetCarouselSlideOpen(int index)
        {
            if (index % 3 == 0)
            {
                string activeClass = index == 0 ? "active" : "";
                return $"<div class='carousel-item {activeClass}'><div class='row justify-content-center g-4'>";
            }
            return "";
        }

        /// <summary>
        /// Get closing HTML for carousel slide (every 3 reviews or at end)
        /// </summary>
        protected string GetCarouselSlideClose(int index, int totalCount)
        {
            if ((index + 1) % 3 == 0 || (index + 1) == totalCount)
            {
                return "</div></div>";
            }
            return "";
        }

        // Update LoadReviews method to also count total reviews
        private void LoadReviews()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                SELECT ReviewID, CustomerName, ReviewText, Rating, Recommends, 
                       ISNULL(ReviewSource, 'Website') as ReviewSource
                FROM Reviews
                WHERE IsActive = 1
                ORDER BY DisplayOrder, ReviewID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        // Store total count in ViewState or Session
                        ViewState["TotalReviews"] = dt.Rows.Count;

                        rptReviews.DataSource = dt;
                        rptReviews.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Review Load Error: " + ex.Message);
            }
        }

        // Helper to get total count
        protected int GetTotalReviews()
        {
            return ViewState["TotalReviews"] != null ? (int)ViewState["TotalReviews"] : 0;
        }

        /// <summary>
        /// Generate star rating display
        /// </summary>
        protected string GetStarRatingForReview(int rating)
        {
            string stars = "";
            for (int i = 0; i < rating; i++)
            {
                stars += "⭐";
            }
            return stars;
        }


        protected void btnSubmitReview_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            string reviewText = txtReview.Text.Trim();
            bool recommends = chkRecommend.Checked;
            int rating;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(reviewText) ||
                !int.TryParse(ddlRating.SelectedValue, out rating))
            {
                // You can show a SweetAlert or simple message here
                ScriptManager.RegisterStartupScript(this, GetType(), "alert",
                    "Swal.fire('Incomplete!', 'Please fill out all fields before submitting.', 'warning');", true);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                INSERT INTO Reviews (CustomerName, ReviewText, Rating, Recommends, IsActive, CreatedAt)
                VALUES (@Name, @Text, @Rating, @Recommends, 1, GETDATE())";
                    // Set IsActive = 1 if you want auto-publish

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", name);
                        cmd.Parameters.AddWithValue("@Text", reviewText);
                        cmd.Parameters.AddWithValue("@Rating", rating);
                        cmd.Parameters.AddWithValue("@Recommends", recommends);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                // Clear inputs
                txtName.Text = "";
                txtReview.Text = "";
                ddlRating.SelectedIndex = 0;
                chkRecommend.Checked = false;

                ScriptManager.RegisterStartupScript(this, GetType(), "success",
                    "Swal.fire('Thank you!', 'Your review has been submitted for approval.', 'success');", true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Review Submit Error: " + ex.Message);
                ScriptManager.RegisterStartupScript(this, GetType(), "error",
                    "Swal.fire('Error!', 'Something went wrong. Please try again later.', 'error');", true);
            }
        }


        protected string GetShortReviewText(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            var words = text.Split(' ');
            if (words.Length <= 50) return text;
            return string.Join(" ", words.Take(50)) + "...";
        }



        #endregion

        #region Service Methods

        /// <summary>
        /// Load services from database
        /// </summary>
        private void LoadServicesFromDatabase()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT ServiceID, ServiceType, ServiceTitle, ServiceDescription, 
                               BulletPoints, ImagePath, DisplayOrder
                        FROM ServicesCMS
                        WHERE IsActive = 1
                        ORDER BY ServiceType, DisplayOrder";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        // Filter and bind Termite Control services
                        DataView dvTermite = new DataView(dt);
                        dvTermite.RowFilter = "ServiceType = 'Termite Control'";
                        rptTermiteServices.DataSource = dvTermite;
                        rptTermiteServices.DataBind();
                        rptTermiteModals.DataSource = dvTermite;
                        rptTermiteModals.DataBind();

                        // Filter and bind General Pest Control services
                        DataView dvPest = new DataView(dt);
                        dvPest.RowFilter = "ServiceType = 'General Pest Control'";
                        rptPestServices.DataSource = dvPest;
                        rptPestServices.DataBind();
                        rptPestModals.DataSource = dvPest;
                        rptPestModals.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Service Load Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Get shortened description for service cards
        /// </summary>
        protected string GetShortDescription(string description)
        {
            if (string.IsNullOrEmpty(description))
                return "";

            // Return first 80 characters or until first period
            int maxLength = 80;
            if (description.Length <= maxLength)
                return description;

            // Try to cut at a sentence
            int periodIndex = description.IndexOf('.', 0, Math.Min(description.Length, maxLength));
            if (periodIndex > 0)
                return description.Substring(0, periodIndex + 1);

            // Otherwise just cut at maxLength
            return description.Substring(0, maxLength) + "...";
        }

        /// <summary>
        /// Format bullet points for display
        /// </summary>
        protected string FormatBulletPoints(string bulletPoints)
        {
            if (string.IsNullOrEmpty(bulletPoints))
                return "";

            var lines = bulletPoints.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var formatted = new System.Text.StringBuilder();

            foreach (var line in lines)
            {
                string trimmed = line.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    // Remove leading dash or bullet if present
                    if (trimmed.StartsWith("-") || trimmed.StartsWith("•"))
                        trimmed = trimmed.Substring(1).Trim();

                    formatted.AppendLine($"<li>{trimmed}</li>");
                }
            }

            return formatted.ToString();
        }

        #endregion

        #region Image Methods

        /// <summary>
        /// Apply lazy loading to images
        /// </summary>
        private void ApplyLazyLoadingToImages()
        {
            SetLazyLoadImage(imgHeroBanner);
            SetLazyLoadImage(imgAbout);
            SetLazyLoadImage(imgVideoThumbnail);
            SetLazyLoadImage(imgCO);
        }

        /// <summary>
        /// Set lazy loading attributes on image control
        /// </summary>
        private void SetLazyLoadImage(System.Web.UI.WebControls.Image imgControl)
        {
            if (imgControl != null && !string.IsNullOrEmpty(imgControl.ImageUrl))
            {
                imgControl.Attributes["data-src"] = ResolveUrl(imgControl.ImageUrl);
                imgControl.ImageUrl = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1 1'%3E%3C/svg%3E";
                imgControl.Attributes["loading"] = "lazy";
            }
        }

        #endregion

        #region CMS Content Methods

        /// <summary>
        /// Load CMS content from database
        /// </summary>
        private void LoadCMSContent()
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
                        imgHeroBanner.ImageUrl = GetImagePath(reader, "HeroBannerPath", "/images/rrc1.png");

                        // About Section
                        imgAbout.ImageUrl = GetImagePath(reader, "AboutImagePath", "/images/ppe.png");

                        // Video Section
                        imgVideoThumbnail.ImageUrl = GetImagePath(reader, "VideoThumbnailPath", "/images/banner tv.jpg");
                        string videoUrl = reader["VideoUrl"]?.ToString() ?? "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
                        string videoType = reader["VideoType"]?.ToString() ?? "YouTube";
                        string videoId = ExtractVideoId(videoUrl, videoType);
                        hfVimeoVideoId.Value = videoId;
                        hfVideoType.Value = videoType;

                        // C&O Section
                        imgCO.ImageUrl = GetImagePath(reader, "COImagePath", "/images/c&o.png");
                    }
                    else
                    {
                        SetDefaultImages();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("CMS Load Error: " + ex.Message);
                SetDefaultImages();
            }
        }

        /// <summary>
        /// Get image path from reader with fallback
        /// </summary>
        private string GetImagePath(SqlDataReader reader, string columnName, string defaultPath)
        {
            try
            {
                string path = reader[columnName]?.ToString();
                return string.IsNullOrEmpty(path) ? defaultPath : path;
            }
            catch
            {
                return defaultPath;
            }
        }

        /// <summary>
        /// Set default images if CMS content not found
        /// </summary>
        private void SetDefaultImages()
        {
            imgHeroBanner.ImageUrl = "/images/rrc1.png";
            imgAbout.ImageUrl = "/images/ppe.png";
            imgVideoThumbnail.ImageUrl = "/images/banner tv.jpg";
            imgCO.ImageUrl = "/images/c&o.png";
        }

        /// <summary>
        /// Extract video ID from URL
        /// </summary>
        private string ExtractVideoId(string url, string videoType)
        {
            if (string.IsNullOrEmpty(url)) return "";

            try
            {
                if (videoType == "YouTube")
                {
                    if (url.Contains("watch?v="))
                    {
                        var uri = new Uri(url);
                        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                        return query["v"] ?? "";
                    }
                    else if (url.Contains("youtu.be/"))
                    {
                        return url.Split('/').Last();
                    }
                    else if (url.Contains("embed/"))
                    {
                        return url.Split('/').Last();
                    }
                }
                else if (videoType == "Vimeo")
                {
                    var parts = url.Split('/');
                    return parts.Last();
                }
            }
            catch
            {
                return "";
            }

            return "";
        }

        #endregion
    }
}