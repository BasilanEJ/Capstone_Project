<%@ Page Title="Inspection Details" Language="C#" MasterPageFile="~/Admin.Master"
    AutoEventWireup="true" CodeBehind="InspectionDetails.aspx.cs"
    Inherits="RRCManagementSystem.InspectionDetails" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PageTitle" runat="server">
    Inspection Details
</asp:Content>

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

        .info-card {
            background: linear-gradient(135deg, #eff6ff 0%, #dbeafe 100%);
            border-left: 5px solid var(--royal-blue);
            transition: all 0.3s ease;
        }

        .info-card:hover {
            transform: translateY(-5px);
            box-shadow: 0 20px 25px -5px rgba(30, 64, 175, 0.1);
        }

        .section-container {
            background: white;
            border-radius: 16px;
            box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
            padding: 2.5rem;
            margin-bottom: 2.5rem;
        }

        .section-header {
            color: var(--royal-blue-dark);
            font-size: 1.75rem;
            font-weight: 700;
            margin-bottom: 2rem;
            padding-bottom: 1rem;
            border-bottom: 3px solid var(--royal-blue);
            display: flex;
            align-items: center;
            gap: 1rem;
        }

        .section-header i {
            color: var(--royal-blue);
            font-size: 2rem;
        }

        .data-label {
            font-size: 0.875rem;
            font-weight: 600;
            color: #6b7280;
            text-transform: uppercase;
            letter-spacing: 0.05em;
            margin-bottom: 0.5rem;
        }

        .data-value {
            font-size: 1.25rem;
            font-weight: 700;
            color: #1f2937;
        }

        .badge-container {
            display: inline-flex;
            align-items: center;
            padding: 0.75rem 1.5rem;
            border-radius: 12px;
            font-weight: 600;
            font-size: 1.125rem;
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
            border-radius: 16px;
            padding: 2rem;
            box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
            border-left: 6px solid var(--royal-blue);
            transition: all 0.3s ease;
        }

        .stat-box:hover {
            box-shadow: 0 20px 25px -5px rgba(30, 64, 175, 0.15);
            transform: translateY(-3px);
        }

        .table-modern {
            width: 100%;
            background: white;
            border-radius: 12px;
            overflow: hidden;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
        }

        .table-modern th {
            background: linear-gradient(135deg, var(--royal-blue-dark) 0%, var(--royal-blue) 100%);
            color: white;
            padding: 1.25rem 1.5rem;
            text-align: left;
            font-size: 0.95rem;
            font-weight: 700;
            text-transform: uppercase;
            letter-spacing: 0.05em;
        }

        .table-modern td {
            padding: 1.25rem 1.5rem;
            color: #374151;
            border-bottom: 1px solid #e5e7eb;
            font-size: 1rem;
        }

        .table-modern tr:last-child td {
            border-bottom: none;
        }

        .table-modern tbody tr:hover {
            background: linear-gradient(135deg, #eff6ff 0%, #dbeafe 50%);
        }

        .photo-grid img {
            border-radius: 12px;
            box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
            transition: all 0.3s ease;
            cursor: pointer;
        }

        .photo-grid img:hover {
            transform: scale(1.05);
            box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.2);
        }

        .expense-item {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 1rem 1.25rem;
            background: #f9fafb;
            border-radius: 8px;
            margin-bottom: 0.75rem;
            border-left: 3px solid var(--royal-blue-light);
        }

        .expense-item:hover {
            background: #eff6ff;
        }

        .btn-primary {
            background: linear-gradient(135deg, var(--royal-blue) 0%, var(--royal-blue-dark) 100%);
            color: white;
            padding: 1rem 2.5rem;
            border-radius: 12px;
            font-weight: 700;
            font-size: 1.125rem;
            transition: all 0.3s ease;
            box-shadow: 0 4px 6px -1px rgba(30, 64, 175, 0.3);
            border: none;
            cursor: pointer;
        }

        .btn-primary:hover {
            transform: translateY(-2px);
            box-shadow: 0 10px 15px -3px rgba(30, 64, 175, 0.4);
        }

        .btn-secondary {
            background: linear-gradient(135deg, #6b7280 0%, #4b5563 100%);
            color: white;
            padding: 1rem 2.5rem;
            border-radius: 12px;
            font-weight: 700;
            font-size: 1.125rem;
            transition: all 0.3s ease;
            box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.2);
            border: none;
            cursor: pointer;
        }

        .btn-secondary:hover {
            transform: translateY(-2px);
            box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.3);
        }

        .total-banner {
            background: linear-gradient(135deg, var(--royal-blue-dark) 0%, var(--royal-blue) 50%, var(--royal-blue-light) 100%);
            border-radius: 16px;
            padding: 2.5rem;
            text-align: center;
            box-shadow: 0 10px 25px -5px rgba(30, 64, 175, 0.4);
        }

        .hero-header {
            background: linear-gradient(135deg, var(--royal-blue-dark) 0%, var(--royal-blue) 50%, var(--royal-blue-light) 100%);
            border-radius: 20px;
            padding: 3rem;
            margin-bottom: 3rem;
            box-shadow: 0 10px 25px -5px rgba(30, 64, 175, 0.4);
        }
    </style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">

    <div class="max-w-7xl mx-auto px-4 py-8">
        
        <!-- PAGE HEADER -->
        <div class="hero-header text-white">
            <div class="flex items-center justify-between flex-wrap gap-6">
                <div>
                    <h1 class="text-4xl font-bold mb-3 flex items-center gap-4">
                        <i class="fa-solid fa-clipboard-check text-5xl"></i>
                        <span>Inspection Report Details</span>
                    </h1>
                    <p class="text-blue-100 text-lg font-medium">Comprehensive inspection analysis and service recommendations</p>
                </div>
                <div class="bg-white/20 backdrop-blur-sm rounded-xl px-8 py-5 border-2 border-white/30">
                    <p class="text-sm text-blue-100 mb-1 font-semibold">Report Status</p>
                    <p class="text-3xl font-bold tracking-wide">SUBMITTED</p>
                </div>
            </div>
        </div>

        <!-- QUICK INFO CARDS -->
        <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-12">
            <div class="info-card rounded-xl p-6 shadow-lg">
                <div class="flex items-center gap-5">
                    <div class="bg-gradient-to-br from-blue-600 to-blue-800 text-white rounded-2xl p-5 shadow-lg">
                        <i class="fa-solid fa-file-invoice text-3xl"></i>
                    </div>
                    <div class="flex-1">
                        <p class="data-label">Quotation Code</p>
                        <asp:Label ID="lblQuotationCode" runat="server" CssClass="data-value" style="color: var(--royal-blue-dark);"></asp:Label>
                    </div>
                </div>
            </div>

            <div class="info-card rounded-xl p-6 shadow-lg">
                <div class="flex items-center gap-5">
                    <div class="bg-gradient-to-br from-blue-600 to-blue-800 text-white rounded-2xl p-5 shadow-lg">
                        <i class="fa-solid fa-clipboard-question text-3xl"></i>
                    </div>
                    <div class="flex-1">
                        <p class="data-label">Inquiry Code</p>
                        <asp:Label ID="lblInquiryCode" runat="server" CssClass="data-value" style="color: var(--royal-blue-dark);"></asp:Label>
                    </div>
                </div>
            </div>

            <div class="info-card rounded-xl p-6 shadow-lg">
                <div class="flex items-center gap-5">
                    <div class="bg-gradient-to-br from-blue-600 to-blue-800 text-white rounded-2xl p-5 shadow-lg">
                        <i class="fa-solid fa-user-tie text-3xl"></i>
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
            
            <div class="grid grid-cols-1 lg:grid-cols-3 gap-8 mb-8">
                <div class="stat-box">
                    <div class="flex items-center justify-between mb-4">
                        <p class="data-label">Infestation Level</p>
                        <i class="fa-solid fa-bug text-3xl" style="color: var(--royal-blue);"></i>
                    </div>
                    <asp:Label ID="lblInfestationLevel" runat="server" CssClass="badge-container badge-moderate"></asp:Label>
                </div>

                <div class="stat-box lg:col-span-2">
                    <div class="flex items-center gap-4 mb-4">
                        <i class="fa-solid fa-house-damage text-3xl" style="color: var(--royal-blue);"></i>
                        <p class="data-label mb-0">Affected Areas</p>
                    </div>
                    <asp:Label ID="lblAffectedAreas" runat="server" CssClass="data-value" style="color: var(--royal-blue-dark);"></asp:Label>
                </div>
            </div>

            <div class="bg-gradient-to-br from-gray-50 to-blue-50 rounded-xl p-8 border-2 border-blue-200 shadow-inner">
                <div class="flex items-start gap-4 mb-4">
                    <i class="fa-solid fa-file-lines text-2xl mt-1" style="color: var(--royal-blue);"></i>
                    <p class="font-bold text-xl" style="color: var(--royal-blue-dark);">Detailed Findings</p>
                </div>
                <div class="text-gray-700 leading-relaxed text-lg whitespace-pre-line pl-10">
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

            <div class="overflow-x-auto rounded-xl">
                <asp:GridView ID="gvServices" runat="server" AutoGenerateColumns="False" 
                    CssClass="table-modern" GridLines="None">
                    <HeaderStyle CssClass="bg-royal-blue" />
                    <RowStyle CssClass="hover:bg-blue-50" />
                    <Columns>
                        <asp:TemplateField HeaderText="Service">
                            <ItemTemplate>
                                <div class="flex items-center gap-4">
                                    <i class="fa-solid fa-spray-can-sparkles text-2xl" style="color: var(--royal-blue);"></i>
                                    <span class="font-bold text-lg"><%# Eval("ServiceName") %></span>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Coverage Area (SQM)">
                            <ItemTemplate>
                                <div class="text-center">
                                    <span class="font-bold text-lg" style="color: var(--royal-blue-dark);"><%# Eval("SQM") %></span>
                                    <span class="text-gray-600 ml-1">m²</span>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Service Price">
                            <ItemTemplate>
                                <div class="text-right">
                                    <span class="font-bold text-xl text-green-700">₱<%# string.Format("{0:N2}", Eval("FlatPrice")) %></span>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>

        <!-- EXPENSES BREAKDOWN -->
        <div class="grid grid-cols-1 lg:grid-cols-2 gap-8 mb-12">
            
            <!-- TRAVEL EXPENSES -->
            <div class="section-container">
                <h3 class="text-2xl font-bold mb-6 pb-4 border-b-3" style="color: var(--royal-blue-dark); border-color: var(--royal-blue);">
                    <i class="fa-solid fa-route mr-3" style="color: var(--royal-blue);"></i>
                    Travel Expense
                </h3>
                <div class="bg-gradient-to-br from-green-50 to-emerald-50 rounded-xl p-8 border-2 border-green-300 shadow-lg">
                    <div class="flex items-center justify-between">
                        <div class="flex items-center gap-5">
                            <div class="bg-green-600 text-white rounded-full p-6">
                                <i class="fa-solid fa-car text-4xl"></i>
                            </div>
                            <div>
                                <p class="text-sm font-semibold text-gray-600 mb-2">Transportation Cost</p>
                                <p class="text-4xl font-bold text-green-700">
                                    ₱<asp:Label ID="lblTravelCost" runat="server"></asp:Label>
                                </p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- MISCELLANEOUS EXPENSES -->
            <div class="section-container">
                <h3 class="text-2xl font-bold mb-6 pb-4 border-b-3" style="color: var(--royal-blue-dark); border-color: var(--royal-blue);">
                    <i class="fa-solid fa-money-bill-wave mr-3" style="color: var(--royal-blue);"></i>
                    Miscellaneous Expenses
                </h3>
                
                <div class="mb-6">
                    <asp:Repeater ID="rptMisc" runat="server">
                        <ItemTemplate>
                            <div class="expense-item">
                                <div class="flex items-center gap-3">
                                    <i class="fa-solid fa-circle-dot" style="color: var(--royal-blue);"></i>
                                    <span class="font-semibold text-gray-800 text-lg"><%# Eval("description") %></span>
                                </div>
                                <span class="font-bold text-xl" style="color: var(--royal-blue-dark);">₱<%# string.Format("{0:N2}", Eval("amount")) %></span>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>

                <div class="bg-gradient-to-br from-orange-50 to-amber-50 rounded-xl p-6 border-2 border-orange-300 shadow-lg">
                    <div class="flex items-center justify-between">
                        <span class="font-bold text-xl text-gray-700">Total Miscellaneous:</span>
                        <span class="text-3xl font-bold text-orange-600">
                            ₱<asp:Label ID="lblMiscTotal" runat="server"></asp:Label>
                        </span>
                    </div>
                </div>
            </div>
        </div>

        <!-- GRAND TOTAL -->
        <div class="total-banner text-white mb-12">
            <div class="flex items-center justify-center gap-6 flex-wrap">
                <i class="fa-solid fa-calculator text-6xl"></i>
                <div>
                    <p class="text-blue-100 text-lg uppercase tracking-widest mb-2 font-semibold">Total Estimated Cost</p>
                    <h3 class="text-6xl font-bold tracking-tight">
                        ₱<asp:Label ID="lblGrandTotal" runat="server"></asp:Label>
                    </h3>
                </div>
                <div class="bg-white/20 backdrop-blur-sm rounded-xl px-6 py-4 border-2 border-white/30">
                    <i class="fa-solid fa-circle-info text-2xl mb-2"></i>
                    <p class="text-sm font-medium">All-inclusive pricing</p>
                </div>
            </div>
        </div>

        <!-- INSPECTION PHOTOS -->
        <div class="section-container">
            <h2 class="section-header">
                <i class="fa-solid fa-images"></i>
                Inspection Photos
            </h2>

            <div class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6 photo-grid">
                <asp:Repeater ID="rptPhotos" runat="server">
                    <ItemTemplate>
                        <div class="relative group">
                            <img src='<%# Eval("PhotoPath") %>' alt="Inspection Photo"
                                class="w-full h-64 object-cover" 
                                onclick="viewImage('<%# Eval("PhotoPath") %>')" />
                            <div class="absolute inset-0 bg-black bg-opacity-0 group-hover:bg-opacity-40 transition-all duration-300 flex items-center justify-center rounded-xl">
                                <i class="fa-solid fa-search-plus text-white text-4xl opacity-0 group-hover:opacity-100 transition-all duration-300"></i>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>

        <!-- ADDITIONAL NOTES & FOLLOW-UP -->
        <div class="grid grid-cols-1 lg:grid-cols-2 gap-8 mb-12">
            
            <!-- ADDITIONAL NOTES -->
            <div class="section-container">
                <h3 class="text-2xl font-bold mb-6 pb-4 border-b-3" style="color: var(--royal-blue-dark); border-color: var(--royal-blue);">
                    <i class="fa-solid fa-clipboard-list mr-3" style="color: var(--royal-blue);"></i>
                    Additional Notes
                </h3>
                <div class="bg-gray-50 rounded-xl p-6 border-2 border-gray-300 shadow-inner min-h-[200px]">
                    <p class="text-gray-800 leading-relaxed text-lg whitespace-pre-line">
                        <asp:Label ID="lblNotes" runat="server"></asp:Label>
                    </p>
                </div>
            </div>

            <!-- FOLLOW-UP DETAILS -->
            <div class="section-container">
                <h3 class="text-2xl font-bold mb-6 pb-4 border-b-3" style="color: var(--royal-blue-dark); border-color: var(--royal-blue);">
                    <i class="fa-solid fa-calendar-check mr-3" style="color: var(--royal-blue);"></i>
                    Follow-up Details
                </h3>
                <div class="space-y-5">
                    <div class="flex items-center gap-4 bg-blue-50 rounded-xl p-5 border-l-4" style="border-color: var(--royal-blue);">
                        <i class="fa-solid fa-toggle-on text-3xl" style="color: var(--royal-blue);"></i>
                        <div>
                            <p class="text-sm font-semibold text-gray-600">Follow-up Required</p>
                            <asp:Label ID="lblFollowUpRequired" runat="server" CssClass="font-bold text-xl" style="color: var(--royal-blue-dark);"></asp:Label>
                        </div>
                    </div>
                    <div class="flex items-center gap-4 bg-blue-50 rounded-xl p-5 border-l-4" style="border-color: var(--royal-blue);">
                        <i class="fa-solid fa-calendar text-3xl" style="color: var(--royal-blue);"></i>
                        <div>
                            <p class="text-sm font-semibold text-gray-600">Follow-up Date</p>
                            <asp:Label ID="lblFollowUpDate" runat="server" CssClass="font-bold text-xl" style="color: var(--royal-blue-dark);"></asp:Label>
                        </div>
                    </div>
                    <div class="bg-blue-50 rounded-xl p-5 border-l-4" style="border-color: var(--royal-blue);">
                        <div class="flex items-start gap-4">
                            <i class="fa-solid fa-comment-dots text-3xl mt-1" style="color: var(--royal-blue);"></i>
                            <div class="flex-1">
                                <p class="text-sm font-semibold text-gray-600 mb-2">Reason</p>
                                <asp:Label ID="lblFollowUpReason" runat="server" CssClass="text-gray-800 text-lg leading-relaxed"></asp:Label>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- ACTION BUTTONS -->
        <div class="bg-gradient-to-r from-gray-50 to-blue-50 rounded-2xl shadow-xl p-10 border-2 border-blue-200">
            <div class="flex flex-col sm:flex-row items-center justify-center gap-8">
                <asp:Button ID="btnApprove" runat="server" Text="✓ Approve Report"
                    CssClass="btn-primary"
                    OnClick="btnApprove_Click" />

                <asp:Button ID="btnArchive" runat="server" Text="📦 Archive Report"
                    CssClass="btn-secondary"
                    OnClick="btnArchive_Click" />
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