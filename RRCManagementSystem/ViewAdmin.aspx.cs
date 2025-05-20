using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace RRCManagementSystem
{
    public partial class ViewAdmin : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 🔐 Require login
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string role = Session["Role"].ToString();

            // 🔐 Block SuperAdmin and Inspector
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadAdmins();
            }
        }


        private void LoadAdmins()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                SELECT 
                    u.UserID, 
                    u.Name, 
                    u.Email, 
                    u.Role, 
                    MIN(u.CreatedAt) AS CreatedAt
                FROM Users u
                WHERE u.Role = 'Admin' AND u.Status != 'Archived'
                GROUP BY u.UserID, u.Name, u.Email, u.Role
                ORDER BY u.UserID";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();

                try
                {
                    da.Fill(dt);
                    gvAdmins.DataSource = dt;
                    gvAdmins.DataBind();
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "⚠ Error loading admins: " + ex.Message;
                }
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                SELECT 
                    u.UserID, 
                    u.Name, 
                    u.Email, 
                    u.Role, 
                    MIN(u.CreatedAt) AS CreatedAt
                FROM Users u
                WHERE u.Role = 'Admin' AND u.Status != 'Archived'
                    AND (u.Name LIKE @Keyword OR u.Email LIKE @Keyword)
                GROUP BY u.UserID, u.Name, u.Email, u.Role
                ORDER BY u.UserID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Keyword", $"%{keyword}%");
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvAdmins.DataSource = dt;
                    gvAdmins.DataBind();
                }
            }
        }

        protected void gvAdmins_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            int userID = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "EditAdmin")
            {
                Response.Redirect($"EditAdmin.aspx?UserID={userID}");
            }

            if (e.CommandName == "ArchiveAdmin")
            {
                ArchiveAdmin(userID);
            }
        }

        private void ArchiveAdmin(int userID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
            UPDATE Users
            SET Status = 'Archived'
            WHERE UserID = @UserID AND Role = 'Admin'";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userID);

                    try
                    {
                        conn.Open();
                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            lblMessage.Text = "✅ Admin archived successfully.";
                            LoadAdmins(); // refresh GridView
                        }
                        else
                        {
                            lblMessage.Text = "⚠ No admin found to archive.";
                        }
                    }
                    catch (Exception ex)
                    {
                        lblMessage.Text = "⚠ Error archiving admin: " + ex.Message;
                    }
                }
            }
        }

    }
}