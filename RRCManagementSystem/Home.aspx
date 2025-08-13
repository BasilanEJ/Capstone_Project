<%@ Page Title="Home" Language="C#" MasterPageFile="~/Client.master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="RRCManagementSystem.Home" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        /* Fluid, readable heading sizes across devices */
        .home-title {
            font-weight: 700;
            color: #0d6efd; /* Bootstrap primary */
            font-size: clamp(1.5rem, 3.5vw, 2.25rem);
            line-height: 1.2;
        }
        .home-subtitle {
            color: #6c757d; /* text-muted */
            font-size: clamp(0.95rem, 2.5vw, 1.125rem);
        }

        /* Card polish */
        .feature-card {
            transition: transform .15s ease, box-shadow .15s ease;
            border: none;
            border-radius: 1rem;
        }
        .feature-card:hover,
        .feature-card:focus-within {
            transform: translateY(-2px);
            box-shadow: 0 10px 24px rgba(0,0,0,.08);
        }
        .feature-icon {
            font-size: clamp(1.75rem, 6vw, 2.5rem);
        }

        /* Buttons fill width on mobile, auto on larger screens */
        .feature-btn {
            width: 100%;
        }
        @media (min-width: 576px) {
            .feature-btn { width: auto; }
        }

        /* Ensure comfy tap targets everywhere */
        .btn, .card, a, button {
            min-height: 44px;
        }

        /* Spacing that adapts */
        .home-wrap {
            padding-top: clamp(1rem, 2.5vw, 1.5rem);
            padding-bottom: clamp(1rem, 3vw, 2rem);
        }

        /* Respect reduced motion preferences */
        @media (prefers-reduced-motion: reduce) {
            .feature-card { transition: none !important; }
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container home-wrap">
        <!-- Welcome Section -->
        <div class="text-center mb-4 mb-md-5">
            <h1 class="home-title">Welcome, <%= Session["Name"] ?? "Valued Client" %>!</h1>
            <p class="home-subtitle">Manage your bookings, check payments, and stay connected with us.</p>
        </div>

        <!-- Feature Grid -->
        <div class="row g-3 g-sm-4 row-cols-1 row-cols-sm-2 row-cols-lg-3 row-cols-xxl-4 justify-content-center">
            <!-- Book a Service -->
            <div class="col">
                <div class="card feature-card h-100 shadow-sm text-center">
                    <div class="card-body d-flex flex-column align-items-center text-center p-4">
                        <i class="fas fa-calendar-plus feature-icon text-primary mb-3" aria-hidden="true"></i>
                        <h5 class="card-title fw-semibold mb-2">Book a Service</h5>
                        <p class="card-text text-muted mb-4">Schedule a new pest control service in a few taps.</p>
                        <a href="BookService.aspx" class="btn btn-primary feature-btn stretched-link">Book Now</a>
                    </div>
                </div>
            </div>

            <!-- My Bookings -->
            <div class="col">
                <div class="card feature-card h-100 shadow-sm text-center">
                    <div class="card-body d-flex flex-column align-items-center text-center p-4">
                        <i class="fas fa-clipboard-list feature-icon text-primary mb-3" aria-hidden="true"></i>
                        <h5 class="card-title fw-semibold mb-2">My Bookings</h5>
                        <p class="card-text text-muted mb-4">Track upcoming and completed service bookings.</p>
                        <a href="MyBookings.aspx" class="btn btn-primary feature-btn stretched-link">View Bookings</a>
                    </div>
                </div>
            </div>

            <!-- Our Contract -->
            <div class="col">
                <div class="card feature-card h-100 shadow-sm text-center">
                    <div class="card-body d-flex flex-column align-items-center text-center p-4">
                        <i class="fas fa-file-contract feature-icon text-primary mb-3" aria-hidden="true"></i>
                        <h5 class="card-title fw-semibold mb-2">Our Contract</h5>
                        <p class="card-text text-muted mb-4">Review your contract and service terms anytime.</p>
                        <a href="OurContract.aspx" class="btn btn-primary feature-btn stretched-link">View Contract</a>
                    </div>
                </div>
            </div>

            <!-- Pending Payments -->
            <div class="col">
                <div class="card feature-card h-100 shadow-sm text-center">
                    <div class="card-body d-flex flex-column align-items-center text-center p-4">
                        <i class="fas fa-wallet feature-icon text-primary mb-3" aria-hidden="true"></i>
                        <h5 class="card-title fw-semibold mb-2">Pending Payments</h5>
                        <p class="card-text text-muted mb-4">Check and settle your pending balances easily.</p>
                        <a href="Payment.aspx" class="btn btn-primary feature-btn stretched-link">Check Payments</a>
                    </div>
                </div>
            </div>
        </div>

        <!-- Accessibility helpers for screen readers -->
        <span class="visually-hidden">Use the buttons above to navigate to booking, tracking, contracts, and payments.</span>
    </div>
</asp:Content>
