using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Ganss.Xss; // Install via NuGet: Install-Package HtmlSanitizer

namespace RRCManagementSystem
{
    public partial class BlogManagement : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        // Constants for validation
        private const int MAX_TITLE_LENGTH = 200;
        private const int MAX_DESCRIPTION_LENGTH = 500;
        private const int MAX_AUTHOR_LENGTH = 100;
        private const int MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB
        private const int MIN_DISPLAY_ORDER = 0;
        private const int MAX_DISPLAY_ORDER = 9999;
        private const int MIN_READ_TIME = 1;
        private const int MAX_READ_TIME = 120;

        #region Security & Initialization

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // CSRF Protection
            if (Session != null && !string.IsNullOrEmpty(Session.SessionID))
            {
                ViewStateUserKey = Session.SessionID;
            }
        }

        /// <summary>
        /// Authorization check - Ensure only authenticated administrators can access
        protected void Page_Load(object sender, EventArgs e)
        {
            // Check if user is logged in
            if (Session["UserRole"] == null)
            {
                Response.Redirect("~/Login.aspx?ReturnUrl=" + Server.UrlEncode(Request.RawUrl), false);
                return;
            }

            // Allow only SuperAdmin
            if (Session["UserRole"].ToString() != "SuperAdmin")
            {
                Response.Redirect("~/Login.aspx", false);
                return;
            }

            // Load data only once when page is first loaded
            if (!IsPostBack)
            {
                LoadBlogs();
            }
        }


        #endregion

        #region HTML Sanitization

        /// <summary>
        /// Creates and configures HTML sanitizer with strict whitelist
        /// Prevents XSS attacks by allowing only safe HTML tags and attributes
        /// </summary>
        private HtmlSanitizer CreateSanitizer()
        {
            var sanitizer = new HtmlSanitizer();

            // Clear default allowed tags/attributes to start with strict whitelist
            sanitizer.AllowedTags.Clear();
            sanitizer.AllowedAttributes.Clear();
            sanitizer.AllowedCssProperties.Clear();
            sanitizer.AllowedSchemes.Clear();

            // Whitelist safe HTML tags
            var allowedTags = new[]
            {
                "p", "br", "div", "span",
                "h1", "h2", "h3", "h4", "h5", "h6",
                "strong", "b", "em", "i", "u", "s", "strike",
                "ul", "ol", "li",
                "a", "img",
                "table", "thead", "tbody", "tfoot", "tr", "th", "td",
                "blockquote", "pre", "code"
            };
            foreach (var tag in allowedTags)
            {
                sanitizer.AllowedTags.Add(tag);
            }

            // Whitelist safe attributes
            var allowedAttributes = new[]
            {
                "href", "title", "target", "rel",           // for links
                "src", "alt", "width", "height",            // for images
                "style",                                     // for inline styles (with CSS property whitelist)
                "class",                                     // for CSS classes
                "border", "cellpadding", "cellspacing",     // for tables
                "colspan", "rowspan"                        // for table cells
            };
            foreach (var attr in allowedAttributes)
            {
                sanitizer.AllowedAttributes.Add(attr);
            }

            // Whitelist safe CSS properties (only if 'style' attribute is needed)
            var allowedCssProperties = new[]
            {
                "color", "background-color",
                "font-size", "font-weight", "font-family", "font-style",
                "text-align", "text-decoration", "line-height",
                "margin", "margin-top", "margin-bottom", "margin-left", "margin-right",
                "padding", "padding-top", "padding-bottom", "padding-left", "padding-right",
                "border", "border-collapse",
                "width", "height", "max-width", "max-height"
            };
            foreach (var prop in allowedCssProperties)
            {
                sanitizer.AllowedCssProperties.Add(prop);
            }

            // Whitelist safe URL schemes
            sanitizer.AllowedSchemes.Add("http");
            sanitizer.AllowedSchemes.Add("https");
            sanitizer.AllowedSchemes.Add("mailto");

            // Additional security settings
            sanitizer.AllowDataAttributes = false;

            // Configure link handling for security
            sanitizer.FilterUrl += (sender, e) =>
            {
                // Add rel="noopener noreferrer" to external links for security
                if (e.OriginalUrl.StartsWith("http") && !e.OriginalUrl.Contains(Request.Url.Host))
                {
                    e.SanitizedUrl = e.OriginalUrl;
                }
            };

            return sanitizer;
        }

        /// <summary>
        /// Sanitizes HTML input to prevent XSS attacks
        /// </summary>
        private string SanitizeHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            var sanitizer = CreateSanitizer();
            return sanitizer.Sanitize(html);
        }

        /// <summary>
        /// Additional output encoding for GridView display (defense in depth)
        /// </summary>
        protected string GetSafeImageUrl(object imagePathObj)
        {
            if (imagePathObj == null || imagePathObj == DBNull.Value)
                return "/images/no-image.jpg"; // Default placeholder

            string imagePath = imagePathObj.ToString();

            // Validate path format
            if (string.IsNullOrWhiteSpace(imagePath) || !imagePath.StartsWith("/Uploads/Blogs/"))
                return "/images/no-image.jpg";

            // HTML encode for safety
            return Server.HtmlEncode(imagePath);
        }

        /// <summary>
        /// Safely renders blog link with XSS protection
        /// </summary>
        protected string GetSafeLinkButton(object linkObj)
        {
            if (linkObj == null || linkObj == DBNull.Value)
                return "<span class='text-muted'>N/A</span>";

            string link = linkObj.ToString();
            if (string.IsNullOrWhiteSpace(link))
                return "<span class='text-muted'>N/A</span>";

            // Encode for display - use HttpUtility for attribute encoding
            string safeLink = HttpUtility.HtmlEncode(link);
            string safeHrefAttribute = HttpUtility.HtmlAttributeEncode(link);
            return $"<a href='{safeHrefAttribute}' target='_blank' class='btn btn-sm btn-outline-info'>🔗 View</a>";
        }

        #endregion

        #region Input Validation

        /// <summary>
        /// Validates and sanitizes text input
        /// </summary>
        private string ValidateAndSanitizeText(string input, int maxLength, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Trim whitespace
            input = input.Trim();

            // Check length
            if (input.Length > maxLength)
            {
                throw new ArgumentException($"{fieldName} exceeds maximum length of {maxLength} characters.");
            }

            // Remove any potentially dangerous characters (basic check)
            // The HTML sanitizer handles HTML content specifically
            return input;
        }

        /// <summary>
        /// Validates integer input within range
        /// </summary>
        private int ValidateInteger(string input, int minValue, int maxValue, int defaultValue, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(input))
                return defaultValue;

            if (!int.TryParse(input, out int result))
            {
                throw new ArgumentException($"{fieldName} must be a valid number.");
            }

            if (result < minValue || result > maxValue)
            {
                throw new ArgumentException($"{fieldName} must be between {minValue} and {maxValue}.");
            }

            return result;
        }

        #endregion

        #region Database Operations

        /// <summary>
        /// Loads all blogs from database with error handling
        /// </summary>
        private void LoadBlogs()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Use parameterized query (not needed here, but good practice)
                    string query = @"
                        SELECT BlogID, BlogTitle, BlogDescription, BlogImagePath,
                               BlogLink, DisplayOrder, IsActive, CreatedDate, LastUpdated
                        FROM Blogs
                        ORDER BY DisplayOrder, BlogID DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Set command timeout for long-running queries
                        cmd.CommandTimeout = 30;

                        conn.Open();

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            gvBlogs.DataSource = dt;
                            gvBlogs.DataBind();
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                // Log SQL exception
                LogError("LoadBlogs SQL Error", sqlEx);
                ShowAlert("Error", "Database error occurred while loading blogs. Please try again.", "error");
            }
            catch (Exception ex)
            {
                // Log general exception
                LogError("LoadBlogs Error", ex);
                ShowAlert("Error", "An unexpected error occurred while loading blogs.", "error");
            }
        }

        #endregion

        #region Add Blog

        /// <summary>
        /// Handles adding new blog post with comprehensive validation
        /// </summary>
        protected void btnAddBlog_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;

            try
            {
                // Get and sanitize HTML content
                string rawHtml = txtBlogContent.Text;

                // Validate content is not empty
                if (string.IsNullOrWhiteSpace(rawHtml) || rawHtml == "<p><br></p>" || rawHtml == "<p></p>")
                {
                    ShowAlert("Warning", "Blog content cannot be empty.", "warning");
                    return;
                }

                // Sanitize HTML to prevent XSS
                string sanitizedHtml = SanitizeHtml(rawHtml);

                // Check if sanitization removed everything (likely malicious input)
                if (string.IsNullOrWhiteSpace(sanitizedHtml))
                {
                    ShowAlert("Warning", "Blog content appears invalid or contains disallowed HTML. Please review your content.", "warning");
                    return;
                }

                // Validate and sanitize text inputs
                string blogTitle = ValidateAndSanitizeText(txtBlogTitle.Text, MAX_TITLE_LENGTH, "Blog Title");
                string blogDescription = ValidateAndSanitizeText(txtBlogDescription.Text, MAX_DESCRIPTION_LENGTH, "Blog Description");
                string blogLink = ValidateAndSanitizeText(txtBlogLink.Text, 200, "Blog Link");
                string author = ValidateAndSanitizeText(txtAuthor.Text, MAX_AUTHOR_LENGTH, "Author");

                if (string.IsNullOrWhiteSpace(blogTitle))
                {
                    ShowAlert("Warning", "Blog title is required.", "warning");
                    return;
                }

                // Set default author if empty
                if (string.IsNullOrWhiteSpace(author))
                {
                    author = "RRC Team";
                }

                // Validate numeric inputs
                int readTime = ValidateInteger(txtReadTime.Text, MIN_READ_TIME, MAX_READ_TIME, 5, "Read Time");
                int displayOrder = ValidateInteger(txtDisplayOrder.Text, MIN_DISPLAY_ORDER, MAX_DISPLAY_ORDER, 0, "Display Order");

                // Handle image upload
                string imagePath = SaveBlogImage();

                // Insert into database using parameterized query
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        INSERT INTO Blogs (BlogTitle, BlogDescription, BlogContent, BlogImagePath,
                                           BlogLink, Author, ReadTime, DisplayOrder, IsActive, CreatedDate, LastUpdated)
                        VALUES (@BlogTitle, @BlogDescription, @BlogContent, @BlogImagePath,
                                @BlogLink, @Author, @ReadTime, @DisplayOrder, @IsActive, GETDATE(), GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Use parameterized queries to prevent SQL injection
                        cmd.Parameters.AddWithValue("@BlogTitle", blogTitle);
                        cmd.Parameters.AddWithValue("@BlogDescription",
                            string.IsNullOrWhiteSpace(blogDescription) ? (object)DBNull.Value : blogDescription);
                        cmd.Parameters.AddWithValue("@BlogContent", sanitizedHtml);
                        cmd.Parameters.AddWithValue("@BlogImagePath",
                            string.IsNullOrEmpty(imagePath) ? (object)DBNull.Value : imagePath);
                        cmd.Parameters.AddWithValue("@BlogLink",
                            string.IsNullOrWhiteSpace(blogLink) ? (object)DBNull.Value : blogLink);
                        cmd.Parameters.AddWithValue("@Author", author);
                        cmd.Parameters.AddWithValue("@ReadTime", readTime);
                        cmd.Parameters.AddWithValue("@DisplayOrder", displayOrder);
                        cmd.Parameters.AddWithValue("@IsActive", chkIsActive.Checked);

                        cmd.CommandTimeout = 30;
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowAlert("Success!", "Blog post added successfully!", "success");
                ClearForm();
                LoadBlogs();
            }
            catch (ArgumentException argEx)
            {
                // Validation error - show to user
                ShowAlert("Validation Error", argEx.Message, "warning");
            }
            catch (SqlException sqlEx)
            {
                LogError("AddBlog SQL Error", sqlEx);
                ShowAlert("Error", "Database error occurred. Please try again.", "error");
            }
            catch (Exception ex)
            {
                LogError("AddBlog Error", ex);
                ShowAlert("Error", "An unexpected error occurred. Please try again.", "error");
            }
        }

        #endregion

        #region Update Blog

        /// <summary>
        /// Handles updating existing blog post
        /// </summary>
        protected void btnUpdateBlog_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;

            try
            {
                // Validate BlogID
                if (!int.TryParse(hfBlogID.Value, out int blogId) || blogId <= 0)
                {
                    ShowAlert("Error", "Invalid Blog ID.", "error");
                    return;
                }

                // Get and sanitize HTML content
                string rawHtml = txtBlogContent.Text;

                if (string.IsNullOrWhiteSpace(rawHtml) || rawHtml == "<p><br></p>" || rawHtml == "<p></p>")
                {
                    ShowAlert("Warning", "Blog content cannot be empty.", "warning");
                    return;
                }

                string sanitizedHtml = SanitizeHtml(rawHtml);

                if (string.IsNullOrWhiteSpace(sanitizedHtml))
                {
                    ShowAlert("Warning", "Blog content appears invalid or contains disallowed HTML.", "warning");
                    return;
                }

                // Validate and sanitize inputs
                string blogTitle = ValidateAndSanitizeText(txtBlogTitle.Text, MAX_TITLE_LENGTH, "Blog Title");
                string blogDescription = ValidateAndSanitizeText(txtBlogDescription.Text, MAX_DESCRIPTION_LENGTH, "Blog Description");
                string blogLink = ValidateAndSanitizeText(txtBlogLink.Text, 200, "Blog Link");
                string author = ValidateAndSanitizeText(txtAuthor.Text, MAX_AUTHOR_LENGTH, "Author");

                if (string.IsNullOrWhiteSpace(blogTitle))
                {
                    ShowAlert("Warning", "Blog title is required.", "warning");
                    return;
                }

                if (string.IsNullOrWhiteSpace(author))
                {
                    author = "RRC Team";
                }

                int readTime = ValidateInteger(txtReadTime.Text, MIN_READ_TIME, MAX_READ_TIME, 5, "Read Time");
                int displayOrder = ValidateInteger(txtDisplayOrder.Text, MIN_DISPLAY_ORDER, MAX_DISPLAY_ORDER, 0, "Display Order");

                // Handle image upload
                string imagePath = SaveBlogImage();
                string oldImagePath = hfCurrentImagePath.Value;

                // If no new image, keep current
                if (string.IsNullOrEmpty(imagePath))
                {
                    imagePath = oldImagePath;
                }

                // Update database
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        UPDATE Blogs
                        SET BlogTitle = @BlogTitle,
                            BlogDescription = @BlogDescription,
                            BlogContent = @BlogContent,
                            BlogImagePath = @BlogImagePath,
                            BlogLink = @BlogLink,
                            Author = @Author,
                            ReadTime = @ReadTime,
                            DisplayOrder = @DisplayOrder,
                            IsActive = @IsActive,
                            LastUpdated = GETDATE()
                        WHERE BlogID = @BlogID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BlogID", blogId);
                        cmd.Parameters.AddWithValue("@BlogTitle", blogTitle);
                        cmd.Parameters.AddWithValue("@BlogDescription",
                            string.IsNullOrWhiteSpace(blogDescription) ? (object)DBNull.Value : blogDescription);
                        cmd.Parameters.AddWithValue("@BlogContent", sanitizedHtml);
                        cmd.Parameters.AddWithValue("@BlogImagePath",
                            string.IsNullOrEmpty(imagePath) ? (object)DBNull.Value : imagePath);
                        cmd.Parameters.AddWithValue("@BlogLink",
                            string.IsNullOrWhiteSpace(blogLink) ? (object)DBNull.Value : blogLink);
                        cmd.Parameters.AddWithValue("@Author", author);
                        cmd.Parameters.AddWithValue("@ReadTime", readTime);
                        cmd.Parameters.AddWithValue("@DisplayOrder", displayOrder);
                        cmd.Parameters.AddWithValue("@IsActive", chkIsActive.Checked);

                        cmd.CommandTimeout = 30;
                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            ShowAlert("Warning", "Blog post not found or no changes were made.", "warning");
                            return;
                        }
                    }
                }

                // Delete old image if a new one was uploaded
                if (!string.IsNullOrEmpty(imagePath) && !string.IsNullOrEmpty(oldImagePath) &&
                    imagePath != oldImagePath)
                {
                    DeleteImageFile(oldImagePath);
                }

                ShowAlert("Success!", "Blog post updated successfully!", "success");
                ClearForm();
                LoadBlogs();
            }
            catch (ArgumentException argEx)
            {
                ShowAlert("Validation Error", argEx.Message, "warning");
            }
            catch (SqlException sqlEx)
            {
                LogError("UpdateBlog SQL Error", sqlEx);
                ShowAlert("Error", "Database error occurred. Please try again.", "error");
            }
            catch (Exception ex)
            {
                LogError("UpdateBlog Error", ex);
                ShowAlert("Error", "An unexpected error occurred. Please try again.", "error");
            }
        }

        #endregion

        #region GridView Actions

        /// <summary>
        /// Handles GridView row commands (Edit, Toggle, Delete)
        /// </summary>
        protected void gvBlogs_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!int.TryParse(e.CommandArgument?.ToString(), out int blogId) || blogId <= 0)
            {
                ShowAlert("Error", "Invalid blog identifier.", "error");
                return;
            }

            try
            {
                switch (e.CommandName)
                {
                    case "EditBlog":
                        LoadBlogForEdit(blogId);
                        break;
                    case "ToggleStatus":
                        ToggleBlogStatus(blogId);
                        break;
                    case "DeleteBlog":
                        DeleteBlog(blogId);
                        break;
                }
            }
            catch (Exception ex)
            {
                LogError($"RowCommand ({e.CommandName}) Error", ex);
                ShowAlert("Error", "An error occurred processing your request.", "error");
            }
        }

        /// <summary>
        /// Loads blog data for editing
        /// </summary>
        private void LoadBlogForEdit(int blogId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT BlogID, BlogTitle, BlogDescription, BlogContent, BlogImagePath,
                               BlogLink, Author, ReadTime, DisplayOrder, IsActive
                        FROM Blogs
                        WHERE BlogID = @BlogID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BlogID", blogId);
                        cmd.CommandTimeout = 30;
                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hfBlogID.Value = reader["BlogID"].ToString();
                                hfCurrentImagePath.Value = reader["BlogImagePath"] != DBNull.Value
                                    ? reader["BlogImagePath"].ToString() : "";

                                txtBlogTitle.Text = reader["BlogTitle"].ToString();
                                txtBlogDescription.Text = reader["BlogDescription"] != DBNull.Value
                                    ? reader["BlogDescription"].ToString() : "";
                                txtBlogContent.Text = reader["BlogContent"] != DBNull.Value
                                    ? reader["BlogContent"].ToString() : "";
                                txtBlogLink.Text = reader["BlogLink"] != DBNull.Value
                                    ? reader["BlogLink"].ToString() : "";
                                txtAuthor.Text = reader["Author"] != DBNull.Value
                                    ? reader["Author"].ToString() : "RRC Team";
                                txtReadTime.Text = reader["ReadTime"].ToString();
                                txtDisplayOrder.Text = reader["DisplayOrder"].ToString();
                                chkIsActive.Checked = Convert.ToBoolean(reader["IsActive"]);

                                // Update image preview
                                if (!string.IsNullOrEmpty(hfCurrentImagePath.Value))
                                {
                                    lblCurrentImage.Text = Path.GetFileName(hfCurrentImagePath.Value);
                                    imgPreview.ImageUrl = hfCurrentImagePath.Value;
                                    imgPreview.Visible = true;
                                }
                                else
                                {
                                    lblCurrentImage.Text = "None";
                                    imgPreview.Visible = false;
                                }

                                // Update UI for edit mode
                                lblFormTitle.Text = "✏️ Edit Blog Post";
                                btnAddBlog.Visible = false;
                                btnUpdateBlog.Visible = true;
                                btnCancelEdit.Visible = true;

                                UpdatePanelForm.Update();
                            }
                            else
                            {
                                ShowAlert("Error", "Blog post not found.", "error");
                                ClearForm();
                            }
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                LogError("LoadBlogForEdit SQL Error", sqlEx);
                ShowAlert("Error", "Failed to load blog for editing.", "error");
            }
            catch (Exception ex)
            {
                LogError("LoadBlogForEdit Error", ex);
                ShowAlert("Error", "An unexpected error occurred.", "error");
            }
        }

        /// <summary>
        /// Toggles blog active/inactive status
        /// </summary>
        private void ToggleBlogStatus(int blogId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        UPDATE Blogs
                        SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END,
                            LastUpdated = GETDATE()
                        WHERE BlogID = @BlogID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BlogID", blogId);
                        cmd.CommandTimeout = 30;
                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            ShowAlert("Success!", "Blog status updated successfully!", "success");
                        }
                        else
                        {
                            ShowAlert("Warning", "Blog post not found.", "warning");
                        }
                    }
                }
                LoadBlogs();
            }
            catch (SqlException sqlEx)
            {
                LogError("ToggleBlogStatus SQL Error", sqlEx);
                ShowAlert("Error", "Failed to toggle blog status.", "error");
            }
            catch (Exception ex)
            {
                LogError("ToggleBlogStatus Error", ex);
                ShowAlert("Error", "An unexpected error occurred.", "error");
            }
        }

        /// <summary>
        /// Deletes blog post and associated image
        /// </summary>
        private void DeleteBlog(int blogId)
        {
            try
            {
                string imagePath = "";

                // Get image path before deletion
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string selectQuery = "SELECT BlogImagePath FROM Blogs WHERE BlogID = @BlogID";
                    using (SqlCommand cmd = new SqlCommand(selectQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@BlogID", blogId);
                        cmd.CommandTimeout = 30;
                        conn.Open();
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            imagePath = result.ToString();
                        }
                    }
                }

                // Delete from database
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM Blogs WHERE BlogID = @BlogID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BlogID", blogId);
                        cmd.CommandTimeout = 30;
                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            ShowAlert("Warning", "Blog post not found.", "warning");
                            LoadBlogs();
                            return;
                        }
                    }
                }

                // Delete image file
                DeleteImageFile(imagePath);

                // Clear form if editing the deleted blog
                if (hfBlogID.Value == blogId.ToString())
                {
                    ClearForm();
                }

                ShowAlert("Success!", "Blog deleted successfully!", "success");
                LoadBlogs();
            }
            catch (SqlException sqlEx)
            {
                LogError("DeleteBlog SQL Error", sqlEx);
                ShowAlert("Error", "Failed to delete blog.", "error");
            }
            catch (Exception ex)
            {
                LogError("DeleteBlog Error", ex);
                ShowAlert("Error", "An unexpected error occurred.", "error");
            }
        }

        /// <summary>
        /// Handles row data binding (optional styling)
        /// </summary>
        protected void gvBlogs_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            // Optional: Add custom styling or logic here
        }

        /// <summary>
        /// Cancels edit mode and clears form
        /// </summary>
        protected void btnCancelEdit_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        #endregion

        #region File Upload

        /// <summary>
        /// Saves uploaded blog image with comprehensive validation
        /// </summary>
        private string SaveBlogImage()
        {
            if (!fuBlogImage.HasFile)
                return null;

            try
            {
                // Validate file extension
                string fileExtension = Path.GetExtension(fuBlogImage.FileName).ToLowerInvariant();
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

                if (Array.IndexOf(allowedExtensions, fileExtension) == -1)
                {
                    ShowAlert("Warning", "Invalid file type. Only JPG, PNG, GIF, and WebP images are allowed.", "warning");
                    return null;
                }

                // Validate content type
                string contentType = fuBlogImage.PostedFile.ContentType.ToLowerInvariant();
                string[] allowedContentTypes = { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };

                if (Array.IndexOf(allowedContentTypes, contentType) == -1)
                {
                    ShowAlert("Warning", "Invalid file content type detected.", "warning");
                    return null;
                }

                // Validate file size
                int fileSize = fuBlogImage.PostedFile.ContentLength;
                if (fileSize <= 0 || fileSize > MAX_FILE_SIZE)
                {
                    ShowAlert("Warning", $"File size must be between 1 byte and {MAX_FILE_SIZE / 1024 / 1024}MB.", "warning");
                    return null;
                }

                // Create upload directory if it doesn't exist
                string uploadFolder = Server.MapPath("~/Uploads/Blogs/");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // Generate unique filename to prevent overwriting and path traversal attacks
                string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                string filePath = Path.Combine(uploadFolder, uniqueFileName);

                // Additional security: Validate the resolved path is within upload directory
                string resolvedPath = Path.GetFullPath(filePath);
                string resolvedUploadFolder = Path.GetFullPath(uploadFolder);

                if (!resolvedPath.StartsWith(resolvedUploadFolder))
                {
                    ShowAlert("Error", "Invalid file path detected.", "error");
                    return null;
                }

                // Save the file
                fuBlogImage.SaveAs(filePath);

                // Return relative URL path for database storage
                return "/Uploads/Blogs/" + uniqueFileName;
            }
            catch (IOException ioEx)
            {
                LogError("SaveBlogImage IO Error", ioEx);
                ShowAlert("Error", "Failed to save image file. Please try again.", "error");
                return null;
            }
            catch (Exception ex)
            {
                LogError("SaveBlogImage Error", ex);
                ShowAlert("Error", "An error occurred while uploading the image.", "error");
                return null;
            }
        }

        /// <summary>
        /// Safely deletes image file with validation
        /// </summary>
        private void DeleteImageFile(string relativeImagePath)
        {
            if (string.IsNullOrWhiteSpace(relativeImagePath))
                return;

            // Validate path starts with expected prefix (prevent path traversal)
            if (!relativeImagePath.StartsWith("/Uploads/Blogs/"))
            {
                LogError("DeleteImageFile", new Exception($"Invalid path format: {relativeImagePath}"));
                return;
            }

            try
            {
                string physicalPath = Server.MapPath(relativeImagePath);

                // Additional security: Ensure resolved path is within Uploads/Blogs directory
                string uploadsFolder = Server.MapPath("~/Uploads/Blogs/");
                string resolvedPath = Path.GetFullPath(physicalPath);
                string resolvedUploadFolder = Path.GetFullPath(uploadsFolder);

                if (!resolvedPath.StartsWith(resolvedUploadFolder))
                {
                    LogError("DeleteImageFile", new Exception($"Path traversal attempt: {relativeImagePath}"));
                    return;
                }

                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                }
            }
            catch (IOException ioEx)
            {
                LogError($"DeleteImageFile IO Error: {relativeImagePath}", ioEx);
            }
            catch (Exception ex)
            {
                LogError($"DeleteImageFile Error: {relativeImagePath}", ex);
            }
        }

        #endregion

        #region Form Management

        /// <summary>
        /// Clears form and resets to add mode
        /// </summary>
        private void ClearForm()
        {
            hfBlogID.Value = "";
            hfCurrentImagePath.Value = "";
            txtBlogTitle.Text = "";
            txtBlogDescription.Text = "";
            txtBlogContent.Text = "";
            txtBlogLink.Text = "";
            txtAuthor.Text = "RRC Team";
            txtReadTime.Text = "5";
            txtDisplayOrder.Text = "0";
            chkIsActive.Checked = true;
            lblCurrentImage.Text = "None";
            imgPreview.Visible = false;
            imgPreview.ImageUrl = "";

            lblFormTitle.Text = "📝 Add New Blog Post";
            btnAddBlog.Visible = true;
            btnUpdateBlog.Visible = false;
            btnCancelEdit.Visible = false;

            UpdatePanelForm.Update();
        }

        #endregion

        #region Alert & Logging

        /// <summary>
        /// Displays SweetAlert with XSS-safe content
        /// </summary>
        private void ShowAlert(string title, string message, string icon)
        {
            // Sanitize alert parameters to prevent XSS in JavaScript context
            string safeTitle = HttpUtility.JavaScriptStringEncode(title ?? "");
            string safeMessage = HttpUtility.JavaScriptStringEncode(message ?? "");
            string safeIcon = HttpUtility.JavaScriptStringEncode(icon ?? "info");

            // Validate icon is one of the expected values
            string[] validIcons = { "success", "error", "warning", "info", "question" };
            if (Array.IndexOf(validIcons, safeIcon.ToLower()) == -1)
            {
                safeIcon = "info";
            }

            string script = $@"
                Swal.fire({{
                    title: '{safeTitle}',
                    text: '{safeMessage}',
                    icon: '{safeIcon}',
                    confirmButtonColor: '#667eea',
                    confirmButtonText: 'OK'
                }});";

            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                $"SweetAlert_{Guid.NewGuid()}",
                script,
                true
            );
        }

        /// <summary>
        /// Logs errors for debugging and monitoring
        /// </summary>
        private void LogError(string context, Exception ex)
        {
            // IMPORTANT: Implement proper logging mechanism
            // Options:
            // 1. Log to file
            // 2. Log to database
            // 3. Use logging framework (log4net, NLog, Serilog)
            // 4. Application Insights / Azure Monitor

            try
            {
                // Example: Simple file logging (replace with your logging solution)
                string logPath = Server.MapPath("~/App_Data/Logs/");
                if (!Directory.Exists(logPath))
                {
                    Directory.CreateDirectory(logPath);
                }

                string logFile = Path.Combine(logPath, $"BlogManagement_{DateTime.Now:yyyyMMdd}.log");
                string logEntry = $@"
[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]
Context: {context}
User: {User.Identity.Name ?? "Anonymous"}
Message: {ex.Message}
StackTrace: {ex.StackTrace}
InnerException: {ex.InnerException?.Message ?? "None"}
----------------------------------------
";
                File.AppendAllText(logFile, logEntry);

                // Also log to System Diagnostics for debugging
                System.Diagnostics.Debug.WriteLine($"[{context}] {ex.Message}");
                System.Diagnostics.Trace.TraceError($"[{context}] {ex.ToString()}");
            }
            catch
            {
                // If logging fails, write to event viewer as last resort
                try
                {
                    System.Diagnostics.EventLog.WriteEntry(
                        "RRCManagementSystem",
                        $"Logging Error - {context}: {ex.Message}",
                        System.Diagnostics.EventLogEntryType.Error
                    );
                }
                catch
                {
                    // Silently fail - don't let logging errors break the application
                }
            }
        }

        #endregion
    }
}