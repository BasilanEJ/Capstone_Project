<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ViewSupplier.aspx.cs" Inherits="RRCManagementSystem.ViewSupplier" %>


<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        /* --- Same internal CSS from the previous response --- */
        .main-content {
            padding: 20px;
            background-color: #f4f4f4;
            min-height: calc(100vh - 100px);
        }

        .supplier-container {
            background-color: #ffffff;
            padding: 25px;
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
            max-width: 1200px;
            margin: 0 auto;
        }

        .supplier-container h2 {
            color: #004085;
            margin-bottom: 20px;
        }

        .table {
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 20px;
            font-size: 14px;
        }

        .table th,
        .table td {
            border: 1px solid #dee2e6;
            padding: 12px 15px;
            text-align: left;
        }

        .table th {
            background-color: #004085;
            color: #ffffff;
            font-weight: bold;
        }

        .table td {
            background-color: #f8f9fa;
        }

        .table a {
            color: #004085;
            text-decoration: none;
            padding: 6px 10px;
            border-radius: 4px;
            transition: background-color 0.3s ease, color 0.3s ease;
        }

        .table a:hover {
            background-color: #004085;
            color: #ffffff;
        }

        #pnlSendEmail {
            background-color: #ffffff;
            padding: 20px;
            margin-top: 30px;
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
            max-width: 700px;
            margin-left: auto;
            margin-right: auto;
        }

        #pnlSendEmail h3 {
            color: #004085;
            margin-bottom: 15px;
        }

        .form-control {
            width: 100%;
            padding: 10px 12px;
            font-size: 14px;
            border: 1px solid #ced4da;
            border-radius: 4px;
            transition: border-color 0.3s, box-shadow 0.3s;
        }

        .form-control:focus {
            border-color: #004085;
            box-shadow: 0 0 5px rgba(0, 64, 133, 0.3);
            outline: none;
        }

        .btn {
            display: inline-block;
            padding: 10px 20px;
            border-radius: 4px;
            font-size: 14px;
            cursor: pointer;
            border: none;
            transition: background-color 0.3s ease;
        }

        .btn-primary {
            background-color: #004085;
            color: #ffffff;
        }

        .btn-primary:hover {
            background-color: #003366;
        }

        .btn-secondary {
            background-color: #6c757d;
            color: #ffffff;
        }

        .btn-secondary:hover {
            background-color: #5a6268;
        }

        #lblMessage {
            margin-bottom: 15px;
            display: block;
            font-size: 14px;
        }

        @media (max-width: 768px) {
            .supplier-container {
                padding: 15px;
            }

            .table th,
            .table td {
                font-size: 12px;
                padding: 10px;
            }

            .btn {
                width: 100%;
                margin-bottom: 10px;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="main-content">
        <div class="supplier-container">

            <h2>Suppliers List</h2>

            <asp:Label ID="lblMessage" runat="server" ForeColor="Red"></asp:Label>

            <asp:GridView ID="gvSuppliers" runat="server" AutoGenerateColumns="False"
                OnRowCommand="gvSuppliers_RowCommand"
                CssClass="table"
                EmptyDataText="No suppliers found.">

                <Columns>
                    <asp:BoundField DataField="SupplierID" HeaderText="ID" />
                    <asp:BoundField DataField="Name" HeaderText="Name" />
                    <asp:BoundField DataField="CompanyName" HeaderText="Company Name" />
                    <asp:BoundField DataField="BusinessType" HeaderText="Business Type" />
                    <asp:BoundField DataField="ContactNumber" HeaderText="Contact Number" />
                    <asp:BoundField DataField="Email" HeaderText="Email" />
                    <asp:BoundField DataField="Status" HeaderText="Status" />
                    <asp:BoundField DataField="CreatedAt" HeaderText="Date Added" DataFormatString="{0:yyyy-MM-dd}" />

                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <!-- Send Email Action -->
                            <asp:LinkButton ID="btnOpenEmailForm" runat="server"
                                CommandName="OpenEmailForm"
                                CommandArgument='<%# Eval("Email") %>'
                                Text="Send Email"
                                OnClientClick="return confirm('Are you sure you want to email this supplier?');" />

                            &nbsp;|&nbsp;

                            <!-- Edit Action -->
                            <asp:LinkButton ID="btnEdit" runat="server"
                                CommandName="EditSupplier"
                                CommandArgument='<%# Eval("SupplierID") %>'
                                Text="Edit"
                                OnClientClick="return confirm('Are you sure you want to edit this supplier?');" />

                            &nbsp;|&nbsp;

                            <!-- Archive Action -->
                            <asp:LinkButton ID="btnArchive" runat="server"
                                CommandName="ArchiveSupplier"
                                CommandArgument='<%# Eval("SupplierID") %>'
                                Text="Archive"
                                OnClientClick="return confirm('Are you sure you want to archive this supplier?');" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>

            <br />

            <!-- Email Panel -->
            <asp:Panel ID="pnlSendEmail" runat="server" Visible="false" BorderStyle="None">
                <h3>Send Email to Supplier</h3>

                <asp:Label ID="lblSendTo" runat="server" Text=""></asp:Label><br /><br />

                <asp:TextBox ID="txtSubject" runat="server" CssClass="form-control" Width="100%" placeholder="Subject"></asp:TextBox><br />

                <asp:TextBox ID="txtMessageBody" runat="server" TextMode="MultiLine" CssClass="form-control" Width="100%" Height="200px" placeholder="Type your message here..."></asp:TextBox><br />

                <asp:Button ID="btnSendEmail" runat="server" Text="Send Email" OnClick="btnSendEmail_Click" CssClass="btn btn-primary" OnClientClick="return confirm('Are you sure you want to send this email?');" />
                &nbsp;
                <asp:Button ID="btnCancelEmail" runat="server" Text="Cancel" OnClick="btnCancelEmail_Click" CssClass="btn btn-secondary" OnClientClick="return confirm('Are you sure you want to cancel?');" />
            </asp:Panel>

        </div>
    </div>

</asp:Content>