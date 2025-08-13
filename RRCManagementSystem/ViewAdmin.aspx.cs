using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class ViewAdmin : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // prevent cached/stale view on back-button
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));

            // 🔐 Require login + SuperAdmin
            if (Session["UserID"] == null || Session["Role"] == null || Session["Role"].ToString() != "SuperAdmin")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadUsers();

                // Show SweetAlert if redirected from actions
                if (Request.QueryString["archived"] == "1")
                {
                    string script = @"Swal.fire({
                        icon: 'success',
                        title: 'Archived!',
                        text: 'User has been successfully archived.',
                        showConfirmButton: false,
                        timer: 2000
                    });";
                    ClientScript.RegisterStartupScript(this.GetType(), "archivedOk", script, true);
                }
                else if (Request.QueryString["deleted"] == "1")
                {
                    string script = @"Swal.fire({
                        icon: 'success',
                        title: 'Deleted!',
                        text: 'User has been permanently deleted.',
                        showConfirmButton: false,
                        timer: 2000
                    });";
                    ClientScript.RegisterStartupScript(this.GetType(), "deletedOk", script, true);
                }
                else if (Request.QueryString["restored"] == "1")
                {
                    string script = @"Swal.fire({
                        icon: 'success',
                        title: 'Restored!',
                        text: 'User has been restored.',
                        showConfirmButton: false,
                        timer: 2000
                    });";
                    ClientScript.RegisterStartupScript(this.GetType(), "restoredOk", script, true);
                }
            }
        }

        private void LoadUsers()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        UserID, 
                        Name, 
                        Email, 
                        Role
                    FROM Users
                    WHERE Role != 'SuperAdmin'
                      AND Status IN ('Active', 'Available')
                    ORDER BY UserID;";

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
                    lblMessage.Text = "⚠ Error loading users: " + ex.Message;
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
                        UserID, 
                        Name, 
                        Email, 
                        Role
                    FROM Users
                    WHERE Role != 'SuperAdmin'
                      AND Status IN ('Active', 'Available')
                      AND (Name LIKE @Keyword OR Email LIKE @Keyword OR Role LIKE @Keyword)
                    ORDER BY UserID;";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Keyword", "%" + keyword + "%");

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();

                    try
                    {
                        da.Fill(dt);
                        gvAdmins.DataSource = dt;
                        gvAdmins.DataBind();
                    }
                    catch (Exception ex)
                    {
                        lblMessage.Text = "⚠ Error searching users: " + ex.Message;
                    }
                }
            }
        }

        protected void gvAdmins_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (int.TryParse(e.CommandArgument.ToString(), out int userID))
            {
                if (e.CommandName == "EditAdmin")
                {
                    Response.Redirect($"EditAdmin.aspx?UserID={userID}");
                }
                else if (e.CommandName == "ArchiveAdmin")
                {
                    ArchiveUser(userID);
                }
            }
        }

        protected void btnConfirmArchive_Click(object sender, EventArgs e)
        {
            if (int.TryParse(hfUserToArchive.Value, out int userId))
            {
                ArchiveUser(userId);
            }
        }

        private void ArchiveUser(int userID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    UPDATE Users 
                    SET Status = 'Archived' 
                    WHERE UserID = @UserID 
                      AND Role != 'SuperAdmin';";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userID);

                    try
                    {
                        conn.Open();
                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            // redirect to force fresh bind and show alert
                            Response.Redirect("ViewAdmin.aspx?archived=1", false);
                            Context.ApplicationInstance.CompleteRequest();
                        }
                        else
                        {
                            lblMessage.Text = "⚠ No matching user found to archive.";
                        }
                    }
                    catch (Exception ex)
                    {
                        lblMessage.Text = "⚠ Error archiving user: " + ex.Message;
                    }
                }
            }
        }

        // Optional: if your GridView uses paging
        protected void gvAdmins_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvAdmins.PageIndex = e.NewPageIndex;
            LoadUsers();
        }
    }
}
