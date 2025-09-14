<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="RRCManagementSystem.Login" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link rel="icon" type="image/png" href="~/Images/rrc-logo.jpg" />
    <title>Login - RRC Management System</title>
    <link href="https://fonts.googleapis.com/css2?family=Poppins:wght@400;600&display=swap" rel="stylesheet" />
    <script src="https://www.google.com/recaptcha/api.js" async defer></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.2/css/all.min.css" />

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

        .login-container:hover { transform: translateY(-5px); }

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

        .input,
        .password-wrapper .input {
            width: 100%;
            padding: 14px 12px;
            padding-right: 40px; /* space for eye icon */
            border-radius: 8px;
            border: 1px solid #ccc;
            font-size: 15px;
            background: #f9f9f9;
            color: #333;
            box-sizing: border-box;
            margin-bottom: 20px;
        }

        .input::placeholder { color: #999; }

        .input:focus {
            background: #fff;
            border-color: #007bff;
            outline: none;
        }

        .password-wrapper {
            position: relative;
            width: 100%;
            margin-bottom: 20px;
        }

        .toggle-password {
            position: absolute;
            top: 50%;
            right: 14px;
            transform: translateY(-50%);
            background: transparent;
            border: none;
            cursor: pointer;
            font-size: 18px;
            color: #888;
            padding: 0;
            margin: 0;
            height: 100%;
            display: flex;
            align-items: center;
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

        .links { margin-top: 20px; }

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

        .g-recaptcha { margin-bottom: 20px; }

        .hidden { display: none; }



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

        <asp:Panel ID="pnlLogin" runat="server" DefaultButton="btnLogin">
            <!-- Email -->
       <asp:TextBox ID="txtEmail" runat="server" CssClass="input" placeholder="Email"
             TextMode="Email" AutoCompleteType="Disabled" 
             MaxLength="100"
             onkeydown="return focusPasswordOnEnter(event)" />


            <!-- Password with eye toggle -->
         <div class="password-wrapper position-relative">
    <!-- Password TextBox -->
    <asp:TextBox ID="txtPassword" runat="server"
                 CssClass="input"
                 placeholder="Password"
                 TextMode="Password"
                 MaxLength="64"
                 AutoCompleteType="Disabled" />

    <!-- Toggle Visibility Button -->
    <button type="button" id="btnTogglePwd" class="toggle-password hidden" aria-label="Show password">
        <i class="fa-solid fa-eye" aria-hidden="true"></i>
    </button>
</div>


            <!-- CAPTCHA Panel -->
            <asp:Panel ID="pnlCaptcha" runat="server" Visible="false">
                <div class="g-recaptcha" data-sitekey="6Ld6VrcrAAAAAGnZnUl3beqIS2JViuw5O5s0WlBh"></div>
            </asp:Panel>

            <!-- Login Button -->
            <asp:Button ID="btnLogin" runat="server" CssClass="btn-login" Text="Login"
                        OnClick="btnLogin_Click" UseSubmitBehavior="false" />
        </asp:Panel>

        <!-- Forgot Password -->
        <div class="links">
            <a href="ForgotPassword.aspx">Forgot Password?</a>
        </div>

        <!-- Message -->
        <asp:Label ID="lblMessage" runat="server" CssClass="message"></asp:Label>
    </div>

    <script>
        // Move focus from Email to Password when pressing Enter in email field
        function focusPasswordOnEnter(event) {
            if (event.key === "Enter") {
                event.preventDefault();
                document.getElementById('<%= txtPassword.ClientID %>').focus();
                return false;
            }
            return true;
        }

        document.addEventListener("DOMContentLoaded", function () {
            const pwdInput = document.getElementById('<%= txtPassword.ClientID %>');
            const toggleBtn = document.getElementById('btnTogglePwd');
            const toggleIcon = toggleBtn.querySelector('i');

            // Show the eye only when there is any character in the password box
            function refreshEyeVisibility() {
                if (pwdInput.value && pwdInput.value.length > 0) {
                    toggleBtn.classList.remove('hidden');
                } else {
                    toggleBtn.classList.add('hidden');
                    // Reset to hidden state when cleared
                    if (pwdInput.type !== 'password') {
                        pwdInput.type = 'password';
                        toggleIcon.classList.remove('fa-eye-slash');
                        toggleIcon.classList.add('fa-eye');
                        toggleBtn.setAttribute('aria-label', 'Show password');
                    }
                }
            }

            // Initial state (handles autofill too)
            refreshEyeVisibility();

            // Update on input
            pwdInput.addEventListener('input', refreshEyeVisibility);

            // Toggle show/hide on click
            toggleBtn.addEventListener('click', function () {
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
                // Keep focus on the input for better UX
                pwdInput.focus();
            });
        });
    </script>
</form>
</body>
</html>
