<%@ Page Title="Inspection Report" Language="C#" MasterPageFile="~/Inspector.master" AutoEventWireup="true" CodeBehind="InspectorReport.aspx.cs" Inherits="RRCManagementSystem.InspectionReport" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" />
    
    <style>
        /* Container */
        .report-container {
            max-width: 1200px;
            margin: 0 auto;
            padding: 20px;
        }

        /* Form Sections */
        .form-section {
            background: white;
            border-radius: 16px;
            padding: 28px;
            margin-bottom: 24px;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
            border: 1px solid #e5e7eb;
        }

        .section-title {
            font-size: 20px;
            font-weight: 700;
            color: #1f2937;
            margin-bottom: 20px;
            padding-bottom: 12px;
            border-bottom: 2px solid #3b82f6;
            display: flex;
            align-items: center;
            gap: 10px;
        }

        .section-title i {
            color: #3b82f6;
            font-size: 22px;
        }

        /* Form Controls */
        .form-group {
            margin-bottom: 20px;
        }

        .form-label {
            display: block;
            font-size: 14px;
            font-weight: 600;
            color: #374151;
            margin-bottom: 8px;
        }

        .form-label .required {
            color: #ef4444;
            margin-left: 4px;
        }

        .form-control {
            width: 100%;
            padding: 12px 16px;
            border: 2px solid #e5e7eb;
            border-radius: 10px;
            font-size: 15px;
            transition: all 0.3s ease;
            background: #f9fafb;
        }

        .form-control:focus {
            outline: none;
            border-color: #3b82f6;
            background: white;
            box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
        }

        textarea.form-control {
            min-height: 120px;
            resize: vertical;
        }

        /* Client Info Card */
        .client-info-card {
            background: linear-gradient(135deg, #eff6ff 0%, #dbeafe 100%);
            border-radius: 12px;
            padding: 20px;
            margin-bottom: 24px;
            border-left: 4px solid #3b82f6;
        }

        .client-info-title {
            font-size: 16px;
            font-weight: 700;
            color: #1e40af;
            margin-bottom: 12px;
            display: flex;
            align-items: center;
            gap: 8px;
        }

        .client-detail {
            display: flex;
            align-items: center;
            gap: 10px;
            margin-bottom: 8px;
            font-size: 14px;
            color: #1e40af;
        }

        .client-detail i {
            width: 20px;
            color: #3b82f6;
        }

        /* Service Checkboxes */
        .service-item {
            display: flex;
            align-items: flex-start;
            padding: 16px;
            border: 2px solid #e5e7eb;
            border-radius: 12px;
            margin-bottom: 12px;
            transition: all 0.3s ease;
            background: #f9fafb;
        }

        .service-item:hover {
            border-color: #3b82f6;
            background: white;
            box-shadow: 0 2px 8px rgba(59, 130, 246, 0.1);
        }

        .service-item.selected {
            border-color: #3b82f6;
            background: #eff6ff;
        }

        .service-checkbox {
            width: 20px;
            height: 20px;
            margin-right: 12px;
            margin-top: 2px;
            cursor: pointer;
        }

        .service-info {
            flex: 1;
        }

        .service-name {
            font-size: 16px;
            font-weight: 600;
            color: #1f2937;
            margin-bottom: 4px;
        }

        .service-description {
            font-size: 13px;
            color: #6b7280;
            margin-bottom: 8px;
        }

        .service-price {
            font-size: 14px;
            font-weight: 700;
            color: #059669;
        }

        .sqm-input-container {
            margin-top: 12px;
            padding: 12px;
            background: white;
            border-radius: 8px;
            border: 1px solid #3b82f6;
            display: none;
        }

        .service-item.selected .sqm-input-container {
            display: block;
        }

        .sqm-input {
            width: 100%;
            max-width: 200px;
            padding: 8px 12px;
            border: 2px solid #e5e7eb;
            border-radius: 8px;
            font-size: 14px;
        }

        .sqm-input:focus {
            outline: none;
            border-color: #3b82f6;
        }

        .service-subtotal {
            margin-top: 8px;
            font-size: 14px;
            font-weight: 600;
            color: #3b82f6;
        }

        /* Travel Expense */
        .travel-info {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 16px 20px;
            background: #f0fdf4;
            border-radius: 10px;
            margin-top: 12px;
            border: 2px solid #10b981;
        }

        .travel-info .label {
            font-weight: 600;
            color: #065f46;
            display: flex;
            align-items: center;
            gap: 8px;
        }

        .travel-info .amount {
            font-size: 18px;
            font-weight: 700;
            color: #10b981;
        }

        /* Miscellaneous Expenses */
        .add-expense-form {
            display: grid;
            grid-template-columns: 1fr 200px 100px;
            gap: 12px;
            padding: 16px;
            background: #f9fafb;
            border-radius: 10px;
            border: 2px dashed #cbd5e1;
            margin-bottom: 16px;
        }

        @media (max-width: 768px) {
            .add-expense-form {
                grid-template-columns: 1fr;
            }
        }

        .expense-item {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 12px 16px;
            background: white;
            border: 1px solid #e5e7eb;
            border-radius: 8px;
            margin-bottom: 8px;
        }

        .expense-description {
            flex: 1;
            font-size: 14px;
            color: #1f2937;
            font-weight: 500;
        }

        .expense-amount {
            font-size: 15px;
            font-weight: 700;
            color: #059669;
            margin-right: 12px;
        }

        .btn-remove-expense {
            background: #ef4444;
            color: white;
            border: none;
            border-radius: 6px;
            padding: 6px 12px;
            cursor: pointer;
            font-size: 12px;
            transition: all 0.3s ease;
        }

        .btn-remove-expense:hover {
            background: #dc2626;
        }

        /* Cost Summary */
        .cost-summary {
            background: linear-gradient(135deg, #f0f9ff 0%, #e0f2fe 100%);
            border: 2px solid #3b82f6;
            border-radius: 12px;
            padding: 16px 20px;
            margin-top: 16px;
        }

        .cost-row {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 8px 0;
            font-size: 15px;
            color: #1f2937;
        }

        .cost-row.total {
            border-top: 2px solid #3b82f6;
            margin-top: 8px;
            padding-top: 12px;
        }

        .cost-label {
            font-weight: 600;
        }

        .cost-amount {
            font-weight: 700;
            color: #3b82f6;
            font-size: 16px;
        }

        .cost-row.total .cost-amount {
            font-size: 24px;
            color: #1e40af;
        }

        /* Grand Total Section */
        .grand-total-section {
            background: linear-gradient(135deg, #10b981 0%, #059669 100%);
            color: white;
            border-radius: 16px;
            padding: 24px;
            text-align: center;
            box-shadow: 0 4px 20px rgba(16, 185, 129, 0.3);
        }

        .grand-total-label {
            font-size: 18px;
            font-weight: 600;
            margin-bottom: 8px;
            opacity: 0.9;
        }

        .grand-total-amount {
            font-size: 42px;
            font-weight: 900;
            letter-spacing: -1px;
        }

        /* Photo Upload */
        .photo-upload-area {
            border: 2px dashed #cbd5e1;
            border-radius: 12px;
            padding: 32px;
            text-align: center;
            background: #f9fafb;
            transition: all 0.3s ease;
            cursor: pointer;
        }

        .photo-upload-area:hover {
            border-color: #3b82f6;
            background: #eff6ff;
        }

        .upload-icon {
            font-size: 48px;
            color: #94a3b8;
            margin-bottom: 12px;
        }

        .photo-preview-container {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(120px, 1fr));
            gap: 12px;
            margin-top: 16px;
        }

        .photo-preview-item {
            position: relative;
            width: 100%;
            padding-bottom: 100%;
            border-radius: 8px;
            overflow: hidden;
            border: 2px solid #e5e7eb;
        }

        .photo-preview-item img {
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            object-fit: cover;
        }

        .photo-remove-btn {
            position: absolute;
            top: 4px;
            right: 4px;
            background: #ef4444;
            color: white;
            border: none;
            border-radius: 50%;
            width: 28px;
            height: 28px;
            cursor: pointer;
            font-size: 14px;
            display: flex;
            align-items: center;
            justify-content: center;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
            transition: all 0.3s ease;
        }

        .photo-remove-btn:hover {
            background: #dc2626;
            transform: scale(1.1);
        }

        /* Buttons */
        .btn-submit {
            width: 100%;
            padding: 16px;
            background: linear-gradient(135deg, #10b981 0%, #059669 100%);
            color: white;
            border: none;
            border-radius: 12px;
            font-size: 16px;
            font-weight: 700;
            cursor: pointer;
            transition: all 0.3s ease;
            box-shadow: 0 4px 16px rgba(16, 185, 129, 0.3);
        }

        .btn-submit:hover:not(:disabled) {
            transform: translateY(-2px);
            box-shadow: 0 6px 24px rgba(16, 185, 129, 0.4);
        }

        .btn-submit:disabled {
            background: #9ca3af;
            cursor: not-allowed;
            box-shadow: none;
        }

        .btn-draft {
            width: 100%;
            padding: 16px;
            background: linear-gradient(135deg, #6b7280 0%, #4b5563 100%);
            color: white;
            border: none;
            border-radius: 12px;
            font-size: 16px;
            font-weight: 700;
            cursor: pointer;
            transition: all 0.3s ease;
            margin-bottom: 12px;
        }

        .btn-draft:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 16px rgba(107, 114, 128, 0.3);
        }

        /* Checkbox Group */
        .checkbox-group {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
            gap: 12px;
        }

        .checkbox-item {
            display: flex;
            align-items: center;
            gap: 8px;
            padding: 10px 14px;
            background: #f9fafb;
            border: 2px solid #e5e7eb;
            border-radius: 8px;
            cursor: pointer;
            transition: all 0.3s ease;
        }

        .checkbox-item:hover {
            border-color: #3b82f6;
            background: white;
        }

        .checkbox-item input[type="checkbox"] {
            width: 18px;
            height: 18px;
            cursor: pointer;
        }

        .checkbox-item label {
            cursor: pointer;
            font-size: 14px;
            color: #1f2937;
            font-weight: 500;
        }

        /* Radio Group */
        .radio-group {
            display: flex;
            flex-wrap: wrap;
            gap: 12px;
        }

        .radio-option {
            flex: 1;
            min-width: 140px;
        }

        .radio-option input[type="radio"] {
            display: none;
        }

        .radio-label {
            display: block;
            padding: 12px 16px;
            border: 2px solid #e5e7eb;
            border-radius: 10px;
            cursor: pointer;
            transition: all 0.3s ease;
            text-align: center;
            font-weight: 500;
            background: #f9fafb;
        }

        .radio-option input[type="radio"]:checked + .radio-label {
            border-color: #3b82f6;
            background: #eff6ff;
            color: #1e40af;
        }

        .radio-label:hover {
            border-color: #3b82f6;
            background: white;
        }

        /* Info Alert */
        .info-alert {
            background: #fef3c7;
            border-left: 4px solid #f59e0b;
            padding: 12px 16px;
            border-radius: 8px;
            margin-bottom: 20px;
            font-size: 14px;
            color: #92400e;
        }

        .info-alert i {
            margin-right: 8px;
        }
        
        .info-note {
            background: #dbeafe;
            border-left: 4px solid #3b82f6;
            padding: 12px 16px;
            border-radius: 8px;
            margin-top: 12px;
            font-size: 13px;
            color: #1e40af;
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="report-container">
        <!-- Page Header -->
        <div class="mb-6">
            <h1 class="text-3xl font-bold text-gray-800 mb-2">
                <i class="fas fa-file-medical-alt text-blue-600"></i>
                Inspection Report
            </h1>
            <p class="text-gray-600">Complete the inspection details and submit your findings</p>
        </div>

        <!-- Client Information -->
        <div class="client-info-card">
            <div class="client-info-title">
                <i class="fas fa-user-circle"></i>
                Client Information
            </div>
            <div class="client-detail">
                <i class="fas fa-hashtag"></i>
                <strong>Inquiry #:</strong>
                <asp:Label ID="lblInquiryNumber" runat="server" />
            </div>
            <div class="client-detail">
                <i class="fas fa-user"></i>
                <strong>Name:</strong>
                <asp:Label ID="lblClientName" runat="server" />
            </div>
            <div class="client-detail">
                <i class="fas fa-phone"></i>
                <strong>Contact:</strong>
                <asp:Label ID="lblClientContact" runat="server" />
            </div>
            <div class="client-detail">
                <i class="fas fa-map-marker-alt"></i>
                <strong>Address:</strong>
                <asp:Label ID="lblClientAddress" runat="server" />
            </div>
            <div class="client-detail">
                <i class="fas fa-bug"></i>
                <strong>Pest Type:</strong>
                <asp:Label ID="lblPestType" runat="server" />
            </div>
        </div>

        <asp:UpdatePanel ID="upReport" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <!-- Section 1: Inspection Findings -->
                <div class="form-section">
                    <div class="section-title">
                        <i class="fas fa-search"></i>
                        Inspection Findings
                    </div>

                    <div class="form-group">
                        <label class="form-label">
                            Infestation Level <span class="required">*</span>
                        </label>
                        <asp:DropDownList ID="ddlInfestationLevel" runat="server" CssClass="form-control">
                            <asp:ListItem Value="">-- Select Level --</asp:ListItem>
                            <asp:ListItem Value="None Detected">✅ None Detected</asp:ListItem>
                            <asp:ListItem Value="Low">🟢 Low (Minimal signs)</asp:ListItem>
                            <asp:ListItem Value="Moderate">🟡 Moderate (Visible activity)</asp:ListItem>
                            <asp:ListItem Value="Severe">🔴 Severe (Heavy infestation)</asp:ListItem>
                            <asp:ListItem Value="Critical">🚨 Critical (Structural damage)</asp:ListItem>
                        </asp:DropDownList>
                    </div>

                    <div class="form-group">
                        <label class="form-label">
                            Detailed Findings <span class="required">*</span>
                        </label>
                        <asp:TextBox ID="txtFindings" runat="server" TextMode="MultiLine" CssClass="form-control"
                            placeholder="Describe your observations, affected areas, severity, pest behavior, entry points, etc."
                            MaxLength="2000" />
                        <small class="text-gray-500">Maximum 2000 characters</small>
                    </div>

                    <div class="form-group">
                        <label class="form-label">
                            Affected Areas <span class="required">*</span>
                        </label>
                        <div class="checkbox-group">
                            <div class="checkbox-item">
                                <asp:CheckBox ID="chkLivingRoom" runat="server" />
                                <label for="<%= chkLivingRoom.ClientID %>">Living Room</label>
                            </div>
                            <div class="checkbox-item">
                                <asp:CheckBox ID="chkKitchen" runat="server" />
                                <label for="<%= chkKitchen.ClientID %>">Kitchen</label>
                            </div>
                            <div class="checkbox-item">
                                <asp:CheckBox ID="chkBedroom" runat="server" />
                                <label for="<%= chkBedroom.ClientID %>">Bedrooms</label>
                            </div>
                            <div class="checkbox-item">
                                <asp:CheckBox ID="chkBathroom" runat="server" />
                                <label for="<%= chkBathroom.ClientID %>">Bathroom</label>
                            </div>
                            <div class="checkbox-item">
                                <asp:CheckBox ID="chkGarage" runat="server" />
                                <label for="<%= chkGarage.ClientID %>">Garage</label>
                            </div>
                            <div class="checkbox-item">
                                <asp:CheckBox ID="chkAttic" runat="server" />
                                <label for="<%= chkAttic.ClientID %>">Attic</label>
                            </div>
                            <div class="checkbox-item">
                                <asp:CheckBox ID="chkBasement" runat="server" />
                                <label for="<%= chkBasement.ClientID %>">Basement</label>
                            </div>
                            <div class="checkbox-item">
                                <asp:CheckBox ID="chkGarden" runat="server" />
                                <label for="<%= chkGarden.ClientID %>">Garden/Outdoor</label>
                            </div>
                            <div class="checkbox-item">
                                <asp:CheckBox ID="chkWalls" runat="server" />
                                <label for="<%= chkWalls.ClientID %>">Walls/Foundation</label>
                            </div>
                            <div class="checkbox-item">
                                <asp:CheckBox ID="chkRoof" runat="server" />
                                <label for="<%= chkRoof.ClientID %>">Roof</label>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Section 2: Recommended Services -->
                <div class="form-section">
                    <div class="section-title">
                        <i class="fas fa-tools"></i>
                        Recommended Services
                    </div>

                    <div class="info-alert">
                        <i class="fas fa-info-circle"></i>
                        Select services and enter total area size. <strong>Bundle pricing applies based on area range.</strong>
                    </div>

                 <asp:Repeater ID="rptServiceTypes" runat="server">
    <ItemTemplate>
        <!-- Category Header -->
        <div style="margin-top: 30px;">
            <h2 style="font-size: 20px; font-weight: 700; color: #1e40af; border-bottom: 2px solid #3b82f6; padding-bottom: 8px;">
                <i class="fas fa-list"></i> <%# Eval("ServiceType") %>
            </h2>
        </div>

        <!-- Services under this category -->
       <asp:Repeater ID="rptServices" runat="server" DataSource='<%# Eval("Services") %>'>
    <ItemTemplate>
        <div class="service-item" id="service_<%# Eval("ServiceID") %>">
            <input type="checkbox" 
                class="service-checkbox" 
                id="chkService_<%# Eval("ServiceID") %>"
                data-service-id="<%# Eval("ServiceID") %>"
                data-service-name='<%# Eval("Name") %>'
                data-pricing-tiers='<%# System.Web.HttpUtility.HtmlEncode(Eval("PricingTiers").ToString()) %>'
                onchange="toggleServiceSQM(this)" />
                    
                    <div class="service-info">
                        <div class="service-name"><%# Eval("Name") %></div>
                        <div class="service-description"><%# Eval("Description") %></div>
                        
                        <div class="service-price">
                            <strong>📦 Package Pricing:</strong><br/>
                            <div style="font-size: 12px; color: #6b7280; margin-top: 4px;">
                                <%# RenderBundlePricing(Eval("PricingTiers")) %>
                            </div>
                        </div>
                        
                        <div class="sqm-input-container">
                            <label style="font-size: 13px; font-weight: 600; color: #374151; margin-bottom: 6px; display: block;">
                                Total Area Size (sqm):
                            </label>
                            <input type="number" 
                                class="sqm-input" 
                                id="sqm_<%# Eval("ServiceID") %>"
                                placeholder="Enter total sqm"
                                step="0.01"
                                min="0"
                                onkeyup="calculateServiceBundlePrice(<%# Eval("ServiceID") %>)"
                                onchange="calculateServiceBundlePrice(<%# Eval("ServiceID") %>)" />
                            
                            <div style="font-size: 12px; color: #3b82f6; margin-top: 6px; font-weight: 600;" 
                                 id="selectedPackage_<%# Eval("ServiceID") %>">
                                Select area size to see package
                            </div>
                            
                            <div class="service-subtotal" id="subtotal_<%# Eval("ServiceID") %>">
                                Total: ₱0.00
                            </div>
                        </div>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </ItemTemplate>
</asp:Repeater>


                    <div class="cost-summary">
                        <div class="cost-row">
                            <span class="cost-label">Services Total:</span>
                            <span class="cost-amount" id="lblServicesTotal">₱0.00</span>
                        </div>
                    </div>
                </div>

                <!-- Section 3: Travel Expenses (AUTO-DETECTED, READ-ONLY) -->
                <div class="form-section">
                    <div class="section-title">
                        <i class="fas fa-car"></i>
                        Travel Expenses
                        <span style="font-size: 12px; font-weight: 400; color: #10b981;">(Auto-detected from client location)</span>
                    </div>

                    <!-- Read-only display -->
                    <div class="travel-info">
                        <span class="label">
                            <i class="fas fa-map-marker-alt"></i> Travel Cost:
                        </span>
                        <span class="amount">
                            <asp:Label ID="lblTravelAmount" runat="server" Text="₱0.00" />
                        </span>
                    </div>
                    
                    <div class="info-note">
                        <i class="fas fa-info-circle"></i>
                        <strong>Note:</strong> Travel expense is automatically calculated based on the client's location (Region & City).
                    </div>
                    
                    <asp:HiddenField ID="hfTravelCost" runat="server" Value="0" />
                </div>

                <!-- Section 4: Miscellaneous Expenses -->
                <div class="form-section">
                    <div class="section-title">
                        <i class="fas fa-receipt"></i>
                        Miscellaneous Expenses
                        <span style="font-size: 12px; font-weight: 400; color: #6b7280;">(Optional)</span>
                    </div>

                    <div class="add-expense-form">
                        <input type="text" 
                            id="txtExpenseDescription" 
                            class="form-control"
                            placeholder="e.g., Food Allowance, Parking Fee, Toll" />
                        
                        <input type="number" 
                            id="txtExpenseAmount" 
                            class="form-control"
                            placeholder="Amount (₱)" 
                            step="0.01" 
                            min="0" />
                        
                        <button type="button" 
                            id="btnAddExpense" 
                            class="btn-submit"
                            style="margin: 0;">
                            <i class="fas fa-plus"></i> Add
                        </button>
                    </div>

                    <div id="expenseItemsContainer"></div>

                    <div class="cost-summary">
                        <div class="cost-row">
                            <span class="cost-label">Miscellaneous Total:</span>
                            <span class="cost-amount" id="lblMiscTotal">₱0.00</span>
                        </div>
                    </div>
                    
                    <asp:HiddenField ID="hfMiscExpenses" runat="server" Value="[]" />
                </div>

                <!-- Section 5: Grand Total -->
                <div class="form-section">
                    <div class="section-title">
                        <i class="fas fa-calculator"></i>
                        Cost Summary
                    </div>

                    <div class="cost-summary" style="background: white; border: 2px solid #e5e7eb;">
                        <div class="cost-row">
                            <span class="cost-label">Services:</span>
                            <span class="cost-amount" id="lblServicesBreakdown">₱0.00</span>
                        </div>
                        <div class="cost-row">
                            <span class="cost-label">Travel:</span>
                            <span class="cost-amount" id="lblTravelBreakdown">₱0.00</span>
                        </div>
                        <div class="cost-row">
                            <span class="cost-label">Miscellaneous:</span>
                            <span class="cost-amount" id="lblMiscBreakdown">₱0.00</span>
                        </div>
                    </div>

                    <div class="grand-total-section mt-4">
                        <div class="grand-total-label">TOTAL ESTIMATED COST</div>
                        <div class="grand-total-amount" id="lblGrandTotal">₱0.00</div>
                    </div>
                    
                    <asp:HiddenField ID="hfSelectedServices" runat="server" Value="[]" />
                    <asp:HiddenField ID="hfGrandTotal" runat="server" Value="0" />
                </div>

                <!-- Section 6: Upload Inspection Photos -->
                <div class="form-section">
                    <div class="section-title">
                        <i class="fas fa-camera"></i>
                        Upload Inspection Photos
                        <span style="font-size: 12px; font-weight: 400; color: #6b7280;">(Optional - Max 10 images)</span>
                    </div>

                    <div class="photo-upload-area" onclick="document.getElementById('<%= fuPhotos.ClientID %>').click();">
                        <div class="upload-icon">📷</div>
                        <div style="font-weight: 600; color: #1f2937; margin-bottom: 4px;">
                            Click to upload inspection photos
                        </div>
                        <div style="font-size: 13px; color: #6b7280;">
                            JPEG or PNG (Max 5MB per file, Up to 10 images)
                        </div>
                    </div>

                    <asp:FileUpload ID="fuPhotos" runat="server" 
                        AllowMultiple="true" 
                        accept=".jpg,.jpeg,.png"
                        onchange="handlePhotoSelect(this)"
                        Style="display: none;" />

                    <div id="photoPreviewContainer" class="photo-preview-container"></div>
                </div>

                <!-- Section 7: Additional Notes -->
                <div class="form-section">
                    <div class="section-title">
                        <i class="fas fa-sticky-note"></i>
                        Additional Notes
                    </div>

                    <div class="form-group">
                        <label class="form-label">
                            Additional Observations (Optional)
                        </label>
                        <asp:TextBox ID="txtAdditionalNotes" runat="server" TextMode="MultiLine" CssClass="form-control"
                            placeholder="Any other important information, safety concerns, accessibility issues, etc."
                            MaxLength="1000" />
                    </div>
                </div>

                <!-- Section 8: Follow-up
                <div class="form-section">
                    <div class="section-title">
                        <i class="fas fa-calendar-check"></i>
                        Follow-up Required?
                    </div>

                    <div class="form-group">
                        <div class="radio-group">
                            <div class="radio-option">
                                <asp:RadioButton ID="rbFollowupNo" runat="server" GroupName="Followup" Checked="true" />
                                <label for="<%= rbFollowupNo.ClientID %>" class="radio-label">
                                    ❌ No Follow-up Needed
                                </label>
                            </div>
                            <div class="radio-option">
                                <asp:RadioButton ID="rbFollowupYes" runat="server" GroupName="Followup" 
                                    onclick="document.getElementById('followupDetails').style.display = 'block';" />
                                <label for="<%= rbFollowupYes.ClientID %>" class="radio-label">
                                    ✅ Yes, Follow-up Required
                                </label>
                            </div>
                        </div>
                    </div>

                    <div id="followupDetails" style="display: none;">
                        <div class="form-group">
                            <label class="form-label">Recommended Follow-up Date</label>
                            <asp:TextBox ID="txtFollowupDate" runat="server" TextMode="Date" CssClass="form-control" />
                        </div>
                        <div class="form-group">
                            <label class="form-label">Reason for Follow-up</label>
                            <asp:TextBox ID="txtFollowupReason" runat="server" TextMode="MultiLine" CssClass="form-control"
                                placeholder="Explain why a follow-up inspection is needed"
                                MaxLength="500" />
                        </div>
                    </div>
                </div>  -->

                <!-- Submit Buttons -->
               <asp:Button ID="btnCancel" runat="server" Text="❌ Cancel" CssClass="btn-draft" 
    OnClick="btnCancel_Click" OnClientClick="return confirmCancel();" />
                    
                    <asp:Button ID="btnSubmitReport" runat="server" Text="✅ Submit Report" CssClass="btn-submit" 
                        OnClick="btnSubmitReport_Click" OnClientClick="return validateAndSubmit();" />
                </div>

            </ContentTemplate>
            <Triggers>
                <asp:PostBackTrigger ControlID="btnSubmitReport" />
                <asp:PostBackTrigger ControlID="btnCancel" />
            </Triggers>
        </asp:UpdatePanel>
    </div>

    <script>
        function showDraftImages(paths) {
            const container = document.getElementById("draftImagePreview");
            if (!container) return;

            container.innerHTML = "";
            paths.forEach(path => {
                const img = document.createElement("img");
                img.src = path.replace("~", "");
                img.style.width = "100px";
                img.style.height = "100px";
                img.style.margin = "5px";
                img.style.borderRadius = "8px";
                img.style.objectFit = "cover";
                container.appendChild(img);
            });
        }
    </script>

<div id="draftImagePreview" style="display:flex;flex-wrap:wrap;gap:6px;margin-top:10px;"></div>


    <script>
        // Global variables
        let miscExpenses = [];
        let selectedPhotos = [];

        // ===== Bundle Pricing Calculation =====
        function calculateServiceBundlePrice(serviceId) {
            const checkbox = document.getElementById('chkService_' + serviceId);
            const sqmInput = document.getElementById('sqm_' + serviceId);
            const subtotalLabel = document.getElementById('subtotal_' + serviceId);
            const packageLabel = document.getElementById('selectedPackage_' + serviceId);

            if (!checkbox.checked) {
                subtotalLabel.textContent = 'Total: ₱0.00';
                packageLabel.textContent = 'Select area size to see package';
                calculateServicesTotal();
                return;
            }

            const sqm = parseFloat(sqmInput.value) || 0;

            if (sqm === 0) {
                subtotalLabel.textContent = 'Total: ₱0.00';
                packageLabel.textContent = 'Select area size to see package';
                calculateServicesTotal();
                return;
            }

            const pricingTiers = JSON.parse(checkbox.dataset.pricingTiers || '[]');

            // Find applicable bundle
            let flatPrice = 0;
            let packageRange = '';

            for (let tier of pricingTiers) {
                const minSqm = tier.MinSQM;
                const maxSqm = tier.MaxSQM;

                if (sqm >= minSqm && (maxSqm === null || sqm <= maxSqm)) {
                    flatPrice = tier.FlatPrice;

                    if (maxSqm === null) {
                        packageRange = `${minSqm}+ sqm`;
                    } else {
                        packageRange = `${minSqm}-${maxSqm} sqm`;
                    }
                    break;
                }
            }

            if (flatPrice === 0) {
                subtotalLabel.textContent = 'Total: ₱0.00';
                packageLabel.innerHTML = '<span style="color: #ef4444;">⚠️ No package available for this area size</span>';
                calculateServicesTotal();
                return;
            }

            subtotalLabel.textContent = 'Total: ₱' + flatPrice.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
            packageLabel.innerHTML = `📦 Package: <strong>${packageRange}</strong> - ₱${flatPrice.toLocaleString('en-US', { minimumFractionDigits: 2 })}`;

            calculateServicesTotal();
        }

        // ===== Service Selection =====
        function toggleServiceSQM(checkbox) {
            const serviceItem = checkbox.closest('.service-item');

            if (checkbox.checked) {
                serviceItem.classList.add('selected');
            } else {
                serviceItem.classList.remove('selected');
                const serviceId = checkbox.dataset.serviceId;
                document.getElementById('sqm_' + serviceId).value = '';
                document.getElementById('subtotal_' + serviceId).textContent = 'Total: ₱0.00';
                document.getElementById('selectedPackage_' + serviceId).textContent = 'Select area size to see package';
            }

            calculateServicesTotal();
        }

        // ===== Calculate Services Total =====
        function calculateServicesTotal() {
            let total = 0;

            document.querySelectorAll('.service-checkbox:checked').forEach(checkbox => {
                const serviceId = checkbox.dataset.serviceId;
                const subtotalText = document.getElementById('subtotal_' + serviceId).textContent;
                const subtotal = parseFloat(subtotalText.replace('Total: ₱', '').replace(/,/g, '')) || 0;
                total += subtotal;
            });

            document.getElementById('lblServicesTotal').textContent = '₱' + total.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
            document.getElementById('lblServicesBreakdown').textContent = '₱' + total.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

            calculateGrandTotal();
        }

        // ===== Grand Total Calculation =====
        function calculateGrandTotal() {
            const servicesText = document.getElementById('lblServicesTotal').textContent.replace('₱', '').replace(/,/g, '');
            const services = parseFloat(servicesText) || 0;

            const travel = parseFloat(document.getElementById('<%= hfTravelCost.ClientID %>').value) || 0;

            const miscText = document.getElementById('lblMiscTotal').textContent.replace('₱', '').replace(/,/g, '');
            const misc = parseFloat(miscText) || 0;

            const grandTotal = services + travel + misc;

            document.getElementById('lblGrandTotal').textContent = '₱' + grandTotal.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
            document.getElementById('lblTravelBreakdown').textContent = '₱' + travel.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

            // Store in hidden field
            document.getElementById('<%= hfGrandTotal.ClientID %>').value = grandTotal.toFixed(2);
        }

        // ===== Prepare Data for Submission =====
        function prepareSaveData() {
            const services = [];

            // Debug: Log all checkboxes found
            const allCheckboxes = document.querySelectorAll('.service-checkbox');
            console.log('Total service checkboxes found:', allCheckboxes.length);

            const checkedCheckboxes = document.querySelectorAll('.service-checkbox:checked');
            console.log('Checked service checkboxes:', checkedCheckboxes.length);

            checkedCheckboxes.forEach((checkbox, index) => {
                console.log(`Checkbox ${index + 1}:`, {
                    id: checkbox.id,
                    serviceId: checkbox.dataset.serviceId,
                    serviceName: checkbox.dataset.serviceName,
                    pricingTiers: checkbox.dataset.pricingTiers
                });

                const serviceId = checkbox.dataset.serviceId;
                const serviceName = checkbox.dataset.serviceName;
                const sqmInput = document.getElementById('sqm_' + serviceId);
                const sqm = sqmInput ? parseFloat(sqmInput.value) || 0 : 0;

                // Get the flat price from the subtotal
                const subtotalElement = document.getElementById('subtotal_' + serviceId);
                const subtotalText = subtotalElement ? subtotalElement.textContent : 'Total: ₱0.00';
                const flatPrice = parseFloat(subtotalText.replace('Total: ₱', '').replace(/,/g, '')) || 0;

                // Get package range
                const packageElement = document.getElementById('selectedPackage_' + serviceId);
                const packageText = packageElement ? packageElement.textContent : '';

                const serviceData = {
                    ServiceID: serviceId,
                    ServiceName: serviceName,
                    SQM: sqm,
                    FlatPrice: flatPrice,
                    PackageInfo: packageText
                };

                console.log(`Service ${index + 1} data:`, serviceData);
                services.push(serviceData);
            });

            const servicesJson = JSON.stringify(services);
            const hiddenField = document.getElementById('<%= hfSelectedServices.ClientID %>');

    if (hiddenField) {
        hiddenField.value = servicesJson;
        console.log('Services saved to hidden field:', servicesJson);
        console.log('Hidden field ID:', hiddenField.id);
        console.log('Hidden field value after assignment:', hiddenField.value);
    } else {
        console.error('Hidden field not found!');
    }

    return true;
}

        // ===== Miscellaneous Expenses =====
        document.getElementById('btnAddExpense').addEventListener('click', function () {
            const desc = document.getElementById('txtExpenseDescription').value.trim();
            const amount = parseFloat(document.getElementById('txtExpenseAmount').value) || 0;

            if (!desc) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Missing Information',
                    text: 'Please enter expense description',
                    confirmButtonColor: '#3b82f6'
                });
                return;
            }

            if (amount <= 0) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Invalid Amount',
                    text: 'Please enter a valid amount',
                    confirmButtonColor: '#3b82f6'
                });
                return;
            }

            miscExpenses.push({ description: desc, amount: amount });

            renderExpenseItems();
            calculateMiscTotal();

            // Clear inputs
            document.getElementById('txtExpenseDescription').value = '';
            document.getElementById('txtExpenseAmount').value = '';
        });

        function removeExpenseItem(index) {
            miscExpenses.splice(index, 1);
            renderExpenseItems();
            calculateMiscTotal();
        }

        function renderExpenseItems() {
            const container = document.getElementById('expenseItemsContainer');

            if (miscExpenses.length === 0) {
                container.innerHTML = '<div style="text-align: center; color: #9ca3af; padding: 20px; font-size: 14px;">No miscellaneous expenses added yet</div>';
                return;
            }

            container.innerHTML = miscExpenses.map((exp, index) => `
                <div class="expense-item">
                    <span class="expense-description">${exp.description}</span>
                    <span class="expense-amount">₱${exp.amount.toLocaleString('en-US', { minimumFractionDigits: 2 })}</span>
                    <button type="button" class="btn-remove-expense" onclick="removeExpenseItem(${index})">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
            `).join('');
        }

        function calculateMiscTotal() {
            const total = miscExpenses.reduce((sum, exp) => sum + exp.amount, 0);
            document.getElementById('lblMiscTotal').textContent = '₱' + total.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
            document.getElementById('lblMiscBreakdown').textContent = '₱' + total.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

            // Store in hidden field
            document.getElementById('<%= hfMiscExpenses.ClientID %>').value = JSON.stringify(miscExpenses);

            calculateGrandTotal();
        }

        // ===== Photo Upload Handling =====
        function handlePhotoSelect(input) {
            const files = Array.from(input.files);
            const maxFiles = 10;
            const maxSize = 5 * 1024 * 1024; // 5MB

            let validFiles = [];
            let errors = [];

            files.forEach(file => {
                if (!file.type.match('image/(jpeg|jpg|png)')) {
                    errors.push(`${file.name}: Invalid file type`);
                    return;
                }

                if (file.size > maxSize) {
                    errors.push(`${file.name}: File too large (max 5MB)`);
                    return;
                }

                if (selectedPhotos.length + validFiles.length >= maxFiles) {
                    errors.push(`Maximum ${maxFiles} images allowed`);
                    return;
                }

                validFiles.push(file);
            });

            if (errors.length > 0) {
                Swal.fire({
                    icon: 'error',
                    title: 'Upload Error',
                    html: errors.join('<br>'),
                    confirmButtonColor: '#3b82f6'
                });
            }

            validFiles.forEach(file => {
                selectedPhotos.push(file);
            });

            renderPhotoPreview();
        }

        function renderPhotoPreview() {
            const container = document.getElementById('photoPreviewContainer');
            container.innerHTML = '';

            selectedPhotos.forEach((file, index) => {
                const reader = new FileReader();
                reader.onload = function (e) {
                    const div = document.createElement('div');
                    div.className = 'photo-preview-item';
                    div.innerHTML = `
                        <img src="${e.target.result}" alt="Photo ${index + 1}" />
                        <button type="button" class="photo-remove-btn" onclick="removePhoto(${index})">
                            <i class="fas fa-times"></i>
                        </button>
                    `;
                    container.appendChild(div);
                };
                reader.readAsDataURL(file);
            });
        }

        function removePhoto(index) {
            selectedPhotos.splice(index, 1);
            renderPhotoPreview();
        }

        // ===== Follow-up Toggle =====
        document.getElementById('<%= rbFollowupNo.ClientID %>').addEventListener('change', function () {
            if (this.checked) {
                document.getElementById('followupDetails').style.display = 'none';
            }
        });

        // ===== Validation =====
        function validateAndSubmit() {
            // Validate infestation level
            if (document.getElementById('<%= ddlInfestationLevel.ClientID %>').value === '') {
                Swal.fire({
                    icon: 'warning',
                    title: 'Missing Information',
                    text: 'Please select infestation level',
                    confirmButtonColor: '#3b82f6'
                });
                return false;
            }

            // Validate findings
            const findings = document.getElementById('<%= txtFindings.ClientID %>').value.trim();
            if (!findings || findings.length < 20) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Incomplete Information',
                    text: 'Please provide detailed findings (at least 20 characters)',
                    confirmButtonColor: '#3b82f6'
                });
                return false;
            }

            // Validate affected areas
            const affectedAreas = document.querySelectorAll('.checkbox-item input[type="checkbox"]:checked');
            if (affectedAreas.length === 0) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Missing Information',
                    text: 'Please select at least one affected area',
                    confirmButtonColor: '#3b82f6'
                });
                return false;
            }

            // Validate services
            const selectedServices = document.querySelectorAll('.service-checkbox:checked');
            if (selectedServices.length === 0) {
                Swal.fire({
                    icon: 'warning',
                    title: 'No Services Selected',
                    text: 'Please select at least one recommended service',
                    confirmButtonColor: '#3b82f6'
                });
                return false;
            }

            // Validate SQM for selected services
            let sqmValid = true;
            selectedServices.forEach(checkbox => {
                const serviceId = checkbox.dataset.serviceId;
                const sqm = parseFloat(document.getElementById('sqm_' + serviceId).value) || 0;
                if (sqm <= 0) {
                    sqmValid = false;
                }
            });

            if (!sqmValid) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Missing Area Size',
                    text: 'Please enter area size (sqm) for all selected services',
                    confirmButtonColor: '#3b82f6'
                });
                return false;
            }

            // Validate follow-up details if required
            if (document.getElementById('<%= rbFollowupYes.ClientID %>').checked) {
                const followupDate = document.getElementById('<%= txtFollowupDate.ClientID %>').value;
                const followupReason = document.getElementById('<%= txtFollowupReason.ClientID %>').value.trim();

                if (!followupDate || !followupReason) {
                    Swal.fire({
                        icon: 'warning',
                        title: 'Incomplete Follow-up Information',
                        text: 'Please provide follow-up date and reason',
                        confirmButtonColor: '#3b82f6'
                    });
                    return false;
                }
            }

            return prepareSaveData();
        }

        // ===== Initialize =====
        document.addEventListener('DOMContentLoaded', function () {
            renderExpenseItems();
            calculateServicesTotal();
            calculateGrandTotal();
        });



        // ===== Cancel Confirmation =====
        function confirmCancel() {
            Swal.fire({
                icon: 'question',
                title: 'Cancel Report?',
                text: 'Are you sure you want to cancel? Any unsaved changes will be lost.',
                showCancelButton: true,
                confirmButtonColor: '#ef4444',
                cancelButtonColor: '#6b7280',
                confirmButtonText: 'Yes, Cancel',
                cancelButtonText: 'No, Stay'
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack('<%= btnCancel.UniqueID %>', '');
        }
    });
    return false; // Prevent default postback
}
    </script>
</asp:Content>