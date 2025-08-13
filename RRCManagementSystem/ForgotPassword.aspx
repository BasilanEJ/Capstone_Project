<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ForgotPassword.aspx.cs" Inherits="RRCManagementSystem.ForgotPassword" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Forgot Password - RRC Management System</title>
    <link href="https://fonts.googleapis.com/css2?family=Poppins:wght@400;600&display=swap" rel="stylesheet" />

    <style>
        body, html {
            margin: 0;
            padding: 0;
            height: 100%;
            font-family: 'Poppins', sans-serif;
            color: #333;
        }

        body {
            background: url('images/logo.jpg') no-repeat center center fixed;
            background-size: cover;
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
        }

        .login-container {
            background: #ffffff;
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
            <img src="images/logorrc.png" alt="RRC Logo" class="logo" />
            <h2>Forgot Password</h2>

            <!-- Email Input -->
            <asp:TextBox
                ID="txtEmail"
                runat="server"
                CssClass="input"
                placeholder="Enter your email"
                TextMode="Email" />

            <!-- Submit Button -->
            <asp:Button
                ID="btnSubmit"
                runat="server"
                Text="Send OTP"
                CssClass="btn-login"
                OnClick="btnSubmit_Click" />

            <!-- Message Label -->
            <asp:Label
                ID="lblMessage"
                runat="server"
                CssClass="message" />

            <!-- Back to Login -->
            <div class="links">
                <a href="Login.aspx">Back to Login</a>
            </div>
        </div>
    </form>
</body>
</html>
