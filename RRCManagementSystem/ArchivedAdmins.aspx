<%@ Page Title="" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="ArchivedAdmins.aspx.cs" Inherits="RRCManagementSystem.ArchivedAdmins" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .table-container {
            width: 90%;
            margin: 0 auto;
            background-color: #fff;
            padding: 20px;
            box-shadow: 0 0 10px rgba(0,0,0,0.1);
            border-radius: 6px;
        }

        .table-container h2 {
            text-align: center;
            margin-bottom: 10px;
            color: #333;
        }

        .search-bar {
            text-align: right;
            margin-bottom: 15px;
        }

        .search-input {
            padding: 6px 10px;
            border: 1px solid #ccc;
            border-radius: 4px;
        }

        .search-button {
            padding: 6px 14px;
            background-color: #007bff;
            color: white;
            border: none;
            border-radius: 4px;
            cursor: pointer;
        }

        .grid {
            width: 100%;
            border-collapse: collapse;
        }

        .grid th, .grid td {
            padding: 12px;
            border: 1px solid #ddd;
            text-align: center;
        }

        .grid th {
            background-color: #007bff;
            color: white;
        }

        .btn-action {
            margin: 0 5px;
            padding: 5px 10px;
            background-color: #007bff;
            color: white;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            text-decoration: none;
        }

        .btn-delete {
            background-color: #dc3545;
        }

        .btn-restore {
            background-color: #28a745;
        }

        .btn-action:hover {
            opacity: 0.9;
        }

        .alert-message {
            text-align: center;
            margin-bottom: 10px;
            color: red;
        }
    </style>

    <div class="table-container">
        <h2>Archived Admin Accounts</h2>

        <div class="search-bar">
            <asp:TextBox ID="txtSearch" runat="server" CssClass="search-input" placeholder="Search by name or email..." />
            <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="search-button" OnClick="btnSearch_Click" />
        </div>

        <asp:Label ID="lblMessage" runat="server" CssClass="alert-message" />

        <asp:GridView ID="gvArchivedAdmins" runat="server" CssClass="grid"
            AutoGenerateColumns="False"
            EmptyDataText="No archived admins found."
            OnRowCommand="gvArchivedAdmins_RowCommand"
            DataKeyNames="UserID">
            <Columns>
                <asp:BoundField DataField="UserID" HeaderText="ID" />
                <asp:BoundField DataField="Name" HeaderText="Name" />
                <asp:BoundField DataField="Email" HeaderText="Email" />
                <asp:BoundField DataField="Role" HeaderText="Role" />
                <asp:TemplateField HeaderText="Actions">
                    <ItemTemplate>
                        <asp:LinkButton ID="btnRestore" runat="server"
                            Text="Restore"
                            CommandName="RestoreAdmin"
                            CommandArgument='<%# Eval("UserID") %>'
                            CssClass="btn-action btn-restore"
                            OnClientClick="return confirm('Restore this account?');" />

                        <asp:LinkButton ID="btnDelete" runat="server"
                            Text="Delete"
                            CommandName="DeleteAdmin"
                            CommandArgument='<%# Eval("UserID") %>'
                            CssClass="btn-action btn-delete"
                            OnClientClick="return confirm('This will permanently delete the account. Continue?');" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>
</asp:Content>

