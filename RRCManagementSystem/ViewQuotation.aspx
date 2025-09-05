<%@ Page Title="View Quotations" Language="C#" MasterPageFile="~/Admin.Master"
    AutoEventWireup="true" CodeBehind="ViewQuotation.aspx.cs"
    Inherits="RRCManagementSystem.ViewQuotation" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <style>
    .inquiry-header { font-size: 24px; font-weight: bold; margin-bottom: 20px; color: #004085; }
    .inquiry-table { width: 100%; border-collapse: collapse; margin-bottom: 30px; }
    .inquiry-table th, .inquiry-table td { border: 1px solid #dee2e6; padding: 10px; text-align: left; }
    .inquiry-table th { background-color: #e9ecef; color: #333; }
    .filters-bar{ display:flex; justify-content:center; align-items:flex-end; gap:20px; margin:20px 0; flex-wrap:wrap; }
    .filters-bar .form-group{ display:flex; flex-direction:column; min-width:200px }
    .filters-bar label{ font-weight:600; margin-bottom:6px; color:#343a40; font-size:15px }
    .filters-bar input, .filters-bar select{ padding:10px; font-size:15px; border-radius:6px; border:1px solid #ced4da; }
    .btn-primary-sm{ background-color:#007bff;color:#fff;border:none;padding:9px 18px;font-size:15px;cursor:pointer;border-radius:6px; }
    .btn-primary-sm:hover{ background-color:#0056b3; }
    .btn-outline-sm{ background:#fff;color:#007bff;border:1px solid #007bff;padding:9px 18px;font-size:15px;cursor:pointer;border-radius:6px; }
    .muted{color:#6c757d}
    .badge-pill{display:inline-block;padding:3px 10px;border-radius:999px;background:#f8f9fa;border:1px solid #dee2e6;font-size:12px}
  </style>

  <h2 class="inquiry-header">📑 View Quotations</h2>
  <div class="filters-bar">
    <div class="form-group">
      <label for="<%= txtDateFrom.ClientID %>">Date From</label>
      <asp:TextBox ID="txtDateFrom" runat="server" TextMode="Date" />
    </div>
    <div class="form-group">
      <label for="<%= txtDateTo.ClientID %>">Date To</label>
      <asp:TextBox ID="txtDateTo" runat="server" TextMode="Date" />             
    </div>
    <div class="form-group">
      <label for="<%= ddlInspector.ClientID %>">Inspector</label>
      <asp:DropDownList ID="ddlInspector" runat="server" />
    </div>
    <div class="form-group" style="flex-direction:row; gap:10px;">
      <asp:Button ID="btnSearch" runat="server" CssClass="btn-primary-sm" Text="Search" OnClick="btnSearch_Click" />
      <asp:Button ID="btnReset"  runat="server" CssClass="btn-outline-sm" Text="Reset" OnClick="btnReset_Click" />
    </div>
    <asp:Label ID="lblMessage" runat="server" CssClass="muted" />
  </div>

<asp:GridView ID="gvQuotations" runat="server"
    AutoGenerateColumns="False"
    CssClass="inquiry-table"
    DataKeyNames="PendingQuotationID"
    AllowPaging="true" PageSize="10"
    OnPageIndexChanging="gvQuotations_PageIndexChanging">

    <Columns>
     <asp:BoundField DataField="PendingQuotationID" HeaderText="Quote #" />
      <asp:BoundField DataField="CreatedAt" HeaderText="Created" DataFormatString="{0:yyyy-MM-dd HH:mm}" HtmlEncode="false" />
      <asp:BoundField DataField="ClientName" HeaderText="Client" />
      <asp:BoundField DataField="InspectorName" HeaderText="Inspector" />
      <asp:BoundField DataField="ServiceNames" HeaderText="Services" />
      <asp:BoundField DataField="SQM" HeaderText="SQM" />
      <asp:BoundField DataField="Price" HeaderText="Price (₱)" DataFormatString="{0:N2}" HtmlEncode="false" />
      <asp:TemplateField HeaderText="Contract?">
        <ItemTemplate>
          <span class="badge-pill"><%# Convert.ToBoolean(Eval("IsContract")) ? "Yes" : "No" %></span>
        </ItemTemplate>
      </asp:TemplateField>
      <asp:TemplateField HeaderText="Status">
        <ItemTemplate>
          <span class="badge-pill" style='<%# Eval("Status").ToString() == "Converted" ? "background-color:#28a745;color:white;" : "background-color:#ffc107;color:#212529;" %>'>
            <%# Eval("Status") %>
          </span>
        </ItemTemplate>
      </asp:TemplateField>
    </Columns>
    <EmptyDataTemplate>
      <div class="muted">No quotations found for the selected filters.</div>
    </EmptyDataTemplate>
  </asp:GridView>
</asp:Content>                                                                                                                          