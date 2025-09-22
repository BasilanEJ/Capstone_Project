<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ApproveRescheduleRequests.aspx.cs" Inherits="RRCManagementSystem.ApproveRescheduleRequests" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.tailwindcss.com"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" />
    <style>
        .table-rounded-corners {
            border-collapse: separate;
            border-spacing: 0;
            border-radius: 0.5rem; /* rounded-lg */
            overflow: hidden;
        }

        .table-rounded-corners thead tr:first-child th:first-child {
            border-top-left-radius: 0.5rem;
        }

        .table-rounded-corners thead tr:first-child th:last-child {
            border-top-right-radius: 0.5rem;
        }
    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto p-4 md:p-8 bg-white rounded-lg shadow-lg mt-8">
        <h3 class="text-2xl font-bold mb-6 text-gray-800 text-center">🔁 Approve Reschedule Requests</h3>

        <!-- 🔎 Filters -->
        <div class="bg-gray-50 rounded-lg p-4 mb-6 shadow-sm border border-gray-200">
            <h4 class="text-lg font-semibold text-gray-700 mb-4">Filters</h4>
            <div class="flex flex-wrap items-end gap-4">
                <div class="flex-grow min-w-48">
                    <label class="block text-gray-700 font-medium mb-1">Search (Client / Service)</label>
                    <div class="relative">
                        <asp:TextBox ID="txtSearch" runat="server" CssClass="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500" placeholder="Type a name or service..." />
                        <i class="fas fa-search absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400"></i>
                    </div>
                </div>
                <div class="flex-grow min-w-32">
                    <label class="block text-gray-700 font-medium mb-1">From (Requested)</label>
                    <asp:TextBox ID="txtFrom" runat="server" TextMode="Date" CssClass="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500" />
                </div>
                <div class="flex-grow min-w-32">
                    <label class="block text-gray-700 font-medium mb-1">To (Requested)</label>
                    <asp:TextBox ID="txtTo" runat="server" TextMode="Date" CssClass="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500" />
                </div>
                <div class="flex-grow min-w-32">
                    <label class="block text-gray-700 font-medium mb-1">Status</label>
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="w-full block appearance-none bg-white border border-gray-300 text-gray-700 py-2 px-4 rounded-md leading-tight focus:outline-none focus:ring-2 focus:ring-blue-500">
                        <asp:ListItem Text="All" Value="All" />
                        <asp:ListItem Text="Pending" Value="Pending" Selected="True" />
                        <asp:ListItem Text="Approved" Value="Approved" />
                        <asp:ListItem Text="Rejected" Value="Rejected" />
                    </asp:DropDownList>
                </div>
                <div class="flex-shrink-0 flex gap-2">
                    <asp:Button ID="btnApply" runat="server" CssClass="px-6 py-2 bg-blue-600 text-white font-semibold rounded-md hover:bg-blue-700 transition-colors" Text="Apply"
                        OnClick="btnApply_Click" UseSubmitBehavior="false" />
                    <asp:Button ID="btnClear" runat="server" CssClass="px-6 py-2 bg-gray-300 text-gray-800 font-semibold rounded-md hover:bg-gray-400 transition-colors" Text="Clear"
                        OnClick="btnClear_Click" UseSubmitBehavior="false" />
                </div>
            </div>
        </div>

        <asp:HiddenField ID="hfRequestID" runat="server" />
        <asp:HiddenField ID="hfRejectReason" runat="server" />

        <div class="overflow-x-auto">
            <asp:GridView ID="gvRescheduleRequests" runat="server" AutoGenerateColumns="False"
                CssClass="min-w-full bg-white rounded-lg shadow-md table-rounded-corners"
                AllowPaging="true" PageSize="10" ClientIDMode="Static"
                OnPageIndexChanging="gvRescheduleRequests_PageIndexChanging"
                OnRowCommand="gvRescheduleRequests_RowCommand"
                OnRowDataBound="gvRescheduleRequests_RowDataBound"
                HeaderStyle-CssClass="bg-blue-600 text-white uppercase text-sm leading-normal"
                RowStyle-CssClass="border-b border-gray-200 hover:bg-gray-100 transition-colors"
                AlternatingRowStyle-CssClass="bg-gray-50 hover:bg-gray-100 transition-colors">
                <Columns>
                    <asp:BoundField DataField="BookingCode" HeaderText="Booking Code" 
    ReadOnly="true" 
    HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" 
    ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" />

                    <asp:BoundField DataField="RequestID" HeaderText="Request ID" ReadOnly="true" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" />
                    <asp:BoundField DataField="ClientName" HeaderText="Client" ReadOnly="true" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" />
                    <asp:BoundField DataField="ServiceNames" HeaderText="Service" ReadOnly="true" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" />
                    <asp:BoundField DataField="ScheduledDate" HeaderText="Original Date" DataFormatString="{0:yyyy-MM-dd}" ReadOnly="true" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" />
                    <asp:BoundField DataField="OperationNumber" HeaderText="Operation #" ReadOnly="true" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" />
                    <asp:BoundField DataField="RequestedDate" HeaderText="Requested On" DataFormatString="{0:yyyy-MM-dd}" ReadOnly="true" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" />
                    <asp:BoundField DataField="Status" HeaderText="Status" ReadOnly="true" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center font-semibold border-r border-gray-200" />
                    <asp:TemplateField HeaderText="New Schedule Date" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200">
                        <ItemTemplate>
                            <asp:Label ID="lblNewDate" runat="server" CssClass="px-2 py-1 bg-gray-200 rounded-md text-sm" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Action" HeaderStyle-CssClass="py-3 px-6 text-center" ItemStyle-CssClass="py-3 px-6 text-center space-x-2 whitespace-nowrap">
                        <ItemTemplate>
                            <asp:Button ID="btnApprove" runat="server" Text="Approve" CssClass="px-4 py-2 bg-green-500 text-white font-bold rounded-md hover:bg-green-600 transition-colors text-sm"
                                CommandName="Approve" CommandArgument='<%# Eval("RequestID") %>' UseSubmitBehavior="false" />
                            <asp:Button ID="btnReject" runat="server" Text="Reject" CssClass="px-4 py-2 bg-red-500 text-white font-bold rounded-md hover:bg-red-600 transition-colors text-sm"
                                OnClientClick="return openRejectModal(this);" CommandName="Reject"
                                CommandArgument='<%# Eval("RequestID") %>' UseSubmitBehavior="false" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

        <asp:Label ID="lblMessage" runat="server" CssClass="text-center block mt-4 font-semibold text-green-500" />
    </div>

    <script type="text/javascript">
        function openRejectModal(button) {
            const requestId = button.getAttribute("commandargument") || button.value;
            Swal.fire({
                title: 'Reject Reschedule Request',
                input: 'text',
                inputLabel: 'Enter rejection reason',
                inputPlaceholder: 'Rejection reason...',
                showCancelButton: true,
                confirmButtonText: 'Reject',
                cancelButtonText: 'Cancel',
                confirmButtonColor: '#dc3545',
                cancelButtonColor: '#6c757d',
                inputValidator: (value) => { if (!value) return 'Please enter a reason'; }
            }).then((result) => {
                if (result.isConfirmed) {
                    document.getElementById('<%= hfRequestID.ClientID %>').value = requestId;
                    document.getElementById('<%= hfRejectReason.ClientID %>').value = result.value;
                    __doPostBack(button.name, '');
                }
            });
            return false;
        }
    </script>
</asp:Content>