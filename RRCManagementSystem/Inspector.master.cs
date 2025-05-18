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
            Session.Clear();
            Session.Abandon();
            FormsAuthentication.SignOut();
            Response.Redirect("~/Login.aspx");
        }
    }
}
