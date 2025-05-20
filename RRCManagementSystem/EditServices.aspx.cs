using System;
using System.Configuration;
using System.Data.SqlClient;

namespace RRCManagementSystem
{
    public partial class EditServices : System.Web.UI.Page
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

            // 🔐 Deny SuperAdmin and Inspector
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            // 🔐 Check Edit Permission
            if (!HasEditPermission(userId, "ManageServices"))
            {
                lblMessage.Text = "❌ You do not have permission to edit services.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                btnUpdate.Enabled = false;
                return;
            }

            if (!IsPostBack)
            {
                if (Request.QueryString["ServiceID"] != null &&
                    int.TryParse(Request.QueryString["ServiceID"], out int serviceID))
                {
                    LoadService(serviceID);
                }
                else
                {
                    lblMessage.Text = "⚠ Invalid or missing Service ID.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    btnUpdate.Enabled = false;
                }
            }
        }


        private bool HasEditPermission(int adminId, string moduleName)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT CanEdit FROM AdminPermissions WHERE UserID = @AdminID AND ModuleName = @ModuleName";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@AdminID", adminId);
                cmd.Parameters.AddWithValue("@ModuleName", moduleName);

                try
                {
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null && Convert.ToBoolean(result);
                }
                catch (Exception ex)
                {
                    lblMessage.Text = $"❌ Permission check failed: {ex.Message}";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    return false;
                }
            }
        }

        private void LoadService(int serviceID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT Name, Description, Price100SQM, Price200SQM, PriceAbove200SQM
                    FROM Services
                    WHERE ServiceID = @ServiceID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ServiceID", serviceID);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    txtName.Text = reader["Name"].ToString();
                    txtDescription.Text = reader["Description"]?.ToString();
                    txtPrice100.Text = reader["Price100SQM"] != DBNull.Value ? Convert.ToDecimal(reader["Price100SQM"]).ToString("F2") : "";
                    txtPrice200.Text = reader["Price200SQM"] != DBNull.Value ? Convert.ToDecimal(reader["Price200SQM"]).ToString("F2") : "";
                    txtPriceAbove200.Text = reader["PriceAbove200SQM"] != DBNull.Value ? Convert.ToDecimal(reader["PriceAbove200SQM"]).ToString("F2") : "";
                }
                else
                {
                    lblMessage.Text = "⚠ Service not found.";
                    btnUpdate.Enabled = false;
                }
            }
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            int adminId = Convert.ToInt32(Session["UserID"]);

            if (!HasEditPermission(adminId, "ManageServices"))
            {
                lblMessage.Text = "❌ You do not have permission to edit services.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            if (!int.TryParse(Request.QueryString["ServiceID"], out int serviceID))
            {
                lblMessage.Text = "⚠ Invalid Service ID.";
                return;
            }

            string name = txtName.Text.Trim();
            string description = txtDescription.Text.Trim();
            decimal price100 = ParseDecimal(txtPrice100.Text);
            decimal price200 = ParseDecimal(txtPrice200.Text);
            decimal priceAbove200 = ParseDecimal(txtPriceAbove200.Text);

            if (string.IsNullOrEmpty(name))
            {
                lblMessage.Text = "⚠ Service name is required.";
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    UPDATE Services
                    SET Name = @Name,
                        Description = @Description,
                        Price100SQM = @Price100,
                        Price200SQM = @Price200,
                        PriceAbove200SQM = @PriceAbove200
                    WHERE ServiceID = @ServiceID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ServiceID", serviceID);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description);
                cmd.Parameters.AddWithValue("@Price100", price100 > 0 ? price100 : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Price200", price200 > 0 ? price200 : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@PriceAbove200", priceAbove200 > 0 ? priceAbove200 : (object)DBNull.Value);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    lblMessage.Text = "✅ Service updated successfully.";
                    lblMessage.ForeColor = System.Drawing.Color.Green;

                    // ✅ Add to Audit Log
                    AddAuditLog(adminId, $"Edited service (ID: {serviceID}) - Name: {name}");
                }
                else
                {
                    lblMessage.Text = "⚠ Update failed. Service not found.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
            }
        }

        private decimal ParseDecimal(string input)
        {
            return decimal.TryParse(input.Trim(), out decimal value) ? value : 0;
        }

        // ✅ Audit Logging Method
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
                        // You may log this internally or ignore silently
                    }
                }
            }
        }
    }
}