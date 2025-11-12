<%@ Page Title="All Inspection Reports" Language="C#" MasterPageFile="~/Admin.Master"
    AutoEventWireup="true" CodeBehind="AllInspectionDetails.aspx.cs"
    Inherits="RRCManagementSystem.AllInspectionDetails" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PageTitle" runat="server">
    All Inspection Reports
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.tailwindcss.com"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .table-rounded-corners {
            border-collapse: separate;
            border-spacing: 0;
            border-radius: 0.5rem;
            overflow: hidden;
        }
        .table-rounded-corners thead tr:first-child th:first-child {
            border-top-left-radius: 0.5rem;
        }
        .table-rounded-corners thead tr:first-child th:last-child {
            border-top-right-radius: 0.5rem;
        }

        /* Status Badge Styles */
        .status-badge {
            padding: 6px 14px;
            border-radius: 12px;
            font-size: 11px;
            font-weight: 600;
            text-transform: uppercase;
            display: inline-block;
            background: #dbeafe;
            color: #1e40af;
        }

        /* Report Card Styles */
        .report-info-card {
            background: linear-gradient(135deg, #f8fafc 0%, #e2e8f0 100%);
            border-radius: 8px;
            padding: 14px;
            border-left: 4px solid #3b82f6;
            transition: all 0.3s ease;
        }

        .report-info-card:hover {
            box-shadow: 0 4px 12px rgba(59, 130, 246, 0.15);
            transform: translateY(-2px);
        }

        .info-row {
            display: flex;
            align-items: center;
            margin-bottom: 8px;
            font-size: 13px;
        }

        .info-label {
            font-weight: 600;
            color: #475569;
            min-width: 120px;
            display: flex;
            align-items: center;
            gap: 8px;
        }

        .info-value {
            color: #1e293b;
            flex: 1;
            font-weight: 500;
        }

        .cost-highlight {
            background: linear-gradient(135deg, #22c55e 0%, #16a34a 100%);
            color: white;
            padding: 8px 16px;
            border-radius: 8px;
            font-size: 16px;
            font-weight: 700;
            display: inline-block;
            box-shadow: 0 2px 8px rgba(34, 197, 94, 0.3);
        }

        .action-btn {
            transition: all 0.2s ease;
        }

        .action-btn:hover {
            transform: scale(1.05);
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
        }

        @media (max-width: 768px) {
            .info-row {
                flex-direction: column;
                align-items: flex-start;
            }
            .info-label {
                min-width: auto;
                margin-bottom: 4px;
            }
        }
    </style>

    <div class="container mx-auto p-4 md:p-8 bg-white rounded-lg shadow-lg mt-8">

        <div class="flex items-center justify-between mb-6 pb-4 border-b-2 border-blue-200">
            <h2 class="text-3xl font-bold text-gray-800 flex items-center">
                <i class="fas fa-file-alt text-blue-600 mr-3"></i>
                Submitted Inspection Reports
            </h2>
            <div class="flex items-center gap-2 px-4 py-2 bg-blue-50 rounded-lg">
                <i class="fas fa-clipboard-check text-blue-600"></i>
                <span class="text-sm font-semibold text-blue-800">Ready for Review</span>
            </div>
        </div>

        <div class="overflow-x-auto shadow-lg rounded-lg">
            <asp:GridView ID="gvReports" runat="server" AutoGenerateColumns="False"
                CssClass="min-w-full bg-white table-rounded-corners"
                DataKeyNames="ReportID" 
                OnRowCommand="gvReports_RowCommand"
                OnRowDataBound="gvReports_RowDataBound"
                HeaderStyle-CssClass="bg-gradient-to-r from-blue-600 to-blue-700 text-white uppercase text-sm leading-normal"
                RowStyle-CssClass="border-b border-gray-200 hover:bg-blue-50 transition-colors"
                AlternatingRowStyle-CssClass="bg-gray-50 hover:bg-blue-50 transition-colors"
                EnableViewState="true">

                <Columns>

                    <asp:TemplateField HeaderText="Report ID"
                        HeaderStyle-CssClass="py-4 px-6 text-center border-r border-blue-400"
                        ItemStyle-CssClass="py-4 px-6 text-center border-r border-gray-200" Visible ="false ">
                        <ItemTemplate>
                            <div class="flex flex-col items-center">
                                <i class="fas fa-hashtag text-blue-500 mb-1"></i>
                                <span class="font-bold text-lg text-blue-700"><%# Eval("ReportID") %></span>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Report Details"
                        HeaderStyle-CssClass="py-4 px-6 text-center border-r border-blue-400"
                        ItemStyle-CssClass="py-4 px-6 border-r border-gray-200">
                        <ItemTemplate>
                            <div class="report-info-card">
                                <div class="info-row">
                                    <span class="info-label">
                                        <i class="fas fa-file-invoice text-purple-500"></i>
                                        <strong>Quotation:</strong>
                                    </span>
                                    <span class="info-value"><%# Eval("QuotationCode") %></span>
                                </div>
                                <div class="info-row">
                                    <span class="info-label">
                                        <i class="fas fa-ticket-alt text-orange-500"></i>
                                        <strong>Inquiry:</strong>
                                    </span>
                                    <span class="info-value"><%# Eval("InquiryNumber") %></span>
                                </div>

                                <div class="info-row">
                                    <span class="info-label">
                                        <i class="fas fa-user-tie text-blue-500"></i>
                                        <strong>Inspector:</strong>
                                    </span>
                                    <span class="info-value"><%# Eval("InspectorName") %></span>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Estimated Cost"
                        HeaderStyle-CssClass="py-4 px-6 text-center border-r border-blue-400"
                        ItemStyle-CssClass="py-4 px-6 text-center border-r border-gray-200">
                        <ItemTemplate>
                            <div class="flex flex-col items-center">
                                <i class="fas fa-peso-sign text-green-600 text-2xl mb-2"></i>
                                <span class="cost-highlight">
                                    ₱<%# Convert.ToDecimal(Eval("TotalEstimatedCost")).ToString("N2") %>
                                </span>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Status & Date"
                        HeaderStyle-CssClass="py-4 px-6 text-center border-r border-blue-400"
                        ItemStyle-CssClass="py-4 px-6 text-center border-r border-gray-200">
                        <ItemTemplate>
                            <div class="flex flex-col items-center gap-3">
                                <span class="status-badge">
                                    <i class="fas fa-check-circle mr-1"></i>
                                    INSPECTED
                                </span>
                                <div class="text-sm">
                                    <i class="fas fa-calendar-check text-gray-500 mr-1"></i>
                                    <span class="font-semibold text-gray-700">
                                        <%# Convert.ToDateTime(Eval("UpdatedAt")).ToString("MMM dd, yyyy") %>
                                    </span>
                                </div>
                                <div class="text-xs text-gray-500">
                                    <i class="fas fa-clock mr-1"></i>
                                    <%# Convert.ToDateTime(Eval("UpdatedAt")).ToString("hh:mm tt") %>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Actions"
                        HeaderStyle-CssClass="py-4 px-6 text-center"
                        ItemStyle-CssClass="py-4 px-6 text-center">
                        <ItemTemplate>
                            <div class="flex flex-col gap-2">
                                <asp:Button ID="btnView" runat="server" 
                                    Text="👁️ View Full Report"
                                    CommandName="ViewDetails" 
                                    CommandArgument='<%# Eval("ReportID") %>'
                                    CssClass="action-btn px-5 py-2.5 bg-gradient-to-r from-blue-500 to-blue-600 text-white font-bold rounded-lg hover:from-blue-600 hover:to-blue-700 shadow-md text-sm" />
                                
                                <asp:Literal ID="litReportID" runat="server" 
                                    Text='<%# "Report #" + Eval("ReportID") %>' 
                                    Visible="false">
                                </asp:Literal>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>

                <EmptyDataTemplate>
                    <div class="text-center py-12">
                        <i class="fas fa-inbox text-gray-300 text-6xl mb-4"></i>
                        <p class="text-gray-500 text-lg font-semibold">No submitted inspection reports found.</p>
                        <p class="text-gray-400 text-sm mt-2">Reports will appear here once inspectors submit them.</p>
                    </div>
                </EmptyDataTemplate>
            </asp:GridView>
        </div>

        <asp:Label ID="lblNoData" runat="server" 
            CssClass="text-center text-gray-600 mt-6 block text-lg" 
            Visible="false">
            <div class="flex flex-col items-center py-8">
                <i class="fas fa-search text-gray-300 text-5xl mb-3"></i>
                <span class="font-semibold">No submitted inspection reports found.</span>
            </div>
        </asp:Label>
    </div>

    <script type="text/javascript">
        // Optional: Add smooth scroll animations
        document.addEventListener('DOMContentLoaded', function() {
            const rows = document.querySelectorAll('[class*="RowStyle"]');
            rows.forEach((row, index) => {
                row.style.opacity = '0';
                row.style.transform = 'translateY(20px)';
                
                setTimeout(() => {
                    row.style.transition = 'all 0.5s ease';
                    row.style.opacity = '1';
                    row.style.transform = 'translateY(0)';
                }, index * 50);
            });
        });
    </script>
</asp:Content>