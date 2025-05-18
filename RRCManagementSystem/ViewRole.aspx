<%@ Page Title="" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="ViewRole.aspx.cs" Inherits="RRCManagementSystem.ViewRole" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .card-container {
            max-width: 900px;
            margin: 50px auto;
            background-color: #ffffff;
            border-radius: 12px;
            box-shadow: 0 8px 16px rgba(0, 0, 0, 0.05);
            padding: 30px 40px;
        }

        .card-title {
            text-align: center;
            font-size: 26px;
            font-weight: 600;
            margin-bottom: 25px;
            color: #1f2937;
        }

        .styled-gridview {
            width: 100%;
            border-collapse: collapse;
        }

        .styled-gridview th {
            background-color: #1f2937;
            color: white;
            padding: 12px;
            font-weight: 600;
            text-align: center;
        }

        .styled-gridview td {
            padding: 12px;
            text-align: center;
            border-bottom: 1px solid #e5e7eb;
        }

        .styled-gridview tr:hover {
            background-color: #f9fafb;
        }

        .btn-delete {
            background-color: #dc3545;
            border: none;
            color: white;
            padding: 6px 14px;
            border-radius: 5px;
            font-size: 14px;
            cursor: pointer;
            transition: background-color 0.3s ease;
        }

        .btn-delete:hover {
            background-color: #c82333;
        }

        .message-label {
            text-align: center;
            margin-top: 20px;
            font-weight: 500;
            color: #dc2626;
        }
    </style>

    <asp:HiddenField ID="hfRoleIDToDelete" runat="server" />

    <div class="card-container">
        <div class="card-title">Manage Roles</div>

        <asp:GridView ID="gvRoles" runat="server" AutoGenerateColumns="False" CssClass="styled-gridview"
            OnRowCommand="gvRoles_RowCommand"
            DataKeyNames="RoleID"
            EmptyDataText="No roles found.">
            <Columns>
                <asp:BoundField DataField="RoleName" HeaderText="Role Name" />
                <asp:TemplateField HeaderText="Actions">
                    <ItemTemplate>
                        <button type="button" class="btn-delete"
                                onclick="confirmDelete('<%# Eval("RoleID") %>')">
                            Delete
                        </button>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>

        <asp:Label ID="lblMessage" runat="server" CssClass="message-label"></asp:Label>
    </div>

    <script type="text/javascript">
        function confirmDelete(roleId) {
            Swal.fire({
                title: 'Are you sure?',
                text: "This will delete the role permanently.",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#dc3545',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Yes, delete it!'
            }).then((result) => {
                if (result.isConfirmed) {
                    document.getElementById('<%= hfRoleIDToDelete.ClientID %>').value = roleId;
                    __doPostBack('DeleteRole', '');
                }
            });
        }
    </script>
</asp:Content>
