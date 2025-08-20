using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class ArchiveEmployee : System.Web.UI.Page
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

            if (!IsPostBack)
            {
                LoadArchivedEmployees();

                if (Request.QueryString["archived"] == "1")
                {
                    ClientScript.RegisterStartupScript(this.GetType(), "archivedOk", @"
                        Swal.fire({ icon:'success', title:'Archived!',
                                    text:'Employee has been archived successfully.',
                                    showConfirmButton:false, timer:1500 });", true);
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

        protected void gvArchivedEmployees_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!int.TryParse(e.CommandArgument?.ToString(), out int employeeID))
                return;

            if (e.CommandName == "Restore")
            {
                RestoreEmployee(employeeID);
            }
            else if (e.CommandName == "DeleteEmp")
            {
                TryHardDelete(employeeID);
            }
        }

        private void gvArchivedEmployees_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            e.Cancel = true;

            if (e.RowIndex < 0 || e.RowIndex >= gvArchivedEmployees.Rows.Count)
                return;

            int employeeID = Convert.ToInt32(gvArchivedEmployees.DataKeys[e.RowIndex].Value);
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
            foreach (GridViewRow row in gvArchivedEmployees.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    string employeeId = gvArchivedEmployees.DataKeys[row.RowIndex].Value.ToString();

                    ClientScript.RegisterForEventValidation(gvArchivedEmployees.UniqueID, "Restore$" + employeeId);
                    ClientScript.RegisterForEventValidation(gvArchivedEmployees.UniqueID, "DeleteEmp$" + employeeId);
                    ClientScript.RegisterForEventValidation(gvArchivedEmployees.UniqueID, "Delete$" + employeeId);
                }
            }
            base.Render(writer);
        }
    }
}
