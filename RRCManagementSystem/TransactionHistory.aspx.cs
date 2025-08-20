using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace RRCManagementSystem
{
    public partial class TransactionHistory : System.Web.UI.Page
    {
        private readonly string cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string role = Session["Role"].ToString();
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                txtFromDate.Text = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd");
                txtToDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                LoadPaymentMethods();
                LoadTransactions();
            }
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            LoadTransactions();
        }

        private void LoadPaymentMethods()
        {
            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spTransactionHistory_PaymentMethods", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        ddlPaymentMethod.Items.Clear();
                        ddlPaymentMethod.Items.Add(new System.Web.UI.WebControls.ListItem("All Payment Methods", ""));
                        while (r.Read())
                        {
                            string pm = r["PaymentMethod"]?.ToString();
                            if (!string.IsNullOrWhiteSpace(pm))
                                ddlPaymentMethod.Items.Add(pm);
                        }
                    }
                }
            }
            catch
            {
                // If loading fails, still show an "All" option
                ddlPaymentMethod.Items.Clear();
                ddlPaymentMethod.Items.Add("All Payment Methods");
            }
        }

        private void LoadTransactions()
        {
            if (!DateTime.TryParse(txtFromDate.Text, out DateTime from) ||
                !DateTime.TryParse(txtToDate.Text, out DateTime to))
            {
                lblMessage.Text = "⚠ Please enter valid dates.";
                gvTransactions.DataSource = null;
                gvTransactions.DataBind();
                return;
            }

            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spTransactionHistory_List", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@FromDate", SqlDbType.Date).Value = from.Date;
                    cmd.Parameters.Add("@ToDate", SqlDbType.Date).Value = to.Date;

                    var pMethod = cmd.Parameters.Add("@PaymentMethod", SqlDbType.NVarChar, 50);
                    pMethod.Value = string.IsNullOrWhiteSpace(ddlPaymentMethod.SelectedValue)
                        ? (object)DBNull.Value
                        : ddlPaymentMethod.SelectedValue;

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        gvTransactions.DataSource = dt;
                        gvTransactions.DataBind();
                        lblMessage.Text = dt.Rows.Count == 0 ? "⚠ No transaction records found." : string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "❌ Error loading transactions: " + ex.Message;
            }
        }
    }
}
