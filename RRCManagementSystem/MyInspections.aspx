<%@ Page Title="My Inspections" Language="C#" MasterPageFile="~/Inspector.master" AutoEventWireup="true" CodeBehind="MyInspections.aspx.cs" Inherits="RRCManagementSystem.MyInspections" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" />
    
    <style>
        /* Card Styles */
        .inspection-card {
            background: white;
            border-radius: 12px;
            padding: 24px;
            margin-bottom: 20px;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
            border-left: 4px solid #3b82f6;
            transition: all 0.3s ease;
        }

        .inspection-card:hover {
            box-shadow: 0 4px 16px rgba(0, 0, 0, 0.15);
            transform: translateY(-2px);
        }

        .inspection-card.urgent {
            border-left-color: #ef4444;
            background: linear-gradient(135deg, #fff5f5 0%, #ffffff 100%);
        }

        .inspection-card.high {
            border-left-color: #f59e0b;
            background: linear-gradient(135deg, #fffbeb 0%, #ffffff 100%);
        }

        .inspection-card.draft {
            border-left-color: #f59e0b;
            background: linear-gradient(135deg, #fffbeb 0%, #ffffff 100%);
        }

        /* Status Badge */
        .status-badge {
            display: inline-flex;
            align-items: center;
            gap: 6px;
            padding: 6px 14px;
            border-radius: 20px;
            font-size: 12px;
            font-weight: 700;
            text-transform: uppercase;
        }

        .status-assigned { background: #dbeafe; color: #1e40af; }
        .status-in-progress { background: #fef3c7; color: #92400e; }
        .status-inspected { background: #d1fae5; color: #065f46; }
        .status-completed { background: #d1fae5; color: #047857; }
        .status-draft { background: #fef3c7; color: #92400e; }

        /* Info Grid */
        .info-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 16px;
            margin: 16px 0;
        }

        .info-item {
            display: flex;
            align-items: center;
            gap: 10px;
        }

        .info-icon {
            width: 36px;
            height: 36px;
            background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
            border-radius: 8px;
            display: flex;
            align-items: center;
            justify-content: center;
            color: white;
            font-size: 16px;
        }

        .info-content {
            flex: 1;
        }

        .info-label {
            font-size: 12px;
            color: #6b7280;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }

        .info-value {
            font-size: 14px;
            color: #1f2937;
            font-weight: 600;
        }

        /* Filter Tabs */
        .filter-tabs {
            display: flex;
            gap: 12px;
            margin-bottom: 24px;
            flex-wrap: wrap;
        }

        .filter-tab {
            padding: 12px 24px;
            border-radius: 10px;
            background: white;
            border: 2px solid #e5e7eb;
            color: #6b7280;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s ease;
        }

        .filter-tab:hover {
            border-color: #3b82f6;
            color: #3b82f6;
        }

        .filter-tab.active {
            background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
            color: white;
            border-color: #3b82f6;
        }

        /* Action Buttons */
        .btn-primary {
            background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
            color: white;
            padding: 10px 20px;
            border-radius: 8px;
            border: none;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s ease;
            display: inline-flex;
            align-items: center;
            gap: 8px;
        }

        .btn-primary:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(59, 130, 246, 0.4);
        }

        .btn-success {
            background: linear-gradient(135deg, #10b981 0%, #059669 100%);
            color: white;
            padding: 10px 20px;
            border-radius: 8px;
            border: none;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s ease;
            display: inline-flex;
            align-items: center;
            gap: 8px;
        }

        .btn-success:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(16, 185, 129, 0.4);
        }

        .btn-warning {
            background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
            color: white;
            padding: 10px 20px;
            border-radius: 8px;
            border: none;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s ease;
            display: inline-flex;
            align-items: center;
            gap: 8px;
        }

        .btn-warning:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(245, 158, 11, 0.4);
        }

        /* Empty State */
        .empty-state {
            text-align: center;
            padding: 60px 20px;
            color: #9ca3af;
        }

        .empty-state i {
            font-size: 64px;
            margin-bottom: 16px;
            opacity: 0.5;
        }

        /* Client Info Card */
        .client-info {
            background: #f9fafb;
            border-radius: 10px;
            padding: 16px;
            margin: 16px 0;
            border: 1px solid #e5e7eb;
        }

        .client-info h4 {
            margin: 0 0 12px 0;
            color: #1f2937;
            font-size: 14px;
            font-weight: 700;
            display: flex;
            align-items: center;
            gap: 8px;
        }

        .client-detail {
            display: flex;
            align-items: center;
            gap: 8px;
            margin-bottom: 8px;
            font-size: 13px;
            color: #4b5563;
        }

        .client-detail i {
            width: 20px;
            color: #3b82f6;
        }

        /* Images Preview */
        .images-preview {
            display: flex;
            gap: 8px;
            flex-wrap: wrap;
            margin-top: 12px;
        }

        .images-preview img {
            width: 80px;
            height: 80px;
            object-fit: cover;
            border-radius: 8px;
            cursor: pointer;
            transition: transform 0.2s ease;
        }

        .images-preview img:hover {
            transform: scale(1.05);
        }

        /* Search Bar */
        .search-container {
            position: relative;
            margin-bottom: 24px;
        }

        .search-icon {
            position: absolute;
            left: 16px;
            top: 50%;
            transform: translateY(-50%);
            color: #9ca3af;
            font-size: 18px;
        }

        @media (max-width: 768px) {
            .info-grid {
                grid-template-columns: 1fr;
            }
            
            .filter-tabs {
                overflow-x: auto;
                flex-wrap: nowrap;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto px-4 py-6">
        <!-- Page Header -->
        <div class="mb-8">
            <h1 class="text-3xl font-bold text-gray-800 mb-2">
                <i class="fas fa-clipboard-check text-blue-600"></i>
                My Inspections
            </h1>
            <p class="text-gray-600">Manage and complete your assigned inspection tasks</p>
        </div>

        <!-- Search Bar -->
        <div class="search-container">
            <i class="fas fa-search search-icon"></i>
            <asp:TextBox ID="txtSearch" runat="server" 
                CssClass="w-full pl-12 pr-4 py-3 border-2 border-gray-300 rounded-lg focus:border-blue-500 focus:outline-none transition-colors"
                placeholder="Search by Inquiry #, Client Name, Pest Type, or Address..."
                onkeyup="autoSearch()"
                AutoCompleteType="Disabled" />
        </div>

        <!-- Filter Tabs -->
        <div class="filter-tabs">
            <asp:Button ID="btnFilterAll" runat="server" Text="📋 All" CssClass="filter-tab active" OnClick="FilterInspections" />
            <asp:Button ID="btnFilterAssigned" runat="server" Text="🆕 Assigned" CssClass="filter-tab" OnClick="FilterInspections" />
            <asp:Button ID="btnFilterInProgress" runat="server" Text="🔄 In Progress" CssClass="filter-tab" OnClick="FilterInspections" />
            <asp:Button ID="btnFilterInspected" runat="server" Text="✅ Inspected" CssClass="filter-tab" OnClick="FilterInspections" />
        </div>

        <!-- Inspections List -->
        <asp:UpdatePanel ID="upInspections" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:Repeater ID="rptInspections" runat="server" OnItemCommand="rptInspections_ItemCommand">
                    <ItemTemplate>
                        <div class="inspection-card <%# GetUrgencyClass(Eval("Urgency")) %> <%# IsReportDraft(Eval("ReportStatus")) ? "draft" : "" %>">
                            <!-- Header -->
                            <div class="flex justify-between items-start mb-4">
                                <div>
                                    <h3 class="text-xl font-bold text-gray-800 mb-2">
                                        <%# Eval("InquiryNumber") %>
                                    </h3>
                                    <%# RenderStatusBadges(Eval("Status"), Eval("ReportStatus"), Eval("QuotationCode")) %>
                                </div>
                                <div class="text-right">
                                    <span class="text-sm font-semibold text-gray-500">Assigned</span>
                                    <div class="text-sm text-gray-600">
                                        <%# Convert.ToDateTime(Eval("AssignedAt")).ToString("MMM dd, yyyy") %>
                                    </div>
                                </div>
                            </div>

                            <!-- Inspection Details -->
                            <div class="info-grid">
                                <div class="info-item">
                                    <div class="info-icon">
                                        <i class="fas fa-calendar"></i>
                                    </div>
                                    <div class="info-content">
                                        <div class="info-label">Inspection Date</div>
                                        <div class="info-value">
                                            <%# Convert.ToDateTime(Eval("InspectionDate")).ToString("MMMM dd, yyyy (dddd)") %>
                                        </div>
                                    </div>
                                </div>

                                <div class="info-item">
                                    <div class="info-icon">
                                        <i class="fas fa-clock"></i>
                                    </div>
                                    <div class="info-content">
                                        <div class="info-label">Time Slot</div>
                                        <div class="info-value"><%# Eval("InspectionTime") %></div>
                                    </div>
                                </div>

                                <div class="info-item">
                                    <div class="info-icon">
                                        <i class="fas fa-bug"></i>
                                    </div>
                                    <div class="info-content">
                                        <div class="info-label">Pest Type</div>
                                        <div class="info-value"><%# Eval("PestType") %></div>
                                    </div>
                                </div>

                                <div class="info-item">
                                    <div class="info-icon">
                                        <i class="fas fa-exclamation-triangle"></i>
                                    </div>
                                    <div class="info-content">
                                        <div class="info-label">Urgency</div>
                                        <div class="info-value"><%# Eval("Urgency") %></div>
                                    </div>
                                </div>
                            </div>

                            <!-- Client Information -->
                            <div class="client-info">
                                <h4>
                                    <i class="fas fa-user-circle"></i>
                                    Client Information
                                </h4>
                                <div class="client-detail">
                                    <i class="fas fa-user"></i>
                                    <strong>Name:</strong> <%# Eval("ClientName") %>
                                </div>
                                <div class="client-detail">
                                    <i class="fas fa-envelope"></i>
                                    <strong>Email:</strong> <%# Eval("ClientEmail") %>
                                </div>
                                <div class="client-detail">
                                    <i class="fas fa-phone"></i>
                                    <strong>Contact:</strong> <%# Eval("ClientContact") %>
                                </div>
                                <div class="client-detail">
                                    <i class="fas fa-map-marker-alt"></i>
                                    <strong>Address:</strong> <%# Eval("FullAddress") %>
                                </div>
                            </div>

                            <!-- Problem Description -->
                            <div class="mt-4">
                                <h4 class="text-sm font-bold text-gray-700 mb-2">
                                    <i class="fas fa-file-alt"></i> Problem Description
                                </h4>
                                <p class="text-sm text-gray-600 bg-gray-50 p-3 rounded-lg">
                                    <%# Eval("ProblemDescription") %>
                                </p>
                            </div>

                            <!-- Client Photos -->
                            <%# RenderClientPhotos(Eval("InspectionReportPath")) %>

                            <!-- Action Buttons -->
                            <div class="flex gap-3 mt-6">
                                <!-- Start Inspection (Only for Assigned) -->
                                <asp:Button ID="btnStartInspection" runat="server" 
                                    Text="🚀 Start Inspection" 
                                    CssClass="btn-primary"
                                    CommandName="StartInspection"
                                    CommandArgument='<%# Eval("InquiryID") %>'
                                    Visible='<%# Eval("Status").ToString() == "Assigned" && string.IsNullOrEmpty(Eval("ReportStatus")?.ToString()) %>' />

                                <!-- Create Report (For In Progress with no report) -->
                                <asp:Button ID="btnCreateReport" runat="server" 
                                    Text="📝 Create Report" 
                                    CssClass="btn-success"
                                    CommandName="CreateReport"
                                    CommandArgument='<%# Eval("InquiryID") %>'
                                    Visible='<%# Eval("Status").ToString() == "In Progress" && string.IsNullOrEmpty(Eval("ReportStatus")?.ToString()) %>' />

                                <!-- View Report -->
                                <asp:Button ID="btnViewReport" runat="server"
                                    Text="👁️ View Report"
                                    CssClass="btn-primary"
                                    CommandName="ViewReport"
                                    CommandArgument='<%# Eval("InquiryID") + "|" + Eval("ReportID") %>'
                                    Visible='<%# !string.IsNullOrEmpty(Eval("ReportStatus")?.ToString()) 
                                        && (Eval("ReportStatus").ToString().Trim().ToLower() == "inspected" 
                                            || Eval("ReportStatus").ToString().Trim().ToLower() == "submitted" 
                                            || Eval("ReportStatus").ToString().Trim().ToLower() == "completed"
                                            || Eval("ReportStatus").ToString().Trim().ToLower() == "approved") %>' />
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>

                <!-- Empty State -->
                <asp:Panel ID="pnlEmptyState" runat="server" CssClass="empty-state" Visible="false">
                    <i class="fas fa-clipboard-list"></i>
                    <h3 class="text-xl font-bold text-gray-600 mb-2">No Inspections Found</h3>
                    <p>You don't have any inspections matching the selected filter.</p>
                </asp:Panel>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="txtSearch" EventName="TextChanged" />
            </Triggers>
        </asp:UpdatePanel>
    </div>

    <script type="text/javascript">
        let searchTimeout;
        
        function autoSearch() {
            // Clear previous timeout
            clearTimeout(searchTimeout);
            
            // Set new timeout for 500ms (0.5 seconds after user stops typing)
            searchTimeout = setTimeout(function() {
                __doPostBack('<%= txtSearch.UniqueID %>', '');
            }, 500);
        }
    </script>
</asp:Content>