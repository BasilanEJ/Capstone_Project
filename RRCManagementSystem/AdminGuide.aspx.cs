using System;
using System.Configuration;

namespace RRCManagementSystem
{
    public partial class AdminGuide : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 🔐 Require login
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string role = Session["Role"].ToString();

            // ✅ Allow SuperAdmin and Admin roles; block Inspector
            if (role == "Inspector")
            {
                Response.Redirect("~/Unauthorized.aspx");
                return;
            }

            if (!IsPostBack)
            {
                // Show "Last updated" in PHT (Asia/Manila)
                try
                {
                    var phTz = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time"); // Windows TZ for Asia/Manila
                    var nowPh = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, phTz);
                    lblUpdated.Text = nowPh.ToString("MMMM dd, yyyy h:mm tt");
                }
                catch
                {
                    // Fallback if timezone is not available
                    lblUpdated.Text = DateTime.UtcNow.AddHours(8).ToString("MMMM dd, yyyy h:mm tt");
                }
            }
        }
    }
}
