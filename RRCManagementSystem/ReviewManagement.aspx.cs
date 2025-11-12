using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class ReviewManagement : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadActiveReviews();
            }
        }

        private void LoadActiveReviews()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT ReviewID, CustomerName, ReviewText, Rating, Recommends, IsActive
                        FROM Reviews
                        WHERE IsActive = 1
                        ORDER BY ISNULL(DisplayOrder, 999999), ReviewID DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        gvReviews.DataSource = dt;
                        gvReviews.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to load active reviews: " + ex.Message, "error");
            }
        }

        protected void gvReviews_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ArchiveReview")
            {
                int reviewId = Convert.ToInt32(e.CommandArgument);
                ArchiveReview(reviewId);
            }
        }

        private void ArchiveReview(int reviewId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE Reviews SET IsActive = 0 WHERE ReviewID = @ReviewID", conn))
                {
                    cmd.Parameters.AddWithValue("@ReviewID", reviewId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                ShowAlert("Archived!", "The review has been archived successfully.", "success");
                LoadActiveReviews();
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to archive review: " + ex.Message, "error");
            }
        }

        protected string GetStarRating(int rating)
        {
            string stars = "";
            for (int i = 0; i < rating; i++)
            {
                stars += "⭐";
            }
            return stars;
        }

        private void ShowAlert(string title, string message, string icon)
        {
            string script = $@"
                Swal.fire({{
                    title: '{title.Replace("'", "\\'")}',
                    text: '{message.Replace("'", "\\'")}',
                    icon: '{icon}',
                    confirmButtonColor: '#667eea'
                }});";
            ScriptManager.RegisterStartupScript(this, GetType(), "SweetAlert", script, true);
        }
    }
}