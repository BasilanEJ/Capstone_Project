<%@ Page Title="Service Pricing" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ViewServicePricing.aspx.cs" Inherits="RRCManagementSystem.ViewServicePricing" %>

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
            <i class="fas fa-sack-dollar mr-2 text-blue-600"></i> Service Pricing
        </h2>

        <asp:HiddenField ID="hfServiceID" runat="server" />

        <!-- Responsive wrapper for the GridView table -->
        <div class="overflow-x-auto rounded-xl border border-gray-300 shadow-md mb-8">
            <asp:GridView ID="gvServicePricing" runat="server"
                AutoGenerateColumns="False"
                CssClass="w-full text-sm text-gray-700 rounded-xl overflow-hidden"
                DataKeyNames="PricingID"
                OnRowEditing="gvServicePricing_RowEditing"
                OnRowUpdating="gvServicePricing_RowUpdating"
                OnRowCancelingEdit="gvServicePricing_RowCancelingEdit"
                OnRowCommand="gvServicePricing_RowCommand"
                EmptyDataText="No pricing data found."
                GridLines="None">
                <HeaderStyle CssClass="bg-gray-200 text-gray-800 font-bold uppercase tracking-wider text-left border-b border-gray-300" />
                <RowStyle CssClass="bg-white border-b border-gray-200 hover:bg-gray-50 transition-colors" />
                <AlternatingRowStyle CssClass="bg-gray-100 border-b border-gray-200 hover:bg-gray-50 transition-colors" />
                <EmptyDataTemplate>
                    <div class="p-6 text-center text-gray-500">
                        <i class="fas fa-search-minus mb-2 text-4xl"></i>
                        <p class="font-semibold">No pricing data found.</p>
                    </div>
                </EmptyDataTemplate>
                <Columns>
                    <asp:BoundField DataField="PricingID" HeaderText="Pricing ID" ReadOnly="True" Visible="False" />
                    <asp:BoundField DataField="Name" HeaderText="Service Name" ReadOnly="True" ItemStyle-CssClass="px-6 py-4 border-r border-gray-200" HeaderStyle-CssClass="px-6 py-3" />
                    <asp:BoundField DataField="MinSQM" HeaderText="Min SQM" ReadOnly="True" ItemStyle-CssClass="px-6 py-4 border-r border-gray-200" HeaderStyle-CssClass="px-6 py-3" />
                    <asp:BoundField DataField="MaxSQM" HeaderText="Max SQM" ReadOnly="True" ItemStyle-CssClass="px-6 py-4 border-r border-gray-200" HeaderStyle-CssClass="px-6 py-3" />

                    <asp:TemplateField HeaderText="Price" HeaderStyle-CssClass="px-6 py-3">
                        <ItemTemplate>
                            <div class="px-6 py-4 border-r border-gray-200">
                                <%# Eval("Price", "{0:N2}") %>
                            </div>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <div class="px-6 py-4">
                                <asp:TextBox ID="txtEditPrice" runat="server" CssClass="form-input w-full px-2 py-1 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500" Text='<%# Bind("Price", "{0:N2}") %>' />
                            </div>
                        </EditItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Actions" HeaderStyle-CssClass="px-6 py-3 text-center">
                        <ItemTemplate>
                            <div class="flex items-center justify-center space-x-2 px-6 py-4">
                                <asp:LinkButton ID="btnEdit" runat="server"
                                    CommandName="Edit" Text="Edit"
                                    CssClass="text-green-600 hover:text-green-800 font-medium transition-colors duration-200" />
                                
                                <span class="text-gray-400">|</span>
                                
                                <asp:LinkButton ID="btnDelete" runat="server"
                                    CommandName="DeletePrice"
                                    CommandArgument='<%# Eval("PricingID") %>'
                                    CssClass="text-red-600 hover:text-red-800 font-medium transition-colors duration-200"
                                    CausesValidation="false"
                                    UseSubmitBehavior="false"
                                    OnClientClick="return confirmDelete(this, event);"
                                    Text="Delete" />
                            </div>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <div class="flex items-center justify-center space-x-2 px-6 py-4">
                                <asp:LinkButton ID="btnUpdate" runat="server"
                                    CommandName="Update" Text="Update"
                                    CssClass="text-blue-600 hover:text-blue-800 font-medium transition-colors duration-200" />
                                
                                <span class="text-gray-400">|</span>
                                
                                <asp:LinkButton ID="btnCancel" runat="server"
                                    CommandName="Cancel" Text="Cancel"
                                    CssClass="text-gray-600 hover:text-gray-800 font-medium transition-colors duration-200" />
                            </div>
                        </EditItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

        <!-- Back Button Below GridView -->
        <div class="text-center">
            <asp:Button ID="btnBack" runat="server" Text="← Back to Services"
                CssClass="inline-flex items-center px-6 py-3 bg-gray-500 hover:bg-gray-600 text-white font-bold rounded-lg transition-colors duration-200 focus:outline-none focus:ring-2 focus:ring-gray-400 focus:ring-offset-2"
                OnClick="btnBack_Click" />
        </div>
    </div>

    <!-- SweetAlert2 for confirmations -->
    <script type="text/javascript">
        function confirmDelete(btn, evt) {
            if (btn.dataset.confirmed === '1') return true;
            if (evt) evt.preventDefault();

            Swal.fire({
                title: 'Delete this price range?',
                text: 'This pricing record will be permanently deleted.',
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
