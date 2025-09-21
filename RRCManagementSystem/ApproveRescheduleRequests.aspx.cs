using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Web.UI;
using System.Web.UI.WebControls;
using RRCManagementSystem.Helpers;

namespace RRCManagementSystem
{
    public partial class ApproveRescheduleRequests : System.Web.UI.Page
    {
        private readonly string cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            var role = Session["Role"].ToString();
            if (!role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) &&
                !role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);
            if (!HasEditPermission(userId, "ManageBooking"))
            {
                lblMessage.Text = "❌ You do not have permission to approve or reject reschedule requests.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                gvRescheduleRequests.Visible = false;
                btnApply.Visible = false;
                btnClear.Visible = false;
                return;
            }

            if (!IsPostBack)
            {
                LoadRescheduleRequests();
            }
        }


        private bool HasEditPermission(int adminId, string moduleName)
        {
            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spAdminPermission_Check", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = adminId;
                    cmd.Parameters.Add("@ModuleName", SqlDbType.NVarChar, 100).Value = moduleName;
                    cmd.Parameters.Add("@Permission", SqlDbType.NVarChar, 10).Value = "CanEdit";

                    con.Open();
                    object allowed = cmd.ExecuteScalar();
                    return allowed != null && allowed != DBNull.Value && Convert.ToBoolean(allowed);
                }
            }
            catch
            {
                return false; // deny by default
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
                            // Decrypt the email
                            var encryptedEmail = r["EmailEnc"] as string;
                            email = AESHelper.DecryptEmail(encryptedEmail);

                            service = r["ServiceNames"] as string ?? "(No Service)";
                            if (r["NewScheduledDate"] != DBNull.Value)
                                newDate = Convert.ToDateTime(r["NewScheduledDate"]);

                            // If SP returns IDs
                            if (HasColumn(r, "ScheduleID")) scheduleId = Convert.ToInt32(r["ScheduleID"]);
                            if (HasColumn(r, "BookingID")) bookingId = Convert.ToInt32(r["BookingID"]);
                        }
                    }
                }

                // Send email
                if (!string.IsNullOrEmpty(email))
                {
                    SendApprovalEmail(email, service, newDate ?? DateTime.MinValue);
                }

                // ✅ Redirect after approval
                string url = "AssignBooking.aspx";
                if (bookingId > 0 || scheduleId > 0)
                {
                    url += $"?{(bookingId > 0 ? "BookingID=" + bookingId : "")}" +
                           $"{(bookingId > 0 && scheduleId > 0 ? "&" : "")}" +
                           $"{(scheduleId > 0 ? "ScheduleID=" + scheduleId : "")}";
                }

                ScriptManager.RegisterStartupScript(this, GetType(), "showApprovedAndRedirect", $@"
            Swal.fire('✅ Approved', 'The request has been approved and rescheduled.', 'success')
                .then(function() {{
                    window.location = '{url}';
                }});", true);

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
                            // ✅ Decrypt the email here
                            var encryptedEmail = r["EmailEnc"] as string;
                            email = AESHelper.DecryptEmail(encryptedEmail);

                            service = r["ServiceNames"]?.ToString() ?? "(No Service)";
                        }
                    }
                }

                // Send rejection email if decrypted email is valid
                if (!string.IsNullOrEmpty(email))
                {
                    try
                    {
                        SendRejectionEmail(email, service, reason);
                    }
                    catch
                    {
                        // Ignore email send errors but do not break process
                    }
                }

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

        private string BuildHtmlEmail(string title, string message, string highlightColor)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
        }}
        .container {{
            max-width: 600px;
            margin: 30px auto;
            background: #ffffff;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
        }}
        .header {{
            background-color: {highlightColor};
            text-align: center;
            padding: 20px;
        }}
        .header h1 {{
            color: #ffffff;
            margin: 0;
            font-size: 24px;
        }}
        .content {{
            padding: 20px;
            color: #333333;
            font-size: 16px;
        }}
        .content p {{
            line-height: 1.6;
        }}
        .footer {{
            background: #f4f4f4;
            padding: 15px;
            text-align: center;
            font-size: 12px;
            color: #777777;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>{title}</h1>
        </div>
        <div class='content'>
            {message}
            <p style='margin-top:30px;'>Thank you,<br><strong>RRC Management Team</strong></p>
        </div>
        <div class='footer'>
            <p>© {DateTime.Now.Year} RRC Management. All rights reserved.</p>
            <p>123 RRC Street, City, Country</p>
        </div>
    </div>
</body>
</html>";
        }



        private void SendApprovalEmail(string toEmail, string serviceName, DateTime newDate)
        {
            if (string.IsNullOrWhiteSpace(toEmail)) return;

            string subject = "✅ Reschedule Approved - RRC Management";

            string message = $@"
        <p>Hello,</p>
        <p>We are pleased to inform you that your reschedule request for 
        <strong>{serviceName}</strong> has been approved.</p>
        <p><strong>New Date:</strong> {newDate:dddd, MMMM dd, yyyy}</p>
        <p>You can view the details in your account.</p>";

            string htmlBody = BuildHtmlEmail("Reschedule Approved", message, "#2563eb");

            SendEmail(toEmail, subject, htmlBody);
        }

        private void SendRejectionEmail(string toEmail, string serviceName, string reason)
        {
            if (string.IsNullOrWhiteSpace(toEmail)) return;

            string subject = "❌ Reschedule Rejected - RRC Management";

            string message = $@"
        <p>Hello,</p>
        <p>We regret to inform you that your reschedule request for 
        <strong>{serviceName}</strong> has been rejected.</p>
        <p><strong>Reason:</strong> {reason}</p>
        <p>If you have any questions, please contact our support team.</p>";

            string htmlBody = BuildHtmlEmail("Reschedule Rejected", message, "#dc2626");

            SendEmail(toEmail, subject, htmlBody);
        }


        private void SendEmail(string toEmail, string subject, string htmlBody)
        {
            using (var mail = new MailMessage())
            {
                // FROM email must be from your domain to avoid spam
                string fromEmail = ConfigurationManager.AppSettings["emailFrom"];
                mail.From = new MailAddress(fromEmail, "RRC Management");

                mail.To.Add(toEmail);
                mail.Subject = subject;
                mail.Body = htmlBody;
                mail.IsBodyHtml = true; // <-- Enable HTML design

                // Additional headers for deliverability
                mail.ReplyToList.Add(new MailAddress(fromEmail));
                mail.Priority = MailPriority.High;

                using (var smtp = new SmtpClient())
                {
                    smtp.Send(mail);
                }
            }
        }

    }
}
