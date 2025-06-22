using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;

namespace RRCManagementSystem
{
    public partial class ViewSupplier : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected string selectedSupplierEmail
        {
            get { return ViewState["SelectedSupplierEmail"] as string; }
            set { ViewState["SelectedSupplierEmail"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
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

            if (!HasPermission(userId, "ManageSupplier", "CanView"))
            {
                Response.Redirect("~/Unauthorized.aspx");
                return;
            }

            if (!IsPostBack)
            {
                ViewState["CanEdit"] = HasPermission(userId, "ManageSupplier", "CanEdit");
                ViewState["CanDelete"] = HasPermission(userId, "ManageSupplier", "CanDelete");

                LoadSuppliers();
            }
        }

        private bool HasPermission(int adminId, string moduleName, string column)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = $"SELECT {column} FROM AdminPermissions WHERE UserID = @UserID AND ModuleName = @ModuleName";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", adminId);
                    cmd.Parameters.AddWithValue("@ModuleName", moduleName);

                    try
                    {
                        conn.Open();
                        object result = cmd.ExecuteScalar();
                        return result != null && result != DBNull.Value && Convert.ToBoolean(result);
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        }

        private void LoadSuppliers()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT SupplierID, Name, CompanyName, BusinessType, Address, ContactNumber, Email, Status, CreatedAt
                                 FROM Supplier
                                 WHERE Status != 'Archived'";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    try
                    {
                        conn.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        // ✅ Add formatted ID for display
                        dt.Columns.Add("FormattedSupplierID", typeof(string));
                        foreach (DataRow row in dt.Rows)
                        {
                            int id = Convert.ToInt32(row["SupplierID"]);
                            row["FormattedSupplierID"] = "Supplier" + id.ToString("D3");
                        }

                        gvSuppliers.DataSource = dt;
                        gvSuppliers.DataBind();
                    }
                    catch (Exception ex)
                    {
                        lblMessage.Text = "⚠ Error loading suppliers: " + ex.Message;
                    }
                }
            }
        }

        protected void gvSuppliers_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            if (e.CommandName == "OpenEmailForm")
            {
                selectedSupplierEmail = e.CommandArgument.ToString();
                pnlSendEmail.Visible = true;
                lblSendTo.Text = $"Sending to: {selectedSupplierEmail}";
                lblMessage.Text = "";
            }
            else if (int.TryParse(e.CommandArgument.ToString(), out int supplierID))
            {
                if (e.CommandName == "EditSupplier" && Convert.ToBoolean(ViewState["CanEdit"]))
                {
                    Response.Redirect($"EditSupplier.aspx?SupplierID={supplierID}");
                }

                if (e.CommandName == "ArchiveSupplier" && Convert.ToBoolean(ViewState["CanDelete"]))
                {
                    ArchiveSupplier(supplierID);
                }
            }
            else
            {
                lblMessage.Text = "⚠ Unable to parse SupplierID.";
            }
        }

        protected void btnSendEmail_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedSupplierEmail))
            {
                lblMessage.Text = "⚠ No recipient selected.";
                return;
            }

            string subject = txtSubject.Text.Trim();
            string body = txtMessageBody.Text.Trim();

            if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(body))
            {
                lblMessage.Text = "⚠ Please enter both subject and message.";
                return;
            }

            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("rrctermiteandpestcontrol@gmail.com", "RRC Management System");
                mail.To.Add(selectedSupplierEmail);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = false;

                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("rrctermiteandpestcontrol@gmail.com", "shdyfyvpwvanxjiq"),
                    EnableSsl = true
                };

                smtp.Send(mail);

                lblMessage.Text = "✅ Email sent successfully!";
                pnlSendEmail.Visible = false;
                ClearEmailForm();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠ Error sending email: " + ex.Message;
            }
        }

        protected void btnCancelEmail_Click(object sender, EventArgs e)
        {
            pnlSendEmail.Visible = false;
            lblMessage.Text = "❌ Email sending cancelled.";
            ClearEmailForm();
        }

        private void ClearEmailForm()
        {
            txtSubject.Text = "";
            txtMessageBody.Text = "";
        }

        private void ArchiveSupplier(int supplierID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string archiveQuery = @"
                INSERT INTO Suppliers_Archive (SupplierID, Name, Address, ContactNumber, Email, Status, CompanyName, BusinessType, DeletedAt)
                SELECT 
                    SupplierID, Name, Address, ContactNumber, Email, Status, CompanyName, BusinessType, GETDATE()
                FROM Supplier
                WHERE SupplierID = @SupplierID;

                UPDATE Supplier SET Status = 'Archived' WHERE SupplierID = @SupplierID;";

                using (SqlCommand cmd = new SqlCommand(archiveQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@SupplierID", supplierID);

                    try
                    {
                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();

                        lblMessage.Text = rowsAffected > 0
                            ? "✅ Supplier archived successfully."
                            : "⚠ Supplier not found or already archived.";

                        LoadSuppliers();
                    }
                    catch (Exception ex)
                    {
                        lblMessage.Text = "⚠ Error archiving supplier: " + ex.Message;
                    }
                }
            }
        }
    }
}
