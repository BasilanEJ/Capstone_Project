<%@ Page Title="Archived Inspection Reports" Language="C#" MasterPageFile="~/Admin.Master"
    AutoEventWireup="true" CodeBehind="ArchivedInspectionDetails.aspx.cs"
    Inherits="RRCManagementSystem.ArchivedInspectionDetails" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PageTitle" runat="server">
    Archived Inspection Reports
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
            background: #9ca3af;
            color: #1f2937;
        }

        /* Report Card Styles */
        .report-info-card {
            background: linear-gradient(135deg, #fef3c7 0%, #fde68a 100%);
            border-radius: 8px;
            padding: 14px;
            border-left: 4px solid #f59e0b;
            transition: all 0.3s ease;
        }

        .report-info-card:hover {
            box-shadow: 0 4px 12px rgba(245, 158, 11, 0.15);
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
            color: #78350f;
            min-width: 120px;
            display: flex;
            align-items: center;
            gap: 8px;
        }

        .info-value {
            color: #92400e;
            flex: 1;
            font-weight: 500;
        }

        .cost-highlight {
            background: linear-gradient(135deg, #dc2626 0%, #991b1b 100%);
            color: white;
            padding: 8px 16px;
            border-radius: 8px;
            font-size: 16px;
            font-weight: 700;
            display: inline-block;
            box-shadow: 0 2px 8px rgba(220, 38, 38, 0.3);
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

        <div class="flex items-center justify-between mb-6 pb-4 border-b-2 border-orange-200">
            <h2 class="text-3xl font-bold text-gray-800 flex items-center">
                <i class="fas fa-archive text-orange-600 mr-3"></i>
                Archived Inspection Reports
            </h2>
            <div class="flex items-center gap-2 px-4 py-2 bg-orange-50 rounded-lg">
                <i class="fas fa-box-archive text-orange-600"></i>
                <span class="text-sm font-semibold text-orange-800">Archive Storage</span>
            </div>
        </div>

        <div class="overflow-x-auto shadow-lg rounded-lg">
            <asp:GridView ID="gvArchived" runat="server" AutoGenerateColumns="False"
                CssClass="min-w-full bg-white table-rounded-corners"
                DataKeyNames="ReportID" 
                OnRowCommand="gvArchived_RowCommand"
                HeaderStyle-CssClass="bg-gradient-to-r from-orange-600 to-orange-700 text-white uppercase text-sm leading-normal"
                RowStyle-CssClass="border-b border-gray-200 hover:bg-orange-50 transition-colors"
                AlternatingRowStyle-CssClass="bg-gray-50 hover:bg-orange-50 transition-colors"
                EnableViewState="true">

                <Columns>
                    <asp:TemplateField HeaderText="Report ID"
                        HeaderStyle-CssClass="py-4 px-6 text-center border-r border-orange-400"
                        ItemStyle-CssClass="py-4 px-6 text-center border-r border-gray-200" Visible="false">
                        <ItemTemplate>
                            <div class="flex flex-col items-center">
                                <i class="fas fa-hashtag text-orange-500 mb-1"></i>
                                <span class="font-bold text-lg text-orange-700"><%# Eval("ReportID") %></span>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>


                    <asp:TemplateField HeaderText="Report Details"
                        HeaderStyle-CssClass="py-4 px-6 text-center border-r border-orange-400"
                        ItemStyle-CssClass="py-4 px-6 border-r border-gray-200">
                        <ItemTemplate>
                            <div class="report-info-card">
                                <div class="info-row">
                                    <span class="info-label">
                                        <i class="fas fa-file-invoice text-purple-600"></i>
                                        <strong>Quotation:</strong>
                                    </span>
                                    <span class="info-value"><%# Eval("QuotationCode") %></span>
                                </div>

                                <div class="info-row">
                                    <span class="info-label">
                                        <i class="fas fa-ticket-alt text-orange-600"></i>
                                        <strong>Inquiry:</strong>
                                    </span>
                                    <span class="info-value"><%# Eval("InquiryNumber") %></span>
                                </div>


                                <div class="info-row">
                                    <span class="info-label">
                                        <i class="fas fa-user-tie text-amber-700"></i>
                                        <strong>Inspector:</strong>
                                    </span>
                                    <span class="info-value"><%# Eval("InspectorName") %></span>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>


                    <asp:TemplateField HeaderText="Estimated Cost"
                        HeaderStyle-CssClass="py-4 px-6 text-center border-r border-orange-400"
                        ItemStyle-CssClass="py-4 px-6 text-center border-r border-gray-200">
                        <ItemTemplate>
                            <div class="flex flex-col items-center">
                                <i class="fas fa-peso-sign text-red-600 text-2xl mb-2"></i>
                                <span class="cost-highlight">
                                    ₱<%# Convert.ToDecimal(Eval("TotalEstimatedCost")).ToString("N2") %>
                                </span>
                                <span class="text-xs text-gray-500 mt-2">
                                    <i class="fas fa-ban"></i> Archived
                                </span>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>


                    <asp:TemplateField HeaderText="Archive Info"
                        HeaderStyle-CssClass="py-4 px-6 text-center border-r border-orange-400"
                        ItemStyle-CssClass="py-4 px-6 text-center border-r border-gray-200">
                        <ItemTemplate>
                            <div class="flex flex-col items-center gap-3">
                                <span class="status-badge">
                                    <i class="fas fa-archive mr-1"></i>
                                    ARCHIVED
                                </span>
                                <div class="text-sm">
                                    <i class="fas fa-calendar-times text-gray-500 mr-1"></i>
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


            <asp:Button ID="btnRestore" runat="server" 
                Text="♻️ Restore Report"
                CommandName="Restore"
                CommandArgument='<%# Eval("ReportID") %>'
                CssClass="action-btn px-5 py-2.5 bg-gradient-to-r from-green-500 to-green-600 text-white font-bold rounded-lg hover:from-green-600 hover:to-green-700 shadow-md text-sm"
                OnClientClick="return false;" />

            <asp:Button ID="btnDelete" runat="server" 
                Text="🗑️ Delete Forever"
                CommandName="DeleteReport"
                CommandArgument='<%# Eval("ReportID") %>'
                CssClass="action-btn px-5 py-2.5 bg-gradient-to-r from-red-500 to-red-600 text-white font-bold rounded-lg hover:from-red-600 hover:to-red-700 shadow-md text-sm"
                OnClientClick="return false;" />

            <asp:HiddenField ID="hdnConfirmAction" runat="server" />
            <asp:HiddenField ID="hdnReportID" runat="server" Value='<%# Eval("ReportID") %>' />
            <asp:HiddenField ID="hdnQuotationCode" runat="server" Value='<%# Eval("QuotationCode") %>' />
        </div>
    </ItemTemplate>
</asp:TemplateField>
                </Columns>

                <EmptyDataTemplate>
                    <div class="text-center py-12">
                        <i class="fas fa-box-open text-gray-300 text-6xl mb-4"></i>
                        <p class="text-gray-500 text-lg font-semibold">No archived reports found.</p>
                        <p class="text-gray-400 text-sm mt-2">Archived reports will appear here when you archive inspection reports.</p>
                    </div>
                </EmptyDataTemplate>
            </asp:GridView>
        </div>

        <!-- No Data Label (Fallback) -->
        <asp:Label ID="lblNoData" runat="server" 
            CssClass="text-center text-gray-600 mt-6 block text-lg" 
            Visible="false">
            <div class="flex flex-col items-center py-8">
                <i class="fas fa-archive text-gray-300 text-5xl mb-3"></i>
                <span class="font-semibold">No archived reports found.</span>
            </div>
        </asp:Label>
    </div>

    <script type="text/javascript">
        // Handle Restore and Delete button clicks with SweetAlert
        document.addEventListener('DOMContentLoaded', function () {
            attachConfirmationHandlers();
        });

        function attachConfirmationHandlers() {
            // Restore buttons
            const restoreButtons = document.querySelectorAll('[id*="btnRestore"]');
            restoreButtons.forEach(btn => {
                btn.addEventListener('click', function (e) {
                    e.preventDefault();
                    const row = this.closest('tr');
                    const quotationCode = row.querySelector('[id*="hdnQuotationCode"]')?.value || 'this report';

                    Swal.fire({
                        title: 'Restore this report?',
                        html: `Are you sure you want to restore:<br><strong>${quotationCode}</strong>?<br><br>This will move it back to Submitted status.`,
                        icon: 'question',
                        showCancelButton: true,
                        confirmButtonColor: '#16a34a',
                        cancelButtonColor: '#6b7280',
                        confirmButtonText: '<i class="fas fa-undo mr-2"></i>Yes, Restore it!'
                    }).then((result) => {
                        if (result.isConfirmed) {
                            const hdnConfirm = row.querySelector('[id*="hdnConfirmAction"]');
                            if (hdnConfirm) {
                                hdnConfirm.value = 'Restore';
                                __doPostBack(this.name, '');
                            }
                        }
                    });

                    return false;
                });
            });

            // Delete buttons
            const deleteButtons = document.querySelectorAll('[id*="btnDelete"]');
            deleteButtons.forEach(btn => {
                btn.addEventListener('click', function (e) {
                    e.preventDefault();
                    const row = this.closest('tr');
                    const quotationCode = row.querySelector('[id*="hdnQuotationCode"]')?.value || 'this report';

                    Swal.fire({
                        title: 'Delete permanently?',
                        html: `⚠️ <strong>WARNING:</strong> This action cannot be undone!<br><br>Are you sure you want to permanently delete:<br><strong>${quotationCode}</strong>?`,
                        icon: 'warning',
                        showCancelButton: true,
                        confirmButtonColor: '#dc2626',
                        cancelButtonColor: '#6b7280',
                        confirmButtonText: '<i class="fas fa-trash mr-2"></i>Yes, Delete Forever'
                    }).then((result) => {
                        if (result.isConfirmed) {
                            const hdnConfirm = row.querySelector('[id*="hdnConfirmAction"]');
                            if (hdnConfirm) {
                                hdnConfirm.value = 'Delete';
                                __doPostBack(this.name, '');
                            }
                        }
                    });

                    return false;
                });
            });
        }

        // Re-attach handlers after postback
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        if (prm) {
            prm.add_endRequest(function () {
                attachConfirmationHandlers();
            });
        }

        // Optional: Add smooth scroll animations
        document.addEventListener('DOMContentLoaded', function () {
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