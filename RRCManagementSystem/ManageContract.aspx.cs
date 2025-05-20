using System;
using System.Configuration;
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
        private readonly string saveDirectory = @"D:\EncryptedContracts\"; // External path

        protected void Page_Load(object sender, EventArgs e)
        {
            // 🔐 Require login
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string role = Session["Role"].ToString();

            // 🔐 Deny access for SuperAdmin and Inspector
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            // 🔐 Optional: Check CanView permission (if applicable)
            // if (!HasPermission(userId, "ManageClients"))
            // {
            //     Response.Redirect("~/Unauthorized.aspx");
            //     return;
            // }

            if (!IsPostBack)
            {
                LoadClients();
            }
        }


        private void LoadClients()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT ClientID, Name FROM Clients WHERE Status = 'Approved'";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                ddlClients.DataSource = reader;
                ddlClients.DataTextField = "Name";
                ddlClients.DataValueField = "ClientID";
                ddlClients.DataBind();
                ddlClients.Items.Insert(0, new ListItem("-- Select Client --", ""));
            }
        }

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            lblMessage.CssClass = "message";

            if (Session["AdminID"] == null)
            {
                lblMessage.Text = "❌ You must be logged in to upload a contract.";
                lblMessage.CssClass += " error";
                return;
            }

            if (!fuContract.HasFile || Path.GetExtension(fuContract.FileName).ToLower() != ".pdf")
            {
                lblMessage.Text = "❌ Please upload a valid PDF file.";
                lblMessage.CssClass += " error";
                return;
            }

            if (string.IsNullOrEmpty(ddlClients.SelectedValue))
            {
                lblMessage.Text = "❌ Please select a client.";
                lblMessage.CssClass += " error";
                return;
            }

            try
            {
                int clientId = Convert.ToInt32(ddlClients.SelectedValue);
                int uploadedBy = Convert.ToInt32(Session["AdminID"]);
                DateTime startDate = Convert.ToDateTime(txtStartDate.Text);
                DateTime endDate = Convert.ToDateTime(txtEndDate.Text);
                string remarks = txtRemarks.Text.Trim();

                string fileName = Guid.NewGuid().ToString() + ".pdf";

                // ✅ Save virtual path (for database)
                string relativePath = "~/EncryptedContracts/" + fileName;

                // ✅ Map virtual to real physical path (for saving file)
                string physicalPath = Server.MapPath(relativePath);

                // ✅ Ensure folder exists
                string folderPath = Path.GetDirectoryName(physicalPath);
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                // ✅ Encrypt and save
                using (MemoryStream ms = new MemoryStream())
                {
                    fuContract.PostedFile.InputStream.CopyTo(ms);
                    byte[] encryptedData = AESHelper.Encrypt(ms.ToArray());
                    File.WriteAllBytes(physicalPath, encryptedData);
                }

                // ✅ Save metadata to database (virtual path only)
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"INSERT INTO ClientContracts 
                            (ClientID, FilePath, StartDate, EndDate, UploadedBy, Remarks)
                            VALUES (@ClientID, @FilePath, @StartDate, @EndDate, @UploadedBy, @Remarks)";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@ClientID", clientId);
                    cmd.Parameters.AddWithValue("@FilePath", relativePath); // 🔵 Save virtual path here
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);
                    cmd.Parameters.AddWithValue("@UploadedBy", uploadedBy);
                    cmd.Parameters.AddWithValue("@Remarks", remarks);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                lblMessage.Text = "✅ Contract uploaded and encrypted successfully!";
                lblMessage.CssClass = "message";
                ClearForm();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "❌ Error: " + ex.Message;
                lblMessage.CssClass += " error";
            }
        }



        private void ClearForm()
        {
            ddlClients.SelectedIndex = 0;
            txtStartDate.Text = "";
            txtEndDate.Text = "";
            txtRemarks.Text = "";
        }
    }
}
