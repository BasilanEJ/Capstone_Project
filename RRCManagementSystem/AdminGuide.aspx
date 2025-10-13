<%@ Page Title="Administrator Guide"
    Language="C#"
    MasterPageFile="~/Admin.Master"
    AutoEventWireup="true"
    CodeBehind="AdminGuide.aspx.cs"
    Inherits="RRCManagementSystem.AdminGuide" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="max-w-7xl mx-auto px-4 py-8">
        <!-- Header -->
        <div class="mb-8">
            <h1 class="text-4xl font-bold text-gray-900 mb-2">Administrator Guide</h1>
            <p class="text-gray-600">
                RRC Termite & Pest Control Management System
            </p>
            <p class="text-sm text-gray-500 mt-1">
                Last updated: <asp:Label ID="lblUpdated" runat="server" />
            </p>
        </div>

        <!-- Search Box -->
        <div class="mb-8">
            <div class="relative max-w-md">
                <input 
                    id="searchBox" 
                    type="text" 
                    placeholder="Search modules..." 
                    class="w-full px-4 py-2 pl-10 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
                <i class="fas fa-search absolute left-3 top-1/2 -translate-y-1/2 text-gray-400"></i>
            </div>
        </div>

        <!-- Main Content Grid -->
        <div class="grid grid-cols-1 lg:grid-cols-4 gap-8">
            <!-- Sidebar Navigation -->
            <div class="lg:col-span-1">
                <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-4 sticky top-4">
                    <h3 class="text-sm font-semibold text-gray-900 uppercase mb-3">Quick Navigation</h3>
                    <nav class="space-y-1">
                        <a href="#dashboard" class="block px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 rounded">Dashboard</a>
                        <a href="#bookings" class="block px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 rounded">Bookings</a>
                        <a href="#clients" class="block px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 rounded">Clients</a>
                        <a href="#sales" class="block px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 rounded">Sales & Transactions</a>
                        <a href="#employees" class="block px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 rounded">Manage Employees</a>
                        <a href="#items" class="block px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 rounded">Manage Items</a>
                        <a href="#equipment" class="block px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 rounded">Manage Equipment</a>
                        <a href="#services" class="block px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 rounded">Manage Services</a>
                        <a href="#supplier" class="block px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 rounded">Manage Supplier</a>
                        <a href="#reports" class="block px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 rounded">Reports</a>
                        <a href="#inquiry" class="block px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 rounded">Manage Inquiry</a>
                        <a href="#create-customer" class="block px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 rounded">Create Customer Account</a>
                    </nav>
                </div>
            </div>

            <!-- Main Content -->
            <div class="lg:col-span-3 space-y-6">
                
                <!-- Dashboard Section -->
                <section id="dashboard" class="module-section bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                    <div class="flex items-center mb-4">
                        <i class="fas fa-chart-line text-blue-600 text-2xl mr-3"></i>
                        <h2 class="text-2xl font-bold text-gray-900">Dashboard</h2>
                    </div>
                    <p class="text-gray-700 mb-4">Get a comprehensive overview of your company's performance and upcoming activities at a glance.</p>
                    <div class="space-y-3">
                        <div class="flex items-start">
                            <i class="fas fa-circle text-blue-500 text-xs mt-1.5 mr-3"></i>
                            <div>
                                <span class="font-semibold text-gray-900">Total Clients:</span>
                                <span class="text-gray-700"> View the total number of registered customers</span>
                            </div>
                        </div>
                        <div class="flex items-start">
                            <i class="fas fa-circle text-blue-500 text-xs mt-1.5 mr-3"></i>
                            <div>
                                <span class="font-semibold text-gray-900">Total Employees:</span>
                                <span class="text-gray-700"> See the count of all active employees</span>
                            </div>
                        </div>
                        <div class="flex items-start">
                            <i class="fas fa-circle text-blue-500 text-xs mt-1.5 mr-3"></i>
                            <div>
                                <span class="font-semibold text-gray-900">Today's Sales:</span>
                                <span class="text-gray-700"> Monitor your daily sales total in real-time</span>
                            </div>
                        </div>
                        <div class="flex items-start">
                            <i class="fas fa-circle text-blue-500 text-xs mt-1.5 mr-3"></i>
                            <div>
                                <span class="font-semibold text-gray-900">This Month's Sales:</span>
                                <span class="text-gray-700"> View a summary of the current month's revenue</span>
                            </div>
                        </div>
                        <div class="flex items-start">
                            <i class="fas fa-circle text-blue-500 text-xs mt-1.5 mr-3"></i>
                            <div>
                                <span class="font-semibold text-gray-900">Weekly Booking Calendar:</span>
                                <span class="text-gray-700"> Check scheduled customer bookings for the current week</span>
                            </div>
                        </div>
                        <div class="flex items-start">
                            <i class="fas fa-circle text-blue-500 text-xs mt-1.5 mr-3"></i>
                            <div>
                                <span class="font-semibold text-gray-900">Sales Overview:</span>
                                <span class="text-gray-700"> Toggle between Daily, Weekly, Monthly, and Yearly views with Bar or Line graph options</span>
                            </div>
                        </div>
                        <div class="flex items-start">
                            <i class="fas fa-circle text-blue-500 text-xs mt-1.5 mr-3"></i>
                            <div>
                                <span class="font-semibold text-gray-900">Blockchain Sales Transparency:</span>
                                <span class="text-gray-700"> Verify blockchain-verified sales by clicking the Verify Blockchain button</span>
                            </div>
                        </div>
                    </div>
                </section>

                <!-- Bookings Section -->
                <section id="bookings" class="module-section bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                    <div class="flex items-center mb-4">
                        <i class="fas fa-calendar-check text-green-600 text-2xl mr-3"></i>
                        <h2 class="text-2xl font-bold text-gray-900">Bookings</h2>
                    </div>
                    <p class="text-gray-700 mb-4">Manage all customer booking requests and scheduling operations efficiently.</p>
                    <div class="space-y-3">
                        <div class="flex items-start">
                            <i class="fas fa-circle text-green-500 text-xs mt-1.5 mr-3"></i>
                            <div><span class="text-gray-700">View all bookings from customers</span></div>
                        </div>
                        <div class="flex items-start">
                            <i class="fas fa-circle text-green-500 text-xs mt-1.5 mr-3"></i>
                            <div><span class="text-gray-700">Approve or reject customer booking requests</span></div>
                        </div>
                        <div class="flex items-start">
                            <i class="fas fa-circle text-green-500 text-xs mt-1.5 mr-3"></i>
                            <div><span class="text-gray-700">Approve reschedule requests for contractual services</span></div>
                        </div>
                        <div class="flex items-start">
                            <i class="fas fa-circle text-green-500 text-xs mt-1.5 mr-3"></i>
                            <div><span class="text-gray-700">View all reschedule booking requests</span></div>
                        </div>
                        <div class="flex items-start">
                            <i class="fas fa-circle text-green-500 text-xs mt-1.5 mr-3"></i>
                            <div><span class="text-gray-700">Access complete booking history and records</span></div>
                        </div>
                    </div>
                </section>

                <!-- Clients Section -->
                <section id="clients" class="module-section bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                    <div class="flex items-center mb-4">
                        <i class="fas fa-users text-purple-600 text-2xl mr-3"></i>
                        <h2 class="text-2xl font-bold text-gray-900">Clients</h2>
                    </div>
                    <p class="text-gray-700 mb-4">Comprehensive client account management and document handling.</p>
                    <div class="space-y-3">
                        <div class="flex items-start">
                            <i class="fas fa-circle text-purple-500 text-xs mt-1.5 mr-3"></i>
                            <div><span class="text-gray-700">View registered clients' profiles and account details</span></div>
                        </div>
                        <div class="flex items-start">
                            <i class="fas fa-circle text-purple-500 text-xs mt-1.5 mr-3"></i>
                            <div><span class="text-gray-700">Resend password reset emails to client accounts</span></div>
                        </div>
                        <div class="flex items-start">
                            <i class="fas fa-circle text-purple-500 text-xs mt-1.5 mr-3"></i>
                            <div><span class="text-gray-700">Archive inactive or unused client accounts</span></div>
                        </div>
                        <div class="flex items-start">
                            <i class="fas fa-circle text-purple-500 text-xs mt-1.5 mr-3"></i>
                            <div><span class="text-gray-700">Restore or permanently delete client accounts</span></div>
                        </div>
                        <div class="flex items-start">
                            <i class="fas fa-circle text-purple-500 text-xs mt-1.5 mr-3"></i>
                            <div><span class="text-gray-700">Upload service contracts for customers availing RRC's services</span></div>
                        </div>
                    </div>
                </section>

                <!-- Sales and Transactions Section -->
                <section id="sales" class="module-section bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                    <div class="flex items-center mb-4">
                        <i class="fas fa-dollar-sign text-yellow-600 text-2xl mr-3"></i>
                        <h2 class="text-2xl font-bold text-gray-900">Sales & Transactions</h2>
                    </div>
                    <p class="text-gray-700 mb-4">Track and manage all financial transactions and payment records.</p>
                    <div class="space-y-3">
                        <div class="flex items-start">
                            <i class="fas fa-circle text-yellow-500 text-xs mt-1.5 mr-3"></i>
                            <div><span class="text-gray-700">View detailed transaction history including payment methods, amounts, and receipts</span></div>
                        </div>
                        <div class="flex items-start">
                            <i class="fas fa-circle text-yellow-500 text-xs mt-1.5 mr-3"></i>
                            <div><span class="text-gray-700">Manually adjust client payments when necessary</span></div>
                        </div>
                        <div class="flex items-start">
                            <i class="fas fa-circle text-yellow-500 text-xs mt-1.5 mr-3"></i>
                            <div><span class="text-gray-700">Monitor payment balances for all client accounts</span></div>
                        </div>
                    </div>
                </section>

                <!-- Manage Employees Section -->
                <section id="employees" class="module-section bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                    <div class="flex items-center mb-4">
                        <i class="fas fa-user-tie text-indigo-600 text-2xl mr-3"></i>
                        <h2 class="text-2xl font-bold text-gray-900">Manage Employees</h2>
                    </div>
                    <p class="text-gray-700 mb-4">Complete employee management system for your workforce.</p>
                    <div class="space-y-3">
                        <div class="flex items-start">
                            <i class="fas fa-circle text-indigo-500 text-xs mt-1.5 mr-3"></i>
                            <div><span class="text-gray-700">View, add, archive, restore, and delete employee records</span></div>
                        </div>
                        <div class="flex items-start">
                            <i class="fas fa-circle text-indigo-500 text-xs mt-1.5 mr-3"></i>
                            <div><span class="text-gray-700">Monitor employment status of all employees</span></div>
                        </div>
                        <div class="flex items-start">
                            <i class="fas fa-circle text-indigo-500 text-xs mt-1.5 mr-3"></i>
                            <div><span class="text-gray-700">Assign employees to service teams and view team composition details</span></div>
                        </div>
                    </div>
                </section>

                <!-- Manage Items Section -->
                <section id="items" class="module-section bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                    <div class="flex items-center mb-4">
                        <i class="fas fa-boxes text-orange-600 text-2xl mr-3"></i>
                        <h2 class="text-2xl font-bold text-gray-900">Manage Items</h2>
                    </div>
                    <p class="text-gray-700 mb-4">Track and manage your inventory of chemicals, equipment, and supplies.</p>
                    <div class="space-y-3">
                        <div class="flex items-start">
                            <i class="fas fa-circle text-orange-500 text-xs mt-1.5 mr-3"></i>
                            <div><span class="text-gray-700">Add and view items including chemicals, sacheted chemicals, and safety gear</span></div>
                        </div>
                        <div class="flex items-start">
                            <i class="fas fa-circle text-orange-500 text-xs mt-1.5 mr-3"></i>
                            <div><span class="text-gray-700">Monitor Daily Total Stocks for detailed inventory updates on a daily basis</span></div>
                        </div>
                    </div>
                </section>

                <!-- Manage Equipment Section -->
                <section id="equipment" class="module-section bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                    <div class="flex items-center mb-4">
                        <i class="fas fa-tools text-red-600 text-2xl mr-3"></i>
                        <h2 class="text-2xl font-bold text-gray-900">Manage Equipment</h2>
                    </div>
                    <p class="text-gray-700 mb-4">Organize and track all service equipment used by your teams.</p>
                    <div class="space-y-3">
                        <div class="flex items-start">
                            <i class="fas fa-circle text-red-500 text-xs mt-1.5 mr-3"></i>
                            <div><span class="text-gray-700">Add and view service equipment for company use (sprayers, extension cords, wood injectors, etc.)</span></div>
                        </div>
                        <div class="flex items-start">
                            <i class="fas fa-circle text-red-500 text-xs mt-1.5 mr-3"></i>
                            <div><span class="text-gray-700">Check equipment availability for scheduled service days</span></div>
                        </div>
                    </div>
                </section>

                <!-- Manage Services Section -->
                <section id="services" class="module-section bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                    <div class="flex items-center mb-4">
                        <i class="fas fa-clipboard-list text-teal-600 text-2xl mr-3"></i>
                        <h2 class="text-2xl font-bold text-gray-900">Manage Services</h2>
                    </div>
                    <p class="text-gray-700 mb-4">Configure and maintain your service offerings and pricing structure.</p>
                    <div class="space-y-3">
                        <div class="flex items-start">
                            <i class="fas fa-circle text-teal-500 text-xs mt-1.5 mr-3"></i>
                            <div><span class="text-gray-700">Add and view services offered with respective pricing per SQM range</span></div>
                        </div>
                        <div class="flex items-start">
                            <i class="fas fa-circle text-teal-500 text-xs mt-1.5 mr-3"></i>
                            <div><span class="text-gray-700">Edit or delete service pricing per SQM range</span></div>
                        </div>
                        <div class="flex items-start">
                            <i class="fas fa-circle text-teal-500 text-xs mt-1.5 mr-3"></i>
                            <div><span class="text-gray-700">Manage services as a whole including adding or removing service types</span></div>
                        </div>
                    </div>
                </section>

                <!-- Manage Supplier Section -->
                <section id="supplier" class="module-section bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                    <div class="flex items-center mb-4">
                        <i class="fas fa-truck text-cyan-600 text-2xl mr-3"></i>
                        <h2 class="text-2xl font-bold text-gray-900">Manage Supplier</h2>
                    </div>
                    <p class="text-gray-700 mb-4">Maintain relationships and records for all your business suppliers.</p>
                    <div class="space-y-3">
                        <div class="flex items-start">
                            <i class="fas fa-circle text-cyan-500 text-xs mt-1.5 mr-3"></i>
                            <div><span class="text-gray-700">Add and view supplier information and records</span></div>
                        </div>
                        <div class="flex items-start">
                            <i class="fas fa-circle text-cyan-500 text-xs mt-1.5 mr-3"></i>
                            <div><span class="text-gray-700">Send emails directly to suppliers through the system</span></div>
                        </div>
                        <div class="flex items-start">
                            <i class="fas fa-circle text-cyan-500 text-xs mt-1.5 mr-3"></i>
                            <div><span class="text-gray-700">Edit supplier details and archive suppliers when needed</span></div>
                        </div>
                    </div>
                </section>

                <!-- Reports Section -->
                <section id="reports" class="module-section bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                    <div class="flex items-center mb-4">
                        <i class="fas fa-file-alt text-pink-600 text-2xl mr-3"></i>
                        <h2 class="text-2xl font-bold text-gray-900">Business Admin Reports</h2>
                    </div>
                    <p class="text-gray-700 mb-4">Generate comprehensive PDF reports for various aspects of your business operations, filtered by specific date ranges.</p>
                    <div class="bg-gray-50 rounded-lg p-4 mt-4">
                        <h3 class="font-semibold text-gray-900 mb-3">Available Reports:</h3>
                        <div class="grid grid-cols-1 md:grid-cols-2 gap-2">
                            <div class="flex items-center">
                                <i class="fas fa-file-pdf text-red-500 mr-2"></i>
                                <span class="text-sm text-gray-700">User Accounts (excluding Admins)</span>
                            </div>
                            <div class="flex items-center">
                                <i class="fas fa-file-pdf text-red-500 mr-2"></i>
                                <span class="text-sm text-gray-700">Inquiries</span>
                            </div>
                            <div class="flex items-center">
                                <i class="fas fa-file-pdf text-red-500 mr-2"></i>
                                <span class="text-sm text-gray-700">Approved Clients</span>
                            </div>
                            <div class="flex items-center">
                                <i class="fas fa-file-pdf text-red-500 mr-2"></i>
                                <span class="text-sm text-gray-700">Inventory Details</span>
                            </div>
                            <div class="flex items-center">
                                <i class="fas fa-file-pdf text-red-500 mr-2"></i>
                                <span class="text-sm text-gray-700">Total Stocks Snapshot (Daily)</span>
                            </div>
                            <div class="flex items-center">
                                <i class="fas fa-file-pdf text-red-500 mr-2"></i>
                                <span class="text-sm text-gray-700">Equipment Status</span>
                            </div>
                            <div class="flex items-center">
                                <i class="fas fa-file-pdf text-red-500 mr-2"></i>
                                <span class="text-sm text-gray-700">Sales</span>
                            </div>
                            <div class="flex items-center">
                                <i class="fas fa-file-pdf text-red-500 mr-2"></i>
                                <span class="text-sm text-gray-700">Booking Details</span>
                            </div>
                            <div class="flex items-center">
                                <i class="fas fa-file-pdf text-red-500 mr-2"></i>
                                <span class="text-sm text-gray-700">Inspection Details</span>
                            </div>
                            <div class="flex items-center">
                                <i class="fas fa-file-pdf text-red-500 mr-2"></i>
                                <span class="text-sm text-gray-700">Inquiry Estimations</span>
                            </div>
                            <div class="flex items-center">
                                <i class="fas fa-file-pdf text-red-500 mr-2"></i>
                                <span class="text-sm text-gray-700">Team Summary (by date/range)</span>
                            </div>
                            <div class="flex items-center">
                                <i class="fas fa-file-pdf text-red-500 mr-2"></i>
                                <span class="text-sm text-gray-700">Team Members (per roster)</span>
                            </div>
                        </div>
                    </div>
                </section>

                <!-- Manage Inquiry Section -->
                <section id="inquiry" class="module-section bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                    <div class="flex items-center mb-4">
                        <i class="fas fa-question-circle text-blue-600 text-2xl mr-3"></i>
                        <h2 class="text-2xl font-bold text-gray-900">Manage Inquiry</h2>
                    </div>
                    <p class="text-gray-700 mb-4">Handle customer inquiries from initial contact through inspection to account creation.</p>
                    <div class="space-y-3">
                        <div class="flex items-start">
                            <i class="fas fa-circle text-blue-500 text-xs mt-1.5 mr-3"></i>
                            <div><span class="text-gray-700">Review inquiries submitted through the Inquiry page</span></div>
                        </div>
                        <div class="flex items-start">
                            <i class="fas fa-circle text-blue-500 text-xs mt-1.5 mr-3"></i>
                            <div><span class="text-gray-700">Assign Inspectors to visit potential client properties</span></div>
                        </div>
                        <div class="flex items-start">
                            <i class="fas fa-circle text-blue-500 text-xs mt-1.5 mr-3"></i>
                            <div><span class="text-gray-700">Create inquiries for walk-in potential customers</span></div>
                        </div>
                        <div class="flex items-start">
                            <i class="fas fa-circle text-blue-500 text-xs mt-1.5 mr-3"></i>
                            <div><span class="text-gray-700">View professional quotations prepared by Inspectors</span></div>
                        </div>
                        <div class="flex items-start">
                            <i class="fas fa-circle text-blue-500 text-xs mt-1.5 mr-3"></i>
                            <div><span class="text-gray-700">View inspected inquiries marked as done by Inspectors with option to automatically create client accounts</span></div>
                        </div>
                        <div class="flex items-start">
                            <i class="fas fa-circle text-blue-500 text-xs mt-1.5 mr-3"></i>
                            <div><span class="text-gray-700">View archived inquiries and restore or permanently delete them</span></div>
                        </div>
                    </div>
                </section>

                <!-- Create Customer Account Section -->
                <section id="create-customer" class="module-section bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                    <div class="flex items-center mb-4">
                        <i class="fas fa-user-plus text-green-600 text-2xl mr-3"></i>
                        <h2 class="text-2xl font-bold text-gray-900">Create Customer Account</h2>
                    </div>
                    <p class="text-gray-700 mb-4">Convert inspected properties into active customer accounts.</p>
                    <div class="space-y-3">
                        <div class="flex items-start">
                            <i class="fas fa-circle text-green-500 text-xs mt-1.5 mr-3"></i>
                            <div><span class="text-gray-700">Create customer accounts after successful property inspections have been completed</span></div>
                        </div>
                    </div>
                </section>

                <!-- Best Practices -->
                <section class="bg-blue-50 border border-blue-200 rounded-lg p-6 mt-8">
                    <h3 class="text-lg font-bold text-blue-900 mb-3 flex items-center">
                        <i class="fas fa-lightbulb mr-2"></i>
                        Best Practices & Tips
                    </h3>
                    <ul class="space-y-2 text-gray-700">
                        <li class="flex items-start">
                            <i class="fas fa-check text-blue-600 mt-1 mr-2"></i>
                            <span>Always log out of your admin account after each session to maintain security</span>
                        </li>
                        <li class="flex items-start">
                            <i class="fas fa-check text-blue-600 mt-1 mr-2"></i>
                            <span>Keep all records updated regularly for accurate reporting and operations</span>
                        </li>
                        <li class="flex items-start">
                            <i class="fas fa-check text-blue-600 mt-1 mr-2"></i>
                            <span>Use strong, unique passwords and change them periodically</span>
                        </li>
                        <li class="flex items-start">
                            <i class="fas fa-check text-blue-600 mt-1 mr-2"></i>
                            <span>Regularly back up sales, bookings, and inventory data</span>
                        </li>
                        <li class="flex items-start">
                            <i class="fas fa-check text-blue-600 mt-1 mr-2"></i>
                            <span>Retain blockchain logs for transparency and auditing purposes</span>
                        </li>
                    </ul>
                </section>


           <!-- Contact Information -->
<section class="bg-gray-50 border border-gray-200 rounded-lg p-6 mt-6">
    <h3 class="text-lg font-bold text-gray-900 mb-3 flex items-center">
        <i class="fas fa-envelope mr-2"></i>
        Need Help?
    </h3>
    <p class="text-gray-700 mb-4">
        For technical support or questions about the system, please contact:
    </p>
    <div class="space-y-2">
        <p class="text-gray-900 font-semibold flex items-center">
            <i class="fas fa-at text-blue-600 mr-2 w-5"></i>
            edgarjosephbasilan@gmail.com
        </p>
        <p class="text-gray-900 font-semibold flex items-center">
            <i class="fas fa-phone text-green-600 mr-2 w-5"></i>
            +63 992 435 7834
        </p>
       <p class="text-gray-900 font-semibold flex items-center">
    <i class="fab fa-facebook text-blue-500 mr-2 w-5"></i>
    <a href="https://www.facebook.com/eiji.delavin/" target="_blank">Eiji Delavin</a>
</p>

    </div>
</section>

</div>
</div>
</div>

<script type="text/javascript">
    document.addEventListener('DOMContentLoaded', function () {
        // Search functionality
        var searchBox = document.getElementById('searchBox');
        var sections = document.querySelectorAll('.module-section');

        searchBox.addEventListener('input', function () {
            var query = searchBox.value.toLowerCase().trim();

            sections.forEach(function (section) {
                var text = section.textContent.toLowerCase();
                if (query === '' || text.includes(query)) {
                    section.style.display = '';
                } else {
                    section.style.display = 'none';
                }
            });
        });

        // Smooth scroll for navigation links
        document.querySelectorAll('a[href^="#"]').forEach(function (anchor) {
            anchor.addEventListener('click', function (e) {
                e.preventDefault();
                var target = document.querySelector(this.getAttribute('href'));
                if (target) {
                    target.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            });
        });
    });
</script>
</asp:Content>