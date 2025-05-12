    <%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="RRCManagementSystem.Login" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Login - RRC Management System</title>

    <!-- Modern Font (Optional) -->
    <link href="https://fonts.googleapis.com/css2?family=Poppins:wght@400;600&display=swap" rel="stylesheet" />

    <style>
        /* Reset and Base Styles */
        body, html {
            margin: 0;
            padding: 0;
            height: 100%;
            font-family: 'Poppins', sans-serif;
            color: #333;
        }

        /* Background Image */
        body {
            background: url('images/banner-1-01.jpg') no-repeat center center fixed;
            background-size: cover;
            display: flex;
            justify-content: center;
            align-items: center;
        }

        /* Glassmorphism Container */
        .login-container {
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
            transition: transform 0.3s ease;
            border: 1px solid rgba(255, 255, 255, 0.18);
        }

        .login-container:hover {
            transform: translateY(-5px);
        }

        /* Logo */
        .logo {
            width: 80px;
            height: auto;
            margin-bottom: 20px;
        }

        /* Heading */
        h2 {
            color: #ffffff;
            font-size: 26px;
            margin-bottom: 25px;
        }

        /* Input Fields */
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

        /* Login Button */
        .btn-login {
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

        .btn-login:hover {
            background-color: #0056b3;
            transform: translateY(-2px);
        }

        /* Links */
        .links {
            margin-top: 20px;
        }

        .links a {
            color: #ffffff;
            font-size: 14px;
            text-decoration: none;
            transition: color 0.3s ease;
        }

        .links a:hover {
            color: #d1d1d1;
        }

        /* Error/Message Label */
        .message {
            margin-top: 15px;
            font-size: 14px;
            color: #ff4d4f;
        }

        /* Responsive */
        @media screen and (max-width: 480px) {
            .login-container {
                width: 90%;
                padding: 30px 20px;
            }
        }
    </style>
</head>

<body>
    <form id="form1" runat="server">
        <div class="login-container">
            <!-- RRC Logo -->
            <img src="images/logorrc.png" alt="RRC Logo" class="logo" />

            <!-- Login Heading -->
            <h2>Login</h2>

            <!-- Email Input -->
            <asp:TextBox ID="txtEmail" runat="server" CssClass="input" placeholder="Email" TextMode="Email"></asp:TextBox>

            <!-- Password Input -->
            <asp:TextBox ID="txtPassword" runat="server" CssClass="input" placeholder="Password" TextMode="Password"></asp:TextBox>

            <!-- Login Button -->
            <asp:Button ID="btnLogin" runat="server" CssClass="btn-login" Text="Login" OnClick="btnLogin_Click" />

            <!-- Forgot Password Link -->
            <div class="links">
                <a href="ForgotPassword.aspx">Forgot Password?</a>
            </div>

            <!-- Message Label -->
            <asp:Label ID="lblMessage" runat="server" CssClass="message"></asp:Label>
        </div>
    </form>
</body>
</html>