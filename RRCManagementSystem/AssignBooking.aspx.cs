using Newtonsoft.Json.Linq;
using RRCManagementSystem.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class AssignBooking : System.Web.UI.Page
    {
        private readonly string cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;
        private int bookingID;
        private int scheduleID;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 1. Check if user session exists
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            // 2. Restrict SuperAdmin and Inspector roles
            string role = Session["Role"].ToString();
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            // 3. Check Edit Permission
            if (!HasEditPermission(userId, "ManageBooking"))
            {
                lblMessage.Visible = true;
                lblMessage.Text = "❌ You do not have permission to assign bookings.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                btnAssignAll.Enabled = false;
                return;
            }

            // 4. Load data on initial page load
            if (!IsPostBack)
            {
                hfConfirmAssign.Value = "false";

                // Validate BookingID
                if (int.TryParse(Request.QueryString["BookingID"], out bookingID))
                {
                    // Optional ScheduleID (for reschedules or follow-ups)
                    int.TryParse(Request.QueryString["ScheduleID"], out scheduleID);

                    LoadBookingCodeAndOperation();

                    // 🟢 Determine which date to use
                    DateTime scheduledDate = GetCorrectScheduledDate();

                    // 🟢 Store scheduled date for reuse in postbacks
                    ViewState["ScheduledDate"] = scheduledDate;
                    ViewState["ScheduleID"] = scheduleID;

                    lblMessage.Text = $"📅 Scheduled Date: {scheduledDate:MMMM dd, yyyy}";
                    lblMessage.ForeColor = System.Drawing.Color.Black;

                    // 🧩 Load related data
                    LoadTeams(scheduledDate);
                    LoadAvailableEquipments();
                    LoadAvailableChemicals();
                    LoadAvailableSachetChemicals();
                    LoadSafetyGear();

                    // ✅ Show success alert if redirected after assignment
                    if (Request.QueryString["status"] == "Assigned")
                    {
                        string script = @"Swal.fire({
                    icon:'success',
                    title:'Assigned!',
                    text:'The booking was successfully assigned.',
                    showConfirmButton:false,
                    timer:2000
                });";
                        ClientScript.RegisterStartupScript(this.GetType(), "AssignSuccess", script, true);
                    }
                }
                else
                {
                    lblMessage.Visible = true;
                    lblMessage.Text = "⚠️ Invalid Booking ID.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    btnAssignAll.Enabled = false;
                }
            }
            else
            {
                // ✅ Restore values on postback
                if (ViewState["ScheduleID"] != null)
                {
                    scheduleID = (int)ViewState["ScheduleID"];
                }
                if (!int.TryParse(Request.QueryString["BookingID"], out bookingID))
                {
                    bookingID = 0;
                }
            }
        }


        private DateTime GetCorrectScheduledDate()
        {
            if (scheduleID > 0)
            {
                // Get the new approved reschedule date
                DateTime newDate = GetNewScheduledDate(scheduleID);

                // If for some reason not found, fallback to original
                if (newDate == DateTime.MinValue)
                    return GetScheduledDate(bookingID);

                return newDate;
            }
            else
            {
                // Normal booking date
                return GetScheduledDate(bookingID);
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
            catch { return false; }
        }
        private void LoadBookingCodeAndOperation()
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.spBooking_GetBookingCodeWithOperation", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingID;
                cmd.Parameters.Add("@ScheduleID", SqlDbType.Int).Value = scheduleID > 0 ? (object)scheduleID : DBNull.Value;

                con.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // 🟦 Display Booking Code
                        lblBookingCode.Text = reader["BookingCode"].ToString();

                        // 🟦 Display Operation Number (if reschedule)
                        if (scheduleID > 0 && reader["OperationNumber"] != DBNull.Value)
                        {
                            lblOperationNumber.Visible = true;
                            lblOperationNumber.Text = $"(Op #{reader["OperationNumber"]})";
                        }
                        else
                        {
                            lblOperationNumber.Visible = false;
                        }

                        // 🟦 Extract data from reader
                        string serviceDetails = reader["ServiceDetails"]?.ToString();
                        string serviceName = reader["ServiceName"]?.ToString();
                        int totalSQM = reader["SQM"] != DBNull.Value ? Convert.ToInt32(reader["SQM"]) : 0;

                        // ✅ SQL already filters contract-only when reschedule — no need to re-filter here

                        // 🟩 Display the services in the header
                        lblServiceName.Text = FormatServiceDetailsForHeader(serviceDetails, serviceName, totalSQM);

                        // 🟩 Store data for chemical/equipment loaders
                        Session["ServiceDetails"] = serviceDetails;
                        Session["ServiceName"] = serviceName;
                        Session["SQM"] = totalSQM;

                        // 🟢 Optional: Debug confirmation (you can remove later)
                        System.Diagnostics.Debug.WriteLine($"[LoadBookingCodeAndOperation] BookingID={bookingID}, ScheduleID={scheduleID}, SQM={totalSQM}");
                        System.Diagnostics.Debug.WriteLine($"[LoadBookingCodeAndOperation] ServiceDetails={serviceDetails}");
                    }
                    else
                    {
                        // 🟥 Handle missing booking (defensive)
                        lblBookingCode.Text = "N/A";
                        lblServiceName.Text = "<span class='text-gray-400 italic'>No service details found.</span>";
                        lblOperationNumber.Visible = false;
                    }
                }
            }
        }




        /// <summary>
        /// Format service details for the header display with individual SQM values
        /// </summary>
        private string FormatServiceDetailsForHeader(string serviceDetailsJson, string serviceNames, int totalSQM)
        {
            var html = new System.Text.StringBuilder();

            // 🟦 Parse JSON if available
            if (!string.IsNullOrWhiteSpace(serviceDetailsJson) && serviceDetailsJson.TrimStart().StartsWith("["))
            {
                try
                {
                    var services = JArray.Parse(serviceDetailsJson);

                    foreach (var service in services)
                    {
                        // Case-insensitive key handling
                        string name = service["ServiceName"]?.ToString()
                                   ?? service["serviceName"]?.ToString()
                                   ?? service["Name"]?.ToString()
                                   ?? service["name"]?.ToString();

                        int sqm = Convert.ToInt32(service["SQM"] ?? 0);

                        if (!string.IsNullOrEmpty(name))
                        {
                            html.Append("<div class='inline-flex items-center gap-2 mr-4 mb-1'>");
                            html.Append("<i class='fas fa-check-circle text-yellow-300' style='font-size:0.875rem;'></i>");
                            html.Append($"<span class='text-white text-sm'>{System.Web.HttpUtility.HtmlEncode(name)}</span>");
                            html.Append($"<span class='bg-yellow-200 text-blue-900 px-2 py-0.5 rounded text-xs font-semibold'>{sqm} m²</span>");
                            html.Append("</div>");
                        }
                    }

                    // Add total SQM if multiple services
                    if (services.Count > 1)
                    {
                        html.Append("<div class='block mt-2 pt-2 border-t border-blue-600 text-xs text-gray-300'>");
                        html.Append($"<strong>Total Coverage:</strong> {totalSQM} m²");
                        html.Append("</div>");
                    }

                    return html.ToString();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"FormatServiceDetailsForHeader JSON parse error: {ex.Message}");
                }
            }

            // 🟩 Fallback for plain ServiceNames
            if (!string.IsNullOrWhiteSpace(serviceNames))
            {
                html.Append("<div class='inline-flex items-center gap-2'>");
                html.Append("<i class='fas fa-tools text-yellow-300' style='font-size:0.875rem;'></i>");
                html.Append($"<span class='text-white text-sm'>{System.Web.HttpUtility.HtmlEncode(serviceNames)}</span>");
                if (totalSQM > 0)
                {
                    html.Append($"<span class='bg-yellow-200 text-blue-900 px-2 py-0.5 rounded text-xs font-semibold'>{totalSQM} m²</span>");
                }
                html.Append("</div>");
                return html.ToString();
            }

            return "<span class='text-gray-400 text-sm italic'>N/A</span>";
        }




        private DateTime GetNewScheduledDate(int scheduleID)
        {
            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spServiceSchedule_GetNewDate", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ScheduleID", SqlDbType.Int).Value = scheduleID;
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null && result != DBNull.Value
                        ? Convert.ToDateTime(result)
                        : DateTime.MinValue;
                }
            }
            catch
            {
                return DateTime.MinValue;
            }
        }


        private void LoadTeams(DateTime scheduledDate)
        {
            // ✅ Get the StartTime from the booking
            TimeSpan startTime = GetBookingStartTime(bookingID);

            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.spTeams_ListAvailable", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Date", SqlDbType.Date).Value = scheduledDate;
                cmd.Parameters.Add("@StartTime", SqlDbType.Time).Value = startTime; // ✅ Pass start time

                con.Open();
                ddlTeams.DataSource = cmd.ExecuteReader();
                ddlTeams.DataTextField = "GroupName";
                ddlTeams.DataValueField = "TeamID";
                ddlTeams.DataBind();

                // Add a default "Select Team" option
                ddlTeams.Items.Insert(0, new ListItem("-- Select Team --", ""));
            }
        }

        // ✅ NEW: Get StartTime from booking
        private TimeSpan GetBookingStartTime(int bookingID)
        {
            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("SELECT StartTime FROM dbo.Bookings WHERE BookingID = @BookingID", con))
                {
                    cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingID;
                    con.Open();
                    object result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        return (TimeSpan)result;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetBookingStartTime error: {ex.Message}");
            }

            // Default to 8 AM if not found
            return new TimeSpan(8, 0, 0);
        }


        private DateTime GetScheduledDate(int bookingID)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.spBooking_GetSchedule", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingID;

                con.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        if (reader["ScheduledDate"] != DBNull.Value)
                        {
                            return Convert.ToDateTime(reader["ScheduledDate"]);
                        }
                        else
                        {
                            throw new Exception("⚠️ Booking has no scheduled date.");
                        }
                    }
                    else
                    {
                        throw new Exception($"⚠️ Booking not found. BookingID = {bookingID}");
                    }
                }
            }
        }



        protected void ddlTeams_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblTeamBookings.Text = "";

            if (!int.TryParse(ddlTeams.SelectedValue, out int teamId))
            {
                lblTeamBookings.Text = "Select a team to view their bookings.";
                return;
            }

            try
            {
                // ✅ Get the scheduled date from ViewState (already determined in Page_Load)
                DateTime scheduledDate;
                if (ViewState["ScheduledDate"] != null)
                {
                    scheduledDate = (DateTime)ViewState["ScheduledDate"];
                }
                else
                {
                    // Fallback: recalculate if ViewState is lost
                    scheduledDate = GetCorrectScheduledDate();
                }

                // Load bookings for the selected team and date
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spBookingSummary", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@TeamID", SqlDbType.Int).Value = teamId;
                    cmd.Parameters.Add("@Date", SqlDbType.Date).Value = scheduledDate;

                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            lblTeamBookings.Text = $"No bookings scheduled for this team on {scheduledDate:MMMM dd, yyyy}.";
                        }
                        else
                        {
                            lblTeamBookings.Text = "<ul class='list-disc ml-5 text-gray-700'>";

                            while (reader.Read())
                            {
                                string startTime = "00:00";
                                if (reader["StartTime"] != DBNull.Value && TimeSpan.TryParse(reader["StartTime"].ToString(), out TimeSpan ts))
                                {
                                    startTime = ts.ToString(@"hh\:mm");
                                }

                                string street = reader["StreetEnc"] != DBNull.Value ? AESHelper.DecryptField(reader["StreetEnc"].ToString()) : "";
                                string barangay = reader["BarangayEnc"] != DBNull.Value ? AESHelper.DecryptField(reader["BarangayEnc"].ToString()) : "";
                                string city = reader["CityEnc"] != DBNull.Value ? AESHelper.DecryptField(reader["CityEnc"].ToString()) : "";

                                string fullLocation = $"{street}, {barangay}, {city}".Trim(',', ' ');

                                lblTeamBookings.Text += $"<li><strong>{reader["BookingCode"]}</strong> - {fullLocation} at {startTime}</li>";
                            }

                            lblTeamBookings.Text += "</ul>";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblTeamBookings.Text = $"❌ Error loading bookings: {ex.Message}";
            }
        }





        private void LoadAvailableEquipments()
        {
            if (!int.TryParse(Request.QueryString["BookingID"], out bookingID)) return;

            // Get scheduleID from ViewState if available
            int scheduleID = 0;
            if (ViewState["ScheduleID"] != null)
            {
                scheduleID = (int)ViewState["ScheduleID"];
            }

            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.spAssign_ListAvailableEquipments", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingID;
                cmd.Parameters.Add("@ScheduleID", SqlDbType.Int).Value = scheduleID > 0 ? (object)scheduleID : DBNull.Value;

                using (var da = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count == 0)
                    {
                        // No equipment available - show warning panel
                        pnlNoEquipment.Visible = true;
                        pnlEquipmentGrid.Visible = false;
                        btnAssignAll.Enabled = false; // Disable assign button
                    }
                    else
                    {
                        // Equipment available - show grid
                        pnlNoEquipment.Visible = false;
                        pnlEquipmentGrid.Visible = true;
                        gvEquipments.DataSource = dt;
                        gvEquipments.DataBind();
                    }
                }
            }
        }

        private void LoadAvailableChemicals()
        {
            // ✅ Get service names from session
            string serviceNames = Session["ServiceName"]?.ToString() ?? "";

            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.spInventory_ListChemicalsByService", con)) // ✅ NEW SP
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ServiceNames", SqlDbType.NVarChar, -1).Value = serviceNames;

                using (var da = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    gvChemicals.DataSource = dt;
                    gvChemicals.DataBind();
                }
            }
        }

        private void LoadAvailableSachetChemicals()
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.spInventory_ListByType", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Type", SqlDbType.NVarChar, 50).Value = "Sachet Pack Chemical";

                using (var da = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    gvSachetChemicals.DataSource = dt;
                    gvSachetChemicals.DataBind();
                }
            }
        }

        private void LoadSafetyGear()
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.spInventory_ListByType", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Type", SqlDbType.NVarChar, 50).Value = "Safety Gear";

                using (var da = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    gvSafetyGears.DataSource = dt;
                    gvSafetyGears.DataBind();
                }
            }
        }

        protected void btnAssignAll_Click(object sender, EventArgs e)
        {
            lblMessage.Visible = true;

            if (!int.TryParse(Request.QueryString["BookingID"], out bookingID))
            {
                lblMessage.Text = "❌ Invalid Booking ID!";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            // ✅ Restore scheduleID from ViewState
            if (ViewState["ScheduleID"] != null)
            {
                scheduleID = (int)ViewState["ScheduleID"];
            }

            if (ddlTeams.SelectedIndex <= 0)
            {
                lblMessage.Text = "⚠️ Please select a team.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            int teamId = Convert.ToInt32(ddlTeams.SelectedValue);
            int adminId = Convert.ToInt32(Session["UserID"]);

            using (var con = new SqlConnection(cs))
            {
                con.Open();
                var tx = con.BeginTransaction();

                try
                {
                    // ✅ 1) Get correct scheduled date based on context
                    DateTime scheduledDate;
                    TimeSpan startTime;

                    if (scheduleID > 0)
                    {
                        scheduledDate = GetNewScheduledDate(scheduleID);
                        if (scheduledDate == DateTime.MinValue)
                        {
                            throw new Exception("⚠️ Could not retrieve new scheduled date for this reschedule.");
                        }

                        using (var cmd = new SqlCommand("dbo.spBooking_GetSchedule", con, tx))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingID;
                            using (var r = cmd.ExecuteReader())
                            {
                                if (!r.Read()) throw new Exception("Booking not found.");
                                startTime = r["StartTime"] == DBNull.Value ? new TimeSpan(8, 0, 0) : (TimeSpan)r["StartTime"];
                            }
                        }
                    }
                    else
                    {
                        using (var cmd = new SqlCommand("dbo.spBooking_GetSchedule", con, tx))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingID;
                            using (var r = cmd.ExecuteReader())
                            {
                                if (!r.Read()) throw new Exception("Booking not found.");
                                scheduledDate = r["ScheduledDate"] == DBNull.Value ? DateTime.Today : Convert.ToDateTime(r["ScheduledDate"]);
                                startTime = r["StartTime"] == DBNull.Value ? new TimeSpan(8, 0, 0) : (TimeSpan)r["StartTime"];
                            }
                        }
                    }

                    DateTime scheduledDateTime = scheduledDate.Date.Add(startTime);

                    // 2) Verify team daily cap (< 2)
                    using (var cmd = new SqlCommand("dbo.spBooking_TeamDailyCount", con, tx))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@TeamID", SqlDbType.Int).Value = teamId;
                        cmd.Parameters.Add("@Date", SqlDbType.Date).Value = scheduledDate.Date;
                        int count = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                        if (count >= 2)
                        {
                            lblMessage.Text = $"⚠️ Team {teamId} is already fully booked for {scheduledDate:yyyy-MM-dd}.";
                            lblMessage.ForeColor = System.Drawing.Color.Red;
                            tx.Rollback();
                            return;
                        }
                    }

                    // 3) Upsert team assignment
                    using (var cmd = new SqlCommand("dbo.spBookingTeam_Upsert", con, tx))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingID;
                        cmd.Parameters.Add("@TeamID", SqlDbType.Int).Value = teamId;
                        cmd.ExecuteNonQuery();
                    }

                    // ✅ 4) REMOVE THIS LINE - It's causing the error and serves no purpose
                    // decimal totalUsage = CalculateChemicalUsageForService(con, tx);

                    // ✅ NEW: Validate Equipment Selection
                    int selectedEquipCount = 0;
                    foreach (GridViewRow row in gvEquipments.Rows)
                    {
                        var chk = row.FindControl("chkAssignEquip") as CheckBox;
                        if (chk != null && chk.Checked)
                        {
                            selectedEquipCount++;
                        }
                    }

                    if (selectedEquipCount == 0)
                    {
                        lblMessage.Text = "⚠️ Please select at least one equipment.";
                        lblMessage.ForeColor = System.Drawing.Color.Red;
                        tx.Rollback();
                        return;
                    }

                    // ✅ NEW: Validate Bottled Chemical Selection
                    int selectedBottledChemCount = 0;
                    foreach (GridViewRow row in gvChemicals.Rows)
                    {
                        var chk = row.FindControl("chkUseChemical") as CheckBox;
                        if (chk != null && chk.Checked)
                        {
                            selectedBottledChemCount++;
                        }
                    }

                    if (selectedBottledChemCount == 0)
                    {
                        lblMessage.Text = "⚠️ Please select at least one bottled chemical.";
                        lblMessage.ForeColor = System.Drawing.Color.Red;
                        tx.Rollback();
                        return;
                    }

                    // ✅ NEW: Validate Sachet Chemical Quantity
                    int totalSachetQty = 0;
                    foreach (GridViewRow row in gvSachetChemicals.Rows)
                    {
                        int qty = 0;
                        int.TryParse(((TextBox)row.FindControl("txtAssignSachet"))?.Text ?? "0", out qty);
                        totalSachetQty += qty;
                    }

                    if (totalSachetQty == 0)
                    {
                        lblMessage.Text = "⚠️ Please assign at least one sachet pack chemical.";
                        lblMessage.ForeColor = System.Drawing.Color.Red;
                        tx.Rollback();
                        return;
                    }

                    // ✅ NEW: Validate Safety Gear Quantity
                    int totalSafetyQty = 0;
                    foreach (GridViewRow row in gvSafetyGears.Rows)
                    {
                        int qty = 0;
                        int.TryParse(((TextBox)row.FindControl("txtAssignSafety"))?.Text ?? "0", out qty);
                        totalSafetyQty += qty;
                    }

                    if (totalSafetyQty == 0)
                    {
                        lblMessage.Text = "⚠️ Please assign at least one safety gear item.";
                        lblMessage.ForeColor = System.Drawing.Color.Red;
                        tx.Rollback();
                        return;
                    }

                    // 5) Equipments
                    var seenEquip = new HashSet<int>();
                    foreach (GridViewRow row in gvEquipments.Rows)
                    {
                        var chk = row.FindControl("chkAssignEquip") as CheckBox;
                        if (chk != null && chk.Checked)
                        {
                            int equipmentId = Convert.ToInt32(gvEquipments.DataKeys[row.RowIndex].Value);
                            if (!seenEquip.Add(equipmentId)) continue;

                            using (var cnt = new SqlCommand("dbo.spBooking_EquipmentDailyCount", con, tx))
                            {
                                cnt.CommandType = CommandType.StoredProcedure;
                                cnt.Parameters.Add("@EquipmentID", SqlDbType.Int).Value = equipmentId;
                                cnt.Parameters.Add("@Date", SqlDbType.Date).Value = scheduledDate.Date;
                                int eqCount = Convert.ToInt32(cnt.ExecuteScalar() ?? 0);
                                if (eqCount >= 2)
                                {
                                    lblMessage.Text = $"⚠️ Equipment {equipmentId} is fully booked for {scheduledDate:yyyy-MM-dd}.";
                                    lblMessage.ForeColor = System.Drawing.Color.Red;
                                    tx.Rollback();
                                    return;
                                }
                            }

                            using (var ins = new SqlCommand("dbo.spBookingEquipment_Insert", con, tx))
                            {
                                ins.CommandType = CommandType.StoredProcedure;
                                ins.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingID;
                                ins.Parameters.Add("@EquipmentID", SqlDbType.Int).Value = equipmentId;
                                ins.Parameters.Add("@QuantityAssigned", SqlDbType.Int).Value = 1;
                                ins.ExecuteNonQuery();
                            }
                        }
                    }

                    // 6) Bottled chemicals - FIXED VERSION
                    foreach (GridViewRow row in gvChemicals.Rows)
                    {
                        var chk = row.FindControl("chkUseChemical") as CheckBox;
                        if (chk == null || !chk.Checked) continue;

                        int itemId = Convert.ToInt32(gvChemicals.DataKeys[row.RowIndex].Value);

                        // ✅ FIX: Get chemical name from the correct cell (DataField="Name" creates a read-only cell)
                        string chemicalName = row.Cells[1].Text; // Index 1 is the "Name" column

                        if (string.IsNullOrWhiteSpace(chemicalName))
                        {
                            lblMessage.Text = $"⚠️ Could not determine chemical name for Item {itemId}.";
                            lblMessage.ForeColor = System.Drawing.Color.Red;
                            tx.Rollback();
                            return;
                        }

                        // ✅ Calculate usage ONLY for services that match this chemical
                        decimal usage = CalculateChemicalUsageForService(con, tx, chemicalName);

                        // ⚠️ If no matching services, skip this chemical
                        if (usage == 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"⚠️ Chemical '{chemicalName}' (Item {itemId}) has no matching services. Skipping.");
                            continue;
                        }

                        int quantity = int.Parse(((Label)row.FindControl("lblQuantity"))?.Text ?? "0");
                        decimal excess = decimal.Parse(((Label)row.FindControl("lblExcessML"))?.Text ?? "0");
                        int buffer = 0;
                        int.TryParse(((TextBox)row.FindControl("txtBottleBuffer"))?.Text ?? "0", out buffer);

                        if (quantity < 5)
                        {
                            lblMessage.Text = $"⚠️ Cannot assign bottled chemical '{chemicalName}' (Item {itemId}) — quantity below 5.";
                            lblMessage.ForeColor = System.Drawing.Color.Red;
                            tx.Rollback();
                            return;
                        }

                        // ✅ Deduct the calculated usage
                        if (excess >= usage)
                        {
                            excess -= usage;
                        }
                        else
                        {
                            if (quantity <= 0)
                            {
                                lblMessage.Text = $"⚠️ Not enough bottled chemical '{chemicalName}' for Item {itemId}.";
                                lblMessage.ForeColor = System.Drawing.Color.Red;
                                tx.Rollback();
                                return;
                            }
                            quantity--;
                            excess = 1000 + excess - usage;
                        }

                        if (buffer > 0)
                        {
                            if (buffer > quantity)
                            {
                                lblMessage.Text = $"⚠️ Buffer exceeds stock for '{chemicalName}' (Item {itemId}).";
                                lblMessage.ForeColor = System.Drawing.Color.Red;
                                tx.Rollback();
                                return;
                            }
                            quantity -= buffer;
                        }

                        // Update inventory
                        using (var u = new SqlCommand("dbo.spInventory_Bottled_Update", con, tx))
                        {
                            u.CommandType = CommandType.StoredProcedure;
                            u.Parameters.Add("@ItemID", SqlDbType.Int).Value = itemId;
                            u.Parameters.Add("@Quantity", SqlDbType.Int).Value = quantity;
                            u.Parameters.Add("@ExcessML", SqlDbType.Decimal).Value = excess;
                            u.ExecuteNonQuery();
                        }

                        // Record assignment
                        using (var ins = new SqlCommand("dbo.spBookingChemical_Insert", con, tx))
                        {
                            ins.CommandType = CommandType.StoredProcedure;
                            ins.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingID;
                            ins.Parameters.Add("@ItemID", SqlDbType.Int).Value = itemId;
                            ins.Parameters.Add("@QuantityAssigned", SqlDbType.Int).Value = 1;
                            ins.ExecuteNonQuery();
                        }

                        if (buffer > 0)
                        {
                            using (var ins2 = new SqlCommand("dbo.spBookingChemical_Insert", con, tx))
                            {
                                ins2.CommandType = CommandType.StoredProcedure;
                                ins2.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingID;
                                ins2.Parameters.Add("@ItemID", SqlDbType.Int).Value = itemId;
                                ins2.Parameters.Add("@QuantityAssigned", SqlDbType.Int).Value = buffer;
                                ins2.ExecuteNonQuery();
                            }
                        }
                    }

                    // 7) Sachet chemicals
                    foreach (GridViewRow row in gvSachetChemicals.Rows)
                    {
                        int qty = 0;
                        int.TryParse(((TextBox)row.FindControl("txtAssignSachet"))?.Text ?? "0", out qty);
                        if (qty <= 0) continue;

                        int itemId = Convert.ToInt32(gvSachetChemicals.DataKeys[row.RowIndex].Value);
                        int current = int.Parse(((Label)row.FindControl("lblSachetQuantity"))?.Text ?? "0");

                        if (current < 5)
                        {
                            lblMessage.Text = $"⚠️ Cannot assign sachet chemical (Item {itemId}) — quantity below 5.";
                            lblMessage.ForeColor = System.Drawing.Color.Red;
                            tx.Rollback();
                            return;
                        }
                        if (qty > current)
                        {
                            lblMessage.Text = $"⚠️ Not enough sachet packs for Item {itemId}.";
                            lblMessage.ForeColor = System.Drawing.Color.Red;
                            tx.Rollback();
                            return;
                        }

                        using (var u = new SqlCommand("dbo.spInventory_Decrease", con, tx))
                        {
                            u.CommandType = CommandType.StoredProcedure;
                            u.Parameters.Add("@ItemID", SqlDbType.Int).Value = itemId;
                            u.Parameters.Add("@Qty", SqlDbType.Int).Value = qty;
                            u.ExecuteNonQuery();
                        }
                        using (var ins = new SqlCommand("dbo.spBookingChemical_Insert", con, tx))
                        {
                            ins.CommandType = CommandType.StoredProcedure;
                            ins.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingID;
                            ins.Parameters.Add("@ItemID", SqlDbType.Int).Value = itemId;
                            ins.Parameters.Add("@QuantityAssigned", SqlDbType.Int).Value = qty;
                            ins.ExecuteNonQuery();
                        }
                    }

                    // 8) Safety gear
                    foreach (GridViewRow row in gvSafetyGears.Rows)
                    {
                        int qty = 0;
                        int.TryParse(((TextBox)row.FindControl("txtAssignSafety"))?.Text ?? "0", out qty);
                        if (qty <= 0) continue;

                        int itemId = Convert.ToInt32(gvSafetyGears.DataKeys[row.RowIndex].Value);
                        int current = int.Parse(((Label)row.FindControl("lblSafetyQuantity"))?.Text ?? "0");

                        if (current < 5)
                        {
                            lblMessage.Text = $"⚠️ Cannot assign safety gear (Item {itemId}) — quantity below 5.";
                            lblMessage.ForeColor = System.Drawing.Color.Red;
                            tx.Rollback();
                            return;
                        }
                        if (qty > current)
                        {
                            lblMessage.Text = $"⚠️ Not enough safety gear for Item {itemId}.";
                            lblMessage.ForeColor = System.Drawing.Color.Red;
                            tx.Rollback();
                            return;
                        }

                        using (var u = new SqlCommand("dbo.spInventory_Decrease", con, tx))
                        {
                            u.CommandType = CommandType.StoredProcedure;
                            u.Parameters.Add("@ItemID", SqlDbType.Int).Value = itemId;
                            u.Parameters.Add("@Qty", SqlDbType.Int).Value = qty;
                            u.ExecuteNonQuery();
                        }
                        using (var ins = new SqlCommand("dbo.spBookingSafety_Insert", con, tx))
                        {
                            ins.CommandType = CommandType.StoredProcedure;
                            ins.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingID;
                            ins.Parameters.Add("@ItemID", SqlDbType.Int).Value = itemId;
                            ins.Parameters.Add("@QuantityAssigned", SqlDbType.Int).Value = qty;
                            ins.ExecuteNonQuery();
                        }
                    }

                    // 9) Set booking status + team on Bookings
                    using (var cmd = new SqlCommand("dbo.spBooking_SetStatusTeam", con, tx))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingID;
                        cmd.Parameters.Add("@TeamID", SqlDbType.Int).Value = teamId;
                        cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = "Assigned";
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new SqlCommand("dbo.spBooking_ApproveAndInsertBalance", con, tx))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingID;
                        cmd.Parameters.Add("@AdminID", SqlDbType.Int).Value = adminId;

                        object result = cmd.ExecuteScalar();
                        int insertResult = (result != null && result != DBNull.Value) ? Convert.ToInt32(result) : 0;

                        if (insertResult == 0)
                        {
                            throw new Exception("Failed to create payment installments. Please check booking details.");
                        }
                    }


                    // ✅ 10) If this is a reschedule, update ServiceSchedule assignment
                    if (scheduleID > 0)
                    {
                        using (var cmd = new SqlCommand("dbo.spServiceSchedule_AssignTeam", con, tx))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@ScheduleID", SqlDbType.Int).Value = scheduleID;
                            cmd.Parameters.Add("@TeamID", SqlDbType.Int).Value = teamId;
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // 11) Create contract ops if needed (only for initial bookings)
                    if (scheduleID == 0)
                    {
                        using (var cmd = new SqlCommand("dbo.spServiceSchedule_InitIfContract", con, tx))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingID;
                            cmd.Parameters.Add("@TeamID", SqlDbType.Int).Value = teamId; // ✅ Pass assigned team
                            cmd.ExecuteNonQuery();
                        }

                    }

                    tx.Commit();
                    AddAuditLog(adminId, $"Assigned Team {teamId} + equipment/consumables to BookingID {bookingID}" + (scheduleID > 0 ? $" (Reschedule Op#{scheduleID})" : ""));

                    // ✅ Trigger SweetAlert success message and redirect after 3 seconds
                    string script = @"
Swal.fire({
    icon: 'success',
    title: 'Assigned Successfully!',
    text: 'The booking was successfully assigned.',
    showConfirmButton: false,
    timer: 3000
}).then(() => {
    window.location.href = 'AllBooking.aspx';
});";

                    ClientScript.RegisterStartupScript(this.GetType(), "AssignSuccess", script, true);

                }
                catch (Exception ex)
                {
                    try { tx.Rollback(); } catch { }
                    lblMessage.Text = "❌ Error: " + ex.Message;
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
            }
        }

        private decimal GetChemicalUsageBasedOnSQMProc(SqlConnection con, SqlTransaction tx, int sqm)
        {
            string settingKey;
            if (sqm <= 100) settingKey = "Usage_0_100";
            else if (sqm <= 250) settingKey = "Usage_101_250";
            else if (sqm <= 400) settingKey = "Usage_251_400";
            else if (sqm <= 600) settingKey = "Usage_401_600";
            else if (sqm <= 800) settingKey = "Usage_601_800";
            else if (sqm <= 1000) settingKey = "Usage_801_1000";
            else settingKey = "Usage_1000plus";
            using (var cmd = new SqlCommand("dbo.spSystemSettings_GetValueDecimal", con, tx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@SettingName", SqlDbType.NVarChar, 100).Value = settingKey;
                object v = cmd.ExecuteScalar();
                return (v != null && v != DBNull.Value && decimal.TryParse(v.ToString(), out var d)) ? d : 333m;
            }
        }


        #region Chemical Matching Configuration (Dynamic)

        private static List<string> _broadSpectrumKeywords = null;
        private static List<string> _excludedWords = null;
        private static int _minWordLength = 3;
        private static DateTime _configLastLoaded = DateTime.MinValue;

        private void LoadChemicalMatchConfig(SqlConnection con, SqlTransaction tx = null)
        {
            // ✅ Cache for 1 hour to avoid excessive DB calls
            if (_broadSpectrumKeywords != null &&
                _excludedWords != null &&
                (DateTime.Now - _configLastLoaded).TotalHours < 1)
            {
                return; // Use cached values
            }

            using (var cmd = new SqlCommand("dbo.spSystemSettings_GetChemicalMatchConfig", con, tx))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string settingName = reader["SettingName"].ToString();
                        string settingValue = reader["SettingValue"].ToString();

                        switch (settingName)
                        {
                            case "ChemicalMatch_BroadSpectrumKeywords":
                                _broadSpectrumKeywords = settingValue
                                    .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(s => s.Trim().ToLower())
                                    .ToList();
                                break;

                            case "ChemicalMatch_ExcludedWords":
                                _excludedWords = settingValue
                                    .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(s => s.Trim().ToLower())
                                    .ToList();
                                break;

                            case "ChemicalMatch_MinWordLength":
                                int.TryParse(settingValue, out _minWordLength);
                                break;
                        }
                    }
                }
            }

            _configLastLoaded = DateTime.Now;

            // ✅ Fallback defaults if settings are missing
            if (_broadSpectrumKeywords == null || _broadSpectrumKeywords.Count == 0)
            {
                _broadSpectrumKeywords = new List<string> { "broad spectrum", "multi-purpose", "general", "universal" };
            }

            if (_excludedWords == null || _excludedWords.Count == 0)
            {
                _excludedWords = new List<string> { "control", "service", "treatment", "system", "pest" };
            }

            System.Diagnostics.Debug.WriteLine($"✅ Chemical matching config loaded: {_broadSpectrumKeywords.Count} broad keywords, {_excludedWords.Count} excluded words, min length {_minWordLength}");
        }

        private bool DoesChemicalMatchService(string chemicalName, string serviceName, SqlConnection con, SqlTransaction tx = null)
        {
            if (string.IsNullOrWhiteSpace(chemicalName) || string.IsNullOrWhiteSpace(serviceName))
                return false;

            // ✅ Load configuration (uses cache if available)
            LoadChemicalMatchConfig(con, tx);

            chemicalName = chemicalName.ToLower();
            serviceName = serviceName.ToLower();

            // ✅ Check if chemical is broad spectrum (matches ALL services)
            foreach (var keyword in _broadSpectrumKeywords)
            {
                if (chemicalName.Contains(keyword))
                {
                    System.Diagnostics.Debug.WriteLine($"✅ '{chemicalName}' is broad spectrum (keyword: '{keyword}')");
                    return true;
                }
            }

            // ✅ Extract meaningful words from service name using DYNAMIC rules
            var serviceWords = serviceName
                .Split(new[] { ' ', ',', '-', '/', '&' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(word => word.Trim().ToLower())
                .Where(word => word.Length > _minWordLength)  // ✅ Dynamic minimum length
                .Where(word => !_excludedWords.Contains(word))  // ✅ Dynamic exclusion list
                .ToList();

            System.Diagnostics.Debug.WriteLine($"🔍 Service '{serviceName}' → Extracted words: {string.Join(", ", serviceWords)}");

            // ✅ Check if chemical name contains any meaningful service word
            foreach (var word in serviceWords)
            {
                if (chemicalName.Contains(word))
                {
                    System.Diagnostics.Debug.WriteLine($"✅ '{chemicalName}' matches '{serviceName}' (keyword: '{word}')");
                    return true;
                }
            }

            System.Diagnostics.Debug.WriteLine($"❌ '{chemicalName}' does NOT match '{serviceName}'");
            return false;
        }

        private decimal CalculateChemicalUsageForService(SqlConnection con, SqlTransaction tx, string chemicalName)
        {
            decimal totalUsage = 0;
            string serviceDetailsJson = Session["ServiceDetails"]?.ToString();

            if (!string.IsNullOrWhiteSpace(serviceDetailsJson) && serviceDetailsJson.TrimStart().StartsWith("["))
            {
                try
                {
                    var services = JArray.Parse(serviceDetailsJson);

                    foreach (var service in services)
                    {
                        string serviceName = service["ServiceName"]?.ToString() ?? "";
                        int sqm = Convert.ToInt32(service["SQM"] ?? 0);

                        // ✅ Only calculate usage if this chemical matches this service
                        if (DoesChemicalMatchService(chemicalName, serviceName, con, tx))
                        {
                            decimal usage = GetChemicalUsageBasedOnSQMProc(con, tx, sqm);
                            totalUsage += usage;

                            System.Diagnostics.Debug.WriteLine($"✅ Chemical '{chemicalName}' matches Service '{serviceName}' ({sqm} SQM) → {usage} mL");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"❌ Chemical '{chemicalName}' does NOT match Service '{serviceName}'");
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"📊 Total Usage for '{chemicalName}': {totalUsage} mL");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"CalculateChemicalUsageForService error: {ex.Message}");
                    // Fallback: use total SQM if JSON parsing fails
                    int totalSQM = Session["SQM"] != null ? Convert.ToInt32(Session["SQM"]) : 0;
                    totalUsage = GetChemicalUsageBasedOnSQMProc(con, tx, totalSQM);
                }
            }
            else
            {
                // OLD bookings: use combined total
                int totalSQM = Session["SQM"] != null ? Convert.ToInt32(Session["SQM"]) : 0;
                totalUsage = GetChemicalUsageBasedOnSQMProc(con, tx, totalSQM);
            }

            return totalUsage;
        }

        #endregion



        private void AddAuditLog(int? adminId, string action)
        {
            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spAudit_Insert", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@AdminID", SqlDbType.Int).Value = (object)adminId ?? DBNull.Value;
                    cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 255).Value = action;
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch { }
        }
    }
}
