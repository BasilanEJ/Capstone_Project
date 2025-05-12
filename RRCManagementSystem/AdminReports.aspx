        <%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AdminReports.aspx.cs" Inherits="RRCManagementSystem.AdminReports" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f5f6fa;
            margin: 0;
            padding: 0;
            color: #333;
        }

        .page-title {
            text-align: center;
            font-size: 28px;
            font-weight: 600;
            color: #2c3e50;
            margin: 30px 0 20px;
            border-bottom: 2px solid #ccc;
            padding-bottom: 10px;
        }

        .metrics-container {
            display: flex;
            flex-wrap: wrap;
            justify-content: space-evenly;
            margin: 20px 0;
        }

        .metric-card {
            background-color: #ffffff;
            border: 1px solid #e0e0e0;
            border-radius: 10px;
            width: 200px;
            text-align: center;
            padding: 15px 10px;
            margin: 10px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.08);
            transition: all 0.3s ease-in-out;
        }

        .metric-card h4 {
            font-size: 16px;
            color: #555;
            margin-bottom: 8px;
        }

        .metric-value {
            font-size: 22px;
            font-weight: bold;
            color: #2980b9;
        }

        .custom-table {
            width: 100%;
            margin-top: 10px;
            border-collapse: collapse;
            font-size: 14px;
            background-color: #fff;
            box-shadow: 0 2px 6px rgba(0,0,0,0.05);
        }

        .custom-table th, .custom-table td {
            border: 1px solid #ddd;
            padding: 10px 12px;
            text-align: left;
        }

        .custom-table th {
            background-color: #f0f2f5;
            font-weight: bold;
            color: #333;
        }

        .custom-table tr:nth-child(even) {
            background-color: #fafafa;
        }

        .custom-table tr:hover {
            background-color: #f1f7fd;
        }

        h3 {
            margin-top: 40px;
            font-size: 20px;
            font-weight: 600;
            color: #2c3e50;
            border-left: 5px solid #3498db;
            padding-left: 10px;
            margin-bottom: 10px;
        }

        .btn-sales {
            background-color: #2980b9;
            color: #fff;
            padding: 8px 16px;
            font-size: 13px;
            border: none;
            margin-top: 10px;
            border-radius: 5px;
            cursor: pointer;
            transition: background-color 0.25s ease;
        }

        .btn-sales:hover {
            background-color: #2471a3;
        }

        label {
            margin: 0 5px;
            font-weight: 500;
        }

        input[type="date"] {
            padding: 5px 8px;
            font-size: 13px;
            border: 1px solid #ccc;
            border-radius: 4px;
        }
    </style>
    <asp:Label ID="lblMessage" runat="server" CssClass="message" />

    <h2 class="page-title">📊 System-Wide Detailed Reports</h2>

    <!-- Date Range Filter -->
    <div style="text-align:center; margin-bottom: 30px;">

        <label>From:</label>
        <asp:TextBox ID="txtFromDate" runat="server" TextMode="Date" />
        <label>To:</label>
        <asp:TextBox ID="txtToDate" runat="server" TextMode="Date" />
        <asp:Button ID="btnFilter" runat="server" Text="Filter" CssClass="btn-sales" OnClick="btnFilter_Click" />
        <asp:Button ID="btnExportPDF" runat="server" Text="Export to PDF" CssClass="btn-sales" OnClick="btnExportPDF_Click" />
    </div>

    <div class="metrics-container">
        <div class="metric-card"><h4>Total Inquiries</h4><asp:Label ID="lblTotalInquiries" runat="server" CssClass="metric-value" /></div>
        <div class="metric-card"><h4>Approved Clients</h4><asp:Label ID="lblTotalClients" runat="server" CssClass="metric-value" /></div>
        <!--<div class="metric-card"><h4>Total Items in Stock</h4><asp:Label ID="lblTotalStock" runat="server" CssClass="metric-value" /></div> -->
        <div class="metric-card"><h4>Available Equipment</h4><asp:Label ID="lblTotalEquipment" runat="server" CssClass="metric-value" /></div>
        <div class="metric-card"><h4>Total Bookings</h4><asp:Label ID="lblTotalBookings" runat="server" CssClass="metric-value" /></div>
       <!-- <div class="metric-card"><h4>Total Suppliers</h4><asp:Label ID="lblTotalSuppliers" runat="server" CssClass="metric-value" /></div> -->
    </div>

    <h3>👥 Users Accounts (Exclude SuperAdmin)</h3>
<asp:GridView ID="gvUserAccounts" runat="server" AutoGenerateColumns="False" CssClass="custom-table" ShowHeaderWhenEmpty="True">
    <Columns>
        <asp:BoundField DataField="UserID" HeaderText="User ID" />
        <asp:BoundField DataField="FullName" HeaderText="Name" />
        <asp:BoundField DataField="Email" HeaderText="Email" />
        <asp:BoundField DataField="Role" HeaderText="Role" />
        <asp:BoundField DataField="CreatedAt" HeaderText="Date Created" DataFormatString="{0:yyyy-MM-dd}" />
    </Columns>
</asp:GridView>

<asp:Button ID="btnExportUsers" runat="server" Text="Export Users to PDF" CssClass="btn-sales" OnClick="btnExportUsers_Click" />

<h3>📬 Inquiries</h3>
<asp:GridView ID="gvInquiries" runat="server" AutoGenerateColumns="False" CssClass="custom-table" ShowHeaderWhenEmpty="True">
    <Columns>
        <asp:BoundField DataField="InquiryID" HeaderText="ID" />
        <asp:BoundField DataField="Email" HeaderText="Email" />
        <asp:BoundField DataField="ContactNumber" HeaderText="Contact" />
        <asp:BoundField DataField="Name" HeaderText="Name" />
        <asp:BoundField DataField="Address" HeaderText="Address" />
        <asp:BoundField DataField="SubmittedAt" HeaderText="Date Sent" />
    </Columns>
</asp:GridView>

<asp:Button ID="btnExportInquiries" runat="server" Text="Export Inquiries to PDF" CssClass="btn-sales" OnClick="btnExportInquiries_Click" />


    <h3>✅ Approved Clients</h3>
    <asp:GridView ID="gvApprovedClients" runat="server" AutoGenerateColumns="False" CssClass="custom-table" ShowHeaderWhenEmpty="True">
        <Columns>
            <asp:BoundField DataField="ClientID" HeaderText="Client ID" />
            <asp:BoundField DataField="Name" HeaderText="Name" />
            <asp:BoundField DataField="Email" HeaderText="Email" />
            <asp:BoundField DataField="Address" HeaderText="Address" />
           <asp:BoundField DataField="CreatedAt" HeaderText="Date Created" DataFormatString="{0:yyyy-MM-dd}" />
        </Columns>
    </asp:GridView>
    <asp:Button ID="btnExportClients" runat="server" Text="Export Clients to PDF" CssClass="btn-sales" OnClick="btnExportClients_Click" />

       <h3>📦 Total Stocks Snapshot (Daily)</h3>
    <asp:GridView ID="gvInventorySnapshots" runat="server" AutoGenerateColumns="False" CssClass="custom-table">
        <Columns>
            <asp:BoundField DataField="Name" HeaderText="Item Name" />
            <asp:BoundField DataField="Type" HeaderText="Type" />
            <asp:BoundField DataField="Quantity" HeaderText="Quantity" />
            <asp:BoundField DataField="ExcessML" HeaderText="Excess (mL)" />
            <asp:BoundField DataField="SnapshotDate" HeaderText="Snapshot Date" />
        </Columns>
    </asp:GridView>


    <h3>📦 Inventory Details</h3>
    <asp:GridView ID="gvInventory" runat="server" AutoGenerateColumns="False" CssClass="custom-table" ShowHeaderWhenEmpty="True">
        <Columns>
            <asp:BoundField DataField="ItemID" HeaderText="Item ID" />
            <asp:BoundField DataField="Name" HeaderText="Item Name" />
            <asp:BoundField DataField="Quantity" HeaderText="Stocks" />
        </Columns>
    </asp:GridView>
    <asp:Button ID="btnExportInventory" runat="server" Text="Export Inventory to PDF" CssClass="btn-sales" OnClick="btnExportInventory_Click" />



    <h3>🛠️ Equipment Status</h3>
    <asp:GridView ID="gvEquipment" runat="server" AutoGenerateColumns="False" CssClass="custom-table" ShowHeaderWhenEmpty="True">
        <Columns>
            <asp:BoundField DataField="EquipmentID" HeaderText="ID" />
            <asp:BoundField DataField="Name" HeaderText="Equipment Name" />
            <asp:BoundField DataField="Status" HeaderText="Status" />
        </Columns>
    </asp:GridView>
    <asp:Button ID="btnExportEquipment" runat="server" Text="Export Equipment to PDF" CssClass="btn-sales" OnClick="btnExportEquipment_Click" />

    <h3>📅 Booking Details</h3>
    <asp:GridView ID="gvBookings" runat="server" AutoGenerateColumns="False" CssClass="custom-table" ShowHeaderWhenEmpty="True">
        <Columns>
            <asp:BoundField DataField="BookingID" HeaderText="Booking ID" />
            <asp:BoundField DataField="ClientName" HeaderText="Client" />
            <asp:BoundField DataField="Service" HeaderText="Service" />
            <asp:BoundField DataField="TeamName" HeaderText="Assigned Team" />
            <asp:BoundField DataField="ScheduledDate" HeaderText="Schedule" />
            <asp:BoundField DataField="Status" HeaderText="Status" />
        </Columns>
    </asp:GridView>
    <asp:Button ID="btnExportBookings" runat="server" Text="Export Bookings to PDF" CssClass="btn-sales" OnClick="btnExportBookings_Click" />

 <h3>🔍 Inspection Details</h3>
<asp:GridView ID="gvInspections" runat="server" AutoGenerateColumns="False" CssClass="custom-table" ShowHeaderWhenEmpty="True">
    <Columns>
        <asp:BoundField DataField="InspectionID" HeaderText="Inspection ID" />
        <asp:BoundField DataField="InspectorName" HeaderText="Inspector Name" />    
        <asp:BoundField DataField="ClientName" HeaderText="Client Name" />
        <asp:BoundField DataField="ClientAddress" HeaderText="Client Address" />
        <asp:BoundField DataField="ScheduledDate" HeaderText="Scheduled Date" />
        <asp:BoundField DataField="InspectionStatus" HeaderText="Status" />
        <asp:BoundField DataField="Remarks" HeaderText="Remarks" />
    </Columns>
</asp:GridView>


<asp:Button ID="btnExportInspections" runat="server" Text="Export Inspections to PDF" CssClass="btn-sales" OnClick="btnExportInspections_Click" />


</asp:Content>