using System;
using System.Web;
using System.Web.Security;

namespace RRCManagementSystem
{
    public partial class SuperAdmin : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // ---- Auth/role gate on EVERY request ----
            var role = Session["Role"] as string;
            if (Session["UserID"] == null || !string.Equals(role, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                SafeRedirect("~/Login.aspx");
                return;
            }

            // ---- Strong no-cache for all protected views ----
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            Response.Cache.AppendCacheExtension("must-revalidate, proxy-revalidate");
        }

        protected string GetActiveClass(string pageName)
        {
            string currentPage = System.IO.Path.GetFileName(Request.Path);
            return string.Equals(currentPage, pageName, StringComparison.OrdinalIgnoreCase) ? "active" : "";
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            // ---- Clear app/session state ----
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

            // ---- Expire session cookie ----
            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.UtcNow.AddDays(-1);
            }

            // ---- Expire FormsAuth cookie (important) ----
            FormsAuthentication.SignOut();
            if (Request.Cookies[FormsAuthentication.FormsCookieName] != null)
            {
                Response.Cookies[FormsAuthentication.FormsCookieName].Value = string.Empty;
                Response.Cookies[FormsAuthentication.FormsCookieName].Expires = DateTime.UtcNow.AddDays(-1);
            }

            // ---- No-cache on the way out ----
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));

            SafeRedirect("~/Login.aspx");
        }

        private void SafeRedirect(string url)
        {
            Response.Redirect(url, false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
