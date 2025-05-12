using System;
using System.Configuration;
using System.Data.SqlClient;

namespace RRCManagementSystem
{
    public partial class EditSupplier : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        private int SupplierID
        {
            get
            {
                int id = 0;
                if (Request.QueryString["SupplierID"] != null)
                {
                    int.TryParse(Request.QueryString["SupplierID"], out id);
                }
                return id;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["AdminID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int adminId = Convert.ToInt32(Session["AdminID"]);

            if (!HasEditPermission(adminId, "ManageSupplier"))
            {
                lblMessage.Text = "❌ You do not have permission to edit suppliers.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                btnUpdate.Enabled = false;
                pnlEditSupplier.Enabled = false;
                return;
            }

            if (!IsPostBack)
            {
                ddlStatus.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Select Status", ""));
                ddlBusinessType.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Select Business Type", ""));

                if (SupplierID > 0)
                {
                    LoadSupplierDetails(SupplierID);
                }
                else
                {
                    lblMessage.Text = "⚠ No supplier selected.";
                    pnlEditSupplier.Visible = false;
                }
            }
        }

        private void LoadSupplierDetails(int supplierID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT Name, CompanyName, BusinessType, Address, ContactNumber, Email, Status
                                 FROM Supplier WHERE SupplierID = @SupplierID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SupplierID", supplierID);

                    try
                    {
                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            txtName.Text = reader["Name"].ToString();
                            txtCompanyName.Text = reader["CompanyName"].ToString();
                            ddlBusinessType.SelectedValue = reader["BusinessType"].ToString();
                            txtAddress.Text = reader["Address"].ToString();
                            txtContactNumber.Text = reader["ContactNumber"].ToString();
                            txtEmail.Text = reader["Email"].ToString();
                            ddlStatus.SelectedValue = reader["Status"].ToString();
                        }
                        else
                        {
                            lblMessage.Text = "⚠ Supplier not found.";
                            pnlEditSupplier.Visible = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        lblMessage.Text = "⚠ Error loading supplier: " + ex.Message;
                        lblMessage.ForeColor = System.Drawing.Color.Red;
                    }
                }
            }
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            if (Session["AdminID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int adminId = Convert.ToInt32(Session["AdminID"]);

            if (!HasEditPermission(adminId, "ManageSupplier"))
            {
                lblMessage.Text = "❌ You do not have permission to update suppliers.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                btnUpdate.Enabled = false;
                return;
            }

            string name = txtName.Text.Trim();
            string companyName = txtCompanyName.Text.Trim();
            string businessType = ddlBusinessType.SelectedValue;
            string address = txtAddress.Text.Trim();
            string contactNumber = txtContactNumber.Text.Trim();
            string email = txtEmail.Text.Trim();
            string status = ddlStatus.SelectedValue;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(address) || string.IsNullOrEmpty(contactNumber))
            {
                lblMessage.Text = "⚠ Please fill in all required fields (marked with *).";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                UPDATE Supplier
                SET Name = @Name,
                    CompanyName = @CompanyName,
                    BusinessType = @BusinessType,
                    Address = @Address,
                    ContactNumber = @ContactNumber,
                    Email = @Email,
                    Status = @Status
                WHERE SupplierID = @SupplierID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SupplierID", SupplierID);
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@CompanyName", string.IsNullOrEmpty(companyName) ? (object)DBNull.Value : companyName);
                    cmd.Parameters.AddWithValue("@BusinessType", string.IsNullOrEmpty(businessType) ? (object)DBNull.Value : businessType);
                    cmd.Parameters.AddWithValue("@Address", address);
                    cmd.Parameters.AddWithValue("@ContactNumber", contactNumber);
                    cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email);
                    cmd.Parameters.AddWithValue("@Status", string.IsNullOrEmpty(status) ? (object)DBNull.Value : status);

                    try
                    {
                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();

                        lblMessage.Text = rowsAffected > 0
                            ? "✅ Supplier updated successfully!"
                            : "⚠ No changes were made or supplier not found.";

                        lblMessage.ForeColor = rowsAffected > 0
                            ? System.Drawing.Color.Green
                            : System.Drawing.Color.Red;

                        // ✅ Audit Log
                        if (rowsAffected > 0)
                        {
                            AddAuditLog(adminId, $"Edited supplier (ID: {SupplierID}) - Name: {name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        lblMessage.Text = "⚠ Error updating supplier: " + ex.Message;
                        lblMessage.ForeColor = System.Drawing.Color.Red;
                    }
                }
            }
        }

        private bool HasEditPermission(int adminId, string moduleName)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT CanEdit FROM AdminPermissions WHERE UserID = @AdminID AND ModuleName = @ModuleName";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
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
                        lblMessage.Text = $"❌ Error checking permissions: {ex.Message}";
                        lblMessage.ForeColor = System.Drawing.Color.Red;
                        return false;
                    }
                }
            }
        }

        // ✅ Audit Log Method
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
                        // Optional: log error or ignore silently
                    }
                }
            }
        }
    }
}