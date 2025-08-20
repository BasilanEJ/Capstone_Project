using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.Security;

namespace RRCManagementSystem
{
    public partial class Inspector : System.Web.UI.MasterPage
    {
        private static readonly string Cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // ---- 1) Auth/role gate on EVERY request ----
            var role = Session["Role"] as string;
            if (Session["UserID"] == null || !string.Equals(role, "Inspector", StringComparison.OrdinalIgnoreCase))
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
                LoadInspectorName();
            }
        }

        private void LoadInspectorName()
        {
            if (!int.TryParse(Session["UserID"]?.ToString(), out var inspectorId))
            {
                SafeRedirect("~/Login.aspx");
                return;
            }

            try
            {
                using (var conn = new SqlConnection(Cs))
                using (var cmd = new SqlCommand(
                    "SELECT Name FROM Users WHERE UserID = @UserID AND Status = 'Active'", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", inspectorId);
                    conn.Open();
                    var result = cmd.ExecuteScalar() as string;
                    lblInspectorName.Text = "👷 " + (string.IsNullOrWhiteSpace(result) ? "Inspector" : result.Trim());
                }
            }
            catch
            {
                lblInspectorName.Text = "👷 Inspector";
            }
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            // ---- Clear app/session state ----
            Session.Remove("IsAuthenticated");
            Session.Remove("UserID");
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

            // ---- Expire FormsAuth cookie ----
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
