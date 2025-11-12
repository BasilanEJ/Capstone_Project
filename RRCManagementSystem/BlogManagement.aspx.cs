using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class BlogManagement : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadBlogList();
            }
        }

        private void LoadBlogList()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== LoadBlogList: Starting ===");

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT BlogID, BlogTitle, BlogDescription, BlogContent, 
                               ImagePath, DisplayOrder, IsActive, CreatedDate
                        FROM [EJBasilan_RRCDB].[EJBasilan_admin].[BlogsCMS]
                        ORDER BY DisplayOrder, BlogID DESC";

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
                            rptBlogList.DataSource = dt;
                            rptBlogList.DataBind();
                            lblBlogCount.Text = $"Total: {dt.Rows.Count} blog(s)";
                            lblNoBlogsMessage.Visible = false;
                        }
                        else
                        {
                            rptBlogList.DataSource = null;
                            rptBlogList.DataBind();
                            lblNoBlogsMessage.Visible = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("❌ Error loading blogs: " + ex.Message);
                ShowAlert("Error", "Failed to load blogs: " + ex.Message, "error");
            }
        }

        protected void btnSaveBlog_Click(object sender, EventArgs e)
        {
            try
            {
                int blogID = Convert.ToInt32(hfBlogID.Value);
                string title = txtBlogTitle.Text.Trim();
                string description = txtBlogDescription.Text.Trim();
                string content = txtBlogContent.Text.Trim();
                int displayOrder = Convert.ToInt32(txtDisplayOrder.Text);
                bool isActive = chkIsActive.Checked;

                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
                {
                    ShowAlert("Validation Error", "Title and Content are required.", "warning");
                    return;
                }

                string imagePath = null;

                // Handle image upload
                if (fuBlogImage.HasFile)
                {
                    string extension = Path.GetExtension(fuBlogImage.FileName).ToLowerInvariant();
                    string[] allowedExtensions = { ".png", ".jpg", ".jpeg" };

                    if (Array.IndexOf(allowedExtensions, extension) < 0)
                    {
                        ShowAlert("Invalid File", "Only PNG or JPEG files are allowed.", "warning");
                        return;
                    }

                    string folderPath = Server.MapPath("~/Images/Blogs/");
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    string filename = "blog_" + Guid.NewGuid().ToString("N") + extension;
                    string savePath = Path.Combine(folderPath, filename);

                    fuBlogImage.SaveAs(savePath);
                    imagePath = "/Images/Blogs/" + filename;
                }
                else if (blogID == 0)
                {
                    ShowAlert("Validation Error", "Blog image is required.", "warning");
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query;

                    if (blogID == 0) // Insert
                    {
                        query = @"
                            INSERT INTO [EJBasilan_RRCDB].[EJBasilan_admin].[BlogsCMS] 
                            (BlogTitle, BlogDescription, BlogContent, ImagePath, DisplayOrder, IsActive)
                            VALUES (@Title, @Description, @Content, @ImagePath, @DisplayOrder, @IsActive)";
                    }
                    else // Update
                    {
                        if (!string.IsNullOrEmpty(imagePath))
                        {
                            query = @"
                                UPDATE [EJBasilan_RRCDB].[EJBasilan_admin].[BlogsCMS] 
                                SET BlogTitle = @Title, 
                                    BlogDescription = @Description,
                                    BlogContent = @Content, 
                                    ImagePath = @ImagePath, 
                                    DisplayOrder = @DisplayOrder, 
                                    IsActive = @IsActive,
                                    ModifiedDate = GETDATE()
                                WHERE BlogID = @BlogID";
                        }
                        else
                        {
                            query = @"
                                UPDATE [EJBasilan_RRCDB].[EJBasilan_admin].[BlogsCMS] 
                                SET BlogTitle = @Title, 
                                    BlogDescription = @Description,
                                    BlogContent = @Content, 
                                    DisplayOrder = @DisplayOrder, 
                                    IsActive = @IsActive,
                                    ModifiedDate = GETDATE()
                                WHERE BlogID = @BlogID";
                        }
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Title", title);
                        cmd.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description);
                        cmd.Parameters.AddWithValue("@Content", content);
                        cmd.Parameters.AddWithValue("@DisplayOrder", displayOrder);
                        cmd.Parameters.AddWithValue("@IsActive", isActive);

                        if (!string.IsNullOrEmpty(imagePath))
                            cmd.Parameters.AddWithValue("@ImagePath", imagePath);

                        if (blogID > 0)
                            cmd.Parameters.AddWithValue("@BlogID", blogID);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowAlert("Success!", blogID == 0 ? "Blog created successfully!" : "Blog updated successfully!", "success");
                ClearForm();
                LoadBlogList();
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to save blog: " + ex.Message, "error");
            }
        }

        protected void rptBlogList_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int blogID = Convert.ToInt32(e.CommandArgument);

            try
            {
                if (e.CommandName == "Edit")
                {
                    LoadBlogForEdit(blogID);
                }
                else if (e.CommandName == "ToggleActive")
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        string checkQuery = "SELECT IsActive FROM [EJBasilan_RRCDB].[EJBasilan_admin].[BlogsCMS] WHERE BlogID = @BlogID";
                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                        {
                            checkCmd.Parameters.AddWithValue("@BlogID", blogID);
                            conn.Open();
                            bool isActive = Convert.ToBoolean(checkCmd.ExecuteScalar());

                            if (!isActive) // Only unarchive without confirmation
                            {
                                ToggleBlogActive(blogID);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error", ex.Message, "error");
            }
        }

        protected void btnHiddenArchive_Click(object sender, EventArgs e)
        {
            try
            {
                int blogID = Convert.ToInt32(hfArchiveBlogID.Value);
                if (blogID > 0)
                {
                    ToggleBlogActive(blogID);
                    hfArchiveBlogID.Value = "0"; // Reset
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to archive blog: " + ex.Message, "error");
            }
        }

        private void LoadBlogForEdit(int blogID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM [EJBasilan_RRCDB].[EJBasilan_admin].[BlogsCMS] WHERE BlogID = @BlogID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BlogID", blogID);
                        conn.Open();

                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            hfBlogID.Value = blogID.ToString();
                            txtBlogTitle.Text = reader["BlogTitle"].ToString();
                            txtBlogDescription.Text = reader["BlogDescription"].ToString();
                            txtBlogContent.Text = reader["BlogContent"].ToString();
                            txtDisplayOrder.Text = reader["DisplayOrder"].ToString();
                            chkIsActive.Checked = Convert.ToBoolean(reader["IsActive"]);

                            string imagePath = reader["ImagePath"].ToString();
                            if (!string.IsNullOrEmpty(imagePath))
                            {
                                imgCurrentBlog.ImageUrl = imagePath;
                                pnlImagePreview.Visible = true;
                            }

                            lblFormTitle.Text = "Edit Blog";
                            btnCancelEdit.Visible = true;

                            // Scroll to form
                            ScriptManager.RegisterStartupScript(this, GetType(), "scrollToForm",
                                "window.scrollTo({ top: 0, behavior: 'smooth' });", true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to load blog: " + ex.Message, "error");
            }
        }

        private void ToggleBlogActive(int blogID)
        {
            try
            {
                bool wasActive = false;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Check current status
                    string checkQuery = "SELECT IsActive FROM [EJBasilan_RRCDB].[EJBasilan_admin].[BlogsCMS] WHERE BlogID = @BlogID";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@BlogID", blogID);
                        conn.Open();
                        wasActive = Convert.ToBoolean(checkCmd.ExecuteScalar());
                    }

                    // Toggle status
                    string query = @"
                        UPDATE [EJBasilan_RRCDB].[EJBasilan_admin].[BlogsCMS] 
                        SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END,
                            ModifiedDate = GETDATE()
                        WHERE BlogID = @BlogID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BlogID", blogID);
                        cmd.ExecuteNonQuery();
                    }
                }

                string message = wasActive ? "Blog archived successfully!" : "Blog unarchived successfully!";
                ShowAlert("Success", message, "success");
                LoadBlogList();
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to update status: " + ex.Message, "error");
            }
        }

        protected void btnCancelEdit_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            hfBlogID.Value = "0";
            txtBlogTitle.Text = "";
            txtBlogDescription.Text = "";
            txtBlogContent.Text = "";
            txtDisplayOrder.Text = "0";
            chkIsActive.Checked = true;
            pnlImagePreview.Visible = false;
            lblFormTitle.Text = "Add New Blog";
            btnCancelEdit.Visible = false;
        }

        protected string GetShortContent(string content, int maxLength)
        {
            if (string.IsNullOrEmpty(content))
                return "";

            // Remove HTML tags for preview
            content = System.Text.RegularExpressions.Regex.Replace(content, "<.*?>", "");

            if (content.Length <= maxLength)
                return content;

            return content.Substring(0, maxLength) + "...";
        }

        private void ShowAlert(string title, string message, string icon)
        {
            string script = $@"
                Swal.fire({{
                    title: '{title.Replace("'", "\\'")}',
                    text: '{message.Replace("'", "\\'")}',
                    icon: '{icon}',
                    confirmButtonColor: '#2563eb',
                    timer: {(icon == "success" ? "2000" : "0")},
                    showConfirmButton: {(icon == "success" ? "false" : "true")}
                }});";

            ScriptManager.RegisterStartupScript(this, GetType(), "showAlert", script, true);
        }
    }
}