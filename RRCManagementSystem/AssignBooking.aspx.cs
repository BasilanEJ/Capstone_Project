using RRCManagementSystem.Helpers;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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
                        lblBookingCode.Text = reader["BookingCode"].ToString();

                        // Show operation number only if it's a reschedule
                        if (scheduleID > 0 && reader["OperationNumber"] != DBNull.Value)
                        {
                            lblOperationNumber.Visible = true;
                            lblOperationNumber.Text = $"(Op #{reader["OperationNumber"]})";
                        }
                        else
                        {
                            lblOperationNumber.Visible = false;
                        }
                    }
                }
            }
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
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.spTeams_ListAvailable", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Date", SqlDbType.Date).Value = scheduledDate;

                con.Open();
                ddlTeams.DataSource = cmd.ExecuteReader();
                ddlTeams.DataTextField = "GroupName";
                ddlTeams.DataValueField = "TeamID";
                ddlTeams.DataBind();

                // Add a default "Select Team" option
                ddlTeams.Items.Insert(0, new ListItem("-- Select Team --", ""));
            }
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
                    gvEquipments.DataSource = dt;
                    gvEquipments.DataBind();
                }
            }
        }

        private void LoadAvailableChemicals()
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.spInventory_ListByType", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Type", SqlDbType.NVarChar, 50).Value = "Bottled Chemical";

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
                        // FOR RESCHEDULES: Get NEW date from ServiceSchedules
                        scheduledDate = GetNewScheduledDate(scheduleID);

                        if (scheduledDate == DateTime.MinValue)
                        {
                            throw new Exception("⚠️ Could not retrieve new scheduled date for this reschedule.");
                        }

                        // Get start time from original booking
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
                        // FOR INITIAL BOOKINGS: Get from Bookings table
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

                    // 4) Determine chemical usage based on SQM (via setting)
                    int sqm = Session["SQM"] != null ? Convert.ToInt32(Session["SQM"]) : 0;
                    decimal usage = GetChemicalUsageBasedOnSQMProc(con, tx, sqm);

                    // ✅ NEW: Validate Equipment Selection
                    int selectedEquipCount = 0;
                    var seen = new System.Collections.Generic.HashSet<int>();
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

                    // 5) Equipments (each selected = quantity 1)
                    seen.Clear(); // Reset the HashSet
                    foreach (GridViewRow row in gvEquipments.Rows)
                    {
                        var chk = row.FindControl("chkAssignEquip") as CheckBox;
                        if (chk != null && chk.Checked)
                        {
                            int equipmentId = Convert.ToInt32(gvEquipments.DataKeys[row.RowIndex].Value);
                            if (!seen.Add(equipmentId)) continue;

                            // enforce daily cap for this equipment (< 2)
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

                    // 6) Bottled chemicals
                    foreach (GridViewRow row in gvChemicals.Rows)
                    {
                        var chk = row.FindControl("chkUseChemical") as CheckBox;
                        if (chk == null || !chk.Checked) continue;

                        int itemId = Convert.ToInt32(gvChemicals.DataKeys[row.RowIndex].Value);

                        int quantity = int.Parse(((Label)row.FindControl("lblQuantity"))?.Text ?? "0");
                        decimal excess = decimal.Parse(((Label)row.FindControl("lblExcessML"))?.Text ?? "0");
                        int buffer = 0; int.TryParse(((TextBox)row.FindControl("txtBottleBuffer"))?.Text ?? "0", out buffer);

                        if (quantity < 5)
                        {
                            lblMessage.Text = $"⚠️ Cannot assign bottled chemical (Item {itemId}) — quantity below 5.";
                            lblMessage.ForeColor = System.Drawing.Color.Red;
                            tx.Rollback();
                            return;
                        }

                        // deduct usage from excess/quantity
                        if (excess >= usage) excess -= usage;
                        else
                        {
                            if (quantity <= 0)
                            {
                                lblMessage.Text = $"⚠️ Not enough bottled chemical for Item {itemId}.";
                                lblMessage.ForeColor = System.Drawing.Color.Red;
                                tx.Rollback();
                                return;
                            }
                            quantity--; // open a new bottle (1000 ml)
                            excess = 1000 + excess - usage;
                        }
                        if (buffer > 0)
                        {
                            if (buffer > quantity)
                            {
                                lblMessage.Text = $"⚠️ Buffer exceeds stock for Item {itemId}.";
                                lblMessage.ForeColor = System.Drawing.Color.Red;
                                tx.Rollback();
                                return;
                            }
                            quantity -= buffer;
                        }

                        // update inventory
                        using (var u = new SqlCommand("dbo.spInventory_Bottled_Update", con, tx))
                        {
                            u.CommandType = CommandType.StoredProcedure;
                            u.Parameters.Add("@ItemID", SqlDbType.Int).Value = itemId;
                            u.Parameters.Add("@Quantity", SqlDbType.Int).Value = quantity;
                            u.Parameters.Add("@ExcessML", SqlDbType.Decimal).Value = excess;
                            u.ExecuteNonQuery();
                        }

                        // record assignment: 1 bottle + optional buffer bottles
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
