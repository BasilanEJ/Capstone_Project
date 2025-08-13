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
            // Safety: if markup ever uses CommandName="Delete", cancel built-in delete
            gvArchivedEmployees.RowDeleting += gvArchivedEmployees_RowDeleting;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // prevent stale cache
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
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // Show all Inactive (case/space-insensitive)
                string query = @"
                    SELECT EmployeeID, LastName, FirstName, MiddleName, Position, Status
                    FROM Employees
                    WHERE UPPER(LTRIM(RTRIM(Status))) = 'INACTIVE'
                    ORDER BY EmployeeID;";

                SqlDataAdapter adapter = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

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
                LoadArchivedEmployees();
                Toast("success", "Restored!", "Employee has been restored successfully.", 1500);
            }
            else if (e.CommandName == "DeleteEmp") // hard delete
            {
                TryHardDelete(employeeID);
            }
        }

        // If built-in Delete ever fires, cancel and run hard delete
        private void gvArchivedEmployees_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            e.Cancel = true;

            if (e.RowIndex < 0 || e.RowIndex >= gvArchivedEmployees.Rows.Count)
                return;

            int employeeID = Convert.ToInt32(gvArchivedEmployees.DataKeys[e.RowIndex].Value);
            TryHardDelete(employeeID);
        }

        private void TryHardDelete(int employeeID)
        {
            try
            {
                HardDeleteEmployee(employeeID);
                LoadArchivedEmployees();
                Toast("success", "Deleted!", "Employee has been permanently removed.", 1500);
            }
            catch (SqlException ex)
            {
                // FK/constraint issues etc.
                Toast("error", "Cannot Delete", $"Delete failed: {ex.Message}", 0);
            }
            catch (Exception ex)
            {
                Toast("error", "Error", $"Delete failed: {ex.Message}", 0);
            }
        }

        private void RestoreEmployee(int employeeID)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(
                "UPDATE Employees SET Status = 'Active' WHERE EmployeeID = @EmployeeID", con))
            {
                cmd.Parameters.AddWithValue("@EmployeeID", employeeID);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // 🔥 HARD DELETE: remove dependents first, then the employee (transactional)
        private void HardDeleteEmployee(int employeeID)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                using (SqlTransaction tx = con.BeginTransaction())
                {
                    try
                    {
                        // 1) Delete child rows referencing Employees.EmployeeID
                        using (SqlCommand cmd = new SqlCommand(
                            "DELETE FROM TeamMembers WHERE EmployeeID = @EmployeeID", con, tx))
                        {
                            cmd.Parameters.AddWithValue("@EmployeeID", employeeID);
                            cmd.ExecuteNonQuery();
                        }

                        // TODO: if other tables reference Employees, delete them here too:
                        // Example:
                        // using (SqlCommand cmd = new SqlCommand(
                        //     "DELETE FROM SomeOtherTable WHERE EmployeeID = @EmployeeID", con, tx)) { ... }

                        // 2) Delete the employee row
                        using (SqlCommand cmd = new SqlCommand(
                            "DELETE FROM Employees WHERE EmployeeID = @EmployeeID", con, tx))
                        {
                            cmd.Parameters.AddWithValue("@EmployeeID", employeeID);
                            int rows = cmd.ExecuteNonQuery();
                            if (rows == 0)
                                throw new InvalidOperationException("Employee not found.");
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        private void Toast(string icon, string title, string text, int timerMs = 2000)
        {
            // timerMs = 0 -> show confirm button (no auto close)
            string extra = timerMs > 0 ? $"showConfirmButton:false, timer:{timerMs}" : "showConfirmButton:true";
            string script = $@"Swal.fire({{
                icon: '{icon}',
                title: '{title}',
                text: '{text}',
                {extra}
            }});";
            ClientScript.RegisterStartupScript(this.GetType(), Guid.NewGuid().ToString(), script, true);
        }

        // Allow our custom __doPostBack commands
        protected override void Render(HtmlTextWriter writer)
        {
            foreach (GridViewRow row in gvArchivedEmployees.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    string employeeId = gvArchivedEmployees.DataKeys[row.RowIndex].Value.ToString();

                    ClientScript.RegisterForEventValidation(gvArchivedEmployees.UniqueID, "Restore$" + employeeId);
                    ClientScript.RegisterForEventValidation(gvArchivedEmployees.UniqueID, "DeleteEmp$" + employeeId);
                    // In case built-in Delete is ever used again:
                    ClientScript.RegisterForEventValidation(gvArchivedEmployees.UniqueID, "Delete$" + employeeId);
                }
            }

            base.Render(writer);
        }
    }
}
