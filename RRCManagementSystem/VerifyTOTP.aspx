<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="VerifyTOTP.aspx.cs" Inherits="RRCManagementSystem.VerifyTOTP" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>2FA Verification - RRC Management System</title>
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
            background: url('images/pestlogo.jpg') no-repeat center center fixed;
            background-size: cover;
            display: flex;
            justify-content: center;
            align-items: center;
        }

        .login-container {
            background: rgba(255, 255, 255, 0.1);
            backdrop-filter: blur(15px);
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

        .logo {
            width: 80px;
            margin-bottom: 20px;
        }

        h2 {
            color: #ffffff;
            font-size: 24px;
            margin-bottom: 25px;
        }

        #otp-inputs {
            display: flex;
            gap: 10px;
            justify-content: center;
            margin-bottom: 20px;
        }

        .otp-box {
            width: 40px;
            height: 50px;
            text-align: center;
            font-size: 22px;
            border-radius: 8px;
            border: none;
            background: rgba(255, 255, 255, 0.2);
            color: white;
            outline: none;
        }

        .otp-box:focus {
            background: rgba(255, 255, 255, 0.3);
        }

        .g-recaptcha {
            margin-bottom: 15px;
        }

        .btn-login {
            width: 100%;
            padding: 14px;
            background-color: #28a745;
            color: #fff;
            font-size: 16px;
            border: none;
            border-radius: 8px;
            cursor: pointer;
        }

        .btn-login:hover {
            background-color: #218838;
        }

        .message {
            margin-top: 15px;
            font-size: 14px;
            color: white;
        }

        @media screen and (max-width: 480px) {
            .login-container {
                width: 90%;
            }
        }
    </style>
</head>

<body>
    <form id="form1" runat="server" autocomplete="off">
        <div class="login-container">
            <img src="images/logorrc.png" alt="RRC Logo" class="logo" />
            <h2>Verify Code</h2>

            <asp:HiddenField ID="txtTOTP" runat="server" />

            <div id="otp-inputs">
                <input type="text" maxlength="1" class="otp-box" oninput="moveNext(this, 0)" onkeydown="handleBackspace(event, 0)" onpaste="return false;" />
                <input type="text" maxlength="1" class="otp-box" oninput="moveNext(this, 1)" onkeydown="handleBackspace(event, 1)" onpaste="return false;" />
                <input type="text" maxlength="1" class="otp-box" oninput="moveNext(this, 2)" onkeydown="handleBackspace(event, 2)" onpaste="return false;" />
                <input type="text" maxlength="1" class="otp-box" oninput="moveNext(this, 3)" onkeydown="handleBackspace(event, 3)" onpaste="return false;" />
                <input type="text" maxlength="1" class="otp-box" oninput="moveNext(this, 4)" onkeydown="handleBackspace(event, 4)" onpaste="return false;" />
                <input type="text" maxlength="1" class="otp-box" oninput="moveNext(this, 5)" onkeydown="handleBackspace(event, 5)" onpaste="return false;" />
            </div>

            <!-- Google reCAPTCHA -->
              <!-- CAPTCHA Panel (initially hidden in code-behind) -->
    <asp:Panel ID="pnlCaptcha" runat="server" Visible="false">
        <div class="g-recaptcha" data-sitekey="6LdFpz4rAAAAAFHN9JRMbSs2zRVZastQVd6GHIpz"></div>
    </asp:Panel>

            <asp:Button ID="btnVerifyTOTP" runat="server" Text="Verify" CssClass="btn-login" OnClick="btnVerifyTOTP_Click" />
            <asp:Label ID="lblMessage" runat="server" CssClass="message" />
        </div>
    </form>

    <script>
        const boxes = document.querySelectorAll('.otp-box');
        const hiddenField = document.getElementById('<%= txtTOTP.ClientID %>');

        function moveNext(input, index) {
            input.value = input.value.replace(/[^0-9]/g, '');
            if (input.value && index < boxes.length - 1) {
                boxes[index + 1].focus();
            }
            updateHiddenField();
        }

        function handleBackspace(e, index) {
            if (e.key === "Backspace" && !boxes[index].value && index > 0) {
                boxes[index - 1].focus();
            }
        }

        function updateHiddenField() {
            let val = '';
            boxes.forEach(box => val += box.value);
            hiddenField.value = val;
        }

        window.onload = function () {
            boxes[0].focus();
        };
    </script>
</body>
</html>
