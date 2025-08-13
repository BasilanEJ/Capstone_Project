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
            // prevent cached/stale view on back-button
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));

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
                    ORDER BY UserID;";

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
            if (!int.TryParse(e.CommandArgument.ToString(), out int userID))
                return;

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
                    WHERE UserID = @UserID AND Role = 'Admin' AND Status = 'Archived';";

                string reset2FAQuery = @"
                    UPDATE Users
                    SET TwoFactorEnabled = 0,
                        TOTPSecret = NULL
                    WHERE UserID = @UserID;";

                using (SqlCommand cmdRestore = new SqlCommand(restoreQuery, conn))
                using (SqlCommand cmdReset2FA = new SqlCommand(reset2FAQuery, conn))
                {
                    cmdRestore.Parameters.AddWithValue("@UserID", userID);
                    cmdReset2FA.Parameters.AddWithValue("@UserID", userID);

                    try
                    {
                        conn.Open();
                        int rows = cmdRestore.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            cmdReset2FA.ExecuteNonQuery(); // 🔐 Force 2FA re-setup
                            // Go back to ViewAdmin and show success toast there
                            Response.Redirect("ViewAdmin.aspx?restored=1", false);
                            Context.ApplicationInstance.CompleteRequest();
                            return;
                        }
                        else
                        {
                            lblMessage.Text = "⚠ Admin not found or not in Archived status.";
                        }
                    }
                    catch (Exception ex)
                    {
                        lblMessage.Text = "⚠ Error restoring admin: " + ex.Message;
                    }
                }
            }

            // if we got here, just refresh the list
            LoadArchivedAdmins();
        }

        private void DeleteAdmin(int userID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmdDeletePerms = new SqlCommand("DELETE FROM AdminPermissions WHERE UserID = @UserID;", conn))
            using (SqlCommand cmdMarkDeleted = new SqlCommand(@"
                UPDATE Users 
                SET Status = 'Deleted' 
                WHERE UserID = @UserID AND Role = 'Admin' AND Status = 'Archived';", conn))
            {
                cmdDeletePerms.Parameters.AddWithValue("@UserID", userID);
                cmdMarkDeleted.Parameters.AddWithValue("@UserID", userID);

                try
                {
                    conn.Open();

                    // Step 1: remove permissions (safe even if none exist)
                    cmdDeletePerms.ExecuteNonQuery();

                    // Step 2: soft-delete user
                    int rows = cmdMarkDeleted.ExecuteNonQuery();
                    if (rows > 0)
                    {
                        // Go back to ViewAdmin and show success toast there
                        Response.Redirect("ViewAdmin.aspx?deleted=1", false);
                        Context.ApplicationInstance.CompleteRequest();
                        return;
                    }
                    else
                    {
                        lblMessage.Text = "⚠ Admin could not be deleted (must be Role=Admin and Status=Archived).";
                    }
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "⚠ Error deleting admin: " + ex.Message;
                }
            }

            // if we got here, just refresh the list
            LoadArchivedAdmins();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT UserID, Name, Email, Role, Status
                FROM Users
                WHERE Role = 'Admin' AND Status = 'Archived'
                  AND (Name LIKE @Keyword OR Email LIKE @Keyword)
                ORDER BY UserID;", conn))
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

        // Optional: if your GridView uses paging
        protected void gvArchivedAdmins_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvArchivedAdmins.PageIndex = e.NewPageIndex;
            LoadArchivedAdmins();
        }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            // Ensure event validation for row commands (Restore/Delete)
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
