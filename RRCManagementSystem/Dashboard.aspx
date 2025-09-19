<%@ Page Title="Dashboard"
    Language="C#"
    MasterPageFile="~/Admin.Master"
    AutoEventWireup="true"
    MaintainScrollPositionOnPostBack="true"
    CodeBehind="Dashboard.aspx.cs"
    Inherits="RRCManagementSystem.Dashboard" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        /* Custom styles to handle specific component behaviors not covered by Tailwind */
        .page-title { font-size: 2.25rem; } /* text-4xl in Tailwind */
        .metric-card {
            transition: transform 0.3s ease-in-out, box-shadow 0.3s ease-in-out;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
        }
        .metric-card:hover { transform: translateY(-5px); box-shadow: 0 8px 20px rgba(0, 0, 0, 0.15); }
        .metric-value { font-size: 2.5rem; font-weight: bold; }
        
        /* * Important: Calendar table styles
         * Ensures consistent cell size and styling across different browsers and content sizes.
         */
        .custom-calendar-table td, .custom-calendar-table th {
            min-width: 140px;
            vertical-align: top;
            border: 1px solid #e5e7eb; /* gray-200 */
            padding: 0.5rem;
            height: 140px; /* Provides a minimum height for empty cells */
        }
        .custom-calendar-table td:hover { background-color: #f3f4f6; /* gray-100 */ }
    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="container mx-auto px-4 py-8">
        <header class="text-center mb-10">
            <h1 class="page-title font-bold text-gray-800">Admin Dashboard</h1>
            <p class="text-lg text-gray-600 mt-2">
                <asp:Label ID="lblWelcome" runat="server" Font-Bold="true" />
            </p>
        </header>

        <section class="mb-10">
            <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
                <div class="metric-card bg-white rounded-xl p-6 text-center">
                    <h4 class="text-gray-500 text-lg mb-2">Total Clients</h4>
                    <asp:Label ID="lblTotalClients" runat="server" CssClass="metric-value text-blue-600"></asp:Label>
                </div>
                <div class="metric-card bg-white rounded-xl p-6 text-center">
                    <h4 class="text-gray-500 text-lg mb-2">Total Employees</h4>
                    <asp:Label ID="lblTotalWorkers" runat="server" CssClass="metric-value text-blue-600"></asp:Label>
                </div>
                <div class="metric-card bg-white rounded-xl p-6 text-center">
                    <h4 class="text-gray-500 text-lg mb-2">Today's Sales</h4>
                    <asp:Label ID="lblTodaySales" runat="server" CssClass="metric-value text-green-600"></asp:Label>
                </div>
                <div class="metric-card bg-white rounded-xl p-6 text-center">
                    <h4 class="text-gray-500 text-lg mb-2">This Month's Sales</h4>
                    <asp:Label ID="lblMonthSales" runat="server" CssClass="metric-value text-indigo-600"></asp:Label>
                </div>
            </div>
        </section>

        <section class="mb-10">
            <div class="bg-white rounded-xl shadow-md overflow-hidden">
                <div class="bg-blue-600 text-white font-semibold text-lg p-4 text-center">
                    <i class="fas fa-calendar-alt mr-2"></i> Weekly Booking Calendar
                </div>
                <div class="overflow-x-auto">
                    <asp:Table ID="tblCalendar" runat="server" CssClass="custom-calendar-table w-full border-collapse" />
                </div>
            </div>
        </section>

        <div class="fixed inset-0 bg-gray-900 bg-opacity-50 hidden items-center justify-center z-[1000]" id="bookingDetailsModal">
            <div class="bg-white rounded-xl shadow-lg w-[90%] max-w-lg p-6">
                <div class="flex justify-between items-center mb-4 border-b pb-3">
                    <h5 class="text-xl font-semibold text-gray-800">Booking Details</h5>
                    <button type="button" class="text-gray-400 hover:text-gray-600" onclick="hideBookingModal()">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
                <div class="text-gray-700" id="bookingDetailsContent"></div>
            </div>
        </div>
        <script>
            function showBookingModal(details) {
                document.getElementById('bookingDetailsContent').innerHTML = details;
                document.getElementById('bookingDetailsModal').classList.remove('hidden');
                document.getElementById('bookingDetailsModal').classList.add('flex');
            }
            function hideBookingModal() {
                document.getElementById('bookingDetailsModal').classList.add('hidden');
                document.getElementById('bookingDetailsModal').classList.remove('flex');
            }
        </script>

        <section class="mb-10">
            <div class="bg-white rounded-xl shadow-md p-6">
                <div class="flex flex-col md:flex-row justify-between items-center mb-6">
                    <h3 class="text-2xl font-semibold text-gray-800 mb-4 md:mb-0">Sales Overview</h3>
                    <div class="flex space-x-2">
                        <button type="button" onclick="setChartType('bar')" class="px-4 py-2 rounded-full font-bold text-white bg-blue-600 hover:bg-blue-700 transition">Bar</button>
                        <button type="button" onclick="setChartType('line')" class="px-4 py-2 rounded-full font-bold text-gray-800 bg-gray-200 hover:bg-gray-300 transition">Line</button>
                    </div>
                </div>
                <div class="flex flex-wrap justify-center md:justify-start gap-2 mb-6">
                    <button type="button" onclick="loadSalesData('daily')" class="px-4 py-2 rounded-full text-blue-600 border border-blue-600 hover:bg-blue-600 hover:text-white transition">Daily</button>
                    <button type="button" onclick="loadSalesData('weekly')" class="px-4 py-2 rounded-full text-blue-600 border border-blue-600 hover:bg-blue-600 hover:text-white transition">Weekly</button>
                    <button type="button" onclick="loadSalesData('monthly')" class="px-4 py-2 rounded-full text-blue-600 border border-blue-600 hover:bg-blue-600 hover:text-white transition">Monthly</button>
                    <button type="button" onclick="loadSalesData('yearly')" class="px-4 py-2 rounded-full text-blue-600 border border-blue-600 hover:bg-blue-600 hover:text-white transition">Yearly</button>
                </div>
                <div class="h-[400px]">
                    <canvas id="salesChart"></canvas>
                </div>
            </div>
        </section>

        <hr class="my-10 border-gray-300" />

        <span id="blockchainSection"></span>
        <asp:UpdatePanel ID="upBlockchain" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <ContentTemplate>
                <section class="text-center py-8">
                    <h3 class="text-2xl font-semibold text-gray-800 mb-4">Blockchain Sales Transparency</h3>
                    <div class="my-6">
                        <asp:Button ID="btnVerifyBlockchain" runat="server"
                            Text="🔍 Verify Blockchain"
                            CssClass="px-6 py-3 rounded-full text-white bg-green-600 hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-500 focus:ring-offset-2 transition"
                            OnClick="btnVerifyBlockchain_Click"
                            CausesValidation="false"
                            UseSubmitBehavior="false" />
                    </div>
                    <asp:UpdateProgress ID="upProgress" runat="server" AssociatedUpdatePanelID="upBlockchain">
                        <ProgressTemplate>
                            <div class="text-center text-blue-600 font-bold mb-3">
                                <i class="fas fa-spinner fa-spin mr-2"></i> Verifying blockchain, please wait...
                            </div>
                        </ProgressTemplate>
                    </asp:UpdateProgress>
                    <asp:Label ID="lblVerificationResult" runat="server" CssClass="text-lg font-bold mt-4 block" />
                </section>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnVerifyBlockchain" EventName="Click" />
            </Triggers>
        </asp:UpdatePanel>

        <script type="text/javascript">
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                var anchor = document.getElementById("blockchainSection");
                if (anchor) {
                    anchor.scrollIntoView({ behavior: "smooth" });
                }
            });
        </script>
        
        <div class="fixed inset-0 bg-gray-900 bg-opacity-50 hidden items-center justify-center z-[1000]" id="jsonModal">
            <div class="bg-white rounded-xl shadow-lg w-[90%] max-w-3xl p-6">
                <div class="flex justify-between items-center mb-4 border-b pb-3">
                    <h5 class="text-xl font-semibold text-gray-800">Transaction Details</h5>
                    <button type="button" class="text-gray-400 hover:text-gray-600" onclick="closeJsonModal()">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
                <div class="text-gray-700">
                    <pre id="jsonModalBody" class="bg-gray-100 p-4 rounded-lg overflow-x-auto text-sm"></pre>
                </div>
            </div>
        </div>

        <script>
            function openJsonModal(jsonStr) {
                try {
                    const parsedJson = JSON.parse(jsonStr);
                    document.getElementById('jsonModalBody').textContent = JSON.stringify(parsedJson, null, 2);
                } catch (e) {
                    document.getElementById('jsonModalBody').textContent = 'Invalid JSON data.';
                }
                document.getElementById('jsonModal').classList.remove('hidden');
                document.getElementById('jsonModal').classList.add('flex');
            }
            function closeJsonModal() {
                document.getElementById('jsonModal').classList.add('hidden');
                document.getElementById('jsonModal').classList.remove('flex');
            }
        </script>

        <input type="hidden" id="salesDataJson" value='<%= salesDataJson %>' />

        <script src="https://cdn.jsdelivr.net/npm/chart.js@4"></script>
        <script>
            let chart, currentChartType = 'bar';
            function setChartType(type) {
                currentChartType = type;
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
                            backgroundColor: 'rgba(59, 130, 246, 0.8)', /* blue-500 */
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
                                    callback: v => '₱' + Number(v).toLocaleString('en-PH', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
                                }
                            }
                        },
                        plugins: {
                            tooltip: {
                                callbacks: {
                                    label: (ctx) => 'Sales: ₱' + Number(ctx.parsed.y).toLocaleString('en-PH', { minimumFractionDigits: 2 })
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