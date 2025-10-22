<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ResetPassword.aspx.cs" Inherits="RRCManagementSystem.ResetPassword" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no" />
    <meta charset="UTF-8" />
    <link rel="icon" type="image/png" href="~/Images/rrc-logo.jpg" />
    <title>Reset Password - RRC Management System</title>

    <link href="https://fonts.googleapis.com/css2?family=Poppins:wght@400;500;600;700&display=swap" rel="stylesheet" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.2/css/all.min.css" />
    <!-- SweetAlert2 CDN -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

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
        }

        .btn-login:hover {
            background: linear-gradient(135deg, #0056b3 0%, #004494 100%);
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(0, 123, 255, 0.4);
        }

        .btn-login:active {
            transform: translateY(0);
        }
        
        .btn-login:disabled {
            background: #ccc;
            cursor: not-allowed;
            box-shadow: none;
        }

        .validation-message {
            font-size: 12px;
            color: #666;
            margin-top: 5px;
            text-align: left;
            padding: 0 4px;
            display: block;
            min-height: 16px; 
        }

        .checkbox-container {
            width: 100%;
            text-align: left;
            font-size: 13px;
            color: #444;
            margin-bottom: 25px;
        }
        
        .checkbox-container input[type="checkbox"] {
            margin-right: 5px;
            transform: scale(1.1);
            accent-color: #007bff;
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

            .input {
                padding: 13px 16px;
            }

            .btn-login {
                padding: 14px;
            }

            .links-container {
                margin-top: 20px;
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
    </style>
</head>

<body onload="initScripts()">
    <form id="form1" runat="server">
        <div class="login-container">
            <img src="images/logorrc.png" alt="RRC Logo" class="logo" />
            
            <h2>Reset Password</h2>
            
            <p class="subtitle">
                Please enter a new password that meets the security requirements.
            </p>

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

            <div class="checkbox-container">
                <input type="checkbox" onclick="togglePasswords()" id="chkShowPass" /> 
                <label for="chkShowPass">Show Passwords</label>
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

            <asp:Literal ID="ltScript" runat="server" />
        </div>
    </form>

<script>
    // --- START: Client-side Validation Logic ---
    const COLOR_RED = "#dc3545";
    const COLOR_GREEN = "#28a745";
    const COLOR_BLUE = "#007bff";

    function validatePasswordStrength(password) {
        const serverPasswordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&.])[A-Za-z\d@$!%*?&.]{8,64}$/;
        const strengthMsg = document.getElementById("passwordStrengthMsg");

        if (password.length === 0) {
            strengthMsg.innerText = "";
            strengthMsg.style.color = "";
            validatePasswordMatch();
            return;
        }

        if (!serverPasswordRegex.test(password)) {
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
            strengthMsg.style.color = COLOR_RED;
        } else {
            if (password.length >= 12) {
                strengthMsg.innerText = "✅ Strong password.";
                strengthMsg.style.color = COLOR_GREEN;
            } else {
                strengthMsg.innerText = "✔ Good password. Consider increasing length.";
                strengthMsg.style.color = COLOR_BLUE;
            }
        }

        validatePasswordMatch();
    }

    function validatePasswordMatch() {
        const password = document.getElementById("<%= txtNewPassword.ClientID %>").value;
        const confirm = document.getElementById("<%= txtConfirmPassword.ClientID %>").value;
        const matchMsg = document.getElementById("passwordMatchMsg");

        if (!password || !confirm) {
            matchMsg.innerText = "";
            matchMsg.style.color = "";
            return;
        }

        if (password !== confirm) {
            matchMsg.innerText = "❌ Passwords do not match.";
            matchMsg.style.color = COLOR_RED; 
        } else {
            matchMsg.innerText = "✅ Passwords match.";
            matchMsg.style.color = COLOR_GREEN; 
        }
    }

    function togglePasswords() {
        const pass1 = document.getElementById("<%= txtNewPassword.ClientID %>");
        const pass2 = document.getElementById("<%= txtConfirmPassword.ClientID %>");
        const type = pass1.type === "password" ? "text" : "password";
        pass1.type = type;
        pass2.type = type;
    }

    // --- END: Client-side Validation Logic ---

    function initScripts() {
        // Prevent zoom on iOS
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