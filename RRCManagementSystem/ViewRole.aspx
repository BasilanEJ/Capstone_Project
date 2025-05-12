<%@ Page Title="" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="ViewRole.aspx.cs" Inherits="RRCManagementSystem.ViewRole" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="container my-5">
        <div class="card shadow p-4">
            <h2 class="text-center mb-4" style="color: #1f2937;">Manage Roles</h2>

            <asp:GridView ID="gvRoles" runat="server" AutoGenerateColumns="False" CssClass="table table-hover text-center"
                OnRowCommand="gvRoles_RowCommand" 
                DataKeyNames="RoleID" 
                EmptyDataText="No roles found.">
                <Columns>
                    <asp:BoundField DataField="RoleName" HeaderText="Role Name" />

                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <asp:Button ID="btnDelete" runat="server" Text="Delete" CommandName="DeleteRole" CommandArgument='<%# Eval("RoleID") %>'
                                CssClass="btn btn-danger btn-sm"
                                OnClientClick="return confirm('Are you sure you want to delete this role?');" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>

            <div class="text-center mt-3">
                <asp:Label ID="lblMessage" runat="server" CssClass="text-danger font-weight-bold"></asp:Label>
            </div>
        </div>
    </div>

</asp:Content>