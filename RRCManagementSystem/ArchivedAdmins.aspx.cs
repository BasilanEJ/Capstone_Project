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
                LoadArchivedAdmins(null);
            }
        }

        /* =========================
           Data binding via SP
           ========================= */
        private void LoadArchivedAdmins(string keyword)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spAdmins_ListArchived", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                if (string.IsNullOrWhiteSpace(keyword))
                    cmd.Parameters.Add("@Keyword", SqlDbType.NVarChar, 100).Value = DBNull.Value;
                else
                    cmd.Parameters.Add("@Keyword", SqlDbType.NVarChar, 100).Value = keyword.Trim();

                var dt = new DataTable();
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

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            LoadArchivedAdmins(txtSearch.Text);
        }

        protected void gvArchivedAdmins_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // Your JS passes "__doPostBack(gridID, 'RestoreAdmin$<id>')" etc.
            var parts = (e.CommandArgument ?? "").ToString().Split('$');
            string cmdName = e.CommandName;
            string arg = e.CommandArgument?.ToString();

            // Some setups place both in CommandArgument. Handle both styles safely.
            if (parts.Length == 2)
            {
                cmdName = parts[0];
                arg = parts[1];
            }

            if (!int.TryParse(arg, out int userID)) return;

            if (cmdName == "RestoreAdmin")
            {
                RestoreAdmin(userID);
            }
            else if (cmdName == "DeletePermanently")
            {
                DeleteAdmin(userID);
            }
        }

        private void RestoreAdmin(int userID)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spAdmin_Restore", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userID;

                try
                {
                    conn.Open();
                    int rows = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);

                    if (rows > 0)
                    {
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

            LoadArchivedAdmins(txtSearch.Text);
        }

        private void DeleteAdmin(int userID)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spAdmin_DeleteArchived", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userID;

                try
                {
                    conn.Open();
                    int rows = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);

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

            LoadArchivedAdmins(txtSearch.Text);
        }

        // Optional: if your GridView uses paging
        protected void gvArchivedAdmins_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvArchivedAdmins.PageIndex = e.NewPageIndex;
            LoadArchivedAdmins(txtSearch.Text);
        }

        // Keep your event validation registrations
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
