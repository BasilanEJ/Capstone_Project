<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="TransactionHistory.aspx.cs" Inherits="RRCManagementSystem.TransactionHistory" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .page-title {
            font-size: 26px;
            text-align: center;
            margin: 30px 0 10px;
            font-weight: bold;
            color: #2c3e50;
        }

        .filter-form {
            display: flex;
            justify-content: center;
            gap: 10px;
            margin-bottom: 20px;
        }

        .filter-form input[type="date"] {
            padding: 6px 10px;
            border: 1px solid #ccc;
            border-radius: 4px;
        }

        .filter-form input[type="submit"] {
            padding: 6px 15px;
            background-color: #3498db;
            border: none;
            color: white;
            border-radius: 4px;
            cursor: pointer;
        }

        .filter-form input[type="submit"]:hover {
            background-color: #2980b9;
        }

        .grid-container {
            margin: 0 auto;
            width: 95%;
            background-color: #fff;
            border: 1px solid #ccc;
            border-radius: 8px;
            padding: 20px;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
        }

        .message-label {
            text-align: center;
            margin-bottom: 15px;
            color: red;
        }
    </style>

    <h2 class="page-title">📄 Transaction History</h2>

    <div class="filter-form">
        <asp:TextBox ID="txtFromDate" runat="server" TextMode="Date"></asp:TextBox>
        <asp:TextBox ID="txtToDate" runat="server" TextMode="Date"></asp:TextBox>
        <asp:Button ID="btnFilter" runat="server" Text="Filter" OnClick="btnFilter_Click" />
    </div>

    <asp:Label ID="lblMessage" runat="server" CssClass="message-label"></asp:Label>

    <div class="grid-container">
        <asp:GridView ID="gvTransactions" runat="server" AutoGenerateColumns="False" CssClass="table table-striped table-bordered" GridLines="None">
            <Columns>
                <asp:BoundField DataField="TransactionID" HeaderText="Transaction ID" />
                <asp:BoundField DataField="SaleID" HeaderText="Sale ID" />
                <asp:BoundField DataField="Amount" HeaderText="Amount" DataFormatString="{0:C}" />
                <asp:BoundField DataField="PaymentMethod" HeaderText="Payment Method" />
                <asp:BoundField DataField="Status" HeaderText="Status" />
                <asp:BoundField DataField="TransactionDate" HeaderText="Transaction Date" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
            </Columns>
        </asp:GridView>
    </div>
</asp:Content>