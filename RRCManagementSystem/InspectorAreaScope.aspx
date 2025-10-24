<%@ Page Title="Inspector Area Scope Management" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="InspectorAreaScope.aspx.cs" Inherits="RRCManagementSystem.InspectorAreaScope" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <style>
        /* Main Card Styling */
        .area-scope-card {
            border-radius: 15px;
            border: none;
            box-shadow: 0 5px 20px rgba(0, 0, 0, 0.1);
            background: #fff;
            animation: fadeIn 0.5s ease;
        }

        @keyframes fadeIn {
            from {
                opacity: 0;
                transform: translateY(20px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }

        .card-header-custom {
            background: linear-gradient(135deg, #4169E1 0%, #2952CC 100%);
            color: white;
            padding: 20px;
            border-radius: 15px 15px 0 0;
            border: none;
        }

        .card-header-custom h2 {
            margin: 0;
            font-size: 1.8rem;
            font-weight: 600;
            display: flex;
            align-items: center;
            gap: 12px;
        }

        .card-body-custom {
            padding: 30px;
        }

        /* Section Divider */
        .section-divider {
            border-top: 2px solid #e9ecef;
            margin: 30px 0;
        }

        .section-title {
            font-size: 1.1rem;
            font-weight: 600;
            color: #2d2d2d;
            margin-bottom: 20px;
            display: flex;
            align-items: center;
            gap: 10px;
        }

        .section-title i {
            color: #4169E1;
        }

        /* Form Styling */
        .form-label {
            display: block;
            font-weight: 600;
            margin-bottom: 8px;
            color: #2d2d2d;
            font-size: 0.9rem;
        }

        .form-label i {
            margin-right: 6px;
            color: #4169E1;
            font-size: 0.85rem;
        }

        .form-select,
        .form-control {
            padding: 10px 12px;
            border: 2px solid #e0e0e0;
            border-radius: 8px;
            font-size: 0.9rem;
            transition: all 0.3s ease;
            background-color: #fff;
        }

        .form-select:focus,
        .form-control:focus {
            border-color: #4169E1;
            box-shadow: 0 0 0 0.2rem rgba(25, 135, 84, 0.15);
            outline: none;
        }

        /* City Checkboxes Container */
        .city-checkboxes-container {
            height: 180px;
            overflow-y: auto;
            background-color: #f8f9fa;
            border: 2px solid #e0e0e0;
            border-radius: 8px;
            padding: 12px;
        }

        .city-checkboxes-container::-webkit-scrollbar {
            width: 6px;
        }

        .city-checkboxes-container::-webkit-scrollbar-track {
            background: #f1f1f1;
            border-radius: 10px;
        }

        .city-checkboxes-container::-webkit-scrollbar-thumb {
            background:#4169E1;
            border-radius: 10px;
        }

        .city-checkboxes-container .form-check {
            margin-bottom: 8px;
        }

        .city-checkboxes-container .form-check-input {
            width: 18px;
            height: 18px;
            cursor: pointer;
            accent-color: #4169E1;
        }

        .city-checkboxes-container .form-check-label {
            font-size: 0.9rem;
            color: #2d2d2d;
            cursor: pointer;
            margin-left: 8px;
        }

        /* Add Button */
        .btn-add {
            padding: 12px 32px;
            background: linear-gradient(135deg, #4169E1 0%, #2952CC 100%);
            color: white;
            border: none;
            border-radius: 8px;
            font-size: 1rem;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s ease;
            box-shadow: 0 4px 15px rgba(25, 135, 84, 0.3);
            display: inline-flex;
            align-items: center;
            gap: 8px;
        }

        .btn-add:hover {
            background: linear-gradient(135deg,#2952CC 0%, #0f5132 100%);
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(25, 135, 84, 0.4);
        }

        /* Table Styling */
        .table-container {
            border-radius: 10px;
            overflow: hidden;
            box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
        }

        .table {
            margin-bottom: 0;
        }

        .table thead {
            background: linear-gradient(135deg, #2d2d2d 0%, #1a1a1a 100%);
            color: white;
        }

        .table thead th {
            font-weight: 600;
            text-transform: uppercase;
            font-size: 0.8rem;
            letter-spacing: 0.5px;
            padding: 12px;
            border: none;
            vertical-align: middle;
        }

        .table tbody tr {
            transition: all 0.3s ease;
            border-bottom: 1px solid #e9ecef;
        }

        .table tbody tr:hover {
            background-color: #f8f9fa;
        }

        .table tbody td {
            padding: 12px;
            vertical-align: middle;
            border-right: 1px solid #e9ecef;
            font-size: 0.9rem;
        }

        .table tbody tr > td:last-child {
            border-right: none;
        }

        /* Remove Button */
        .btn-remove {
            color: #dc3545;
            text-decoration: none;
            font-weight: 600;
            font-size: 0.875rem;
            transition: all 0.3s ease;
            display: inline-flex;
            align-items: center;
            gap: 6px;
        }

        .btn-remove:hover {
            color: #bd2130;
            transform: scale(1.05);
        }

        /* Pagination Styling */
        .custom-pagination {
            display: flex;
            justify-content: center;
            align-items: center;
            gap: 8px;
            padding: 15px;
            background: #f8f9fa;
            border-top: 1px solid #e9ecef;
        }

        .pagination-button {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            min-width: 36px;
            height: 36px;
            padding: 8px;
            font-weight: 500;
            font-size: 0.875rem;
            color: #2d2d2d;
            background-color: white;
            border: 1px solid #dee2e6;
            border-radius: 6px;
            cursor: pointer;
            transition: all 0.3s ease;
            text-decoration: none;
        }

        .pagination-button:hover:not(:disabled) {
            background-color: #4169E1;
            color: white;
            border-color: #4169E1;
            transform: translateY(-2px);
            box-shadow: 0 2px 8px rgba(25, 135, 84, 0.3);
        }

        .pagination-button:disabled {
            opacity: 0.4;
            cursor: not-allowed;
        }

        .pagination-info {
            font-size: 0.875rem;
            color: #6c757d;
            font-weight: 500;
            padding: 0 12px;
        }

        /* Validation Messages */
        .text-danger {
            display: block;
            margin-top: 6px;
            font-size: 0.85rem;
            color: #dc3545;
        }

        .validation-summary {
            color: #721c24;
            margin-top: 20px;
            padding: 12px 16px;
            border: 1px solid #f5c2c7;
            border-radius: 8px;
            background: linear-gradient(135deg, #f8d7da 0%, #f5c6cb 100%);
            border-left: 4px solid #dc3545;
        }

        /* Alert Messages */
        .alert-success {
            background: linear-gradient(135deg, #d4edda 0%, #c3e6cb 100%);
            color: #155724;
            padding: 12px 16px;
            border-radius: 8px;
            margin-bottom: 20px;
            border-left: 4px solid #28a745;
            display: flex;
            align-items: center;
            gap: 10px;
        }

        .alert-danger {
            background: linear-gradient(135deg, #f8d7da 0%, #f5c6cb 100%);
            color: #721c24;
            padding: 12px 16px;
            border-radius: 8px;
            margin-bottom: 20px;
            border-left: 4px solid #dc3545;
            display: flex;
            align-items: center;
            gap: 10px;
        }

        /* Breadcrumb */
        .breadcrumb {
            background: transparent;
            padding: 0;
            margin-bottom: 0;
        }

        .breadcrumb-item a {
            color: #4169E1;
            text-decoration: none;
            transition: color 0.3s ease;
        }

        .breadcrumb-item a:hover {
            color: #2952CC;
            text-decoration: underline;
        }

        .breadcrumb-item.active {
            color: #6c757d;
        }

        .breadcrumb-item + .breadcrumb-item::before {
            content: "›";
            color: #6c757d;
        }

        /* Responsive Design */
        @media (max-width: 992px) {
            .card-body-custom {
                padding: 20px;
            }

            .table thead th,
            .table tbody td {
                padding: 10px 8px;
                font-size: 0.85rem;
            }
        }

        @media (max-width: 768px) {
            .card-header-custom {
                padding: 15px;
            }

            .card-header-custom h2 {
                font-size: 1.4rem;
            }

            .card-body-custom {
                padding: 15px;
            }

            .city-checkboxes-container {
                height: 150px;
            }

            .btn-add {
                width: 100%;
                justify-content: center;
            }

            .table thead th,
            .table tbody td {
                font-size: 0.8rem;
                padding: 8px 6px;
            }
        }

        @media (max-width: 576px) {
            .card-header-custom h2 {
                font-size: 1.2rem;
            }

            .section-title {
                font-size: 1rem;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <!-- Breadcrumb Navigation -->
        <div class="row mb-4">
            <div class="col-12">
                <nav aria-label="breadcrumb">
                    <ol class="breadcrumb">
                        <li class="breadcrumb-item"><a href="SuperAdminDashboard.aspx"><i class="fas fa-home me-1"></i>Dashboard</a></li>
                        <li class="breadcrumb-item active" aria-current="page">Inspector Area Scope</li>
                    </ol>
                </nav>
            </div>
        </div>

        <!-- Main Card -->
        <div class="card area-scope-card">
            <!-- Card Header -->
            <div class="card-header-custom">
                <h2>
                    <i class="fas fa-map-marked-alt"></i>
                    Inspector Area Scope Management
                </h2>
            </div>

            <!-- Card Body -->
            <div class="card-body-custom">
                <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <!-- Add New Area Scope Section -->
                        <div class="section-title">
                            <i class="fas fa-plus-circle"></i>
                            Add New Area Scope
                        </div>

                        <asp:Literal ID="litMessage" runat="server" EnableViewState="False" />

                        <div class="row g-3">
                            <!-- Inspector Selection -->
                            <div class="col-12 col-lg-4">
                                <label for="<%= ddlInspector.ClientID %>" class="form-label">
                                    <i class="fas fa-user-shield"></i>Inspector
                                </label>
                                <asp:DropDownList ID="ddlInspector" runat="server" CssClass="form-select">
                                    <asp:ListItem Text="-- Select Inspector --" Value=""></asp:ListItem>
                                </asp:DropDownList>
                                <asp:RequiredFieldValidator 
                                    ID="rfvInspector" 
                                    runat="server" 
                                    ControlToValidate="ddlInspector" 
                                    InitialValue="" 
                                    ErrorMessage="Inspector is required." 
                                    CssClass="text-danger" 
                                    Display="Dynamic" 
                                    ValidationGroup="ScopeGroup" 
                                    EnableClientScript="False" />
                            </div>

                            <!-- Region Selection -->
                            <div class="col-12 col-lg-4">
                                <label for="<%= ddlRegion.ClientID %>" class="form-label">
                                    <i class="fas fa-globe-asia"></i>Region
                                </label>
                                <asp:DropDownList 
                                    ID="ddlRegion" 
                                    runat="server" 
                                    AutoPostBack="true"
                                    OnSelectedIndexChanged="ddlRegion_SelectedIndexChanged"
                                    CssClass="form-select">
                                    <asp:ListItem Text="-- Select Region --" Value=""></asp:ListItem>
                                    <asp:ListItem Text="NCR - National Capital Region" Value="NCR"></asp:ListItem>
                                    <asp:ListItem Text="Region I - Ilocos Region" Value="Region I"></asp:ListItem>
                                    <asp:ListItem Text="Region II - Cagayan Valley" Value="Region II"></asp:ListItem>
                                    <asp:ListItem Text="Region III - Central Luzon" Value="Region III"></asp:ListItem>
                                    <asp:ListItem Text="Region IV-A - CALABARZON" Value="Region IV-A"></asp:ListItem>
                                    <asp:ListItem Text="Region IV-B - MIMAROPA" Value="Region IV-B"></asp:ListItem>
                                    <asp:ListItem Text="Region V - Bicol Region" Value="Region V"></asp:ListItem>
                                    <asp:ListItem Text="Region VI - Western Visayas" Value="Region VI"></asp:ListItem>
                                    <asp:ListItem Text="Region VII - Central Visayas" Value="Region VII"></asp:ListItem>
                                </asp:DropDownList>
                                <asp:RequiredFieldValidator 
                                    ID="rfvRegion" 
                                    runat="server" 
                                    ControlToValidate="ddlRegion" 
                                    InitialValue="" 
                                    ErrorMessage="Region is required." 
                                    CssClass="text-danger" 
                                    Display="Dynamic" 
                                    ValidationGroup="ScopeGroup" 
                                    EnableClientScript="False" />
                            </div>

                            <!-- City Selection -->
                            <div class="col-12 col-lg-4">
                                <label for="<%= pnlCityCheckboxes.ClientID %>" class="form-label">
                                    <i class="fas fa-city"></i>Cities
                                </label>
                                <div class="city-checkboxes-container">
                                    <asp:Panel ID="pnlCityCheckboxes" runat="server">
                                        <asp:Label 
                                            ID="lblCityPlaceholder" 
                                            runat="server" 
                                            Text="Please select a region first." 
                                            CssClass="text-muted" />
                                    </asp:Panel>
                                </div>
                                <asp:CustomValidator 
                                    ID="cvCityRequired" 
                                    runat="server" 
                                    ErrorMessage="At least one city must be selected." 
                                    CssClass="text-danger" 
                                    Display="Dynamic" 
                                    OnServerValidate="cvCityRequired_ServerValidate" 
                                    ValidationGroup="ScopeGroup" 
                                    EnableClientScript="False" />
                            </div>
                        </div>

                        <!-- Add Button -->
                        <div class="text-center mt-4">
                            <asp:Button 
                                ID="btnAddScope" 
                                runat="server" 
                                Text="Add Area Scope" 
                                OnClick="btnAddScope_Click"
                                CssClass="btn-add" 
                                ValidationGroup="ScopeGroup">
                            </asp:Button>
                        </div>

                        <asp:ValidationSummary 
                            ID="vsAreaScope" 
                            runat="server" 
                            ShowSummary="true" 
                            HeaderText="Please correct the following errors:" 
                            CssClass="validation-summary" 
                            ValidationGroup="ScopeGroup" 
                            EnableClientScript="False" />
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="ddlRegion" EventName="SelectedIndexChanged" />
                        <asp:AsyncPostBackTrigger ControlID="btnAddScope" EventName="Click" />
                    </Triggers>
                </asp:UpdatePanel>

                <!-- Section Divider -->
                <div class="section-divider"></div>

                <!-- Current Area Scopes Section -->
                <div class="section-title">
                    <i class="fas fa-list"></i>
                    Current Area Scopes
                </div>

                <asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <div class="table-container">
                            <div class="table-responsive">
                                <asp:GridView 
                                    ID="gvAreaScopes" 
                                    runat="server" 
                                    AutoGenerateColumns="False"
                                    DataKeyNames="AreaScopeID"
                                    AllowPaging="True" 
                                    PageSize="10" 
                                    OnPageIndexChanging="gvAreaScopes_PageIndexChanging"
                                    GridLines="None"
                                    CssClass="table table-hover mb-0"
                                    PagerSettings-Visible="false">
                                    <Columns>
                                        <asp:BoundField 
                                            DataField="InspectorName" 
                                            HeaderText="Inspector Name" 
                                            ItemStyle-CssClass="text-start" 
                                            HeaderStyle-CssClass="text-start" />
                                        
                                        <asp:BoundField 
                                            DataField="Region" 
                                            HeaderText="Region" 
                                            ItemStyle-CssClass="text-center" 
                                            HeaderStyle-CssClass="text-center" />
                                        
                                        <asp:BoundField 
                                            DataField="City" 
                                            HeaderText="City" 
                                            ItemStyle-CssClass="text-center" 
                                            HeaderStyle-CssClass="text-center" />

                                        <asp:BoundField 
                                            DataField="CreatedAt" 
                                            HeaderText="Assigned Date" 
                                            DataFormatString="{0:MMM dd, yyyy}" 
                                            ItemStyle-CssClass="text-center" 
                                            HeaderStyle-CssClass="text-center" />
                                        
                                        <asp:TemplateField 
                                            HeaderText="Action" 
                                            ItemStyle-CssClass="text-center" 
                                            HeaderStyle-CssClass="text-center">
                                            <ItemTemplate>
                                                <asp:LinkButton 
                                                    ID="btnDelete" 
                                                    runat="server" 
                                                    CssClass="btn-remove"
                                                    OnClientClick='<%# "return confirmDelete(" + Eval("AreaScopeID") + ", \u0027" + Eval("InspectorName") + "\u0027, \u0027" + Eval("City") + "\u0027);" %>'> 
                                                    <i class="fas fa-trash-alt"></i> Remove
                                                </asp:LinkButton>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                            </div>
                            
                            <asp:Panel ID="pnlCustomPagination" runat="server" CssClass="custom-pagination">
                                <asp:LinkButton 
                                    ID="btnFirstPage" 
                                    runat="server" 
                                    CssClass="pagination-button" 
                                    OnClick="btnFirstPage_Click" 
                                    ToolTip="First Page" 
                                    CausesValidation="false">
                                    <i class="fas fa-angle-double-left"></i>
                                </asp:LinkButton>
                                
                                <asp:LinkButton 
                                    ID="btnPrevPage" 
                                    runat="server" 
                                    CssClass="pagination-button" 
                                    OnClick="btnPrevPage_Click" 
                                    ToolTip="Previous Page" 
                                    CausesValidation="false">
                                    <i class="fas fa-angle-left"></i>
                                </asp:LinkButton>
                                
                                <span class="pagination-info">
                                    <asp:Literal ID="litPageInfo" runat="server"></asp:Literal>
                                </span>
                                
                                <asp:LinkButton 
                                    ID="btnNextPage" 
                                    runat="server" 
                                    CssClass="pagination-button" 
                                    OnClick="btnNextPage_Click" 
                                    ToolTip="Next Page" 
                                    CausesValidation="false">
                                    <i class="fas fa-angle-right"></i>
                                </asp:LinkButton>
                                
                                <asp:LinkButton 
                                    ID="btnLastPage" 
                                    runat="server" 
                                    CssClass="pagination-button" 
                                    OnClick="btnLastPage_Click" 
                                    ToolTip="Last Page" 
                                    CausesValidation="false">
                                    <i class="fas fa-angle-double-right"></i>
                                </asp:LinkButton>
                            </asp:Panel>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>

    <!-- JavaScript -->
    <script type="text/javascript">
        // Confirm delete action
        function confirmDelete(areaScopeId, inspectorName, city) {
            Swal.fire({
                title: 'Remove Area Scope?',
                html: '<div style="text-align:center;"><i class="fas fa-exclamation-triangle" style="font-size:3rem;color:#dc3545;margin-bottom:15px;"></i><br/>Remove <strong style="color:#dc3545;">' + city + '</strong> from<br/><strong style="font-size:1.1rem;color:#4169E1;">' + inspectorName + '</strong>\'s area scope?</div>',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#dc3545',
                cancelButtonColor: '#6c757d',
                confirmButtonText: '<i class="fas fa-trash me-2"></i>Yes, Remove',
                cancelButtonText: '<i class="fas fa-times me-2"></i>Cancel',
                customClass: {
                    popup: 'animated-popup',
                    confirmButton: 'btn-confirm-custom',
                    cancelButton: 'btn-cancel-custom'
                }
            }).then((result) => {
                if (result.isConfirmed) {
                    // Show loading
                    Swal.fire({
                        title: 'Removing...',
                        html: 'Please wait while we remove the area scope.',
                        allowOutsideClick: false,
                        allowEscapeKey: false,
                        didOpen: () => {
                            Swal.showLoading();
                        }
                    });

                    __doPostBack('DeleteAreaScope', areaScopeId);
                }
            });
            return false;
        }
    </script>

    <style>
        /* SweetAlert Custom Styling */
        .animated-popup {
            animation: slideInDown 0.3s ease;
        }

        @keyframes slideInDown {
            from {
                transform: translateY(-50px);
                opacity: 0;
            }
            to {
                transform: translateY(0);
                opacity: 1;
            }
        }

        .btn-confirm-custom,
        .btn-cancel-custom {
            padding: 10px 24px !important;
            font-weight: 600 !important;
            border-radius: 8px !important;
        }
    </style>
</asp:Content>