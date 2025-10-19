<%@ Page Title="Inspected Inquiries" Language="C#" MasterPageFile="~/Admin.Master"
    AutoEventWireup="true" CodeBehind="InspectedInquiry.aspx.cs"
    Inherits="RRCManagementSystem.InspectedInquiry" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=5.0">
    <script src="https://cdn.tailwindcss.com"></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" crossorigin="anonymous" referrerpolicy="no-referrer" />
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap" rel="stylesheet">

    <style>
        * {
            box-sizing: border-box;
        }

        body {
            font-family: 'Inter', sans-serif;
            background-color: #f8fafc;
        }

        /* ===== TABLE STYLING ===== */
        .table-grid {
            border-collapse: collapse;
            width: 100%;
            font-size: 0.875rem;
        }

        .table-grid th, .table-grid td {
            border: 1px solid #e2e8f0;
            word-wrap: break-word;
            overflow-wrap: break-word;
            white-space: normal;
            padding: 0.75rem 0.5rem;
        }

        /* Responsive table wrapper */
        .table-container {
            overflow-x: auto;
            -webkit-overflow-scrolling: touch;
            border-radius: 0.5rem;
            box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
        }

        /* Extra Large Screens (1536px+) */
        @media (min-width: 1536px) {
            .table-grid {
                table-layout: fixed;
            }
            .table-grid th:nth-child(1), .table-grid td:nth-child(1) { width: 9%; }
            .table-grid th:nth-child(2), .table-grid td:nth-child(2) { width: 13%; }
            .table-grid th:nth-child(3), .table-grid td:nth-child(3) { width: 15%; }
            .table-grid th:nth-child(4), .table-grid td:nth-child(4) { width: 11%; }
            .table-grid th:nth-child(5), .table-grid td:nth-child(5) { width: 7%; }
            .table-grid th:nth-child(6), .table-grid td:nth-child(6) { width: 13%; }
            .table-grid th:nth-child(7), .table-grid td:nth-child(7) { width: 17%; }
            .table-grid th:nth-child(8), .table-grid td:nth-child(8) { width: 15%; }
        }

        /* Large Screens (1024px - 1535px) */
        @media (min-width: 1024px) and (max-width: 1535px) {
            .table-grid { min-width: 1200px; }
            .table-grid th, .table-grid td { font-size: 0.8125rem; padding: 0.625rem 0.5rem; }
        }

        /* Tablets (768px - 1023px) */
        @media (min-width: 768px) and (max-width: 1023px) {
            .table-grid { min-width: 1100px; }
            .table-grid th, .table-grid td { font-size: 0.75rem; padding: 0.5rem 0.375rem; }
        }

        /* Hide table on mobile, show cards */
        @media (max-width: 767px) {
            .table-container { display: none !important; }
            .card-container { display: block !important; }
        }

        /* ===== MODAL STYLING ===== */
        .modal-overlay {
            transition: opacity 0.3s ease-in-out;
            z-index: 9999;
        }

        .modal-open {
            opacity: 1 !important;
            pointer-events: auto;
        }

        .modal-closed {
            opacity: 0;
            pointer-events: none;
        }

        .modal-content-wrapper {
            max-height: 70vh;
            overflow-y: auto;
        }

        /* ===== PAGINATION STYLING ===== */
        .pagination-container {
            display: flex;
            justify-content: center;
            align-items: center;
            padding: 1rem;
            border-top: 1px solid #e5e7eb;
            background-color: #f9fafb;
            border-bottom-left-radius: 0.5rem;
            border-bottom-right-radius: 0.5rem;
            flex-wrap: wrap;
            gap: 0.5rem;
        }

        .pagination-container table {
            border-collapse: separate;
            border-spacing: 0.25rem;
        }

        .pagination-container td { padding: 0; }

        .pagination-container a,
        .pagination-container span {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            min-width: 2.5rem;
            height: 2.5rem;
            padding: 0.5rem 0.75rem;
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

        /* Mobile pagination adjustments */
        @media (max-width: 640px) {
            .pagination-container {
                padding: 0.75rem 0.5rem;
                gap: 0.25rem;
            }

            .pagination-container a,
            .pagination-container span {
                min-width: 2rem;
                height: 2rem;
                padding: 0.25rem 0.5rem;
                font-size: 0.75rem;
            }

            .pagination-container table {
                border-spacing: 0.125rem;
            }
        }

        /* ===== CARD LAYOUT (MOBILE) ===== */
        .card-container {
            display: none;
        }

        .inquiry-card {
            background: white;
            border-radius: 0.75rem;
            box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
            margin-bottom: 1rem;
            overflow: hidden;
            transition: all 0.3s ease;
        }

        .inquiry-card:hover {
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
            transform: translateY(-2px);
        }

        .card-header {
            background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
            padding: 1rem;
            color: white;
        }

        .card-body {
            padding: 1rem;
        }

        .card-row {
            display: flex;
            padding: 0.75rem 0;
            border-bottom: 1px solid #f1f5f9;
            gap: 0.5rem;
        }

        .card-row:last-child {
            border-bottom: none;
        }

        .card-label {
            font-weight: 600;
            color: #475569;
            min-width: 90px;
            font-size: 0.875rem;
            flex-shrink: 0;
        }

        .card-value {
            color: #1e293b;
            flex: 1;
            font-size: 0.875rem;
            word-break: break-word;
        }

        /* Extra small mobile adjustments */
        @media (max-width: 374px) {
            .card-header { padding: 0.75rem; font-size: 0.875rem; }
            .card-body { padding: 0.75rem; }
            .card-row { flex-direction: column; gap: 0.25rem; padding: 0.5rem 0; }
            .card-label { min-width: auto; }
        }

        /* ===== SEARCH BAR ===== */
        .search-container {
            position: relative;
            margin-bottom: 1.5rem;
        }

        .search-input {
            width: 100%;
            padding: 0.75rem 1rem 0.75rem 3rem;
            border: 2px solid #e2e8f0;
            border-radius: 0.5rem;
            font-size: 1rem;
            transition: all 0.3s ease;
        }

        .search-input:focus {
            outline: none;
            border-color: #3b82f6;
            box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
        }

        .search-icon {
            position: absolute;
            left: 1rem;
            top: 50%;
            transform: translateY(-50%);
            color: #94a3b8;
        }

        /* Mobile search adjustments */
        @media (max-width: 640px) {
            .search-input {
                padding: 0.625rem 0.875rem 0.625rem 2.5rem;
                font-size: 0.875rem;
            }
            .search-icon {
                left: 0.75rem;
                font-size: 0.875rem;
            }
        }

        /* ===== NO RESULTS MESSAGE ===== */
        .no-results {
            text-align: center;
            padding: 3rem 1rem;
            color: #64748b;
        }

        .no-results i {
            font-size: 3rem;
            margin-bottom: 1rem;
            color: #cbd5e1;
        }

        @media (max-width: 640px) {
            .no-results {
                padding: 2rem 1rem;
            }
            .no-results i {
                font-size: 2rem;
            }
        }

        /* ===== ACTION BUTTONS ===== */
        .action-btn {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            gap: 0.5rem;
            padding: 0.5rem 1rem;
            border-radius: 9999px;
            font-size: 0.875rem;
            font-weight: 600;
            white-space: nowrap;
            transition: all 0.3s ease;
        }

        @media (max-width: 640px) {
            .action-btn {
                padding: 0.375rem 0.75rem;
                font-size: 0.8125rem;
                gap: 0.375rem;
            }
        }

        /* ===== HEADER RESPONSIVE ===== */
        .page-header {
            text-align: center;
            margin-bottom: 1.5rem;
        }

        .page-title {
            font-size: 1.875rem;
            font-weight: 700;
            color: #0f172a;
        }

        @media (max-width: 640px) {
            .page-header {
                margin-bottom: 1rem;
            }
            .page-title {
                font-size: 1.5rem;
            }
        }

        /* ===== CONTENT HEADER ===== */
        .content-header {
            display: flex;
            flex-direction: column;
            gap: 1rem;
            margin-bottom: 1.5rem;
        }

        @media (min-width: 768px) {
            .content-header {
                flex-direction: row;
                justify-content: space-between;
                align-items: center;
            }
        }

        .section-title {
            font-size: 1.5rem;
            font-weight: 700;
            color: #0f172a;
        }

        @media (max-width: 640px) {
            .section-title {
                font-size: 1.25rem;
            }
        }

        /* ===== CONTAINER PADDING ===== */
        .main-container {
            padding: 1rem;
            max-width: 100%;
            margin: 0 auto;
        }

        @media (min-width: 640px) {
            .main-container {
                padding: 1.5rem;
            }
        }

        @media (min-width: 768px) {
            .main-container {
                padding: 2rem;
            }
        }

        @media (min-width: 1024px) {
            .main-container {
                max-width: 1400px;
            }
        }

        /* ===== EMPTY STATE ===== */
        .empty-state {
            background-color: #dbeafe;
            border: 1px solid #93c5fd;
            color: #1e40af;
            padding: 1rem;
            border-radius: 0.5rem;
            text-align: center;
            margin-bottom: 1rem;
        }

        @media (max-width: 640px) {
            .empty-state {
                font-size: 0.875rem;
                padding: 0.75rem;
            }
        }
    </style>

    <div class="main-container">
        <div class="page-header">
            <h1 class="page-title">
                ✅ Inspected Inquiries
            </h1>
        </div>

        <asp:UpdatePanel ID="updPanel" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="content-header">
                    <h2 class="section-title">
                        Completed with Findings
                    </h2>
                    <asp:Label ID="lblCount" runat="server" CssClass="text-sm font-medium text-slate-500"></asp:Label>
                </div>

                <!-- Search Bar -->
                <div class="search-container">
                    <i class="fas fa-search search-icon"></i>
                    <input type="text" id="searchInput" class="search-input" placeholder="Search by Reference Code or Client Name..." onkeyup="filterInquiries()" />
                </div>

                <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="empty-state">
                    No completed inspections with findings yet.
                </asp:Panel>

                <!-- Table View (Desktop & Tablet) -->
                <div class="table-container">
                    <asp:GridView ID="gvCompleted" runat="server"
                        CssClass="min-w-full bg-white table-auto rounded-lg table-grid"
                        AutoGenerateColumns="False"
                        DataKeyNames="InspectionID"
                        AllowPaging="True" PageSize="10"
                        AllowSorting="True"
                        OnRowCommand="gvCompleted_RowCommand"
                        OnPageIndexChanging="gvCompleted_PageIndexChanging"
                        OnSorting="gvCompleted_Sorting"
                        OnRowDataBound="gvCompleted_RowDataBound"
                        PagerStyle-CssClass="pagination-container"
                        PagerSettings-Mode="NumericFirstLast"
                        PagerSettings-Position="Bottom"
                        PagerSettings-PageButtonCount="5"
                        PagerSettings-FirstPageText="<i class='fas fa-angle-double-left'></i>"
                        PagerSettings-LastPageText="<i class='fas fa-angle-double-right'></i>"
                        HeaderStyle-CssClass="bg-blue-600 text-white uppercase text-xs leading-normal font-bold"
                        RowStyle-CssClass="border-b border-gray-200 hover:bg-gray-100 transition-colors inquiry-row">

                        <Columns>
                            <asp:BoundField DataField="InspectionID" HeaderText="Inspection #" Visible="False" />
                            <asp:BoundField DataField="InquiryCode" HeaderText="Reference Code" SortExpression="InquiryCode" HeaderStyle-CssClass="py-3 px-4 text-center" ItemStyle-CssClass="py-3 px-4 text-center font-bold text-blue-600 inquiry-code text-sm" />
                            <asp:TemplateField HeaderText="Client">
                                <HeaderStyle CssClass="py-3 px-4 text-left" />
                                <ItemStyle CssClass="py-3 px-4 text-left" />
                                <ItemTemplate>
                                    <div class="font-bold text-slate-800 client-name text-sm"><%# Eval("FullName") %></div>
                                    <div class="text-xs text-slate-500"><%# Eval("Email") %></div>
                                    <div class="text-xs text-slate-500"><%# Eval("ContactNumber") %></div>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Address">
                                <HeaderStyle CssClass="py-3 px-4 text-left" />
                                <ItemStyle CssClass="py-3 px-4 text-left text-xs" />
                                <ItemTemplate>
                                    <asp:Literal ID="litAddress" runat="server"></asp:Literal>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="ScheduledDate" HeaderText="Scheduled" DataFormatString="{0:yyyy-MM-dd hh:mm tt}" HtmlEncode="false" HeaderStyle-CssClass="py-3 px-4 text-center" ItemStyle-CssClass="py-3 px-4 text-center text-xs text-slate-800" />
                            <asp:TemplateField HeaderText="Status">
                                <HeaderStyle CssClass="py-3 px-4 text-center" />
                                <ItemStyle CssClass="py-3 px-4 text-center" />
                                <ItemTemplate>
                                    <span class='inline-block px-2 py-1 text-xs font-semibold rounded-full whitespace-nowrap <%# Eval("InspectionStatus").ToString() == "Completed" ? "bg-green-100 text-green-700" : "bg-amber-100 text-amber-700" %>'>
                                        <%# Eval("InspectionStatus") %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Remarks">
                                <HeaderStyle CssClass="py-3 px-4 text-left" />
                                <ItemStyle CssClass="py-3 px-4 text-left text-xs text-slate-800" />
                                <ItemTemplate>
                                    <%# Eval("Remarks") %>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Findings">
                                <HeaderStyle CssClass="py-3 px-4 text-left" />
                                <ItemStyle CssClass="py-3 px-4 text-left text-xs text-slate-800" />
                                <ItemTemplate>
                                    <asp:Literal ID="litFindings" runat="server"></asp:Literal>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Action">
                                <HeaderStyle CssClass="py-3 px-4 text-center" />
                                <ItemStyle CssClass="py-3 px-4 text-center" />
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnCreate" runat="server"
                                        CommandName="create"
                                        CommandArgument='<%# Eval("InspectionID") %>'>
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>

                        <EmptyDataTemplate>
                            <div class="text-center text-slate-500 py-6">No inspected inquiries found.</div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>

                <!-- Card View (Mobile) -->
                <div class="card-container" id="cardContainer">
                    <asp:Repeater ID="rptCards" runat="server">
                        <ItemTemplate>
                            <div class="inquiry-card" data-code='<%# Eval("InquiryCode") %>' data-client='<%# Eval("FullName") %>'>
                                <div class="card-header">
                                    <div class="flex justify-between items-start">
                                        <div>
                                            <div class="text-xs opacity-90 mb-1">Reference Code</div>
                                            <div class="text-lg font-bold"><%# Eval("InquiryCode") %></div>
                                        </div>
                                        <span class='<%# Eval("InspectionStatus").ToString() == "Completed" ? "bg-green-500" : "bg-amber-500" %> px-3 py-1 text-xs font-semibold rounded-full'>
                                            <%# Eval("InspectionStatus") %>
                                        </span>
                                    </div>
                                </div>
                                <div class="card-body">
                                    <div class="card-row">
                                        <div class="card-label"><i class="fas fa-user mr-2"></i>Client:</div>
                                        <div class="card-value">
                                            <div class="font-semibold text-slate-900"><%# Eval("FullName") %></div>
                                            <div class="text-xs text-slate-500"><%# Eval("Email") %></div>
                                            <div class="text-xs text-slate-500"><%# Eval("ContactNumber") %></div>
                                        </div>
                                    </div>
                                    <div class="card-row">
                                        <div class="card-label"><i class="fas fa-map-marker-alt mr-2"></i>Address:</div>
                                        <div class="card-value"><%# Eval("Address") %></div>
                                    </div>
                                    <div class="card-row">
                                        <div class="card-label"><i class="fas fa-calendar mr-2"></i>Scheduled:</div>
                                        <div class="card-value"><%# Eval("ScheduledDate", "{0:MMM dd, yyyy hh:mm tt}") %></div>
                                    </div>
                                    <div class="card-row">
                                        <div class="card-label"><i class="fas fa-comment mr-2"></i>Remarks:</div>
                                        <div class="card-value"><%# Eval("Remarks") %></div>
                                    </div>
                                    <div class="card-row">
                                        <div class="card-label"><i class="fas fa-clipboard-list mr-2"></i>Findings:</div>
                                        <div class="card-value"><%# Eval("Findings") %></div>
                                    </div>
                                    <%# !string.IsNullOrEmpty(Eval("ActionButton")?.ToString()) ? 
                                        "<div class='mt-4 pt-4 border-t border-slate-200'>" + Eval("ActionButton") + "</div>" : "" %>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                    <div id="noResults" class="no-results" style="display: none;">
                        <i class="fas fa-search"></i>
                        <h3 class="text-lg font-semibold mb-2">No Results Found</h3>
                        <p>Try adjusting your search terms</p>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>

    <!-- Modal -->
    <div id="infoModal" class="modal-overlay fixed inset-0 bg-gray-900 bg-opacity-75 flex items-center justify-center p-4 modal-closed">
        <div class="bg-white rounded-lg shadow-xl max-w-lg w-full relative" onclick="event.stopPropagation();">
            <div class="bg-blue-600 text-white p-4 flex items-center justify-between rounded-t-lg">
                <h5 class="font-bold text-lg" id="infoModalLabel"></h5>
                <button onclick="closeModal('infoModal')" class="text-white hover:text-gray-200 transition-colors" aria-label="Close">
                    <i class="fa fa-times"></i>
                </button>
            </div>
            <div class="modal-content-wrapper p-6 text-slate-800" id="modal-content"></div>
            <div class="bg-gray-100 p-4 flex justify-end rounded-b-lg">
                <button onclick="closeModal('infoModal')" class="bg-gray-400 text-white font-bold py-2 px-4 rounded-lg hover:bg-gray-500 transition-colors">Close</button>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(EndRequestHandler);
        function EndRequestHandler(sender, args) {
            filterInquiries();
        }

        function openModal(title, content) {
            const modal = document.getElementById('infoModal');
            modal.querySelector('#infoModalLabel').innerText = title;
            modal.querySelector('#modal-content').innerText = content;
            modal.classList.remove('modal-closed');
            modal.classList.add('modal-open');
            modal.onclick = function () { closeModal('infoModal'); };
        }

        function closeModal(modalId) {
            const modal = document.getElementById(modalId);
            modal.classList.remove('modal-open');
            modal.classList.add('modal-closed');
        }

        function filterInquiries() {
            const searchTerm = document.getElementById('searchInput').value.toLowerCase();

            // Filter table rows (desktop)
            const tableRows = document.querySelectorAll('.inquiry-row');
            let visibleTableRows = 0;

            tableRows.forEach(row => {
                const code = row.querySelector('.inquiry-code')?.textContent.toLowerCase() || '';
                const client = row.querySelector('.client-name')?.textContent.toLowerCase() || '';

                if (code.includes(searchTerm) || client.includes(searchTerm)) {
                    row.style.display = '';
                    visibleTableRows++;
                } else {
                    row.style.display = 'none';
                }
            });

            // Filter cards (mobile)
            const cards = document.querySelectorAll('.inquiry-card');
            const noResults = document.getElementById('noResults');
            let visibleCards = 0;

            cards.forEach(card => {
                const code = card.getAttribute('data-code').toLowerCase();
                const client = card.getAttribute('data-client').toLowerCase();

                if (code.includes(searchTerm) || client.includes(searchTerm)) {
                    card.style.display = '';
                    visibleCards++;
                } else {
                    card.style.display = 'none';
                }
            });

            // Show/hide no results message for cards
            if (noResults) {
                noResults.style.display = visibleCards === 0 && cards.length > 0 ? 'block' : 'none';
            }
        }

        // Prevent zoom on double tap for iOS
        let lastTouchEnd = 0;
        document.addEventListener('touchend', function (event) {
            const now = (new Date()).getTime();
            if (now - lastTouchEnd <= 300) {
                event.preventDefault();
            }
            lastTouchEnd = now;
        }, false);
    </script>
</asp:Content>