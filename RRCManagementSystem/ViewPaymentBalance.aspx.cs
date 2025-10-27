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

        // Handle row data binding to apply red highlighting based on payment plan
        protected void gvBalances_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Get the data from the row
                DataRowView drv = e.Row.DataItem as DataRowView;
                if (drv != null)
                {
                    // Get IsOverdue flag from the stored procedure
                    int isOverdue = Convert.ToInt32(drv["IsOverdue"]);
                    int monthsOverdue = Convert.ToInt32(drv["MonthsOverdue"]);
                    string paymentPlan = drv["PaymentPlan"]?.ToString() ?? "";

                    // If flagged as overdue, apply red styling
                    if (isOverdue == 1)
                    {
                        e.Row.CssClass += " overdue-row";

                        // Add a badge to the months column with context
                        int monthsColumnIndex = 7; // The MonthsOverdue column (0-indexed, accounting for PaymentPlan column)
                        if (e.Row.Cells.Count > monthsColumnIndex)
                        {
                            string badgeText = "OVERDUE";

                            // Add context based on payment plan
                            if (paymentPlan.Contains("100"))
                                badgeText = "OVERDUE (2+ mo)";
                            else if (paymentPlan.Contains("70") && paymentPlan.Contains("30"))
                                badgeText = "OVERDUE (3+ mo)";
                            else if (paymentPlan.Contains("50") && paymentPlan.Contains("25"))
                                badgeText = "OVERDUE (4+ mo)";

                            e.Row.Cells[monthsColumnIndex].Text +=
                                $" <span class='overdue-badge'>{badgeText}</span>";
                        }
                    }
                }
            }
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