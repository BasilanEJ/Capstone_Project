using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class AssignTeam : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;
        private int bookingID = 0;

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
          

            // 🔐 Validate BookingID from query
            if (!int.TryParse(Request.QueryString["BookingID"], out bookingID))
            {
                Response.Redirect("~ApproveRejectBookings.aspx");
                return;
            }

            lblBookingID.Text = bookingID.ToString();

            if (!IsPostBack)
            {
                LoadTeams();
                LoadEquipments();
                LoadChemicals();
            }
        }


        private void LoadTeams()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT TeamID, GroupName FROM Teams WHERE Status = 'Available'";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ddlTeams.DataSource = dt;
                ddlTeams.DataValueField = "TeamID";
                ddlTeams.DataTextField = "GroupName";
                ddlTeams.DataBind();
                ddlTeams.Items.Insert(0, new ListItem("Select a team", ""));
            }
        }

        private void LoadEquipments()
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

        private void LoadChemicals()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT ItemID, Name, Quantity FROM Inventory WHERE Type = 'Chemical' AND Quantity > 0";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvChemicals.DataSource = dt;
                gvChemicals.DataBind();
            }
        }

        protected void btnAssign_Click(object sender, EventArgs e)
        {
            if (ddlTeams.SelectedValue == "")
            {
                lblMessage.Text = "⚠️ Please select a team.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            int teamID = Convert.ToInt32(ddlTeams.SelectedValue);
            int adminId = Convert.ToInt32(Session["AdminID"]);

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string teamInsert = "INSERT INTO BookingTeams (BookingID, TeamID, Status) VALUES (@BookingID, @TeamID, 'Assigned')";
                    using (SqlCommand cmd = new SqlCommand(teamInsert, con))
                    {
                        cmd.Parameters.AddWithValue("@BookingID", bookingID);
                        cmd.Parameters.AddWithValue("@TeamID", teamID);
                        cmd.ExecuteNonQuery();
                    }

                    string updateTeamStatus = "UPDATE Teams SET Status = 'Unavailable' WHERE TeamID = @TeamID";
                    using (SqlCommand cmd = new SqlCommand(updateTeamStatus, con))
                    {
                        cmd.Parameters.AddWithValue("@TeamID", teamID);
                        cmd.ExecuteNonQuery();
                    }

                    foreach (GridViewRow row in gvEquipments.Rows)
                    {
                        CheckBox chkSelect = (CheckBox)row.FindControl("chkSelectEquipment");

                        if (chkSelect != null && chkSelect.Checked)
                        {
                            int equipmentID = Convert.ToInt32(gvEquipments.DataKeys[row.RowIndex].Value);

                            string equipInsert = "INSERT INTO BookingEquipments (BookingID, EquipmentID, QuantityAssigned) VALUES (@BookingID, @EquipmentID, 1)";
                            using (SqlCommand cmd = new SqlCommand(equipInsert, con))
                            {
                                cmd.Parameters.AddWithValue("@BookingID", bookingID);
                                cmd.Parameters.AddWithValue("@EquipmentID", equipmentID);
                                cmd.ExecuteNonQuery();
                            }

                            string updateEquipStatus = "UPDATE EquipmentStatus SET Status = 'Unavailable' WHERE EquipmentID = @EquipmentID";
                            using (SqlCommand cmd = new SqlCommand(updateEquipStatus, con))
                            {
                                cmd.Parameters.AddWithValue("@EquipmentID", equipmentID);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    foreach (GridViewRow row in gvChemicals.Rows)
                    {
                        int itemID = Convert.ToInt32(gvChemicals.DataKeys[row.RowIndex].Value);
                        TextBox txtQuantity = (TextBox)row.FindControl("txtQuantityAssign");
                        int quantityAssigned = int.Parse(txtQuantity.Text);

                        if (quantityAssigned > 0)
                        {
                            string chemInsert = "INSERT INTO BookingChemicals (BookingID, ItemID, QuantityAssigned) VALUES (@BookingID, @ItemID, @QuantityAssigned)";
                            using (SqlCommand cmd = new SqlCommand(chemInsert, con))
                            {
                                cmd.Parameters.AddWithValue("@BookingID", bookingID);
                                cmd.Parameters.AddWithValue("@ItemID", itemID);
                                cmd.Parameters.AddWithValue("@QuantityAssigned", quantityAssigned);
                                cmd.ExecuteNonQuery();
                            }

                            string updateInventoryQty = "UPDATE Inventory SET Quantity = Quantity - @Qty WHERE ItemID = @ItemID";
                            using (SqlCommand cmd = new SqlCommand(updateInventoryQty, con))
                            {
                                cmd.Parameters.AddWithValue("@Qty", quantityAssigned);
                                cmd.Parameters.AddWithValue("@ItemID", itemID);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    // ✅ Log the assignment
                    AddAuditLog(adminId, $"Assigned team and resources to Booking ID {bookingID}");

                    lblMessage.Text = "✅ Team, Equipments, and Chemicals assigned successfully!";
                    lblMessage.ForeColor = System.Drawing.Color.Green;

                    Response.Redirect("~/ADMIN/ApproveRejectBookings.aspx");
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "❌ Error: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        // ✅ Audit Logging Method
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
                        // Optional: silently log error
                    }
                }
            }
        }
    }
}