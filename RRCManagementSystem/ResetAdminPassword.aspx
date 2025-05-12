<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ResetAdminPassword.aspx.cs" Inherits="RRCManagementSystem.ResetAdminPassword" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Reset Admin Password - RRC Management System</title>

    <!-- Poppins Font -->
    <link href="https://fonts.googleapis.com/css2?family=Poppins:wght@400;600&display=swap" rel="stylesheet" />

    <style>
        /* Reset and Base */
        body, html {
            margin: 0;
            padding: 0;
            height: 100%;
            font-family: 'Poppins', sans-serif;
            color: #333;
        }

        /* Background */
        body {
            background: url('images/banner-1-01.jpg') no-repeat center center fixed;
            background-size: cover;
            display: flex;
            justify-content: center;
            align-items: center;
        }

        /* Glass Container */
        .reset-admin-container {
            background: rgba(255, 255, 255, 0.1);
            backdrop-filter: blur(15px);
            -webkit-backdrop-filter: blur(15px);
            border-radius: 16px;
            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.37);
            width: 350px;
            padding: 40px 30px;
            display: flex;
            flex-direction: column;
            align-items: center;
            text-align: center;
            border: 1px solid rgba(255, 255, 255, 0.18);
        }

        /* Heading */
        h2 {
            color: #ffffff;
            font-size: 26px;
            margin-bottom: 25px;
        }

        /* Input Fields */
        .input {
            width: 100%;
            padding: 14px 12px;
            margin-bottom: 15px;
            border-radius: 8px;
            border: none;
            font-size: 15px;
            outline: none;
            background: rgba(255, 255, 255, 0.2);
            color: #ffffff;
            transition: background-color 0.3s ease;
        }

        .input::placeholder {
            color: #e0e0e0;
        }

        .input:focus {
            background: rgba(255, 255, 255, 0.3);
        }

        /* Button */
        .btn-reset {
            width: 100%;
            padding: 14px;
            background-color: #28a745;
            color: #fff;
            font-size: 16px;
            border: none;
            border-radius: 8px;
            cursor: pointer;
            transition: background-color 0.3s ease, transform 0.2s ease;
        }

        .btn-reset:hover {
            background-color: #218838;
            transform: translateY(-2px);
        }

        /* Message */
        .message {
            margin-top: 15px;
            font-size: 14px;
            color: #ff4d4f;
        }

        /* Password strength and match messages */
        .validation-message {
            font-size: 13px;
            margin-bottom: 10px;
        }

        /* Responsive */
        @media screen and (max-width: 480px) {
            .reset-admin-container {
                width: 90%;
                padding: 30px 20px;
            }
        }
    </style>

    <script>
        function validatePasswordStrength(password) {
            const specialCharRegex = /[!@#$%^&*(),.?":{}|<>]/;
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
                strengthMsg.style.color = "lightgreen";
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
                matchMsg.style.color = "lightgreen";
            }
        }
    </script>
</head>

<body>
    <form id="form1" runat="server">
        <div class="reset-admin-container">
            <h2>Reset Admin Password</h2>

            <!-- Error/Success Message -->
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
            <asp:Button ID="btnResetPassword" runat="server" Text="Reset Password" CssClass="btn-reset" OnClick="btnResetPassword_Click" />
        </div>
    </form>
</body>
</html>
