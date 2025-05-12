using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class ViewEquipment : System.Web.UI.Page
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

            // ✅ Check CanView permission for ManageEquipment module
            if (!HasViewPermission(adminId, "ManageEquipment"))
            {
                Response.Redirect("~/Unauthorized.aspx");
                return;
            }

            if (!IsPostBack)
            {
                txtFilterDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
                LoadEquipmentsWithAvailability(DateTime.Today);
            }
        }

        private bool HasViewPermission(int adminId, string moduleName)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT CanView FROM AdminPermissions WHERE UserID = @UserID AND ModuleName = @ModuleName";

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
                    catch
                    {
                        return false;
                    }
                }
            }
        }

        private void LoadEquipmentsWithAvailability(DateTime targetDate)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Step 1: Get all equipments
                    string queryEquipments = @"
                SELECT EquipmentID, Name, Status AS OriginalStatus, ImagePath, CreatedAt 
                FROM EquipmentStatus 
                ORDER BY CreatedAt DESC";

                    SqlDataAdapter daEquipments = new SqlDataAdapter(queryEquipments, con);
                    DataTable dtEquipments = new DataTable();
                    daEquipments.Fill(dtEquipments);

                    // Step 2: Get assigned equipments with count
                    string queryAssigned = @"
                SELECT BE.EquipmentID, COUNT(*) AS AssignmentsCount
                FROM BookingEquipments BE
                INNER JOIN Bookings B ON BE.BookingID = B.BookingID
                WHERE CAST(B.ScheduledDate AS DATE) = @ScheduledDate
                GROUP BY BE.EquipmentID";

                    SqlCommand cmdAssigned = new SqlCommand(queryAssigned, con);
                    cmdAssigned.Parameters.AddWithValue("@ScheduledDate", targetDate);

                    SqlDataAdapter daAssigned = new SqlDataAdapter(cmdAssigned);
                    DataTable dtAssigned = new DataTable();
                    daAssigned.Fill(dtAssigned);

                    var assignedEquipmentsCount = dtAssigned.AsEnumerable()
                        .ToDictionary(
                            r => r.Field<int>("EquipmentID"),
                            r => r.Field<int>("AssignmentsCount")
                        );

                    // Step 3: Add derived status
                    var equipmentList = dtEquipments.AsEnumerable().Select(eq => new
                    {
                        EquipmentID = eq.Field<int>("EquipmentID"),
                        Name = eq.Field<string>("Name"),
                        ImagePath = eq.Field<string>("ImagePath"),
                        CreatedAt = eq.Field<DateTime>("CreatedAt"),
                        Status = assignedEquipmentsCount.ContainsKey(eq.Field<int>("EquipmentID")) && assignedEquipmentsCount[eq.Field<int>("EquipmentID")] >= 2
                            ? "Unavailable" : "Available"
                    }).ToList();

                    gvEquipment.DataSource = equipmentList;
                    gvEquipment.DataBind();

                    lblMessage.Text = $"✅ Equipments loaded for {targetDate:yyyy-MM-dd}.";
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"❌ Error loading equipments: {ex.Message}";
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }


        protected void ddlStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterAndLoadEquipment();
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            if (DateTime.TryParse(txtFilterDate.Text, out DateTime selectedDate))
            {
                LoadEquipmentsWithAvailability(selectedDate);
            }
            else
            {
                lblMessage.Text = "⚠️ Please select a valid date!";
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void FilterAndLoadEquipment()
        {
            DateTime selectedDate = DateTime.TryParse(txtFilterDate.Text, out DateTime date)
                ? date
                : DateTime.Today;

            LoadEquipmentsWithAvailability(selectedDate);
        }

        protected void gvEquipment_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvEquipment.PageIndex = e.NewPageIndex;
            FilterAndLoadEquipment();
        }

        protected void gvEquipment_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DeleteEquipment")
            {
                int equipmentId = Convert.ToInt32(e.CommandArgument);

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM EquipmentStatus WHERE EquipmentID = @EquipmentID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@EquipmentID", equipmentId);

                    try
                    {
                        con.Open();
                        int rows = cmd.ExecuteNonQuery();
                        lblMessage.Text = rows > 0 ? "✅ Equipment deleted successfully." : "⚠️ Equipment not found.";
                        lblMessage.ForeColor = rows > 0 ? System.Drawing.Color.Green : System.Drawing.Color.OrangeRed;
                    }
                    catch (Exception ex)
                    {
                        lblMessage.Text = $"❌ Error deleting equipment: {ex.Message}";
                        lblMessage.ForeColor = System.Drawing.Color.Red;
                    }
                }

                FilterAndLoadEquipment(); // Refresh
            }
        }

    }
}