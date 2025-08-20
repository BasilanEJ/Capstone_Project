using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class BookingHistory : System.Web.UI.Page
    {
        private readonly string cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 🔐 Require login
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string role = Session["Role"].ToString();
            // 🔐 Deny SuperAdmin / Inspector (same as your other pages)
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            // (Optional) enforce view permission for ManageBooking
            int userId = Convert.ToInt32(Session["UserID"]);
            if (!HasViewPermission(userId, "ManageBooking"))
            {
                lblMessage.Text = "❌ You do not have permission to view booking history.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                gvBookingHistory.Visible = false;
                return;
            }

            if (!IsPostBack)
            {
                LoadBookingHistory();  // no filters initially
            }
        }

        private bool HasViewPermission(int adminId, string moduleName)
        {
            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spAdminPermission_Check", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = adminId;
                    cmd.Parameters.Add("@ModuleName", SqlDbType.NVarChar, 100).Value = moduleName;
                    cmd.Parameters.Add("@Permission", SqlDbType.NVarChar, 10).Value = "CanView";
                    con.Open();
                    object allowed = cmd.ExecuteScalar();
                    return allowed != null && allowed != DBNull.Value && Convert.ToBoolean(allowed);
                }
            }
            catch { return false; }
        }

        private void LoadBookingHistory(DateTime? startDate = null, DateTime? endDate = null, string status = "")
        {
            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spBookingHistory_List", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Pass NULLs when no filter
                    var pStart = cmd.Parameters.Add("@StartDate", SqlDbType.Date);
                    pStart.Value = startDate.HasValue ? (object)startDate.Value.Date : DBNull.Value;

                    var pEnd = cmd.Parameters.Add("@EndDate", SqlDbType.Date);
                    pEnd.Value = endDate.HasValue ? (object)endDate.Value.Date : DBNull.Value;

                    var pStatus = cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 50);
                    pStatus.Value = string.IsNullOrWhiteSpace(status) ? (object)DBNull.Value : status.Trim();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        gvBookingHistory.DataSource = dt;
                        gvBookingHistory.DataBind();

                        lblMessage.Text = dt.Rows.Count == 0 ? "No bookings found for selected filters." : "";
                        lblMessage.ForeColor = System.Drawing.Color.Green;
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"❌ Error loading booking history: {ex.Message}";
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            DateTime startDate, endDate;
            bool hasStart = DateTime.TryParse(txtStartDate.Text.Trim(), out startDate);
            bool hasEnd = DateTime.TryParse(txtEndDate.Text.Trim(), out endDate);
            string selectedStatus = ddlStatus.SelectedValue;

            LoadBookingHistory(hasStart ? startDate : (DateTime?)null,
                               hasEnd ? endDate : (DateTime?)null,
                               selectedStatus);
        }

        protected void gvBookingHistory_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvBookingHistory.PageIndex = e.NewPageIndex;

            // Reapply the current filters from the inputs
            btnFilter_Click(null, null);
        }
    }
}
