using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net.Configuration;
using System.Net.Mail;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class ViewSupplier : System.Web.UI.Page
    {
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 🔐 Require login
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string role = Convert.ToString(Session["Role"]);

            // 🔐 Deny SuperAdmin & Inspector
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            // 🔐 Must have CanView for ManageSupplier
            if (!HasPermission(userId, "ManageSupplier", "CanView"))
            {
                Response.Redirect("~/Unauthorized.aspx");
                return;
            }

            if (!IsPostBack)
            {
                // Cache edit/delete permissions in ViewState
                ViewState["CanEdit"] = HasPermission(userId, "ManageSupplier", "CanEdit");
                ViewState["CanDelete"] = HasPermission(userId, "ManageSupplier", "CanDelete");

                LoadSuppliers();
            }
        }

        /// <summary>
        /// Checks if the current user has permission for a specific action on a module.
        /// </summary>
        private bool HasPermission(int adminId, string moduleName, string which)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAdminPermission_Check", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", adminId);
                    cmd.Parameters.AddWithValue("@ModuleName", moduleName);
                    cmd.Parameters.AddWithValue("@Permission", which);

                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null && result != DBNull.Value && Convert.ToBoolean(result);
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Loads active suppliers and binds them to the GridView.
        /// </summary>
        private void LoadSuppliers()
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spSupplier_ListActive", conn))
                using (var da = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    var dt = new DataTable();
                    da.Fill(dt);

                    gvSuppliers.DataSource = dt;
                    gvSuppliers.DataBind();
                }
            }
            catch (Exception ex)
            {
                // Display error using SweetAlert
                ClientScript.RegisterStartupScript(this.GetType(), "loadError",
                    $"showAlert('Error!', 'Error loading suppliers: {ex.Message.Replace("'", "\\'")}', 'error');", true);
            }
        }

        /// <summary>
        /// Handles GridView commands for Edit, Archive, or optional Email.
        /// </summary>
        protected void gvSuppliers_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            try
            {
                if (e.CommandName == "OpenEmailForm")
                {
                    // This command is handled by the client-side JavaScript now
                    return;
                }

                if (!int.TryParse(Convert.ToString(e.CommandArgument), out int supplierID))
                    return;

                if (e.CommandName == "EditSupplier" && Convert.ToBoolean(ViewState["CanEdit"]))
                {
                    Response.Redirect($"EditSupplier.aspx?SupplierID={supplierID}");
                    return;
                }

                if (e.CommandName == "ArchiveSupplier" && Convert.ToBoolean(ViewState["CanDelete"]))
                {
                    ArchiveSupplier(supplierID);
                    LoadSuppliers();
                }
            }
            catch (Exception ex)
            {
                // Display error using SweetAlert
                ClientScript.RegisterStartupScript(this.GetType(), "commandError",
                    $"showAlert('Error!', 'Command error: {ex.Message.Replace("'", "\\'")}', 'error');", true);
            }
        }

        /// <summary>
        /// Sends email to the selected supplier using SMTP settings from Web.config.
        /// </summary>
        protected void btnSendEmail_Click(object sender, EventArgs e)
        {
            string toEmail = hfSupplierEmail.Value?.Trim();
            string toName = hfSupplierName.Value?.Trim();

            if (string.IsNullOrWhiteSpace(toEmail))
            {
                ClientScript.RegisterStartupScript(this.GetType(), "emailError",
                    "showAlert('Error!', 'No recipient selected.', 'error');", true);
                return;
            }

            string subject = txtSubject.Text.Trim();
            string body = txtMessageBody.Text.Trim();

            if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(body))
            {
                ClientScript.RegisterStartupScript(this.GetType(), "emailError",
                    "showAlert('Error!', 'Please enter both subject and message.', 'error');", true);
                return;
            }

            try
            {
                // Load SMTP configuration from Web.config
                var smtpSection = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
                if (smtpSection == null)
                {
                    ClientScript.RegisterStartupScript(this.GetType(), "emailError",
                        "showAlert('Error!', 'SMTP configuration not found in Web.config.', 'error');", true);
                    return;
                }

                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(smtpSection.From, "RRC Management System");
                    mail.To.Add(toEmail);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = false;

                    using (var smtp = new SmtpClient())
                    {
                        // The following lines are commented out in your original code,
                        // so I will keep them commented out here.
                        // smtp.Host = smtpSection.Network.Host;
                        // smtp.Port = smtpSection.Network.Port;
                        // smtp.EnableSsl = smtpSection.Network.EnableSsl;
                        // smtp.Credentials = new System.Net.NetworkCredential(smtpSection.Network.UserName, smtpSection.Network.Password);

                        smtp.Send(mail);
                    }
                }

                // Display success message using SweetAlert
                string recipientDisplay = string.IsNullOrEmpty(toName) ? toEmail : $"{toName} <{toEmail}>";
                ClientScript.RegisterStartupScript(this.GetType(), "emailSuccess",
                    $"showAlert('Success!', 'Email sent to {recipientDisplay}.', 'success');", true);

                // Clear form and hide panel
                txtSubject.Text = "";
                txtMessageBody.Text = "";
                hfSupplierEmail.Value = "";
                hfSupplierName.Value = "";
                pnlSendEmail.Visible = false;
            }
            catch (Exception ex)
            {
                // Display error using SweetAlert
                ClientScript.RegisterStartupScript(this.GetType(), "emailFail",
                    $"showAlert('Error!', 'Error sending email: {ex.Message.Replace("'", "\\'")}', 'error');", true);
            }
        }

        /// <summary>
        /// Cancels the email send operation and clears fields.
        /// This button now also handles the client-side confirmation.
        /// </summary>
        protected void btnCancelEmail_Click(object sender, EventArgs e)
        {
            pnlSendEmail.Visible = false;
            txtSubject.Text = "";
            txtMessageBody.Text = "";
            hfSupplierEmail.Value = "";
            hfSupplierName.Value = "";

            ClientScript.RegisterStartupScript(this.GetType(), "cancelEmail",
                "showAlert('Cancelled', 'Email sending cancelled.', 'info');", true);
        }

        /// <summary>
        /// Archives the selected supplier by calling stored procedure.
        /// </summary>
        private void ArchiveSupplier(int supplierID)
        {
            try
            {
                int archived = 0;
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spSupplier_Archive", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SupplierID", supplierID);

                    conn.Open();
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                            archived = Convert.ToInt32(rdr["Archived"]);
                    }
                }

                if (archived == 1)
                {
                    // Display success using SweetAlert
                    ClientScript.RegisterStartupScript(this.GetType(), "archiveSuccess",
                        "showAlert('Success!', 'Supplier archived successfully.', 'success');", true);
                }
                else
                {
                    // Display warning using SweetAlert
                    ClientScript.RegisterStartupScript(this.GetType(), "archiveWarning",
                        "showAlert('Warning', 'Supplier not found or already archived.', 'warning');", true);
                }
            }
            catch (Exception ex)
            {
                // Display error using SweetAlert
                ClientScript.RegisterStartupScript(this.GetType(), "archiveError",
                    $"showAlert('Error!', 'Error archiving supplier: {ex.Message.Replace("'", "\\'")}', 'error');", true);
            }
        }
    }
}
