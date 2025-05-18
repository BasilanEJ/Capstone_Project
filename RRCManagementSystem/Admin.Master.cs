using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class Admin : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // ✅ Enforce only Admin role can use this MasterPage
                if (Session["UserID"] == null || Session["Role"]?.ToString() != "Admin")
                {
                    Response.Redirect("~/Login.aspx");
                    return;
                }

                // ✅ Display admin's name
                lblAdminName.Text = Session["Name"]?.ToString() ?? "Admin";

                // ✅ Load sidebar permissions
                int adminId = Convert.ToInt32(Session["UserID"]);
                LoadSidebarPermissions(adminId);
            }
        }

        private void LoadSidebarPermissions(int adminId)
        {
            List<string> allowedModules = new List<string>();

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString))
            {
                string query = "SELECT ModuleName FROM AdminPermissions WHERE UserID = @UserID AND CanView = 1";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserID", adminId);
                    con.Open();

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        allowedModules.Add(reader["ModuleName"].ToString());
                    }
                }
            }

            ViewState["AllowedModules"] = allowedModules;
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Login.aspx");
        }
    }
}
