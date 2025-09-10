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

            // Determine if this is a contractual service
            bool isContract = serviceType == "Termite Control";

            try
            {
                int newServiceID = 0;

                // 1️⃣ Insert new service into Services table
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spService_Insert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Name", serviceName);
                    cmd.Parameters.AddWithValue("@ServiceType", (object)serviceType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description);
                    cmd.Parameters.AddWithValue("@IsContract", isContract);

                    conn.Open();
                    var result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                        newServiceID = Convert.ToInt32(result);
                }

                if (newServiceID <= 0)
                {
                    lblMessage.Text = "⚠ Failed to create service. Please try again.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                // 2️⃣ Insert the seven pricing ranges for the service
                // 2️⃣ Insert the seven pricing ranges for the service using a stored procedure
                decimal price0_100 = ParseDecimal(txtPrice_0_100.Text);
                decimal price101_250 = ParseDecimal(txtPrice_101_250.Text);
                decimal price251_400 = ParseDecimal(txtPrice_251_400.Text);
                decimal price401_600 = ParseDecimal(txtPrice_401_600.Text);
                decimal price601_800 = ParseDecimal(txtPrice_601_800.Text);
                decimal price801_1000 = ParseDecimal(txtPrice_801_1000.Text);
                decimal price1000Plus = ParseDecimal(txtPrice_1000_Plus.Text);

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spServicePricing_InsertRanges", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@ServiceID", newServiceID);
                    cmd.Parameters.AddWithValue("@Price0_100", price0_100);
                    cmd.Parameters.AddWithValue("@Price101_250", price101_250);
                    cmd.Parameters.AddWithValue("@Price251_400", price251_400);
                    cmd.Parameters.AddWithValue("@Price401_600", price401_600);
                    cmd.Parameters.AddWithValue("@Price601_800", price601_800);
                    cmd.Parameters.AddWithValue("@Price801_1000", price801_1000);
                    cmd.Parameters.AddWithValue("@Price1000Plus", price1000Plus);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }


                // 3️⃣ Insert into Audit logs
                using (var conn = new SqlConnection(connectionString))
                using (var a = new SqlCommand("dbo.spAudit_Insert", conn))
                {
                    a.CommandType = CommandType.StoredProcedure;
                    a.Parameters.AddWithValue("@AdminID", (object)adminId ?? DBNull.Value);
                    a.Parameters.AddWithValue("@Action", $"Added service (ID: {newServiceID}) - {serviceName} ({serviceType}) | Contractual: {isContract}");
                    conn.Open();
                    a.ExecuteNonQuery();
                }

                // 4️⃣ Clear the form and set success session
                ClearForm();

                Session["ServiceAdded"] = true;
                lblMessage.Visible = true;
                lblMessage.Text = "✅ Service and pricing added successfully!";
                lblMessage.ForeColor = System.Drawing.Color.Green;
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠ Error adding service: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        // Helper to safely parse decimals
        private decimal ParseDecimal(string input)
        {
            return string.IsNullOrWhiteSpace(input) ? 0 : Convert.ToDecimal(input);
        }

        private void ClearForm()
        {
            txtName.Text = "";
            ddlServiceType.SelectedIndex = 0;
            txtDescription.Text = "";

            txtPrice_0_100.Text = "";
            txtPrice_101_250.Text = "";
            txtPrice_251_400.Text = "";
            txtPrice_401_600.Text = "";
            txtPrice_601_800.Text = "";
            txtPrice_801_1000.Text = "";
            txtPrice_1000_Plus.Text = "";
        }
    }
}
