using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class ArchivedClients : Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string role = Session["Role"].ToString();

            if (role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
                role.Equals("Inspector", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);
            ViewState["CanEditClients"] = HasEditPermission(userId, "ManageClient");
            ViewState["CanDeleteClients"] = HasDeletePermission(userId, "ManageClient");

            if (!IsPostBack)
            {
                LoadArchivedClients();
            }
        }

        private void LoadArchivedClients()
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spClients_ListArchived", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                var dt = new DataTable();
                da.Fill(dt);

                gvArchivedClients.DataSource = dt;
                gvArchivedClients.DataBind();
            }
        }

        protected void gvArchivedClients_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvArchivedClients.PageIndex = e.NewPageIndex;
            LoadArchivedClients();
        }

        #region Hidden Button Click Handlers
        protected void btnRestoreHidden_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(hfClientID.Value, out int clientId)) return;

            bool canEdit = ViewState["CanEditClients"] != null && (bool)ViewState["CanEditClients"];
            if (!canEdit)
            {
                ShowSweetAlert("No Permission", "You do not have permission to restore clients.", "warning");
                return;
            }

            UpdateClientStatus(clientId, "Approved", "✅ Client restored successfully.");
        }

        protected void btnDeleteHidden_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(hfClientID.Value, out int clientId)) return;

            bool canDelete = ViewState["CanDeleteClients"] != null && (bool)ViewState["CanDeleteClients"];
            if (!canDelete)
            {
                ShowSweetAlert("No Permission", "You do not have permission to delete clients.", "warning");
                return;
            }

            DeleteClient(clientId);
        }
        #endregion

        #region Client Actions
        private void UpdateClientStatus(int clientId, string newStatus, string successMessage)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spClient_UpdateStatus", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = newStatus;

                conn.Open();
                var rows = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);

                LoadArchivedClients();
                ShowSweetAlert(rows > 0 ? "Success" : "Not Found",
                               rows > 0 ? successMessage : "Client not found.",
                               rows > 0 ? "success" : "warning");
            }
        }

        private void DeleteClient(int clientId)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spClient_Delete", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;

                conn.Open();
                var rows = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);

                LoadArchivedClients();
                ShowSweetAlert(rows > 0 ? "Deleted" : "Not Found",
                               rows > 0 ? "Client has been permanently deleted." : "Client not found.",
                               rows > 0 ? "success" : "warning");
            }
        }
        #endregion

        #region Permissions
        private bool HasEditPermission(int userId, string moduleName)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAdminPermission_CanEdit", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                    cmd.Parameters.Add("@ModuleName", SqlDbType.NVarChar, 100).Value = moduleName;

                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null && Convert.ToInt32(result) == 1;
                }
            }
            catch
            {
                return false;
            }
        }
        protected void gvArchivedClients_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // We are using hidden buttons for actual actions, so this can stay empty
        }

        private bool HasDeletePermission(int userId, string moduleName)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAdminPermission_CanDelete", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                    cmd.Parameters.Add("@ModuleName", SqlDbType.NVarChar, 100).Value = moduleName;

                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null && Convert.ToInt32(result) == 1;
                }
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region SweetAlert
        private void ShowSweetAlert(string title, string message, string icon)
        {
            string script = $@"
<script>
Swal.fire({{
    title: '{title}',
    text: '{message.Replace("'", "\\'")}',
    icon: '{icon}',
    confirmButtonColor: '#007bff'
}});
</script>";
            ScriptManager.RegisterStartupScript(this, GetType(), "SweetAlert" + Guid.NewGuid(), script, false);
        }
        #endregion
    }
}
