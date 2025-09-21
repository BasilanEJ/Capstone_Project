<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="VerifyTOTP.aspx.cs" Inherits="RRCManagementSystem.VerifyTOTP" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" class="h-full">
<head runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <link rel="icon" type="image/png" href="~/Images/rrc-logo.jpg" />
    <title>2FA Verification - RRC Management System</title>
    <link href="https://fonts.googleapis.com/css2?family=Poppins:wght@400;600&display=swap" rel="stylesheet" />
    <script src="https://cdn.tailwindcss.com"></script>
    <script src="https://www.google.com/recaptcha/api.js" async defer></script>
    <style>
        body {
            font-family: 'Poppins', sans-serif;
            color: #333;
        }
        .bg-image {
            background-image: url('images/logo.jpg');
        }
    </style>
</head>

<body class="h-full flex items-center justify-center bg-cover bg-center bg-fixed bg-image">
    <form id="form1" runat="server" autocomplete="off">
        <asp:ScriptManager ID="ScriptManager1" runat="server" />
        <div class="login-container bg-white p-10 md:p-14 rounded-2xl shadow-2xl flex flex-col items-center text-center border border-gray-200 w-11/12 max-w-lg transition-transform hover:translate-y-[-5px]">
            <img src="images/logorrc.png" alt="RRC Logo" class="w-48 mb-5" />
            <h2 class="text-3xl font-semibold text-gray-800 mb-6">Two-Factor Verification</h2>

            <asp:HiddenField ID="txtTOTP" runat="server" />

            <div id="otp-inputs" class="flex gap-2 justify-center mb-5">
                <input type="text" maxlength="1" class="otp-box w-12 h-14 text-center text-2xl rounded-lg border border-gray-300 bg-gray-50 text-gray-800 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent" oninput="moveNext(this, 0)" onkeydown="handleBackspace(event, 0)" onpaste="return false;" />
                <input type="text" maxlength="1" class="otp-box w-12 h-14 text-center text-2xl rounded-lg border border-gray-300 bg-gray-50 text-gray-800 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent" oninput="moveNext(this, 1)" onkeydown="handleBackspace(event, 1)" onpaste="return false;" />
                <input type="text" maxlength="1" class="otp-box w-12 h-14 text-center text-2xl rounded-lg border border-gray-300 bg-gray-50 text-gray-800 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent" oninput="moveNext(this, 2)" onkeydown="handleBackspace(event, 2)" onpaste="return false;" />
                <input type="text" maxlength="1" class="otp-box w-12 h-14 text-center text-2xl rounded-lg border border-gray-300 bg-gray-50 text-gray-800 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent" oninput="moveNext(this, 3)" onkeydown="handleBackspace(event, 3)" onpaste="return false;" />
                <input type="text" maxlength="1" class="otp-box w-12 h-14 text-center text-2xl rounded-lg border border-gray-300 bg-gray-50 text-gray-800 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent" oninput="moveNext(this, 4)" onkeydown="handleBackspace(event, 4)" onpaste="return false;" />
                <input type="text" maxlength="1" class="otp-box w-12 h-14 text-center text-2xl rounded-lg border border-gray-300 bg-gray-50 text-gray-800 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent" oninput="moveNext(this, 5)" onkeydown="handleBackspace(event, 5)" onpaste="return false;" />
            </div>

            <asp:Button ID="btnVerifyTOTP" runat="server" Text="Verify Code" CssClass="btn-primary w-full py-3 bg-green-600 text-white font-semibold rounded-lg hover:bg-green-700 transition-colors duration-300 mb-2" OnClick="btnVerifyTOTP_Click" />

            <div class="divider relative w-full text-center my-5">
                <span class="bg-white px-2 text-sm text-gray-600">OR</span>
            </div>

            <asp:Button ID="btnSendEmailCode" runat="server" Text="Send Code to Email" CssClass="btn-secondary w-full py-3 bg-blue-600 text-white font-semibold rounded-lg hover:bg-blue-700 transition-colors duration-300 mb-5" OnClick="btnSendEmailCode_Click" />

            <asp:Label ID="lblInfo" runat="server" CssClass="info-message text-sm text-blue-600 mt-2" />
            <asp:Label ID="lblMessage" runat="server" CssClass="message text-sm text-red-500 mt-2" />

            <asp:Panel ID="pnlCaptcha" runat="server" Visible="false" CssClass="mt-5">
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

        // Autofocus first box on page load
        window.onload = function () {
            boxes[0].focus();
        };
    </script>
</body>
</html>