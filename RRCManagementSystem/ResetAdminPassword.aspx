<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ResetAdminPassword.aspx.cs" Inherits="RRCManagementSystem.ResetAdminPassword" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" class="h-full">
<head runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <link rel="icon" type="image/png" href="~/Images/rrc-logo.jpg" />
    <title>Reset Admin Password - RRC Management System</title>
    <link href="https://fonts.googleapis.com/css2?family=Poppins:wght@400;600&display=swap" rel="stylesheet" />
    <script src="https://cdn.tailwindcss.com"></script>
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
    <form id="form1" runat="server">
        <div class="login-container bg-white p-10 md:p-14 rounded-2xl shadow-2xl flex flex-col items-center text-center border border-gray-200 w-11/12 max-w-lg transition-transform hover:translate-y-[-5px]">
            <h2 class="text-3xl font-semibold text-gray-800 mb-6">Reset Admin Password</h2>

            <asp:Label ID="lblMessage" runat="server" CssClass="message mt-2 mb-4 text-sm text-red-500" />

            <div class="w-full mb-3">
                <asp:TextBox ID="txtNewPassword" runat="server" CssClass="input block w-full px-4 py-3 rounded-lg border border-gray-300 text-gray-800 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent" TextMode="Password"
                             placeholder="Enter new password" onkeyup="validatePasswordStrength(this.value)" />
                <span id="passwordStrengthMsg" class="validation-message text-xs text-gray-500 mt-1 block text-left"></span>
            </div>

            <div class="w-full mb-5">
                <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="input block w-full px-4 py-3 rounded-lg border border-gray-300 text-gray-800 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent" TextMode="Password"
                             placeholder="Confirm new password" onkeyup="validatePasswordMatch()" />
                <span id="passwordMatchMsg" class="validation-message text-xs text-gray-500 mt-1 block text-left"></span>
            </div>

            <asp:Button ID="btnResetPassword" runat="server" Text="Reset Password" CssClass="btn-reset w-full py-3 bg-blue-600 text-white font-semibold rounded-lg hover:bg-blue-700 transition-colors duration-300"
                        OnClick="btnResetPassword_Click" />
        </div>
    </form>

  <script>
      function validatePasswordStrength(password) {
          const specialCharRegex = /[!@#$%^&*(),.?":{}|<>\-_=+\\\/]/; // Special characters
          const uppercaseRegex = /[A-Z]/; // Uppercase letters
          const lowercaseRegex = /[a-z]/; // Lowercase letters
          const strengthMsg = document.getElementById("passwordStrengthMsg");

          // Validate password step by step
          if (password.length < 8) {
              strengthMsg.innerText = "❌ Password must be at least 8 characters.";
              strengthMsg.style.color = "red";
          }
          else if (!uppercaseRegex.test(password)) {
              strengthMsg.innerText = "❌ Must include at least one uppercase letter.";
              strengthMsg.style.color = "red";
          }
          else if (!lowercaseRegex.test(password)) {
              strengthMsg.innerText = "❌ Must include at least one lowercase letter.";
              strengthMsg.style.color = "red";
          }
          else if (!specialCharRegex.test(password)) {
              strengthMsg.innerText = "❌ Must include at least one special character.";
              strengthMsg.style.color = "red";
          }
          else if (password.length >= 8 && password.length < 12) {
              strengthMsg.innerText = "⚠ Medium strength. Add more characters for a stronger password.";
              strengthMsg.style.color = "orange";
          }
          else {
              strengthMsg.innerText = "✅ Strong password.";
              strengthMsg.style.color = "green";
          }

          // Always check if passwords match
          validatePasswordMatch();
      }

      function validatePasswordMatch() {
          const password = document.getElementById("<%= txtNewPassword.ClientID %>").value;
        const confirm = document.getElementById("<%= txtConfirmPassword.ClientID %>").value;
          const matchMsg = document.getElementById("passwordMatchMsg");

          if (!confirm) {
              matchMsg.innerText = "";
              return;
          }

          if (password !== confirm) {
              matchMsg.innerText = "❌ Passwords do not match.";
              matchMsg.style.color = "red";
          } else {
              matchMsg.innerText = "✅ Passwords match.";
              matchMsg.style.color = "green";
          }
      }
  </script>

</body>
</html>