using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI.WebControls;
using System.Web.UI;

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
                if (Request.QueryString["BookingID"] != null && int.TryParse(Request.QueryString["BookingID"], out bookingID))
                {
                    LoadTeams();
                    LoadAvailableEquipments();
                    LoadAvailableChemicals();
                    LoadAvailableSachetChemicals();
                    LoadSafetyGear();
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
            if (!int.TryParse(Request.QueryString["BookingID"], out bookingID))
            {
                lblMessage.Text = "❌ Booking ID is missing or invalid!";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            int teamID;
            if (!int.TryParse(ddlTeams.SelectedValue, out teamID))
            {
                lblMessage.Text = "⚠️ Please select a team.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            int adminId = Convert.ToInt32(Session["AdminID"]);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlTransaction trans = con.BeginTransaction();

                try
                {
                    int sqm = Session["SQM"] != null ? Convert.ToInt32(Session["SQM"]) : 0;
                    decimal usage = GetChemicalUsageBasedOnSQM(sqm);

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

                            TextBox txtBuffer = (TextBox)row.FindControl("txtBottleBuffer");
                            int bufferBottles = 0;
                            if (txtBuffer != null)
                                int.TryParse(txtBuffer.Text.Trim(), out bufferBottles);

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

                    foreach (GridViewRow row in gvSachetChemicals.Rows)
                    {
                        TextBox txtQty = (TextBox)row.FindControl("txtAssignSachet");
                        Label lblSachetQuantity = row.FindControl("lblSachetQuantity") as Label;

                        if (txtQty != null && int.TryParse(txtQty.Text.Trim(), out int qtyAssigned) && qtyAssigned > 0)
                        {
                            int itemId = Convert.ToInt32(gvSachetChemicals.DataKeys[row.RowIndex].Value);
                            int currentStock = 0;
                            if (lblSachetQuantity != null)
                                int.TryParse(lblSachetQuantity.Text, out currentStock);

                            if (qtyAssigned > currentStock)
                            {
                                lblMessage.Text = $"⚠️ Not enough sachet packs for Item ID {itemId}. Available: {currentStock}, Requested: {qtyAssigned}";
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

                    trans.Commit();
                    lblMessage.Text = "✅ Booking, chemical, and buffer assignment successful.";
                    lblMessage.ForeColor = System.Drawing.Color.Green;

                    AddAuditLog(adminId, $"Assigned team, chemicals, buffer, and sachets to Booking ID {bookingID}");
                }
                catch (Exception ex)
                {
                    trans.Rollback();
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
