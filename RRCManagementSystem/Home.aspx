<%@ Page Title="" Language="C#" MasterPageFile="~/Client.master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="RRCManagementSystem.Home" %>


<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        body {
            background-color: #f8f9fa;
        }

        .welcome-section {
            text-align: center;
            margin-bottom: 40px;
        }

        .welcome-section h1 {
            color: #004085;
            font-weight: 700;
        }

        .welcome-section p {
            color: #6c757d;
            font-size: 1.1rem;
        }

        .card-container {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 20px;
        }

        .card-box {
            background: #ffffff;
            border-radius: 10px;
            padding: 30px 20px;
            box-shadow: 0 8px 16px rgba(0, 0, 0, 0.1);
            text-align: center;
            transition: 0.3s ease;
        }

        .card-box:hover {
            transform: translateY(-8px);
            box-shadow: 0 12px 20px rgba(0, 0, 0, 0.15);
        }

        .card-box i {
            font-size: 3rem;
            color: #004085;
            margin-bottom: 15px;
        }

        .card-box h5 {
            color: #333333;
            margin-bottom: 10px;
            font-weight: 600;
        }

        .card-box p {
            color: #6c757d;
            font-size: 14px;
            margin-bottom: 20px;
        }

        .btn-action {
            background-color: #004085;
            color: #ffffff;
            padding: 8px 16px;
            font-size: 14px;
            border-radius: 5px;
            text-decoration: none;
            display: inline-block;
            transition: background-color 0.3s ease;
        }

        .btn-action:hover {
            background-color: #003366;
        }

        /* Floating Chat Button */
        .chat-btn {
            position: fixed;
            bottom: 30px;
            right: 30px;
            background-color: #0078FF; /* Messenger blue */
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
            text-decoration: none;
            transform: scale(1.1);
        }

        @media (max-width: 768px) {
            .card-container {
                grid-template-columns: 1fr;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mt-4">

        <!-- Welcome Section -->
        <div class="welcome-section">
            <h1>Welcome, <%= Session["Name"] ?? "Valued Client" %>!</h1>
            <p>Manage your bookings, check payments, and stay connected with us.</p>
        </div>

        <!-- Card Action Section -->
        <div class="card-container">

            <!-- Book a Service -->
            <div class="card-box">
                <i class="fas fa-calendar-plus"></i>
                <h5>Book a Service</h5>
                <p>Schedule a new pest control service easily in a few clicks.</p>
                <a href="BookService.aspx" class="btn-action">Book Now</a>
            </div>

            <!-- My Bookings -->
            <div class="card-box">
                <i class="fas fa-clipboard-list"></i>
                <h5>My Bookings</h5>
                <p>Track your upcoming and completed service bookings.</p>
                <a href="MyBookings.aspx" class="btn-action">View Bookings</a>
            </div>

                        <!-- Our Contract -->
            <div class="card-box">
                <i class="fas fa-file-contract"></i>
                <h5>Our Contract</h5>
                <p>Review your current pest control contract and service terms.</p>
                <a href="OurContract.aspx" class="btn-action">View Contract</a>
            </div>


            <!-- Pending Payments -->
            <div class="card-box">
                <i class="fas fa-wallet"></i>
                <h5>Pending Payments</h5>
                <p>Check and settle your pending balances easily.</p>
                <a href="Payment.aspx" class="btn-action">Check Payments</a>
            </div>

            <!-- My Profile -->
          

        </div>

        <!-- Floating Messenger Chat Button -->
        <a href="ChatWithAdmin.aspx" class="chat-btn" title="Chat with Admin">
            <i class="fab fa-facebook-messenger"></i>
        </a>

    </div>
</asp:Content>