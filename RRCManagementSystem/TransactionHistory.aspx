<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="TransactionHistory.aspx.cs" Inherits="RRCManagementSystem.TransactionHistory" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<style>
    body { font-family: 'Segoe UI', sans-serif; }
    .page-title { text-align:center; color:#1e293b; font-size:26px; font-weight:600; margin:30px 0 20px; }
    .filter-form { display:flex; justify-content:center; gap:15px; margin-bottom:25px; flex-wrap:wrap; }
    .filter-form input[type="date"], .filter-form select {
        padding:8px 10px; border:1px solid #cbd5e1; border-radius:6px; font-size:14px; min-width:160px;
    }
    .filter-form .btn-filter {
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

    /* Shared pill base */
.pill {
  display:inline-block;
  border-radius:999px;
  padding:6px 12px;
  font-size:13px;
  font-weight:600;
  text-decoration:none;
  transition:all 0.2s ease;
}

/* View Receipt = Blue */
.pill-view {
  background:#e0f2fe;          /* light blue */
  color:#075985;              /* dark blue text */
  border:1px solid #7dd3fc;   /* blue border */
}
.pill-view:hover {
  background:#bae6fd;
  color:#0c4a6e;
}

/* Add Receipt = Green */
.pill-add {
  background:#dcfce7;          /* light green */
  color:#166534;              /* dark green text */
  border:1px solid #86efac;   /* green border */
}
.pill-add:hover {
  background:#bbf7d0;
  color:#14532d;
}


</style>

<h2 class="page-title">📄 Transaction History</h2>

<div class="filter-form">
    <asp:TextBox ID="txtFromDate" runat="server" TextMode="Date" />
    <asp:TextBox ID="txtToDate" runat="server" TextMode="Date" />
    <asp:DropDownList ID="ddlPaymentMethod" runat="server"></asp:DropDownList>
    <asp:Button ID="btnFilter" runat="server" Text="Filter" CssClass="btn-filter" OnClick="btnFilter_Click" />
</div>

<asp:Label ID="lblMessage" runat="server" CssClass="message-label" />

<div class="grid-container">
    <asp:GridView ID="gvTransactions" runat="server" AutoGenerateColumns="False"
              CssClass="table table-striped table-bordered" GridLines="None"
              EmptyDataText="No transaction records found.">
    <Columns>
        <asp:BoundField DataField="TransactionID" HeaderText="Transaction ID" />
        <asp:BoundField DataField="SaleID" HeaderText="Sale ID" />
        <asp:BoundField DataField="Amount" HeaderText="Amount" DataFormatString="₱{0:N2}" HtmlEncode="false" />
        <asp:BoundField DataField="PaymentMethod" HeaderText="Payment Method" />
        <asp:BoundField DataField="TransactionDate" HeaderText="Transaction Date" DataFormatString="{0:yyyy-MM-dd HH:mm}" />

<asp:TemplateField HeaderText="Receipt">
  <ItemTemplate>
    <asp:Literal ID="litView" runat="server" Mode="PassThrough"
      Visible='<%# !string.IsNullOrWhiteSpace(Eval("Receipt") as string) %>'
      Text='<%# GetReceiptLink(Eval("Receipt")) %>' />
    <asp:HyperLink ID="lnkAdd" runat="server"
      CssClass="pill pill-add"
      NavigateUrl='<%# "AddReceipt.aspx?tx=" + Eval("TransactionID") %>'
      Text="Add Receipt"
      Visible='<%# string.IsNullOrWhiteSpace(Eval("Receipt") as string) %>' />
  </ItemTemplate>
</asp:TemplateField>

    </Columns>
</asp:GridView>

</div>

</asp:Content>
