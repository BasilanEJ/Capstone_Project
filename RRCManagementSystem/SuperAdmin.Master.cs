using System;
using System.Web;

namespace RRCManagementSystem
{
    public partial class SuperAdmin : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Session validation to make sure SuperAdmin is logged in
            /* if (!IsPostBack)
             {
                 // Assuming you store SuperAdmin session like this:
                 if (Session["SuperAdminID"] == null)
                 {
                     // Redirect to login page if not logged in
                     Response.Redirect("~/Login.aspx");
                 }
             } */

        }


        protected void btnLogout_Click(object sender, EventArgs e)
        {
            // Clear the session variables
            Session.Clear();
            Session.Abandon();

            // Redirect to login page
            Response.Redirect("~/Login.aspx");
        }
    }
}

