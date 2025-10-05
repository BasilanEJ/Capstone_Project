using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.Security;

namespace RRCManagementSystem
{
    public partial class Client : System.Web.UI.MasterPage
    {
        private static readonly string Cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // ---- 1) Auth gate on EVERY request ----
            if (Session["ClientID"] == null)
            {
                SafeRedirect("~/Login.aspx");
                return;
            }

            int clientId = Convert.ToInt32(Session["ClientID"]);

            // ---- 2) Single-session enforcement ----
            if (!IsClientSessionValid(clientId))
            {
                ForceLogout("You were logged out because your account was accessed from another device.");
                return;
            }

            // ---- 3) Strong no-cache for protected views ----
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            Response.Cache.AppendCacheExtension("must-revalidate, proxy-revalidate");

            // ---- 4) First-load UI init ----
            if (!IsPostBack)
            {
                LoadClientProfile();
            }
        }

        /// <summary>
        /// Verify the current client session matches the database session
        /// </summary>
        private bool IsClientSessionValid(int clientId)
        {
            if (Session["SessionID"] == null) return false;

            try
            {
                Guid currentSessionID;
                if (!Guid.TryParse(Session["SessionID"].ToString(), out currentSessionID))
                    return false;

                using (var conn = new SqlConnection(Cs))
                using (var cmd = new SqlCommand("SELECT CurrentSessionID FROM Clients WHERE ClientID = @ClientID", conn))
                {
                    cmd.Parameters.AddWithValue("@ClientID", clientId);
                    conn.Open();

                    var dbSession = cmd.ExecuteScalar();
                    return dbSession != null && (Guid)dbSession == currentSessionID;
                }
            }
            catch
            {
                // Fail-safe: logout if check fails
                return false;
            }
        }

        private void LoadClientProfile()
        {
            var cachedName = Session["ClientName"] as string;
            if (!string.IsNullOrWhiteSpace(cachedName))
            {
                lblClientName.Text = cachedName;
                return;
            }

            if (!int.TryParse(Session["ClientID"]?.ToString(), out var clientId))
            {
                SafeRedirect("~/Login.aspx");
                return;
            }

            try
            {
                using (var con = new SqlConnection(Cs))
                using (var cmd = new SqlCommand("SELECT Name FROM Clients WHERE ClientID = @ClientID", con))
                {
                    cmd.Parameters.AddWithValue("@ClientID", clientId);
                    con.Open();
                    var result = cmd.ExecuteScalar() as string;

                    var name = string.IsNullOrWhiteSpace(result) ? "My Profile" : result.Trim();
                    lblClientName.Text = name;
                    Session["ClientName"] = name; // cache for later requests
                }
            }
            catch
            {
                lblClientName.Text = "My Profile";
            }
        }

        /// <summary>
        /// Logout button click handler
        /// </summary>
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            ForceLogout("You have been logged out successfully.");
        }

        /// <summary>
        /// Force logout logic (manual logout or session invalidation)
        /// </summary>
        private void ForceLogout(string message)
        {
            // Clear session in database
            ClearDatabaseSession();

            // Clear ASP.NET session
            Session.Remove("IsAuthenticated");
            Session.Remove("ClientID");
            Session.Remove("ClientName");
            Session.Remove("Role");
            Session.Remove("Name");
            Session.Remove("Email");

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

            // Optional: Show message on login page
            Session["LogoutMessage"] = message;

            // Prevent caching
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));

            SafeRedirect("~/Login.aspx");
        }

        /// <summary>
        /// Clears CurrentSessionID in the database when logging out
        /// </summary>
        private void ClearDatabaseSession()
        {
            try
            {
                if (Session["ClientID"] != null)
                {
                    using (var conn = new SqlConnection(Cs))
                    using (var cmd = new SqlCommand(
                        "UPDATE Clients SET CurrentSessionID = NULL, CurrentSessionAt = NULL WHERE ClientID = @ClientID", conn))
                    {
                        cmd.Parameters.AddWithValue("@ClientID", Convert.ToInt32(Session["ClientID"]));
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                // Fail silently - logout should still continue
            }
        }

        private void SafeRedirect(string url)
        {
            Response.Redirect(url, false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
