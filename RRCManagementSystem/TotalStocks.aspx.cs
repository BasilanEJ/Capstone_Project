using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class TotalStocks : Page
    {
        private readonly string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

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

            if (!IsPostBack)
            {
                CreateYesterdaySnapshot(); // ✅ Snapshot for yesterday only if not yet created

                txtFromDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
                txtToDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
                LoadSnapshot(DateTime.Today, DateTime.Today);
            }
        }

        private void CreateYesterdaySnapshot()
        {
            DateTime snapshotDate = DateTime.Today.AddDays(-1);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string checkQuery = "SELECT COUNT(*) FROM InventorySnapshots WHERE SnapshotDate = @SnapshotDate";
                SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@SnapshotDate", snapshotDate);

                int count = (int)checkCmd.ExecuteScalar();

                if (count == 0)
                {
                    string insertQuery = @"
                        INSERT INTO InventorySnapshots (ItemID, Name, Type, Quantity, ExcessML, SnapshotDate)
                        SELECT ItemID, Name, Type, Quantity, ExcessML, @SnapshotDate
                        FROM Inventory";

                    SqlCommand insertCmd = new SqlCommand(insertQuery, conn);
                    insertCmd.Parameters.AddWithValue("@SnapshotDate", snapshotDate);
                    insertCmd.ExecuteNonQuery();
                }
            }
        }

        private void LoadSnapshot(DateTime fromDate, DateTime toDate)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT ItemID, Name, Type, Quantity, ExcessML, SnapshotDate 
                                 FROM InventorySnapshots 
                                 WHERE SnapshotDate BETWEEN @FromDate AND @ToDate
                                 ORDER BY SnapshotDate DESC, Name ASC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvTotalStocks.DataSource = dt;
                    gvTotalStocks.DataBind();

                    lblMessage.Text = dt.Rows.Count == 0 ? "⚠ No snapshot data available for the selected date range." : "";
                }
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
