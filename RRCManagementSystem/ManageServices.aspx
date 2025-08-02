<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ManageServices.aspx.cs" Inherits="RRCManagementSystem.ManageServices" %>


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

        .services-links-container {
            display: flex;
            justify-content: center;
            flex-wrap: wrap;
            gap: 20px;
            padding: 30px 20px;
        }

        .services-card {
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

        .services-card:hover {
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
            transform: translateY(-3px);
            background-color: #f9fbff;
        }

        .services-card i {
            font-size: 28px;
            margin-bottom: 10px;
            color: #2980b9;
        }

        .services-card-title {
            font-size: 16px;
            font-weight: 600;
        }
    </style>

    <h2 class="page-title">🪲 Manage Pest Control Services</h2>

    <div class="services-links-container">
           <a href="AddServices.aspx" class="services-card">
       <i class="fas fa-plus"></i>
       <div class="services-card-title">Add New Service</div>
   </a>
        <a href="ViewServices.aspx" class="services-card">
            <i class="fas fa-list"></i>
            <div class="services-card-title">View Services</div>
        </a>
    
    </div>
</asp:Content>