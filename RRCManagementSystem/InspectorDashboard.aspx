<%@ Page Title="" Language="C#" MasterPageFile="~/Inspector.master" AutoEventWireup="true" CodeBehind="InspectorDashboard.aspx.cs" Inherits="RRCManagementSystem.InspectorDashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="px-4 py-8 lg:px-8">
        <h2 class="text-3xl lg:text-4xl font-extrabold text-blue-600 mb-8">
            <i class="fas fa-chart-line mr-2"></i> Inspector Dashboard
        </h2>

        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 mb-12">
            <div class="bg-white rounded-xl shadow-lg p-6 text-center transform transition-transform hover:scale-105">
                <div class="flex flex-col items-center">
                    <div class="text-4xl font-bold text-blue-600 mb-2">
                        <asp:Label ID="lblTotalInspections" runat="server" Text="0" />
                    </div>
                    <p class="text-gray-500 font-medium text-lg">Total Inspections</p>
                </div>
            </div>

            <div class="bg-white rounded-xl shadow-lg p-6 text-center transform transition-transform hover:scale-105">
                <div class="flex flex-col items-center">
                    <div class="text-4xl font-bold text-blue-600 mb-2">
                        <asp:Label ID="lblTodayInspections" runat="server" Text="0" />
                    </div>
                    <p class="text-gray-500 font-medium text-lg">Inspections Today</p>
                </div>
            </div>
            
            </div>

        <div id="calendar" class="bg-white rounded-2xl shadow-xl p-4 md:p-6 mb-8"></div>

        <div id="eventModal" class="fixed inset-0 z-[9999] hidden items-center justify-center p-4">
            <div class="fixed inset-0 bg-black bg-opacity-50" onclick="closeModal()"></div>
            <div class="relative bg-white rounded-xl shadow-2xl w-full max-w-lg mx-auto transform transition-transform duration-300 scale-95" id="modalContent">
                <div class="flex justify-between items-center p-6 border-b border-gray-200">
                    <h3 class="text-xl font-bold text-gray-800" id="eventModalLabel">Inspection Details</h3>
                    <button type="button" class="text-gray-400 hover:text-gray-600 transition-colors" onclick="closeModal()">
                        <i class="fas fa-times text-2xl"></i>
                    </button>
                </div>
                <div class="p-6 text-gray-700">
                    <p class="mb-4">
                        <strong class="font-semibold text-gray-900">Date:</strong> <span id="modalDate"></span>
                    </p>
                    <div id="modalDetails" class="text-gray-600">Loading details...</div>
                </div>
            </div>
        </div>
    </div>

    <link href="https://cdn.jsdelivr.net/npm/fullcalendar@6.1.8/index.global.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/fullcalendar@6.1.8/index.global.min.js"></script>

    <style>
        /* Mobile-first styling for FullCalendar header */
        .fc-toolbar.fc-header-toolbar {
            flex-direction: column;
            align-items: center;
            margin-bottom: 1.5rem; /* Add some space below the header */
        }
        
        .fc-toolbar-chunk:first-child {
            order: 2; /* Puts the month name below the navigation buttons */
            margin-top: 1rem; /* Space between buttons and month name */
            font-size: 1.5rem; /* Make the month title more prominent on mobile */
        }

        .fc-toolbar-chunk:nth-child(2) {
            order: 1; /* Puts the navigation buttons at the top */
            display: flex;
            align-items: center;
            justify-content: space-between;
            width: 100%;
        }

        .fc-toolbar-chunk:last-child {
            display: none; /* Hide the view switcher on mobile */
        }

        /* Desktop-specific styles to revert the layout */
        @media (min-width: 640px) { /* Adjust based on your Tailwind breakpoints (sm) */
            .fc-toolbar.fc-header-toolbar {
                flex-direction: row;
                align-items: center;
                justify-content: space-between;
                margin-bottom: 0;
            }
            .fc-toolbar-chunk:first-child {
                order: 1;
                margin-top: 0;
                font-size: inherit;
            }
            .fc-toolbar-chunk:nth-child(2) {
                order: 2;
                width: auto;
            }
            .fc-toolbar-chunk:last-child {
                display: block;
                order: 3;
            }
        }
    </style>
    
    <script>
        // Modal functions
        function showModal() {
            const modal = document.getElementById('eventModal');
            modal.classList.remove('hidden');
            modal.classList.add('flex');
            setTimeout(() => document.getElementById('modalContent').classList.add('scale-100'), 10);
        }

        function closeModal() {
            const modal = document.getElementById('eventModal');
            document.getElementById('modalContent').classList.remove('scale-100');
            setTimeout(() => {
                modal.classList.remove('flex');
                modal.classList.add('hidden');
            }, 300);
        }

        window.addEventListener('load', function () {
            const calendarEl = document.getElementById('calendar');
            if (!calendarEl) return;

            const calendar = new FullCalendar.Calendar(calendarEl, {
                initialView: 'dayGridMonth',
                height: "auto",
                headerToolbar: {
                    left: 'prev,next today',
                    center: 'title',
                    right: 'dayGridMonth,timeGridWeek,timeGridDay'
                },
                events: 'LoadInspections.ashx',
                dateClick: function (info) {
                    fetch('GetInspectionDetails.ashx?date=' + info.dateStr)
                        .then(response => response.text())
                        .then(html => {
                            document.getElementById("modalDate").textContent = info.dateStr;
                            document.getElementById("modalDetails").innerHTML = html;
                            showModal(); // Use the new Tailwind modal function
                        })
                        .catch(error => console.error("❌ Error loading details:", error));
                }
            });

            setTimeout(() => calendar.render(), 100);
        });
    </script>
</asp:Content>