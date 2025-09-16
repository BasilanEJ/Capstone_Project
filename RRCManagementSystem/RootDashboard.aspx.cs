using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using RRCManagementSystem.Helpers; // Required for AESHelper

namespace RRCManagementSystem
{
    public partial class RootDashboard : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadSuperAdminCount();
                LoadActiveSuperAdmins();
            }
        }

        // Load total count of active SuperAdmins
        private void LoadSuperAdminCount()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_GetActiveSuperAdminCount", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                lblTotalSuperAdmins.Text = count.ToString();
            }
        }

        // Load active SuperAdmins into GridView
        private void LoadActiveSuperAdmins(string search = "")
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_GetSuperAdmins", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Search", search);
                cmd.Parameters.AddWithValue("@PageIndex", gvActiveSuperAdmins.PageIndex + 1);
                cmd.Parameters.AddWithValue("@PageSize", gvActiveSuperAdmins.PageSize);

                con.Open();
                DataTable dt = new DataTable();
                dt.Load(cmd.ExecuteReader());

                // 🔹 Decrypt each email before binding to the GridView
                foreach (DataRow row in dt.Rows)
                {
                    if (row["Email"] != DBNull.Value && !string.IsNullOrEmpty(row["Email"].ToString()))
                    {
                        try
                        {
                            string encryptedEmail = row["Email"].ToString();
                            string decryptedEmail = AESHelper.DecryptEmail(encryptedEmail);
                            row["Email"] = decryptedEmail;
                        }
                        catch
                        {
                            // If decryption fails, show placeholder
                            row["Email"] = "[Decryption Error]";
                        }
                    }
                }

                gvActiveSuperAdmins.DataSource = dt;
                gvActiveSuperAdmins.DataBind();
            }
        }

        // GridView paging event
        protected void gvActiveSuperAdmins_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvActiveSuperAdmins.PageIndex = e.NewPageIndex;
            LoadActiveSuperAdmins();
        }
    }
}
