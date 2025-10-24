<%@ Page Title="" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="EditAdmin.aspx.cs" Inherits="RRCManagementSystem.EditAdmin" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <style>
        /* Main Card Styling */
        .edit-admin-card {
            border-radius: 15px;
            border: none;
            box-shadow: 0 5px 20px rgba(0, 0, 0, 0.1);
            background: #fff;
            animation: fadeIn 0.5s ease;
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
            background: linear-gradient(135deg, #198754 0%, #146c43 100%);
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

        /* Form Group Styling */
        .form-group {
            margin-bottom: 25px;
        }

        .form-group label {
            display: block;
            font-weight: 600;
            margin-bottom: 10px;
            color: #2d2d2d;
            font-size: 0.95rem;
        }

        .form-group label i {
            margin-right: 8px;
            color: #198754;
        }

        .form-control {
            width: 100%;
            padding: 12px 15px;
            border: 2px solid #e0e0e0;
            border-radius: 8px;
            font-size: 0.95rem;
            transition: all 0.3s ease;
            background-color: #fff;
        }

        .form-control:focus {
            border-color: #198754;
            box-shadow: 0 0 0 0.2rem rgba(25, 135, 84, 0.15);
            outline: none;
        }

        .form-control:read-only {
            background-color: #f8f9fa;
            cursor: not-allowed;
            color: #6c757d;
        }

        /* Section Header */
        .section-header {
            background: linear-gradient(135deg, #2d2d2d 0%, #1a1a1a 100%);
            color: white;
            padding: 15px 20px;
            border-radius: 10px;
            margin: 30px 0 20px 0;
            display: flex;
            align-items: center;
            gap: 12px;
        }

        .section-header h3 {
            margin: 0;
            font-size: 1.3rem;
            font-weight: 600;
        }

        /* Permissions Table */
        .permissions-table-container {
            border-radius: 10px;
            overflow: hidden;
            box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
            margin-bottom: 30px;
        }

        .permissions-table {
            width: 100%;
            border-collapse: collapse;
            margin: 0;
        }

        .permissions-table thead {
            background: linear-gradient(135deg, #2d2d2d 0%, #1a1a1a 100%);
            color: white;
        }

        .permissions-table thead th {
            padding: 15px;
            font-weight: 600;
            text-align: center;
            font-size: 0.9rem;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            border: none;
        }

        .permissions-table thead th:first-child {
            text-align: left;
            padding-left: 20px;
        }

        .permissions-table tbody tr {
            transition: all 0.3s ease;
            border-bottom: 1px solid #e9ecef;
        }

        .permissions-table tbody tr:hover {
            background-color: #f8f9fa;
            transform: scale(1.01);
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
        }

        .permissions-table tbody tr:last-child {
            border-bottom: none;
        }

        .permissions-table tbody td {
            padding: 15px;
            text-align: center;
            vertical-align: middle;
        }

        .permissions-table tbody td:first-child {
            text-align: left;
            padding-left: 20px;
            font-weight: 600;
            color: #2d2d2d;
        }

        /* Checkbox Styling */
        .checkbox-scale {
            width: 22px;
            height: 22px;
            cursor: pointer;
            accent-color: #198754;
            transition: transform 0.2s ease;
        }

        .checkbox-scale:hover {
            transform: scale(1.2);
        }

        .checkbox-scale:checked {
            filter: brightness(1.1);
        }

        /* Master Checkbox in Header */
        .permissions-table thead .checkbox-scale {
            width: 20px;
            height: 20px;
            vertical-align: middle;
            margin-left: 8px;
        }

        /* Column Header with Checkbox */
        .permission-header {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 8px;
        }

        /* Action Buttons */
        .action-buttons {
            display: flex;
            gap: 15px;
            margin-top: 30px;
            flex-wrap: wrap;
        }

        .btn-save {
            flex: 1;
            min-width: 200px;
            padding: 14px 30px;
            background: linear-gradient(135deg, #198754 0%, #146c43 100%);
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
        }

        .btn-save:hover {
            background: linear-gradient(135deg, #146c43 0%, #0f5132 100%);
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(25, 135, 84, 0.4);
        }

        .btn-save:active {
            transform: translateY(0);
        }

        .btn-cancel {
            flex: 1;
            min-width: 200px;
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

        /* Info Box */
        .info-box {
            background: linear-gradient(135deg, #d1ecf1 0%, #bee5eb 100%);
            border-left: 4px solid #17a2b8;
            padding: 15px 20px;
            border-radius: 10px;
            margin-bottom: 25px;
            display: flex;
            align-items: start;
            gap: 12px;
        }

        .info-box i {
            color: #0c5460;
            font-size: 1.3rem;
            margin-top: 2px;
        }

        .info-box-content {
            flex: 1;
        }

        .info-box-content strong {
            display: block;
            color: #0c5460;
            margin-bottom: 5px;
        }

        .info-box-content p {
            margin: 0;
            color: #0c5460;
            font-size: 0.9rem;
        }

        /* Breadcrumb */
        .breadcrumb {
            background: transparent;
            padding: 0;
            margin-bottom: 0;
        }

        .breadcrumb-item a {
            color: #198754;
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

        /* Responsive Design */
        @media (max-width: 992px) {
            .card-body-custom {
                padding: 25px;
            }

            .permissions-table {
                font-size: 0.9rem;
            }

            .permissions-table thead th,
            .permissions-table tbody td {
                padding: 12px 10px;
            }
        }

        @media (max-width: 768px) {
            .card-header-custom {
                padding: 20px;
            }

            .card-header-custom h2 {
                font-size: 1.4rem;
            }

            .card-body-custom {
                padding: 20px 15px;
            }

            .section-header h3 {
                font-size: 1.1rem;
            }

            .action-buttons {
                flex-direction: column;
            }

            .btn-save,
            .btn-cancel {
                width: 100%;
                min-width: auto;
            }

            /* Make table scrollable on mobile */
            .permissions-table-container {
                overflow-x: auto;
            }

            .permissions-table {
                min-width: 600px;
            }

            .permissions-table thead th,
            .permissions-table tbody td {
                padding: 10px 8px;
                font-size: 0.85rem;
            }

            .permissions-table tbody td:first-child,
            .permissions-table thead th:first-child {
                padding-left: 12px;
            }
        }

        @media (max-width: 576px) {
            .card-header-custom h2 {
                font-size: 1.2rem;
            }

            .form-control {
                font-size: 0.9rem;
            }

            .checkbox-scale {
                width: 20px;
                height: 20px;
            }
        }

        /* Custom Scrollbar */
        .permissions-table-container::-webkit-scrollbar {
            height: 8px;
        }

        .permissions-table-container::-webkit-scrollbar-track {
            background: #f1f1f1;
            border-radius: 10px;
        }

        .permissions-table-container::-webkit-scrollbar-thumb {
            background: #198754;
            border-radius: 10px;
        }

        .permissions-table-container::-webkit-scrollbar-thumb:hover {
            background: #146c43;
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
                        <li class="breadcrumb-item active" aria-current="page">Edit User</li>
                    </ol>
                </nav>
            </div>
        </div>

        <!-- Main Card -->
        <div class="card edit-admin-card">
            <!-- Card Header -->
            <div class="card-header-custom">
                <h2>
                    <i class="fas fa-user-edit"></i>
                    Edit User Account
                </h2>
            </div>

            <!-- Card Body -->
            <div class="card-body-custom">
                <!-- Alert Message -->
                <asp:Label ID="lblMessage" runat="server" />

                <!-- Info Box -->
                <div class="info-box">
                    <i class="fas fa-info-circle"></i>
                    <div class="info-box-content">
                        <strong>Important:</strong>
                        <p>Update user information and manage their module permissions below. Changes will take effect immediately after saving.</p>
                    </div>
                </div>

                <!-- User Information Form -->
                <div class="form-group">
                    <label for="txtName">
                        <i class="fas fa-user"></i>Full Name
                    </label>
                    <asp:TextBox 
                        ID="txtName" 
                        runat="server" 
                        CssClass="form-control" 
                        placeholder="Enter full name"
                        MaxLength="100" />
                </div>

                <div class="form-group">
                    <label for="txtEmail">
                        <i class="fas fa-envelope"></i>Email Address
                    </label>
                    <asp:TextBox 
                        ID="txtEmail" 
                        runat="server" 
                        CssClass="form-control" 
                        placeholder="Enter email address"
                        ReadOnly="true"
                        ToolTip="Email address cannot be changed" />
                    <small class="text-muted">
                        <i class="fas fa-lock me-1"></i>Email address is read-only and cannot be modified
                    </small>
                </div>

                <!-- Module Permissions Section -->
                <div class="section-header">
                    <i class="fas fa-shield-alt"></i>
                    <h3>Module Permissions</h3>
                </div>

                <div class="permissions-table-container">
                    <asp:Repeater ID="rptPermissions" runat="server">
                        <HeaderTemplate>
                            <table class="permissions-table">
                                <thead>
                                    <tr>
                                        <th>Module Name</th>
                                        <th>
                                            <div class="permission-header">
                                                View
                                                <input type="checkbox" class="checkbox-scale" onclick="toggleAll(this, 'chkView')" title="Toggle all View permissions" />
                                            </div>
                                        </th>
                                        <th>
                                            <div class="permission-header">
                                                Add
                                                <input type="checkbox" class="checkbox-scale" onclick="toggleAll(this, 'chkAdd')" title="Toggle all Add permissions" />
                                            </div>
                                        </th>
                                        <th>
                                            <div class="permission-header">
                                                Edit
                                                <input type="checkbox" class="checkbox-scale" onclick="toggleAll(this, 'chkEdit')" title="Toggle all Edit permissions" />
                                            </div>
                                        </th>
                                        <th>
                                            <div class="permission-header">
                                                Delete
                                                <input type="checkbox" class="checkbox-scale" onclick="toggleAll(this, 'chkDelete')" title="Toggle all Delete permissions" />
                                            </div>
                                        </th>
                                    </tr>
                                </thead>
                                <tbody>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                                <td>
                                    <i class="fas fa-cube me-2 text-muted"></i>
                                    <%# Eval("ModuleName") %>
                                    <asp:HiddenField ID="hfModuleName" runat="server" Value='<%# Eval("ModuleName") %>' />
                                </td>
                                <td class="chkView">
                                    <asp:CheckBox ID="chkView" runat="server" CssClass="checkbox-scale" Checked='<%# Eval("CanView") %>' />
                                </td>
                                <td class="chkAdd">
                                    <asp:CheckBox ID="chkAdd" runat="server" CssClass="checkbox-scale" Checked='<%# Eval("CanAdd") %>' onclick="ensureView(this)" />
                                </td>
                                <td class="chkEdit">
                                    <asp:CheckBox ID="chkEdit" runat="server" CssClass="checkbox-scale" Checked='<%# Eval("CanEdit") %>' onclick="ensureView(this)" />
                                </td>
                                <td class="chkDelete">
                                    <asp:CheckBox ID="chkDelete" runat="server" CssClass="checkbox-scale" Checked='<%# Eval("CanDelete") %>' onclick="ensureView(this)" />
                                </td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate>
                                </tbody>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>
                </div>

                <!-- Action Buttons -->
                <div class="action-buttons">
                    <button type="button" class="btn-save" onclick="confirmSave()">
                        <i class="fas fa-save"></i>
                        Save Changes
                    </button>
                    <a href="ViewAdmin.aspx" class="btn-cancel">
                        <i class="fas fa-times"></i>
                        Cancel
                    </a>
                </div>

                <!-- Hidden Save Button -->
                <asp:Button ID="btnSave" runat="server" Style="display:none;" OnClick="btnSave_Click" />
            </div>
        </div>
    </div>

<script type="text/javascript">
    // Toggle all checkboxes in a column
    function toggleAll(masterCheckbox, className) {
        const checkboxes = document.querySelectorAll(`.${className} input[type="checkbox"]`);
        checkboxes.forEach(cb => {
            cb.checked = masterCheckbox.checked;
            cb.dispatchEvent(new Event('change', { bubbles: true }));
        });

        // Add visual feedback
        if (masterCheckbox.checked) {
            Swal.fire({
                toast: true,
                position: 'top-end',
                icon: 'success',
                title: `All ${className.replace('chk', '')} permissions enabled`,
                showConfirmButton: false,
                timer: 1500
            });
        }
    }

    // Ensure View permission is checked when other permissions are checked
    function ensureView(cb) {
        const row = cb.closest('tr');
        if (!row) return;

        const viewCheckbox = row.querySelector('.chkView input[type="checkbox"]');
        if (cb.checked && viewCheckbox && !viewCheckbox.checked) {
            viewCheckbox.checked = true;

            // Show tooltip
            Swal.fire({
                toast: true,
                position: 'top-end',
                icon: 'info',
                title: 'View permission automatically enabled',
                showConfirmButton: false,
                timer: 2000
            });
        }
    }

    // Confirm save action
    function confirmSave() {
        // Validate name field
        const nameField = document.getElementById('<%= txtName.ClientID %>');
        if (!nameField.value.trim()) {
            Swal.fire({
                icon: 'error',
                title: 'Validation Error',
                text: 'Please enter the user\'s full name.',
                confirmButtonColor: '#dc3545'
            });
            nameField.focus();
            return;
        }

        // Show confirmation dialog
        Swal.fire({
            title: 'Save Changes?',
            html: '<div style="text-align:center;"><i class="fas fa-save" style="font-size:3rem;color:#198754;margin-bottom:15px;"></i><br/>This will update the user account and permissions.<br/><small class="text-muted">The changes will take effect immediately.</small></div>',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#198754',
            cancelButtonColor: '#6c757d',
            confirmButtonText: '<i class="fas fa-check me-2"></i>Yes, Save Changes',
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
                    title: 'Saving...',
                    html: 'Please wait while we update the user account.',
                    allowOutsideClick: false,
                    allowEscapeKey: false,
                    didOpen: () => {
                        Swal.showLoading();
                    }
                });

                // Trigger server-side save
                setTimeout(function () {
                    document.getElementById('<%= btnSave.ClientID %>').click();
                }, 100);
            }
        });
    }

    // Page load animations
    document.addEventListener('DOMContentLoaded', function() {
        // Animate form groups
        const formGroups = document.querySelectorAll('.form-group');
        formGroups.forEach((group, index) => {
            setTimeout(() => {
                group.style.opacity = '0';
                group.style.transform = 'translateY(20px)';
                group.style.transition = 'all 0.4s ease';
                setTimeout(() => {
                    group.style.opacity = '1';
                    group.style.transform = 'translateY(0)';
                }, 50);
            }, index * 100);
        });

        // Auto-hide success/error messages after 5 seconds
        const lblMessage = document.getElementById('<%= lblMessage.ClientID %>');
        if (lblMessage && lblMessage.textContent.trim() !== '') {
            setTimeout(function () {
                lblMessage.style.transition = 'opacity 0.5s ease';
                lblMessage.style.opacity = '0';
                setTimeout(function () {
                    lblMessage.style.display = 'none';
                }, 500);
            }, 5000);
        }
    });
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