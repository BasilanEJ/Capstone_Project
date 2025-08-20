using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
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
                BindUsers(null);

                // show toast if redirected from actions
                if (Request.QueryString["archived"] == "1")
                    Toast("Archived!", "User has been successfully archived.", "success");
            }
        }

        /* =========================
           Data binding via SP
           ========================= */
        private void BindUsers(string keyword)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spUsers_List", conn))
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
            BindUsers(txtSearch.Text);
        }

        /* =========================
           Grid actions
           ========================= */
        protected void gvAdmins_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditAdmin" && int.TryParse(e.CommandArgument.ToString(), out int userID))
            {
                Response.Redirect($"EditAdmin.aspx?UserID={userID}", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        // Fired by your SweetAlert confirmation handler
        protected void btnConfirmArchive_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(hfUserToArchive.Value, out int userId))
            {
                lblMessage.Text = "⚠ Invalid user selection.";
                return;
            }

            ArchiveUser(userId);
        }

        private void ArchiveUser(int userID)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spUser_Archive", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userID;

                try
                {
                    conn.Open();
                    // spUser_Archive returns @@ROWCOUNT as RowsAffected
                    int rows = 0;
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read() && rdr["RowsAffected"] != DBNull.Value)
                            rows = Convert.ToInt32(rdr["RowsAffected"]);
                    }

                    if (rows > 0)
                    {
                        // redirect to force fresh bind and show toast
                        Response.Redirect("ViewAdmin.aspx?archived=1", false);
                        Context.ApplicationInstance.CompleteRequest();
                    }
                    else
                    {
                        lblMessage.Text = "⚠ No matching user found to archive (or user is SuperAdmin).";
                    }
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "⚠ Error archiving user: " + ex.Message;
                }
            }
        }

        /* =========================
           UI helpers
           ========================= */
        private void Toast(string title, string text, string icon)
        {
            var script = $@"Swal.fire({{
                icon: '{icon}',
                title: '{title}',
                text: '{text}',
                showConfirmButton: false,
                timer: 1800
            }});";
            ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(), script, true);
        }
    }
}
