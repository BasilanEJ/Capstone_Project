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
            // 🔐 Require login
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string role = Session["Role"].ToString();

            // 🔐 Block SuperAdmin and Inspector (kept, as before)
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadApprovedClients();
            }
        }

        private void LoadApprovedClients()
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spClients_ListApproved", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                var dt = new DataTable();
                da.Fill(dt);

                gvClients.DataSource = dt;
                gvClients.DataBind();

                // Optional: show a hint if nothing returned
                if (dt.Rows.Count == 0)
                {
                    // You can replace with a label on the page if you prefer
                    // lblEmpty.Text = "No approved clients found.";
                    System.Diagnostics.Debug.WriteLine("No approved clients found.");
                }
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
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spClient_UpdateStatus", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = "Inactive";

                try
                {
                    conn.Open();
                    var rows = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);

                    LoadApprovedClients(); // Refresh grid
                    ShowSweetAlert(rows > 0 ? "Archived" : "Not Found",
                                   rows > 0 ? "Client has been archived successfully." : "Client not found.",
                                   rows > 0 ? "success" : "warning");
                }
                catch (Exception ex)
                {
                    ShowSweetAlert("Error", "Failed to archive client. " + ex.Message, "error");
                }
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
        confirmButtonColor: '#007bff'
    }});
</script>";
            ScriptManager.RegisterStartupScript(this, GetType(), "SweetAlert" + Guid.NewGuid(), script, false);
        }
    }
}
