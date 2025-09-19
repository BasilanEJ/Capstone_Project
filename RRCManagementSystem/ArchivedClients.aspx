<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ArchivedClients.aspx.cs" Inherits="RRCManagementSystem.ArchivedClients" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto py-8 px-4">
        <div class="bg-white rounded-xl shadow-lg overflow-hidden border border-gray-200">
            <div class="bg-blue-800 text-white text-center font-bold p-5">
                Archived Clients
            </div>

            <div class="p-6">
                <asp:HiddenField ID="hfClientID" runat="server" />

                <div class="overflow-x-auto rounded-lg border border-gray-200 shadow-sm">
                    <asp:GridView ID="gvArchivedClients" runat="server" AutoGenerateColumns="False"
                        CssClass="min-w-full divide-y divide-gray-200 border-collapse"
                        AllowPaging="True" PageSize="10"
                        OnPageIndexChanging="gvArchivedClients_PageIndexChanging"
                        OnRowCommand="gvArchivedClients_RowCommand">
                        <HeaderStyle CssClass="bg-gray-50 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" />
                        <RowStyle CssClass="bg-white even:bg-gray-50 hover:bg-gray-100 transition-colors" />
                        <PagerStyle CssClass="bg-white text-gray-500 font-medium py-3 px-4 flex justify-between items-center" />
                        <PagerSettings Mode="NumericFirstLast" />
                        <EmptyDataTemplate>
                            <div class="py-4 px-6 text-center text-sm text-gray-500">
                                No archived accounts found.
                            </div>
                        </EmptyDataTemplate>
                        <Columns>
                            <asp:BoundField DataField="ClientID" HeaderText="Client ID" ReadOnly="True" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-900 border-r border-gray-200" />
                            <asp:BoundField DataField="Name" HeaderText="Name" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900 border-r border-gray-200" />
                            <asp:BoundField DataField="Email" HeaderText="Email" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500 border-r border-gray-200" />
                            <asp:BoundField DataField="ContactNumber" HeaderText="Contact Number" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500 border-r border-gray-200" />
                            <asp:BoundField DataField="City" HeaderText="City" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500 border-r border-gray-200" />
                            <asp:BoundField DataField="Country" HeaderText="Country" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500 border-r border-gray-200" />
                            <asp:TemplateField HeaderText="Actions" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                                <ItemTemplate>
                                    <div class="flex items-center space-x-2">
                                        <button type="button" class="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-green-600 hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-green-500 transition-colors" onclick="confirmRestore('<%# Eval("ClientID") %>')">
                                            Restore
                                        </button>
                                        <button type="button" class="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-red-600 hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 transition-colors" onclick="confirmDelete('<%# Eval("ClientID") %>')">
                                            Delete
                                        </button>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>

                <asp:Button ID="btnRestoreHidden" runat="server" OnClick="btnRestoreHidden_Click" style="display:none;" />
                <asp:Button ID="btnDeleteHidden" runat="server" OnClick="btnDeleteHidden_Click" style="display:none;" />
            </div>
        </div>
    </div>

    <script type="text/javascript">
        function confirmRestore(clientId) {
            Swal.fire({
                title: 'Restore Client?',
                text: 'This will change the client status to Active.',
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#16a34a', // Tailwind's green-600
                cancelButtonColor: '#4b5563', // Tailwind's gray-600
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
                confirmButtonColor: '#dc2626', // Tailwind's red-600
                cancelButtonColor: '#4b5563', // Tailwind's gray-600
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