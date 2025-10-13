<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="BookingHistory.aspx.cs" Inherits="RRCManagementSystem.BookingHistory" %>

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
    
        /* --- Custom Pagination Styles Start --- */
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
        /* --- Custom Pagination Styles End --- */
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto p-4 md:p-8 bg-white rounded-lg shadow-lg mt-8">
        <h2 class="text-center text-2xl font-bold mb-6 text-gray-800">Booking History</h2>

        <div class="bg-gray-50 rounded-lg p-4 mb-6 shadow-sm border border-gray-200">
            <h4 class="text-lg font-semibold text-gray-700 mb-4">Filter Bookings</h4>
            <div class="flex flex-wrap items-end gap-4">
                <div class="flex-grow min-w-32">
                    <label class="block text-gray-700 font-medium mb-1">Start Date:</label>
                    <asp:TextBox ID="txtStartDate" runat="server" TextMode="Date" CssClass="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500" />
                </div>
                <div class="flex-grow min-w-32">
                    <label class="block text-gray-700 font-medium mb-1">End Date:</label>
                    <asp:TextBox ID="txtEndDate" runat="server" TextMode="Date" CssClass="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500" />
                </div>
                <div class="flex-grow min-w-32">
                    <label class="block text-gray-700 font-medium mb-1">Status:</label>
                    <div class="relative">
                        <asp:DropDownList ID="ddlStatus" runat="server" CssClass="w-full block appearance-none bg-white border border-gray-300 text-gray-700 py-2 px-4 pr-8 rounded-md leading-tight focus:outline-none focus:ring-2 focus:ring-blue-500">
                            <asp:ListItem Text="All" Value="" />
                            <asp:ListItem Text="Pending" Value="Pending" />
                            <asp:ListItem Text="Approved" Value="Approved" />
                            <asp:ListItem Text="Assigned" Value="Assigned" />
                            <asp:ListItem Text="Completed" Value="Completed" />
                            <asp:ListItem Text="Cancelled" Value="Cancelled" />
                            <asp:ListItem Text="Rejected" Value="Rejected" />
                        </asp:DropDownList>
                        <div class="pointer-events-none absolute inset-y-0 right-0 flex items-center px-2 text-gray-700">
                            <i class="fas fa-chevron-down"></i>
                        </div>
                    </div>
                </div>
                <div class="flex-shrink-0">
                    <asp:Button ID="btnFilter" runat="server" Text="Filter" CssClass="w-full px-6 py-2 bg-blue-600 text-white font-semibold rounded-md hover:bg-blue-700 transition-colors" OnClick="btnFilter_Click" />
                </div>
            </div>
        </div>

        <div class="overflow-x-auto shadow-lg rounded-lg">
            <asp:GridView ID="gvBookingHistory" runat="server" AutoGenerateColumns="False" 
                CssClass="min-w-full bg-white table-rounded-corners" AllowPaging="true" PageSize="10" 
                OnPageIndexChanging="gvBookingHistory_PageIndexChanging"
                HeaderStyle-CssClass="bg-blue-600 text-white uppercase text-sm leading-normal"
                RowStyle-CssClass="border-b border-gray-200 hover:bg-gray-100 transition-colors"
                AlternatingRowStyle-CssClass="bg-gray-50 hover:bg-gray-100 transition-colors"
                
                PagerStyle-CssClass="pagination-container"
                PagerSettings-Mode="NumericFirstLast"
                PagerSettings-Position="Bottom"
                PagerSettings-PageButtonCount="5"
                PagerSettings-FirstPageText="<i class='fas fa-angle-double-left'></i>"
                PagerSettings-LastPageText="<i class='fas fa-angle-double-right'></i>"
                PagerSettings-PreviousPageText="<i class='fas fa-angle-left'></i>"
                PagerSettings-NextPageText="<i class='fas fa-angle-right'></i>">


                <Columns>
                    <asp:BoundField DataField="BookingID" HeaderText="Booking ID" Visible="False" />
                    
                    <asp:BoundField DataField="BookingCode" HeaderText="Code" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center font-bold text-blue-700 border-r border-gray-200" />
                    
                    <asp:BoundField DataField="ClientName" HeaderText="Client Name" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" />
                    <asp:BoundField DataField="ServiceNames" HeaderText="Services" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" />
                    <asp:BoundField DataField="ScheduledDate" HeaderText="Scheduled Date" DataFormatString="{0:yyyy-MM-dd}" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" />
                    <asp:BoundField DataField="StartTime" HeaderText="Start Time" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" />
                    <asp:BoundField DataField="Status" HeaderText="Status" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center font-semibold border-r border-gray-200" />
                    <asp:BoundField DataField="Price" HeaderText="Price (₱)" DataFormatString="{0:N2}" HeaderStyle-CssClass="py-3 px-6 text-center" ItemStyle-CssClass="py-3 px-6 text-center" />
                </Columns>
            </asp:GridView>
        </div>

        <asp:Label ID="lblMessage" runat="server" CssClass="d-block text-center text-red-500 font-semibold mt-4" />
    </div>
</asp:Content>