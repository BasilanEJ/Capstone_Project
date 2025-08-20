<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SystemAdmin.aspx.cs" Inherits="RRCManagementSystem.SystemAdmin" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <title>Create SuperAdmin</title>

    <!-- Font Awesome (for eye icon) -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.2/css/all.min.css" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>


    <style>
        body{font-family:Segoe UI,Arial,sans-serif;background:#f5f7fb;margin:0}
        .wrap{max-width:520px;margin:60px auto;background:#fff;border:1px solid #e5e7eb;border-radius:12px;
              padding:24px;box-shadow:0 6px 24px rgba(0,0,0,.06)}
        h1{margin:0 0 6px;font-size:22px}
        p.muted{margin:0 0 18px;color:#6b7280}
        .row{margin-bottom:12px}
        label{display:block;margin-bottom:6px;font-weight:600}
        input[type=text],input[type=password],input[type=email]{
            width:90%;padding:10px 42px 10px 10px;border:1px solid #d1d5db;border-radius:8px;outline:none
        }
        .input-wrap{position:relative}
        .toggle-eye{
            position:absolute;right:10px;top:50%;transform:translateY(-50%);
            cursor:pointer;color:#6b7280
        }
        .btn{background:#0d6efd;border:none;color:#fff;padding:10px 16px;border-radius:8px;cursor:pointer}
        .btn:disabled{opacity:.6;cursor:not-allowed}
        .msg{margin:8px 0 0}
        .msg.error{color:#b42318}
        .msg.ok{color:#05603a}
        .val-summary{color:#b42318;margin:8px 0}
    </style>
</head>
<body>
<form id="form1" runat="server">
    <div class="wrap">
        <h1>Create SuperAdmin</h1>
        <p class="muted">One-time setup to create your system SuperAdmin account.</p>

        <asp:ValidationSummary ID="valSummary" runat="server" CssClass="val-summary" />

        <div class="row">
            <label for="txtEmail">Email</label>
            <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" ClientIDMode="Static" />
            <asp:RequiredFieldValidator ID="rfvEmail" runat="server"
                ControlToValidate="txtEmail" Display="Dynamic" ErrorMessage="Email is required." />
            <asp:RegularExpressionValidator ID="revEmail" runat="server"
                ControlToValidate="txtEmail" Display="Dynamic"
                ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$"
                ErrorMessage="Enter a valid email." />
        </div>

        <div class="row">
            <label for="txtPassword">Password</label>
            <div class="input-wrap">
                <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" ClientIDMode="Static" />
                <i class="fa-solid fa-eye toggle-eye" id="eyePassword" title="Show/Hide"></i>
            </div>
            <asp:RequiredFieldValidator ID="rfvPass" runat="server"
                ControlToValidate="txtPassword" Display="Dynamic" ErrorMessage="Password is required." />
            <asp:RegularExpressionValidator ID="revPass" runat="server"
                ControlToValidate="txtPassword" Display="Dynamic"
                ValidationExpression="^(?=.*[A-Za-z])(?=.*\d).{8,}$"
                ErrorMessage="Min 8 chars, include letters and numbers." />
        </div>

        <div class="row">
            <label for="txtConfirm">Confirm Password</label>
            <div class="input-wrap">
                <asp:TextBox ID="txtConfirm" runat="server" TextMode="Password" ClientIDMode="Static" />
                <i class="fa-solid fa-eye toggle-eye" id="eyeConfirm" title="Show/Hide"></i>
            </div>
            <asp:RequiredFieldValidator ID="rfvConfirm" runat="server"
                ControlToValidate="txtConfirm" Display="Dynamic" ErrorMessage="Confirm your password." />
            <asp:CompareValidator ID="cmpPass" runat="server"
                ControlToValidate="txtConfirm" ControlToCompare="txtPassword"
                Display="Dynamic" ErrorMessage="Passwords do not match." />
        </div>

        <asp:Label ID="lblMessage" runat="server" CssClass="msg"></asp:Label>

        <div class="row" style="margin-top:14px">
            <asp:Button ID="btnCreate" runat="server" Text="Create SuperAdmin" CssClass="btn"
                OnClick="btnCreate_Click" />
        </div>

        <p class="muted">This page disables itself after a SuperAdmin exists.</p>
    </div>
</form>

<script>
(function(){
    function hookEye(eyeId, inputId){
        var eye = document.getElementById(eyeId);
        var input = document.getElementById(inputId);
        if(!eye || !input) return;
        eye.addEventListener('click', function(){
            var isPwd = input.getAttribute('type') === 'password';
            input.setAttribute('type', isPwd ? 'text' : 'password');
            eye.classList.toggle('fa-eye');
            eye.classList.toggle('fa-eye-slash');
        });
    }
    hookEye('eyePassword','txtPassword');
    hookEye('eyeConfirm','txtConfirm');
})();
</script>


</body>
</html>
