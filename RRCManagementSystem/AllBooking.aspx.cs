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
            // 🔐 Require login
            if (Session["AdminID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int adminId = Convert.ToInt32(Session["AdminID"]);

            // 🔐 Check CanView permission for ManageBooking
            if (!HasViewPermission(adminId, "ManageBooking"))
            {
                Response.Redirect("~/Unauthorized.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadAllBookings();
                LoadAllInquiries();
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
                        return false; // Fail-safe: deny access if permission check fails
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
    ISNULL(b.Price, 0) - ISNULL((
        SELECT SUM(t.Amount) 
        FROM Transactions t 
        WHERE t.SaleID = b.BookingID
    ), 0) AS RemainingBalance
FROM Bookings b
INNER JOIN Clients c ON b.ClientID = c.ClientID
ORDER BY b.CreatedAt DESC
";

                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvBookings.DataSource = dt;
                    gvBookings.DataBind();

                    lblMessage.Text = $"{dt.Rows.Count} booking(s) loaded.";
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"❌ Error loading bookings: {ex.Message}";
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }


        private void LoadAllInquiries()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string query = @"
                        SELECT 
                            i.InquiryID,
                            ISNULL(c.Name, 'N/A') AS ClientName,
                            i.Email,
                            ISNULL(c.ContactNumber, 'N/A') AS ContactNumber,
                            ISNULL(s.Name, 'N/A') AS ServiceName,
                            ISNULL(i.Message, '') AS Message,
                            ISNULL(i.SentAt, GETDATE()) AS SentAt
                        FROM Inquiry i
                        INNER JOIN Clients c ON i.ClientID = c.ClientID
                        LEFT JOIN Services s ON i.ServiceID = s.ServiceID
                        ORDER BY i.SentAt DESC";

                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvInquiries.DataSource = dt;
                    gvInquiries.DataBind();

                    lblMessageInquiry.Text = $"{dt.Rows.Count} inquiry(s) loaded.";
                    lblMessageInquiry.ForeColor = System.Drawing.Color.Green;
                }
            }
            catch (Exception ex)
            {
                lblMessageInquiry.Text = $"❌ Error loading inquiries: {ex.Message}";
                lblMessageInquiry.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void gvBookings_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditBooking")
            {
                string bookingId = e.CommandArgument.ToString();
                Response.Redirect($"EditBooking.aspx?BookingID={bookingId}");
            }
        }

        protected void gvBookings_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvBookings.PageIndex = e.NewPageIndex;
            LoadAllBookings();
        }

        protected void gvInquiries_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvInquiries.PageIndex = e.NewPageIndex;
            LoadAllInquiries();
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