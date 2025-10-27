using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using RRCManagementSystem.Helpers; // Required for AESHelper

namespace RRCManagementSystem
{
    public partial class ArchivedAdmins : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Prevent cached/stale view on back-button
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));

            if (!IsPostBack)
            {
                LoadArchivedAdmins(null);
            }
            else
            {
                // Handle postback from JavaScript
                string eventTarget = Request.Form["__EVENTTARGET"];
                string eventArgument = Request.Form["__EVENTARGUMENT"];

                if (!string.IsNullOrEmpty(eventTarget) && eventTarget == gvArchivedAdmins.UniqueID)
                {
                    if (!string.IsNullOrEmpty(eventArgument))
                    {
                        var parts = eventArgument.Split('$');
                        if (parts.Length == 2)
                        {
                            string action = parts[0];
                            if (int.TryParse(parts[1], out int userID))
                            {
                                if (action == "RestoreAdmin")
                                {
                                    RestoreAdmin(userID);
                                }
                                else if (action == "DeletePermanently")
                                {
                                    DeleteAdmin(userID);
                                }
                            }
                        }
                    }
                }
            }
        }

        /* =========================
           Load Data for Archived Admins
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
            // This handles direct LinkButton clicks (not JavaScript postbacks)
            if (!int.TryParse(e.CommandArgument?.ToString(), out int userID))
            {
                return;
            }

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
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spAdmin_Restore", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userID;

                try
                {
                    conn.Open();

                    int rows = 0;

                    // ✅ Read and close the reader immediately
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            rows = reader.GetInt32(0);
                        }
                    } // Reader is disposed here, connection is freed

                    // ✅ Now register the script after reader is closed
                    if (rows > 0)
                    {
                        string script = @"
                    Swal.fire({
                        title: 'Restore Successful!',
                        text: 'The admin account has been restored.',
                        icon: 'success',
                        timer: 3000,
                        showConfirmButton: false
                    }).then(() => {
                        window.location.href = 'ArchivedAdmins.aspx';
                    });";

                        ScriptManager.RegisterStartupScript(this, GetType(), "RestoreAlert", script, true);
                    }
                    else
                    {
                        lblMessage.Text = "<div class='alert alert-danger'><i class='fas fa-exclamation-triangle alert-icon'></i>Admin not found or not in Archived status.</div>";
                        LoadArchivedAdmins(txtSearch.Text);
                    }
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "<div class='alert alert-danger'><i class='fas fa-exclamation-triangle alert-icon'></i>Error restoring admin: " + ex.Message + "</div>";
                    LoadArchivedAdmins(txtSearch.Text);
                }
            }
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

                    int rows = 0;

                    // ✅ Read and close the reader immediately
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            rows = reader.GetInt32(0);
                        }
                    } // Reader is disposed here, connection is freed

                    // ✅ Now register the script after reader is closed
                    if (rows > 0)
                    {
                        string script = @"
                    Swal.fire({
                        title: 'Delete Successful!',
                        text: 'The admin account has been permanently deleted.',
                        icon: 'success',
                        timer: 3000,
                        showConfirmButton: false
                    }).then(() => {
                        window.location.href = 'ArchivedAdmins.aspx';
                    });";

                        ScriptManager.RegisterStartupScript(this, GetType(), "DeleteAlert", script, true);
                    }
                    else
                    {
                        LoadArchivedAdmins(txtSearch.Text);
                    }
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "<div class='alert alert-danger'><i class='fas fa-exclamation-triangle alert-icon'></i>Error deleting admin: " + ex.Message + "</div>";
                    LoadArchivedAdmins(txtSearch.Text);
                }
            }
        }

        protected void gvArchivedAdmins_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvArchivedAdmins.PageIndex = e.NewPageIndex;
            LoadArchivedAdmins(txtSearch.Text);
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
