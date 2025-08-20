using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class MyBookings : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["ClientID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                int clientId = Convert.ToInt32(Session["ClientID"]);
                LoadMyBookings(clientId);
                LoadUpcomingOperations(clientId);
                LoadAllOperations(clientId);
                CheckIfContractCompleted(clientId);
                CheckUpcomingContractualOperation(clientId);
                CheckForMissedOperations(clientId);

                // 🔔 Seed notifications for any bookings that are already confirmed/assigned
                // (DedupKey prevents duplicates if this runs again)
                try { SeedConfirmedBookingNotifications(clientId); } catch { /* non-blocking */ }
            }
        }

        private void LoadMyBookings(int clientId)
        {
            using (var con = new SqlConnection(connectionString))
            using (var da = new SqlDataAdapter("dbo.usp_ClientBookings_ListAndAutocomplete", con))
            {
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;

                var dt = new DataTable();
                da.Fill(dt);

                gvMyBookings.DataSource = dt;
                gvMyBookings.DataBind();

                // ✅ Plain label update (no SweetAlert)
                if (dt.Rows.Count > 0)
                {
                    lblMessage.Text = $"✅ You have {dt.Rows.Count} booking(s).";
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                }
                else
                {
                    lblMessage.Text = "⚠️ You have no bookings yet.";
                    lblMessage.ForeColor = System.Drawing.Color.Orange;
                }
            }
        }

        private void LoadUpcomingOperations(int clientId)
        {
            using (var con = new SqlConnection(connectionString))
            using (var da = new SqlDataAdapter("dbo.usp_ClientUpcomingOps_TermiteControl", con))
            {
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                da.SelectCommand.Parameters.Add("@DaysAhead", SqlDbType.Int).Value = 30;

                var dt = new DataTable();
                da.Fill(dt);

                gvUpcoming.DataSource = dt;
                gvUpcoming.DataBind();
                pnlUpcomingOps.Visible = dt.Rows.Count > 0;

                if (dt.Rows.Count > 0 && dt.Rows[0]["ScheduledDate"] != DBNull.Value)
                {
                    DateTime nextDate = Convert.ToDateTime(dt.Rows[0]["ScheduledDate"]);
                    lblNextOperationNotice.Text = $"⏰ Reminder: Your next operation is scheduled on {nextDate:MMMM dd, yyyy}.";
                    lblNextOperationNotice.Visible = true;
                }
                else
                {
                    lblNextOperationNotice.Visible = false;
                }
            }
        }

        private void LoadAllOperations(int clientId)
        {
            using (var con = new SqlConnection(connectionString))
            using (var da = new SqlDataAdapter("dbo.usp_ClientAllOps_AfterFirstCompleted", con))
            {
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;

                var dt = new DataTable();
                da.Fill(dt);

                gvAllOps.DataSource = dt;
                gvAllOps.DataBind();
                pnlAllOps.Visible = dt.Rows.Count > 0;
            }
        }

        protected void gvAllOps_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Reschedule")
            {
                string[] args = e.CommandArgument.ToString().Split('|');
                if (args.Length == 2)
                {
                    string scheduleId = args[0];
                    string scheduledDateTime = args[1];
                    string script = $"showModal('{scheduleId}', '{scheduledDateTime}');";
                    ScriptManager.RegisterStartupScript(this, GetType(), "ShowRescheduleModal", script, true);
                }
            }
        }

        protected void gvAllOps_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                string status = DataBinder.Eval(e.Row.DataItem, "Status").ToString();
                Label lbl = e.Row.FindControl("ltProgress") as Label;

                if (lbl != null)
                {
                    lbl.Text = status == "Completed"
                        ? "<span class='badge bg-success'>Done</span>"
                        : "<span class='badge bg-secondary'>Pending</span>";
                }
            }
        }

        private void CheckIfContractCompleted(int clientId)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.usp_ClientContractProgress", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;

                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        int totalOps = rdr["TotalOps"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["TotalOps"]);
                        int completedOps = rdr["CompletedOps"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["CompletedOps"]);

                        if (completedOps == totalOps && totalOps > 0)
                        {
                            lblContractStatus.Text = $"🎉 Congratulations! Your Termite Control service contract is fully completed as of {DateTime.Today:MMMM dd, yyyy}.";
                            lblContractStatus.Visible = true;
                        }
                    }
                }
            }
        }

        private void CheckUpcomingContractualOperation(int clientId)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.usp_ClientUpcomingOpInNDays", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                cmd.Parameters.Add("@Days", SqlDbType.Int).Value = 3;

                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        int op = Convert.ToInt32(rdr["OperationNumber"]);
                        lblContractStatus.Text = $"⏰ Reminder: Your Operation #{op} is coming up in 3 days. Please confirm your availability.";
                        lblContractStatus.Visible = true;
                    }
                }
            }
        }

        private void CheckForMissedOperations(int clientId)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.usp_ClientLatestMissedOp", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;

                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        DateTime missedDate = Convert.ToDateTime(rdr["ScheduledDate"]);
                        int op = Convert.ToInt32(rdr["OperationNumber"]);

                        lblContractStatus.Text = $"⚠️ You missed Operation #{op} on {missedDate:MMMM dd, yyyy}. Please contact us to reschedule.";
                        lblContractStatus.CssClass = "alert alert-warning fw-bold mt-4 d-block";
                        lblContractStatus.Visible = true;
                    }
                }
            }
        }

        protected void gvMyBookings_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvMyBookings.PageIndex = e.NewPageIndex;
            LoadMyBookings(Convert.ToInt32(Session["ClientID"]));
        }

        protected void gvMyBookings_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                string status = DataBinder.Eval(e.Row.DataItem, "Status").ToString();
                if (status == "Assigned")
                    e.Row.Cells[4].CssClass = "status-assigned";
                else if (status == "Pending")
                    e.Row.Cells[4].CssClass = "status-pending";
                else if (status == "Cancelled")
                    e.Row.Cells[4].CssClass = "status-cancelled";
            }
        }

        protected void gvUpcoming_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "SetSchedule")
            {
                string[] args = e.CommandArgument.ToString().Split('|');
                if (args.Length == 2)
                {
                    string scheduleId = args[0];
                    string scheduledDateTime = args[1];

                    string script = $"showModal('{scheduleId}', '{scheduledDateTime}');";
                    ScriptManager.RegisterStartupScript(this, GetType(), "ShowSetScheduleModal", script, true);
                }
            }
        }

        protected void btnConfirmSchedule_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(hfSelectedScheduleID.Value))
            {
                int scheduleId = Convert.ToInt32(hfSelectedScheduleID.Value);
                int clientId = Convert.ToInt32(Session["ClientID"]);

                DateTime newDate = DateTime.Parse(txtNewScheduleDate.Text);
                TimeSpan newTime = TimeSpan.Parse(txtNewScheduleTime.Text);
                DateTime combinedDate = newDate.Add(newTime);

                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.usp_ServiceSchedule_ClientReschedule", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ScheduleID", SqlDbType.Int).Value = scheduleId;
                    cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                    cmd.Parameters.Add("@NewDateTime", SqlDbType.DateTime).Value = combinedDate;

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                ScriptManager.RegisterStartupScript(this, GetType(), "ScheduleSuccess", "Swal.fire('Saved!', 'Schedule updated successfully.', 'success');", true);
                hfSelectedScheduleID.Value = "";
                LoadUpcomingOperations(clientId);
                ScriptManager.RegisterStartupScript(this, GetType(), "HideModal", "hideModal();", true);
            }
        }

        protected void gvMyBookings_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "CancelBooking")
            {
                int bookingId = Convert.ToInt32(e.CommandArgument);
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.usp_Booking_CancelIfPending", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingId;

                    con.Open();
                    int rows = cmd.ExecuteNonQuery();

                    if (rows > 0)
                        ScriptManager.RegisterStartupScript(this, GetType(), "CancelSuccess", "Swal.fire('Cancelled!', 'Booking cancelled successfully.', 'success');", true);
                    else
                        ScriptManager.RegisterStartupScript(this, GetType(), "CancelFail", "Swal.fire('Oops!', 'Unable to cancel. Booking may already be processed.', 'warning');", true);
                }

                LoadMyBookings(Convert.ToInt32(Session["ClientID"]));
            }
        }

        /// <summary>
        /// Creates a "Booking Confirmed" notification for any booking that is assigned/approved/confirmed
        /// and has a scheduled date/time. DedupKey prevents duplicates across page visits.
        /// </summary>
        private void SeedConfirmedBookingNotifications(int clientId)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"
                SELECT TOP 50 b.BookingID,
                       b.ScheduledDate,      -- DATE
                       b.StartTime,          -- TIME
                       b.Status
                FROM dbo.Bookings b
                WHERE b.ClientID = @ClientID
                  AND b.Status IN ('Assigned','Approved','Confirmed')
                  AND b.ScheduledDate IS NOT NULL
                  AND b.StartTime IS NOT NULL
                ORDER BY b.ScheduledDate DESC, b.StartTime DESC;", con))
            {
                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;

                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        int bookingId = Convert.ToInt32(r["BookingID"]);
                        DateTime scheduledDate = Convert.ToDateTime(r["ScheduledDate"]);
                        // StartTime from SQL TIME → read as TimeSpan
                        TimeSpan startTime = (r["StartTime"] is TimeSpan)
                            ? (TimeSpan)r["StartTime"]
                            : TimeSpan.Parse(r["StartTime"].ToString());

                        // 🔔 Insert via SP (dedup ensures this is only added once)
                        try
                        {
                            using (var con2 = new SqlConnection(connectionString))
                            using (var cmd2 = new SqlCommand("dbo.usp_Notifications_Add", con2))
                            {
                                cmd2.CommandType = CommandType.StoredProcedure;
                                cmd2.Parameters.AddWithValue("@ClientID", clientId);
                                cmd2.Parameters.AddWithValue("@Type", "booking");
                                cmd2.Parameters.AddWithValue("@Title", "Booking Confirmed");
                                cmd2.Parameters.AddWithValue("@Body",
                                    "Your service is scheduled on " +
                                    scheduledDate.ToString("MMM dd, yyyy") +
                                    " at " + startTime.ToString(@"hh\:mm") + ".");
                                cmd2.Parameters.AddWithValue("@Url", "MyBookings.aspx");
                                cmd2.Parameters.AddWithValue("@DedupKey", "BOOK-" + bookingId + "-CONFIRMED");
                                con2.Open();
                                cmd2.ExecuteNonQuery();
                            }
                        }
                        catch
                        {
                            // Ignore dup/other errors so the page keeps working
                        }
                    }
                }
            }
        }
    }
}
