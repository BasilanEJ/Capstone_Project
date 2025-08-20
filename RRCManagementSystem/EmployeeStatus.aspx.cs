using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class EmployeeStatus : System.Web.UI.Page
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

            // 🔐 Deny SuperAdmin and Inspector
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                // status filter
                ddlStatus.Items.Clear();
                ddlStatus.Items.Add(new ListItem("-- Select Status --", ""));
                ddlStatus.Items.Add(new ListItem("Active", "Active"));
                ddlStatus.Items.Add(new ListItem("Inactive", "Inactive"));

                LoadEmployees(); // all
            }
        }

        // Load Employees (via SP)
        private void LoadEmployees(string status = "")
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spEmployees_ListByStatus", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                // pass NULL to return all
                if (string.IsNullOrWhiteSpace(status))
                    cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = DBNull.Value;
                else
                    cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = status;

                var dt = new DataTable();
                da.Fill(dt);

                gvEmployees.DataSource = dt;
                gvEmployees.DataBind();
            }
        }

        // Filter Employees when Status changes
        protected void ddlStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadEmployees(ddlStatus.SelectedValue);
        }

        // Pagination (keep current filter)
        protected void gvEmployees_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvEmployees.PageIndex = e.NewPageIndex;
            LoadEmployees(ddlStatus.SelectedValue);
        }
    }
}
