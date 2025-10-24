<%@ Page Title="" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="ViewAdmin.aspx.cs" Inherits="RRCManagementSystem.ViewAdmin" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <style>
        /* Card Enhancements */
        .main-card {
            border-radius: 15px;
            border: none;
            box-shadow: 0 5px 20px rgba(0, 0, 0, 0.1);
            background: #fff;
        }

        .card-header-custom {
            background: linear-gradient(135deg,#4169E1 0%, #2952CC 100%);
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

        /* Search Box Styling */
        .search-container {
            margin-bottom: 25px;
        }

        .search-wrapper {
            position: relative;
            max-width: 450px;
            width: 100%;
        }

        .search-icon {
            position: absolute;
            left: 15px;
            top: 50%;
            transform: translateY(-50%);
            color: #6c757d;
            font-size: 1rem;
            z-index: 1;
        }

        #txtSearch {
            padding-left: 45px !important;
            border-radius: 10px;
            border: 2px solid #e0e0e0;
            height: 45px;
            font-size: 0.95rem;
            transition: all 0.3s ease;
        }

        #txtSearch:focus {
            border-color: #4169E1;
            box-shadow: 0 0 0 0.2rem rgba(25, 135, 84, 0.15);
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

        .btn-sm {
            padding: 8px 16px;
            font-size: 0.875rem;
            border-radius: 6px;
            font-weight: 500;
            transition: all 0.3s ease;
            border: none;
        }

        .btn-primary {
            background: linear-gradient(135deg, #0d6efd 0%, #0b5ed7 100%);
            box-shadow: 0 2px 8px rgba(13, 110, 253, 0.3);
        }

        .btn-primary:hover {
            background: linear-gradient(135deg, #0b5ed7 0%, #0a58ca 100%);
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(13, 110, 253, 0.4);
        }

        .btn-danger {
            background: linear-gradient(135deg, #dc3545 0%, #c82333 100%);
            box-shadow: 0 2px 8px rgba(220, 53, 69, 0.3);
        }

        .btn-danger:hover {
            background: linear-gradient(135deg, #c82333 0%, #bd2130 100%);
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(220, 53, 69, 0.4);
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

        /* Badge Styling for Roles */
        .role-badge {
            display: inline-block;
            padding: 6px 12px;
            border-radius: 20px;
            font-size: 0.85rem;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }

        /* Loading Animation */
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

        .main-card {
            animation: fadeIn 0.5s ease;
        }

        /* Responsive Design */
        @media (max-width: 768px) {
            .card-header-custom h2 {
                font-size: 1.4rem;
            }

            .card-body-custom {
                padding: 20px 15px;
            }

            .search-wrapper {
                max-width: 100%;
            }

            #txtSearch {
                height: 42px;
                font-size: 0.9rem;
            }

            .table thead th {
                font-size: 0.75rem;
                padding: 12px 8px;
            }

            .table tbody td {
                padding: 12px 8px;
                font-size: 0.875rem;
            }

            .btn-sm {
                padding: 6px 12px;
                font-size: 0.8rem;
            }

            .btn-action-group {
                gap: 5px;
            }
        }

        @media (max-width: 576px) {
            .card-header-custom {
                padding: 15px;
            }

            .card-header-custom h2 {
                font-size: 1.2rem;
            }

            .table-responsive {
                border-radius: 8px;
            }

            .btn-action-group {
                flex-direction: column;
            }

            .btn-sm {
                width: 100%;
            }
        }

        /* Custom Scrollbar for Table */
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
    <div class="container-fluid">
        <!-- Page Header -->
        <div class="row mb-4">
            <div class="col-12">
                <nav aria-label="breadcrumb">
                    <ol class="breadcrumb">
                        <li class="breadcrumb-item"><a href="SuperAdminDashboard.aspx"><i class="fas fa-home me-1"></i>Dashboard</a></li>
                        <li class="breadcrumb-item active" aria-current="page">User Accounts</li>
                    </ol>
                </nav>
            </div>
        </div>

        <!-- Main Card -->
        <div class="card main-card">
            <!-- Card Header -->
            <div class="card-header-custom">
                <h2>
                    <i class="fas fa-users"></i>
                    User Accounts Management
                </h2>
            </div>

            <!-- Card Body -->
            <div class="card-body-custom">
                <!-- Search Box -->
                <div class="search-container d-flex justify-content-between align-items-center flex-wrap">
                    <div class="search-wrapper">
                        <asp:TextBox 
                            ID="txtSearch" 
                            runat="server"
                            placeholder="Search by name, email, or Employee ID..."
                            CssClass="form-control"
                            onkeyup="filterAdmins()" />
                    </div>
                    <div class="mt-3 mt-md-0">
                        <a href="AddAdmin.aspx" class="btn mt-3" style="background: #4169E1; color: white; border: none;">
                            <i class="fas fa-plus me-2"></i>Add New User
                        </a>
                    </div>
                </div>

                <!-- Table Container -->
                <div class="table-container">
                    <div class="table-responsive">
                        <asp:GridView 
                            ID="gvAdmins" 
                            runat="server" 
                            CssClass="table table-hover text-center align-middle"
                            AutoGenerateColumns="False"
                            EmptyDataText="No users found."
                            DataKeyNames="UserID"
                            OnRowCommand="gvAdmins_RowCommand">
                            <Columns>
                                <asp:BoundField DataField="UserID" HeaderText="UserID" Visible="False" />
                                
                                <asp:TemplateField HeaderText="Employee ID">
                                    <ItemTemplate>
                                        <span class="admin-empid fw-bold text-primary">
                                            <%# Eval("EmployeeID") %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Name">
                                    <ItemTemplate>
                                        <div class="admin-name d-flex align-items-center justify-content-center">
                                            <i class="fas fa-user-circle me-2 text-muted"></i>
                                            <span class="fw-semibold"><%# Eval("Name") %></span>
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Email">
                                    <ItemTemplate>
                                        <div class="admin-email">
                                            <i class="fas fa-envelope me-2 text-muted"></i>
                                            <span><%# Eval("Email") %></span>
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Role">
                                    <ItemTemplate>
                                        <span class="admin-role role-badge bg-info text-white">
                                            <%# Eval("Role") %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Actions">
                                    <ItemTemplate>
                                        <div class="btn-action-group">
                                            <asp:LinkButton ID="btnEdit" runat="server"
                                                CommandName="EditAdmin"
                                                CommandArgument='<%# Eval("UserID") %>'
                                                CssClass="btn btn-sm btn-primary"
                                                ToolTip="Edit User">
                                                <i class="fas fa-edit me-1"></i>Edit
                                            </asp:LinkButton>

                                            <asp:LinkButton ID="btnDelete" runat="server"
                                                CssClass="btn btn-sm btn-danger"
                                                ToolTip="Archive User"
                                                OnClientClick='<%# "return showArchiveConfirmation(" + Eval("UserID") + ", \u0027" + Eval("EmployeeID") + "\u0027);" %>'>
                                                <i class="fas fa-archive me-1"></i>Archive
                                            </asp:LinkButton>
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>

                            <EmptyDataTemplate>
                                <div class="empty-state">
                                    <i class="fas fa-users-slash"></i>
                                    <h4>No Users Found</h4>
                                    <p class="text-muted">There are currently no user accounts in the system.</p>
                                    <a href="AddAdmin.aspx" class="btn btn-success mt-3">
                                        <i class="fas fa-plus me-2"></i>Add First User
                                    </a>
                                </div>
                            </EmptyDataTemplate>
                        </asp:GridView>
                    </div>
                </div>

                <!-- Message Label -->
                <asp:Label ID="lblMessage" runat="server" CssClass="text-danger fw-bold mt-3 d-block text-center" />
            </div>
        </div>
    </div>

    <!-- Hidden Fields and Buttons -->
    <asp:HiddenField ID="hfUserToArchive" runat="server" />
    <asp:Button ID="btnConfirmArchive" runat="server" Style="display:none;" OnClick="btnConfirmArchive_Click" />

    <!-- JavaScript Section -->
    <script type="text/javascript">
        // Archive Confirmation Dialog
        function showArchiveConfirmation(userId, employeeId) {
            if (window.event) window.event.preventDefault();

            Swal.fire({
                title: 'Archive User Account?',
                html: '<div style="text-align:center;"><i class="fas fa-archive" style="font-size:3rem;color:#dc3545;margin-bottom:15px;"></i><br/>This will archive the user account:<br/><strong style="font-size:1.2rem;color:#4169E1;">' + employeeId + '</strong><br/><small class="text-muted">You can restore this account later from the Archive.</small></div>',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#dc3545',
                cancelButtonColor: '#6c757d',
                confirmButtonText: '<i class="fas fa-check me-2"></i>Yes, Archive It!',
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
                        title: 'Archiving...',
                        html: 'Please wait while we archive the user account.',
                        allowOutsideClick: false,
                        didOpen: () => {
                            Swal.showLoading();
                        }
                    });

                    setTimeout(function () {
                        document.getElementById('<%= hfUserToArchive.ClientID %>').value = userId;
                        document.getElementById('<%= btnConfirmArchive.ClientID %>').click();
                    }, 50);
                }
            });

            return false;
        }

        // Instant Search Filter (Client-Side)
        function filterAdmins() {
            var input = document.getElementById('<%= txtSearch.ClientID %>');
            var filter = input.value.toLowerCase().trim();
            var table = document.getElementById('<%= gvAdmins.ClientID %>');
            
            if (!table) return;

            var rows = table.getElementsByTagName('tr');
            var visibleCount = 0;
            var hasHeader = rows.length > 0 && rows[0].getElementsByTagName('th').length > 0;

            for (var i = hasHeader ? 1 : 0; i < rows.length; i++) {
                var row = rows[i];
                
                // Skip if it's the empty data row
                if (row.querySelector('.empty-state')) continue;

                var empid = row.querySelector('.admin-empid');
                var name = row.querySelector('.admin-name');
                var email = row.querySelector('.admin-email');
                var role = row.querySelector('.admin-role');

                if (empid && name && email && role) {
                    var text = (
                        empid.textContent + ' ' +
                        name.textContent + ' ' +
                        email.textContent + ' ' +
                        role.textContent
                    ).toLowerCase();

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
                    msg.innerHTML = '<i class="fas fa-search-minus"></i><div class="mt-2">No users found matching your search criteria.</div><small class="text-muted">Try adjusting your search terms.</small>';
                    document.querySelector('.table-responsive').appendChild(msg);
                } else {
                    existingMsg.style.display = 'block';
                }
            } else if (existingMsg) {
                existingMsg.style.display = 'none';
            }
        }

        // Add smooth scroll to top after actions
        window.addEventListener('load', function() {
            // Check if there's a success message
            var lblMessage = document.getElementById('<%= lblMessage.ClientID %>');
            if (lblMessage && lblMessage.textContent.trim() !== '') {
                window.scrollTo({ top: 0, behavior: 'smooth' });

                // Auto-hide message after 5 seconds
                setTimeout(function () {
                    lblMessage.style.transition = 'opacity 0.5s';
                    lblMessage.style.opacity = '0';
                    setTimeout(function () {
                        lblMessage.style.display = 'none';
                    }, 500);
                }, 5000);
            }
        });

        // Add loading state to edit buttons
        document.addEventListener('DOMContentLoaded', function () {
            var editButtons = document.querySelectorAll('[id*="btnEdit"]');
            editButtons.forEach(function (btn) {
                btn.addEventListener('click', function () {
                    this.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Loading...';
                    this.disabled = true;
                });
            });
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

        /* Breadcrumb Styling */
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
            color:#2952CC;
            text-decoration: underline;
        }

        .breadcrumb-item.active {
            color: #6c757d;
        }

        .breadcrumb-item + .breadcrumb-item::before {
            content: "›";
            color: #6c757d;
        }
    </style>
</asp:Content>