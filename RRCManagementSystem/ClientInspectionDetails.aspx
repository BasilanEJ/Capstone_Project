<%@ Page Title="Inspection Report" Language="C#" MasterPageFile="~/Client.master"
    AutoEventWireup="true" CodeBehind="ClientInspectionDetails.aspx.cs"
    Inherits="RRCManagementSystem.ClientInspectionDetails" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.tailwindcss.com"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" />
    <style>
        :root {
            --royal-blue: #1e40af;
            --royal-blue-light: #3b82f6;
            --royal-blue-dark: #1e3a8a;
        }

        body {
            font-size: 14px;
            color: #374151;
        }

        .info-card {
            background: linear-gradient(135deg, #eff6ff 0%, #dbeafe 100%);
            border-left: 4px solid var(--royal-blue);
            transition: all 0.3s ease;
        }

        .info-card:hover {
            transform: translateY(-3px);
            box-shadow: 0 10px 20px -5px rgba(30, 64, 175, 0.1);
        }

        .section-container {
            background: white;
            border-radius: 12px;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.08);
            padding: 1.75rem;
            margin-bottom: 1.5rem;
        }

        .section-header {
            color: var(--royal-blue-dark);
            font-size: 1.25rem;
            font-weight: 700;
            margin-bottom: 1.25rem;
            padding-bottom: 0.75rem;
            border-bottom: 2px solid var(--royal-blue);
            display: flex;
            align-items: center;
            gap: 0.75rem;
        }

        .section-header i {
            color: var(--royal-blue);
            font-size: 1.35rem;
        }

        .data-label {
            font-size: 0.75rem;
            font-weight: 600;
            color: #6b7280;
            text-transform: uppercase;
            letter-spacing: 0.05em;
            margin-bottom: 0.35rem;
        }

        .data-value {
            font-size: 0.95rem;
            font-weight: 600;
            color: #1f2937;
        }

        .badge-container {
            display: inline-flex;
            align-items: center;
            padding: 0.5rem 1rem;
            border-radius: 8px;
            font-weight: 600;
            font-size: 0.875rem;
        }

        .badge-moderate {
            background: linear-gradient(135deg, #fef3c7 0%, #fde68a 100%);
            color: #92400e;
            border: 2px solid #f59e0b;
        }

        .badge-high {
            background: linear-gradient(135deg, #fee2e2 0%, #fecaca 100%);
            color: #991b1b;
            border: 2px solid #ef4444;
        }

        .badge-low {
            background: linear-gradient(135deg, #d1fae5 0%, #a7f3d0 100%);
            color: #065f46;
            border: 2px solid #10b981;
        }

        .stat-box {
            background: white;
            border-radius: 10px;
            padding: 1.25rem;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.08);
            border-left: 4px solid var(--royal-blue);
            transition: all 0.3s ease;
        }

        .stat-box:hover {
            box-shadow: 0 8px 16px -5px rgba(30, 64, 175, 0.12);
            transform: translateY(-2px);
        }

        .table-modern {
            width: 100%;
            background: white;
            border-radius: 10px;
            overflow: hidden;
            box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
        }

        .table-modern th {
            background: linear-gradient(135deg, var(--royal-blue-dark) 0%, var(--royal-blue) 100%);
            color: white;
            padding: 0.875rem 1rem;
            text-align: left;
            font-size: 0.8rem;
            font-weight: 700;
            text-transform: uppercase;
            letter-spacing: 0.05em;
        }

        .table-modern td {
            padding: 0.875rem 1rem;
            color: #374151;
            border-bottom: 1px solid #e5e7eb;
            font-size: 0.875rem;
        }

        .table-modern tr:last-child td {
            border-bottom: none;
        }

        .table-modern tbody tr:hover {
            background: linear-gradient(135deg, #eff6ff 0%, #dbeafe 50%);
        }

        .photo-grid img {
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
            transition: all 0.3s ease;
            cursor: pointer;
        }

        .photo-grid img:hover {
            transform: scale(1.03);
            box-shadow: 0 8px 16px rgba(0, 0, 0, 0.15);
        }

        .expense-item {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 0.75rem 1rem;
            background: #f9fafb;
            border-radius: 6px;
            margin-bottom: 0.5rem;
            border-left: 3px solid var(--royal-blue-light);
        }

        .expense-item:hover {
            background: #eff6ff;
        }

        .btn-primary {
            background: linear-gradient(135deg, var(--royal-blue) 0%, var(--royal-blue-dark) 100%);
            color: white;
            padding: 0.75rem 1.75rem;
            border-radius: 8px;
            font-weight: 600;
            font-size: 0.95rem;
            transition: all 0.3s ease;
            box-shadow: 0 2px 4px rgba(30, 64, 175, 0.3);
            border: none;
            cursor: pointer;
            text-decoration: none;
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
        }

        .btn-primary:hover {
            transform: translateY(-2px);
            box-shadow: 0 6px 12px rgba(30, 64, 175, 0.4);
        }

        .btn-success {
            background: linear-gradient(135deg, #10b981 0%, #059669 100%);
            color: white;
            padding: 0.75rem 1.75rem;
            border-radius: 8px;
            font-weight: 600;
            font-size: 0.95rem;
            transition: all 0.3s ease;
            box-shadow: 0 2px 4px rgba(16, 185, 129, 0.3);
            border: none;
            cursor: pointer;
            text-decoration: none;
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
        }

        .btn-success:hover {
            transform: translateY(-2px);
            box-shadow: 0 6px 12px rgba(16, 185, 129, 0.4);
        }

        .btn-secondary {
            background: #f3f4f6;
            color: #4b5563;
            padding: 0.75rem 1.75rem;
            border-radius: 8px;
            font-weight: 600;
            font-size: 0.95rem;
            transition: all 0.3s ease;
            border: 1px solid #d1d5db;
            cursor: pointer;
            text-decoration: none;
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
        }

        .btn-secondary:hover {
            background: #e5e7eb;
            transform: translateY(-1px);
        }

        .total-banner {
            background: linear-gradient(135deg, var(--royal-blue-dark) 0%, var(--royal-blue) 50%, var(--royal-blue-light) 100%);
            border-radius: 12px;
            padding: 2rem;
            text-align: center;
            box-shadow: 0 6px 16px rgba(30, 64, 175, 0.3);
        }

        .hero-header {
            background: linear-gradient(135deg, var(--royal-blue-dark) 0%, var(--royal-blue) 50%, var(--royal-blue-light) 100%);
            border-radius: 14px;
            padding: 2rem;
            margin-bottom: 2rem;
            box-shadow: 0 6px 16px rgba(30, 64, 175, 0.3);
        }

        .icon-badge {
            background: var(--royal-blue);
            color: white;
            border-radius: 10px;
            padding: 0.875rem;
            box-shadow: 0 2px 6px rgba(30, 64, 175, 0.3);
        }
    </style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">

    <div class="max-w-7xl mx-auto px-4 py-6">
        
        <!-- PAGE HEADER -->
        <div class="hero-header text-white">
            <div class="flex items-center justify-between flex-wrap gap-4">
                <div>
                    <h1 class="text-2xl md:text-3xl font-bold mb-2 flex items-center gap-3">
                        <i class="fa-solid fa-clipboard-check text-3xl"></i>
                        <span>Inspection Report</span>
                    </h1>
                    <p class="text-blue-100 text-sm font-medium">Detailed findings and service recommendations</p>
                </div>
                <div class="bg-white/20 backdrop-blur-sm rounded-lg px-5 py-3 border border-white/30">
                    <p class="text-xs text-blue-100 mb-1 font-semibold">Status</p>
                    <p class="text-xl font-bold tracking-wide">APPROVED</p>
                </div>
            </div>
        </div>

        <!-- QUICK INFO CARDS -->
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
            <div class="info-card rounded-lg p-4 shadow">
                <div class="flex items-center gap-3">
                    <div class="icon-badge">
                        <i class="fa-solid fa-file-invoice text-xl"></i>
                    </div>
                    <div class="flex-1">
                        <p class="data-label">Quotation Code</p>
                        <asp:Label ID="lblQuotationCode" runat="server" CssClass="data-value" style="color: var(--royal-blue-dark);"></asp:Label>
                    </div>
                </div>
            </div>

            <div class="info-card rounded-lg p-4 shadow">
                <div class="flex items-center gap-3">
                    <div class="icon-badge">
                        <i class="fa-solid fa-clipboard-question text-xl"></i>
                    </div>
                    <div class="flex-1">
                        <p class="data-label">Inquiry Code</p>
                        <asp:Label ID="lblInquiryCode" runat="server" CssClass="data-value" style="color: var(--royal-blue-dark);"></asp:Label>
                    </div>
                </div>
            </div>

            <div class="info-card rounded-lg p-4 shadow">
                <div class="flex items-center gap-3">
                    <div class="icon-badge">
                        <i class="fa-solid fa-user-tie text-xl"></i>
                    </div>
                    <div class="flex-1">
                        <p class="data-label">Inspector</p>
                        <asp:Label ID="lblInspectorName" runat="server" CssClass="data-value" style="color: var(--royal-blue-dark);"></asp:Label>
                    </div>
                </div>
            </div>
        </div>

        <!-- FINDINGS SECTION -->
        <div class="section-container">
            <h2 class="section-header">
                <i class="fa-solid fa-magnifying-glass-chart"></i>
                Inspection Findings
            </h2>
            
            <div class="grid grid-cols-1 lg:grid-cols-3 gap-4 mb-4">
                <div class="stat-box">
                    <div class="flex items-center justify-between mb-3">
                        <p class="data-label">Infestation Level</p>
                        <i class="fa-solid fa-bug text-2xl" style="color: var(--royal-blue);"></i>
                    </div>
                    <asp:Label ID="lblInfestationLevel" runat="server" CssClass="badge-container badge-moderate"></asp:Label>
                </div>

                <div class="stat-box lg:col-span-2">
                    <div class="flex items-center gap-3 mb-3">
                        <i class="fa-solid fa-house-damage text-2xl" style="color: var(--royal-blue);"></i>
                        <p class="data-label mb-0">Affected Areas</p>
                    </div>
                    <asp:Label ID="lblAffectedAreas" runat="server" CssClass="data-value" style="color: var(--royal-blue-dark);"></asp:Label>
                </div>
            </div>

            <div class="bg-gradient-to-br from-gray-50 to-blue-50 rounded-lg p-5 border border-blue-200">
                <div class="flex items-start gap-3 mb-3">
                    <i class="fa-solid fa-file-lines text-lg mt-0.5" style="color: var(--royal-blue);"></i>
                    <p class="font-bold text-base" style="color: var(--royal-blue-dark);">Detailed Findings</p>
                </div>
                <div class="text-gray-700 leading-relaxed text-sm whitespace-pre-line pl-7">
                    <asp:Label ID="lblFindings" runat="server"></asp:Label>
                </div>
            </div>
        </div>

        <!-- RECOMMENDED SERVICES -->
        <div class="section-container">
            <h2 class="section-header">
                <i class="fa-solid fa-list-check"></i>
                Recommended Services
            </h2>

            <div class="overflow-x-auto rounded-lg">
                <asp:GridView ID="gvServices" runat="server" AutoGenerateColumns="False" 
                    CssClass="table-modern" GridLines="None">
                    <HeaderStyle CssClass="bg-royal-blue" />
                    <RowStyle CssClass="hover:bg-blue-50" />
                    <Columns>
                        <asp:TemplateField HeaderText="Service">
                            <ItemTemplate>
                                <div class="flex items-center gap-3">
                                    <i class="fa-solid fa-spray-can-sparkles text-lg" style="color: var(--royal-blue);"></i>
                                    <span class="font-semibold text-sm"><%# Eval("ServiceName") %></span>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Coverage (SQM)">
                            <ItemTemplate>
                                <div class="text-center">
                                    <span class="font-semibold text-sm" style="color: var(--royal-blue-dark);"><%# Eval("SQM") %></span>
                                    <span class="text-gray-600 text-xs ml-1">m²</span>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Price">
                            <ItemTemplate>
                                <div class="text-right">
                                    <span class="font-bold text-base text-green-700">₱<%# string.Format("{0:N2}", Eval("FlatPrice")) %></span>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>

        <!-- EXPENSES BREAKDOWN -->
        <div class="grid grid-cols-1 lg:grid-cols-2 gap-4 mb-6">
            
            <!-- TRAVEL EXPENSES -->
            <div class="section-container">
                <h3 class="text-base font-bold mb-4 pb-3 border-b-2" style="color: var(--royal-blue-dark); border-color: var(--royal-blue);">
                    <i class="fa-solid fa-route mr-2" style="color: var(--royal-blue);"></i>
                    Travel Expense
                </h3>
                <div class="bg-gradient-to-br from-green-50 to-emerald-50 rounded-lg p-5 border border-green-300">
                    <div class="flex items-center gap-4">
                        <div class="bg-green-600 text-white rounded-full p-4">
                            <i class="fa-solid fa-car text-2xl"></i>
                        </div>
                        <div>
                            <p class="text-xs font-semibold text-gray-600 mb-1">Transportation Cost</p>
                            <p class="text-2xl font-bold text-green-700">
                                ₱<asp:Label ID="lblTravelCost" runat="server"></asp:Label>
                            </p>
                        </div>
                    </div>
                </div>
            </div>

            <!-- MISCELLANEOUS EXPENSES -->
            <div class="section-container">
                <h3 class="text-base font-bold mb-4 pb-3 border-b-2" style="color: var(--royal-blue-dark); border-color: var(--royal-blue);">
                    <i class="fa-solid fa-money-bill-wave mr-2" style="color: var(--royal-blue);"></i>
                    Miscellaneous Expenses
                </h3>
                
                <div class="mb-4">
                    <asp:Repeater ID="rptMisc" runat="server">
                        <ItemTemplate>
                            <div class="expense-item">
                                <div class="flex items-center gap-2">
                                    <i class="fa-solid fa-circle-dot text-sm" style="color: var(--royal-blue);"></i>
                                    <span class="font-medium text-gray-800 text-sm"><%# Eval("description") %></span>
                                </div>
                                <span class="font-bold text-base" style="color: var(--royal-blue-dark);">₱<%# string.Format("{0:N2}", Eval("amount")) %></span>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>

                <div class="bg-gradient-to-br from-orange-50 to-amber-50 rounded-lg p-4 border border-orange-300">
                    <div class="flex items-center justify-between">
                        <span class="font-bold text-sm text-gray-700">Total Miscellaneous:</span>
                        <span class="text-2xl font-bold text-orange-600">
                            ₱<asp:Label ID="lblMiscTotal" runat="server"></asp:Label>
                        </span>
                    </div>
                </div>
            </div>
        </div>

        <!-- GRAND TOTAL -->
        <div class="total-banner text-white mb-6">
            <div class="flex items-center justify-center gap-5 flex-wrap">
                <i class="fa-solid fa-calculator text-4xl"></i>
                <div>
                    <p class="text-blue-100 text-xs uppercase tracking-widest mb-1 font-semibold">Total Estimated Cost</p>
                    <h3 class="text-4xl md:text-5xl font-bold tracking-tight">
                        ₱<asp:Label ID="lblGrandTotal" runat="server"></asp:Label>
                    </h3>
                </div>
                <div class="bg-white/20 backdrop-blur-sm rounded-lg px-4 py-3 border border-white/30">
                    <i class="fa-solid fa-circle-info text-lg mb-1"></i>
                    <p class="text-xs font-medium">All-inclusive</p>
                </div>
            </div>
        </div>

        <!-- INSPECTION PHOTOS -->
        <div class="section-container">
            <h2 class="section-header">
                <i class="fa-solid fa-images"></i>
                Inspection Photos
            </h2>

            <div class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-3 photo-grid">
                <asp:Repeater ID="rptPhotos" runat="server">
                    <ItemTemplate>
                        <div class="relative group">
                            <img src='<%# Eval("PhotoPath") %>' alt="Inspection Photo"
                                class="w-full h-48 object-cover" 
                                onclick="viewImage('<%# Eval("PhotoPath") %>')" />
                            <div class="absolute inset-0 bg-black bg-opacity-0 group-hover:bg-opacity-40 transition-all duration-300 flex items-center justify-center rounded-lg">
                                <i class="fa-solid fa-search-plus text-white text-3xl opacity-0 group-hover:opacity-100 transition-all duration-300"></i>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>

        <!-- ADDITIONAL NOTES & FOLLOW-UP -->
        <div class="grid grid-cols-1 lg:grid-cols-2 gap-4 mb-6">
            
            <!-- ADDITIONAL NOTES -->
            <div class="section-container">
                <h3 class="text-base font-bold mb-4 pb-3 border-b-2" style="color: var(--royal-blue-dark); border-color: var(--royal-blue);">
                    <i class="fa-solid fa-clipboard-list mr-2" style="color: var(--royal-blue);"></i>
                    Additional Notes
                </h3>
                <div class="bg-gray-50 rounded-lg p-4 border border-gray-300 min-h-[150px]">
                    <p class="text-gray-800 leading-relaxed text-sm whitespace-pre-line">
                        <asp:Label ID="lblNotes" runat="server"></asp:Label>
                    </p>
                </div>
            </div>

            <!-- FOLLOW-UP DETAILS -->
            <div class="section-container">
                <h3 class="text-base font-bold mb-4 pb-3 border-b-2" style="color: var(--royal-blue-dark); border-color: var(--royal-blue);">
                    <i class="fa-solid fa-calendar-check mr-2" style="color: var(--royal-blue);"></i>
                    Follow-up Details
                </h3>
                <div class="space-y-3">
                    <div class="flex items-center gap-3 bg-blue-50 rounded-lg p-3 border-l-4" style="border-color: var(--royal-blue);">
                        <i class="fa-solid fa-toggle-on text-2xl" style="color: var(--royal-blue);"></i>
                        <div>
                            <p class="text-xs font-semibold text-gray-600">Follow-up Required</p>
                            <asp:Label ID="lblFollowUpRequired" runat="server" CssClass="font-bold text-sm" style="color: var(--royal-blue-dark);"></asp:Label>
                        </div>
                    </div>
                    <div class="flex items-center gap-3 bg-blue-50 rounded-lg p-3 border-l-4" style="border-color: var(--royal-blue);">
                        <i class="fa-solid fa-calendar text-2xl" style="color: var(--royal-blue);"></i>
                        <div>
                            <p class="text-xs font-semibold text-gray-600">Follow-up Date</p>
                            <asp:Label ID="lblFollowUpDate" runat="server" CssClass="font-bold text-sm" style="color: var(--royal-blue-dark);"></asp:Label>
                        </div>
                    </div>
                    <div class="bg-blue-50 rounded-lg p-3 border-l-4" style="border-color: var(--royal-blue);">
                        <div class="flex items-start gap-3">
                            <i class="fa-solid fa-comment-dots text-2xl mt-0.5" style="color: var(--royal-blue);"></i>
                            <div class="flex-1">
                                <p class="text-xs font-semibold text-gray-600 mb-1">Reason</p>
                                <asp:Label ID="lblFollowUpReason" runat="server" CssClass="text-gray-800 text-sm leading-relaxed"></asp:Label>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

      <!-- ACTION BUTTONS -->
<div class="bg-gradient-to-r from-gray-50 to-blue-50 rounded-xl shadow-lg p-6 border border-blue-200">
    <div class="flex flex-col sm:flex-row items-center justify-center gap-4">
        <asp:LinkButton ID="btnBookService" runat="server" 
            CssClass="btn-success w-full sm:w-auto justify-center"
            OnClick="btnBookService_Click">
            <i class="fa-solid fa-calendar-check"></i>
            Book Service Now
        </asp:LinkButton>
        <a href="MyInquiries.aspx" class="btn-secondary w-full sm:w-auto justify-center">
            <i class="fa-solid fa-arrow-left"></i>
            Back to Inspections
        </a>
    </div>
</div>

    </div>

    <script>
        function viewImage(path) {
            Swal.fire({
                imageUrl: path,
                imageAlt: 'Inspection Photo',
                width: '80%',
                showCloseButton: true,
                showConfirmButton: false,
                background: '#000',
                customClass: {
                    image: 'rounded-lg'
                }
            });
        }
    </script>

</asp:Content>