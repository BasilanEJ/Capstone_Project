<%@ Page Title="" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="AuditLogs.aspx.cs" Inherits="RRCManagementSystem.AuditLogs" %>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .log-container {
            width: 90%;
            margin: 40px auto;
            background-color: #fff;
            padding: 25px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
            border-radius: 10px;
        }

        .log-container h2 {
            text-align: center;
            color: #333;
            margin-bottom: 20px;
        }

        .grid {
            width: 100%;
            border-collapse: collapse;
        }

        .grid th, .grid td {
            padding: 12px;
            border: 1px solid #ddd;
            text-align: center;
        }

        .grid th {
            background-color: #007bff;
            color: #fff;
        }

        .grid tr:nth-child(even) {
            background-color: #f9f9f9;
        }

        .alert-message {
            text-align: center;
            margin-bottom: 10px;
            color: red;
        }
    </style>

    <div class="log-container">
        <h2>Audit Logs</h2>

        <asp:Label ID="lblMessage" runat="server" CssClass="alert-message" />

        <asp:GridView ID="gvAuditLogs" runat="server" CssClass="grid" AutoGenerateColumns="False" EmptyDataText="No logs found.">
            <Columns>
                <asp:BoundField DataField="LogID" HeaderText="Log ID" />
                <asp:BoundField DataField="AdminName" HeaderText="Admin Name" />
                <asp:BoundField DataField="Action" HeaderText="Action Performed" />
                <asp:BoundField DataField="Timestamp" HeaderText="Date & Time" DataFormatString="{0:yyyy-MM-dd HH:mm:ss}" />
            </Columns>
        </asp:GridView>
    </div>

</asp:Content>




