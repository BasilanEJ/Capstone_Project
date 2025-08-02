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

            // 🔐 Deny access for SuperAdmin and Inspector
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            // 🔐 Check CanView permission for ManageBooking
         
            if (!IsPostBack)
            {
                LoadRescheduleBookings();
            }
        }




        protected void gvReschedules_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Get the status from the current data item (i.e., the value from the DB)
                string status = DataBinder.Eval(e.Row.DataItem, "Status").ToString();

                // Find the dropdown in this row
                DropDownList ddlStatus = (DropDownList)e.Row.FindControl("ddlStatus");

                if (ddlStatus != null && !string.IsNullOrEmpty(status))
                {
                    ListItem selectedItem = ddlStatus.Items.FindByValue(status);
                    if (selectedItem != null)
                    {
                        ddlStatus.ClearSelection();
                        selectedItem.Selected = true;
                    }
                }
            }
        }

        private void LoadRescheduleBookings()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT 
                ss.ScheduleID,
                b.BookingID,
                (c.LastName + ', ' + c.FirstName + ' ' + ISNULL(c.MiddleName, '')) AS ClientName,
                b.ServiceNames AS ServiceName,
                ss.OperationNumber,
                ss.ScheduledDate,
                ss.Status
            FROM ServiceSchedule ss
            INNER JOIN Bookings b ON ss.BookingID = b.BookingID
            INNER JOIN Clients c ON b.ClientID = c.ClientID
            ORDER BY ss.ScheduledDate DESC";

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvReschedules.DataSource = dt;
                gvReschedules.DataBind();
            }
        }


        protected void gvReschedules_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "UpdateStatus")
            {
                GridViewRow row = (GridViewRow)((Control)e.CommandSource).NamingContainer;
                string scheduleId = e.CommandArgument.ToString();
                DropDownList ddlStatus = (DropDownList)row.FindControl("ddlStatus");

                if (!string.IsNullOrEmpty(scheduleId) && ddlStatus != null)
                {
                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();
                        SqlCommand cmd = new SqlCommand("UPDATE ServiceSchedule SET Status = @Status WHERE ScheduleID = @ScheduleID", con);
                        cmd.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue);
                        cmd.Parameters.AddWithValue("@ScheduleID", scheduleId);
                        cmd.ExecuteNonQuery();

                        lblMessage.Text = "✅ Status updated successfully.";
                    }

                    LoadRescheduleBookings();
                }
            }
        }
    }
}