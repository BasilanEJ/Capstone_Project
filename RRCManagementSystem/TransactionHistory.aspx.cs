using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

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
                        ddlPaymentMethod.Items.Add(new ListItem("All Payment Methods", "")); // value = empty

                        while (r.Read())
                        {
                            var pm = r["PaymentMethod"]?.ToString();
                            if (!string.IsNullOrWhiteSpace(pm))
                                ddlPaymentMethod.Items.Add(new ListItem(pm, pm));
                        }
                    }
                }
            }
            catch
            {
                ddlPaymentMethod.Items.Clear();
                ddlPaymentMethod.Items.Add(new ListItem("All Payment Methods", ""));
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

                    // inclusive to-date (SP uses < DATEADD(DAY,1,@ToDate))
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

        protected string GetReceiptLink(object receiptObj)
        {
            var v = (receiptObj == null || receiptObj == DBNull.Value) ? "" : receiptObj.ToString();
            if (string.IsNullOrWhiteSpace(v)) return ""; // handled by lnkAdd

            // safety: use only the file name
            var file = System.IO.Path.GetFileName(v);
            var url = "DecryptReceipt.aspx?file=" + Server.UrlEncode(file);
            return $"<a class='pill pill-view' href='{url}' target='_blank' rel='noopener'>View Receipt</a>";
        }

    }
}
