using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class InspectedInquiry : Page
    {
        private readonly string _cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // ✅ Admin-only
            if (Session["UserID"] == null || Session["Role"] == null ||
                !string.Equals(Session["Role"].ToString(), "Admin", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                ViewState["SortExpression"] = "ScheduledDate";
                ViewState["SortDirection"] = "DESC";
                gvCompleted.Columns[0].Visible = false;
                BindCompleted();
            }
        }

        #region Data Binding

        private void BindCompleted()
        {
            var dt = GetCompletedData();

            // Apply client-side sort based on ViewState
            string sortExp = (ViewState["SortExpression"] as string) ?? "ScheduledDate";
            string sortDir = (ViewState["SortDirection"] as string) ?? "DESC";

            if (dt.Rows.Count > 0)
            {
                var dv = dt.DefaultView;
                dv.Sort = $"{sortExp} {sortDir}";
                dt = dv.ToTable();
            }

            lblCount.Text = dt.Rows.Count == 0 ? "" : $"{dt.Rows.Count} item(s)";
            pnlEmpty.Visible = dt.Rows.Count == 0;

            gvCompleted.DataSource = dt;
            gvCompleted.DataBind();

            // Keep a copy (optional)
            ViewState["CurrentData"] = dt;
        }

        private DataTable GetCompletedData()
        {
            using (var conn = new SqlConnection(_cs))
            using (var da = new SqlDataAdapter("dbo.usp_Admin_ListCompletedInspections", conn))
            {
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        #endregion

        #region Grid Events

        protected void gvCompleted_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvCompleted.PageIndex = e.NewPageIndex;
            BindCompleted();
        }

        protected void gvCompleted_Sorting(object sender, GridViewSortEventArgs e)
        {
            string currentExp = (ViewState["SortExpression"] as string) ?? "ScheduledDate";
            string currentDir = (ViewState["SortDirection"] as string) ?? "DESC";

            if (string.Equals(currentExp, e.SortExpression, StringComparison.OrdinalIgnoreCase))
            {
                // toggle direction
                ViewState["SortDirection"] = currentDir.Equals("ASC", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";
            }
            else
            {
                ViewState["SortExpression"] = e.SortExpression;
                ViewState["SortDirection"] = "ASC";
            }

            BindCompleted();
        }

        protected void gvCompleted_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!string.Equals(e.CommandName, "create", StringComparison.OrdinalIgnoreCase))
                return;

            if (!int.TryParse(e.CommandArgument?.ToString(), out int inspectionId))
                return;

            var dt = LoadCompletedRows(inspectionId);
            if (dt.Rows.Count == 0) return;

            DataRow r = dt.Rows[0];

            // existing prefill…
            Session["Prefill_LastName"] = Safe(r["LastName"]);
            Session["Prefill_FirstName"] = Safe(r["FirstName"]);
            Session["Prefill_MiddleName"] = Safe(r["MiddleName"]);
            Session["Prefill_Email"] = Safe(r["Email"]);
            Session["Prefill_Contact"] = Safe(r["ContactNumber"]);
            Session["Prefill_Street"] = Safe(r["StreetAndUnit"]);
            Session["Prefill_Barangay"] = Safe(r["Barangay"]);
            Session["Prefill_City"] = Safe(r["City"]);
            Session["Prefill_Region"] = Safe(r["Region"]);
            Session["Prefill_Country"] = string.IsNullOrWhiteSpace(Safe(r["Country"])) ? "Philippines" : Safe(r["Country"]);
            Session["Prefill_Landmark"] = Safe(r["Landmark"]);

            // 🔴 ADD THESE 3 LINES
            Session["Prefill_InspectionID"] = Safe(r["InspectionID"]);
            Session["Prefill_InquiryCode"] = r.Table.Columns.Contains("InquiryCode") ? Safe(r["InquiryCode"]) : "";
            Session["Prefill_Findings"] = r.Table.Columns.Contains("Findings") ? Safe(r["Findings"]) : "";

            Response.Redirect("~/CreateCustomerAccount.aspx?prefill=1", false);
            Context.ApplicationInstance.CompleteRequest();
        }




        #endregion

        #region Helpers

        private DataTable LoadCompletedRows(int inspectionId)
        {
            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("dbo.usp_GetCompletedInspectionDetails", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@InspectionID", SqlDbType.Int).Value = inspectionId;

                var dt = new DataTable();
                da.Fill(dt);

                return dt; // Return all matching rows now
            }
        }


        private static string Safe(object v) =>
            (v == null || v == DBNull.Value) ? "" : v.ToString();

        #endregion
    }
}
