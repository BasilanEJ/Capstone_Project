using System;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class Maintenance : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Optional: Log page hits for analytics or debugging
            System.Diagnostics.Debug.WriteLine($"Maintenance page accessed at {DateTime.Now} from IP {Request.UserHostAddress}");
        }

        protected void btnHome_Click(object sender, EventArgs e)
        {
            // Clear session to avoid BeginRequest redirecting again
            Session.Abandon();
            Session.Clear();

            // Redirect user to login
            Response.Redirect("~/Login.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

    }
}
