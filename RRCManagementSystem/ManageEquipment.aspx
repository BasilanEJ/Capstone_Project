<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ManageEquipment.aspx.cs" Inherits="RRCManagementSystem.ManageEquipment" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto px-4 py-12">
        <h2 class="text-3xl text-center mb-8 font-semibold text-gray-800">🔧 Manage Equipment</h2>

        <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 justify-items-center">

            <div class="w-full">
                <a href="AddEquipment.aspx" class="block">
                    <div class="bg-white shadow-lg rounded-xl p-6 h-full flex flex-col justify-center items-center text-center transition-transform duration-300 hover:scale-105 hover:shadow-2xl">
                        <i class="fas fa-plus text-3xl text-blue-600 mb-4"></i>
                        <h5 class="text-xl font-semibold text-gray-800">Add Equipment</h5>
                    </div>
                </a>
            </div>

            <div class="w-full">
                <a href="ViewEquipment.aspx" class="block">
                    <div class="bg-white shadow-lg rounded-xl p-6 h-full flex flex-col justify-center items-center text-center transition-transform duration-300 hover:scale-105 hover:shadow-2xl">
                        <i class="fas fa-list text-3xl text-blue-600 mb-4"></i>
                        <h5 class="text-xl font-semibold text-gray-800">View Equipment</h5>
                    </div>
                </a>
            </div>

        </div>
    </div>

    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.all.min.js"></script>

    <script>
        // Your existing script for SweetAlert remains the same
    </script>
</asp:Content>
