using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class AllRescheduleBooking : System.Web.UI.Page
    {
        private readonly string cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;
        private const string CacheKey = "AllRescheduleBooking_Data";

        protected void Page_Load(object sender, EventArgs e)
        {
            // 🔐 Require login
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string role = Session["Role"].ToString();

            // 🔐 Deny access for SuperAdmin and Inspector (kept from your original logic)
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            // 🔐 CanView permission for ManageBooking
            if (!HasViewPermission(userId, "ManageBooking"))
            {
                lblMessage.Text = "❌ You do not have permission to view reschedules.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                gvReschedules.Visible = false;
                return;
            }

            if (!IsPostBack)
            {
                LoadRescheduleBookings();     // loads and caches DataTable
                BindFiltered();               // initial bind (no filters)
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
            catch
            {
                return false;
            }
        }

        private void LoadRescheduleBookings()
        {
            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spServiceSchedule_ListAll", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        // Ensure types are friendly for filtering
                        // If ScheduledDate comes as string, try to parse to DateTime
                        if (dt.Columns.Contains("ScheduledDate") && dt.Columns["ScheduledDate"].DataType != typeof(DateTime))
                        {
                            dt.Columns["ScheduledDate"].ColumnName = "ScheduledDateRaw";
                            dt.Columns.Add("ScheduledDate", typeof(DateTime));
                            foreach (DataRow r in dt.Rows)
                            {
                                if (DateTime.TryParse(Convert.ToString(r["ScheduledDateRaw"]), out var d))
                                    r["ScheduledDate"] = d.Date;
                            }
                            dt.Columns.Remove("ScheduledDateRaw");
                        }

                        // Cache it for filtering + paging
                        Session[CacheKey] = dt;
                        gvReschedules.DataSource = dt;
                        gvReschedules.DataBind();
                    }
                }
                lblMessage.Text = string.Empty;
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠️ Error loading schedules: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        // 🔎 Apply filters and bind
        private void BindFiltered()
        {
            var dt = Session[CacheKey] as DataTable;
            if (dt == null)
            {
                LoadRescheduleBookings();
                dt = Session[CacheKey] as DataTable;
                if (dt == null) return;
            }

            string search = (txtSearch.Text ?? string.Empty).Trim();
            string status = ddlFilterStatus.SelectedValue?.Trim() ?? string.Empty;

            DateTime from, to;
            bool hasFrom = DateTime.TryParseExact(txtFrom.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out from);
            bool hasTo = DateTime.TryParseExact(txtTo.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out to);

            // Build DataView RowFilter safely
            var dv = new DataView(dt);
            string filter = "1=1";

            if (!string.IsNullOrEmpty(search))
            {
                // Escape single quotes for RowFilter
                string s = search.Replace("'", "''");
                // BookingID (string cast), ClientName, ServiceName, OperationNumber
                filter += $" AND (Convert(BookingID, 'System.String') LIKE '%{s}%' " +
                          $"OR ClientName LIKE '%{s}%' " +
                          $"OR ServiceName LIKE '%{s}%' " +
                          $"OR Convert(OperationNumber, 'System.String') LIKE '%{s}%')";
            }

            if (!string.IsNullOrEmpty(status))
            {
                string st = status.Replace("'", "''");
                filter += $" AND Status = '{st}'";
            }

            if (hasFrom && hasTo)
            {
                // inclusive end date
                filter += $" AND ScheduledDate >= #{from:MM/dd/yyyy}# AND ScheduledDate < #{to.AddDays(1):MM/dd/yyyy}#";
            }
            else if (hasFrom)
            {
                filter += $" AND ScheduledDate >= #{from:MM/dd/yyyy}#";
            }
            else if (hasTo)
            {
                filter += $" AND ScheduledDate < #{to.AddDays(1):MM/dd/yyyy}#";
            }

            dv.RowFilter = filter;

            gvReschedules.PageIndex = 0; // reset to first page after filtering
            gvReschedules.DataSource = dv;
            gvReschedules.DataBind();
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            BindFiltered();
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtSearch.Text = string.Empty;
            ddlFilterStatus.SelectedIndex = 0;
            txtFrom.Text = string.Empty;
            txtTo.Text = string.Empty;

            BindFiltered();
        }

        protected void gvReschedules_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvReschedules.PageIndex = e.NewPageIndex;
            BindFiltered();
        }

        protected void gvReschedules_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            string status = DataBinder.Eval(e.Row.DataItem, "Status")?.ToString();
            var ddlStatus = (DropDownList)e.Row.FindControl("ddlStatus");
            if (ddlStatus != null && !string.IsNullOrEmpty(status))
            {
                var li = ddlStatus.Items.FindByValue(status);
                if (li != null)
                {
                    ddlStatus.ClearSelection();
                    li.Selected = true;
                }
            }
        }

        protected void gvReschedules_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "UpdateStatus") return;

            var row = (GridViewRow)((Control)e.CommandSource).NamingContainer;
            var ddl = (DropDownList)row.FindControl("ddlStatus");
            if (ddl == null) return;

            if (!int.TryParse(e.CommandArgument.ToString(), out int scheduleId))
            {
                lblMessage.Text = "⚠️ Invalid Schedule ID.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spServiceSchedule_UpdateStatus", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ScheduleID", SqlDbType.Int).Value = scheduleId;
                    cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = ddl.SelectedValue;
                    con.Open();
                    int rows = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                    if (rows > 0)
                    {
                        lblMessage.Text = "✅ Status updated successfully.";
                        lblMessage.ForeColor = System.Drawing.Color.Green;
                    }
                    else
                    {
                        lblMessage.Text = "⚠️ No changes were made.";
                        lblMessage.ForeColor = System.Drawing.Color.DarkOrange;
                    }
                }

                // Refresh cache and re-apply filters so the grid stays consistent
                LoadRescheduleBookings();
                BindFiltered();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "❌ Update failed: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }
    }
}
