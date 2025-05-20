using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace RRCManagementSystem
{
    public partial class ViewServices : System.Web.UI.Page
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

            // 🔐 Deny access to SuperAdmin and Inspector
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            // ✅ Check CanView permission for ManageServices
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

                        gvServices.DataSource = dt;
                        gvServices.DataBind();
                    }
                    catch (Exception ex)
                    {
                        lblMessage.Text = "⚠ Error loading services: " + ex.Message;
                    }
                }
            }
        }

        protected void gvServices_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            int serviceID = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "EditService" && Convert.ToBoolean(ViewState["CanEdit"]))
            {
                Response.Redirect($"EditServices.aspx?ServiceID={serviceID}");
            }

            if (e.CommandName == "DeleteService" && Convert.ToBoolean(ViewState["CanDelete"]))
            {
                DeleteService(serviceID);
            }
        }

        private void DeleteService(int serviceID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Services WHERE ServiceID = @ServiceID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ServiceID", serviceID);

                    try
                    {
                        conn.Open();
                        int rows = cmd.ExecuteNonQuery();

                        lblMessage.Text = rows > 0
                            ? "✅ Service deleted successfully."
                            : "⚠ Service not found.";

                        LoadServices();
                    }
                    catch (Exception ex)
                    {
                        lblMessage.Text = "⚠ Error deleting service: " + ex.Message;
                    }
                }
            }
        }
    }
}