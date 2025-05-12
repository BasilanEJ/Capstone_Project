<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Receipt.aspx.cs" Inherits="RRCManagementSystem.Receipt" %>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .receipt-container {
            max-width: 600px;
            margin: 40px auto;
            padding: 30px;
            background-color: #ffffff;
            border-radius: 10px;
            box-shadow: 0 0 15px rgba(0,0,0,0.2);
            font-family: 'Arial', sans-serif;
        }

        .receipt-header {
            text-align: center;
            margin-bottom: 30px;
        }

        .receipt-header h2 {
            margin-bottom: 5px;
        }

        .receipt-details {
            font-size: 16px;
            line-height: 1.6;
        }

        .receipt-details label {
            font-weight: bold;
        }

        .receipt-footer {
            margin-top: 30px;
            text-align: center;
        }

        .btn-print {
            margin-top: 20px;
            background-color: #007bff;
            color: white;
            padding: 10px 20px;
            border: none;
            font-weight: bold;
            border-radius: 6px;
            cursor: pointer;
        }

        .btn-print:hover {
            background-color: #0056b3;
        }
    </style>

    <div class="receipt-container" id="receiptContent">
        <div class="receipt-header">
            <h2>RRC Management System</h2>
            <p>Official Payment Receipt</p>
        </div>

      <div class="receipt-details">
            <p><label>Client:</label> <asp:Label ID="lblClientName" runat="server" /></p>
            <p><label>Amount Paid:</label> ₱<asp:Label ID="lblAmount" runat="server" /></p>
            <p><label>Payment Method:</label> <asp:Label ID="lblPaymentMethod" runat="server" /></p>
            <p><label>Transaction Date:</label> <asp:Label ID="lblTransactionDate" runat="server" /></p>
            <p><label>Remarks:</label> <asp:Label ID="lblRemarks" runat="server" /></p>
            <p><label>Transaction ID:</label> <asp:Label ID="lblTransactionID" runat="server" /></p>
        </div>

        <div class="receipt-footer">
            <button onclick="printReceipt()" class="btn-print">🖨️ Print Receipt</button>
        </div>
    </div>

    <script>
        function printReceipt() {
            var printContents = document.getElementById('receiptContent').innerHTML;
            var originalContents = document.body.innerHTML;
            document.body.innerHTML = printContents;
            window.print();
            document.body.innerHTML = originalContents;
        }
    </script>
</asp:Content>