using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class ViewPaymentBalance : System.Web.UI.Page
    {
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindGrid(txtClientName.Text?.Trim());
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            // Reset to first page on new search
            if (gvBalances.AllowPaging) gvBalances.PageIndex = 0;
            BindGrid(txtClientName.Text?.Trim());
        }

        protected void gvBalances_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvBalances.PageIndex = e.NewPageIndex;
            BindGrid(txtClientName.Text?.Trim());
        }

        private void BindGrid(string nameFilter)
        {
            try
            {
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.usp_ViewPaymentBalances", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 200)
                        .Value = (object)(nameFilter ?? string.Empty) ?? string.Empty;

                    var dt = new DataTable();
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }

                    gvBalances.DataSource = dt;
                    gvBalances.DataBind();

                    lblMessage.Text = dt.Rows.Count == 0
                        ? "No balances found for the given filter."
                        : string.Empty;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error loading balances: " + ex.Message;
            }
        }
    }
}
