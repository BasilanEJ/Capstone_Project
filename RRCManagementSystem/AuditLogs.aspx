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

        .date-filters {
            display: flex;
            justify-content: center;
            gap: 10px;
            margin-bottom: 20px;
        }

        .date-filters input,
        .date-filters button {
            padding: 6px 10px;
            font-size: 14px;
        }

        .year-header {
            font-size: 20px;
            font-weight: bold;
            color: #007bff;
            margin-top: 30px;
            margin-bottom: 10px;
        }

        .month-header {
            font-size: 16px;
            font-weight: bold;
            color: #343a40;
            margin-top: 20px;
        }

        .grid {
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 10px;
        }

        .grid th, .grid td {
            padding: 10px;
            border: 1px solid #ccc;
            text-align: center;
        }

        .grid th {
            background-color: #007bff;
            color: #fff;
        }

        .alert-message {
            text-align: center;
            margin-bottom: 10px;
            color: red;
        }
    </style>

    <div class="log-container">
        <h2>Audit Logs</h2>
      <div class="date-filters">
    <div class="row mb-3 align-items-end">
        <div class="col-md-4">
            <label for="txtFrom" class="form-label">From Date</label>
            <asp:TextBox ID="txtFrom" runat="server" CssClass="form-control" type="date" />
        </div>
        <div class="col-md-4">
            <label for="txtTo" class="form-label">To Date</label>
            <asp:TextBox ID="txtTo" runat="server" CssClass="form-control" type="date" />
        </div>
        <div class="col-md-4 d-grid">
            <asp:Label runat="server" AssociatedControlID="btnFilter" CssClass="form-label">&nbsp;</asp:Label>
            <asp:Button ID="btnFilter" runat="server" Text="Filter" CssClass="btn btn-primary" OnClick="btnFilter_Click" />
        </div>
    </div>
</div>
         <asp:Label ID="lblNoData" runat="server" CssClass="text-danger text-center d-block mb-3" Visible="false" />

        <asp:Label ID="lblMessage" runat="server" CssClass="alert-message" />

        <asp:Repeater ID="rptYears" runat="server" OnItemDataBound="rptYears_ItemDataBound">
            <ItemTemplate>
                <div class="year-header">Year: <%# Eval("Year") %></div>
                <asp:Repeater ID="rptMonths" runat="server" OnItemDataBound="rptMonths_ItemDataBound">
                    <ItemTemplate>
                        <div class="month-header">Month: <%# Eval("MonthName") %></div>
                        <asp:GridView ID="gvLogs" runat="server" CssClass="grid" AutoGenerateColumns="False">
                            <Columns>
                                <asp:BoundField DataField="LogID" HeaderText="Log ID" />
                                <asp:BoundField DataField="AdminName" HeaderText="User Name" />
                                <asp:BoundField DataField="Action" HeaderText="Action" />
                                <asp:BoundField DataField="Timestamp" HeaderText="Timestamp" DataFormatString="{0:yyyy-MM-dd HH:mm:ss}" />
                            </Columns>
                        </asp:GridView>
                    </ItemTemplate>
                </asp:Repeater>
            </ItemTemplate>
        </asp:Repeater>
    </div>
</asp:Content>
