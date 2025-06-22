<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ArchivedClients.aspx.cs" Inherits="RRCManagementSystem.ArchivedClients" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container py-5">
        <div class="card shadow">
            <div class="card-header bg-primary text-white text-center fw-bold">
                Archived Clients
            </div>
            <div class="card-body">
                <asp:HiddenField ID="hfClientID" runat="server" />

                <asp:GridView ID="gvArchivedClients" runat="server" AutoGenerateColumns="False"
                    CssClass="table table-bordered table-striped table-hover" AllowPaging="True" PageSize="10"
                    OnPageIndexChanging="gvArchivedClients_PageIndexChanging" OnRowCommand="gvArchivedClients_RowCommand">
                    <Columns>
                        <asp:BoundField DataField="ClientID" HeaderText="Client ID" ReadOnly="True" />
                        <asp:BoundField DataField="Name" HeaderText="Name" />
                        <asp:BoundField DataField="Email" HeaderText="Email" />
                        <asp:BoundField DataField="ContactNumber" HeaderText="Contact Number" />
                        <asp:BoundField DataField="City" HeaderText="City" />
                        <asp:BoundField DataField="Country" HeaderText="Country" />
                        <asp:TemplateField HeaderText="Actions">
                            <ItemTemplate>
                                <button type="button" class="btn btn-primary btn-sm me-2" onclick="confirmRestore('<%# Eval("ClientID") %>')">Restore</button>
                                <button type="button" class="btn btn-danger btn-sm" onclick="confirmDelete('<%# Eval("ClientID") %>')">Delete</button>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>

                <asp:Button ID="btnRestoreHidden" runat="server" OnClick="btnRestoreHidden_Click" style="display:none;" />
                <asp:Button ID="btnDeleteHidden" runat="server" OnClick="btnDeleteHidden_Click" style="display:none;" />
            </div>
        </div>
    </div>

    <script>
        function confirmRestore(clientId) {
            Swal.fire({
                title: 'Restore Client?',
                text: 'This will change the client status to Active.',
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#198754',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Yes, Restore'
            }).then((result) => {
                if (result.isConfirmed) {
                    document.getElementById('<%= hfClientID.ClientID %>').value = clientId;
                    document.getElementById('<%= btnRestoreHidden.ClientID %>').click();
                }
            });
        }

        function confirmDelete(clientId) {
            Swal.fire({
                title: 'Delete Permanently?',
                text: 'This action cannot be undone.',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#dc3545',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Yes, Delete'
            }).then((result) => {
                if (result.isConfirmed) {
                    document.getElementById('<%= hfClientID.ClientID %>').value = clientId;
                    document.getElementById('<%= btnDeleteHidden.ClientID %>').click();
                }
            });
        }
    </script>
</asp:Content>