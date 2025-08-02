using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class ApproveRescheduleRequests : System.Web.UI.Page
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

            // 🔐 Block SuperAdmin and Inspector
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            // 🔐 Check CanView permission for ManageBooking (adjust module name if needed)
          

            if (!IsPostBack)
            {
                LoadRescheduleRequests();
            }
        }

        private void LoadRescheduleRequests()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
    SELECT 
        rr.RequestID, 
        rr.ScheduleID, 
        rr.ClientID, 
        (c.LastName + ', ' + c.FirstName + ' ' + ISNULL(c.MiddleName, '')) AS ClientName, 
        c.Email,
        ss.ScheduledDate, 
        ss.OperationNumber, 
        rr.RequestedDate, 
        rr.Status, 
        b.BookingID,
        svc.ServiceNames
    FROM RescheduleRequests rr
    INNER JOIN Clients c ON rr.ClientID = c.ClientID
    INNER JOIN ServiceSchedule ss ON rr.ScheduleID = ss.ScheduleID
    INNER JOIN Bookings b ON ss.BookingID = b.BookingID
    LEFT JOIN (
        SELECT bs.BookingID, STRING_AGG(s.Name, ', ') AS ServiceNames
        FROM BookingServices bs
        INNER JOIN Services s ON bs.ServiceID = s.ServiceID
        GROUP BY bs.BookingID
    ) svc ON b.BookingID = svc.BookingID
    WHERE rr.Status = 'Pending'
    ORDER BY rr.RequestedDate DESC";


                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvRescheduleRequests.DataSource = dt;
                gvRescheduleRequests.DataBind();
            }
        }

        protected void gvRescheduleRequests_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                Label lblNewDate = (Label)e.Row.FindControl("lblNewDate");
                DateTime scheduledDate = Convert.ToDateTime(DataBinder.Eval(e.Row.DataItem, "ScheduledDate"));
                lblNewDate.Text = scheduledDate.ToString("yyyy-MM-dd");
            }
        }

        protected void gvRescheduleRequests_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int requestId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "Approve")
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand updateCmd = new SqlCommand(@"
                UPDATE RescheduleRequests 
                SET Status = 'Approved', ApprovedDate = GETDATE()
                WHERE RequestID = @RequestID;

                SELECT 
                    rr.ScheduleID, 
                    rr.ClientID, 
                    b.BookingID, 
                    svc.ServiceNames, 
                    c.Email
                FROM RescheduleRequests rr
                INNER JOIN ServiceSchedule ss ON rr.ScheduleID = ss.ScheduleID
                INNER JOIN Bookings b ON ss.BookingID = b.BookingID
                INNER JOIN Clients c ON rr.ClientID = c.ClientID
                LEFT JOIN (
                    SELECT bs.BookingID, STRING_AGG(s.Name, ', ') AS ServiceNames
                    FROM BookingServices bs
                    INNER JOIN Services s ON bs.ServiceID = s.ServiceID
                    GROUP BY bs.BookingID
                ) svc ON b.BookingID = svc.BookingID
                WHERE rr.RequestID = @RequestID;", con);

                    updateCmd.Parameters.AddWithValue("@RequestID", requestId);

                    using (SqlDataReader reader = updateCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int bookingId = Convert.ToInt32(reader["BookingID"]);
                            int scheduleId = Convert.ToInt32(reader["ScheduleID"]);
                            string serviceName = reader["ServiceNames"]?.ToString() ?? "(No Service)";
                            string clientEmail = reader["Email"].ToString();

                            reader.Close();

                            SqlCommand getDateCmd = new SqlCommand("SELECT ScheduledDate FROM ServiceSchedule WHERE ScheduleID = @ScheduleID", con);
                            getDateCmd.Parameters.AddWithValue("@ScheduleID", scheduleId);
                            object result = getDateCmd.ExecuteScalar();

                            if (result != null)
                            {
                                DateTime newScheduledDate = Convert.ToDateTime(result);

                                SqlCommand updateBookingCmd = new SqlCommand("UPDATE Bookings SET ScheduledDate = @NewDate, StartTime = @NewTime WHERE BookingID = @BookingID", con);
                                updateBookingCmd.Parameters.AddWithValue("@NewDate", newScheduledDate.Date);
                                updateBookingCmd.Parameters.AddWithValue("@NewTime", newScheduledDate.TimeOfDay);
                                updateBookingCmd.Parameters.AddWithValue("@BookingID", bookingId);
                                updateBookingCmd.ExecuteNonQuery();
                            }

                            SendApprovalEmail(clientEmail, serviceName);

                            ScriptManager.RegisterStartupScript(this, GetType(), "showSuccess", "Swal.fire('✅ Approved', 'The request has been approved.', 'success');", true);
                            LoadRescheduleRequests();
                        }
                    }
                }
            }
            else if (e.CommandName == "Reject")
            {
                string reason = hfRejectReason.Value.Trim();
                if (string.IsNullOrEmpty(reason))
                {
                    lblMessage.Text = "❌ Rejection reason required.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(@"
                UPDATE RescheduleRequests
                SET Status = 'Rejected', ApprovedDate = GETDATE(), RejectReason = @Reason
                WHERE RequestID = @RequestID;

                SELECT 
                    rr.ClientID, 
                    c.Email, 
                    svc.ServiceNames
                FROM RescheduleRequests rr
                INNER JOIN Clients c ON rr.ClientID = c.ClientID
                INNER JOIN ServiceSchedule ss ON rr.ScheduleID = ss.ScheduleID
                INNER JOIN Bookings b ON ss.BookingID = b.BookingID
                LEFT JOIN (
                    SELECT bs.BookingID, STRING_AGG(s.Name, ', ') AS ServiceNames
                    FROM BookingServices bs
                    INNER JOIN Services s ON bs.ServiceID = s.ServiceID
                    GROUP BY bs.BookingID
                ) svc ON b.BookingID = svc.BookingID
                WHERE rr.RequestID = @RequestID;", con);

                    cmd.Parameters.AddWithValue("@RequestID", requestId);
                    cmd.Parameters.AddWithValue("@Reason", reason);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string email = reader["Email"].ToString();
                            string service = reader["ServiceNames"]?.ToString() ?? "(No Service)";
                            SendRejectionEmail(email, service, reason);
                        }
                    }
                }

                ScriptManager.RegisterStartupScript(this, GetType(), "showRejected", "Swal.fire('❌ Rejected', 'The request has been rejected.', 'info');", true);
                LoadRescheduleRequests();
            }
        }

        private void SendApprovalEmail(string toEmail, string serviceName)
        {
            string subject = "✅ Reschedule Approved - RRC Management";
            string body = $"Hello,\n\nYour reschedule request for '{serviceName}' has been approved.\n\nThank you,\nRRC Management Team";
            SendEmail(toEmail, subject, body);
        }

        private void SendRejectionEmail(string toEmail, string serviceName, string reason)
        {
            string subject = "❌ Reschedule Rejected - RRC Management";
            string body = $"Hello,\n\nYour reschedule request for '{serviceName}' has been rejected.\nReason: {reason}\n\nIf you have questions, please contact our support.\n\nThank you,\nRRC Management Team";
            SendEmail(toEmail, subject, body);
        }

        private void SendEmail(string toEmail, string subject, string body)
        {
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(ConfigurationManager.AppSettings["emailFrom"]);
                mail.To.Add(toEmail);
                mail.Subject = subject;
                mail.Body = body;

                SmtpClient smtp = new SmtpClient();
                smtp.Send(mail);
            }
        }

        protected void gvRescheduleRequests_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvRescheduleRequests.PageIndex = e.NewPageIndex;
            LoadRescheduleRequests();
        }
    }
}
    