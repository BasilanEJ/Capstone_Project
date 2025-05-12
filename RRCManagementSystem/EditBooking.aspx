<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="EditBooking.aspx.cs" Inherits="RRCManagementSystem.EditBooking" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        body {
            font-family: Arial, sans-serif;
            background-color: #f4f6f9;
            margin: 0;
            padding: 0;
        }

        .container {
            max-width: 700px;
            margin: 40px auto;
            padding: 20px;
        }

        .card {
            background-color: #fff;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
            overflow: hidden;
        }

        .card-header {
            background-color: #007bff;
            color: #fff;
            padding: 16px 20px;
            text-align: center;
            font-size: 20px;
            font-weight: bold;
        }

        .card-body {
            padding: 30px;
        }

        .form-group {
            margin-bottom: 20px;
        }

        label {
            font-weight: bold;
            margin-bottom: 8px;
            display: block;
            color: #333;
        }

        .form-control {
            width: 100%;
            padding: 12px 15px;
            border: 1px solid #ced4da;
            border-radius: 4px;
            font-size: 14px;
            box-sizing: border-box;
            transition: border-color 0.3s ease-in-out;
        }

        .form-control:focus {
            border-color: #007bff;
            outline: none;
        }

        .btn {
            display: inline-block;
            padding: 12px 20px;
            font-size: 16px;
            border-radius: 6px;
            cursor: pointer;
            text-align: center;
            text-decoration: none;
            border: none;
            transition: background-color 0.3s ease-in-out;
        }

        .btn-success {
            background-color: #28a745;
            color: #fff;
        }

        .btn-success:hover {
            background-color: #218838;
        }

        .btn-secondary {
            background-color: #6c757d;
            color: #fff;
        }

        .btn-secondary:hover {
            background-color: #5a6268;
        }

        .error-message {
            color: red;
            font-weight: bold;
            margin-bottom: 15px;
            display: block;
        }

        /* Responsive adjustments */
        @media (max-width: 768px) {
            .container {
                padding: 10px;
            }

            .btn {
                width: 100%;
                margin-bottom: 10px;
            }
        }
    </style>

    <!-- ✅ FORM START -->
    <div class="container">
        <div class="card shadow-sm">
            <div class="card-header">
                Edit Booking Status
            </div>

            <div class="card-body">
                <asp:Label ID="lblMessage" runat="server" CssClass="error-message" />

                <!-- Booking ID -->
                <div class="form-group">
                    <label>Booking ID:</label>
                    <asp:Label ID="lblBookingID" runat="server" CssClass="form-control" />
                </div>

                <!-- Client Name -->
                <div class="form-group">
                    <label>Client Name:</label>
                    <asp:TextBox ID="txtClientName" runat="server" CssClass="form-control" ReadOnly="true" />
                </div>

                <!-- Service Name -->
                <div class="form-group">
                    <label>Service Name:</label>
                    <asp:TextBox ID="txtServiceName" runat="server" CssClass="form-control" ReadOnly="true" />
                </div>

                <!-- Scheduled Date -->
                <div class="form-group">
                    <label>Scheduled Date:</label>
                    <asp:TextBox ID="txtScheduledDate" runat="server" CssClass="form-control" ReadOnly="true" />
                </div>

                <!-- Start Time -->
                <div class="form-group">
                    <label>Start Time:</label>
                    <asp:TextBox ID="txtStartTime" runat="server" CssClass="form-control" ReadOnly="true" />
                </div>

                <!-- Status Dropdown -->
                <div class="form-group">
                    <label>Status:</label>
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-control">
                        <asp:ListItem Text="Pending" Value="Pending" />
                        <asp:ListItem Text="Rejected" Value="Rejected" />
                        <asp:ListItem Text="Approved" Value="Approved" />
                        <asp:ListItem Text="In Progress" Value="InProgress" />
                        <asp:ListItem Text="Completed" Value="Completed" />
                        <asp:ListItem Text="Cancelled" Value="Cancelled" />
                    </asp:DropDownList>
                </div>

                <!-- Notes -->
                <div class="form-group">
                    <label>Notes:</label>
                    <asp:TextBox ID="txtNotes" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" ReadOnly="true" />
                </div>

                <!-- Buttons -->
                <div class="form-group text-center">
                    <asp:Button ID="btnSave" runat="server" Text="Save Status" CssClass="btn btn-success"
                        OnClientClick="return confirm('Are you sure you want to save changes?');"
                        OnClick="btnSave_Click" />

                    <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-secondary"
                        OnClientClick="window.location.href='AllBooking.aspx'; return false;" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>