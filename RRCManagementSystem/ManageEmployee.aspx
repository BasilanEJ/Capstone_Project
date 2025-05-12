<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ManageEmployee.aspx.cs" Inherits="RRCManagementSystem.ManageEmployee" %>

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

        .employee-links-container {
            display: flex;
            justify-content: center;
            flex-wrap: wrap;
            gap: 20px;
            padding: 30px 20px;
        }

        .employee-card {
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

        .employee-card:hover {
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
            transform: translateY(-3px);
            background-color: #f9fbff;
        }

        .employee-card i {
            font-size: 28px;
            margin-bottom: 10px;
            color: #2980b9;
        }

        .employee-card-title {
            font-size: 16px;
            font-weight: 600;
        }
    </style>

    <h2 class="page-title">👥 Manage Employees</h2>

    <div class="employee-links-container">
        <a href="AllEmployee.aspx" class="employee-card">
            <i class="fas fa-list"></i>
            <div class="employee-card-title">View Employees</div>
        </a>
        <a href="AddEmployees.aspx" class="employee-card">
            <i class="fas fa-plus"></i>
            <div class="employee-card-title">Add New Employees</div>
        </a>
        <a href="EmployeeStatus.aspx" class="employee-card">
            <i class="fas fa-info-circle"></i>
            <div class="employee-card-title">Employee Status</div>
        </a>
        <a href="GroupEmployees.aspx" class="employee-card">
            <i class="fas fa-users"></i>
            <div class="employee-card-title">Employee's Team</div>
        </a>
        <a href="ViewTeams.aspx" class="employee-card">
            <i class="fas fa-user-group"></i>
            <div class="employee-card-title">View Teams</div>
        </a>
    </div>
</asp:Content>