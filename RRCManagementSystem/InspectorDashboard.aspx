<%@ Page Title="" Language="C#" MasterPageFile="~/Inspector.master" AutoEventWireup="true" CodeBehind="InspectorDashboard.aspx.cs" Inherits="RRCManagementSystem.InspectorDashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid py-4">
        <h2 class="fw-bold text-primary mb-4">🗂 Inspector Dashboard</h2>

        <!-- Summary Cards -->
        <div class="row g-4 mb-5">
            <div class="col-sm-6 col-md-4 col-lg-3">
                <div class="card shadow-sm text-center">
                    <div class="card-body">
                        <h2 class="text-primary display-6">
                            <asp:Label ID="lblTotalInspections" runat="server" Text="0" />
                        </h2>
                        <p class="text-muted mb-0">Total Inspections</p>
                    </div>
                </div>
            </div>
            <div class="col-sm-6 col-md-4 col-lg-3">
                <div class="card shadow-sm text-center">
                    <div class="card-body">
                        <h2 class="text-primary display-6">
                            <asp:Label ID="lblTodayInspections" runat="server" Text="0" />
                        </h2>
                        <p class="text-muted mb-0">Inspections Today</p>
                    </div>
                </div>
            </div>
        </div>

        <!-- FullCalendar Container -->
        <div id="calendar" class="bg-white rounded shadow p-3 mb-4"></div>

        <!-- Modal for Inspection Details -->
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
    </div>

    <!-- ✅ Scripts & Libraries -->
    <link href="https://cdn.jsdelivr.net/npm/fullcalendar@6.1.8/index.global.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/fullcalendar@6.1.8/index.global.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>

    <!-- ✅ FullCalendar Script -->
    <script>
        window.addEventListener('load', function () {
            const calendarEl = document.getElementById('calendar');
            if (!calendarEl) return;

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
                            new bootstrap.Modal(document.getElementById('eventModal')).show();
                        })
                        .catch(error => console.error("❌ Error loading details:", error));
                }
            });

            setTimeout(() => calendar.render(), 100);
        });
    </script>
</asp:Content>