using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class ManageInquiry : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["AdminID"] == null)
                Response.Redirect("~/Login.aspx");

            if (!IsPostBack)
                LoadInquiries();
        }

        protected void btnAssignHidden_Click(object sender, EventArgs e)
        {
            string[] parts = hfAssignData.Value.Split('|');
            if (parts.Length >= 9)
            {
                int inspectorId = int.Parse(parts[0]);
                DateTime schedule = DateTime.Parse(parts[1]);
                string remarks = parts[2];
                string name = parts[3];
                string street = parts[4];
                string barangay = parts[5];
                string city = parts[6];
                string region = parts[7];
                string country = parts[8];
                int inquiryId = int.Parse(hfSelectedInquiryID.Value);

                int maxPerDay = GetSystemSettingInt("MaxInspectionsPerDay");
                if (GetInspectionsCount(inspectorId, schedule.Date) >= maxPerDay)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "LimitReached",
                        "Swal.fire('Max Limit Reached', 'Inspector already has the maximum inspections on this day.', 'error');", true);
                    return;
                }

                UpdateInquiryInfo(inquiryId, name, street, barangay, city, region, country);
                AssignInspector(inquiryId, inspectorId, schedule, remarks);
                ScriptManager.RegisterStartupScript(this, GetType(), "Success",
                    "Swal.fire('Success', 'Inspector assigned successfully.', 'success');", true);
            }
        }

        private void LoadInquiries()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT InquiryID, Email, ContactNumber, Message, SubmittedAt FROM InquirySimple
                                 WHERE NOT EXISTS (SELECT 1 FROM Inspections WHERE Inspections.InquiryID = InquirySimple.InquiryID)
                                 ORDER BY SubmittedAt DESC";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvInquiries.DataSource = dt;
                gvInquiries.DataBind();
            }
        }

        private int GetSystemSettingInt(string settingName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT SettingValue FROM SystemSettings WHERE SettingName = @SettingName";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@SettingName", settingName);
                conn.Open();
                object result = cmd.ExecuteScalar();
                return result != null && int.TryParse(result.ToString(), out int value) ? value : 6;
            }
        }

        private int GetInspectionsCount(int inspectorId, DateTime date)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM Inspections WHERE InspectorID = @InspectorID AND CAST(ScheduledDate AS DATE) = @Date";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@InspectorID", inspectorId);
                cmd.Parameters.AddWithValue("@Date", date);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private void UpdateInquiryInfo(int inquiryId, string name, string street, string barangay, string city, string region, string country)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"UPDATE InquirySimple
                                 SET Name = @Name, StreetAndUnit = @Street, Barangay = @Barangay,
                                     City = @City, Region = @Region, Country = @Country
                                 WHERE InquiryID = @InquiryID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Street", street);
                cmd.Parameters.AddWithValue("@Barangay", barangay);
                cmd.Parameters.AddWithValue("@City", city);
                cmd.Parameters.AddWithValue("@Region", region);
                cmd.Parameters.AddWithValue("@Country", country);
                cmd.Parameters.AddWithValue("@InquiryID", inquiryId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void AssignInspector(int inquiryId, int inspectorUserId, DateTime scheduleDate, string remarks)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string insertQuery = @"INSERT INTO Inspections (InquiryID, InspectorID, ScheduledDate, InspectionStatus, Remarks, CreatedAt)
                                       VALUES (@InquiryID, @InspectorID, @ScheduledDate, 'Pending', @Remarks, GETDATE())";

                SqlCommand cmd = new SqlCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("@InquiryID", inquiryId);
                cmd.Parameters.AddWithValue("@InspectorID", inspectorUserId); // Must exist in Users table
                cmd.Parameters.AddWithValue("@ScheduledDate", scheduleDate);
                cmd.Parameters.AddWithValue("@Remarks", remarks);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            LoadInquiries();
        }

        public string GetInspectorOptions()
        {
            string options = "";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT UserID, Name FROM Users WHERE Role = 'Inspector' AND Status = 'Active'";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    options += $"<option value='{reader["UserID"]}'>{reader["Name"]}</option>";
                }
            }
            return options;
        }

        protected void gvInquiries_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // No commands implemented yet (this is required to prevent compilation errors)
        }
    }
}
