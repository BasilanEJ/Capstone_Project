using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class AddServices : Page
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
                lblMessage.Text = string.Empty;

                // SweetAlert after successful add
                if (Session["ServiceAdded"] is bool b && b)
                {
                    litScript.Text = @"
<script>
Swal.fire({
  icon: 'success',
  title: 'Success!',
  text: 'Service added successfully.',
  confirmButtonColor: '#004085'
}).then(() => { window.location.href = 'ViewServices.aspx'; });
</script>";
                    Session["ServiceAdded"] = null;
                }
            }
        }

        private bool HasPermissionToAddService(int adminId, string moduleName)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAdminPermission_Check", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", adminId);
                    cmd.Parameters.AddWithValue("@ModuleName", moduleName);
                    cmd.Parameters.AddWithValue("@Permission", "CanAdd");

                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null && result != DBNull.Value && Convert.ToBoolean(result);
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "❌ Error checking permissions: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return false;
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

            if (string.IsNullOrWhiteSpace(serviceName))
            {
                lblMessage.Text = "⚠ Please enter a service name.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            // Your original rule
            bool isContract = serviceType == "Termite Control";

            try
            {
                int newId = 0;
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spService_Insert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Name", serviceName);
                    cmd.Parameters.AddWithValue("@ServiceType", (object)serviceType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description);
                    cmd.Parameters.AddWithValue("@IsContract", isContract);

                    // 👇 Optional, if proc handles it you don’t need this
                    // cmd.Parameters.AddWithValue("@Status", "Available");

                    conn.Open();
                    var result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value) newId = Convert.ToInt32(result);
                }

                // Audit
                using (var conn = new SqlConnection(connectionString))
                using (var a = new SqlCommand("dbo.spAudit_Insert", conn))
                {
                    a.CommandType = CommandType.StoredProcedure;
                    a.Parameters.AddWithValue("@AdminID", (object)adminId ?? DBNull.Value);
                    a.Parameters.AddWithValue("@Action", $"Added service (ID: {newId}) - {serviceName} ({serviceType}) | Contractual: {isContract}");
                    conn.Open();
                    a.ExecuteNonQuery();
                }

                // Clear the form after successful submission
                ClearForm();

                Session["ServiceAdded"] = true;
                lblMessage.Visible = true;
                lblMessage.Text = "✅ Service added successfully!";
                lblMessage.ForeColor = System.Drawing.Color.Green;
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠ Error adding service: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void ClearForm()
        {
            txtName.Text = "";
            ddlServiceType.SelectedIndex = 0;
            txtDescription.Text = "";
        }
    }
}
