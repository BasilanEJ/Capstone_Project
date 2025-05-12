
using System;

namespace RRCManagementSystem
{
    public partial class Logout : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // ✅ Clear all Session data
            Session.Clear();
            Session.Abandon();

            // ✅ Optional: Clear authentication cookies if you have any (good practice)
            Response.Cookies.Clear();

            // ✅ Redirect to Login page
            Response.Redirect("~/Login.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
