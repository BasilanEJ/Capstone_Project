<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="TotalStocks.aspx.cs" Inherits="RRCManagementSystem.TotalStocks" %>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .container {
            max-width: 1100px;
            margin: 30px auto;
            padding: 20px;
            background: #fff;
            border-radius: 10px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
        }

        .page-title {
            text-align: center;
            font-size: 28px;
            margin-bottom: 20px;
            font-weight: bold;
        }

        .form-inline {
            text-align: center;
            margin-bottom: 20px;
        }

        label {
            font-weight: bold;
            margin: 0 5px;
        }

        input[type="date"] {
            padding: 6px;
            margin: 0 5px;
            border-radius: 5px;
            border: 1px solid #ccc;
        }

        .btn-filter {
            background-color: #007bff;
            color: white;
            border: none;
            padding: 8px 15px;
            border-radius: 5px;
            font-weight: bold;
            cursor: pointer;
            margin-left: 5px;
        }

        .btn-filter:hover {
            background-color: #0056b3;
        }

        .custom-table {
            width: 100%;
            margin-top: 20px;
            border-collapse: collapse;
            font-size: 14px;
        }

        .custom-table th, .custom-table td {
            border: 1px solid #ddd;
            padding: 10px;
            text-align: center;
        }

        .custom-table th {
            background-color: #f0f2f5;
        }

        .alert-message {
            color: red;
            text-align: center;
            font-weight: bold;
            margin-top: 10px;
        }
    </style>

    <div class="container">
        <h2 class="page-title">📦 Daily Inventory Snapshot</h2>

        <div class="form-inline">
            <label>From:</label>
            <asp:TextBox ID="txtFromDate" runat="server" TextMode="Date" />
            <label>To:</label>
            <asp:TextBox ID="txtToDate" runat="server" TextMode="Date" />
            <asp:Button ID="btnFilter" runat="server" Text="Filter" CssClass="btn-filter" OnClick="btnFilter_Click" />
        </div>

        <asp:Label ID="lblMessage" runat="server" CssClass="alert-message" />

        <asp:GridView ID="gvTotalStocks" runat="server" CssClass="custom-table" AutoGenerateColumns="False" AllowPaging="True" PageSize="15"
            OnPageIndexChanging="gvTotalStocks_PageIndexChanging">
            <Columns>
                <asp:BoundField DataField="ItemID" HeaderText="Item ID" />
                <asp:BoundField DataField="Name" HeaderText="Item Name" />
                <asp:BoundField DataField="Type" HeaderText="Type" />
                <asp:BoundField DataField="Quantity" HeaderText="Quantity" />
                <asp:BoundField DataField="ExcessML" HeaderText="Excess (mL)" />
                <asp:BoundField DataField="SnapshotDate" HeaderText="Snapshot Date" DataFormatString="{0:yyyy-MM-dd}" />
            </Columns>
        </asp:GridView>
    </div>

</asp:Content>


