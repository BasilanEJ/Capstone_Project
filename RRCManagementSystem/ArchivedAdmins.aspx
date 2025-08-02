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

     <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <script type="text/javascript">
        function confirmRestore(btn, userId) {
            Swal.fire({
                title: 'Are you sure?',
                text: "Do you want to restore this admin account?",
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#28a745',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes, restore it!'
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack(btn, 'RestoreAdmin$' + userId); // ✅ CommandName$UserID
                }
            });
            return false;
        }

        function confirmDelete(btn, userId) {
            Swal.fire({
                title: 'Are you sure?',
                text: "This will permanently delete the admin account.",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#dc3545',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Yes, delete it!'
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack(btn, 'DeletePermanently$' + userId); // ✅ CommandName$UserID
                }
            });
            return false;
        }

    </script>


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
                        <a href="javascript:void(0);"
                           class="btn-action btn-restore"
                           onclick='return confirmRestore("<%= gvArchivedAdmins.UniqueID %>", "<%# Eval("UserID") %>");'>
                            Restore
                        </a>

                        <a href="javascript:void(0);"
                           class="btn-action btn-delete"
                           onclick='return confirmDelete("<%= gvArchivedAdmins.UniqueID %>", "<%# Eval("UserID") %>");'>
                            Delete
                        </a>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>
</asp:Content>

