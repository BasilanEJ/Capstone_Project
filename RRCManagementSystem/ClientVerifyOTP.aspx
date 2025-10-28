<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ClientVerifyOTP.aspx.cs" Inherits="RRCManagementSystem.ClientVerifyOTP" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no" />
    <meta charset="UTF-8" />
    <link rel="icon" type="image/png" href="~/Images/rrc-logo.jpg" />
    <title>Device Verification - RRC Management System</title>

    <link href="https://fonts.googleapis.com/css2?family=Poppins:wght@400;500;600;700&display=swap" rel="stylesheet" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.2/css/all.min.css" />

    <style>
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

        .verify-container {
            background: rgba(255, 255, 255, 0.98);
            backdrop-filter: blur(10px);
            border-radius: 20px;
            box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
            width: 100%;
            max-width: 480px;
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
            from {
                opacity: 0;
                transform: translateY(30px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }

        .verify-container:hover {
            transform: translateY(-8px);
            box-shadow: 0 25px 70px rgba(0, 0, 0, 0.35);
        }

        .logo {
            width: 100%;
            max-width: 180px;
            height: auto;
            margin-bottom: 20px;
            filter: drop-shadow(0 4px 8px rgba(0, 0, 0, 0.1));
            transition: transform 0.3s ease;
        }

        .logo:hover {
            transform: scale(1.05);
        }

        .icon-shield {
            font-size: 60px;
            color: #007bff;
            margin-bottom: 20px;
            animation: pulse 2s infinite;
        }

        @keyframes pulse {
            0%, 100% { transform: scale(1); }
            50% { transform: scale(1.05); }
        }

        h2 {
            color: #1a1a1a;
            font-size: clamp(24px, 6vw, 28px);
            margin-bottom: 10px;
            font-weight: 700;
            letter-spacing: -0.5px;
        }

        .subtitle {
            color: #666;
            font-size: clamp(13px, 3.5vw, 15px);
            margin-bottom: 20px;
            font-weight: 400;
            line-height: 1.5;
        }

        .info-box {
            background: linear-gradient(135deg, #e3f2fd 0%, #bbdefb 100%);
            border-left: 4px solid #007bff;
            padding: 15px;
            border-radius: 8px;
            margin: 20px 0;
            text-align: left;
            font-size: 14px;
        }

        .info-box i {
            color: #007bff;
            margin-right: 8px;
        }

        .form-content {
            width: 100%;
        }

        .input-group {
            margin-bottom: 20px;
            position: relative;
        }

        .input {
            width: 100%;
            padding: 14px 18px;
            font-size: clamp(16px, 4vw, 20px);
            border: 2px solid #e0e0e0;
            border-radius: 12px;
            background: #f8f9fa;
            color: #333;
            transition: all 0.3s ease;
            font-family: 'Courier New', monospace;
            font-weight: 600;
            letter-spacing: 8px;
            text-align: center;
            outline: none;
        }

        .input:focus {
            border-color: #007bff;
            background: #ffffff;
            box-shadow: 0 0 0 4px rgba(0, 123, 255, 0.1);
            transform: translateY(-2px);
        }

        .input::placeholder {
            color: #999;
            letter-spacing: normal;
            font-family: 'Poppins', sans-serif;
            font-weight: 400;
        }

        .timer {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 8px;
            margin: 15px 0;
            font-size: 14px;
            color: #666;
        }

        .timer.warning {
            color: #d32f2f;
            font-weight: 600;
        }

        .timer i {
            font-size: 16px;
        }

        .btn-verify {
            width: 100%;
            padding: 15px;
            font-size: clamp(15px, 4vw, 17px);
            font-weight: 600;
            color: #ffffff;
            background: linear-gradient(135deg, #007bff 0%, #0056b3 100%);
            border: none;
            border-radius: 12px;
            cursor: pointer;
            transition: all 0.3s ease;
            box-shadow: 0 4px 15px rgba(0, 123, 255, 0.3);
            margin-top: 10px;
        }

        .btn-verify:hover:not(:disabled) {
            background: linear-gradient(135deg, #0056b3 0%, #004494 100%);
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(0, 123, 255, 0.4);
        }

        .btn-verify:active:not(:disabled) {
            transform: translateY(0);
        }

        .btn-verify:disabled {
            opacity: 0.6;
            cursor: not-allowed;
        }

        .btn-secondary {
            width: 100%;
            padding: 12px;
            font-size: clamp(14px, 3.5vw, 15px);
            font-weight: 500;
            color: #007bff;
            background: transparent;
            border: 2px solid #007bff;
            border-radius: 12px;
            cursor: pointer;
            transition: all 0.3s ease;
            margin-top: 10px;
        }

        .btn-secondary:hover:not(:disabled) {
            background: rgba(0, 123, 255, 0.1);
            transform: translateY(-2px);
        }

        .btn-secondary:disabled {
            opacity: 0.5;
            cursor: not-allowed;
        }

        .links-container {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 10px;
            margin-top: 20px;
            flex-wrap: wrap;
        }

        .link {
            color: #007bff;
            text-decoration: none;
            font-size: clamp(13px, 3vw, 14px);
            font-weight: 500;
            transition: all 0.3s ease;
            display: inline-flex;
            align-items: center;
        }

        .link:hover {
            color: #0056b3;
            text-decoration: underline;
        }

        .link-separator {
            color: #ccc;
            font-size: 18px;
        }

        #messageContainer {
            margin-top: 20px;
            opacity: 0;
            transform: translateY(-10px);
            transition: all 0.3s ease;
        }

        #messageContainer.show {
            opacity: 1;
            transform: translateY(0);
        }

        .message {
            padding: 15px;
            border-radius: 10px;
            font-size: clamp(13px, 3.5vw, 14px);
            line-height: 1.5;
            text-align: left;
            display: block;
        }

        .message-error {
            background: linear-gradient(135deg, #ffebee 0%, #ffcdd2 100%);
            color: #c62828;
            border-left: 4px solid #d32f2f;
        }

        .message-success {
            background: linear-gradient(135deg, #e8f5e9 0%, #c8e6c9 100%);
            color: #2e7d32;
            border-left: 4px solid #4caf50;
        }

        .message-info {
            background: linear-gradient(135deg, #e3f2fd 0%, #bbdefb 100%);
            color: #1565c0;
            border-left: 4px solid #2196f3;
        }

        @media (max-width: 480px) {
            .verify-container {
                padding: 40px 25px;
            }

            .input {
                letter-spacing: 6px;
                font-size: 18px;
            }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" />

        <div class="verify-container">
            <img src="images/logorrc.png" alt="RRC Logo" class="logo" />
            <i class="fa-solid fa-shield-halved icon-shield"></i>
            <h2>Device Verification</h2>
            <p class="subtitle">We've sent a 6-digit code to your email address. Please enter it below to verify this device.</p>

            <div class="info-box">
                <i class="fa-solid fa-info-circle"></i>
                <strong>Why am I seeing this?</strong><br/>
                You're signing in from a new device. This extra step helps keep your account secure.
            </div>

            <asp:UpdatePanel ID="upVerify" runat="server">
                <ContentTemplate>
                    <asp:Panel ID="pnlVerify" runat="server" DefaultButton="btnVerify" CssClass="form-content">

                        <div class="input-group">
                            <asp:TextBox ID="txtOTPCode" runat="server"
                                CssClass="input"
                                placeholder="000000"
                                MaxLength="6"
                                AutoCompleteType="Disabled"
                                inputmode="numeric"
                                pattern="[0-9]*" />
                        </div>

                        <div class="timer" id="timerDisplay">
                            <i class="fa-solid fa-clock"></i>
                            <span id="timeRemaining">Loading...</span>
                        </div>

                        <asp:Button ID="btnVerify" runat="server"
                            CssClass="btn-verify"
                            Text="Verify Device"
                            OnClick="btnVerify_Click"
                            UseSubmitBehavior="false" />

                        <asp:Button ID="btnResend" runat="server"
                            CssClass="btn-secondary"
                            Text="Resend Code"
                            OnClick="btnResend_Click"
                            UseSubmitBehavior="false" />

                        <div class="links-container">
                            <a href="Login.aspx" class="link">
                                <i class="fa-solid fa-arrow-left" style="margin-right: 5px;"></i>Back to Login
                            </a>
                        </div>

                        <div id="messageContainer">
                            <asp:Label ID="lblMessage" runat="server" CssClass="message"></asp:Label>
                        </div>

                    </asp:Panel>
                </ContentTemplate>

                <Triggers>
                    <asp:PostBackTrigger ControlID="btnVerify" />
                    <asp:PostBackTrigger ControlID="btnResend" />
                </Triggers>
            </asp:UpdatePanel>
        </div>

        <asp:HiddenField ID="hiddenExpiresAt" runat="server" />

        <script>
            let countdownInterval;

            function initScripts() {
                const otpInput = document.getElementById('<%= txtOTPCode.ClientID %>');
                const msgContainer = document.getElementById('messageContainer');
                const msgLabel = msgContainer ? document.getElementById('<%= lblMessage.ClientID %>') : null;
                const expiresAtField = document.getElementById('<%= hiddenExpiresAt.ClientID %>');

                // Auto-focus OTP input
                if (otpInput) {
                    otpInput.focus();
                    
                    // Allow only numbers
                    otpInput.addEventListener('input', function(e) {
                        this.value = this.value.replace(/[^0-9]/g, '');
                    });
                }

                // Show/hide message container
                if (msgContainer && msgLabel) {
                    const text = msgLabel.innerText.trim();
                    if (text !== "") {
                        // Apply formatting
                        const formattedText = text.replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>');
                        msgLabel.innerHTML = formattedText;
                        
                        // Add appropriate class
                        msgLabel.classList.remove('message-error', 'message-success', 'message-info');
                        if (text.toLowerCase().includes('error') || text.toLowerCase().includes('failed') || text.toLowerCase().includes('invalid')) {
                            msgLabel.classList.add('message-error');
                        } else if (text.toLowerCase().includes('success') || text.toLowerCase().includes('verified')) {
                            msgLabel.classList.add('message-success');
                        } else {
                            msgLabel.classList.add('message-info');
                        }
                        
                        msgContainer.classList.add('show');
                    } else {
                        msgContainer.classList.remove('show');
                    }
                }

                // Start countdown timer
                if (expiresAtField && expiresAtField.value) {
                    startCountdown(expiresAtField.value);
                }
            }

            function startCountdown(expiresAtString) {
                // Clear any existing interval
                if (countdownInterval) {
                    clearInterval(countdownInterval);
                }

                const expiresAt = new Date(expiresAtString);
                const timerDisplay = document.getElementById('timerDisplay');
                const timeRemaining = document.getElementById('timeRemaining');

                function updateTimer() {
                    const now = new Date();
                    const diff = expiresAt - now;

                    if (diff <= 0) {
                        clearInterval(countdownInterval);
                        timeRemaining.textContent = 'Code expired';
                        timerDisplay.classList.add('warning');
                        return;
                    }

                    const minutes = Math.floor(diff / 60000);
                    const seconds = Math.floor((diff % 60000) / 1000);
                    
                    timeRemaining.textContent = `Expires in ${minutes}:${seconds.toString().padStart(2, '0')}`;
                    
                    // Warning at 2 minutes
                    if (diff <= 120000) {
                        timerDisplay.classList.add('warning');
                    }
                }

                updateTimer();
                countdownInterval = setInterval(updateTimer, 1000);
            }

            // Prevent zoom on iOS
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

            // Initialize on page load
            document.addEventListener("DOMContentLoaded", function () {
                addMaximumScaleToMetaViewport();
                initScripts();
            });

            // Re-initialize after AJAX postback
            if (typeof Sys !== 'undefined') {
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(initScripts);
            }
        </script>

    </form>
</body>
</html>
