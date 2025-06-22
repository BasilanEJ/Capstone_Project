<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="GroupEmployees.aspx.cs" Inherits="RRCManagementSystem.GroupEmployees" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Bootstrap 5 CSS -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <!-- SweetAlert2 CSS -->
    <link href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container my-5">
        <div class="card shadow-sm">
            <div class="card-body">
                <h3 class="mb-4">Create or Assign Technicians to a Team</h3>

                <!-- Button to open Create Team Modal -->
                <asp:Button ID="btnOpenModal" runat="server" Text="➕ Create New Team" CssClass="btn btn-primary mb-4"
                    OnClientClick="openCreateTeamModal(); return false;" />

                <!-- Existing Teams Dropdown -->
                <div class="mb-3">
                    <label for="ddlExistingTeams" class="form-label">Assign to Existing Team</label>
                    <asp:DropDownList ID="ddlExistingTeams" runat="server" CssClass="form-select">
                        <asp:ListItem Text="Select an existing team" Value="" />
                    </asp:DropDownList>
                </div>

                <h5 class="mb-3">Select Technicians</h5>

                <asp:GridView ID="gvTechnicians" runat="server" AutoGenerateColumns="False"
                    CssClass="table table-striped table-bordered align-middle" DataKeyNames="EmployeeID"
                    OnRowDataBound="gvTechnicians_RowDataBound" Responsive="True">
                    <Columns>
                        <asp:TemplateField HeaderText="Select" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="5%">
                            <ItemTemplate>
                                <asp:CheckBox ID="chkSelect" runat="server" CssClass="form-check-input" />
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:BoundField DataField="EmployeeID" HeaderText="ID" ItemStyle-Width="10%" />
                        <asp:BoundField DataField="FullName" HeaderText="Full Name" ItemStyle-Width="45%" />
                        <asp:BoundField DataField="CurrentTeam" HeaderText="Current Team" ItemStyle-Width="25%" />

                        <asp:TemplateField HeaderText="Action" ItemStyle-Width="15%">
                            <ItemTemplate>
                                <asp:DropDownList ID="ddlAction" runat="server" CssClass="form-select form-select-sm">
                                    <asp:ListItem Text="No Action" Value="" />
                                    <asp:ListItem Text="Remove from team" Value="REMOVE" />
                                </asp:DropDownList>
                                <asp:HiddenField ID="hfEmployeeName" runat="server" Value='<%# Eval("FullName") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>

                <div class="d-flex flex-wrap gap-3 mt-4">
                    <asp:Button ID="btnSaveChanges" runat="server" CssClass="btn btn-primary flex-grow-1"
                        Text="Save Changes" OnClick="btnSaveChanges_Click"
                        OnClientClick="return confirmSaveChanges();" />

                    <asp:Button ID="btnCancel" runat="server" CssClass="btn btn-secondary flex-grow-1"
                        Text="Cancel" PostBackUrl="Dashboard.aspx"
                        OnClientClick="return confirmCancel();" />
                </div>

                <asp:Label ID="lblMessage" runat="server" CssClass="form-text mt-3" />
            </div>
        </div>
    </div>

    <!-- Modal: Create Team -->
    <div class="modal fade" id="createTeamModal" tabindex="-1" aria-labelledby="createTeamModalLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header bg-primary text-white">
                    <h5 class="modal-title" id="createTeamModalLabel">Create New Team</h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <label for="txtModalTeamName" class="form-label">Team Name</label>
                    <asp:TextBox ID="txtModalTeamName" runat="server" CssClass="form-control" placeholder="Enter team name" />
                </div>
                <div class="modal-footer">
                    <asp:Button ID="btnCreateTeamModal" runat="server" CssClass="btn btn-primary" Text="Create Team"
                        OnClick="btnCreateTeamModal_Click" />
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                </div>
            </div>
        </div>
    </div>

    <!-- Bootstrap 5 JS Bundle (includes Popper) -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <!-- SweetAlert2 JS -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.all.min.js"></script>

    <script type="text/javascript">
        // Bootstrap 5 modal instance
        var createTeamModal;

        function openCreateTeamModal() {
            if (!createTeamModal) {
                var modalEl = document.getElementById('createTeamModal');
                createTeamModal = new bootstrap.Modal(modalEl);
            }
            createTeamModal.show();
        }

        // Confirm Save Changes using SweetAlert2
        function confirmSaveChanges() {
            event.preventDefault();
            Swal.fire({
                title: 'Are you sure?',
                text: "Do you want to save these changes?",
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Yes, save it!',
                cancelButtonText: 'Cancel'
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack('<%= btnSaveChanges.UniqueID %>', '');
                }
            });
            return false; // prevent default postback; will trigger manually if confirmed
        }

        // Confirm Cancel using SweetAlert2
        function confirmCancel() {
            event.preventDefault();
            Swal.fire({
                title: 'Are you sure?',
                text: "Do you want to cancel and go back to the dashboard?",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Yes, cancel',
                cancelButtonText: 'Stay here'
            }).then((result) => {
                if (result.isConfirmed) {
                    window.location.href = '<%= ResolveUrl("Dashboard.aspx") %>';
                }
            });
            return false; // prevent default postback; redirect manually if confirmed
        }
    </script>
</asp:Content>
