using RRCManagementSystem.Helpers;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class AllInquiry : System.Web.UI.Page
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Check if admin is logged in
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                lblPermission.Text = "⚠️ Access Denied. Admin privileges required.";
                lblPermission.Visible = true;
                gvInquiries.Visible = false;
                return;
            }

            if (!IsPostBack)
            {
                LoadInquiries();
            }
        }

        #region Load Inquiries

        /// <summary>
        /// Load all pending and validated inquiries (not archived)
        /// </summary>
        private void LoadInquiries()
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(@"
                    SELECT 
                        i.InquiryID,
                        i.InquiryNumber,
                        i.ClientID,
                        i.InspectionDate,
                        i.InspectionTime,
                        i.PestType,
                        i.ProblemDescription,
                        i.Urgency,
                        i.Status,
                        i.CreatedAt,
                        i.InspectionReportPath,
                        -- Encrypted Address Fields
                        i.AddressEnc,
                        i.BarangayEnc,
                        i.CityEnc,
                        i.RegionEnc,
                        i.LandmarkEnc,
                        -- Client Info from Clients table
                        c.FirstName,
                        c.LastName,
                        c.MiddleName,
                        c.EmailEnc,
                        c.ContactEnc
                    FROM dbo.Inquiries i
                    INNER JOIN dbo.Clients c ON i.ClientID = c.ClientID
                    WHERE i.IsDeleted = 0
                        AND i.Status IN ('Pending', 'Validated', 'Assigned')
                    ORDER BY 
                        CASE i.Urgency
                            WHEN 'Emergency' THEN 1
                            WHEN 'High' THEN 2
                            WHEN 'Medium' THEN 3
                            WHEN 'Low' THEN 4
                            ELSE 5
                        END,
                        i.CreatedAt ASC
                ", conn))
                {
                    var dt = new DataTable();
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }

                    // Decrypt sensitive data
                    foreach (DataRow row in dt.Rows)
                    {
                        // Decrypt Email
                        if (row["EmailEnc"] != DBNull.Value)
                        {
                            row["EmailEnc"] = DecryptField(row["EmailEnc"]);
                        }

                        // Decrypt Contact
                        if (row["ContactEnc"] != DBNull.Value)
                        {
                            row["ContactEnc"] = DecryptField(row["ContactEnc"]);
                        }

                        // Decrypt Address Fields
                        if (row["AddressEnc"] != DBNull.Value)
                        {
                            row["AddressEnc"] = DecryptField(row["AddressEnc"]);
                        }

                        if (row["BarangayEnc"] != DBNull.Value)
                        {
                            row["BarangayEnc"] = DecryptField(row["BarangayEnc"]);
                        }

                        if (row["CityEnc"] != DBNull.Value)
                        {
                            row["CityEnc"] = DecryptField(row["CityEnc"]);
                        }

                        if (row["RegionEnc"] != DBNull.Value)
                        {
                            row["RegionEnc"] = DecryptField(row["RegionEnc"]);
                        }

                        if (row["LandmarkEnc"] != DBNull.Value)
                        {
                            row["LandmarkEnc"] = DecryptField(row["LandmarkEnc"]);
                        }
                    }

                    // Add computed columns for display
                    dt.Columns.Add("ClientName", typeof(string));
                    dt.Columns.Add("ClientEmail", typeof(string));
                    dt.Columns.Add("ClientContact", typeof(string));
                    dt.Columns.Add("Street", typeof(string));
                    dt.Columns.Add("Barangay", typeof(string));
                    dt.Columns.Add("City", typeof(string));
                    dt.Columns.Add("Region", typeof(string));
                    dt.Columns.Add("Landmark", typeof(string));

                    foreach (DataRow row in dt.Rows)
                    {
                        // Build full name
                        string firstName = row["FirstName"]?.ToString() ?? "";
                        string middleName = row["MiddleName"]?.ToString() ?? "";
                        string lastName = row["LastName"]?.ToString() ?? "";
                        row["ClientName"] = $"{firstName} {middleName} {lastName}".Trim();

                        // Set decrypted values
                        row["ClientEmail"] = row["EmailEnc"];
                        row["ClientContact"] = row["ContactEnc"];
                        row["Street"] = row["AddressEnc"];
                        row["Barangay"] = row["BarangayEnc"];
                        row["City"] = row["CityEnc"];
                        row["Region"] = row["RegionEnc"];
                        row["Landmark"] = row["LandmarkEnc"];
                    }

                    gvInquiries.DataSource = dt;
                    gvInquiries.DataBind();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadInquiries Error: {ex.Message}");
                lblPermission.Text = $"⚠️ Error loading inquiries: {ex.Message}";
                lblPermission.Visible = true;
            }
        }

        #endregion

        #region GridView Events

        /// <summary>
        /// Handle GridView row commands
        /// </summary>
        protected void gvInquiries_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                int inquiryId = Convert.ToInt32(e.CommandArgument);

                if (e.CommandName == "Assign")
                {
                    AutoAssignInspector(inquiryId);
                }
                else if (e.CommandName == "Archive")
                {
                    ArchiveInquiry(inquiryId);
                }
                else if (e.CommandName == "ViewDetails")
                {
                    Response.Redirect($"InquiryDetails.aspx?id={inquiryId}", false);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RowCommand Error: {ex.Message}");
                ShowError("Error processing request.");
            }
        }

        /// <summary>
        /// Handle GridView row data bound
        /// </summary>
        protected void gvInquiries_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var dataItem = (DataRowView)e.Row.DataItem;

                // Handle image display
                var litImages = (Literal)e.Row.FindControl("litImages");
                if (litImages != null)
                {
                    string imagePaths = dataItem["InspectionReportPath"]?.ToString();
                    litImages.Text = RenderImages(imagePaths);
                }
            }
        }

        #endregion

        #region Auto-Assign Inspector (Round-Robin with Area Filtering)

        /// <summary>
        /// Automatically assign inspector using round-robin algorithm WITH AREA FILTERING
        /// </summary>
        private void AutoAssignInspector(int inquiryId)
        {
            try
            {
                // Get inquiry details (date/time and LOCATION)
                DateTime inspectionDate;
                string inspectionTime;
                string region, city;

                if (!GetInquiryDetailsWithLocation(inquiryId, out inspectionDate, out inspectionTime, out region, out city))
                {
                    ShowError("Unable to retrieve inquiry details.");
                    return;
                }

                // Validate location
                if (string.IsNullOrEmpty(region) || string.IsNullOrEmpty(city))
                {
                    ShowError("Inquiry location is incomplete. Cannot assign inspector.");
                    return;
                }

                // Get next available inspector using round-robin WITH AREA FILTERING
                int inspectorId = GetNextInspectorRoundRobin(inspectionDate, region, city);

                if (inspectorId == 0)
                {
                    ShowError($"No available inspectors found for {city}, {region} on {inspectionDate:MMM dd, yyyy}. " +
                             "Please check inspector area assignments or try another date.");
                    return;
                }

                // Assign inspector
                if (AssignInspectorToInquiry(inquiryId, inspectorId, inspectionDate, inspectionTime))
                {
                    // Update inspector schedule
                    UpdateInspectorSchedule(inspectorId, inspectionDate);

                    // Get inspector name for confirmation
                    string inspectorName = GetInspectorName(inspectorId);

                    ShowSuccess($"Inspector {inspectorName} automatically assigned! " +
                               $"Inspection scheduled for {inspectionDate:MMM dd, yyyy} at {inspectionTime} in {city}, {region}");
                    LoadInquiries();
                }
                else
                {
                    ShowError("Failed to assign inspector.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AutoAssignInspector Error: {ex.Message}");
                ShowError("Error assigning inspector.");
            }
        }

        /// <summary>
        /// Get inquiry details INCLUDING LOCATION (region and city)
        /// </summary>
        private bool GetInquiryDetailsWithLocation(int inquiryId, out DateTime inspectionDate,
            out string inspectionTime, out string region, out string city)
        {
            inspectionDate = DateTime.MinValue;
            inspectionTime = string.Empty;
            region = string.Empty;
            city = string.Empty;

            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(@"
            SELECT 
                InspectionDate, 
                InspectionTime,
                RegionEnc,
                CityEnc
            FROM dbo.Inquiries
            WHERE InquiryID = @InquiryID
        ", conn))
                {
                    cmd.Parameters.AddWithValue("@InquiryID", inquiryId);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            inspectionDate = Convert.ToDateTime(reader["InspectionDate"]);
                            inspectionTime = reader["InspectionTime"]?.ToString() ?? "Not specified";

                            // Decrypt region and city
                            region = DecryptField(reader["RegionEnc"]);
                            city = DecryptField(reader["CityEnc"]);

                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetInquiryDetailsWithLocation Error: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Get next inspector using round-robin algorithm WITH AREA FILTERING
        /// </summary>
        private int GetNextInspectorRoundRobin(DateTime inspectionDate, string region, string city)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Get ONLY inspectors who service this Region + City, ordered by UserID
                    var inspectors = new System.Collections.Generic.List<int>();
                    using (var cmd = new SqlCommand(@"
                SELECT DISTINCT u.UserID
                FROM dbo.Users u
                INNER JOIN dbo.InspectorAreaScope ias 
                    ON u.UserID = ias.UserID
                WHERE u.Role = 'Inspector' 
                    AND u.Status IN ('Active', 'Available')
                    AND ias.Region = @Region
                    AND ias.City = @City
                ORDER BY u.UserID ASC
            ", conn))
                    {
                        cmd.Parameters.AddWithValue("@Region", region);
                        cmd.Parameters.AddWithValue("@City", city);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                inspectors.Add(Convert.ToInt32(reader["UserID"]));
                            }
                        }
                    }

                    // Check if any inspectors service this area
                    if (inspectors.Count == 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"No inspectors found for {city}, {region}");
                        return 0;
                    }

                    // Get count of assignments for today to determine round-robin position
                    int assignmentCountToday = 0;
                    using (var cmd = new SqlCommand(@"
                SELECT COUNT(*) 
                FROM dbo.Inquiries
                WHERE CAST(AssignedAt AS DATE) = CAST(GETDATE() AS DATE)
                    AND AssignedInspectorID IS NOT NULL
                    AND IsDeleted = 0
            ", conn))
                    {
                        assignmentCountToday = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // Calculate round-robin index
                    int inspectorIndex = assignmentCountToday % inspectors.Count;
                    int selectedInspectorId = inspectors[inspectorIndex];

                    // Check if selected inspector has capacity for this date
                    bool hasCapacity = CheckInspectorCapacity(selectedInspectorId, inspectionDate, conn);

                    if (!hasCapacity)
                    {
                        // Try next inspectors in the area
                        for (int i = 1; i < inspectors.Count; i++)
                        {
                            int nextIndex = (inspectorIndex + i) % inspectors.Count;
                            int nextInspectorId = inspectors[nextIndex];

                            if (CheckInspectorCapacity(nextInspectorId, inspectionDate, conn))
                            {
                                System.Diagnostics.Debug.WriteLine($"Inspector {selectedInspectorId} full, assigned {nextInspectorId} instead");
                                return nextInspectorId;
                            }
                        }

                        // No inspector in this area has capacity for this date
                        System.Diagnostics.Debug.WriteLine($"All inspectors for {city}, {region} are fully booked on {inspectionDate:yyyy-MM-dd}");
                        return 0;
                    }

                    return selectedInspectorId;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetNextInspectorRoundRobin Error: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Check if inspector has capacity for the given date (still max 6 per day)
        /// </summary>
        private bool CheckInspectorCapacity(int inspectorId, DateTime inspectionDate, SqlConnection conn)
        {
            try
            {
                using (var cmd = new SqlCommand(@"
            SELECT 
                ISNULL(TotalInspections, 0) AS TotalInspections,
                ISNULL(MaxInspections, 6) AS MaxInspections
            FROM dbo.InspectorSchedule
            WHERE InspectorID = @InspectorID 
                AND ScheduleDate = @ScheduleDate
        ", conn))
                {
                    cmd.Parameters.AddWithValue("@InspectorID", inspectorId);
                    cmd.Parameters.AddWithValue("@ScheduleDate", inspectionDate.Date);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int totalInspections = Convert.ToInt32(reader["TotalInspections"]);
                            int maxInspections = Convert.ToInt32(reader["MaxInspections"]);

                            return totalInspections < maxInspections;
                        }
                        else
                        {
                            // No record = has capacity
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CheckInspectorCapacity Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get inspector name for display
        /// </summary>
        /// <summary>
        /// Get inspector name for display
        /// </summary>
        private string GetInspectorName(int inspectorId)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(@"
            SELECT Name 
            FROM dbo.Users 
            WHERE UserID = @UserID
        ", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", inspectorId);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string name = reader["Name"]?.ToString() ?? "";
                            return name.Trim();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetInspectorName Error: {ex.Message}");
            }

            return "Inspector";
        }

        private bool AssignInspectorToInquiry(int inquiryId, int inspectorId, DateTime inspectionDate, string inspectionTime)
        {
            try
            {
                int adminId = Convert.ToInt32(Session["UserID"]);

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(@"
            UPDATE dbo.Inquiries
            SET 
                AssignedInspectorID = @InspectorID,
                AssignedAt = GETDATE(),
                AssignedBy = @AdminID,
                Status = 'Assigned',
                ValidationNotes = 'Auto-assigned via Round-Robin (Area-Filtered)',
                UpdatedAt = GETDATE()
            WHERE InquiryID = @InquiryID
        ", conn))
                {
                    cmd.Parameters.AddWithValue("@InquiryID", inquiryId);
                    cmd.Parameters.AddWithValue("@InspectorID", inspectorId);
                    cmd.Parameters.AddWithValue("@AdminID", adminId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AssignInspectorToInquiry Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Update inspector schedule count
        /// </summary>
        private void UpdateInspectorSchedule(int inspectorId, DateTime scheduleDate)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Check if schedule record exists
                    using (var checkCmd = new SqlCommand(@"
                SELECT ScheduleID, TotalInspections, MaxInspections 
                FROM dbo.InspectorSchedule 
                WHERE InspectorID = @InspectorID AND ScheduleDate = @ScheduleDate
            ", conn))
                    {
                        checkCmd.Parameters.AddWithValue("@InspectorID", inspectorId);
                        checkCmd.Parameters.AddWithValue("@ScheduleDate", scheduleDate.Date);

                        using (var reader = checkCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Update existing record
                                int currentTotal = Convert.ToInt32(reader["TotalInspections"]);
                                int maxInspections = Convert.ToInt32(reader["MaxInspections"]);
                                reader.Close();

                                using (var updateCmd = new SqlCommand(@"
                            UPDATE dbo.InspectorSchedule
                            SET 
                                TotalInspections = TotalInspections + 1,
                                IsAvailable = CASE WHEN TotalInspections + 1 >= MaxInspections THEN 0 ELSE 1 END,
                                UpdatedAt = GETDATE()
                            WHERE InspectorID = @InspectorID AND ScheduleDate = @ScheduleDate
                        ", conn))
                                {
                                    updateCmd.Parameters.AddWithValue("@InspectorID", inspectorId);
                                    updateCmd.Parameters.AddWithValue("@ScheduleDate", scheduleDate.Date);
                                    updateCmd.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                reader.Close();

                                // Insert new record
                                using (var insertCmd = new SqlCommand(@"
                            INSERT INTO dbo.InspectorSchedule (InspectorID, ScheduleDate, TotalInspections, MaxInspections, IsAvailable)
                            VALUES (@InspectorID, @ScheduleDate, 1, 6, 1)
                        ", conn))
                                {
                                    insertCmd.Parameters.AddWithValue("@InspectorID", inspectorId);
                                    insertCmd.Parameters.AddWithValue("@ScheduleDate", scheduleDate.Date);
                                    insertCmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateInspectorSchedule Error: {ex.Message}");
            }
        }

        #endregion


        #region Archive Inquiry

        /// <summary>
        /// Archive inquiry (change status to Archived)
        /// </summary>
        private void ArchiveInquiry(int inquiryId)
        {
            try
            {
                int adminId = Convert.ToInt32(Session["UserID"]);

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(@"
                    UPDATE dbo.Inquiries
                    SET 
                        Status = 'Archived',
                        UpdatedAt = GETDATE()
                    WHERE InquiryID = @InquiryID
                ", conn))
                {
                    cmd.Parameters.AddWithValue("@InquiryID", inquiryId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                ShowSuccess("Inquiry archived successfully!");
                LoadInquiries();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ArchiveInquiry Error: {ex.Message}");
                ShowError("Failed to archive inquiry.");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Decrypt encrypted field
        /// </summary>
        private string DecryptField(object value)
        {
            if (value == null || value == DBNull.Value)
                return string.Empty;

            string encrypted = value.ToString();
            if (string.IsNullOrEmpty(encrypted))
                return string.Empty;

            try
            {
                return AESHelper.DecryptField(encrypted);
            }
            catch
            {
                return "[Decryption Error]";
            }
        }

        /// <summary>
        /// Render images for display
        /// </summary>
        private string RenderImages(string imagePaths)
        {
            if (string.IsNullOrEmpty(imagePaths))
                return "<span class='text-gray-500 italic text-sm'>No photos</span>";

            var images = imagePaths.Split(',');
            var sb = new StringBuilder();

            sb.Append("<div class='images-grid'>");
            foreach (var img in images)
            {
                if (!string.IsNullOrWhiteSpace(img))
                {
                    string imgUrl = ResolveUrl(img.Trim());
                    sb.Append($@"
                        <img src='{imgUrl}' 
                             class='inquiry-photo' 
                             onclick='showImageModal(""{imgUrl}""); return false;' 
                             alt='Inspection Photo' />
                    ");
                }
            }
            sb.Append("</div>");

            return sb.ToString();
        }

        /// <summary>
        /// Get CSS class for status badge
        /// </summary>
        public string GetStatusClass(object status)
        {
            if (status == null || status == DBNull.Value)
                return "pending";

            string statusStr = status.ToString().ToLower().Replace(" ", "-");

            switch (statusStr)
            {
                case "pending":
                    return "pending";
                case "validated":
                    return "validated";
                case "assigned":
                case "scheduled":
                    return "assigned";
                case "in-progress":
                case "inspected":
                    return "inspected";
                case "completed":
                    return "completed";
                default:
                    return "pending";
            }
        }

        #endregion

        #region UI Messages

        /// <summary>
        /// Show error message
        /// </summary>
        private void ShowError(string message)
        {
            string script = $@"
                Swal.fire({{
                    icon: 'error',
                    title: 'Error',
                    text: '{message.Replace("'", "\\'")}',
                    confirmButtonColor: '#ef4444'
                }});
            ";
            ScriptManager.RegisterStartupScript(this, GetType(), "ShowError", script, true);
        }

        /// <summary>
        /// Show success message
        /// </summary>
        private void ShowSuccess(string message)
        {
            string script = $@"
                Swal.fire({{
                    icon: 'success',
                    title: 'Success!',
                    text: '{message.Replace("'", "\\'")}',
                    confirmButtonColor: '#10b981'
                }});
            ";
            ScriptManager.RegisterStartupScript(this, GetType(), "ShowSuccess", script, true);
        }

        #endregion
    }
}