using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class MyBookings : System.Web.UI.Page
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["ClientID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (IsPostBack)
            {
                string eventTarget = Request["__EVENTTARGET"];
                if (eventTarget == "CancelBooking")
                {
                    string bookingIdStr = hdnCancelBooking.Value;
                    if (int.TryParse(bookingIdStr, out int bookingId))
                    {
                        CancelBooking(bookingId);
                    }
                }
            }
            else
            {
                int clientId = Convert.ToInt32(Session["ClientID"]);
                LoadMyBookings(clientId);
                LoadUpcomingOperations(clientId);
                LoadAllOperations(clientId);
                CheckIfContractCompleted(clientId);
                CheckUpcomingContractualOperation(clientId);
                CheckForMissedOperations(clientId);

                try { SeedConfirmedBookingNotifications(clientId); } catch { }
            }
        }

        #region Load Bookings

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

                lblMessage.Text = dt.Rows.Count > 0
                    ? $"✅ You have {dt.Rows.Count} booking(s)."
                    : "⚠️ You have no bookings yet.";

                lblMessage.CssClass = dt.Rows.Count > 0
                    ? "block"
                    : "block";
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
            using (var da = new SqlDataAdapter(@"
                SELECT 
                    ss.ScheduleID,
                    ss.BookingID,
                    b.BookingCode,
                    ss.OperationNumber,
                    ss.ScheduledDate,
                    ss.Status,
                    b.CreatedAt
                FROM ServiceSchedule ss
                INNER JOIN Bookings b ON ss.BookingID = b.BookingID
                WHERE b.ClientID = @ClientID
                ORDER BY ss.OperationNumber;", con))
            {
                da.SelectCommand.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;

                var dt = new DataTable();
                da.Fill(dt);

                gvAllOps.DataSource = dt;
                gvAllOps.DataBind();
                pnlAllOps.Visible = dt.Rows.Count > 0;
            }
        }

        #endregion

        #region GridView Events

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
                // Status styling is now handled in the template field
            }
        }

        protected void gvUpcoming_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "SetSchedule")
            {
                string[] args = e.CommandArgument.ToString().Split('|');
                if (args.Length == 3)
                {
                    string scheduleId = args[0];
                    string scheduledDateTime = args[1];
                    string bookingId = args[2];

                    // Load SQM for this booking
                    LoadBookingSQMForReschedule(Convert.ToInt32(bookingId));

                    string script = $"showModal('{scheduleId}', '{scheduledDateTime}', '{bookingId}');";
                    ScriptManager.RegisterStartupScript(this, GetType(), "ShowSetScheduleModal", script, true);
                }
            }
        }

        protected void gvAllOps_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Reschedule")
            {
                string[] args = e.CommandArgument.ToString().Split('|');
                if (args.Length == 3)
                {
                    string scheduleId = args[0];
                    string scheduledDateTime = args[1];
                    string bookingId = args[2];

                    // Load SQM for this booking
                    LoadBookingSQMForReschedule(Convert.ToInt32(bookingId));

                    string script = $"showModal('{scheduleId}', '{scheduledDateTime}', '{bookingId}');";
                    ScriptManager.RegisterStartupScript(this, GetType(), "ShowRescheduleModal", script, true);
                }
            }
        }

        protected void gvAllOps_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                string status = DataBinder.Eval(e.Row.DataItem, "Status").ToString();
                var lt = e.Row.FindControl("ltProgress") as Literal;

                if (lt != null)
                {
                    lt.Text = status == "Completed"
                        ? "<span class='progress-badge progress-done'><i class='fas fa-check-circle mr-1'></i>Done</span>"
                        : "<span class='progress-badge progress-pending'><i class='far fa-clock mr-1'></i>Pending</span>";
                }
            }
        }

        #endregion

        #region Helper Methods

        private void LoadBookingSQMForReschedule(int bookingId)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("SELECT SQM FROM Bookings WHERE BookingID = @BookingID", con))
            {
                cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingId;

                con.Open();
                var result = cmd.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    int sqm = Convert.ToInt32(result);
                    hfSQM.Value = sqm.ToString();
                }
                else
                {
                    hfSQM.Value = "100"; // Default fallback
                }
            }
        }

        #endregion

        #region WebMethods

        [WebMethod]
        public static int GetBookingSQM(int bookingId)
        {
            try
            {
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("SELECT SQM FROM Bookings WHERE BookingID = @BookingID", con))
                {
                    cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingId;

                    con.Open();
                    var result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetBookingSQM Error: {ex.Message}");
            }

            return 100; // Default fallback
        }

        [WebMethod]
        public static object CheckDateAvailability(string date, int sqm)
        {
            try
            {
                if (!DateTime.TryParse(date, out DateTime serviceDate))
                {
                    return new { Success = false, Message = "Invalid date format" };
                }

                var timeSlots = new System.Collections.Generic.List<object>();

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spTeam_CheckDateAvailabilityBySlot", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ServiceDate", SqlDbType.Date).Value = serviceDate;
                    cmd.Parameters.Add("@SQM", SqlDbType.Int).Value = sqm;

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            timeSlots.Add(new
                            {
                                TimeSlotID = Convert.ToInt32(reader["TimeSlotID"]),
                                TimeSlotName = reader["TimeSlotName"].ToString(),
                                TotalTeams = Convert.ToInt32(reader["TotalTeams"]),
                                BookedCount = Convert.ToInt32(reader["BookedCount"]),
                                AvailableTeams = Convert.ToInt32(reader["AvailableTeams"]),
                                IsAvailable = Convert.ToBoolean(reader["IsAvailable"])
                            });
                        }
                    }
                }

                return new { Success = true, TimeSlots = timeSlots };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CheckDateAvailability Error: {ex.Message}");
                return new { Success = false, Message = "Error checking availability" };
            }
        }

        [WebMethod]
        public static object CheckTimeSlotAvailability(string date, int timeSlotId, int sqm)
        {
            try
            {
                if (!DateTime.TryParse(date, out DateTime serviceDate))
                {
                    return new { IsAvailable = false, Message = "Invalid date format" };
                }

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spTeam_CheckTimeSlotAvailability", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ServiceDate", SqlDbType.Date).Value = serviceDate;
                    cmd.Parameters.Add("@TimeSlotID", SqlDbType.Int).Value = timeSlotId;
                    cmd.Parameters.Add("@SQM", SqlDbType.Int).Value = sqm;

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int availableTeams = Convert.ToInt32(reader["AvailableTeams"]);
                            int totalTeams = Convert.ToInt32(reader["TotalTeams"]);

                            return new
                            {
                                IsAvailable = Convert.ToBoolean(reader["IsAvailable"]),
                                TotalTeams = totalTeams,
                                BookedCount = Convert.ToInt32(reader["BookedCount"]),
                                AvailableTeams = availableTeams,
                                Message = availableTeams > 0
                                    ? $"{availableTeams} team(s) available"
                                    : "This time slot is fully booked"
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CheckTimeSlotAvailability Error: {ex.Message}");
            }

            return new { IsAvailable = false, Message = "Error checking availability" };
        }

        #endregion

        #region Actions

        protected void btnConfirmSchedule_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(hfSelectedScheduleID.Value))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "NoSched",
                    @"Swal.fire({
                        icon: 'warning',
                        title: 'No Schedule Selected',
                        text: 'Please select a schedule first.',
                        confirmButtonColor: '#2563eb'
                    });", true);
                return;
            }

            int scheduleId = Convert.ToInt32(hfSelectedScheduleID.Value);
            int clientId = Convert.ToInt32(Session["ClientID"]);

            if (string.IsNullOrWhiteSpace(txtNewScheduleDate.Text))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "MissingDate",
                    @"Swal.fire({
                        icon: 'warning',
                        title: 'Missing Information',
                        text: 'Please select a date.',
                        confirmButtonColor: '#2563eb'
                    });", true);
                return;
            }

            if (string.IsNullOrWhiteSpace(hfSelectedTimeSlotID.Value))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "MissingTimeSlot",
                    @"Swal.fire({
                        icon: 'warning',
                        title: 'Missing Time Slot',
                        text: 'Please select a time slot.',
                        confirmButtonColor: '#2563eb'
                    });", true);
                return;
            }

            if (!DateTime.TryParse(txtNewScheduleDate.Text, out DateTime newDate))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "BadDate",
                    @"Swal.fire({
                        icon: 'error',
                        title: 'Invalid Input',
                        text: 'Invalid date format.',
                        confirmButtonColor: '#dc2626'
                    });", true);
                return;
            }

            if (!int.TryParse(hfSelectedTimeSlotID.Value, out int timeSlotId) || timeSlotId < 1 || timeSlotId > 4)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "BadTimeSlot",
                    @"Swal.fire({
                        icon: 'error',
                        title: 'Invalid Time Slot',
                        text: 'Please select a valid time slot.',
                        confirmButtonColor: '#dc2626'
                    });", true);
                return;
            }

            // Convert time slot ID to StartTime
            TimeSpan newTime = GetStartTimeFromSlot(timeSlotId);
            DateTime newScheduledDate = newDate.Date.Add(newTime);

            // Validate: must be at least 1 hour from now
            DateTime oneHourFromNow = DateTime.Now.AddHours(1);
            if (newScheduledDate < oneHourFromNow)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "TooSoon",
                    @"Swal.fire({
                        icon: 'error',
                        title: 'Invalid Schedule',
                        text: 'Please select a date and time at least 1 hour from now.',
                        confirmButtonColor: '#dc2626'
                    });", true);
                return;
            }

            // Insert reschedule request
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.usp_RescheduleRequest_Create", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ScheduleID", SqlDbType.Int).Value = scheduleId;
                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                cmd.Parameters.Add("@NewScheduledDate", SqlDbType.DateTime).Value = newScheduledDate;

                try
                {
                    con.Open();
                    cmd.ExecuteNonQuery();

                    // ✅ CLEAR THE HIDDEN FIELDS FIRST - BEFORE showing popup
                    hfSelectedScheduleID.Value = "";
                    hfSelectedTimeSlotID.Value = "";
                    txtNewScheduleDate.Text = "";

                    // ✅ Reload the grids to show updated data
                    LoadUpcomingOperations(clientId);
                    LoadAllOperations(clientId);

                    // ✅ Show success message WITHOUT window.location.reload()
                    ScriptManager.RegisterStartupScript(this, GetType(), "ReqSent",
                        @"Swal.fire({
                            icon: 'success',
                            title: 'Request Sent!',
                            text: 'Your reschedule request has been submitted for approval.',
                            confirmButtonColor: '#16a34a'
                        }).then(() => {
                            hideModal();
                        });", true);
                }
                catch (Exception ex)
                {
                    string safeMessage = ex.Message
                        .Replace("'", "\\'")
                        .Replace("\"", "\\\"")
                        .Replace("\r", "")
                        .Replace("\n", " ");

                    ScriptManager.RegisterStartupScript(this, GetType(), "ReqError",
                        $@"Swal.fire({{
                            icon: 'error',
                            title: 'Request Failed',
                            text: 'Unable to submit request: {safeMessage}',
                            confirmButtonColor: '#dc2626'
                        }});", true);
                }
            }
        }

        private TimeSpan GetStartTimeFromSlot(int timeSlotId)
        {
            switch (timeSlotId)
            {
                case 1: return new TimeSpan(8, 0, 0);   // 8:00 AM
                case 2: return new TimeSpan(12, 0, 0);  // 12:00 PM
                case 3: return new TimeSpan(16, 0, 0);  // 4:00 PM
                case 4: return new TimeSpan(20, 0, 0);  // 8:00 PM
                default: return new TimeSpan(8, 0, 0);  // Default to 8 AM
            }
        }

        private void CancelBooking(int bookingId)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.usp_Booking_CancelIfPending", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingId;

                try
                {
                    con.Open();
                    int rows = cmd.ExecuteNonQuery();

                    if (rows > 0)
                    {
                        ScriptManager.RegisterStartupScript(this, GetType(), "CancelSuccess",
                            @"Swal.fire({
                                icon: 'success',
                                title: 'Booking Cancelled',
                                text: 'Your booking has been cancelled successfully.',
                                confirmButtonColor: '#16a34a'
                            });", true);
                    }
                    else
                    {
                        ScriptManager.RegisterStartupScript(this, GetType(), "CancelFail",
                            $@"Swal.fire({{
                                icon: 'warning',
                                title: 'Unable to Cancel',
                                text: 'Booking may already be processed or assigned. Please contact support. (ID: {bookingId})',
                                confirmButtonColor: '#f59e0b'
                            }});", true);
                    }
                }
                catch (Exception ex)
                {
                    string safeMessage = ex.Message
                        .Replace("'", "\\'")
                        .Replace("\"", "\\\"")
                        .Replace("\r", "")
                        .Replace("\n", " ");

                    ScriptManager.RegisterStartupScript(this, GetType(), "CancelError",
                        $@"Swal.fire({{
                            icon: 'error',
                            title: 'Cancellation Failed',
                            text: 'Error: {safeMessage}',
                            confirmButtonColor: '#dc2626'
                        }});", true);
                }
            }

            LoadMyBookings(Convert.ToInt32(Session["ClientID"]));
        }

        #endregion

        #region Contract & Notifications

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
                        lblContractStatus.Visible = true;
                    }
                }
            }
        }

        private void SeedConfirmedBookingNotifications(int clientId)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"
                SELECT TOP 50 b.BookingID, b.ScheduledDate, b.StartTime, b.Status
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
                        TimeSpan startTime = TimeSpan.Parse(r["StartTime"].ToString());

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
                                    $"Your service is scheduled on {scheduledDate:MMM dd, yyyy} at {startTime:hh\\:mm}.");
                                cmd2.Parameters.AddWithValue("@Url", "MyBookings.aspx");
                                cmd2.Parameters.AddWithValue("@DedupKey", $"BOOK-{bookingId}-CONFIRMED");
                                con2.Open();
                                cmd2.ExecuteNonQuery();
                            }
                        }
                        catch { /* Ignore notification errors */ }
                    }
                }
            }
        }

        #endregion
    }
}