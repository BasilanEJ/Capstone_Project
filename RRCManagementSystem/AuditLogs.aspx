<%@ Page Title="" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="AuditLogs.aspx.cs" Inherits="RRCManagementSystem.AuditLogs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    
    <script src="https://cdn.tailwindcss.com"></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" />
    <style>
        /* General container for the entire page content */
        .log-container {
            width: 95%; /* Adjusted for a slightly wider card */
            max-width: 1200px; /* Prevents it from getting too wide on large screens */
            margin: 40px auto;
            background-color: #fff;
            padding: 25px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
            border-radius: 10px;
        }

        /* Centering the heading */
        .log-container h2 {
            text-align: center;
            color: #333;
            margin-bottom: 20px;
            font-size: 1.75rem;
            font-weight: bold;
        }

        /* Flexbox for centering date filters */
        .date-filters {
            display: flex;
            justify-content: center;
            gap: 15px;
            margin-bottom: 20px;
        }

        .date-filters input,
        .date-filters button {
            padding: 8px 12px;
            font-size: 14px;
            text-align: center;
            border-radius: 6px;
        }
        
        /* Centering the GridView and making it responsive */
        .grid {
            width: 95%; /* This is the key to centering */
            margin: 20px auto; /* Centers the grid and adds vertical spacing */
            border-collapse: collapse;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05); /* Optional: Adds a subtle shadow */
            overflow-x: auto; /* Adds horizontal scroll on small screens */
        }

        .grid th,
        .grid td {
            padding: 12px;
            border: 1px solid #e0e0e0; /* Lighter border color for a cleaner look */
            text-align: center;
            font-size: 14px;
            vertical-align: middle;
        }

        .grid th {
            background-color: #007bff;
            color: #fff;
            font-weight: 600;
        }

        .alert-message {
            text-align: center;
            margin-bottom: 10px;
            color: red;
        }

        .custom-pager {
            display: flex;
            justify-content: center;
            align-items: center;
            gap: 8px;
            padding: 15px;
            background-color: #f9f9f9;
            border-top: 1px solid #e5e5e5;
            margin-top: 10px;
        }

        .custom-pager a,
        .custom-pager span {
            display: inline-block;
            padding: 8px 12px;
            font-size: 14px;
            color: #007bff;
            border-radius: 4px;
            transition: background 0.3s, color 0.3s;
            cursor: pointer;
            text-decoration: none;
        }

        .custom-pager a:hover {
            background-color: #007bff;
            color: white;
        }

        .custom-pager .selected-page {
            background-color: #007bff;
            color: white;
            font-weight: bold;
        }
    </style>

    <div class="log-container">
        <h2>Audit Logs</h2>

        <div class="date-filters">
            <div class="row mb-3 align-items-end">
                <div class="col-md-4">
                    <label for="txtFrom" class="form-label">From Date</label>
                    <asp:TextBox ID="txtFrom" runat="server" CssClass="form-control text-center" type="date" />
                </div>
                <div class="col-md-4">
                    <label for="txtTo" class="form-label">To Date</label>
                    <asp:TextBox ID="txtTo" runat="server" CssClass="form-control text-center" type="date" />
                </div>
                <div class="col-md-4 d-grid">
                    <asp:Label runat="server" AssociatedControlID="btnFilter" CssClass="form-label">&nbsp;</asp:Label>
                    <asp:Button ID="btnFilter" runat="server" Text="Filter"
                        CssClass="btn btn-primary w-full"
                        OnClick="btnFilter_Click" />
                </div>
            </div>
        </div>

        <asp:UpdatePanel ID="UpdatePanelLogs" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:Label ID="lblNoData" runat="server"
                    CssClass="text-danger text-center d-block mb-3"
                    Visible="false" />
                <asp:Label ID="lblMessage" runat="server" CssClass="alert-message" />

                <asp:GridView ID="gvLogs" runat="server" CssClass="grid"
                    AutoGenerateColumns="False"
                    AllowPaging="True"
                    PageSize="50"
                    OnPageIndexChanging="gvLogs_PageIndexChanging">
                    <PagerStyle CssClass="custom-pager" HorizontalAlign="Center" />
                    <Columns>
                        <asp:BoundField DataField="LogID" HeaderText="Log ID" />
                        <asp:BoundField DataField="AdminName" HeaderText="User Name" />
                        <asp:BoundField DataField="Action" HeaderText="Action" />
                        <asp:BoundField DataField="Timestamp" HeaderText="Timestamp"
                            DataFormatString="{0:yyyy-MM-dd HH:mm:ss}" />
                    </Columns>
                </asp:GridView>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnFilter" EventName="Click" />
            </Triggers>
        </asp:UpdatePanel>
    </div>
</asp:Content>