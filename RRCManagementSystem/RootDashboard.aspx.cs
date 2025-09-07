using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

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

        private void LoadActiveSuperAdmins()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_GetSuperAdmins", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();
                DataTable dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                gvActiveSuperAdmins.DataSource = dt;
                gvActiveSuperAdmins.DataBind();
            }
        }

        // GridView paging
        protected void gvActiveSuperAdmins_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvActiveSuperAdmins.PageIndex = e.NewPageIndex;
            LoadActiveSuperAdmins();
        }

    }
}