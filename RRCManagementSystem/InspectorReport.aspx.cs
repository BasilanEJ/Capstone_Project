using RRCManagementSystem.Helpers;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class InspectionReport : System.Web.UI.Page
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;
        private const int MaxFileSize = 5 * 1024 * 1024; // 5MB
        private const int MaxFiles = 10;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Check authentication
            if (Session["UserID"] == null || Session["Role"]?.ToString() != "Inspector")
            {
                Response.Redirect("~/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                int inquiryId = GetInquiryIdFromQuery();
                if (inquiryId == 0)
                {
                    Response.Redirect("MyInspections.aspx", false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }

                LoadInquiryDetails(inquiryId);
                LoadCategorizedServices(); // ✅ Replaced old function
                AutoLoadTravelExpense(inquiryId);
            }

        }

        #region Load Data

        /// <summary>
        /// Get inquiry ID from query string
        /// </summary>
        private int GetInquiryIdFromQuery()
        {
            if (int.TryParse(Request.QueryString["id"], out int inquiryId))
            {
                return inquiryId;
            }
            return 0;
        }

        /// <summary>
        /// Load inquiry details and client information
        /// </summary>
        private void LoadInquiryDetails(int inquiryId)
        {
            try
            {
                int inspectorId = Convert.ToInt32(Session["UserID"]);

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(@"
                    SELECT 
                        i.InquiryNumber,
                        i.PestType,
                        i.AddressEnc,
                        i.BarangayEnc,
                        i.CityEnc,
                        i.RegionEnc,
                        i.LandmarkEnc,
                        c.FirstName,
                        c.MiddleName,
                        c.LastName,
                        c.ContactEnc
                    FROM dbo.Inquiries i
                    INNER JOIN dbo.Clients c ON i.ClientID = c.ClientID
                    WHERE i.InquiryID = @InquiryID 
                        AND i.AssignedInspectorID = @InspectorID
                        AND i.IsDeleted = 0
                ", conn))
                {
                    cmd.Parameters.AddWithValue("@InquiryID", inquiryId);
                    cmd.Parameters.AddWithValue("@InspectorID", inspectorId);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lblInquiryNumber.Text = reader["InquiryNumber"]?.ToString();
                            lblPestType.Text = reader["PestType"]?.ToString();

                            // Build client name
                            string firstName = reader["FirstName"]?.ToString() ?? "";
                            string middleName = reader["MiddleName"]?.ToString() ?? "";
                            string lastName = reader["LastName"]?.ToString() ?? "";
                            lblClientName.Text = $"{firstName} {middleName} {lastName}".Trim();

                            // Decrypt and display contact
                            lblClientContact.Text = DecryptField(reader["ContactEnc"]);

                            // Build address
                            string street = DecryptField(reader["AddressEnc"]);
                            string barangay = DecryptField(reader["BarangayEnc"]);
                            string city = DecryptField(reader["CityEnc"]);
                            string region = DecryptField(reader["RegionEnc"]);
                            string landmark = DecryptField(reader["LandmarkEnc"]);

                            var addressParts = new System.Collections.Generic.List<string>();
                            if (!string.IsNullOrEmpty(street)) addressParts.Add(street);
                            if (!string.IsNullOrEmpty(barangay)) addressParts.Add(barangay);
                            if (!string.IsNullOrEmpty(city)) addressParts.Add(city);
                            if (!string.IsNullOrEmpty(region)) addressParts.Add(region);

                            lblClientAddress.Text = string.Join(", ", addressParts);

                            if (!string.IsNullOrEmpty(landmark))
                            {
                                lblClientAddress.Text += $" (Near: {landmark})";
                            }
                        }
                        else
                        {
                            ShowError("Inquiry not found or you don't have access to this inspection.");
                            Response.Redirect("MyInspections.aspx", false);
                            Context.ApplicationInstance.CompleteRequest();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadInquiryDetails Error: {ex.Message}");
                ShowError("Error loading inquiry details.");
            }
        }

        /// <summary>
        /// ✅ NEW: Load services with tiered pricing structure
        /// </summary>
        private void LoadCategorizedServices()
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(@"
            SELECT 
                s.ServiceType,
                s.ServiceID, 
                s.Name, 
                s.Description,
                (
                    SELECT 
                        TierID,
                        MinSQM,
                        MaxSQM,
                        FlatPrice
                    FROM dbo.ServicePricingTiers
                    WHERE ServiceID = s.ServiceID
                    ORDER BY MinSQM
                    FOR JSON PATH
                ) AS PricingTiers
            FROM dbo.Services s
            WHERE s.Status = 'Active'
            ORDER BY s.ServiceType, s.Name
        ", conn))
                {
                    var dt = new DataTable();
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }

                    // ✅ Group by ServiceType
                    var grouped = dt.AsEnumerable()
                        .GroupBy(r => r.Field<string>("ServiceType"))
                        .Select(g => new
                        {
                            ServiceType = g.Key,
                            Services = g.CopyToDataTable()
                        })
                        .ToList();

                    rptServiceTypes.DataSource = grouped;
                    rptServiceTypes.DataBind();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadCategorizedServices Error: {ex.Message}");
                ShowError("Error loading categorized services.");
            }
        }


        /// <summary>
        /// ✅ NEW: Auto-load travel expense based on client's location
        /// </summary>
        private void AutoLoadTravelExpense(int inquiryId)
        {
            try
            {
                // Get client's location from inquiry
                string region, city;
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(@"
                    SELECT RegionEnc, CityEnc 
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
                            region = DecryptField(reader["RegionEnc"]);
                            city = DecryptField(reader["CityEnc"]);
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                // Find matching travel expense
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(@"
                    SELECT TravelExpenseID, TravelPrice
                    FROM dbo.TravelExpenses
                    WHERE Region = @Region AND City = @City AND IsActive = 1
                ", conn))
                {
                    cmd.Parameters.AddWithValue("@Region", region);
                    cmd.Parameters.AddWithValue("@City", city);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int travelExpenseId = Convert.ToInt32(reader["TravelExpenseID"]);
                            decimal travelPrice = Convert.ToDecimal(reader["TravelPrice"]);

                            // ✅ Auto-populate travel expense (Read-only display)
                            lblTravelAmount.Text = $"₱{travelPrice:N2} (Auto-detected: {city}, {region})";
                            hfTravelCost.Value = travelPrice.ToString("F2");
                            ViewState["TravelExpenseID"] = travelExpenseId;

                            // ✅ Trigger client-side calculation
                            ScriptManager.RegisterStartupScript(this, GetType(), "InitTravel",
                                "calculateGrandTotal();", true);
                        }
                        else
                        {
                            // No travel expense found for this location
                            lblTravelAmount.Text = $"⚠️ No travel rate set for {city}, {region}";
                            hfTravelCost.Value = "0";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AutoLoadTravelExpense Error: {ex.Message}");
            }
        }

        #endregion

        #region Save Draft

        /// <summary>
        /// Save report as draft
        /// </summary>
        protected void btnSaveDraft_Click(object sender, EventArgs e)
        {
            try
            {
                int inquiryId = GetInquiryIdFromQuery();
                int inspectorId = Convert.ToInt32(Session["UserID"]);

                // ✅ Generate quotation code
                string quotationCode = GenerateQuotationCode();

                if (!SaveReport(inquiryId, inspectorId, quotationCode, "Draft"))
                {
                    ShowError("Failed to save draft.");
                    return;
                }

                ShowSuccess($"Draft saved successfully!<br><strong>Quotation Code: {quotationCode}</strong>");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Save Draft Error: {ex.Message}");
                ShowError("Error saving draft.");
            }
        }

        #endregion

        #region Submit Report

        /// <summary>
        /// Submit final report
        /// </summary>
        protected void btnSubmitReport_Click(object sender, EventArgs e)
        {
            try
            {
                int inquiryId = GetInquiryIdFromQuery();
                int inspectorId = Convert.ToInt32(Session["UserID"]);

                // ✅ Generate Quotation Code
                string quotationCode = GenerateQuotationCode();

                if (!SaveReport(inquiryId, inspectorId, quotationCode, "Inspected"))
                {
                    ShowError("Failed to submit report.");
                    return;
                }

                // Update inquiry status to Inspected
                UpdateInquiryStatus(inquiryId, "Inspected");

                ShowSuccessAndRedirect($"Report submitted successfully!<br><strong>Quotation Code: {quotationCode}</strong>");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Submit Report Error: {ex.Message}");
                ShowError("Error submitting report.");
            }
        }

        #endregion

        #region Save Report Logic

        /// <summary>
        /// ✅ NEW: Generate unique quotation code
        /// </summary>
        private string GenerateQuotationCode()
        {
            try
            {
                int year = DateTime.Now.Year;
                int nextNumber;

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(@"
                    SELECT ISNULL(MAX(CAST(RIGHT(QuotationCode, 4) AS INT)), 0) + 1
                    FROM dbo.InspectionReports
                    WHERE QuotationCode LIKE 'QUOT-' + @Year + '-%'
                ", conn))
                {
                    cmd.Parameters.AddWithValue("@Year", year.ToString());
                    conn.Open();
                    nextNumber = Convert.ToInt32(cmd.ExecuteScalar());
                }

                return $"QUOT-{year}-{nextNumber:D4}";
            }
            catch
            {
                return $"QUOT-{DateTime.Now:yyyyMMddHHmmss}"; // Fallback
            }
        }

        /// <summary>
        /// Save inspection report to database
        /// </summary>
        private bool SaveReport(int inquiryId, int inspectorId, string quotationCode, string status)
        {
            try
            {
                // Get form values
                string infestationLevel = ddlInfestationLevel.SelectedValue;
                string findingsDescription = txtFindings.Text.Trim();
                string affectedAreas = GetSelectedAffectedAreas();
                string selectedServices = hfSelectedServices.Value;

                // ✅ Get travel expense from ViewState (auto-detected)
                int travelExpenseId = ViewState["TravelExpenseID"] != null
                    ? Convert.ToInt32(ViewState["TravelExpenseID"])
                    : 0;
                decimal travelCost = Convert.ToDecimal(hfTravelCost.Value);

                string miscExpenses = hfMiscExpenses.Value;
                decimal miscTotal = CalculateMiscTotal(miscExpenses);
                decimal grandTotal = Convert.ToDecimal(hfGrandTotal.Value);
                string photoPaths = ProcessPhotoUploads(inquiryId, inspectorId);
                string additionalNotes = txtAdditionalNotes.Text.Trim();
                bool followupRequired = rbFollowupYes.Checked;
                DateTime? followupDate = null;
                string followupReason = null;

                if (followupRequired)
                {
                    if (DateTime.TryParse(txtFollowupDate.Text, out DateTime parsedDate))
                    {
                        followupDate = parsedDate;
                    }
                    followupReason = txtFollowupReason.Text.Trim();
                }

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spInspectionReport_Save", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Input parameters
                    cmd.Parameters.AddWithValue("@InquiryID", inquiryId);
                    cmd.Parameters.AddWithValue("@InspectorID", inspectorId);
                    cmd.Parameters.AddWithValue("@QuotationCode", quotationCode); // ✅ NEW
                    cmd.Parameters.AddWithValue("@InfestationLevel", infestationLevel);
                    cmd.Parameters.AddWithValue("@FindingsDescription", findingsDescription);
                    cmd.Parameters.AddWithValue("@AffectedAreas", affectedAreas);
                    cmd.Parameters.AddWithValue("@SelectedServices", selectedServices);
                    cmd.Parameters.AddWithValue("@TravelExpenseID", travelExpenseId > 0 ? (object)travelExpenseId : DBNull.Value);
                    cmd.Parameters.AddWithValue("@TravelCost", travelCost);
                    cmd.Parameters.AddWithValue("@MiscellaneousExpenses", string.IsNullOrEmpty(miscExpenses) ? "[]" : miscExpenses);
                    cmd.Parameters.AddWithValue("@MiscellaneousTotal", miscTotal);
                    cmd.Parameters.AddWithValue("@TotalEstimatedCost", grandTotal);
                    cmd.Parameters.AddWithValue("@InspectionPhotosPath", (object)photoPaths ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AdditionalNotes", (object)additionalNotes ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FollowupRequired", followupRequired);
                    cmd.Parameters.AddWithValue("@FollowupDate", (object)followupDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FollowupReason", (object)followupReason ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", status);

                    // Output parameter
                    var pReportId = new SqlParameter("@ReportID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(pReportId);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    int reportId = pReportId.Value != DBNull.Value ? Convert.ToInt32(pReportId.Value) : 0;
                    return reportId > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SaveReport Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Update inquiry status
        /// </summary>
        private void UpdateInquiryStatus(int inquiryId, string status)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(@"
                    UPDATE dbo.Inquiries
                    SET Status = @Status,
                        UpdatedAt = GETDATE()
                    WHERE InquiryID = @InquiryID
                ", conn))
                {
                    cmd.Parameters.AddWithValue("@InquiryID", inquiryId);
                    cmd.Parameters.AddWithValue("@Status", status);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateInquiryStatus Error: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get selected affected areas as JSON array
        /// </summary>
        private string GetSelectedAffectedAreas()
        {
            var areas = new System.Collections.Generic.List<string>();

            if (chkLivingRoom.Checked) areas.Add("Living Room");
            if (chkKitchen.Checked) areas.Add("Kitchen");
            if (chkBedroom.Checked) areas.Add("Bedrooms");
            if (chkBathroom.Checked) areas.Add("Bathroom");
            if (chkGarage.Checked) areas.Add("Garage");
            if (chkAttic.Checked) areas.Add("Attic");
            if (chkBasement.Checked) areas.Add("Basement");
            if (chkGarden.Checked) areas.Add("Garden/Outdoor");
            if (chkWalls.Checked) areas.Add("Walls/Foundation");
            if (chkRoof.Checked) areas.Add("Roof");

            var serializer = new JavaScriptSerializer();
            return serializer.Serialize(areas);
        }

        /// <summary>
        /// Calculate miscellaneous total from JSON
        /// </summary>
        private decimal CalculateMiscTotal(string miscExpensesJson)
        {
            if (string.IsNullOrEmpty(miscExpensesJson) || miscExpensesJson == "[]")
                return 0;

            try
            {
                var serializer = new JavaScriptSerializer();
                var expenses = serializer.Deserialize<System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>>>(miscExpensesJson);

                decimal total = 0;
                foreach (var expense in expenses)
                {
                    if (expense.ContainsKey("amount"))
                    {
                        total += Convert.ToDecimal(expense["amount"]);
                    }
                }

                return total;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Process and save uploaded photos
        /// </summary>
        private string ProcessPhotoUploads(int inquiryId, int inspectorId)
        {
            if (!fuPhotos.HasFiles)
                return null;

            try
            {
                // Create directory: ~/Upload/InspectionReports/{InquiryID}/
                string baseDir = Server.MapPath("~/Upload/InspectionReports/");
                string inquiryDir = Path.Combine(baseDir, inquiryId.ToString());

                if (!Directory.Exists(inquiryDir))
                {
                    Directory.CreateDirectory(inquiryDir);
                }

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var imagePaths = new System.Collections.Generic.List<string>();

                int fileIndex = 1;
                foreach (HttpPostedFile file in fuPhotos.PostedFiles)
                {
                    if (file.ContentLength > 0 && file.ContentLength <= MaxFileSize)
                    {
                        string extension = Path.GetExtension(file.FileName);
                        string fileName = $"REPORT_{timestamp}_{fileIndex}{extension}";
                        string filePath = Path.Combine(inquiryDir, fileName);

                        file.SaveAs(filePath);

                        string relativePath = $"~/Upload/InspectionReports/{inquiryId}/{fileName}";
                        imagePaths.Add(relativePath);

                        fileIndex++;

                        if (fileIndex > MaxFiles)
                            break;
                    }
                }

                return string.Join(",", imagePaths);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ProcessPhotoUploads Error: {ex.Message}");
                return null;
            }
        }

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
        /// Render bundle pricing tiers for display
        /// </summary>
        public string RenderBundlePricing(object pricingTiersJson)
        {
            if (pricingTiersJson == null || pricingTiersJson == DBNull.Value)
                return "No pricing information available";

            try
            {
                var serializer = new JavaScriptSerializer();
                var tiers = serializer.Deserialize<System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>>>(pricingTiersJson.ToString());

                var sb = new System.Text.StringBuilder();
                sb.Append("<div style='display: flex; flex-direction: column; gap: 4px;'>");

                foreach (var tier in tiers)
                {
                    int minSqm = Convert.ToInt32(tier["MinSQM"]);
                    object maxSqmObj = tier["MaxSQM"];
                    decimal flatPrice = Convert.ToDecimal(tier["FlatPrice"]);

                    string range;
                    if (maxSqmObj == null || maxSqmObj == DBNull.Value)
                    {
                        range = $"{minSqm}+ sqm";
                    }
                    else
                    {
                        int maxSqm = Convert.ToInt32(maxSqmObj);
                        range = $"{minSqm}-{maxSqm} sqm";
                    }

                    sb.Append($"<span>📦 {range}: <strong style='color: #059669;'>₱{flatPrice:N2}</strong></span>");
                }

                sb.Append("</div>");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RenderBundlePricing Error: {ex.Message}");
                return "Error loading pricing information";
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
                    html: '{message.Replace("'", "\\'")}',
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
                    html: '{message.Replace("'", "\\'")}',
                    confirmButtonColor: '#10b981'
                }});
            ";
            ScriptManager.RegisterStartupScript(this, GetType(), "ShowSuccess", script, true);
        }

        /// <summary>
        /// Show success and redirect
        /// </summary>
        private void ShowSuccessAndRedirect(string message)
        {
            string script = $@"
                Swal.fire({{
                    icon: 'success',
                    title: 'Success!',
                    html: '{message.Replace("'", "\\'")}',
                    confirmButtonColor: '#10b981',
                    allowOutsideClick: false
                }}).then((result) => {{
                    if (result.isConfirmed) {{
                        window.location.href = 'MyInspections.aspx';
                    }}
                }});
            ";
            ScriptManager.RegisterStartupScript(this, GetType(), "ShowSuccess", script, true);
        }

        #endregion
    }
}