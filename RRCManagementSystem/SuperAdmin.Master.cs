using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.Security;

namespace RRCManagementSystem
{
    public partial class SuperAdmin : System.Web.UI.MasterPage
    {
        private static readonly string Cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // ---- Auth/role gate on EVERY request ----
            var role = Session["Role"] as string;
            if (Session["UserID"] == null || !string.Equals(role, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                SafeRedirect("~/Login.aspx");
                return;
            }

            int superAdminId = Convert.ToInt32(Session["UserID"]);

            // 🔹 1) Check if account is still active
            if (!IsSuperAdminStatusStillValid(superAdminId))
            {
                ForceLogout("Your account status has been changed. Please contact the root administrator.");
                return;
            }

            // 🔹 2) Check if the session is still valid (single-session enforcement)
            if (!IsSuperAdminSessionValid(superAdminId))
            {
                ForceLogout("You were logged out because your account was accessed from another device.");
                return;
            }

            // ---- Strong no-cache for all protected views ----
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            Response.Cache.AppendCacheExtension("must-revalidate, proxy-revalidate");
        }

        /// <summary>
        /// Verify that the SuperAdmin's account is still active
        /// </summary>
        private bool IsSuperAdminStatusStillValid(int userId)
        {
            try
            {
                using (var conn = new SqlConnection(Cs))
                using (var cmd = new SqlCommand("SELECT Status FROM Users WHERE UserID = @UserID", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    conn.Open();
                    var status = cmd.ExecuteScalar()?.ToString();

                    return status != null &&
                           (status.Equals("Active", StringComparison.OrdinalIgnoreCase) ||
                            status.Equals("Available", StringComparison.OrdinalIgnoreCase));
                }
            }
            catch
            {
                return false; // Fail safe: treat as invalid
            }
        }

        /// <summary>
        /// Single-session check: compare current ASP.NET session vs DB
        /// </summary>
        private bool IsSuperAdminSessionValid(int userId)
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
                return false;
            }
        }

        /// <summary>
        /// Clears the session both in ASP.NET and the database, then redirects to login
        /// </summary>
        private void ForceLogout(string message)
        {
            // Clear CurrentSessionID in the database
            ClearDatabaseSession();

            // Clear session state
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

            // Store message to display on login page (optional)
            Session["LogoutMessage"] = message;

            // Redirect safely
            SafeRedirect("~/Login.aspx");
        }

        /// <summary>
        /// Clears CurrentSessionID in the database
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
                // Fail silently to avoid blocking logout
            }
        }

        protected string GetActiveClass(string pageName)
        {
            string currentPage = System.IO.Path.GetFileName(Request.Path);
            return string.Equals(currentPage, pageName, StringComparison.OrdinalIgnoreCase) ? "active" : "";
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            ForceLogout("You have been logged out successfully.");
        }

        private void SafeRedirect(string url)
        {
            Response.Redirect(url, false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
