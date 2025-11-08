<%@ Page Title="Home" Language="C#" MasterPageFile="~/Client.master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="RRCManagementSystem.Home" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .feature-card {
            transition: all 0.3s ease;
            position: relative;
        }

        .feature-card:hover {
            transform: translateY(-8px);
            box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
        }

        .gradient-text {
            background: linear-gradient(135deg, #3b82f6, #1e40af);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
        }

        .stat-card {
            transition: transform 0.3s ease;
        }

        .stat-card:hover {
            transform: translateY(-4px);
        }

        /* Responsive grid adjustments for better PC layout */
        @media (min-width: 1024px) {
            .services-grid {
                display: grid;
                grid-template-columns: repeat(2, 1fr);
                gap: 1.5rem;
            }
        }

        @media (min-width: 1280px) {
            .services-grid {
                grid-template-columns: repeat(3, 1fr);
            }
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <!-- Hero Section -->
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <div class="text-center mb-12">
            <div class="inline-flex items-center bg-blue-50 rounded-full px-4 py-2 mb-6">
                <i class="fas fa-home text-blue-600 mr-2"></i>
                <span class="text-sm font-medium text-gray-700">Client Portal</span>
            </div>
            
            <h1 class="text-4xl sm:text-5xl lg:text-6xl font-bold mb-4">
                Welcome, <span class="gradient-text"><%= Session["Name"] ?? "Valued Client" %></span>
            </h1>
            
            <p class="text-lg text-gray-600 max-w-2xl mx-auto">
                Manage your pest control services efficiently in one convenient location
            </p>
        </div>

        <!-- Key Statistics -->
        <div class="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-12">
            <div class="stat-card bg-white rounded-xl p-6 text-center shadow-sm border border-gray-100">
                <div class="text-3xl font-bold text-blue-600 mb-1">FREE</div>
                <div class="text-sm text-gray-600">Initial Inspection</div>
            </div>
            <div class="stat-card bg-white rounded-xl p-6 text-center shadow-sm border border-gray-100">
                <div class="text-3xl font-bold text-blue-600 mb-1">24/7</div>
                <div class="text-sm text-gray-600">Customer Support</div>
            </div>
            <div class="stat-card bg-white rounded-xl p-6 text-center shadow-sm border border-gray-100">
                <div class="text-3xl font-bold text-blue-600 mb-1">Licensed</div>
                <div class="text-sm text-gray-600">Professionals</div>
            </div>
            <div class="stat-card bg-white rounded-xl p-6 text-center shadow-sm border border-gray-100">
                <div class="text-3xl font-bold text-blue-600 mb-1">Eco-Safe</div>
                <div class="text-sm text-gray-600">Treatments</div>
            </div>
        </div>

        <!-- Services Section -->
        <div class="mb-12">
            <h2 class="text-2xl font-bold text-gray-900 mb-6">Quick Actions</h2>
            
            <!-- Mobile: Single column, Tablet: 2 columns, Desktop: 3 columns with last 2 centered -->
            <div class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-6 gap-6">
                
                <!-- Request Inspection -->
                <div class="feature-card bg-white rounded-xl p-6 shadow-sm border border-gray-200 hover:border-blue-300 xl:col-span-2">
                    <div class="flex items-start mb-4">
                        <div class="bg-blue-100 rounded-lg p-3 mr-4">
                            <i class="fas fa-calendar-check text-blue-600 text-2xl"></i>
                        </div>
                        <div class="flex-1">
                            <h3 class="text-lg font-semibold text-gray-900 mb-1">Request Inspection</h3>
                            <span class="inline-block bg-green-100 text-green-700 text-xs font-medium px-2 py-1 rounded">FREE</span>
                        </div>
                    </div>
                    <p class="text-gray-600 text-sm mb-4">
                        Schedule a complimentary property inspection by our certified experts
                    </p>
                    <a href="BookInspection.aspx" class="inline-flex items-center justify-center w-full bg-blue-600 text-white px-4 py-2.5 rounded-lg font-medium hover:bg-blue-700 transition">
                        Book Inspection
                        <i class="fas fa-arrow-right ml-2 text-sm"></i>
                    </a>
                </div>

                <!-- Track Inspections -->
                <div class="feature-card bg-white rounded-xl p-6 shadow-sm border border-gray-200 hover:border-blue-300 xl:col-span-2">
                    <div class="flex items-start mb-4">
                        <div class="bg-blue-100 rounded-lg p-3 mr-4">
                            <i class="fas fa-clipboard-list text-blue-600 text-2xl"></i>
                        </div>
                        <div class="flex-1">
                            <h3 class="text-lg font-semibold text-gray-900">Track Inspections</h3>
                        </div>
                    </div>
                    <p class="text-gray-600 text-sm mb-4">
                        Monitor inspection status, view reports, and review recommendations
                    </p>
                    <a href="MyInquiries.aspx" class="inline-flex items-center justify-center w-full bg-blue-600 text-white px-4 py-2.5 rounded-lg font-medium hover:bg-blue-700 transition">
                        View Inspections
                        <i class="fas fa-arrow-right ml-2 text-sm"></i>
                    </a>
                </div>

                <!-- Service Appointments -->
                <div class="feature-card bg-white rounded-xl p-6 shadow-sm border border-gray-200 hover:border-blue-300 xl:col-span-2">
                    <div class="flex items-start mb-4">
                        <div class="bg-blue-100 rounded-lg p-3 mr-4">
                            <i class="fas fa-briefcase text-blue-600 text-2xl"></i>
                        </div>
                        <div class="flex-1">
                            <h3 class="text-lg font-semibold text-gray-900">Service Appointments</h3>
                        </div>
                    </div>
                    <p class="text-gray-600 text-sm mb-4">
                        View scheduled treatments and service history
                    </p>
                    <a href="MyBookings.aspx" class="inline-flex items-center justify-center w-full bg-blue-600 text-white px-4 py-2.5 rounded-lg font-medium hover:bg-blue-700 transition">
                        View Appointments
                        <i class="fas fa-arrow-right ml-2 text-sm"></i>
                    </a>
                </div>

                <!-- Service Contracts -->
                <div class="feature-card bg-white rounded-xl p-6 shadow-sm border border-gray-200 hover:border-blue-300 md:col-start-1 xl:col-start-2 xl:col-span-2">
                    <div class="flex items-start mb-4">
                        <div class="bg-blue-100 rounded-lg p-3 mr-4">
                            <i class="fas fa-file-contract text-blue-600 text-2xl"></i>
                        </div>
                        <div class="flex-1">
                            <h3 class="text-lg font-semibold text-gray-900">Service Contracts</h3>
                        </div>
                    </div>
                    <p class="text-gray-600 text-sm mb-4">
                        Access and manage your active service agreements
                    </p>
                    <a href="OurContract.aspx" class="inline-flex items-center justify-center w-full bg-blue-600 text-white px-4 py-2.5 rounded-lg font-medium hover:bg-blue-700 transition">
                        View Contracts
                        <i class="fas fa-arrow-right ml-2 text-sm"></i>
                    </a>
                </div>

                <!-- Billing & Payments -->
                <div class="feature-card bg-white rounded-xl p-6 shadow-sm border border-gray-200 hover:border-blue-300 md:col-start-2 xl:col-span-2">
                    <div class="flex items-start mb-4">
                        <div class="bg-blue-100 rounded-lg p-3 mr-4">
                            <i class="fas fa-credit-card text-blue-600 text-2xl"></i>
                        </div>
                        <div class="flex-1">
                            <h3 class="text-lg font-semibold text-gray-900">Billing & Payments</h3>
                        </div>
                    </div>
                    <p class="text-gray-600 text-sm mb-4">
                        Review invoices and submit secure online payments
                    </p>
                    <a href="Payment.aspx" class="inline-flex items-center justify-center w-full bg-blue-600 text-white px-4 py-2.5 rounded-lg font-medium hover:bg-blue-700 transition">
                        Manage Payments
                        <i class="fas fa-arrow-right ml-2 text-sm"></i>
                    </a>
                </div>

            </div>
        </div>

        <!-- Process Overview -->
        <div class="bg-gray-50 rounded-xl p-8 mb-12">
            <h2 class="text-2xl font-bold text-gray-900 mb-6 text-center">Our Service Process</h2>
            
            <div class="grid grid-cols-1 md:grid-cols-3 gap-8">
                <div class="text-center">
                    <div class="bg-blue-600 text-white w-12 h-12 rounded-full flex items-center justify-center text-xl font-bold mx-auto mb-4">1</div>
                    <h3 class="font-semibold text-gray-900 mb-2">Schedule Inspection</h3>
                    <p class="text-sm text-gray-600">Book a free inspection at your convenience</p>
                </div>
                
                <div class="text-center">
                    <div class="bg-blue-600 text-white w-12 h-12 rounded-full flex items-center justify-center text-xl font-bold mx-auto mb-4">2</div>
                    <h3 class="font-semibold text-gray-900 mb-2">Expert Assessment</h3>
                    <p class="text-sm text-gray-600">Receive a detailed evaluation and treatment plan</p>
                </div>
                
                <div class="text-center">
                    <div class="bg-blue-600 text-white w-12 h-12 rounded-full flex items-center justify-center text-xl font-bold mx-auto mb-4">3</div>
                    <h3 class="font-semibold text-gray-900 mb-2">Professional Service</h3>
                    <p class="text-sm text-gray-600">Safe, effective treatment by licensed technicians</p>
                </div>
            </div>
        </div>

        <!-- Contact Support -->
        <div class="bg-gradient-to-r from-blue-600 to-blue-700 rounded-xl p-8 text-white text-center">
            <h2 class="text-2xl font-bold mb-3">Need Assistance?</h2>
            <p class="text-blue-100 mb-6">Our support team is available to help you</p>
            <div class="flex flex-col sm:flex-row gap-4 justify-center">
                <a href="tel:+639924357834" class="inline-flex items-center justify-center bg-white text-blue-600 px-6 py-3 rounded-lg font-medium hover:bg-blue-50 transition">
                    <i class="fas fa-phone mr-2"></i>
                    +63 992 435 7834
                </a>
                <a href="mailto:rrctermiteandpestcontrol@gmail.com" class="inline-flex items-center justify-center bg-blue-500 text-white px-6 py-3 rounded-lg font-medium hover:bg-blue-600 transition">
                    <i class="fas fa-envelope mr-2"></i>
                    Email Support
                </a>
            </div>
        </div>
    </div>
</asp:Content>