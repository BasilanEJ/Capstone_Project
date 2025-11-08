using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class AddServices : Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
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

                if (Session["ServiceAdded"] is bool added && added)
                {
                    litScript.Text = @"
<script>
Swal.fire({
    icon: 'success',
    title: 'Success!',
    text: 'Service added successfully. You can now set its pricing tiers.',
    confirmButtonColor: '#2563EB'
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

        protected void btnAddService_Click(object sender, EventArgs e)
        {
            string serviceName = txtName.Text.Trim();
            string serviceType = ddlServiceType.SelectedValue;
            string description = txtDescription.Text.Trim();

            if (string.IsNullOrWhiteSpace(serviceName))
            {
                lblMessage.Text = "⚠ Please enter a service name.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            if (string.IsNullOrWhiteSpace(serviceType))
            {
                lblMessage.Text = "⚠ Please select a service type.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            bool isContract = serviceType == "Termite Control";

            try
            {
                int newServiceID = 0;

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spService_Insert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Name", serviceName);
                    cmd.Parameters.AddWithValue("@ServiceType", serviceType);
                    cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description);
                    cmd.Parameters.AddWithValue("@IsContract", isContract);

                    conn.Open();
                    var result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                        newServiceID = Convert.ToInt32(result);
                }

                if (newServiceID > 0)
                {
                    // Log audit entry
                    int adminId = Convert.ToInt32(Session["UserID"]);
                    using (var conn = new SqlConnection(connectionString))
                    using (var a = new SqlCommand("dbo.spAudit_Insert", conn))
                    {
                        a.CommandType = CommandType.StoredProcedure;
                        a.Parameters.AddWithValue("@AdminID", adminId);
                        a.Parameters.AddWithValue("@Action", $"Added new service (ID: {newServiceID}) - {serviceName} ({serviceType}) | Contractual: {isContract}");
                        conn.Open();
                        a.ExecuteNonQuery();
                    }

                    ClearForm();
                    Session["ServiceAdded"] = true;
                    Response.Redirect("AddServices.aspx");
                }
                else
                {
                    lblMessage.Text = "⚠ Failed to create service. Please try again.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "❌ Error adding service: " + ex.Message;
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
