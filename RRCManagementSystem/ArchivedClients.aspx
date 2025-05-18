<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ArchivedClients.aspx.cs" Inherits="RRCManagementSystem.ArchivedClients" %>



<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        body {
            font-family: Arial, sans-serif;
        }

        .container {
            max-width: 1100px;
            margin: 30px auto;
            padding: 15px;
        }

        .card {
            border-radius: 10px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
            overflow: hidden;
            background-color: #fff;
            transition: box-shadow 0.3s ease;
        }

        .card:hover {
            box-shadow: 0 6px 18px rgba(0, 0, 0, 0.15);
        }

        .card-header {
            background-color: #0073CF;
            color: #ffffff;
            padding: 20px;
            font-size: 20px;
            font-weight: 600;
            text-align: center;
        }

        .card-body {
            padding: 20px;
        }

        .table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 15px;
        }

        .table th, .table td {
            padding: 12px 15px;
            text-align: center;
            vertical-align: middle;
        }

        .table th {
            background-color: #0073CF;
            color: #ffffff;
            font-size: 14px;
        }

        .table-striped tbody tr:nth-of-type(odd) {
            background-color: #f9f9f9;
        }

        .table-bordered {
            border: 1px solid #dee2e6;
        }

        .table-bordered th,
        .table-bordered td {
            border: 1px solid #dee2e6;
        }

        .btn-action {
            background-color: #007bff;
            color: white;
            padding: 6px 14px;
            border-radius: 5px;
            border: none;
            font-size: 14px;
            cursor: pointer;
            margin: 2px;
        }

        .btn-delete {
            background-color: #dc3545;
        }

        .btn-action:hover {
            opacity: 0.9;
        }
    </style>

    <div class="container mt-4">
        <div class="card">
            <div class="card-header">Archived Clients</div>
            <div class="card-body">
                <asp:HiddenField ID="hfClientID" runat="server" />

                <asp:GridView ID="gvArchivedClients" runat="server" AutoGenerateColumns="False"
                    CssClass="table table-bordered table-striped" AllowPaging="True" PageSize="10"
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
                                <button type="button" class="btn-action" onclick="confirmRestore('<%# Eval("ClientID") %>')">Restore</button>
                                <button type="button" class="btn-action btn-delete" onclick="confirmDelete('<%# Eval("ClientID") %>')">Delete</button>
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
                confirmButtonColor: '#28a745',
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


