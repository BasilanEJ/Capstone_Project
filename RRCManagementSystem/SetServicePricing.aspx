<%@ Page Title="Set Service Pricing" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true"
    CodeBehind="SetServicePricing.aspx.cs" Inherits="RRCManagementSystem.SetServicePricing" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- TailwindCSS -->
    <script src="https://cdn.tailwindcss.com"></script>
    <!-- SweetAlert2 -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <!-- Font Awesome -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" />
    
    <style>
        /* Modern Card Styling */
        .pricing-card {
            background: linear-gradient(135deg, #ffffff 0%, #f8fafc 100%);
            border-radius: 16px;
            box-shadow: 0 10px 30px rgba(0, 0, 0, 0.1);
        }

        /* Page Header */
        .page-header {
            background: linear-gradient(135deg, #1e3a8a 0%, #1e40af 100%);
            color: white;
            padding: 2rem;
            border-radius: 12px;
            margin-bottom: 2rem;
            box-shadow: 0 4px 12px rgba(30, 64, 175, 0.3);
        }

        /* Input Fields Enhancement */
        .form-input {
            transition: all 0.3s ease;
            border: 2px solid #e5e7eb;
        }

        .form-input:focus {
            border-color: #2563eb;
            box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
            transform: translateY(-2px);
        }

        /* Button Enhancement */
        .btn-add-tier {
            background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%);
            transition: all 0.3s ease;
            box-shadow: 0 4px 12px rgba(37, 99, 235, 0.3);
        }

        .btn-add-tier:hover {
            transform: translateY(-2px);
            box-shadow: 0 6px 16px rgba(37, 99, 235, 0.4);
        }

        /* GridView Modern Styling */
        .modern-gridview {
            border-collapse: separate;
            border-spacing: 0;
            border-radius: 12px;
            overflow: hidden;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
        }

        /* Table Header */
        .modern-gridview thead {
            background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%);
            color: white;
        }

        .modern-gridview thead th {
            padding: 1rem;
            text-align: center;
            font-weight: 600;
            font-size: 0.95rem;
            border-right: 1px solid rgba(255, 255, 255, 0.2);
            border-bottom: 2px solid rgba(255, 255, 255, 0.3);
        }

        .modern-gridview thead th:last-child {
            border-right: none;
        }

        /* Table Body */
        .modern-gridview tbody tr {
            background: white;
            transition: all 0.2s ease;
        }

        .modern-gridview tbody tr:nth-child(even) {
            background: #f8fafc;
        }

        .modern-gridview tbody tr:hover {
            background: #dbeafe;
            transform: scale(1.01);
            box-shadow: 0 2px 8px rgba(37, 99, 235, 0.1);
        }

        .modern-gridview tbody td {
            padding: 1rem;
            text-align: center;
            border-right: 1px solid #e5e7eb;
            border-bottom: 1px solid #e5e7eb;
            font-size: 0.95rem;
        }

        .modern-gridview tbody td:last-child {
            border-right: none;
        }

        .modern-gridview tbody tr:last-child td {
            border-bottom: none;
        }

        /* Edit Mode Styling */
        .modern-gridview tbody tr[style*="background"] td {
            background: #fef3c7 !important;
            border-color: #fbbf24 !important;
        }

        /* Action Buttons in GridView */
        .modern-gridview a {
            padding: 0.5rem 1rem;
            border-radius: 6px;
            font-weight: 600;
            font-size: 0.85rem;
            transition: all 0.2s ease;
            display: inline-block;
            text-decoration: none;
        }

        /* Edit Button */
        .modern-gridview a[href*="Edit"] {
            background: linear-gradient(135deg, #10b981 0%, #059669 100%);
            color: white;
            box-shadow: 0 2px 6px rgba(16, 185, 129, 0.3);
        }

        .modern-gridview a[href*="Edit"]:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 10px rgba(16, 185, 129, 0.4);
        }

        /* Update Button */
        .modern-gridview a[href*="Update"] {
            background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
            color: white;
            box-shadow: 0 2px 6px rgba(59, 130, 246, 0.3);
        }

        .modern-gridview a[href*="Update"]:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 10px rgba(59, 130, 246, 0.4);
        }

        /* Cancel Button */
        .modern-gridview a[href*="Cancel"] {
            background: linear-gradient(135deg, #6b7280 0%, #4b5563 100%);
            color: white;
            box-shadow: 0 2px 6px rgba(107, 114, 128, 0.3);
        }

        .modern-gridview a[href*="Cancel"]:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 10px rgba(107, 114, 128, 0.4);
        }

        /* Delete Button */
        .modern-gridview a[href*="Delete"] {
            background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
            color: white;
            box-shadow: 0 2px 6px rgba(239, 68, 68, 0.3);
        }

        .modern-gridview a[href*="Delete"]:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 10px rgba(239, 68, 68, 0.4);
        }

        /* Empty Data Message */
        .modern-gridview tbody tr td[colspan] {
            padding: 3rem;
            text-align: center;
            color: #6b7280;
            font-style: italic;
        }

        /* Form Section Card */
        .form-section-card {
            background: white;
            padding: 1.5rem;
            border-radius: 12px;
            border: 2px solid #e5e7eb;
            margin-bottom: 1.5rem;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
        }

        /* Input Icons */
        .input-with-icon {
            position: relative;
        }

        .input-icon {
            position: absolute;
            left: 12px;
            top: 50%;
            transform: translateY(-50%);
            color: #6b7280;
            font-size: 1rem;
        }

        .input-with-icon input,
        .input-with-icon select {
            padding-left: 2.5rem;
        }

        /* Responsive */
        @media (max-width: 768px) {
            .modern-gridview {
                font-size: 0.85rem;
            }

            .modern-gridview thead th,
            .modern-gridview tbody td {
                padding: 0.75rem 0.5rem;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100 p-6">
        <div class="max-w-6xl mx-auto">
            
            <!-- Page Header -->
            <div class="page-header">
                <h2 class="text-3xl font-bold flex items-center justify-center gap-3">
                    <i class="fas fa-tags text-4xl"></i>
                    <span>Service Pricing Management</span>
                </h2>
                <p class="text-center mt-2 text-blue-100 text-sm">
                    Configure pricing tiers for your services based on square meter ranges
                </p>
            </div>

            <!-- Main Card -->
            <div class="pricing-card p-8">
                
                <asp:Label ID="lblMessage" runat="server" CssClass="block text-center mb-4 font-semibold"></asp:Label>

                <!-- Service Selection Section -->
                <div class="form-section-card">
                    <div class="flex items-center gap-2 mb-4">
                        <i class="fas fa-list-alt text-blue-600 text-xl"></i>
                        <h3 class="text-lg font-bold text-gray-800">Select Service</h3>
                    </div>
                    
                    <div class="input-with-icon">
                        <i class="fas fa-building input-icon"></i>
                        <asp:DropDownList ID="ddlServices" runat="server" 
                            AutoPostBack="true" 
                            OnSelectedIndexChanged="ddlServices_SelectedIndexChanged"
                            CssClass="w-full border-2 border-gray-300 rounded-lg px-4 py-3 form-input focus:ring-2 focus:ring-blue-500 focus:outline-none">
                        </asp:DropDownList>
                    </div>
                    <small class="text-gray-500 mt-2 block">
                        <i class="fas fa-info-circle mr-1"></i>
                        Choose a service to view or add pricing tiers
                    </small>
                </div>

                <!-- Add Pricing Tier Section -->
                <div class="form-section-card">
                    <div class="flex items-center gap-2 mb-4">
                        <i class="fas fa-plus-circle text-blue-600 text-xl"></i>
                        <h3 class="text-lg font-bold text-gray-800">Add New Pricing Tier</h3>
                    </div>

                    <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                        <!-- Min SQM -->
                        <div>
                            <label class="flex items-center gap-2 text-gray-700 font-semibold mb-2">
                                <i class="fas fa-ruler-combined text-blue-600"></i>
                                Minimum SQM
                            </label>
                            <asp:TextBox ID="txtMinSQM" runat="server" 
                                TextMode="Number" 
                                placeholder="e.g., 0"
                                CssClass="w-full border-2 border-gray-300 rounded-lg px-4 py-3 form-input focus:ring-2 focus:ring-blue-500 focus:outline-none" />
                        </div>

                        <!-- Max SQM -->
                        <div>
                            <label class="flex items-center gap-2 text-gray-700 font-semibold mb-2">
                                <i class="fas fa-ruler-combined text-blue-600"></i>
                                Maximum SQM
                            </label>
                            <asp:TextBox ID="txtMaxSQM" runat="server" 
                                TextMode="Number" 
                                placeholder="e.g., 100"
                                CssClass="w-full border-2 border-gray-300 rounded-lg px-4 py-3 form-input focus:ring-2 focus:ring-blue-500 focus:outline-none" />
                        </div>

                        <!-- Flat Price -->
                        <div>
                            <label class="flex items-center gap-2 text-gray-700 font-semibold mb-2">
                                <i class="fas fa-peso-sign text-green-600"></i>
                                Flat Price (₱)
                            </label>
                            <asp:TextBox ID="txtFlatPrice" runat="server" 
                                TextMode="Number" 
                                placeholder="e.g., 5000"
                                CssClass="w-full border-2 border-gray-300 rounded-lg px-4 py-3 form-input focus:ring-2 focus:ring-blue-500 focus:outline-none" />
                        </div>
                    </div>

                    <div class="text-right mt-4">
                        <asp:Button ID="btnAddTier" runat="server" 
                            Text="➕ Add Pricing Tier"
                            CssClass="btn-add-tier text-white font-bold py-3 px-8 rounded-lg"
                            OnClick="btnAddTier_Click" />
                    </div>
                </div>

                <!-- Pricing Tiers Table Section -->
                <div class="form-section-card">
                    <div class="flex items-center justify-between mb-4">
                        <div class="flex items-center gap-2">
                            <i class="fas fa-table text-blue-600 text-xl"></i>
                            <h3 class="text-lg font-bold text-gray-800">Existing Pricing Tiers</h3>
                        </div>
                        <span class="text-sm text-gray-500">
                            <i class="fas fa-info-circle mr-1"></i>
                            Click Edit to modify or Delete to remove
                        </span>
                    </div>

                    <div class="overflow-x-auto">
                        <asp:GridView ID="gvPricing" runat="server" 
                            AutoGenerateColumns="False" 
                            CssClass="min-w-full modern-gridview"
                            DataKeyNames="TierID" 
                            OnRowEditing="gvPricing_RowEditing" 
                            OnRowUpdating="gvPricing_RowUpdating"
                            OnRowCancelingEdit="gvPricing_RowCancelingEdit" 
                            OnRowDeleting="gvPricing_RowDeleting"
                            GridLines="None">

                            <HeaderStyle CssClass="bg-blue-600 text-white" />
                            <RowStyle CssClass="hover:bg-blue-50" />
                            <AlternatingRowStyle CssClass="bg-gray-50" />

                            <Columns>
                                <asp:BoundField DataField="TierID" HeaderText="Tier ID" Visible="False" ReadOnly="True" />

                                <asp:BoundField DataField="ServiceName" HeaderText="Service Name" ReadOnly="True" 
                                    ItemStyle-CssClass="font-semibold text-gray-800" />

                                <asp:BoundField DataField="MinSQM" HeaderText="Min SQM" 
                                    ItemStyle-CssClass="text-blue-600 font-semibold" />
                                
                                <asp:BoundField DataField="MaxSQM" HeaderText="Max SQM" 
                                    ItemStyle-CssClass="text-blue-600 font-semibold" />
                                
                                <asp:BoundField DataField="FlatPrice" HeaderText="Flat Price (₱)" DataFormatString="₱{0:N2}" 
                                    ItemStyle-CssClass="text-green-600 font-bold" />

                                <asp:CommandField ShowEditButton="True" 
                                    EditText="<i class='fas fa-edit'></i> Edit" 
                                    UpdateText="<i class='fas fa-check'></i> Update"
                                    CancelText="<i class='fas fa-times'></i> Cancel"
                                    HeaderText="Actions"
                                    ItemStyle-CssClass="whitespace-nowrap" />
                                
                                <asp:CommandField ShowDeleteButton="True" 
                                    DeleteText="<i class='fas fa-trash'></i> Delete" 
                                    HeaderText="Remove"
                                    ItemStyle-CssClass="whitespace-nowrap" />
                            </Columns>

                            <EmptyDataTemplate>
                                <div class="text-center py-12">
                                    <i class="fas fa-inbox text-gray-300 text-6xl mb-4"></i>
                                    <p class="text-gray-500 text-lg font-semibold">No pricing tiers found</p>
                                    <p class="text-gray-400 text-sm mt-2">Select a service and add pricing tiers to get started</p>
                                </div>
                            </EmptyDataTemplate>
                        </asp:GridView>
                    </div>
                </div>

            </div>
        </div>
    </div>
</asp:Content>