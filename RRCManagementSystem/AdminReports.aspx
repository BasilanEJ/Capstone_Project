<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AdminReports.aspx.cs" Inherits="RRCManagementSystem.AdminReports" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" rel="stylesheet" />

    <div class="container mx-auto px-4 py-8 md:py-12">
        
        <div class="text-center mb-10">
            <asp:Label ID="lblMessage" runat="server" CssClass="text-green-500 font-medium mb-2" />
            <h1 class="text-3xl md:text-4xl font-extrabold text-gray-800 tracking-tight inline-block pb-2 border-b-4 border-blue-500">
                📊 System-Wide Detailed Reports
            </h1>
        </div>
        
   <div class="bg-white shadow-md rounded-xl p-6 mb-8 flex flex-col md:flex-row items-center justify-center space-y-4 md:space-y-0 md:space-x-6">
    <div class="flex flex-col sm:flex-row items-center space-y-2 sm:space-y-0 sm:space-x-4 w-full md:w-auto">
        <label class="font-semibold text-gray-700">From:</label>
        <asp:TextBox ID="txtFromDate" runat="server" TextMode="Date" CssClass="p-2 border border-gray-300 rounded-lg w-full sm:w-auto" />
        <label class="font-semibold text-gray-700">To:</label>
        <asp:TextBox ID="txtToDate" runat="server" TextMode="Date" CssClass="p-2 border border-gray-300 rounded-lg w-full sm:w-auto" />
    </div>
    <div class="flex space-x-4 w-full md:w-auto">
        <asp:Button ID="btnFilter" runat="server" Text="Filter" 
            CssClass="w-full bg-blue-600 text-white font-semibold py-2 px-4 rounded-lg hover:bg-blue-700 transition duration-200" 
            OnClick="btnFilter_Click" />
        <asp:Button ID="btnExportPDF" runat="server" Text="Export All to PDF" 
            UseSubmitBehavior="true"
            CausesValidation="false"
            CssClass="w-full bg-gray-600 text-white font-semibold py-2 px-4 rounded-lg hover:bg-gray-700 transition duration-200" 
            OnClick="btnExportPDF_Click" />
    </div>
</div>

        <asp:UpdatePanel ID="ReportsUpdatePanel" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
              
                <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
                    <div class="bg-white shadow-md rounded-xl p-6 text-center transition-transform duration-200 hover:scale-[1.02]">
                        <h4 class="text-lg font-medium text-gray-500 mb-2">Total Inquiries</h4>
                        <asp:Label ID="lblTotalInquiries" runat="server" CssClass="text-3xl font-extrabold text-blue-600" />
                    </div>
                    <div class="bg-white shadow-md rounded-xl p-6 text-center transition-transform duration-200 hover:scale-[1.02]">
                        <h4 class="text-lg font-medium text-gray-500 mb-2">Approved Clients</h4>
                        <asp:Label ID="lblTotalClients" runat="server" CssClass="text-3xl font-extrabold text-blue-600" />
                    </div>
                    <div class="bg-white shadow-md rounded-xl p-6 text-center transition-transform duration-200 hover:scale-[1.02]">
                        <h4 class="text-lg font-medium text-gray-500 mb-2">Available Equipment</h4>
                        <asp:Label ID="lblTotalEquipment" runat="server" CssClass="text-3xl font-extrabold text-blue-600" />
                    </div>
                    <div class="bg-white shadow-md rounded-xl p-6 text-center transition-transform duration-200 hover:scale-[1.02]">
                        <h4 class="text-lg font-medium text-gray-500 mb-2">Total Bookings</h4>
                        <asp:Label ID="lblTotalBookings" runat="server" CssClass="text-3xl font-extrabold text-blue-600" />
                    </div>
                </div>

                <div class="folder-tab-navigation flex flex-wrap justify-start border-b border-gray-300 mb-8 max-w-full overflow-x-auto">
                    <asp:Button ID="btnTabUsers" runat="server" Text="User Accounts" CssClass="folder-tab" OnClick="TabButton_Click" />
                    <asp:Button ID="btnTabInquiries" runat="server" Text="Inquiries" CssClass="folder-tab" OnClick="TabButton_Click" />
                    <asp:Button ID="btnTabClients" runat="server" Text="Clients" CssClass="folder-tab" OnClick="TabButton_Click" />
                      <asp:Button ID="btnTabInventory" runat="server" Text="Inventory Details" CssClass="folder-tab" OnClick="TabButton_Click" />
                    <asp:Button ID="btnTabInventorySnapshots" runat="server" Text="Total Stocks" CssClass="folder-tab" OnClick="TabButton_Click" />
                    <asp:Button ID="btnTabEquipment" runat="server" Text="Equipment" CssClass="folder-tab" OnClick="TabButton_Click" />
                    <asp:Button ID="btnTabSales" runat="server" Text="Sales" CssClass="folder-tab" OnClick="TabButton_Click" />
                    <asp:Button ID="btnTabBookings" runat="server" Text="Bookings" CssClass="folder-tab" OnClick="TabButton_Click" />
                     <asp:Button ID="btnTabInspectionReports" runat="server" Text="Inspection Reports" CssClass="folder-tab" OnClick="TabButton_Click" />
                    <asp:Button ID="btnTabTeams" runat="server" Text="Team Reports" CssClass="folder-tab" OnClick="TabButton_Click" />
                </div>

                <asp:Panel ID="pnlUsers" runat="server" Visible="true" CssClass="report-panel">
                    <div class="flex items-center justify-between mb-4">
                        <h3 class="text-2xl font-semibold text-gray-800">👥 Users Accounts (Exclude SuperAdmin)</h3>
                        <asp:Button ID="btnExportUsers" runat="server" Text="Export Users to PDF" CssClass="bg-blue-600 text-white font-semibold py-2 px-4 rounded-lg hover:bg-blue-700 transition duration-200" OnClick="btnExportUsers_Click" />
                    </div>
                    <div class="overflow-x-auto bg-white rounded-lg shadow-md">
                        <asp:GridView ID="gvUserAccounts" runat="server" AutoGenerateColumns="False" CssClass="min-w-full custom-table" HeaderStyle-CssClass="bg-gray-100 font-semibold text-gray-700 uppercase tracking-wider" RowStyle-CssClass="border-b border-gray-200 hover:bg-gray-50" TableSection-THead="true">
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
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlInquiries" runat="server" Visible="false" CssClass="report-panel">
                    <div class="flex items-center justify-between mb-4">
                        <h3 class="text-2xl font-semibold text-gray-800">📬 Inquiries</h3>
                        <asp:Button ID="btnExportInquiries" runat="server" Text="Export Inquiries to PDF" CssClass="bg-blue-600 text-white font-semibold py-2 px-4 rounded-lg hover:bg-blue-700 transition duration-200" OnClick="btnExportInquiries_Click" />
                    </div>
                    <div class="overflow-x-auto bg-white rounded-lg shadow-md">
                        <asp:GridView ID="gvInquiries" runat="server" AutoGenerateColumns="False" CssClass="min-w-full custom-table" HeaderStyle-CssClass="bg-gray-100 font-semibold text-gray-700 uppercase tracking-wider" RowStyle-CssClass="border-b border-gray-200 hover:bg-gray-50">
                            <Columns>
                                <asp:TemplateField HeaderText="ID">
                                    <ItemTemplate><%# "Inq" + String.Format("{0:D4}", Eval("InquiryID")) %></ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="Email" HeaderText="Email" />
                                <asp:BoundField DataField="Contact" HeaderText="Contact" />
                                <asp:BoundField DataField="FullName" HeaderText="Client Name" />
                                <asp:BoundField DataField="Address" HeaderText="Address" />
                                <asp:BoundField DataField="SubmittedAt" HeaderText="Date Sent" />
                            </Columns>
                        </asp:GridView>
                    </div>
                </asp:Panel>

              <asp:Panel ID="pnlClients" runat="server" Visible="false" CssClass="report-panel">
    <div class="flex items-center justify-between mb-4">
        <h3 class="text-2xl font-semibold text-gray-800">✅ Approved Clients</h3>
        <asp:Button ID="btnExportClients" runat="server" Text="Export Clients to PDF" CssClass="bg-blue-600 text-white font-semibold py-2 px-4 rounded-lg hover:bg-blue-700 transition duration-200" OnClick="btnExportClients_Click" />
    </div>
    <div class="overflow-x-auto bg-white rounded-lg shadow-md">
        <asp:GridView ID="gvApprovedClients" runat="server" AutoGenerateColumns="False"
            CssClass="min-w-full custom-table"
            HeaderStyle-CssClass="bg-gray-100 font-semibold text-gray-700 uppercase tracking-wider"
            RowStyle-CssClass="border-b border-gray-200 hover:bg-gray-50">

            <Columns>
                <asp:BoundField DataField="ClientNumber" HeaderText="Client Number" />
                <asp:BoundField DataField="FullName" HeaderText="Name" />
                <asp:BoundField DataField="Email" HeaderText="Email" />
                <asp:BoundField DataField="Address" HeaderText="Address" />
                <asp:BoundField DataField="CreatedAt" HeaderText="Date Created" DataFormatString="{0:yyyy-MM-dd}" />
            </Columns>
        </asp:GridView>
    </div>
</asp:Panel>


             

                <asp:Panel ID="pnlInventory" runat="server" Visible="false" CssClass="report-panel">
                    <div class="flex items-center justify-between mb-4">
                        <h3 class="text-2xl font-semibold text-gray-800">📦 Inventory Details</h3>
                        <asp:Button ID="btnExportInventory" runat="server" Text="Export Inventory to PDF" CssClass="bg-blue-600 text-white font-semibold py-2 px-4 rounded-lg hover:bg-blue-700 transition duration-200" OnClick="btnExportInventory_Click" />
                    </div>
                    <div class="overflow-x-auto bg-white rounded-lg shadow-md">
                        <asp:GridView ID="gvInventory" runat="server" AutoGenerateColumns="False" CssClass="min-w-full custom-table" HeaderStyle-CssClass="bg-gray-100 font-semibold text-gray-700 uppercase tracking-wider" RowStyle-CssClass="border-b border-gray-200 hover:bg-gray-50">
                            <Columns>
                                <asp:TemplateField HeaderText="Item ID">
                                    <ItemTemplate><%# "Itemid" + String.Format("{0:D4}", Eval("ItemID")) %></ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="Name" HeaderText="Item Name" />
                                <asp:BoundField DataField="Quantity" HeaderText="Stocks" />
                            </Columns>
                        </asp:GridView>
                    </div>
                </asp:Panel>

                   <asp:Panel ID="pnlInventorySnapshots" runat="server" Visible="false" CssClass="report-panel">
       <div class="flex items-center justify-between mb-4">
           <h3 class="text-2xl font-semibold text-gray-800">📦 Total Stocks Snapshot (Daily)</h3>
           <asp:Button ID="btnExportInventorySnapshots" runat="server" Text="Export Snapshots to PDF" CssClass="bg-blue-600 text-white font-semibold py-2 px-4 rounded-lg hover:bg-blue-700 transition duration-200" OnClick="btnExportInventorySnapshots_Click" />
       </div>
       <div class="overflow-x-auto bg-white rounded-lg shadow-md">
           <asp:GridView ID="gvInventorySnapshots" runat="server" AutoGenerateColumns="False" CssClass="min-w-full custom-table" HeaderStyle-CssClass="bg-gray-100 font-semibold text-gray-700 uppercase tracking-wider" RowStyle-CssClass="border-b border-gray-200 hover:bg-gray-50">
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
       </div>
   </asp:Panel>


                <asp:Panel ID="pnlEquipment" runat="server" Visible="false" CssClass="report-panel">
                    <div class="flex items-center justify-between mb-4">
                        <h3 class="text-2xl font-semibold text-gray-800">🛠️ Equipment Status</h3>
                        <asp:Button ID="btnExportEquipment" runat="server" Text="Export Equipment to PDF" CssClass="bg-blue-600 text-white font-semibold py-2 px-4 rounded-lg hover:bg-blue-700 transition duration-200" OnClick="btnExportEquipment_Click" />
                    </div>
                    <div class="overflow-x-auto bg-white rounded-lg shadow-md">
                   <asp:GridView ID="gvEquipment" runat="server" AutoGenerateColumns="False"
    CssClass="min-w-full custom-table"
    HeaderStyle-CssClass="bg-gray-100 font-semibold text-gray-700 uppercase tracking-wider"
    RowStyle-CssClass="border-b border-gray-200 hover:bg-gray-50">
    <Columns>
        <asp:TemplateField HeaderText="ID">
            <ItemTemplate><%# "Equip" + String.Format("{0:D4}", Eval("EquipmentID")) %></ItemTemplate>
        </asp:TemplateField>
        <asp:BoundField DataField="Name" HeaderText="Equipment Name" />
        <asp:BoundField DataField="StatusToday" HeaderText="Status Today" />
    </Columns>
</asp:GridView>



                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlSales" runat="server" Visible="false" CssClass="report-panel">
                    <div class="flex items-center justify-between mb-4">
                        <h3 class="text-2xl font-semibold text-gray-800">💰 Sales</h3>
                        <asp:Button ID="btnExportSales" runat="server" Text="Export Sales to PDF" CssClass="bg-blue-600 text-white font-semibold py-2 px-4 rounded-lg hover:bg-blue-700 transition duration-200" OnClick="btnExportSales_Click" />
                    </div>
                    <div class="text-gray-500 text-sm mb-4">
                        <asp:Label ID="lblSalesSummary" runat="server"></asp:Label>
                    </div>
                    <div class="overflow-x-auto bg-white rounded-lg shadow-md">
                        <asp:GridView ID="gvSales" runat="server" AutoGenerateColumns="False" CssClass="min-w-full custom-table" HeaderStyle-CssClass="bg-gray-100 font-semibold text-gray-700 uppercase tracking-wider" RowStyle-CssClass="border-b border-gray-200 hover:bg-gray-50">
                            <Columns>
                                <asp:BoundField DataField="TransactionIDFormatted" HeaderText="Txn ID" />
                                <asp:BoundField DataField="ClientName" HeaderText="Client" />
                                <asp:BoundField DataField="Amount" HeaderText="Amount ₱" DataFormatString="{0:N2}" HtmlEncode="False" />
                                <asp:BoundField DataField="PaymentMethod" HeaderText="Method" />
                                <asp:BoundField DataField="Status" HeaderText="Status" />
                                <asp:BoundField DataField="TransactionDatePHT" HeaderText="Date" />
                                <asp:BoundField DataField="Remarks" HeaderText="Remarks" />
                            </Columns>
                        </asp:GridView>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlBookings" runat="server" Visible="false" CssClass="report-panel">
                    <div class="flex items-center justify-between mb-4">
                        <h3 class="text-2xl font-semibold text-gray-800">📅 Booking Details</h3>
                        <asp:Button ID="btnExportBookings" runat="server" Text="Export Bookings to PDF" CssClass="bg-blue-600 text-white font-semibold py-2 px-4 rounded-lg hover:bg-blue-700 transition duration-200" OnClick="btnExportBookings_Click" />
                    </div>
                    <div class="overflow-x-auto bg-white rounded-lg shadow-md">
                        <asp:GridView ID="gvBookings" runat="server" AutoGenerateColumns="False" CssClass="min-w-full custom-table" HeaderStyle-CssClass="bg-gray-100 font-semibold text-gray-700 uppercase tracking-wider" RowStyle-CssClass="border-b border-gray-200 hover:bg-gray-50">
                            <Columns>
                                <asp:BoundField DataField="BookingCode" HeaderText="Booking Code" />
                                <asp:BoundField DataField="ClientName" HeaderText="Client" />
                                <asp:BoundField DataField="Services" HeaderText="Service" />
                                <asp:BoundField DataField="TeamName" HeaderText="Assigned Team" />
                                <asp:BoundField DataField="ScheduledDate" HeaderText="Schedule" DataFormatString="{0:yyyy-MM-dd}" />
                                <asp:BoundField DataField="Status" HeaderText="Status" />
                            </Columns>
                        </asp:GridView>
                    </div>
                </asp:Panel>


                <asp:Panel ID="pnlInspectionReports" runat="server" Visible="false" CssClass="report-panel">
    <div class="flex items-center justify-between mb-4">
        <h3 class="text-2xl font-semibold text-gray-800">📋 Inspection Reports Summary</h3>
        <asp:Button ID="btnExportInspectionReports" runat="server" Text="Export Reports to PDF" 
            CssClass="bg-blue-600 text-white font-semibold py-2 px-4 rounded-lg hover:bg-blue-700 transition duration-200" 
            OnClick="btnExportInspectionReports_Click" />
    </div>
    <div class="overflow-x-auto bg-white rounded-lg shadow-md">
        <asp:GridView ID="gvInspectionReports" runat="server" AutoGenerateColumns="False" 
            CssClass="min-w-full custom-table" 
            HeaderStyle-CssClass="bg-gray-100 font-semibold text-gray-700 uppercase tracking-wider" 
            RowStyle-CssClass="border-b border-gray-200 hover:bg-gray-50">
            <Columns>
                <asp:BoundField DataField="QuotationCode" HeaderText="Quotation Code" />
                <asp:BoundField DataField="InquiryNumber" HeaderText="Inquiry #" />
                <asp:BoundField DataField="ClientName" HeaderText="Client" />
                <asp:BoundField DataField="InspectorName" HeaderText="Inspector" />
                <asp:BoundField DataField="PestType" HeaderText="Pest Type" />
                <asp:BoundField DataField="InfestationLevel" HeaderText="Infestation Level" />
                <asp:BoundField DataField="TotalEstimatedCost" HeaderText="Estimated Cost ₱" DataFormatString="{0:N2}" HtmlEncode="False" />
                <asp:BoundField DataField="Status" HeaderText="Status" />
                <asp:BoundField DataField="SubmittedAt" HeaderText="Submitted Date" DataFormatString="{0:yyyy-MM-dd}" />
            </Columns>
        </asp:GridView>
    </div>
</asp:Panel>


                <asp:Panel ID="pnlTeams" runat="server" Visible="false" CssClass="report-panel">
                    <div class="flex items-center justify-between mb-4">
                        <h3 class="text-2xl font-semibold text-gray-800">🧑‍🤝‍🧑 Team Reports</h3>
                        <div class="flex flex-col sm:flex-row items-center space-y-2 sm:space-y-0 sm:space-x-4">
                            <label class="font-semibold text-gray-700">Availability Date:</label>
                            <asp:TextBox ID="txtTeamDate" runat="server" TextMode="Date" CssClass="p-2 border border-gray-300 rounded-lg" />
                            <asp:Button ID="btnTeamDateApply" runat="server" Text="Apply" CssClass="bg-blue-600 text-white font-semibold py-2 px-4 rounded-lg hover:bg-blue-700 transition duration-200" OnClick="btnTeamDateApply_Click" />
                        </div>
                    </div>

                    <div class="mb-8">
                        <div class="flex items-center justify-between mb-4">
                            <h4 class="text-xl font-semibold text-gray-700">📋 Team Summary (by date & range)</h4>
                            <asp:Button ID="btnExportTeamsSummary" runat="server" Text="Export Summary to PDF" CssClass="bg-blue-600 text-white font-semibold py-2 px-4 rounded-lg hover:bg-blue-700 transition duration-200" OnClick="btnExportTeamsSummary_Click" />
                        </div>
                        <div class="overflow-x-auto bg-white rounded-lg shadow-md">
                           <asp:GridView ID="gvTeamsSummary" runat="server" AutoGenerateColumns="False" 
    CssClass="min-w-full custom-table" 
    HeaderStyle-CssClass="bg-gray-100 font-semibold text-gray-700 uppercase tracking-wider" 
    RowStyle-CssClass="border-b border-gray-200 hover:bg-gray-50">
    <Columns>
        <asp:TemplateField HeaderText="Team">
            <ItemTemplate><%# "Team" + String.Format("{0:D3}", Eval("TeamID")) %> — <%# Eval("GroupName") %></ItemTemplate>
        </asp:TemplateField>
        <asp:BoundField DataField="ShiftType" HeaderText="Shift" />
        <asp:BoundField DataField="TeamLeader" HeaderText="Team Leader" />
        <asp:BoundField DataField="MembersCount" HeaderText="Members" />
        <asp:BoundField DataField="AssignmentsOnDate" HeaderText="Jobs on Availability Date" />
        <asp:BoundField DataField="AssignmentsInRange" HeaderText="Total Assignments in Date Range" />
        <asp:BoundField DataField="LastScheduled" HeaderText="Last Scheduled" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
        <asp:BoundField DataField="Status" HeaderText="Status" />
    </Columns>
</asp:GridView>
                        </div>
                    </div>

                    <div>
                        <div class="flex items-center justify-between mb-4">
                            <h4 class="text-xl font-semibold text-gray-700">👥 Team Members (roster)</h4>
                            <asp:Button ID="btnExportTeamMembers" runat="server" Text="Export Team Members to PDF" CssClass="bg-blue-600 text-white font-semibold py-2 px-4 rounded-lg hover:bg-blue-700 transition duration-200" OnClick="btnExportTeamMembers_Click" />
                        </div>
                        <div class="overflow-x-auto bg-white rounded-lg shadow-md">
                           <asp:GridView ID="gvTeamMembers" runat="server" AutoGenerateColumns="False" 
    CssClass="min-w-full custom-table" 
    HeaderStyle-CssClass="bg-gray-100 font-semibold text-gray-700 uppercase tracking-wider" 
    RowStyle-CssClass="border-b border-gray-200 hover:bg-gray-50">
    <Columns>
        <asp:TemplateField HeaderText="Team">
            <ItemTemplate><%# "Team" + String.Format("{0:D3}", Eval("TeamID")) %> — <%# Eval("GroupName") %></ItemTemplate>
        </asp:TemplateField>
        <asp:BoundField DataField="ShiftType" HeaderText="Shift" />
        <asp:BoundField DataField="TeamLeader" HeaderText="Team Leader" />
        <asp:BoundField DataField="EmployeeID" HeaderText="Emp ID" />
        <asp:TemplateField HeaderText="Member">
            <ItemTemplate><%# Eval("LastName") %>, <%# Eval("FirstName") %> <%# Eval("MiddleName") %></ItemTemplate>
        </asp:TemplateField>
        <asp:BoundField DataField="Department" HeaderText="Department" />
    </Columns>
</asp:GridView>
                        </div>
                    </div>
                </asp:Panel>
            </ContentTemplate>

                <Triggers>
        <asp:PostBackTrigger ControlID="btnExportUsers" />
        <asp:PostBackTrigger ControlID="btnExportInquiries" />
        <asp:PostBackTrigger ControlID="btnExportClients" />
        <asp:PostBackTrigger ControlID="btnExportInventory" />
        <asp:PostBackTrigger ControlID="btnExportInventorySnapshots" />
        <asp:PostBackTrigger ControlID="btnExportEquipment" />
        <asp:PostBackTrigger ControlID="btnExportSales" />
        <asp:PostBackTrigger ControlID="btnExportBookings" />
        <asp:PostBackTrigger ControlID="btnExportInspectionReports" />
        <asp:PostBackTrigger ControlID="btnExportTeamsSummary" />
        <asp:PostBackTrigger ControlID="btnExportTeamMembers" />
    </Triggers>

        </asp:UpdatePanel>
    </div>


 
   <style>
        .folder-tab {
            background-color: #e5e7eb;
            color: #4b5563;
            font-weight: 600;
            padding: 0.75rem 1.5rem;
            margin-bottom: -1px;
            border-top-left-radius: 0.5rem;
            border-top-right-radius: 0.5rem;
            transition: 0.2s;
            cursor: pointer;
            white-space: nowrap;
            border: 1px solid transparent;
            border-bottom: none;
        }
        .folder-tab:hover { background-color: #d1d5db; }
        .folder-tab.active-tab {
            background-color: #ffffff;
            color: #2563eb;
            font-weight: 700;
            border-color: #d1d5db;
            z-index: 2;
        }

        .custom-table th, .custom-table td {
            padding: 1rem;
            border: 1px solid #e5e7eb;
        }
        .custom-table th {
            background-color: #f3f4f6;
            font-weight: 600;
            color: #4b5563;
        }

        .report-panel {
            background-color: #ffffff;
            border-radius: 0.5rem;
            box-shadow: 0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1);
            padding: 1.5rem;
            border: 1px solid #d1d5db;
            margin-top: -1px;
        }
      
    </style>
</asp:Content>