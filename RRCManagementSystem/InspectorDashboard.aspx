<%@ Page Title="" Language="C#" MasterPageFile="~/Inspector.master" AutoEventWireup="true" CodeBehind="InspectorDashboard.aspx.cs" Inherits="RRCManagementSystem.InspectorDashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .dashboard-header {
            font-size: 24px;
            font-weight: bold;
            margin-bottom: 20px;
            color: #004085;
        }

        .dashboard-summary {
            display: flex;
            gap: 30px;
            margin-bottom: 40px;
            flex-wrap: wrap;
        }

        .summary-box {
            flex: 1;
            background-color: #ffffff;
            border-radius: 10px;
            box-shadow: 0 2px 6px rgba(0,0,0,0.1);
            padding: 20px;
            text-align: center;
            min-width: 200px;
        }

        .summary-box h2 {
            font-size: 36px;
            color: #007bff;
            margin-bottom: 10px;
        }

        .summary-box span {
            font-size: 16px;
            color: #555;
        }

        #calendar {
            background-color: white;
            border-radius: 10px;
            padding: 20px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
            margin-bottom: 30px;
            min-height: 500px;
        }
    </style>

    <h2 class="dashboard-header">🗂 Inspector Dashboard</h2>

    <!-- Summary Cards -->
    <div class="dashboard-summary">
        <div class="summary-box">
            <h2><asp:Label ID="lblTotalInspections" runat="server" Text="0" /></h2>
            <span>Total Inspections</span>
        </div>
        <div class="summary-box">
            <h2><asp:Label ID="lblTodayInspections" runat="server" Text="0" /></h2>
            <span>Inspections Today</span>
        </div>
    </div>

    <!-- FullCalendar Container -->
    <div id="calendar"></div>

    <!-- Modal for Inspection Details 
    <div class="modal fade" id="eventModal" tabindex="-1" aria-labelledby="eventModalLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="eventModalLabel">Inspection Details</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <p><strong>Date:</strong> <span id="modalDate"></span></p>
                    <div id="modalDetails">Loading details...</div>
                </div>
            </div>
        </div>
    </div>
                -->

    <!-- ✅ Scripts & Libraries -->
    <link href="https://cdn.jsdelivr.net/npm/fullcalendar@6.1.8/index.global.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/fullcalendar@6.1.8/index.global.min.js"></script>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>

    <!-- ✅ FullCalendar Script -->
    <script>
        window.addEventListener('load', function () {
            const calendarEl = document.getElementById('calendar');
            if (!calendarEl) {
                console.warn("Calendar element not found.");
                return;
            }

            console.log("✅ Calendar element found. Initializing FullCalendar...");

            const calendar = new FullCalendar.Calendar(calendarEl, {
                initialView: 'dayGridMonth',
                height: "auto",
                events: 'LoadInspections.ashx',
                dateClick: function (info) {
                    fetch('GetInspectionDetails.ashx?date=' + info.dateStr)
                        .then(response => response.text())
                        .then(html => {
                            document.getElementById("modalDate").textContent = info.dateStr;
                            document.getElementById("modalDetails").innerHTML = html;
                            const modal = new bootstrap.Modal(document.getElementById('eventModal'));
                            modal.show();
                        })
                        .catch(error => console.error("❌ Error loading details:", error));
                },
                loading: function (isLoading) {
                    console.log("🔄 Loading events: ", isLoading);
                },
                eventsSet: function (events) {
                    console.log("📅 Events loaded:", events);
                }
            });

            setTimeout(() => calendar.render(), 100);
        });
    </script>
</asp:Content>
