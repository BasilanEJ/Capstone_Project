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

            // 🔐 Block SuperAdmin and Inspector
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            // 🔐 Check CanView permission for ManageClients (or use "ArchivedClients" if it's a separate module)
           

            if (!IsPostBack)
            {
                LoadArchivedClients();
            }
        }


        private void LoadArchivedClients()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
    SELECT ClientID,
           LastName + ', ' + FirstName +
           CASE 
               WHEN MiddleName IS NULL OR LTRIM(RTRIM(MiddleName)) = '' THEN ''
               ELSE ' ' + MiddleName
           END AS Name,
           Email, ContactNumber, City, Country
    FROM Clients
    WHERE Status = 'Inactive'
    ORDER BY CreatedAt DESC";


                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

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
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "UPDATE Clients SET Status = @Status WHERE ClientID = @ClientID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Status", newStatus);
                    cmd.Parameters.AddWithValue("@ClientID", clientId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            LoadArchivedClients();
            ShowSweetAlert("Success", successMessage, "success");
        }

        private void DeleteClient(int clientId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Clients WHERE ClientID = @ClientID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ClientID", clientId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            LoadArchivedClients();
            ShowSweetAlert("Deleted", "Client has been permanently deleted.", "success");
        }

        private void ShowSweetAlert(string title, string message, string icon)
        {
            string script = $@"
            <script>
                Swal.fire({{
                    title: '{title}',
                    text: '{message}',
                    icon: '{icon}',
                    confirmButtonColor: '#3085d6'
                }});
            </script>";
            ScriptManager.RegisterStartupScript(this, GetType(), "SweetAlert", script, false);
        }
    }
}
