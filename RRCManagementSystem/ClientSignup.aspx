<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ClientSignup.aspx.cs" Inherits="RRCManagementSystem.ClientSignup" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=5.0, user-scalable=yes" />
    <meta charset="UTF-8" />
    <link rel="icon" type="image/png" href="~/Images/rrc-logo.jpg" />
    <title>Sign Up - RRC Management System</title>

    <link href="https://fonts.googleapis.com/css2?family=Poppins:wght@400;500;600;700&display=swap" rel="stylesheet" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.2/css/all.min.css" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <style>
        * {
            box-sizing: border-box;
            margin: 0;
            padding: 0;
        }

        body, html {
            width: 100%;
            min-height: 100%;
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
            padding: 30px 20px;
        }

        .signup-container {
            background: rgba(255, 255, 255, 0.98);
            backdrop-filter: blur(10px);
            border-radius: 20px;
            box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
            width: 100%;
            max-width: 900px;
            padding: 50px 40px;
            display: flex;
            flex-direction: column;
            align-items: center;
            text-align: center;
            border: 1px solid rgba(255, 255, 255, 0.3);
            transition: all 0.4s ease;
            animation: fadeInUp 0.6s ease;
            margin: 20px auto;
        }

        @keyframes fadeInUp {
            from {
                opacity: 0;
                transform: translateY(30px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }

        .logo {
            width: 100%;
            max-width: 180px;
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
            font-size: 32px;
            margin-bottom: 10px;
            font-weight: 700;
            letter-spacing: -0.5px;
        }

        .subtitle {
            color: #666;
            font-size: 15px;
            margin-bottom: 30px;
            font-weight: 400;
        }

        .form-content {
            width: 100%;
        }

        .form-row {
            display: grid;
            grid-template-columns: 1fr;
            gap: 20px;
            margin-bottom: 20px;
        }

        .form-row.two-cols {
            grid-template-columns: repeat(2, 1fr);
        }

        .form-row.three-cols {
            grid-template-columns: repeat(3, 1fr);
        }

        .input-group {
            text-align: left;
            min-width: 0;
        }

        .input-group label {
            display: block;
            font-size: 14px;
            font-weight: 500;
            color: #555;
            margin-bottom: 6px;
        }

        .input-group label .required {
            color: #dc3545;
            margin-left: 2px;
        }

        .input, .select {
            width: 100%;
            padding: 14px 18px;
            font-size: 16px;
            border: 2px solid #e0e0e0;
            border-radius: 12px;
            background: #f8f9fa;
            color: #333;
            transition: all 0.3s ease;
            font-family: 'Poppins', sans-serif;
            outline: none;
            -webkit-appearance: none;
            -moz-appearance: none;
            appearance: none;
        }

        .input:focus, .select:focus {
            border-color: #007bff;
            background: #ffffff;
            box-shadow: 0 0 0 4px rgba(0, 123, 255, 0.1);
            transform: translateY(-2px);
        }

        .input::placeholder {
            color: #999;
        }

        .select {
            background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='12' height='12' viewBox='0 0 12 12'%3E%3Cpath fill='%23333' d='M6 9L1 4h10z'/%3E%3C/svg%3E");
            background-repeat: no-repeat;
            background-position: right 15px center;
            padding-right: 40px;
        }

        .password-wrapper {
            position: relative;
            width: 100%;
        }

        .password-wrapper .input {
            padding-right: 50px;
        }

        .toggle-password {
            position: absolute;
            right: 12px;
            top: 50%;
            transform: translateY(-50%);
            background: none;
            border: none;
            cursor: pointer;
            color: #888;
            font-size: 18px;
            padding: 8px;
            border-radius: 6px;
            transition: all 0.3s ease;
            display: flex;
            align-items: center;
            justify-content: center;
            min-width: 40px;
            min-height: 40px;
        }

        .toggle-password:hover {
            background: rgba(0, 123, 255, 0.1);
            color: #007bff;
        }

        .password-strength {
            margin-top: 8px;
            font-size: 12px;
            text-align: left;
        }

        .password-strength .strength-bar {
            height: 4px;
            background: #e0e0e0;
            border-radius: 2px;
            overflow: hidden;
            margin-bottom: 6px;
        }

        .password-strength .strength-bar-fill {
            height: 100%;
            transition: width 0.3s ease, background-color 0.3s ease;
        }

        .strength-weak { background: #dc3545; width: 33%; }
        .strength-medium { background: #ffc107; width: 66%; }
        .strength-strong { background: #28a745; width: 100%; }

        .terms-container {
            text-align: left;
            margin: 20px 0;
            padding: 15px;
            background: #f8f9fa;
            border-radius: 12px;
        }

        .terms-container label {
            display: flex;
            align-items: flex-start;
            cursor: pointer;
            font-size: 14px;
            color: #555;
            line-height: 1.6;
        }

        .terms-container input[type="checkbox"] {
            margin-right: 10px;
            margin-top: 3px;
            min-width: 18px;
            width: 18px;
            height: 18px;
            cursor: pointer;
            accent-color: #007bff;
            flex-shrink: 0;
        }

        .terms-link {
            color: #007bff;
            text-decoration: underline;
            cursor: pointer;
        }

        .terms-link:hover {
            color: #0056b3;
        }

        .btn-signup {
            width: 100%;
            padding: 15px;
            font-size: 17px;
            font-weight: 600;
            color: #ffffff;
            background: linear-gradient(135deg, #007bff 0%, #0056b3 100%);
            border: none;
            border-radius: 12px;
            cursor: pointer;
            transition: all 0.3s ease;
            box-shadow: 0 4px 15px rgba(0, 123, 255, 0.3);
            margin-top: 10px;
        }

        .btn-signup:hover:not(:disabled) {
            background: linear-gradient(135deg, #0056b3 0%, #004494 100%);
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(0, 123, 255, 0.4);
        }

        .btn-signup:active:not(:disabled) {
            transform: translateY(0);
        }

        .btn-signup:disabled {
            background: #ccc;
            cursor: not-allowed;
            box-shadow: none;
        }

        .links-container {
            margin-top: 25px;
            width: 100%;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 12px;
            flex-wrap: wrap;
            font-size: 14px;
        }

        .link {
            color: #007bff;
            text-decoration: none;
            font-size: 14px;
            font-weight: 500;
            transition: all 0.3s ease;
            padding: 6px 10px;
            border-radius: 6px;
            white-space: nowrap;
        }

        .link:hover {
            color: #0056b3;
            background: rgba(0, 123, 255, 0.1);
        }

        .validation-error {
            color: #dc3545;
            font-size: 12px;
            margin-top: 4px;
            display: none;
            line-height: 1.4;
        }

        .validation-error.show {
            display: block;
        }

        /* Modal styles for Terms */
        .modal-overlay {
            display: none;
            position: fixed;
            inset: 0;
            background: rgba(0, 0, 0, 0.6);
            z-index: 9999;
            backdrop-filter: blur(4px);
            padding: 20px;
            overflow-y: auto;
        }

        .modal-overlay.show {
            display: flex;
            justify-content: center;
            align-items: flex-start;
            padding-top: 40px;
            padding-bottom: 40px;
        }

        .modal-content {
            background: white;
            border-radius: 16px;
            max-width: 800px;
            width: 100%;
            max-height: 90vh;
            box-shadow: 0 10px 40px rgba(0, 0, 0, 0.3);
            display: flex;
            flex-direction: column;
            margin: auto;
        }

        .modal-header {
            padding: 25px 30px;
            border-bottom: 2px solid #e9ecef;
            display: flex;
            justify-content: space-between;
            align-items: center;
            position: sticky;
            top: 0;
            background: white;
            border-radius: 16px 16px 0 0;
            z-index: 10;
        }

        .modal-header h3 {
            color: #1a1a1a;
            font-size: 24px;
            font-weight: 700;
            margin: 0;
        }

        .btn-close-x {
            background: none;
            border: none;
            font-size: 32px;
            color: #666;
            cursor: pointer;
            padding: 0;
            width: 35px;
            height: 35px;
            display: flex;
            align-items: center;
            justify-content: center;
            border-radius: 50%;
            transition: all 0.3s ease;
            line-height: 1;
        }

        .btn-close-x:hover {
            background: #f8f9fa;
            color: #dc3545;
            transform: rotate(90deg);
        }

        .modal-body {
            padding: 30px;
            overflow-y: auto;
            flex: 1;
        }

        .modal-body h6 {
            font-size: 16px;
            font-weight: 600;
            color: #2d3748;
            margin-top: 20px;
            margin-bottom: 10px;
        }

        .modal-body h6:first-child {
            margin-top: 0;
        }

        .modal-body p {
            font-size: 14px;
            line-height: 1.7;
            color: #4a5568;
            margin-bottom: 12px;
        }

        .modal-body ul {
            padding-left: 25px;
            margin-bottom: 15px;
        }

        .modal-body ul li {
            font-size: 14px;
            line-height: 1.7;
            color: #4a5568;
            margin-bottom: 8px;
        }

        .modal-body hr {
            border: none;
            border-top: 2px solid #007bff;
            margin: 30px 0;
        }

        .modal-body em {
            color: #6c757d;
            font-size: 13px;
        }

        .terms-agreement {
            background: #f0f8ff;
            padding: 20px;
            border-radius: 12px;
            border: 2px solid #007bff;
            margin-top: 20px;
        }

        .terms-agreement-label {
            display: flex;
            align-items: flex-start;
            cursor: pointer;
            margin: 0;
        }

        .terms-agreement-checkbox {
            margin-right: 12px;
            margin-top: 4px;
            min-width: 20px;
            width: 20px;
            height: 20px;
            cursor: pointer;
            accent-color: #007bff;
            flex-shrink: 0;
        }

        .terms-agreement-text {
            font-size: 15px;
            color: #2d3748;
            line-height: 1.6;
            font-weight: 500;
        }

        .modal-footer {
            padding: 20px 30px;
            border-top: 2px solid #e9ecef;
            display: flex;
            gap: 12px;
            justify-content: flex-end;
            position: sticky;
            bottom: 0;
            background: white;
            border-radius: 0 0 16px 16px;
        }

        .btn-accept, .btn-decline {
            padding: 12px 28px;
            font-size: 15px;
            font-weight: 600;
            border: none;
            border-radius: 8px;
            cursor: pointer;
            transition: all 0.3s ease;
        }

        .btn-accept {
            background: linear-gradient(135deg, #28a745 0%, #1e7e34 100%);
            color: white;
            box-shadow: 0 4px 12px rgba(40, 167, 69, 0.3);
        }

        .btn-accept:hover:not(:disabled) {
            background: linear-gradient(135deg, #1e7e34 0%, #155724 100%);
            transform: translateY(-2px);
            box-shadow: 0 6px 16px rgba(40, 167, 69, 0.4);
        }

        .btn-accept:disabled {
            background: #ccc;
            cursor: not-allowed;
            box-shadow: none;
        }

        .btn-decline {
            background: #6c757d;
            color: white;
        }

        .btn-decline:hover {
            background: #5a6268;
            transform: translateY(-2px);
        }

        /* Tablet and below */
        @media screen and (max-width: 1024px) {
            .form-row.three-cols {
                grid-template-columns: repeat(2, 1fr);
            }
        }

        /* Mobile devices */
        @media screen and (max-width: 768px) {
            body {
                padding: 20px 15px;
            }

            .signup-container {
                padding: 35px 25px;
                margin: 15px auto;
                border-radius: 16px;
            }

            .form-row.two-cols,
            .form-row.three-cols {
                grid-template-columns: 1fr;
                gap: 18px;
            }

            .form-row {
                margin-bottom: 18px;
            }

            h2 {
                font-size: 26px;
            }

            .subtitle {
                font-size: 14px;
                margin-bottom: 25px;
            }

            .logo {
                max-width: 150px;
                margin-bottom: 18px;
            }

            .input, .select {
                padding: 12px 16px;
                font-size: 15px;
            }

            .password-wrapper .input {
                padding-right: 45px;
            }

            .toggle-password {
                min-width: 36px;
                min-height: 36px;
                font-size: 16px;
            }

            .btn-signup {
                padding: 14px;
                font-size: 16px;
            }

            .links-container {
                font-size: 13px;
                gap: 10px;
            }

            .link {
                font-size: 13px;
                padding: 5px 8px;
            }

            /* Modal responsive */
            .modal-overlay.show {
                padding-top: 20px;
                padding-bottom: 20px;
            }

            .modal-header {
                padding: 20px;
            }

            .modal-header h3 {
                font-size: 20px;
            }

            .modal-body {
                padding: 20px;
            }

            .modal-body h6 {
                font-size: 15px;
            }

            .modal-body p, .modal-body ul li {
                font-size: 13px;
            }

            .terms-agreement {
                padding: 15px;
            }

            .terms-agreement-text {
                font-size: 14px;
            }

            .modal-footer {
                padding: 15px 20px;
                flex-direction: column;
            }

            .btn-accept, .btn-decline {
                width: 100%;
                padding: 12px;
            }
        }

        /* Small phones */
        @media screen and (max-width: 480px) {
            body {
                padding: 15px 10px;
            }

            .signup-container {
                padding: 25px 20px;
                margin: 10px auto;
                border-radius: 14px;
            }

            h2 {
                font-size: 24px;
            }

            .subtitle {
                font-size: 13px;
                margin-bottom: 20px;
            }

            .logo {
                max-width: 130px;
                margin-bottom: 15px;
            }

            .input-group label {
                font-size: 13px;
                margin-bottom: 5px;
            }

            .input, .select {
                padding: 11px 14px;
                font-size: 14px;
                border-radius: 10px;
            }

            .form-row {
                gap: 15px;
                margin-bottom: 15px;
            }

            .terms-container {
                padding: 12px;
                margin: 15px 0;
                border-radius: 10px;
            }

            .terms-container label {
                font-size: 13px;
            }

            .btn-signup {
                padding: 13px;
                font-size: 15px;
                border-radius: 10px;
            }

            .links-container {
                margin-top: 20px;
                font-size: 12px;
            }

            .link {
                font-size: 12px;
                padding: 4px 6px;
            }

            .modal-overlay {
                padding: 10px;
            }

            .modal-overlay.show {
                padding-top: 15px;
                padding-bottom: 15px;
            }

            .modal-header {
                padding: 15px;
            }

            .modal-header h3 {
                font-size: 18px;
            }

            .btn-close-x {
                font-size: 28px;
                width: 30px;
                height: 30px;
            }

            .modal-body {
                padding: 15px;
            }

            .modal-body h6 {
                font-size: 14px;
                margin-top: 15px;
            }

            .modal-body p, .modal-body ul li {
                font-size: 12px;
            }

            .terms-agreement {
                padding: 12px;
            }

            .terms-agreement-checkbox {
                width: 18px;
                height: 18px;
            }

            .terms-agreement-text {
                font-size: 13px;
            }

            .modal-footer {
                padding: 12px 15px;
            }

            .btn-accept, .btn-decline {
                padding: 11px;
                font-size: 14px;
            }
        }

        /* Very small phones */
        @media screen and (max-width: 360px) {
            body {
                padding: 10px 8px;
            }

            .signup-container {
                padding: 20px 15px;
                margin: 8px auto;
            }

            h2 {
                font-size: 22px;
            }

            .subtitle {
                font-size: 12px;
            }

            .logo {
                max-width: 120px;
            }

            .input, .select {
                padding: 10px 12px;
                font-size: 14px;
            }

            .form-row {
                gap: 12px;
            }

            .btn-signup {
                padding: 12px;
                font-size: 14px;
            }

            .modal-header h3 {
                font-size: 16px;
            }

            .modal-body {
                padding: 12px;
            }
        }
    </style>
</head>

<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" />

        <div class="signup-container">
            <img src="images/logorrc.png" alt="RRC Logo" class="logo" />
            <h2>Create Your Account</h2>
            <p class="subtitle">Join RRC Termite & Pest Control</p>

            <asp:UpdatePanel ID="upSignup" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="form-content">

                        <!-- Personal Information -->
                        <div class="form-row three-cols">
                            <div class="input-group">
                                <label>Last Name <span class="required">*</span></label>
                                <asp:TextBox ID="txtLastName" runat="server" CssClass="input" 
                                    placeholder="Dela Cruz" MaxLength="100" />
                            </div>
                            <div class="input-group">
                                <label>First Name <span class="required">*</span></label>
                                <asp:TextBox ID="txtFirstName" runat="server" CssClass="input" 
                                    placeholder="Juan" MaxLength="100" />
                            </div>
                            <div class="input-group">
                                <label>Middle Name</label>
                                <asp:TextBox ID="txtMiddleName" runat="server" CssClass="input" 
                                    placeholder="Santos" MaxLength="100" />
                            </div>
                        </div>

                        <!-- Contact Information -->
                        <div class="form-row two-cols">
                            <div class="input-group">
                                <label>Email Address <span class="required">*</span></label>
                                <asp:TextBox ID="txtEmail" runat="server" CssClass="input" 
                                    TextMode="Email" placeholder="email@gmail.com" MaxLength="100" />
                                <asp:RegularExpressionValidator ID="revEmail" runat="server"
                                    ControlToValidate="txtEmail"
                                    ErrorMessage="Please enter a valid Gmail, Yahoo, Outlook, iCloud, or school/government email."
                                    ForeColor="Red" Display="Dynamic" CssClass="validation-error"
                                    ValidationExpression="^[A-Za-z0-9._%+\-]+@(?:(?:gmail|yahoo|ymail|rocketmail|outlook|hotmail|live|msn|icloud|me|mac|protonmail|proton|zoho|zohomail)\.com|(?:[A-Za-z0-9-]+\.)+edu\.ph|(?:[A-Za-z0-9-]+\.)+gov\.ph)$" />
                            </div>
                            <div class="input-group">
                                <label>Contact Number <span class="required">*</span></label>
                                <asp:TextBox ID="txtContact" runat="server" CssClass="input" 
                                    placeholder="09xxxxxxxxx" MaxLength="11" />
                                <span id="contactError" class="validation-error">Must be 11 digits starting with 09</span>
                            </div>
                        </div>

                        <!-- Address Information -->
                        <div class="form-row three-cols">
                            <div class="input-group">
                                <label>Country <span class="required">*</span></label>
                                <asp:TextBox ID="txtCountry" runat="server" CssClass="input" 
                                    Text="Philippines" MaxLength="100" />
                            </div>
                            <div class="input-group">
                                <label>Region <span class="required">*</span></label>
                                <asp:DropDownList ID="ddlRegion" runat="server" CssClass="select" 
                                    AutoPostBack="true" OnSelectedIndexChanged="ddlRegion_SelectedIndexChanged">
                                </asp:DropDownList>
                            </div>
                            <div class="input-group">
                                <label>City <span class="required">*</span></label>
                                <asp:DropDownList ID="ddlCity" runat="server" CssClass="select">
                                </asp:DropDownList>
                            </div>
                        </div>

                        <div class="form-row three-cols">
                            <div class="input-group">
                                <label>Barangay <span class="required">*</span></label>
                                <asp:TextBox ID="txtBarangay" runat="server" CssClass="input" 
                                    placeholder="Barangay" MaxLength="100" />
                            </div>
                            <div class="input-group">
                                <label>Street & Unit <span class="required">*</span></label>
                                <asp:TextBox ID="txtStreet" runat="server" CssClass="input" 
                                    placeholder="123 Main St" MaxLength="255" />
                            </div>
                            <div class="input-group">
                                <label>Landmark</label>
                                <asp:TextBox ID="txtLandmark" runat="server" CssClass="input" 
                                    placeholder="Near School" MaxLength="255" />
                            </div>
                        </div>

                        <!-- Password -->
                        <div class="form-row two-cols">
                            <div class="input-group">
                                <label>Password <span class="required">*</span></label>
                                <div class="password-wrapper">
                                    <asp:TextBox ID="txtPassword" runat="server" CssClass="input" 
                                        TextMode="Password" placeholder="Create password" MaxLength="64" />
                                    <button type="button" class="toggle-password" onclick="togglePassword('txtPassword', this)">
                                        <i class="fa-solid fa-eye"></i>
                                    </button>
                                </div>
                                <div class="password-strength">
                                    <div class="strength-bar">
                                        <div id="strengthBar" class="strength-bar-fill"></div>
                                    </div>
                                    <span id="strengthText">Password strength</span>
                                </div>
                            </div>
                            <div class="input-group">
                                <label>Confirm Password <span class="required">*</span></label>
                                <div class="password-wrapper">
                                    <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="input" 
                                        TextMode="Password" placeholder="Confirm password" MaxLength="64" />
                                    <button type="button" class="toggle-password" onclick="togglePassword('txtConfirmPassword', this)">
                                        <i class="fa-solid fa-eye"></i>
                                    </button>
                                </div>
                                <span id="passwordMatchError" class="validation-error">Passwords do not match</span>
                            </div>
                        </div>


                      <!-- Terms & Conditions -->
                            <div class="terms-container">
                                <label style="justify-content: center;">
                                    <asp:CheckBox ID="chkTerms" runat="server" style="display: none;" />
                                    <span style="text-align: center; width: 100%;">
                                        By clicking "Create Account", you agree to our 
                                        <span class="terms-link" onclick="showTermsModal()">Terms & Conditions</span> 
                                        and 
                                        <span class="terms-link" onclick="showTermsModal()">Privacy Policy</span>
                                    </span>
                                </label>
                            </div>

                        <!-- Submit Button -->
                        <asp:Button ID="btnSignup" runat="server" CssClass="btn-signup" 
                            Text="Create Account" OnClientClick="return validateForm()" 
                            OnClick="btnSignup_Click" />

                        <!-- Links -->
                        <div class="links-container">
                            <span>Already have an account?</span>
                            <a href="Login.aspx" class="link">Sign In</a>
                            <span>•</span>
                            <a href="Default.aspx" class="link">
                                <i class="fa-solid fa-arrow-left"></i> Back to Home
                            </a>
                        </div>

                    </div>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="ddlRegion" EventName="SelectedIndexChanged" />
                </Triggers>
            </asp:UpdatePanel>
        </div>

        <!-- Terms & Conditions Modal -->
        <div id="termsModal" class="modal-overlay">
            <div class="modal-content">
                <div class="modal-header">
                    <h3>Terms & Conditions and Privacy Policy</h3>
                    <button type="button" class="btn-close-x" onclick="closeTermsModal()">&times;</button>
                </div>
                
                <div class="modal-body">
                    <h6>1. Agreement to Terms</h6>
                    <p>
                        Welcome to RRC Termite &amp; Pest Control! By accessing and using our signup page,
                        you agree to comply with and be bound by these Terms and Conditions.
                        Your use of RRC services is subject to the Philippine Data Privacy Act of 2012 (DPA 2012) and other relevant laws.
                        These Terms govern your use of the signup form, the accuracy of information you provide,
                        and how we may process your submitted data. You agree to:
                    </p>
                    <ul>
                        <li>Use the signup page only for lawful purposes.</li>
                        <li>Provide truthful, accurate, and current information.</li>
                        <li>Refrain from submitting harmful, offensive, or unsolicited content.</li>
                    </ul>
                    <p>
                        If you do not agree to these Terms, you must not submit any information or otherwise use the signup page.
                        By proceeding, you acknowledge that you have read, understood, and accepted these Terms.
                    </p>

                    <h6>2. Definitions</h6>
                    <ul>
                        <li><strong>"RRC"</strong> – The official online platform of RRC Termite &amp; Pest Control,
                            provided for creating accounts, submitting inquiries, viewing bookings, bills, and booking schedules.</li>
                        <li><strong>"User"</strong> – Any individual accessing RRC, including customers, inspectors, and administrators.</li>
                        <li><strong>"Personal Data"</strong> – Information that identifies or can identify a person, as defined under the DPA 2012.</li>
                        <li><strong>"Services"</strong> – The account-related functions including registration, submission of inquiries, viewing of bookings, bills, and schedules.</li>
                    </ul>

                    <h6>3. Purpose of the Signup Page</h6>
                    <p>
                        This page is provided solely for users to create accounts, submit inquiries, view their bookings, bills, and schedules.
                        It is not intended for placing orders, entering into contracts, or seeking emergency assistance unless explicitly stated otherwise.
                    </p>

                    <h6>4. Information Accuracy</h6>
                    <p>
                        You agree that all information submitted is accurate, current, and complete.
                        You are responsible for maintaining the confidentiality of any account or contact information
                        and notifying us immediately of any unauthorized use.
                    </p>

                    <h6>5. Use Restrictions</h6>
                    <ul>
                        <li>Submitting offensive, discriminatory, defamatory, or harassing content.</li>
                        <li>Uploading or sharing harmful, dangerous, or spam content.</li>
                        <li>Using automated systems or bots to create accounts.</li>
                    </ul>
                    <p>
                        We reserve the right to decline registrations that violate these guidelines or block repeat offenders.
                    </p>

                    <h6>6. Privacy &amp; Data Use</h6>
                    <p>
                        Any personal data you submit will be processed in accordance with our Privacy Policy.
                        We keep data only as long as necessary to provide services and for legitimate business purposes.
                    </p>

                    <h6>7. Intellectual Property</h6>
                    <p>
                        All content of the signup form and related materials is our property or licensed to us.
                        You may not reproduce, distribute, modify, or create derivative works from it without written permission.
                    </p>

                    <h6>8. Disclaimer of Warranty</h6>
                    <p>
                        The signup form is provided "as-is" and "as-available."
                        We make no warranties—express or implied—regarding its accuracy, reliability, or availability.
                        All information submitted is at your own risk.
                    </p>

                    <h6>9. Limitation of Liability</h6>
                    <p>
                        To the maximum extent permitted by law, we shall not be liable for any indirect, incidental, special,
                        or consequential damages arising from the use of the signup page, even if we have been advised of the possibility of such damages.
                    </p>

                    <h6>10. Modification and Interruptions</h6>
                    <p>
                        We reserve the right to modify, suspend, or discontinue the signup page at any time, with or without notice.
                        We are not liable for any interruptions or errors and may revise these Terms at our discretion.
                    </p>

                    <h6>11. Governing Law</h6>
                    <p>
                        These Terms are governed by the laws of the Republic of the Philippines.
                        Any disputes will be subject to the jurisdiction of Philippine courts.
                    </p>

                    <h6>12. Changes to Terms</h6>
                    <p>
                        We may update these Terms periodically. Continued use after changes constitutes agreement to those revisions.
                    </p>

                    <h6>13. Severability</h6>
                    <p>
                        If any provision is found unenforceable, the remainder will remain in effect to the fullest extent permitted by law.
                    </p>

                    <h6>14. Contact Information</h6>
                    <p>
                        Email: <a href="mailto:rrctermiteandpestcontrol@gmail.com">rrctermiteandpestcontrol@gmail.com</a><br />
                        Address: #33 Kaligatasan Street, Brgy. Holy Spirit, Quezon City, Philippines
                    </p>

                    <hr />

                    <h6>Privacy Policy</h6>
                    <p><em>Last Updated: November 5, 2025</em></p>

                    <p>
                        RRC Termite &amp; Pest Control respects your privacy and is committed to protecting your personal data
                        in compliance with the Data Privacy Act of 2012 (Republic Act No. 10173) of the Philippines.
                        This Privacy Policy explains how we collect, use, store, and protect your information when you use our signup page.
                    </p>

                    <h6>1. Collection of Personal Data</h6>
                    <p>When you use the RRC signup page, we collect personal data such as:</p>
                    <ul>
                        <li>Name (First, Middle, Last)</li>
                        <li>Contact number</li>
                        <li>Email address</li>
                        <li>Address (Country, Region, City, Barangay, Street, Landmark)</li>
                        <li>Account credentials (Password - encrypted)</li>
                    </ul>

                    <h6>2. Purpose of Data Collection</h6>
                    <ul>
                        <li>To create and manage your account</li>
                        <li>To respond to your inquiries</li>
                        <li>To allow you to view your bookings, bills, and schedules</li>
                        <li>To improve our services and customer experience</li>
                        <li>To comply with legal and regulatory requirements</li>
                    </ul>

                    <h6>3. Data Sharing and Disclosure</h6>
                    <p>
                        We do not sell, trade, or otherwise transfer your personal data to third parties without your consent,
                        except when required by law, regulation, or competent authority.
                    </p>

                    <h6>4. Data Retention</h6>
                    <p>
                        We will retain your personal data only for as long as necessary to fulfill the purposes stated above,
                        and as required by applicable laws and regulations.
                    </p>

                    <h6>5. Data Security</h6>
                    <p>
                        We implement appropriate organizational, physical, and technical measures to protect your personal data
                        from unauthorized access, alteration, disclosure, or destruction. All sensitive data is encrypted using industry-standard encryption methods.
                    </p>

                    <h6>6. User Rights Under the DPA 2012</h6>
                    <ul>
                        <li>The right to be informed</li>
                        <li>The right to access</li>
                        <li>The right to rectification</li>
                        <li>The right to object (in certain cases)</li>
                        <li>The right to erasure/blocking when no longer necessary</li>
                        <li>The right to data portability</li>
                        <li>The right to lodge a complaint with the NPC</li>
                    </ul>

                    <h6>7. Updates to this Privacy Policy</h6>
                    <p>
                        We may update this Privacy Policy from time to time to reflect changes in laws, technology, or business practices.
                        Any updates will be posted on this page with a new effective date.
                    </p>

                    <h6>8. Contact Information</h6>
                    <p>
                        Email: <a href="mailto:rrctermiteandpestcontrol@gmail.com">rrctermiteandpestcontrol@gmail.com</a><br />
                        Address: #33 Kaligatasan Street, Brgy. Holy Spirit, Quezon City, Philippines
                    </p>

                    <hr />

                    <div class="terms-agreement">
                        <label class="terms-agreement-label">
                            <input type="checkbox" id="chkModalTerms" class="terms-agreement-checkbox" />
                            <span class="terms-agreement-text">
                                I have read and agree to the <strong>Terms &amp; Conditions</strong> and <strong>Privacy Policy</strong>
                            </span>
                        </label>
                    </div>
                </div>

                <div class="modal-footer">
                    <button type="button" class="btn-accept" id="btnAcceptTerms" onclick="acceptTerms()" disabled>Accept &amp; Continue</button>
                    <button type="button" class="btn-decline" onclick="closeTermsModal()">Cancel</button>
                </div>
            </div>
        </div>

        <script>
            // Enable/disable accept button based on checkbox
            document.addEventListener('DOMContentLoaded', function () {
                const modalCheckbox = document.getElementById('chkModalTerms');
                const acceptBtn = document.getElementById('btnAcceptTerms');

                if (modalCheckbox && acceptBtn) {
                    modalCheckbox.addEventListener('change', function () {
                        acceptBtn.disabled = !this.checked;
                    });
                }
            });

            // Toggle password visibility
            function togglePassword(inputId, btn) {
                const input = document.getElementById('<%= txtPassword.ClientID %>').id.includes(inputId)
                    ? document.getElementById('<%= txtPassword.ClientID %>')
                    : document.getElementById('<%= txtConfirmPassword.ClientID %>');
                const icon = btn.querySelector('i');

                if (input.type === 'password') {
                    input.type = 'text';
                    icon.classList.remove('fa-eye');
                    icon.classList.add('fa-eye-slash');
                } else {
                    input.type = 'password';
                    icon.classList.remove('fa-eye-slash');
                    icon.classList.add('fa-eye');
                }
            }

            // Password strength checker
            document.addEventListener('DOMContentLoaded', function () {
                const pwdInput = document.getElementById('<%= txtPassword.ClientID %>');
                const strengthBar = document.getElementById('strengthBar');
                const strengthText = document.getElementById('strengthText');

                if (pwdInput) {
                    pwdInput.addEventListener('input', function () {
                        const pwd = this.value;
                        let strength = 0;

                        if (pwd.length >= 8) strength++;
                        if (pwd.match(/[a-z]/)) strength++;
                        if (pwd.match(/[A-Z]/)) strength++;
                        if (pwd.match(/[0-9]/)) strength++;
                        if (pwd.match(/[^a-zA-Z0-9]/)) strength++;

                        strengthBar.className = 'strength-bar-fill';

                        if (strength <= 2) {
                            strengthBar.classList.add('strength-weak');
                            strengthText.textContent = 'Weak password';
                            strengthText.style.color = '#dc3545';
                        } else if (strength <= 4) {
                            strengthBar.classList.add('strength-medium');
                            strengthText.textContent = 'Medium password';
                            strengthText.style.color = '#ffc107';
                        } else {
                            strengthBar.classList.add('strength-strong');
                            strengthText.textContent = 'Strong password';
                            strengthText.style.color = '#28a745';
                        }
                    });
                }

                // Contact number validation
                const contactInput = document.getElementById('<%= txtContact.ClientID %>');
                const contactError = document.getElementById('contactError');

                if (contactInput) {
                    contactInput.addEventListener('input', function () {
                        this.value = this.value.replace(/\D/g, '').substring(0, 11);

                        if (this.value.length > 0 && !this.value.startsWith('09')) {
                            contactError.classList.add('show');
                        } else {
                            contactError.classList.remove('show');
                        }
                    });

                    contactInput.addEventListener('keypress', function (e) {
                        if (!/[0-9]/.test(e.key)) {
                            e.preventDefault();
                        }
                    });
                }
            });

            // Show terms modal
            function showTermsModal() {
                document.getElementById('termsModal').classList.add('show');
                document.body.style.overflow = 'hidden';
            }

            // Close terms modal
            function closeTermsModal() {
                document.getElementById('termsModal').classList.remove('show');
                document.body.style.overflow = 'auto';
                // Reset modal checkbox and button
                document.getElementById('chkModalTerms').checked = false;
                document.getElementById('btnAcceptTerms').disabled = true;
            }

            // Accept terms
            function acceptTerms() {
                const modalCheckbox = document.getElementById('chkModalTerms');
                const formCheckbox = document.getElementById('<%= chkTerms.ClientID %>');

                if (modalCheckbox.checked) {
                    formCheckbox.checked = true;
                    closeTermsModal();

                    Swal.fire({
                        icon: 'success',
                        title: 'Terms Accepted',
                        text: 'Thank you for accepting our Terms & Conditions',
                        timer: 2000,
                        showConfirmButton: false
                    });
                }
            }

            // Validate form before submit
            function validateForm() {
                const lastName = document.getElementById('<%= txtLastName.ClientID %>').value.trim();
                const firstName = document.getElementById('<%= txtFirstName.ClientID %>').value.trim();
                const email = document.getElementById('<%= txtEmail.ClientID %>').value.trim();
                const contact = document.getElementById('<%= txtContact.ClientID %>').value.trim();
                const street = document.getElementById('<%= txtStreet.ClientID %>').value.trim();
                const barangay = document.getElementById('<%= txtBarangay.ClientID %>').value.trim();
                const city = document.getElementById('<%= ddlCity.ClientID %>').value;
                const region = document.getElementById('<%= ddlRegion.ClientID %>').value;
                const country = document.getElementById('<%= txtCountry.ClientID %>').value.trim();
                const password = document.getElementById('<%= txtPassword.ClientID %>').value;
                const confirmPassword = document.getElementById('<%= txtConfirmPassword.ClientID %>').value;
                const terms = document.getElementById('<%= chkTerms.ClientID %>').checked;

                // Check required fields
                if (!lastName || !firstName || !email || !contact || !street || !barangay || !city || !region || !country || !password || !confirmPassword) {
                    Swal.fire({
                        icon: 'error',
                        title: 'Missing Information',
                        text: 'Please fill in all required fields marked with *',
                        confirmButtonColor: '#007bff'
                    });
                    return false;
                }

                // Validate contact number
                if (!/^09\d{9}$/.test(contact)) {
                    Swal.fire({
                        icon: 'error',
                        title: 'Invalid Contact Number',
                        text: 'Contact number must be 11 digits starting with 09',
                        confirmButtonColor: '#007bff'
                    });
                    return false;
                }

                // Validate password match
                if (password !== confirmPassword) {
                    document.getElementById('passwordMatchError').classList.add('show');
                    Swal.fire({
                        icon: 'error',
                        title: 'Password Mismatch',
                        text: 'Passwords do not match',
                        confirmButtonColor: '#007bff'
                    });
                    return false;
                } else {
                    document.getElementById('passwordMatchError').classList.remove('show');
                }

                // Validate password strength
                if (password.length < 8) {
                    Swal.fire({
                        icon: 'error',
                        title: 'Weak Password',
                        text: 'Password must be at least 8 characters long',
                        confirmButtonColor: '#007bff'
                    });
                    return false;
                }

                // Check terms - if not checked, show modal
                if (!terms) {
                    showTermsModal();
                    Swal.fire({
                        icon: 'warning',
                        title: 'Terms Required',
                        text: 'Please read and accept the Terms & Conditions to continue',
                        confirmButtonColor: '#007bff'
                    });
                    return false;
                }

                return true;
            }

            // Close modal on outside click
            document.getElementById('termsModal').addEventListener('click', function (e) {
                if (e.target === this) {
                    closeTermsModal();
                }
            });
        </script>
    </form>
</body>
</html>
