using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.Security;

namespace RRCManagementSystem
{
    public partial class Inspector : System.Web.UI.MasterPage
    {
        // Connection string
        private static readonly string Cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // --- Authorization / Role Check ---
            string role = Session["Role"] as string;
            if (Session["UserID"] == null || !string.Equals(role, "Inspector", StringComparison.OrdinalIgnoreCase))
            {
                SafeRedirect("~/Login.aspx");
                return;
            }

            // --- Disable caching ---
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetRevalidation(System.Web.HttpCacheRevalidation.AllCaches);
            Response.Cache.AppendCacheExtension("must-revalidate, proxy-revalidate");

            if (!IsPostBack)
            {
                LoadInspectorName();
                LoadNotificationCount();
            }
        }

        /// <summary>
        /// Loads the inspector's name into the header label
        /// </summary>
        private void LoadInspectorName()
        {
            if (!int.TryParse(Session["UserID"]?.ToString(), out int inspectorId))
            {
                SafeRedirect("~/Login.aspx");
                return;
            }

            try
            {
                using (var conn = new SqlConnection(Cs))
                using (var cmd = new SqlCommand("SELECT Name FROM Users WHERE UserID=@UserID AND Status='Active'", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", inspectorId);
                    conn.Open();
                    string name = cmd.ExecuteScalar() as string;
                    lblInspectorName.Text = "👷 " + (!string.IsNullOrWhiteSpace(name) ? name.Trim() : "Inspector");
                }
            }
            catch
            {
                lblInspectorName.Text = "👷 Inspector";
            }
        }

        /// <summary>
        /// Loads unread notification count for the inspector
        /// </summary>
        private void LoadNotificationCount()
        {
            if (!int.TryParse(Session["UserID"]?.ToString(), out int userId))
                return;

            try
            {
                using (var conn = new SqlConnection(Cs))
                using (var cmd = new SqlCommand(
                    @"SELECT COUNT(*) FROM Notifications 
                      WHERE UserID=@UserID AND (IsRead=0 OR IsRead IS NULL)", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    conn.Open();
                    int unread = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);

                    // Update badge
                    notificationCount.InnerText = unread.ToString();
                    notificationCount.Style["display"] = unread > 0 ? "inline-block" : "none";
                }
            }
            catch
            {
                notificationCount.InnerText = "0";
                notificationCount.Style["display"] = "none";
            }
        }

        /// <summary>
        /// Allows refreshing notifications manually (used after assigning)
        /// </summary>
        public void RefreshNotifications()
        {
            LoadNotificationCount();
        }

        /// <summary>
        /// Logout button click handler
        /// </summary>
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            // Clear session
            Session.Clear();
            Session.Abandon();

            // Expire session cookie
            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.UtcNow.AddDays(-1);
            }

            // Sign out of FormsAuthentication
            FormsAuthentication.SignOut();
            if (Request.Cookies[FormsAuthentication.FormsCookieName] != null)
            {
                Response.Cookies[FormsAuthentication.FormsCookieName].Value = string.Empty;
                Response.Cookies[FormsAuthentication.FormsCookieName].Expires = DateTime.UtcNow.AddDays(-1);
            }

            SafeRedirect("~/Login.aspx");
        }

        /// <summary>
        /// Redirects safely without ThreadAbortException
        /// </summary>
        private void SafeRedirect(string url)
        {
            Response.Redirect(url, false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
