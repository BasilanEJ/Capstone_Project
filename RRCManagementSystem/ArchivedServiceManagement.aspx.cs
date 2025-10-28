using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class ArchivedServiceManagement : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadDeletedServices();

                // Store current tab in ViewState
                ViewState["CurrentTab"] = "Services";
            }
        }

        // ============ Tab Click Events ============
        protected void btnTabServices_Click(object sender, EventArgs e)
        {
            ShowServicesTab();
        }

        protected void btnTabBlogs_Click(object sender, EventArgs e)
        {
            ShowBlogsTab();
        }

        protected void btnTabFaqs_Click(object sender, EventArgs e)
        {
            ShowFaqsTab();
        }

        // ============ Show Services Tab ============
        private void ShowServicesTab()
        {
            pnlServices.Visible = true;
            pnlBlogs.Visible = false;
            pnlFaqs.Visible = false;

            btnTabServices.CssClass = "tab-btn active";
            btnTabBlogs.CssClass = "tab-btn";
            btnTabFaqs.CssClass = "tab-btn";

            ViewState["CurrentTab"] = "Services";
            LoadDeletedServices();
        }

        // ============ Show Blogs Tab ============
        private void ShowBlogsTab()
        {
            pnlServices.Visible = false;
            pnlBlogs.Visible = true;
            pnlFaqs.Visible = false;

            btnTabServices.CssClass = "tab-btn";
            btnTabBlogs.CssClass = "tab-btn active";
            btnTabFaqs.CssClass = "tab-btn";

            ViewState["CurrentTab"] = "Blogs";
            LoadDeletedBlogs();
        }

        // ============ Show FAQs Tab ============
        private void ShowFaqsTab()
        {
            pnlServices.Visible = false;
            pnlBlogs.Visible = false;
            pnlFaqs.Visible = true;

            btnTabServices.CssClass = "tab-btn";
            btnTabBlogs.CssClass = "tab-btn";
            btnTabFaqs.CssClass = "tab-btn active";

            ViewState["CurrentTab"] = "Faqs";
            LoadDeletedFaqs();
        }

        // ============================================= 
        // HIDDEN BUTTON SUBMIT - Handles all actions via SweetAlert
        // ============================================= 
        protected void btnHiddenSubmit_Click(object sender, EventArgs e)
        {
            string action = hdnAction.Value;
            string itemIdStr = hdnItemId.Value;

            if (string.IsNullOrEmpty(action) || string.IsNullOrEmpty(itemIdStr))
            {
                ShowAlert("Error", "Invalid action or item ID.", "error");
                return;
            }

            int itemId;
            if (!int.TryParse(itemIdStr, out itemId))
            {
                ShowAlert("Error", "Invalid item ID format.", "error");
                return;
            }

            // Handle different actions
            switch (action)
            {
                case "RestoreService":
                    RestoreService(itemId);
                    break;

                case "DeletePermanent":
                    DeleteServicePermanently(itemId);
                    break;

                case "RestoreBlog":
                    RestoreBlog(itemId);
                    break;

                case "DeletePermanentBlog":
                    DeleteBlogPermanently(itemId);
                    break;

                case "RestoreFaq":
                    RestoreFaq(itemId);
                    break;

                case "DeletePermanentFaq":
                    DeleteFaqPermanently(itemId);
                    break;

                default:
                    ShowAlert("Error", "Unknown action.", "error");
                    break;
            }

            // Clear hidden fields
            hdnAction.Value = string.Empty;
            hdnItemId.Value = string.Empty;
        }

        // ============================================= 
        // SERVICES SECTION
        // ============================================= 

        // ============ Load Deleted Services ============
        private void LoadDeletedServices()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT ServiceID, ServiceType, ServiceTitle, ServiceDescription, 
                               BulletPoints, ImagePath, DisplayOrder, IsActive, 
                               CreatedDate, LastUpdated
                        FROM ServicesCMS
                        WHERE IsActive = 0
                        ORDER BY LastUpdated DESC, ServiceType, DisplayOrder";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count == 0)
                        {
                            // Show empty state
                            pnlEmptyStateServices.Visible = true;
                            gvDeletedServices.Visible = false;
                        }
                        else
                        {
                            // Show grid
                            pnlEmptyStateServices.Visible = false;
                            gvDeletedServices.Visible = true;
                            gvDeletedServices.DataSource = dt;
                            gvDeletedServices.DataBind();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to load archived services: " + ex.Message, "error");
            }
        }

        // ============ Restore Service (Soft Delete Undo) ============
        private void RestoreService(int serviceId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        UPDATE ServicesCMS 
                        SET IsActive = 1, LastUpdated = GETDATE() 
                        WHERE ServiceID = @ServiceID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ServiceID", serviceId);

                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            ShowAlert("Success!", "Service restored successfully! It is now active again.", "success");
                            LoadDeletedServices();
                        }
                        else
                        {
                            ShowAlert("Error", "Service not found or could not be restored.", "error");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to restore service: " + ex.Message, "error");
            }
        }

        // ============ Delete Service Permanently (Hard Delete) ============
        private void DeleteServicePermanently(int serviceId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM ServicesCMS WHERE ServiceID = @ServiceID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ServiceID", serviceId);

                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            ShowAlert("Deleted!", "Service has been permanently deleted from the database.", "success");
                            LoadDeletedServices();
                        }
                        else
                        {
                            ShowAlert("Error", "Service not found or could not be deleted.", "error");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to delete service permanently: " + ex.Message, "error");
            }
        }

        // ============================================= 
        // BLOGS SECTION
        // ============================================= 

        // ============ Load Deleted Blogs ============
        private void LoadDeletedBlogs()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT BlogID, BlogTitle, BlogDescription, BlogContent, 
                               ImagePath, DisplayOrder, IsActive, 
                               CreatedDate, ModifiedDate
                        FROM BlogsCMS
                        WHERE IsActive = 0
                        ORDER BY ModifiedDate DESC, DisplayOrder, BlogID DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count == 0)
                        {
                            // Show empty state
                            pnlEmptyStateBlogs.Visible = true;
                            gvDeletedBlogs.Visible = false;
                        }
                        else
                        {
                            // Show grid
                            pnlEmptyStateBlogs.Visible = false;
                            gvDeletedBlogs.Visible = true;
                            gvDeletedBlogs.DataSource = dt;
                            gvDeletedBlogs.DataBind();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to load archived blogs: " + ex.Message, "error");
            }
        }

        // ============ Restore Blog (Soft Delete Undo) ============
        private void RestoreBlog(int blogId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        UPDATE BlogsCMS 
                        SET IsActive = 1, ModifiedDate = GETDATE() 
                        WHERE BlogID = @BlogID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BlogID", blogId);

                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            ShowAlert("Success!", "Blog restored successfully! It is now active again.", "success");
                            LoadDeletedBlogs();
                        }
                        else
                        {
                            ShowAlert("Error", "Blog not found or could not be restored.", "error");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to restore blog: " + ex.Message, "error");
            }
        }

        // ============ Delete Blog Permanently (Hard Delete) ============
        private void DeleteBlogPermanently(int blogId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM BlogsCMS WHERE BlogID = @BlogID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BlogID", blogId);

                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            ShowAlert("Deleted!", "Blog has been permanently deleted from the database.", "success");
                            LoadDeletedBlogs();
                        }
                        else
                        {
                            ShowAlert("Error", "Blog not found or could not be deleted.", "error");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to delete blog permanently: " + ex.Message, "error");
            }
        }

        // ============================================= 
        // FAQs SECTION
        // ============================================= 

        // ============ Load Deleted FAQs ============
        private void LoadDeletedFaqs()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT ID, Question, Answer, DisplayOrder, IsActive, 
                               CreatedDate, UpdatedDate
                        FROM FAQs
                        WHERE IsActive = 0
                        ORDER BY UpdatedDate DESC, DisplayOrder, ID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count == 0)
                        {
                            // Show empty state
                            pnlEmptyStateFaqs.Visible = true;
                            gvDeletedFaqs.Visible = false;
                        }
                        else
                        {
                            // Show grid
                            pnlEmptyStateFaqs.Visible = false;
                            gvDeletedFaqs.Visible = true;
                            gvDeletedFaqs.DataSource = dt;
                            gvDeletedFaqs.DataBind();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to load archived FAQs: " + ex.Message, "error");
            }
        }

        // ============ Restore FAQ (Soft Delete Undo) ============
        private void RestoreFaq(int faqId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        UPDATE FAQs 
                        SET IsActive = 1, UpdatedDate = GETDATE() 
                        WHERE ID = @ID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", faqId);

                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            ShowAlert("Success!", "FAQ restored successfully! It is now active again.", "success");
                            LoadDeletedFaqs();
                        }
                        else
                        {
                            ShowAlert("Error", "FAQ not found or could not be restored.", "error");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to restore FAQ: " + ex.Message, "error");
            }
        }

        // ============ Delete FAQ Permanently (Hard Delete) ============
        private void DeleteFaqPermanently(int faqId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM FAQs WHERE ID = @ID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", faqId);

                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            ShowAlert("Deleted!", "FAQ has been permanently deleted from the database.", "success");
                            LoadDeletedFaqs();
                        }
                        else
                        {
                            ShowAlert("Error", "FAQ not found or could not be deleted.", "error");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to delete FAQ permanently: " + ex.Message, "error");
            }
        }

        // ============ Show Alert ============
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