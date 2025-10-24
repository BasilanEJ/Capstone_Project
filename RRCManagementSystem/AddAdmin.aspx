<%@ Page Title="" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="AddAdmin.aspx.cs" Inherits="RRCManagementSystem.AddAdmin" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <style>
        /* Main Card Styling */
        .add-admin-card {
            border-radius: 15px;
            border: none;
            box-shadow: 0 5px 20px rgba(0, 0, 0, 0.1);
            background: #fff;
            animation: fadeIn 0.5s ease;
            max-width: 750px;
            margin: 0 auto;
        }

        @keyframes fadeIn {
            from {
                opacity: 0;
                transform: translateY(20px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }

        .card-header-custom {
            background: linear-gradient(135deg, #4169E1 0%, #2952CC 100%);
            color: white;
            padding: 25px;
            border-radius: 15px 15px 0 0;
            border: none;
        }

        .card-header-custom h2 {
            margin: 0;
            font-size: 1.8rem;
            font-weight: 600;
            display: flex;
            align-items: center;
            gap: 12px;
        }

        .card-body-custom {
            padding: 35px;
        }

        /* Info Alert Box */
        .info-alert {
            background: linear-gradient(135deg, #d1ecf1 0%, #bee5eb 100%);
            border-left: 4px solid #17a2b8;
            padding: 15px 20px;
            border-radius: 10px;
            margin-bottom: 30px;
            display: flex;
            align-items: start;
            gap: 12px;
            animation: slideInLeft 0.5s ease;
        }

        @keyframes slideInLeft {
            from {
                opacity: 0;
                transform: translateX(-20px);
            }
            to {
                opacity: 1;
                transform: translateX(0);
            }
        }

        .info-alert i {
            color: #0c5460;
            font-size: 1.5rem;
            margin-top: 2px;
        }

        .info-alert-content {
            flex: 1;
            color: #0c5460;
        }

        .info-alert-content strong {
            display: block;
            margin-bottom: 5px;
            font-size: 1rem;
        }

        .info-alert-content p {
            margin: 0;
            font-size: 0.9rem;
        }

        /* Form Group Styling */
        .form-group {
            margin-bottom: 25px;
        }

        .form-label {
            display: block;
            font-weight: 600;
            margin-bottom: 10px;
            color: #2d2d2d;
            font-size: 0.95rem;
        }

        .form-label i {
            margin-right: 8px;
            color:  #4169E1;
        }

        .form-label .required {
            color: #dc3545;
            margin-left: 3px;
        }

        .form-control,
        .form-select {
            width: 100%;
            padding: 12px 15px;
            border: 2px solid #e0e0e0;
            border-radius: 8px;
            font-size: 0.95rem;
            transition: all 0.3s ease;
            background-color: #fff;
        }

        .form-control:focus,
        .form-select:focus {
            border-color: #4169E1;
            box-shadow: 0 0 0 0.2rem rgba(25, 135, 84, 0.15);
            outline: none;
        }

        .form-control::placeholder {
            color: #adb5bd;
        }

        /* Help Text */
        .form-text {
            display: block;
            margin-top: 8px;
            font-size: 0.85rem;
            color: #6c757d;
        }

        .form-text i {
            margin-right: 5px;
        }

        /* Validation Messages */
        .text-danger {
            display: block;
            margin-top: 8px;
            font-size: 0.85rem;
            color: #dc3545;
            animation: shake 0.3s ease;
        }

        @keyframes shake {
            0%, 100% { transform: translateX(0); }
            25% { transform: translateX(-5px); }
            75% { transform: translateX(5px); }
        }

        /* Alert Messages */
        .alert-success {
            background: linear-gradient(135deg, #d4edda 0%, #c3e6cb 100%);
            color: #155724;
            padding: 15px 20px;
            border-radius: 10px;
            margin-bottom: 25px;
            border-left: 4px solid #28a745;
            display: flex;
            align-items: center;
            gap: 12px;
            animation: slideInDown 0.4s ease;
        }

        .alert-danger {
            background: linear-gradient(135deg, #f8d7da 0%, #f5c6cb 100%);
            color: #721c24;
            padding: 15px 20px;
            border-radius: 10px;
            margin-bottom: 25px;
            border-left: 4px solid #dc3545;
            display: flex;
            align-items: center;
            gap: 12px;
            animation: slideInDown 0.4s ease;
        }

        @keyframes slideInDown {
            from {
                transform: translateY(-20px);
                opacity: 0;
            }
            to {
                transform: translateY(0);
                opacity: 1;
            }
        }

        .alert-icon {
            font-size: 1.5rem;
        }

        /* Action Buttons */
        .btn-create {
            width: 100%;
            padding: 14px 30px;
            background: linear-gradient(135deg,  #4169E1 0%,#2952CC 100%);
            color: white;
            border: none;
            border-radius: 10px;
            font-size: 1rem;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s ease;
            box-shadow: 0 4px 15px rgba(25, 135, 84, 0.3);
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 10px;
            margin-bottom: 15px;
        }

        .btn-create:hover {
            background: linear-gradient(135deg, #146c43 0%, #0f5132 100%);
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(25, 135, 84, 0.4);
        }

        .btn-create:active {
            transform: translateY(0);
        }

        .btn-cancel {
            width: 100%;
            padding: 14px 30px;
            background: linear-gradient(135deg, #6c757d 0%, #5a6268 100%);
            color: white;
            border: none;
            border-radius: 10px;
            font-size: 1rem;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s ease;
            box-shadow: 0 4px 15px rgba(108, 117, 125, 0.3);
            text-decoration: none;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 10px;
        }

        .btn-cancel:hover {
            background: linear-gradient(135deg, #5a6268 0%, #545b62 100%);
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(108, 117, 125, 0.4);
            color: white;
        }

        /* Input Icons */
        .input-group-custom {
            position: relative;
        }

        .input-icon {
            position: absolute;
            left: 15px;
            top: 50%;
            transform: translateY(-50%);
            color: #6c757d;
            font-size: 1rem;
            z-index: 1;
        }

        .input-group-custom .form-control {
            padding-left: 45px;
        }

        /* Breadcrumb */
        .breadcrumb {
            background: transparent;
            padding: 0;
            margin-bottom: 0;
        }

        .breadcrumb-item a {
            color:  #4169E1;
            text-decoration: none;
            transition: color 0.3s ease;
        }

        .breadcrumb-item a:hover {
            color: #146c43;
            text-decoration: underline;
        }

        .breadcrumb-item.active {
            color: #6c757d;
        }

        .breadcrumb-item + .breadcrumb-item::before {
            content: "›";
            color: #6c757d;
        }

        /* Loading State */
        .btn-loading {
            pointer-events: none;
            opacity: 0.7;
        }

        .btn-loading::after {
            content: "";
            display: inline-block;
            width: 16px;
            height: 16px;
            margin-left: 10px;
            border: 2px solid #ffffff;
            border-radius: 50%;
            border-top-color: transparent;
            animation: spinner 0.6s linear infinite;
        }

        @keyframes spinner {
            to {
                transform: rotate(360deg);
            }
        }

        /* Responsive Design */
        @media (max-width: 768px) {
            .card-header-custom {
                padding: 20px;
            }

            .card-header-custom h2 {
                font-size: 1.4rem;
            }

            .card-body-custom {
                padding: 25px 20px;
            }

            .form-control,
            .form-select {
                font-size: 0.9rem;
            }
        }

        @media (max-width: 576px) {
            .add-admin-card {
                margin: 0 10px;
            }

            .card-header-custom h2 {
                font-size: 1.2rem;
            }

            .card-body-custom {
                padding: 20px 15px;
            }

            .btn-create,
            .btn-cancel {
                padding: 12px 24px;
                font-size: 0.95rem;
            }
        }

        /* Form Animation */
        .form-group {
            opacity: 0;
            animation: fadeInUp 0.5s ease forwards;
        }

        .form-group:nth-child(1) { animation-delay: 0.1s; }
        .form-group:nth-child(2) { animation-delay: 0.2s; }
        .form-group:nth-child(3) { animation-delay: 0.3s; }
        .form-group:nth-child(4) { animation-delay: 0.4s; }

        @keyframes fadeInUp {
            from {
                opacity: 0;
                transform: translateY(20px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <!-- Breadcrumb Navigation -->
        <div class="row mb-4">
            <div class="col-12">
                <nav aria-label="breadcrumb">
                    <ol class="breadcrumb">
                        <li class="breadcrumb-item"><a href="SuperAdminDashboard.aspx"><i class="fas fa-home me-1"></i>Dashboard</a></li>
                        <li class="breadcrumb-item"><a href="ViewAdmin.aspx"><i class="fas fa-users me-1"></i>User Accounts</a></li>
                        <li class="breadcrumb-item active" aria-current="page">Add New User</li>
                    </ol>
                </nav>
            </div>
        </div>

        <!-- Main Card -->
        <div class="card add-admin-card">
            <!-- Card Header -->
            <div class="card-header-custom">
                <h2>
                    <i class="fas fa-user-plus"></i>
                    Add New User Account
                </h2>
            </div>

            <!-- Card Body -->
            <div class="card-body-custom">
                <!-- Alert Message -->
                <asp:Label ID="lblMessage" runat="server" Visible="false" />

                <!-- Info Alert -->
                <div class="info-alert">
                    <i class="fas fa-info-circle"></i>
                    <div class="info-alert-content">
                        <strong>Account Creation Process</strong>
                        <p>Once you create this account, an email invitation will be automatically sent to the user with instructions to set their password.</p>
                    </div>
                </div>

                <!-- Name Field -->
                <div class="form-group">
                    <label for="txtName" class="form-label">
                        <i class="fas fa-user"></i>
                        Full Name<span class="required">*</span>
                    </label>
                    <div class="input-group-custom">
                        <i class="fas fa-user input-icon"></i>
                        <asp:TextBox 
                            ID="txtName" 
                            runat="server" 
                            CssClass="form-control"
                            placeholder="Enter full name (e.g., Juan Dela Cruz)"
                            MaxLength="100"
                            oninput="sanitizeName(this)" />
                    </div>
                    <asp:RequiredFieldValidator 
                        ID="rfvName" 
                        runat="server"
                        ControlToValidate="txtName" 
                        CssClass="text-danger"
                        Display="Dynamic" 
                        ErrorMessage="⚠ Full name is required." />
                    <asp:RegularExpressionValidator 
                        ID="revName" 
                        runat="server"
                        ControlToValidate="txtName" 
                        CssClass="text-danger"
                        Display="Dynamic"
                        ValidationExpression="^[A-Za-zÀ-ÖØ-öø-ÿ\s'\-]+$"
                        ErrorMessage="⚠ Name can only contain letters, spaces, hyphens (-), and apostrophes (')." />
                </div>

                <!-- Email Field -->
                <div class="form-group">
                    <label for="txtEmail" class="form-label">
                        <i class="fas fa-envelope"></i>
                        Email Address<span class="required">*</span>
                    </label>
                    <div class="input-group-custom">
                        <i class="fas fa-envelope input-icon"></i>
                        <asp:TextBox 
                            ID="txtEmail" 
                            runat="server" 
                            CssClass="form-control" 
                            placeholder="Enter email address (e.g., user@gmail.com)"
                            TextMode="Email"
                            MaxLength="100" />
                    </div>
                    <asp:RequiredFieldValidator 
                        ID="rfvEmail" 
                        runat="server"
                        ControlToValidate="txtEmail" 
                        CssClass="text-danger"
                        Display="Dynamic" 
                        ErrorMessage="⚠ Email address is required." />
                    <asp:RegularExpressionValidator 
                        ID="revEmail" 
                        runat="server"
                        ControlToValidate="txtEmail" 
                        CssClass="text-danger"
                        Display="Dynamic"
                        ValidationExpression="^[^@\s]+@(gmail\.com|yahoo\.com|outlook\.com)$"
                        ErrorMessage="⚠ Email must be from Gmail, Yahoo, or Outlook domain." />
                    <small class="form-text">
                        <i class="fas fa-check-circle"></i>
                        Accepted domains: Gmail, Yahoo, Outlook
                    </small>
                </div>

                <!-- Role Field -->
                <div class="form-group">
                    <label for="ddlRole" class="form-label">
                        <i class="fas fa-user-tag"></i>
                        User Role<span class="required">*</span>
                    </label>
                    <div class="input-group-custom">
                        <asp:DropDownList 
                            ID="ddlRole" 
                            runat="server" 
                            CssClass="form-select">
                        </asp:DropDownList>
                    </div>
                    <small class="form-text">
                        <i class="fas fa-shield-alt"></i>
                        The user will inherit all permissions assigned to this role
                    </small>
                </div>



                <!-- Action Buttons -->
                <asp:Button ID="btnSubmit" runat="server" Style="display:none;" OnClick="btnSubmit_Click" />
                
                <button type="button" class="btn-create" onclick="confirmCreate()">
                    <i class="fas fa-user-plus"></i>
                    Create User Account
                </button>

                <a href="ViewAdmin.aspx" class="btn-cancel">
                    <i class="fas fa-times"></i>
                    Cancel
                </a>
            </div>
        </div>
    </div>

    <!-- JavaScript -->
    <script type="text/javascript">
        // Sanitize name input - allow only letters, spaces, hyphens, and apostrophes
        function sanitizeName(el) {
            el.value = el.value.replace(/[^A-Za-z\u00C0-\u024F\s'\-]/g, '');
        }

        // Confirm account creation
        function confirmCreate() {
            // Client-side validation
            const nameField = document.getElementById('<%= txtName.ClientID %>');
            const emailField = document.getElementById('<%= txtEmail.ClientID %>');
            const roleField = document.getElementById('<%= ddlRole.ClientID %>');

            // Check if name is filled
            if (!nameField.value.trim()) {
                Swal.fire({
                    icon: 'error',
                    title: 'Validation Error',
                    text: 'Please enter the full name.',
                    confirmButtonColor: '#dc3545'
                });
                nameField.focus();
                return;
            }

            // Check if email is filled
            if (!emailField.value.trim()) {
                Swal.fire({
                    icon: 'error',
                    title: 'Validation Error',
                    text: 'Please enter the email address.',
                    confirmButtonColor: '#dc3545'
                });
                emailField.focus();
                return;
            }

            // Check if role is selected
            if (!roleField.value || roleField.selectedIndex === 0) {
                Swal.fire({
                    icon: 'error',
                    title: 'Validation Error',
                    text: 'Please select a user role.',
                    confirmButtonColor: '#dc3545'
                });
                roleField.focus();
                return;
            }

            // Show confirmation dialog
            Swal.fire({
                title: 'Create User Account?',
                html: '<div style="text-align:center;"><i class="fas fa-user-plus" style="font-size:3rem;color: #4169E1;margin-bottom:15px;"></i><br/>This will create a new user account and send an invitation email to:<br/><strong style="font-size:1.1rem;color:#4169E1;">' + emailField.value + '</strong><br/><small class="text-muted">The user will receive instructions to set their password.</small></div>',
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: ' #4169E1',
                cancelButtonColor: '#6c757d',
                confirmButtonText: '<i class="fas fa-check me-2"></i>Yes, Create Account',
                cancelButtonText: '<i class="fas fa-times me-2"></i>Cancel',
                customClass: {
                    popup: 'animated-popup',
                    confirmButton: 'btn-confirm-custom',
                    cancelButton: 'btn-cancel-custom'
                }
            }).then((result) => {
                if (result.isConfirmed) {
                    // Show loading state
                    Swal.fire({
                        title: 'Creating Account...',
                        html: 'Please wait while we create the user account and send the invitation email.',
                        allowOutsideClick: false,
                        allowEscapeKey: false,
                        didOpen: () => {
                            Swal.showLoading();
                        }
                    });

                    // Trigger server-side save
                    setTimeout(function () {
                        document.getElementById('<%= btnSubmit.ClientID %>').click();
                    }, 100);
                }
            });
        }

        // Page load setup
        document.addEventListener('DOMContentLoaded', function() {
            // Auto-hide success/error messages after 5 seconds
            const lblMessage = document.getElementById('<%= lblMessage.ClientID %>');
            if (lblMessage && lblMessage.textContent.trim() !== '') {
                window.scrollTo({ top: 0, behavior: 'smooth' });
                
                setTimeout(function() {
                    lblMessage.style.transition = 'opacity 0.5s ease';
                    lblMessage.style.opacity = '0';
                    setTimeout(function() {
                        lblMessage.style.display = 'none';
                    }, 500);
                }, 5000);
            }

            // Add input event listeners for real-time validation feedback
            const inputs = document.querySelectorAll('.form-control, .form-select');
            inputs.forEach(input => {
                input.addEventListener('focus', function() {
                    this.style.transform = 'scale(1.02)';
                    this.style.transition = 'transform 0.2s ease';
                });

                input.addEventListener('blur', function() {
                    this.style.transform = 'scale(1)';
                });
            });

            // Email domain helper
            const emailField = document.getElementById('<%= txtEmail.ClientID %>');
            emailField.addEventListener('blur', function () {
                const email = this.value.trim();
                if (email && !email.includes('@')) {
                    Swal.fire({
                        toast: true,
                        position: 'top-end',
                        icon: 'info',
                        title: 'Remember to include @gmail.com, @yahoo.com, or @outlook.com',
                        showConfirmButton: false,
                        timer: 3000
                    });
                }
            });
        });

        // Prevent form resubmission on page refresh
        if (window.history.replaceState) {
            window.history.replaceState(null, null, window.location.href);
        }
    </script>

    <style>
        /* SweetAlert Custom Styling */
        .animated-popup {
            animation: slideInDown 0.3s ease;
        }

        @keyframes slideInDown {
            from {
                transform: translateY(-50px);
                opacity: 0;
            }
            to {
                transform: translateY(0);
                opacity: 1;
            }
        }

        .btn-confirm-custom,
        .btn-cancel-custom {
            padding: 10px 24px !important;
            font-weight: 600 !important;
            border-radius: 8px !important;
        }
    </style>
</asp:Content>