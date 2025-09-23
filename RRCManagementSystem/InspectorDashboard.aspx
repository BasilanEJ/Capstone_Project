<%@ Page Title="Inspector Dashboard" Language="C#" MasterPageFile="~/Inspector.master" AutoEventWireup="true" CodeBehind="InspectorDashboard.aspx.cs" Inherits="RRCManagementSystem.InspectorDashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="px-4 py-6 sm:px-6 lg:px-8">
        <!-- HEADER -->
        <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between mb-6">
            <h2 class="text-2xl sm:text-3xl lg:text-4xl font-extrabold text-blue-700 mb-4 sm:mb-0">
                <i class="fas fa-chart-line mr-2"></i> Inspector Dashboard
            </h2>
        </div>

        <!-- KPI CARDS -->
        <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 xl:grid-cols-4 gap-5 mb-10">
            <!-- Total Inspections -->
            <div class="bg-white rounded-xl shadow hover:shadow-lg p-5 transition transform hover:-translate-y-1">
                <div class="text-center">
                    <div class="text-3xl sm:text-4xl font-bold text-blue-600">
                        <asp:Label ID="lblTotalInspections" runat="server" Text="0" />
                    </div>
                    <p class="text-gray-500 text-base sm:text-lg font-medium mt-2">Total Inspections</p>
                </div>
            </div>

            <!-- Today's Inspections -->
            <div class="bg-white rounded-xl shadow hover:shadow-lg p-5 transition transform hover:-translate-y-1">
                <div class="text-center">
                    <div class="text-3xl sm:text-4xl font-bold text-green-600">
                        <asp:Label ID="lblTodayInspections" runat="server" Text="0" />
                    </div>
                    <p class="text-gray-500 text-base sm:text-lg font-medium mt-2">Inspections Today</p>
                </div>
            </div>
        </div>

        <!-- CALENDAR -->
        <div class="bg-white rounded-2xl shadow-lg p-4 sm:p-6 mb-8">
            <div id="calendar" class="w-full"></div>
        </div>

        <!-- MODAL FOR INSPECTION DETAILS -->
        <div id="eventModal" class="fixed inset-0 z-[9999] hidden items-center justify-center p-4">
            <!-- Overlay -->
            <div class="fixed inset-0 bg-black bg-opacity-50 transition-opacity" onclick="closeModal()"></div>

            <!-- Modal Content -->
            <div id="modalContent" class="relative bg-white rounded-xl shadow-2xl w-full max-w-md transform transition-transform duration-300 scale-95">
                <div class="flex justify-between items-center px-5 py-4 border-b border-gray-200">
                    <h3 class="text-lg sm:text-xl font-bold text-gray-800">Inspection Details</h3>
                    <button type="button" class="text-gray-400 hover:text-gray-600" onclick="closeModal()">
                        <i class="fas fa-times text-xl"></i>
                    </button>
                </div>

                <div class="p-5 text-gray-700 space-y-4">
                    <p>
                        <strong class="text-gray-900">Date:</strong>
                        <span id="modalDate" class="ml-1"></span>
                    </p>
                    <div id="modalDetails" class="text-gray-600 text-sm">Loading details...</div>
                </div>

            </div>
        </div>
    </div>

    <!-- FullCalendar -->
    <link href="https://cdn.jsdelivr.net/npm/fullcalendar@6.1.8/index.global.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/fullcalendar@6.1.8/index.global.min.js"></script>

    <style>
        /* ===== FullCalendar Toolbar Optimizations ===== */
        .fc-toolbar.fc-header-toolbar {
            display: flex;
            flex-wrap: wrap;
            align-items: center;
            justify-content: space-between;
            gap: 0.5rem;
            margin-bottom: 1rem;
        }

        /* Left group (Prev, Today, Next) */
        .fc-toolbar-chunk:first-child {
            display: flex;
            gap: 0.5rem;
            align-items: center;
        }

        /* Center title */
        .fc-toolbar-chunk:nth-child(2) {
            flex: 1;
            text-align: center;
            font-weight: bold;
            font-size: 1.25rem;
        }

        /* Right group */
        .fc-toolbar-chunk:last-child {
            display: flex;
            gap: 0.5rem;
        }

        /* Mobile View: Stack vertically */
        @media (max-width: 640px) {
            .fc-toolbar.fc-header-toolbar {
                flex-direction: column;
                text-align: center;
            }

            .fc-toolbar-chunk:nth-child(2) {
                order: 1;
                margin-top: 0.5rem;
            }

            .fc-toolbar-chunk:first-child {
                order: 2;
            }

            .fc-toolbar-chunk:last-child {
                order: 3;
                margin-top: 0.5rem;
            }
        }

        /* Event Style */
        .fc-event {
            border-radius: 0.5rem;
            padding: 2px 4px;
            font-size: 0.85rem;
        }

        /* Modal animation */
        #modalContent {
            transform: scale(0.95);
            transition: transform 0.3s ease;
        }

        #modalContent.scale-100 {
            transform: scale(1);
        }
    </style>

    <script>
        let scrollPosition = 0;

        // === Modal Functions ===
        function showModal() {
            // Save current scroll position
            scrollPosition = window.scrollY;

            const modal = document.getElementById('eventModal');

            // Lock body scroll without layout shift
            document.body.style.position = 'fixed';
            document.body.style.top = `-${scrollPosition}px`;
            document.body.style.left = '0';
            document.body.style.right = '0';
            document.body.style.width = '100%';

            // Show modal
            modal.classList.remove('hidden');
            modal.classList.add('flex');

            setTimeout(() => {
                document.getElementById('modalContent').classList.add('scale-100');
            }, 10);
        }

        function closeModal() {
            const modal = document.getElementById('eventModal');
            const modalContent = document.getElementById('modalContent');

            // Start hide animation
            modalContent.classList.remove('scale-100');

            setTimeout(() => {
                modal.classList.remove('flex');
                modal.classList.add('hidden');

                // Restore scroll
                document.body.style.position = '';
                document.body.style.top = '';
                document.body.style.left = '';
                document.body.style.right = '';
                document.body.style.width = '';

                window.scrollTo(0, scrollPosition); // Return to saved scroll position
            }, 300); // Match transition duration
        }

        // === Calendar Initialization ===
        window.addEventListener('load', function () {
            const calendarEl = document.getElementById('calendar');
            if (!calendarEl) return;

            // Detect screen size
            const isMobile = window.innerWidth < 768;

            // Default view
            const initialView = isMobile ? 'listWeek' : 'dayGridMonth';

            // If mobile, disable Month and Week buttons by hiding them
            const customHeader = isMobile
                ? { left: 'prev today next', center: 'title', right: 'listWeek' }
                : { left: 'prev today next', center: 'title', right: 'dayGridMonth,timeGridWeek,listWeek' };

            const calendar = new FullCalendar.Calendar(calendarEl, {
                initialView: initialView,
                height: "auto",
                headerToolbar: customHeader,
                events: 'LoadInspections.ashx',
                eventClick: function (info) {
                    fetch('GetInspectionDetails.ashx?id=' + info.event.id)
                        .then(response => response.text())
                        .then(html => {
                            document.getElementById("modalDate").textContent =
                                new Date(info.event.start).toLocaleString();
                            document.getElementById("modalDetails").innerHTML = html;
                            showModal();
                        })
                        .catch(error => console.error("❌ Error loading details:", error));
                },
                eventTimeFormat: {
                    hour: 'numeric',
                    minute: '2-digit',
                    meridiem: 'short'
                }
            });

            calendar.render();
        });
    </script>
</asp:Content>
