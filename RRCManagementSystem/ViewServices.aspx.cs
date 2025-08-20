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
            // 🔐 Require login
            if (Session["UserID"] == null || Session["Role"] == null)
            { Response.Redirect("~/Login.aspx"); return; }

            string role = Convert.ToString(Session["Role"]);
            if (role == "SuperAdmin" || role == "Inspector")
            { Response.Redirect("~/Login.aspx"); return; }

            int userId = Convert.ToInt32(Session["UserID"]);
            if (!HasPermission(userId, "ManageServices", "CanView"))
            { Response.Redirect("~/Unauthorized.aspx"); return; }

            if (!IsPostBack)
            {
                ViewState["CanEdit"] = HasPermission(userId, "ManageServices", "CanEdit");
                ViewState["CanDelete"] = HasPermission(userId, "ManageServices", "CanDelete");
                LoadServices();
            }
        }

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
            catch { return false; }
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

                    // Optional formatted ID
                    if (!dt.Columns.Contains("FormattedServiceID"))
                        dt.Columns.Add("FormattedServiceID", typeof(string));
                    foreach (DataRow row in dt.Rows)
                        row["FormattedServiceID"] = "Service" + ((int)row["ServiceID"]).ToString("D3");

                    gvServices.DataSource = dt;
                    gvServices.DataBind();

                    lblMessage.Text = "";
                    lblMessage.Visible = false;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠ Error loading services: " + ex.Message;
                lblMessage.Visible = true;
            }
        }

        protected void gvServices_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // Expect CommandArgument to be ServiceID
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out int serviceId))
                return;

            if (e.CommandName == "EditService" && Convert.ToBoolean(ViewState["CanEdit"]))
            {
                Response.Redirect($"EditServices.aspx?ServiceID={serviceId}");
                return;
            }

            if (e.CommandName == "DeleteService" && Convert.ToBoolean(ViewState["CanDelete"]))
            {
                try
                {
                    int affected = 0;
                    using (var conn = new SqlConnection(connectionString))
                    using (var cmd = new SqlCommand("dbo.spService_Delete", conn))
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
                        // SweetAlert success
                        ClientScript.RegisterStartupScript(GetType(), "delok", @"
Swal.fire({ icon:'success', title:'Deleted!', text:'Service has been deleted.', timer:2000, showConfirmButton:false });
", true);
                    }
                    else
                    {
                        lblMessage.Text = "⚠ Service not found.";
                        lblMessage.Visible = true;
                    }
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "⚠ Error deleting service: " + ex.Message;
                    lblMessage.Visible = true;
                }
            }
        }
    }
}
