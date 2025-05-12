using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net.Mail;

namespace RRCManagementSystem
{
    public partial class ClientApproval : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["AdminID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int adminId = Convert.ToInt32(Session["AdminID"]);

            if (!HasPermission(adminId, "ClientApproval"))
            {
                Response.Redirect("~/Unauthorized.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadPendingClients();
            }
        }

        private bool HasPermission(int userId, string moduleName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT CanView FROM AdminPermissions 
                                 WHERE UserID = @UserID AND ModuleName = @ModuleName";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@ModuleName", moduleName);

                    try
                    {
                        conn.Open();
                        object result = cmd.ExecuteScalar();
                        return result != null && Convert.ToBoolean(result);
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        }

        private void LoadPendingClients()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT ClientID, Name, Email, ContactNumber, Status, CreatedAt FROM Clients WHERE Status = 'Pending'";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvClients.DataSource = dt;
                gvClients.DataBind();
            }
        }

        protected void gvClients_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            int clientId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "Approve")
            {
                HandleClientDecision(clientId, "Approved");
            }
            else if (e.CommandName == "Reject") // 🟢 Matches your ASPX button
            {
                HandleClientDecision(clientId, "Declined"); // 🟢 Matches CHECK constraint
            }

            LoadPendingClients();
        }

        private void HandleClientDecision(int clientId, string status)
        {
            string clientEmail = "";
            string clientName = "";
            string performedBy = Session["AdminEmail"]?.ToString() ?? "System";
            int adminId = Convert.ToInt32(Session["AdminID"]);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Get email and name
                string selectQuery = "SELECT Email, Name FROM Clients WHERE ClientID = @ClientID";
                using (SqlCommand cmd = new SqlCommand(selectQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@ClientID", clientId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            clientEmail = reader["Email"].ToString();
                            clientName = reader["Name"].ToString();
                        }
                    }
                }

                // Update client status
                string updateQuery = "UPDATE Clients SET Status = @Status, ApprovedAt = GETDATE() WHERE ClientID = @ClientID";
                using (SqlCommand cmdUpdate = new SqlCommand(updateQuery, conn))
                {
                    cmdUpdate.Parameters.AddWithValue("@Status", status);
                    cmdUpdate.Parameters.AddWithValue("@ClientID", clientId);
                    cmdUpdate.ExecuteNonQuery();
                }

                // Insert client history
                string insertHistory = @"
                    INSERT INTO ClientHistory (ClientID, ActionTaken, PerformedBy, Remarks)
                    VALUES (@ClientID, @ActionTaken, @PerformedBy, @Remarks)";
                using (SqlCommand cmdHistory = new SqlCommand(insertHistory, conn))
                {
                    cmdHistory.Parameters.AddWithValue("@ClientID", clientId);
                    cmdHistory.Parameters.AddWithValue("@ActionTaken", status);
                    cmdHistory.Parameters.AddWithValue("@PerformedBy", performedBy);
                    cmdHistory.Parameters.AddWithValue("@Remarks", $"Client was {status.ToLower()} via ClientApproval");
                    cmdHistory.ExecuteNonQuery();
                }

                // Audit log
                AddAuditLog(adminId, $"Client '{clientName}' (ID: {clientId}) was {status.ToLower()}.");
            }

            if (!string.IsNullOrEmpty(clientEmail))
            {
                SendStatusEmail(clientEmail, clientName, status);
            }

            ClientScript.RegisterStartupScript(this.GetType(), "alert", $"alert('Client has been {status.ToLower()} and notified via email.');", true);
        }

        private void SendStatusEmail(string toEmail, string clientName, string status)
        {
            string subject = $"RRC Termite & Pest Control - Account {status}";
            string body = "";

            if (status == "Approved")
            {
                body = $@"
                    <p>Dear {clientName},</p>
                    <p>We are pleased to inform you that your account has been <strong>approved</strong>.</p>
                    <p>You may now log in and use our services.</p>
                    <p>Thank you,<br/>RRC Termite & Pest Control Team</p>";
            }
            else if (status == "Declined")
            {
                body = $@"
                    <p>Dear {clientName},</p>
                    <p>We regret to inform you that your account has been <strong>rejected</strong> after review.</p>
                    <p>If you believe this is an error, please contact our support team.</p>
                    <p>Thank you,<br/>RRC Termite & Pest Control Team</p>";
            }

            try
            {
                MailMessage mail = new MailMessage
                {
                    To = { toEmail },
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                SmtpClient smtp = new SmtpClient(); // Config in web.config
                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", $"alert('Status updated, but email failed: {ex.Message}');", true);
            }
        }

        private void AddAuditLog(int? userID, string action)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO AuditLogs (AdminID, Action, Timestamp) VALUES (@AdminID, @Action, GETDATE())";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@AdminID", (object)userID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Action", action);

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch
                    {
                        // Optional: handle audit log failure
                    }
                }
            }
        }
    }
}
    