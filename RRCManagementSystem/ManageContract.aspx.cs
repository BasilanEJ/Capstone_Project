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
    public partial class ManageContract : System.Web.UI.Page
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

            // Keep your original restriction
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadClients();
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

                ddlClients.DataSource = dt;
                ddlClients.DataTextField = "FullName";   // <— bind to FullName
                ddlClients.DataValueField = "ClientID";
                ddlClients.DataBind();

                // friendly first item
                ddlClients.Items.Insert(0, new ListItem("-- Select Client --", ""));
            }
        }

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            lblMessage.CssClass = "form-text text-center mb-3";
            lblMessage.Text = "";

            // session re-check
            if (Session["UserID"] == null)
            {
                Fail("❌ You must be logged in to upload a contract.");
                return;
            }

            // file checks
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

                // Save encrypted PDF under a virtual path that maps inside the app
                string fileName = Guid.NewGuid().ToString("N") + ".pdf";
                string relativePath = "~/EncryptedContracts/" + fileName;     // path saved in DB
                string physicalPath = Server.MapPath(relativePath);           // where we store the file

                // ensure folder exists
                string dir = Path.GetDirectoryName(physicalPath);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

                // encrypt and save
                using (var ms = new MemoryStream())
                {
                    fuContract.PostedFile.InputStream.CopyTo(ms);
                    byte[] encrypted = AESHelper.Encrypt(ms.ToArray());
                    File.WriteAllBytes(physicalPath, encrypted);
                }

                // DB insert via SP
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spClientContract_Insert", conn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                    cmd.Parameters.Add("@FilePath", SqlDbType.NVarChar, 260).Value = relativePath;
                    cmd.Parameters.Add("@StartDate", SqlDbType.DateTime).Value = startDate;
                    cmd.Parameters.Add("@EndDate", SqlDbType.DateTime).Value = endDate;
                    cmd.Parameters.Add("@UploadedBy", SqlDbType.Int).Value = uploadedBy;
                    cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar).Value = (object)remarks ?? DBNull.Value;

                    conn.Open();
                    cmd.ExecuteScalar(); // (Optionally returns ContractID)
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
    }
}
