<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AdminReports.aspx.cs" Inherits="RRCManagementSystem.AdminReports" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<style>
    body {
        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        background-color: #f9fafc;
        color: #2c3e50;
    }

    .page-title {
        text-align: center;
        font-size: 32px;
        font-weight: 700;
        color: #34495e;
        margin: 40px 0 30px;
        border-bottom: 3px solid #3498db;
        display: inline-block;
        padding-bottom: 10px;
    }

    .metrics-container {
        display: flex;
        flex-wrap: wrap;
        justify-content: center;
        gap: 20px;
        margin: 30px auto;
        max-width: 1000px;
    }

    .metric-card {
        background: #ffffff;
        border-radius: 15px;
        box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
        padding: 25px;
        width: 220px;
        text-align: center;
        transition: transform 0.3s ease;
    }

    .metric-card:hover { transform: translateY(-5px); }

    .metric-card h4 { font-size: 18px; color: #7f8c8d; margin-bottom: 10px; }
    .metric-value { font-size: 26px; font-weight: 700; color: #2980b9; }

    .btn-sales {
        background-color: #2980b9;
        color: white;
        border: none;
        padding: 8px 16px;
        border-radius: 5px;
        cursor: pointer;
        font-weight: bold;
        transition: background-color 0.3s;
    }
    .btn-sales:hover { background-color: #2471a3; }

    .custom-table {
        width: 95%;
        margin: 0 auto;
        border-collapse: collapse;
        background: #fff;
        box-shadow: 0 2px 5px rgba(0, 0, 0, 0.05);
    }

    .custom-table th, .custom-table td {
        padding: 12px 15px;
        border: 1px solid #dee2e6;
        text-align: left;
    }

    .custom-table th {
        background-color: #ecf0f1;
        font-weight: bold;
    }

    /* New header bar style */
    .section-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        margin: 40px auto 10px;
        width: 95%;
    }
    .section-header h3 {
        margin: 0;
        font-size: 22px;
        color: #2c3e50;
    }
</style>

<asp:Label ID="lblMessage" runat="server" CssClass="message" />
<h2 class="page-title">📊 System-Wide Detailed Reports</h2>

<div style="text-align:center; margin-bottom: 30px;">
    <label>From:</label>
    <asp:TextBox ID="txtFromDate" runat="server" TextMode="Date" />
    <label>To:</label>
    <asp:TextBox ID="txtToDate" runat="server" TextMode="Date" />
    <asp:Button ID="btnFilter" runat="server" Text="Filter" CssClass="btn-sales" OnClick="btnFilter_Click" />
    <asp:Button ID="btnExportPDF" runat="server" Text="Export to PDF" CssClass="btn-sales" OnClick="btnExportPDF_Click" />
</div>

<div class="metrics-container">
    <div class="metric-card">
        <h4>Total Inquiries</h4>
        <asp:Label ID="lblTotalInquiries" runat="server" CssClass="metric-value" />
    </div>
    <div class="metric-card">
        <h4>Approved Clients</h4>
        <asp:Label ID="lblTotalClients" runat="server" CssClass="metric-value" />
    </div>
    <div class="metric-card">
        <h4>Available Equipment</h4>
        <asp:Label ID="lblTotalEquipment" runat="server" CssClass="metric-value" />
    </div>
    <div class="metric-card">
        <h4>Total Bookings</h4>
        <asp:Label ID="lblTotalBookings" runat="server" CssClass="metric-value" />
    </div>
</div>

<!-- 👥 Users Accounts -->
<div class="section-header">
  <h3>👥 Users Accounts (Exclude SuperAdmin)</h3>
  <asp:Button ID="btnExportUsers" runat="server" Text="Export Users to PDF" CssClass="btn-sales" OnClick="btnExportUsers_Click" />
</div>
<asp:GridView ID="gvUserAccounts" runat="server" AutoGenerateColumns="False" CssClass="custom-table">
    <Columns>
        <asp:TemplateField HeaderText="User ID">
            <ItemTemplate><%# "User" + String.Format("{0:D4}", Eval("UserID")) %></ItemTemplate>
        </asp:TemplateField>
        <asp:BoundField DataField="Name" HeaderText="Name" />
        <asp:BoundField DataField="Email" HeaderText="Email" />
        <asp:BoundField DataField="Role" HeaderText="Role" />
        <asp:BoundField DataField="CreatedAt" HeaderText="Date Created" DataFormatString="{0:yyyy-MM-dd}" />
    </Columns>
</asp:GridView>

<!-- 📬 Inquiries -->
<div class="section-header">
  <h3>📬 Inquiries</h3>
  <asp:Button ID="btnExportInquiries" runat="server" Text="Export Inquiries to PDF" CssClass="btn-sales" OnClick="btnExportInquiries_Click" />
</div>
<asp:GridView ID="gvInquiries" runat="server" AutoGenerateColumns="False" CssClass="custom-table">
    <Columns>
        <asp:TemplateField HeaderText="ID">
            <ItemTemplate><%# "Inq" + String.Format("{0:D4}", Eval("InquiryID")) %></ItemTemplate>
        </asp:TemplateField>
        <asp:BoundField DataField="Email" HeaderText="Email" />
        <asp:BoundField DataField="ContactNumber" HeaderText="Contact" />
        <asp:BoundField DataField="FullName" HeaderText="Client Name" />
        <asp:BoundField DataField="Address" HeaderText="Address" />
        <asp:BoundField DataField="SubmittedAt" HeaderText="Date Sent" />
    </Columns>
</asp:GridView>

<!-- ✅ Approved Clients -->
<div class="section-header">
  <h3>✅ Approved Clients</h3>
  <asp:Button ID="btnExportClients" runat="server" Text="Export Clients to PDF" CssClass="btn-sales" OnClick="btnExportClients_Click" />
</div>
<asp:GridView ID="gvApprovedClients" runat="server" AutoGenerateColumns="False" CssClass="custom-table">
    <Columns>
        <asp:TemplateField HeaderText="Client ID">
            <ItemTemplate><%# "Client" + String.Format("{0:D4}", Eval("ClientID")) %></ItemTemplate>
        </asp:TemplateField>
        <asp:BoundField DataField="FullName" HeaderText="Name" />
        <asp:BoundField DataField="Email" HeaderText="Email" />
        <asp:BoundField DataField="Address" HeaderText="Address" />
        <asp:BoundField DataField="CreatedAt" HeaderText="Date Created" DataFormatString="{0:yyyy-MM-dd}" />
    </Columns>
</asp:GridView>

<!-- 📦 Total Stocks Snapshot -->
<div class="section-header">
  <h3>📦 Total Stocks Snapshot (Daily)</h3>
  <asp:Button ID="btnExportInventorySnapshots" runat="server" Text="Export Inventory Snapshots to PDF" CssClass="btn-sales" OnClick="btnExportInventorySnapshots_Click" />
</div>
<asp:GridView ID="gvInventorySnapshots" runat="server" AutoGenerateColumns="False" CssClass="custom-table">
    <Columns>
        <asp:TemplateField HeaderText="Snapshot ID">
            <ItemTemplate><%# Container.DataItemIndex >= 0 ? "Snap" + String.Format("{0:D4}", Container.DataItemIndex + 1) : "" %></ItemTemplate>
        </asp:TemplateField>
        <asp:BoundField DataField="Name" HeaderText="Item Name" />
        <asp:BoundField DataField="Type" HeaderText="Type" />
        <asp:BoundField DataField="Quantity" HeaderText="Quantity" />
        <asp:BoundField DataField="ExcessML" HeaderText="Excess (mL)" />
        <asp:BoundField DataField="SnapshotDate" HeaderText="Snapshot Date" />
    </Columns>
</asp:GridView>

<!-- 📦 Inventory Details -->
<div class="section-header">
  <h3>📦 Inventory Details</h3>
  <asp:Button ID="btnExportInventory" runat="server" Text="Export Inventory to PDF" CssClass="btn-sales" OnClick="btnExportInventory_Click" />
</div>
<asp:GridView ID="gvInventory" runat="server" AutoGenerateColumns="False" CssClass="custom-table">
    <Columns>
        <asp:TemplateField HeaderText="Item ID">
            <ItemTemplate><%# "Itemid" + String.Format("{0:D4}", Eval("ItemID")) %></ItemTemplate>
        </asp:TemplateField>
        <asp:BoundField DataField="Name" HeaderText="Item Name" />
        <asp:BoundField DataField="Quantity" HeaderText="Stocks" />
    </Columns>
</asp:GridView>

<!-- 💰 Sales -->
<div class="section-header">
  <h3>💰 Sales</h3>
  <asp:Button ID="btnExportSales" runat="server" Text="Export Sales to PDF" CssClass="btn-sales" OnClick="btnExportSales_Click" />
</div>
<div class="mb-2">
    <asp:Label ID="lblSalesSummary" runat="server" CssClass="text-muted"></asp:Label>
</div>
<asp:GridView ID="gvSales" runat="server" AutoGenerateColumns="False" CssClass="custom-table">
    <Columns>
        <asp:BoundField DataField="TransactionIDFormatted" HeaderText="Txn ID" />
        <asp:BoundField DataField="ClientName" HeaderText="Client" />
        <asp:BoundField DataField="Amount" HeaderText="Amount" DataFormatString="{0:N2}" HtmlEncode="False" />
        <asp:BoundField DataField="PaymentMethod" HeaderText="Method" />
        <asp:BoundField DataField="Status" HeaderText="Status" />
        <asp:BoundField DataField="TransactionDatePHT" HeaderText="Date" />
        <asp:BoundField DataField="Remarks" HeaderText="Remarks" />
    </Columns>
</asp:GridView>

<!-- 🛠️ Equipment -->
<div class="section-header">
  <h3>🛠️ Equipment Status</h3>
  <asp:Button ID="btnExportEquipment" runat="server" Text="Export Equipment to PDF" CssClass="btn-sales" OnClick="btnExportEquipment_Click" />
</div>
<asp:GridView ID="gvEquipment" runat="server" AutoGenerateColumns="False" CssClass="custom-table">
    <Columns>
        <asp:TemplateField HeaderText="ID">
            <ItemTemplate><%# "Equip" + String.Format("{0:D4}", Eval("EquipmentID")) %></ItemTemplate>
        </asp:TemplateField>
        <asp:BoundField DataField="Name" HeaderText="Equipment Name" />
     <asp:BoundField DataField="StatusToday" HeaderText="Status Today" />
    </Columns>
</asp:GridView>

<!-- 📅 Bookings -->
<div class="section-header">
  <h3>📅 Booking Details</h3>
  <asp:Button ID="btnExportBookings" runat="server" Text="Export Bookings to PDF" CssClass="btn-sales" OnClick="btnExportBookings_Click" />
</div>
<asp:GridView ID="gvBookings" runat="server" AutoGenerateColumns="False" CssClass="custom-table">
    <Columns>
        <asp:TemplateField HeaderText="Booking ID">
            <ItemTemplate><%# "Booking" + String.Format("{0:D4}", Eval("BookingID")) %></ItemTemplate>
        </asp:TemplateField>
        <asp:BoundField DataField="ClientName" HeaderText="Client" />
        <asp:BoundField DataField="Services" HeaderText="Service" />
        <asp:BoundField DataField="TeamName" HeaderText="Assigned Team" />
        <asp:BoundField DataField="ScheduledDate" HeaderText="Schedule" />
        <asp:BoundField DataField="Status" HeaderText="Status" />
    </Columns>
</asp:GridView>

<!-- 🔍 Inspections -->
<div class="section-header">
  <h3>🔍 Inspection Details</h3>
  <asp:Button ID="btnExportInspections" runat="server" Text="Export Inspections to PDF" CssClass="btn-sales" OnClick="btnExportInspections_Click" />
</div>
<asp:GridView ID="gvInspections" runat="server" AutoGenerateColumns="False" CssClass="custom-table">
    <Columns>
        <asp:TemplateField HeaderText="Inspection ID">
            <ItemTemplate><%# "Inspect" + String.Format("{0:D4}", Eval("InspectionID")) %></ItemTemplate>
        </asp:TemplateField>
        <asp:BoundField DataField="InspectorName" HeaderText="Inspector Name" />
        <asp:BoundField DataField="ClientName" HeaderText="Client Name" />
        <asp:BoundField DataField="ClientAddress" HeaderText="Client Address" />
        <asp:BoundField DataField="ScheduledDate" HeaderText="Scheduled Date" />
        <asp:BoundField DataField="InspectionStatus" HeaderText="Status" />
        <asp:BoundField DataField="Remarks" HeaderText="Remarks" />
    </Columns>
</asp:GridView>

<!-- 🧑‍🤝‍🧑 Team Reports -->
<div class="section-header">
  <h3>🧑‍🤝‍🧑 Team Reports</h3>
</div>
<div style="text-align:center; margin-bottom: 18px;">
    <label>Availability Date:</label>
    <asp:TextBox ID="txtTeamDate" runat="server" TextMode="Date" />
    <asp:Button ID="btnTeamDateApply" runat="server" Text="Apply" CssClass="btn-sales" OnClick="btnTeamDateApply_Click" />
</div>

<!-- 📋 Team Summary -->
<div class="section-header">
  <h3>📋 Team Summary (by date & range)</h3>
  <asp:Button ID="btnExportTeamsSummary" runat="server" Text="Export Team Summary to PDF" CssClass="btn-sales" OnClick="btnExportTeamsSummary_Click" />
</div>
<asp:GridView ID="gvTeamsSummary" runat="server" AutoGenerateColumns="False" CssClass="custom-table">
    <Columns>
        <asp:TemplateField HeaderText="Team">
            <ItemTemplate><%# "Team" + String.Format("{0:D3}", Eval("TeamID")) %> — <%# Eval("GroupName") %></ItemTemplate>
        </asp:TemplateField>
        <asp:BoundField DataField="MembersCount" HeaderText="Members" />
        <asp:BoundField DataField="AssignmentsOnDate" HeaderText="Jobs on Availability Date" />
        <asp:BoundField DataField="AssignmentsInRange" HeaderText="Total Assignments in Date Range" />
        <asp:BoundField DataField="LastScheduled" HeaderText="Last Scheduled" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
        <asp:BoundField DataField="Status" HeaderText="Status" />
    </Columns>
</asp:GridView>

<!-- 👥 Team Members -->
<div class="section-header">
  <h3>👥 Team Members (roster)</h3>
  <asp:Button ID="btnExportTeamMembers" runat="server" Text="Export Team Members to PDF" CssClass="btn-sales" OnClick="btnExportTeamMembers_Click" />
</div>
<asp:GridView ID="gvTeamMembers" runat="server" AutoGenerateColumns="False" CssClass="custom-table">
    <Columns>
        <asp:TemplateField HeaderText="Team">
            <ItemTemplate><%# "Team" + String.Format("{0:D3}", Eval("TeamID")) %> — <%# Eval("GroupName") %></ItemTemplate>
        </asp:TemplateField>
        <asp:BoundField DataField="EmployeeID" HeaderText="Emp ID" />
        <asp:TemplateField HeaderText="Member">
            <ItemTemplate><%# Eval("LastName") %>, <%# Eval("FirstName") %> <%# Eval("MiddleName") %></ItemTemplate>
        </asp:TemplateField>
    </Columns>
</asp:GridView>

</asp:Content>
