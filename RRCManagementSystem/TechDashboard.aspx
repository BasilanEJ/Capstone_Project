<%@ Page Title="Team Leader Dashboard" Language="C#" MasterPageFile="~/HeadTechnician.master" 
    AutoEventWireup="true" CodeBehind="TechDashboard.aspx.cs" Inherits="RRCManagementSystem.TechDashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- FullCalendar -->
    <link href="https://cdn.jsdelivr.net/npm/fullcalendar@6.1.8/index.global.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/fullcalendar@6.1.8/index.global.min.js"></script>

    <style>
        /* === KPI Cards === */
        .kpi-card {
            background: white;
            border-radius: 1rem;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
            padding: 1.5rem;
            transition: all 0.3s ease;
        }

        .kpi-card:hover {
            transform: translateY(-4px);
            box-shadow: 0 8px 16px rgba(0, 0, 0, 0.15);
        }

        .kpi-value {
            font-size: 2.5rem;
            font-weight: 700;
            line-height: 1;
        }

        .kpi-label {
            color: #64748b;
            font-size: 1rem;
            font-weight: 500;
            margin-top: 0.5rem;
        }

        /* === FullCalendar Toolbar (Responsive) === */
        .fc-toolbar.fc-header-toolbar {
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            gap: 0.5rem;
            margin-bottom: 1rem;
        }

        .fc-toolbar-title {
            order: -1;
            font-size: 1.25rem;
            font-weight: 700;
            color: #1e40af;
            text-align: center;
        }

        .fc-toolbar-chunk:first-child {
            display: flex;
            justify-content: center;
            gap: 0.5rem;
        }

        .fc-button {
            background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%) !important;
            border: none !important;
            padding: 0.5rem 1rem !important;
            border-radius: 0.5rem !important;
            font-weight: 600 !important;
        }

        .fc-button:hover {
            background: linear-gradient(135deg, #1e40af 0%, #1e3a8a 100%) !important;
        }

        .fc-button-active {
            background: linear-gradient(135deg, #1e3a8a 0%, #1e293b 100%) !important;
        }

        .fc-event {
            border-radius: 0.5rem;
            padding: 4px 6px;
            font-size: 0.875rem;
            font-weight: 500;
            cursor: pointer;
        }

        .fc-list-event:hover td {
            background-color: #f1f5f9 !important;
        }

        /* === Modal Styles === */
        #eventModal {
            backdrop-filter: blur(4px);
        }

        #modalContent {
            transform: scale(0.95);
            transition: transform 0.3s ease;
        }

        #modalContent.scale-100 {
            transform: scale(1);
        }

        .modal-header {
            background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%);
            color: white;
            padding: 1.25rem 1.5rem;
            border-radius: 0.75rem 0.75rem 0 0;
        }

        .modal-body {
            padding: 1.5rem;
        }

        .detail-item {
            display: flex;
            padding: 0.75rem 0;
            border-bottom: 1px solid #e5e7eb;
        }

        .detail-item:last-child {
            border-bottom: none;
        }

        .detail-label {
            font-weight: 600;
            color: #374151;
            min-width: 120px;
        }

        .detail-value {
            color: #6b7280;
            flex: 1;
        }

        /* === Mobile Responsiveness === */
        @media (min-width: 640px) {
            .fc-toolbar.fc-header-toolbar {
                flex-direction: row;
                justify-content: space-between;
            }

            .fc-toolbar-title {
                order: 0;
                font-size: 1.5rem;
            }

            .kpi-value {
                font-size: 3rem;
            }
        }

        @media (max-width: 640px) {
            .kpi-card {
                padding: 1rem;
            }

            .kpi-value {
                font-size: 2rem;
            }

            .kpi-label {
                font-size: 0.875rem;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="px-4 py-6 sm:px-6 lg:px-8">
        <!-- HEADER -->
        <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between mb-6">
            <h2 class="text-2xl sm:text-3xl lg:text-4xl font-extrabold text-blue-700 mb-4 sm:mb-0">
                <i class="fas fa-tachometer-alt mr-2"></i> Team Leader Dashboard
            </h2>
        </div>

        <!-- KPI CARDS -->
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5 mb-10">
            <!-- Total Bookings -->
            <div class="kpi-card">
                <div class="text-center">
                    <div class="kpi-value text-blue-600">
                        <asp:Label ID="lblTotalBookings" runat="server" Text="0" />
                    </div>
                    <p class="kpi-label">
                        <i class="fas fa-clipboard-list mr-2"></i>Total Bookings
                    </p>
                </div>
            </div>

            <!-- Today's Bookings -->
            <div class="kpi-card">
                <div class="text-center">
                    <div class="kpi-value text-green-600">
                        <asp:Label ID="lblTodayBookings" runat="server" Text="0" />
                    </div>
                    <p class="kpi-label">
                        <i class="fas fa-calendar-day mr-2"></i>Bookings Today
                    </p>
                </div>
            </div>

            <!-- In Progress -->
            <div class="kpi-card">
                <div class="text-center">
                    <div class="kpi-value text-orange-600">
                        <asp:Label ID="lblInProgress" runat="server" Text="0" />
                    </div>
                    <p class="kpi-label">
                        <i class="fas fa-spinner mr-2"></i>In Progress
                    </p>
                </div>
            </div>

            <!-- Completed -->
            <div class="kpi-card">
                <div class="text-center">
                    <div class="kpi-value text-purple-600">
                        <asp:Label ID="lblCompleted" runat="server" Text="0" />
                    </div>
                    <p class="kpi-label">
                        <i class="fas fa-check-circle mr-2"></i>Completed
                    </p>
                </div>
            </div>
        </div>

        <!-- CALENDAR -->
        <div class="bg-white rounded-2xl shadow-lg p-4 sm:p-6 mb-8">
            <h3 class="text-xl font-bold text-gray-800 mb-4">
                <i class="far fa-calendar-alt mr-2"></i>Team Schedule
            </h3>
            <div id="calendar" class="w-full"></div>
        </div>

        <!-- MODAL -->
        <div id="eventModal" class="fixed inset-0 z-[9999] hidden items-center justify-center p-4">
            <div class="fixed inset-0 bg-black bg-opacity-50 transition-opacity" onclick="closeModal()"></div>

            <div id="modalContent" class="relative bg-white rounded-xl shadow-2xl w-full max-w-2xl transform transition-transform duration-300 scale-95 max-h-[90vh] overflow-y-auto">
                <div class="modal-header">
                    <div class="flex justify-between items-center">
                        <h3 class="text-lg sm:text-xl font-bold">
                            <i class="fas fa-clipboard-check mr-2"></i>Booking Details
                        </h3>
                        <button type="button" class="text-white hover:text-gray-200 transition-colors" onclick="closeModal()">
                            <i class="fas fa-times text-xl"></i>
                        </button>
                    </div>
                </div>

                <div class="modal-body">
                    <div id="modalDetails" class="text-gray-700">
                        <div class="flex items-center justify-center py-8">
                            <i class="fas fa-spinner fa-spin text-3xl text-blue-600"></i>
                            <span class="ml-3 text-gray-600">Loading details...</span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

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
                modal.classList.remove('flex');
                document.body.style.position = '';
                document.body.style.top = '';
                window.scrollTo(0, scrollPosition);
            }, 300);
        }

        // === Calendar Initialization ===
        window.addEventListener('load', function () {
            const calendarEl = document.getElementById('calendar');
            if (!calendarEl) return;

            const calendar = new FullCalendar.Calendar(calendarEl, {
                initialView: 'listWeek',
                height: "auto",
                headerToolbar: {
                    left: 'prev today next',
                    center: 'title',
                    right: ''
                },
                events: 'LoadTeamBookings.ashx',  // ✅ Handler to load team's bookings
                eventClick: function (info) {
                    // Load booking details
                    fetch('GetTeamBookingDetails.ashx?id=' + info.event.id)
                        .then(response => response.text())
                        .then(html => {
                            document.getElementById("modalDetails").innerHTML = html;
                            showModal();
                        })
                        .catch(error => {
                            console.error("❌ Error loading details:", error);
                            document.getElementById("modalDetails").innerHTML = 
                                '<div class="text-center text-red-600 py-4"><i class="fas fa-exclamation-triangle mr-2"></i>Error loading booking details</div>';
                        });
                },
                eventTimeFormat: {
                    hour: 'numeric',
                    minute: '2-digit',
                    meridiem: 'short'
                },
                eventClassNames: function(arg) {
                    // Color code by status
                    if (arg.event.extendedProps.status === 'Completed') {
                        return ['bg-green-500', 'text-white'];
                    } else if (arg.event.extendedProps.status === 'In Progress') {
                        return ['bg-orange-500', 'text-white'];
                    } else {
                        return ['bg-blue-500', 'text-white'];
                    }
                }
            });

            calendar.render();
        });

        // Close modal on ESC key
        document.addEventListener('keydown', function(e) {
            if (e.key === 'Escape') {
                closeModal();
            }
        });
    </script>
</asp:Content>