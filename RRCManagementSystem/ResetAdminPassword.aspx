<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ResetAdminPassword.aspx.cs" Inherits="RRCManagementSystem.ResetAdminPassword" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Reset Admin Password - RRC Management System</title>
    <link href="https://fonts.googleapis.com/css2?family=Poppins:wght@400;600&display=swap" rel="stylesheet" />

    <style>
        body, html {
            margin: 0;
            padding: 0;
            height: 100%;
            font-family: 'Poppins', sans-serif;
            color: #333;
        }

        body {
            background: url('images/bg.jpg') no-repeat center center fixed;
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

        .login-container:hover {
            transform: translateY(-5px);
        }

        h2 {
            color: #333;
            font-size: 26px;
            margin-bottom: 25px;
        }

        .input {
            width: 100%;
            padding: 14px 12px;
            margin-bottom: 10px;
            border-radius: 8px;
            border: 1px solid #ccc;
            font-size: 15px;
            background: #f9f9f9;
            color: #333;
        }

        .input::placeholder {
            color: #999;
        }

        .input:focus {
            background: #fff;
            border-color: #007bff;
            outline: none;
        }

        .btn-reset {
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

        .btn-reset:hover {
            background-color: #0056b3;
            transform: translateY(-2px);
        }

        .message {
            margin-top: 10px;
            font-size: 14px;
            color: #ff4d4f;
        }

        .validation-message {
            font-size: 13px;
            margin-bottom: 10px;
        }

        @media screen and (max-width: 480px) {
            .login-container {
                width: 90%;
                padding: 30px 20px;
            }
        }
    </style>

    <script>
        function validatePasswordStrength(password) {
            const specialCharRegex = /[!@#$%^&*(),.?\":{}|<>]/;
            const strengthMsg = document.getElementById("passwordStrengthMsg");

            if (password.length < 8) {
                strengthMsg.innerText = "❌ Password must be at least 8 characters.";
                strengthMsg.style.color = "red";
            } else if (!specialCharRegex.test(password)) {
                strengthMsg.innerText = "❌ Must include at least one special character.";
                strengthMsg.style.color = "red";
            } else if (password.length >= 8 && password.length < 12) {
                strengthMsg.innerText = "⚠ Medium strength. Add more characters for a stronger password.";
                strengthMsg.style.color = "orange";
            } else {
                strengthMsg.innerText = "✅ Strong password.";
                strengthMsg.style.color = "green";
            }

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
</head>

<body>
    <form id="form1" runat="server">
        <div class="login-container">
            <h2>Reset Admin Password</h2>

            <!-- Server Message -->
            <asp:Label ID="lblMessage" runat="server" CssClass="message" />

            <!-- New Password -->
            <asp:TextBox ID="txtNewPassword" runat="server" CssClass="input" TextMode="Password"
                placeholder="Enter new password" onkeyup="validatePasswordStrength(this.value)" />
            <span id="passwordStrengthMsg" class="validation-message" style="color: gray;"></span>

            <!-- Confirm Password -->
            <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="input" TextMode="Password"
                placeholder="Confirm new password" onkeyup="validatePasswordMatch()" />
            <span id="passwordMatchMsg" class="validation-message" style="color: gray;"></span>

            <!-- Reset Button -->
            <asp:Button ID="btnResetPassword" runat="server" Text="Reset Password" CssClass="btn-reset"
                OnClick="btnResetPassword_Click" />
        </div>
    </form>
</body>
</html>
