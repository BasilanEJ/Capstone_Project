<%@ Page Title="View Suppliers" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ViewSupplier.aspx.cs" Inherits="RRCManagementSystem.ViewSupplier" Async="true" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Tailwind CSS CDN -->
    <script src="https://cdn.tailwindcss.com"></script>
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <!-- SweetAlert2 -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <!-- Font Awesome for icons -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0-beta3/css/all.min.css">
    
    <script>
        tailwind.config = {
            theme: {
                extend: {
                    fontFamily: {
                        sans: ['Inter', 'sans-serif'],
                    },
                    colors: {
                        primary: {
                            50: '#F0F8FF', 100: '#E6F0FF', 200: '#C0D8FF', 300: '#9AB8FF', 400: '#7599FF',
                            500: '#527BFF', 600: '#2E5FFF', 700: '#1D4ED8', 800: '#1A43AD', 900: '#17398A'
                        },
                    }
                }
            }
        }

        // A centralized function to display messages using SweetAlert
        function showAlert(title, text, icon) {
            Swal.fire({
                title: title,
                text: text,
                icon: icon,
                confirmButtonColor: '#1D4ED8',
            });
        }
    </script>
    
    <script type="text/javascript">
        function swalPostBack(el, opts) {
            Swal.fire({
                title: opts.title || 'Are you sure?',
                text: opts.text || 'Proceed?',
                icon: opts.icon || 'question',
                showCancelButton: true,
                confirmButtonColor: '#1D4ED8',
                cancelButtonColor: '#6B7280',
                confirmButtonText: opts.confirmText || 'Yes',
                cancelButtonText: opts.cancelText || 'Cancel'
            }).then(function (res) {
                if (res.isConfirmed) {
                    var href = el.getAttribute('href');
                    if (href && href.indexOf('__doPostBack') >= 0) {
                        var m = href.match(/__doPostBack\('([^']*)','([^']*)'\)/);
                        if (m) { __doPostBack(m[1], m[2]); }
                        else { eval(href); }
                    } else {
                        eval(href);
                    }
                }
            });
            return false;
        }

        function confirmEdit(el) {
            return swalPostBack(el, { title: 'Edit supplier?', icon: 'question', confirmText: 'Edit' });
        }

        function confirmArchive(el) {
            return swalPostBack(el, {
                title: 'Archive supplier?',
                text: 'This will move the supplier to the archive.',
                icon: 'warning',
                confirmText: 'Archive'
            });
        }

        function openEmailModal(email, name) {
            document.getElementById('<%= hfSupplierEmail.ClientID %>').value = email || '';
            document.getElementById('<%= hfSupplierName.ClientID %>').value = name || '';
            document.getElementById('<%= lblSendTo.ClientID %>').textContent =
                'Sending to: ' + (name ? (name + ' <' + email + '>') : email);
            document.getElementById('emailModal').classList.remove('hidden');
            return false;
        }

        function closeEmailModal() {
            document.getElementById('emailModal').classList.add('hidden');
        }

        function confirmSendEmail(el) {
            Swal.fire({
                title: 'Send email?',
                icon: 'info',
                confirmButtonText: 'Send',
                confirmButtonColor: '#1D4ED8',
                showCancelButton: true,
                cancelButtonText: 'Cancel'
            }).then(function (result) {
                if (result.isConfirmed) {
                    __doPostBack('<%= btnSendEmail.UniqueID %>', '');
                }
            });
            return false;
        }

        function confirmCancelEmail() {
            Swal.fire({
                title: 'Cancel email?',
                text: 'Discard your email content?',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#EF4444',
                cancelButtonColor: '#6B7280',
                confirmButtonText: 'Discard',
                cancelButtonText: 'Stay'
            }).then(function (res) {
                if (res.isConfirmed) {
                    closeEmailModal();
                    document.getElementById('<%= txtSubject.ClientID %>').value = '';
                    document.getElementById('<%= txtMessageBody.ClientID %>').value = '';
                }
            });
            return false;
        }
    </script>
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <asp:HiddenField ID="hfSupplierEmail" runat="server" />
    <asp:HiddenField ID="hfSupplierName" runat="server" />
    <asp:Panel ID="pnlSendEmail" runat="server" Visible="false" style="display:none;"></asp:Panel>
    
    <div class="main-content bg-gray-100 min-h-screen p-6 sm:p-8">
        <div class="supplier-container bg-white rounded-xl shadow-lg p-6 sm:p-8 max-w-7xl mx-auto">
            <h2 class="text-2xl sm:text-3xl font-bold text-primary-700 mb-6 flex items-center">
                <i class="fas fa-list-ul mr-3 text-primary-500"></i> Suppliers List
            </h2>

            <div class="overflow-x-auto rounded-xl border border-gray-200 shadow-md">
                <asp:GridView ID="gvSuppliers" runat="server" AutoGenerateColumns="False"
                    OnRowCommand="gvSuppliers_RowCommand"
                    CssClass="min-w-full divide-y divide-gray-200 text-sm"
                    EmptyDataText="No suppliers found.">
                    <HeaderStyle CssClass="bg-primary-700 text-white font-semibold text-left text-sm uppercase tracking-wider" />
                    <RowStyle CssClass="bg-white border-b border-gray-200 text-gray-700 hover:bg-gray-50 transition-colors duration-150" />
                    <AlternatingRowStyle CssClass="bg-gray-50 border-b border-gray-200 text-gray-700 hover:bg-gray-100 transition-colors duration-150" />
                    <Columns>
                        <asp:BoundField DataField="SupplierID" HeaderText="ID" ItemStyle-CssClass="px-4 py-3 border-r border-gray-200" />
                        <asp:BoundField DataField="Name" HeaderText="Name" ItemStyle-CssClass="px-4 py-3 border-r border-gray-200" />
                        <asp:BoundField DataField="CompanyName" HeaderText="Company Name" ItemStyle-CssClass="px-4 py-3 border-r border-gray-200 hidden sm:table-cell" />
                        <asp:BoundField DataField="BusinessType" HeaderText="Business Type" ItemStyle-CssClass="px-4 py-3 border-r border-gray-200 hidden md:table-cell" />
                        <asp:BoundField DataField="ContactNumber" HeaderText="Contact #" ItemStyle-CssClass="px-4 py-3 border-r border-gray-200" />
                        <asp:BoundField DataField="Email" HeaderText="Email" ItemStyle-CssClass="px-4 py-3 border-r border-gray-200 hidden lg:table-cell" />
                        <asp:BoundField DataField="Status" HeaderText="Status" ItemStyle-CssClass="px-4 py-3 border-r border-gray-200 hidden md:table-cell" />
                        <asp:BoundField DataField="CreatedAt" HeaderText="Date Added" DataFormatString="{0:yyyy-MM-dd}" ItemStyle-CssClass="px-4 py-3 border-r border-gray-200 hidden md:table-cell" />
                        
                        <asp:TemplateField HeaderText="Actions" ItemStyle-CssClass="px-4 py-3 text-center border-l border-gray-200">
                            <ItemTemplate>
                                <div class="flex flex-row flex-wrap items-center justify-center sm:space-x-4 space-x-2">
                                    <a href="#" class="text-blue-600 hover:text-blue-800 transition-colors duration-150"
                                        title="Send Email"
                                        onclick="return openEmailModal('<%# Eval("Email") %>', '<%# (Eval("Name") ?? "").ToString().Replace("'", "\\'") %>');">
                                        <i class="fas fa-envelope"></i>
                                    </a>
                                    <asp:LinkButton ID="btnEdit" runat="server"
                                        CommandName="EditSupplier"
                                        CommandArgument='<%# Eval("SupplierID") %>'
                                        Text="<i class='fas fa-edit'></i>"
                                        CssClass="text-green-600 hover:text-green-800 transition-colors duration-150"
                                        OnClientClick="return confirmEdit(this);" />
                                    <asp:LinkButton ID="btnArchive" runat="server"
                                        CommandName="ArchiveSupplier"
                                        CommandArgument='<%# Eval("SupplierID") %>'
                                        Text="<i class='fas fa-archive'></i>"
                                        CssClass="text-red-600 hover:text-red-800 transition-colors duration-150"
                                        OnClientClick="return confirmArchive(this);" />
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </div>
    
    <!-- Email Modal -->
    <div id="emailModal" class="hidden fixed inset-0 z-50 overflow-y-auto">
        <div class="flex items-center justify-center min-h-screen p-4">
            <div class="fixed inset-0 bg-gray-900 bg-opacity-50 transition-opacity"></div>
            
            <div class="bg-white rounded-xl shadow-2xl max-w-2xl w-full z-50 relative p-6 space-y-4">
                <div class="flex justify-between items-center pb-4 border-b border-gray-200">
                    <h5 class="text-xl font-bold text-gray-800">Send Email to Supplier</h5>
                    <button type="button" onclick="closeEmailModal()" class="text-gray-400 hover:text-gray-600 focus:outline-none">
                        <i class="fas fa-times"></i>
                    </button>
                </div>

                <asp:Label ID="lblSendTo" runat="server" CssClass="block text-sm text-gray-600"></asp:Label>
                
                <asp:TextBox ID="txtSubject" runat="server"
                    CssClass="w-full p-3 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:outline-none"
                    placeholder="Subject"></asp:TextBox>
                
                <asp:TextBox ID="txtMessageBody" runat="server" TextMode="MultiLine" Rows="8"
                    CssClass="w-full p-3 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:outline-none"
                    placeholder="Type your message here..."></asp:TextBox>
                
                <div class="flex justify-end pt-4 space-x-3">
                    <asp:Button ID="btnSendEmail" runat="server" Text="Send Email"
                        CssClass="bg-primary-700 hover:bg-primary-800 text-white font-semibold px-4 py-2 rounded-lg transition-colors duration-200"
                        OnClientClick="return confirmSendEmail(this);"
                        OnClick="btnSendEmail_Click" UseSubmitBehavior="false" />
                        
                    <button type="button" onclick="return confirmCancelEmail();"
                        class="bg-gray-300 hover:bg-gray-400 text-gray-800 font-semibold px-4 py-2 rounded-lg transition-colors duration-200">
                        Cancel
                    </button>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
