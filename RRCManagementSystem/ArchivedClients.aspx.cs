using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class ArchivedClients : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 🔐 Require login
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string role = Session["Role"].ToString();

            // 🔐 Block SuperAdmin and Inspector (kept as in your code)
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

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

        protected void gvArchivedClients_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int clientId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "RestoreClient")
            {
                UpdateClientStatus(clientId, "Approved", "✅ Client restored successfully.");
            }
            else if (e.CommandName == "DeleteClient")
            {
                DeleteClient(clientId);
            }
        }

        protected void btnRestoreHidden_Click(object sender, EventArgs e)
        {
            int clientId = Convert.ToInt32(hfClientID.Value);
            UpdateClientStatus(clientId, "Approved", "✅ Client restored successfully.");
        }

        protected void btnDeleteHidden_Click(object sender, EventArgs e)
        {
            int clientId = Convert.ToInt32(hfClientID.Value);
            DeleteClient(clientId);
        }

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

        private void ShowSweetAlert(string title, string message, string icon)
        {
            string script = $@"
<script>
Swal.fire({{
    title: '{title}',
    text: '{message.Replace("'", "\\'")}',
    icon: '{icon}',
    confirmButtonColor: '#3085d6'
}});
</script>";
            ScriptManager.RegisterStartupScript(this, GetType(), "SweetAlert" + Guid.NewGuid(), script, false);
        }
    }
}
