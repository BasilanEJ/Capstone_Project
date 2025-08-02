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
            if (Session["UserID"] == null || Session["Role"] == null || Session["Role"].ToString() != "SuperAdmin")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadUsers();

                // Show SweetAlert if redirected from archive
                if (Request.QueryString["archived"] == "1")
                {
                    string script = @"Swal.fire({
            icon: 'success',
            title: 'Archived!',
            text: 'User has been successfully archived.',
            showConfirmButton: false,
            timer: 2000
        });";
                    ClientScript.RegisterStartupScript(this.GetType(), "showSuccess", script, true);
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
                    WHERE Role != 'SuperAdmin' AND Status != 'Archived'
                    ORDER BY UserID";

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
                    WHERE Role != 'SuperAdmin' AND Status != 'Archived'
                        AND (Name LIKE @Keyword OR Email LIKE @Keyword OR Role LIKE @Keyword)
                    ORDER BY UserID";

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

        protected void gvAdmins_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
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
                string query = "UPDATE Users SET Status = 'Archived' WHERE UserID = @UserID AND Role != 'SuperAdmin'";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userID);

                    try
                    {
                        conn.Open();
                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                        {
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
    }
}
