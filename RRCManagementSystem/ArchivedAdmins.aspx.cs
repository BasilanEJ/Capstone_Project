using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class ArchivedAdmins : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadArchivedAdmins();
            }
        }

        private void LoadArchivedAdmins()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT UserID, Name, Email, Role, Status
                    FROM Users
                    WHERE Role = 'Admin' AND Status = 'Archived'
                    ORDER BY UserID";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();

                try
                {
                    da.Fill(dt);
                    gvArchivedAdmins.DataSource = dt;
                    gvArchivedAdmins.DataBind();
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "⚠ Error loading archived admins: " + ex.Message;
                }
            }
        }

        protected void gvArchivedAdmins_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            int userID = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "RestoreAdmin")
            {
                RestoreAdmin(userID);
            }
            else if (e.CommandName == "DeletePermanently")
            {
                DeleteAdmin(userID);
            }
        }

        private void RestoreAdmin(int userID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "UPDATE Users SET Status = 'Available' WHERE UserID = @UserID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userID);

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        lblMessage.Text = "✅ Admin restored successfully.";
                        LoadArchivedAdmins();
                    }
                    catch (Exception ex)
                    {
                        lblMessage.Text = "⚠ Error restoring admin: " + ex.Message;
                    }
                }
            }
        }

        private void DeleteAdmin(int userID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Users WHERE UserID = @UserID AND Role = 'Admin' AND Status = 'Archived'";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userID);

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        lblMessage.Text = "✅ Admin deleted permanently.";
                        LoadArchivedAdmins();
                    }
                    catch (Exception ex)
                    {
                        lblMessage.Text = "⚠ Error deleting admin: " + ex.Message;
                    }
                }
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT UserID, Name, Email, Role, Status
                    FROM Users
                    WHERE Role = 'Admin' AND Status = 'Archived'
                      AND (Name LIKE @Keyword OR Email LIKE @Keyword)
                    ORDER BY UserID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Keyword", $"%{keyword}%");

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();

                    try
                    {
                        da.Fill(dt);
                        gvArchivedAdmins.DataSource = dt;
                        gvArchivedAdmins.DataBind();
                    }
                    catch (Exception ex)
                    {
                        lblMessage.Text = "⚠ Error during search: " + ex.Message;
                    }
                }
            }
        }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            foreach (GridViewRow row in gvArchivedAdmins.Rows)
            {
                string userId = gvArchivedAdmins.DataKeys[row.RowIndex].Value.ToString();

                ClientScript.RegisterForEventValidation(gvArchivedAdmins.UniqueID, "RestoreAdmin$" + userId);
                ClientScript.RegisterForEventValidation(gvArchivedAdmins.UniqueID, "DeletePermanently$" + userId);
            }

            base.Render(writer);
        }


    }
}
