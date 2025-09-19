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
                PagerStyle-CssClass="px-4 py-2 border-t border-gray-200 text-sm flex justify-center items-center"
                PagerSettings-Mode="Numeric"
                PagerSettings-Position="Bottom"
                PagerSettings-PageButtonCount="5">
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
            return false; // prevent default postback
        }
    </script>

    <script>
        // JavaScript for dynamic styling of the status labels
        function applyStatusStyles(label, status) {
            label.className = "font-semibold"; // Reset classes
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
                    // Keep default styling
                    break;
            }
        }

        document.addEventListener("DOMContentLoaded", function () {
            // Reapply styles after partial postback
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

        // ASP.NET Web Forms might not handle client-side changes gracefully on postback.
        // It's recommended to handle this in the RowDataBound event in the C# code-behind for robustness.
    </script>
</asp:Content>