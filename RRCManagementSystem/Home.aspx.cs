using System;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class Home : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Check if user session exists
            if (Session["ClientID"] == null)
            {
                // Redirect to login page if not logged in
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadClientInfo();
            }
        }

        private void LoadClientInfo()
        {
            // Example: You can fetch more client information from database here if needed

            // If Session["ClientName"] isn't set already, fetch and assign it
            if (Session["Name"] == null)
            {
                int clientId = Convert.ToInt32(Session["ClientID"]);

                // Sample query to get the ClientName from DB (optional)
                string clientName = GetClientNameFromDatabase(clientId);

                if (!string.IsNullOrEmpty(clientName))
                {
                    Session["Name"] = clientName;
                }
                else
                {
                    Session["Name"] = "Valued Client";
                }
            }
        }

        private string GetClientNameFromDatabase(int clientId)
        {
            string clientName = string.Empty;

            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

            using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                string query = "SELECT Name FROM Clients WHERE ClientID = @ClientID";

                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@ClientID", clientId);

                    try
                    {
                        con.Open();
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            clientName = result.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log or handle error as needed
                        throw new Exception("Error fetching client name: " + ex.Message);
                    }
                }
            }

            return clientName;
        }
    }
}