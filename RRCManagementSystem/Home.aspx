<%@ Page Title="Home" Language="C#" MasterPageFile="~/Client.master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="RRCManagementSystem.Home" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        /* Card hover effects */
        .feature-card {
            transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
            backdrop-filter: blur(10px);
        }

        .feature-card:hover {
            transform: translateY(-12px) scale(1.02);
        }

        .feature-card::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            border-radius: 1.5rem;
            padding: 2px;
            background: linear-gradient(135deg, #3b82f6, #1d4ed8);
            -webkit-mask: linear-gradient(#fff 0 0) content-box, linear-gradient(#fff 0 0);
            -webkit-mask-composite: xor;
            mask-composite: exclude;
            opacity: 0;
            transition: opacity 0.4s;
        }

        .feature-card:hover::before {
            opacity: 1;
        }

        /* Icon animation */
        .icon-bounce {
            animation: gentle-bounce 2s infinite;
        }

        @keyframes gentle-bounce {
            0%, 100% { transform: translateY(0); }
            50% { transform: translateY(-10px); }
        }

        /* Gradient text - Blue theme only */
        .gradient-text {
            background: linear-gradient(135deg, #3b82f6, #1e40af, #2563eb);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
        }

        /* Glass morphism */
        .glass {
            background: rgba(255, 255, 255, 0.7);
            backdrop-filter: blur(20px);
            -webkit-backdrop-filter: blur(20px);
            border: 1px solid rgba(255, 255, 255, 0.3);
        }

        /* Smooth scroll */
        html {
            scroll-behavior: smooth;
        }

        /* Pulse ring */
        .pulse-ring {
            animation: pulse-ring 2s cubic-bezier(0.4, 0, 0.6, 1) infinite;
        }

        @keyframes pulse-ring {
            0%, 100% {
                opacity: 1;
                transform: scale(1);
            }
            50% {
                opacity: 0.5;
                transform: scale(1.1);
            }
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <!-- Hero Section -->
    <div class="relative mb-12 overflow-hidden">
        <div class="relative py-12 sm:py-16 lg:py-20 px-4 sm:px-6 lg:px-8">
            <div class="relative max-w-7xl mx-auto text-center">
                <div class="mb-6 inline-block">
                    <div class="relative">
                        <div class="relative bg-gradient-to-r from-blue-50 to-blue-100 rounded-full px-6 py-2 text-sm font-medium border border-blue-200">
                            <i class="fas fa-sparkles mr-2 text-blue-600"></i>
                            <span class="text-gray-700">Welcome Back</span>
                        </div>
                    </div>
                </div>
                
                <h1 class="text-4xl sm:text-5xl lg:text-6xl xl:text-7xl font-black mb-6 leading-tight">
                    <span class="gradient-text">Hello, <%= Session["Name"] ?? "Valued Client" %></span>
                    <div class="inline-block animate-pulse">👋</div>
                </h1>
                
              <p class="text-lg sm:text-xl lg:text-2xl mb-8 max-w-3xl mx-auto font-bold text-gray-600 leading-relaxed"> Your all-in-one pest control management hub. Book services, track progress, and manage payments effortlessly. </p>
            </div>
        </div>
    </div>

    <!-- Quick Stats Section -->
    <div class="max-w-7xl mx-auto mb-16 px-4">
        <div class="grid grid-cols-2 lg:grid-cols-4 gap-4 sm:gap-6">
            <div class="glass rounded-2xl p-6 text-center hover:shadow-2xl transition transform hover:-translate-y-2">
                <div class="text-4xl sm:text-5xl font-black gradient-text mb-2">24/7</div>
                <div class="text-gray-600 text-sm sm:text-base font-medium">Support Available</div>
            </div>
            <div class="glass rounded-2xl p-6 text-center hover:shadow-2xl transition transform hover:-translate-y-2">
                <div class="text-4xl sm:text-5xl font-black gradient-text mb-2">100%</div>
                <div class="text-gray-600 text-sm sm:text-base font-medium">Satisfaction Rate</div>
            </div>
            <div class="glass rounded-2xl p-6 text-center hover:shadow-2xl transition transform hover:-translate-y-2">
                <div class="text-4xl sm:text-5xl font-black gradient-text mb-2">Fast</div>
                <div class="text-gray-600 text-sm sm:text-base font-medium">Service Delivery</div>
            </div>
            <div class="glass rounded-2xl p-6 text-center hover:shadow-2xl transition transform hover:-translate-y-2">
                <div class="text-4xl sm:text-5xl font-black gradient-text mb-2">Safe</div>
                <div class="text-gray-600 text-sm sm:text-base font-medium">Eco-Friendly</div>
            </div>
        </div>
    </div>

    <!-- Main Features Section -->
    <div class="max-w-7xl mx-auto px-4 mb-16">
        <div class="text-center mb-12">
            <h2 class="text-3xl sm:text-4xl lg:text-5xl font-black mb-4">
                <span class="gradient-text">Everything You Need</span>
            </h2>
            <p class="text-gray-600 text-lg max-w-2xl mx-auto">
                Manage your pest control services with our comprehensive suite of tools
            </p>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 lg:gap-8">
            <!-- Book a Service -->
            <div class="feature-card relative glass rounded-3xl p-8 text-center group overflow-hidden">
                <div class="absolute top-0 right-0 w-32 h-32 bg-gradient-to-br from-blue-400/20 to-blue-500/20 rounded-full blur-2xl -mr-16 -mt-16"></div>
                
                <div class="relative mb-6">
                    <div class="absolute inset-0 bg-gradient-to-br from-blue-500 to-blue-700 rounded-3xl opacity-0 group-hover:opacity-20 blur-xl transition-opacity duration-500"></div>
                    <div class="relative bg-gradient-to-br from-blue-600 to-blue-700 w-20 h-20 mx-auto rounded-3xl flex items-center justify-center shadow-xl group-hover:shadow-2xl transition transform group-hover:rotate-6 group-hover:scale-110">
                        <i class="fas fa-calendar-check text-white text-3xl icon-bounce"></i>
                    </div>
                </div>
                
                <h3 class="text-xl font-bold mb-3 text-gray-800 group-hover:text-blue-700 transition">Book a Service</h3>
                <p class="text-gray-600 mb-6 text-sm leading-relaxed">
                    Schedule your pest control service in minutes with our streamlined booking system
                </p>
                
                <a href="BookService.aspx" class="inline-flex items-center justify-center gap-2 bg-gradient-to-r from-blue-600 to-blue-700 text-white px-6 py-3 rounded-xl font-semibold shadow-lg hover:shadow-xl transition transform hover:scale-105 w-full">
                    Book Now
                    <i class="fas fa-arrow-right group-hover:translate-x-1 transition-transform"></i>
                </a>
            </div>

            <!-- My Bookings -->
            <div class="feature-card relative glass rounded-3xl p-8 text-center group overflow-hidden">
                <div class="absolute top-0 right-0 w-32 h-32 bg-gradient-to-br from-gray-400/20 to-gray-500/20 rounded-full blur-2xl -mr-16 -mt-16"></div>
                
                <div class="relative mb-6">
                    <div class="absolute inset-0 bg-gradient-to-br from-gray-600 to-gray-800 rounded-3xl opacity-0 group-hover:opacity-20 blur-xl transition-opacity duration-500"></div>
                    <div class="relative bg-gradient-to-br from-gray-600 to-gray-800 w-20 h-20 mx-auto rounded-3xl flex items-center justify-center shadow-xl group-hover:shadow-2xl transition transform group-hover:rotate-6 group-hover:scale-110">
                        <i class="fas fa-clipboard-list text-white text-3xl icon-bounce"></i>
                    </div>
                </div>
                
                <h3 class="text-xl font-bold mb-3 text-gray-800 group-hover:text-gray-700 transition">My Bookings</h3>
                <p class="text-gray-600 mb-6 text-sm leading-relaxed">
                    Track all your service appointments and view booking history in one place
                </p>
                
                <a href="MyBookings.aspx" class="inline-flex items-center justify-center gap-2 bg-gradient-to-r from-gray-600 to-gray-800 text-white px-6 py-3 rounded-xl font-semibold shadow-lg hover:shadow-xl transition transform hover:scale-105 w-full">
                    View Bookings
                    <i class="fas fa-arrow-right group-hover:translate-x-1 transition-transform"></i>
                </a>
            </div>

            <!-- Our Contract -->
            <div class="feature-card relative glass rounded-3xl p-8 text-center group overflow-hidden">
                <div class="absolute top-0 right-0 w-32 h-32 bg-gradient-to-br from-blue-400/20 to-blue-500/20 rounded-full blur-2xl -mr-16 -mt-16"></div>
                
                <div class="relative mb-6">
                    <div class="absolute inset-0 bg-gradient-to-br from-blue-500 to-blue-700 rounded-3xl opacity-0 group-hover:opacity-20 blur-xl transition-opacity duration-500"></div>
                    <div class="relative bg-gradient-to-br from-blue-600 to-blue-700 w-20 h-20 mx-auto rounded-3xl flex items-center justify-center shadow-xl group-hover:shadow-2xl transition transform group-hover:rotate-6 group-hover:scale-110">
                        <i class="fas fa-file-contract text-white text-3xl icon-bounce"></i>
                    </div>
                </div>
                
                <h3 class="text-xl font-bold mb-3 text-gray-800 group-hover:text-blue-700 transition">Our Contract</h3>
                <p class="text-gray-600 mb-6 text-sm leading-relaxed">
                    Access your service agreement and review terms whenever you need
                </p>
                
                <a href="OurContract.aspx" class="inline-flex items-center justify-center gap-2 bg-gradient-to-r from-blue-600 to-blue-700 text-white px-6 py-3 rounded-xl font-semibold shadow-lg hover:shadow-xl transition transform hover:scale-105 w-full">
                    View Contract
                    <i class="fas fa-arrow-right group-hover:translate-x-1 transition-transform"></i>
                </a>
            </div>

            <!-- Pending Payments -->
            <div class="feature-card relative glass rounded-3xl p-8 text-center group overflow-hidden">
                <div class="absolute top-0 right-0 w-32 h-32 bg-gradient-to-br from-gray-400/20 to-gray-500/20 rounded-full blur-2xl -mr-16 -mt-16"></div>
                
                <div class="relative mb-6">
                    <div class="absolute inset-0 bg-gradient-to-br from-gray-600 to-gray-800 rounded-3xl opacity-0 group-hover:opacity-20 blur-xl transition-opacity duration-500"></div>
                    <div class="relative bg-gradient-to-br from-gray-600 to-gray-800 w-20 h-20 mx-auto rounded-3xl flex items-center justify-center shadow-xl group-hover:shadow-2xl transition transform group-hover:rotate-6 group-hover:scale-110">
                        <i class="fas fa-credit-card text-white text-3xl icon-bounce"></i>
                    </div>
                </div>
                
                <h3 class="text-xl font-bold mb-3 text-gray-800 group-hover:text-gray-700 transition">Pending Payments</h3>
                <p class="text-gray-600 mb-6 text-sm leading-relaxed">
                    Review outstanding balances and make secure online payments instantly
                </p>
                
                <a href="Payment.aspx" class="inline-flex items-center justify-center gap-2 bg-gradient-to-r from-gray-600 to-gray-800 text-white px-6 py-3 rounded-xl font-semibold shadow-lg hover:shadow-xl transition transform hover:scale-105 w-full">
                    Check Payments
                    <i class="fas fa-arrow-right group-hover:translate-x-1 transition-transform"></i>
                </a>
            </div>
        </div>
    </div>

    <!-- Why Choose Us Section -->
    <div class="max-w-7xl mx-auto px-4 mb-16">
        <div class="glass rounded-3xl p-8 sm:p-12 lg:p-16 overflow-hidden relative">
            <div class="absolute top-0 left-0 w-96 h-96 bg-gradient-to-br from-blue-300/20 to-blue-400/20 rounded-full blur-3xl -ml-48 -mt-48"></div>
            <div class="absolute bottom-0 right-0 w-96 h-96 bg-gradient-to-br from-gray-300/20 to-gray-400/20 rounded-full blur-3xl -mr-48 -mb-48"></div>
            
            <div class="relative">
                <h2 class="text-3xl sm:text-4xl font-black text-center mb-4">
                    <span class="gradient-text">Why Choose RRC?</span>
                </h2>
                <p class="text-center text-gray-600 mb-12 max-w-2xl mx-auto">
                    We're committed to providing exceptional service and peace of mind
                </p>
                
                <div class="grid grid-cols-1 md:grid-cols-3 gap-8">
                    <div class="text-center group">
                        <div class="bg-gradient-to-br from-blue-600 to-blue-700 w-16 h-16 mx-auto rounded-2xl flex items-center justify-center mb-4 shadow-lg group-hover:shadow-2xl transition transform group-hover:scale-110 group-hover:rotate-6">
                            <i class="fas fa-shield-alt text-white text-2xl"></i>
                        </div>
                        <h3 class="text-xl font-bold mb-2 text-gray-800">Licensed & Insured</h3>
                        <p class="text-gray-600 text-sm">Fully certified professionals you can trust</p>
                    </div>
                    
                    <div class="text-center group">
                        <div class="bg-gradient-to-br from-gray-600 to-gray-800 w-16 h-16 mx-auto rounded-2xl flex items-center justify-center mb-4 shadow-lg group-hover:shadow-2xl transition transform group-hover:scale-110 group-hover:rotate-6">
                            <i class="fas fa-leaf text-white text-2xl"></i>
                        </div>
                        <h3 class="text-xl font-bold mb-2 text-gray-800">Eco-Friendly</h3>
                        <p class="text-gray-600 text-sm">Safe for your family, pets, and environment</p>
                    </div>
                    
                    <div class="text-center group">
                        <div class="bg-gradient-to-br from-blue-600 to-blue-700 w-16 h-16 mx-auto rounded-2xl flex items-center justify-center mb-4 shadow-lg group-hover:shadow-2xl transition transform group-hover:scale-110 group-hover:rotate-6">
                            <i class="fas fa-clock text-white text-2xl"></i>
                        </div>
                        <h3 class="text-xl font-bold mb-2 text-gray-800">Quick Response</h3>
                        <p class="text-gray-600 text-sm">Fast scheduling and emergency services available</p>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- CTA Section -->
  <div class="max-w-5xl mx-auto px-4 mb-8">
    <div class="relative overflow-hidden rounded-3xl bg-gradient-to-r from-blue-600 to-blue-700 p-12 text-center text-white shadow-2xl">
        <div class="absolute inset-0 bg-black/10"></div>
        <div class="absolute top-0 left-0 w-64 h-64 bg-white/10 rounded-full blur-3xl -ml-32 -mt-32"></div>
        <div class="absolute bottom-0 right-0 w-64 h-64 bg-white/10 rounded-full blur-3xl -mr-32 -mb-32"></div>
        
        <div class="relative">
            <h2 class="text-3xl sm:text-4xl font-black mb-4">Need Support?</h2>
            <p class="text-lg mb-8 text-white/90 max-w-2xl mx-auto">
                Our customer support team is ready to assist you 24/7
            </p>
            <div class="flex flex-wrap gap-4 justify-center">
                
                <a href="tel:+639924357834" class="bg-white text-blue-600 px-8 py-4 rounded-full font-bold hover:shadow-2xl transition transform hover:scale-105 flex items-center gap-2">
                    <i class="fas fa-phone"></i>
                    Call Us
                </a>

                <a href="mailto:rrctermiteandpestcontrol@gmail.com" class="bg-white/20 backdrop-blur-md border-2 border-white/30 text-white px-8 py-4 rounded-full font-bold hover:bg-white/30 transition transform hover:scale-105 flex items-center gap-2">
                    <i class="fas fa-envelope"></i>
                    Email Us
                </a>
            </div>
        </div>
    </div>
</div>

    <!-- Accessibility -->
    <span class="sr-only">
        Navigate using the feature cards above to manage bookings, contracts, and payments. Contact support for assistance.
    </span>
</asp:Content>