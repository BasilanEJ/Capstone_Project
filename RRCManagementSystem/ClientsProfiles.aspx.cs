using RRCManagementSystem.Helpers;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;              // <-- Required for sending email
using System.Net.Mail;

namespace RRCManagementSystem
{
    public partial class ClientsProfiles : System.Web.UI.Page
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

            var role = Session["Role"].ToString();

            // 🔐 Block SuperAdmin and Inspector
            if (role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
                role.Equals("Inspector", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            // ✅ Check CanEdit permission for ManageClient
            int userId = Convert.ToInt32(Session["UserID"]);
            bool canEdit = HasEditPermission(userId, "ManageClient");
            ViewState["CanEditClients"] = canEdit;

            if (!IsPostBack)
            {
                LoadApprovedClients();
            }
        }

        #region Permissions
        private bool HasEditPermission(int userId, string moduleName)
        {
            try
            {
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAdminPermission_CanEdit", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                    cmd.Parameters.Add("@ModuleName", SqlDbType.NVarChar, 100).Value = moduleName;

                    con.Open();
                    object result = cmd.ExecuteScalar();

                    // SP returns 1 for CanEdit, 0 otherwise
                    return result != null && result != DBNull.Value && Convert.ToInt32(result) == 1;
                }
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Load Clients
        private void LoadApprovedClients()
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spClients_ListApproved", conn))
                using (var da = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    var dt = new DataTable();
                    da.Fill(dt);

                    // ✅ Add decrypted columns for GridView
                    if (!dt.Columns.Contains("Email")) dt.Columns.Add("Email", typeof(string));
                    if (!dt.Columns.Contains("ContactNumber")) dt.Columns.Add("ContactNumber", typeof(string));
                    if (!dt.Columns.Contains("City")) dt.Columns.Add("City", typeof(string));
                    if (!dt.Columns.Contains("Country")) dt.Columns.Add("Country", typeof(string));

                    // ✅ Decrypt each row
                    foreach (DataRow row in dt.Rows)
                    {
                        // Email
                        if (row["EmailEnc"] != DBNull.Value)
                            row["Email"] = AESHelper.DecryptEmail(row["EmailEnc"].ToString());

                        // Contact
                        if (row["ContactEnc"] != DBNull.Value)
                            row["ContactNumber"] = AESHelper.DecryptField(row["ContactEnc"].ToString());

                        // City
                        if (row["CityEnc"] != DBNull.Value)
                            row["City"] = AESHelper.DecryptField(row["CityEnc"].ToString());

                        // Country
                        if (row["CountryEnc"] != DBNull.Value)
                            row["Country"] = AESHelper.DecryptField(row["CountryEnc"].ToString());
                    }

                    // ✅ Bind the decrypted DataTable to the GridView
                    gvClients.DataSource = dt;
                    gvClients.DataBind();
                }
            }
            catch (Exception ex)
            {
                ShowSweetAlert("Error", "Failed to load clients. " + ex.Message, "error");
            }
        }

        #endregion

        #region GridView Events
        protected void gvClients_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvClients.PageIndex = e.NewPageIndex;
            LoadApprovedClients();
        }

        protected void gvClients_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            bool canEdit = ViewState["CanEditClients"] != null && (bool)ViewState["CanEditClients"];

            var btnArchive = e.Row.FindControl("btnArchive") as Button;
            if (btnArchive != null)
            {
                btnArchive.Enabled = canEdit;
                if (!canEdit)
                {
                    btnArchive.ToolTip = "You do not have permission to archive clients.";
                    btnArchive.CssClass += " disabled"; // optional Bootstrap styling
                }
            }
        }

        protected void gvClients_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!int.TryParse(e.CommandArgument.ToString(), out int clientId))
            {
                ShowSweetAlert("Error", "Invalid Client ID.", "error");
                return;
            }

            switch (e.CommandName)
            {
                case "ViewProfile":
                    Response.Redirect($"ViewClientProfile.aspx?ClientID={clientId}");
                    break;

                case "ArchiveClient":
                    bool canEdit = ViewState["CanEditClients"] != null && (bool)ViewState["CanEditClients"];
                    if (!canEdit)
                    {
                        ShowSweetAlert("No Permission", "You do not have permission to archive clients.", "warning");
                        return;
                    }
                    ArchiveClient(clientId);
                    break;

                case "ResendEmail":
                    ResendResetTokenForClient(clientId);
                    break;
            }
        }


        private void ResendResetTokenForClient(int clientId)
        {
            try
            {
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("SELECT EmailEnc FROM Clients WHERE ClientID = @ClientID", con))
                {
                    cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                    con.Open();

                    var encryptedEmail = cmd.ExecuteScalar() as string;

                    if (string.IsNullOrEmpty(encryptedEmail))
                    {
                        ShowSweetAlert("Error", "Client email not found.", "error");
                        return;
                    }

                    // Decrypt the email
                    string email = AESHelper.DecryptEmail(encryptedEmail);

                    // Generate new token
                    string newToken = Guid.NewGuid().ToString();
                    DateTime newExpiry = DateTime.Now.AddHours(1); // ✅ Match the CreateCustomerAccount logic

                    // Update token in DB
                    using (var updateCmd = new SqlCommand("dbo.spClient_UpdateResetToken", con))
                    {
                        updateCmd.CommandType = CommandType.StoredProcedure;
                        updateCmd.Parameters.Add("@EmailHash", SqlDbType.Char, 64).Value = AESHelper.ComputeSHA256WithPepper(email);
                        updateCmd.Parameters.Add("@NewToken", SqlDbType.NVarChar, 200).Value = newToken;
                        updateCmd.Parameters.Add("@NewExpiry", SqlDbType.DateTime).Value = newExpiry;

                        int rowsAffected = Convert.ToInt32(updateCmd.ExecuteScalar() ?? 0);

                        if (rowsAffected == 0)
                        {
                            ShowSweetAlert("Error", "Failed to update reset token.", "error");
                            return;
                        }
                    }

                    // Send reset email
                    bool emailSent = SendResetEmail(email, newToken);

                    ShowSweetAlert(
                        emailSent ? "Success" : "Partial Success",
                        emailSent
                            ? "A new password reset email has been sent to the client."
                            : "Token updated but failed to send email.",
                        emailSent ? "success" : "warning"
                    );
                }
            }
            catch (Exception ex)
            {
                ShowSweetAlert("Error", "An error occurred: " + ex.Message, "error");
            }
        }

        private bool SendResetEmail(string toEmail, string token)
        {
            try
            {
                // Fetch credentials from Web.config
                string fromEmail = ConfigurationManager.AppSettings["emailFrom"] ?? "rrctermiteandpestcontrol@gmail.com";
                string appPassword = ConfigurationManager.AppSettings["emailPassword"] ?? "";

                // Generate reset link
                string resetLink = $"https://rrcmngmnt.com/ResetPassword.aspx?type=client&token={token}";
                string subject = "Set Your Password - RRC Management System";

                // Professional, table-based HTML email with inline styling
                string body = $@"
<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>Set Your Password - RRC Management System</title>
    <style type=""text/css"">
        body {{ 
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif, 
            'Apple Color Emoji', 'Segoe UI Emoji', 'Segoe UI Symbol'; 
            margin: 0; 
            padding: 0; 
            background-color: #f4f7fa; 
        }}
        table {{ border-collapse: collapse; }}
        a {{ text-decoration: none; }}
        .button {{
            background-color: #2b6cb0;
            color: #ffffff;
            font-size: 16px;
            font-weight: bold;
            padding: 12px 24px;
            border-radius: 6px;
            display: inline-block;
        }}
        .link-text {{ color: #2b6cb0; text-decoration: underline; word-break: break-all; }}
        .content-box {{
            background-color: #ffffff;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
            padding: 30px;
        }}
        .footer {{
            font-size: 12px;
            color: #718096;
            margin-top: 25px;
            border-top: 1px solid #e2e8f0;
            padding-top: 20px;
            text-align: center;
        }}
    </style>
</head>
<body style=""margin: 0; padding: 0; background-color: #f4f7fa;"">
    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
        <tr>
            <td align=""center"" style=""padding: 20px;"">
                <table role=""presentation"" width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""max-width: 600px; width: 100%;"">
                    <tr>
                        <td align=""center"" style=""padding-bottom: 20px;"">
                            <h1 style=""color: #2b6cb0; font-size: 28px; margin: 0;"">RRC Management System</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 30px; background-color: #ffffff; border-radius: 8px; box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);"">
                            <h2 style=""color: #2d3748; font-size: 24px; margin: 0 0 15px;"">Welcome to RRC Management System</h2>
                            <p style=""color: #4a5568; font-size: 16px; line-height: 1.6; margin: 0 0 15px;"">Hello,</p>
                            <p style=""color: #4a5568; font-size: 16px; line-height: 1.6; margin: 0 0 15px;"">
                                You have been registered as a <strong>Client</strong>. To get started, you'll need to set your password.
                            </p>
                            <p style=""color: #4a5568; font-size: 16px; line-height: 1.6; margin: 0 0 30px;"">
                                Please click the button below to continue:
                            </p>
                            <p align=""center"" style=""margin: 0; text-align: center;"">
                                <a href=""{resetLink}"" class=""button"">Set Password</a>
                            </p>
                            <p style=""color: #4a5568; font-size: 14px; line-height: 1.6; margin: 30px 0 15px; text-align: center;"">
                                If the button does not work, you can copy and paste the following URL into your browser:
                            </p>
                            <p style=""text-align: center; font-size: 14px; margin: 0;"">
                                <a href=""{resetLink}"" class=""link-text"">{resetLink}</a>
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td align=""center"" style=""padding-top: 20px;"">
                            <p style=""font-size: 12px; color: #718096; margin: 0; text-align: center;"">
                                This link will expire in 1 hour. If you did not request this, you can safely ignore this email.
                                <br/><br/>
                                &copy; {DateTime.Now.Year} RRC Management System. All rights reserved.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";

                // Ensure TLS 1.2 is enforced for secure email transmission
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // Send email via Gmail SMTP
                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(fromEmail, "RRC Management System");
                    mail.To.Add(toEmail);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;

                    using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new NetworkCredential(fromEmail, appPassword);
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                // Debugging
                System.Diagnostics.Debug.WriteLine("Email error: " + ex.Message);
                return false;
            }
        }



        #endregion

        #region Archive Client
        private void ArchiveClient(int clientId)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spClient_UpdateStatus", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = "Inactive";

                try
                {
                    conn.Open();
                    var rows = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);

                    LoadApprovedClients(); // Refresh grid
                    ShowSweetAlert(rows > 0 ? "Archived" : "Not Found",
                                   rows > 0 ? "Client has been archived successfully." : "Client not found.",
                                   rows > 0 ? "success" : "warning");
                }
                catch (Exception ex)
                {
                    ShowSweetAlert("Error", "Failed to archive client. " + ex.Message, "error");
                }
            }
        }
        #endregion

        #region SweetAlert Helper
        private void ShowSweetAlert(string title, string message, string icon)
        {
            string script = $@"
<script>
    Swal.fire({{
        title: '{title}',
        text: '{message.Replace("'", "\\'")}',
        icon: '{icon}',
        confirmButtonColor: '#007bff'
    }}); 
</script>";
            ScriptManager.RegisterStartupScript(this, GetType(), "SweetAlert" + Guid.NewGuid(), script, false);
        }
        #endregion
    }
}
