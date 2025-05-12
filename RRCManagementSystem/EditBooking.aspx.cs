using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class EditBooking : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["AdminID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int adminId = Convert.ToInt32(Session["AdminID"]);

            if (!IsPostBack)
            {
                if (!HasEditPermission(adminId, "ManageBooking"))
                {
                    lblMessage.Text = "❌ You do not have permission to edit bookings.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    btnSave.Enabled = false;
                    return;
                }

                if (Request.QueryString["BookingID"] != null && int.TryParse(Request.QueryString["BookingID"], out int bookingID))
                {
                    LoadBookingDetails(bookingID);
                }
                else
                {
                    lblMessage.Text = "❌ Invalid or missing Booking ID.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
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
                    catch (Exception ex)
                    {
                        lblMessage.Text = $"❌ Permission check failed: {ex.Message}";
                        lblMessage.ForeColor = System.Drawing.Color.Red;
                        return false;
                    }
                }
            }
        }

        private void LoadBookingDetails(int bookingID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT b.BookingID, c.Name AS ClientName, s.Name AS ServiceName, 
                               b.ScheduledDate, b.StartTime, b.Status, b.Notes
                        FROM Bookings b
                        LEFT JOIN Clients c ON b.ClientID = c.ClientID
                        LEFT JOIN Services s ON b.ServiceID = s.ServiceID
                        WHERE b.BookingID = @BookingID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@BookingID", bookingID);
                        con.Open();

                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            lblBookingID.Text = reader["BookingID"].ToString();
                            txtClientName.Text = reader["ClientName"].ToString();
                            txtServiceName.Text = reader["ServiceName"].ToString();
                            txtScheduledDate.Text = Convert.ToDateTime(reader["ScheduledDate"]).ToString("yyyy-MM-dd");
                            txtStartTime.Text = reader["StartTime"].ToString();
                            ddlStatus.SelectedValue = reader["Status"].ToString();
                            txtNotes.Text = reader["Notes"].ToString();
                        }
                        else
                        {
                            lblMessage.Text = "❌ Booking not found.";
                            lblMessage.ForeColor = System.Drawing.Color.Red;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"❌ Error loading booking: {ex.Message}";
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int adminId = Convert.ToInt32(Session["AdminID"]);

            if (!HasEditPermission(adminId, "ManageBooking"))
            {
                lblMessage.Text = "❌ You do not have permission to edit bookings.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"
                        UPDATE Bookings
                        SET Status = @Status
                        WHERE BookingID = @BookingID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        int bookingID = Convert.ToInt32(lblBookingID.Text);
                        string newStatus = ddlStatus.SelectedValue;

                        cmd.Parameters.AddWithValue("@Status", newStatus);
                        cmd.Parameters.AddWithValue("@BookingID", bookingID);

                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            lblMessage.Text = "✅ Booking status updated successfully!";
                            lblMessage.ForeColor = System.Drawing.Color.Green;

                            // ✅ Log the edit to AuditLogs
                            AddAuditLog(adminId, $"Updated status of Booking ID {bookingID} to '{newStatus}'");
                        }
                        else
                        {
                            lblMessage.Text = "❌ Failed to update booking.";
                            lblMessage.ForeColor = System.Drawing.Color.Red;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"❌ Error updating booking: {ex.Message}";
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("AllBooking.aspx");
        }

        // ✅ Audit Log Method
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
                        // Optional: log or ignore
                    }
                }
            }
        }
    }
}