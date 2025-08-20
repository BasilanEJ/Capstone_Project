using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net.Configuration;   // <-- for SmtpSection
using System.Net.Mail;

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
            // 🔐 Deny SuperAdmin & Inspector per your pattern
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
                // cache edit/delete permissions in ViewState for quick checks
                ViewState["CanEdit"] = HasPermission(userId, "ManageSupplier", "CanEdit");
                ViewState["CanDelete"] = HasPermission(userId, "ManageSupplier", "CanDelete");

                LoadSuppliers();
            }
        }

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
                    lblMessage.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠ Error loading suppliers: " + ex.Message;
            }
        }

        /// <summary>
        /// Handles GridView commands for Edit / Archive (and optionally OpenEmailForm if you still raise it server-side).
        /// If you're opening the email modal purely on the client, OpenEmailForm won't reach here.
        /// </summary>
        protected void gvSuppliers_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            try
            {
                if (e.CommandName == "OpenEmailForm")
                {
                    // If you still raise this command server-side, populate the panel here:
                    string email = Convert.ToString(e.CommandArgument);
                    hfSupplierEmail.Value = email;
                    hfSupplierName.Value = ""; // optional if you also pass name
                    lblSendTo.Text = "Sending to: " + email;
                    pnlSendEmail.Visible = true;
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
                lblMessage.Text = "⚠ Command error: " + ex.Message;
            }
        }

        /// <summary>
        /// Send email using Web.config <system.net><mailSettings><smtp> (SmtpSection).
        /// Expects hfSupplierEmail/hfSupplierName set from client-side before postback.
        /// </summary>
        protected void btnSendEmail_Click(object sender, EventArgs e)
        {
            string toEmail = hfSupplierEmail.Value?.Trim();
            string toName = hfSupplierName.Value?.Trim();

            if (string.IsNullOrWhiteSpace(toEmail))
            {
                lblMessage.Text = "⚠ No recipient selected.";
                return;
            }

            string subject = txtSubject.Text.Trim();
            string body = txtMessageBody.Text.Trim();

            if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(body))
            {
                lblMessage.Text = "⚠ Please enter both subject and message.";
                return;
            }

            try
            {
                // Read SMTP config directly from Web.config <system.net><mailSettings>
                var smtpSection = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
                if (smtpSection == null)
                {
                    lblMessage.Text = "⚠ SMTP configuration not found in Web.config.";
                    return;
                }

                using (var mail = new MailMessage())
                {
                    // From address from <smtp from="...">
                    mail.From = new MailAddress(smtpSection.From, "RRC Management System");
                    mail.To.Add(toEmail);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = false;

                    using (var smtp = new SmtpClient())
                    {
                        // NOTE: SmtpClient auto-binds to <mailSettings>. These are optional if you want to be explicit.
                        // smtp.Host = smtpSection.Network.Host;
                        // smtp.Port = smtpSection.Network.Port;
                        // smtp.EnableSsl = smtpSection.Network.EnableSsl;
                        // smtp.Credentials = new System.Net.NetworkCredential(smtpSection.Network.UserName, smtpSection.Network.Password);

                        smtp.Send(mail);
                    }
                }

                lblMessage.ForeColor = System.Drawing.Color.Green;
                lblMessage.Text = $"✅ Email sent to {(string.IsNullOrEmpty(toName) ? toEmail : toName + " <" + toEmail + ">")}";

                // Clear and hide panel
                txtSubject.Text = "";
                txtMessageBody.Text = "";
                hfSupplierEmail.Value = "";
                hfSupplierName.Value = "";
                pnlSendEmail.Visible = false;
            }
            catch (Exception ex)
            {
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblMessage.Text = "⚠ Error sending email: " + ex.Message;
            }
        }

        protected void btnCancelEmail_Click(object sender, EventArgs e)
        {
            // Just hide panel & clear fields
            pnlSendEmail.Visible = false;
            txtSubject.Text = "";
            txtMessageBody.Text = "";
            hfSupplierEmail.Value = "";
            hfSupplierName.Value = "";
            lblMessage.Text = "❌ Email sending cancelled.";
        }

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

                lblMessage.Text = (archived == 1)
                    ? "✅ Supplier archived successfully."
                    : "⚠ Supplier not found or already archived.";
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠ Error archiving supplier: " + ex.Message;
            }
        }
    }
}
