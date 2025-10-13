using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class ApproveRejectBookings : System.Web.UI.Page
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
                    object allowed = cmd.ExecuteScalar();
                    return allowed != null && allowed != DBNull.Value && Convert.ToBoolean(allowed);
                }
            }
            catch
            {
                return false;
            }
        }

        private void LoadPendingBookings()
        {
            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spBooking_ListPendingForApproval", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        gvBookings.DataSource = dt;
                        gvBookings.DataBind();

                        lblMessage.Text = dt.Rows.Count == 0 ? "No pending bookings found." : string.Empty;
                        lblMessage.ForeColor = System.Drawing.Color.Green;
                    }
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

            int adminId = Convert.ToInt32(Session["UserID"]);

            if (e.CommandName == "Approve")
            {
                int clientId = GetClientIdFromBooking(bookingID);

                // 🔍 Check if client has contract first
                if (!HasClientContract(clientId))
                {
                    // ⚠️ Show SweetAlert and redirect to ManageContract.aspx
                    string js = $@"
                        Swal.fire({{
                            icon: 'warning',
                            title: 'No Contract Found',
                            text: 'This client does not have a contract yet. Please upload one before approving.',
                            confirmButtonText: 'Go to Contract Upload',
                            confirmButtonColor: '#2563eb'
                        }}).then((result) => {{
                            if (result.isConfirmed) {{
                                window.location.href = 'ManageContract.aspx?ClientID={clientId}';
                            }}
                        }});";
                    ScriptManager.RegisterStartupScript(this, GetType(), "NoContractAlert", js, true);
                    return;
                }

                // ✅ Proceed with normal approval
                if (ApproveBookingAndInsertBalance(bookingID, adminId))
                {
                    var info = GetBookingBasics(bookingID);
                    Session["BookingID"] = bookingID;
                    Session["Price"] = info.Price;
                    Session["SQM"] = info.SQM;
                    Session["ServiceName"] = info.ServiceNames;
                    Session["ScheduledDate"] = info.ScheduledDate;

                    lblMessage.Text = $"✅ Booking {bookingID} approved! Redirecting to assign team...";
                    lblMessage.ForeColor = System.Drawing.Color.Green;

                    AddAuditLog(adminId, $"Approved booking ID: {bookingID} and balance initialized.");

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
                if (SetBookingStatus(bookingID, "Rejected"))
                {
                    LoadPendingBookings();
                    AddAuditLog(adminId, $"Rejected booking ID: {bookingID}");

                    string js = $@"Swal.fire({{
                        icon: 'success',
                        title: 'Booking Rejected',
                        text: 'Booking #{bookingID} has been rejected.',
                        confirmButtonColor: '#dc3545'
                    }}).then((result) => {{
                        if (result.isConfirmed) {{
                            window.location.href = 'ApproveRejectBookings.aspx';
                        }}
                    }});";

                    ScriptManager.RegisterStartupScript(this, GetType(), "RejectOK", js, true);
                }
                else
                {
                    string js = $@"Swal.fire({{
                        icon: 'error',
                        title: 'Failed to Reject',
                        text: 'We could not reject booking #{bookingID}. Please try again.',
                        confirmButtonColor: '#6c757d'
                    }});";
                    ScriptManager.RegisterStartupScript(this, GetType(), "RejectFail", js, true);
                }
            }
        }

        // 🔹 Check if client has contract
        private bool HasClientContract(int clientId)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.ClientContracts WHERE ClientID = @ClientID", con))
            {
                cmd.Parameters.AddWithValue("@ClientID", clientId);
                con.Open();
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        // 🔹 Get the ClientID from booking
        private int GetClientIdFromBooking(int bookingID)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("SELECT ClientID FROM dbo.Bookings WHERE BookingID = @BookingID", con))
            {
                cmd.Parameters.AddWithValue("@BookingID", bookingID);
                con.Open();
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        private bool ApproveBookingAndInsertBalance(int bookingID, int adminId)
        {
            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spBooking_ApproveAndInsertBalance", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingID;
                    cmd.Parameters.Add("@AdminID", SqlDbType.Int).Value = adminId;

                    con.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null && Convert.ToInt32(result) == 1;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"⚠️ Error approving booking: {ex.Message}";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return false;
            }
        }

        private bool SetBookingStatus(int bookingID, string status)
        {
            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spBooking_SetStatus", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingID;
                    cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = status;

                    con.Open();
                    object rows = cmd.ExecuteScalar();
                    return rows != null && rows != DBNull.Value && Convert.ToInt32(rows) > 0;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"⚠️ Error updating booking status: {ex.Message}";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return false;
            }
        }

        private (decimal Price, int SQM, string ServiceNames, DateTime ScheduledDate) GetBookingBasics(int bookingID)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.spBooking_GetBasics", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingID;

                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        decimal price = r["Price"] != DBNull.Value ? Convert.ToDecimal(r["Price"]) : 0m;
                        int sqm = r["SQM"] != DBNull.Value ? Convert.ToInt32(r["SQM"]) : 0;
                        string services = r["ServiceNames"]?.ToString() ?? "";
                        DateTime sched = r["ScheduledDate"] != DBNull.Value ? Convert.ToDateTime(r["ScheduledDate"]) : DateTime.MinValue;
                        return (price, sqm, services, sched);
                    }
                }
            }
            return (0m, 0, "", DateTime.MinValue);
        }

        private void AddAuditLog(int? userID, string action)
        {
            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spAudit_Insert", con))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add("@AdminID", SqlDbType.Int).Value = (object)userID ?? DBNull.Value;
                    cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 255).Value = action;

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch { /* ignore audit errors */ }
        }
    }
}
