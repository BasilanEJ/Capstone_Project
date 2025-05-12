<%@ Page Title="" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="Reports.aspx.cs" Inherits="RRCManagementSystem.Reports" %>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .report-container {
            width: 90%;
            margin: 20px auto;
            background: #fff;
            padding: 20px;
            border-radius: 6px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
        }

        .report-container h2 {
            text-align: center;
            margin-bottom: 20px;
        }

        .filters {
            margin-bottom: 20px;
            display: flex;
            justify-content: space-between;
        }

        .filters label {
            font-weight: bold;
        }

        .filters input, .filters select {
            padding: 8px;
            width: 200px;
            margin-left: 10px;
        }

        .grid {
            width: 100%;
            border-collapse: collapse;
        }

        .grid th, .grid td {
            padding: 10px;
            border: 1px solid #ddd;
            text-align: center;
        }

        .grid th {
            background-color: #007bff;
            color: #fff;
        }

        .btn {
            padding: 8px 16px;
            background-color: #007bff;
            color: #fff;
            border: none;
            border-radius: 4px;
            cursor: pointer;
        }

        .btn:hover {
            background-color: #0056b3;
        }

        .message {
            text-align: center;
            color: red;
            margin-bottom: 10px;
        }
    </style>

    <div class="report-container">
        <h2>Reports</h2>

        <asp:Label ID="lblMessage" runat="server" CssClass="message"></asp:Label>

        <div class="filters">
            <div>
                <label for="ddlReportType">Select Report:</label>
                <asp:DropDownList ID="ddlReportType" runat="server" AutoPostBack="true" CssClass="form-control" OnSelectedIndexChanged="ddlReportType_SelectedIndexChanged">
                    <asp:ListItem Text="Select Report" Value="" />
                    <asp:ListItem Text="Admin Activity" Value="AdminActivity" />
                    <asp:ListItem Text="Sales" Value="Sales" />
                    <asp:ListItem Text="Audit Logs" Value="AuditLogs" />
                    <asp:ListItem Text="Work Orders" Value="WorkOrders" />
                </asp:DropDownList>
            </div>

            <div>
                <label for="txtDateFrom">Date From:</label>
                <asp:TextBox ID="txtDateFrom" runat="server" CssClass="form-control" TextMode="Date" />

                <label for="txtDateTo" style="margin-left: 10px;">Date To:</label>
                <asp:TextBox ID="txtDateTo" runat="server" CssClass="form-control" TextMode="Date" />
            </div>

            <div>
                <asp:Button ID="btnGenerate" runat="server" Text="Generate Report" CssClass="btn" OnClick="btnGenerate_Click" />
            </div>
        </div>

        <asp:GridView ID="gvReports" runat="server" CssClass="grid" AutoGenerateColumns="True" EmptyDataText="No data available."></asp:GridView>
    </div>
</asp:Content>

