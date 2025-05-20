using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class SalesAndTransactions : System.Web.UI.Page
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

            // 🔐 Check CanView permission for Sales&Transaction
            if (!HasPermission(userId, "Sales&Transaction"))
            {
                Response.Redirect("~/Unauthorized.aspx");
                return;
            }

            if (!IsPostBack)
            {
                // ✅ Page logic here (e.g., load sales data)
            }
        }



        private bool HasPermission(int adminId, string moduleName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT CanView FROM AdminPermissions 
                                 WHERE UserID = @UserID AND ModuleName = @ModuleName";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", adminId);
                    cmd.Parameters.AddWithValue("@ModuleName", moduleName);

                    try
                    {
                        conn.Open();
                        object result = cmd.ExecuteScalar();
                        return result != null && Convert.ToBoolean(result);
                    }
                    catch (Exception ex)
                    {
                        Response.Write("⚠ Error checking permission: " + ex.Message);
                        return false;
                    }
                }
            }
        }
    }
}