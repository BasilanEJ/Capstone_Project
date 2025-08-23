<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="SalesAndTransactions.aspx.cs" Inherits="RRCManagementSystem.SalesAndTransactions" %>

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

        .sales-links-container {
            display: flex;
            justify-content: center;
            flex-wrap: wrap;
            gap: 20px;
            padding: 30px 20px;
        }

        .sales-card {
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

        .sales-card:hover {
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
            transform: translateY(-3px);
            background-color: #f9fbff;
        }

        .sales-card i {
            font-size: 28px;
            margin-bottom: 10px;
            color: #27ae60;
        }

        .sales-card-title {
            font-size: 16px;
            font-weight: 600;
        }
    </style>

    <h2 class="page-title">💰 Sales & Transactions</h2>

   <div class="sales-links-container">
   <!--  <a href="ViewSales.aspx" class="sales-card">
         <i class="fas fa-chart-line"></i>
         <div class="sales-card-title">View Sales Summary</div>
     </a> -->

   <a href="TransactionHistory.aspx" class="sales-card">
       <i class="fas fa-receipt"></i>
       <div class="sales-card-title">Transaction History</div>
   </a>

   <a href="ManagePayment.aspx" class="sales-card">
       <i class="fas fa-credit-card"></i>
       <div class="sales-card-title">Manage Payment</div>
   </a>

   <!-- ✅ New link -->
   <a href="ViewPaymentBalance.aspx" class="sales-card">
       <i class="fas fa-wallet"></i>
       <div class="sales-card-title">View Payment Balances</div>
   </a>
</div>

</asp:Content>