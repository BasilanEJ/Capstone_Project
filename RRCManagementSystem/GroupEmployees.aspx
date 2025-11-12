<%@ Page Title="Team Management" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="GroupEmployees.aspx.cs" Inherits="RRCManagementSystem.GroupEmployees" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Bootstrap 5 CSS -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <!-- SweetAlert2 CSS -->
    <link href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css" rel="stylesheet" />
    
    <style>
        /* Custom Styles to Match Admin.Master Theme */
        .page-header {
            background: linear-gradient(135deg, #1e3a8a 0%, #1e40af 100%);
            color: white;
            padding: 2rem;
            border-radius: 12px;
            margin-bottom: 2rem;
            box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
        }

        .page-header h3 {
            margin: 0;
            font-weight: 600;
            font-size: 1.75rem;
        }

        .page-header p {
            margin: 0.5rem 0 0 0;
            opacity: 0.9;
            font-size: 0.95rem;
        }

        .card {
            border: none;
            border-radius: 12px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
            overflow: hidden;
        }

        .card-header {
            background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);
            border-bottom: 2px solid #e2e8f0;
            padding: 1.25rem 1.5rem;
            font-weight: 600;
            color: #1e293b;
            font-size: 1.1rem;
        }

        .btn-create-team {
            background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%);
            border: none;
            color: white;
            padding: 0.75rem 1.5rem;
            border-radius: 8px;
            font-weight: 600;
            transition: all 0.3s ease;
            box-shadow: 0 2px 8px rgba(37, 99, 235, 0.3);
        }

        .btn-create-team:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(37, 99, 235, 0.4);
            background: linear-gradient(135deg, #1e40af 0%, #1e3a8a 100%);
        }

        /* Enhanced Modal Styling */
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

        .form-control,
        .form-select {
            border: 2px solid #e2e8f0;
            border-radius: 8px;
            padding: 0.75rem 1rem;
            font-size: 0.95rem;
            transition: all 0.3s ease;
        }

        .form-control:focus,
        .form-select:focus {
            border-color: #2563eb;
            box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
        }

        /* Radio Button Styling */
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

        /* Modal Footer */
        .modal-footer {
            border: none;
            padding: 1.5rem 2rem;
            background: #f8fafc;
        }

        .btn-modal-create {
            background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%);
            border: none;
            color: white;
            padding: 0.75rem 2rem;
            border-radius: 8px;
            font-weight: 600;
            transition: all 0.3s ease;
        }

        .btn-modal-create:hover {
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

        /* GridView Styling */
        .table {
            border-radius: 8px;
            overflow: hidden;
        }

        .table thead {
            background: linear-gradient(135deg, #1e3a8a 0%, #1e40af 100%);
            color: white;
        }

        .table thead th {
            border: none;
            padding: 1rem;
            font-weight: 600;
            font-size: 0.9rem;
        }

        .table tbody tr {
            transition: all 0.2s ease;
        }

        .table tbody tr:hover {
            background-color: #f8fafc;
            transform: scale(1.01);
        }

        .checkbox-scale {
            transform: scale(1.5);
            cursor: pointer;
        }

        /* Action Buttons */
        .btn-save {
            background: linear-gradient(135deg, #10b981 0%, #059669 100%);
            border: none;
            color: white;
            padding: 0.875rem 2rem;
            border-radius: 8px;
            font-weight: 600;
            transition: all 0.3s ease;
        }

        .btn-save:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(16, 185, 129, 0.4);
        }

        .btn-cancel {
            background: white;
            border: 2px solid #e2e8f0;
            color: #64748b;
            padding: 0.875rem 2rem;
            border-radius: 8px;
            font-weight: 600;
            transition: all 0.3s ease;
        }

        .btn-cancel:hover {
            background: #f8fafc;
            border-color: #cbd5e1;
        }

        /* Alert Messages with Auto-Fade Animation */
        .alert-message {
            padding: 1rem 1.5rem;
            border-radius: 8px;
            margin-top: 1.5rem;
            font-weight: 500;
            transition: opacity 0.5s ease-in-out, transform 0.5s ease-in-out;
        }

        @keyframes fadeInOut {
            0% { opacity: 0; transform: translateY(-10px); }
            10% { opacity: 1; transform: translateY(0); }
            90% { opacity: 1; transform: translateY(0); }
            100% { opacity: 0; transform: translateY(-10px); }
        }

        .alert-message.auto-fade {
            animation: fadeInOut 6s ease-in-out forwards;
        }

        /* SweetAlert Custom Width */
        .swal-wide {
            width: 600px !important;
        }

        /* Responsive */
        @media (max-width: 768px) {
            .shift-type-group {
                flex-direction: column;
            }

            .page-header {
                padding: 1.5rem;
            }

            .modal-body {
                padding: 1.5rem;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <!-- Page Header -->
        <div class="page-header">
            <h3><i class="fas fa-users-cog"></i> Team Management</h3>
            <p>Create teams and assign technicians to organize your workforce efficiently</p>
        </div>

        <!-- Main Card -->
        <div class="card">
            <div class="card-header">
                <i class="fas fa-user-friends"></i> Technician Team Assignment
            </div>
            <div class="card-body p-4">
                <!-- Create Team Button -->
                <div class="mb-4">
                    <asp:Button ID="btnOpenModal" runat="server" Text="➕ Create New Team" 
                        CssClass="btn-create-team"
                        OnClientClick="openCreateTeamModal(); return false;" />
                </div>

                <!-- Existing Teams Dropdown -->
                <div class="form-section">
                    <label class="form-label">
                        <i class="fas fa-layer-group"></i>
                        Assign to Existing Team (Bulk Assignment)
                    </label>
                    <asp:DropDownList ID="ddlExistingTeams" runat="server" CssClass="form-select">
                        <asp:ListItem Text="-- Select a team to assign selected technicians --" Value="" />
                    </asp:DropDownList>
                    <small class="text-muted d-block mt-2">
                        <i class="fas fa-info-circle"></i> Select a team here to assign all checked technicians at once
                    </small>
                </div>

                <hr class="my-4" />

                <!-- Technicians Grid -->
                <h5 class="mb-3"><i class="fas fa-clipboard-list"></i> Select Technicians</h5>

                <div class="table-responsive">
                    <asp:GridView ID="gvTechnicians" runat="server" AutoGenerateColumns="False"
                        CssClass="table table-hover align-middle" DataKeyNames="EmployeeID"
                        OnRowDataBound="gvTechnicians_RowDataBound">
                        <Columns>
                            <asp:TemplateField HeaderText="Select" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="8%">
                                <ItemTemplate>
                                    <div class="form-check d-flex justify-content-center">
                                        <asp:CheckBox ID="chkSelect" runat="server" CssClass="form-check-input checkbox-scale" />
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:BoundField DataField="EmployeeID" HeaderText="ID" ItemStyle-Width="8%" />
                            <asp:BoundField DataField="FullName" HeaderText="Full Name" ItemStyle-Width="35%" />
                            <asp:BoundField DataField="CurrentTeam" HeaderText="Current Team" ItemStyle-Width="25%" />

                            <asp:TemplateField HeaderText="Action" ItemStyle-Width="24%">
                                <ItemTemplate>
                                    <asp:DropDownList ID="ddlAction" runat="server" CssClass="form-select form-select-sm">
                                    </asp:DropDownList>
                                    <asp:HiddenField ID="hfEmployeeName" runat="server" Value='<%# Eval("FullName") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>

                <!-- Action Buttons -->
                <div class="d-flex gap-3 mt-4">
                    <asp:Button ID="btnSaveChanges" runat="server" CssClass="btn-save flex-grow-1"
                        Text="💾 Save Changes" OnClick="btnSaveChanges_Click"
                        OnClientClick="return confirmSaveChanges();" />

                    <asp:Button ID="btnCancel" runat="server" CssClass="btn-cancel flex-grow-1"
                        Text="↩️ Cancel" PostBackUrl="Dashboard.aspx"
                        OnClientClick="return confirmCancel();" />
                </div>

                <!-- Message Label -->
                <asp:Label ID="lblMessage" runat="server" CssClass="alert-message d-block" />
            </div>
        </div>
    </div>

    <!-- Enhanced Create Team Modal -->
    <div class="modal fade" id="createTeamModal" tabindex="-1" aria-labelledby="createTeamModalLabel" aria-hidden="true" data-bs-backdrop="static">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="createTeamModalLabel">
                        <i class="fas fa-users-cog"></i>
                        Create New Team
                    </h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <!-- Team Name -->
                    <div class="form-section">
                        <label for="<%= txtModalTeamName.ClientID %>" class="form-label">
                            <i class="fas fa-tag"></i>
                            Team Name
                        </label>
                        <asp:TextBox ID="txtModalTeamName" runat="server" CssClass="form-control" 
                            placeholder="e.g., Emergency Response Unit" MaxLength="50" />
                        <small class="text-muted d-block mt-1">
                            <i class="fas fa-info-circle"></i> Enter a unique name for this team
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
                                <asp:RadioButton ID="rbMorningShift" runat="server" GroupName="ShiftType" 
                                    ClientIDMode="Static" />
                                <label for="rbMorningShift" class="shift-label">
                                    <i class="fas fa-sun"></i>
                                    Morning Shift
                                </label>
                            </div>
                            <div class="shift-option">
                                <asp:RadioButton ID="rbNightShift" runat="server" GroupName="ShiftType" 
                                    ClientIDMode="Static" />
                                <label for="rbNightShift" class="shift-label">
                                    <i class="fas fa-moon"></i>
                                    Night Shift
                                </label>
                            </div>
                        </div>
                    </div>

                    <!-- Team Leader -->
                    <div class="form-section">
                        <label for="<%= ddlTeamLeader.ClientID %>" class="form-label">
                            <i class="fas fa-user-shield"></i>
                            Team Leader
                        </label>
                        <asp:DropDownList ID="ddlTeamLeader" runat="server" CssClass="form-select">
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
                    <asp:Button ID="btnCreateTeamModal" runat="server" CssClass="btn btn-modal-create" 
                        Text="✅ Create Team" OnClick="btnCreateTeamModal_Click"
                        OnClientClick="return validateTeamForm();" />
                </div>
            </div>
        </div>
    </div>

    <!-- Bootstrap 5 JS Bundle -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <!-- SweetAlert2 JS -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.all.min.js"></script>

    <script type="text/javascript">
        var createTeamModal;

        // Initialize modal
        document.addEventListener("DOMContentLoaded", function () {
            var modalEl = document.getElementById('createTeamModal');
            createTeamModal = new bootstrap.Modal(modalEl);

            // Reset form when modal is closed
            modalEl.addEventListener('hidden.bs.modal', function () {
                document.getElementById('<%= txtModalTeamName.ClientID %>').value = '';
                document.getElementById('rbMorningShift').checked = false;
                document.getElementById('rbNightShift').checked = false;
                document.getElementById('<%= ddlTeamLeader.ClientID %>').selectedIndex = 0;
            });

            // Auto-fade alert messages
            autoFadeAlerts();
        });

        // Open modal
        function openCreateTeamModal() {
            createTeamModal.show();
            return false;
        }

        // Auto-fade alert messages after 6 seconds
        function autoFadeAlerts() {
            var alertMessage = document.getElementById('<%= lblMessage.ClientID %>');
            
            if (alertMessage && alertMessage.innerText.trim() !== '') {
                // Add the auto-fade animation class
                alertMessage.classList.add('auto-fade');
                
                // Clear the message after animation completes (6 seconds)
                setTimeout(function() {
                    alertMessage.innerText = '';
                    alertMessage.className = 'alert-message d-block';
                }, 6000);
            }
        }

        // Validate form before submission
        function validateTeamForm() {
            var teamName = document.getElementById('<%= txtModalTeamName.ClientID %>').value.trim();
            var morningShift = document.getElementById('rbMorningShift').checked;
            var nightShift = document.getElementById('rbNightShift').checked;
            var teamLeader = document.getElementById('<%= ddlTeamLeader.ClientID %>').value;

            if (teamName === '') {
                Swal.fire({
                    icon: 'warning',
                    title: 'Team Name Required',
                    text: 'Please enter a team name',
                    confirmButtonColor: '#2563eb'
                });
                return false;
            }

            if (!morningShift && !nightShift) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Shift Type Required',
                    text: 'Please select a shift type (Morning or Night)',
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

        // Confirm Save Changes
        function confirmSaveChanges() {
            event.preventDefault();
            Swal.fire({
                title: 'Save Changes?',
                text: "Do you want to save these team assignments?",
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#10b981',
                cancelButtonColor: '#64748b',
                confirmButtonText: '<i class="fas fa-check"></i> Yes, save it!',
                cancelButtonText: '<i class="fas fa-times"></i> Cancel'
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack('<%= btnSaveChanges.UniqueID %>', '');
                }
            });
            return false;
        }

        // Confirm Cancel
        function confirmCancel() {
            event.preventDefault();
            Swal.fire({
                title: 'Cancel Changes?',
                text: "Are you sure you want to go back to the dashboard?",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#ef4444',
                cancelButtonColor: '#64748b',
                confirmButtonText: '<i class="fas fa-sign-out-alt"></i> Yes, go back',
                cancelButtonText: '<i class="fas fa-times"></i> Stay here'
            }).then((result) => {
                if (result.isConfirmed) {
                    window.location.href = '<%= ResolveUrl("Dashboard.aspx") %>';
                }
            });
            return false;
        }

        // Scale checkboxes
        document.addEventListener("DOMContentLoaded", function () {
            document.querySelectorAll(".checkbox-scale").forEach(function (checkbox) {
                checkbox.style.cursor = "pointer";
            });
        });
    </script>
</asp:Content>