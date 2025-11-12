using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class FAQ : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadFaqs();
            }
        }

        private void LoadFaqs()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT * FROM [EJBasilan_RRCDB].[EJBasilan_admin].[FAQs] WHERE IsActive = 1 ORDER BY DisplayOrder, ID", conn))
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
                ShowAlert("Validation Error", "Please fill in both question and answer fields.", "warning");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO [EJBasilan_RRCDB].[EJBasilan_admin].[FAQs] 
                    (Question, Answer, DisplayOrder, IsActive, CreatedDate) 
                    VALUES (@Question, @Answer, 
                        (SELECT ISNULL(MAX(DisplayOrder), 0) + 1 
                         FROM [EJBasilan_RRCDB].[EJBasilan_admin].[FAQs]), 
                        1, GETDATE())", conn))
                {
                    cmd.Parameters.AddWithValue("@Question", question);
                    cmd.Parameters.AddWithValue("@Answer", answer);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                txtNewFaqQuestion.Text = string.Empty;
                txtNewFaqAnswer.Text = string.Empty;

                LoadFaqs();
                ShowAlert("Success!", "FAQ has been added successfully.", "success");
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to add FAQ: " + ex.Message, "error");
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
            try
            {
                int id = Convert.ToInt32(gvFaqs.DataKeys[e.RowIndex].Value);
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

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE [EJBasilan_RRCDB].[EJBasilan_admin].[FAQs] SET Question = @Question, Answer = @Answer, UpdatedDate = GETDATE() WHERE ID = @ID", conn))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    cmd.Parameters.AddWithValue("@Question", question);
                    cmd.Parameters.AddWithValue("@Answer", answer);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                gvFaqs.EditIndex = -1;
                LoadFaqs();
                ShowAlert("Updated!", "FAQ has been updated successfully.", "success");
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to update FAQ: " + ex.Message, "error");
            }
        }

        protected void btnConfirmDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(hfDeleteRowIndex.Value))
                {
                    int rowIndex = Convert.ToInt32(hfDeleteRowIndex.Value);
                    int id = Convert.ToInt32(gvFaqs.DataKeys[rowIndex].Value);

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    using (SqlCommand cmd = new SqlCommand(
                        "UPDATE [EJBasilan_RRCDB].[EJBasilan_admin].[FAQs] SET IsActive = 0, UpdatedDate = GETDATE() WHERE ID = @ID", conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", id);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }

                    hfDeleteRowIndex.Value = string.Empty;
                    LoadFaqs();
                    ShowAlert("Archived!", "FAQ has been archived successfully.", "success");
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to archive FAQ: " + ex.Message, "error");
            }
        }

        protected void gvFaqs_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                int id = Convert.ToInt32(gvFaqs.DataKeys[e.RowIndex].Value);

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE [EJBasilan_RRCDB].[EJBasilan_admin].[FAQs] SET IsActive = 0, UpdatedDate = GETDATE() WHERE ID = @ID", conn))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                LoadFaqs();
                ShowAlert("Archived!", "FAQ has been archived successfully.", "success");
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to archive FAQ: " + ex.Message, "error");
            }
        }

        protected void gvFaqs_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "ToggleActive" &&
                e.CommandName != "MoveUp" &&
                e.CommandName != "MoveDown")
                return;

            try
            {
                int id = Convert.ToInt32(e.CommandArgument);

                if (e.CommandName == "ToggleActive")
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    using (SqlCommand cmd = new SqlCommand(
                        "UPDATE [EJBasilan_RRCDB].[EJBasilan_admin].[FAQs] SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END, UpdatedDate = GETDATE() WHERE ID = @ID", conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", id);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }

                    LoadFaqs();
                    ShowAlert("Status Changed!", "FAQ status has been updated successfully.", "success");
                }
                else if (e.CommandName == "MoveUp")
                {
                    MoveFaq(id, -1);
                    LoadFaqs();
                    ShowAlert("Moved Up!", "FAQ has been moved up successfully.", "success");
                }
                else if (e.CommandName == "MoveDown")
                {
                    MoveFaq(id, 1);
                    LoadFaqs();
                    ShowAlert("Moved Down!", "FAQ has been moved down successfully.", "success");
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Operation failed: " + ex.Message, "error");
            }
        }

        private void MoveFaq(int id, int direction)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    int currentOrder;
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT DisplayOrder FROM [EJBasilan_RRCDB].[EJBasilan_admin].[FAQs] WHERE ID = @ID", conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", id);
                        currentOrder = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    int newOrder = currentOrder + direction;

                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT COUNT(*) FROM [EJBasilan_RRCDB].[EJBasilan_admin].[FAQs] WHERE DisplayOrder = @NewOrder AND IsActive = 1", conn))
                    {
                        cmd.Parameters.AddWithValue("@NewOrder", newOrder);
                        if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                            return;
                    }

                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            using (SqlCommand cmd = new SqlCommand(
                                "UPDATE [EJBasilan_RRCDB].[EJBasilan_admin].[FAQs] SET DisplayOrder = @CurrentOrder, UpdatedDate = GETDATE() WHERE DisplayOrder = @NewOrder AND IsActive = 1", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@CurrentOrder", currentOrder);
                                cmd.Parameters.AddWithValue("@NewOrder", newOrder);
                                cmd.ExecuteNonQuery();
                            }

                            using (SqlCommand cmd = new SqlCommand(
                                "UPDATE [EJBasilan_RRCDB].[EJBasilan_admin].[FAQs] SET DisplayOrder = @NewOrder, UpdatedDate = GETDATE() WHERE ID = @ID", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@NewOrder", newOrder);
                                cmd.Parameters.AddWithValue("@ID", id);
                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to reorder FAQ: " + ex.Message, "error");
            }
        }

        private void ShowAlert(string title, string message, string icon)
        {
            title = title.Replace("'", "\\'");
            message = message.Replace("'", "\\'");

            string script = $@"
                Swal.fire({{
                    title: '{title}',
                    text: '{message}',
                    icon: '{icon}',
                    confirmButtonColor: '#2563eb',
                    timer: 3000,
                    timerProgressBar: true
                }});";

            ScriptManager.RegisterStartupScript(this, GetType(), "alert_" + Guid.NewGuid(), script, true);
        }
    }
}
