using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class AddRole : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadDefaultPermissions();
            }
        }

        private void LoadDefaultPermissions()
        {
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

            DataTable dt = new DataTable();
            dt.Columns.Add("ModuleName");
            dt.Columns.Add("CanView", typeof(bool));
            dt.Columns.Add("CanAdd", typeof(bool));
            dt.Columns.Add("CanEdit", typeof(bool));
            dt.Columns.Add("CanDelete", typeof(bool));

            // Default: all unchecked (SuperAdmin can select)
            foreach (string module in modules)
            {
                var row = dt.NewRow();
                row["ModuleName"] = module;
                row["CanView"] = false;
                row["CanAdd"] = false;
                row["CanEdit"] = false;
                row["CanDelete"] = false;
                dt.Rows.Add(row);
            }

            rptPermissions.DataSource = dt;
            rptPermissions.DataBind();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            Page.Validate();
            if (!Page.IsValid)
            {
                ShowError("⚠ Please fix the highlighted errors.");
                return;
            }

            string roleName = (txtRoleName.Text ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(roleName))
            {
                ShowError("⚠ Role name is required.");
                return;
            }

            // Title Case for consistency
            TextInfo ti = new CultureInfo("en-US", false).TextInfo;
            roleName = ti.ToTitleCase(roleName.ToLower());

            // Validate at least one permission is granted
            bool hasAnyPermission = false;
            foreach (RepeaterItem item in rptPermissions.Items)
            {
                if (((CheckBox)item.FindControl("chkView")).Checked ||
                    ((CheckBox)item.FindControl("chkAdd")).Checked ||
                    ((CheckBox)item.FindControl("chkEdit")).Checked ||
                    ((CheckBox)item.FindControl("chkDelete")).Checked)
                {
                    hasAnyPermission = true;
                    break;
                }
            }

            if (!hasAnyPermission)
            {
                ShowError("⚠ Please grant at least one permission for this role.");
                return;
            }

            try
            {
                int newRoleId;

                // Step 1: Add the role
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spRole_Add", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@RoleName", SqlDbType.NVarChar, 100).Value = roleName;

                    var outParam = new SqlParameter("@NewRoleID", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(outParam);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    newRoleId = (outParam.Value == DBNull.Value) ? 0 : Convert.ToInt32(outParam.Value);
                }

                if (newRoleId > 0)
                {
                    // Step 2: Save permissions for this role
                    SaveRolePermissions(newRoleId);

                    ShowSweetAlert("Success", $"✅ Role '{roleName}' added successfully with permissions!", "success", true);
                    txtRoleName.Text = string.Empty;
                    LoadDefaultPermissions(); // Reset checkboxes
                }
                else
                {
                    ShowSweetAlert("Duplicate", "This role already exists.", "error", false);
                }
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                ShowSweetAlert("Duplicate", "This role already exists.", "error", false);
            }
            catch (Exception ex)
            {
                ShowError("❌ Error: " + ex.Message);
            }
        }

        private void SaveRolePermissions(int roleId)
        {
            // Build TVP for bulk insert
            var tvp = new DataTable();
            tvp.Columns.Add("ModuleName", typeof(string));
            tvp.Columns.Add("CanView", typeof(bool));
            tvp.Columns.Add("CanAdd", typeof(bool));
            tvp.Columns.Add("CanEdit", typeof(bool));
            tvp.Columns.Add("CanDelete", typeof(bool));

            foreach (RepeaterItem item in rptPermissions.Items)
            {
                string module = ((HiddenField)item.FindControl("hfModuleName")).Value;
                bool canView = ((CheckBox)item.FindControl("chkView")).Checked;
                bool canAdd = ((CheckBox)item.FindControl("chkAdd")).Checked;
                bool canEdit = ((CheckBox)item.FindControl("chkEdit")).Checked;
                bool canDelete = ((CheckBox)item.FindControl("chkDelete")).Checked;

                tvp.Rows.Add(module, canView, canAdd, canEdit, canDelete);
            }

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spRolePermissions_BulkInsert", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@RoleID", SqlDbType.Int).Value = roleId;

                var p = cmd.Parameters.AddWithValue("@Perms", tvp);
                p.SqlDbType = SqlDbType.Structured;
                p.TypeName = "dbo.RolePermissionTVP";

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void ShowSweetAlert(string title, string message, string icon, bool redirect)
        {
            string script = $@"
                Swal.fire({{
                    title: '{title}',
                    html: '{message}',
                    icon: '{icon}',
                    confirmButtonColor: '#1f2937'
                }})";

            if (redirect)
            {
                script += @".then((result) => {
                    if (result.isConfirmed) {
                        window.location.href = 'ViewRoles.aspx';
                    }
                });";
            }

            ScriptManager.RegisterStartupScript(this, GetType(), "SweetAlert", script, true);
        }

        private void ShowError(string message)
        {
            lblMessage.Visible = true;
            lblMessage.ForeColor = System.Drawing.Color.Red;
            lblMessage.Text = message;
        }
    }
}