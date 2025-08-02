using System;
using System.Data.SqlClient;
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
            if (Session["Name"] == null)
            {
                int clientId = Convert.ToInt32(Session["ClientID"]);
                string clientName = GetClientNameFromDatabase(clientId);

                Session["Name"] = string.IsNullOrEmpty(clientName) ? "Valued Client" : clientName;
            }
        }


        private string GetClientNameFromDatabase(int clientId)
        {
            string clientName = string.Empty;

            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

            using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                string query = "SELECT FirstName, MiddleName, LastName FROM Clients WHERE ClientID = @ClientID";

                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@ClientID", clientId);

                    try
                    {
                        con.Open();
                        using (System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string firstName = reader["FirstName"].ToString();
                                string middleName = reader["MiddleName"].ToString();
                                string lastName = reader["LastName"].ToString();

                                // Optional: Format as "LastName, FirstName MiddleName"
                                if (!string.IsNullOrWhiteSpace(middleName))
                                    clientName = $"{lastName}, {firstName} {middleName}";
                                else
                                    clientName = $"{lastName}, {firstName}";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error fetching client name: " + ex.Message);
                    }
                }
            }

            return clientName;
        }

    }
}