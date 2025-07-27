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
    c.Name AS ClientName,
    b.ServiceNames AS ServiceName,
    b.ScheduledDate,
    b.StartTime,
    b.Status,
    b.CreatedAt,
    b.Price,
    ISNULL(b.Price, 0) - ISNULL((SELECT SUM(t.Amount) FROM Transactions t WHERE t.SaleID = b.BookingID), 0) AS RemainingBalance,
    (SELECT TOP 1 Status FROM ServiceSchedule WHERE BookingID = b.BookingID AND OperationNumber = 1) AS Op1Status
FROM Bookings b
INNER JOIN Clients c ON b.ClientID = c.ClientID
WHERE 
    (@SearchTerm IS NULL OR c.Name LIKE '%' + @SearchTerm + '%' OR b.ServiceNames LIKE '%' + @SearchTerm + '%')
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
            string bookingId = e.CommandArgument.ToString();

            if (e.CommandName == "EditBooking")
            {
                Response.Redirect($"EditBooking.aspx?BookingID={bookingId}");
            }
            else if (e.CommandName == "CompleteOp1")
            {
                MarkOp1AsCompleted(Convert.ToInt32(bookingId));
                LoadAllBookings();
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
                            lblMessage.Text = "✅ Operation 1 marked as completed.";
                            lblMessage.ForeColor = System.Drawing.Color.Green;
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

        protected void gvBookings_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                string status = DataBinder.Eval(e.Row.DataItem, "Status")?.ToString();

                if (status == "Assigned")
                    e.Row.Cells[5].CssClass = "status-assigned";
                else if (status == "Pending")
                    e.Row.Cells[5].CssClass = "status-pending";
                else if (status == "Cancelled")
                    e.Row.Cells[5].CssClass = "status-cancelled";
            }
        }
    }
}
