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
                LoadReviews();
            }
        }

        private void LoadReviews()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT ReviewID, CustomerName, ReviewText, Rating, 
                               Recommends, DisplayOrder, IsActive, 
                               CreatedDate, LastUpdated
                        FROM Reviews
                        ORDER BY DisplayOrder, ReviewID";

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
                ShowAlert("Error", "Failed to load reviews: " + ex.Message, "error");
            }
        }

        protected void btnAddReview_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        INSERT INTO Reviews (CustomerName, ReviewText, Rating, Recommends, DisplayOrder, IsActive, CreatedDate, LastUpdated)
                        VALUES (@CustomerName, @ReviewText, @Rating, @Recommends, @DisplayOrder, @IsActive, GETDATE(), GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CustomerName", txtCustomerName.Text.Trim());
                        cmd.Parameters.AddWithValue("@ReviewText", txtReviewText.Text.Trim());
                        cmd.Parameters.AddWithValue("@Rating", Convert.ToInt32(ddlRating.SelectedValue));
                        cmd.Parameters.AddWithValue("@Recommends", chkRecommends.Checked);
                        cmd.Parameters.AddWithValue("@DisplayOrder",
                            string.IsNullOrWhiteSpace(txtDisplayOrder.Text) ? 0 : Convert.ToInt32(txtDisplayOrder.Text));
                        cmd.Parameters.AddWithValue("@IsActive", chkIsActive.Checked);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowAlert("Success!", "Review added successfully.", "success");
                ClearForm();
                LoadReviews();
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to add review: " + ex.Message, "error");
            }
        }

        protected void btnUpdateReview_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                int reviewId = Convert.ToInt32(hfReviewID.Value);

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        UPDATE Reviews 
                        SET CustomerName = @CustomerName,
                            ReviewText = @ReviewText,
                            Rating = @Rating,
                            Recommends = @Recommends,
                            DisplayOrder = @DisplayOrder,
                            IsActive = @IsActive,
                            LastUpdated = GETDATE()
                        WHERE ReviewID = @ReviewID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ReviewID", reviewId);
                        cmd.Parameters.AddWithValue("@CustomerName", txtCustomerName.Text.Trim());
                        cmd.Parameters.AddWithValue("@ReviewText", txtReviewText.Text.Trim());
                        cmd.Parameters.AddWithValue("@Rating", Convert.ToInt32(ddlRating.SelectedValue));
                        cmd.Parameters.AddWithValue("@Recommends", chkRecommends.Checked);
                        cmd.Parameters.AddWithValue("@DisplayOrder",
                            string.IsNullOrWhiteSpace(txtDisplayOrder.Text) ? 0 : Convert.ToInt32(txtDisplayOrder.Text));
                        cmd.Parameters.AddWithValue("@IsActive", chkIsActive.Checked);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowAlert("Success!", "Review updated successfully.", "success");
                ClearForm();
                LoadReviews();
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to update review: " + ex.Message, "error");
            }
        }

        protected void btnCancelEdit_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        protected void gvReviews_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int reviewId = Convert.ToInt32(e.CommandArgument);

            try
            {
                if (e.CommandName == "EditReview")
                {
                    LoadReviewForEdit(reviewId);
                }
                else if (e.CommandName == "ToggleStatus")
                {
                    ToggleReviewStatus(reviewId);
                }
                else if (e.CommandName == "DeleteReview")
                {
                    DeleteReview(reviewId);
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Operation failed: " + ex.Message, "error");
            }
        }

        private void LoadReviewForEdit(int reviewId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM Reviews WHERE ReviewID = @ReviewID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ReviewID", reviewId);
                        conn.Open();

                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            hfReviewID.Value = reader["ReviewID"].ToString();
                            txtCustomerName.Text = reader["CustomerName"].ToString();
                            txtReviewText.Text = reader["ReviewText"].ToString();
                            ddlRating.SelectedValue = reader["Rating"].ToString();
                            chkRecommends.Checked = Convert.ToBoolean(reader["Recommends"]);
                            txtDisplayOrder.Text = reader["DisplayOrder"].ToString();
                            chkIsActive.Checked = Convert.ToBoolean(reader["IsActive"]);

                            // Switch to edit mode
                            lblFormTitle.Text = "✏️ Edit Review";
                            btnAddReview.Visible = false;
                            btnUpdateReview.Visible = true;
                            btnCancelEdit.Visible = true;

                            // Scroll to form
                            ScriptManager.RegisterStartupScript(this, GetType(), "ScrollToTop",
                                "window.scrollTo(0, 0);", true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to load review: " + ex.Message, "error");
            }
        }

        private void ToggleReviewStatus(int reviewId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        UPDATE Reviews 
                        SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END,
                            LastUpdated = GETDATE()
                        WHERE ReviewID = @ReviewID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ReviewID", reviewId);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowAlert("Success!", "Review status updated.", "success");
                LoadReviews();
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to toggle status: " + ex.Message, "error");
            }
        }

        private void DeleteReview(int reviewId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM Reviews WHERE ReviewID = @ReviewID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ReviewID", reviewId);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowAlert("Success!", "Review deleted successfully.", "success");
                LoadReviews();
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to delete review: " + ex.Message, "error");
            }
        }

        protected void gvReviews_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            // Optional: Add any row-specific formatting here
        }

        // THIS IS THE METHOD THAT WAS MISSING!
        // It generates star rating display for the GridView
        protected string GetStarRating(int rating)
        {
            string stars = "";
            for (int i = 0; i < rating; i++)
            {
                stars += "⭐";
            }
            return stars;
        }

        private void ClearForm()
        {
            hfReviewID.Value = "";
            txtCustomerName.Text = "";
            txtReviewText.Text = "";
            ddlRating.SelectedValue = "5";
            chkRecommends.Checked = true;
            txtDisplayOrder.Text = "0";
            chkIsActive.Checked = true;

            lblFormTitle.Text = "📝 Add New Review";
            btnAddReview.Visible = true;
            btnUpdateReview.Visible = false;
            btnCancelEdit.Visible = false;
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
