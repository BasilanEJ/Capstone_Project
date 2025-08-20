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

            string role = Session["Role"].ToString();
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            // (Optional) enforce view permission
            // if (!HasViewPermission(userId, "ManageBooking")) { ... }

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
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        gvRescheduleRequests.DataSource = dt;
                        gvRescheduleRequests.DataBind();
                    }
                }
                lblMessage.Text = "";
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠️ Error loading requests: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void gvRescheduleRequests_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            // show the *requested* new date to admins
            var lblNewDate = (Label)e.Row.FindControl("lblNewDate");
            if (lblNewDate != null)
            {
                var requested = DataBinder.Eval(e.Row.DataItem, "RequestedDate");
                if (requested != null && requested != DBNull.Value)
                    lblNewDate.Text = Convert.ToDateTime(requested).ToString("yyyy-MM-dd");
            }
        }

        protected void gvRescheduleRequests_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int requestId;
            if (!int.TryParse(e.CommandArgument?.ToString(), out requestId))
            {
                lblMessage.Text = "⚠️ Invalid Request ID.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            if (e.CommandName == "Approve")
            {
                try
                {
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
                                // values returned for email
                                string email = r["Email"]?.ToString();
                                string service = r["ServiceNames"]?.ToString() ?? "(No Service)";
                                DateTime newDate = Convert.ToDateTime(r["NewScheduledDate"]);

                                SendApprovalEmail(email, service, newDate);

                                ScriptManager.RegisterStartupScript(this, GetType(),
                                    "showSuccess",
                                    "Swal.fire('✅ Approved', 'The request has been approved and rescheduled.', 'success');",
                                    true);
                            }
                        }
                    }
                    LoadRescheduleRequests();
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "❌ Approve failed: " + ex.Message;
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
            }
            else if (e.CommandName == "Reject")
            {
                string reason = hfRejectReason.Value.Trim();
                if (string.IsNullOrWhiteSpace(reason))
                {
                    lblMessage.Text = "❌ Rejection reason required.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                try
                {
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
                                string email = r["Email"]?.ToString();
                                string service = r["ServiceNames"]?.ToString() ?? "(No Service)";
                                SendRejectionEmail(email, service, reason);

                                ScriptManager.RegisterStartupScript(this, GetType(),
                                    "showRejected",
                                    "Swal.fire('❌ Rejected', 'The request has been rejected.', 'info');",
                                    true);
                            }
                        }
                    }
                    LoadRescheduleRequests();
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "❌ Reject failed: " + ex.Message;
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
            }
        }

        private void SendApprovalEmail(string toEmail, string serviceName, DateTime newDate)
        {
            string subject = "✅ Reschedule Approved - RRC Management";
            string body = $"Hello,\n\nYour reschedule request for '{serviceName}' has been approved.\nNew date: {newDate:yyyy-MM-dd}.\n\nThank you,\nRRC Management Team";
            SendEmail(toEmail, subject, body);
        }

        private void SendRejectionEmail(string toEmail, string serviceName, string reason)
        {
            string subject = "❌ Reschedule Rejected - RRC Management";
            string body = $"Hello,\n\nYour reschedule request for '{serviceName}' has been rejected.\nReason: {reason}\n\nThank you,\nRRC Management Team";
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
