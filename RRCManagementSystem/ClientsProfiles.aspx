<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ClientsProfiles.aspx.cs" Inherits="RRCManagementSystem.ClientsProfiles" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    </asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto py-8 px-4">
        <div class="bg-white rounded-xl shadow-lg overflow-hidden border border-gray-200">
            <div class="bg-blue-800 text-white text-center font-bold p-5">
                Approved Client Profiles
            </div>

            <div class="p-6">
                <div class="flex flex-col md:flex-row items-center justify-between mb-6 space-y-4 md:space-y-0">
                    <h2 class="text-2xl font-bold text-gray-800">Client Profiles</h2>
                    </div>

                <div class="overflow-x-auto rounded-lg border border-gray-200 shadow-sm">
                    <asp:GridView ID="gvClients" runat="server" AutoGenerateColumns="False"
                        CssClass="min-w-full divide-y divide-gray-200 border-collapse"
                        AllowPaging="True" PageSize="10"
                        OnPageIndexChanging="gvClients_PageIndexChanging"
                        OnRowCommand="gvClients_RowCommand">
                        <HeaderStyle CssClass="bg-gray-50 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" />
                        <RowStyle CssClass="bg-white even:bg-gray-50 hover:bg-gray-100 transition-colors" />
                        <PagerStyle CssClass="bg-white text-gray-500 font-medium py-3 px-4 flex justify-between items-center" />
                        <PagerSettings Mode="NumericFirstLast" />
                        <EmptyDataTemplate>
                            <div class="py-4 px-6 text-center text-sm text-gray-500">
                                No client profiles found.
                            </div>
                        </EmptyDataTemplate>
                        <Columns>
                           <asp:BoundField DataField="ClientID" HeaderText="Client ID" ReadOnly="True" Visible="False" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-900 border-r border-gray-200" />

                            <asp:TemplateField HeaderText="Name" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900 border-r border-gray-200">
                                <ItemTemplate>
                                    <%# Eval("LastName") %>, <%# Eval("FirstName") %> <%# Eval("MiddleName") %>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:BoundField DataField="Email" HeaderText="Email" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500 border-r border-gray-200" />
                            <asp:BoundField DataField="ContactNumber" HeaderText="Contact Number" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500 border-r border-gray-200" />
                            <asp:BoundField DataField="City" HeaderText="City" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500 border-r border-gray-200" />
                            <asp:BoundField DataField="Country" HeaderText="Country" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500 border-r border-gray-200" />

                            <asp:TemplateField HeaderText="Actions" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                                <ItemTemplate>
                                    <div class="flex items-center space-x-2">
                                        <asp:Button ID="btnView" runat="server"
                                            CssClass="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors"
                                            Text="View Profile"
                                            CommandName="ViewProfile"
                                            CommandArgument='<%# Eval("ClientID") %>' />

                                        <asp:Button ID="btnArchive" runat="server"
                                            CssClass="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-red-600 hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 transition-colors"
                                            Text="Archive"
                                            CommandName="ArchiveClient"
                                            CommandArgument='<%# Eval("ClientID") %>'
                                            UseSubmitBehavior="false"
                                            OnClientClick="return confirmArchive(this);" />
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        function confirmArchive(btn) {
            // Stop the normal submit right away
            if (window.event) window.event.preventDefault();

            Swal.fire({
                title: 'Archive Client?',
                text: 'The client will be archived and no longer appear in the active list.',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#2563eb', // Tailwind's blue-600
                cancelButtonColor: '#dc2626', // Tailwind's red-600
                confirmButtonText: 'Yes, archive'
            }).then((result) => {
                if (result.isConfirmed) {
                    // Use a small delay to ensure the event is fully handled before postback
                    setTimeout(function () {
                        // Trigger the WebForms postback for this specific button
                        __doPostBack(btn.name, '');
                    }, 100);
                }
            });
            // Always cancel the original click; we'll post back ourselves if confirmed
            return false;
        }
    </script>
</asp:Content>