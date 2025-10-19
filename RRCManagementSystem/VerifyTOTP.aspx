<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="VerifyTOTP.aspx.cs" Inherits="RRCManagementSystem.VerifyTOTP" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no" />
    <meta charset="UTF-8" />
    <link rel="icon" type="image/png" href="~/Images/rrc-logo.jpg" />
    <title>2FA Verification - RRC Management System</title>

    <link href="https://fonts.googleapis.com/css2?family=Poppins:wght@400;500;600;700&display=swap" rel="stylesheet" />
    <script src="https://www.google.com/recaptcha/api.js" async defer></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.2/css/all.min.css" />

    <style>
        /*
         * INLINE STYLES FROM Login.aspx - This ensures visual consistency
         * (Only the OTP specific styles are new)
         */
        * {
            box-sizing: border-box;
            margin: 0;
            padding: 0;
        }

        body, html {
            height: 100%;
            font-family: 'Poppins', sans-serif;
            color: #333;
            -webkit-font-smoothing: antialiased;
            -moz-osx-font-smoothing: grayscale;
        }

        body {
            background: linear-gradient(135deg, rgba(0, 123, 255, 0.1) 0%, rgba(0, 86, 179, 0.2) 100%),
                        url('images/logo.jpg') no-repeat center center fixed;
            background-size: cover;
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
            padding: 15px;
            overflow-x: hidden;
        }

        .login-container {
            background: rgba(255, 255, 255, 0.98);
            backdrop-filter: blur(10px);
            border-radius: 20px;
            box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
            width: 100%;
            max-width: 480px; /* Kept max-width from Login.aspx */
            padding: 50px 40px;
            display: flex;
            flex-direction: column;
            align-items: center;
            text-align: center;
            border: 1px solid rgba(255, 255, 255, 0.3);
            transition: all 0.4s ease;
            animation: fadeInUp 0.6s ease;
            margin: 0 auto;
        }

        @keyframes fadeInUp {
            from { opacity: 0; transform: translateY(30px); }
            to { opacity: 1; transform: translateY(0); }
        }

        .login-container:hover {
            transform: translateY(-8px);
            box-shadow: 0 25px 70px rgba(0, 0, 0, 0.35);
        }

        .logo {
            width: 100%;
            max-width: 200px;
            height: auto;
            margin-bottom: 20px;
            filter: drop-shadow(0 4px 8px rgba(0, 0, 0, 0.1));
            transition: transform 0.3s ease;
        }

        .logo:hover {
            transform: scale(1.05);
        }

        h2 {
            color: #1a1a1a;
            font-size: clamp(26px, 6vw, 32px);
            margin-bottom: 10px;
            font-weight: 700;
            letter-spacing: -0.5px;
        }
        
        /* New style for the subtitle/instruction text */
        .subtitle {
            color: #666;
            font-size: clamp(13px, 3.5vw, 15px);
            margin-bottom: 30px;
            font-weight: 400;
        }

        /* Re-used button style for consistency */
        .btn-login, .btn-secondary-action {
            width: 100%;
            padding: 15px;
            font-size: clamp(15px, 4vw, 17px);
            font-weight: 600;
            color: #ffffff;
            border: none;
            border-radius: 12px;
            cursor: pointer;
            transition: all 0.3s ease;
            margin-top: 10px;
        }

        .btn-login { /* Primary (Verify) button */
            background: linear-gradient(135deg, #28a745 0%, #1e7e34 100%); /* Changed to Green for verification */
            box-shadow: 0 4px 15px rgba(40, 167, 69, 0.3);
        }

        .btn-login:hover {
            background: linear-gradient(135deg, #1e7e34 0%, #175e2a 100%);
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(40, 167, 69, 0.4);
        }

        .btn-secondary-action { /* Secondary (Email Code) button */
            background: linear-gradient(135deg, #007bff 0%, #0056b3 100%); /* Blue from Login button */
            box-shadow: 0 4px 15px rgba(0, 123, 255, 0.3);
            margin-top: 0; /* Adjust spacing */
            margin-bottom: 20px;
        }

        .btn-secondary-action:hover {
            background: linear-gradient(135deg, #0056b3 0%, #004494 100%);
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(0, 123, 255, 0.4);
        }

        .btn-login:active, .btn-secondary-action:active {
            transform: translateY(0);
        }

        /* New OTP specific styles */
        .otp-inputs {
            display: flex;
            gap: 10px; /* Reduced gap slightly */
            justify-content: center;
            margin-bottom: 30px;
        }

        .otp-box {
            width: 50px; /* Adjusted size for consistency */
            height: 60px; /* Adjusted size for consistency */
            text-align: center;
            font-size: 24px;
            font-weight: 600;
            border: 2px solid #e0e0e0;
            border-radius: 12px;
            background: #f8f9fa;
            color: #333;
            transition: all 0.3s ease;
            font-family: 'Poppins', sans-serif;
            outline: none;
        }

        .otp-box:focus {
            border-color: #007bff;
            background: #ffffff;
            box-shadow: 0 0 0 4px rgba(0, 123, 255, 0.1);
            transform: translateY(-2px);
        }

        .divider {
            position: relative;
            width: 100%;
            text-align: center;
            margin: 20px 0;
        }

        .divider::before {
            content: '';
            position: absolute;
            top: 50%;
            left: 0;
            right: 0;
            height: 1px;
            background-color: #e0e0e0;
            z-index: 1;
        }

        .divider span {
            background-color: #fff;
            padding: 0 10px;
            color: #999;
            font-size: 14px;
            font-weight: 500;
            position: relative;
            z-index: 2;
        }

        /* Message Styles */
        .info-message {
            font-size: clamp(12px, 3.5vw, 13px);
            color: #007bff;
            padding: 10px 0;
            line-height: 1.6;
        }
        
        .message {
            font-size: clamp(12px, 3.5vw, 13px);
            color: #dc3545;
            padding: 10px 0;
            line-height: 1.6;
        }

        /* Responsive adjustments from Login.aspx (simplified) */
        @media screen and (max-width: 480px) {
            .login-container {
                padding: 30px 20px;
            }
            .otp-inputs {
                gap: 8px;
            }
            .otp-box {
                width: 45px;
                height: 55px;
                font-size: 22px;
            }
            .logo {
                max-width: 150px;
            }
        }
    </style>
</head>

<body onload="initScripts()">
    <form id="form1" runat="server" autocomplete="off">
        <asp:ScriptManager ID="ScriptManager1" runat="server" />
        <div class="login-container">
            <img src="images/logorrc.png" alt="RRC Logo" class="logo" />
            <h2>Two-Factor Verification</h2>
            <p class="subtitle">Enter the 6-digit code from your authenticator app.</p>

            <asp:HiddenField ID="txtTOTP" runat="server" />

            <div id="otp-inputs" class="otp-inputs">
                <input type="text" maxlength="1" class="otp-box" oninput="moveNext(this, 0)" onkeydown="handleBackspace(event, 0)" onpaste="return false;" />
                <input type="text" maxlength="1" class="otp-box" oninput="moveNext(this, 1)" onkeydown="handleBackspace(event, 1)" onpaste="return false;" />
                <input type="text" maxlength="1" class="otp-box" oninput="moveNext(this, 2)" onkeydown="handleBackspace(event, 2)" onpaste="return false;" />
                <input type="text" maxlength="1" class="otp-box" oninput="moveNext(this, 3)" onkeydown="handleBackspace(event, 3)" onpaste="return false;" />
                <input type="text" maxlength="1" class="otp-box" oninput="moveNext(this, 4)" onkeydown="handleBackspace(event, 4)" onpaste="return false;" />
                <input type="text" maxlength="1" class="otp-box" oninput="moveNext(this, 5)" onkeydown="handleBackspace(event, 5)" onpaste="return false;" />
            </div>

            <asp:Button ID="btnVerifyTOTP" runat="server" Text="Verify Code" CssClass="btn-login" OnClick="btnVerifyTOTP_Click" />

            <div class="divider">
                <span>OR</span>
            </div>

            <asp:Button ID="btnSendEmailCode" runat="server" Text="Send Code to Email" CssClass="btn-secondary-action" OnClick="btnSendEmailCode_Click" />

            <asp:Label ID="lblInfo" runat="server" CssClass="info-message"></asp:Label>
            <asp:Label ID="lblMessage" runat="server" CssClass="message"></asp:Label>

            <asp:Panel ID="pnlCaptcha" runat="server" Visible="false" CssClass="captcha-container">
                <div class="g-recaptcha" data-sitekey="6Ld6VrcrAAAAAGnZnUl3beqIS2JViuw5O5s0WlBh"></div>
            </asp:Panel>
        </div>
    </form>

    <script>
        const boxes = document.querySelectorAll('.otp-box');
        const hiddenField = document.getElementById('<%= txtTOTP.ClientID %>');

        // Move to next box automatically when typing
        function moveNext(input, index) {
            input.value = input.value.replace(/[^0-9]/g, '');
            if (input.value && index < boxes.length - 1) {
                boxes[index + 1].focus();
            }
            updateHiddenField();
        }

        // Move back on backspace
        function handleBackspace(e, index) {
            if (e.key === "Backspace" && !boxes[index].value && index > 0) {
                boxes[index - 1].focus();
            }
        }

        // Combine values into hidden field
        function updateHiddenField() {
            let val = '';
            boxes.forEach(box => val += box.value);
            hiddenField.value = val;
        }

        // Autofocus first box on page load and handle iOS zoom
        function initScripts() {
            boxes[0].focus();
            addMaximumScaleToMetaViewport();
        }

        // Prevent zoom on iOS (copied from Login.aspx)
        const addMaximumScaleToMetaViewport = () => {
            const el = document.querySelector('meta[name=viewport]');
            if (el !== null) {
                let content = el.getAttribute('content');
                let re = /maximum\-scale=[0-9\.]+/g;
                if (re.test(content)) {
                    content = content.replace(re, 'maximum-scale=1.0');
                } else {
                    content = [content, 'maximum-scale=1.0'].join(', ')
                }
                el.setAttribute('content', content);
            }
        };

        document.addEventListener("DOMContentLoaded", initScripts);

        // Re-initialize after AJAX postback
        if (typeof Sys !== 'undefined') {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(initScripts);
        }
    </script>
</body>
</html>