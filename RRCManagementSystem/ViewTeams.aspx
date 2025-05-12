<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ViewTeams.aspx.cs" Inherits="RRCManagementSystem.ViewTeams" %>

<%@ Import Namespace="System.Data" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }

        .main-content {
            padding: 30px;
            background-color: #f8f9fa;
            min-height: calc(100vh - 100px);
        }

        h3 {
            margin-bottom: 20px;
            color: #004085;
        }

        .date-selector {
            margin-bottom: 20px;
        }

        .date-selector input[type="date"] {
            padding: 10px;
            border-radius: 5px;
            border: 1px solid #ced4da;
            margin-right: 10px;
        }

        .date-selector button {
            padding: 10px 20px;
            background-color: #004085;
            color: white;
            border: none;
            border-radius: 5px;
            cursor: pointer;
        }

        .team-container {
            background: #ffffff;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
            margin-bottom: 20px;
        }

        .team-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 15px;
        }

        .team-name {
            font-size: 18px;
            font-weight: 600;
            color: #004085;
        }

        .team-status {
            padding: 5px 10px;
            border-radius: 4px;
            font-size: 12px;
            font-weight: 500;
        }

        .status-available {
            background-color: #28a745;
            color: white;
        }

        .status-unavailable {
            background-color: #dc3545;
            color: white;
        }

        .employee-list {
            list-style: none;
            padding-left: 0;
        }

        .employee-list li {
            padding: 8px;
            border-bottom: 1px solid #e9ecef;
        }

        .employee-list li:last-child {
            border-bottom: none;
        }

        .no-members {
            font-style: italic;
            color: #6c757d;
        }
    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="main-content">
        <h3>Teams and Members Overview</h3>

        <!-- Date Filter -->
        <div class="date-selector">
            <asp:TextBox ID="txtDate" runat="server" TextMode="Date" />
            <asp:Button ID="btnFilterDate" runat="server" Text="Check Availability" OnClick="btnFilterDate_Click" />
        </div>

        <!-- Teams Listing -->
        <asp:Repeater ID="rptTeams" runat="server" OnItemDataBound="rptTeams_ItemDataBound">
            <ItemTemplate>
                <div class="team-container">
                    <div class="team-header">
                        <span class="team-name">Team: <%# Eval("GroupName") %></span>
                        <span class='team-status <%# Eval("Status").ToString() == "Available" ? "status-available" : "status-unavailable" %>'>
                            <%# Eval("Status") %>
                        </span>
                    </div>

                    <ul class="employee-list">
                        <asp:Repeater ID="rptEmployees" runat="server">
                            <ItemTemplate>
                                <li><%# Eval("FullName") %> (<%# Eval("Department") %>)</li>
                            </ItemTemplate>
                        </asp:Repeater>

                        <asp:PlaceHolder ID="phNoMembers" runat="server">
                            <li class="no-members">No members assigned to this team.</li>
                        </asp:PlaceHolder>
                    </ul>
                </div>
            </ItemTemplate>
        </asp:Repeater>

        <asp:Label ID="lblMessage" runat="server" ForeColor="Red" />
    </div>
</asp:Content>