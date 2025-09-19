<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AllRescheduleBooking.aspx.cs" Inherits="RRCManagementSystem.AllRescheduleBooking" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
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

    <div class="container mx-auto p-4 md:p-8 bg-white rounded-lg shadow-lg mt-8">
        <h3 class="text-2xl font-bold mb-6 text-gray-800 text-center">🔁 All Rescheduled Operations</h3>
    
        <!-- Filters -->
        <div class="bg-gray-50 rounded-lg p-4 mb-6 shadow-sm border border-gray-200">
            <h4 class="text-lg font-semibold text-gray-700 mb-4">Filters</h4>
            <div class="flex flex-wrap items-end gap-4">
                <div class="flex-grow min-w-48">
                    <label class="block text-gray-700 font-medium mb-1">Search (Code / Client / Service)</label>
                    <div class="relative">
                        <asp:TextBox ID="txtSearch" runat="server" CssClass="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500" placeholder="e.g. TD0003 or Juan Dela Cruz or Termite"></asp:TextBox>
                    </div>
                </div>
                <div class="flex-grow min-w-32">
                    <label class="block text-gray-700 font-medium mb-1">Status</label>
                    <asp:DropDownList ID="ddlFilterStatus" runat="server" CssClass="w-full block appearance-none bg-white border border-gray-300 text-gray-700 py-2 px-4 rounded-md leading-tight focus:outline-none focus:ring-2 focus:ring-blue-500">
                        <asp:ListItem Text="All" Value="" />
                        <asp:ListItem Text="Pending" Value="Pending" />
                        <asp:ListItem Text="Rejected" Value="Rejected" />
                        <asp:ListItem Text="Approved" Value="Approved" />
                        <asp:ListItem Text="In Progress" Value="InProgress" />
                        <asp:ListItem Text="Completed" Value="Completed" />
                        <asp:ListItem Text="Cancelled" Value="Cancelled" />
                    </asp:DropDownList>
                </div>
                <div class="flex-grow min-w-32">
                    <label class="block text-gray-700 font-medium mb-1">From</label>
                    <asp:TextBox ID="txtFrom" runat="server" CssClass="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500" TextMode="Date"></asp:TextBox>
                </div>
                <div class="flex-grow min-w-32">
                    <label class="block text-gray-700 font-medium mb-1">To</label>
                    <asp:TextBox ID="txtTo" runat="server" CssClass="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500" TextMode="Date"></asp:TextBox>
                </div>
                <div class="flex-shrink-0 flex gap-2">
                    <asp:Button ID="btnFilter" runat="server" CssClass="px-6 py-2 bg-blue-600 text-white font-semibold rounded-md hover:bg-blue-700 transition-colors" Text="Filter" OnClick="btnFilter_Click" />
                    <asp:Button ID="btnReset" runat="server" CssClass="px-6 py-2 bg-gray-300 text-gray-800 font-semibold rounded-md hover:bg-gray-400 transition-colors" Text="Reset" OnClick="btnReset_Click" />
                </div>
            </div>
        </div>

        <div class="overflow-x-auto">
            <asp:GridView ID="gvReschedules" runat="server" AutoGenerateColumns="False"
                CssClass="min-w-full bg-white rounded-lg shadow-md table-rounded-corners"
                AllowPaging="True" PageSize="10"
                OnPageIndexChanging="gvReschedules_PageIndexChanging"
                OnRowCommand="gvReschedules_RowCommand"
                OnRowDataBound="gvReschedules_RowDataBound"
                HeaderStyle-CssClass="bg-blue-600 text-white uppercase text-sm leading-normal"
                RowStyle-CssClass="border-b border-gray-200 hover:bg-gray-100 transition-colors"
                AlternatingRowStyle-CssClass="bg-gray-50 hover:bg-gray-100 transition-colors">

                <Columns>
                    <asp:BoundField DataField="BookingID" HeaderText="Booking ID" Visible="false" />
                    <asp:BoundField DataField="BookingCode" HeaderText="Booking Code" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" />
                    <asp:BoundField DataField="ClientName" HeaderText="Client Name" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" />
                    <asp:BoundField DataField="ServiceName" HeaderText="Service" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" />
                    <asp:BoundField DataField="OperationNumber" HeaderText="Operation #" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" />
                    <asp:BoundField DataField="ScheduledDate" HeaderText="Scheduled Date" DataFormatString="{0:yyyy-MM-dd}" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" />
                    <asp:TemplateField HeaderText="Status" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200">
                        <ItemTemplate>
                            <asp:DropDownList ID="ddlStatus" runat="server" CssClass="px-2 py-1 bg-gray-200 rounded-md text-sm border-0">
                                <asp:ListItem Text="Pending" Value="Pending" />
                                <asp:ListItem Text="Rejected" Value="Rejected" />
                                <asp:ListItem Text="Approved" Value="Approved" />
                                <asp:ListItem Text="In Progress" Value="InProgress" />
                                <asp:ListItem Text="Completed" Value="Completed" />
                                <asp:ListItem Text="Cancelled" Value="Cancelled" />
                            </asp:DropDownList>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Action" HeaderStyle-CssClass="py-3 px-6 text-center" ItemStyle-CssClass="py-3 px-6 text-center space-x-2 whitespace-nowrap">
                        <ItemTemplate>
                            <asp:Button ID="btnUpdate" runat="server" Text="Update" CommandName="UpdateStatus"
                                CommandArgument='<%# Eval("ScheduleID") %>' CssClass="px-4 py-2 bg-blue-500 text-white font-bold rounded-md hover:bg-blue-600 transition-colors text-sm"
                                OnClientClick="return confirmUpdate(this);" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

        <asp:Label ID="lblMessage" runat="server" CssClass="text-center block mt-4 font-semibold text-green-500" />
    </div>

    <script type="text/javascript">
        function confirmUpdate(button) {
            event.preventDefault();
            Swal.fire({
                title: 'Update Schedule?',
                text: 'Are you sure you want to update this schedule status?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes, update it'
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack(button.name, '');
                }
            });
            return false;
        }
    </script>
</asp:Content>
