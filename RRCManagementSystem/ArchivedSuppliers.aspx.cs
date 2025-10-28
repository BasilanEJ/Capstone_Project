using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class ArchivedSuppliers : System.Web.UI.Page
    {
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 🔐 Require login
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                SafeRedirect("~/Login.aspx");
                return;
            }

            string role = Convert.ToString(Session["Role"]);

            // 🔐 Deny SuperAdmin & Inspector
            if (role == "SuperAdmin" || role == "Inspector")
            {
                SafeRedirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            // 🔐 Must have CanView for ManageSupplier
            if (!HasPermission(userId, "ManageSupplier", "CanView"))
            {
                SafeRedirect("~/Unauthorized.aspx");
                return;
            }

            if (!IsPostBack)
            {
                // Cache permissions in ViewState
                ViewState["CanEdit"] = HasPermission(userId, "ManageSupplier", "CanEdit");
                ViewState["CanDelete"] = HasPermission(userId, "ManageSupplier", "CanDelete");

                LoadArchivedSuppliers();
            }
        }

        /// <summary>
        /// Safe redirect without ThreadAbortException
        /// </summary>
        private void SafeRedirect(string url)
        {
            Response.Redirect(url, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// Checks if the current user has permission for a specific action on a module.
        /// </summary>
        private bool HasPermission(int adminId, string moduleName, string which)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAdminPermission_Check", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", adminId);
                    cmd.Parameters.AddWithValue("@ModuleName", moduleName);
                    cmd.Parameters.AddWithValue("@Permission", which);

                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null && result != DBNull.Value && Convert.ToBoolean(result);
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Loads archived suppliers and binds them to the GridView.
        /// </summary>
        private void LoadArchivedSuppliers()
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("spSupplier_ListArchived", conn))
                using (var da = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    var dt = new DataTable();
                    da.Fill(dt);

                    gvArchivedSuppliers.DataSource = dt;
                    gvArchivedSuppliers.DataBind();

                    if (dt.Rows.Count == 0)
                    {
                        ClientScript.RegisterStartupScript(this.GetType(), "noArchived",
                            "showAlert('Info', 'No archived suppliers found.', 'info');", true);
                    }
                }
            }
            catch (Exception ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "loadError",
                    $"showAlert('Error!', 'Error loading archived suppliers: {ex.Message.Replace("'", "\\'")}', 'error');", true);
            }
        }

        /// <summary>
        /// Handles GridView commands for Restore and Delete.
        /// </summary>
        protected void gvArchivedSuppliers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                if (!int.TryParse(Convert.ToString(e.CommandArgument), out int supplierID))
                    return;

                if (e.CommandName == "RestoreSupplier" && Convert.ToBoolean(ViewState["CanEdit"]))
                {
                    RestoreSupplier(supplierID);
                    LoadArchivedSuppliers();
                }

                if (e.CommandName == "DeleteSupplier" && Convert.ToBoolean(ViewState["CanDelete"]))
                {
                    DeleteSupplier(supplierID);
                    LoadArchivedSuppliers();
                }
            }
            catch (Exception ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "commandError",
                    $"showAlert('Error!', 'Command error: {ex.Message.Replace("'", "\\'")}', 'error');", true);
            }
        }

        /// <summary>
        /// Restores an archived supplier back to Active status.
        /// </summary>
        private void RestoreSupplier(int supplierID)
        {
            try
            {
                int restored = 0;
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("spSupplier_Restore", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SupplierID", supplierID);

                    conn.Open();
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                            restored = Convert.ToInt32(rdr["Restored"]);
                    }
                }

                if (restored == 1)
                {
                    string script = @"
                        Swal.fire({
                            icon: 'success',
                            title: 'Restored!',
                            text: 'Supplier has been restored to active list.',
                            confirmButtonColor: '#2563eb',
                            timer: 2000,
                            showConfirmButton: true
                        });
                    ";
                    ClientScript.RegisterStartupScript(this.GetType(), "restoreSuccess", script, true);
                }
                else
                {
                    ClientScript.RegisterStartupScript(this.GetType(), "restoreWarning",
                        "showAlert('Warning', 'Supplier not found or already active.', 'warning');", true);
                }
            }
            catch (Exception ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "restoreError",
                    $"showAlert('Error!', 'Error restoring supplier: {ex.Message.Replace("'", "\\'")}', 'error');", true);
            }
        }

        /// <summary>
        /// Permanently deletes a supplier from the database.
        /// </summary>
        private void DeleteSupplier(int supplierID)
        {
            try
            {
                int deleted = 0;
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spSupplier_Delete", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SupplierID", supplierID);

                    conn.Open();
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                            deleted = Convert.ToInt32(rdr["Deleted"]);
                    }
                }

                if (deleted == 1)
                {
                    string script = @"
                        Swal.fire({
                            icon: 'success',
                            title: 'Deleted!',
                            text: 'Supplier has been permanently deleted.',
                            confirmButtonColor: '#2563eb',
                            timer: 2000,
                            showConfirmButton: true
                        });
                    ";
                    ClientScript.RegisterStartupScript(this.GetType(), "deleteSuccess", script, true);
                }
                else
                {
                    ClientScript.RegisterStartupScript(this.GetType(), "deleteWarning",
                        "showAlert('Warning', 'Supplier not found or could not be deleted.', 'warning');", true);
                }
            }
            catch (Exception ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "deleteError",
                    $"showAlert('Error!', 'Error deleting supplier: {ex.Message.Replace("'", "\\'")}', 'error');", true);
            }
        }
    }
}