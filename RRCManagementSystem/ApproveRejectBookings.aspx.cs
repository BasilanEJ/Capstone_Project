using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace RRCManagementSystem
{
    public partial class ApproveRejectBookings : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 🔐 Require login
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string role = Session["Role"].ToString();

            // 🔐 Block SuperAdmin and Inspector
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            // 🔐 Check CanEdit permission for ManageBooking
            if (!HasEditPermission(userId, "ManageBooking"))
            {
                lblMessage.Text = "❌ You do not have permission to approve or reject bookings.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                gvBookings.Visible = false;
                return;
            }

            if (!IsPostBack)
            {
                LoadPendingBookings();
            }
        }

        private bool HasEditPermission(int adminId, string moduleName)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT CanEdit FROM AdminPermissions WHERE UserID = @UserID AND ModuleName = @ModuleName";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserID", adminId);
                    cmd.Parameters.AddWithValue("@ModuleName", moduleName);

                    try
                    {
                        con.Open();
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

        private void LoadPendingBookings()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"
SELECT 
    b.BookingID,
    (c.LastName + ', ' + c.FirstName + ' ' + ISNULL(c.MiddleName, '')) AS ClientName,
    b.ServiceNames AS ServiceName,
    b.ScheduledDate,
    b.StartTime,
    b.Status,
    b.SQM,
    b.Price
FROM Bookings b
INNER JOIN Clients c ON b.ClientID = c.ClientID
WHERE b.Status = 'Pending'
ORDER BY b.ScheduledDate ASC";


                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvBookings.DataSource = dt;
                    gvBookings.DataBind();

                    lblMessage.Text = dt.Rows.Count == 0
                        ? "No pending bookings found."
                        : "";
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠️ Error loading bookings: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void gvBookings_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            if (!int.TryParse(e.CommandArgument.ToString(), out int bookingID))
            {
                lblMessage.Text = "⚠️ Invalid Booking ID.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            int adminId = Convert.ToInt32(Session["AdminID"]);

            if (e.CommandName == "Approve")
            {
                if (UpdateBookingStatus(bookingID, "Approved"))
                {
                    decimal price = GetBookingPrice(bookingID);
                    int sqm = GetBookingSQM(bookingID);
                    string serviceName = GetServiceName(bookingID);
                    DateTime scheduledDate = GetScheduledDate(bookingID);

                    Session["BookingID"] = bookingID;
                    Session["Price"] = price;
                    Session["SQM"] = sqm;
                    Session["ServiceName"] = serviceName;
                    Session["ScheduledDate"] = scheduledDate;

                    lblMessage.Text = $"✅ Booking {bookingID} approved! Redirecting to assign team...";
                    lblMessage.ForeColor = System.Drawing.Color.Green;

                    // ✅ Log approval
                    AddAuditLog(adminId, $"Approved booking ID: {bookingID}");

                    Response.AddHeader("REFRESH", "1.5;URL=AssignBooking.aspx?BookingID=" + bookingID);
                }
                else
                {
                    lblMessage.Text = $"❌ Failed to approve booking {bookingID}.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
            }
            else if (e.CommandName == "Reject")
            {
                if (UpdateBookingStatus(bookingID, "Rejected"))
                {
                    lblMessage.Text = $"⚠️ Booking {bookingID} rejected.";
                    lblMessage.ForeColor = System.Drawing.Color.OrangeRed;

                    // ✅ Log rejection
                    AddAuditLog(adminId, $"Rejected booking ID: {bookingID}");

                    LoadPendingBookings();
                }
                else
                {
                    lblMessage.Text = $"❌ Failed to reject booking {bookingID}.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
            }
        }

        private bool UpdateBookingStatus(int bookingID, string status)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Bookings SET Status = @Status WHERE BookingID = @BookingID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Status", status);
                        cmd.Parameters.AddWithValue("@BookingID", bookingID);

                        con.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"⚠️ Error updating booking status: {ex.Message}";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return false;
            }
        }

        private decimal GetBookingPrice(int bookingID)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT Price FROM Bookings WHERE BookingID = @BookingID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@BookingID", bookingID);
                con.Open();
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }

        private int GetBookingSQM(int bookingID)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT SQM FROM Bookings WHERE BookingID = @BookingID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@BookingID", bookingID);
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private string GetServiceName(int bookingID)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT STRING_AGG(s.Name, ', ') AS ServiceNames
            FROM BookingServices bs
            INNER JOIN Services s ON bs.ServiceID = s.ServiceID
            WHERE bs.BookingID = @BookingID";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@BookingID", bookingID);
                con.Open();
                return cmd.ExecuteScalar()?.ToString() ?? "";
            }
        }


        private DateTime GetScheduledDate(int bookingID)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT ScheduledDate FROM Bookings WHERE BookingID = @BookingID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@BookingID", bookingID);
                con.Open();
                return (DateTime)cmd.ExecuteScalar();
            }
        }

        // ✅ AddAuditLog method for recording actions
        private void AddAuditLog(int? userID, string action)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO AuditLogs (AdminID, Action, Timestamp) VALUES (@AdminID, @Action, GETDATE())";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@AdminID", (object)userID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Action", action);

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch
                    {
                        // Optionally handle errors silently
                    }
                }
            }
        }
    }
}