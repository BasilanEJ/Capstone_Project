<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="BookingHistory.aspx.cs" Inherits="RRCManagementSystem.BookingHistory" %>


<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .filter-form {
            margin-bottom: 20px;
            display: flex;
            flex-wrap: wrap;
            gap: 10px;
            align-items: center;
        }

        .filter-form label {
            font-weight: bold;
        }

        .grid-container {
            margin-top: 20px;
        }

        .btn-filter {
            background-color: #004085;
            color: #fff;
            padding: 8px 20px;
            border: none;
            border-radius: 6px;
            cursor: pointer;
            font-weight: bold;
        }

        .btn-filter:hover {
            background-color: #002f6c;
        }

        .message {
            margin-top: 10px;
            font-weight: bold;
            text-align: center;
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


