using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using RRCManagementSystem.Helpers; // Needed for AESHelper

namespace RRCManagementSystem
{
    public partial class ArchivedSuperUser : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Prevent caching to avoid stale data when navigating back
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));

            if (!IsPostBack)
                LoadArchivedUsers(null);
        }

        private void LoadArchivedUsers(string keyword)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spSysUsers_ListArchived", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Keyword", string.IsNullOrWhiteSpace(keyword) ? DBNull.Value : (object)keyword.Trim());

                var dt = new DataTable();
                try
                {
                    da.Fill(dt);

                    // 🔹 Decrypt each email before displaying
                    foreach (DataRow row in dt.Rows)
                    {
                        if (row["Email"] != DBNull.Value && !string.IsNullOrEmpty(row["Email"].ToString()))
                        {
                            try
                            {
                                string encryptedEmail = row["Email"].ToString();
                                string decryptedEmail = AESHelper.DecryptEmail(encryptedEmail);
                                row["Email"] = decryptedEmail;
                            }
                            catch
                            {
                                row["Email"] = "[Decryption Error]";
                            }
                        }
                    }

                    gvArchivedUsers.DataSource = dt;
                    gvArchivedUsers.DataBind();
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "⚠ Error loading archived users: " + ex.Message;
                }
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            LoadArchivedUsers(txtSearch.Text);
        }

        protected void gvArchivedUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            var parts = (e.CommandArgument ?? "").ToString().Split('$');
            string cmdName = e.CommandName;
            string arg = e.CommandArgument?.ToString();

            if (parts.Length == 2)
            {
                cmdName = parts[0];
                arg = parts[1];
            }

            if (!int.TryParse(arg, out int userID)) return;

            if (cmdName == "RestoreUser")
                RestoreUser(userID);
            else if (cmdName == "DeleteUser")
                DeleteUser(userID);
        }

        private void RestoreUser(int userID)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spSysUser_Restore", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userID;

                try
                {
                    conn.Open();
                    int rows = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                    if (rows > 0)
                        Response.Redirect("ViewUser.aspx?restored=1", false);
                    else
                        lblMessage.Text = "⚠ User not found or not archived.";
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "⚠ Error restoring user: " + ex.Message;
                }
            }
            LoadArchivedUsers(txtSearch.Text);
        }

        private void DeleteUser(int userID)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spSysUser_Delete", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userID;

                try
                {
                    conn.Open();
                    int rows = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                    if (rows > 0)
                        Response.Redirect("ViewUser.aspx?deleted=1", false);
                    else
                        lblMessage.Text = "⚠ User could not be deleted.";
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "⚠ Error deleting user: " + ex.Message;
                }
            }
            LoadArchivedUsers(txtSearch.Text);
        }

        protected void gvArchivedUsers_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvArchivedUsers.PageIndex = e.NewPageIndex;
            LoadArchivedUsers(txtSearch.Text);
        }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            foreach (GridViewRow row in gvArchivedUsers.Rows)
            {
                string userId = gvArchivedUsers.DataKeys[row.RowIndex].Value.ToString();
                ClientScript.RegisterForEventValidation(gvArchivedUsers.UniqueID, "RestoreUser$" + userId);
                ClientScript.RegisterForEventValidation(gvArchivedUsers.UniqueID, "DeleteUser$" + userId);
            }
            base.Render(writer);
        }
    }
}
