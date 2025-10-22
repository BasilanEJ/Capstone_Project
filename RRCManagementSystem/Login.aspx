<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="RRCManagementSystem.Login" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no" />
    <meta charset="UTF-8" />
    <link rel="icon" type="image/png" href="~/Images/rrc-logo.jpg" />
    <title>Login - RRC Management System</title>

    <link href="https://fonts.googleapis.com/css2?family=Poppins:wght@400;500;600;700&display=swap" rel="stylesheet" />
    <script src="https://www.google.com/recaptcha/api.js" async defer></script>
    <script src="js/enhanced-fingerprint.js"></script>
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

        .login-container {
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

        .subtitle {
            color: #666;
            font-size: clamp(13px, 3.5vw, 15px);
            margin-bottom: 30px;
            font-weight: 400;
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
            font-size: clamp(14px, 3.5vw, 16px);
            border: 2px solid #e0e0e0;
            border-radius: 12px;
            background: #f8f9fa;
            color: #333;
            transition: all 0.3s ease;
            font-family: 'Poppins', sans-serif;
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
        }

        .password-wrapper {
            position: relative;
            width: 100%;
        }

        .password-wrapper .input {
            padding-right: 50px;
        }

        .toggle-password {
            position: absolute;
            right: 15px;
            top: 50%;
            transform: translateY(-50%);
            background: none;
            border: none;
            cursor: pointer;
            color: #888;
            font-size: 18px;
            padding: 8px;
            border-radius: 6px;
            transition: all 0.3s ease;
            display: none;
        }

        .toggle-password.show {
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .toggle-password:hover {
            background: rgba(0, 123, 255, 0.1);
            color: #007bff;
        }

        .captcha-container {
            margin-bottom: 20px;
            display: flex;
            justify-content: center;
            transform: scale(1);
            transform-origin: center;
        }

        .btn-login {
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

        .btn-login:hover {
            background: linear-gradient(135deg, #0056b3 0%, #004494 100%);
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(0, 123, 255, 0.4);
        }

        .btn-login:active {
            transform: translateY(0);
        }

        .links-container {
            margin-top: 25px;
            width: 100%;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 12px;
            flex-wrap: wrap;
            padding: 0 10px;
        }

        .link {
            color: #007bff;
            text-decoration: none;
            font-size: clamp(13px, 3.5vw, 14px);
            font-weight: 500;
            transition: all 0.3s ease;
            padding: 4px 8px;
            border-radius: 6px;
        }

        .link:hover {
            color: #0056b3;
            background: rgba(0, 123, 255, 0.1);
        }

        .link-separator {
            color: #ccc;
            font-size: clamp(12px, 3vw, 14px);
            user-select: none;
        }

        #messageContainer {
            margin-top: 20px;
            width: 100%;
            transition: all 0.3s ease;
            opacity: 0;
            transform: translateY(-10px);
        }

     #messageContainer.show {
            opacity: 1;
            transform: translateY(0);
        }

        .message {
            font-size: clamp(12px, 3.5vw, 13px);
            color: #dc3545;
            line-height: 1.6;
            padding: 14px 16px;
            background: none;
            border-radius: 8px;
            text-align: left;
            word-wrap: break-word;
            overflow-wrap: break-word;
            max-width: 100%;
            box-sizing: border-box;
        }

        .message strong {
            font-weight: 600;
            margin-right: 5px;
            white-space: nowrap;
        }

        @media screen and (max-width: 480px) {
            .message {
                font-size: 12px;
                padding: 12px 14px;
                line-height: 1.7;
            }
        }

        /* Loading State */
        .btn-login:disabled {
            background: #ccc;
            cursor: not-allowed;
            box-shadow: none;
        }

        /* Responsive adjustments */
        @media screen and (max-width: 768px) {
            body {
                padding: 15px;
            }

            .login-container {
                padding: 40px 30px;
            }
        }

        @media screen and (max-width: 480px) {
            body {
                padding: 10px;
                align-items: center;
                justify-content: center;
            }

            .login-container {
                padding: 30px 20px;
                border-radius: 16px;
                transform: none !important;
                max-width: 100%;
            }

            .login-container:hover {
                transform: none !important;
                box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
            }

            .logo {
                max-width: 150px;
                margin-bottom: 16px;
            }

            h2 {
                margin-bottom: 8px;
            }

            .subtitle {
                margin-bottom: 25px;
            }

            .input-group {
                margin-bottom: 16px;
            }

            .input {
                padding: 13px 16px;
            }

            .btn-login {
                padding: 14px;
            }

            .links-container {
                margin-top: 20px;
                gap: 10px;
                flex-direction: column;
            }

            .link-separator {
                display: none;
            }

            .captcha-container {
                transform: scale(0.9);
                overflow: hidden;
            }
        }

        @media screen and (max-width: 360px) {
            .login-container {
                padding: 30px 20px;
            }

            .logo {
                max-width: 140px;
            }

            .input {
                padding: 12px 14px;
            }

            .captcha-container {
                transform: scale(0.85);
            }
        }

        /* Landscape mode for mobile */
        @media screen and (max-height: 600px) and (orientation: landscape) {
            body {
                align-items: flex-start;
                padding-top: 15px;
            }

            .login-container {
                margin: 15px auto;
                padding: 25px 30px;
            }

            .logo {
                max-width: 120px;
                margin-bottom: 12px;
            }

            h2 {
                margin-bottom: 5px;
            }

            .subtitle {
                margin-bottom: 15px;
            }

            .input-group {
                margin-bottom: 12px;
            }

            .links-container {
                margin-top: 15px;
            }
        }

        /* reCAPTCHA responsive */
        @media screen and (max-width: 380px) {
            .captcha-container > div {
                transform: scale(0.85);
                transform-origin: 0 0;
            }
        }
    </style>
</head>

<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" />

        <div class="login-container">
            <img src="images/logorrc.png" alt="RRC Logo" class="logo" />
            <h2>Welcome Back</h2>
            <p class="subtitle">Sign in to your account</p>

            <asp:UpdatePanel ID="upLogin" runat="server">
                <ContentTemplate>
                    <asp:Panel ID="pnlLogin" runat="server" DefaultButton="btnLogin" CssClass="form-content">

                        <div class="input-group">
                            <asp:TextBox ID="txtEmail" runat="server"
                                CssClass="input"
                                placeholder="Email Address"
                                TextMode="Email"
                                AutoCompleteType="Disabled"
                                MaxLength="100"
                                onkeydown="return focusPasswordOnEnter(event)" />
                        </div>

                        <div class="input-group">
                            <div class="password-wrapper">
                                <asp:TextBox ID="txtPassword" runat="server"
                                    CssClass="input"
                                    placeholder="Password"
                                    TextMode="Password"
                                    MaxLength="64"
                                    AutoCompleteType="Disabled" />

                                <button type="button" id="btnTogglePwd" 
                                    class="toggle-password" 
                                    aria-label="Show password">
                                    <i class="fa-solid fa-eye" aria-hidden="true"></i>
                                </button>
                            </div>
                        </div>

<asp:HiddenField ID="hiddenFingerprint" runat="server" />
<asp:HiddenField ID="hiddenCanvasFingerprint" runat="server" />
<asp:HiddenField ID="hiddenHardwareID" runat="server" />
<asp:HiddenField ID="hiddenOSInfo" runat="server" />
<asp:HiddenField ID="hiddenBrowserName" runat="server" />
<asp:HiddenField ID="hiddenScreenResolution" runat="server" />
<asp:HiddenField ID="hiddenTimezoneOffset" runat="server" />
<asp:HiddenField ID="hiddenLanguage" runat="server" />
<asp:HiddenField ID="hiddenHardwareConcurrency" runat="server" />
<asp:HiddenField ID="hiddenColorDepth" runat="server" />
<asp:HiddenField ID="hiddenDeviceMemory" runat="server" />
<asp:HiddenField ID="hiddenMaxTouchPoints" runat="server" />
<asp:HiddenField ID="hiddenPlatform" runat="server" />

                        <asp:Panel ID="pnlCaptcha" runat="server" Visible="false" CssClass="captcha-container">
                            <div class="g-recaptcha" data-sitekey="6Ld6VrcrAAAAAGnZnUl3beqIS2JViuw5O5s0WlBh"></div>
                        </asp:Panel>

                        <asp:Button ID="btnLogin" runat="server"
                            CssClass="btn-login"
                            Text="Sign In"
                            OnClick="btnLogin_Click"
                            UseSubmitBehavior="false" />

                        <div class="links-container">
                            <a href="Default.aspx" class="link">
                                <i class="fa-solid fa-arrow-left" style="margin-right: 5px;"></i>Back to Landing Page
                            </a>
                            <span class="link-separator">•</span>
                            <a href="ForgotPassword.aspx" class="link">
                                Forgot Password?<i class="fa-solid fa-arrow-right" style="margin-left: 5px;"></i>
                            </a>
                        </div>

                        <div id="messageContainer">
                            <asp:Label ID="lblMessage" runat="server" CssClass="message"></asp:Label>
                        </div>

                    </asp:Panel>
                </ContentTemplate>

                <Triggers>
                    <asp:PostBackTrigger ControlID="btnLogin" />
                </Triggers>
            </asp:UpdatePanel>
        </div>

        <script>
            function initScripts() {
                const pwdInput = document.getElementById('<%= txtPassword.ClientID %>');
                const toggleBtn = document.getElementById('btnTogglePwd');
                const toggleIcon = toggleBtn ? toggleBtn.querySelector('i') : null;
                const msgContainer = document.getElementById('messageContainer');
                const msgLabel = msgContainer ? document.getElementById('<%= lblMessage.ClientID %>') : null;

                // Toggle password visibility
                if (pwdInput && toggleBtn && toggleIcon) {
                    const refreshEyeVisibility = () => {
                        if (pwdInput.value && pwdInput.value.length > 0) {
                            toggleBtn.classList.add('show');
                        } else {
                            toggleBtn.classList.remove('show');
                            if (pwdInput.type !== 'password') {
                                pwdInput.type = 'password';
                                toggleIcon.classList.remove('fa-eye-slash');
                                toggleIcon.classList.add('fa-eye');
                                toggleBtn.setAttribute('aria-label', 'Show password');
                            }
                        }
                    };

                    refreshEyeVisibility();
                    pwdInput.removeEventListener('input', refreshEyeVisibility);
                    pwdInput.addEventListener('input', refreshEyeVisibility);

                    toggleBtn.removeEventListener('click', togglePassword);
                    toggleBtn.addEventListener('click', togglePassword);

                    function togglePassword() {
                        if (pwdInput.type === 'password') {
                            pwdInput.type = 'text';
                            toggleIcon.classList.remove('fa-eye');
                            toggleIcon.classList.add('fa-eye-slash');
                            toggleBtn.setAttribute('aria-label', 'Hide password');
                        } else {
                            pwdInput.type = 'password';
                            toggleIcon.classList.remove('fa-eye-slash');
                            toggleIcon.classList.add('fa-eye');
                            toggleBtn.setAttribute('aria-label', 'Show password');
                        }
                        pwdInput.focus();
                    }
                }

                // FIX: Show or hide error container and ensure proper formatting
                if (msgContainer && msgLabel) {
                    const text = msgLabel.innerText.trim();
                    if (text !== "") {
                        // Attempt to format the standard error message pattern: "Text. Rest of message."
                        const errorRegex = /^(.*?\.?)\s*(.*)$/;
                        const match = text.match(errorRegex);

                        let formattedText = text;

                        if (match && match[1].length > 0) {
                            // Apply bold only to the first sentence/phrase if it exists
                            const boldPart = match[1].trim();
                            const restOfMessage = match[2].trim();
                            
                            // Reconstruct the message with <strong> for the first part
                            formattedText = `<strong>${boldPart}</strong> ${restOfMessage}`;
                        } else {
                             // Fallback for custom messages using **Markdown**
                             formattedText = text.replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>');
                        }

                        msgLabel.innerHTML = formattedText;
                        msgContainer.classList.add('show');
                    } else {
                        msgContainer.classList.remove('show');
                    }
                }
            }

            // Focus password field when Enter is pressed in email field
            function focusPasswordOnEnter(event) {
                if (event.key === "Enter") {
                    event.preventDefault();
                    document.getElementById('<%= txtPassword.ClientID %>').focus();
                    return false;
                }
                return true;
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

            // Initialize scripts after DOM is ready
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