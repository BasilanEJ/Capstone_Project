using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
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

            int userId = Convert.ToInt32(Session["UserID"]);

            // Check CanAdd permission for Sales&Transaction module
            bool canAdd = HasAddPermission(userId, "Sales&Transaction");
            ViewState["CanAddReceipt"] = canAdd;

            if (!IsPostBack)
            {
                txtFromDate.Text = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd");
                txtToDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                LoadPaymentMethods();
                LoadTransactions();

                if (!canAdd)
                {
                    lblMessage.Text = "⚠️ You do not have permission to add receipts.";
                    lblMessage.CssClass = "form-text text-center mb-3 text-warning";
                }
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
                        ddlPaymentMethod.Items.Add(new ListItem("All Payment Methods", ""));
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
            if (string.IsNullOrWhiteSpace(v)) return "";

            var file = System.IO.Path.GetFileName(v);
            var url = "DecryptReceipt.aspx?file=" + Server.UrlEncode(file);

            // Instead of opening in new tab, call JavaScript function to show modal
            return $"<a class='pill pill-view' href='#' onclick=\"viewReceipt('{url}', '{file}'); return false;\">View Receipt</a>";
        }

        #region Permissions
        private bool HasAddPermission(int userId, string moduleName)
        {
            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spAdminPermission_CanAdd", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                    cmd.Parameters.Add("@ModuleName", SqlDbType.NVarChar, 100).Value = moduleName;

                    con.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null && Convert.ToInt32(result) == 1;
                }
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region GridView Events
        protected void gvTransactions_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                bool canAdd = ViewState["CanAddReceipt"] != null && (bool)ViewState["CanAddReceipt"];
                var btnAddReceipt = e.Row.FindControl("btnAddReceipt") as LinkButton;
                if (btnAddReceipt != null)
                {
                    btnAddReceipt.Enabled = canAdd;
                    if (!canAdd)
                        btnAddReceipt.CssClass += " disabled";
                }
            }
        }

        protected void btnAddReceipt_Click(object sender, EventArgs e)
        {
            LinkButton btn = sender as LinkButton;
            if (btn == null) return;

            bool canAdd = ViewState["CanAddReceipt"] != null && (bool)ViewState["CanAddReceipt"];
            if (!canAdd)
            {
                // Security: prevent unauthorized access
                lblMessage.Text = "⚠️ You do not have permission to add receipts.";
                return;
            }

            int txId = Convert.ToInt32(btn.CommandArgument);
            Response.Redirect("AddReceipt.aspx?tx=" + txId);
        }
        #endregion
    }
}