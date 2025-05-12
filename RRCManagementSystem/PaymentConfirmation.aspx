<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PaymentConfirmation.aspx.cs" Inherits="RRCManagementSystem.PaymentConfirmation" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Payment Confirmation</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            padding: 50px;
            background-color: #f4f6f8;
            text-align: center;
        }

        .message-box {
            margin: auto;
            background: #fff;
            padding: 30px;
            border-radius: 8px;
            max-width: 600px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.15);
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="message-box">
            <asp:Literal ID="litStatus" runat="server" />
            <asp:Literal ID="litResponse" runat="server" />
            <br /><br />
            <a href="~/Payment.aspx">⬅ Back to Payment Page</a>
        </div>
    </form>
</body>
</html>
