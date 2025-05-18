<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="TransactionHistory.aspx.cs" Inherits="RRCManagementSystem.TransactionHistory" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<style>
    body {
        font-family: 'Segoe UI', sans-serif;
    }

    .page-title {
        text-align: center;
        color: #1e293b;
        font-size: 26px;
        font-weight: 600;
        margin: 30px 0 20px;
    }

    .filter-form {
        display: flex;
        justify-content: center;
        gap: 15px;
        margin-bottom: 25px;
        flex-wrap: wrap;
    }

    .filter-form input[type="date"] {
        padding: 8px 10px;
        border: 1px solid #cbd5e1;
        border-radius: 6px;
        font-size: 14px;
        min-width: 160px;
    }

    .filter-form input[type="submit"],
    .filter-form input[type="button"],
    .filter-form button,
    .filter-form .btn-filter,
    .filter-form input[type="submit"].btn-filter {
        padding: 8px 18px;
        background-color: #2563eb;
        color: #fff;
        border: none;
        border-radius: 6px;
        font-weight: 600;
        cursor: pointer;
        transition: background-color 0.3s ease;
    }

    .filter-form input[type="submit"]:hover,
    .filter-form .btn-filter:hover {
        background-color: #1d4ed8;
    }

    .message-label {
        text-align: center;
        color: #ef4444;
        font-weight: 500;
        margin-bottom: 15px;
    }

    .grid-container {
        width: 95%;
        margin: 0 auto 50px auto;
        background-color: #ffffff;
        border-radius: 10px;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
        overflow-x: auto;
    }

    .table {
        width: 100%;
        border-collapse: collapse;
    }

    .table th {
        background-color: #1e3a8a;
        color: white;
        padding: 12px;
        text-align: center;
        font-weight: 600;
    }

    .table td {
        padding: 10px;
        text-align: center;
        font-size: 14px;
        color: #374151;
        border: 1px solid #e5e7eb;
    }

    .table tr:nth-child(even) {
        background-color: #f9fafb;
    }

    .table tr:hover {
        background-color: #f1f5f9;
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