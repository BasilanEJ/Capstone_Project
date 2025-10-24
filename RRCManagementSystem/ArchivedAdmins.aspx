<%@ Page Title="" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="ArchivedAdmins.aspx.cs" Inherits="RRCManagementSystem.ArchivedAdmins" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <style>
        /* Main Card Styling */
        .archived-card {
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
            background: linear-gradient(135deg, #dc3545 0%, #c82333 100%);
            color: white;
            padding: 20px;
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
            padding: 30px;
        }

        /* Search Bar Styling */
        .search-container {
            margin-bottom: 25px;
            display: flex;
            justify-content: space-between;
            align-items: center;
            gap: 15px;
            flex-wrap: wrap;
        }

        .search-wrapper {
            display: flex;
            gap: 10px;
            flex: 1;
            max-width: 500px;
        }

        .search-input-wrapper {
            position: relative;
            flex: 1;
        }

        .search-icon {
            position: absolute;
            left: 15px;
            top: 50%;
            transform: translateY(-50%);
            color: #6c757d;
            font-size: 1rem;
        }

        .search-input {
            width: 100%;
            padding: 12px 15px 12px 45px;
            border: 2px solid #e0e0e0;
            border-radius: 10px;
            font-size: 0.95rem;
            transition: all 0.3s ease;
        }

        .search-input:focus {
            border-color: #dc3545;
            box-shadow: 0 0 0 0.2rem rgba(220, 53, 69, 0.15);
            outline: none;
        }

        .btn-search {
            padding: 12px 24px;
            background: linear-gradient(135deg, #4169E1 0%, #2952CC 100%);
            color: white;
            border: none;
            border-radius: 10px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s ease;
            box-shadow: 0 2px 8px rgba(25, 135, 84, 0.3);
            white-space: nowrap;
        }

        .btn-search:hover {
            background: linear-gradient(135deg, #2952CC 0%, #0f5132 100%);
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(25, 135, 84, 0.4);
        }

        .btn-back {
            padding: 12px 24px;
            background: linear-gradient(135deg, #6c757d 0%, #5a6268 100%);
            color: white;
            border: none;
            border-radius: 10px;
            font-weight: 600;
            text-decoration: none;
            transition: all 0.3s ease;
            box-shadow: 0 2px 8px rgba(108, 117, 125, 0.3);
            display: inline-flex;
            align-items: center;
            gap: 8px;
            white-space: nowrap;
        }

        .btn-back:hover {
            background: linear-gradient(135deg, #5a6268 0%, #545b62 100%);
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(108, 117, 125, 0.4);
            color: white;
        }

        /* Info Box */
        .info-box {
            background: linear-gradient(135deg, #fff3cd 0%, #ffeeba 100%);
            border-left: 4px solid #ffc107;
            padding: 15px 20px;
            border-radius: 10px;
            margin-bottom: 25px;
            display: flex;
            align-items: start;
            gap: 12px;
        }

        .info-box i {
            color: #856404;
            font-size: 1.3rem;
            margin-top: 2px;
        }

        .info-box-content {
            flex: 1;
        }

        .info-box-content strong {
            display: block;
            color: #856404;
            margin-bottom: 5px;
        }

        .info-box-content p {
            margin: 0;
            color: #856404;
            font-size: 0.9rem;
        }

        /* Table Styling */
        .table-container {
            border-radius: 10px;
            overflow: hidden;
            box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
        }

        .table {
            margin-bottom: 0;
        }

        .table thead {
            background: linear-gradient(135deg, #2d2d2d 0%, #1a1a1a 100%);
            color: white;
        }

        .table thead th {
            font-weight: 600;
            text-transform: uppercase;
            font-size: 0.85rem;
            letter-spacing: 0.5px;
            padding: 15px;
            border: none;
            vertical-align: middle;
        }

        .table tbody tr {
            transition: all 0.3s ease;
        }

        .table tbody tr:hover {
            background-color: #f8f9fa;
            transform: scale(1.01);
            box-shadow: 0 3px 10px rgba(0, 0, 0, 0.1);
        }

        .table tbody td {
            padding: 15px;
            vertical-align: middle;
            border-color: #e9ecef;
        }

        /* Action Buttons */
        .btn-action-group {
            display: flex;
            gap: 8px;
            justify-content: center;
            flex-wrap: wrap;
        }

        .btn-action {
            padding: 8px 16px;
            border: none;
            border-radius: 6px;
            font-size: 0.875rem;
            font-weight: 500;
            cursor: pointer;
            transition: all 0.3s ease;
            text-decoration: none;
            display: inline-flex;
            align-items: center;
            gap: 6px;
        }

        .btn-restore {
            background: linear-gradient(135deg, #28a745 0%, #218838 100%);
            color: white;
            box-shadow: 0 2px 8px rgba(40, 167, 69, 0.3);
        }

        .btn-restore:hover {
            background: linear-gradient(135deg, #218838 0%, #1e7e34 100%);
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(40, 167, 69, 0.4);
            color: white;
        }

        .btn-delete {
            background: linear-gradient(135deg, #dc3545 0%, #c82333 100%);
            color: white;
            box-shadow: 0 2px 8px rgba(220, 53, 69, 0.3);
        }

        .btn-delete:hover {
            background: linear-gradient(135deg, #c82333 0%, #bd2130 100%);
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(220, 53, 69, 0.4);
            color: white;
        }

        /* Empty State */
        .empty-state {
            text-align: center;
            padding: 60px 20px;
            color: #6c757d;
        }

        .empty-state i {
            font-size: 4rem;
            margin-bottom: 20px;
            color: #dee2e6;
        }

        .empty-state h4 {
            margin-bottom: 10px;
            color: #495057;
        }

        .empty-state p {
            color: #6c757d;
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

        /* Badge for User ID */
        .user-id-badge {
            display: inline-block;
            padding: 4px 10px;
            background: linear-gradient(135deg, #6c757d 0%, #5a6268 100%);
            color: white;
            border-radius: 20px;
            font-size: 0.85rem;
            font-weight: 600;
        }

        /* Responsive Design */
        @media (max-width: 768px) {
            .card-header-custom h2 {
                font-size: 1.4rem;
            }

            .card-body-custom {
                padding: 20px 15px;
            }

            .search-container {
                flex-direction: column;
                align-items: stretch;
            }

            .search-wrapper {
                max-width: 100%;
            }

            .btn-back {
                width: 100%;
                justify-content: center;
            }

            .table thead th {
                font-size: 0.75rem;
                padding: 12px 8px;
            }

            .table tbody td {
                padding: 12px 8px;
                font-size: 0.875rem;
            }

            .btn-action {
                padding: 6px 12px;
                font-size: 0.8rem;
            }
        }

        @media (max-width: 576px) {
            .card-header-custom {
                padding: 15px;
            }

            .card-header-custom h2 {
                font-size: 1.2rem;
            }

            .table-container {
                border-radius: 8px;
            }

            .btn-action-group {
                flex-direction: column;
            }

            .btn-action {
                width: 100%;
                justify-content: center;
            }
        }

        /* Custom Scrollbar */
        .table-responsive::-webkit-scrollbar {
            height: 8px;
        }

        .table-responsive::-webkit-scrollbar-track {
            background: #f1f1f1;
            border-radius: 10px;
        }

        .table-responsive::-webkit-scrollbar-thumb {
            background: #dc3545;
            border-radius: 10px;
        }

        .table-responsive::-webkit-scrollbar-thumb:hover {
            background: #c82333;
        }

        /* Stats Card */
        .stats-card {
            background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
            padding: 15px 20px;
            border-radius: 10px;
            margin-bottom: 25px;
            display: flex;
            align-items: center;
            gap: 15px;
            border-left: 4px solid #dc3545;
        }

        .stats-icon {
            font-size: 2.5rem;
            color: #dc3545;
        }

        .stats-content h3 {
            margin: 0;
            font-size: 2rem;
            font-weight: 700;
            color: #2d2d2d;
        }

        .stats-content p {
            margin: 0;
            color: #6c757d;
            font-size: 0.9rem;
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
                        <li class="breadcrumb-item active" aria-current="page">Archived Accounts</li>
                    </ol>
                </nav>
            </div>
        </div>

        <!-- Main Card -->
        <div class="card archived-card">
            <!-- Card Header -->
            <div class="card-header-custom">
                <h2>
                    <i class="fas fa-archive"></i>
                    Archived User Accounts
                </h2>
            </div>

            <!-- Card Body -->
            <div class="card-body-custom">
                <!-- Alert Message -->
                <asp:Label ID="lblMessage" runat="server" />

                <!-- Info Box -->
                <div class="info-box">
                    <i class="fas fa-exclamation-triangle"></i>
                    <div class="info-box-content">
                        <strong>Archive Information</strong>
                        <p>These accounts are temporarily archived. You can restore them to active status or permanently delete them from the system.</p>
                    </div>
                </div>

                <!-- Search Bar -->
                <div class="search-container">
                    <div class="search-wrapper">
                        <div class="search-input-wrapper">
                            <i class="fas fa-search search-icon"></i>
                            <asp:TextBox 
                                ID="txtSearch" 
                                runat="server" 
                                CssClass="search-input" 
                                placeholder="Search by name, email, or Employee ID..." />
                        </div>
                        <asp:Button 
                            ID="btnSearch" 
                            runat="server" 
                            Text="Search" 
                            CssClass="btn-search" 
                            OnClick="btnSearch_Click" />
                    </div>
                    <a href="ViewAdmin.aspx" class="btn-back">
                        <i class="fas fa-arrow-left"></i>
                        Back to Active Users
                    </a>
                </div>

                <!-- Table Container -->
                <div class="table-container">
                    <div class="table-responsive">
                        <asp:GridView 
                            ID="gvArchivedAdmins" 
                            runat="server" 
                            CssClass="table table-hover text-center align-middle"
                            AutoGenerateColumns="False"
                            EmptyDataText="No archived accounts found."
                            OnRowCommand="gvArchivedAdmins_RowCommand"
                            DataKeyNames="UserID">
                            <Columns>
                                <asp:TemplateField HeaderText="User ID">
                                    <ItemTemplate>
                                        <span class="user-id-badge">
                                            #<%# Eval("UserID") %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Name">
                                    <ItemTemplate>
                                        <div class="d-flex align-items-center justify-content-center">
                                            <i class="fas fa-user-circle me-2 text-muted"></i>
                                            <span class="fw-semibold"><%# Eval("Name") %></span>
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Email">
                                    <ItemTemplate>
                                        <div>
                                            <i class="fas fa-envelope me-2 text-muted"></i>
                                            <span><%# Eval("Email") %></span>
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Role">
                                    <ItemTemplate>
                                        <span class="badge bg-secondary">
                                            <%# Eval("Role") %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Actions">
                                    <ItemTemplate>
                                        <div class="btn-action-group">
                                            <a href="javascript:void(0);"
                                               class="btn-action btn-restore"
                                               onclick='return confirmRestore("<%= gvArchivedAdmins.UniqueID %>", "<%# Eval("UserID") %>", "<%# Eval("Name") %>");'>
                                                <i class="fas fa-undo"></i>
                                                Restore
                                            </a>

                                            <a href="javascript:void(0);"
                                               class="btn-action btn-delete"
                                               onclick='return confirmDelete("<%= gvArchivedAdmins.UniqueID %>", "<%# Eval("UserID") %>", "<%# Eval("Name") %>");'>
                                                <i class="fas fa-trash-alt"></i>
                                                Delete
                                            </a>
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>

                            <EmptyDataTemplate>
                                <div class="empty-state">
                                    <i class="fas fa-inbox"></i>
                                    <h4>No Archived Accounts</h4>
                                    <p>There are currently no archived user accounts in the system.</p>
                                </div>
                            </EmptyDataTemplate>
                        </asp:GridView>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- JavaScript -->
    <script type="text/javascript">
        // Confirm restore action
        function confirmRestore(gridId, userId, userName) {
            Swal.fire({
                title: 'Restore User Account?',
                html: '<div style="text-align:center;"><i class="fas fa-undo" style="font-size:3rem;color:#28a745;margin-bottom:15px;"></i><br/>This will restore the account:<br/><strong style="font-size:1.2rem;color:#198754;">' + userName + '</strong><br/><small class="text-muted">The user will regain access to the system.</small></div>',
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#28a745',
                cancelButtonColor: '#6c757d',
                confirmButtonText: '<i class="fas fa-check me-2"></i>Yes, Restore Account',
                cancelButtonText: '<i class="fas fa-times me-2"></i>Cancel',
                customClass: {
                    popup: 'animated-popup',
                    confirmButton: 'btn-confirm-custom',
                    cancelButton: 'btn-cancel-custom'
                }
            }).then((result) => {
                if (result.isConfirmed) {
                    // Show loading
                    Swal.fire({
                        title: 'Restoring...',
                        html: 'Please wait while we restore the user account.',
                        allowOutsideClick: false,
                        allowEscapeKey: false,
                        didOpen: () => {
                            Swal.showLoading();
                        }
                    });

                    __doPostBack(gridId, 'RestoreAdmin$' + userId);
                }
            });
            return false;
        }

        // Confirm delete action
        function confirmDelete(gridId, userId, userName) {
            Swal.fire({
                title: 'Permanently Delete Account?',
                html: '<div style="text-align:center;"><i class="fas fa-exclamation-triangle" style="font-size:3rem;color:#dc3545;margin-bottom:15px;"></i><br/>This will <strong style="color:#dc3545;">permanently delete</strong> the account:<br/><strong style="font-size:1.2rem;color:#dc3545;">' + userName + '</strong><br/><small class="text-muted">⚠️ This action cannot be undone!</small></div>',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#dc3545',
                cancelButtonColor: '#6c757d',
                confirmButtonText: '<i class="fas fa-trash me-2"></i>Yes, Delete Permanently',
                cancelButtonText: '<i class="fas fa-times me-2"></i>Cancel',
                customClass: {
                    popup: 'animated-popup',
                    confirmButton: 'btn-confirm-custom',
                    cancelButton: 'btn-cancel-custom'
                },
                // Add extra confirmation for delete
                input: 'checkbox',
                inputValue: 0,
                inputPlaceholder: 'I understand this action cannot be undone',
                inputValidator: (result) => {
                    return !result && 'You need to confirm this action'
                }
            }).then((result) => {
                if (result.isConfirmed) {
                    // Show loading
                    Swal.fire({
                        title: 'Deleting...',
                        html: 'Please wait while we permanently delete the account.',
                        allowOutsideClick: false,
                        allowEscapeKey: false,
                        didOpen: () => {
                            Swal.showLoading();
                        }
                    });

                    __doPostBack(gridId, 'DeletePermanently$' + userId);
                }
            });
            return false;
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

            // Add search on Enter key
            const searchInput = document.getElementById('<%= txtSearch.ClientID %>');
            if (searchInput) {
                searchInput.addEventListener('keypress', function(e) {
                    if (e.key === 'Enter') {
                        e.preventDefault();
                        document.getElementById('<%= btnSearch.ClientID %>').click();
                    }
                });
            }

            // Add loading state to search button
            const searchBtn = document.getElementById('<%= btnSearch.ClientID %>');
            if (searchBtn) {
                searchBtn.addEventListener('click', function () {
                    this.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Searching...';
                    this.disabled = true;
                });
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