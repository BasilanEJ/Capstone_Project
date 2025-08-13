<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ClientsProfiles.aspx.cs" Inherits="RRCManagementSystem.ClientsProfiles" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container my-4">
        <div class="card shadow-sm">
            <div class="card-header bg-primary text-white text-center fw-bold">
                Approved Client Profiles
            </div>
            <div class="card-body">
                <asp:GridView ID="gvClients" runat="server" AutoGenerateColumns="False"
                    CssClass="table table-bordered table-striped text-center"
                    AllowPaging="True" PageSize="10"
                    OnPageIndexChanging="gvClients_PageIndexChanging"
                    OnRowCommand="gvClients_RowCommand">
                    <Columns>
                        <asp:BoundField DataField="ClientID" HeaderText="Client ID" ReadOnly="True" />
                        <asp:TemplateField HeaderText="Name">
                            <ItemTemplate>
                                <%# Eval("LastName") %>, <%# Eval("FirstName") %> <%# Eval("MiddleName") %>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:BoundField DataField="Email" HeaderText="Email" />
                        <asp:BoundField DataField="ContactNumber" HeaderText="Contact Number" />
                        <asp:BoundField DataField="City" HeaderText="City" />
                        <asp:BoundField DataField="Country" HeaderText="Country" />
                        <asp:TemplateField HeaderText="Actions">
  <ItemTemplate>
    <asp:Button ID="btnView" runat="server"
        CssClass="btn btn-primary btn-sm me-2"
        Text="View Profile"
        CommandName="ViewProfile"
        CommandArgument='<%# Eval("ClientID") %>' />

    <asp:Button ID="btnArchive" runat="server"
        CssClass="btn btn-danger btn-sm"
        Text="Archive"
        CommandName="ArchiveClient"
        CommandArgument='<%# Eval("ClientID") %>'
        UseSubmitBehavior="false"
        OnClientClick="return confirmArchive(this);" />
  </ItemTemplate>
</asp:TemplateField>

                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </div>

    <script>
        function confirmArchive(btn) {
            // Stop the normal submit right away
            if (window.event) window.event.preventDefault();

            Swal.fire({
                title: 'Archive Client?',
                text: 'The client will be archived and no longer appear in the active list.',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes, archive'
            }).then((result) => {
                if (result.isConfirmed) {
                    // Trigger the WebForms postback for this specific button
                    __doPostBack(btn.name, '');
                }
            });

            // Always cancel the original click; we'll post back ourselves if confirmed
            return false;
        }
    </script>
</asp:Content>
