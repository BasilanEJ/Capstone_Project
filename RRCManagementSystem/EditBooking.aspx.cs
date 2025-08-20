using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class EditBooking : System.Web.UI.Page
    {
        private readonly string cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

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

            if (!IsPostBack)
            {
                int adminId = Convert.ToInt32(Session["UserID"]);
                if (!HasEditPermission(adminId, "ManageBooking"))
                {
                    lblMessage.Text = "❌ You do not have permission to edit bookings.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    btnSave.Enabled = false;
                    return;
                }

                if (int.TryParse(Request.QueryString["BookingID"], out int bookingID))
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
            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spAdminPermission_Check", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = adminId;
                    cmd.Parameters.Add("@ModuleName", SqlDbType.NVarChar, 100).Value = moduleName;
                    cmd.Parameters.Add("@Permission", SqlDbType.NVarChar, 10).Value = "CanEdit";

                    con.Open();
                    object scalar = cmd.ExecuteScalar();
                    return scalar != null && scalar != DBNull.Value && Convert.ToBoolean(scalar);
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"❌ Permission check failed: {ex.Message}";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return false;
            }
        }

        private void LoadBookingDetails(int bookingID)
        {
            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spBooking_GetForEdit", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingID;

                    con.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            lblBookingID.Text = r["BookingID"].ToString();
                            txtClientName.Text = r["ClientName"]?.ToString();
                            txtServiceName.Text = r["ServiceNames"]?.ToString();

                            if (r["ScheduledDate"] != DBNull.Value)
                                txtScheduledDate.Text = Convert.ToDateTime(r["ScheduledDate"]).ToString("yyyy-MM-dd");

                            // StartTime is time(7) → TimeSpan
                            if (r["StartTime"] != DBNull.Value)
                                txtStartTime.Text = TimeSpan.Parse(r["StartTime"].ToString()).ToString(@"hh\:mm");

                            ddlStatus.SelectedValue = r["Status"]?.ToString() ?? "Pending";
                            txtNotes.Text = r["Notes"]?.ToString() ?? "";
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
            int adminId = Convert.ToInt32(Session["UserID"]);
            if (!HasEditPermission(adminId, "ManageBooking"))
            {
                lblMessage.Text = "❌ You do not have permission to edit bookings.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            if (!int.TryParse(lblBookingID.Text, out int bookingID))
            {
                lblMessage.Text = "❌ Invalid Booking ID.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            string newStatus = ddlStatus.SelectedValue;

            try
            {
                int affected = 0;
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spBooking_UpdateStatus", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingID;
                    cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = newStatus;

                    con.Open();
                    object result = cmd.ExecuteScalar(); // proc returns RowsAffected
                    affected = (result == null || result == DBNull.Value) ? 0 : Convert.ToInt32(result);
                }

                if (affected > 0)
                {
                    lblMessage.Text = "✅ Booking status updated successfully!";
                    lblMessage.ForeColor = System.Drawing.Color.Green;

                    // audit
                    AddAuditLog(adminId, $"Updated status of Booking ID {bookingID} to '{newStatus}'");
                }
                else
                {
                    lblMessage.Text = "❌ Failed to update booking.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
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

        private void AddAuditLog(int? userID, string action)
        {
            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spAudit_Insert", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@AdminID", SqlDbType.Int).Value = (object)userID ?? DBNull.Value;
                    cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 255).Value = action;

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch { /* don't block UI if audit fails */ }
        }
    }
}
