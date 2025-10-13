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
            <div class="bg-white rounded-xl shadow hover:shadow-lg p-5 transition transform hover:-translate-y-1">
                <div class="text-center">
                    <div class="text-3xl sm:text-4xl font-bold text-blue-600">
                        <asp:Label ID="lblTotalInspections" runat="server" Text="0" />
                    </div>
                    <p class="text-gray-500 text-base sm:text-lg font-medium mt-2">Total Inspections</p>
                </div>
            </div>

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

        <!-- MODAL -->
        <div id="eventModal" class="fixed inset-0 z-[9999] hidden items-center justify-center p-4">
            <div class="fixed inset-0 bg-black bg-opacity-50 transition-opacity" onclick="closeModal()"></div>

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
        /* === FullCalendar Toolbar (Responsive) === */
        .fc-toolbar.fc-header-toolbar {
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            gap: 0.5rem;
            margin-bottom: 1rem;
        }

        /* Date title styling */
        .fc-toolbar-title {
            order: -1; /* Move title above buttons */
            font-size: 1.25rem;
            font-weight: 700;
            color: #1e40af; /* Tailwind blue-800 */
            text-align: center;
        }

        /* Buttons group */
        .fc-toolbar-chunk:first-child {
            display: flex;
            justify-content: center;
            gap: 0.5rem;
        }

        /* Event style */
        .fc-event {
            border-radius: 0.5rem;
            padding: 2px 4px;
            font-size: 0.85rem;
        }

        /* Mobile responsiveness */
        @media (min-width: 640px) {
            .fc-toolbar.fc-header-toolbar {
                flex-direction: row;
                justify-content: space-between;
            }
            .fc-toolbar-title {
                order: 0; /* Reset order for larger screens */
                font-size: 1.5rem;
            }
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

        function showModal() {
            scrollPosition = window.scrollY;
            const modal = document.getElementById('eventModal');
            document.body.style.position = 'fixed';
            document.body.style.top = `-${scrollPosition}px`;
            document.body.style.width = '100%';
            modal.classList.remove('hidden');
            modal.classList.add('flex');
            setTimeout(() => document.getElementById('modalContent').classList.add('scale-100'), 10);
        }

        function closeModal() {
            const modal = document.getElementById('eventModal');
            const modalContent = document.getElementById('modalContent');
            modalContent.classList.remove('scale-100');
            setTimeout(() => {
                modal.classList.add('hidden');
                document.body.style.position = '';
                document.body.style.top = '';
                window.scrollTo(0, scrollPosition);
            }, 300);
        }

        // === Calendar Initialization (List View Only) ===
        window.addEventListener('load', function () {
            const calendarEl = document.getElementById('calendar');
            if (!calendarEl) return;

            const calendar = new FullCalendar.Calendar(calendarEl, {
                initialView: 'listWeek',
                height: "auto",
                headerToolbar: {
                    left: 'prev today next',
                    center: 'title',  // ✅ The date will automatically move above buttons due to CSS
                    right: ''
                },
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
