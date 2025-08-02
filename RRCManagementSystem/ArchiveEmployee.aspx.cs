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

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadArchivedEmployees();
            }
        }

        private void LoadArchivedEmployees()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT EmployeeID, LastName, FirstName, MiddleName, Position FROM Employees WHERE Status = 'Inactive'";
                SqlDataAdapter adapter = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                gvArchivedEmployees.DataSource = dt;
                gvArchivedEmployees.DataBind();
            }
        }

        protected void gvArchivedEmployees_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int employeeID = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "Restore")
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Employees SET Status = 'Active' WHERE EmployeeID = @EmployeeID";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@EmployeeID", employeeID);
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                LoadArchivedEmployees();

                // ✅ SweetAlert Toast: Restore Success
                string script = @"Swal.fire({
                    icon: 'success',
                    title: 'Restored!',
                    text: 'Employee has been restored successfully.',
                    showConfirmButton: false,
                    timer: 1500
                });";
                ClientScript.RegisterStartupScript(this.GetType(), "restoreSuccess", script, true);
            }
            else if (e.CommandName == "Delete")
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM Employees WHERE EmployeeID = @EmployeeID";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@EmployeeID", employeeID);
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                LoadArchivedEmployees();

                // ✅ SweetAlert Toast: Delete Success
                string script = @"Swal.fire({
                    icon: 'success',
                    title: 'Deleted!',
                    text: 'Employee has been deleted permanently.',
                    showConfirmButton: false,
                    timer: 1500
                });";
                ClientScript.RegisterStartupScript(this.GetType(), "deleteSuccess", script, true);
            }
        }

        // ✅ Register expected postback events to fix "Invalid postback" error
        protected override void Render(HtmlTextWriter writer)
        {
            foreach (GridViewRow row in gvArchivedEmployees.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    string employeeId = gvArchivedEmployees.DataKeys[row.RowIndex].Value.ToString();
                    ClientScript.RegisterForEventValidation(gvArchivedEmployees.UniqueID, "Restore$" + employeeId);
                    ClientScript.RegisterForEventValidation(gvArchivedEmployees.UniqueID, "Delete$" + employeeId);
                }
            }

            base.Render(writer); // Call base AFTER registering
        }

    }
}
