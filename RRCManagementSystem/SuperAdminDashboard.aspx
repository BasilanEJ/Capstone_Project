<%@ Page Title="" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="SuperAdminDashboard.aspx.cs" Inherits="RRCManagementSystem.SuperAdminDashboard" %>

    
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .dashboard-container {
            display: flex;
            flex-wrap: wrap;
            gap: 20px;
        }

        .dashboard-card {
            background-color: #ffffff;
            border-radius: 8px;
            box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
            flex: 1 1 250px;
            padding: 20px;
            text-align: center;
            transition: transform 0.3s;
        }

        .dashboard-card:hover {
            transform: translateY(-5px);
        }

        .dashboard-card h3 {
            font-size: 28px;
            color: #111827;
            margin: 0;
        }

        .dashboard-card p {
            font-size: 16px;
            color: #6b7280;
        }

        .recent-logs {
            margin-top: 40px;
        }

        .recent-logs h2 {
            margin-bottom: 20px;
        }

        .table-container {
            overflow-x: auto;
        }

        .table-logs {
            width: 100%;
            border-collapse: collapse;
            background-color: #ffffff;
        }

        .table-logs th, .table-logs td {
            padding: 12px 15px;
            border: 1px solid #e5e7eb;
        }

        .table-logs th {
            background-color: #f3f4f6;
            color: #374151;
        }

        .table-logs tr:hover {
            background-color: #f9fafb;
        }

    </style>

    <h1>Welcome to the SuperAdmin Dashboard</h1>

    <div class="dashboard-container">

        <!-- Total Admin Accounts -->
        <div class="dashboard-card">
            <h3><asp:Label ID="lblTotalAdmins" runat="server" Text="0"></asp:Label></h3>
            <p>Total Admin Accounts</p>
        </div>

        <!-- Audit Logs -->
        <div class="dashboard-card">
            <h3><asp:Label ID="lblAuditLogs" runat="server" Text="0"></asp:Label></h3>
            <p>Audit Logs</p>
        </div>

    </div>

    <!-- Recent Logs Section -->
    <div class="recent-logs">
        <h2>Recent Audit Logs</h2>

        <div class="table-container">
            <asp:GridView ID="gvAuditLogs" runat="server" AutoGenerateColumns="False" CssClass="table-logs">
                <Columns>
                    <asp:BoundField DataField="LogID" HeaderText="Log ID" />
                    <asp:BoundField DataField="AdminName" HeaderText="Admin Name" />
                    <asp:BoundField DataField="Action" HeaderText="Action" />
                    <asp:BoundField DataField="Timestamp" HeaderText="Timestamp" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
                </Columns>
            </asp:GridView>
        </div>
    </div>

</asp:Content>
