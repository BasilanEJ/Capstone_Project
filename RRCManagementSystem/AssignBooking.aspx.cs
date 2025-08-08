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
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT EquipmentID, Name, Status FROM EquipmentStatus WHERE Status = 'Available'";
                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    DataTable dt = new DataTable();
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
                    int sqm = Session["SQM"] != null ? Convert.ToInt32(Session["SQM"]) : 0;
                    decimal usage = GetChemicalUsageBasedOnSQM(sqm);

                    // 🔹 Bottled Chemicals
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

                            // ❌ Reject if stock below 5
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

                            SqlCommand updateChemical = new SqlCommand("UPDATE Inventory SET Quantity = @Quantity, ExcessML = @ExcessML WHERE ItemID = @ItemID", con, trans);
                            updateChemical.Parameters.AddWithValue("@Quantity", quantity);
                            updateChemical.Parameters.AddWithValue("@ExcessML", excessML);
                            updateChemical.Parameters.AddWithValue("@ItemID", itemId);
                            updateChemical.ExecuteNonQuery();

                            SqlCommand insertChemical = new SqlCommand("INSERT INTO BookingChemicals (BookingID, ItemID, QuantityAssigned, AssignedAt) VALUES (@BookingID, @ItemID, @QuantityAssigned, @AssignedAt)", con, trans);
                            insertChemical.Parameters.AddWithValue("@BookingID", bookingID);
                            insertChemical.Parameters.AddWithValue("@ItemID", itemId);
                            insertChemical.Parameters.AddWithValue("@QuantityAssigned", 1);
                            insertChemical.Parameters.AddWithValue("@AssignedAt", DateTime.Now);
                            insertChemical.ExecuteNonQuery();

                            if (bufferBottles > 0)
                            {
                                SqlCommand insertBuffer = new SqlCommand("INSERT INTO BookingChemicals (BookingID, ItemID, QuantityAssigned, AssignedAt) VALUES (@BookingID, @ItemID, @BufferQty, @AssignedAt)", con, trans);
                                insertBuffer.Parameters.AddWithValue("@BookingID", bookingID);
                                insertBuffer.Parameters.AddWithValue("@ItemID", itemId);
                                insertBuffer.Parameters.AddWithValue("@BufferQty", bufferBottles);
                                insertBuffer.Parameters.AddWithValue("@AssignedAt", DateTime.Now);
                                insertBuffer.ExecuteNonQuery();
                            }
                        }
                    }

                    // 🔹 Sachet Chemicals
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

                            SqlCommand updateSachet = new SqlCommand("UPDATE Inventory SET Quantity = Quantity - @QtyAssigned WHERE ItemID = @ItemID", con, trans);
                            updateSachet.Parameters.AddWithValue("@QtyAssigned", qtyAssigned);
                            updateSachet.Parameters.AddWithValue("@ItemID", itemId);
                            updateSachet.ExecuteNonQuery();

                            SqlCommand insertSachet = new SqlCommand("INSERT INTO BookingChemicals (BookingID, ItemID, QuantityAssigned, AssignedAt) VALUES (@BookingID, @ItemID, @QuantityAssigned, GETDATE())", con, trans);
                            insertSachet.Parameters.AddWithValue("@BookingID", bookingID);
                            insertSachet.Parameters.AddWithValue("@ItemID", itemId);
                            insertSachet.Parameters.AddWithValue("@QuantityAssigned", qtyAssigned);
                            insertSachet.ExecuteNonQuery();
                        }
                    }

                    // 🔹 Safety Gears
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

                            SqlCommand updateSafety = new SqlCommand("UPDATE Inventory SET Quantity = Quantity - @QtyAssigned WHERE ItemID = @ItemID", con, trans);
                            updateSafety.Parameters.AddWithValue("@QtyAssigned", qtyAssigned);
                            updateSafety.Parameters.AddWithValue("@ItemID", itemId);
                            updateSafety.ExecuteNonQuery();

                            SqlCommand insertSafety = new SqlCommand("INSERT INTO BookingChemicals (BookingID, ItemID, QuantityAssigned, AssignedAt) VALUES (@BookingID, @ItemID, @QuantityAssigned, GETDATE())", con, trans);
                            insertSafety.Parameters.AddWithValue("@BookingID", bookingID);
                            insertSafety.Parameters.AddWithValue("@ItemID", itemId);
                            insertSafety.Parameters.AddWithValue("@QuantityAssigned", qtyAssigned);
                            insertSafety.ExecuteNonQuery();
                        }
                    }

                    // 🔹 Team assignment + status update
                    SqlCommand cmd = new SqlCommand("UPDATE Bookings SET TeamID = @TeamID, Status = @Status WHERE BookingID = @BookingID", con, trans);
                    cmd.Parameters.AddWithValue("@Status", "Assigned");
                    cmd.Parameters.AddWithValue("@TeamID", teamId);
                    cmd.Parameters.AddWithValue("@BookingID", bookingID);
                    cmd.ExecuteNonQuery();

                    // 🔹 Fetch and combine scheduled date & time
                    DateTime scheduledDate = DateTime.Today;
                    TimeSpan startTime = new TimeSpan(8, 0, 0); // default 8 AM

                    SqlCommand getDateCmd = new SqlCommand("SELECT ScheduledDate, StartTime FROM Bookings WHERE BookingID = @BookingID", con, trans);
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
                        reader.Close();
                    }

                    DateTime scheduledDateTime = scheduledDate.Date.Add(startTime);

                    // 🔹 Generate contractual operations
                    SqlCommand checkExistingOps = new SqlCommand("SELECT COUNT(*) FROM ServiceSchedule WHERE BookingID = @BookingID", con, trans);
                    checkExistingOps.Parameters.AddWithValue("@BookingID", bookingID);
                    int opsExist = Convert.ToInt32(checkExistingOps.ExecuteScalar());

                    if (opsExist == 0)
                    {
                        SqlCommand checkContract = new SqlCommand(@"
                    SELECT COUNT(*) 
                    FROM BookingServices bs
                    INNER JOIN Services s ON bs.ServiceID = s.ServiceID
                    WHERE bs.BookingID = @BookingID AND s.IsContract = 1", con, trans);
                        checkContract.Parameters.AddWithValue("@BookingID", bookingID);
                        int contractCount = Convert.ToInt32(checkContract.ExecuteScalar());

                        if (contractCount > 0)
                        {
                            int[] monthOffsets = new int[] { 0, 1, 2, 5, 8, 11, 17, 23 };
                            for (int i = 1; i <= 8; i++)
                            {
                                DateTime expectedDate = scheduledDateTime.AddMonths(monthOffsets[i - 1]);
                                SqlCommand insertOp = new SqlCommand("INSERT INTO ServiceSchedule (BookingID, OperationNumber, ScheduledDate, Status, CreatedAt) VALUES (@BookingID, @OpNum, @ScheduledDate, 'Pending', GETDATE())", con, trans);
                                insertOp.Parameters.AddWithValue("@BookingID", bookingID);
                                insertOp.Parameters.AddWithValue("@OpNum", i);
                                insertOp.Parameters.AddWithValue("@ScheduledDate", expectedDate);
                                insertOp.ExecuteNonQuery();
                            }
                        }
                    }

                    // 🔹 Commit and audit
                    trans.Commit();
                    AddAuditLog(adminId, $"Assigned team (TeamID: {teamId}) and inventory to BookingID {bookingID}");
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
