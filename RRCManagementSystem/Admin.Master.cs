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

            int userId = Convert.ToInt32(Session["UserID"]);
            string role = Session["Role"].ToString();

            // 🔹 NEW: Check if user status is still Active or Available
            if (!IsUserStatusStillValid(userId))
            {
                ForceLogout("Your account status has been changed. Please contact the administrator.");
                return; // stop further execution
            }

            // Admin master is for Admin role only
            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                SafeRedirect("~/Login.aspx");
                return;
            }

            // ----- Step 2: No-cache headers for all protected views -----
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            Response.Cache.AppendCacheExtension("must-revalidate, proxy-revalidate");

            // ----- Step 3: Load sidebar permissions (once) -----
            if (Session["AllowedModules"] == null)
            {
                LoadSidebarPermissions(userId);
            }

            // ----- Step 4: UI init on first load -----
            if (!IsPostBack)
            {
                lblAdminName.Text = (Session["Name"] as string) ?? "User";
            }
        }

        // ✅ Check database to see if user's status is still valid
        private bool IsUserStatusStillValid(int userId)
        {
            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand("SELECT Status FROM Users WHERE UserID = @UserID", con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                con.Open();

                var status = cmd.ExecuteScalar()?.ToString();

                // Only allow Active or Available
                return status != null &&
                       (status.Equals("Active", StringComparison.OrdinalIgnoreCase) ||
                        status.Equals("Available", StringComparison.OrdinalIgnoreCase));
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

        // Helper to check visibility in .master markup
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

        // 🔹 Force logout if status changes or if user clicks logout
        private void ForceLogout(string message)
        {
            // Clear session
            Session.Clear();
            Session.RemoveAll();
            Session.Abandon();

            // Expire session cookie
            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.UtcNow.AddDays(-1);
            }

            // Expire FormsAuth cookie
            FormsAuthentication.SignOut();
            if (Request.Cookies[FormsAuthentication.FormsCookieName] != null)
            {
                Response.Cookies[FormsAuthentication.FormsCookieName].Value = string.Empty;
                Response.Cookies[FormsAuthentication.FormsCookieName].Expires = DateTime.UtcNow.AddDays(-1);
            }

            // Optional: store message to display on login page
            Session["LogoutMessage"] = message;

            SafeRedirect("~/Login.aspx");
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            ForceLogout("You have been logged out successfully.");
        }

        private void SafeRedirect(string url)
        {
            // Avoid ThreadAbortException
            Response.Redirect(url, false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
