<%@ Page Title="" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="ViewRole.aspx.cs" Inherits="RRCManagementSystem.ViewRole" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <asp:HiddenField ID="hfRoleIDToDelete" runat="server" />

    <div class="container my-5">
        <div class="card shadow mx-auto" style="max-width: 900px;">
            <div class="card-body">
                <h2 class="text-center text-primary fw-bold mb-4">Manage Roles</h2>

                <div class="table-responsive">
                    <asp:GridView ID="gvRoles" runat="server" AutoGenerateColumns="False" CssClass="table table-bordered table-hover text-center"
                        OnRowCommand="gvRoles_RowCommand"
                        DataKeyNames="RoleID"
                        EmptyDataText="No roles found.">
                        <Columns>
                            <asp:BoundField DataField="RoleName" HeaderText="Role Name" />
                            <asp:TemplateField HeaderText="Actions">
                                <ItemTemplate>
                                    <button type="button" class="btn btn-sm btn-danger"
                                            onclick="confirmDelete('<%# Eval("RoleID") %>')">
                                        Delete
                                    </button>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>

                <asp:Label ID="lblMessage" runat="server" CssClass="text-danger fw-semibold text-center d-block mt-3"></asp:Label>
            </div>
        </div>
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