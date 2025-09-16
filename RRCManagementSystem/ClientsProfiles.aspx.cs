using RRCManagementSystem.Helpers;
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

            var role = Session["Role"].ToString();

            // 🔐 Block SuperAdmin and Inspector
            if (role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
                role.Equals("Inspector", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            // ✅ Check CanEdit permission for ManageClient
            int userId = Convert.ToInt32(Session["UserID"]);
            bool canEdit = HasEditPermission(userId, "ManageClient");
            ViewState["CanEditClients"] = canEdit;

            if (!IsPostBack)
            {
                LoadApprovedClients();
            }
        }

        #region Permissions
        private bool HasEditPermission(int userId, string moduleName)
        {
            try
            {
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAdminPermission_CanEdit", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                    cmd.Parameters.Add("@ModuleName", SqlDbType.NVarChar, 100).Value = moduleName;

                    con.Open();
                    object result = cmd.ExecuteScalar();

                    // SP returns 1 for CanEdit, 0 otherwise
                    return result != null && result != DBNull.Value && Convert.ToInt32(result) == 1;
                }
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Load Clients
        private void LoadApprovedClients()
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spClients_ListApproved", conn))
                using (var da = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    var dt = new DataTable();
                    da.Fill(dt);

                    // ✅ Add decrypted columns for GridView
                    if (!dt.Columns.Contains("Email")) dt.Columns.Add("Email", typeof(string));
                    if (!dt.Columns.Contains("ContactNumber")) dt.Columns.Add("ContactNumber", typeof(string));
                    if (!dt.Columns.Contains("City")) dt.Columns.Add("City", typeof(string));
                    if (!dt.Columns.Contains("Country")) dt.Columns.Add("Country", typeof(string));

                    // ✅ Decrypt each row
                    foreach (DataRow row in dt.Rows)
                    {
                        // Email
                        if (row["EmailEnc"] != DBNull.Value)
                            row["Email"] = AESHelper.DecryptEmail(row["EmailEnc"].ToString());

                        // Contact
                        if (row["ContactEnc"] != DBNull.Value)
                            row["ContactNumber"] = AESHelper.DecryptField(row["ContactEnc"].ToString());

                        // City
                        if (row["CityEnc"] != DBNull.Value)
                            row["City"] = AESHelper.DecryptField(row["CityEnc"].ToString());

                        // Country
                        if (row["CountryEnc"] != DBNull.Value)
                            row["Country"] = AESHelper.DecryptField(row["CountryEnc"].ToString());
                    }

                    // ✅ Bind the decrypted DataTable to the GridView
                    gvClients.DataSource = dt;
                    gvClients.DataBind();
                }
            }
            catch (Exception ex)
            {
                ShowSweetAlert("Error", "Failed to load clients. " + ex.Message, "error");
            }
        }

        #endregion

        #region GridView Events
        protected void gvClients_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvClients.PageIndex = e.NewPageIndex;
            LoadApprovedClients();
        }

        protected void gvClients_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            bool canEdit = ViewState["CanEditClients"] != null && (bool)ViewState["CanEditClients"];

            var btnArchive = e.Row.FindControl("btnArchive") as Button;
            if (btnArchive != null)
            {
                btnArchive.Enabled = canEdit;
                if (!canEdit)
                {
                    btnArchive.ToolTip = "You do not have permission to archive clients.";
                    btnArchive.CssClass += " disabled"; // optional Bootstrap styling
                }
            }
        }

        protected void gvClients_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!int.TryParse(e.CommandArgument.ToString(), out int clientId))
            {
                ShowSweetAlert("Error", "Invalid Client ID.", "error");
                return;
            }

            if (e.CommandName == "ViewProfile")
            {
                Response.Redirect($"ViewClientProfile.aspx?ClientID={clientId}");
            }
            else if (e.CommandName == "ArchiveClient")
            {
                bool canEdit = ViewState["CanEditClients"] != null && (bool)ViewState["CanEditClients"];
                if (!canEdit)
                {
                    ShowSweetAlert("No Permission", "You do not have permission to archive clients.", "warning");
                    return;
                }

                ArchiveClient(clientId);
            }
        }
        #endregion

        #region Archive Client
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
        #endregion

        #region SweetAlert Helper
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
