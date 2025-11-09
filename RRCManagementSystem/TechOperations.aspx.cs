using Newtonsoft.Json.Linq;
using RRCManagementSystem.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class TechOperations : System.Web.UI.Page
    {
        private readonly string cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Verify Team Leader role
            if (Session["UserID"] == null || Session["Role"]?.ToString() != "Headtechnician")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadTeamBookings();
            }
        }

        /// <summary>
        /// Load all bookings assigned to the team leader's team
        /// </summary>
        private void LoadTeamBookings()
        {
            int teamLeaderID = Convert.ToInt32(Session["UserID"]);

            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spTeamLeader_GetAssignedBookings", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@TeamLeaderID", SqlDbType.Int).Value = teamLeaderID;

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            rptBookings.DataSource = dt;
                            rptBookings.DataBind();
                            lblNoBookings.Visible = false;
                        }
                        else
                        {
                            rptBookings.Visible = false;
                            lblNoBookings.Visible = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠️ Error loading bookings: " + ex.Message;
                lblMessage.Visible = true;
                System.Diagnostics.Debug.WriteLine($"LoadTeamBookings error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle repeater item data binding
        /// </summary>
        protected void rptBookings_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                // Format services with SQM
                var litServices = e.Item.FindControl("litServices") as Literal;
                if (litServices != null)
                {
                    string serviceDetails = DataBinder.Eval(e.Item.DataItem, "ServiceDetails")?.ToString();
                    string serviceNames = DataBinder.Eval(e.Item.DataItem, "ServiceNames")?.ToString();
                    litServices.Text = FormatServices(serviceDetails, serviceNames);
                }

                // Decrypt and format address
                var litAddress = e.Item.FindControl("litAddress") as Literal;
                if (litAddress != null)
                {
                    litAddress.Text = GetDecryptedAddress(e.Item.DataItem);
                }
            }
        }

        /// <summary>
        /// Handle repeater commands (View Details, View Photos, Start Service, Complete Service)
        /// </summary>
        protected void rptBookings_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int bookingID;
            int scheduleID;

            switch (e.CommandName)
            {
                case "ViewDetails":
                    string[] detailArgs = e.CommandArgument.ToString().Split('|');
                    bookingID = Convert.ToInt32(detailArgs[0]);
                    int reportID = detailArgs.Length > 1 && int.TryParse(detailArgs[1], out int rId) ? rId : 0;
                    ShowBookingDetails(bookingID, reportID);
                    break;

                case "ViewPhotos":
                    string photosPath = e.CommandArgument.ToString();
                    ShowPhotos(photosPath);
                    break;

                case "StartService":
                    string[] startArgs = e.CommandArgument.ToString().Split('|');
                    bookingID = Convert.ToInt32(startArgs[0]);
                    scheduleID = startArgs.Length > 1 && int.TryParse(startArgs[1], out int sId1) ? sId1 : 0;
                    UpdateServiceStatus(scheduleID, bookingID, "In Progress");
                    break;

                case "CompleteService":
                    string[] completeArgs = e.CommandArgument.ToString().Split('|');
                    bookingID = Convert.ToInt32(completeArgs[0]);
                    scheduleID = completeArgs.Length > 1 && int.TryParse(completeArgs[1], out int sId2) ? sId2 : 0;
                    UpdateServiceStatus(scheduleID, bookingID, "Completed");
                    break;
            }
        }

        /// <summary>
        /// Update service status at operation level (handles both contracts and single bookings)
        /// </summary>
        private void UpdateServiceStatus(int scheduleID, int bookingID, string newStatus)
        {
            int teamLeaderID = Convert.ToInt32(Session["UserID"]);

            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spServiceSchedule_UpdateStatus", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ScheduleID", SqlDbType.Int).Value = scheduleID;
                    cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingID;
                    cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = newStatus;
                    cmd.Parameters.Add("@UpdatedBy", SqlDbType.Int).Value = teamLeaderID;

                    con.Open();
                    object result = cmd.ExecuteScalar(); // ✅ Read SELECT result
                    int rowsAffected = result != null ? Convert.ToInt32(result) : 0;

                    if (rowsAffected > 0)
                    {
                        string icon = newStatus == "Completed" ? "success" : "info";
                        string title = newStatus == "Completed" ? "Service Completed!" : "Service Started!";
                        string message = newStatus == "Completed"
                            ? "The service has been marked as completed."
                            : "The service is now in progress.";

                        ShowAlert(icon, title, message);
                        LoadTeamBookings();
                    }
                    else
                    {
                        ShowAlert("error", "Error", "Failed to update status. You may not have permission to update this booking.");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateServiceStatus error: {ex.Message}");
                ShowAlert("error", "Database Error", $"An error occurred: {ex.Message}");
            }
        }


        /// <summary>
        /// Show detailed booking information in modal
        /// </summary>
        private void ShowBookingDetails(int bookingID, int reportID)
        {
            DataRow bookingData = GetBookingFullDetails(bookingID);

            if (bookingData == null)
            {
                ShowAlert("error", "Error", "Unable to load booking details.");
                return;
            }

            string modalContent = BuildDetailedModalContent(bookingData);

            string script = $@"
                Swal.fire({{
                    title: '<div style=""text-align:center; padding: 1rem;""><i class=""fas fa-file-invoice"" style=""color: #2563eb; font-size: 2rem; margin-bottom: 0.5rem;""></i><br><strong style=""font-size: 1.5rem; color: #1e293b;"">{System.Web.HttpUtility.JavaScriptStringEncode(bookingData["BookingCode"].ToString())}</strong></div>',
                    html: `{modalContent}`,
                    width: '900px',
                    showCloseButton: true,
                    showConfirmButton: false,
                    customClass: {{
                        popup: 'swal-professional-modal',
                        htmlContainer: 'swal-scrollable-content'
                    }},
                    didOpen: () => {{
                        const style = document.createElement('style');
                        style.textContent = `
                            .swal-professional-modal {{
                                border-radius: 16px;
                                box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
                            }}
                            .swal-scrollable-content {{
                                max-height: 70vh;
                                overflow-y: auto;
                                text-align: left;
                                padding: 0 1.5rem;
                            }}
                            .swal-scrollable-content::-webkit-scrollbar {{
                                width: 8px;
                            }}
                            .swal-scrollable-content::-webkit-scrollbar-track {{
                                background: #f1f5f9;
                                border-radius: 4px;
                            }}
                            .swal-scrollable-content::-webkit-scrollbar-thumb {{
                                background: #cbd5e1;
                                border-radius: 4px;
                            }}
                            .swal-scrollable-content::-webkit-scrollbar-thumb:hover {{
                                background: #94a3b8;
                            }}
                        `;
                        document.head.appendChild(style);
                    }}
                }});";

            ScriptManager.RegisterStartupScript(this, GetType(), "ShowDetails" + bookingID, script, true);
        }

        /// <summary>
        /// Get full booking details from database
        /// </summary>
        private DataRow GetBookingFullDetails(int bookingID)
        {
            try
            {
                int teamLeaderID = Convert.ToInt32(Session["UserID"]);

                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spTeamLeader_GetAssignedBookings", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@TeamLeaderID", SqlDbType.Int).Value = teamLeaderID;

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        foreach (DataRow row in dt.Rows)
                        {
                            if (Convert.ToInt32(row["BookingID"]) == bookingID)
                                return row;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetBookingFullDetails error: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Build detailed modal content with all booking, inspection, and client information
        /// </summary>
        private string BuildDetailedModalContent(DataRow data)
        {
            var html = new System.Text.StringBuilder();

            // Container with professional styling
            html.Append("<div style='font-family: system-ui, -apple-system, sans-serif;'>");

            // ========== STATUS BANNER ==========
            string operationStatus = data["OperationStatus"]?.ToString() ?? "N/A";
            string statusColor = operationStatus == "Completed" ? "#10b981" : (operationStatus == "In Progress" ? "#f59e0b" : "#3b82f6");
            string statusBg = operationStatus == "Completed" ? "#d1fae5" : (operationStatus == "In Progress" ? "#fef3c7" : "#dbeafe");

            html.Append($@"
                <div style='background: linear-gradient(135deg, {statusColor}15 0%, {statusColor}05 100%); 
                            border-left: 4px solid {statusColor}; 
                            padding: 1rem 1.25rem; 
                            border-radius: 8px; 
                            margin-bottom: 1.5rem;
                            display: flex;
                            align-items: center;
                            justify-content: space-between;'>
                    <div>
                        <p style='margin: 0; font-size: 0.875rem; color: #64748b; font-weight: 600;'>OPERATION STATUS</p>
                        <p style='margin: 0.25rem 0 0 0; font-size: 1.125rem; font-weight: 700; color: {statusColor};'>
                            {System.Web.HttpUtility.HtmlEncode(operationStatus)}
                        </p>
                    </div>
                    <div style='text-align: right;'>
                        <p style='margin: 0; font-size: 0.875rem; color: #64748b; font-weight: 600;'>SCHEDULED DATE</p>
                        <p style='margin: 0.25rem 0 0 0; font-size: 1rem; font-weight: 600; color: #1e293b;'>
                            {Convert.ToDateTime(data["ScheduledDate"]):MMMM dd, yyyy}
                        </p>
                    </div>
                </div>");

            // ========== INSPECTION REPORT SECTION ==========
            if (data["ReportID"] != DBNull.Value)
            {
                html.Append(@"
                    <div style='background: linear-gradient(to bottom, #eff6ff, #ffffff); 
                                border: 1px solid #bfdbfe; 
                                border-radius: 12px; 
                                padding: 1.5rem; 
                                margin-bottom: 1.5rem;
                                box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);'>
                        <h3 style='margin: 0 0 1.25rem 0; 
                                   font-size: 1.125rem; 
                                   font-weight: 700; 
                                   color: #1e40af; 
                                   display: flex; 
                                   align-items: center; 
                                   gap: 0.5rem;
                                   border-bottom: 2px solid #3b82f6;
                                   padding-bottom: 0.75rem;'>
                            <i class='fas fa-file-medical-alt'></i>
                            Inspection Report
                        </h3>
                        <div style='display: grid; grid-template-columns: repeat(2, 1fr); gap: 1rem;'>");

                if (data["QuotationCode"] != DBNull.Value)
                {
                    html.Append($@"
                        <div style='padding: 0.75rem; background: white; border-radius: 6px;'>
                            <p style='margin: 0; font-size: 0.75rem; color: #64748b; font-weight: 600; text-transform: uppercase;'>Quotation Code</p>
                            <p style='margin: 0.25rem 0 0 0; font-size: 1rem; color: #1e293b; font-weight: 600;'>{System.Web.HttpUtility.HtmlEncode(data["QuotationCode"].ToString())}</p>
                        </div>");
                }

                if (data["InspectionDate"] != DBNull.Value)
                {
                    html.Append($@"
                        <div style='padding: 0.75rem; background: white; border-radius: 6px;'>
                            <p style='margin: 0; font-size: 0.75rem; color: #64748b; font-weight: 600; text-transform: uppercase;'>Inspection Date</p>
                            <p style='margin: 0.25rem 0 0 0; font-size: 1rem; color: #1e293b; font-weight: 600;'>{Convert.ToDateTime(data["InspectionDate"]):MMMM dd, yyyy}</p>
                        </div>");
                }

                if (data["InfestationLevel"] != DBNull.Value)
                {
                    string infestationLevel = data["InfestationLevel"].ToString();
                    string infestationColor = infestationLevel == "High" ? "#dc2626" : (infestationLevel == "Medium" ? "#f59e0b" : "#16a34a");

                    html.Append($@"
                        <div style='padding: 0.75rem; background: white; border-radius: 6px; grid-column: span 2;'>
                            <p style='margin: 0; font-size: 0.75rem; color: #64748b; font-weight: 600; text-transform: uppercase;'>Infestation Level</p>
                            <p style='margin: 0.25rem 0 0 0; font-size: 1.125rem; color: {infestationColor}; font-weight: 700;'>
                                <i class='fas fa-exclamation-triangle'></i> {System.Web.HttpUtility.HtmlEncode(infestationLevel)}
                            </p>
                        </div>");
                }

                if (data["FindingsDescription"] != DBNull.Value && !string.IsNullOrWhiteSpace(data["FindingsDescription"].ToString()))
                {
                    html.Append($@"
                        <div style='padding: 0.75rem; background: white; border-radius: 6px; grid-column: span 2;'>
                            <p style='margin: 0; font-size: 0.75rem; color: #64748b; font-weight: 600; text-transform: uppercase;'>Findings</p>
                            <p style='margin: 0.5rem 0 0 0; font-size: 0.9375rem; color: #475569; line-height: 1.6;'>{System.Web.HttpUtility.HtmlEncode(data["FindingsDescription"].ToString())}</p>
                        </div>");
                }

                if (data["AffectedAreas"] != DBNull.Value && !string.IsNullOrWhiteSpace(data["AffectedAreas"].ToString()))
                {
                    html.Append($@"
                        <div style='padding: 0.75rem; background: white; border-radius: 6px; grid-column: span 2;'>
                            <p style='margin: 0; font-size: 0.75rem; color: #64748b; font-weight: 600; text-transform: uppercase;'>Affected Areas</p>
                            <p style='margin: 0.5rem 0 0 0; font-size: 0.9375rem; color: #475569; line-height: 1.6;'>{System.Web.HttpUtility.HtmlEncode(data["AffectedAreas"].ToString())}</p>
                        </div>");
                }

                if (data["FollowUpRequired"] != DBNull.Value && Convert.ToBoolean(data["FollowUpRequired"]))
                {
                    string followUpReason = data["FollowUpReason"] != DBNull.Value ? data["FollowUpReason"].ToString() : "Not specified";
                    html.Append($@"
                        <div style='padding: 0.75rem; background: #fef3c7; border: 1px solid #fbbf24; border-radius: 6px; grid-column: span 2;'>
                            <p style='margin: 0; font-size: 0.75rem; color: #b45309; font-weight: 600; text-transform: uppercase;'>
                                <i class='fas fa-calendar-check'></i> Follow-up Required
                            </p>
                            <p style='margin: 0.5rem 0 0 0; font-size: 0.9375rem; color: #78350f; font-weight: 500;'>{System.Web.HttpUtility.HtmlEncode(followUpReason)}</p>");

                    if (data["FollowUpDate"] != DBNull.Value)
                    {
                        html.Append($"<p style='margin: 0.25rem 0 0 0; font-size: 0.875rem; color: #92400e;'>Date: {Convert.ToDateTime(data["FollowUpDate"]):MMMM dd, yyyy}</p>");
                    }

                    html.Append("</div>");
                }

                html.Append("</div></div>");
            }

            // ========== CLIENT INFORMATION ==========
            html.Append(@"
                <div style='background: linear-gradient(to bottom, #f8fafc, #ffffff); 
                            border: 1px solid #e2e8f0; 
                            border-radius: 12px; 
                            padding: 1.5rem; 
                            margin-bottom: 1.5rem;
                            box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);'>
                    <h3 style='margin: 0 0 1.25rem 0; 
                               font-size: 1.125rem; 
                               font-weight: 700; 
                               color: #334155; 
                               display: flex; 
                               align-items: center; 
                               gap: 0.5rem;
                               border-bottom: 2px solid #64748b;
                               padding-bottom: 0.75rem;'>
                        <i class='fas fa-user'></i>
                        Client Information
                    </h3>
                    <div style='display: grid; gap: 1rem;'>");

            html.Append($@"
                <div style='padding: 0.75rem; background: white; border-radius: 6px;'>
                    <p style='margin: 0; font-size: 0.75rem; color: #64748b; font-weight: 600; text-transform: uppercase;'>Client Name</p>
                    <p style='margin: 0.25rem 0 0 0; font-size: 1rem; color: #1e293b; font-weight: 600;'>{System.Web.HttpUtility.HtmlEncode(data["ClientName"].ToString())}</p>
                </div>");

            string contact = "N/A";
            if (data["ClientContactEnc"] != DBNull.Value)
            {
                try { contact = AESHelper.DecryptField(data["ClientContactEnc"].ToString()); }
                catch { contact = "Unable to decrypt"; }
            }

            html.Append($@"
                <div style='padding: 0.75rem; background: white; border-radius: 6px;'>
                    <p style='margin: 0; font-size: 0.75rem; color: #64748b; font-weight: 600; text-transform: uppercase;'>Contact Number</p>
                    <p style='margin: 0.25rem 0 0 0; font-size: 1rem; color: #1e293b; font-weight: 600;'>
                        <i class='fas fa-phone' style='color: #3b82f6;'></i> {System.Web.HttpUtility.HtmlEncode(contact)}
                    </p>
                </div>");

            var addressParts = new List<string>();
            try
            {
                if (data["StreetEnc"] != DBNull.Value)
                {
                    string street = AESHelper.DecryptField(data["StreetEnc"].ToString());
                    if (!string.IsNullOrEmpty(street)) addressParts.Add(street);
                }
                if (data["BarangayEnc"] != DBNull.Value)
                {
                    string barangay = AESHelper.DecryptField(data["BarangayEnc"].ToString());
                    if (!string.IsNullOrEmpty(barangay)) addressParts.Add(barangay);
                }
                if (data["CityEnc"] != DBNull.Value)
                {
                    string city = AESHelper.DecryptField(data["CityEnc"].ToString());
                    if (!string.IsNullOrEmpty(city)) addressParts.Add(city);
                }
                if (data["RegionEnc"] != DBNull.Value)
                {
                    string region = AESHelper.DecryptField(data["RegionEnc"].ToString());
                    if (!string.IsNullOrEmpty(region)) addressParts.Add(region);
                }
                if (data["CountryEnc"] != DBNull.Value)
                {
                    string country = AESHelper.DecryptField(data["CountryEnc"].ToString());
                    if (!string.IsNullOrEmpty(country)) addressParts.Add(country);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Address decryption error: {ex.Message}");
            }

            html.Append($@"
                <div style='padding: 0.75rem; background: white; border-radius: 6px;'>
                    <p style='margin: 0; font-size: 0.75rem; color: #64748b; font-weight: 600; text-transform: uppercase;'>
                        <i class='fas fa-map-marker-alt'></i> Service Address
                    </p>
                    <p style='margin: 0.5rem 0 0 0; font-size: 0.9375rem; color: #475569; line-height: 1.6;'>{System.Web.HttpUtility.HtmlEncode(string.Join(", ", addressParts))}</p>
                </div>");

            if (data["LandmarkEnc"] != DBNull.Value)
            {
                try
                {
                    string landmark = AESHelper.DecryptField(data["LandmarkEnc"].ToString());
                    if (!string.IsNullOrEmpty(landmark))
                    {
                        html.Append($@"
                            <div style='padding: 0.5rem 0.75rem; background: #fef3c7; border-left: 3px solid #f59e0b; border-radius: 4px;'>
                                <p style='margin: 0; font-size: 0.875rem; color: #78350f;'>
                                    <i class='fas fa-location-dot' style='color: #f59e0b;'></i> 
                                    <strong>Landmark:</strong> {System.Web.HttpUtility.HtmlEncode(landmark)}
                                </p>
                            </div>");
                    }
                }
                catch { }
            }

            html.Append("</div></div>");

            // ========== SERVICE DETAILS ==========
            html.Append(@"
                <div style='background: linear-gradient(to bottom, #f0fdf4, #ffffff); 
                            border: 1px solid #bbf7d0; 
                            border-radius: 12px; 
                            padding: 1.5rem; 
                            margin-bottom: 1.5rem;
                            box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);'>
                    <h3 style='margin: 0 0 1.25rem 0; 
                               font-size: 1.125rem; 
                               font-weight: 700; 
                               color: #15803d; 
                               display: flex; 
                               align-items: center; 
                               gap: 0.5rem;
                               border-bottom: 2px solid #22c55e;
                               padding-bottom: 0.75rem;'>
                        <i class='fas fa-tasks'></i>
                        Service Details
                    </h3>
                    <div style='display: grid; grid-template-columns: repeat(2, 1fr); gap: 1rem;'>");

            html.Append($@"
                <div style='padding: 0.75rem; background: white; border-radius: 6px; grid-column: span 2;'>
                    <p style='margin: 0; font-size: 0.75rem; color: #64748b; font-weight: 600; text-transform: uppercase;'>Services Requested</p>
                    <p style='margin: 0.5rem 0 0 0; font-size: 0.9375rem; color: #1e293b; font-weight: 500;'>{System.Web.HttpUtility.HtmlEncode(data["ServiceNames"].ToString())}</p>
                </div>
                <div style='padding: 0.75rem; background: white; border-radius: 6px;'>
                    <p style='margin: 0; font-size: 0.75rem; color: #64748b; font-weight: 600; text-transform: uppercase;'>Coverage Area</p>
                    <p style='margin: 0.25rem 0 0 0; font-size: 1.125rem; color: #16a34a; font-weight: 700;'>{data["SQM"]} m²</p>
                </div>
                <div style='padding: 0.75rem; background: white; border-radius: 6px;'>
                    <p style='margin: 0; font-size: 0.75rem; color: #64748b; font-weight: 600; text-transform: uppercase;'>Inspector Assigned</p>
                    <p style='margin: 0.25rem 0 0 0; font-size: 1rem; color: #1e293b; font-weight: 600;'>{System.Web.HttpUtility.HtmlEncode(data["InspectorName"]?.ToString() ?? "N/A")}</p>
                </div>");

            // Financial Information
            html.Append(@"
                <div style='padding: 1rem; background: #dbeafe; border-radius: 8px; grid-column: span 2;'>
                    <p style='margin: 0 0 0.75rem 0; font-size: 0.875rem; color: #1e40af; font-weight: 700; text-transform: uppercase;'>Financial Breakdown</p>
                    <div style='display: grid; gap: 0.5rem;'>");

            html.Append($@"
                        <div style='display: flex; justify-content: space-between; align-items: center;'>
                            <span style='color: #475569; font-size: 0.9375rem;'>Service Price:</span>
                            <span style='color: #1e293b; font-size: 1rem; font-weight: 600;'>₱{Convert.ToDecimal(data["Price"]):N2}</span>
                        </div>
                        <div style='display: flex; justify-content: space-between; align-items: center;'>
                            <span style='color: #475569; font-size: 0.9375rem;'>Travel Expense:</span>
                            <span style='color: #1e293b; font-size: 1rem; font-weight: 600;'>₱{Convert.ToDecimal(data["TravelExpense"]):N2}</span>
                        </div>
                        <div style='display: flex; justify-content: space-between; align-items: center;'>
                            <span style='color: #475569; font-size: 0.9375rem;'>Miscellaneous:</span>
                            <span style='color: #1e293b; font-size: 1rem; font-weight: 600;'>₱{Convert.ToDecimal(data["Miscellaneous"]):N2}</span>
                        </div>
                        <div style='border-top: 2px solid #3b82f6; padding-top: 0.5rem; margin-top: 0.25rem; display: flex; justify-content: space-between; align-items: center;'>
                            <span style='color: #1e40af; font-size: 1rem; font-weight: 700;'>Payment Plan:</span>
                            <span style='color: #1e40af; font-size: 1rem; font-weight: 700;'>{System.Web.HttpUtility.HtmlEncode(data["PaymentPlan"].ToString())}</span>
                        </div>");

            html.Append("</div></div>");

            if (data["Notes"] != DBNull.Value && !string.IsNullOrWhiteSpace(data["Notes"].ToString()))
            {
                html.Append($@"
                    <div style='padding: 0.75rem; background: #fef3c7; border-left: 3px solid #f59e0b; border-radius: 6px; grid-column: span 2;'>
                        <p style='margin: 0; font-size: 0.75rem; color: #78350f; font-weight: 600; text-transform: uppercase;'>
                            <i class='fas fa-sticky-note'></i> Additional Notes
                        </p>
                        <p style='margin: 0.5rem 0 0 0; font-size: 0.9375rem; color: #78350f; line-height: 1.6;'>{System.Web.HttpUtility.HtmlEncode(data["Notes"].ToString())}</p>
                    </div>");
            }

            html.Append("</div></div>");

            // ========== ASSIGNED RESOURCES ==========
            html.Append(@"
                <div style='background: linear-gradient(to bottom, #faf5ff, #ffffff); 
                            border: 1px solid #e9d5ff; 
                            border-radius: 12px; 
                            padding: 1.5rem;
                            box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);'>
                    <h3 style='margin: 0 0 1.25rem 0; 
                               font-size: 1.125rem; 
                               font-weight: 700; 
                               color: #7c3aed; 
                               display: flex; 
                               align-items: center; 
                               gap: 0.5rem;
                               border-bottom: 2px solid #a855f7;
                               padding-bottom: 0.75rem;'>
                        <i class='fas fa-toolbox'></i>
                        Assigned Resources
                    </h3>
                    <div style='display: grid; gap: 1rem;'>");

            string equipment = data["AssignedEquipment"] != DBNull.Value ? data["AssignedEquipment"].ToString() : "None assigned";
            string chemicals = data["AssignedChemicals"] != DBNull.Value ? data["AssignedChemicals"].ToString() : "None assigned";
            string safetyGear = data["AssignedSafetyGear"] != DBNull.Value ? data["AssignedSafetyGear"].ToString() : "None assigned";

            html.Append($@"
                <div style='padding: 0.75rem; background: white; border-left: 3px solid #3b82f6; border-radius: 6px;'>
                    <p style='margin: 0; font-size: 0.75rem; color: #3b82f6; font-weight: 700; text-transform: uppercase;'>
                        <i class='fas fa-wrench'></i> Equipment
                    </p>
                    <p style='margin: 0.5rem 0 0 0; font-size: 0.9375rem; color: #475569; line-height: 1.6;'>{System.Web.HttpUtility.HtmlEncode(equipment)}</p>
                </div>
                <div style='padding: 0.75rem; background: white; border-left: 3px solid #22c55e; border-radius: 6px;'>
                    <p style='margin: 0; font-size: 0.75rem; color: #22c55e; font-weight: 700; text-transform: uppercase;'>
                        <i class='fas fa-flask'></i> Chemicals
                    </p>
                    <p style='margin: 0.5rem 0 0 0; font-size: 0.9375rem; color: #475569; line-height: 1.6;'>{System.Web.HttpUtility.HtmlEncode(chemicals)}</p>
                </div>
                <div style='padding: 0.75rem; background: white; border-left: 3px solid #f59e0b; border-radius: 6px;'>
                    <p style='margin: 0; font-size: 0.75rem; color: #f59e0b; font-weight: 700; text-transform: uppercase;'>
                        <i class='fas fa-hard-hat'></i> Safety Gear
                    </p>
                    <p style='margin: 0.5rem 0 0 0; font-size: 0.9375rem; color: #475569; line-height: 1.6;'>{System.Web.HttpUtility.HtmlEncode(safetyGear)}</p>
                </div>");

            html.Append("</div></div>");

            html.Append("</div>");

            return html.ToString()
                .Replace("'", "\\'")
                .Replace("\r\n", " ")
                .Replace("\n", " ")
                .Replace("\r", " ");
        }

        /// <summary>
        /// Show inspection photos
        /// </summary>
        private void ShowPhotos(string photosPath)
        {
            if (string.IsNullOrWhiteSpace(photosPath))
            {
                ShowAlert("info", "No Photos", "No inspection photos available for this booking.");
                return;
            }

            string script = $@"
                Swal.fire({{
                    title: 'Inspection Photos',
                    html: '<p>Photo viewing feature coming soon</p><p class=""text-sm text-gray-600"">Path: {System.Web.HttpUtility.JavaScriptStringEncode(photosPath)}</p>',
                    icon: 'info',
                    confirmButtonColor: '#2563eb'
                }});";

            ScriptManager.RegisterStartupScript(this, GetType(), "ShowPhotos", script, true);
        }

        /// <summary>
        /// Show SweetAlert notification
        /// </summary>
        private void ShowAlert(string icon, string title, string text)
        {
            string script = $@"
                Swal.fire({{
                    icon: '{icon}',
                    title: '{System.Web.HttpUtility.JavaScriptStringEncode(title)}',
                    text: '{System.Web.HttpUtility.JavaScriptStringEncode(text)}',
                    confirmButtonColor: '#2563eb'
                }});";

            ScriptManager.RegisterStartupScript(this, GetType(), "Alert" + DateTime.Now.Ticks, script, true);
        }

        /// <summary>
        /// Decrypt and format address for display in repeater
        /// </summary>
        private string GetDecryptedAddress(object dataItem)
        {
            try
            {
                var addressParts = new List<string>();

                string street = DataBinder.Eval(dataItem, "StreetEnc")?.ToString();
                string barangay = DataBinder.Eval(dataItem, "BarangayEnc")?.ToString();
                string city = DataBinder.Eval(dataItem, "CityEnc")?.ToString();
                string region = DataBinder.Eval(dataItem, "RegionEnc")?.ToString();

                if (!string.IsNullOrEmpty(street))
                {
                    string decrypted = AESHelper.DecryptField(street);
                    if (!string.IsNullOrEmpty(decrypted)) addressParts.Add(decrypted);
                }

                if (!string.IsNullOrEmpty(barangay))
                {
                    string decrypted = AESHelper.DecryptField(barangay);
                    if (!string.IsNullOrEmpty(decrypted)) addressParts.Add(decrypted);
                }

                if (!string.IsNullOrEmpty(city))
                {
                    string decrypted = AESHelper.DecryptField(city);
                    if (!string.IsNullOrEmpty(decrypted)) addressParts.Add(decrypted);
                }

                if (!string.IsNullOrEmpty(region))
                {
                    string decrypted = AESHelper.DecryptField(region);
                    if (!string.IsNullOrEmpty(decrypted)) addressParts.Add(decrypted);
                }

                string fullAddress = string.Join(", ", addressParts);

                return $"<span class='info-value'>{System.Web.HttpUtility.HtmlEncode(fullAddress)}</span>";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetDecryptedAddress error: {ex.Message}");
                return "<span class='info-value text-red-500'>Unable to decrypt address</span>";
            }
        }

        /// <summary>
        /// Decrypt contact number
        /// </summary>
        protected string GetDecryptedContact(string contactEnc)
        {
            try
            {
                if (string.IsNullOrEmpty(contactEnc))
                    return "N/A";

                return System.Web.HttpUtility.HtmlEncode(AESHelper.DecryptField(contactEnc));
            }
            catch
            {
                return "<span class='text-red-500'>Unable to decrypt</span>";
            }
        }

        /// <summary>
        /// Format services with individual SQM values from JSON
        /// </summary>
        private string FormatServices(string serviceDetailsJson, string serviceNames)
        {
            if (!string.IsNullOrWhiteSpace(serviceDetailsJson) && serviceDetailsJson.TrimStart().StartsWith("["))
            {
                try
                {
                    var services = JArray.Parse(serviceDetailsJson);
                    var serviceList = new List<string>();

                    foreach (var service in services)
                    {
                        string name = service["ServiceName"]?.ToString();
                        int sqm = Convert.ToInt32(service["SQM"] ?? 0);

                        if (!string.IsNullOrEmpty(name))
                        {
                            serviceList.Add($"{System.Web.HttpUtility.HtmlEncode(name)} ({sqm} m²)");
                        }
                    }

                    return $"<span class='info-value'>{string.Join("<br/>", serviceList)}</span>";
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"FormatServices JSON parse error: {ex.Message}");
                }
            }

            return $"<span class='info-value'>{System.Web.HttpUtility.HtmlEncode(serviceNames ?? "N/A")}</span>";
        }

        /// <summary>
        /// Get CSS class based on booking status
        /// </summary>
        protected string GetStatusClass(object status)
        {
            string statusStr = status?.ToString()?.ToLower() ?? "";

            if (statusStr == "in progress") return "in-progress";
            if (statusStr == "completed") return "completed";

            return "";
        }

        /// <summary>
        /// Get status badge CSS class
        /// </summary>
        protected string GetStatusBadgeClass(object status)
        {
            string statusStr = status?.ToString()?.ToLower() ?? "";

            if (statusStr == "in progress") return "status-in-progress";
            if (statusStr == "completed") return "status-completed";

            return "status-assigned";
        }

        /// <summary>
        /// Get Font Awesome icon for status
        /// </summary>
        protected string GetStatusIcon(object status)
        {
            string statusStr = status?.ToString()?.ToLower() ?? "";

            if (statusStr == "in progress") return "fa-spinner";
            if (statusStr == "completed") return "fa-check-circle";

            return "fa-clipboard-check";
        }

        /// <summary>
        /// Format TimeSpan to readable time string
        /// </summary>
        protected string FormatTime(object timeValue)
        {
            if (timeValue == null || timeValue == DBNull.Value)
                return "—";

            try
            {
                if (timeValue is TimeSpan ts)
                {
                    return DateTime.Today.Add(ts).ToString("hh:mm tt");
                }

                if (TimeSpan.TryParse(timeValue.ToString(), out TimeSpan parsedTime))
                {
                    return DateTime.Today.Add(parsedTime).ToString("hh:mm tt");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FormatTime error: {ex.Message}");
            }

            return "—";
        }
    }
}