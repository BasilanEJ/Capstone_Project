using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class ClientsProfiles : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 🔐 Require Login Only
            if (Session["AdminID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
                LoadApprovedClients();
        }

        private void LoadApprovedClients()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT ClientID, Name, Email, ContactNumber, City, Country
                    FROM Clients
                    WHERE Status = 'Approved'
                    ORDER BY CreatedAt DESC";

                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                gvClients.DataSource = dt;
                gvClients.DataBind();
            }
        }

        protected void gvClients_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvClients.PageIndex = e.NewPageIndex;
            LoadApprovedClients();
        }

        protected void gvClients_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int clientId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "ViewProfile")
            {
                Response.Redirect($"ViewClientProfile.aspx?ClientID={clientId}");
            }
            else if (e.CommandName == "ArchiveClient")
            {
                ArchiveClient(clientId);
            }
        }

        private void ArchiveClient(int clientId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string updateQuery = "UPDATE Clients SET Status = 'Inactive' WHERE ClientID = @ClientID";

                using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@ClientID", clientId);

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        LoadApprovedClients(); // Refresh grid
                        ShowSweetAlert("Archived", "Client has been archived successfully.", "success");
                    }
                    catch (Exception ex)
                    {
                        ShowSweetAlert("Error", "Failed to archive client. " + ex.Message, "error");
                    }
                }
            }
        }

        private void ShowSweetAlert(string title, string message, string icon)
        {
            string script = $@"
    <script>
        Swal.fire({{
            title: '{title}',
            text: '{message}',
            icon: '{icon}',
            confirmButtonColor: '#007bff'
        }});
    </script>";

            ScriptManager.RegisterStartupScript(this, GetType(), "SweetAlert", script, false);
        }


    }
}
