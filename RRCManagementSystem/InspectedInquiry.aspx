<%@ Page Title="Inspected Inquiries" Language="C#" MasterPageFile="~/Admin.Master"
    AutoEventWireup="true" CodeBehind="InspectedInquiry.aspx.cs"
    Inherits="RRCManagementSystem.InspectedInquiry" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <script src="https://cdn.tailwindcss.com"></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" crossorigin="anonymous" referrerpolicy="no-referrer" />
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap" rel="stylesheet">

    <style>
        body {
            font-family: 'Inter', sans-serif;
            background-color: #f8fafc;
        }

        .table-grid {
            border-collapse: collapse;
        }

        .table-grid th, .table-grid td {
            border: 1px solid #e2e8f0; /* slate-200 */
        }

        /* === Modal Fade Animation === */
        .modal-overlay {
            transition: opacity 0.3s ease-in-out;
        }

        .modal-open {
            opacity: 1 !important;
            pointer-events: auto;
        }

        .modal-closed {
            opacity: 0;
            pointer-events: none;
        }

        /* === Pagination Styling (same as AllBooking.aspx) === */
        .pagination-container {
            display: flex;
            justify-content: center;
            align-items: center;
            padding: 1rem;
            border-top: 1px solid #e5e7eb;
            background-color: #f9fafb;
            border-bottom-left-radius: 0.5rem;
            border-bottom-right-radius: 0.5rem;
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

        .pagination-container a:first-child,
        .pagination-container a:last-child {
            background-color: #f3f4f6;
            border-color: #d1d5db;
        }

        .pagination-container a:first-child:hover,
        .pagination-container a:last-child:hover {
            background-color: #2563eb;
            border-color: #2563eb;
        }

        .pagination-container i {
            font-size: 1rem;
        }

        @media (max-width: 640px) {
            .pagination-container {
                padding: 0.75rem 0.5rem;
            }

            .pagination-container a,
            .pagination-container span {
                min-width: 2rem;
                height: 2rem;
                padding: 0.375rem 0.5rem;
                font-size: 0.75rem;
            }
        }
    </style>

    <div class="container mx-auto p-4 md:p-8">

        <h2 class="text-center text-3xl font-bold text-slate-900 mb-6">
            ✅ Inspected Inquiries
        </h2>

        <asp:UpdatePanel ID="updPanel" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="flex flex-col md:flex-row justify-between items-center mb-6">
                    <h2 class="text-xl md:text-2xl font-bold text-slate-900 mb-2 md:mb-0">
                        Completed with Findings
                    </h2>
                    <asp:Label ID="lblCount" runat="server" CssClass="text-sm font-medium text-slate-500"></asp:Label>
                </div>

                <asp:Panel ID="pnlEmpty" runat="server" Visible="false"
                    CssClass="bg-blue-100 border border-blue-400 text-blue-700 px-4 py-3 rounded-lg relative text-center">
                    No completed inspections with findings yet.
                </asp:Panel>

                <div class="overflow-x-auto shadow-lg rounded-lg">
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
                        PagerSettings-PreviousPageText="<i class='fas fa-angle-left'></i>"
                        PagerSettings-NextPageText="<i class='fas fa-angle-right'></i>"
                        HeaderStyle-CssClass="bg-blue-600 text-white uppercase text-xs leading-normal font-bold"
                        RowStyle-CssClass="border-b border-gray-200 hover:bg-gray-100 transition-colors">

                        <Columns>
                            <asp:BoundField DataField="InspectionID" HeaderText="Inspection #" Visible="False" />
                            <asp:BoundField DataField="InquiryCode" HeaderText="Reference Code" SortExpression="InquiryCode" HeaderStyle-CssClass="py-3 px-6 text-center" ItemStyle-CssClass="py-3 px-6 text-center font-bold text-blue-600" />
                            <asp:TemplateField HeaderText="Client">
                                <HeaderStyle CssClass="py-3 px-6 text-left" />
                                <ItemStyle CssClass="py-3 px-6 text-left" />
                                <ItemTemplate>
                                    <div class="font-bold text-slate-800"><%# Eval("FullName") %></div>
                                    <div class="text-sm text-slate-500"><%# Eval("Email") %></div>
                                    <div class="text-sm text-slate-500"><%# Eval("ContactNumber") %></div>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Address">
                                <HeaderStyle CssClass="py-3 px-6 text-left" />
                                <ItemStyle CssClass="py-3 px-6 text-left" />
                                <ItemTemplate>
                                    <asp:Literal ID="litAddress" runat="server"></asp:Literal>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="ScheduledDate" HeaderText="Scheduled" DataFormatString="{0:yyyy-MM-dd hh:mm tt}" HtmlEncode="false" HeaderStyle-CssClass="py-3 px-6 text-center" ItemStyle-CssClass="py-3 px-6 text-center text-sm text-slate-800" />
                            <asp:TemplateField HeaderText="Status">
                                <HeaderStyle CssClass="py-3 px-6 text-center" />
                                <ItemStyle CssClass="py-3 px-6 text-center" />
                                <ItemTemplate>
                                    <span class='inline-block px-3 py-1 text-xs font-semibold rounded-full <%# Eval("InspectionStatus").ToString() == "Completed" ? "bg-green-100 text-green-700" : "bg-amber-100 text-amber-700" %>'>
                                        <%# Eval("InspectionStatus") %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Remarks">
                                <HeaderStyle CssClass="py-3 px-6 text-left" />
                                <ItemStyle CssClass="py-3 px-6 text-left text-sm text-slate-800" />
                                <ItemTemplate>
                                    <%# Eval("Remarks") %>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Findings">
                                <HeaderStyle CssClass="py-3 px-6 text-left" />
                                <ItemStyle CssClass="py-3 px-6 text-left text-sm text-slate-800" />
                                <ItemTemplate>
                                    <asp:Literal ID="litFindings" runat="server"></asp:Literal>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Action">
                                <HeaderStyle CssClass="py-3 px-6 text-center" />
                                <ItemStyle CssClass="py-3 px-6 text-center" />
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
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>

    <div id="infoModal" class="modal-overlay fixed inset-0 bg-gray-900 bg-opacity-75 flex items-center justify-center modal-closed">
        <div class="bg-white rounded-lg shadow-xl max-w-lg w-full m-4 relative" onclick="event.stopPropagation();">
            <div class="bg-blue-600 text-white p-4 flex items-center justify-between rounded-t-lg">
                <h5 class="font-bold text-lg" id="infoModalLabel"></h5>
                <button onclick="closeModal('infoModal')" class="text-white hover:text-gray-200 transition-colors" aria-label="Close">
                    <i class="fa fa-times"></i>
                </button>
            </div>
            <div class="p-6 text-slate-800" id="modal-content"></div>
            <div class="bg-gray-100 p-4 flex justify-end rounded-b-lg">
                <button onclick="closeModal('infoModal')" class="bg-gray-400 text-white font-bold py-2 px-4 rounded-lg hover:bg-gray-500 transition-colors">Close</button>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(EndRequestHandler);
        function EndRequestHandler(sender, args) { }

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
    </script>
</asp:Content>
