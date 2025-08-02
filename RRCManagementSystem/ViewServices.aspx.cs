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

            // 🔐 Restrict access
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            if (!HasPermission(userId, "ManageServices", "CanView"))
            {
                Response.Redirect("~/Unauthorized.aspx");
                return;
            }

            if (!IsPostBack)
            {
                ViewState["CanEdit"] = HasPermission(userId, "ManageServices", "CanEdit");
                ViewState["CanDelete"] = HasPermission(userId, "ManageServices", "CanDelete");

                LoadServices();
            }
        }

        private bool HasPermission(int adminId, string moduleName, string permissionColumn)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = $"SELECT {permissionColumn} FROM AdminPermissions WHERE UserID = @UserID AND ModuleName = @ModuleName";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", adminId);
                    cmd.Parameters.AddWithValue("@ModuleName", moduleName);

                    try
                    {
                        conn.Open();
                        object result = cmd.ExecuteScalar();
                        return result != null && result != DBNull.Value && Convert.ToBoolean(result);
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        }

        private void LoadServices()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        ServiceID, 
                        Name, 
                        Description, 
                        Price100SQM, 
                        Price200SQM, 
                        PriceAbove200SQM 
                    FROM Services";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    try
                    {
                        conn.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        // Optional: for display only
                        dt.Columns.Add("FormattedServiceID", typeof(string));
                        foreach (DataRow row in dt.Rows)
                        {
                            int id = Convert.ToInt32(row["ServiceID"]);
                            row["FormattedServiceID"] = "Service" + id.ToString("D3");
                        }

                        gvServices.DataSource = dt;
                        gvServices.DataBind();
                    }
                    catch (Exception ex)
                    {
                        lblMessage.Text = "⚠ Error loading services: " + ex.Message;
                        lblMessage.Visible = true;
                    }
                }
            }
        }

        protected void gvServices_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditService" || e.CommandName == "DeleteService")
            {
                int serviceId;
                if (!int.TryParse(e.CommandArgument.ToString(), out serviceId))
                    return;

                if (e.CommandName == "EditService")
                {
                    Response.Redirect($"EditServices.aspx?ServiceID={serviceId}");
                }
                else if (e.CommandName == "DeleteService")
                {
                    try
                    {
                        using (SqlConnection conn = new SqlConnection(connectionString))
                        {
                            conn.Open();
                            SqlCommand cmd = new SqlCommand("DELETE FROM Services WHERE ServiceID = @ServiceID", conn);
                            cmd.Parameters.AddWithValue("@ServiceID", serviceId);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                LoadServices();

                                // ✅ SweetAlert success feedback
                                string script = @"Swal.fire({
                                    icon: 'success',
                                    title: 'Deleted!',
                                    text: 'Service has been deleted.',
                                    timer: 2000,
                                    showConfirmButton: false
                                });";

                                ClientScript.RegisterStartupScript(this.GetType(), "deleteSuccess", script, true);
                            }
                            else
                            {
                                lblMessage.Text = "⚠ Service not found.";
                                lblMessage.Visible = true;
                            }
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

        // ✅ Register each dynamic postback key for SweetAlert
        protected override void Render(HtmlTextWriter writer)
        {
            foreach (GridViewRow row in gvServices.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    string serviceId = gvServices.DataKeys[row.RowIndex].Value.ToString();
                    ClientScript.RegisterForEventValidation(gvServices.UniqueID, "DeleteService$" + serviceId);
                }
            }

            base.Render(writer);
        }
    }
}
