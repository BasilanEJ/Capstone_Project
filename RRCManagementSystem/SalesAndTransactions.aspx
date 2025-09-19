<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="SalesAndTransactions.aspx.cs" Inherits="RRCManagementSystem.SalesAndTransactions" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- FontAwesome for icons -->
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" rel="stylesheet" />
    <!-- SweetAlert2 CSS is kept as a separate library -->
    <link href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <!-- Main container with Tailwind utilities for padding and centering -->
    <div class="container mx-auto px-4 py-12">
        <h2 class="text-3xl text-center mb-8 font-semibold text-gray-800">💰 Sales & Transactions</h2>

        <!-- Grid layout for cards with gap -->
        <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6 justify-items-center">

            <!-- <a href="ViewSales.aspx" class="sales-card">
                 <i class="fas fa-chart-line"></i>
                 <div class="sales-card-title">View Sales Summary</div>
             </a> -->

            <div class="w-full">
                <a href="TransactionHistory.aspx" class="block">
                    <!-- The card styling uses Tailwind classes. Note the blue color applied to the icon for consistency. -->
                    <div class="bg-white shadow-lg rounded-xl p-6 h-full flex flex-col justify-center items-center text-center transition-transform duration-300 hover:scale-105 hover:shadow-2xl">
                        <i class="fas fa-receipt text-3xl text-blue-600 mb-4"></i>
                        <h5 class="text-xl font-semibold text-gray-800">Transaction History</h5>
                    </div>
                </a>
            </div>

            <div class="w-full">
                <a href="ManagePayment.aspx" class="block">
                    <div class="bg-white shadow-lg rounded-xl p-6 h-full flex flex-col justify-center items-center text-center transition-transform duration-300 hover:scale-105 hover:shadow-2xl">
                        <i class="fas fa-credit-card text-3xl text-blue-600 mb-4"></i>
                        <h5 class="text-xl font-semibold text-gray-800">Manage Payment</h5>
                    </div>
                </a>
            </div>

            <!-- New link converted to Tailwind CSS -->
            <div class="w-full">
                <a href="ViewPaymentBalance.aspx" class="block">
                    <div class="bg-white shadow-lg rounded-xl p-6 h-full flex flex-col justify-center items-center text-center transition-transform duration-300 hover:scale-105 hover:shadow-2xl">
                        <i class="fas fa-wallet text-3xl text-blue-600 mb-4"></i>
                        <h5 class="text-xl font-semibold text-gray-800">View Payment Balances</h5>
                    </div>
                </a>
            </div>
        </div>
    </div>

    <!-- SweetAlert2 JS is kept as a separate library -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.all.min.js"></script>
    <script>
        // Your existing script for SweetAlert remains the same
    </script>
</asp:Content>