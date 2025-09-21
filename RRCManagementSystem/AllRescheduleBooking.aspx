<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AllRescheduleBooking.aspx.cs" Inherits="RRCManagementSystem.AllRescheduleBooking" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <script src="https://cdn.tailwindcss.com"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" />

    <style>
        /* Modern Table */
        .grid-modern th {
            padding: 12px;
            background-color: #f9fafb; /* gray-50 */
            color: #374151; /* gray-700 */
            text-align: left;
            font-weight: 600;
            font-size: 14px;
            border-bottom: 2px solid #e5e7eb; /* gray-200 */
        }

        .grid-modern td {
            padding: 12px;
            font-size: 14px;
            color: #4b5563; /* gray-600 */
            border-bottom: 1px solid #e5e7eb; /* gray-200 */
        }

        .grid-modern tr:hover td {
            background-color: #f3f4f6; /* gray-100 */
        }

        /* Rounded card look */
        .grid-wrapper {
            border: 1px solid #e5e7eb; /* gray-200 */
            border-radius: 0.75rem;
            overflow: hidden;
            background: white;
            box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
        }

        /* Pagination */
        .custom-pager {
            display: flex;
            justify-content: center;
            gap: 6px;
            padding: 12px;
        }

        .custom-pager a, .custom-pager span {
            display: inline-block;
            padding: 6px 12px;
            font-size: 14px;
            border: 1px solid #d1d5db;
            border-radius: 6px;
            text-decoration: none;
            color: #2563eb; /* blue-600 */
            transition: background 0.2s ease, color 0.2s ease;
        }

        .custom-pager a:hover {
            background-color: #2563eb;
            color: white;
        }

        .custom-pager span {
            background-color: #2563eb;
            color: white;
            font-weight: 600;
        }
    </style>

    <div class="container mx-auto p-6">
        <div class="bg-white rounded-xl shadow-lg p-6">
            <h3 class="text-2xl font-bold text-gray-800 text-center mb-6">
                🔁 All Rescheduled Operations
            </h3>

            <!-- Filters -->
            <div class="bg-gray-50 p-4 rounded-lg shadow-inner mb-6 border border-gray-200">
                <h4 class="text-lg font-semibold text-gray-700 mb-4">Filters</h4>
                <div class="grid grid-cols-1 md:grid-cols-5 gap-4">
                    <div class="col-span-2">
                        <label class="block text-gray-700 font-medium mb-1">Search</label>
                        <asp:TextBox ID="txtSearch" runat="server"
                            CssClass="w-full px-4 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                            placeholder="Booking Code / Client / Service">
                        </asp:TextBox>
                    </div>

                    <div>
                        <label class="block text-gray-700 font-medium mb-1">Status</label>
                        <asp:DropDownList ID="ddlFilterStatus" runat="server"
                            CssClass="w-full border rounded-md px-4 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500">
                            <asp:ListItem Text="All" Value="" />
                            <asp:ListItem Text="Pending" Value="Pending" />
                            <asp:ListItem Text="Rejected" Value="Rejected" />
                            <asp:ListItem Text="Approved" Value="Approved" />
                            <asp:ListItem Text="In Progress" Value="InProgress" />
                            <asp:ListItem Text="Completed" Value="Completed" />
                            <asp:ListItem Text="Cancelled" Value="Cancelled" />
                        </asp:DropDownList>
                    </div>

                    <div>
                        <label class="block text-gray-700 font-medium mb-1">From</label>
                        <asp:TextBox ID="txtFrom" runat="server" TextMode="Date"
                            CssClass="w-full px-4 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500">
                        </asp:TextBox>
                    </div>

                    <div>
                        <label class="block text-gray-700 font-medium mb-1">To</label>
                        <asp:TextBox ID="txtTo" runat="server" TextMode="Date"
                            CssClass="w-full px-4 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500">
                        </asp:TextBox>
                    </div>
                </div>

                <div class="flex justify-end mt-4 space-x-3">
                    <asp:Button ID="btnFilter" runat="server"
                        Text="Apply Filter"
                        CssClass="px-5 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition" OnClick="btnFilter_Click" />
                    <asp:Button ID="btnReset" runat="server"
                        Text="Reset"
                        CssClass="px-5 py-2 bg-gray-300 text-gray-800 rounded-md hover:bg-gray-400 transition" OnClick="btnReset_Click" />
                </div>
            </div>

            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <div class="grid-wrapper overflow-x-auto">
                        <asp:GridView ID="gvReschedules" runat="server"
                            AutoGenerateColumns="False"
                            AllowPaging="True" PageSize="10"
                            CssClass="min-w-full grid-modern"
                            PagerStyle-CssClass="custom-pager"
                            OnPageIndexChanging="gvReschedules_PageIndexChanging"
                            OnRowCommand="gvReschedules_RowCommand"
                            OnRowDataBound="gvReschedules_RowDataBound">
                            
                            <Columns>
                                <asp:BoundField DataField="BookingID" HeaderText="Booking ID" Visible="false" />
                                <asp:BoundField DataField="BookingCode" HeaderText="Booking Code" />
                                <asp:BoundField DataField="ClientName" HeaderText="Client Name" />
                                <asp:BoundField DataField="ServiceName" HeaderText="Service" />
                                <asp:BoundField DataField="OperationNumber" HeaderText="Operation #" />
                                <asp:BoundField DataField="ScheduledDate" HeaderText="Scheduled Date" DataFormatString="{0:yyyy-MM-dd}" />

                             
                                <asp:TemplateField HeaderText="Status">
                                    <ItemTemplate>
                                        <asp:DropDownList ID="ddlStatus" runat="server"
                                            CssClass="px-2 py-1 bg-gray-200 rounded-md text-sm border">
                                            <asp:ListItem Text="Pending" Value="Pending" />
                                            <asp:ListItem Text="Rejected" Value="Rejected" />
                                            <asp:ListItem Text="Approved" Value="Approved" />
                                            <asp:ListItem Text="In Progress" Value="InProgress" />
                                            <asp:ListItem Text="Completed" Value="Completed" />
                                            <asp:ListItem Text="Cancelled" Value="Cancelled" />
                                        </asp:DropDownList>
                                    </ItemTemplate>
                                </asp:TemplateField>

                         
                                <asp:TemplateField HeaderText="Action">
                                    <ItemTemplate>
                                        <asp:Button ID="btnUpdate" runat="server"
                                            Text="Update" CommandName="UpdateStatus"
                                            CommandArgument='<%# Eval("ScheduleID") %>'
                                            CssClass="px-4 py-1 bg-blue-500 text-white rounded hover:bg-blue-600 text-sm"
                                            OnClientClick="return confirmUpdate(this);" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="gvReschedules" EventName="PageIndexChanging" />
                </Triggers>
            </asp:UpdatePanel>

         
            <asp:Label ID="lblMessage" runat="server" CssClass="text-center block mt-4 font-semibold text-green-600"></asp:Label>
        </div>
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
