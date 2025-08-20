using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class TotalStocks : Page
    {
        private readonly string connectionString =
            System.Configuration.ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 🔐 Require login
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string role = Session["Role"].ToString();

            // 🔐 Block SuperAdmin and Inspector (follow your pattern)
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                // Create yesterday’s snapshot once if it doesn’t exist yet
                EnsureYesterdaySnapshot();

                txtFromDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
                txtToDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
                LoadSnapshot(DateTime.Today, DateTime.Today);
            }
        }

        private void EnsureYesterdaySnapshot()
        {
            DateTime snapshotDate = DateTime.Today.AddDays(-1);

            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spInventorySnapshot_EnsureForDate", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SnapshotDate", snapshotDate);
                    conn.Open();
                    // Optional: read the row count the proc returns
                    using (var rdr = cmd.ExecuteReader())
                    {
                        // int inserted = rdr.Read() ? Convert.ToInt32(rdr["Inserted"]) : 0;
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠ Failed to ensure yesterday snapshot: " + ex.Message;
            }
        }

        private void LoadSnapshot(DateTime fromDate, DateTime toDate)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spInventorySnapshot_ListBetween", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                    cmd.Parameters.AddWithValue("@ToDate", toDate.Date);

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        gvTotalStocks.DataSource = dt;
                        gvTotalStocks.DataBind();

                        lblMessage.Text = dt.Rows.Count == 0
                            ? "⚠ No snapshot data available for the selected date range."
                            : "";
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠ Error loading snapshot: " + ex.Message;
            }
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            if (DateTime.TryParse(txtFromDate.Text, out DateTime fromDate) &&
                DateTime.TryParse(txtToDate.Text, out DateTime toDate))
            {
                LoadSnapshot(fromDate, toDate);
            }
            else
            {
                lblMessage.Text = "⚠ Please select valid From and To dates.";
            }
        }

        protected void gvTotalStocks_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvTotalStocks.PageIndex = e.NewPageIndex;

            if (DateTime.TryParse(txtFromDate.Text, out DateTime fromDate) &&
                DateTime.TryParse(txtToDate.Text, out DateTime toDate))
            {
                LoadSnapshot(fromDate, toDate);
            }
            else
            {
                LoadSnapshot(DateTime.Today, DateTime.Today);
            }
        }
    }
}
