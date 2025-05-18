<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ForgotPassword.aspx.cs" Inherits="RRCManagementSystem.ForgotPassword" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Forgot Password - RRC Management System</title>

    <!-- Modern Font -->
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
            background: url('images/pestlogo.jpg') no-repeat center center fixed;
            background-size: cover;
            display: flex;
            justify-content: center;
            align-items: center;
        }

        /* Glassmorphism Container */
        .forgot-container {
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

        .forgot-container:hover {
            transform: translateY(-5px);
        }

        /* Logo */
        .logo {
            width: 200px;
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

        /* Submit Button */
        .btn-submit {
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

        .btn-submit:hover {
            background-color: #0056b3;
            transform: translateY(-2px);
        }

        /* Message Label */
        .message {
            margin-top: 15px;
            font-size: 14px;
            color: #ff4d4f;
        }

        /* Back to Login Link */
        .back-to-login {
            margin-top: 20px;
        }

        .back-to-login a {
            color: #ffffff;
            font-size: 14px;
            text-decoration: none;
            transition: color 0.3s ease;
        }

        .back-to-login a:hover {
            color: #d1d1d1;
        }

        /* Responsive */
        @media screen and (max-width: 480px) {
            .forgot-container {
                width: 90%;
                padding: 30px 20px;
            }
        }
    </style>
</head>

<body>
    <form id="form1" runat="server">
        <div class="forgot-container">
            <!-- Logo -->
            <img src="images/pestlogo.jpg" alt="RRC Logo" class="logo" />

            <!-- Forgot Password Heading -->
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
                CssClass="btn-submit"
                OnClick="btnSubmit_Click" />

            <!-- Message Label -->
            <asp:Label
                ID="lblMessage"
                runat="server"
                CssClass="message" />

            <!-- Back to Login Link -->
            <div class="back-to-login">
                <a href="Login.aspx">Back to Login</a>
            </div>
        </div>
    </form>
</body>
</html>