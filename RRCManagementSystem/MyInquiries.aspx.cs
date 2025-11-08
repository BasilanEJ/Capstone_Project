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
    public partial class MyInquiries : System.Web.UI.Page
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

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
                LoadInquiries("All");
                UpdateStatusCounts();
            }
        }

        #region Load Inquiries

        /// <summary>
        /// Load inquiries based on status filter
        /// </summary>
        private void LoadInquiries(string statusFilter)
        {
            try
            {
                pnlLoading.Visible = true;
                pnlEmpty.Visible = false;
                rptInquiries.Visible = false;

                int clientId = Convert.ToInt32(Session["ClientID"]);

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spInquiry_GetByClient", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                    cmd.Parameters.Add("@StatusFilter", SqlDbType.NVarChar, 50).Value = statusFilter;

                    var dt = new DataTable();
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }

                    pnlLoading.Visible = false;

                    if (dt.Rows.Count > 0)
                    {
                        rptInquiries.DataSource = dt;
                        rptInquiries.DataBind();
                        rptInquiries.Visible = true;
                        pnlEmpty.Visible = false;
                    }
                    else
                    {
                        rptInquiries.Visible = false;
                        pnlEmpty.Visible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadInquiries Error: {ex.Message}");
                ShowError("Error loading inquiries. Please try again.");
                pnlLoading.Visible = false;
                pnlEmpty.Visible = true;
            }
        }

        /// <summary>
        /// Update status count badges
        /// </summary>
        private void UpdateStatusCounts()
        {
            try
            {
                int clientId = Convert.ToInt32(Session["ClientID"]);

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spInquiry_GetStatusCounts", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string status = reader["Status"]?.ToString();
                            int count = reader["Count"] != DBNull.Value ? Convert.ToInt32(reader["Count"]) : 0;

                            switch (status)
                            {
                                case "All":
                                    lblCountAll.Text = count.ToString();
                                    break;
                                case "Pending":
                                    lblCountPending.Text = count.ToString();
                                    break;
                                case "Assigned":
                                case "Validated":
                                case "Scheduled":
                                    lblCountAssigned.Text = (Convert.ToInt32(lblCountAssigned.Text) + count).ToString();
                                    break;
                                case "Inspected":
                                case "In Progress":
                                    lblCountInspected.Text = (Convert.ToInt32(lblCountInspected.Text) + count).ToString();
                                    break;
                                case "Quotation Sent":
                                    lblCountQuotation.Text = count.ToString();
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateStatusCounts Error: {ex.Message}");
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handle status filter tab clicks
        /// </summary>
        protected void FilterStatus_Click(object sender, EventArgs e)
        {
            try
            {
                var btn = (LinkButton)sender;
                string statusFilter = btn.CommandArgument;

                // Update active tab styling
                btnAll.CssClass = "status-tab";
                btnPending.CssClass = "status-tab";
                btnAssigned.CssClass = "status-tab";
                btnInspected.CssClass = "status-tab";
                btnQuotation.CssClass = "status-tab";

                btn.CssClass = "status-tab active";

                // Load filtered inquiries
                LoadInquiries(statusFilter);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FilterStatus_Click Error: {ex.Message}");
                ShowError("Error filtering inquiries.");
            }
        }

        /// <summary>
        /// Handle view details button click - UPDATED TO ROUTE TO ClientInspectionDetails
        /// </summary>
        protected void btnViewDetails_Click(object sender, EventArgs e)
        {
            try
            {
                var btn = (LinkButton)sender;
                int inquiryId = Convert.ToInt32(btn.CommandArgument);

                // Check if there's an approved inspection report
                var inspectionInfo = GetInspectionInfo(inquiryId);

                if (inspectionInfo != null && inspectionInfo.HasReport)
                {
                    // Redirect to ClientInspectionDetails (detailed inspection report)
                    Response.Redirect($"ClientInspectionDetails.aspx?ReportID={inspectionInfo.ReportID}", false);
                }
                else
                {
                    // Redirect to basic inquiry details (manual quotation or no quotation)
                    Response.Redirect($"InquiryDetails.aspx?id={inquiryId}", false);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"btnViewDetails_Click Error: {ex.Message}");
                ShowError("Error viewing details.");
            }
        }

        /// <summary>
        /// Get inspection report info for an inquiry
        /// </summary>
        private InspectionInfo GetInspectionInfo(int inquiryId)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(@"
    SELECT TOP 1 ReportID, Status 
    FROM dbo.InspectionReports 
    WHERE InquiryID = @InquiryID 
      AND Status IN ('Quotation Sent', 'Approved')
    ORDER BY CreatedAt DESC", conn))

                {
                    cmd.Parameters.Add("@InquiryID", SqlDbType.Int).Value = inquiryId;

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new InspectionInfo
                            {
                                HasReport = true,
                                ReportID = Convert.ToInt32(reader["ReportID"]),
                                Status = reader["Status"].ToString()
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetInspectionInfo Error: {ex.Message}");
            }

            return new InspectionInfo { HasReport = false };
        }

        /// <summary>
        /// Helper class for inspection info
        /// </summary>
        private class InspectionInfo
        {
            public bool HasReport { get; set; }
            public int ReportID { get; set; }
            public string Status { get; set; }
        }

        /// <summary>
        /// Handle approve quotation button click
        /// </summary>
        protected void btnApproveQuotation_Click(object sender, EventArgs e)
        {
            try
            {
                var btn = (LinkButton)sender;
                int inquiryId = Convert.ToInt32(btn.CommandArgument);

                if (ApproveQuotation(inquiryId))
                {
                    ShowSuccess("Quotation approved successfully! We'll contact you to schedule the service.");
                    LoadInquiries("All");
                    UpdateStatusCounts();
                }
                else
                {
                    ShowError("Failed to approve quotation. Please try again.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"btnApproveQuotation_Click Error: {ex.Message}");
                ShowError("Error approving quotation.");
            }
        }

        /// <summary>
        /// Handle reject quotation button click
        /// </summary>
        protected void btnRejectQuotation_Click(object sender, EventArgs e)
        {
            try
            {
                var btn = (LinkButton)sender;
                int inquiryId = Convert.ToInt32(btn.CommandArgument);

                // Show SweetAlert prompt for rejection reason
                string script = $@"
                    Swal.fire({{
                        title: 'Reject Quotation',
                        text: 'Please provide a reason for rejection:',
                        input: 'textarea',
                        inputPlaceholder: 'Enter your reason here...',
                        showCancelButton: true,
                        confirmButtonText: 'Submit',
                        confirmButtonColor: '#ef4444',
                        cancelButtonColor: '#6b7280',
                        inputValidator: (value) => {{
                            if (!value) {{
                                return 'Please enter a reason for rejection'
                            }}
                        }}
                    }}).then((result) => {{
                        if (result.isConfirmed) {{
                            __doPostBack('btnRejectConfirm', '{inquiryId}|' + result.value);
                        }}
                    }});
                ";
                ScriptManager.RegisterStartupScript(this, GetType(), "RejectPrompt", script, true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"btnRejectQuotation_Click Error: {ex.Message}");
                ShowError("Error rejecting quotation.");
            }
        }

        /// <summary>
        /// Handle cancel inspection button click
        /// </summary>
        protected void btnCancelInquiry_Click(object sender, EventArgs e)
        {
            try
            {
                var btn = (LinkButton)sender;
                int inquiryId = Convert.ToInt32(btn.CommandArgument);

                // Show confirmation dialog
                string script = $@"
                    Swal.fire({{
                        title: 'Cancel Inspection?',
                        text: 'Are you sure you want to cancel this inspection request?',
                        icon: 'warning',
                        showCancelButton: true,
                        confirmButtonText: 'Yes, Cancel It',
                        cancelButtonText: 'No, Keep It',
                        confirmButtonColor: '#ef4444',
                        cancelButtonColor: '#6b7280'
                    }}).then((result) => {{
                        if (result.isConfirmed) {{
                            __doPostBack('btnCancelConfirm', '{inquiryId}');
                        }}
                    }});
                ";
                ScriptManager.RegisterStartupScript(this, GetType(), "CancelPrompt", script, true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"btnCancelInquiry_Click Error: {ex.Message}");
                ShowError("Error cancelling inquiry.");
            }
        }

        /// <summary>
        /// Handle postback for reject/cancel confirmations
        /// </summary>
        protected override void RaisePostBackEvent(IPostBackEventHandler sourceControl, string eventArgument)
        {
            if (eventArgument.StartsWith("btnRejectConfirm|"))
            {
                string[] parts = eventArgument.Split('|');
                if (parts.Length >= 2)
                {
                    int inquiryId = Convert.ToInt32(parts[0].Replace("btnRejectConfirm|", ""));
                    string reason = parts.Length > 2 ? string.Join("|", parts, 1, parts.Length - 1) : parts[1];

                    if (RejectQuotation(inquiryId, reason))
                    {
                        ShowSuccess("Quotation rejected. Thank you for your feedback.");
                        LoadInquiries("All");
                        UpdateStatusCounts();
                    }
                    else
                    {
                        ShowError("Failed to reject quotation. Please try again.");
                    }
                }
            }
            else if (eventArgument.StartsWith("btnCancelConfirm|"))
            {
                int inquiryId = Convert.ToInt32(eventArgument.Replace("btnCancelConfirm|", ""));

                if (CancelInquiry(inquiryId))
                {
                    ShowSuccess("Inspection request cancelled successfully.");
                    LoadInquiries("All");
                    UpdateStatusCounts();
                }
                else
                {
                    ShowError("Failed to cancel inquiry. Please try again.");
                }
            }
            else
            {
                base.RaisePostBackEvent(sourceControl, eventArgument);
            }
        }

        /// <summary>
        /// Handle repeater item data bound
        /// </summary>
        protected void rptInquiries_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var dataItem = (DataRowView)e.Item.DataItem;

                // Find PlaceHolder controls
                var phInspectorInfo = (PlaceHolder)e.Item.FindControl("phInspectorInfo");
                var phImages = (PlaceHolder)e.Item.FindControl("phImages");
                var phQuotationInfo = (PlaceHolder)e.Item.FindControl("phQuotationInfo");
                var phTimeline = (PlaceHolder)e.Item.FindControl("phTimeline");
                var phApprovalButtons = (PlaceHolder)e.Item.FindControl("phApprovalButtons");
                var phCancelButton = (PlaceHolder)e.Item.FindControl("phCancelButton");

                // Add HTML using Literal controls
                phInspectorInfo.Controls.Add(new Literal { Text = ShowInspectorInfo(dataItem["InspectorName"], dataItem["AssignedAt"]) });
                phImages.Controls.Add(new Literal { Text = ShowImages(dataItem["InspectionReportPath"]) });
                phQuotationInfo.Controls.Add(new Literal { Text = ShowQuotationInfo(dataItem["QuotationAmount"], dataItem["QuotationDetails"], dataItem["QuotationSentAt"]) });
                phTimeline.Controls.Add(new Literal { Text = ShowTimeline(dataItem["Status"], dataItem["CreatedAt"], dataItem["AssignedAt"], dataItem["InspectionCompletedAt"], dataItem["QuotationSentAt"]) });

                // Handle dynamic buttons
                string status = dataItem["Status"]?.ToString();
                string clientApproval = dataItem["ClientApproval"]?.ToString();
                int inquiryId = Convert.ToInt32(dataItem["InquiryID"]);

                // Show Approval/Rejection buttons if status is "Quotation Sent" and approval is "Pending"
                if (status == "Quotation Sent" && clientApproval == "Pending")
                {
                    var btnApprove = new LinkButton
                    {
                        CssClass = "btn btn-success",
                        CommandArgument = inquiryId.ToString(),
                        Text = "<i class='fas fa-check'></i> Approve Quotation"
                    };
                    btnApprove.Click += btnApproveQuotation_Click;

                    var btnReject = new LinkButton
                    {
                        CssClass = "btn btn-danger",
                        CommandArgument = inquiryId.ToString(),
                        Text = "<i class='fas fa-times'></i> Reject Quotation"
                    };
                    btnReject.Click += btnRejectQuotation_Click;

                    phApprovalButtons.Controls.Add(btnApprove);
                    phApprovalButtons.Controls.Add(btnReject);
                }

                // Show Cancel button if status allows (Pending, Validated, Assigned)
                if (status == "Pending" || status == "Validated" || status == "Assigned")
                {
                    var btnCancel = new LinkButton
                    {
                        CssClass = "btn btn-secondary",
                        CommandArgument = inquiryId.ToString(),
                        Text = "<i class='fas fa-ban'></i> Cancel Request"
                    };
                    btnCancel.Click += btnCancelInquiry_Click;

                    phCancelButton.Controls.Add(btnCancel);
                }
            }
        }

        #endregion

        #region Database Operations

        /// <summary>
        /// Approve quotation
        /// </summary>
        private bool ApproveQuotation(int inquiryId)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spInquiry_ApproveQuotation", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@InquiryID", SqlDbType.Int).Value = inquiryId;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ApproveQuotation Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Reject quotation
        /// </summary>
        private bool RejectQuotation(int inquiryId, string reason)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spInquiry_RejectQuotation", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@InquiryID", SqlDbType.Int).Value = inquiryId;
                    cmd.Parameters.Add("@RejectionReason", SqlDbType.NVarChar, -1).Value = reason;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RejectQuotation Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Cancel inquiry
        /// </summary>
        private bool CancelInquiry(int inquiryId)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spInquiry_Cancel", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@InquiryID", SqlDbType.Int).Value = inquiryId;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CancelInquiry Error: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Helper Methods for Display

        /// <summary>
        /// Get CSS class for status badge - PUBLIC for use in ASPX
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
                case "quotation-sent":
                    return "quotation";
                case "approved":
                    return "approved";
                case "rejected":
                    return "rejected";
                case "completed":
                    return "completed";
                case "cancelled":
                    return "cancelled";
                default:
                    return "pending";
            }
        }

        /// <summary>
        /// Show inspector information if assigned
        /// </summary>
        private string ShowInspectorInfo(object inspectorName, object assignedAt)
        {
            if (inspectorName == null || inspectorName == DBNull.Value)
                return string.Empty;

            string name = inspectorName.ToString();
            string dateStr = assignedAt != DBNull.Value
                ? Convert.ToDateTime(assignedAt).ToString("MMM dd, yyyy")
                : "N/A";

            return $@"
                <div class='info-item'>
                    <span class='info-label'>Assigned Inspector</span>
                    <span class='info-value'>
                        <i class='fas fa-user-tie'></i>
                        {name}
                    </span>
                </div>
                <div class='info-item'>
                    <span class='info-label'>Assigned On</span>
                    <span class='info-value'>
                        <i class='fas fa-calendar-check'></i>
                        {dateStr}
                    </span>
                </div>
            ";
        }

        /// <summary>
        /// Show uploaded images
        /// </summary>
        private string ShowImages(object imagePaths)
        {
            if (imagePaths == null || imagePaths == DBNull.Value)
                return string.Empty;

            string paths = imagePaths.ToString();
            if (string.IsNullOrEmpty(paths))
                return string.Empty;

            var images = paths.Split(',');
            var sb = new StringBuilder();

            sb.Append("<div class='images-section'>");
            sb.Append("<div class='info-label' style='margin-bottom: 12px;'>UPLOADED IMAGES</div>");
            sb.Append("<div class='images-grid'>");

            foreach (var img in images)
            {
                if (!string.IsNullOrWhiteSpace(img))
                {
                    string imgUrl = ResolveUrl(img.Trim());
                    sb.Append($@"
                        <div class='image-thumbnail' onclick='openImageModal(""{imgUrl}"")'>
                            <img src='{imgUrl}' alt='Inspection Image' />
                        </div>
                    ");
                }
            }

            sb.Append("</div></div>");
            return sb.ToString();
        }

        /// <summary>
        /// Show quotation information if available
        /// </summary>
        private string ShowQuotationInfo(object amount, object details, object sentAt)
        {
            if (amount == null || amount == DBNull.Value)
                return string.Empty;

            decimal quotationAmount = Convert.ToDecimal(amount);
            string quotationDetails = details != DBNull.Value ? details.ToString() : "No details provided";
            string sentDate = sentAt != DBNull.Value
                ? Convert.ToDateTime(sentAt).ToString("MMMM dd, yyyy")
                : "N/A";

            return $@"
                <div class='problem-description' style='background: #fef3c7; border-left-color: #f59e0b;'>
                    <div class='label' style='color: #92400e;'>
                        <i class='fas fa-file-invoice-dollar'></i> QUOTATION RECEIVED
                    </div>
                    <div class='card-body' style='margin-top: 12px; margin-bottom: 0;'>
                        <div class='info-item'>
                            <span class='info-label'>Amount</span>
                            <span class='info-value' style='font-size: 24px; font-weight: 700; color: #059669;'>
                                ₱{quotationAmount:N2}
                            </span>
                        </div>
                        <div class='info-item'>
                            <span class='info-label'>Sent On</span>
                            <span class='info-value'>
                                <i class='fas fa-calendar'></i>
                                {sentDate}
                            </span>
                        </div>
                    </div>
                    <div class='text' style='margin-top: 12px; color: #92400e;'>
                        <strong>Details:</strong> {quotationDetails}
                    </div>
                </div>
            ";
        }

        /// <summary>
        /// Show timeline based on status
        /// </summary>
        private string ShowTimeline(object status, object createdAt, object assignedAt, object inspectedAt, object quotationAt)
        {
            var sb = new StringBuilder();
            sb.Append("<div class='timeline'>");
            sb.Append("<div class='timeline-title'>📍 STATUS TIMELINE</div>");

            // Created
            if (createdAt != DBNull.Value)
            {
                sb.Append($@"
                    <div class='timeline-item'>
                        <div class='timeline-dot'><i class='fas fa-check'></i></div>
                        <div class='timeline-content'>
                            <div class='timeline-status'>Request Submitted</div>
                            <div class='timeline-date'>{Convert.ToDateTime(createdAt):MMM dd, yyyy hh:mm tt}</div>
                        </div>
                    </div>
                ");
            }

            // Assigned
            if (assignedAt != null && assignedAt != DBNull.Value)
            {
                sb.Append($@"
                    <div class='timeline-item'>
                        <div class='timeline-dot'><i class='fas fa-user-check'></i></div>
                        <div class='timeline-content'>
                            <div class='timeline-status'>Inspector Assigned</div>
                            <div class='timeline-date'>{Convert.ToDateTime(assignedAt):MMM dd, yyyy hh:mm tt}</div>
                        </div>
                    </div>
                ");
            }

            // Inspected
            if (inspectedAt != null && inspectedAt != DBNull.Value)
            {
                sb.Append($@"
                    <div class='timeline-item'>
                        <div class='timeline-dot'><i class='fas fa-clipboard-check'></i></div>
                        <div class='timeline-content'>
                            <div class='timeline-status'>Inspection Completed</div>
                            <div class='timeline-date'>{Convert.ToDateTime(inspectedAt):MMM dd, yyyy hh:mm tt}</div>
                        </div>
                    </div>
                ");
            }

            // Quotation
            if (quotationAt != null && quotationAt != DBNull.Value)
            {
                sb.Append($@"
                    <div class='timeline-item'>
                        <div class='timeline-dot'><i class='fas fa-file-invoice-dollar'></i></div>
                        <div class='timeline-content'>
                            <div class='timeline-status'>Quotation Sent</div>
                            <div class='timeline-date'>{Convert.ToDateTime(quotationAt):MMM dd, yyyy hh:mm tt}</div>
                        </div>
                    </div>
                ");
            }

            sb.Append("</div>");
            return sb.ToString();
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
                    confirmButtonColor: '#3b82f6'
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