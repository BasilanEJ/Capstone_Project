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
            u.CreatedAt,
            p.ModuleName,
            ISNULL(p.CanView, 0) AS CanView,
            ISNULL(p.CanAdd, 0) AS CanAdd,
            ISNULL(p.CanEdit, 0) AS CanEdit,
            ISNULL(p.CanDelete, 0) AS CanDelete
        FROM Users u
        LEFT JOIN AdminPermissions p ON u.UserID = p.UserID
        WHERE u.Role = 'Admin' AND u.Status != 'Archived'
        ORDER BY u.UserID, p.ModuleName";

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
            u.UserID, u.Name, u.Email, u.Role, u.CreatedAt,
            p.ModuleName,
            ISNULL(p.CanView, 0) AS CanView,
            ISNULL(p.CanAdd, 0) AS CanAdd,
            ISNULL(p.CanEdit, 0) AS CanEdit,
            ISNULL(p.CanDelete, 0) AS CanDelete
        FROM Users u
        LEFT JOIN AdminPermissions p ON u.UserID = p.UserID
        WHERE u.Role = 'Admin' AND u.Status != 'Archived'
          AND (u.Name LIKE @Keyword OR u.Email LIKE @Keyword)
        ORDER BY u.UserID, p.ModuleName";

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
                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // 1. Insert user data into UserArchive table
                    string insertArchiveQuery = @"
                INSERT INTO UserArchive (UserID, Name, Email, Role, Status, ArchivedBy, ArchivedAt, Remarks)
                SELECT 
                    UserID, 
                    Name, 
                    Email, 
                    Role, 
                    Status,
                    @ArchivedBy,
                    GETDATE(),
                    @Remarks
                FROM Users
                WHERE UserID = @UserID";

                    using (SqlCommand cmdArchive = new SqlCommand(insertArchiveQuery, conn, transaction))
                    {
                        cmdArchive.Parameters.AddWithValue("@UserID", userID);
                        cmdArchive.Parameters.AddWithValue("@ArchivedBy", /* your SuperAdmin ID or session value here */ 1); // Example: SuperAdminID = 1
                        cmdArchive.Parameters.AddWithValue("@Remarks", "Archived by SuperAdmin");

                        cmdArchive.ExecuteNonQuery();
                    }

                    // 2. Update the original user record's status to 'Archived'
                    string updateUserQuery = @"
                UPDATE Users
                SET Status = 'Archived'
                WHERE UserID = @UserID AND Role = 'Admin'";

                    using (SqlCommand cmdUpdate = new SqlCommand(updateUserQuery, conn, transaction))
                    {
                        cmdUpdate.Parameters.AddWithValue("@UserID", userID);
                        int rowsAffected = cmdUpdate.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            lblMessage.Text = "✅ Admin archived successfully.";
                            transaction.Commit(); // ✅ Commit transaction
                        }
                        else
                        {
                            lblMessage.Text = "⚠ Admin not found or already archived.";
                            transaction.Rollback(); // ⚠ Rollback if no rows affected
                        }
                    }

                    LoadAdmins(); // Refresh the grid
                }
                catch (Exception ex)
                {
                    transaction.Rollback(); // Rollback on error
                    lblMessage.Text = "⚠ Error archiving admin: " + ex.Message;
                }
            }
        }

    }
}