<%@ Page Title="" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="ViewAdmin.aspx.cs" Inherits="RRCManagementSystem.ViewAdmin" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container my-5">
        <div class="card shadow">
            <div class="card-body">
                <h2 class="text-center mb-4 text-primary">User Accounts</h2>

                <div class="d-flex justify-content-end mb-3 gap-2 flex-wrap">
                    <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control w-auto" placeholder="Search by name or email..." />
                    <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary" OnClick="btnSearch_Click" />
                </div>

                <asp:Label ID="lblMessage" runat="server" CssClass="text-danger text-center d-block mb-3 fw-bold" />

                <div class="table-responsive">
                    <asp:GridView ID="gvAdmins" runat="server" CssClass="table table-bordered table-hover text-center"
                        AutoGenerateColumns="False"
                        EmptyDataText="No Admins found."
                        OnRowCommand="gvAdmins_RowCommand"
                        DataKeyNames="UserID">
                        <Columns>
                            <asp:BoundField DataField="UserID" HeaderText="ID" />
                            <asp:BoundField DataField="Name" HeaderText="Name" />
                            <asp:BoundField DataField="Email" HeaderText="Email" />
                            <asp:BoundField DataField="Role" HeaderText="Role" />
                            <asp:TemplateField HeaderText="Actions">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnEdit" runat="server"
                                        Text="Edit"
                                        CommandName="EditAdmin"
                                        CommandArgument='<%# Eval("UserID") %>'
                                        CssClass="btn btn-sm btn-primary me-1" />

                                    <asp:LinkButton ID="btnDelete" runat="server"
                                        Text="Archive"
                                        CommandName="ArchiveAdmin"
                                        CommandArgument='<%# Eval("UserID") %>'
                                        CssClass="btn btn-sm btn-danger"
                                        OnClientClick="return confirm('Are you sure you want to archive this admin?');" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>
</asp:Content>