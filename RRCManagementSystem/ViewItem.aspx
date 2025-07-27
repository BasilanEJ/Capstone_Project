<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ViewItem.aspx.cs" Inherits="RRCManagementSystem.ViewItem" %>



<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        body {
            font-family: Arial, sans-serif;
            background-color: #f4f6f9;
        }

        .container {
            max-width: 1100px;
            margin: 30px auto;
            padding: 20px;
        }

        .card {
            background-color: #fff;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
            overflow: hidden;
        }

        .card-header {
            background-color: #007bff;
            color: white;
            text-align: center;
            padding: 15px;
        }

        .card-body {
            padding: 20px;
        }

        .page-title {
            margin: 0;
            font-size: 24px;
        }

        .form-group {
            margin-bottom: 20px;
        }

        label {
            font-weight: bold;
            margin-bottom: 8px;
            display: block;
        }

        .form-control {
            width: 100%;
            padding: 10px;
            font-size: 14px;
            border-radius: 4px;
            border: 1px solid #ccc;
        }

        .alert-message {
            margin-bottom: 15px;
            padding: 10px 15px;
            background-color: #fff3cd;
            border: 1px solid #ffeeba;
            color: #856404;
            border-radius: 5px;
        }

        .table-responsive {
            margin-top: 20px;
        }

        .table {
            width: 100%;
            border-collapse: collapse;
        }

        .table th {
            background-color: #007bff;
            color: white;
            padding: 10px;
            text-align: center;
        }

        .table td {
            padding: 10px;
            text-align: center;
            vertical-align: middle;
        }

        .btn-action {
            padding: 6px 12px;
            font-size: 13px;
            font-weight: bold;
            border-radius: 4px;
            border: none;
            cursor: pointer;
            text-decoration: none;
        }

        .btn-edit {
            background-color: #28a745;
            color: white;
        }

        .btn-edit:hover {
            background-color: #218838;
        }

        .error-message {
            color: red;
            font-weight: bold;
        }
.alert-message {
    margin-bottom: 20px;
    padding: 15px 20px;
    background-color: #fff8e5;
    border: 1px solid #ffc107;
    color: #856404;
    border-radius: 6px;
    font-size: 15px;
    font-weight: normal;
    line-height: 1.6;
    box-shadow: 0 2px 6px rgba(255, 193, 7, 0.2);
    display: block;
}


.alert-message strong {
    font-weight: bold;
    color: #cc0000;
}

.alert-message::before {
    content: "⚠ ";
    font-size: 18px;
    margin-right: 5px;
    font-weight: bold;
}

    </style>

<div class="container">
    <div class="card shadow-sm">
        <div class="card-header bg-primary text-white text-center">
            <h2 class="page-title">View Items</h2>
        </div>

        <div class="card-body">
  
            <asp:Label ID="lblMessage" runat="server" ForeColor="Red" CssClass="error-message" />

            <div class="form-group" style="margin-bottom: 30px;">
                <asp:Label ID="lblRestockNotice" runat="server" CssClass="alert-message" />
            </div>

 
            <div class="form-group" style="margin-bottom: 25px;">
                <label for="ddlType">Filter by Item Type:</label>
                <asp:DropDownList ID="ddlType" runat="server" CssClass="form-control input-lg" AutoPostBack="true" OnSelectedIndexChanged="ddlType_SelectedIndexChanged">
                    <asp:ListItem Text="All" Value="All" Selected="True" />
                    <asp:ListItem Text="Bottled Chemical" Value="Bottled Chemical" />
                    <asp:ListItem Text="Sachet Pack Chemical" Value="Sachet Pack Chemical" />
                    <asp:ListItem Text="Safety Gear" Value="Safety Gear" />
                </asp:DropDownList>
            </div>


                <div class="table-responsive">
<asp:GridView ID="gvItems" runat="server" CssClass="table table-bordered table-striped"
    AutoGenerateColumns="False" AllowPaging="True" PageSize="10"
    OnPageIndexChanging="gvItems_PageIndexChanging" OnRowCommand="gvItems_RowCommand">
    <Columns>

        <asp:TemplateField HeaderText="Item ID">
            <ItemTemplate>
                <%# "Item" + Convert.ToInt32(Eval("ItemID")).ToString("D3") %>
            </ItemTemplate>
        </asp:TemplateField>

        <asp:BoundField DataField="Name" HeaderText="Item Name" />
        <asp:BoundField DataField="Type" HeaderText="Type" />

        <asp:TemplateField HeaderText="Quantity">
            <ItemTemplate>
                <%# 
                    Eval("Type").ToString() == "Bottled Chemical" ? Eval("Quantity") + " bottles" :
                    Eval("Type").ToString() == "Sachet Pack Chemical" ? Eval("Quantity") + " packs" :
                    Eval("Type").ToString() == "Safety Gear" ? Eval("Quantity") + " pcs" :
                    Eval("Quantity") + " unit(s)"
                %>
            </ItemTemplate>
        </asp:TemplateField>

        <asp:TemplateField HeaderText="Excess (mL)">
            <ItemTemplate>
                <%# string.IsNullOrEmpty(Eval("ExcessML").ToString()) ? "-" : Eval("ExcessML") + " mL" %>
            </ItemTemplate>
        </asp:TemplateField>

        <asp:BoundField DataField="ExpirationDate" HeaderText="Expiration Date" DataFormatString="{0:yyyy-MM-dd}" />
        <asp:BoundField DataField="CreatedAt" HeaderText="Created At" DataFormatString="{0:yyyy-MM-dd HH:mm}" />

        <asp:ImageField DataImageUrlField="ImagePath" HeaderText="Image">
            <ControlStyle Width="70px" Height="70px" />
        </asp:ImageField>

        <asp:TemplateField HeaderText="Actions">
            <ItemTemplate>
                <a href='<%# "EditItem.aspx?ItemID=" + EncodeID(Eval("ItemID").ToString()) %>' class="btn-action btn-edit" onclick="return confirm('Are you sure you want to edit this item?');">Edit</a>
            </ItemTemplate>
        </asp:TemplateField>

    </Columns>
</asp:GridView>

                </div>
            </div>
        </div>
    </div>

</asp:Content>

