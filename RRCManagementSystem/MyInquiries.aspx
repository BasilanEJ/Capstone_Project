<%@ Page Title="My Inspections" Language="C#" MasterPageFile="~/Client.master" AutoEventWireup="true" CodeBehind="MyInquiries.aspx.cs" Inherits="RRCManagementSystem.MyInquiries" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    
    <style>
        /* Page Container */
        .inquiries-container {
            max-width: 1200px;
            margin: 0 auto;
            padding: 20px;
        }

        /* Page Header */
        .page-header {
            text-align: center;
            margin-bottom: 40px;
        }

        .page-title {
            font-size: 36px;
            font-weight: 800;
            background: linear-gradient(135deg, #3b82f6, #1e40af);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
            margin-bottom: 12px;
        }

        .page-subtitle {
            color: #6b7280;
            font-size: 16px;
        }

        /* Status Filter Tabs */
        .status-tabs {
            display: flex;
            gap: 12px;
            margin-bottom: 32px;
            flex-wrap: wrap;
            justify-content: center;
        }

        .status-tab {
            padding: 12px 24px;
            border-radius: 25px;
            background: white;
            border: 2px solid #e5e7eb;
            cursor: pointer;
            transition: all 0.3s ease;
            font-weight: 600;
            font-size: 14px;
            color: #6b7280;
            display: flex;
            align-items: center;
            gap: 8px;
        }

        .status-tab:hover {
            border-color: #3b82f6;
            background: #eff6ff;
            color: #1e40af;
        }

        .status-tab.active {
            background: linear-gradient(135deg, #3b82f6, #1e40af);
            border-color: #3b82f6;
            color: white;
        }

        .status-tab .badge {
            background: rgba(255, 255, 255, 0.3);
            padding: 2px 8px;
            border-radius: 12px;
            font-size: 12px;
            font-weight: 700;
        }

        .status-tab.active .badge {
            background: rgba(255, 255, 255, 0.3);
        }

        /* Inquiry Cards */
        .inquiry-card {
            background: white;
            border-radius: 16px;
            padding: 24px;
            margin-bottom: 20px;
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
            border: 1px solid #e5e7eb;
            transition: all 0.3s ease;
            position: relative;
            overflow: hidden;
        }

        .inquiry-card:hover {
            transform: translateY(-4px);
            box-shadow: 0 8px 30px rgba(0, 0, 0, 0.12);
        }

        .inquiry-card::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            width: 4px;
            height: 100%;
            background: var(--status-color, #6b7280);
        }

        /* Status Colors */
        .status-pending::before { --status-color: #f59e0b; }
        .status-validated::before { --status-color: #10b981; }
        .status-assigned::before { --status-color: #3b82f6; }
        .status-scheduled::before { --status-color: #8b5cf6; }
        .status-inspected::before { --status-color: #06b6d4; }
        .status-quotation::before { --status-color: #ec4899; }
        .status-approved::before { --status-color: #10b981; }
        .status-rejected::before { --status-color: #ef4444; }
        .status-completed::before { --status-color: #059669; }
        .status-cancelled::before { --status-color: #6b7280; }

        /* Card Header */
        .card-header {
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            margin-bottom: 20px;
            flex-wrap: wrap;
            gap: 12px;
        }

        .inquiry-number {
            font-size: 18px;
            font-weight: 700;
            color: #1f2937;
        }

        .status-badge {
            padding: 6px 16px;
            border-radius: 20px;
            font-size: 12px;
            font-weight: 700;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }

        .status-pending { background: #fef3c7; color: #92400e; }
        .status-validated { background: #d1fae5; color: #065f46; }
        .status-assigned { background: #dbeafe; color: #1e40af; }
        .status-scheduled { background: #ede9fe; color: #6b21a8; }
        .status-inspected { background: #cffafe; color: #155e75; }
        .status-quotation { background: #fce7f3; color: #9f1239; }
        .status-approved { background: #d1fae5; color: #065f46; }
        .status-rejected { background: #fee2e2; color: #991b1b; }
        .status-completed { background: #d1fae5; color: #047857; }
        .status-cancelled { background: #f3f4f6; color: #4b5563; }

        /* Card Body */
        .card-body {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 16px;
            margin-bottom: 20px;
        }

        .info-item {
            display: flex;
            flex-direction: column;
            gap: 4px;
        }

        .info-label {
            font-size: 12px;
            font-weight: 600;
            color: #6b7280;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }

        .info-value {
            font-size: 15px;
            color: #1f2937;
            font-weight: 500;
        }

        .info-value i {
            color: #3b82f6;
            margin-right: 6px;
        }

        /* Problem Description */
        .problem-description {
            background: #f9fafb;
            padding: 16px;
            border-radius: 10px;
            margin-bottom: 20px;
            border-left: 3px solid #3b82f6;
        }

        .problem-description .label {
            font-size: 12px;
            font-weight: 700;
            color: #6b7280;
            text-transform: uppercase;
            margin-bottom: 8px;
        }

        .problem-description .text {
            color: #374151;
            line-height: 1.6;
            font-size: 14px;
        }

        /* Images Section */
        .images-section {
            margin-bottom: 20px;
        }

        .images-grid {
            display: flex;
            gap: 12px;
            flex-wrap: wrap;
        }

        .image-thumbnail {
            width: 100px;
            height: 100px;
            border-radius: 8px;
            overflow: hidden;
            cursor: pointer;
            border: 2px solid #e5e7eb;
            transition: all 0.3s ease;
        }

        .image-thumbnail:hover {
            transform: scale(1.05);
            border-color: #3b82f6;
            box-shadow: 0 4px 12px rgba(59, 130, 246, 0.3);
        }

        .image-thumbnail img {
            width: 100%;
            height: 100%;
            object-fit: cover;
        }

        /* Card Actions */
        .card-actions {
            display: flex;
            gap: 12px;
            flex-wrap: wrap;
        }

        .btn {
            padding: 10px 20px;
            border-radius: 8px;
            font-size: 14px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s ease;
            border: none;
            display: inline-flex;
            align-items: center;
            gap: 8px;
        }

        .btn-primary {
            background: linear-gradient(135deg, #3b82f6, #1e40af);
            color: white;
        }

        .btn-primary:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(59, 130, 246, 0.4);
        }

        .btn-success {
            background: linear-gradient(135deg, #10b981, #059669);
            color: white;
        }

        .btn-success:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(16, 185, 129, 0.4);
        }

        .btn-danger {
            background: linear-gradient(135deg, #ef4444, #dc2626);
            color: white;
        }

        .btn-danger:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(239, 68, 68, 0.4);
        }

        .btn-secondary {
            background: #f3f4f6;
            color: #4b5563;
            border: 1px solid #e5e7eb;
        }

        .btn-secondary:hover {
            background: #e5e7eb;
        }

        /* Empty State */
        .empty-state {
            text-align: center;
            padding: 60px 20px;
            background: white;
            border-radius: 16px;
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
        }

        .empty-state-icon {
            font-size: 64px;
            color: #d1d5db;
            margin-bottom: 20px;
        }

        .empty-state-title {
            font-size: 24px;
            font-weight: 700;
            color: #1f2937;
            margin-bottom: 12px;
        }

        .empty-state-text {
            color: #6b7280;
            margin-bottom: 24px;
        }

        /* Timeline */
        .timeline {
            margin-top: 20px;
            padding: 20px;
            background: #f9fafb;
            border-radius: 12px;
        }

        .timeline-title {
            font-weight: 700;
            color: #1f2937;
            margin-bottom: 16px;
            font-size: 14px;
        }

        .timeline-item {
            display: flex;
            gap: 12px;
            margin-bottom: 16px;
            position: relative;
        }

        .timeline-item:not(:last-child)::after {
            content: '';
            position: absolute;
            left: 11px;
            top: 28px;
            width: 2px;
            height: calc(100% - 20px);
            background: #e5e7eb;
        }

        .timeline-dot {
            width: 24px;
            height: 24px;
            border-radius: 50%;
            background: #3b82f6;
            display: flex;
            align-items: center;
            justify-content: center;
            color: white;
            font-size: 12px;
            flex-shrink: 0;
        }

        .timeline-content {
            flex: 1;
        }

        .timeline-status {
            font-weight: 600;
            color: #1f2937;
            font-size: 14px;
        }

        .timeline-date {
            font-size: 12px;
            color: #6b7280;
        }

        /* Responsive */
        @media (max-width: 768px) {
            .page-title {
                font-size: 28px;
            }

            .card-body {
                grid-template-columns: 1fr;
            }

            .status-tabs {
                justify-content: flex-start;
                overflow-x: auto;
                padding-bottom: 8px;
            }

            .status-tab {
                flex-shrink: 0;
            }

            .card-actions {
                flex-direction: column;
            }

            .btn {
                width: 100%;
                justify-content: center;
            }
        }

        /* Loading Spinner */
        .loading-container {
            text-align: center;
            padding: 60px 20px;
        }

        .spinner {
            width: 48px;
            height: 48px;
            border: 4px solid #e5e7eb;
            border-top-color: #3b82f6;
            border-radius: 50%;
            animation: spin 0.8s linear infinite;
            margin: 0 auto 20px;
        }

        @keyframes spin {
            to { transform: rotate(360deg); }
        }

        /* Image Modal */
        .image-modal {
            display: none;
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(0, 0, 0, 0.9);
            z-index: 9999;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }

        .image-modal.show {
            display: flex;
        }

        .image-modal-content {
            max-width: 90%;
            max-height: 90%;
            border-radius: 12px;
        }

        .image-modal-close {
            position: absolute;
            top: 20px;
            right: 20px;
            background: white;
            color: #1f2937;
            width: 40px;
            height: 40px;
            border-radius: 50%;
            border: none;
            font-size: 24px;
            cursor: pointer;
            display: flex;
            align-items: center;
            justify-content: center;
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="inquiries-container">
        
        <div class="page-header">
            <h1 class="page-title">
                <i class="fas fa-search"></i> My Inspections
            </h1>
            <p class="page-subtitle">Track and manage your inspection requests</p>
        </div>

        <!-- Updated Status Tabs - Removed Completed -->
        <div class="status-tabs">
            <asp:LinkButton ID="btnAll" runat="server" CssClass="status-tab active" OnClick="FilterStatus_Click" CommandArgument="All">
                <i class="fas fa-list"></i>
                <span>All</span>
                <span class="badge"><asp:Label ID="lblCountAll" runat="server" Text="0"></asp:Label></span>
            </asp:LinkButton>

            <asp:LinkButton ID="btnPending" runat="server" CssClass="status-tab" OnClick="FilterStatus_Click" CommandArgument="Pending">
                <i class="fas fa-clock"></i>
                <span>Pending</span>
                <span class="badge"><asp:Label ID="lblCountPending" runat="server" Text="0"></asp:Label></span>
            </asp:LinkButton>

            <asp:LinkButton ID="btnAssigned" runat="server" CssClass="status-tab" OnClick="FilterStatus_Click" CommandArgument="Assigned">
                <i class="fas fa-user-check"></i>
                <span>Assigned</span>
                <span class="badge"><asp:Label ID="lblCountAssigned" runat="server" Text="0"></asp:Label></span>
            </asp:LinkButton>

            <asp:LinkButton ID="btnInspected" runat="server" CssClass="status-tab" OnClick="FilterStatus_Click" CommandArgument="Inspected">
                <i class="fas fa-clipboard-check"></i>
                <span>Inspected</span>
                <span class="badge"><asp:Label ID="lblCountInspected" runat="server" Text="0"></asp:Label></span>
            </asp:LinkButton>

            <asp:LinkButton ID="btnQuotation" runat="server" CssClass="status-tab" OnClick="FilterStatus_Click" CommandArgument="Quotation Sent">
                <i class="fas fa-file-invoice-dollar"></i>
                <span>Quotation</span>
                <span class="badge"><asp:Label ID="lblCountQuotation" runat="server" Text="0"></asp:Label></span>
            </asp:LinkButton>
        </div>

        <!-- Inquiries List -->
        <asp:UpdatePanel ID="upInquiries" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                
                <!-- Loading State -->
                <asp:Panel ID="pnlLoading" runat="server" Visible="false" CssClass="loading-container">
                    <div class="spinner"></div>
                    <p style="color: #6b7280;">Loading your inspections...</p>
                </asp:Panel>

                <!-- Empty State -->
                <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="empty-state">
                    <div class="empty-state-icon">📋</div>
                    <h3 class="empty-state-title">No Inspections Found</h3>
                    <p class="empty-state-text">You haven't booked any inspections yet. Start by scheduling your FREE inspection today!</p>
                    <a href="BookInspection.aspx" class="btn btn-primary">
                        <i class="fas fa-calendar-plus"></i>
                        Book Free Inspection
                    </a>
                </asp:Panel>

                <!-- Inquiries Repeater -->
                <asp:Repeater ID="rptInquiries" runat="server" OnItemDataBound="rptInquiries_ItemDataBound">
                    <ItemTemplate>
                        <div class="inquiry-card status-<%# GetStatusClass(Eval("Status")) %>">
                            <!-- Card Header -->
                            <div class="card-header">
                                <div class="inquiry-number">
                                    <i class="fas fa-hashtag"></i>
                                    <%# Eval("InquiryNumber") %>
                                </div>
                                <span class="status-badge status-<%# GetStatusClass(Eval("Status")) %>">
                                    <%# Eval("Status") %>
                                </span>
                            </div>

                            <!-- Card Body -->
                            <div class="card-body">
                                <div class="info-item">
                                    <span class="info-label">Inspection Date</span>
                                    <span class="info-value">
                                        <i class="fas fa-calendar"></i>
                                        <%# Convert.ToDateTime(Eval("InspectionDate")).ToString("MMM dd, yyyy (dddd)") %>
                                    </span>
                                </div>

                                <div class="info-item">
                                    <span class="info-label">Time Slot</span>
                                    <span class="info-value">
                                        <i class="fas fa-clock"></i>
                                        <%# Eval("InspectionTime") %>
                                    </span>
                                </div>

                                <div class="info-item">
                                    <span class="info-label">Pest Type</span>
                                    <span class="info-value">
                                        <i class="fas fa-bug"></i>
                                        <%# Eval("PestType") %>
                                    </span>
                                </div>

                                <div class="info-item">
                                    <span class="info-label">Urgency</span>
                                    <span class="info-value">
                                        <i class="fas fa-exclamation-circle"></i>
                                        <%# Eval("Urgency") %>
                                    </span>
                                </div>

                                <div class="info-item">
                                    <span class="info-label">Submitted On</span>
                                    <span class="info-value">
                                        <i class="fas fa-calendar-plus"></i>
                                        <%# Convert.ToDateTime(Eval("CreatedAt")).ToString("MMM dd, yyyy") %>
                                    </span>
                                </div>

                                <!-- Inspector Info Placeholder -->
                                <asp:PlaceHolder ID="phInspectorInfo" runat="server"></asp:PlaceHolder>
                            </div>

                            <!-- Problem Description -->
                            <div class="problem-description">
                                <div class="label">Problem Description</div>
                                <div class="text"><%# Eval("ProblemDescription") %></div>
                            </div>

                            <!-- Images Placeholder -->
                            <asp:PlaceHolder ID="phImages" runat="server"></asp:PlaceHolder>

                            <!-- Quotation Info Placeholder -->
                            <asp:PlaceHolder ID="phQuotationInfo" runat="server"></asp:PlaceHolder>

                            <!-- Timeline Placeholder -->
                            <asp:PlaceHolder ID="phTimeline" runat="server"></asp:PlaceHolder>

                            <!-- Card Actions -->
                            <div class="card-actions">
                                <asp:LinkButton ID="btnViewDetails" runat="server" 
                                    CssClass="btn btn-primary" 
                                    CommandArgument='<%# Eval("InquiryID") %>'
                                    OnClick="btnViewDetails_Click">
                                    <i class="fas fa-eye"></i> View Details
                                </asp:LinkButton>

                                <!-- Approval/Rejection Buttons Placeholder -->
                                <asp:PlaceHolder ID="phApprovalButtons" runat="server"></asp:PlaceHolder>

                                <!-- Cancel Button Placeholder -->
                                <asp:PlaceHolder ID="phCancelButton" runat="server"></asp:PlaceHolder>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>

            </ContentTemplate>
        </asp:UpdatePanel>

    </div>

    <!-- Image Modal -->
    <div id="imageModal" class="image-modal" onclick="closeImageModal()">
        <button class="image-modal-close" onclick="closeImageModal()">
            <i class="fas fa-times"></i>
        </button>
        <img id="modalImage" class="image-modal-content" src="" alt="Inspection Image" onclick="event.stopPropagation()">
    </div>

    <script>
        // Image Modal Functions
        function openImageModal(imageSrc) {
            document.getElementById('modalImage').src = imageSrc;
            document.getElementById('imageModal').classList.add('show');
            document.body.style.overflow = 'hidden';
        }

        function closeImageModal() {
            document.getElementById('imageModal').classList.remove('show');
            document.body.style.overflow = '';
        }

        // Close modal on Escape key
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                closeImageModal();
            }
        });
    </script>
</asp:Content>