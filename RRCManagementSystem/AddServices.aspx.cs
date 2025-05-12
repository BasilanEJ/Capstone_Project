using System;
using System.Configuration;
using System.Data.SqlClient;

namespace RRCManagementSystem
{
    public partial class AddServices : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                lblMessage.Text = "";

                if (Session["AdminID"] == null)
                {
                    Response.Redirect("~/Unauthorized.aspx");
                    return;
                }

                int adminId = Convert.ToInt32(Session["AdminID"]);
                if (!HasPermissionToAddService(adminId, "ManageServices"))
                {
                    Response.Redirect("~/Unauthorized.aspx");
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
            int adminId = Convert.ToInt32(Session["AdminID"]);
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

            decimal price100 = ParseDecimal(txtPrice100.Text);
            decimal price200 = ParseDecimal(txtPrice200.Text);
            decimal priceAbove200 = ParseDecimal(txtPriceAbove200.Text);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    INSERT INTO Services (Name, ServiceType, Description, Price100SQM, Price200SQM, PriceAbove200SQM, CreatedAt)
                    VALUES (@Name, @ServiceType, @Description, @Price100, @Price200, @PriceAbove200, GETDATE())";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Name", serviceName);
                cmd.Parameters.AddWithValue("@ServiceType", serviceType);
                cmd.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description);
                cmd.Parameters.AddWithValue("@Price100", price100 > 0 ? price100 : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Price200", price200 > 0 ? price200 : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@PriceAbove200", priceAbove200 > 0 ? priceAbove200 : (object)DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();

                // ✅ Add audit log
                AddAuditLog(adminId, $"Added new service: {serviceName} ({serviceType})");
            }

            lblMessage.Text = "✅ Service added successfully!";
            lblMessage.ForeColor = System.Drawing.Color.Green;
            ClearForm();
        }

        private decimal ParseDecimal(string input)
        {
            return decimal.TryParse(input.Trim(), out decimal value) ? value : 0;
        }

        private void ClearForm()
        {
            txtName.Text = "";
            ddlServiceType.SelectedIndex = 0;
            txtDescription.Text = "";
            txtPrice100.Text = "";
            txtPrice200.Text = "";
            txtPriceAbove200.Text = "";
        }

        // ✅ Audit log method
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
                        // Optional: handle logging error silently
                    }
                }
            }
        }
    }
}