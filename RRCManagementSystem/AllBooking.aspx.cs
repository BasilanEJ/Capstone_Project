using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

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
            // If your intention is to ALLOW Admins and BLOCK others, adjust logic as needed.
            if (role == "SuperAdmin" || role == "Inspector")
            {
                // Example: Only Admin can view; change this to your real rule.
                // Response.Redirect("~/Login.aspx"); return;
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
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAdminPermission_Check", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = adminId;
                    cmd.Parameters.Add("@ModuleName", SqlDbType.NVarChar, 100).Value = moduleName;
                    cmd.Parameters.Add("@Permission", SqlDbType.NVarChar, 10).Value = "CanView";

                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return (result != null && result != DBNull.Value) && Convert.ToBoolean(result);
                }
            }
            catch
            {
                return false;
            }
        }

        private bool HasEditPermission(int adminId, string moduleName)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAdminPermission_CanEdit", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = adminId;
                    cmd.Parameters.Add("@ModuleName", SqlDbType.NVarChar, 100).Value = moduleName;

                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null && Convert.ToBoolean(result);
                }
            }
            catch
            {
                return false; // deny by default
            }
        }


        private void LoadAllBookings()
        {
            try
            {
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spBooking_ListAllFiltered", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    string search = txtSearch.Text.Trim();
                    string status = ddlStatusFilter.SelectedValue;

                    var pSearch = cmd.Parameters.Add("@SearchTerm", SqlDbType.NVarChar, 200);
                    pSearch.Value = string.IsNullOrWhiteSpace(search) ? (object)DBNull.Value : search;

                    var pStatus = cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 50);
                    pStatus.Value = string.IsNullOrWhiteSpace(status) ? (object)DBNull.Value : status;

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        gvBookings.DataSource = dt;
                        gvBookings.DataBind();

                        lblMessage.Text = $"{dt.Rows.Count} booking(s) found.";
                        lblMessage.ForeColor = System.Drawing.Color.Green;
                    }
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
            if (e.CommandName == "EditBooking")
            {
                if (int.TryParse(e.CommandArgument.ToString(), out int bookingId))
                    Response.Redirect($"EditBooking.aspx?BookingID={bookingId}");
            }
        }

        private void MarkOp1AsCompleted(int bookingId)
        {
            try
            {
                int completedFlag = 0;

                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spServiceSchedule_CompleteOp1", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingId;

                    con.Open();
                    object result = cmd.ExecuteScalar();
                    completedFlag = (result == null || result == DBNull.Value) ? 0 : Convert.ToInt32(result);
                }

                AddAuditLog(Convert.ToInt32(Session["UserID"]),
                    completedFlag == 1
                        ? $"Marked Op1 completed for BookingID {bookingId}"
                        : $"Attempted to mark Op1 completed (no change) for BookingID {bookingId}");

                if (completedFlag == 1)
                {
                    // The redirection in btnHiddenCompleteOp1_Click handles the SweetAlert.
                }
                else
                {
                    lblMessage.Text = "⚠️ Operation 1 not found or already completed.";
                    lblMessage.ForeColor = System.Drawing.Color.Orange;
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
                MarkOp1AsCompleted(bookingId);
                Response.Redirect("AllBooking.aspx?op1=completed");
            }
        }

        protected void gvBookings_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            // Get status values from the DataItem
            string bookingStatus = DataBinder.Eval(e.Row.DataItem, "Status")?.ToString();
            string op1Status = DataBinder.Eval(e.Row.DataItem, "Op1Status")?.ToString();
            bool isContract = false;
            var isContractObj = DataBinder.Eval(e.Row.DataItem, "IsContract");
            if (isContractObj != null && isContractObj != DBNull.Value)
                isContract = Convert.ToBoolean(isContractObj);

            // Find controls within the row
            var lblOp1 = (Label)e.Row.FindControl("lblOp1Status");
            var btnOp1 = (Button)e.Row.FindControl("btnTriggerCompleteOp1");
            var btnEdit = (Button)e.Row.FindControl("btnEdit");

            // 1. Apply status color styling to the Booking Status column
            int statusCol = 7;
            Label statusLabel = e.Row.Cells[statusCol].Controls.Count > 0 ? e.Row.Cells[statusCol].Controls[0] as Label : null;
            if (statusLabel != null)
            {
                e.Row.Cells[statusCol].CssClass = "py-3 px-6 text-center font-semibold";
                switch (bookingStatus?.ToLower())
                {
                    case "assigned":
                        e.Row.Cells[statusCol].CssClass += " text-blue-500";
                        break;
                    case "pending":
                    case "ongoing":
                        e.Row.Cells[statusCol].CssClass += " text-yellow-500";
                        break;
                    case "cancelled":
                    case "rejected":
                        e.Row.Cells[statusCol].CssClass += " text-red-500";
                        break;
                    case "completed":
                    case "confirmed":
                    case "approved":
                        e.Row.Cells[statusCol].CssClass += " text-green-500";
                        break;
                    default:
                        // No extra styling
                        break;
                }
            }

            // 2. Hide/show Op1-related controls based on contract type
            if (lblOp1 != null)
            {
                lblOp1.Visible = isContract;
                // Apply color to Op1 status label
                if (isContract && !string.IsNullOrEmpty(op1Status))
                {
                    if (string.Equals(op1Status, "Completed", StringComparison.OrdinalIgnoreCase))
                    {
                        lblOp1.CssClass += " text-green-500";
                    }
                }
            }

            if (btnOp1 != null)
                btnOp1.Visible = isContract
                               && string.Equals(bookingStatus, "Assigned", StringComparison.OrdinalIgnoreCase)
                               && !string.Equals(op1Status, "Completed", StringComparison.OrdinalIgnoreCase);

            // 3. Permission check for Edit button
            int userId = Convert.ToInt32(Session["UserID"]);
            if (btnEdit != null)
            {
                btnEdit.Enabled = HasEditPermission(userId, "ManageBooking");
                if (!btnEdit.Enabled)
                {
                    btnEdit.CssClass += " opacity-50 cursor-not-allowed";
                }
            }
        }


        private void AddAuditLog(int adminId, string action)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAudit_Insert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@AdminID", SqlDbType.Int).Value = adminId;
                    cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 255).Value = action;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                // swallow
            }
        }
    }
}