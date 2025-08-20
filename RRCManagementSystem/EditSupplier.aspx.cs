using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace RRCManagementSystem
{
    public partial class EditSupplier : System.Web.UI.Page
    {
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        private int SupplierID
        {
            get
            {
                int id;
                return int.TryParse(Request.QueryString["SupplierID"], out id) ? id : 0;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // 🔐 Require login
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx"); return;
            }

            string role = Convert.ToString(Session["Role"]);
            // 🔐 Block roles per your rules
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx"); return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);
            if (!HasEditPermission(userId, "ManageSupplier"))
            {
                lblMessage.Text = "❌ You do not have permission to edit suppliers.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                btnUpdate.Enabled = false;
                pnlEditSupplier.Enabled = false;
                return;
            }

            if (!IsPostBack)
            {
                // Optional default entries
                if (ddlStatus.Items.Count == 0)
                    ddlStatus.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Select Status", ""));
                if (ddlBusinessType.Items.Count == 0)
                    ddlBusinessType.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Select Business Type", ""));

                if (SupplierID > 0)
                    LoadSupplierDetails(SupplierID);
                else
                {
                    lblMessage.Text = "⚠ No supplier selected.";
                    lblMessage.ForeColor = System.Drawing.Color.Orange;
                    pnlEditSupplier.Visible = false;
                }
            }
        }

        private bool HasEditPermission(int adminId, string moduleName)
        {
            try
            {
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAdminPermission_Check", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", adminId);
                    cmd.Parameters.AddWithValue("@ModuleName", moduleName);
                    cmd.Parameters.AddWithValue("@Permission", "CanEdit");
                    con.Open();
                    var result = cmd.ExecuteScalar();
                    return result != null && result != DBNull.Value && Convert.ToBoolean(result);
                }
            }
            catch
            {
                return false;
            }
        }

        private void LoadSupplierDetails(int supplierID)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spSupplier_GetById", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SupplierID", supplierID);

                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        txtName.Text = Convert.ToString(rdr["Name"]);
                        txtCompanyName.Text = rdr["CompanyName"] as string ?? "";
                        ddlBusinessType.SelectedValue = rdr["BusinessType"] as string ?? "";
                        txtAddress.Text = Convert.ToString(rdr["Address"]);
                        txtContactNumber.Text = Convert.ToString(rdr["ContactNumber"]);
                        txtEmail.Text = rdr["Email"] as string ?? "";
                        ddlStatus.SelectedValue = rdr["Status"] as string ?? "";
                    }
                    else
                    {
                        lblMessage.Text = "⚠ Supplier not found.";
                        lblMessage.ForeColor = System.Drawing.Color.Red;
                        pnlEditSupplier.Visible = false;
                    }
                }
            }
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            if (SupplierID <= 0)
            {
                lblMessage.Text = "⚠ Invalid Supplier ID.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            int adminId = Convert.ToInt32(Session["UserID"]);

            string name = txtName.Text.Trim();
            string companyName = txtCompanyName.Text.Trim();
            string businessType = ddlBusinessType.SelectedValue;
            string address = txtAddress.Text.Trim();
            string contact = txtContactNumber.Text.Trim();
            string email = txtEmail.Text.Trim();
            string status = ddlStatus.SelectedValue;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(address) || string.IsNullOrEmpty(contact))
            {
                lblMessage.Text = "⚠ Please fill in all required fields (marked with *).";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            int affected = 0;
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spSupplier_Update", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SupplierID", SupplierID);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@CompanyName", (object)companyName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@BusinessType", (object)businessType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", address);
                cmd.Parameters.AddWithValue("@ContactNumber", contact);
                cmd.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email);
                cmd.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(status) ? (object)DBNull.Value : status);

                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                        affected = Convert.ToInt32(rdr["Affected"]);
                }
            }

            if (affected == 1)
            {
                // Audit
                using (var con = new SqlConnection(connectionString))
                using (var a = new SqlCommand("dbo.spAudit_Insert", con))
                {
                    a.CommandType = CommandType.StoredProcedure;
                    a.Parameters.AddWithValue("@AdminID", (object)adminId ?? DBNull.Value);
                    a.Parameters.AddWithValue("@Action", $"Edited supplier (ID: {SupplierID}) - Name: {name}");
                    con.Open();
                    a.ExecuteNonQuery();
                }

                lblMessage.Text = "✅ Supplier updated successfully!";
                lblMessage.ForeColor = System.Drawing.Color.Green;
            }
            else
            {
                lblMessage.Text = "⚠ No changes were made or supplier not found.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }
    }
}
