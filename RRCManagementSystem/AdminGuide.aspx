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
    <div class="max-w-6xl mx-auto px-4 py-6 md:py-12">
        <div class="text-center mb-8">
            <h1 class="text-3xl md:text-4xl font-extrabold text-gray-800 tracking-tight">Administrator Guide</h1>
            <p class="text-gray-500 text-sm md:text-base mt-2">
                RRC Termite &amp; Pest Control Management System &middot;
                Last updated: <asp:Label ID="lblUpdated" runat="server" />
            </p>
        </div>

        <div class="flex flex-col md:flex-row items-center md:space-x-3 space-y-3 md:space-y-0 mb-6">
            <div class="w-full relative">
                <input id="filterBox" type="text" placeholder="Search guide..." class="w-full p-2 pl-10 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition duration-200" />
                <i class="fas fa-search absolute left-3 top-1/2 -translate-y-1/2 text-gray-400"></i>
            </div>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div class="md:col-span-1">
                <div class="bg-white shadow-md rounded-xl p-6 border border-gray-200 sticky top-4">
                    <h3 class="text-xs font-bold text-gray-500 uppercase tracking-wider mb-3">Table of Contents</h3>
                    <ul class="list-disc list-inside space-y-2">
                        <li><a href="#top" class="text-blue-600 hover:underline">Introduction</a></li>
                        <li><a href="#dashboard" class="text-blue-600 hover:underline">Dashboard</a></li>
                        <li><a href="#manage-inquiry" class="text-blue-600 hover:underline">Manage Inquiry</a></li>
                        <li><a href="#create-customer" class="text-blue-600 hover:underline">Create Customer Account</a></li>
                        <li><a href="#manage-employees" class="text-blue-600 hover:underline">Manage Employees</a></li>
                        <li><a href="#manage-items" class="text-blue-600 hover:underline">Manage Items</a></li>
                        <li><a href="#manage-equipment" class="text-blue-600 hover:underline">Manage Equipment</a></li>
                        <li><a href="#clients" class="text-blue-600 hover:underline">Clients</a></li>
                        <li><a href="#bookings" class="text-blue-600 hover:underline">Bookings</a></li>
                        <li><a href="#sales" class="text-blue-600 hover:underline">Sales & Transactions</a></li>
                        <li><a href="#supplier" class="text-blue-600 hover:underline">Manage Supplier</a></li>
                        <li><a href="#services" class="text-blue-600 hover:underline">Manage Services</a></li>
                        <li><a href="#reports" class="text-blue-600 hover:underline">Reports</a></li>
                        <li><a href="#best" class="text-blue-600 hover:underline">Best Practices</a></li>
                        <li><a href="#contact" class="text-blue-600 hover:underline">Contact Information</a></li>
                    </ul>
                </div>
            </div>

            <div class="md:col-span-2">
                <div id="top" class="bg-white shadow-md rounded-xl p-6 border border-gray-200 mb-6">
                    <h2 class="text-xl font-bold text-gray-800 mb-2">1. Introduction</h2>
                    <p class="text-gray-600 leading-relaxed">
                        Welcome to the RRC Termite &amp; Pest Control Management Business Administrator Guide.
                        This manual explains how to use each module to manage clients, employees, equipment,
                        bookings, inventory, suppliers, sales, and reports.
                    </p>
                </div>

                <div class="mb-6">
                    <h2 class="text-lg font-bold text-gray-500 mb-4">2. Module Descriptions</h2>

                    <details id="dashboard" data-filter-item="dashboard overview weekly calendar sales blockchain graphs" class="bg-white border border-gray-200 rounded-xl mb-4 overflow-hidden">
                        <summary class="flex items-center justify-between p-4 font-semibold cursor-pointer select-none text-gray-800">
                            2.1 Dashboard
                            <span class="transform transition-transform duration-200 fas fa-chevron-right"></span>
                        </summary>
                        <div class="p-4 border-t border-gray-200 text-gray-600">
                            <p>The Dashboard provides a quick overview of company performance and upcoming activities. Features include:</p>
                            <ul class="list-disc list-inside pl-4 mt-2 space-y-1">
                                <li><b>Total Clients</b> – Displays the total registered customers.</li>
                                <li><b>Total Employees</b> – Shows active employees.</li>
                                <li><b>Today's Sales</b> – Displays daily total sales.</li>
                                <li><b>This Month's Sales</b> – Summarizes monthly sales.</li>
                                <li><b>Weekly Booking Calendar</b> – View booked customers for the current week.</li>
                                <li><b>Sales Overview</b> – Switch between Daily, Weekly, Monthly, and Yearly views; choose Bar or Line graph formats.</li>
                                <li><b>Blockchain Sales Transparency</b> – Select a date range to view blockchain-verified sales, including Log ID, Transaction ID, Sale Hash, and Timestamp.</li>
                            </ul>
                        </div>
                    </details>
                    
                    <details id="manage-inquiry" data-filter-item="manage inquiry inspectors assignments" class="bg-white border border-gray-200 rounded-xl mb-4 overflow-hidden">
                        <summary class="flex items-center justify-between p-4 font-semibold cursor-pointer select-none text-gray-800">
                            2.2 Manage Inquiry
                            <span class="transform transition-transform duration-200 fas fa-chevron-right"></span>
                        </summary>
                        <div class="p-4 border-t border-gray-200 text-gray-600">
                            <ul class="list-disc list-inside pl-4 space-y-1">
                                <li>Review inquiries submitted via the Inquiry page.</li>
                                <li>Assign Inspectors to potential client properties.</li>
                            </ul>
                        </div>
                    </details>
                    
                    <details id="create-customer" data-filter-item="create customer account accounts after inspection" class="bg-white border border-gray-200 rounded-xl mb-4 overflow-hidden">
                        <summary class="flex items-center justify-between p-4 font-semibold cursor-pointer select-none text-gray-800">
                            2.3 Create Customer Account
                            <span class="transform transition-transform duration-200 fas fa-chevron-right"></span>
                        </summary>
                        <div class="p-4 border-t border-gray-200 text-gray-600">
                            <ul class="list-disc list-inside pl-4 space-y-1">
                                <li>Create accounts for customers after a successful property inspection.</li>
                            </ul>
                        </div>
                    </details>

                    <details id="manage-employees" data-filter-item="manage employees add archive teams team details" class="bg-white border border-gray-200 rounded-xl mb-4 overflow-hidden">
                        <summary class="flex items-center justify-between p-4 font-semibold cursor-pointer select-none text-gray-800">
                            2.4 Manage Employees
                            <span class="transform transition-transform duration-200 fas fa-chevron-right"></span>
                        </summary>
                        <div class="p-4 border-t border-gray-200 text-gray-600">
                            <ul class="list-disc list-inside pl-4 space-y-1">
                                <li>View, add, and archive employees.</li>
                                <li>Assign employees to service teams and view team details.</li>
                            </ul>
                        </div>
                    </details>
                    
                    <details id="manage-items" data-filter-item="manage items chemicals sachet safety gear daily total stocks inventory" class="bg-white border border-gray-200 rounded-xl mb-4 overflow-hidden">
                        <summary class="flex items-center justify-between p-4 font-semibold cursor-pointer select-none text-gray-800">
                            2.5 Manage Items
                            <span class="transform transition-transform duration-200 fas fa-chevron-right"></span>
                        </summary>
                        <div class="p-4 border-t border-gray-200 text-gray-600">
                            <ul class="list-disc list-inside pl-4 space-y-1">
                                <li>Add and view items such as chemicals, sacheted chemicals, and safety gear.</li>
                                <li>Monitor <b>Daily Total Stocks</b> for detailed daily inventory updates.</li>
                            </ul>
                        </div>
                    </details>
                    
                    <details id="manage-equipment" data-filter-item="manage equipment sprayers cords wood injectors availability booked days" class="bg-white border border-gray-200 rounded-xl mb-4 overflow-hidden">
                        <summary class="flex items-center justify-between p-4 font-semibold cursor-pointer select-none text-gray-800">
                            2.6 Manage Equipment
                            <span class="transform transition-transform duration-200 fas fa-chevron-right"></span>
                        </summary>
                        <div class="p-4 border-t border-gray-200 text-gray-600">
                            <ul class="list-disc list-inside pl-4 space-y-1">
                                <li>Add and view service equipment (e.g., sprayers, extension cords, wood injectors).</li>
                                <li>Check equipment availability for booked service days.</li>
                            </ul>
                        </div>
                    </details>
                    
                    <details id="clients" data-filter-item="clients register archive contracts upload" class="bg-white border border-gray-200 rounded-xl mb-4 overflow-hidden">
                        <summary class="flex items-center justify-between p-4 font-semibold cursor-pointer select-none text-gray-800">
                            2.7 Clients
                            <span class="transform transition-transform duration-200 fas fa-chevron-right"></span>
                        </summary>
                        <div class="p-4 border-t border-gray-200 text-gray-600">
                            <ul class="list-disc list-inside pl-4 space-y-1">
                                <li>View all registered clients.</li>
                                <li>Archive inactive or non-using accounts.</li>
                                <li>Upload contracts for customers availing RRC’s services.</li>
                            </ul>
                        </div>
                    </details>
                    
                    <details id="bookings" data-filter-item="bookings approve reschedule history contractual" class="bg-white border border-gray-200 rounded-xl mb-4 overflow-hidden">
                        <summary class="flex items-center justify-between p-4 font-semibold cursor-pointer select-none text-gray-800">
                            2.8 Bookings
                            <span class="transform transition-transform duration-200 fas fa-chevron-right"></span>
                        </summary>
                        <div class="p-4 border-t border-gray-200 text-gray-600">
                            <ul class="list-disc list-inside pl-4 space-y-1">
                                <li>View and approve customer bookings.</li>
                                <li>Approve reschedule requests for contractual services.</li>
                                <li>View booking history.</li>
                            </ul>
                        </div>
                    </details>
                    
                    <details id="sales" data-filter-item="sales transactions history payment methods amounts manual adjust" class="bg-white border border-gray-200 rounded-xl mb-4 overflow-hidden">
                        <summary class="flex items-center justify-between p-4 font-semibold cursor-pointer select-none text-gray-800">
                            2.9 Sales &amp; Transactions
                            <span class="transform transition-transform duration-200 fas fa-chevron-right"></span>
                        </summary>
                        <div class="p-4 border-t border-gray-200 text-gray-600">
                            <ul class="list-disc list-inside pl-4 space-y-1">
                                <li>View transaction history with payment methods and amounts.</li>
                                <li>Manually adjust client payments if needed.</li>
                            </ul>
                        </div>
                    </details>
                    
                    <details id="supplier" data-filter-item="supplier add view edit archive" class="bg-white border border-gray-200 rounded-xl mb-4 overflow-hidden">
                        <summary class="flex items-center justify-between p-4 font-semibold cursor-pointer select-none text-gray-800">
                            2.10 Manage Supplier
                            <span class="transform transition-transform duration-200 fas fa-chevron-right"></span>
                        </summary>
                        <div class="p-4 border-t border-gray-200 text-gray-600">
                            <ul class="list-disc list-inside pl-4 space-y-1">
                                <li>Add and view supplier records.</li>
                                <li>Edit supplier details and archive as needed.</li>
                            </ul>
                        </div>
                    </details>
                    
                    <details id="services" data-filter-item="services add view edit delete" class="bg-white border border-gray-200 rounded-xl mb-4 overflow-hidden">
                        <summary class="flex items-center justify-between p-4 font-semibold cursor-pointer select-none text-gray-800">
                            2.11 Manage Services
                            <span class="transform transition-transform duration-200 fas fa-chevron-right"></span>
                        </summary>
                        <div class="p-4 border-t border-gray-200 text-gray-600">
                            <ul class="list-disc list-inside pl-4 space-y-1">
                                <li>Add and view services offered.</li>
                                <li>Edit or delete services.</li>
                            </ul>
                        </div>
                    </details>
                    
                    <details id="reports" data-filter-item="reports pdf totals inquiries clients equipment bookings user accounts inventory sales inspection team" class="bg-white border border-gray-200 rounded-xl mb-4 overflow-hidden">
                        <summary class="flex items-center justify-between p-4 font-semibold cursor-pointer select-none text-gray-800">
                            2.12 Reports
                            <span class="transform transition-transform duration-200 fas fa-chevron-right"></span>
                        </summary>
                        <div class="p-4 border-t border-gray-200 text-gray-600">
                            <p>Generate PDF reports filtered by specific dates. Available reports include:</p>
                            <ul class="list-disc list-inside pl-4 mt-2 space-y-1">
                                <li>User Accounts (excluding admins)</li>
                                <li>Inquiries</li>
                                <li>Approved Clients</li>
                                <li>Total Stocks Snapshot (Daily)</li>
                                <li>Inventory Details</li>
                                <li>Sales</li>
                                <li>Equipment Status</li>
                                <li>Booking Details</li>
                                <li>Inspection Details</li>
                                <li>Team Reports (by date/range)</li>
                                <li>Team Members (per roster)</li>
                            </ul>
                        </div>
                    </details>
                </div>

                <hr class="border-t border-gray-300 my-8" />

                <div id="best" class="bg-white shadow-md rounded-xl p-6 border border-gray-200 mb-6">
                    <h2 class="text-xl font-bold text-gray-800 mb-2">3. Best Practices &amp; Security Notes</h2>
                    <ul class="list-disc list-inside pl-4 text-gray-600 space-y-1">
                        <li>Always log out after each session.</li>
                        <li>Keep all records updated regularly.</li>
                        <li>Use strong, unique passwords.</li>
                        <li>Back up sales, bookings, and inventory data.</li>
                        <li>Retain blockchain logs for transparency and auditing.</li>
                    </ul>
                </div>

                <div id="contact" class="bg-white shadow-md rounded-xl p-6 border border-gray-200">
                    <h2 class="text-xl font-bold text-gray-800 mb-2">4. Contact Information</h2>
                    <p class="text-gray-500 text-sm">
                        For support or question: <b>edgarjosephbasilan@gmail.com</b>
                    </p>
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        // Script to handle the accordion chevron rotation and content filtering
        document.addEventListener('DOMContentLoaded', function () {
            var detailsElements = document.querySelectorAll('details');
            detailsElements.forEach(function (details) {
                details.addEventListener('toggle', function () {
                    var chevron = details.querySelector('.fas.fa-chevron-right');
                    if (details.open) {
                        chevron.style.transform = 'rotate(90deg)';
                    } else {
                        chevron.style.transform = 'rotate(0deg)';
                    }
                });
            });

            // Search/filter functionality
            var input = document.getElementById('filterBox');
            var groups = Array.from(document.querySelectorAll('[data-filter-item]'));
            function normalize(s) { return (s || '').toLowerCase().trim(); }
            function applyFilter() {
                var q = normalize(input.value);
                groups.forEach(function (el) {
                    var hay = normalize(el.getAttribute('data-filter-item'));
                    if (!q || hay.includes(q)) {
                        el.style.display = '';
                        if (q) {
                            el.setAttribute('open', 'open');
                        }
                    } else {
                        el.style.display = 'none';
                        el.removeAttribute('open');
                    }
                });
            }
            input.addEventListener('input', applyFilter);
        });
    </script>
</asp:Content>

