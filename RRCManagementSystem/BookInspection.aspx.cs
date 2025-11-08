using RRCManagementSystem.Helpers;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class BookInspection : System.Web.UI.Page
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;
        private const int MaxFileSize = 5 * 1024 * 1024; // 5MB
        private const int MaxFiles = 5;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Authentication check
            if (Session["ClientID"] == null)
            {
                Response.Redirect("~/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                LoadClientAddress();
            }
        }

        #region Load Client Address

        /// <summary>
        /// Load and decrypt client's registered address from database
        /// </summary>
        private void LoadClientAddress()
        {
            try
            {
                int clientId = Convert.ToInt32(Session["ClientID"]);

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(@"
            SELECT StreetEnc, BarangayEnc, CityEnc, RegionEnc, LandmarkEnc 
            FROM Clients 
            WHERE ClientID = @ClientID", conn))
                {
                    cmd.Parameters.AddWithValue("@ClientID", clientId);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Decrypt address fields
                            string street = DecryptField(reader["StreetEnc"]);
                            string barangay = DecryptField(reader["BarangayEnc"]);
                            string city = DecryptField(reader["CityEnc"]);
                            string region = DecryptField(reader["RegionEnc"]);
                            string landmark = DecryptField(reader["LandmarkEnc"]) ?? "None specified";

                            // Set textbox values
                            txtStreet.Text = street;
                            txtBarangay.Text = barangay;
                            txtCity.Text = city;
                            txtRegion.Text = region;
                            txtLandmark.Text = landmark;
                            hfClientRegion.Value = region;
                            hfClientCity.Value = city;

                            // Check if any inspectors service this area
                            if (!HasInspectorCoverage(region, city))
                            {
                                ShowWarning($"Please note: We currently have limited coverage in {city}, {region}. " +
                                          "Our team will contact you to discuss alternative arrangements.");
                            }
                        }
                        else
                        {
                            ShowError("Unable to load your address. Please update your profile.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadClientAddress Error: {ex.Message}");
                ShowError("Error loading your address. Please try again.");
            }
        }

        /// <summary>
        /// Safely decrypt field, return empty string if null/empty
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

        #endregion

        #region AJAX - Check Date Availability

        [WebMethod]
        public static object CheckAvailability(string date, int timeSlotId, string region, string city)
        {
            try
            {
                if (!DateTime.TryParse(date, out DateTime inspectionDate))
                {
                    return new { IsAvailable = false, Message = "Invalid date format" };
                }

                // Validate region and city
                if (string.IsNullOrEmpty(region) || string.IsNullOrEmpty(city))
                {
                    return new { IsAvailable = false, Message = "Location information is missing" };
                }

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spInspector_CheckTimeSlotAvailability", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@InspectionDate", SqlDbType.Date).Value = inspectionDate;
                    cmd.Parameters.Add("@TimeSlotID", SqlDbType.Int).Value = timeSlotId;
                    cmd.Parameters.Add("@Region", SqlDbType.NVarChar, 100).Value = region;
                    cmd.Parameters.Add("@City", SqlDbType.NVarChar, 100).Value = city;

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int availableSlots = Convert.ToInt32(reader["AvailableSlots"]);
                            int totalInspectors = Convert.ToInt32(reader["TotalInspectors"]);

                            // Check if there are ANY inspectors for this area
                            if (totalInspectors == 0)
                            {
                                return new
                                {
                                    IsAvailable = false,
                                    TotalInspectors = 0,
                                    BookedCount = 0,
                                    AvailableSlots = 0,
                                    Message = $"No inspectors available for {city}, {region}"
                                };
                            }

                            return new
                            {
                                IsAvailable = Convert.ToBoolean(reader["IsAvailable"]),
                                TotalInspectors = totalInspectors,
                                BookedCount = Convert.ToInt32(reader["BookedCount"]),
                                AvailableSlots = availableSlots,
                                Message = availableSlots > 0
                                    ? $"{availableSlots} inspector(s) available for {city}"
                                    : "This time slot is fully booked"
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CheckAvailability Error: {ex.Message}");
            }

            return new { IsAvailable = false, Message = "Error checking availability" };
        }

        /// <summary>
        /// Check all time slots for a date WITH area filtering
        /// </summary>
        [WebMethod]
        public static object CheckDateAvailability(string date, string region, string city)
        {
            try
            {
                if (!DateTime.TryParse(date, out DateTime inspectionDate))
                {
                    return new { Success = false, Message = "Invalid date format" };
                }

                // Validate region and city
                if (string.IsNullOrEmpty(region) || string.IsNullOrEmpty(city))
                {
                    return new { Success = false, Message = "Location information is missing" };
                }

                var timeSlots = new System.Collections.Generic.List<object>();

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spInspector_CheckDateAvailabilityBySlot", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@InspectionDate", SqlDbType.Date).Value = inspectionDate;
                    cmd.Parameters.Add("@Region", SqlDbType.NVarChar, 100).Value = region;
                    cmd.Parameters.Add("@City", SqlDbType.NVarChar, 100).Value = city;

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        int totalInspectors = 0;

                        while (reader.Read())
                        {
                            totalInspectors = Convert.ToInt32(reader["TotalInspectors"]);

                            timeSlots.Add(new
                            {
                                TimeSlotID = Convert.ToInt32(reader["TimeSlotID"]),
                                TimeSlotName = reader["TimeSlotName"].ToString(),
                                TimeSlotDisplay = reader["TimeSlotDisplay"].ToString(),
                                TotalInspectors = totalInspectors,
                                BookedCount = Convert.ToInt32(reader["BookedCount"]),
                                AvailableSlots = Convert.ToInt32(reader["AvailableSlots"]),
                                IsAvailable = Convert.ToBoolean(reader["IsAvailable"])
                            });
                        }

                        // Check if no inspectors service this area
                        if (totalInspectors == 0)
                        {
                            return new
                            {
                                Success = false,
                                Message = $"Sorry, we currently don't have inspectors available in {city}, {region}. Please contact us for assistance.",
                                NoInspectors = true
                            };
                        }
                    }
                }

                return new { Success = true, TimeSlots = timeSlots };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CheckDateAvailability Error: {ex.Message}");
                return new { Success = false, Message = "Error checking availability" };
            }
        }


        private bool HasInspectorCoverage(string region, string city)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("SELECT dbo.fnHasInspectorCoverage(@Region, @City)", conn))
                {
                    cmd.Parameters.AddWithValue("@Region", region);
                    cmd.Parameters.AddWithValue("@City", city);
                    conn.Open();

                    var result = cmd.ExecuteScalar();
                    return result != null && Convert.ToBoolean(result);
                }
            }
            catch
            {
                return true; // Assume coverage if check fails
            }
        }

        /// <summary>
        /// Show warning message using SweetAlert
        /// </summary>
        private void ShowWarning(string message)
        {
            string script = $@"
        Swal.fire({{
            icon: 'warning',
            title: 'Limited Coverage',
            text: '{message.Replace("'", "\\'")}',
            confirmButtonColor: '#f59e0b'
        }});
    ";
            ScriptManager.RegisterStartupScript(this, GetType(), "ShowWarning", script, true);
        }
        #endregion

        #region Submit Inspection Request

        /// <summary>
        /// Handle form submission
        /// </summary>
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Validate inputs
                if (!ValidateInputs())
                    return;

                // 2. Get form data
                int clientId = Convert.ToInt32(Session["ClientID"]);
                DateTime inspectionDate = DateTime.Parse(hfInspectionDate.Value);
                string inspectionTime = GetSelectedTimeSlot();
                string pestType = ddlPestType.SelectedValue;
                string problemDescription = txtProblemDescription.Text.Trim();
                string urgency = GetSelectedUrgency();

                // 3. Get encrypted address from database
                string addressEnc, barangayEnc, cityEnc, regionEnc, landmarkEnc;
                if (!GetClientEncryptedAddress(clientId, out addressEnc, out barangayEnc, out cityEnc, out regionEnc, out landmarkEnc))
                {
                    ShowError("Unable to retrieve your address. Please try again.");
                    return;
                }

                // 4. Process image uploads (if any)
                string imagePaths = ProcessImageUploads(clientId);

                // 5. Create inquiry in database
                int inquiryId;
                string inquiryNumber;
                if (!CreateInquiry(clientId, inspectionDate, inspectionTime, pestType, problemDescription, urgency,
                    addressEnc, barangayEnc, cityEnc, regionEnc, landmarkEnc, imagePaths, out inquiryId, out inquiryNumber))
                {
                    ShowError("Failed to submit your inspection request. Please try again.");
                    return;
                }

                // 6. Send confirmation email (optional)
                SendConfirmationEmail(clientId, inquiryNumber, inspectionDate, inspectionTime);

                // 7. Show success message and redirect
                ShowSuccessAndRedirect(inquiryNumber);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Submit Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ShowError("An unexpected error occurred. Please try again.");
            }
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validate all form inputs
        /// </summary>
        private bool ValidateInputs()
        {
            // Check inspection date
            if (string.IsNullOrEmpty(hfInspectionDate.Value))
            {
                ShowError("Please select an inspection date.");
                return false;
            }

            if (!DateTime.TryParse(hfInspectionDate.Value, out DateTime inspectionDate))
            {
                ShowError("Invalid inspection date format.");
                return false;
            }

            if (inspectionDate < DateTime.Today)
            {
                ShowError("Inspection date cannot be in the past.");
                return false;
            }

            // ✅ FIX: Check all 6 time slots (5 AM - 10 PM)
            if (!rb5AM8AM.Checked && !rb8AM11AM.Checked && !rb11AM2PM.Checked &&
                !rb2PM5PM.Checked && !rb5PM8PM.Checked && !rb8PM10PM.Checked)
            {
                ShowError("Please select a preferred time slot.");
                return false;
            }

            // Check pest type
            if (string.IsNullOrEmpty(ddlPestType.SelectedValue))
            {
                ShowError("Please select the type of pest.");
                return false;
            }

            // Check problem description
            string description = txtProblemDescription.Text.Trim();
            if (string.IsNullOrEmpty(description) || description.Length < 10)
            {
                ShowError("Please provide a detailed problem description (at least 10 characters).");
                return false;
            }

            // Check urgency
            if (!rbLow.Checked && !rbMedium.Checked && !rbHigh.Checked && !rbEmergency.Checked)
            {
                ShowError("Please select the urgency level.");
                return false;
            }

            // Validate file uploads (if any)
            if (fuImages.HasFiles)
            {
                foreach (HttpPostedFile file in fuImages.PostedFiles)
                {
                    // Check file size
                    if (file.ContentLength > MaxFileSize)
                    {
                        ShowError($"File '{file.FileName}' exceeds the maximum size of 5MB.");
                        return false;
                    }

                    // Check file type
                    string extension = Path.GetExtension(file.FileName).ToLower();
                    if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                    {
                        ShowError($"File '{file.FileName}' is not a valid image. Only JPEG and PNG are allowed.");
                        return false;
                    }
                }

                // Check file count
                if (fuImages.PostedFiles.Count > MaxFiles)
                {
                    ShowError($"Maximum {MaxFiles} images allowed.");
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get selected time slot - ✅ FIXED for 6 time slots
        /// </summary>
        private string GetSelectedTimeSlot()
        {
            if (rb5AM8AM.Checked)
                return "5:00 AM - 8:00 AM (Early Morning)";
            if (rb8AM11AM.Checked)
                return "8:00 AM - 11:00 AM (Morning)";
            if (rb11AM2PM.Checked)
                return "11:00 AM - 2:00 PM (Midday)";
            if (rb2PM5PM.Checked)
                return "2:00 PM - 5:00 PM (Afternoon)";
            if (rb5PM8PM.Checked)
                return "5:00 PM - 8:00 PM (Evening)";
            if (rb8PM10PM.Checked)
                return "8:00 PM - 10:00 PM (Night)";

            return "Not Specified";
        }

        /// <summary>
        /// Get selected urgency level
        /// </summary>
        private string GetSelectedUrgency()
        {
            if (rbLow.Checked) return "Low";
            if (rbMedium.Checked) return "Medium";
            if (rbHigh.Checked) return "High";
            if (rbEmergency.Checked) return "Emergency";
            return "Medium"; // Default
        }

        /// <summary>
        /// Get client's encrypted address from database
        /// </summary>
        private bool GetClientEncryptedAddress(int clientId, out string addressEnc, out string barangayEnc,
            out string cityEnc, out string regionEnc, out string landmarkEnc)
        {
            addressEnc = barangayEnc = cityEnc = regionEnc = landmarkEnc = null;

            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("SELECT StreetEnc, BarangayEnc, CityEnc, RegionEnc, LandmarkEnc FROM Clients WHERE ClientID = @ClientID", conn))
                {
                    cmd.Parameters.AddWithValue("@ClientID", clientId);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            addressEnc = reader["StreetEnc"]?.ToString();
                            barangayEnc = reader["BarangayEnc"]?.ToString();
                            cityEnc = reader["CityEnc"]?.ToString();
                            regionEnc = reader["RegionEnc"]?.ToString();
                            landmarkEnc = reader["LandmarkEnc"]?.ToString();
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetClientEncryptedAddress Error: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Process and save uploaded images
        /// </summary>
        private string ProcessImageUploads(int clientId)
        {
            if (!fuImages.HasFiles)
                return null;

            try
            {
                // Create directory structure: ~/Uploads/Inquiries/{ClientID}/
                string baseDir = Server.MapPath("~/Upload/Inquiries/");
                string clientDir = Path.Combine(baseDir, clientId.ToString());

                if (!Directory.Exists(clientDir))
                {
                    Directory.CreateDirectory(clientDir);
                }

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var imagePaths = new System.Collections.Generic.List<string>();

                int fileIndex = 1;
                foreach (HttpPostedFile file in fuImages.PostedFiles)
                {
                    if (file.ContentLength > 0)
                    {
                        // Generate unique filename
                        string extension = Path.GetExtension(file.FileName);
                        string fileName = $"IMG_{timestamp}_{fileIndex}{extension}";
                        string filePath = Path.Combine(clientDir, fileName);

                        // Save file
                        file.SaveAs(filePath);

                        // Store relative path for database
                        string relativePath = $"~/Upload/Inquiries/{clientId}/{fileName}";
                        imagePaths.Add(relativePath);

                        fileIndex++;
                    }
                }

                // Return comma-separated paths
                return string.Join(",", imagePaths);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ProcessImageUploads Error: {ex.Message}");
                return null;
            }
        }


        private bool CreateInquiry(int clientId, DateTime inspectionDate, string inspectionTime,
            string pestType, string problemDescription, string urgency,
            string addressEnc, string barangayEnc, string cityEnc, string regionEnc, string landmarkEnc,
            string imagePaths, out int inquiryId, out string inquiryNumber)
        {
            inquiryId = 0;
            inquiryNumber = string.Empty;

            try
            {
                System.Diagnostics.Debug.WriteLine("=== CreateInquiry Started ===");

                // Get TimeSlotID
                int timeSlotId = GetTimeSlotID();
                System.Diagnostics.Debug.WriteLine($"TimeSlotID: {timeSlotId}");

                // ✅ CRITICAL: Decrypt region and city for availability checking
                string clientRegionPlain = null;
                string clientCityPlain = null;

                try
                {
                    clientRegionPlain = AESHelper.DecryptField(regionEnc);
                    clientCityPlain = AESHelper.DecryptField(cityEnc);

                    System.Diagnostics.Debug.WriteLine($"Decrypted Region: {clientRegionPlain}");
                    System.Diagnostics.Debug.WriteLine($"Decrypted City: {clientCityPlain}");
                }
                catch (Exception decEx)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Decryption failed: {decEx.Message}");
                    ShowError("Unable to verify your location. Please try again.");
                    return false;
                }

                // Validate decrypted values
                if (string.IsNullOrEmpty(clientRegionPlain) || string.IsNullOrEmpty(clientCityPlain))
                {
                    System.Diagnostics.Debug.WriteLine("❌ Decrypted location is empty");
                    ShowError("Your location information is incomplete. Please update your profile.");
                    return false;
                }

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spInquiry_Create", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 30;

                    // Input parameters
                    cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                    cmd.Parameters.Add("@InspectionDate", SqlDbType.Date).Value = inspectionDate;
                    cmd.Parameters.Add("@InspectionTime", SqlDbType.NVarChar, 50).Value = inspectionTime;
                    cmd.Parameters.Add("@TimeSlotID", SqlDbType.Int).Value = timeSlotId;
                    cmd.Parameters.Add("@PestType", SqlDbType.NVarChar, 100).Value = pestType;
                    cmd.Parameters.Add("@ProblemDescription", SqlDbType.NVarChar, -1).Value = problemDescription;
                    cmd.Parameters.Add("@Urgency", SqlDbType.NVarChar, 20).Value = urgency;
                    cmd.Parameters.Add("@AddressEnc", SqlDbType.NVarChar, -1).Value = (object)addressEnc ?? DBNull.Value;
                    cmd.Parameters.Add("@BarangayEnc", SqlDbType.NVarChar, -1).Value = (object)barangayEnc ?? DBNull.Value;
                    cmd.Parameters.Add("@CityEnc", SqlDbType.NVarChar, -1).Value = (object)cityEnc ?? DBNull.Value;
                    cmd.Parameters.Add("@RegionEnc", SqlDbType.NVarChar, -1).Value = (object)regionEnc ?? DBNull.Value;
                    cmd.Parameters.Add("@LandmarkEnc", SqlDbType.NVarChar, -1).Value = (object)landmarkEnc ?? DBNull.Value;
                    cmd.Parameters.Add("@ImagePaths", SqlDbType.NVarChar, -1).Value = (object)imagePaths ?? DBNull.Value;

                    // ✅ NEW: Plain text parameters for availability checking
                    cmd.Parameters.Add("@ClientRegionPlain", SqlDbType.NVarChar, 100).Value = clientRegionPlain;
                    cmd.Parameters.Add("@ClientCityPlain", SqlDbType.NVarChar, 100).Value = clientCityPlain;

                    // Output parameters
                    var pInquiryId = new SqlParameter("@InquiryID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    var pInquiryNumber = new SqlParameter("@InquiryNumber", SqlDbType.NVarChar, 20) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(pInquiryId);
                    cmd.Parameters.Add(pInquiryNumber);

                    System.Diagnostics.Debug.WriteLine("Executing stored procedure...");
                    conn.Open();
                    cmd.ExecuteNonQuery();

                    inquiryId = pInquiryId.Value != DBNull.Value ? Convert.ToInt32(pInquiryId.Value) : 0;
                    inquiryNumber = pInquiryNumber.Value != DBNull.Value ? pInquiryNumber.Value.ToString() : string.Empty;

                    System.Diagnostics.Debug.WriteLine($"✅ Inquiry Created Successfully!");
                    System.Diagnostics.Debug.WriteLine($"InquiryID: {inquiryId}");
                    System.Diagnostics.Debug.WriteLine($"InquiryNumber: {inquiryNumber}");

                    return inquiryId > 0 && !string.IsNullOrEmpty(inquiryNumber);
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("=== SQL EXCEPTION ===");
                System.Diagnostics.Debug.WriteLine($"Error Number: {ex.Number}");
                System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Procedure: {ex.Procedure}");
                System.Diagnostics.Debug.WriteLine($"Line: {ex.LineNumber}");

                if (ex.Number == 50001)
                {
                    ShowError("This time slot is fully booked. Please select another time.");
                }
                else if (ex.Number == 50002)
                {
                    ShowError("We currently don't have inspectors available in your area. Our team will contact you to arrange service.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Unexpected SQL Error: {ex.Message}");
                    ShowError($"Database error occurred. Please try again or contact support.");
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("=== GENERAL EXCEPTION ===");
                System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

                ShowError("An unexpected error occurred while creating the inquiry.");
                return false;
            }
        }

        /// <summary>
        /// Send confirmation email to client
        /// </summary>
        private void SendConfirmationEmail(int clientId, string inquiryNumber, DateTime inspectionDate, string inspectionTime)
        {
            try
            {
                // Get client email
                string clientEmail = null;
                string clientName = Session["ClientName"]?.ToString() ?? "Valued Client";

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("SELECT EmailEnc FROM Clients WHERE ClientID = @ClientID", conn))
                {
                    cmd.Parameters.AddWithValue("@ClientID", clientId);
                    conn.Open();
                    var result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        clientEmail = AESHelper.DecryptEmail(result.ToString());
                    }
                }

                if (string.IsNullOrEmpty(clientEmail))
                    return;

                // Email content
                string subject = $"Inspection Request Confirmed - {inquiryNumber}";
                string body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #10b981 0%, #059669 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9fafb; padding: 30px; border-radius: 0 0 10px 10px; }}
        .info-box {{ background: white; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #10b981; }}
        .info-row {{ display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #e5e7eb; }}
        .info-label {{ font-weight: 600; color: #6b7280; }}
        .info-value {{ color: #1f2937; }}
        .button {{ display: inline-block; background: #10b981; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; margin: 20px 0; }}
        .footer {{ text-align: center; color: #6b7280; font-size: 12px; margin-top: 30px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✅ Inspection Request Confirmed!</h1>
        </div>
        <div class='content'>
            <p>Dear {HttpUtility.HtmlEncode(clientName)},</p>
            <p>Thank you for choosing RRC Termite & Pest Control! Your FREE inspection request has been successfully submitted.</p>
            
            <div class='info-box'>
                <h3 style='margin-top: 0; color: #059669;'>📋 Booking Details</h3>
                <div class='info-row'>
                    <span class='info-label'>Inquiry Number:</span>
                    <span class='info-value'><strong>{HttpUtility.HtmlEncode(inquiryNumber)}</strong></span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Inspection Date:</span>
                    <span class='info-value'>{inspectionDate:MMMM dd, yyyy (dddd)}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Preferred Time:</span>
                    <span class='info-value'>{HttpUtility.HtmlEncode(inspectionTime)}</span>
                </div>
            </div>

            <h3 style='color: #1f2937;'>🔍 What Happens Next?</h3>
            <ol style='padding-left: 20px; color: #4b5563;'>
                <li><strong>Admin Review</strong> - Our team will review your request within 24 hours</li>
                <li><strong>Inspector Assignment</strong> - We'll assign a certified inspector to your case</li>
                <li><strong>Confirmation Call</strong> - You'll receive a call to confirm the appointment</li>
                <li><strong>Inspection Day</strong> - Inspector visits your property at the scheduled time</li>
                <li><strong>Report & Quotation</strong> - You'll receive a detailed report and treatment options</li>
            </ol>

            <p style='margin-top: 20px;'>
                <a href='https://rrcmngmnt.com/MyInquiries.aspx' class='button'>Track Your Request</a>
            </p>

            <div style='background: #fef3c7; border-left: 4px solid #f59e0b; padding: 15px; border-radius: 6px; margin-top: 20px;'>
                <p style='margin: 0; color: #92400e;'>
                    <strong>⚠️ Important:</strong> Please ensure someone is available at the inspection location during the scheduled time.
                </p>
            </div>
        </div>
        <div class='footer'>
            <p>Need help? Contact us at <strong>rrctermiteandpestcontrol@gmail.com</strong> or call <strong>+63 992 435 7834</strong></p>
            <p>&copy; {DateTime.Now.Year} RRC Termite & Pest Control. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

                // Send email
                EmailHelper.SendEmail(clientEmail, subject, body, true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SendConfirmationEmail Error: {ex.Message}");
                // Don't fail the booking if email fails
            }
        }

        #endregion

        #region UI Messages

        /// <summary>
        /// Show error message using SweetAlert
        /// </summary>
        private void ShowError(string message)
        {
            string script = $@"
                Swal.fire({{
                    icon: 'error',
                    title: 'Oops...',
                    text: '{message.Replace("'", "\\'")}',
                    confirmButtonColor: '#3b82f6'
                }});
            ";
            ScriptManager.RegisterStartupScript(this, GetType(), "ShowError", script, true);
        }

        private int GetTimeSlotID()
        {
            if (rb5AM8AM.Checked) return 1;
            if (rb8AM11AM.Checked) return 2;
            if (rb11AM2PM.Checked) return 3;
            if (rb2PM5PM.Checked) return 4;
            if (rb5PM8PM.Checked) return 5;
            if (rb8PM10PM.Checked) return 6;
            return 2; // Default to morning
        }

        /// <summary>
        /// Show success message and redirect
        /// </summary>
        private void ShowSuccessAndRedirect(string inquiryNumber)
        {
            string script = $@"
                Swal.fire({{
                    icon: 'success',
                    title: 'Inspection Booked!',
                    html: '<p>Your FREE inspection request has been submitted successfully.</p><p><strong>Inquiry Number: {inquiryNumber}</strong></p><p>We will contact you within 24 hours to confirm your appointment.</p>',
                    confirmButtonColor: '#10b981',
                    confirmButtonText: 'View My Requests',
                    allowOutsideClick: false
                }}).then((result) => {{
                    if (result.isConfirmed) {{
                        window.location.href = 'MyInquiries.aspx';
                    }}
                }});
            ";
            ScriptManager.RegisterStartupScript(this, GetType(), "ShowSuccess", script, true);
        }

        #endregion
    }
}