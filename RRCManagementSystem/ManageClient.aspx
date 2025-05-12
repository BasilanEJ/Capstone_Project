<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ManageClient.aspx.cs" Inherits="RRCManagementSystem.ManageClient" %>


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

        .client-links-container {
            display: flex;
            justify-content: center;
            flex-wrap: wrap;
            gap: 20px;
            padding: 30px 20px;
        }

        .client-card {
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

        .client-card:hover {
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
            transform: translateY(-3px);
            background-color: #f9fbff;
        }

        .client-card i {
            font-size: 28px;
            margin-bottom: 10px;
            color: #2980b9;
        }

        .client-card-title {
            font-size: 16px;
            font-weight: 600;
        }
    </style>

    <h2 class="page-title">👤 Manage Clients</h2>

    <div class="client-links-container">
        <a href="ClientsProfiles.aspx" class="client-card">
            <i class="fas fa-user"></i>
            <div class="client-card-title">View Client Profiles</div>
        </a>
       <!-- <a href="ClientsHistory.aspx" class="client-card">
            <i class="fas fa-history"></i>
            <div class="client-card-title">Clients History / Transactions</div>
        </a>
        <a href="SendNotifications.aspx" class="client-card">
            <i class="fas fa-bell"></i>
            <div class="client-card-title">Send Notifications</div>
        </a> -->
        <a href="ManageContract.aspx" class="client-card">
            <i class="fas fa-file-signature"></i>
            <div class="client-card-title">Manage Contracts</div>
        </a>
    </div>
</asp:Content>