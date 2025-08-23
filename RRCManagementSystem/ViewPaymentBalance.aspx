<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ViewPaymentBalance.aspx.cs" Inherits="RRCManagementSystem.ViewPaymentBalance" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<style>
    body { font-family: 'Segoe UI', sans-serif; }
    .page-title { text-align:center; color:#1e293b; font-size:26px; font-weight:600; margin:30px 0 20px; }
    .filter-form { display:flex; justify-content:center; gap:15px; margin-bottom:25px; flex-wrap:wrap; }
    .filter-form input[type="text"]{
        padding:8px 10px; border:1px solid #cbd5e1; border-radius:6px; font-size:14px; min-width:260px;
    }
    .filter-form .btn-filter{
        padding:8px 18px; background-color:#2563eb; color:#fff; border:none; border-radius:6px; font-weight:600; cursor:pointer;
    }
    .filter-form .btn-filter:hover { background-color:#1d4ed8; }
    .message-label { text-align:center; color:#ef4444; font-weight:500; margin-bottom:15px; }
    .grid-container { width:95%; margin:0 auto 50px auto; background:#fff; border-radius:10px; box-shadow:0 4px 12px rgba(0,0,0,.05); overflow-x:auto; }
    .table { width:100%; border-collapse:collapse; }
    .table th { background:#1e3a8a; color:#fff; padding:12px; text-align:center; font-weight:600; }
    .table td { padding:10px; text-align:center; font-size:14px; color:#374151; border:1px solid #e5e7eb; }
    .table tr:nth-child(even){ background:#f9fafb; }
    .table tr:hover{ background:#f1f5f9; }
</style>

<h2 class="page-title">💳 View Payment Balances</h2>

<div class="filter-form">
    <asp:TextBox ID="txtClientName" runat="server" placeholder="Search by client name..." />
    <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn-filter" OnClick="btnSearch_Click" />
</div>

<asp:Label ID="lblMessage" runat="server" CssClass="message-label" />

<div class="grid-container">
    <asp:GridView ID="gvBalances" runat="server" AutoGenerateColumns="False"
                  CssClass="table table-striped table-bordered" GridLines="None"
                  EmptyDataText="No balances found.">
        <Columns>
           <asp:BoundField DataField="ClientID" HeaderText="Client ID" Visible="false" />
            <asp:BoundField DataField="ClientName" HeaderText="Client Name" />
            <asp:BoundField DataField="TotalAmount" HeaderText="Total Amount" DataFormatString="₱{0:N2}" HtmlEncode="false" />
            <asp:BoundField DataField="AlreadyPaid" HeaderText="Already Paid" DataFormatString="₱{0:N2}" HtmlEncode="false" />
            <asp:BoundField DataField="RemainingBalance" HeaderText="Remaining Balance" DataFormatString="₱{0:N2}" HtmlEncode="false" />
        </Columns>
    </asp:GridView>
</div>
</asp:Content>