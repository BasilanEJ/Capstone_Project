using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class ViewUser : Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                LoadUsers();
        }

        // Load users into GridView
        private void LoadUsers(string search = "")
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_GetSuperAdmins", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Search", search);
                cmd.Parameters.AddWithValue("@PageIndex", gvUsers.PageIndex + 1);
                cmd.Parameters.AddWithValue("@PageSize", gvUsers.PageSize);

                con.Open();
                DataTable dt = new DataTable();
                dt.Load(cmd.ExecuteReader());

                gvUsers.DataSource = dt;
                gvUsers.DataBind();
            }
        }

        // Handle GridView paging
        protected void gvUsers_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvUsers.PageIndex = e.NewPageIndex;
            LoadUsers(txtSearch.Text.Trim());
        }

        // Handle search
        protected void txtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadUsers(txtSearch.Text.Trim());
        }

        // Hidden LinkButton click triggered from JS/SweetAlert
        protected void lnkHiddenArchive_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(hfArchiveUserID.Value, out int userId) || userId <= 0)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "error",
                    "Swal.fire('Error', 'Invalid User ID!', 'error');", true);
                return;
            }

            ArchiveUser(userId);

            LoadUsers(txtSearch.Text.Trim());
            updUsers.Update();

            ScriptManager.RegisterStartupScript(this, GetType(), "success",
                "Swal.fire('Archived!', 'User has been archived.', 'success');", true);
        }

        // Archive a user in DB
        private void ArchiveUser(int userId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_ArchiveSuperAdmin", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserID", userId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Save edits from modal
        protected void btnSaveEdit_Click(object sender, EventArgs e)
        {
            int userId = Convert.ToInt32(hfEditUserID.Value);
            string name = txtName.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "error",
                    @"Swal.fire('Error', 'Name cannot be empty!', 'error').then(() => {
                var editModal = new bootstrap.Modal(document.getElementById('editModal'));
                editModal.show();
            });", true);
                return;
            }

            // Update DB
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_UpdateSuperAdminName", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Name", name);
                con.Open();
                cmd.ExecuteNonQuery();
            }

            // Refresh GridView immediately
            gvUsers.PageIndex = 0; // optional: reset to first page
            LoadUsers(txtSearch.Text.Trim());
            updUsers.Update();

            // Close modal + success alert
            ScriptManager.RegisterStartupScript(this, GetType(), "closeModal",
                @"var editModal = bootstrap.Modal.getInstance(document.getElementById('editModal'));
          editModal.hide();
          Swal.fire('Updated!', 'User name has been updated.', 'success');", true);
        }
    }
}
