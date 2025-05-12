using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class ViewSales : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                txtFrom.Text = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd");
                txtTo.Text = DateTime.Now.ToString("yyyy-MM-dd");
                LoadSales();
            }
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            LoadSales();
        }

        private void LoadSales()
        {
            DateTime fromDate;
            DateTime toDate;

            if (!DateTime.TryParse(txtFrom.Text, out fromDate) || !DateTime.TryParse(txtTo.Text, out toDate))
            {
                lblMessage.Text = "⚠ Please enter valid dates.";
                gvSales.DataSource = null;
                gvSales.DataBind();
                return;
            }

            toDate = toDate.AddDays(1); // Include entire 'to' day

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        s.SaleID,
                        c.Name AS ClientName,
                        t.Amount,
                        t.PaymentMethod,
                        t.Status,
                        t.TransactionDate
                    FROM Sales s
                    INNER JOIN Clients c ON s.ClientID = c.ClientID
                    INNER JOIN Transactions t ON s.SaleID = t.SaleID
                    WHERE t.TransactionDate BETWEEN @From AND @To
                    ORDER BY t.TransactionDate DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@From", fromDate);
                cmd.Parameters.AddWithValue("@To", toDate);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();

                try
                {
                    da.Fill(dt);
                    gvSales.DataSource = dt;
                    gvSales.DataBind();
                    lblMessage.Text = dt.Rows.Count == 0 ? "⚠ No sales records found." : "";
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "❌ Error loading sales: " + ex.Message;
                }
            }
        }
    }
}