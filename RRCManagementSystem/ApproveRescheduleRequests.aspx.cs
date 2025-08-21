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
        private readonly string cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 🔐 Require login
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            // ✅ Allow only SuperAdmin/Admin (adjust if Inspectors should also access)
            var role = Session["Role"].ToString();
            if (!role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) &&
                !role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadRescheduleRequests();
            }
        }

        private void LoadRescheduleRequests()
        {
            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spReschedule_ListPending", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Status filter
                    cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = ddlStatus.SelectedValue;

                    // Search filter
                    string search = string.IsNullOrWhiteSpace(txtSearch.Text) ? null : txtSearch.Text.Trim();
                    cmd.Parameters.Add("@Search", SqlDbType.NVarChar, 100).Value = (object)search ?? DBNull.Value;

                    // Date filters
                    DateTime fromDate, toDate;
                    if (DateTime.TryParse(txtFrom.Text, out fromDate))
                        cmd.Parameters.Add("@FromDate", SqlDbType.Date).Value = fromDate;
                    else
                        cmd.Parameters.Add("@FromDate", SqlDbType.Date).Value = DBNull.Value;

                    if (DateTime.TryParse(txtTo.Text, out toDate))
                        cmd.Parameters.Add("@ToDate", SqlDbType.Date).Value = toDate;
                    else
                        cmd.Parameters.Add("@ToDate", SqlDbType.Date).Value = DBNull.Value;

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        gvRescheduleRequests.DataSource = dt;
                        gvRescheduleRequests.DataBind();
                    }
                }

                lblMessage.Text = "";
                lblMessage.ForeColor = System.Drawing.Color.Green;
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠️ Error loading requests: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnApply_Click(object sender, EventArgs e)
        {
            gvRescheduleRequests.PageIndex = 0; // reset to first page
            LoadRescheduleRequests();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            txtFrom.Text = "";
            txtTo.Text = "";
            ddlStatus.SelectedValue = "All";
            gvRescheduleRequests.PageIndex = 0;
            LoadRescheduleRequests();
        }


        protected void gvRescheduleRequests_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvRescheduleRequests.PageIndex = e.NewPageIndex;
            LoadRescheduleRequests();
        }

        protected void gvRescheduleRequests_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            // New Schedule Date column: prefer NewScheduledDate; fallback to RequestedDate; else "-"
            var lblNewDate = (Label)e.Row.FindControl("lblNewDate");
            if (lblNewDate != null)
            {
                var newSched = DataBinder.Eval(e.Row.DataItem, "NewScheduledDate");
                var requested = DataBinder.Eval(e.Row.DataItem, "RequestedDate");

                if (newSched != null && newSched != DBNull.Value && DateTime.TryParse(newSched.ToString(), out var dtNew))
                {
                    lblNewDate.Text = dtNew.ToString("yyyy-MM-dd");
                }
                else if (requested != null && requested != DBNull.Value && DateTime.TryParse(requested.ToString(), out var dtReq))
                {
                    lblNewDate.Text = dtReq.ToString("yyyy-MM-dd");
                }
                else
                {
                    lblNewDate.Text = "-";
                }
            }
        }

        protected void gvRescheduleRequests_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // Buttons pass RequestID via CommandArgument
            if (!int.TryParse(e.CommandArgument?.ToString(), out var requestId))
            {
                lblMessage.Text = "⚠️ Invalid Request ID.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            if (e.CommandName == "Approve")
            {
                HandleApprove(requestId);
            }
            else if (e.CommandName == "Reject")
            {
                // Reason is captured in hfRejectReason by JS modal
                var reason = (hfRejectReason.Value ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(reason))
                {
                    lblMessage.Text = "❌ Rejection reason required.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                HandleReject(requestId, reason);
            }
        }

        private void HandleApprove(int requestId)
        {
            try
            {
                string email = null, service = null;
                DateTime? newDate = null;
                int scheduleId = 0, bookingId = 0;

                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spReschedule_Approve", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@RequestID", SqlDbType.Int).Value = requestId;

                    con.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            // Expected: Email, ServiceNames, NewScheduledDate
                            email = r["Email"] as string;
                            service = r["ServiceNames"] as string ?? "(No Service)";
                            if (r["NewScheduledDate"] != DBNull.Value)
                                newDate = Convert.ToDateTime(r["NewScheduledDate"]);

                            // If your SP already returns ScheduleID/BookingID, read them here:
                            if (HasColumn(r, "ScheduleID")) scheduleId = Convert.ToInt32(r["ScheduleID"]);
                            if (HasColumn(r, "BookingID")) bookingId = Convert.ToInt32(r["BookingID"]);
                        }
                    }
                }

                // If SP didn’t include IDs, look them up quickly
                if (scheduleId == 0 || bookingId == 0)
                {
                    LookupScheduleAndBookingIdsByRequest(requestId, out scheduleId, out bookingId);
                }

                // Send email (best-effort)
                try { SendApprovalEmail(email, service, newDate ?? DateTime.MinValue); } catch { /* ignore mail errors */ }

                // ✅ SweetAlert then redirect to AssignBooking.aspx (with IDs in query string)
                var url = "AssignBooking.aspx";
                if (scheduleId > 0 || bookingId > 0)
                {
                    url += $"?{(bookingId > 0 ? "BookingID=" + bookingId : "")}" +
                           $"{(bookingId > 0 && scheduleId > 0 ? "&" : "")}" +
                           $"{(scheduleId > 0 ? "ScheduleID=" + scheduleId : "")}";
                }

                string js = $@"
                    Swal.fire('✅ Approved', 'The request has been approved and rescheduled.', 'success')
                        .then(function() {{
                            window.location = '{url}';
                        }});";
                ScriptManager.RegisterStartupScript(this, GetType(), "showApprovedAndRedirect", js, true);

                // Keep the grid fresh if user cancels the dialog somehow
                LoadRescheduleRequests();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "❌ Approve failed: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void HandleReject(int requestId, string reason)
        {
            try
            {
                string email = null, service = null;

                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spReschedule_Reject", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@RequestID", SqlDbType.Int).Value = requestId;
                    cmd.Parameters.Add("@Reason", SqlDbType.NVarChar, 500).Value = reason;

                    con.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            email = r["Email"]?.ToString();
                            service = r["ServiceNames"]?.ToString() ?? "(No Service)";
                        }
                    }
                }

                try { SendRejectionEmail(email, service, reason); } catch { /* ignore */ }

                ScriptManager.RegisterStartupScript(this, GetType(),
                    "showRejected",
                    "Swal.fire('❌ Rejected', 'The request has been rejected.', 'info');",
                    true);

                LoadRescheduleRequests();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "❌ Reject failed: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        private static bool HasColumn(IDataRecord r, string name)
        {
            for (int i = 0; i < r.FieldCount; i++)
                if (r.GetName(i).Equals(name, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        /// <summary>
        /// Fallback: get ScheduleID & BookingID from the request.
        /// </summary>
        private void LookupScheduleAndBookingIdsByRequest(int requestId, out int scheduleId, out int bookingId)
        {
            scheduleId = 0; bookingId = 0;

            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(@"
                SELECT TOP 1 rr.ScheduleID, ss.BookingID
                FROM dbo.RescheduleRequests rr
                JOIN dbo.ServiceSchedule ss ON ss.ScheduleID = rr.ScheduleID
                WHERE rr.RequestID = @RequestID;", con))
            {
                cmd.Parameters.Add("@RequestID", SqlDbType.Int).Value = requestId;
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        if (r["ScheduleID"] != DBNull.Value) scheduleId = Convert.ToInt32(r["ScheduleID"]);
                        if (r["BookingID"] != DBNull.Value) bookingId = Convert.ToInt32(r["BookingID"]);
                    }
                }
            }
        }

        private void SendApprovalEmail(string toEmail, string serviceName, DateTime newDate)
        {
            if (string.IsNullOrWhiteSpace(toEmail)) return;

            string subject = "✅ Reschedule Approved - RRC Management";
            string body =
$@"Hello,

Your reschedule request for '{serviceName}' has been approved.
New date: {newDate:yyyy-MM-dd}.

Thank you,
RRC Management Team";

            SendEmail(toEmail, subject, body);
        }

        private void SendRejectionEmail(string toEmail, string serviceName, string reason)
        {
            if (string.IsNullOrWhiteSpace(toEmail)) return;

            string subject = "❌ Reschedule Rejected - RRC Management";
            string body =
$@"Hello,

Your reschedule request for '{serviceName}' has been rejected.
Reason: {reason}

Thank you,
RRC Management Team";

            SendEmail(toEmail, subject, body);
        }

        private void SendEmail(string toEmail, string subject, string body)
        {
            using (var mail = new MailMessage())
            {
                mail.From = new MailAddress(ConfigurationManager.AppSettings["emailFrom"]);
                mail.To.Add(toEmail);
                mail.Subject = subject;
                mail.Body = body;

                using (var smtp = new SmtpClient()) { smtp.Send(mail); }
            }
        }
    }
}
