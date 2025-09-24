<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="RRCManagementSystem.Login" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" class="h-full">
<head runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <link rel="icon" type="image/png" href="~/Images/rrc-logo.jpg" />
    <title>Login - RRC Management System</title>

    <link href="https://fonts.googleapis.com/css2?family=Poppins:wght@400;600&display=swap" rel="stylesheet" />

    <script src="https://cdn.tailwindcss.com"></script>

    <script src="https://www.google.com/recaptcha/api.js" async defer></script>

    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.2/css/all.min.css" />

    <style>
        body {
            font-family: 'Poppins', sans-serif;
            color: #333;
        }

        .bg-image {
            background-image: url('images/logo.jpg');
        }

        .fa-eye, .fa-eye-slash {
            color: #888;
        }

        /* Error message container to prevent layout shifting */
        #messageContainer {
            min-height: 1.5rem; /* Reserves space for one line of text */
            transition: opacity 0.3s ease-in-out; /* Adds a fade effect */
            opacity: 0;
        }

        /* When message is visible */
        #messageContainer.show {
            opacity: 1;
        }
    </style>
</head>
<body class="h-full flex items-center justify-center bg-cover bg-center bg-fixed bg-image">
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" />

        <div class="login-container bg-white p-12 md:p-16 rounded-2xl shadow-2xl flex flex-col items-center text-center border border-gray-200 w-11/12 max-w-xl transition-transform hover:translate-y-[-5px]">
                 
            <img src="images/logorrc.png" alt="RRC Logo" class="w-48 h-auto mb-5" />

            <h2 class="text-3xl font-semibold text-gray-800 mb-6">Login</h2>

            <asp:UpdatePanel ID="upLogin" runat="server">
                <ContentTemplate>
                    <asp:Panel ID="pnlLogin" runat="server" DefaultButton="btnLogin" CssClass="w-full">

                        <asp:TextBox ID="txtEmail" runat="server"
                            CssClass="input block w-full px-4 py-3 rounded-lg border border-gray-300 text-gray-800 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent mb-5"
                            placeholder="Email"
                            TextMode="Email"
                            AutoCompleteType="Disabled"
                            MaxLength="100"
                            onkeydown="return focusPasswordOnEnter(event)" />

                        <div class="relative w-full mb-5">
                            <asp:TextBox ID="txtPassword" runat="server"
                                CssClass="input block w-full px-4 py-3 rounded-lg border border-gray-300 text-gray-800 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent pr-10"
                                placeholder="Password"
                                TextMode="Password"
                                MaxLength="64"
                                AutoCompleteType="Disabled" />

                            <button type="button" id="btnTogglePwd" class="toggle-password absolute inset-y-0 right-0 flex items-center pr-3 hidden" aria-label="Show password">
                                <i class="fa-solid fa-eye" aria-hidden="true"></i>
                            </button>
                        </div>

                        <asp:Panel ID="pnlCaptcha" runat="server" Visible="false" CssClass="mb-5">
                            <div class="g-recaptcha" data-sitekey="6Ld6VrcrAAAAAGnZnUl3beqIS2JViuw5O5s0WlBh"></div>
                        </asp:Panel>

                        <asp:Button ID="btnLogin" runat="server"
                            CssClass="btn-login w-full py-3 bg-blue-600 text-white font-semibold rounded-lg hover:bg-blue-700 transition-colors duration-300"
                            Text="Login"
                            OnClick="btnLogin_Click"
                            UseSubmitBehavior="false" />
                    </asp:Panel>

                    <div class="links mt-5">
                        <a href="ForgotPassword.aspx" class="text-blue-600 text-sm hover:underline">Forgot Password?</a>
                    </div>

                    <div id="messageContainer" class="mt-4 text-sm text-red-500">
                        <asp:Label ID="lblMessage" runat="server"></asp:Label>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>

        <script>
            function initScripts() {
                const pwdInput = document.getElementById('<%= txtPassword.ClientID %>');
                const toggleBtn = document.getElementById('btnTogglePwd');
                const toggleIcon = toggleBtn.querySelector('i');
                const msgContainer = document.getElementById('messageContainer');

                if (pwdInput && toggleBtn) {
                    const refreshEyeVisibility = () => {
                        if (pwdInput.value && pwdInput.value.length > 0) {
                            toggleBtn.classList.remove('hidden');
                        } else {
                            toggleBtn.classList.add('hidden');
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

                if (msgContainer) {
                    if (msgContainer.innerText.trim() !== "") {
                        msgContainer.classList.add('show');
                    } else {
                        msgContainer.classList.remove('show');
                    }
                }
            }

            function focusPasswordOnEnter(event) {
                if (event.key === "Enter") {
                    event.preventDefault();
                    document.getElementById('<%= txtPassword.ClientID %>').focus();
                    return false;
                }
                return true;
            }

            document.addEventListener("DOMContentLoaded", initScripts);
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(initScripts);
        </script>
    </form>
</body>
</html>