using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class ManageServices : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // 🔐 Require login
                if (Session["AdminID"] == null)
                {
                    Response.Redirect("~/Login.aspx");
                    return;
                }

                int adminId = Convert.ToInt32(Session["AdminID"]);
                if (!HasPermission(adminId, "ManageServices"))
                {
                    Response.Redirect("~/Unauthorized.aspx");
                    return;
                }

                // ✅ Page logic goes here if permission is granted
            }
        }

        private bool HasPermission(int userId, string moduleName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT CanView FROM AdminPermissions 
                                 WHERE UserID = @UserID AND ModuleName = @ModuleName";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@ModuleName", moduleName);

                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null && Convert.ToBoolean(result);
                }
            }
        }
    }
}