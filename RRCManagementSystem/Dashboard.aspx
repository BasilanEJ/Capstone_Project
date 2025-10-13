<%@ Page Title="Dashboard"
    Language="C#"
    MasterPageFile="~/Admin.Master"
    AutoEventWireup="true"
    MaintainScrollPositionOnPostBack="true"
    CodeBehind="Dashboard.aspx.cs"
    Inherits="RRCManagementSystem.Dashboard" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        /* Custom styles for components */
        .page-title { 
            font-size: 1.75rem;
            line-height: 1.2;
        }
        @media (min-width: 640px) {
            .page-title { font-size: 2.25rem; }
        }
        
        .metric-card {
            transition: transform 0.3s ease-in-out, box-shadow 0.3s ease-in-out;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
        }
        @media (hover: hover) {
            .metric-card:hover { 
                transform: translateY(-5px); 
                box-shadow: 0 8px 20px rgba(0, 0, 0, 0.15); 
            }
        }
        
        .metric-value { 
            font-size: 1.875rem;
            font-weight: bold;
        }
        @media (min-width: 640px) {
            .metric-value { font-size: 2.5rem; }
        }
        
        /* Responsive Calendar Table */
        .calendar-wrapper {
            overflow-x: auto;
            -webkit-overflow-scrolling: touch;
        }
        .custom-calendar-table {
            min-width: 100%;
            width: max-content;
        }
        .custom-calendar-table td, .custom-calendar-table th {
            min-width: 100px;
            max-width: 100px;
            vertical-align: top;
            border: 1px solid #e5e7eb;
            padding: 0.375rem;
            height: auto;
            min-height: 100px;
            font-size: 0.875rem;
        }
        @media (min-width: 640px) {
            .custom-calendar-table td, .custom-calendar-table th {
                min-width: 120px;
                max-width: 120px;
                padding: 0.5rem;
                min-height: 120px;
            }
        }
        @media (min-width: 1024px) {
            .custom-calendar-table td, .custom-calendar-table th {
                min-width: 140px;
                max-width: 140px;
                height: 140px;
            }
        }
        @media (hover: hover) {
            .custom-calendar-table td:hover { background-color: #f3f4f6; }
        }

        /* Enhanced loading state styling */
        .loading-container {
            position: relative;
            min-height: 150px;
        }
        .loading-overlay {
            display: flex;
            justify-content: center;
            align-items: center;
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background-color: rgba(255, 255, 255, 0.8);
            z-index: 10;
            transition: opacity 0.3s ease;
            opacity: 0;
            pointer-events: none;
        }
        .loading-overlay.visible {
            opacity: 1;
            pointer-events: auto;
        }
        .spinner {
            border: 4px solid #f3f3f3;
            border-top: 4px solid #3b82f6;
            border-radius: 50%;
            width: 30px;
            height: 30px;
            animation: spin 1s linear infinite;
        }
        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }

        /* Responsive Modal */
        .modal-content {
            max-height: 90vh;
            overflow-y: auto;
        }

        /* Chart Container Responsive */
        .chart-container {
            height: 300px;
            position: relative;
        }
        @media (min-width: 640px) {
            .chart-container { height: 350px; }
        }
        @media (min-width: 1024px) {
            .chart-container { height: 400px; }
        }

        /* Button Groups Responsive */
        .button-group {
            display: flex;
            flex-wrap: wrap;
            gap: 0.5rem;
            justify-content: center;
        }
        @media (min-width: 768px) {
            .button-group { justify-content: flex-start; }
        }

        /* Responsive padding adjustments */
        .container-mobile {
            padding-left: 1rem;
            padding-right: 1rem;
        }
        @media (min-width: 640px) {
            .container-mobile {
                padding-left: 1.5rem;
                padding-right: 1.5rem;
            }
        }
        @media (min-width: 1024px) {
            .container-mobile {
                padding-left: 2rem;
                padding-right: 2rem;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto container-mobile py-4 sm:py-6 lg:py-8">
        <header class="text-center mb-6 sm:mb-8 lg:mb-10">
            <h1 class="page-title font-bold text-gray-800">Admin Dashboard</h1>
            <p class="text-base sm:text-lg text-gray-600 mt-2">
                <asp:Label ID="lblWelcome" runat="server" Font-Bold="true" />
            </p>
        </header>

        <section class="mb-6 sm:mb-8 lg:mb-10">
            <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 sm:gap-6">
                <div class="metric-card bg-white rounded-xl p-4 sm:p-6 text-center">
                    <h4 class="text-gray-500 text-base sm:text-lg mb-2">Total Clients</h4>
                    <asp:Label ID="lblTotalClients" runat="server" CssClass="metric-value text-blue-600"></asp:Label>
                </div>
                <div class="metric-card bg-white rounded-xl p-4 sm:p-6 text-center">
                    <h4 class="text-gray-500 text-base sm:text-lg mb-2">Total Employees</h4>
                    <asp:Label ID="lblTotalWorkers" runat="server" CssClass="metric-value text-blue-600"></asp:Label>
                </div>
                <div class="metric-card bg-white rounded-xl p-4 sm:p-6 text-center">
                    <h4 class="text-gray-500 text-base sm:text-lg mb-2">Today's Sales</h4>
                    <asp:Label ID="lblTodaySales" runat="server" CssClass="metric-value text-green-600"></asp:Label>
                </div>
                <div class="metric-card bg-white rounded-xl p-4 sm:p-6 text-center">
                    <h4 class="text-gray-500 text-base sm:text-lg mb-2">This Month's Sales</h4>
                    <asp:Label ID="lblMonthSales" runat="server" CssClass="metric-value text-indigo-600"></asp:Label>
                </div>
            </div>
        </section>

        <section class="mb-6 sm:mb-8 lg:mb-10">
            <div class="bg-white rounded-xl shadow-md overflow-hidden">
                <div class="bg-blue-600 text-white font-semibold text-base sm:text-lg p-3 sm:p-4 text-center">
                    <i class="fas fa-calendar-alt mr-2"></i> Weekly Booking Calendar
                </div>
                <div class="calendar-wrapper">
                    <asp:Table ID="tblCalendar" runat="server" CssClass="custom-calendar-table border-collapse" />
                </div>
            </div>
        </section>

        <div class="fixed inset-0 bg-gray-900 bg-opacity-50 hidden items-center justify-center z-[1000] p-4" id="bookingDetailsModal">
            <div class="bg-white rounded-xl shadow-lg w-full max-w-lg modal-content p-4 sm:p-6">
                <div class="flex justify-between items-center mb-4 border-b pb-3">
                    <h5 class="text-lg sm:text-xl font-semibold text-gray-800">Booking Details</h5>
                    <button type="button" class="text-gray-400 hover:text-gray-600 text-xl sm:text-2xl" onclick="hideBookingModal()">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
                <div class="text-gray-700 text-sm sm:text-base" id="bookingDetailsContent"></div>
            </div>
        </div>
        <script>
            const bookingModal = document.getElementById('bookingDetailsModal');
            function showBookingModal(details) {
                document.getElementById('bookingDetailsContent').innerHTML = details;
                bookingModal.classList.remove('hidden');
                bookingModal.classList.add('flex');
                document.body.style.overflow = 'hidden';
            }
            function hideBookingModal() {
                bookingModal.classList.add('hidden');
                bookingModal.classList.remove('flex');
                document.body.style.overflow = '';
            }
            bookingModal.addEventListener('click', (e) => {
                if (e.target === bookingModal) {
                    hideBookingModal();
                }
            });
        </script>

        <section class="mb-6 sm:mb-8 lg:mb-10">
            <div class="bg-white rounded-xl shadow-md p-4 sm:p-6">
                <div class="flex flex-col md:flex-row justify-between items-center mb-4 sm:mb-6">
                    <h3 class="text-xl sm:text-2xl font-semibold text-gray-800 mb-4 md:mb-0">Sales Overview</h3>
                    <div class="flex space-x-2">
                        <button type="button" onclick="setChartType('bar')" class="px-3 sm:px-4 py-2 rounded-full font-bold text-sm sm:text-base text-white bg-blue-600 hover:bg-blue-700 transition">Bar</button>
                        <button type="button" onclick="setChartType('line')" class="px-3 sm:px-4 py-2 rounded-full font-bold text-sm sm:text-base text-gray-800 bg-gray-200 hover:bg-gray-300 transition">Line</button>
                    </div>
                </div>
                <div class="button-group mb-4 sm:mb-6">
                    <button type="button" onclick="loadSalesData('daily')" class="px-3 sm:px-4 py-2 rounded-full text-sm sm:text-base text-blue-600 border border-blue-600 hover:bg-blue-600 hover:text-white transition">Daily</button>
                    <button type="button" onclick="loadSalesData('weekly')" class="px-3 sm:px-4 py-2 rounded-full text-sm sm:text-base text-blue-600 border border-blue-600 hover:bg-blue-600 hover:text-white transition">Weekly</button>
                    <button type="button" onclick="loadSalesData('monthly')" class="px-3 sm:px-4 py-2 rounded-full text-sm sm:text-base text-blue-600 border border-blue-600 hover:bg-blue-600 hover:text-white transition">Monthly</button>
                    <button type="button" onclick="loadSalesData('yearly')" class="px-3 sm:px-4 py-2 rounded-full text-sm sm:text-base text-blue-600 border border-blue-600 hover:bg-blue-600 hover:text-white transition">Yearly</button>
                </div>
                <div class="chart-container">
                    <canvas id="salesChart"></canvas>
                </div>
            </div>
        </section>

        <hr class="my-6 sm:my-8 lg:my-10 border-gray-300" />
        
        <span id="blockchainSection"></span>
        <section class="text-center py-6 sm:py-8 loading-container" id="blockchainContainer">
            <div class="loading-overlay" id="loadingOverlay">
                <div class="flex flex-col items-center px-4">
                    <div class="spinner mb-2"></div>
                    <span class="text-blue-600 font-bold text-sm sm:text-base text-center">Verifying blockchain, please wait...</span>
                </div>
            </div>

            <asp:UpdatePanel ID="upBlockchain" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                <ContentTemplate>
                    <h3 class="text-xl sm:text-2xl font-semibold text-gray-800 mb-4 px-4">Blockchain Sales Transparency</h3>
                    <div class="my-4 sm:my-6">
                        <asp:Button ID="btnVerifyBlockchain" runat="server"
                            Text="🔍 Verify Blockchain"
                            CssClass="px-4 sm:px-6 py-2 sm:py-3 rounded-full text-sm sm:text-base text-white bg-green-600 hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-500 focus:ring-offset-2 transition"
                            OnClick="btnVerifyBlockchain_Click"
                            CausesValidation="false"
                            UseSubmitBehavior="false" />
                    </div>
                    <asp:Label ID="lblVerificationResult" runat="server" CssClass="text-base sm:text-lg font-bold mt-4 block px-4" />
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="btnVerifyBlockchain" EventName="Click" />
                </Triggers>
            </asp:UpdatePanel>
        </section>

        <script type="text/javascript">
            var prm = Sys.WebForms.PageRequestManager.getInstance();
            var loadingOverlay = document.getElementById('loadingOverlay');
            var blockchainSection = document.getElementById('blockchainContainer');

            prm.add_beginRequest(function (sender, args) {
                loadingOverlay.classList.add('visible');
                blockchainSection.style.minHeight = blockchainSection.offsetHeight + 'px';
            });
            prm.add_endRequest(function (sender, args) {
                loadingOverlay.classList.remove('visible');
                blockchainSection.style.minHeight = '';

                var anchor = document.getElementById("blockchainSection");
                if (anchor) {
                    anchor.scrollIntoView({ behavior: "smooth" });
                }
            });
        </script>
        
        <div class="fixed inset-0 bg-gray-900 bg-opacity-50 hidden items-center justify-center z-[1000] p-4" id="jsonModal">
            <div class="bg-white rounded-xl shadow-lg w-full max-w-3xl modal-content p-4 sm:p-6">
                <div class="flex justify-between items-center mb-4 border-b pb-3">
                    <h5 class="text-lg sm:text-xl font-semibold text-gray-800">Transaction Details</h5>
                    <button type="button" class="text-gray-400 hover:text-gray-600 text-xl sm:text-2xl" onclick="closeJsonModal()">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
                <div class="text-gray-700">
                    <pre id="jsonModalBody" class="bg-gray-100 p-3 sm:p-4 rounded-lg overflow-x-auto text-xs sm:text-sm"></pre>
                </div>
            </div>
        </div>
        <script>
            const jsonModal = document.getElementById('jsonModal');
            function openJsonModal(jsonStr) {
                try {
                    const parsedJson = JSON.parse(jsonStr);
                    document.getElementById('jsonModalBody').textContent = JSON.stringify(parsedJson, null, 2);
                } catch (e) {
                    document.getElementById('jsonModalBody').textContent = 'Invalid JSON data.';
                }
                jsonModal.classList.remove('hidden');
                jsonModal.classList.add('flex');
                document.body.style.overflow = 'hidden';
            }
            function closeJsonModal() {
                jsonModal.classList.add('hidden');
                jsonModal.classList.remove('flex');
                document.body.style.overflow = '';
            }
            jsonModal.addEventListener('click', (e) => {
                if (e.target === jsonModal) {
                    closeJsonModal();
                }
            });
        </script>

        <input type="hidden" id="salesDataJson" value='<%= salesDataJson %>' />

        <script src="https://cdn.jsdelivr.net/npm/chart.js@4"></script>
        <script>
            let chart, currentChartType = 'bar';
            function setChartType(type) {
                currentChartType = type;
                const buttons = document.querySelectorAll('[onclick^="setChartType"]');
                buttons.forEach(btn => {
                    if (btn.textContent.toLowerCase().includes(type)) {
                        btn.className = 'px-3 sm:px-4 py-2 rounded-full font-bold text-sm sm:text-base text-white bg-blue-600 hover:bg-blue-700 transition';
                    } else {
                        btn.className = 'px-3 sm:px-4 py-2 rounded-full font-bold text-sm sm:text-base text-gray-800 bg-gray-200 hover:bg-gray-300 transition';
                    }
                });
                if (window._lastRange) {
                    loadSalesData(window._lastRange);
                }
            }
            function loadSalesData(type) {
                window._lastRange = type;
                fetch('Dashboard.aspx/GetSalesData', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json; charset=utf-8' },
                    body: JSON.stringify({ type })
                }).then(r => r.json()).then(res => updateChart(res.d)).catch(console.error);
            }
            function updateChart(salesData) {
                if (!salesData) return;
                const ctx = document.getElementById('salesChart').getContext('2d');
                if (chart) chart.destroy();
                chart = new Chart(ctx, {
                    type: currentChartType,
                    data: {
                        labels: salesData.labels,
                        datasets: [{
                            label: 'Sales (₱)',
                            data: salesData.data,
                            backgroundColor: 'rgba(59, 130, 246, 0.8)',
                            borderColor: 'rgb(59, 130, 246)',
                            borderWidth: 2,
                            tension: currentChartType === 'line' ? 0.3 : 0,
                            fill: false
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        scales: {
                            y: {
                                beginAtZero: true,
                                ticks: {
                                    callback: v => '₱' + Number(v).toLocaleString('en-PH', { minimumFractionDigits: 2, maximumFractionDigits: 2 }),
                                    font: { size: window.innerWidth < 640 ? 10 : 12 }
                                }
                            },
                            x: {
                                ticks: {
                                    font: { size: window.innerWidth < 640 ? 10 : 12 },
                                    maxRotation: window.innerWidth < 640 ? 45 : 0,
                                    minRotation: window.innerWidth < 640 ? 45 : 0
                                }
                            }
                        },
                        plugins: {
                            tooltip: {
                                callbacks: {
                                    label: (ctx) => 'Sales: ₱' + Number(ctx.parsed.y).toLocaleString('en-PH', { minimumFractionDigits: 2 })
                                }
                            },
                            legend: {
                                labels: {
                                    font: { size: window.innerWidth < 640 ? 11 : 12 }
                                }
                            }
                        },
                        elements: {
                            point: { radius: currentChartType === 'line' ? 3 : 0 }
                        }
                    }
                });
            }
            document.addEventListener('DOMContentLoaded', () => loadSalesData('monthly'));
        </script>
    </div>
</asp:Content>