<%@ Page Title="Add New Service" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true"
    CodeBehind="AddServices.aspx.cs" Inherits="RRCManagementSystem.AddServices" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Tailwind CSS -->
    <script src="https://cdn.tailwindcss.com"></script>
    <!-- SweetAlert2 -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <!-- Font Awesome -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" />

    <script>
        // Tailwind config
        tailwind.config = {
            theme: {
                extend: { fontFamily: { sans: ['Inter', 'sans-serif'] } }
            }
        };

        // SweetAlert Confirmation
        function confirmAddService(btn) {
            Swal.fire({
                title: 'Add this service?',
                text: "You can set pricing tiers afterward in the Pricing Settings page.",
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
    </script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="min-h-screen bg-gray-100 p-6 flex justify-center items-center">
        <div class="bg-white shadow-lg rounded-2xl w-full max-w-2xl p-8">
            <h2 class="text-2xl font-bold text-center text-blue-700 mb-6">
                <i class="fas fa-handshake mr-2"></i>Add New Service
            </h2>

            <asp:Label ID="lblMessage" runat="server" CssClass="block text-center text-red-500 font-semibold mb-4"></asp:Label>
            <asp:Literal ID="litScript" runat="server" />

            <!-- Service Name -->
            <div class="mb-5">
                <label for="txtName" class="block font-semibold text-gray-700 mb-2">Service Name *</label>
                <asp:TextBox ID="txtName" runat="server" CssClass="w-full border border-gray-300 rounded-lg px-4 py-2 focus:ring-2 focus:ring-blue-500 focus:outline-none" MaxLength="100" />
            </div>

            <!-- Service Type -->
            <div class="mb-5">
                <label for="ddlServiceType" class="block font-semibold text-gray-700 mb-2">Service Type *</label>
                <asp:DropDownList ID="ddlServiceType" runat="server"
                    CssClass="w-full border border-gray-300 rounded-lg px-4 py-2 focus:ring-2 focus:ring-blue-500 focus:outline-none">
                    <asp:ListItem Text="Select Service Type" Value="" />
                    <asp:ListItem Text="Termite Control" Value="Termite Control" />
                    <asp:ListItem Text="General Pest Control" Value="General Pest Control" />
                </asp:DropDownList>
            </div>

            <!-- Description -->
            <div class="mb-5">
                <label for="txtDescription" class="block font-semibold text-gray-700 mb-2">Description</label>
                <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="4"
                    CssClass="w-full border border-gray-300 rounded-lg px-4 py-2 focus:ring-2 focus:ring-blue-500 focus:outline-none" />
            </div>

            <!-- Buttons -->
            <div class="flex justify-end space-x-4 mt-6">
                <asp:Button ID="btnAddService" runat="server" Text="➕ Add Service"
                    CssClass="bg-blue-600 hover:bg-blue-700 text-white font-bold py-3 px-6 rounded-lg transition"
                    OnClick="btnAddService_Click" UseSubmitBehavior="false" OnClientClick="return confirmAddService(this);" />

                <asp:Button ID="btnCancel" runat="server" Text="Cancel"
                    CssClass="bg-gray-500 hover:bg-gray-600 text-white font-bold py-3 px-6 rounded-lg transition"
                    PostBackUrl="ViewServices.aspx" />
            </div>
        </div>
    </div>
</asp:Content>
