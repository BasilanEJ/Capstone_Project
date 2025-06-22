<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ViewServices.aspx.cs" Inherits="RRCManagementSystem.ViewServices" %>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

        <style>
        /* Main Content Wrapper */
        .main-content {
            padding: 20px;
            background-color: #f4f4f4;
            min-height: calc(100vh - 100px);
        }

        /* Card Container */
        .card {
            background-color: #fff;
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
            overflow: hidden;
            margin-bottom: 30px;
        }

        /* Card Header */
        .card-header {
            background-color: #004085;
            color: #fff;
            padding: 15px 20px;
            font-size: 18px;
            font-weight: bold;
        }

        /* Card Body */
        .card-body {
            padding: 20px;
        }

        /* Alert/Message Styling */
        .alert-message {
            display: block;
            margin-bottom: 15px;
            padding: 10px 15px;
            background-color: #d1ecf1;
            color: #0c5460;
            border: 1px solid #bee5eb;
            border-radius: 5px;
            font-size: 14px;
        }

        /* GridView Table */
        .table {
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 20px;
        }

        .table th,
        .table td {
            border: 1px solid #dee2e6;
            padding: 12px 15px;
            text-align: left;
        }

        .table th {
            background-color: #004085;
            color: #fff;
            font-size: 14px;
        }

        .table td {
            background-color: #f8f9fa;
            font-size: 14px;
        }

        /* Action Buttons */
        .action-btn {
            color: #004085;
            text-decoration: none;
            padding: 6px 10px;
            border-radius: 4px;
            font-size: 13px;
            transition: background-color 0.3s, color 0.3s;
        }

        .action-btn:hover {
            background-color: #004085;
            color: #fff;
        }

        /* Responsive Table */
        @media (max-width: 768px) {
            .table th,
            .table td {
                padding: 8px 10px;
                font-size: 12px;
            }

            .card-header {
                font-size: 16px;
            }

            .action-btn {
                font-size: 12px;
                padding: 5px 8px;
            }
        }
    </style>
 
    <div class="main-content">
        <div class="card">
            <div class="card-header">
                Services List
            </div>

            <div class="card-body">
                <asp:Label ID="lblMessage" runat="server" CssClass="alert-message"></asp:Label>
<asp:GridView ID="gvServices" runat="server" AutoGenerateColumns="False" CssClass="table"
    OnRowCommand="gvServices_RowCommand" EmptyDataText="No services found.">
    <Columns>

        <asp:TemplateField HeaderText="Service ID">
            <ItemTemplate>
                <%# "Service" + Convert.ToInt32(Eval("ServiceID")).ToString("D3") %>
            </ItemTemplate>
        </asp:TemplateField>

        <asp:BoundField DataField="Name" HeaderText="Service Name" />
        <asp:BoundField DataField="Description" HeaderText="Description" />

        <asp:BoundField DataField="Price100SQM" HeaderText="100 SQM Price (₱)" DataFormatString="{0:C}" />
        <asp:BoundField DataField="Price200SQM" HeaderText="200 SQM Price (₱)" DataFormatString="{0:C}" />
        <asp:BoundField DataField="PriceAbove200SQM" HeaderText="200+ SQM Price (₱)" DataFormatString="{0:C}" />

        <asp:TemplateField HeaderText="Actions">
            <ItemTemplate>
                <asp:LinkButton ID="btnEdit" runat="server"
                    CommandName="EditService"
                    CommandArgument='<%# Eval("ServiceID") %>'
                    CssClass="action-btn" Text="Edit" />
                &nbsp;|&nbsp;
                <asp:LinkButton ID="btnDelete" runat="server"
                    CommandName="DeleteService"
                    CommandArgument='<%# Eval("ServiceID") %>'
                    CssClass="action-btn"
                    Text="Delete"
                    OnClientClick="return confirm('Are you sure you want to delete this service?');" />
            </ItemTemplate>
        </asp:TemplateField>
    </Columns>
</asp:GridView>


            </div>
        </div>
    </div>

</asp:Content>