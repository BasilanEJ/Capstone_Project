using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace RRCManagementSystem
{
    public partial class AddSupplier : System.Web.UI.Page
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

            string role = Session["Role"].ToString();

            // 🔐 Deny access for SuperAdmin and Inspector
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            // 🔐 Check CanAdd permission for ManageSupplier (via SP)
            if (!HasAddPermission(userId, "ManageSupplier"))
            {
                Response.Redirect("~/Unauthorized.aspx");
                return;
            }

            if (!IsPostBack)
            {
                if (ddlStatus.Items.Count == 0)
                {
                    ddlStatus.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Select Status", ""));
                    ddlStatus.Items.Insert(1, new System.Web.UI.WebControls.ListItem("Active", "Active"));
                    ddlStatus.Items.Insert(2, new System.Web.UI.WebControls.ListItem("Inactive", "Inactive"));
                }
            }
        }

        private bool HasAddPermission(int adminId, string moduleName)
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
                DisplayMessage("❌ Permission check failed: " + ex.Message, System.Drawing.Color.Red);
                return false;
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            int adminId = Convert.ToInt32(Session["UserID"]);
            if (!HasAddPermission(adminId, "ManageSupplier"))
            {
                DisplayMessage("❌ You don't have permission to add suppliers.", System.Drawing.Color.Red);
                return;
            }

            // Gather inputs
            string name = txtName.Text.Trim();
            string companyName = txtCompanyName.Text.Trim();
            string businessType = ddlBusinessType.SelectedValue;
            string address = txtAddress.Text.Trim();
            string contactNumber = txtContactNumber.Text.Trim();
            string email = txtEmail.Text.Trim();
            string status = ddlStatus.SelectedValue;

            // Basic required validation
            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(address) ||
                string.IsNullOrWhiteSpace(contactNumber))
            {
                DisplayMessage("⚠ Please fill in all required fields (marked with *).", System.Drawing.Color.Red);
                return;
            }

            // Insert via stored procedure
            try
            {
                int affected = 0;
                int newId = 0;
                string reason = "";

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spSupplier_Add", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 255).Value = name;
                    cmd.Parameters.Add("@CompanyName", SqlDbType.NVarChar, 255).Value =
                        string.IsNullOrWhiteSpace(companyName) ? (object)DBNull.Value : companyName;
                    cmd.Parameters.Add("@BusinessType", SqlDbType.NVarChar, 100).Value =
                        string.IsNullOrWhiteSpace(businessType) ? (object)DBNull.Value : businessType;
                    cmd.Parameters.Add("@Address", SqlDbType.NVarChar, 500).Value = address;
                    cmd.Parameters.Add("@ContactNumber", SqlDbType.NVarChar, 20).Value = contactNumber;
                    cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value =
                        string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email;
                    cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value =
                        string.IsNullOrWhiteSpace(status) ? (object)DBNull.Value : status;

                    var pOut = cmd.Parameters.Add("@NewSupplierID", SqlDbType.Int);
                    pOut.Direction = ParameterDirection.Output;

                    conn.Open();
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            affected = Convert.ToInt32(rdr["Affected"]);
                            reason = rdr["Reason"].ToString();
                            newId = Convert.ToInt32(rdr["SupplierID"]);
                        }
                    }

                    // OUTPUT param also has it
                    if (newId == 0 && pOut.Value != DBNull.Value)
                        newId = Convert.ToInt32(pOut.Value);
                }

                if (affected == 1 && newId > 0)
                {
                    // Audit
                    InsertAudit(adminId, $"Added new supplier (ID: {newId}): {name} ({companyName})");

                    DisplayMessage("✅ Supplier added successfully!", System.Drawing.Color.Green);
                    ClearForm();
                }
                else
                {
                    var msg = reason == "Duplicate"
                        ? "⚠ A supplier with the same Name and Company already exists."
                        : "⚠ Insert failed.";
                    DisplayMessage(msg, System.Drawing.Color.Red);
                }
            }
            catch (Exception ex)
            {
                DisplayMessage("⚠ Error adding supplier: " + ex.Message, System.Drawing.Color.Red);
            }
        }

        private void DisplayMessage(string message, System.Drawing.Color color)
        {
            lblMessage.Text = message;
            lblMessage.ForeColor = color;
        }

        private void ClearForm()
        {
            txtName.Text = "";
            txtCompanyName.Text = "";
            txtAddress.Text = "";
            txtContactNumber.Text = "";
            txtEmail.Text = "";
            ddlStatus.SelectedIndex = 0;
            ddlBusinessType.SelectedIndex = 0;
        }

        private void InsertAudit(int? adminId, string action)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAudit_Insert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AdminID", (object)adminId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Action", action);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                // optional: log
            }
        }
    }
}
