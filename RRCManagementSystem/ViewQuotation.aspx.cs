using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class ViewQuotation : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                BindInspectors();
                LoadQuotations();
            }
        }

        private void BindInspectors()
        {
            ddlInspector.Items.Clear();
            ddlInspector.Items.Add(new ListItem("-- All Inspectors --", ""));

            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("usp_GetInspectors", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();

                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        ddlInspector.Items.Add(new ListItem(
                            Convert.ToString(rdr["Name"]),
                            Convert.ToString(rdr["UserID"])
                        ));
                    }
                }
            }
        }


        protected void btnSearch_Click(object sender, EventArgs e)
        {
            gvQuotations.PageIndex = 0;
            LoadQuotations();
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtDateFrom.Text = "";
            txtDateTo.Text = "";
            ddlInspector.SelectedIndex = 0;
            lblMessage.Text = "";
            gvQuotations.PageIndex = 0;
            LoadQuotations();
        }

        protected void gvQuotations_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvQuotations.PageIndex = e.NewPageIndex;
            LoadQuotations();
        }

        private void LoadQuotations()
        {
            DateTime? from = ParseDate(txtDateFrom.Text);
            DateTime? to = ParseDate(txtDateTo.Text);
            int? inspectorId = ParseInt(ddlInspector.SelectedValue);

            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("usp_GetPendingQuotations", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@FromDate", SqlDbType.DateTime).Value = (object)from ?? DBNull.Value;
                cmd.Parameters.Add("@ToDate", SqlDbType.DateTime).Value = (object)to ?? DBNull.Value;
                cmd.Parameters.Add("@InspectorID", SqlDbType.Int).Value = (object)inspectorId ?? DBNull.Value;

                var dt = new DataTable();
                using (var da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }

                gvQuotations.DataSource = dt;
                gvQuotations.DataBind();

                lblMessage.Text = dt.Rows.Count > 0
                    ? $"Showing {dt.Rows.Count} quotation(s)."
                    : "No quotations found for the selected filters.";
            }
        }


        private static DateTime? ParseDate(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            return DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var d) ? d : (DateTime?)null;
        }

        private static int? ParseInt(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : (int?)null;
        }
    }
}
