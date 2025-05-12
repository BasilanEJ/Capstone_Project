using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace RRCManagementSystem
{
    public partial class Client : System.Web.UI.MasterPage
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Redirect if session expired
            /*  if (Session["ClientID"] == null)
              {
                  Response.Redirect("~/Login.aspx");
              } */

            if (!IsPostBack)
            {
                LoadClientProfile();
            }
        }

        private void LoadClientProfile()
        {
            try
            {
                int clientId = Convert.ToInt32(Session["ClientID"]);

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT Name FROM Clients WHERE ClientID = @ClientID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@ClientID", clientId);

                        con.Open();

                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            lblClientName.Text = result.ToString();
                        }
                        else
                        {
                            lblClientName.Text = "My Profile";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log exception if necessary
                lblClientName.Text = "My Profile";
                // Optionally show an alert/log if debugging
            }
        }
    }
}
