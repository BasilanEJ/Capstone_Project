using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
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
        /// Sends a customized HTML email to the selected supplier.
        /// </summary>
        private async Task SendSupplierEmail(string toEmail, string subject, string body)
        {
            try
            {
                // Fetch credentials from App.config or Web.config
                string fromEmail = ConfigurationManager.AppSettings["emailFrom"] ?? "defaultsender@example.com";
                string appPassword = ConfigurationManager.AppSettings["emailPassword"] ?? "";

                // A professional HTML template that wraps the user-provided message body
                string htmlBody = $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{subject}</title>
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif, 'Apple Color Emoji', 'Segoe UI Emoji', 'Segoe UI Symbol';
            margin: 0;
            padding: 0;
            background-color: #f0f4f8;
            color: #333;
        }}
        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            border-radius: 12px;
            overflow: hidden;
            box-shadow: 0 4px 15px rgba(0, 0, 0, 0.05);
        }}
        .header {{
            background-color: #17398A;
            color: #ffffff;
            padding: 24px;
            text-align: center;
        }}
        .header h1 {{
            margin: 0;
            font-size: 28px;
            font-weight: 600;
        }}
        .content {{
            padding: 32px 24px;
        }}
        .content h2 {{
            color: #17398A;
            font-size: 22px;
            margin-top: 0;
            margin-bottom: 20px;
        }}
        .content p {{
            font-size: 16px;
            line-height: 1.6;
            margin-bottom: 15px;
            white-space: pre-wrap;
        }}
        .footer {{
            background-color: #e6eef5;
            color: #666;
            text-align: center;
            padding: 24px;
            font-size: 12px;
            border-top: 1px solid #d4e0eb;
        }}
        .footer p {{
            margin: 0;
        }}
    </style>
</head>
<body>
    <div role=""article"" aria-label=""Email"" lang=""en"">
        <div class=""email-container"">
            <div class=""header"">
                <h1>RRC Management System</h1>
            </div>
            <div class=""content"">
                <h2>{subject}</h2>
                <p>{body}</p>
                <p>—<br>RRC Management System</p>
            </div>
            <div class=""footer"">
                <p>This email was sent from the RRC Management System.<br>
                &copy; 2024 RRC Management System. All rights reserved.</p>
            </div>
        </div>
    </div>
</body>
</html>";

                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(fromEmail, "RRC Management System");
                    mail.To.Add(toEmail);
                    mail.Subject = subject;
                    mail.Body = htmlBody;
                    mail.IsBodyHtml = true;

                    using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new NetworkCredential(fromEmail, appPassword);
                        smtp.EnableSsl = true;
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtp.Timeout = 20000;
                        await smtp.SendMailAsync(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log and rethrow or handle as needed
                throw new Exception("Error sending email: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Sends email to the selected supplier.
        /// </summary>
        protected async void btnSendEmail_Click(object sender, EventArgs e)
        {
            string toEmail = hfSupplierEmail.Value?.Trim();
            string toName = hfSupplierName.Value?.Trim();
            string subject = txtSubject.Text.Trim();
            string body = txtMessageBody.Text.Trim();

            if (string.IsNullOrWhiteSpace(toEmail) || string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(body))
            {
                ClientScript.RegisterStartupScript(this.GetType(), "emailError",
                    "showAlert('Error!', 'Please enter recipient, subject, and message.', 'error');", true);
                return;
            }

            try
            {
                await SendSupplierEmail(toEmail, subject, body);

                string recipientDisplay = string.IsNullOrEmpty(toName) ? toEmail : $"{toName} <{toEmail}>";
                ClientScript.RegisterStartupScript(this.GetType(), "emailSuccess",
                    $"showAlert('Success!', 'Email sent to {recipientDisplay}.', 'success');", true);
            }
            catch (Exception ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "emailFail",
                    $"showAlert('Error!', 'Error sending email: {ex.Message.Replace("'", "\\'")}', 'error');", true);
            }

            // Clear form fields
            txtSubject.Text = "";
            txtMessageBody.Text = "";
            hfSupplierEmail.Value = "";
            hfSupplierName.Value = "";
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
