<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ResetPassword.aspx.cs" Inherits="RRCManagementSystem.ResetPassword" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Reset Password - RRC Management System</title>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <style>
        body {
            font-family: Arial, sans-serif;
            background-color: #f3f4f6;
        }

        .container {
            width: 400px;
            margin: 100px auto;
            background-color: white;
            padding: 30px;
            border-radius: 10px;
            box-shadow: 0 4px 10px rgba(0,0,0,0.1);
        }

        h2 {
            text-align: center;
            color: #333;
        }

        .form-control {
            width: 100%;
            padding: 10px;
            margin-top: 10px;
            margin-bottom: 15px;
            border-radius: 4px;
            border: 1px solid #ccc;
        }

        .btn-submit {
            width: 100%;
            padding: 12px;
            background-color: #28a745;
            color: white;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            font-weight: bold;
        }

        .btn-submit:hover {
            background-color: #218838;
        }

        .validation-message {
            font-size: 13px;
        }

        .checkbox-container {
            margin-top: -10px;
            margin-bottom: 20px;
            font-size: 14px;
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

        function togglePasswords() {
            const pass1 = document.getElementById("<%= txtNewPassword.ClientID %>");
            const pass2 = document.getElementById("<%= txtConfirmPassword.ClientID %>");
            const type = pass1.type === "password" ? "text" : "password";
            pass1.type = type;
            pass2.type = type;
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <h2>Reset Password</h2>

            <asp:TextBox ID="txtNewPassword" runat="server" CssClass="form-control" TextMode="Password"
                placeholder="Enter new password" onkeyup="validatePasswordStrength(this.value)" />
            <span id="passwordStrengthMsg" class="validation-message" style="color: gray;"></span>

            <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="form-control" TextMode="Password"
                placeholder="Confirm new password" onkeyup="validatePasswordMatch()" />
            <span id="passwordMatchMsg" class="validation-message" style="color: gray;"></span>

            <div class="checkbox-container">
                <input type="checkbox" onclick="togglePasswords()" /> Show Passwords
            </div>

            <asp:Button ID="btnResetPassword" runat="server" Text="Reset Password" CssClass="btn-submit" OnClick="btnResetPassword_Click" />
            <asp:Literal ID="ltScript" runat="server" />
        </div>
    </form>
</body>
</html>