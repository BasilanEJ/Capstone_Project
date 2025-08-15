using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class AllInquiry : System.Web.UI.Page
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
                BindInspectors();   // populate hidden ddl (ddlInspectorSource)
                LoadInquiries();
            }
        }

        private void BindInspectors()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT UserID, Name FROM Users WHERE Role = 'Inspector' AND Status = 'Active' ORDER BY Name", conn))
            {
                conn.Open();
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    ddlInspectorSource.Items.Clear();
                    ddlInspectorSource.Items.Add(new ListItem("-- Select Inspector --", ""));
                    while (rd.Read())
                    {
                        // Explicitly uses WebControls.ListItem (avoid iTextSharp ambiguity)
                        ddlInspectorSource.Items.Add(new ListItem(
                            rd["Name"].ToString(),
                            rd["UserID"].ToString()
                        ));
                    }
                }
            }
        }

        protected void btnAssignHidden_Click(object sender, EventArgs e)
        {
            string[] parts = hfAssignData.Value.Split('|');
            if (parts.Length >= 12)
            {
                int inspectorId = int.Parse(parts[0]);
                // schedule is local from browser (PHT)
                DateTime scheduleLocal = DateTime.Parse(parts[1]);
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
                if (GetInspectionsCount(inspectorId, scheduleLocal.Date) >= maxPerDay)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "LimitReached",
                        "Swal.fire('Max Limit Reached', 'Inspector already has the maximum inspections on this day.', 'error');", true);
                    return;
                }

                UpdateInquiryInfo(inquiryId, firstName, middleName, lastName, street, barangay, city, region, country, landmark);
                AssignInspector(inquiryId, inspectorId, scheduleLocal, remarks);

                // refresh grid and hidden ddl (in case of changes)
                BindInspectors();
                LoadInquiries();

                ScriptManager.RegisterStartupScript(this, GetType(), "Success",
                    "Swal.fire('Success', 'Inspector assigned successfully.', 'success');", true);
            }
        }

        private void LoadInquiries()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlDataAdapter da = new SqlDataAdapter(@"
                SELECT
                    InquiryID,
                    Email,
                    ContactNumber,
                    Message,
                    SubmittedAt,
                    PhotoPath,
                    -- extra fields for autofill
                    FirstName,
                    MiddleName,
                    LastName,
                    StreetAndUnit,
                    Barangay,
                    City,
                    Region,
                    Country,
                    Landmark
                FROM InquirySimple
                WHERE NOT EXISTS (
                    SELECT 1 FROM Inspections WHERE Inspections.InquiryID = InquirySimple.InquiryID
                )
                ORDER BY SubmittedAt DESC;", conn))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvInquiries.DataSource = dt;
                gvInquiries.DataBind();
            }
        }

        private int GetSystemSettingInt(string settingName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT SettingValue FROM SystemSettings WHERE SettingName = @SettingName", conn))
            {
                cmd.Parameters.AddWithValue("@SettingName", settingName);
                conn.Open();
                object result = cmd.ExecuteScalar();
                return result != null && int.TryParse(result.ToString(), out int value) ? value : 6;
            }
        }

        private int GetInspectionsCount(int inspectorId, DateTime date)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT COUNT(*) FROM Inspections WHERE InspectorID = @InspectorID AND CAST(ScheduledDate AS DATE) = @Date", conn))
            {
                cmd.Parameters.AddWithValue("@InspectorID", inspectorId);
                cmd.Parameters.AddWithValue("@Date", date);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private void UpdateInquiryInfo(int inquiryId, string firstName, string middleName, string lastName,
                                      string street, string barangay, string city, string region, string country, string landmark)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(@"
                UPDATE InquirySimple
                   SET FirstName    = @FirstName,
                       MiddleName   = @MiddleName,
                       LastName     = @LastName,
                       StreetAndUnit= @Street,
                       Barangay     = @Barangay,
                       City         = @City,
                       Region       = @Region,
                       Country      = @Country,
                       Landmark     = @Landmark
                 WHERE InquiryID    = @InquiryID;", conn))
            {
                cmd.Parameters.AddWithValue("@FirstName", firstName);
                cmd.Parameters.AddWithValue("@MiddleName", string.IsNullOrWhiteSpace(middleName) ? (object)DBNull.Value : middleName);
                cmd.Parameters.AddWithValue("@LastName", lastName);
                cmd.Parameters.AddWithValue("@Street", string.IsNullOrWhiteSpace(street) ? (object)DBNull.Value : street);
                cmd.Parameters.AddWithValue("@Barangay", string.IsNullOrWhiteSpace(barangay) ? (object)DBNull.Value : barangay);
                cmd.Parameters.AddWithValue("@City", string.IsNullOrWhiteSpace(city) ? (object)DBNull.Value : city);
                cmd.Parameters.AddWithValue("@Region", string.IsNullOrWhiteSpace(region) ? (object)DBNull.Value : region);
                cmd.Parameters.AddWithValue("@Country", string.IsNullOrWhiteSpace(country) ? (object)DBNull.Value : country);
                cmd.Parameters.AddWithValue("@Landmark", string.IsNullOrWhiteSpace(landmark) ? (object)DBNull.Value : landmark);
                cmd.Parameters.AddWithValue("@InquiryID", inquiryId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void AssignInspector(int inquiryId, int inspectorUserId, DateTime scheduleLocalPHT, string remarks)
        {
            if (!InquiryExists(inquiryId)) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO Inspections
                    (InquiryID, InspectorID, ScheduledDate, InspectionStatus, Remarks, CreatedAt)
                VALUES
                    (@InquiryID, @InspectorID, @ScheduledDate, 'Pending', @Remarks, DATEADD(HOUR, 8, GETUTCDATE()));", conn))
            {
                cmd.Parameters.AddWithValue("@InquiryID", inquiryId);
                cmd.Parameters.AddWithValue("@InspectorID", inspectorUserId);
                cmd.Parameters.AddWithValue("@ScheduledDate", scheduleLocalPHT); // store as datetime (assumed local/PHT)
                cmd.Parameters.AddWithValue("@Remarks", string.IsNullOrWhiteSpace(remarks) ? (object)DBNull.Value : remarks);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private bool InquiryExists(int inquiryId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM InquirySimple WHERE InquiryID = @InquiryID", conn))
            {
                cmd.Parameters.AddWithValue("@InquiryID", inquiryId);
                conn.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        protected void btnDeleteHidden_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(hfDeleteInquiryID.Value, out int inquiryId))
                return;

            try
            {
                DeleteInquiry(inquiryId);
                LoadInquiries();

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
            using (SqlCommand cmd = new SqlCommand("DELETE FROM InquirySimple WHERE InquiryID = @InquiryID", conn))
            {
                cmd.Parameters.AddWithValue("@InquiryID", inquiryId);
                conn.Open();
                int rows = cmd.ExecuteNonQuery();
                if (rows == 0)
                    throw new InvalidOperationException("Inquiry not found.");
            }
        }

        protected void gvInquiries_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var drv = (DataRowView)e.Row.DataItem;

                Button btnAssign = (Button)e.Row.FindControl("btnAssign");
                string inquiryId = drv["InquiryID"].ToString();

                // Put inquiry details as data-* attributes for autofill in the modal
                btnAssign.Attributes["data-fn"] = SafeAttr(drv["FirstName"]);
                btnAssign.Attributes["data-mn"] = SafeAttr(drv["MiddleName"]);
                btnAssign.Attributes["data-ln"] = SafeAttr(drv["LastName"]);
                btnAssign.Attributes["data-str"] = SafeAttr(drv["StreetAndUnit"]);
                btnAssign.Attributes["data-brgy"] = SafeAttr(drv["Barangay"]);
                btnAssign.Attributes["data-city"] = SafeAttr(drv["City"]);
                btnAssign.Attributes["data-reg"] = SafeAttr(drv["Region"]);
                btnAssign.Attributes["data-ctry"] = SafeAttr(drv["Country"]);
                btnAssign.Attributes["data-lmk"] = SafeAttr(drv["Landmark"]);

                // Pass the button element so JS can read el.dataset.*
                btnAssign.OnClientClick = $"showAssignModal(this, {inquiryId}); return false;";
            }
        }

        private static string SafeAttr(object val)
        {
            return val == null || val == DBNull.Value ? "" : val.ToString();
        }
    }
}
