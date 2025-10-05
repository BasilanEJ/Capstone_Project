using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace RRCManagementSystem
{
    public partial class RootAdmin : System.Web.UI.MasterPage
    {
        private static readonly string Cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Get current page file name
            string currentPage = System.IO.Path.GetFileName(Request.Path);
            var role = Session["Role"] as string;

            // --- RootAdmin access check ---
            if (Session["UserID"] == null || !string.Equals(role ?? "", "RootAdmin", StringComparison.OrdinalIgnoreCase))
            {
                // If session invalid, redirect to Login
                SafeRedirect("~/Login.aspx");
                return;
            }

            int rootAdminId = Convert.ToInt32(Session["UserID"]);

            // 🔹 1) Single-session enforcement
            if (!IsRootAdminSessionValid(rootAdminId))
            {
                ForceLogout("You were logged out because your account was accessed from another device.");
                return;
            }

            // --- Only run once on first load ---
            if (!IsPostBack)
            {
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
        /// Checks if the current RootAdmin session matches the one stored in the database
        /// </summary>
        private bool IsRootAdminSessionValid(int userId)
        {
            if (Session["SessionID"] == null) return false;

            try
            {
                Guid currentSessionID;
                if (!Guid.TryParse(Session["SessionID"].ToString(), out currentSessionID))
                    return false;

                using (var conn = new SqlConnection(Cs))
                using (var cmd = new SqlCommand("SELECT CurrentSessionID FROM Users WHERE UserID = @UserID", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    conn.Open();

                    var dbSession = cmd.ExecuteScalar();

                    return dbSession != null && (Guid)dbSession == currentSessionID;
                }
            }
            catch
            {
                return false; // Fail-safe: force logout if check fails
            }
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
        /// Force logout if RootAdmin logs in somewhere else or manually logs out
        /// </summary>
        private void ForceLogout(string message)
        {
            ClearDatabaseSession();

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

            // Optional: Store message to display on login page
            Session["LogoutMessage"] = message;

            // ---- Prevent Caching after logout ----
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));

            SafeRedirect("~/Login.aspx");
        }

        /// <summary>
        /// Clears CurrentSessionID in the database for this RootAdmin
        /// </summary>
        private void ClearDatabaseSession()
        {
            try
            {
                if (Session["UserID"] != null)
                {
                    using (var conn = new SqlConnection(Cs))
                    using (var cmd = new SqlCommand(
                        "UPDATE Users SET CurrentSessionID = NULL, CurrentSessionAt = NULL WHERE UserID = @UserID", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", Convert.ToInt32(Session["UserID"]));
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                // Fail silently, logout continues
            }
        }

        /// <summary>
        /// Logout button click - manually logs out RootAdmin
        /// </summary>
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            ForceLogout("You have been logged out successfully.");
        }

        /// <summary>
        /// Redirect safely without causing ThreadAbortException
        /// </summary>
        private void SafeRedirect(string url)
        {
            Response.Redirect(url, false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
