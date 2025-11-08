<%@ Page Title="View Teams" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ViewTeams.aspx.cs" Inherits="RRCManagementSystem.ViewTeams" %>
<%@ Import Namespace="System.Data" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Bootstrap 5 CSS -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <!-- SweetAlert2 CSS -->
    <link href="https://cdn.jsdelivr.net/npm/sweetalert2@11.7.32/dist/sweetalert2.min.css" rel="stylesheet">
    
    <style>
        /* Match GroupEmployees styling */
        .page-header {
            background: linear-gradient(135deg, #1e3a8a 0%, #1e40af 100%);
            color: white;
            padding: 2rem;
            border-radius: 12px;
            margin-bottom: 2rem;
            box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
        }

        .page-header h2 {
            margin: 0;
            font-weight: 600;
            font-size: 1.75rem;
        }

        .filter-card {
            background: white;
            padding: 1.5rem;
            border-radius: 12px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
            margin-bottom: 2rem;
        }

        .team-card {
            background: white;
            padding: 1.5rem;
            border-radius: 12px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
            margin-bottom: 1.5rem;
            border-left: 4px solid #2563eb;
        }

        .team-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 1rem;
            padding-bottom: 1rem;
            border-bottom: 2px solid #e2e8f0;
        }

        .team-title {
            font-size: 1.5rem;
            font-weight: 700;
            color: #1e3a8a;
        }

        .team-info {
            display: flex;
            gap: 0.75rem;
            align-items: center;
            flex-wrap: wrap;
        }

        .badge {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.5rem 1rem;
            border-radius: 9999px;
            font-size: 0.875rem;
            font-weight: 600;
        }

        .badge-available {
            background: linear-gradient(135deg, #10b981 0%, #059669 100%);
            color: white;
        }

        .badge-unavailable {
            background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
            color: white;
        }

        .badge-morning {
            background: linear-gradient(135deg, #fbbf24 0%, #f59e0b 100%);
            color: white;
        }

        .badge-night {
            background: linear-gradient(135deg, #6366f1 0%, #4f46e5 100%);
            color: white;
        }

        .btn-action {
            padding: 0.5rem 1rem;
            border-radius: 8px;
            font-weight: 600;
            font-size: 0.875rem;
            transition: all 0.3s ease;
            border: none;
            cursor: pointer;
        }

        .btn-edit {
            background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%);
            color: white;
        }

        .btn-edit:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(37, 99, 235, 0.4);
        }

        .btn-delete {
            background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
            color: white;
        }

        .btn-delete:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(239, 68, 68, 0.4);
        }

        .member-list {
            list-style: none;
            padding: 0;
            margin: 0;
        }

        .member-item {
            display: flex;
            align-items: center;
            gap: 1rem;
            padding: 0.75rem;
            border-radius: 8px;
            transition: background 0.2s ease;
        }

        .member-item:hover {
            background: #f8fafc;
        }

        .member-avatar {
            width: 2.5rem;
            height: 2.5rem;
            border-radius: 50%;
            background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%);
            color: white;
            display: flex;
            align-items: center;
            justify-content: center;
            font-weight: 700;
            font-size: 0.875rem;
        }

        .filter-group {
            display: flex;
            gap: 1rem;
            flex-wrap: wrap;
            align-items: end;
        }

        .filter-item {
            flex: 1;
            min-width: 200px;
        }

        .filter-label {
            display: block;
            font-weight: 600;
            color: #1e293b;
            margin-bottom: 0.5rem;
            font-size: 0.95rem;
        }

        .filter-input {
            width: 100%;
            padding: 0.75rem 1rem;
            border: 2px solid #e2e8f0;
            border-radius: 8px;
            font-size: 0.95rem;
            transition: all 0.3s ease;
        }

        .filter-input:focus {
            border-color: #2563eb;
            box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
            outline: none;
        }

        .btn-filter {
            background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%);
            color: white;
            padding: 0.75rem 2rem;
            border-radius: 8px;
            font-weight: 600;
            border: none;
            cursor: pointer;
            transition: all 0.3s ease;
        }

        .btn-filter:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(37, 99, 235, 0.4);
        }

        /* Modal styling */
        .modal-content {
            border: none;
            border-radius: 16px;
            box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
        }

        .modal-header {
            background: linear-gradient(135deg, #1e3a8a 0%, #1e40af 100%);
            color: white;
            border-radius: 16px 16px 0 0;
            padding: 1.5rem 2rem;
            border: none;
        }

        .modal-header .modal-title {
            font-weight: 600;
            font-size: 1.5rem;
            display: flex;
            align-items: center;
            gap: 0.75rem;
        }

        .modal-header .modal-title i {
            font-size: 1.75rem;
        }

        .modal-body {
            padding: 2rem;
        }

        .modal-footer {
            border: none;
            padding: 1.5rem 2rem;
            background: #f8fafc;
        }

        .form-section {
            margin-bottom: 1.5rem;
        }

        .form-section:last-child {
            margin-bottom: 0;
        }

        .form-label {
            font-weight: 600;
            color: #1e293b;
            margin-bottom: 0.5rem;
            font-size: 0.95rem;
            display: flex;
            align-items: center;
            gap: 0.5rem;
        }

        .form-label i {
            color: #2563eb;
            font-size: 1rem;
        }

        .form-control, .form-select {
            border: 2px solid #e2e8f0;
            border-radius: 8px;
            padding: 0.75rem 1rem;
            font-size: 0.95rem;
            transition: all 0.3s ease;
            width: 100%;
        }

        .form-control:focus, .form-select:focus {
            border-color: #2563eb;
            box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
            outline: none;
        }

        .form-control:disabled, .form-control[readonly] {
            background-color: #f8fafc;
            cursor: not-allowed;
        }

        .shift-type-group {
            display: flex;
            gap: 1rem;
            margin-top: 0.5rem;
        }

        .shift-option {
            flex: 1;
            position: relative;
        }

        .shift-option input[type="radio"] {
            position: absolute;
            opacity: 0;
        }

        .shift-label {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 0.5rem;
            padding: 1rem;
            border: 2px solid #e2e8f0;
            border-radius: 8px;
            cursor: pointer;
            transition: all 0.3s ease;
            background: white;
            font-weight: 500;
        }

        .shift-option input[type="radio"]:checked + .shift-label {
            background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%);
            color: white;
            border-color: #1e40af;
            box-shadow: 0 4px 12px rgba(37, 99, 235, 0.3);
        }

        .shift-label i {
            font-size: 1.25rem;
        }

        .team-leader-info {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            color: #64748b;
            font-size: 0.875rem;
            margin-top: 0.5rem;
        }

        .btn-modal-save {
            background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%);
            border: none;
            color: white;
            padding: 0.75rem 2rem;
            border-radius: 8px;
            font-weight: 600;
            transition: all 0.3s ease;
        }

        .btn-modal-save:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(37, 99, 235, 0.4);
        }

        .btn-modal-cancel {
            background: white;
            border: 2px solid #e2e8f0;
            color: #64748b;
            padding: 0.75rem 2rem;
            border-radius: 8px;
            font-weight: 600;
            transition: all 0.3s ease;
        }

        .btn-modal-cancel:hover {
            background: #f8fafc;
            border-color: #cbd5e1;
        }

        /* Responsive */
        @media (max-width: 768px) {
            .shift-type-group {
                flex-direction: column;
            }

            .team-header {
                flex-direction: column;
                align-items: flex-start;
                gap: 1rem;
            }

            .team-info {
                width: 100%;
                justify-content: flex-start;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <!-- Page Header -->
        <div class="page-header">
            <h2><i class="fas fa-users"></i> Teams and Members Overview</h2>
        </div>

        <!-- Filter Card -->
        <div class="filter-card">
            <div class="filter-group">
                <div class="filter-item">
                    <label class="filter-label">
                        <i class="fas fa-calendar"></i> Check Availability Date
                    </label>
                    <asp:TextBox ID="txtDate" runat="server" TextMode="Date" CssClass="filter-input" />
                </div>

                <div class="filter-item">
                    <label class="filter-label">
                        <i class="fas fa-clock"></i> Shift Type
                    </label>
                    <asp:DropDownList ID="ddlShiftFilter" runat="server" CssClass="filter-input">
                        <asp:ListItem Text="All Shifts" Value="" />
                        <asp:ListItem Text="☀️ Morning Shift" Value="MorningShift" />
                        <asp:ListItem Text="🌙 Night Shift" Value="NightShift" />
                    </asp:DropDownList>
                </div>

                <div class="filter-item">
                    <asp:Button ID="btnFilterDate" runat="server" Text="🔍 Check Availability"
                        CssClass="btn-filter" OnClick="btnFilterDate_Click" />
                </div>
            </div>
        </div>

        <!-- Teams Repeater -->
        <asp:Repeater ID="rptTeams" runat="server" OnItemDataBound="rptTeams_ItemDataBound">
            <ItemTemplate>
                <div class="team-card">
                    <div class="team-header">
                        <div>
                            <h4 class="team-title"><%# Eval("GroupName") %></h4>
                            <div class="team-leader-info">
                                <i class="fas fa-user-shield"></i>
                                <strong>Leader:</strong> <%# Eval("TeamLeaderName") %>
                            </div>
                        </div>
                        <div class="team-info">
                            <span class='badge <%# Eval("ShiftType").ToString() == "MorningShift" ? "badge-morning" : "badge-night" %>'>
                                <%# Eval("ShiftType").ToString() == "MorningShift" ? "☀️ Morning" : "🌙 Night" %>
                            </span>
                            <span class='badge <%# Eval("Status").ToString() == "Available" ? "badge-available" : "badge-unavailable" %>'>
                                <%# Eval("Status") %>
                            </span>

                           <asp:Button ID="btnEditTeam" runat="server" 
                            Text="✏️ Edit" 
                            CommandArgument='<%# Eval("TeamID") + "," + Eval("GroupName") + "," + Eval("ShiftType") + "," + Eval("TeamLeaderID") %>'
                            CssClass="btn-action btn-edit"
                            OnClick="btnEditTeam_Click" />

                            <asp:PlaceHolder ID="phDeleteTeam" runat="server" Visible="false">
                                <asp:Button ID="btnDeleteTeam" runat="server" 
                                    Text="🗑️ Delete" 
                                    CommandArgument='<%# Eval("TeamID") %>'
                                    CssClass="btn-action btn-delete"
                                    OnClientClick="return confirmDelete(this, event);"
                                    OnClick="btnDeleteTeam_Click" />
                            </asp:PlaceHolder>
                        </div>
                    </div>

                    <ul class="member-list">
                        <asp:Repeater ID="rptEmployees" runat="server">
                            <ItemTemplate>
                                <li class="member-item">
                                    <div class="member-avatar">
                                        <%# Eval("FirstName").ToString().Substring(0, 1) + Eval("LastName").ToString().Substring(0, 1) %>
                                    </div>
                                    <div>
                                        <div style="font-weight: 600; color: #1e293b;">
                                            <%# Eval("LastName") %>, <%# Eval("FirstName") %> <%# string.IsNullOrEmpty(Eval("MiddleName") as string) ? "" : Eval("MiddleName") %>
                                        </div>
                                        <div style="font-size: 0.875rem; color: #64748b;">
                                            <%# Eval("Department") %>
                                        </div>
                                    </div>
                                </li>
                            </ItemTemplate>
                        </asp:Repeater>

                        <asp:PlaceHolder ID="phNoMembers" runat="server">
                            <li class="member-item" style="justify-content: center; color: #94a3b8; font-style: italic;">
                                <i class="fas fa-info-circle"></i>
                                No members assigned to this team.
                            </li>
                        </asp:PlaceHolder>
                    </ul>
                </div>
            </ItemTemplate>
        </asp:Repeater>

        <asp:Label ID="lblMessage" runat="server" CssClass="text-center text-xl font-bold text-gray-500 mt-8" Visible="false" />
        
        <!-- Hidden fields -->
        <asp:HiddenField ID="hdnConfirmDelete" runat="server" Value="false" />
        <asp:HiddenField ID="hdnTeamIdToDelete" runat="server" Value="" />
    </div>

    <!-- Edit Team Modal -->
    <div class="modal fade" id="editTeamModal" tabindex="-1" aria-labelledby="editTeamModalLabel" aria-hidden="true" data-bs-backdrop="static">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="editTeamModalLabel">
                        <i class="fas fa-edit"></i>
                        Edit Team
                    </h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <asp:HiddenField ID="hdnEditTeamID" runat="server" />

                    <!-- Team Name (Read-only) -->
                    <div class="form-section">
                        <label class="form-label">
                            <i class="fas fa-tag"></i>
                            Team Name
                        </label>
                        <asp:TextBox ID="txtEditTeamName" runat="server" CssClass="form-control" ReadOnly="true" />
                        <small class="text-muted d-block mt-1">
                            <i class="fas fa-info-circle"></i> Team name cannot be changed
                        </small>
                    </div>

                    <!-- Shift Type -->
                    <div class="form-section">
                        <label class="form-label">
                            <i class="fas fa-clock"></i>
                            Shift Type
                        </label>
                        <div class="shift-type-group">
                            <div class="shift-option">
                                <asp:RadioButton ID="rbEditMorning" runat="server" GroupName="EditShiftType" ClientIDMode="Static" />
                                <label for="rbEditMorning" class="shift-label">
                                    <i class="fas fa-sun"></i>
                                    Morning Shift
                                </label>
                            </div>
                            <div class="shift-option">
                                <asp:RadioButton ID="rbEditNight" runat="server" GroupName="EditShiftType" ClientIDMode="Static" />
                                <label for="rbEditNight" class="shift-label">
                                    <i class="fas fa-moon"></i>
                                    Night Shift
                                </label>
                            </div>
                        </div>
                    </div>

                    <!-- Team Leader -->
                    <div class="form-section">
                        <label for="<%= ddlEditTeamLeader.ClientID %>" class="form-label">
                            <i class="fas fa-user-shield"></i>
                            Team Leader
                        </label>
                        <asp:DropDownList ID="ddlEditTeamLeader" runat="server" CssClass="form-select">
                            <asp:ListItem Text="-- Select a Head Technician --" Value="" />
                        </asp:DropDownList>
                        <small class="text-muted d-block mt-1">
                            <i class="fas fa-info-circle"></i> Choose a Head Technician to lead this team
                        </small>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-modal-cancel" data-bs-dismiss="modal">
                        <i class="fas fa-times"></i> Cancel
                    </button>
                    <asp:Button ID="btnSaveEdit" runat="server" CssClass="btn btn-modal-save" 
                        Text="💾 Save Changes" OnClick="btnSaveEdit_Click"
                        OnClientClick="return validateEditForm();" />
                </div>
            </div>
        </div>
    </div>

    <!-- Bootstrap JS -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <!-- SweetAlert2 JS -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11.7.32/dist/sweetalert2.all.min.js"></script>
    
    <script type="text/javascript">
        function validateEditForm() {
            var morningShift = document.getElementById('rbEditMorning').checked;
            var nightShift = document.getElementById('rbEditNight').checked;
            var teamLeader = document.getElementById('<%= ddlEditTeamLeader.ClientID %>').value;

            if (!morningShift && !nightShift) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Shift Type Required',
                    text: 'Please select a shift type',
                    confirmButtonColor: '#2563eb'
                });
                return false;
            }

            if (teamLeader === '') {
                Swal.fire({
                    icon: 'warning',
                    title: 'Team Leader Required',
                    text: 'Please select a team leader',
                    confirmButtonColor: '#2563eb'
                });
                return false;
            }

            return true;
        }

        function confirmDelete(button, event) {
            event.preventDefault();
            
            var teamId = button.getAttribute('commandargument');
            
            Swal.fire({
                title: 'Are you sure?',
                text: "Do you want to delete this empty team? This action cannot be undone!",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#dc2626',
                cancelButtonColor: '#6b7280',
                confirmButtonText: 'Yes, delete it!',
                cancelButtonText: 'Cancel',
                reverseButtons: true
            }).then((result) => {
                if (result.isConfirmed) {
                    document.getElementById('<%= hdnConfirmDelete.ClientID %>').value = 'true';
                    document.getElementById('<%= hdnTeamIdToDelete.ClientID %>').value = teamId;
                    __doPostBack(button.name, '');
                }
            });

            return false;
        }

        function showSuccessAlert(message) {
            Swal.fire({
                title: 'Success!',
                text: message,
                icon: 'success',
                confirmButtonColor: '#2563eb'
            });
        }

        function showErrorAlert(message) {
            Swal.fire({
                title: 'Error!',
                text: message,
                icon: 'error',
                confirmButtonColor: '#dc2626'
            });
        }

        function showWarningAlert(message) {
            Swal.fire({
                title: 'Warning!',
                text: message,
                icon: 'warning',
                confirmButtonColor: '#f59e0b'
            });
        }

        function showInfoAlert(message) {
            Swal.fire({
                title: 'Info',
                text: message,
                icon: 'info',
                confirmButtonColor: '#3b82f6'
            });
        }
    </script>
</asp:Content>