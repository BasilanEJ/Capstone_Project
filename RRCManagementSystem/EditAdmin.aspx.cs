using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI.WebControls;
using RRCManagementSystem.Helpers; // For AESHelper

namespace RRCManagementSystem
{
    public partial class EditAdmin : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;
        private int userID;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 🔐 Require login + SuperAdmin
            if (Session["UserID"] == null || Session["Role"] == null ||
                !Session["Role"].ToString().Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                if (int.TryParse(Request.QueryString["UserID"], out userID))
                {
                    LoadAdminDetails(userID);
                    LoadPermissions(userID);
                }
                else
                {
                    lblMessage.Text = "⚠ No Admin selected.";
                }

                // Register ItemDataBound after binding
                rptPermissions.ItemDataBound += rptPermissions_ItemDataBound;
            }

            // ✅ Show SweetAlert2 success message after postback (optional flag from previous save)
            if (Session["ShowSuccess"] != null && (bool)Session["ShowSuccess"])
            {
                Session.Remove("ShowSuccess");

                string script = @"<script src='https://cdn.jsdelivr.net/npm/sweetalert2@11'></script>
<script>
Swal.fire({
    icon: 'success',
    title: 'Changes Saved',
    text: 'The admin permissions were updated successfully!',
    confirmButtonColor: '#007bff'
});
</script>";
                ClientScript.RegisterStartupScript(this.GetType(), "SuccessAlert", script);
            }
        }

        /* =========================
           LOAD: Admin details
           ========================= */
        private void LoadAdminDetails(int id)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spUser_GetByID", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = id;
                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        txtName.Text = reader["Name"]?.ToString() ?? "";

                        // 🔹 Decrypt email before displaying
                        if (reader["Email"] != DBNull.Value && !string.IsNullOrEmpty(reader["Email"].ToString()))
                        {
                            try
                            {
                                txtEmail.Text = AESHelper.DecryptEmail(reader["Email"].ToString());
                            }
                            catch
                            {
                                txtEmail.Text = "[Decryption Error]";
                            }
                        }
                        else
                        {
                            txtEmail.Text = "";
                        }
                    }
                    else
                    {
                        lblMessage.Text = "⚠ Admin not found.";
                    }
                }
            }
        }

        /* =========================
           LOAD: Permissions
           ========================= */
        private void LoadPermissions(int id)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ModuleName", typeof(string));
            dt.Columns.Add("CanView", typeof(bool));
            dt.Columns.Add("CanAdd", typeof(bool));
            dt.Columns.Add("CanEdit", typeof(bool));
            dt.Columns.Add("CanDelete", typeof(bool));

            // Pull existing permissions via SP
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spAdminPermissions_GetByUser", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = id;

                try
                {
                    da.Fill(dt);
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "⚠ Error loading permissions: " + ex.Message;
                }
            }

            // Ensure all modules are present (defaults if missing)
            string[] modules = {
                "Dashboard",
                "ManageInquiry",
                "ClientApproval",
                "CreateCustomerAccount",
                "ManageEmployees",
                "ManageItem",
                "ManageEquipment",
                "ManageClient",
                "ManageBooking",
                "Sales&Transaction",
                "ManageSupplier",
                "ManageServices",
                "AdminReports",
                "AdminGuide"
            };

            var present = dt.AsEnumerable()
                            .Select(r => r.Field<string>("ModuleName"))
                            .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (string m in modules)
            {
                if (!present.Contains(m))
                {
                    var row = dt.NewRow();
                    row["ModuleName"] = m;
                    row["CanView"] = false;
                    row["CanAdd"] = false;
                    row["CanEdit"] = false;
                    row["CanDelete"] = false;
                    dt.Rows.Add(row);
                }
            }

            // Sort by ModuleName
            DataView dv = dt.DefaultView;
            dv.Sort = "ModuleName ASC";
            var finalTable = dv.ToTable();

            rptPermissions.DataSource = finalTable;
            rptPermissions.DataBind();
        }

        protected void rptPermissions_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var chkView = (CheckBox)e.Item.FindControl("chkView");
                var chkAdd = (CheckBox)e.Item.FindControl("chkAdd");
                var chkEdit = (CheckBox)e.Item.FindControl("chkEdit");
                var chkDelete = (CheckBox)e.Item.FindControl("chkDelete");

                string scaleStyle = "transform: scale(1.8); cursor: pointer;";

                chkView.InputAttributes["style"] = scaleStyle;
                chkAdd.InputAttributes["style"] = scaleStyle;
                chkEdit.InputAttributes["style"] = scaleStyle;
                chkDelete.InputAttributes["style"] = scaleStyle;
            }
        }

        /* =========================
           SAVE: Update admin + permissions (transaction)
           ========================= */
        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(Request.QueryString["UserID"], out userID))
            {
                lblMessage.Text = "⚠ No Admin selected.";
                return;
            }

            // Build TVP for permissions
            var tvp = new DataTable();
            tvp.Columns.Add("ModuleName", typeof(string));
            tvp.Columns.Add("CanView", typeof(bool));
            tvp.Columns.Add("CanAdd", typeof(bool));
            tvp.Columns.Add("CanEdit", typeof(bool));
            tvp.Columns.Add("CanDelete", typeof(bool));

            foreach (RepeaterItem item in rptPermissions.Items)
            {
                string moduleName = ((HiddenField)item.FindControl("hfModuleName")).Value;
                bool canView = ((CheckBox)item.FindControl("chkView")).Checked;
                bool canAdd = ((CheckBox)item.FindControl("chkAdd")).Checked;
                bool canEdit = ((CheckBox)item.FindControl("chkEdit")).Checked;
                bool canDelete = ((CheckBox)item.FindControl("chkDelete")).Checked;
                tvp.Rows.Add(moduleName, canView, canAdd, canEdit, canDelete);
            }

            // Encrypt the email and generate its SHA-256 hash before saving
            string plainEmail = (txtEmail.Text ?? "").Trim().ToLowerInvariant();
            string encryptedEmail = AESHelper.EncryptEmail(plainEmail);
            string emailHash = AESHelper.ComputeSHA256(plainEmail);

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        // 1) Update basic info
                        using (var cmdUpdate = new SqlCommand("dbo.spUser_UpdateBasic", conn, tx))
                        {
                            cmdUpdate.CommandType = CommandType.StoredProcedure;
                            cmdUpdate.Parameters.Add("@UserID", SqlDbType.Int).Value = userID;
                            cmdUpdate.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = (txtName.Text ?? "").Trim();
                            cmdUpdate.Parameters.Add("@Email", SqlDbType.NVarChar, -1).Value = encryptedEmail;
                            cmdUpdate.Parameters.Add("@EmailHash", SqlDbType.Char, 64).Value = emailHash;
                            cmdUpdate.ExecuteNonQuery();
                        }

                        // 2) Replace permissions in one go (TVP)
                        using (var cmdPerms = new SqlCommand("dbo.spAdminPermissions_BulkReplace", conn, tx))
                        {
                            cmdPerms.CommandType = CommandType.StoredProcedure;
                            cmdPerms.Parameters.Add("@UserID", SqlDbType.Int).Value = userID;

                            var p = cmdPerms.Parameters.AddWithValue("@Perms", tvp);
                            p.SqlDbType = SqlDbType.Structured;
                            p.TypeName = "dbo.AdminPermissionTVP";

                            cmdPerms.ExecuteNonQuery();
                        }

                        tx.Commit();
                        Session["ShowSuccess"] = true; // optional flag for SweetAlert
                        lblMessage.Text = "✅ Admin updated successfully!";
                    }
                    catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
                    {
                        tx.Rollback();
                        lblMessage.Text = "⚠ Email already exists. Please use a different email.";
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        lblMessage.Text = "⚠ Error updating admin: " + ex.Message;

                        string errorScript = $@"<script src='https://cdn.jsdelivr.net/npm/sweetalert2@11'></script>
<script>
Swal.fire({{
    icon: 'error',
    title: 'Error Saving',
    text: '{ex.Message.Replace("'", "\\'")}',
    confirmButtonColor: '#dc3545'
}});
</script>";
                        ClientScript.RegisterStartupScript(this.GetType(), "ErrorAlert", errorScript);
                    }
                }
            }
        }
    }
}
