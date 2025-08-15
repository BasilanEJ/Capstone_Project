using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class AssignBooking : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;
        private int bookingID;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 🔐 Require login
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string role = Session["Role"].ToString();

            // 🔐 Deny access for SuperAdmin and Inspector
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            // 🔐 Check CanEdit permission for ManageBooking
            if (!HasEditPermission(userId, "ManageBooking"))
            {
                lblMessage.Text = "❌ You do not have permission to assign bookings.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                btnAssignAll.Enabled = false;
                return;
            }

            if (!IsPostBack)
            {
                hfConfirmAssign.Value = "false";
                if (Request.QueryString["BookingID"] != null && int.TryParse(Request.QueryString["BookingID"], out bookingID))
                {
                    LoadTeams();
                    LoadAvailableEquipments();
                    LoadAvailableChemicals();
                    LoadAvailableSachetChemicals();
                    LoadSafetyGear();

                    // ✅ Show SweetAlert if just assigned
                    if (Request.QueryString["status"] == "Assigned")
                    {
                        string script = @"Swal.fire({
                icon: 'success',
                title: 'Assigned!',
                text: 'The booking was successfully assigned.',
                showConfirmButton: false,
                timer: 2000
            });";
                        ClientScript.RegisterStartupScript(this.GetType(), "AssignSuccess", script, true);
                    }
                }
                else
                {
                    lblMessage.Text = "⚠️ Invalid Booking ID.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    btnAssignAll.Enabled = false;
                }
            }

        }

        private bool HasEditPermission(int adminId, string moduleName)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT CanEdit FROM AdminPermissions WHERE UserID = @AdminID AND ModuleName = @ModuleName";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@AdminID", adminId);
                    cmd.Parameters.AddWithValue("@ModuleName", moduleName);
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null && result != DBNull.Value && Convert.ToBoolean(result);
                }
            }
        }

        private void LoadTeams()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT TeamID, GroupName FROM Teams WHERE Status = 'Available'";
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                ddlTeams.DataSource = cmd.ExecuteReader();
                ddlTeams.DataTextField = "GroupName";
                ddlTeams.DataValueField = "TeamID";
                ddlTeams.DataBind();
                ddlTeams.Items.Insert(0, new ListItem("-- Select Team --", ""));
            }
        }

        private void LoadAvailableEquipments()
        {
            // Get this booking's scheduled date
            DateTime schedDate = DateTime.Today;
            using (var con = new SqlConnection(connectionString))
            using (var getDateCmd = new SqlCommand(
                "SELECT ScheduledDate FROM Bookings WHERE BookingID=@BookingID", con))
            {
                getDateCmd.Parameters.AddWithValue("@BookingID", bookingID);
                con.Open();
                object d = getDateCmd.ExecuteScalar();
                if (d != null && d != DBNull.Value) schedDate = Convert.ToDateTime(d);
            }

            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"
        WITH counts AS (
            SELECT be.EquipmentID, COUNT(*) AS Cnt
            FROM BookingEquipments be
            INNER JOIN Bookings b ON b.BookingID = be.BookingID
            WHERE CAST(b.ScheduledDate AS date) = @D
            GROUP BY be.EquipmentID
        )
        SELECT e.EquipmentID, e.Name, e.Status
        FROM EquipmentStatus e
        LEFT JOIN counts c ON c.EquipmentID = e.EquipmentID
        WHERE ISNULL(c.Cnt,0) < 2   -- strictly less than 2 assignments that day
        ORDER BY e.Name;", con))
            {
                cmd.Parameters.Add("@D", SqlDbType.Date).Value = schedDate.Date;
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                gvEquipments.DataSource = dt;
                gvEquipments.DataBind();
            }
        }


        private void LoadAvailableChemicals()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT ItemID, Name, Quantity, ISNULL(ExcessML, 0) AS ExcessML FROM Inventory WHERE Type = 'Bottled Chemical' AND Quantity >= 0";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvChemicals.DataSource = dt;
                gvChemicals.DataBind();
            }
        }

        private void LoadAvailableSachetChemicals()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT ItemID, Name, Quantity FROM Inventory WHERE Type = 'Sachet Pack Chemical' AND Quantity >= 0";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvSachetChemicals.DataSource = dt;
                gvSachetChemicals.DataBind();
            }
        }

        private void LoadSafetyGear()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT ItemID, Name, Quantity FROM Inventory WHERE Type = 'Safety Gear'";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvSafetyGears.DataSource = dt;
                gvSafetyGears.DataBind();
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

            if (ddlTeams.SelectedIndex <= 0)
            {
                lblMessage.Text = "⚠️ Please select a team.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            int teamId = Convert.ToInt32(ddlTeams.SelectedValue);
            int adminId = Convert.ToInt32(Session["UserID"]);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlTransaction trans = con.BeginTransaction();

                try
                {
                    // 1) Get booking schedule (date-only used for per-day caps)
                    DateTime scheduledDate = DateTime.Today;
                    TimeSpan startTime = new TimeSpan(8, 0, 0);

                    using (SqlCommand getDateCmd = new SqlCommand(
                        "SELECT ScheduledDate, StartTime FROM Bookings WHERE BookingID = @BookingID", con, trans))
                    {
                        getDateCmd.Parameters.AddWithValue("@BookingID", bookingID);
                        using (SqlDataReader reader = getDateCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                if (reader["ScheduledDate"] != DBNull.Value)
                                    scheduledDate = Convert.ToDateTime(reader["ScheduledDate"]);
                                if (reader["StartTime"] != DBNull.Value)
                                    startTime = (TimeSpan)reader["StartTime"];
                            }
                        }
                    }
                    DateTime scheduledDateTime = scheduledDate.Date.Add(startTime);

                    // 2) TEAM CAP via BookingTeams (max 2 bookings for a team on that date)
                    using (var teamCntCmd = new SqlCommand(@"
                SELECT COUNT(*)
                FROM BookingTeams bt
                INNER JOIN Bookings b ON b.BookingID = bt.BookingID
                WHERE bt.TeamID = @TeamID
                  AND CAST(b.ScheduledDate AS date) = @SchedDate;", con, trans))
                    {
                        teamCntCmd.Parameters.AddWithValue("@TeamID", teamId);
                        teamCntCmd.Parameters.Add("@SchedDate", SqlDbType.Date).Value = scheduledDate.Date;

                        int todaysTeamCount = Convert.ToInt32(teamCntCmd.ExecuteScalar() ?? 0);
                        if (todaysTeamCount >= 2)
                        {
                            lblMessage.Text = $"⚠️ Team {teamId} is already fully booked for {scheduledDate:yyyy-MM-dd}.";
                            lblMessage.ForeColor = System.Drawing.Color.Red;
                            trans.Rollback();
                            return;
                        }
                    }

                    // 3) Upsert into BookingTeams (avoid duplicate row for same booking/team)
                    bool teamRowExists;
                    using (var chkTeam = new SqlCommand(
                        "SELECT 1 FROM BookingTeams WHERE BookingID=@B AND TeamID=@T;", con, trans))
                    {
                        chkTeam.Parameters.AddWithValue("@B", bookingID);
                        chkTeam.Parameters.AddWithValue("@T", teamId);
                        teamRowExists = chkTeam.ExecuteScalar() != null;
                    }

                    if (teamRowExists)
                    {
                        using (var updTeam = new SqlCommand(
                            "UPDATE BookingTeams SET AssignedAt=GETDATE(), Status='Assigned' WHERE BookingID=@B AND TeamID=@T;", con, trans))
                        {
                            updTeam.Parameters.AddWithValue("@B", bookingID);
                            updTeam.Parameters.AddWithValue("@T", teamId);
                            updTeam.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        using (var insTeam = new SqlCommand(
                            "INSERT INTO BookingTeams (BookingID, TeamID, AssignedAt, Status) VALUES (@B,@T,GETDATE(),'Assigned');", con, trans))
                        {
                            insTeam.Parameters.AddWithValue("@B", bookingID);
                            insTeam.Parameters.AddWithValue("@T", teamId);
                            insTeam.ExecuteNonQuery();
                        }
                    }

                    // 4) Usage for chemicals
                    int sqm = Session["SQM"] != null ? Convert.ToInt32(Session["SQM"]) : 0;
                    decimal usage = GetChemicalUsageBasedOnSQM(sqm);

                    // 5) EQUIPMENTS: enforce ≤ 2 per day (date-only), then insert into BookingEquipments
                    int selectedEquipCount = 0;
                    var seenEquip = new System.Collections.Generic.HashSet<int>();

                    foreach (GridViewRow row in gvEquipments.Rows)
                    {
                        CheckBox chk = row.FindControl("chkAssignEquip") as CheckBox;
                        if (chk != null && chk.Checked)
                        {
                            int equipmentId = Convert.ToInt32(gvEquipments.DataKeys[row.RowIndex].Value);
                            if (!seenEquip.Add(equipmentId)) continue; // avoid duplicate selection in one postback

                            using (var cntCmd = new SqlCommand(@"
                        SELECT COUNT(*)
                        FROM BookingEquipments be
                        INNER JOIN Bookings b ON b.BookingID = be.BookingID
                        WHERE be.EquipmentID = @EquipmentID
                          AND CAST(b.ScheduledDate AS date) = @SchedDate;", con, trans))
                            {
                                cntCmd.Parameters.AddWithValue("@EquipmentID", equipmentId);
                                cntCmd.Parameters.Add("@SchedDate", SqlDbType.Date).Value = scheduledDate.Date;

                                int todaysCount = Convert.ToInt32(cntCmd.ExecuteScalar() ?? 0);
                                if (todaysCount >= 2)
                                {
                                    lblMessage.Text = $"⚠️ Equipment {equipmentId} is already fully booked for {scheduledDate:yyyy-MM-dd}.";
                                    lblMessage.ForeColor = System.Drawing.Color.Red;
                                    trans.Rollback();
                                    return;
                                }
                            }

                            using (var insEq = new SqlCommand(
                                "INSERT INTO BookingEquipments (BookingID, EquipmentID, AssignedAt) VALUES (@B,@E,GETDATE());", con, trans))
                            {
                                insEq.Parameters.AddWithValue("@B", bookingID);
                                insEq.Parameters.AddWithValue("@E", equipmentId);
                                insEq.ExecuteNonQuery();
                            }

                            selectedEquipCount++;
                        }
                    }

                    if (selectedEquipCount == 0)
                    {
                        lblMessage.Text = "⚠️ Please select at least one equipment.";
                        lblMessage.ForeColor = System.Drawing.Color.Red;
                        trans.Rollback();
                        return;
                    }

                    // 6) BOTTLED CHEMICALS
                    foreach (GridViewRow row in gvChemicals.Rows)
                    {
                        CheckBox chk = (CheckBox)row.FindControl("chkUseChemical");
                        if (chk != null && chk.Checked)
                        {
                            int itemId = Convert.ToInt32(gvChemicals.DataKeys[row.RowIndex].Value);
                            Label lblQuantity = row.FindControl("lblQuantity") as Label;
                            Label lblExcessML = row.FindControl("lblExcessML") as Label;

                            int quantity = 0;
                            decimal excessML = 0;
                            if (lblQuantity != null) int.TryParse(lblQuantity.Text, out quantity);
                            if (lblExcessML != null) decimal.TryParse(lblExcessML.Text, out excessML);

                            if (quantity < 5)
                            {
                                lblMessage.Text = $"⚠️ Cannot assign bottled chemical (Item ID {itemId}) — quantity below 5.";
                                lblMessage.ForeColor = System.Drawing.Color.Red;
                                trans.Rollback();
                                return;
                            }

                            TextBox txtBuffer = (TextBox)row.FindControl("txtBottleBuffer");
                            int bufferBottles = 0;
                            if (txtBuffer != null) int.TryParse(txtBuffer.Text.Trim(), out bufferBottles);

                            if (excessML >= usage)
                            {
                                excessML -= usage;
                            }
                            else
                            {
                                if (quantity > 0)
                                {
                                    quantity--;
                                    excessML = 1000 + excessML - usage;
                                }
                                else
                                {
                                    lblMessage.Text = $"⚠️ Not enough bottled chemical for Item ID {itemId}";
                                    lblMessage.ForeColor = System.Drawing.Color.Red;
                                    trans.Rollback();
                                    return;
                                }
                            }

                            if (bufferBottles > 0)
                            {
                                if (bufferBottles > quantity)
                                {
                                    lblMessage.Text = $"⚠️ Buffer amount exceeds stock for Item ID {itemId}";
                                    lblMessage.ForeColor = System.Drawing.Color.Red;
                                    trans.Rollback();
                                    return;
                                }
                                quantity -= bufferBottles;
                            }

                            using (SqlCommand updateChemical = new SqlCommand(
                                "UPDATE Inventory SET Quantity = @Quantity, ExcessML = @ExcessML WHERE ItemID = @ItemID", con, trans))
                            {
                                updateChemical.Parameters.AddWithValue("@Quantity", quantity);
                                updateChemical.Parameters.AddWithValue("@ExcessML", excessML);
                                updateChemical.Parameters.AddWithValue("@ItemID", itemId);
                                updateChemical.ExecuteNonQuery();
                            }

                            using (SqlCommand insertChemical = new SqlCommand(
                                "INSERT INTO BookingChemicals (BookingID, ItemID, QuantityAssigned, AssignedAt) VALUES (@B, @I, @Q, @D)", con, trans))
                            {
                                insertChemical.Parameters.AddWithValue("@B", bookingID);
                                insertChemical.Parameters.AddWithValue("@I", itemId);
                                insertChemical.Parameters.AddWithValue("@Q", 1);
                                insertChemical.Parameters.AddWithValue("@D", DateTime.Now);
                                insertChemical.ExecuteNonQuery();
                            }

                            if (bufferBottles > 0)
                            {
                                using (SqlCommand insertBuffer = new SqlCommand(
                                    "INSERT INTO BookingChemicals (BookingID, ItemID, QuantityAssigned, AssignedAt) VALUES (@B, @I, @Q, @D)", con, trans))
                                {
                                    insertBuffer.Parameters.AddWithValue("@B", bookingID);
                                    insertBuffer.Parameters.AddWithValue("@I", itemId);
                                    insertBuffer.Parameters.AddWithValue("@Q", bufferBottles);
                                    insertBuffer.Parameters.AddWithValue("@D", DateTime.Now);
                                    insertBuffer.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    // 7) SACHET CHEMICALS
                    foreach (GridViewRow row in gvSachetChemicals.Rows)
                    {
                        TextBox txtQty = (TextBox)row.FindControl("txtAssignSachet");
                        Label lblSachetQuantity = row.FindControl("lblSachetQuantity") as Label;

                        if (txtQty != null && int.TryParse(txtQty.Text.Trim(), out int qtyAssigned) && qtyAssigned > 0)
                        {
                            int itemId = Convert.ToInt32(gvSachetChemicals.DataKeys[row.RowIndex].Value);
                            int currentStock = 0;
                            if (lblSachetQuantity != null) int.TryParse(lblSachetQuantity.Text, out currentStock);

                            if (currentStock < 5)
                            {
                                lblMessage.Text = $"⚠️ Cannot assign sachet chemical (Item ID {itemId}) — quantity below 5.";
                                lblMessage.ForeColor = System.Drawing.Color.Red;
                                trans.Rollback();
                                return;
                            }

                            if (qtyAssigned > currentStock)
                            {
                                lblMessage.Text = $"⚠️ Not enough sachet packs for Item ID {itemId}.";
                                lblMessage.ForeColor = System.Drawing.Color.Red;
                                trans.Rollback();
                                return;
                            }

                            using (SqlCommand updateSachet = new SqlCommand(
                                "UPDATE Inventory SET Quantity = Quantity - @Qty WHERE ItemID = @ItemID", con, trans))
                            {
                                updateSachet.Parameters.AddWithValue("@Qty", qtyAssigned);
                                updateSachet.Parameters.AddWithValue("@ItemID", itemId);
                                updateSachet.ExecuteNonQuery();
                            }

                            using (SqlCommand insertSachet = new SqlCommand(
                                "INSERT INTO BookingChemicals (BookingID, ItemID, QuantityAssigned, AssignedAt) VALUES (@B, @I, @Q, GETDATE())", con, trans))
                            {
                                insertSachet.Parameters.AddWithValue("@B", bookingID);
                                insertSachet.Parameters.AddWithValue("@I", itemId);
                                insertSachet.Parameters.AddWithValue("@Q", qtyAssigned);
                                insertSachet.ExecuteNonQuery();
                            }
                        }
                    }

                    // 8) SAFETY GEAR
                    foreach (GridViewRow row in gvSafetyGears.Rows)
                    {
                        TextBox txtQty = (TextBox)row.FindControl("txtAssignSafety");
                        Label lblStock = row.FindControl("lblSafetyQuantity") as Label;

                        if (txtQty != null && int.TryParse(txtQty.Text.Trim(), out int qtyAssigned) && qtyAssigned > 0)
                        {
                            int itemId = Convert.ToInt32(gvSafetyGears.DataKeys[row.RowIndex].Value);
                            int currentStock = 0;
                            if (lblStock != null) int.TryParse(lblStock.Text, out currentStock);

                            if (currentStock < 5)
                            {
                                lblMessage.Text = $"⚠️ Cannot assign safety gear (Item ID {itemId}) — quantity below 5.";
                                lblMessage.ForeColor = System.Drawing.Color.Red;
                                trans.Rollback();
                                return;
                            }

                            if (qtyAssigned > currentStock)
                            {
                                lblMessage.Text = $"⚠️ Not enough safety gear for Item ID {itemId}.";
                                lblMessage.ForeColor = System.Drawing.Color.Red;
                                trans.Rollback();
                                return;
                            }

                            using (SqlCommand updateSafety = new SqlCommand(
                                "UPDATE Inventory SET Quantity = Quantity - @Qty WHERE ItemID = @ItemID", con, trans))
                            {
                                updateSafety.Parameters.AddWithValue("@Qty", qtyAssigned);
                                updateSafety.Parameters.AddWithValue("@ItemID", itemId);
                                updateSafety.ExecuteNonQuery();
                            }

                            using (SqlCommand insertSafety = new SqlCommand(
                                "INSERT INTO BookingChemicals (BookingID, ItemID, QuantityAssigned, AssignedAt) VALUES (@B, @I, @Q, GETDATE())", con, trans))
                            {
                                insertSafety.Parameters.AddWithValue("@B", bookingID);
                                insertSafety.Parameters.AddWithValue("@I", itemId);
                                insertSafety.Parameters.AddWithValue("@Q", qtyAssigned);
                                insertSafety.ExecuteNonQuery();
                            }
                        }
                    }

                    // 9) Update base Bookings row (kept for compatibility)
                    using (SqlCommand cmd = new SqlCommand(
                        "UPDATE Bookings SET TeamID = @TeamID, Status = @Status WHERE BookingID = @BookingID", con, trans))
                    {
                        cmd.Parameters.AddWithValue("@Status", "Assigned");
                        cmd.Parameters.AddWithValue("@TeamID", teamId);
                        cmd.Parameters.AddWithValue("@BookingID", bookingID);
                        cmd.ExecuteNonQuery();
                    }

                    // 10) Generate contractual operations (unchanged)
                    int opsExist = 0;
                    using (SqlCommand checkExistingOps = new SqlCommand(
                        "SELECT COUNT(*) FROM ServiceSchedule WHERE BookingID = @B", con, trans))
                    {
                        checkExistingOps.Parameters.AddWithValue("@B", bookingID);
                        opsExist = Convert.ToInt32(checkExistingOps.ExecuteScalar());
                    }

                    if (opsExist == 0)
                    {
                        int contractCount = 0;
                        using (SqlCommand checkContract = new SqlCommand(@"
                    SELECT COUNT(*)
                    FROM BookingServices bs
                    INNER JOIN Services s ON bs.ServiceID = s.ServiceID
                    WHERE bs.BookingID = @B AND s.IsContract = 1", con, trans))
                        {
                            checkContract.Parameters.AddWithValue("@B", bookingID);
                            contractCount = Convert.ToInt32(checkContract.ExecuteScalar());
                        }

                        if (contractCount > 0)
                        {
                            int[] monthOffsets = new int[] { 0, 1, 2, 5, 8, 11, 17, 23 };
                            for (int i = 1; i <= 8; i++)
                            {
                                DateTime expectedDate = scheduledDateTime.AddMonths(monthOffsets[i - 1]);
                                using (SqlCommand insertOp = new SqlCommand(
                                    "INSERT INTO ServiceSchedule (BookingID, OperationNumber, ScheduledDate, Status, CreatedAt) VALUES (@B, @Op, @Dt, 'Pending', GETDATE())", con, trans))
                                {
                                    insertOp.Parameters.AddWithValue("@B", bookingID);
                                    insertOp.Parameters.AddWithValue("@Op", i);
                                    insertOp.Parameters.AddWithValue("@Dt", expectedDate);
                                    insertOp.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    // ✅ Commit + audit
                    trans.Commit();
                    AddAuditLog(adminId, $"Assigned Team {teamId} + equipment/consumables to BookingID {bookingID}");
                    Response.Redirect("AssignBooking.aspx?BookingID=" + bookingID + "&status=Assigned");
                }
                catch (Exception ex)
                {
                    try { trans.Rollback(); } catch { }
                    lblMessage.Text = "❌ Error: " + ex.Message;
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
            }
        }


        private decimal GetChemicalUsageBasedOnSQM(int sqm)
        {
            string settingName = "BottledChemicalUsageML100sqm";
            if (sqm >= 101 && sqm <= 200)
                settingName = "BottledChemicalUsageML200sqm";
            else if (sqm > 200)
                settingName = "BottledChemicalUsageML200Plus";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT SettingValue FROM SystemSettings WHERE SettingName = @SettingName";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@SettingName", settingName);
                conn.Open();
                object result = cmd.ExecuteScalar();
                return result != null && decimal.TryParse(result.ToString(), out decimal usage) ? usage : 333;
            }
        }


        private void AddAuditLog(int? userID, string action)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO AuditLogs (AdminID, Action, Timestamp) VALUES (@AdminID, @Action, GETDATE())";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@AdminID", (object)userID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Action", action);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}