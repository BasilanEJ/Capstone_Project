using System;
using System.Web;

namespace RRCManagementSystem
{
    public partial class SuperAdmin : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // ✅ Validate session and role
            if (!IsPostBack)
            {
                if (Session["UserID"] == null || Session["Role"]?.ToString() != "SuperAdmin")
                {
                    // 🚫 Not logged in or not SuperAdmin → redirect
                    Response.Redirect("~/Login.aspx");
                    return;
                }
            }
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            // ✅ Clear session and logout
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Login.aspx");
        }
    }
}
