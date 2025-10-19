<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Enable2FA.aspx.cs" Inherits="RRCManagementSystem.Enable2FA" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no" />
    <meta charset="UTF-8" />
    <link rel="icon" type="image/png" href="~/Images/rrc-logo.jpg" />
    <title>Enable 2FA - RRC Management System</title>
    <link href="https://fonts.googleapis.com/css2?family=Poppins:wght@400;500;600;700&display=swap" rel="stylesheet" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.2/css/all.min.css" />

    <style>
        /*
         * CORE STYLES MATCHING Login.aspx
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
            /* Applied the blue gradient and stronger background styling from Login.aspx */
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
            /* Applied modern card styling from Login.aspx */
            background: rgba(255, 255, 255, 0.98);
            backdrop-filter: blur(10px);
            border-radius: 20px;
            box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
            width: 100%;
            max-width: 480px; /* Increased max-width slightly for better fit */
            padding: 50px 40px; /* Increased padding */
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
            transform: translateY(-8px); /* Stronger hover lift */
            box-shadow: 0 25px 70px rgba(0, 0, 0, 0.35); /* Stronger hover shadow */
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

        .instruction {
            color: #666;
            font-size: clamp(13px, 3.5vw, 15px);
            margin-bottom: 30px; /* Increased margin-bottom */
            font-weight: 400;
            line-height: 1.6;
        }

        .qr-img {
            width: 100%;
            max-width: 200px;
            height: auto;
            margin: 10px 0 25px; /* Adjusted margin */
            border: 5px solid #fff;
            box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
            border-radius: 8px;
        }

        .manual-link {
            color: #007bff;
            text-decoration: none;
            font-size: clamp(13px, 3.5vw, 14px);
            font-weight: 500;
            cursor: pointer;
            margin: 0 0 25px; /* Adjusted margin */
            display: inline-block;
            padding: 4px 8px;
            border-radius: 6px;
            transition: all 0.3s ease;
        }

        .manual-link:hover {
            color: #0056b3;
            background: rgba(0, 123, 255, 0.1);
            text-decoration: none;
        }

        .divider {
            width: 100%;
            text-align: center;
            margin: 25px 0 30px; /* Increased margin */
            position: relative;
        }

        .divider::before {
            content: '';
            position: absolute;
            left: 0;
            top: 50%;
            width: 100%;
            height: 1px;
            background: #e0e0e0; /* Lighter divider */
        }

        .divider span {
            background: #f8f9fa; /* Lighter background for text */
            padding: 0 10px;
            position: relative;
            color: #999;
            font-size: clamp(12px, 3.5vw, 14px);
            font-weight: 500;
        }

        #otp-inputs {
            display: flex;
            gap: 10px; /* Increased gap */
            justify-content: center;
            margin-bottom: 30px; /* Increased margin */
            width: 100%;
            max-width: 360px; /* Slightly wider */
        }

        .otp-box {
            width: 100%;
            max-width: 50px; /* Slightly wider box */
            height: 55px; /* Taller box */
            text-align: center;
            font-size: clamp(20px, 5vw, 24px);
            border-radius: 12px; /* More rounded */
            border: 2px solid #e0e0e0;
            background: #f8f9fa;
            color: #333;
            outline: none;
            flex: 1;
            transition: all 0.3s ease;
            font-weight: 600;
        }

        .otp-box:focus {
            background: #ffffff;
            border-color: #007bff;
            box-shadow: 0 0 0 4px rgba(0, 123, 255, 0.1);
        }

        /* The main button style from Login.aspx (btn-login) */
        .btn-verify {
            width: 100%;
            padding: 15px; /* Taller padding */
            font-size: clamp(15px, 4vw, 17px);
            font-weight: 600;
            color: #ffffff;
            background: linear-gradient(135deg, #007bff 0%, #0056b3 100%);
            border: none;
            border-radius: 12px;
            cursor: pointer;
            transition: all 0.3s ease;
            box-shadow: 0 4px 15px rgba(0, 123, 255, 0.3);
        }

        .btn-verify:hover {
            background: linear-gradient(135deg, #0056b3 0%, #004494 100%);
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(0, 123, 255, 0.4);
        }

        .btn-verify:active {
            transform: translateY(0);
        }
        
        /* Message/Error Label Styling */
        #messageContainer {
            margin-top: 20px;
            width: 100%;
            transition: all 0.3s ease;
            min-height: 20px; /* Prevents jump */
        }

        .message {
            font-size: clamp(12px, 3.5vw, 13px);
            color: #dc3545; /* Consistent error color */
            line-height: 1.6;
            padding: 10px 15px;
            background: rgba(220, 53, 69, 0.1); /* Subtle error background */
            border-radius: 8px;
            text-align: center;
            word-wrap: break-word;
            max-width: 100%;
            box-sizing: border-box;
            display: inline-block;
        }

        /* --- Modal Styles --- */
        .modal {
            position: fixed;
            z-index: 1000;
            left: 0;
            top: 0;
            width: 100%;
            height: 100%;
            background-color: rgba(0, 0, 0, 0.6); /* Darker overlay */
            animation: fadeIn 0.3s ease;
            padding: 15px;
            overflow-y: auto;
            display: none;
        }

        .modal.show {
            display: flex;
            justify-content: center;
            align-items: center;
        }

        .modal-content {
            background: #ffffff;
            border-radius: 16px; /* More rounded */
            padding: 30px 25px; /* Increased padding */
            width: 100%;
            max-width: 480px;
            box-shadow: 0 15px 40px rgba(0, 0, 0, 0.4); /* Stronger shadow */
            position: relative;
            animation: slideIn 0.3s cubic-bezier(0.34, 1.56, 0.64, 1); /* Bouncier animation */
            margin: auto;
        }
        
        @keyframes slideIn {
            from { transform: scale(0.8) translateY(20px); opacity: 0; }
            to { transform: scale(1) translateY(0); opacity: 1; }
        }

        .modal-title {
            font-size: clamp(20px, 5vw, 24px);
            font-weight: 700;
            color: #1a1a1a;
        }

        .close-btn {
            font-size: 32px; /* Larger close button */
            color: #666;
        }

        .modal-instruction {
            margin-bottom: 20px; /* Increased margin */
            color: #444;
            font-size: clamp(14px, 3.5vw, 15px);
            line-height: 1.6;
        }

        .secret-key-box {
            background: #e9ecef; /* Lighter gray background */
            border: 1px dashed #ced4da; /* Dashed border for key */
            border-radius: 8px;
            padding: 20px; /* More padding */
            font-family: 'Consolas', 'Courier New', monospace;
            font-size: clamp(14px, 3.5vw, 16px);
            color: #333;
            font-weight: 500;
            word-break: break-all;
            margin-bottom: 20px;
            line-height: 1.8;
            cursor: pointer;
            transition: all 0.3s ease;
        }
        
        .secret-key-box:hover {
            border-color: #007bff;
            background: #f8f9fa;
        }

        .copy-btn {
            /* Applied the standard blue button style to the copy button as well */
            width: 100%;
            background: linear-gradient(135deg, #007bff 0%, #0056b3 100%);
            color: white;
            border: none;
            border-radius: 12px;
            padding: 15px;
            font-size: clamp(15px, 4vw, 17px);
            cursor: pointer;
            transition: all 0.3s ease;
            font-weight: 600;
            box-shadow: 0 4px 15px rgba(0, 123, 255, 0.3);
        }

        .copy-btn:hover {
            background: linear-gradient(135deg, #0056b3 0%, #004494 100%);
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(0, 123, 255, 0.4);
        }
        
        /* --- Responsive adjustments --- (Updated to match the new container/padding) */
        @media screen and (max-width: 480px) {
            .login-container {
                padding: 30px 20px;
                border-radius: 16px;
                transform: none !important;
                max-width: 100%;
            }

            .logo {
                max-width: 150px;
            }

            h2 {
                font-size: 28px;
            }

            .instruction {
                margin-bottom: 20px;
            }
            
            .qr-img {
                max-width: 180px;
                margin: 10px 0 20px;
            }

            .divider {
                margin: 20px 0;
            }

            #otp-inputs {
                gap: 8px;
                margin-bottom: 25px;
            }

            .otp-box {
                height: 50px;
                max-width: 45px;
            }
            
            .btn-verify {
                padding: 14px;
            }
            
            .modal-content {
                padding: 25px 20px;
            }
            
            .copy-btn {
                padding: 14px;
            }
        }
        
        @media screen and (max-height: 500px) and (orientation: landscape) {
             .login-container {
                padding: 20px;
                margin: 10px auto;
            }
             .qr-img {
                max-width: 120px;
            }
            .otp-box {
                height: 40px;
                font-size: 18px;
            }
            .btn-verify {
                padding: 12px;
            }
        }
    </style>
</head>

<body>
    <form id="form1" runat="server" autocomplete="off">
        <asp:ScriptManager ID="ScriptManager1" runat="server" />
        <div class="login-container">
            <img src="images/logorrc.png" alt="RRC Logo" class="logo" />
            
            <h2 class="mb-2">Enable 2FA</h2>

            <div id="messageContainer">
                <asp:Label ID="lblMessage" runat="server" CssClass="message" />
            </div>

            <asp:Label ID="lblInstruction" runat="server" CssClass="instruction" 
                Text="Scan the QR code below with your authenticator app (Google Authenticator, Microsoft Authenticator, etc.)" />

            <asp:Panel ID="pnlQRCode" runat="server">
                <asp:Image ID="imgQRCode" runat="server" CssClass="qr-img" />
                
                <asp:Panel ID="pnlManualEntryLink" runat="server" Visible="false">
                    <a href="javascript:void(0);" class="manual-link" onclick="openModal()">
                        <i class="fa-solid fa-keyboard" style="margin-right: 5px;"></i>Can't scan QR code? Enter manually
                    </a>
                </asp:Panel>
            </asp:Panel>

            <div class="divider"><span>Enter Verification Code</span></div>

            <asp:HiddenField ID="txtCode" runat="server" />
            <asp:HiddenField ID="hdnSecretKey" runat="server" />

            <div id="otp-inputs">
                <input type="text" maxlength="1" class="otp-box" inputmode="numeric" pattern="[0-9]*" oninput="moveNext(this, 0)" onkeydown="handleBackspace(event, 0)" onpaste="return false;" />
                <input type="text" maxlength="1" class="otp-box" inputmode="numeric" pattern="[0-9]*" oninput="moveNext(this, 1)" onkeydown="handleBackspace(event, 1)" onpaste="return false;" />
                <input type="text" maxlength="1" class="otp-box" inputmode="numeric" pattern="[0-9]*" oninput="moveNext(this, 2)" onkeydown="handleBackspace(event, 2)" onpaste="return false;" />
                <input type="text" maxlength="1" class="otp-box" inputmode="numeric" pattern="[0-9]*" oninput="moveNext(this, 3)" onkeydown="handleBackspace(event, 3)" onpaste="return false;" />
                <input type="text" maxlength="1" class="otp-box" inputmode="numeric" pattern="[0-9]*" oninput="moveNext(this, 4)" onkeydown="handleBackspace(event, 4)" onpaste="return false;" />
                <input type="text" maxlength="1" class="otp-box" inputmode="numeric" pattern="[0-9]*" oninput="moveNext(this, 5)" onkeydown="handleBackspace(event, 5)" onpaste="return false;" />
            </div>

            <asp:Button ID="btnVerify" runat="server" Text="Verify & Enable 2FA" CssClass="btn-verify" OnClick="btnVerify_Click" UseSubmitBehavior="false" />
            
            </div>

        <div id="manualEntryModal" class="modal">
            <div class="modal-content">
                <div class="modal-header">
                    <span class="modal-title">Manual Entry</span>
                    <button type="button" class="close-btn" onclick="closeModal()">&times;</button>
                </div>
                <div class="modal-body">
                    <div class="modal-instruction">
                        Open your authenticator app and select "Enter a setup key" or "Manual entry". 
                        Then enter the following secret key:
                    </div>
                    <div class="secret-key-box" id="secretKeyDisplay" onclick="selectText(this)">
                        <asp:Label ID="lblSecretKey" runat="server" />
                    </div>
                    <button type="button" class="copy-btn" onclick="copySecretKey()">
                        <i class="fa-regular fa-copy" style="margin-right: 5px;"></i>Copy Secret Key
                    </button>
                </div>
            </div>
        </div>
    </form>

    <script>
        const boxes = document.querySelectorAll('.otp-box');
        const hiddenField = document.getElementById('<%= txtCode.ClientID %>');
        const messageContainer = document.getElementById('messageContainer');
        const messageLabel = document.getElementById('<%= lblMessage.ClientID %>'); // Correct ClientID usage
        const modal = document.getElementById('manualEntryModal');

        // --- OTP Input Functions ---
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

        // --- Modal Functions ---
        function openModal() {
            modal.classList.add('show');
            document.body.style.overflow = 'hidden';
        }

        function closeModal() {
            modal.classList.remove('show');
            document.body.style.overflow = 'auto';
        }

        modal.addEventListener('click', function(e) {
            if (e.target === modal) {
                closeModal();
            }
        });

        // --- Message Display Function (Adapted from Login.aspx) ---
        function initScripts() {
            // Display server-side message with consistent style
            if (messageLabel) {
                const text = messageLabel.innerText.trim();
                if (text !== "") {
                    // Simple logic to show the container if text is present
                    messageContainer.style.opacity = 1;
                    messageContainer.style.transform = 'translateY(0)';
                } else {
                    messageContainer.style.opacity = 0;
                    messageContainer.style.transform = 'translateY(-10px)';
                }
            }
            
            // Prevent zoom on iOS (consistent with other pages)
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

            // Ensure first OTP box is focused on load if no errors
            if (boxes.length > 0 && messageLabel && messageLabel.innerText.trim() === "") {
                boxes[0].focus();
            }
        }

        // --- Copy Functions (Retained original logic) ---
        function selectText(element) {
            if (window.getSelection && document.createRange) {
                const range = document.createRange();
                range.selectNodeContents(element);
                const sel = window.getSelection();
                sel.removeAllRanges();
                sel.addRange(range);
            }
        }

        function copySecretKey() {
            const secretKey = document.getElementById('<%= hdnSecretKey.ClientID %>').value;

            if (!secretKey) {
                alert('No secret key available to copy.');
                return;
            }

            const cleanKey = secretKey.replace(/\s/g, '');

            if (navigator.clipboard && window.isSecureContext) {
                navigator.clipboard.writeText(cleanKey).then(function () {
                    showCopyFeedback(event.target);
                }).catch(function (err) {
                    console.error('Clipboard API failed:', err);
                    fallbackCopy(cleanKey);
                });
            } else {
                fallbackCopy(cleanKey);
            }
        }

        function fallbackCopy(text) {
            const textarea = document.createElement('textarea');
            textarea.value = text;
            textarea.style.position = 'fixed';
            textarea.style.top = '0';
            textarea.style.left = '0';
            textarea.style.width = '2em';
            textarea.style.height = '2em';
            textarea.style.padding = '0';
            textarea.style.border = 'none';
            textarea.style.outline = 'none';
            textarea.style.boxShadow = 'none';
            textarea.style.background = 'transparent';
            textarea.setAttribute('readonly', '');

            document.body.appendChild(textarea);
            textarea.focus();
            textarea.select();

            let success = false;
            try {
                success = document.execCommand('copy');
                if (success) {
                    showCopyFeedback(document.activeElement);
                } else {
                    showManualCopyPrompt(text);
                }
            } catch (err) {
                console.error('execCommand failed:', err);
                showManualCopyPrompt(text);
            }

            document.body.removeChild(textarea);
        }

        function showManualCopyPrompt(text) {
            const promptText = 'Copy failed. Please select and copy this key manually:\n\n' + text;
            prompt(promptText, text);
        }

        function showCopyFeedback(btn) {
            const originalText = btn.innerHTML;
            const originalBg = btn.style.backgroundColor;

            btn.innerHTML = '<i class="fa-solid fa-check" style="margin-right: 5px;"></i>Copied!';
            btn.style.backgroundColor = '#218838'; // Green for success

            setTimeout(function () {
                btn.innerHTML = originalText;
                btn.style.backgroundColor = originalBg;
            }, 2000);
        }

        window.onload = function () {
            initScripts();
        };

        // Re-initialize after AJAX postback 
        if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(initScripts);
        }
    </script>
</body>
</html>