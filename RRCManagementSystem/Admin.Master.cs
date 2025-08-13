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
            // ✅ Step 1: Authentication check
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            // ✅ Step 2: Restrict access for specific roles
            string role = Session["Role"].ToString();
            if (role == "Inspector" || role == "SuperAdmin")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            // ✅ Step 3: Load module permissions only once
            if (Session["AllowedModules"] == null)
            {
                int userId = Convert.ToInt32(Session["UserID"]);
                LoadSidebarPermissions(userId);
            }

            // ✅ Step 4: Update UI on first load only
            if (!IsPostBack)
            {
                lblAdminName.Text = Session["Name"]?.ToString() ?? "User";
            }
        }

        private void LoadSidebarPermissions(int userId)
        {
            List<string> allowedModules = new List<string>();

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString))
            {
                string query = "SELECT ModuleName FROM AdminPermissions WHERE UserID = @UserID AND CanView = 1";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            allowedModules.Add(reader["ModuleName"].ToString());
                        }
                    }
                }
            }

            // ✅ Step 5: Store modules in Session
            Session["AllowedModules"] = allowedModules;
        }


        protected string GetActiveClass(string page)
        {
            string currentPath = System.IO.Path.GetFileName(Request.Path).ToLower();
            return currentPath == page.ToLower() ? "active" : "";
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            // Clear all session data
            Session.Clear();
            Session.RemoveAll();
            Session.Abandon();

            // Remove authentication flag
            Session["IsAuthenticated"] = null;

            // Invalidate session cookie
            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.UtcNow.AddDays(-1);
            }

            // Prevent browser from caching previous pages
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));

            // Redirect to login
            Response.Redirect("~/Login.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

    }
}
