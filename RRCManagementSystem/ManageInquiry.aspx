<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ManageInquiry.aspx.cs" Inherits="RRCManagementSystem.ManageInquiry" %>

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

        .links-container {
            display: flex;
            justify-content: center;
            flex-wrap: wrap;
            gap: 30px;
            padding: 40px 20px;
        }

        .dash-card {
            background-color: #ffffff;
            border: 1px solid #dfe6ed;
            border-radius: 12px;
            width: 260px;
            height: 180px;
            padding: 20px;
            text-align: center;
            box-shadow: 0 2px 8px rgba(0,0,0,0.08);
            transition: all 0.3s ease;
            text-decoration: none;
            color: #2c3e50;
            display: flex;
            flex-direction: column;
            justify-content: center;
            align-items: center;
        }

        .dash-card:hover {
            box-shadow: 0 4px 14px rgba(0,0,0,0.12);
            transform: translateY(-5px);
            background-color: #f9fbff;
        }

        .dash-card i {
            font-size: 36px;
            margin-bottom: 12px;
            color: #2980b9;
        }

        .dash-card-title {
            font-size: 18px;
            font-weight: 600;
        }
    </style>

    <h2 class="page-title">📨 Manage Inquiries</h2>

    <div class="links-container">
        <a href="AllInquiry.aspx" class="dash-card">
            <i class="fas fa-list"></i>
            <div class="dash-card-title">All Inquiries</div>
        </a>

        <a href="CreateInquiry.aspx" class="dash-card">
            <i class="fas fa-plus-circle"></i>
            <div class="dash-card-title">Create Inquiry</div>
        </a>

        <a href="ViewQuotation.aspx" class="dash-card">
  <i class="fas fa-file-invoice"></i>
  <div class="dash-card-title">View Quotations</div>
</a>



    </div>
</asp:Content>