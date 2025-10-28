<%@ Page Title="Booking Details" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="BookingDetails.aspx.cs" Inherits="RRCManagementSystem.BookingDetails" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <style>
        * {
            box-sizing: border-box;
        }

        .detail-card {
            background: white;
            border-radius: 8px;
            box-shadow: 0 1px 3px rgba(0,0,0,0.1);
            padding: 1.5rem;
            margin-bottom: 1.5rem;
            border: 1px solid #e5e7eb;
        }

        .detail-row {
            display: flex;
            flex-direction: column;
            padding: 0.75rem 0;
            border-bottom: 1px solid #f3f4f6;
            gap: 0.25rem;
        }

        @media (min-width: 640px) {
            .detail-row {
                flex-direction: row;
                justify-content: space-between;
                align-items: center;
            }
        }

        .detail-row:last-child {
            border-bottom: none;
        }

        .detail-label {
            font-weight: 600;
            color: #374151;
            font-size: 0.875rem;
        }

        .detail-value {
            color: #1f2937;
            font-size: 0.875rem;
        }

        .status-badge {
            display: inline-block;
            padding: 0.25rem 0.75rem;
            border-radius: 9999px;
            font-size: 0.75rem;
            font-weight: 600;
        }

        .status-assigned {
            background-color: #dbeafe;
            color: #1e40af;
        }

        .status-pending {
            background-color: #fef3c7;
            color: #92400e;
        }

        .status-completed {
            background-color: #d1fae5;
            color: #065f46;
        }

        .summary-box {
            background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%);
            color: white;
            padding: 1.5rem;
            border-radius: 8px;
            text-align: center;
            box-shadow: 0 4px 6px rgba(37, 99, 235, 0.2);
        }

        .summary-box h3 {
            font-size: 2.5rem;
            margin: 0;
            font-weight: 700;
        }

        @media (max-width: 640px) {
            .summary-box h3 {
                font-size: 2rem;
            }
        }

        .summary-box p {
            margin: 0.5rem 0 0 0;
            opacity: 0.95;
            font-size: 0.875rem;
        }

        .grid-container {
            display: grid;
            grid-template-columns: 1fr;
            gap: 1.5rem;
        }

        @media (min-width: 1024px) {
            .grid-container {
                grid-template-columns: repeat(2, 1fr);
            }
        }

        .summary-grid {
            display: grid;
            grid-template-columns: 1fr;
            gap: 1rem;
        }

        @media (min-width: 640px) {
            .summary-grid {
                grid-template-columns: repeat(3, 1fr);
            }
        }

        .search-section {
            background: white;
            border-radius: 8px;
            box-shadow: 0 1px 3px rgba(0,0,0,0.1);
            border: 1px solid #e5e7eb;
            margin-bottom: 1.5rem;
            padding: 1.5rem;
        }

        .search-flex {
            display: flex;
            flex-direction: column;
            gap: 1rem;
        }

        @media (min-width: 640px) {
            .search-flex {
                flex-direction: row;
                align-items: flex-end;
            }
        }

        .search-input-group {
            flex: 1;
        }

        .search-input-group label {
            display: block;
            color: #374151;
            font-weight: 600;
            margin-bottom: 0.5rem;
            font-size: 0.875rem;
        }

        .search-input-group input {
            width: 100%;
            padding: 0.625rem 1rem;
            border: 1px solid #d1d5db;
            border-radius: 6px;
            font-size: 0.875rem;
        }

        .search-input-group input:focus {
            outline: none;
            border-color: #2563eb;
            box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
        }

        .btn-primary {
            padding: 0.625rem 1.5rem;
            background: #2563eb;
            color: white;
            font-weight: 600;
            border-radius: 6px;
            border: none;
            cursor: pointer;
            transition: all 0.2s;
            font-size: 0.875rem;
            white-space: nowrap;
        }

        .btn-primary:hover {
            background: #1e40af;
            box-shadow: 0 4px 6px rgba(37, 99, 235, 0.3);
        }

        .btn-secondary {
            padding: 0.625rem 1.5rem;
            background: #6b7280;
            color: white;
            font-weight: 600;
            border-radius: 6px;
            border: none;
            cursor: pointer;
            transition: all 0.2s;
            font-size: 0.875rem;
            white-space: nowrap;
        }

        .btn-secondary:hover {
            background: #4b5563;
        }

        .header-section {
            background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%);
            color: white;
            padding: 1.5rem;
            border-radius: 8px;
            margin-bottom: 1.5rem;
            box-shadow: 0 4px 6px rgba(37, 99, 235, 0.2);
        }

        .header-flex {
            display: flex;
            flex-direction: column;
            gap: 1rem;
        }

        @media (min-width: 768px) {
            .header-flex {
                flex-direction: row;
                align-items: center;
                justify-content: space-between;
            }
        }

        .header-title h1 {
            font-size: 1.5rem;
            font-weight: 700;
            margin: 0 0 0.5rem 0;
        }

        @media (min-width: 768px) {
            .header-title h1 {
                font-size: 2rem;
            }
        }

        .header-title p {
            margin: 0;
            opacity: 0.9;
            font-size: 0.875rem;
        }

        .header-code {
            text-align: left;
        }

        @media (min-width: 768px) {
            .header-code {
                text-align: right;
            }
        }

        .header-code-value {
            font-size: 2rem;
            font-weight: 700;
            margin: 0;
        }

        @media (min-width: 768px) {
            .header-code-value {
                font-size: 2.5rem;
            }
        }

        .header-code-subtitle {
            opacity: 0.9;
            font-size: 0.75rem;
            margin-top: 0.25rem;
        }

        .section-title {
            font-size: 1.25rem;
            font-weight: 700;
            color: #1e40af;
            margin-bottom: 1rem;
            display: flex;
            align-items: center;
            gap: 0.5rem;
        }

        .table-container {
            overflow-x: auto;
            -webkit-overflow-scrolling: touch;
        }

        table {
            width: 100%;
            border-collapse: collapse;
            font-size: 0.875rem;
        }

        thead {
            background: #f9fafb;
        }

        th {
            padding: 0.75rem 1rem;
            text-align: left;
            font-weight: 600;
            color: #374151;
            text-transform: uppercase;
            font-size: 0.75rem;
            letter-spacing: 0.05em;
            border-bottom: 2px solid #e5e7eb;
        }

        td {
            padding: 0.75rem 1rem;
            border-bottom: 1px solid #f3f4f6;
        }

        tbody tr:hover {
            background: #f9fafb;
        }

        .action-buttons {
            display: flex;
            flex-direction: column;
            gap: 1rem;
            margin-top: 1.5rem;
        }

        @media (min-width: 640px) {
            .action-buttons {
                flex-direction: row;
                justify-content: space-between;
            }
        }

        .no-data {
            color: #6b7280;
            font-style: italic;
            font-size: 0.875rem;
            padding: 1rem;
            text-align: center;
            background: #f9fafb;
            border-radius: 6px;
        }

        .container-responsive {
            max-width: 1280px;
            margin: 0 auto;
            padding: 1rem;
        }

        @media (min-width: 640px) {
            .container-responsive {
                padding: 2rem;
            }
        }

        @media print {
            .search-section,
            .action-buttons {
                display: none !important;
            }
            
            .detail-card {
                break-inside: avoid;
                box-shadow: none;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-responsive">
        
        <!-- Search Section -->
        <div class="search-section">
            <h2 class="section-title">
                <i class="fas fa-search"></i>Search Booking
            </h2>
            <div class="search-flex">
                <div class="search-input-group">
                    <label for="txtSearchBookingCode">Enter Booking Code:</label>
                    <asp:TextBox 
                        ID="txtSearchBookingCode" 
                        runat="server" 
                        placeholder="e.g., BK-2024-001">
                    </asp:TextBox>
                </div>
                <asp:Button 
                    ID="btnSearch" 
                    runat="server" 
                    Text="🔍 Search"
                    OnClick="btnSearch_Click"
                    CssClass="btn-primary" />
                <asp:Button 
                    ID="btnClear" 
                    runat="server" 
                    Text="🔄 Clear"
                    OnClick="btnClear_Click"
                    CssClass="btn-secondary" />
            </div>
        </div>

        <!-- Details Panel (Hidden initially) -->
        <asp:Panel ID="pnlDetails" runat="server" Visible="false">
        
        <!-- Header Section -->
        <div class="header-section">
            <div class="header-flex">
                <div class="header-title">
                    <h1>
                        <i class="fas fa-clipboard-list"></i> Booking Details
                    </h1>
                    <p>Complete information and resource assignments</p>
                </div>
                <div class="header-code">
                    <div class="header-code-value">
                        <asp:Label ID="lblBookingCode" runat="server" />
                    </div>
                    <asp:Label ID="lblOperationNumber" runat="server" CssClass="header-code-subtitle" Visible="false" />
                </div>
            </div>
        </div>

        <!-- Summary Cards -->
        <div class="summary-grid">
            <div class="summary-box">
                <h3><asp:Label ID="lblTeamCount" runat="server" Text="0" /></h3>
                <p><i class="fas fa-users"></i> Team Assigned</p>
            </div>
            <div class="summary-box">
                <h3><asp:Label ID="lblEquipmentCount" runat="server" Text="0" /></h3>
                <p><i class="fas fa-tools"></i> Equipment Items</p>
            </div>
            <div class="summary-box">
                <h3><asp:Label ID="lblChemicalCount" runat="server" Text="0" /></h3>
                <p><i class="fas fa-vial"></i> Chemicals</p>
            </div>
        </div>

        <!-- Main Content Grid -->
        <div class="grid-container">
            
            <!-- Booking Information -->
            <div class="detail-card">
                <h2 class="section-title">
                    <i class="fas fa-info-circle"></i>Booking Information
                </h2>
                <div class="detail-row">
                    <span class="detail-label">Booking Code:</span>
                    <span class="detail-value"><asp:Label ID="lblDetailBookingCode" runat="server" /></span>
                </div>
                <div class="detail-row">
                    <span class="detail-label">Service Type:</span>
                    <span class="detail-value"><asp:Label ID="lblServiceType" runat="server" /></span>
                </div>
                <div class="detail-row">
                    <span class="detail-label">Status:</span>
                    <span class="detail-value"><asp:Label ID="lblStatus" runat="server" CssClass="status-badge" /></span>
                </div>
                <div class="detail-row">
                    <span class="detail-label">Scheduled Date:</span>
                    <span class="detail-value"><asp:Label ID="lblScheduledDate" runat="server" /></span>
                </div>
                <div class="detail-row">
                    <span class="detail-label">Start Time:</span>
                    <span class="detail-value"><asp:Label ID="lblStartTime" runat="server" /></span>
                </div>
                <div class="detail-row">
                    <span class="detail-label">Square Meters:</span>
                    <span class="detail-value"><asp:Label ID="lblSquareMeters" runat="server" /></span>
                </div>
                <div class="detail-row">
                    <span class="detail-label">Total Cost:</span>
                    <span class="detail-value"><asp:Label ID="lblTotalCost" runat="server" /></span>
                </div>
                <div class="detail-row">
                    <span class="detail-label">Created At:</span>
                    <span class="detail-value"><asp:Label ID="lblCreatedAt" runat="server" /></span>
                </div>
            </div>

            <!-- Customer Information -->
            <div class="detail-card">
                <h2 class="section-title">
                    <i class="fas fa-user"></i>Customer Information
                </h2>
                <div class="detail-row">
                    <span class="detail-label">Name:</span>
                    <span class="detail-value"><asp:Label ID="lblCustomerName" runat="server" /></span>
                </div>
                <div class="detail-row">
                    <span class="detail-label">Email:</span>
                    <span class="detail-value"><asp:Label ID="lblCustomerEmail" runat="server" /></span>
                </div>
                <div class="detail-row">
                    <span class="detail-label">Phone:</span>
                    <span class="detail-value"><asp:Label ID="lblCustomerPhone" runat="server" /></span>
                </div>
                <div class="detail-row">
                    <span class="detail-label">Address:</span>
                    <span class="detail-value"><asp:Label ID="lblCustomerAddress" runat="server" /></span>
                </div>
            </div>

            <!-- Assigned Team -->
            <div class="detail-card">
                <h2 class="section-title">
                    <i class="fas fa-users"></i>Assigned Team
                </h2>
                <asp:Panel ID="pnlNoTeam" runat="server" Visible="false" CssClass="no-data">
                    No team assigned yet.
                </asp:Panel>
                <div class="table-container">
                    <asp:GridView ID="gvTeams" runat="server" AutoGenerateColumns="False" Visible="false">
                        <Columns>
                            <asp:BoundField DataField="TeamName" HeaderText="Team Name" />
                            <asp:BoundField DataField="TeamStatus" HeaderText="Status" />
                            <asp:BoundField DataField="TeamCreatedAt" HeaderText="Created At" DataFormatString="{0:MMM dd, yyyy}" />
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>

        <!-- Full Width Sections -->
        
        <!-- Assigned Equipment -->
        <div class="detail-card">
            <h2 class="section-title">
                <i class="fas fa-tools"></i>Assigned Equipment
            </h2>
            <asp:Panel ID="pnlNoEquipment" runat="server" Visible="false" CssClass="no-data">
                No equipment assigned.
            </asp:Panel>
            <div class="table-container">
                <asp:GridView ID="gvEquipment" runat="server" AutoGenerateColumns="False" Visible="false">
                    <Columns>
                        <asp:BoundField DataField="EquipmentID" HeaderText="Equipment ID" />
                        <asp:BoundField DataField="QuantityAssigned" HeaderText="Quantity" />
                        <asp:BoundField DataField="AssignedAt" HeaderText="Assigned At" DataFormatString="{0:MMM dd, yyyy hh:mm tt}" />
                    </Columns>
                </asp:GridView>
            </div>
        </div>

        <!-- Assigned Chemicals -->
        <div class="detail-card">
            <h2 class="section-title">
                <i class="fas fa-vial"></i>Assigned Chemicals
            </h2>
            <asp:Panel ID="pnlNoChemicals" runat="server" Visible="false" CssClass="no-data">
                No chemicals assigned.
            </asp:Panel>
            <div class="table-container">
                <asp:GridView ID="gvChemicals" runat="server" AutoGenerateColumns="False" Visible="false">
                    <Columns>
                        <asp:BoundField DataField="ItemID" HeaderText="Item ID" />
                        <asp:BoundField DataField="QuantityAssigned" HeaderText="Quantity" />
                        <asp:BoundField DataField="AssignedAt" HeaderText="Assigned At" DataFormatString="{0:MMM dd, yyyy hh:mm tt}" />
                    </Columns>
                </asp:GridView>
            </div>
        </div>

        <!-- Action Buttons -->
        <div class="action-buttons">
            <asp:Button ID="btnBack" runat="server" Text="← Back to Bookings" 
                OnClick="btnBack_Click"
                CssClass="btn-secondary" />
            
            <asp:Button ID="btnExportPdf" runat="server" Text="📄 Export to PDF" 
                OnClick="btnExportPdf_Click"
                CssClass="btn-primary" />
            
            <asp:Button ID="btnPrint" runat="server" Text="🖨️ Print Details" 
                OnClientClick="window.print(); return false;"
                CssClass="btn-primary" />
        </div>
        
        </asp:Panel>
        <!-- End of Details Panel -->

        <!-- Message Label -->
        <asp:Label ID="lblMessage" runat="server" Visible="false"
            CssClass="detail-card" />
    </div>
</asp:Content>