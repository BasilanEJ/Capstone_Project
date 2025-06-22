<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ManageBooking.aspx.cs" Inherits="RRCManagementSystem.ManageBooking" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Bootstrap 5 CSS -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <!-- FontAwesome for icons -->
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" rel="stylesheet" />
    <!-- SweetAlert2 CSS -->
    <link href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container py-5">
        <h2 class="text-center mb-4 fw-semibold">📚 Manage Bookings</h2>

        <div class="row g-4 justify-content-center">

            <div class="col-12 col-sm-6 col-md-4 col-lg-3">
                <a href="AllBooking.aspx" class="text-decoration-none">
                    <div class="card shadow-sm h-100 text-center border-0 rounded-3 hover-shadow">
                        <div class="card-body d-flex flex-column justify-content-center align-items-center p-4">
                            <i class="fas fa-list fa-2x text-primary mb-3"></i>
                            <h5 class="card-title fw-semibold text-dark">View All Bookings</h5>
                        </div>
                    </div>
                </a>
            </div>

            <div class="col-12 col-sm-6 col-md-4 col-lg-3">
                <a href="ApproveRejectBookings.aspx" class="text-decoration-none">
                    <div class="card shadow-sm h-100 text-center border-0 rounded-3 hover-shadow">
                        <div class="card-body d-flex flex-column justify-content-center align-items-center p-4">
                            <i class="fas fa-check-circle fa-2x text-primary mb-3"></i>
                            <h5 class="card-title fw-semibold text-dark">Approve / Reject Bookings</h5>
                        </div>
                    </div>
                </a>
            </div>

            <div class="col-12 col-sm-6 col-md-4 col-lg-3">
                <a href="ApproveRescheduleRequests.aspx" class="text-decoration-none">
                    <div class="card shadow-sm h-100 text-center border-0 rounded-3 hover-shadow">
                        <div class="card-body d-flex flex-column justify-content-center align-items-center p-4">
                            <i class="fas fa-calendar-check fa-2x text-primary mb-3"></i>
                            <h5 class="card-title fw-semibold text-dark">Approve Reschedule Requests</h5>
                        </div>
                    </div>
                </a>
            </div>

            <div class="col-12 col-sm-6 col-md-4 col-lg-3">
                <a href="AllRescheduleBooking.aspx" class="text-decoration-none">
                    <div class="card shadow-sm h-100 text-center border-0 rounded-3 hover-shadow">
                        <div class="card-body d-flex flex-column justify-content-center align-items-center p-4">
                            <i class="fas fa-calendar-alt fa-2x text-primary mb-3"></i>
                            <h5 class="card-title fw-semibold text-dark">All Reschedule Booking</h5>
                        </div>
                    </div>
                </a>
            </div>

            <div class="col-12 col-sm-6 col-md-4 col-lg-3">
                <a href="BookingHistory.aspx" class="text-decoration-none">
                    <div class="card shadow-sm h-100 text-center border-0 rounded-3 hover-shadow">
                        <div class="card-body d-flex flex-column justify-content-center align-items-center p-4">
                            <i class="fas fa-history fa-2x text-primary mb-3"></i>
                            <h5 class="card-title fw-semibold text-dark">View Booking History</h5>
                        </div>
                    </div>
                </a>
            </div>

        </div>
    </div>

    <!-- Bootstrap 5 JS Bundle (Popper included) -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <!-- SweetAlert2 JS -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.all.min.js"></script>

    <script>
        // Example: Show SweetAlert on page load (optional)
        // Uncomment to test SweetAlert notification
        /*
        document.addEventListener('DOMContentLoaded', function () {
            Swal.fire({
                icon: 'info',
                title: 'Welcome to Manage Bookings',
                text: 'Use the cards to navigate different booking management options.',
                timer: 3000,
                showConfirmButton: false
            });
        });
        */
    </script>

    <style>
        /* Optional: subtle hover shadow effect on cards */
        .hover-shadow:hover {
            box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.15);
            transform: translateY(-4px);
            transition: all 0.3s ease;
        }
    </style>
</asp:Content>
