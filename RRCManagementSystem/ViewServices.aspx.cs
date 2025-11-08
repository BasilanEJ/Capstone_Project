using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class ViewServices : Page
    {
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Require login
            if (Session["UserID"] == null || Session["Role"] == null)
            { Response.Redirect("~/Login.aspx"); return; }

            string role = Convert.ToString(Session["Role"]);
            // Keep your original role rule (update if needed)
            if (role == "SuperAdmin" || role == "Inspector")
            { Response.Redirect("~/Login.aspx"); return; }

            int userId = Convert.ToInt32(Session["UserID"]);
            if (!HasPermission(userId, "ManageServices", "CanView"))
            { Response.Redirect("~/Unauthorized.aspx"); return; }

            if (!IsPostBack)
            {
                ViewState["CanEdit"] = HasPermission(userId, "ManageServices", "CanEdit");
                ViewState["CanDisable"] = HasPermission(userId, "ManageServices", "CanDelete"); // reuse delete perm
                LoadServices();
            }
        }

        private bool HasPermission(int userId, string module, string which)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAdminPermission_Check", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@ModuleName", module);
                    cmd.Parameters.AddWithValue("@Permission", which);
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null && result != DBNull.Value && Convert.ToBoolean(result);
                }
            }
            catch
            {
                // If permission check fails, treat as no permission
                return false;
            }
        }

        private void LoadServices()
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spService_List", conn))
                using (var da = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    var dt = new DataTable();
                    da.Fill(dt);

                    gvServices.DataSource = dt;
                    gvServices.DataBind();
                }
            }
            catch (Exception ex)
            {
                AlertError($"Error loading services: {ex.Message}");
            }
        }

        protected void gvServices_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // Validate that we have a numeric ServiceID
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out int serviceId)) return;

            // ✅ Handle View Price
            if (e.CommandName == "ViewPrice")
            {
                // Redirect to SetServicePricing and pass ServiceID via query string
                Response.Redirect("SetServicePricing.aspx?ServiceID=" + serviceId);
                return;
            }


            // Existing EditService logic
            if (e.CommandName == "EditService" && Convert.ToBoolean(ViewState["CanEdit"]))
            {
                Response.Redirect($"EditServices.aspx?ServiceID={serviceId}");
                return;
            }

            // Existing DisableService logic
            if (e.CommandName == "DisableService" && Convert.ToBoolean(ViewState["CanDisable"]))
            {
                try
                {
                    int affected = 0;
                    using (var conn = new SqlConnection(connectionString))
                    using (var cmd = new SqlCommand("dbo.spService_MarkUnavailable", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ServiceID", serviceId);
                        conn.Open();
                        using (var rdr = cmd.ExecuteReader())
                        {
                            if (rdr.Read())
                                affected = Convert.ToInt32(rdr["Affected"]);
                        }
                    }

                    if (affected > 0)
                    {
                        LoadServices();
                        AlertSuccess("Service deleted.");
                    }
                    else
                    {
                        AlertInfo("Service not found.");
                    }
                }
                catch (Exception ex)
                {
                    AlertError($"Error updating service: {ex.Message}");
                }
            }
        }



        protected void gvServices_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            bool canEdit = Convert.ToBoolean(ViewState["CanEdit"]);
            bool canDisable = Convert.ToBoolean(ViewState["CanDisable"]);

            var btnEdit = e.Row.FindControl("btnEdit") as LinkButton;
            var btnDelete = e.Row.FindControl("btnDelete") as LinkButton;

            if (btnEdit != null) btnEdit.Visible = canEdit;
            if (btnDelete != null) btnDelete.Visible = canDisable;
        }

        // ---------- SweetAlert helpers ----------
        private void AlertSuccess(string msg) =>
            ClientScript.RegisterStartupScript(
                this.GetType(), Guid.NewGuid().ToString("N"),
                $"Swal.fire('Success','{Js(msg)}','success');", true);

        private void AlertInfo(string msg) =>
            ClientScript.RegisterStartupScript(
                this.GetType(), Guid.NewGuid().ToString("N"),
                $"Swal.fire('Notice','{Js(msg)}','info');", true);

        private void AlertError(string msg) =>
            ClientScript.RegisterStartupScript(
                this.GetType(), Guid.NewGuid().ToString("N"),
                $"Swal.fire('Error','{Js(msg)}','error');", true);

        private static string Js(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace(@"\", @"\\")
                    .Replace("'", @"\'")
                    .Replace("\"", "\\\"")
                    .Replace("\r", "\\r")
                    .Replace("\n", "\\n");
        }
    }
}
