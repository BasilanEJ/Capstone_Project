<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ViewSales.aspx.cs" Inherits="RRCManagementSystem.ViewSales" %>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .page-title {
            font-size: 26px;
            text-align: center;
            margin: 30px 0 20px;
            font-weight: bold;
            color: #2c3e50;
        }

        .filter-section {
            display: flex;
            justify-content: center;
            gap: 15px;
            margin-bottom: 20px;
        }

        .filter-section input[type="date"] {
            padding: 6px 10px;
            border: 1px solid #ccc;
            border-radius: 5px;
        }

        .filter-section asp:Button {
            padding: 6px 12px;
            background-color: #3498db;
            color: white;
            border: none;
            border-radius: 5px;
            cursor: pointer;
        }

        .grid-container {
            margin: 0 auto;
            width: 90%;
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

    <h2 class="page-title">📈 View Sales Summary</h2>

    <div class="filter-section">
        <asp:Label runat="server" AssociatedControlID="txtFrom" Text="From:"></asp:Label>
        <asp:TextBox ID="txtFrom" runat="server" type="date" />
        <asp:Label runat="server" AssociatedControlID="txtTo" Text="To:"></asp:Label>
        <asp:TextBox ID="txtTo" runat="server" type="date" />
        <asp:Button ID="btnFilter" runat="server" Text="Filter" OnClick="btnFilter_Click" CssClass="btn btn-primary" />
    </div>

    <asp:Label ID="lblMessage" runat="server" CssClass="message-label"></asp:Label>

    <div class="grid-container">
  
        <asp:GridView ID="gvSales" runat="server" AutoGenerateColumns="False"
    CssClass="table table-striped table-bordered" GridLines="None">
    <Columns>
        <!-- Use pre-formatted column from code-behind -->
        <asp:BoundField DataField="FormattedSaleID" HeaderText="Sale ID" />

        <asp:BoundField DataField="ClientName" HeaderText="Client" />
        <asp:BoundField DataField="Amount" HeaderText="Amount" DataFormatString="{0:C}" />
        <asp:BoundField DataField="PaymentMethod" HeaderText="Method" />
        <asp:BoundField DataField="Status" HeaderText="Status" />
        <asp:BoundField DataField="TransactionDate" HeaderText="Date" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
    </Columns>
</asp:GridView>


    </div>
</asp:Content>

