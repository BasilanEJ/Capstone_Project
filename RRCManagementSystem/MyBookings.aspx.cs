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
            }
        }

        private void LoadMyBookings(int clientId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT b.BookingID, b.ServiceNames, b.ScheduledDate,
                           CONVERT(varchar(5), b.StartTime, 108) AS StartTime,
                           ISNULL(b.Status, 'Pending') AS Status,
                           b.Notes, b.CreatedAt,
                           (SELECT COUNT(*) FROM ServiceSchedule ss WHERE ss.BookingID = b.BookingID) AS TotalOps,
                           (SELECT COUNT(*) FROM ServiceSchedule ss WHERE ss.BookingID = b.BookingID AND ss.Status = 'Completed') AS CompletedOps
                    FROM Bookings b
                    WHERE b.ClientID = @ClientID
                    ORDER BY b.CreatedAt DESC";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ClientID", clientId);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                con.Open();
                foreach (DataRow row in dt.Rows)
                {
                    int total = Convert.ToInt32(row["TotalOps"]);
                    int completed = Convert.ToInt32(row["CompletedOps"]);
                    int bookingId = Convert.ToInt32(row["BookingID"]);

                    if (total > 0 && completed == total && row["Status"].ToString() != "Completed")
                    {
                        string updateQuery = "UPDATE Bookings SET Status = 'Completed' WHERE BookingID = @BookingID";
                        SqlCommand updateCmd = new SqlCommand(updateQuery, con);
                        updateCmd.Parameters.AddWithValue("@BookingID", bookingId);
                        updateCmd.ExecuteNonQuery();
                        row["Status"] = "Completed";
                    }
                }

                gvMyBookings.DataSource = dt;
                gvMyBookings.DataBind();

                lblMessage.Text = dt.Rows.Count > 0
                    ? $"✅ You have {dt.Rows.Count} booking(s)."
                    : "⚠️ You have no bookings yet.";
                lblMessage.ForeColor = dt.Rows.Count > 0 ? System.Drawing.Color.Green : System.Drawing.Color.Orange;
            }
        }

        private void LoadUpcomingOperations(int clientId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT ss.ScheduleID, ss.ScheduledDate, ss.OperationNumber, ss.Status
                    FROM ServiceSchedule ss
                    INNER JOIN Bookings b ON ss.BookingID = b.BookingID
                    INNER JOIN BookingServices bs ON b.BookingID = bs.BookingID
                    INNER JOIN Services s ON bs.ServiceID = s.ServiceID
                    WHERE b.ClientID = @ClientID
                      AND s.ServiceType = 'Termite Control'
                      AND ss.Status != 'Completed'
                      AND ss.ScheduledDate BETWEEN GETDATE() AND DATEADD(DAY, 30, GETDATE())
                    ORDER BY ss.ScheduledDate";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ClientID", clientId);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvUpcoming.DataSource = dt;
                gvUpcoming.DataBind();
                pnlUpcomingOps.Visible = dt.Rows.Count > 0;

                if (dt.Rows.Count > 0)
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
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT ss.ScheduleID, ss.BookingID, ss.OperationNumber, ss.ScheduledDate, ss.Status, b.CreatedAt
                    FROM ServiceSchedule ss
                    INNER JOIN Bookings b ON ss.BookingID = b.BookingID
                    INNER JOIN BookingServices bs ON b.BookingID = bs.BookingID
                    INNER JOIN Services s ON bs.ServiceID = s.ServiceID
                    WHERE b.ClientID = @ClientID
                      AND s.ServiceType = 'Termite Control'
                      AND EXISTS (
                        SELECT 1 FROM ServiceSchedule s2 
                        WHERE s2.BookingID = b.BookingID AND s2.OperationNumber = 1 AND s2.Status = 'Completed'
                      )
                    ORDER BY ss.ScheduledDate";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ClientID", clientId);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
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
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT COUNT(*) AS TotalOps,
                           SUM(CASE WHEN ss.Status = 'Completed' THEN 1 ELSE 0 END) AS CompletedOps
                    FROM ServiceSchedule ss
                    INNER JOIN Bookings b ON ss.BookingID = b.BookingID
                    INNER JOIN BookingServices bs ON b.BookingID = bs.BookingID
                    INNER JOIN Services s ON bs.ServiceID = s.ServiceID
                    WHERE b.ClientID = @ClientID AND s.ServiceType = 'Termite Control'";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ClientID", clientId);
                con.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    int totalOps = Convert.ToInt32(reader["TotalOps"]);
                    int completedOps = reader["CompletedOps"] != DBNull.Value ? Convert.ToInt32(reader["CompletedOps"]) : 0;

                    if (completedOps == totalOps && totalOps > 0)
                    {
                        lblContractStatus.Text = $"🎉 Congratulations! Your Termite Control service contract is fully completed as of {DateTime.Today:MMMM dd, yyyy}.";
                        lblContractStatus.Visible = true;
                    }
                }

            }
        }

        private void CheckUpcomingContractualOperation(int clientId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT TOP 1 ss.ScheduledDate, ss.OperationNumber
                    FROM ServiceSchedule ss
                    INNER JOIN Bookings b ON ss.BookingID = b.BookingID
                    INNER JOIN BookingServices bs ON b.BookingID = bs.BookingID
                    INNER JOIN Services s ON bs.ServiceID = s.ServiceID
                    WHERE b.ClientID = @ClientID 
                      AND s.ServiceType = 'Termite Control'
                      AND ss.Status = 'Scheduled'
                      AND DATEDIFF(DAY, GETDATE(), ss.ScheduledDate) = 3
                    ORDER BY ss.ScheduledDate";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ClientID", clientId);
                con.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    int op = Convert.ToInt32(reader["OperationNumber"]);
                    lblContractStatus.Text = $"⏰ Reminder: Your Operation #{op} is coming up in 3 days. Please confirm your availability.";
                    lblContractStatus.Visible = true;
                }
            }
        }

        private void CheckForMissedOperations(int clientId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT TOP 1 ss.ScheduledDate, ss.OperationNumber
                    FROM ServiceSchedule ss
                    INNER JOIN Bookings b ON ss.BookingID = b.BookingID
                    INNER JOIN BookingServices bs ON b.BookingID = bs.BookingID
                    INNER JOIN Services s ON bs.ServiceID = s.ServiceID
                    WHERE b.ClientID = @ClientID
                      AND s.ServiceType = 'Termite Control'
                      AND ss.Status != 'Completed'
                      AND ss.ScheduledDate < GETDATE()
                    ORDER BY ss.ScheduledDate DESC";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ClientID", clientId);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    DateTime missedDate = Convert.ToDateTime(reader["ScheduledDate"]);
                    int op = Convert.ToInt32(reader["OperationNumber"]);

                    lblContractStatus.Text = $"⚠️ You missed Operation #{op} on {missedDate:MMMM dd, yyyy}. Please contact us to reschedule.";
                    lblContractStatus.CssClass = "alert alert-warning fw-bold mt-4 d-block";
                    lblContractStatus.Visible = true;
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

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string updateScheduleQuery = "UPDATE ServiceSchedule SET ScheduledDate = @NewDate WHERE ScheduleID = @ScheduleID";
                    using (SqlCommand cmd = new SqlCommand(updateScheduleQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@NewDate", combinedDate);
                        cmd.Parameters.AddWithValue("@ScheduleID", scheduleId);
                        cmd.ExecuteNonQuery();
                    }

                    string checkRequestQuery = "SELECT COUNT(*) FROM RescheduleRequests WHERE ScheduleID = @ScheduleID AND Status = 'Pending'";
                    using (SqlCommand checkCmd = new SqlCommand(checkRequestQuery, con))
                    {
                        checkCmd.Parameters.AddWithValue("@ScheduleID", scheduleId);
                        int count = (int)checkCmd.ExecuteScalar();

                        if (count == 0)
                        {
                            string insertRequestQuery = @"
                                INSERT INTO RescheduleRequests (ScheduleID, ClientID, RequestedDate, Status)
                                VALUES (@ScheduleID, @ClientID, GETDATE(), 'Pending')";

                            using (SqlCommand insertCmd = new SqlCommand(insertRequestQuery, con))
                            {
                                insertCmd.Parameters.AddWithValue("@ScheduleID", scheduleId);
                                insertCmd.Parameters.AddWithValue("@ClientID", clientId);
                                insertCmd.ExecuteNonQuery();
                            }
                        }
                    }
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
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Bookings SET Status = 'Cancelled' WHERE BookingID = @BookingID AND Status = 'Pending'";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@BookingID", bookingId);
                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        ScriptManager.RegisterStartupScript(this, GetType(), "CancelSuccess", "Swal.fire('Cancelled!', 'Booking cancelled successfully.', 'success');", true);
                    }
                    else
                    {
                        ScriptManager.RegisterStartupScript(this, GetType(), "CancelFail", "Swal.fire('Oops!', 'Unable to cancel. Booking may already be processed.', 'warning');", true);
                    }
                }

                LoadMyBookings(Convert.ToInt32(Session["ClientID"]));
            }
        }
    }
}
