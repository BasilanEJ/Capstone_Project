<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ManageClient.aspx.cs" Inherits="RRCManagementSystem.ManageClient" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto px-4 py-12">
        <h2 class="text-3xl text-center mb-8 font-semibold text-gray-800">👤 Manage Clients</h2>

        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 justify-items-center">

            <div class="w-full">
                <a href="ClientsProfiles.aspx" class="block">
                    <div class="bg-white shadow-lg rounded-xl p-6 h-full flex flex-col justify-center items-center text-center transition-transform duration-300 hover:scale-105 hover:shadow-2xl">
                        <i class="fas fa-user text-3xl text-blue-600 mb-4"></i>
                        <h5 class="text-xl font-semibold text-gray-800">View Client Profiles</h5>
                    </div>
                </a>
            </div>

            <div class="w-full">
                <a href="ArchivedClients.aspx" class="block">
                    <div class="bg-white shadow-lg rounded-xl p-6 h-full flex flex-col justify-center items-center text-center transition-transform duration-300 hover:scale-105 hover:shadow-2xl">
                        <i class="fas fa-archive text-3xl text-blue-600 mb-4"></i>
                        <h5 class="text-xl font-semibold text-gray-800">Archive Client Accounts</h5>
                    </div>
                </a>
            </div>

            <%--
            <div class="w-full">
                <a href="ClientsHistory.aspx" class="block">
                    <div class="bg-white shadow-lg rounded-xl p-6 h-full flex flex-col justify-center items-center text-center transition-transform duration-300 hover:scale-105 hover:shadow-2xl">
                        <i class="fas fa-history text-3xl text-blue-600 mb-4"></i>
                        <h5 class="text-xl font-semibold text-gray-800">Clients History / Transactions</h5>
                    </div>
                </a>
            </div>

            <div class="w-full">
                <a href="SendNotifications.aspx" class="block">
                    <div class="bg-white shadow-lg rounded-xl p-6 h-full flex flex-col justify-center items-center text-center transition-transform duration-300 hover:scale-105 hover:shadow-2xl">
                        <i class="fas fa-bell text-3xl text-blue-600 mb-4"></i>
                        <h5 class="text-xl font-semibold text-gray-800">Send Notifications</h5>
                    </div>
                </a>
            </div>
            --%>

            <div class="w-full">
                <a href="ManageContract.aspx" class="block">
                    <div class="bg-white shadow-lg rounded-xl p-6 h-full flex flex-col justify-center items-center text-center transition-transform duration-300 hover:scale-105 hover:shadow-2xl">
                        <i class="fas fa-file-signature text-3xl text-blue-600 mb-4"></i>
                        <h5 class="text-xl font-semibold text-gray-800">Manage Contracts</h5>
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