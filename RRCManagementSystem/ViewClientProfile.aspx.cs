using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace RRCManagementSystem
{
    public partial class ViewClientProfile : System.Web.UI.Page
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

            // 🔐 Block SuperAdmin and Inspector
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            // ✅ Load client info on first load
            if (!IsPostBack)
            {
                if (int.TryParse(Request.QueryString["ClientID"], out int clientId))
                {
                    LoadClientProfile(clientId);
                    LoadClientHistory(clientId);
                }
                else
                {
                    Response.Redirect("ClientsProfiles.aspx");
                }
            }
        }


        private void LoadClientProfile(int clientId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT Name, Email, ContactNumber, City, Region, Country, Status 
                                 FROM Clients WHERE ClientID = @ClientID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ClientID", clientId);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    lblName.Text = $"<span class='profile-label'>Name:</span> {reader["Name"]}";
                    lblEmail.Text = $"<span class='profile-label'>Email:</span> {reader["Email"]}";
                    lblContact.Text = $"<span class='profile-label'>Contact:</span> {reader["ContactNumber"]}";
                    lblCity.Text = $"<span class='profile-label'>City:</span> {reader["City"]}";
                    lblRegion.Text = $"<span class='profile-label'>Region:</span> {reader["Region"]}";
                    lblCountry.Text = $"<span class='profile-label'>Country:</span> {reader["Country"]}";
                    lblStatus.Text = $"<span class='profile-label'>Status:</span> {reader["Status"]}";
                }
                else
                {
                    Response.Redirect("ClientsProfiles.aspx");
                }

                reader.Close();
            }
        }

        private void LoadClientHistory(int clientId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT ActionTaken, PerformedBy, Remarks, ActionDate 
                                 FROM ClientHistory 
                                 WHERE ClientID = @ClientID 
                                 ORDER BY ActionDate DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ClientID", clientId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    gvHistory.DataSource = dt;
                    gvHistory.DataBind();
                    lblNoHistory.Visible = false;
                }
                else
                {
                    gvHistory.Visible = false;
                    lblNoHistory.Visible = true;
                }
            }
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("ClientsProfiles.aspx");
        }
    }
}
