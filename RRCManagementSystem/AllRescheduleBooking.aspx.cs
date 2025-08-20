using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class AllRescheduleBooking : System.Web.UI.Page
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

            // 🔐 Deny access for SuperAdmin and Inspector
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
                LoadRescheduleBookings();
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
                        gvReschedules.DataSource = dt;
                        gvReschedules.DataBind();
                    }
                }
                lblMessage.Text = "";
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠️ Error loading schedules: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
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

                LoadRescheduleBookings();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "❌ Update failed: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }
    }
}
    