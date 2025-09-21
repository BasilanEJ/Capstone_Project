<%@ Page Title="Add Services" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AddServices.aspx.cs" Inherits="RRCManagementSystem.AddServices" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Tailwind CSS CDN -->
    <script src="https://cdn.tailwindcss.com"></script>
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <!-- Font Awesome for icons -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0-beta3/css/all.min.css">
    <!-- SweetAlert2 CDN -->
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
    <div class="main-content min-h-screen p-4 sm:p-8 md:p-12 bg-gray-100">
        <div class="card bg-white rounded-xl shadow-lg overflow-hidden max-w-2xl mx-auto">
            <div class="card-header bg-blue-600 text-white p-4 text-center">
                <h2 class="text-xl font-bold"><i class="fas fa-handshake mr-2"></i> Add New Service</h2>
            </div>
            
            <div class="card-body p-6">
                <asp:Label ID="lblMessage" runat="server" CssClass="block text-center text-red-500 font-semibold mb-4"></asp:Label>
                <asp:Literal ID="litScript" runat="server" />

                <!-- Service Name -->
                <div class="mb-4">
                    <label for="txtName" class="block font-semibold text-gray-700 mb-2">Service Name *</label>
                    <asp:TextBox ID="txtName" runat="server" CssClass="form-input w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors" MaxLength="100" />
                </div>

                <!-- Service Type -->
                <div class="mb-4">
                    <label for="ddlServiceType" class="block font-semibold text-gray-700 mb-2">Service Type *</label>
                    <asp:DropDownList ID="ddlServiceType" runat="server" CssClass="form-select w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors">
                        <asp:ListItem Text="Select Service Type" Value="" />
                        <asp:ListItem Text="Termite Control" Value="Termite Control" />
                        <asp:ListItem Text="General Pest Control" Value="General Pest Control" />
                    </asp:DropDownList>
                </div>

                <!-- Description -->
                <div class="mb-4">
                    <label for="txtDescription" class="block font-semibold text-gray-700 mb-2">Description</label>
                    <asp:TextBox ID="txtDescription" runat="server" CssClass="form-textarea w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors" TextMode="MultiLine" Rows="3" MaxLength="500" />
                </div>

                <!-- Pricing by SQM Range -->
                <div class="mb-6">
                    <label class="block font-semibold text-gray-700 mb-2">Pricing by SQM Range *</label>
                    <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
                        <div class="space-y-1">
                            <label class="block text-sm text-gray-600">0 - 100 SQM</label>
                            <asp:TextBox ID="txtPrice_0_100" runat="server" CssClass="form-input w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors" />
                        </div>
                        <div class="space-y-1">
                            <label class="block text-sm text-gray-600">101 - 250 SQM</label>
                            <asp:TextBox ID="txtPrice_101_250" runat="server" CssClass="form-input w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors" />
                        </div>
                        <div class="space-y-1">
                            <label class="block text-sm text-gray-600">251 - 400 SQM</label>
                            <asp:TextBox ID="txtPrice_251_400" runat="server" CssClass="form-input w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors" />
                        </div>
                        <div class="space-y-1">
                            <label class="block text-sm text-gray-600">401 - 600 SQM</label>
                            <asp:TextBox ID="txtPrice_401_600" runat="server" CssClass="form-input w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors" />
                        </div>
                        <div class="space-y-1">
                            <label class="block text-sm text-gray-600">601 - 800 SQM</label>
                            <asp:TextBox ID="txtPrice_601_800" runat="server" CssClass="form-input w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors" />
                        </div>
                        <div class="space-y-1">
                            <label class="block text-sm text-gray-600">801 - 1000 SQM</label>
                            <asp:TextBox ID="txtPrice_801_1000" runat="server" CssClass="form-input w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors" />
                        </div>
                        <div class="space-y-1">
                            <label class="block text-sm text-gray-600">1000+ SQM</label>
                            <asp:TextBox ID="txtPrice_1000_Plus" runat="server" CssClass="form-input w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors" />
                        </div>
                    </div>
                </div>

                <!-- Buttons -->
                <div class="flex flex-col sm:flex-row justify-end space-y-4 sm:space-y-0 sm:space-x-4 mt-8">
                    <asp:Button ID="Button1" runat="server" Text="➕ Add Service"
                        CssClass="custom-btn bg-blue-600 hover:bg-blue-700 text-white font-bold py-3 px-6 rounded-lg transition-colors duration-200 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
                        OnClick="btnSubmit_Click"
                        UseSubmitBehavior="false"
                        OnClientClick="return confirmAddService(this);" />
                    
                    <asp:Button ID="Button2" runat="server" Text="Cancel"
                        CssClass="custom-btn bg-gray-500 hover:bg-gray-600 text-white font-bold py-3 px-6 rounded-lg transition-colors duration-200 focus:outline-none focus:ring-2 focus:ring-gray-400 focus:ring-offset-2"
                        PostBackUrl="ViewServices.aspx" />
                </div>
            </div>
        </div>
    </div>

    <!-- JAVASCRIPT -->
    <script type="text/javascript">
        function confirmAddService(btn) {
            Swal.fire({
                title: 'Are you sure?',
                text: "Do you want to add this service?",
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#2563EB',
                cancelButtonColor: '#6B7280',
                confirmButtonText: 'Yes, add it!'
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack(btn.name, '');
                }
            });
            return false;
        }

        function showSuccessAlert() {
            Swal.fire({
                icon: 'success',
                title: 'Success!',
                text: 'Service added successfully.',
                confirmButtonColor: '#2563EB'
            });
        }
    </script>
</asp:Content>
