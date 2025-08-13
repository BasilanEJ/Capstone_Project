using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.Security;

namespace RRCManagementSystem
{
    public partial class Inspector : System.Web.UI.MasterPage
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            string currentPath = HttpContext.Current.Request.Url.AbsolutePath.ToLower();

            // ✅ Only validate session if not on login or 2FA page
            if (!currentPath.EndsWith("/login.aspx") &&
                !currentPath.EndsWith("/verifytotp.aspx") &&
                !currentPath.EndsWith("/enable2fa.aspx"))
            {
                if (Session["UserID"] == null || Session["Role"]?.ToString() != "Inspector")
                {
                    Response.Redirect("~/Login.aspx");
                    return;
                }

                if (!IsPostBack)
                {
                    LoadInspectorName();
                }
            }
        }

        private void LoadInspectorName()
        {
            try
            {
                int inspectorId = Convert.ToInt32(Session["UserID"]);

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT Name FROM Users WHERE UserID = @UserID AND Status = 'Active'";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@UserID", inspectorId);

                    conn.Open();
                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        lblInspectorName.Text = "👷 " + result.ToString();
                    }
                }
            }
            catch
            {
                lblInspectorName.Text = "👷 Inspector";
            }
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            // 1) Remove authentication state
            Session.Remove("IsAuthenticated");
            Session.Remove("UserID");
            Session.Remove("Role");
            Session.Remove("Name");
            Session.Remove("Email");

            // 2) Clear and abandon session
            Session.Clear();
            Session.RemoveAll();
            Session.Abandon();

            // 3) Expire session cookie
            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.UtcNow.AddDays(-1);
            }

            // 4) Sign out Forms Authentication cookie
            System.Web.Security.FormsAuthentication.SignOut();

            // 5) Disable caching so Back/Forward buttons can't load old pages
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));

            // 6) Redirect to login
            Response.Redirect("~/Login.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

    }
}
