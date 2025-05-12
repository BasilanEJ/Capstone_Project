<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Enable2FA.aspx.cs" Inherits="RRCManagementSystem.Enable2FA" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Enable 2FA - RRC Management System</title>

    <!-- Poppins Font -->
    <link href="https://fonts.googleapis.com/css2?family=Poppins:wght@400;600&display=swap" rel="stylesheet" />

    <style>
        /* Reset and Base */
        body, html {
            margin: 0;
            padding: 0;
            height: 100%;
            font-family: 'Poppins', sans-serif;
            color: #333;
        }

        /* Background */
        body {
            background: url('images/banner-1-01.jpg') no-repeat center center fixed;
            background-size: cover;
            display: flex;
            justify-content: center;
            align-items: center;
        }

        /* Glass container */
        .enable2fa-container {
            background: rgba(255, 255, 255, 0.1);
            backdrop-filter: blur(15px);
            -webkit-backdrop-filter: blur(15px);
            border-radius: 16px;
            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.37);
            width: 350px;
            padding: 40px 30px;
            display: flex;
            flex-direction: column;
            align-items: center;
            text-align: center;
            border: 1px solid rgba(255, 255, 255, 0.18);
        }

        /* Title */
        h2 {
            color: #ffffff;
            font-size: 26px;
            margin-bottom: 20px;
        }

        /* QR Image */
        .qr-img {
            width: 100%;
            max-width: 200px;
            margin: 20px 0;
        }

        /* Input */
        .input {
            width: 100%;
            padding: 14px 12px;
            margin-bottom: 20px;
            border-radius: 8px;
            border: none;
            font-size: 15px;
            outline: none;
            background: rgba(255, 255, 255, 0.2);
            color: #ffffff;
            transition: background-color 0.3s ease;
        }

        .input::placeholder {
            color: #e0e0e0;
        }

        .input:focus {
            background: rgba(255, 255, 255, 0.3);
        }

        /* Button */
        .btn-verify {
            width: 100%;
            padding: 14px;
            background-color: #007bff;
            color: #fff;
            font-size: 16px;
            border: none;
            border-radius: 8px;
            cursor: pointer;
            transition: background-color 0.3s ease, transform 0.2s ease;
        }

        .btn-verify:hover {
            background-color: #0056b3;
            transform: translateY(-2px);
        }

        /* Message */
        .message {
            margin-top: 15px;
            font-size: 14px;
            color: #ff4d4f;
        }

        /* Responsive */
        @media screen and (max-width: 480px) {
            .enable2fa-container {
                width: 90%;
                padding: 30px 20px;
            }
        }
    </style>
</head>

<body>
    <form id="form1" runat="server">
        <div class="enable2fa-container">
            <h2>Enable 2FA</h2>

            <asp:Label ID="lblInstruction" runat="server" Text="Scan the QR code using Google Authenticator." ForeColor="White"></asp:Label>

            <asp:Image ID="imgQRCode" runat="server" CssClass="qr-img" />

            <asp:TextBox ID="txtCode" runat="server" CssClass="input" placeholder="Enter Code from App"></asp:TextBox>

            <asp:Button ID="btnVerify" runat="server" Text="Verify & Enable 2FA" CssClass="btn-verify" OnClick="btnVerify_Click" />

            <asp:Label ID="lblMessage" runat="server" CssClass="message" />
        </div>
    </form>
</body>
</html>
