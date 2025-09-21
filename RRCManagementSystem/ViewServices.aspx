<%@ Page Title="View Services" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ViewServices.aspx.cs" Inherits="RRCManagementSystem.ViewServices" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Tailwind CSS CDN -->
    <script src="https://cdn.tailwindcss.com"></script>
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <!-- Font Awesome for icons -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0-beta3/css/all.min.css">
    <!-- SweetAlert2 CDN for modern alerts -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <script>
        tailwind.config = {
            theme: {
                extend: {
                    fontFamily: {
                        sans: ['Inter', 'sans-serif'],
                    },
                }
            }
        }
    </script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto p-4 sm:p-6 md:p-8">
        <h2 class="text-2xl md:text-3xl font-bold text-center mb-6 text-gray-800">
            <i class="fas fa-list-check mr-2 text-blue-600"></i> All Services
        </h2>

        <!-- Responsive wrapper for the GridView table -->
        <div class="overflow-x-auto rounded-xl border border-gray-300 shadow-md">
            <asp:GridView ID="gvServices" runat="server"
                AutoGenerateColumns="False"
                CssClass="w-full text-sm text-gray-700 rounded-xl overflow-hidden"
                DataKeyNames="ServiceID"
                OnRowCommand="gvServices_RowCommand"
                OnRowDataBound="gvServices_RowDataBound"
                EmptyDataText="No services found."
                GridLines="None">
                <HeaderStyle CssClass="bg-gray-200 text-gray-800 font-bold uppercase tracking-wider text-left border-b border-gray-300" />
                <RowStyle CssClass="bg-white border-b border-gray-200 hover:bg-gray-50 transition-colors" />
                <AlternatingRowStyle CssClass="bg-gray-100 border-b border-gray-200 hover:bg-gray-50 transition-colors" />
                <EmptyDataTemplate>
                    <div class="p-6 text-center text-gray-500">
                        <i class="fas fa-search-minus mb-2 text-4xl"></i>
                        <p class="font-semibold">No services found.</p>
                    </div>
                </EmptyDataTemplate>
                <Columns>
                    <asp:BoundField DataField="ServiceID" HeaderText="Service ID" ReadOnly="True" Visible="False" />
                    <asp:BoundField DataField="Name" HeaderText="Service Name" ItemStyle-CssClass="px-6 py-4 border-r border-gray-200" HeaderStyle-CssClass="px-6 py-3" />
                    <asp:BoundField DataField="Description" HeaderText="Description" ItemStyle-CssClass="px-6 py-4 border-r border-gray-200" HeaderStyle-CssClass="px-6 py-3" />
                    <asp:BoundField DataField="ServiceType" HeaderText="Service Type" ItemStyle-CssClass="px-6 py-4 border-r border-gray-200" HeaderStyle-CssClass="px-6 py-3" />

                    <asp:TemplateField HeaderText="Service Price">
                        <ItemTemplate>
                            <asp:LinkButton ID="btnViewPrice" runat="server"
                                CommandName="ViewPrice"
                                CommandArgument='<%# Eval("ServiceID") %>'
                                CssClass="text-blue-600 hover:text-blue-800 font-medium transition-colors duration-200 px-6 py-4 border-r border-gray-200"
                                Text="View Price" />
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <div class="flex items-center justify-center space-x-2 px-6 py-4">
                                <asp:LinkButton ID="btnEdit" runat="server"
                                    CommandName="EditService"
                                    CommandArgument='<%# Eval("ServiceID") %>'
                                    CssClass="text-green-600 hover:text-green-800 font-medium transition-colors duration-200" Text="Edit" />
                                
                                <span class="text-gray-400">|</span>

                                <asp:LinkButton ID="btnDelete" runat="server"
                                    CommandName="DisableService"
                                    CommandArgument='<%# Eval("ServiceID") %>'
                                    CssClass="text-red-600 hover:text-red-800 font-medium transition-colors duration-200"
                                    CausesValidation="false"
                                    UseSubmitBehavior="false"
                                    OnClientClick="return confirmDelete(this, event);"
                                    Text="Delete" />
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>

    <!-- SweetAlert2 for confirmations -->
    <script type="text/javascript">
        function confirmDelete(btn, evt) {
            if (btn.dataset.confirmed === '1') return true;
            if (evt) evt.preventDefault();

            Swal.fire({
                title: 'Delete this service?',
                text: 'This service will be deleted.',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#DC2626',
                cancelButtonColor: '#6B7280',
                confirmButtonText: 'Yes, delete it'
            }).then((result) => {
                if (result.isConfirmed) {
                    btn.dataset.confirmed = '1';
                    btn.click();
                }
            });
            return false;
        }
    </script>
</asp:Content>
