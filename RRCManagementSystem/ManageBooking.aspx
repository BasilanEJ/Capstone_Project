<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ManageBooking.aspx.cs" Inherits="RRCManagementSystem.ManageBooking" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f5f6fa;
            margin: 0;
            padding: 0;
            color: #333;
        }

        .page-title {
            text-align: center;
            font-size: 28px;
            font-weight: 600;
            color: #2c3e50;
            margin: 30px 0 20px;
            border-bottom: 2px solid #ccc;
            padding-bottom: 10px;
        }

        .booking-links-container {
            display: flex;
            justify-content: center;
            flex-wrap: wrap;
            gap: 20px;
            padding: 30px 20px;
        }

        .booking-card {
            background-color: #ffffff;
            border: 1px solid #e0e0e0;
            border-radius: 12px;
            width: 280px;
            padding: 20px;
            text-align: center;
            box-shadow: 0 2px 6px rgba(0,0,0,0.06);
            transition: all 0.3s ease;
            text-decoration: none;
            color: #2c3e50;
        }

        .booking-card:hover {
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
            transform: translateY(-3px);
            background-color: #f9fbff;
        }

        .booking-card i {
            font-size: 28px;
            margin-bottom: 10px;
            color: #2980b9;
        }

        .booking-card-title {
            font-size: 16px;
            font-weight: 600;
        }
    </style>

    <h2 class="page-title">📚 Manage Bookings</h2>

    <div class="booking-links-container">
        <a href="AllBooking.aspx" class="booking-card">
            <i class="fas fa-list"></i>
            <div class="booking-card-title">View All Bookings</div>
        </a>
        <a href="ApproveRejectBookings.aspx" class="booking-card">
            <i class="fas fa-check-circle"></i>
            <div class="booking-card-title">Approve / Reject Bookings</div>
        </a>

         <a href="ApproveRescheduleRequests.aspx" class="booking-card">
     <i class="fas fa-check-circle"></i>
     <div class="booking-card-title">Approve Reschedule Requests</div>
 </a>

        <a href="AllRescheduleBooking.aspx" class="booking-card">
    <i class="fas fa-calendar-alt"></i>
    <div class="booking-card-title">All Reschedule Booking</div>
</a>
        <a href="BookingHistory.aspx" class="booking-card">
            <i class="fas fa-history"></i>
            <div class="booking-card-title">View Booking History</div>
        </a>
    </div>
</asp:Content>