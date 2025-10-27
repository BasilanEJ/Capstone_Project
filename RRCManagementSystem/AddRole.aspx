<%@ Page Title="" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="AddRole.aspx.cs" Inherits="RRCManagementSystem.AddRole" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <style>
        /* Main Card Styling */
        .add-role-card {
            border-radius: 15px;
            border: none;
            box-shadow: 0 5px 20px rgba(0, 0, 0, 0.1);
            background: #fff;
            animation: fadeIn 0.5s ease;
            max-width: 950px;
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
            color: #4169E1;
        }

        .form-label .required {
            color: #dc3545;
            margin-left: 3px;
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
            border-color: #4169E1;
            box-shadow: 0 0 0 0.2rem rgba(25, 135, 84, 0.15);
            outline: none;
        }

        .form-control::placeholder {
            color: #adb5bd;
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
        .form-check-input {
            width: 22px;
            height: 22px;
            cursor: pointer;
            accent-color: #4169E1;
            transition: transform 0.2s ease;
        }

        .form-check-input:hover {
            transform: scale(1.2);
        }

        .form-check-input:checked {
            filter: brightness(1.1);
        }

        /* Action Buttons */
        .action-buttons {
            display: flex;
            gap: 15px;
            margin-top: 30px;
            flex-wrap: wrap;
            justify-content: center;
        }

        .btn-create {
            min-width: 200px;
            padding: 14px 30px;
             background: linear-gradient(135deg, #4169E1 0%, #2952CC 100%);
            color: white;
            border: none;
            border-radius: 10px;
            font-size: 1rem;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s ease;
            box-shadow: 0 4px 15px rgba(25, 135, 84, 0.3);
            display: inline-flex;
            align-items: center;
            justify-content: center;
            gap: 10px;
        }

        .btn-create:hover {
            background: linear-gradient(135deg, #2952CC 0%, #0f5132 100%);
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(25, 135, 84, 0.4);
        }

        .btn-create:active {
            transform: translateY(0);
        }

        .btn-cancel {
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
            display: inline-flex;
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

        /* Input Group with Icon */
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
            color: #4169E1;
            text-decoration: none;
            transition: color 0.3s ease;
        }

        .breadcrumb-item a:hover {
            color: #2952CC;
            text-decoration: underline;
        }

        .breadcrumb-item.active {
            color: #6c757d;
        }

        .breadcrumb-item + .breadcrumb-item::before {
            content: "›";
            color: #6c757d;
        }

        /* Helper Text */
        .helper-text {
            display: flex;
            align-items: center;
            gap: 8px;
            margin-top: 8px;
            font-size: 0.85rem;
            color: #6c757d;
        }

        .helper-text i {
            color: #4169E1;
        }

        /* Quick Action Buttons */
        .quick-actions {
            display: flex;
            gap: 10px;
            margin-bottom: 20px;
            flex-wrap: wrap;
        }

        .btn-quick {
            padding: 8px 16px;
            background: linear-gradient(135deg, #17a2b8 0%, #138496 100%);
            color: white;
            border: none;
            border-radius: 6px;
            font-size: 0.875rem;
            font-weight: 500;
            cursor: pointer;
            transition: all 0.3s ease;
            box-shadow: 0 2px 8px rgba(23, 162, 184, 0.3);
        }

        .btn-quick:hover {
            background: linear-gradient(135deg, #138496 0%, #117a8b 100%);
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(23, 162, 184, 0.4);
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

            .btn-create,
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

            .form-check-input {
                width: 20px;
                height: 20px;
            }

            .quick-actions {
                flex-direction: column;
            }

            .btn-quick {
                width: 100%;
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
            background: #4169E1;
            border-radius: 10px;
        }

        .permissions-table-container::-webkit-scrollbar-thumb:hover {
            background: #2952CC;
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
                        <li class="breadcrumb-item"><a href="ViewRole.aspx"><i class="fas fa-user-tag me-1"></i>Roles</a></li>
                        <li class="breadcrumb-item active" aria-current="page">Add New Role</li>
                    </ol>
                </nav>
            </div>
        </div>

        <!-- Main Card -->
        <div class="card add-role-card">
            <!-- Card Header -->
            <div class="card-header-custom">
                <h2>
                    <i class="fas fa-user-tag"></i>
                    Add New Role
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
                        <strong>Role Creation</strong>
                        <p>Define the permissions for this role. All users assigned to this role will automatically inherit these permissions.</p>
                    </div>
                </div>

                <!-- Role Name Field -->
                <div class="form-group">
                    <label for="txtRoleName" class="form-label">
                        <i class="fas fa-tag"></i>
                        Role Name<span class="required">*</span>
                    </label>
                    <div class="input-group-custom">
                        <i class="fas fa-tag input-icon"></i>
                        <asp:TextBox 
                            ID="txtRoleName" 
                            runat="server" 
                            CssClass="form-control"
                            placeholder="e.g., Manager, Staff, Supervisor, Inspector"
                            MaxLength="100" />
                    </div>
                    <asp:RequiredFieldValidator 
                        ID="rfvRoleName" 
                        runat="server"
                        ControlToValidate="txtRoleName" 
                        CssClass="text-danger"
                        Display="Dynamic" 
                        ErrorMessage="⚠ Role name is required." />
                    <div class="helper-text">
                        <i class="fas fa-lightbulb"></i>
                        <span>Choose a descriptive name that clearly identifies the role's function</span>
                    </div>
                </div>

                <!-- Module Permissions Section -->
                <div class="section-header">
                    <i class="fas fa-shield-alt"></i>
                    <h3>Module Permissions</h3>
                </div>


                <!-- Permissions Table -->
                <div class="permissions-table-container">
                    <asp:Repeater ID="rptPermissions" runat="server">
                        <HeaderTemplate>
                            <table class="permissions-table">
                                <thead>
                                    <tr>
                                        <th>Module Name</th>
                                        <th><i class="fas fa-eye me-2"></i>View</th>
                                        <th><i class="fas fa-plus me-2"></i>Add</th>
                                        <th><i class="fas fa-edit me-2"></i>Edit</th>
                                        <th><i class="fas fa-trash me-2"></i>Delete</th>
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
                                <td>
                                    <asp:CheckBox 
                                        ID="chkView" 
                                        runat="server" 
                                        Checked='<%# Eval("CanView") %>' 
                                        CssClass="form-check-input chkView" 
                                        onclick="toggleAllInRow(this, 'view')" />
                                </td>
                                <td>
                                    <asp:CheckBox 
                                        ID="chkAdd" 
                                        runat="server" 
                                        Checked='<%# Eval("CanAdd") %>' 
                                        CssClass="form-check-input chkAdd" 
                                        onclick="ensureView(this)" />
                                </td>
                                <td>
                                    <asp:CheckBox 
                                        ID="chkEdit" 
                                        runat="server" 
                                        Checked='<%# Eval("CanEdit") %>' 
                                        CssClass="form-check-input chkEdit" 
                                        onclick="ensureView(this)" />
                                </td>
                                <td>
                                    <asp:CheckBox 
                                        ID="chkDelete" 
                                        runat="server" 
                                        Checked='<%# Eval("CanDelete") %>' 
                                        CssClass="form-check-input chkDelete" 
                                        onclick="ensureView(this)" />
                                </td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate>
                                </tbody>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>
                </div>

                <!-- Required Field Note -->
                <div class="form-group">
                    <small class="text-muted">
                        <i class="fas fa-asterisk me-1" style="font-size: 0.6rem; color: #dc3545;"></i>
                        Fields marked with asterisk are required
                    </small>
                </div>

                <!-- Action Buttons -->
                <div class="action-buttons">
                    <button type="button" class="btn-create" onclick="showSaveConfirmation()">
                        <i class="fas fa-save"></i>
                        Create Role
                    </button>

                    <a href="ViewRole.aspx" class="btn-cancel">
                        <i class="fas fa-times"></i>
                        Cancel
                    </a>
                </div>

                <!-- Hidden Save Button -->
                <asp:Button ID="btnSaveHidden" runat="server" OnClick="btnSave_Click" Style="display:none;" />
            </div>
        </div>
    </div>

    <!-- JavaScript -->
    <script type="text/javascript">
        // Show save confirmation dialog
        function showSaveConfirmation() {
            // Validate role name
            const roleNameField = document.getElementById('<%= txtRoleName.ClientID %>');
    if (!roleNameField.value.trim()) {
        Swal.fire({
            icon: 'error',
            title: 'Validation Error',
            text: 'Please enter a role name.',
            confirmButtonColor: '#dc3545'
        });
        roleNameField.focus();
        return false;
    }

    // Count selected permissions - FIX: Better selector that works with ASP.NET
    const checkedBoxes = document.querySelectorAll('.permissions-table input[type="checkbox"]:checked').length;
    
    // Debug: Log the count (remove this after testing)
    console.log('Checked boxes count:', checkedBoxes);
    
    if (checkedBoxes === 0) {
        Swal.fire({
            icon: 'warning',
            title: 'No Permissions Selected',
            text: 'Please select at least one permission for this role.',
            confirmButtonColor: '#ffc107'
        });
        return false;
    }

    // Show confirmation dialog
    Swal.fire({
        title: 'Create New Role?',
        html: '<div style="text-align:center;"><i class="fas fa-user-tag" style="font-size:3rem;color:#4169E1;margin-bottom:15px;"></i><br/>This will create a new role:<br/><strong style="font-size:1.2rem;color:#4169E1;">' + roleNameField.value + '</strong><br/><small class="text-muted">With ' + checkedBoxes + ' permission(s) selected</small></div>',
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#4169E1',
        cancelButtonColor: '#6c757d',
        confirmButtonText: '<i class="fas fa-check me-2"></i>Yes, Create Role',
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
                title: 'Creating Role...',
                html: 'Please wait while we create the new role.',
                allowOutsideClick: false,
                allowEscapeKey: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            // Trigger server-side save
            setTimeout(function () {
                document.getElementById('<%= btnSaveHidden.ClientID %>').click();
            }, 100);
        }
    });

    return false;
}

        // Ensure View permission is checked when other permissions are checked
        function ensureView(cb) {
            const row = cb.closest('tr');
            if (!row) return;
            
            const viewCheckbox = row.querySelector('.chkView');
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

        // Toggle all permissions in row based on View checkbox
        function toggleAllInRow(checkbox, permission) {
            const row = checkbox.closest('tr');
            if (!row) return;

            if (permission === 'view') {
                // If unchecking View, uncheck all others
                if (!checkbox.checked) {
                    row.querySelectorAll('.chkAdd, .chkEdit, .chkDelete').forEach(cb => {
                        cb.checked = false;
                    });
                }
            }
        }

   

        // Page load setup
        document.addEventListener('DOMContentLoaded', function() {
            // Auto-hide success/error messages after 5 seconds
            const lblMessage = document.getElementById('<%= lblMessage.ClientID %>');
            if (lblMessage && lblMessage.textContent.trim() !== '') {
                window.scrollTo({ top: 0, behavior: 'smooth' });

                setTimeout(function () {
                    lblMessage.style.transition = 'opacity 0.5s ease';
                    lblMessage.style.opacity = '0';
                    setTimeout(function () {
                        lblMessage.style.display = 'none';
                    }, 500);
                }, 5000);
            }

            // Add hover effect to table rows
            const tableRows = document.querySelectorAll('.permissions-table tbody tr');
            tableRows.forEach(row => {
                row.addEventListener('mouseenter', function () {
                    this.style.transition = 'all 0.3s ease';
                });
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