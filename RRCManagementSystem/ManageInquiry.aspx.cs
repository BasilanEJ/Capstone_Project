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
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string role = Session["Role"].ToString();
            if (role == "Inspector" || role == "SuperAdmin")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadInquiries();
            }
        }

        protected void btnAssignHidden_Click(object sender, EventArgs e)
        {
            string[] parts = hfAssignData.Value.Split('|');
            if (parts.Length >= 12)
            {
                int inspectorId = int.Parse(parts[0]);
                DateTime schedule = DateTime.Parse(parts[1]);
                string remarks = parts[2];
                string firstName = parts[3];
                string middleName = parts[4];
                string lastName = parts[5];
                string street = parts[6];
                string barangay = parts[7];
                string city = parts[8];
                string region = parts[9];
                string country = parts[10];
                string landmark = parts[11];
                int inquiryId = int.Parse(hfSelectedInquiryID.Value);

                int maxPerDay = GetSystemSettingInt("MaxInspectionsPerDay");
                if (GetInspectionsCount(inspectorId, schedule.Date) >= maxPerDay)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "LimitReached",
                        "Swal.fire('Max Limit Reached', 'Inspector already has the maximum inspections on this day.', 'error');", true);
                    return;
                }

                UpdateInquiryInfo(inquiryId, firstName, middleName, lastName, street, barangay, city, region, country, landmark);
                AssignInspector(inquiryId, inspectorId, schedule, remarks);

                ScriptManager.RegisterStartupScript(this, GetType(), "Success",
                    "Swal.fire('Success', 'Inspector assigned successfully.', 'success');", true);
            }
        }

        private void LoadInquiries()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
        SELECT InquiryID, Email, ContactNumber, Message, SubmittedAt, PhotoPath
        FROM InquirySimple
        WHERE NOT EXISTS (
            SELECT 1 FROM Inspections WHERE Inspections.InquiryID = InquirySimple.InquiryID
        )
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

        private void UpdateInquiryInfo(int inquiryId, string firstName, string middleName, string lastName, string street, string barangay, string city, string region, string country, string landmark)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"UPDATE InquirySimple
                         SET FirstName = @FirstName, MiddleName = @MiddleName, LastName = @LastName,
                             StreetAndUnit = @Street, Barangay = @Barangay,
                             City = @City, Region = @Region, Country = @Country, Landmark = @Landmark
                         WHERE InquiryID = @InquiryID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@FirstName", firstName);
                cmd.Parameters.AddWithValue("@MiddleName", string.IsNullOrEmpty(middleName) ? DBNull.Value : (object)middleName);
                cmd.Parameters.AddWithValue("@LastName", lastName);
                cmd.Parameters.AddWithValue("@Street", street);
                cmd.Parameters.AddWithValue("@Barangay", barangay);
                cmd.Parameters.AddWithValue("@City", city);
                cmd.Parameters.AddWithValue("@Region", region);
                cmd.Parameters.AddWithValue("@Country", country);
                cmd.Parameters.AddWithValue("@Landmark", string.IsNullOrEmpty(landmark) ? DBNull.Value : (object)landmark);
                cmd.Parameters.AddWithValue("@InquiryID", inquiryId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void AssignInspector(int inquiryId, int inspectorUserId, DateTime scheduleDate, string remarks)
        {
            if (!InquiryExists(inquiryId)) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string insertQuery = @"INSERT INTO Inspections (InquiryID, InspectorID, ScheduledDate, InspectionStatus, Remarks, CreatedAt)
                               VALUES (@InquiryID, @InspectorID, @ScheduledDate, 'Pending', @Remarks, GETDATE())";

                SqlCommand cmd = new SqlCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("@InquiryID", inquiryId);
                cmd.Parameters.AddWithValue("@InspectorID", inspectorUserId);
                cmd.Parameters.AddWithValue("@ScheduledDate", scheduleDate);
                cmd.Parameters.AddWithValue("@Remarks", remarks);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            LoadInquiries();
        }

        private bool InquiryExists(int inquiryId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM InquirySimple WHERE InquiryID = @InquiryID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@InquiryID", inquiryId);
                conn.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
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


        protected void btnDeleteHidden_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(hfDeleteInquiryID.Value, out int inquiryId))
                return;

            try
            {
                DeleteInquiry(inquiryId);
                LoadInquiries();

                // toast
                ScriptManager.RegisterStartupScript(this, GetType(), "DeletedOK",
                    "Swal.fire('Deleted', 'Inquiry has been removed.', 'success');", true);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "DeletedErr",
                    $"Swal.fire('Error', 'Failed to delete inquiry: {ex.Message.Replace("'", "\\'")}', 'error');", true);
            }
        }

        private void DeleteInquiry(int inquiryId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(
                "DELETE FROM InquirySimple WHERE InquiryID = @InquiryID", conn))
            {
                cmd.Parameters.AddWithValue("@InquiryID", inquiryId);
                conn.Open();
                int rows = cmd.ExecuteNonQuery();
                if (rows == 0)
                {
                    throw new InvalidOperationException("Inquiry not found.");
                }
            }
        }


        protected void gvInquiries_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                Button btnAssign = (Button)e.Row.FindControl("btnAssign");
                string inquiryId = DataBinder.Eval(e.Row.DataItem, "InquiryID").ToString();
                btnAssign.OnClientClick = $"showAssignModal({inquiryId}); return false;";
            }
        }
    }
}
