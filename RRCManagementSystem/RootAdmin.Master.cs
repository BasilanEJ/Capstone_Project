using System;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace RRCManagementSystem
{
    public partial class RootAdmin : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Get current page file name
            string currentPage = System.IO.Path.GetFileName(Request.Path);
            var role = Session["Role"] as string;

            // --- RootAdmin access check ---
            // Only allow users with Role="RootAdmin" and a valid UserID
            if (Session["UserID"] == null || !string.Equals(role ?? "", "RootAdmin", StringComparison.OrdinalIgnoreCase))
            {
                // If session invalid, redirect to Login
                SafeRedirect("~/Login.aspx");
                return;
            }

            // --- Only run once ---
            if (!IsPostBack)
            {
                // Display welcome message safely
                lblRootAdminName.Text = "Welcome, " + (Session["Name"] ?? "Root Admin");
            }

            // --- Disable caching to prevent back-button issues ---
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetRevalidation(System.Web.HttpCacheRevalidation.AllCaches);
            Response.Cache.AppendCacheExtension("must-revalidate, proxy-revalidate");
        }

        /// <summary>
        /// Redirects safely without causing ThreadAbortException
        /// </summary>
        private void SafeRedirect(string url)
        {
            Response.Redirect(url, false);
            Context.ApplicationInstance.CompleteRequest();
        }
    


/// <summary>
/// Returns "active" if the current page matches the given page name (for sidebar highlighting).
/// </summary>
public string GetActiveClasses(string pageName)

        {
            string currentPage = System.IO.Path.GetFileName(Request.Path);
            return string.Equals(currentPage, pageName, StringComparison.OrdinalIgnoreCase) ? "active" : "";
        }

        /// <summary>
        /// Logout logic for RootAdmin. Clears session, cookies, and redirects to Login.
        /// </summary>
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            // ---- Clear all Session values ----
            Session.Remove("IsAuthenticated");
            Session.Remove("UserID");
            Session.Remove("Role");
            Session.Remove("Name");
            Session.Remove("Email");
            Session.Remove("Pending2FA_UserID");
            Session.Remove("Pending2FA_Email");
            Session.Remove("Pending2FA_Name");
            Session.Remove("Pending2FA_Role");

            Session.Clear();
            Session.RemoveAll();
            Session.Abandon();

            // ---- Expire ASP.NET Session Cookie ----
            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.UtcNow.AddDays(-1);
            }

            // ---- Expire Forms Authentication Cookie ----
            FormsAuthentication.SignOut();
            if (Request.Cookies[FormsAuthentication.FormsCookieName] != null)
            {
                Response.Cookies[FormsAuthentication.FormsCookieName].Value = string.Empty;
                Response.Cookies[FormsAuthentication.FormsCookieName].Expires = DateTime.UtcNow.AddDays(-1);
            }

            // ---- Prevent Caching after logout ----
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));

            SafeRedirect("~/Login.aspx");
        }

        /// <summary>
        /// Safe redirect method to prevent ThreadAbortException.
        /// </summary>
       
    }
}
