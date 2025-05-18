<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="BookingHistory.aspx.cs" Inherits="RRCManagementSystem.BookingHistory" %>


<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <style>
    body {
        font-family: 'Segoe UI', sans-serif;
    }

    h2 {
        text-align: center;
        color: #1f2937;
        margin-top: 30px;
        font-weight: 600;
    }

    .filter-form {
        display: flex;
        flex-wrap: wrap;
        justify-content: center;
        gap: 20px;
        background-color: #f1f5f9;
        padding: 20px;
        margin: 20px auto;
        width: 95%;
        border-radius: 10px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.05);
    }

    .filter-form label {
        font-weight: 500;
        color: #374151;
        margin-right: 5px;
    }

    .form-control {
        padding: 6px 10px;
        border: 1px solid #cbd5e1;
        border-radius: 6px;
        min-width: 160px;
        font-size: 14px;
    }

    .btn-filter {
        padding: 8px 16px;
        background-color: #2563eb;
        color: #ffffff;
        border: none;
        border-radius: 6px;
        cursor: pointer;
        font-weight: 600;
        transition: background-color 0.3s ease;
    }

    .btn-filter:hover {
        background-color: #1d4ed8;
    }

    .grid-container {
        width: 95%;
        margin: 0 auto 40px auto;
        background-color: #ffffff;
        border-radius: 10px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.05);
        overflow-x: auto;
    }

    .table {
        width: 100%;
        border-collapse: collapse;
    }

    .table th {
        background-color: #1e3a8a;
        color: white;
        padding: 12px;
        text-align: center;
        font-weight: 600;
    }

    .table td {
        padding: 10px;
        border: 1px solid #e5e7eb;
        text-align: center;
        font-size: 14px;
        color: #374151;
    }

    .table tr:nth-child(even) {
        background-color: #f9fafb;
    }

    .message {
        display: block;
        text-align: center;
        margin-top: 15px;
        color: #dc2626;
        font-weight: 500;
    }
</style>


    <h2>Booking History</h2>

    <div class="filter-form">
        <label>Start Date:</label>
        <asp:TextBox ID="txtStartDate" runat="server" TextMode="Date" CssClass="form-control" />
        <label>End Date:</label>
        <asp:TextBox ID="txtEndDate" runat="server" TextMode="Date" CssClass="form-control" />
        
        <label>Status:</label>
        <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-control">
            <asp:ListItem Text="All" Value="" />
            <asp:ListItem Text="Pending" Value="Pending" />
            <asp:ListItem Text="Approved" Value="Approved" />
            <asp:ListItem Text="Assigned" Value="Assigned" />
            <asp:ListItem Text="Completed" Value="Completed" />
            <asp:ListItem Text="Cancelled" Value="Cancelled" />
            <asp:ListItem Text="Rejected" Value="Rejected" />
        </asp:DropDownList>

        <asp:Button ID="btnFilter" runat="server" Text="Filter" CssClass="btn-filter" OnClick="btnFilter_Click" />
    </div>

    <div class="grid-container">
        <asp:GridView ID="gvBookingHistory" runat="server" AutoGenerateColumns="False" CssClass="table table-bordered" AllowPaging="true" PageSize="10" OnPageIndexChanging="gvBookingHistory_PageIndexChanging">
            <Columns>
                <asp:BoundField DataField="BookingID" HeaderText="Booking ID" />
                <asp:BoundField DataField="ClientName" HeaderText="Client Name" />
                <asp:BoundField DataField="ServiceNames" HeaderText="Services" />
                <asp:BoundField DataField="ScheduledDate" HeaderText="Scheduled Date" DataFormatString="{0:yyyy-MM-dd}" />
                <asp:BoundField DataField="StartTime" HeaderText="Start Time" />
                <asp:BoundField DataField="Status" HeaderText="Status" />
                <asp:BoundField DataField="Price" HeaderText="Price (₱)" DataFormatString="{0:N2}" />
            </Columns>
        </asp:GridView>
    </div>

    <asp:Label ID="lblMessage" runat="server" CssClass="message" />

</asp:Content>


