using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class ArchiveEmployee : Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Init(object sender, EventArgs e)
        {
            gvArchivedEmployees.RowDeleting += gvArchivedEmployees_RowDeleting;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));

            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadArchivedEmployees();

                if (Request.QueryString["archived"] == "1")
                {
                    Toast("success", "Archived!", "Employee has been archived successfully.", 1500);
                }
            }
        }

        private void LoadArchivedEmployees()
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spEmployees_ListArchived", con))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                var dt = new DataTable();
                da.Fill(dt);

                gvArchivedEmployees.DataSource = dt;
                gvArchivedEmployees.DataBind();
            }
        }

        protected void gvArchivedEmployees_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvArchivedEmployees.PageIndex = e.NewPageIndex;
            LoadArchivedEmployees();
        }

        protected void gvArchivedEmployees_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!int.TryParse(e.CommandArgument?.ToString(), out int employeeID))
                return;

            int userId = Convert.ToInt32(Session["UserID"]);

            if (e.CommandName == "Restore")
            {
                if (!HasEditPermission(userId, "ManageEmployees"))
                {
                    Toast("error", "Access Denied", "You do not have permission to restore employees.", 0);
                    return;
                }

                RestoreEmployee(employeeID);
            }
            else if (e.CommandName == "DeleteEmp")
            {
                if (!HasDeletePermission(userId, "ManageEmployees"))
                {
                    Toast("error", "Access Denied", "You do not have permission to delete employees.", 0);
                    return;
                }

                TryHardDelete(employeeID);
            }
        }

        private void gvArchivedEmployees_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            e.Cancel = true;

            if (e.RowIndex < 0 || e.RowIndex >= gvArchivedEmployees.Rows.Count)
                return;

            int employeeID = Convert.ToInt32(gvArchivedEmployees.DataKeys[e.RowIndex].Value);
            int userId = Convert.ToInt32(Session["UserID"]);

            if (!HasDeletePermission(userId, "ManageEmployees"))
            {
                Toast("error", "Access Denied", "You do not have permission to delete employees.", 0);
                return;
            }

            TryHardDelete(employeeID);
        }

        private void RestoreEmployee(int employeeID)
        {
            try
            {
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spEmployee_Restore", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = employeeID;

                    con.Open();
                    var rows = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                }

                LoadArchivedEmployees();
                Toast("success", "Restored!", "Employee has been restored successfully.", 1500);
            }
            catch (Exception ex)
            {
                Toast("error", "Error", $"Restore failed: {ex.Message}", 0);
            }
        }

        private void TryHardDelete(int employeeID)
        {
            try
            {
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spEmployee_HardDelete", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = employeeID;

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                LoadArchivedEmployees();
                Toast("success", "Deleted!", "Employee has been permanently removed.", 1500);
            }
            catch (SqlException ex)
            {
                Toast("error", "Cannot Delete", $"Delete failed: {ex.Message}", 0);
            }
            catch (Exception ex)
            {
                Toast("error", "Error", $"Delete failed: {ex.Message}", 0);
            }
        }

        // Permission checks
        private bool HasEditPermission(int adminId, string moduleName)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAdminPermission_CanEdit", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = adminId;
                    cmd.Parameters.Add("@ModuleName", SqlDbType.NVarChar, 100).Value = moduleName;

                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null && Convert.ToBoolean(result);
                }
            }
            catch
            {
                return false; // deny by default if SP missing
            }
        }

        private bool HasDeletePermission(int adminId, string moduleName)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spAdminPermission_CanDelete", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = adminId;
                cmd.Parameters.Add("@ModuleName", SqlDbType.NVarChar, 100).Value = moduleName;

                conn.Open();
                object result = cmd.ExecuteScalar();
                return result != null && Convert.ToBoolean(result);
            }
        }

        private void Toast(string icon, string title, string text, int timerMs = 2000)
        {
            string extra = timerMs > 0 ? $"showConfirmButton:false, timer:{timerMs}" : "showConfirmButton:true";
            string script = $@"Swal.fire({{
                icon: '{icon}',
                title: '{title}',
                text: '{text}',
                {extra}
            }});";
            ClientScript.RegisterStartupScript(this.GetType(), Guid.NewGuid().ToString(), script, true);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            int userId = Convert.ToInt32(Session["UserID"]);

            foreach (GridViewRow row in gvArchivedEmployees.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    string employeeId = gvArchivedEmployees.DataKeys[row.RowIndex].Value.ToString();

                    LinkButton btnRestore = (LinkButton)row.FindControl("btnRestore");
                    LinkButton btnDelete = (LinkButton)row.FindControl("btnDelete");

                    if (btnRestore != null)
                        btnRestore.Enabled = HasEditPermission(userId, "ManageEmployees");

                    if (btnDelete != null)
                        btnDelete.Enabled = HasEditPermission(userId, "ManageEmployees");

                    // Required for event validation
                    ClientScript.RegisterForEventValidation(gvArchivedEmployees.UniqueID, "Restore$" + employeeId);
                    ClientScript.RegisterForEventValidation(gvArchivedEmployees.UniqueID, "DeleteEmp$" + employeeId);
                }
            }

            base.Render(writer);
        }
    }
}