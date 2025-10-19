<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ResetAdminPassword.aspx.cs" Inherits="RRCManagementSystem.ResetAdminPassword" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no" />
    <meta charset="UTF-8" />
    <link rel="icon" type="image/png" href="~/Images/rrc-logo.jpg" />
    <title>Reset Admin Password - RRC Management System</title>

    <link href="https://fonts.googleapis.com/css2?family=Poppins:wght@400;500;600;700&display=swap" rel="stylesheet" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.2/css/all.min.css" />

    <style>
        /*
         * INLINE STYLES MATCHING Login.aspx
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
        
        .subtitle { 
            color: #666;
            font-size: clamp(13px, 3.5vw, 15px);
            margin-bottom: 30px;
            font-weight: 400;
            line-height: 1.6;
        }

        .input-group {
            margin-bottom: 20px;
            width: 100%;
            position: relative; /* For the validation message */
        }
        
        .input-group:last-of-type {
            margin-bottom: 30px; /* Space before the button */
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

        .btn-login { /* Reusing btn-login class name */
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

        #messageContainer {
            margin-top: 10px; /* Reduced margin */
            margin-bottom: 20px; /* Space between message and first input */
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
            color: #dc3545; /* Default error color */
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

        /* Validation Message Styling */
        .validation-message {
            font-size: 12px;
            margin-top: 5px;
            text-align: left;
            padding: 0 4px; /* Slight padding to match input */
            display: block;
            min-height: 16px; /* Reserve space to prevent layout jump */
        }

        /* Responsive adjustments */
        @media screen and (max-width: 768px) {
            .login-container {
                padding: 40px 30px;
            }
        }

        @media screen and (max-width: 480px) {
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
        }
    </style>
</head>

<body onload="initScripts()">
    <form id="form1" runat="server">
        <div class="login-container">
            <h2 class="mb-2">Reset Admin Password</h2>
            
            <p class="subtitle">
                Please enter and confirm your new administrator password.
            </p>

            <div id="messageContainer">
                <asp:Label ID="lblMessage" runat="server" CssClass="message" />
            </div>

            <div class="input-group">
                <asp:TextBox ID="txtNewPassword" runat="server" CssClass="input" TextMode="Password"
                             placeholder="Enter new password" onkeyup="validatePasswordStrength(this.value)" />
                <span id="passwordStrengthMsg" class="validation-message"></span>
            </div>

            <div class="input-group">
                <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="input" TextMode="Password"
                             placeholder="Confirm new password" onkeyup="validatePasswordMatch()" />
                <span id="passwordMatchMsg" class="validation-message"></span>
            </div>

            <asp:Button ID="btnResetPassword" runat="server" Text="Reset Password" 
                        CssClass="btn-login" 
                        OnClick="btnResetPassword_Click" 
                        UseSubmitBehavior="false" />

            <div class="links-container">
                <a href="Login.aspx" class="link">
                    <i class="fa-solid fa-arrow-left" style="margin-right: 5px;"></i>Back to Login
                </a>
            </div>
        </div>
    </form>

<script>
    function validatePasswordStrength(password) {
        // --- ALIGNED WITH SERVER-SIDE REGEX ---
        // Requires: 8-64 chars, 1 lower, 1 upper, 1 digit, 1 special (@$!%*?&.)
        const serverPasswordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&.])[A-Za-z\d@$!%*?&.]{8,64}$/;

        const strengthMsg = document.getElementById("passwordStrengthMsg");

        if (password.length === 0) {
            strengthMsg.innerText = "";
            strengthMsg.style.color = "";
            validatePasswordMatch();
            return;
        }

        if (!serverPasswordRegex.test(password)) {
            // Detailed messages for user experience (UX) based on the server-side rules
            if (password.length < 8) {
                strengthMsg.innerText = "❌ Password must be at least 8 characters long.";
            } else if (password.length > 64) {
                strengthMsg.innerText = "❌ Password cannot exceed 64 characters.";
            } else if (!/[A-Z]/.test(password)) {
                strengthMsg.innerText = "❌ Must include at least one uppercase letter.";
            } else if (!/[a-z]/.test(password)) {
                strengthMsg.innerText = "❌ Must include at least one lowercase letter.";
            } else if (!/\d/.test(password)) {
                strengthMsg.innerText = "❌ Must include at least one number (digit).";
            } else if (!/[@$!%*?&.]/.test(password)) {
                strengthMsg.innerText = "❌ Must include one special character (@$!%*?&.).";
            } else {
                strengthMsg.innerText = "❌ Password strength policy failed.";
            }
            strengthMsg.style.color = "#dc3545"; // Red
        } else {
            // Optional: Provide a medium/strong indicator based on length
            if (password.length >= 12) {
                strengthMsg.innerText = "✅ Strong password.";
                strengthMsg.style.color = "#28a745"; // Green
            } else {
                strengthMsg.innerText = "✔ Good password. Consider increasing length.";
                strengthMsg.style.color = "#007bff"; // Blue
            }
        }

        // Always check if passwords match
        validatePasswordMatch();
    }

    function validatePasswordMatch() {
        const password = document.getElementById("<%= txtNewPassword.ClientID %>").value;
        const confirm = document.getElementById("<%= txtConfirmPassword.ClientID %>").value;
        const matchMsg = document.getElementById("passwordMatchMsg");

        if (!confirm && password) {
            matchMsg.innerText = "";
            return;
        }

        // Ensure both fields have input before showing match/mismatch status
        if (!password || !confirm) {
            matchMsg.innerText = "";
            matchMsg.style.color = "";
            return;
        }

        if (password !== confirm) {
            matchMsg.innerText = "❌ Passwords do not match.";
            matchMsg.style.color = "#dc3545"; // Red
        } else {
            matchMsg.innerText = "✅ Passwords match.";
            matchMsg.style.color = "#28a745"; // Green
        }
    }
    
    // Function to handle message display and viewport fix
    function initScripts() {
        const msgContainer = document.getElementById('messageContainer');
        const msgLabel = msgContainer ? document.getElementById('<%= lblMessage.ClientID %>') : null;

        if (msgContainer && msgLabel) {
            const text = msgLabel.innerText.trim();
            if (text !== "") {
                const errorRegex = /^(.*?\.?)\s*(.*)$/;
                const match = text.match(errorRegex);
                let formattedText = text;

                if (match && match[1].length > 0) {
                    const boldPart = match[1].trim();
                    const restOfMessage = match[2].trim();
                    formattedText = `<strong>${boldPart}</strong> ${restOfMessage}`;
                } else {
                    formattedText = text.replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>');
                }

                msgLabel.innerHTML = formattedText;
                msgContainer.classList.add('show');
            } else {
                msgContainer.classList.remove('show');
            }
        }

        // Prevent zoom on iOS (copied from Login.aspx)
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
    }

    // Initialize scripts after DOM is ready
    document.addEventListener("DOMContentLoaded", initScripts);

    // Re-initialize after AJAX postback 
    if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(initScripts);
    }
</script>

</body>
</html>