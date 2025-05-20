﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="RRCManagementSystem.Login" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Login - RRC Management System</title>
    <link href="https://fonts.googleapis.com/css2?family=Poppins:wght@400;600&display=swap" rel="stylesheet" />
    <script src="https://www.google.com/recaptcha/api.js" async defer></script>

<style>
    body, html {
        margin: 0;
        padding: 0;
        height: 100%;
        font-family: 'Poppins', sans-serif;
        color: #333;
    }

    body {
        background: url('images/bg.jpg') no-repeat center center fixed;
        background-size: cover;
        display: flex;
        justify-content: center;   /* Horizontal center */
        align-items: center;       /* Vertical center */
        min-height: 100vh;
    }

    .login-container {
        background: #ffffff; /* Solid white background */
        border-radius: 16px;
        box-shadow: 0 8px 32px rgba(0, 0, 0, 0.2);
        width: 400px;
        padding: 40px 30px;
        display: flex;
        flex-direction: column;
        align-items: center;
        text-align: center;
        border: 1px solid #ddd;
        transition: transform 0.3s ease;
    }

    .login-container:hover {
        transform: translateY(-5px);
    }

    .logo {
        width: 200px;
        height: auto;
        margin-bottom: 20px;
    }

    h2 {
        color: #333;
        font-size: 26px;
        margin-bottom: 25px;
    }

    .input {
        width: 100%;
        padding: 14px 12px;
        margin-bottom: 20px;
        border-radius: 8px;
        border: 1px solid #ccc;
        font-size: 15px;
        background: #f9f9f9;
        color: #333;
    }

    .input::placeholder {
        color: #999;
    }

    .input:focus {
        background: #fff;
        border-color: #007bff;
        outline: none;
    }

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

    .links {
        margin-top: 20px;
    }

    .links a {
        color: #007bff;
        font-size: 14px;
        text-decoration: none;
    }

    .message {
        margin-top: 15px;
        font-size: 14px;
        color: #ff4d4f;
    }

    .g-recaptcha {
        margin-bottom: 20px;
    }

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
        <asp:ScriptManager ID="ScriptManager1" runat="server" />
        <div class="login-container">
            <img src="images/logorrc.png" alt="RRC Logo" class="logo" />
            <h2>Login</h2>

            <!-- Email -->
            <asp:TextBox ID="txtEmail" runat="server" CssClass="input" placeholder="Email" TextMode="Email" AutoCompleteType="Disabled"></asp:TextBox>

            <!-- Password -->
            <asp:TextBox ID="txtPassword" runat="server" CssClass="input" placeholder="Password" TextMode="Password" AutoCompleteType="Disabled"></asp:TextBox>

            <!-- CAPTCHA Panel (initially hidden in code-behind) -->
            <asp:Panel ID="pnlCaptcha" runat="server" Visible="false">
                <div class="g-recaptcha" data-sitekey="6LdFpz4rAAAAAFHN9JRMbSs2zRVZastQVd6GHIpz"></div>
            </asp:Panel>

            <!-- Login Button -->
            <asp:Button ID="btnLogin" runat="server" CssClass="btn-login" Text="Login" 
                        OnClick="btnLogin_Click" UseSubmitBehavior="false" />

            <!-- Forgot Link -->
            <div class="links">
                <a href="ForgotPassword.aspx">Forgot Password?</a>
            </div>

            <!-- Message -->
            <asp:Label ID="lblMessage" runat="server" CssClass="message"></asp:Label>
        </div>
    </form>
</body>
</html>