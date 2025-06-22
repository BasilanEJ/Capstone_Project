<%@ Page Title="" Language="C#" MasterPageFile="~/Client.master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="RRCManagementSystem.Home" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Font Awesome & Bootstrap already assumed to be included in Master -->
    <style>
        .chat-btn {
            position: fixed;
            bottom: 30px;
            right: 30px;
            background-color: #0078FF;
            color: #ffffff;
            font-size: 24px;
            width: 60px;
            height: 60px;
            text-align: center;
            line-height: 60px;
            border-radius: 50%;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
            transition: all 0.3s ease;
            z-index: 9999;
        }

        .chat-btn:hover {
            background-color: #0056b3;
            transform: scale(1.1);
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mt-5">

        <!-- Welcome Section -->
        <div class="text-center mb-5">
            <h1 class="text-primary fw-bold">Welcome, <%= Session["Name"] ?? "Valued Client" %>!</h1>
            <p class="text-muted fs-5">Manage your bookings, check payments, and stay connected with us.</p>
        </div>

      <div class="row row-cols-1 row-cols-md-2 row-cols-lg-3 row-cols-xl-4 g-4 justify-content-center">
    <!-- Book a Service -->
    <div class="col">
        <div class="card h-100 shadow-sm text-center">
            <div class="card-body">
                <i class="fas fa-calendar-plus fa-3x text-primary mb-3"></i>
                <h5 class="card-title fw-semibold">Book a Service</h5>
                <p class="card-text text-muted">Schedule a new pest control service easily in a few clicks.</p>
                <a href="BookService.aspx" class="btn btn-primary">Book Now</a>
            </div>
        </div>
    </div>

    <!-- My Bookings -->
    <div class="col">
        <div class="card h-100 shadow-sm text-center">
            <div class="card-body">
                <i class="fas fa-clipboard-list fa-3x text-primary mb-3"></i>
                <h5 class="card-title fw-semibold">My Bookings</h5>
                <p class="card-text text-muted">Track your upcoming and completed service bookings.</p>
                <a href="MyBookings.aspx" class="btn btn-primary">View Bookings</a>
            </div>
        </div>
    </div>

    <!-- Our Contract -->
    <div class="col">
        <div class="card h-100 shadow-sm text-center">
            <div class="card-body">
                <i class="fas fa-file-contract fa-3x text-primary mb-3"></i>
                <h5 class="card-title fw-semibold">Our Contract</h5>
                <p class="card-text text-muted">Review your current pest control contract and service terms.</p>
                <a href="OurContract.aspx" class="btn btn-primary">View Contract</a>
            </div>
        </div>
    </div>

    <!-- Pending Payments -->
    <div class="col">
        <div class="card h-100 shadow-sm text-center">
            <div class="card-body">
                <i class="fas fa-wallet fa-3x text-primary mb-3"></i>
                <h5 class="card-title fw-semibold">Pending Payments</h5>
                <p class="card-text text-muted">Check and settle your pending balances easily.</p>
                <a href="Payment.aspx" class="btn btn-primary">Check Payments</a>
            </div>
        </div>
    </div>
</div>

        <!-- Floating Messenger Chat Button -->
        <a href="ChatWithAdmin.aspx" class="chat-btn" title="Chat with Admin">
            <i class="fab fa-facebook-messenger"></i>
        </a>

    </div>
</asp:Content>
