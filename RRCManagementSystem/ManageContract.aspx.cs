using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using RRCManagementSystem.Helpers;

namespace RRCManagementSystem
{
    public partial class ManageContract : Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Require login
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string role = Session["Role"].ToString();

            if (role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
                role.Equals("Inspector", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            // Store permission in ViewState
            ViewState["CanEditContract"] = HasEditPermission(userId, "ManageClient");

            if (!IsPostBack)
            {
                LoadClients();

                bool canEdit = ViewState["CanEditContract"] != null && (bool)ViewState["CanEditContract"];
                btnUpload.Enabled = canEdit;

                if (!canEdit)
                {
                    lblMessage.Text = "⚠️ You do not have permission to upload contracts.";
                    lblMessage.CssClass = "form-text text-center mb-3 text-warning";
                }
            }
        }

        private void LoadClients()
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spClients_ListApproved", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                var dt = new DataTable();
                da.Fill(dt);

                // 🔹 Add a FullName column for dropdown display
                if (!dt.Columns.Contains("FullName"))
                    dt.Columns.Add("FullName", typeof(string));

                foreach (DataRow row in dt.Rows)
                {
                    string lastName = row["LastName"]?.ToString() ?? "";
                    string firstName = row["FirstName"]?.ToString() ?? "";
                    string middleName = row["MiddleName"]?.ToString() ?? "";

                    // Combine for dropdown
                    row["FullName"] = $"{lastName}, {firstName} {middleName}".Trim();

                    // 🔹 Decrypt encrypted columns for internal use
                    if (row["EmailEnc"] != DBNull.Value)
                        row["EmailEnc"] = AESHelper.DecryptEmail(row["EmailEnc"].ToString());

                    if (row["ContactEnc"] != DBNull.Value)
                        row["ContactEnc"] = AESHelper.DecryptField(row["ContactEnc"].ToString());

                    if (row["CityEnc"] != DBNull.Value)
                        row["CityEnc"] = AESHelper.DecryptField(row["CityEnc"].ToString());

                    if (row["CountryEnc"] != DBNull.Value)
                        row["CountryEnc"] = AESHelper.DecryptField(row["CountryEnc"].ToString());
                }

                ddlClients.DataSource = dt;
                ddlClients.DataTextField = "FullName";
                ddlClients.DataValueField = "ClientID";
                ddlClients.DataBind();

                ddlClients.Items.Insert(0, new ListItem("-- Select Client --", ""));
            }
        }


        protected void btnUpload_Click(object sender, EventArgs e)
        {
            bool canEdit = ViewState["CanEditContract"] != null && (bool)ViewState["CanEditContract"];
            if (!canEdit)
            {
                ShowSweetAlert("No Permission", "You do not have permission to upload contracts.", "warning");
                return;
            }

            lblMessage.CssClass = "form-text text-center mb-3";
            lblMessage.Text = "";

            // Validation
            if (!fuContract.HasFile || Path.GetExtension(fuContract.FileName).ToLowerInvariant() != ".pdf")
            {
                Fail("❌ Please upload a valid PDF file.");
                return;
            }

            if (string.IsNullOrEmpty(ddlClients.SelectedValue))
            {
                Fail("❌ Please select a client.");
                return;
            }

            if (!DateTime.TryParse(txtStartDate.Text, out var startDate) ||
                !DateTime.TryParse(txtEndDate.Text, out var endDate))
            {
                Fail("❌ Please enter valid Start/End dates.");
                return;
            }

            if (endDate < startDate)
            {
                Fail("❌ End Date cannot be earlier than Start Date.");
                return;
            }

            try
            {
                int clientId = Convert.ToInt32(ddlClients.SelectedValue);
                int uploadedBy = Convert.ToInt32(Session["UserID"]);
                string remarks = (txtRemarks.Text ?? string.Empty).Trim();

                string fileName = Guid.NewGuid().ToString("N") + ".pdf";
                string relativePath = "~/EncryptedContracts/" + fileName;
                string physicalPath = Server.MapPath(relativePath);

                string dir = Path.GetDirectoryName(physicalPath);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

                using (var ms = new MemoryStream())
                {
                    fuContract.PostedFile.InputStream.CopyTo(ms);
                    byte[] encrypted = AESHelper.Encrypt(ms.ToArray());
                    File.WriteAllBytes(physicalPath, encrypted);
                }

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spClientContract_Insert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                    cmd.Parameters.Add("@FilePath", SqlDbType.NVarChar, 260).Value = relativePath;
                    cmd.Parameters.Add("@StartDate", SqlDbType.DateTime).Value = startDate;
                    cmd.Parameters.Add("@EndDate", SqlDbType.DateTime).Value = endDate;
                    cmd.Parameters.Add("@UploadedBy", SqlDbType.Int).Value = uploadedBy;
                    cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar).Value = (object)remarks ?? DBNull.Value;

                    conn.Open();
                    cmd.ExecuteScalar();
                }

                lblMessage.Text = "✅ Contract uploaded and encrypted successfully!";
                lblMessage.CssClass = "form-text text-center mb-3 text-success";
                ClearForm();
            }
            catch (Exception ex)
            {
                Fail("❌ Error: " + ex.Message);
            }
        }

        private void ClearForm()
        {
            ddlClients.SelectedIndex = 0;
            txtStartDate.Text = "";
            txtEndDate.Text = "";
            txtRemarks.Text = "";
        }

        private void Fail(string msg)
        {
            lblMessage.Text = msg;
            lblMessage.CssClass = "form-text text-center mb-3 text-danger";
        }

        private bool HasEditPermission(int userId, string moduleName)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAdminPermission_CanEdit", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                    cmd.Parameters.Add("@ModuleName", SqlDbType.NVarChar, 100).Value = moduleName;

                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null && Convert.ToInt32(result) == 1;
                }
            }
            catch
            {
                return false;
            }
        }

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
    }
}
