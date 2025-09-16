using RRCManagementSystem.Helpers;
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

                        // ✅ Decrypt sensitive fields
                        string email = rdr["EmailEnc"] != DBNull.Value
                            ? AESHelper.DecryptEmail(rdr["EmailEnc"].ToString())
                            : "[No Email]";

                        string contact = rdr["ContactEnc"] != DBNull.Value
                            ? AESHelper.DecryptField(rdr["ContactEnc"].ToString())
                            : "[No Contact]";

                        string city = rdr["CityEnc"] != DBNull.Value
                            ? AESHelper.DecryptField(rdr["CityEnc"].ToString())
                            : "[No City]";

                        string region = rdr["RegionEnc"] != DBNull.Value
                            ? AESHelper.DecryptField(rdr["RegionEnc"].ToString())
                            : "[No Region]";

                        string country = rdr["CountryEnc"] != DBNull.Value
                            ? AESHelper.DecryptField(rdr["CountryEnc"].ToString())
                            : "[No Country]";

                        // ✅ Populate labels
                        lblName.Text = $"<span class='profile-label'>Name:</span> {fullName}";
                        lblEmail.Text = $"<span class='profile-label'>Email:</span> {email}";
                        lblContact.Text = $"<span class='profile-label'>Contact:</span> {contact}";
                        lblCity.Text = $"<span class='profile-label'>City:</span> {city}";
                        lblRegion.Text = $"<span class='profile-label'>Region:</span> {region}";
                        lblCountry.Text = $"<span class='profile-label'>Country:</span> {country}";
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
