using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class BlogPost : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString["id"] != null && int.TryParse(Request.QueryString["id"], out int blogId))
                {
                    LoadBlogPost(blogId);
                    LoadMoreBlogs(blogId); // Load other blogs, excluding this one
                }
                else
                {
                    pnlBlogPost.Visible = false;
                    pnlMoreBlogs.Visible = false;
                    pnlNotFound.Visible = true;
                }
            }
        }

        private void LoadBlogPost(int blogId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM Blogs WHERE BlogID = @BlogID AND IsActive = 1";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BlogID", blogId);
                        conn.Open();

                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            pnlBlogPost.Visible = true;
                            pnlNotFound.Visible = false;

                            // Set the browser tab title
                            this.Title = reader["BlogTitle"].ToString();

                            // Populate the Hero Section
                            litBlogTitle.Text = reader["BlogTitle"].ToString();
                            litBlogDescription.Text = reader["BlogDescription"] != DBNull.Value ? reader["BlogDescription"].ToString() : "";

                            if (reader["BlogImagePath"] != DBNull.Value)
                            {
                                imgFeatured.ImageUrl = reader["BlogImagePath"].ToString();
                                imgFeatured.Visible = true;
                            }
                            else
                            {
                                imgFeatured.Visible = false; // Hide image if none provided
                            }

                            // Populate Meta
                            litAuthor.Text = reader["Author"] != DBNull.Value ? reader["Author"].ToString() : "RRC Team";
                            litReadTime.Text = reader["ReadTime"] != DBNull.Value ? reader["ReadTime"].ToString() : "5";

                            // Populate the Full Content from Summernote
                            litBlogContent.Text = reader["BlogContent"] != DBNull.Value ? reader["BlogContent"].ToString() : "";
                        }
                        else
                        {
                            // ID was valid but no blog was found (or it's not active)
                            pnlBlogPost.Visible = false;
                            pnlNotFound.Visible = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                pnlBlogPost.Visible = false;
                pnlNotFound.Visible = true;
                // You should log this error
                System.Diagnostics.Debug.WriteLine("Error loading blog: " + ex.Message);
            }
        }

        private void LoadMoreBlogs(int currentBlogId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Get 3 other active blogs, but NOT the one we are currently viewing
                    string query = @"
                        SELECT TOP 3 BlogID, BlogTitle, BlogImagePath 
                        FROM Blogs 
                        WHERE IsActive = 1 AND BlogID != @CurrentBlogID
                        ORDER BY DisplayOrder, CreatedDate DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CurrentBlogID", currentBlogId);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            rptMoreBlogs.DataSource = dt;
                            rptMoreBlogs.DataBind();
                            pnlMoreBlogs.Visible = true;
                        }
                        else
                        {
                            pnlMoreBlogs.Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                pnlMoreBlogs.Visible = false;
                // Log this error
                System.Diagnostics.Debug.WriteLine("Error loading more blogs: " + ex.Message);
            }
        }
    }
}