using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace RRCManagementSystem
{
    public partial class TransactionHistory : System.Web.UI.Page
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

            if (!IsPostBack)
            {
                txtFromDate.Text = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd");
                txtToDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                LoadTransactions();
            }
        }


        protected void btnFilter_Click(object sender, EventArgs e)
        {
            LoadTransactions();
        }

        private void LoadTransactions()
        {
            DateTime from, to;
            if (!DateTime.TryParse(txtFromDate.Text, out from) || !DateTime.TryParse(txtToDate.Text, out to))
            {
                lblMessage.Text = "⚠ Please enter valid dates.";
                gvTransactions.DataSource = null;
                gvTransactions.DataBind();
                return;
            }

            to = to.AddDays(1); // Include full end date

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        TransactionID,
                        SaleID,
                        Amount,
                        PaymentMethod,
                        Status,
                        TransactionDate
                    FROM Transactions
                    WHERE TransactionDate BETWEEN @From AND @To
                    ORDER BY TransactionDate DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@From", from);
                cmd.Parameters.AddWithValue("@To", to);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();

                try
                {
                    da.Fill(dt);
                    gvTransactions.DataSource = dt;
                    gvTransactions.DataBind();
                    lblMessage.Text = dt.Rows.Count == 0 ? "⚠ No transaction records found." : "";
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "❌ Error loading transactions: " + ex.Message;
                }
            }
        }
    }
}   