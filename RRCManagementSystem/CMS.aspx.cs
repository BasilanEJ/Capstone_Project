using System;
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

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadCurrentImages();
                LoadFaqs();
            }
        }
        private void LoadFaqs()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT ID, Question, Answer, DisplayOrder, IsActive, CreatedDate " +
                    "FROM FAQs ORDER BY DisplayOrder, ID", conn))
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvFaqs.DataSource = dt;
                    gvFaqs.DataBind();
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to load FAQs: " + ex.Message, "error");
            }
        }


        protected void btnAddFaq_Click(object sender, EventArgs e)
        {
            string question = txtNewFaqQuestion.Text.Trim();
            string answer = txtNewFaqAnswer.Text.Trim();

            if (string.IsNullOrEmpty(question) || string.IsNullOrEmpty(answer))
            {
                ShowAlert("Validation Error", "Please provide both question and answer.", "warning");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(
                    "INSERT INTO FAQs (Question, Answer, DisplayOrder, IsActive, CreatedDate) " +
                    "VALUES (@Question, @Answer, (SELECT ISNULL(MAX(DisplayOrder), 0) + 1 FROM FAQs), 1, GETDATE())",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@Question", question);
                    cmd.Parameters.AddWithValue("@Answer", answer);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                ShowAlert("Success", "FAQ added successfully!", "success");
                txtNewFaqQuestion.Text = "";
                txtNewFaqAnswer.Text = "";
                LoadFaqs();
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to add FAQ: " + ex.Message, "error");
            }
        }

        protected void gvFaqs_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // Only handle custom commands, not built-in GridView commands like Edit, Update, Cancel, Delete
            if (e.CommandName == "MoveUp" || e.CommandName == "MoveDown" || e.CommandName == "ToggleActive")
            {
                try
                {
                    int faqId = Convert.ToInt32(e.CommandArgument);

                    if (e.CommandName == "MoveUp")
                    {
                        MoveFaq(faqId, -1);
                    }
                    else if (e.CommandName == "MoveDown")
                    {
                        MoveFaq(faqId, 1);
                    }
                    else if (e.CommandName == "ToggleActive")
                    {
                        ToggleFaqStatus(faqId);
                    }
                }
                catch (Exception ex)
                {
                    ShowAlert("Error", "Failed to process command: " + ex.Message, "error");
                }
            }
        }

        protected void gvFaqs_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvFaqs.EditIndex = e.NewEditIndex;
            LoadFaqs();
        }

        protected void gvFaqs_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvFaqs.EditIndex = -1;
            LoadFaqs();
        }

        protected void gvFaqs_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            int faqId = Convert.ToInt32(gvFaqs.DataKeys[e.RowIndex].Value);

            TextBox txtQuestion = (TextBox)gvFaqs.Rows[e.RowIndex].FindControl("txtEditQuestion");
            TextBox txtAnswer = (TextBox)gvFaqs.Rows[e.RowIndex].FindControl("txtEditAnswer");

            if (txtQuestion == null || txtAnswer == null)
            {
                ShowAlert("Error", "Could not find edit controls.", "error");
                return;
            }

            string question = txtQuestion.Text.Trim();
            string answer = txtAnswer.Text.Trim();

            if (string.IsNullOrEmpty(question) || string.IsNullOrEmpty(answer))
            {
                ShowAlert("Validation Error", "Question and answer cannot be empty.", "warning");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE FAQs SET Question = @Question, Answer = @Answer, UpdatedDate = GETDATE() " +
                    "WHERE ID = @ID", conn))
                {
                    cmd.Parameters.AddWithValue("@Question", question);
                    cmd.Parameters.AddWithValue("@Answer", answer);
                    cmd.Parameters.AddWithValue("@ID", faqId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                ShowAlert("Success", "FAQ updated successfully!", "success");
                gvFaqs.EditIndex = -1;
                LoadFaqs();
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to update FAQ: " + ex.Message, "error");
            }
        }

        protected void gvFaqs_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int faqId = Convert.ToInt32(gvFaqs.DataKeys[e.RowIndex].Value);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("DELETE FROM FAQs WHERE ID = @ID", conn))
                {
                    cmd.Parameters.AddWithValue("@ID", faqId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                ShowAlert("Success", "FAQ deleted successfully!", "success");
                LoadFaqs();
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to delete FAQ: " + ex.Message, "error");
            }
        }

        private void MoveFaq(int faqId, int direction)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Get current FAQ details
                    SqlCommand cmdGet = new SqlCommand(
                        "SELECT DisplayOrder FROM FAQs WHERE ID = @ID", conn);
                    cmdGet.Parameters.AddWithValue("@ID", faqId);
                    int currentOrder = (int)cmdGet.ExecuteScalar();

                    int newOrder = currentOrder + direction;

                    // Swap with adjacent FAQ
                    SqlCommand cmdSwap = new SqlCommand(
                        "UPDATE FAQs SET DisplayOrder = @TempOrder WHERE DisplayOrder = @NewOrder; " +
                        "UPDATE FAQs SET DisplayOrder = @NewOrder WHERE ID = @ID; " +
                        "UPDATE FAQs SET DisplayOrder = @CurrentOrder WHERE DisplayOrder = @TempOrder;",
                        conn);

                    cmdSwap.Parameters.AddWithValue("@ID", faqId);
                    cmdSwap.Parameters.AddWithValue("@CurrentOrder", currentOrder);
                    cmdSwap.Parameters.AddWithValue("@NewOrder", newOrder);
                    cmdSwap.Parameters.AddWithValue("@TempOrder", -1);

                    cmdSwap.ExecuteNonQuery();
                }

                LoadFaqs();
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to reorder FAQ: " + ex.Message, "error");
            }
        }

        private void ToggleFaqStatus(int faqId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE FAQs SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END, " +
                    "UpdatedDate = GETDATE() WHERE ID = @ID", conn))
                {
                    cmd.Parameters.AddWithValue("@ID", faqId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                LoadFaqs();
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to toggle FAQ status: " + ex.Message, "error");
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
                        txtVideoUrl.Text = reader["VideoUrl"]?.ToString() ?? "https://www.youtube.com/watch?v=dQw4w9WgXcQ";

                        string videoType = reader["VideoType"]?.ToString() ?? "YouTube";
                        ddlVideoType.SelectedValue = videoType;

                        // C&O and Blogs
                        SetImagePreview(imgCOPreview, reader["COImagePath"]?.ToString(), "/images/c&o.png");
                        SetImagePreview(imgBlog1Preview, reader["Blog1ImagePath"]?.ToString(), "/Images/DIY.jpg");
                        SetImagePreview(imgBlog2Preview, reader["Blog2ImagePath"]?.ToString(), "/Images/blog2.jpg");
                        SetImagePreview(imgBlog3Preview, reader["Blog3ImagePath"]?.ToString(), "/Images/blog3.jpg");

                        // Load Service Content
                        LoadServiceContent(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to load current images: " + ex.Message, "error");
            }
        }

        private void LoadServiceContent(SqlDataReader reader)
        {
            // Baiting System
            txtBaitingTitle.Text = reader["BaitingTitle"]?.ToString() ?? "🛡️ Baiting System";
            txtBaitingDescription.Text = reader["BaitingDescription"]?.ToString() ?? "";
            txtBaitingBullets.Text = reader["BaitingBulletPoints"]?.ToString() ?? "";

            // Termite Prevention
            txtTermitePreventionTitle.Text = reader["TermitePreventionTitle"]?.ToString() ?? "🔰 Termite Prevention";
            txtTermitePreventionDescription.Text = reader["TermitePreventionDescription"]?.ToString() ?? "";
            txtTermitePreventionBullets.Text = reader["TermitePreventionBulletPoints"]?.ToString() ?? "";

            // Soil Poisoning
            txtSoilTitle.Text = reader["SoilTitle"]?.ToString() ?? "🏗️ Soil Poisoning Treatment";
            txtSoilDescription.Text = reader["SoilDescription"]?.ToString() ?? "";
            txtSoilBullets.Text = reader["SoilBulletPoints"]?.ToString() ?? "";

            // Reticulation
            txtReticulationTitle.Text = reader["ReticulationTitle"]?.ToString() ?? "⚙️ Reticulation System";
            txtReticulationDescription.Text = reader["ReticulationDescription"]?.ToString() ?? "";
            txtReticulationBullets.Text = reader["ReticulationBulletPoints"]?.ToString() ?? "";

            // Mound Demolition
            txtMoundTitle.Text = reader["MoundTitle"]?.ToString() ?? "🎯 Mound Demolition";
            txtMoundDescription.Text = reader["MoundDescription"]?.ToString() ?? "";
            txtMoundBullets.Text = reader["MoundBulletPoints"]?.ToString() ?? "";

            // General Pest Control
            txtGeneralPestTitle.Text = reader["GeneralPestTitle"]?.ToString() ?? "🐜 General Pest Control";
            txtGeneralPestDescription.Text = reader["GeneralPestDescription"]?.ToString() ?? "";
            txtGeneralPestBullets.Text = reader["GeneralPestBulletPoints"]?.ToString() ?? "";

            // Tick & Fleas
            txtTickFleasTitle.Text = reader["TickFleasTitle"]?.ToString() ?? "🐕 Tick & Fleas Control";
            txtTickFleasDescription.Text = reader["TickFleasDescription"]?.ToString() ?? "";
            txtTickFleasBullets.Text = reader["TickFleasBulletPoints"]?.ToString() ?? "";

            // Bedbugs
            txtBedbugsTitle.Text = reader["BedbugsTitle"]?.ToString() ?? "🛏️ Bedbugs Control";
            txtBedbugsDescription.Text = reader["BedbugsDescription"]?.ToString() ?? "";
            txtBedbugsBullets.Text = reader["BedbugsBulletPoints"]?.ToString() ?? "";

            // Rats & Rodents
            txtRatsTitle.Text = reader["RatsTitle"]?.ToString() ?? "🐀 Rat & Rodents Control";
            txtRatsDescription.Text = reader["RatsDescription"]?.ToString() ?? "";
            txtRatsBullets.Text = reader["RatsBulletPoints"]?.ToString() ?? "";
        }

        protected void btnUpdateServiceContent_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(@"
                    UPDATE CMSContent SET 
                        BaitingTitle = @BaitingTitle,
                        BaitingDescription = @BaitingDescription,
                        BaitingBulletPoints = @BaitingBullets,
                        
                        TermitePreventionTitle = @TermitePreventionTitle,
                        TermitePreventionDescription = @TermitePreventionDescription,
                        TermitePreventionBulletPoints = @TermitePreventionBullets,
                        
                        SoilTitle = @SoilTitle,
                        SoilDescription = @SoilDescription,
                        SoilBulletPoints = @SoilBullets,
                        
                        ReticulationTitle = @ReticulationTitle,
                        ReticulationDescription = @ReticulationDescription,
                        ReticulationBulletPoints = @ReticulationBullets,
                        
                        MoundTitle = @MoundTitle,
                        MoundDescription = @MoundDescription,
                        MoundBulletPoints = @MoundBullets,
                        
                        GeneralPestTitle = @GeneralPestTitle,
                        GeneralPestDescription = @GeneralPestDescription,
                        GeneralPestBulletPoints = @GeneralPestBullets,
                        
                        TickFleasTitle = @TickFleasTitle,
                        TickFleasDescription = @TickFleasDescription,
                        TickFleasBulletPoints = @TickFleasBullets,
                        
                        BedbugsTitle = @BedbugsTitle,
                        BedbugsDescription = @BedbugsDescription,
                        BedbugsBulletPoints = @BedbugsBullets,
                        
                        RatsTitle = @RatsTitle,
                        RatsDescription = @RatsDescription,
                        RatsBulletPoints = @RatsBullets,
                        
                        LastUpdated = GETDATE()
                    WHERE ID = 1", conn))
                {
                    // Baiting System
                    cmd.Parameters.AddWithValue("@BaitingTitle", txtBaitingTitle.Text.Trim());
                    cmd.Parameters.AddWithValue("@BaitingDescription", txtBaitingDescription.Text.Trim());
                    cmd.Parameters.AddWithValue("@BaitingBullets", txtBaitingBullets.Text.Trim());

                    // Termite Prevention
                    cmd.Parameters.AddWithValue("@TermitePreventionTitle", txtTermitePreventionTitle.Text.Trim());
                    cmd.Parameters.AddWithValue("@TermitePreventionDescription", txtTermitePreventionDescription.Text.Trim());
                    cmd.Parameters.AddWithValue("@TermitePreventionBullets", txtTermitePreventionBullets.Text.Trim());

                    // Soil Poisoning
                    cmd.Parameters.AddWithValue("@SoilTitle", txtSoilTitle.Text.Trim());
                    cmd.Parameters.AddWithValue("@SoilDescription", txtSoilDescription.Text.Trim());
                    cmd.Parameters.AddWithValue("@SoilBullets", txtSoilBullets.Text.Trim());

                    // Reticulation
                    cmd.Parameters.AddWithValue("@ReticulationTitle", txtReticulationTitle.Text.Trim());
                    cmd.Parameters.AddWithValue("@ReticulationDescription", txtReticulationDescription.Text.Trim());
                    cmd.Parameters.AddWithValue("@ReticulationBullets", txtReticulationBullets.Text.Trim());

                    // Mound Demolition
                    cmd.Parameters.AddWithValue("@MoundTitle", txtMoundTitle.Text.Trim());
                    cmd.Parameters.AddWithValue("@MoundDescription", txtMoundDescription.Text.Trim());
                    cmd.Parameters.AddWithValue("@MoundBullets", txtMoundBullets.Text.Trim());

                    // General Pest Control
                    cmd.Parameters.AddWithValue("@GeneralPestTitle", txtGeneralPestTitle.Text.Trim());
                    cmd.Parameters.AddWithValue("@GeneralPestDescription", txtGeneralPestDescription.Text.Trim());
                    cmd.Parameters.AddWithValue("@GeneralPestBullets", txtGeneralPestBullets.Text.Trim());

                    // Tick & Fleas
                    cmd.Parameters.AddWithValue("@TickFleasTitle", txtTickFleasTitle.Text.Trim());
                    cmd.Parameters.AddWithValue("@TickFleasDescription", txtTickFleasDescription.Text.Trim());
                    cmd.Parameters.AddWithValue("@TickFleasBullets", txtTickFleasBullets.Text.Trim());

                    // Bedbugs
                    cmd.Parameters.AddWithValue("@BedbugsTitle", txtBedbugsTitle.Text.Trim());
                    cmd.Parameters.AddWithValue("@BedbugsDescription", txtBedbugsDescription.Text.Trim());
                    cmd.Parameters.AddWithValue("@BedbugsBullets", txtBedbugsBullets.Text.Trim());

                    // Rats & Rodents
                    cmd.Parameters.AddWithValue("@RatsTitle", txtRatsTitle.Text.Trim());
                    cmd.Parameters.AddWithValue("@RatsDescription", txtRatsDescription.Text.Trim());
                    cmd.Parameters.AddWithValue("@RatsBullets", txtRatsBullets.Text.Trim());

                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        ShowAlert("Success", "All service content updated successfully!", "success");
                        LoadCurrentImages();
                    }
                    else
                    {
                        ShowAlert("Warning", "No changes were made.", "warning");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to update service content: " + ex.Message, "error");
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

        protected void btnUpdateServices_Click(object sender, EventArgs e)
        {
            bool updated = false;
            var serviceSize = ImageHelper.RecommendedDimensions.ServiceCard;

            if (fuBaiting.HasFile)
            {
                string path = SaveUploadedImageWithResize(fuBaiting, "Baiting", serviceSize.Width, serviceSize.Height);
                if (!string.IsNullOrEmpty(path))
                {
                    UpdateDatabase("BaitingImagePath", path);
                    updated = true;
                }
            }

            if (fuTermitePrevention.HasFile)
            {
                string path = SaveUploadedImageWithResize(fuTermitePrevention, "TermitePrevention", serviceSize.Width, serviceSize.Height);
                if (!string.IsNullOrEmpty(path))
                {
                    UpdateDatabase("TermitePreventionImagePath", path);
                    updated = true;
                }
            }

            if (fuSoil.HasFile)
            {
                string path = SaveUploadedImageWithResize(fuSoil, "Soil", serviceSize.Width, serviceSize.Height);
                if (!string.IsNullOrEmpty(path))
                {
                    UpdateDatabase("SoilImagePath", path);
                    updated = true;
                }
            }

            if (fuReticulation.HasFile)
            {
                string path = SaveUploadedImageWithResize(fuReticulation, "Reticulation", serviceSize.Width, serviceSize.Height);
                if (!string.IsNullOrEmpty(path))
                {
                    UpdateDatabase("ReticulationImagePath", path);
                    updated = true;
                }
            }

            if (fuMound.HasFile)
            {
                string path = SaveUploadedImageWithResize(fuMound, "Mound", serviceSize.Width, serviceSize.Height);
                if (!string.IsNullOrEmpty(path))
                {
                    UpdateDatabase("MoundImagePath", path);
                    updated = true;
                }
            }

            if (fuGeneralPest.HasFile)
            {
                string path = SaveUploadedImageWithResize(fuGeneralPest, "GeneralPest", serviceSize.Width, serviceSize.Height);
                if (!string.IsNullOrEmpty(path))
                {
                    UpdateDatabase("GeneralPestImagePath", path);
                    updated = true;
                }
            }

            if (fuTickFleas.HasFile)
            {
                string path = SaveUploadedImageWithResize(fuTickFleas, "TickFleas", serviceSize.Width, serviceSize.Height);
                if (!string.IsNullOrEmpty(path))
                {
                    UpdateDatabase("TickFleasImagePath", path);
                    updated = true;
                }
            }

            if (fuBedbugs.HasFile)
            {
                string path = SaveUploadedImageWithResize(fuBedbugs, "Bedbugs", serviceSize.Width, serviceSize.Height);
                if (!string.IsNullOrEmpty(path))
                {
                    UpdateDatabase("BedbugsImagePath", path);
                    updated = true;
                }
            }

            if (fuRats.HasFile)
            {
                string path = SaveUploadedImageWithResize(fuRats, "Rats", serviceSize.Width, serviceSize.Height);
                if (!string.IsNullOrEmpty(path))
                {
                    UpdateDatabase("RatsImagePath", path);
                    updated = true;
                }
            }

            if (updated)
            {
                ShowAlert("Success", "Service images updated successfully! All images optimized to 400x400px", "success");
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