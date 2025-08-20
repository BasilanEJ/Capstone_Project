using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class Admin : System.Web.UI.MasterPage
    {
        private static readonly string Cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // ----- Step 1: Authentication / Role gate -----
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                SafeRedirect("~/Login.aspx");
                return;
            }

            string role = Session["Role"].ToString();
            // Admin master is for Admin role only (not Inspector, not SuperAdmin)
            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                SafeRedirect("~/Login.aspx");
                return;
            }

            // ----- Step 2: No-cache headers for all protected views -----
            // Prevent Back/Forward from showing stale content after logout
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            Response.Cache.AppendCacheExtension("must-revalidate, proxy-revalidate");

            // ----- Step 3: Load sidebar permissions (once) -----
            if (Session["AllowedModules"] == null)
            {
                int userId;
                if (!int.TryParse(Session["UserID"].ToString(), out userId))
                {
                    SafeRedirect("~/Login.aspx");
                    return;
                }
                LoadSidebarPermissions(userId);
            }

            // ----- Step 4: UI init on first load -----
            if (!IsPostBack)
            {
                lblAdminName.Text = (Session["Name"] as string) ?? "User";
            }
        }

        private void LoadSidebarPermissions(int userId)
        {
            var allowedModules = new List<string>();

            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(
                "SELECT ModuleName FROM AdminPermissions WHERE UserID = @UserID AND CanView = 1", con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                con.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var name = reader["ModuleName"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(name))
                            allowedModules.Add(name);
                    }
                }
            }

            Session["AllowedModules"] = allowedModules;
        }

        // Helper to check visibility in .master markup:
        // <% if (IsModuleAllowed("ViewTeams")) { %> ... <% } %>
        public bool IsModuleAllowed(string moduleName)
        {
            var list = Session["AllowedModules"] as List<string>;
            if (list == null || string.IsNullOrWhiteSpace(moduleName)) return false;
            return list.Contains(moduleName, StringComparer.OrdinalIgnoreCase);
        }

        protected string GetActiveClass(string page)
        {
            string currentPath = System.IO.Path.GetFileName(Request.Path).ToLowerInvariant();
            return currentPath == (page ?? string.Empty).ToLowerInvariant() ? "active" : "";
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            // ----- Clear app/session state -----
            Session.Remove("IsAuthenticated");
            Session.Remove("UserID");
            Session.Remove("Role");
            Session.Remove("Name");
            Session.Remove("Email");
            Session.Remove("AllowedModules");
            Session.Remove("Pending2FA_UserID");
            Session.Remove("Pending2FA_Email");
            Session.Remove("Pending2FA_Name");
            Session.Remove("Pending2FA_Role");

            Session.Clear();
            Session.RemoveAll();
            Session.Abandon();

            // ----- Expire session cookie -----
            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.UtcNow.AddDays(-1);
            }

            // ----- Expire FormsAuth cookie (important even if no OTP) -----
            FormsAuthentication.SignOut();
            if (Request.Cookies[FormsAuthentication.FormsCookieName] != null)
            {
                Response.Cookies[FormsAuthentication.FormsCookieName].Value = string.Empty;
                Response.Cookies[FormsAuthentication.FormsCookieName].Expires = DateTime.UtcNow.AddDays(-1);
            }

            // ----- Strong no-cache on the way out -----
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));

            SafeRedirect("~/Login.aspx");
        }

        private void SafeRedirect(string url)
        {
            // Avoid ThreadAbortException + ensure the pipeline stops
            Response.Redirect(url, false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
