using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class AllBooking : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string role = Session["Role"].ToString();
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);
            if (!HasViewPermission(userId, "ManageBooking"))
            {
                Response.Redirect("~/Unauthorized.aspx");
                return;
            }
            if (!IsPostBack)
            {
                LoadAllBookings();

                if (Request.QueryString["op1"] == "completed")
                {
                    string script = @"Swal.fire({
                icon: 'success',
                title: 'Completed!',
                text: 'Operation 1 was successfully marked as completed.',
                showConfirmButton: false,
                timer: 2000
            });";
                    ClientScript.RegisterStartupScript(this.GetType(), "ShowSuccess", script, true);
                }
            }
        }

        private bool HasViewPermission(int adminId, string moduleName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT CanView FROM AdminPermissions WHERE UserID = @UserID AND ModuleName = @ModuleName";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", adminId);
                    cmd.Parameters.AddWithValue("@ModuleName", moduleName);

                    try
                    {
                        conn.Open();
                        object result = cmd.ExecuteScalar();
                        return result != null && result != DBNull.Value && Convert.ToBoolean(result);
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        }

        private void LoadAllBookings()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string query = @"
SELECT 
    b.BookingID,
    (c.LastName + ', ' + c.FirstName + ' ' + ISNULL(c.MiddleName, '')) AS ClientName,
    b.ServiceNames AS ServiceName,
    b.ScheduledDate,
    b.StartTime,
    b.Status,
    b.CreatedAt,
    b.Price,
    ISNULL(b.Price, 0) - ISNULL((SELECT SUM(t.Amount) FROM Transactions t WHERE t.SaleID = b.BookingID), 0) AS RemainingBalance,
    b.IsContract,
    CASE 
        WHEN b.IsContract = 1 
             THEN ISNULL((SELECT TOP 1 Status 
                          FROM ServiceSchedule 
                          WHERE BookingID = b.BookingID AND OperationNumber = 1), 'Pending')
        ELSE NULL
    END AS Op1Status
FROM Bookings b
INNER JOIN Clients c ON b.ClientID = c.ClientID
WHERE 
    (@SearchTerm IS NULL OR c.LastName LIKE '%' + @SearchTerm + '%' 
     OR c.FirstName LIKE '%' + @SearchTerm + '%' 
     OR b.ServiceNames LIKE '%' + @SearchTerm + '%')
    AND (@Status IS NULL OR b.Status = @Status)
ORDER BY b.CreatedAt DESC";


                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@SearchTerm", string.IsNullOrEmpty(txtSearch.Text.Trim()) ? (object)DBNull.Value : txtSearch.Text.Trim());
                    cmd.Parameters.AddWithValue("@Status", string.IsNullOrEmpty(ddlStatusFilter.SelectedValue) ? (object)DBNull.Value : ddlStatusFilter.SelectedValue);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvBookings.DataSource = dt;
                    gvBookings.DataBind();

                    lblMessage.Text = $"{dt.Rows.Count} booking(s) found.";
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"❌ Error loading bookings: {ex.Message}";
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void txtSearch_TextChanged(object sender, EventArgs e)
        {
            gvBookings.PageIndex = 0;
            LoadAllBookings();
        }

        protected void ddlStatusFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            gvBookings.PageIndex = 0;
            LoadAllBookings();
        }

        protected void gvBookings_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvBookings.PageIndex = e.NewPageIndex;
            LoadAllBookings();
        }

        protected void gvBookings_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            lblMessage.Text = $"Command triggered: {e.CommandName} for Argument: {e.CommandArgument}";
            lblMessage.ForeColor = System.Drawing.Color.Black;

            if (e.CommandName == "EditBooking")
            {
                int bookingId;
                if (int.TryParse(e.CommandArgument.ToString(), out bookingId))
                {
                    Response.Redirect($"EditBooking.aspx?BookingID={bookingId}");
                }
            }
        }




        private void MarkOp1AsCompleted(int bookingId)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string updateQuery = @"
UPDATE ServiceSchedule
SET Status = 'Completed'
WHERE BookingID = @BookingID AND OperationNumber = 1";

                    using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@BookingID", bookingId);
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // ✅ Explicit audit log line as requested
                            AddAuditLog(Convert.ToInt32(Session["UserID"]), $"Marked Op1 completed for BookingID {bookingId}");

                            string script = @"Swal.fire({
                        icon: 'success',
                        title: 'Marked Completed!',
                        text: 'Operation 1 was successfully marked as completed.',
                        showConfirmButton: false,
                        timer: 2000
                    });";
                            ClientScript.RegisterStartupScript(this.GetType(), "CompleteSuccess", script, true);
                        }
                        else
                        {
                            lblMessage.Text = "⚠️ Operation 1 not found or already completed.";
                            lblMessage.ForeColor = System.Drawing.Color.Orange;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"❌ Error updating Op1: {ex.Message}";
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }


        protected void btnHiddenCompleteOp1_Click(object sender, EventArgs e)
        {
            if (int.TryParse(hfBookingIDToComplete.Value, out int bookingId))
            {
                // Update operation status
                MarkOp1AsCompleted(bookingId);

                // Redirect to trigger full page reload with SweetAlert
                Response.Redirect("AllBooking.aspx?op1=completed");
            }
        }




        protected void gvBookings_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            // Existing status coloring
            string bookingStatus = DataBinder.Eval(e.Row.DataItem, "Status")?.ToString();
            if (bookingStatus == "Assigned") e.Row.Cells[5].CssClass = "status-assigned";
            else if (bookingStatus == "Pending") e.Row.Cells[5].CssClass = "status-pending";
            else if (bookingStatus == "Cancelled") e.Row.Cells[5].CssClass = "status-cancelled";

            // Contract-only controls
            bool isContract = false;
            var isContractObj = DataBinder.Eval(e.Row.DataItem, "IsContract");
            if (isContractObj != null && isContractObj != DBNull.Value)
                isContract = Convert.ToBoolean(isContractObj);

            string op1Status = DataBinder.Eval(e.Row.DataItem, "Op1Status")?.ToString();

            var lblOp1 = (Label)e.Row.FindControl("lblOp1Status");
            var btnOp1 = (Button)e.Row.FindControl("btnTriggerCompleteOp1");

            // Show OP1 status column value only for contracts
            if (lblOp1 != null)
                lblOp1.Visible = isContract;

            // Show action only for contracts, when booking is Assigned and OP1 not yet Completed
            if (btnOp1 != null)
                btnOp1.Visible = isContract
                                 && string.Equals(bookingStatus, "Assigned", StringComparison.OrdinalIgnoreCase)
                                 && !string.Equals(op1Status, "Completed", StringComparison.OrdinalIgnoreCase);
        }

        private void AddAuditLog(int adminId, string action)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO AuditLogs (AdminID, Action, Timestamp) VALUES (@AdminID, @Action, GETDATE())";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@AdminID", adminId);
                    cmd.Parameters.AddWithValue("@Action", action);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

    }
}
