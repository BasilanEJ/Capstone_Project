using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class AddServices : Page
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

            string role = Session["Role"].ToString();

            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            if (!HasPermissionToAddService(userId, "ManageServices"))
            {
                Response.Redirect("~/Unauthorized.aspx");
                return;
            }

            if (!IsPostBack)
            {
                lblMessage.Text = "";

                // ✅ Inject SweetAlert only on initial load
                if (Session["ServiceAdded"] != null && (bool)Session["ServiceAdded"])
                {
                    litScript.Text = @"
<script>
    Swal.fire({
        icon: 'success',
        title: 'Success!',
        text: 'Service added successfully.',
        confirmButtonColor: '#004085'
    }).then(() => {
        window.location.href = 'ViewServices.aspx';
    });
</script>";
                    Session["ServiceAdded"] = null;
                }
            }
        }

        private bool HasPermissionToAddService(int adminId, string moduleName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT CanAdd FROM AdminPermissions WHERE UserID = @UserID AND ModuleName = @ModuleName";

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
                    catch (Exception ex)
                    {
                        lblMessage.Text = "❌ Error checking permissions: " + ex.Message;
                        lblMessage.ForeColor = System.Drawing.Color.Red;
                        return false;
                    }
                }
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            int adminId = Convert.ToInt32(Session["UserID"]);
            if (!HasPermissionToAddService(adminId, "ManageServices"))
            {
                lblMessage.Text = "❌ You do not have permission to add services.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            string serviceName = txtName.Text.Trim();
            string serviceType = ddlServiceType.SelectedValue;
            string description = txtDescription.Text.Trim();

            if (string.IsNullOrEmpty(serviceName))
            {
                lblMessage.Text = "⚠ Please enter a service name.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            bool isContract = serviceType == "Termite Control";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    INSERT INTO Services 
                        (Name, ServiceType, Description, IsContract, CreatedAt)
                    VALUES 
                        (@Name, @ServiceType, @Description, @IsContract, GETDATE())";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Name", serviceName);
                cmd.Parameters.AddWithValue("@ServiceType", serviceType);
                cmd.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description);
                cmd.Parameters.AddWithValue("@IsContract", isContract);

                conn.Open();
                cmd.ExecuteNonQuery();

                AddAuditLog(adminId, $"Added new service: {serviceName} ({serviceType}) | Contractual: {isContract}");
            }

            Session["ServiceAdded"] = true;

            lblMessage.Visible = true;
            lblMessage.Text = "✅ Service added successfully!";
            lblMessage.ForeColor = System.Drawing.Color.Green;
        }

        private void ClearForm()
        {
            txtName.Text = "";
            ddlServiceType.SelectedIndex = 0;
            txtDescription.Text = "";
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
                        // Silent fail
                    }
                }
            }
        }
    }
}
