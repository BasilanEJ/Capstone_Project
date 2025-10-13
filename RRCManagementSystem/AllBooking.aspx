<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AllBooking.aspx.cs" Inherits="RRCManagementSystem.AllBooking" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto p-4 md:p-8 bg-white rounded-lg shadow-lg mt-8">
        <h3 class="text-2xl font-bold mb-6 text-gray-800">All Bookings Overview</h3>

        <div class="flex flex-col md:flex-row md:items-center gap-4 mb-6">
            <div class="relative flex-grow">
                <asp:TextBox ID="txtSearch" runat="server"
                    placeholder="Search by Booking Code, Client, or Service"
                    AutoPostBack="true"
                    OnTextChanged="txtSearch_TextChanged"
                    CssClass="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500" />
                <i class="fas fa-search absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400"></i>
            </div>
            <div class="relative">
                <asp:DropDownList ID="ddlStatusFilter" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlStatusFilter_SelectedIndexChanged" CssClass="block appearance-none w-full bg-white border border-gray-300 text-gray-700 py-2 px-4 pr-8 rounded-md leading-tight focus:outline-none focus:bg-white focus:border-gray-500">
                    <asp:ListItem Text="All Statuses" Value="" />
                    <asp:ListItem Text="Pending" Value="Pending" />
                    <asp:ListItem Text="Ongoing" Value="Ongoing" />
                    <asp:ListItem Text="Completed" Value="Completed" />
                    <asp:ListItem Text="Cancelled" Value="Cancelled" />
                    <asp:ListItem Text="Assigned" Value="Assigned" />
                    <asp:ListItem Text="Approved" Value="Approved" />
                    <asp:ListItem Text="Confirmed" Value="Confirmed" />
                </asp:DropDownList>
                <div class="pointer-events-none absolute inset-y-0 right-0 flex items-center px-2 text-gray-700">
                    <i class="fas fa-chevron-down"></i>
                </div>
            </div>
        </div>

        <div class="overflow-x-auto">
          <asp:GridView ID="gvBookings" runat="server" AutoGenerateColumns="False"
                CssClass="min-w-full bg-white rounded-lg shadow-md"
                DataKeyNames="BookingID"
                AllowPaging="True" PageSize="10"
                OnPageIndexChanging="gvBookings_PageIndexChanging"
                OnRowCommand="gvBookings_RowCommand"
                OnRowDataBound="gvBookings_RowDataBound"
                HeaderStyle-CssClass="bg-blue-600 text-white uppercase text-sm leading-normal"
                RowStyle-CssClass="border-b border-gray-200 hover:bg-gray-100 transition-colors"
                PagerStyle-CssClass="pagination-container"
                PagerSettings-Mode="NumericFirstLast"
                PagerSettings-Position="Bottom"
                PagerSettings-PageButtonCount="5"
                PagerSettings-FirstPageText="<i class='fas fa-angle-double-left'></i>"
                PagerSettings-LastPageText="<i class='fas fa-angle-double-right'></i>"
                PagerSettings-PreviousPageText="<i class='fas fa-angle-left'></i>"
                PagerSettings-NextPageText="<i class='fas fa-angle-right'></i>">
                <Columns>
                    <asp:BoundField DataField="BookingID" HeaderText="Booking ID" Visible="false" />
                    <asp:BoundField DataField="BookingCode" HeaderText="Booking Code" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" />
                    <asp:BoundField DataField="ClientName" HeaderText="Client Name" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" />
                    <asp:BoundField DataField="ServiceName" HeaderText="Service" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" />
                    <asp:BoundField DataField="ScheduledDate" HeaderText="Date" DataFormatString="{0:yyyy-MM-dd}" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" />
                    <asp:BoundField DataField="StartTime" HeaderText="Time" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" />
                    <asp:BoundField DataField="Price" HeaderText="Price" DataFormatString="₱ {0:N2}" HtmlEncode="false" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" />
                    <asp:BoundField DataField="Status" HeaderText="Status" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center font-semibold border-r border-gray-200" />
                    <asp:BoundField DataField="CreatedAt" HeaderText="Date Booked" DataFormatString="{0:yyyy-MM-dd}" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" />
                    <asp:TemplateField HeaderText="Op1 Status" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200">
                        <ItemTemplate>
                            <asp:Label ID="lblOp1Status" runat="server"
                                Text='<%# Eval("Op1Status") == null ? "—" : Eval("Op1Status").ToString() %>' CssClass="font-semibold" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Actions" HeaderStyle-CssClass="py-3 px-6 text-center" ItemStyle-CssClass="py-3 px-6 text-center space-x-2 whitespace-nowrap">
                        <ItemTemplate>
                            <asp:Button ID="btnEdit" runat="server" Text="Edit"
                                CommandName="EditBooking"
                                CommandArgument='<%# Eval("BookingID") %>'
                                CssClass="bg-blue-500 hover:bg-blue-600 text-white font-bold py-2 px-4 rounded transition-colors duration-200 text-sm" />
                            <asp:Button ID="btnTriggerCompleteOp1" runat="server"
                                Text="Mark Op1 Complete"
                                CssClass="bg-green-500 hover:bg-green-600 text-white font-bold py-2 px-4 rounded transition-colors duration-200 text-sm"
                                OnClientClick='<%# "return confirmCompleteOp1(" + Eval("BookingID") + ");" %>'
                                UseSubmitBehavior="false" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>


        <asp:HiddenField ID="hfBookingIDToComplete" runat="server" />
        <asp:Button ID="btnHiddenCompleteOp1" runat="server" Style="display:none;" OnClick="btnHiddenCompleteOp1_Click" UseSubmitBehavior="false" />

        <asp:Label ID="lblMessage" runat="server" CssClass="mt-4 text-red-500 font-semibold" />
    </div>

    <style>
        /* Custom Pagination Styles */
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

        /* Page number links */
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

        /* Current page (selected) */
        .pagination-container span {
            background-color: #3b82f6;
            color: white;
            border: 1px solid #3b82f6;
            font-weight: 600;
            box-shadow: 0 2px 4px rgba(59, 130, 246, 0.3);
        }

        /* First/Last/Prev/Next buttons with icons */
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

        /* Icon sizing */
        .pagination-container i {
            font-size: 1rem;
        }

        /* Responsive adjustments */
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

    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <script type="text/javascript">
        function confirmCompleteOp1(bookingId) {
            Swal.fire({
                title: 'Mark Operation 1 as Complete?',
                text: 'This action cannot be undone.',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Yes, mark it!',
                cancelButtonText: 'Cancel'
            }).then((result) => {
                if (result.isConfirmed) {
                    document.getElementById('<%= hfBookingIDToComplete.ClientID %>').value = bookingId;
                    document.getElementById('<%= btnHiddenCompleteOp1.ClientID %>').click();
                }
            });
            return false;
        }
    </script>

    <script>
        function applyStatusStyles(label, status) {
            label.className = "font-semibold";
            switch (status) {
                case "Assigned":
                    label.classList.add("text-blue-500");
                    break;
                case "Pending":
                    label.classList.add("text-yellow-500");
                    break;
                case "Cancelled":
                case "Rejected":
                    label.classList.add("text-red-500");
                    break;
                case "Completed":
                    label.classList.add("text-green-500");
                    break;
                default:
                    break;
            }
        }

        document.addEventListener("DOMContentLoaded", function () {
            const gridView = document.getElementById('<%= gvBookings.ClientID %>');
            if (gridView) {
                const rows = gridView.querySelectorAll("tr");
                rows.forEach(row => {
                    const statusCell = row.querySelector("td:nth-child(8)");
                    if (statusCell) {
                        const statusLabel = statusCell.querySelector("span");
                        if (statusLabel) {
                            applyStatusStyles(statusLabel, statusLabel.innerText.trim());
                        }
                    }
                    const op1StatusCell = row.querySelector("td:nth-child(10)");
                    if (op1StatusCell) {
                        const op1StatusLabel = op1StatusCell.querySelector("span");
                        if (op1StatusLabel) {
                            applyStatusStyles(op1StatusLabel, op1StatusLabel.innerText.trim());
                        }
                    }
                });
            }
        });
    </script>
</asp:Content>