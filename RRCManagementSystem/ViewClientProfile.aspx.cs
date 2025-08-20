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

            // 🔐 Block SuperAdmin and Inspector (kept same behavior)
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

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
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spClient_GetByID", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                conn.Open();

                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        var lastName = rdr["LastName"]?.ToString();
                        var firstName = rdr["FirstName"]?.ToString();
                        var middle = rdr["MiddleName"] as string;

                        string fullName = string.IsNullOrWhiteSpace(middle)
                            ? $"{lastName}, {firstName}"
                            : $"{lastName}, {firstName} {middle}";

                        lblName.Text = $"<span class='profile-label'>Name:</span> {fullName}";
                        lblEmail.Text = $"<span class='profile-label'>Email:</span> {rdr["Email"]}";
                        lblContact.Text = $"<span class='profile-label'>Contact:</span> {rdr["ContactNumber"]}";
                        lblCity.Text = $"<span class='profile-label'>City:</span> {rdr["City"]}";
                        lblRegion.Text = $"<span class='profile-label'>Region:</span> {rdr["Region"]}";
                        lblCountry.Text = $"<span class='profile-label'>Country:</span> {rdr["Country"]}";
                        lblStatus.Text = $"<span class='profile-label'>Status:</span> {rdr["Status"]}";
                    }
                    else
                    {
                        Response.Redirect("ClientsProfiles.aspx");
                    }
                }
            }
        }

        private void LoadClientHistory(int clientId)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spClientHistory_ListByClient", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;

                var dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    gvHistory.Visible = true;
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
