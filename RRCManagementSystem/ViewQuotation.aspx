<%@ Page Title="View Quotations" Language="C#" MasterPageFile="~/Admin.Master"
    AutoEventWireup="true" CodeBehind="ViewQuotation.aspx.cs"
    Inherits="RRCManagementSystem.ViewQuotation" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <script src="https://cdn.tailwindcss.com"></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <style>
        /* Mobile-first responsive table */
        .table-container {
            background: white;
            border-radius: 0.5rem;
            box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
            overflow: hidden;
        }

        .responsive-table-wrapper {
            overflow-x: auto;
            -webkit-overflow-scrolling: touch;
        }

        .table-grid {
            width: 100%;
            border-collapse: collapse;
        }

        .table-grid th,
        .table-grid td {
            padding: 0.875rem 1rem;
            text-align: left;
            border: 1px solid #e5e7eb;
        }

        .table-grid tr:hover {
            background-color: #f8fafc;
        }

        .table-grid th {
            font-weight: 600;
            font-size: 0.75rem;
            text-transform: uppercase;
            letter-spacing: 0.05em;
            white-space: nowrap;
            background-color: #3b82f6;
            color: white;
        }

        .table-grid td {
            font-size: 0.875rem;
            background-color: white;
        }

        .table-grid tbody tr {
            border: 1px solid #e5e7eb;
        }

        .table-grid tbody tr:hover td {
            background-color: #f8fafc;
        }

        /* Mobile card view */
        @media (max-width: 1024px) {
            .table-grid thead {
                display: none;
            }

            .table-grid,
            .table-grid tbody,
            .table-grid tr,
            .table-grid td {
                display: block;
                width: 100%;
            }

            .table-grid tr {
                margin-bottom: 1rem;
                border: 2px solid #e5e7eb;
                border-radius: 0.5rem;
                background: white;
                box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
                overflow: hidden;
            }

            .table-grid td {
                text-align: right;
                padding: 0.75rem 1rem;
                position: relative;
                border: 1px solid #f3f4f6;
                background-color: white;
            }

            .table-grid td:last-child {
                border-bottom: none;
            }

            .table-grid td::before {
                content: attr(data-label);
                position: absolute;
                left: 1rem;
                font-weight: 600;
                color: #374151;
                text-transform: uppercase;
                font-size: 0.75rem;
                letter-spacing: 0.05em;
            }

            .table-grid td:first-child {
                background: linear-gradient(to right, #3b82f6, #2563eb);
                color: white;
                font-weight: 700;
                font-size: 1rem;
                text-align: center;
                padding: 1rem;
            }

            .table-grid td:first-child::before {
                display: none;
            }
        }

        /* Desktop view - wider table */
        @media (min-width: 1025px) {
            .table-grid {
                width: 100%;
            }

            .service-name-cell {
                max-width: 300px;
                white-space: normal;
                word-wrap: break-word;
            }
        }

        /* View Details Link */
        .view-details-link {
            color: #3b82f6;
            cursor: pointer;
            text-decoration: underline;
            font-size: 0.75rem;
            display: inline-block;
            margin-top: 0.25rem;
        }

        .view-details-link:hover {
            color: #2563eb;
        }

        /* Modal Styles */
        .modal-overlay {
            display: none;
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background-color: rgba(0, 0, 0, 0.5);
            z-index: 1000;
            align-items: center;
            justify-content: center;
            padding: 1rem;
            animation: fadeIn 0.2s ease-out;
        }

        .modal-overlay.active {
            display: flex;
        }

        .modal-content {
            background: white;
            border-radius: 0.75rem;
            max-width: 500px;
            width: 100%;
            max-height: 90vh;
            overflow-y: auto;
            box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1);
            animation: slideUp 0.3s ease-out;
        }

        @keyframes fadeIn {
            from { opacity: 0; }
            to { opacity: 1; }
        }

        @keyframes slideUp {
            from { 
                opacity: 0;
                transform: translateY(20px);
            }
            to { 
                opacity: 1;
                transform: translateY(0);
            }
        }

        /* Pagination styles */
        .pagination-container {
            display: flex;
            justify-content: center;
            align-items: center;
            padding: 1rem;
            background-color: #f9fafb;
            border-top: 1px solid #e5e7eb;
            flex-wrap: wrap;
            gap: 0.5rem;
        }

        .pagination-container table {
            border-collapse: separate;
            border-spacing: 0.25rem;
        }

        .pagination-container td {
            padding: 0;
        }

        .pagination-container a,
        .pagination-container span {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            min-width: 2.5rem;
            height: 2.5rem;
            padding: 0.5rem;
            font-size: 0.875rem;
            font-weight: 500;
            border-radius: 0.375rem;
            transition: all 0.2s ease;
            text-decoration: none;
        }

        .pagination-container a {
            background-color: white;
            color: #374151;
            border: 1px solid #d1d5db;
        }

        .pagination-container a:hover {
            background-color: #3b82f6;
            color: white;
            border-color: #3b82f6;
            transform: translateY(-1px);
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
        }

        .pagination-container span {
            background-color: #3b82f6;
            color: white;
            border: 1px solid #3b82f6;
            font-weight: 600;
            box-shadow: 0 2px 4px rgba(59, 130, 246, 0.3);
        }

        /* Search/Filter Section */
        .filter-section {
            background: linear-gradient(135deg, #f9fafb 0%, #ffffff 100%);
            border: 1px solid #e5e7eb;
        }

        /* Mobile optimizations */
        @media (max-width: 640px) {
            .pagination-container a,
            .pagination-container span {
                min-width: 2rem;
                height: 2rem;
                padding: 0.375rem;
                font-size: 0.75rem;
            }

            .modal-content {
                margin: 0.5rem;
            }
        }

        /* Badge styles */
        .status-badge {
            display: inline-flex;
            align-items: center;
            padding: 0.375rem 0.75rem;
            border-radius: 9999px;
            font-size: 0.75rem;
            font-weight: 600;
            white-space: nowrap;
        }

        /* Empty state */
        .empty-state {
            padding: 3rem 1rem;
            text-align: center;
        }

        .empty-state i {
            font-size: 3rem;
            color: #d1d5db;
            margin-bottom: 1rem;
        }
    </style>

    <div class="container mx-auto px-4 py-6 max-w-full">
        <!-- Header -->
        <div class="mb-6">
            <h2 class="text-2xl md:text-3xl font-bold text-slate-900 flex items-center gap-2">
                <i class="fas fa-file-invoice text-blue-600"></i>
                View Quotations
            </h2>
            <p class="text-slate-600 text-sm mt-1">Search and filter quotations by date, inspector, and status</p>
        </div>

        <!-- Filter Section -->
        <div class="filter-section rounded-lg shadow-sm p-4 md:p-6 mb-6">
            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-4">
                <!-- Date From -->
                <div class="flex flex-col">
                    <label for="<%= txtDateFrom.ClientID %>" class="font-semibold text-slate-700 mb-2 text-sm flex items-center gap-1">
                        <i class="fas fa-calendar-alt text-blue-600"></i>
                        Date From
                    </label>
                    <asp:TextBox ID="txtDateFrom" runat="server" TextMode="Date" 
                        CssClass="w-full px-3 py-2 text-sm border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition" />
                </div>

                <!-- Date To -->
                <div class="flex flex-col">
                    <label for="<%= txtDateTo.ClientID %>" class="font-semibold text-slate-700 mb-2 text-sm flex items-center gap-1">
                        <i class="fas fa-calendar-alt text-blue-600"></i>
                        Date To
                    </label>
                    <asp:TextBox ID="txtDateTo" runat="server" TextMode="Date" 
                        CssClass="w-full px-3 py-2 text-sm border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition" />
                </div>

                <!-- Inspector -->
                <div class="flex flex-col">
                    <label for="<%= ddlInspector.ClientID %>" class="font-semibold text-slate-700 mb-2 text-sm flex items-center gap-1">
                        <i class="fas fa-user-tie text-blue-600"></i>
                        Inspector
                    </label>
                    <asp:DropDownList ID="ddlInspector" runat="server" 
                        CssClass="w-full px-3 py-2 text-sm border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition" />
                </div>

                <!-- Action Buttons -->
                <div class="flex flex-col justify-end">
                    <div class="flex gap-2">
                        <asp:Button ID="btnSearch" runat="server" Text="Search" OnClick="btnSearch_Click"
                            CssClass="flex-1 bg-blue-600 text-white px-4 py-2 rounded-lg font-semibold text-sm hover:bg-blue-700 transition-colors shadow-sm flex items-center justify-center gap-2" />
                        <asp:Button ID="btnReset" runat="server" Text="Reset" OnClick="btnReset_Click"
                            CssClass="flex-1 bg-white text-slate-700 border border-slate-300 px-4 py-2 rounded-lg font-semibold text-sm hover:bg-slate-50 transition-colors shadow-sm flex items-center justify-center gap-2" />
                    </div>
                </div>
            </div>

            <!-- Message Label -->
            <asp:Label ID="lblMessage" runat="server" CssClass="text-slate-600 text-sm font-medium block" />
        </div>

        <!-- Table Container -->
        <div class="table-container">
            <div class="responsive-table-wrapper">
                <asp:GridView ID="gvQuotations" runat="server"
                    AutoGenerateColumns="False"
                    CssClass="table-grid"
                    DataKeyNames="PendingQuotationID"
                    AllowPaging="true" PageSize="10"
                    OnPageIndexChanging="gvQuotations_PageIndexChanging"
                    OnRowDataBound="gvQuotations_RowDataBound"
                    PagerStyle-CssClass="pagination-container"
                    PagerSettings-Mode="NumericFirstLast"
                    PagerSettings-Position="Bottom"
                    PagerSettings-PageButtonCount="5"
                    PagerSettings-FirstPageText="<i class='fas fa-angle-double-left'></i>"
                    PagerSettings-LastPageText="<i class='fas fa-angle-double-right'></i>"
                    HeaderStyle-CssClass="bg-blue-600 text-white"
                    RowStyle-CssClass="hover:bg-gray-50 transition-colors">
                    <Columns>
                        <asp:BoundField DataField="PendingQuotationID" HeaderText="Quote #" Visible="False" />
                        
                        <asp:TemplateField HeaderText="Code">
                            <ItemTemplate>
                                <div class="font-semibold text-blue-600"><%# Eval("QuotationCode") %></div>
                            </ItemTemplate>
                            <ItemStyle CssClass="quotation-code-cell" />
                        </asp:TemplateField>

                        <asp:BoundField DataField="CreatedAt" HeaderText="Date" DataFormatString="{0:MM/dd/yyyy}" HtmlEncode="false" />
                        <asp:BoundField DataField="ClientName" HeaderText="Client" ItemStyle-CssClass="font-medium" />
                        <asp:BoundField DataField="InspectorName" HeaderText="Inspector" />
                        <asp:BoundField DataField="ServiceNames" HeaderText="Services" ItemStyle-CssClass="service-name-cell" />
                        <asp:BoundField DataField="SQM" HeaderText="SQM" DataFormatString="{0:N2}" HtmlEncode="false" />
                        <asp:BoundField DataField="BasePrice" HeaderText="Base (₱)" DataFormatString="{0:N2}" HtmlEncode="false" />
                        <asp:BoundField DataField="TravelExpense" HeaderText="Travel (₱)" DataFormatString="{0:N2}" HtmlEncode="false" />
                        
                        <asp:TemplateField HeaderText="Misc (₱)">
                            <ItemTemplate>
                                <div>
                                    <div class="font-semibold text-slate-900">₱<%# String.Format("{0:N2}", Eval("Miscellaneous")) %></div>
                                    <%# !string.IsNullOrWhiteSpace(Eval("MiscellaneousDetails") as string) 
                                        ? "<a href='javascript:void(0)' class='view-details-link' onclick=\"showMiscDetails('" + System.Web.HttpUtility.JavaScriptStringEncode(Eval("MiscellaneousDetails") as string ?? "") + "')\"><i class='fas fa-info-circle'></i> View Details</a>" 
                                        : "<span class='text-xs text-gray-400'>No details</span>" %>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:BoundField DataField="Price" HeaderText="Total (₱)" DataFormatString="{0:N2}" HtmlEncode="false" 
                            ItemStyle-CssClass="font-bold text-green-700 text-base" />

                        <asp:TemplateField HeaderText="Contract">
                            <ItemTemplate>
                                <span class="status-badge <%# Convert.ToBoolean(Eval("IsContract")) ? "bg-blue-100 text-blue-700 border border-blue-300" : "bg-gray-100 text-gray-700 border border-gray-300" %>">
                                    <i class="fas fa-file-contract mr-1"></i>
                                    <%# Convert.ToBoolean(Eval("IsContract")) ? "Yes" : "No" %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        
                        <asp:TemplateField HeaderText="Status">
                            <ItemTemplate>
                                <span class="status-badge <%# Eval("Status").ToString() == "Converted" ? "bg-green-100 text-green-700 border border-green-300" : "bg-amber-100 text-amber-700 border border-amber-300" %>">
                                    <i class="fas <%# Eval("Status").ToString() == "Converted" ? "fa-check-circle" : "fa-clock" %> mr-1"></i>
                                    <%# Eval("Status") %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        <div class="empty-state">
                            <i class="fas fa-inbox"></i>
                            <p class="text-slate-500 font-medium">No quotations found for the selected filters.</p>
                            <p class="text-slate-400 text-sm mt-2">Try adjusting your search criteria</p>
                        </div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>
        </div>
    </div>

    <!-- Miscellaneous Details Modal -->
    <div id="miscModal" class="modal-overlay">
        <div class="modal-content">
            <div class="bg-gradient-to-r from-blue-600 to-blue-700 px-6 py-4 rounded-t-lg">
                <h3 class="text-xl font-semibold text-white flex items-center justify-between">
                    <span><i class="fas fa-list-ul mr-2"></i>Miscellaneous Expense Details</span>
                    <button onclick="closeMiscModal()" class="text-white hover:text-gray-200 transition" aria-label="Close modal">
                        <i class="fas fa-times text-2xl"></i>
                    </button>
                </h3>
            </div>

            <div id="miscModalBody" class="p-6 space-y-3">
                <!-- Content populated by JavaScript -->
            </div>

            <div class="bg-gray-50 px-6 py-4 rounded-b-lg flex justify-end gap-2">
                <button onclick="closeMiscModal()" 
                    class="px-6 py-2.5 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-semibold transition shadow-sm">
                    <i class="fas fa-times mr-2"></i>Close
                </button>
            </div>
        </div>
    </div>

    <script>
        function showMiscDetails(details) {
            if (!details || details.trim() === '') {
                Swal.fire({
                    icon: 'info',
                    title: 'No Details',
                    text: 'No miscellaneous expense details available.',
                    confirmButtonColor: '#3b82f6'
                });
                return;
            }

            var items = details.split(';').filter(function (item) {
                return item.trim() !== '';
            });

            var html = '<div class="space-y-3">';

            if (items.length === 0) {
                html += '<p class="text-gray-500 text-center">No items found.</p>';
            } else {
                items.forEach(function (item, index) {
                    var trimmed = item.trim();
                    html += '<div class="flex items-start p-3 bg-gray-50 rounded-lg border border-gray-200 hover:border-blue-300 transition">';
                    html += '<div class="flex-shrink-0 w-8 h-8 bg-blue-100 rounded-full flex items-center justify-center mr-3">';
                    html += '<span class="text-blue-600 font-semibold text-sm">' + (index + 1) + '</span>';
                    html += '</div>';
                    html += '<div class="flex-1">';
                    html += '<p class="text-gray-800 font-medium">' + escapeHtml(trimmed) + '</p>';
                    html += '</div>';
                    html += '</div>';
                });
            }

            html += '</div>';

            document.getElementById('miscModalBody').innerHTML = html;
            document.getElementById('miscModal').classList.add('active');
            document.body.style.overflow = 'hidden';
        }

        function closeMiscModal() {
            document.getElementById('miscModal').classList.remove('active');
            document.body.style.overflow = '';
        }

        function escapeHtml(text) {
            var map = {
                '&': '&amp;',
                '<': '&lt;',
                '>': '&gt;',
                '"': '&quot;',
                "'": '&#039;'
            };
            return text.replace(/[&<>"']/g, function (m) { return map[m]; });
        }

        // Close modal on escape key
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                closeMiscModal();
            }
        });

        // Close modal on backdrop click
        document.getElementById('miscModal').addEventListener('click', function (e) {
            if (e.target.id === 'miscModal') {
                closeMiscModal();
            }
        });

        // Add data-label attributes for mobile view
        window.addEventListener('DOMContentLoaded', function () {
            var headers = ['Code', 'Date', 'Client', 'Inspector', 'Services', 'SQM', 'Base (₱)', 'Travel (₱)', 'Misc (₱)', 'Total (₱)', 'Contract', 'Status'];
            var rows = document.querySelectorAll('.table-grid tbody tr');

            rows.forEach(function (row) {
                var cells = row.querySelectorAll('td');
                cells.forEach(function (cell, index) {
                    if (index > 0 && headers[index - 1]) {
                        cell.setAttribute('data-label', headers[index - 1]);
                    }
                });
            });
        });
    </script>
</asp:Content>