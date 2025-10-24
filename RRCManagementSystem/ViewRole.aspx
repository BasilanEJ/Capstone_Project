<%@ Page Title="" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="ViewRole.aspx.cs" Inherits="RRCManagementSystem.ViewRole" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <style>
        /* Main Card Styling */
        .view-role-card {
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
            background: linear-gradient(135deg, #4169E1 0%, #2952CC 100%);
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

        /* Search Container */
        .search-container {
            margin-bottom: 25px;
            display: flex;
            justify-content: space-between;
            align-items: center;
            gap: 15px;
            flex-wrap: wrap;
        }

        .search-wrapper {
            position: relative;
            flex: 1;
            max-width: 450px;
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
            border-color:#4169E1;
            box-shadow: 0 0 0 0.2rem rgba(25, 135, 84, 0.15);
            outline: none;
        }

        .btn-add-role {
            padding: 12px 24px;
            background: linear-gradient(135deg, #4169E1 0%, #2952CC 100%);
            color: white;
            border: none;
            border-radius: 10px;
            font-weight: 600;
            text-decoration: none;
            transition: all 0.3s ease;
            box-shadow: 0 2px 8px rgba(25, 135, 84, 0.3);
            display: inline-flex;
            align-items: center;
            gap: 8px;
            white-space: nowrap;
        }

        .btn-add-role:hover {
            background: linear-gradient(135deg, #2952CC 0%, #0f5132 100%);
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(25, 135, 84, 0.4);
            color: white;
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

        /* Role Badge */
        .role-badge {
            display: inline-flex;
            align-items: center;
            gap: 8px;
            padding: 8px 16px;
            background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
            border-radius: 20px;
            font-weight: 600;
            color: #2d2d2d;
            border: 2px solid #dee2e6;
        }

        .role-badge i {
            color: #4169E1;
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

        .btn-view {
            background: linear-gradient(135deg, #0d6efd 0%, #0b5ed7 100%);
            color: white;
            box-shadow: 0 2px 8px rgba(13, 110, 253, 0.3);
        }

        .btn-view:hover {
            background: linear-gradient(135deg, #0b5ed7 0%, #0a58ca 100%);
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(13, 110, 253, 0.4);
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

        /* Stats Card */
        .stats-card {
            background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
            padding: 15px 20px;
            border-radius: 10px;
            margin-bottom: 25px;
            display: flex;
            align-items: center;
            gap: 15px;
            border-left: 4px solid #4169E1;
        }

        .stats-icon {
            font-size: 2.5rem;
            color: #4169E1;
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

        /* No Results Message */
        #noResultsMessage {
            padding: 40px;
            text-align: center;
            color: #6c757d;
            font-size: 1.1rem;
        }

        #noResultsMessage i {
            font-size: 3rem;
            display: block;
            margin-bottom: 15px;
            color: #dee2e6;
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

            .btn-add-role {
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

            .role-badge {
                font-size: 0.85rem;
                padding: 6px 12px;
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
            background: #4169E1;
            border-radius: 10px;
        }

        .table-responsive::-webkit-scrollbar-thumb:hover {
            background: #2952CC;
        }
    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <asp:HiddenField ID="hfRoleIDToDelete" runat="server" />
    
    <div class="container-fluid">
        <!-- Breadcrumb Navigation -->
        <div class="row mb-4">
            <div class="col-12">
                <nav aria-label="breadcrumb">
                    <ol class="breadcrumb">
                        <li class="breadcrumb-item"><a href="SuperAdminDashboard.aspx"><i class="fas fa-home me-1"></i>Dashboard</a></li>
                        <li class="breadcrumb-item active" aria-current="page">Manage Roles</li>
                    </ol>
                </nav>
            </div>
        </div>

        <!-- Main Card -->
        <div class="card view-role-card">
            <!-- Card Header -->
            <div class="card-header-custom">
                <h2>
                    <i class="fas fa-user-tag"></i>
                    Role Management
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
                        <strong>Role Management</strong>
                        <p>Roles define the permissions for users in the system. Create, view, and manage roles to control access levels.</p>
                    </div>
                </div>

                <!-- Search Bar -->
                <div class="search-container">
                    <div class="search-wrapper">
                        <i class="fas fa-search search-icon"></i>
                        <input 
                            type="text" 
                            id="txtSearchRole" 
                            class="search-input" 
                            placeholder="Search roles by name..."
                            onkeyup="filterRoles()" />
                    </div>
                    <a href="AddRole.aspx" class="btn-add-role">
                        <i class="fas fa-plus"></i>
                        Add New Role
                    </a>
                </div>

                <!-- Table Container -->
                <div class="table-container">
                    <div class="table-responsive">
                        <asp:GridView 
                            ID="gvRoles" 
                            runat="server" 
                            AutoGenerateColumns="False" 
                            CssClass="table table-hover text-center align-middle"
                            OnRowCommand="gvRoles_RowCommand"
                            DataKeyNames="RoleID"
                            EmptyDataText="No roles found.">
                            <Columns>
                                <asp:TemplateField HeaderText="Role Name">
                                    <ItemTemplate>
                                        <div class="role-badge">
                                            <i class="fas fa-user-tag"></i>
                                            <span class="role-name"><%# Eval("RoleName") %></span>
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Actions">
                                    <ItemTemplate>
                                        <div class="btn-action-group">                                                                               
                                            <button 
                                                type="button" 
                                                class="btn-action btn-delete"
                                                onclick="confirmDelete('<%# Eval("RoleID") %>', '<%# Eval("RoleName") %>')">
                                                <i class="fas fa-trash-alt"></i>
                                                Delete
                                            </button>
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>

                            <EmptyDataTemplate>
                                <div class="empty-state">
                                    <i class="fas fa-user-tag"></i>
                                    <h4>No Roles Found</h4>
                                    <p>There are currently no roles in the system.</p>
                                    <a href="AddRole.aspx" class="btn btn-success mt-3">
                                        <i class="fas fa-plus me-2"></i>Add First Role
                                    </a>
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
        // Confirm delete action
        function confirmDelete(roleId, roleName) {
            Swal.fire({
                title: 'Delete Role?',
                html: '<div style="text-align:center;"><i class="fas fa-exclamation-triangle" style="font-size:3rem;color:#dc3545;margin-bottom:15px;"></i><br/>This will <strong style="color:#dc3545;">permanently delete</strong> the role:<br/><strong style="font-size:1.2rem;color:#dc3545;">' + roleName + '</strong><br/><small class="text-muted">⚠️ Users with this role will lose their permissions!</small></div>',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#dc3545',
                cancelButtonColor: '#6c757d',
                confirmButtonText: '<i class="fas fa-trash me-2"></i>Yes, Delete Role',
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
                        title: 'Deleting Role...',
                        html: 'Please wait while we delete the role.',
                        allowOutsideClick: false,
                        allowEscapeKey: false,
                        didOpen: () => {
                            Swal.showLoading();
                        }
                    });

                    document.getElementById('<%= hfRoleIDToDelete.ClientID %>').value = roleId;
                    __doPostBack('DeleteRole', '');
                }
            });
        }

        // Filter roles by name (client-side search)
        function filterRoles() {
            var input = document.getElementById('txtSearchRole');
            var filter = input.value.toLowerCase().trim();
            var table = document.getElementById('<%= gvRoles.ClientID %>');
            
            if (!table) return;

            var rows = table.getElementsByTagName('tr');
            var visibleCount = 0;
            var hasHeader = rows.length > 0 && rows[0].getElementsByTagName('th').length > 0;

            for (var i = hasHeader ? 1 : 0; i < rows.length; i++) {
                var row = rows[i];
                
                // Skip if it's the empty data row
                if (row.querySelector('.empty-state')) continue;

                var roleNameElement = row.querySelector('.role-name');

                if (roleNameElement) {
                    var text = roleNameElement.textContent.toLowerCase();

                    if (text.includes(filter)) {
                        row.style.display = '';
                        visibleCount++;
                    } else {
                        row.style.display = 'none';
                    }
                }
            }

            // Show/hide no results message
            var existingMsg = document.getElementById('noResultsMessage');
            
            if (visibleCount === 0 && filter !== '') {
                if (!existingMsg) {
                    var msg = document.createElement('div');
                    msg.id = 'noResultsMessage';
                    msg.innerHTML = '<i class="fas fa-search-minus"></i><div class="mt-2">No roles found matching your search criteria.</div><small class="text-muted">Try adjusting your search terms.</small>';
                    document.querySelector('.table-responsive').appendChild(msg);
                } else {
                    existingMsg.style.display = 'block';
                }
            } else if (existingMsg) {
                existingMsg.style.display = 'none';
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

            // Add search on Enter key
            const searchInput = document.getElementById('txtSearchRole');
            if (searchInput) {
                searchInput.addEventListener('keypress', function (e) {
                    if (e.key === 'Enter') {
                        e.preventDefault();
                    }
                });
            }
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