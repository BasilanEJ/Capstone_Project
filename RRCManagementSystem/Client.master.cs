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

            // ---- 2) Strong no-cache for protected views ----
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            Response.Cache.AppendCacheExtension("must-revalidate, proxy-revalidate");

            // ---- 3) First-load UI init ----
            if (!IsPostBack)
            {
                LoadClientProfile();
            }
        }

        private void LoadClientProfile()
        {
            // Try to use cached name first
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
                // Optional: log the exception
                lblClientName.Text = "My Profile";
            }
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            // ---- Clear app/session state ----
            Session.Remove("IsAuthenticated");
            Session.Remove("ClientID");
            Session.Remove("ClientName");
            Session.Remove("Role");
            Session.Remove("Name");
            Session.Remove("Email");

            Session.Clear();
            Session.RemoveAll();
            Session.Abandon();

            // ---- Expire session cookie ----
            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.UtcNow.AddDays(-1);
            }

            // ---- Expire FormsAuth cookie (Forms auth still issues this even for clients) ----
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
