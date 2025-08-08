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

        protected void gvArchivedAdmins_RowCommand(object sender, GridViewCommandEventArgs e)
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
                string restoreQuery = @"
            UPDATE Users
            SET Status = 'Available'
            WHERE UserID = @UserID AND Role = 'Admin'";

                string reset2FAQuery = @"
            UPDATE Users
            SET TwoFactorEnabled = 0,
                TOTPSecret = NULL
            WHERE UserID = @UserID";

                using (SqlCommand cmdRestore = new SqlCommand(restoreQuery, conn))
                using (SqlCommand cmdReset2FA = new SqlCommand(reset2FAQuery, conn))
                {
                    cmdRestore.Parameters.AddWithValue("@UserID", userID);
                    cmdReset2FA.Parameters.AddWithValue("@UserID", userID);

                    try
                    {
                        conn.Open();
                        cmdRestore.ExecuteNonQuery();
                        cmdReset2FA.ExecuteNonQuery(); // 🔐 Force 2FA rescan

                        lblMessage.Text = "✅ Admin restored successfully. 2FA setup will be required again.";
                        LoadArchivedAdmins(); // Refresh the GridView
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
                string deletePermissionsQuery = "DELETE FROM AdminPermissions WHERE UserID = @UserID";
                string markUserDeletedQuery = "UPDATE Users SET Status = 'Deleted' WHERE UserID = @UserID AND Role = 'Admin' AND Status = 'Archived'";

                try
                {
                    conn.Open();

                    // Step 1: Remove permissions
                    using (SqlCommand cmd = new SqlCommand(deletePermissionsQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userID);
                        cmd.ExecuteNonQuery();
                    }

                    // Step 2: Mark admin as Deleted (instead of physical deletion)
                    using (SqlCommand cmd = new SqlCommand(markUserDeletedQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userID);
                        int rows = cmd.ExecuteNonQuery();

                        lblMessage.Text = rows > 0
                            ? "✅ Admin permanently deleted (status set to Deleted)."
                            : "⚠ Admin could not be deleted.";
                    }

                    LoadArchivedAdmins(); // Refresh list
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "⚠ Error deleting admin: " + ex.Message;
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
