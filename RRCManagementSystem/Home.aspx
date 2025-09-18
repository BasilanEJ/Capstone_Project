<%@ Page Title="Home" Language="C#" MasterPageFile="~/Client.master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="RRCManagementSystem.Home" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.tailwindcss.com"></script>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="max-w-7xl mx-auto px-4 py-10">
        <!-- Welcome Section -->
        <div class="text-center mb-12">
            <h1 class="text-3xl md:text-4xl font-extrabold text-blue-700 tracking-tight">
                Welcome, <%= Session["Name"] ?? "Valued Client" %>!
            </h1>
            <p class="text-gray-600 text-lg mt-2 max-w-2xl mx-auto">
                Manage your bookings, check payments, and stay connected with us.
            </p>
        </div>

        <!-- Feature Grid -->
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-8">
            <!-- Book a Service -->
            <div class="group relative bg-white/80 backdrop-blur-lg rounded-2xl shadow-md hover:shadow-xl transition transform hover:-translate-y-2 hover:border-blue-500 border border-gray-100 p-6 text-center">
                <div class="absolute top-0 right-0 mt-3 mr-3 text-blue-100 group-hover:text-blue-300 transition">
                    <i class="fas fa-star text-2xl"></i>
                </div>
                <div class="bg-gradient-to-tr from-blue-100 to-blue-200 w-16 h-16 mx-auto rounded-full flex items-center justify-center mb-5 group-hover:scale-110 transition">
                    <i class="fas fa-calendar-check text-blue-700 text-2xl"></i>
                </div>
                <h5 class="text-lg font-semibold mb-2 text-gray-800">Book a Service</h5>
                <p class="text-gray-500 mb-4 text-sm md:text-base">
                    Schedule a new pest control service in just a few taps.
                </p>
                <a href="BookService.aspx"
                   class="inline-block bg-blue-600 hover:bg-blue-700 text-white px-5 py-2 rounded-lg font-medium shadow transition">
                   Book Now
                </a>
            </div>

            <!-- My Bookings -->
            <div class="group relative bg-white/80 backdrop-blur-lg rounded-2xl shadow-md hover:shadow-xl transition transform hover:-translate-y-2 hover:border-blue-500 border border-gray-100 p-6 text-center">
                <div class="absolute top-0 right-0 mt-3 mr-3 text-blue-100 group-hover:text-blue-300 transition">
                    <i class="fas fa-clock text-2xl"></i>
                </div>
                <div class="bg-gradient-to-tr from-blue-100 to-blue-200 w-16 h-16 mx-auto rounded-full flex items-center justify-center mb-5 group-hover:scale-110 transition">
                    <i class="fas fa-clipboard-list text-blue-700 text-2xl"></i>
                </div>
                <h5 class="text-lg font-semibold mb-2 text-gray-800">My Bookings</h5>
                <p class="text-gray-500 mb-4 text-sm md:text-base">
                    Track upcoming and completed service bookings.
                </p>
                <a href="MyBookings.aspx"
                   class="inline-block bg-blue-600 hover:bg-blue-700 text-white px-5 py-2 rounded-lg font-medium shadow transition">
                   View Bookings
                </a>
            </div>

            <!-- Our Contract -->
            <div class="group relative bg-white/80 backdrop-blur-lg rounded-2xl shadow-md hover:shadow-xl transition transform hover:-translate-y-2 hover:border-blue-500 border border-gray-100 p-6 text-center">
                <div class="absolute top-0 right-0 mt-3 mr-3 text-blue-100 group-hover:text-blue-300 transition">
                    <i class="fas fa-file-signature text-2xl"></i>
                </div>
                <div class="bg-gradient-to-tr from-blue-100 to-blue-200 w-16 h-16 mx-auto rounded-full flex items-center justify-center mb-5 group-hover:scale-110 transition">
                    <i class="fas fa-file-contract text-blue-700 text-2xl"></i>
                </div>
                <h5 class="text-lg font-semibold mb-2 text-gray-800">Our Contract</h5>
                <p class="text-gray-500 mb-4 text-sm md:text-base">
                    Review your contract and service terms anytime.
                </p>
                <a href="OurContract.aspx"
                   class="inline-block bg-blue-600 hover:bg-blue-700 text-white px-5 py-2 rounded-lg font-medium shadow transition">
                   View Contract
                </a>
            </div>

            <!-- Pending Payments -->
            <div class="group relative bg-white/80 backdrop-blur-lg rounded-2xl shadow-md hover:shadow-xl transition transform hover:-translate-y-2 hover:border-blue-500 border border-gray-100 p-6 text-center">
                <div class="absolute top-0 right-0 mt-3 mr-3 text-blue-100 group-hover:text-blue-300 transition">
                    <i class="fas fa-wallet text-2xl"></i>
                </div>
                <div class="bg-gradient-to-tr from-blue-100 to-blue-200 w-16 h-16 mx-auto rounded-full flex items-center justify-center mb-5 group-hover:scale-110 transition">
                    <i class="fas fa-credit-card text-blue-700 text-2xl"></i>
                </div>
                <h5 class="text-lg font-semibold mb-2 text-gray-800">Pending Payments</h5>
                <p class="text-gray-500 mb-4 text-sm md:text-base">
                    Check and settle your pending balances easily.
                </p>
                <a href="Payment.aspx"
                   class="inline-block bg-blue-600 hover:bg-blue-700 text-white px-5 py-2 rounded-lg font-medium shadow transition">
                   Check Payments
                </a>
            </div>
        </div>

        <!-- Accessibility for Screen Readers -->
        <span class="sr-only">
            Use the buttons above to navigate to booking, tracking, contracts, and payments.
        </span>
    </div>
</asp:Content>
