<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ArchiveEmployee.aspx.cs" Inherits="RRCManagementSystem.ArchiveEmployee" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" />
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container my-4">
        <div class="card shadow-sm">
            <div class="card-header bg-primary text-white text-center fw-bold">
                Archived Employees
            </div>
            <div class="card-body">
                <div class="table-responsive">
                    <asp:GridView ID="gvArchivedEmployees" runat="server" AutoGenerateColumns="False"
                        CssClass="table table-bordered table-striped text-center align-middle"
                        OnRowCommand="gvArchivedEmployees_RowCommand"
                        DataKeyNames="EmployeeID">
                        <Columns>
                            <asp:BoundField DataField="EmployeeID" HeaderText="ID" />
                            <asp:BoundField DataField="LastName" HeaderText="Last Name" />
                            <asp:BoundField DataField="FirstName" HeaderText="First Name" />
                            <asp:BoundField DataField="MiddleName" HeaderText="Middle Name" />
                            <asp:BoundField DataField="Position" HeaderText="Position" />
                            <asp:TemplateField HeaderText="Actions">
                                <ItemTemplate>
                                    <div class="d-flex justify-content-center gap-2">
                                        <asp:LinkButton ID="btnRestore" runat="server"
                                            CommandName="Restore"
                                            CommandArgument='<%# Eval("EmployeeID") %>'
                                            CssClass="btn btn-sm btn-success"
                                            OnClientClick='<%# "return confirmRestore(" + Eval("EmployeeID") + ");" %>'>
                                            <i class="fas fa-undo me-1"></i>Restore
                                        </asp:LinkButton>

                                        <asp:LinkButton ID="btnDelete" runat="server"
                                            CommandName="Delete"
                                            CommandArgument='<%# Eval("EmployeeID") %>'
                                            CssClass="btn btn-sm btn-danger"
                                            OnClientClick='<%# "return confirmDelete(" + Eval("EmployeeID") + ");" %>'>
                                            <i class="fas fa-trash-alt me-1"></i>Delete
                                        </asp:LinkButton>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        function confirmDelete(employeeId) {
            event.preventDefault();

            Swal.fire({
                title: 'Permanently Delete?',
                text: 'This employee record will be permanently deleted and cannot be recovered.',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Yes, delete'
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack('<%= gvArchivedEmployees.UniqueID %>', 'Delete$' + employeeId);
                }
            });

            return false;
        }

        function confirmRestore(employeeId) {
            event.preventDefault();

            Swal.fire({
                title: 'Restore Employee?',
                text: 'This employee will be restored to the active list.',
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#28a745',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Yes, restore'
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack('<%= gvArchivedEmployees.UniqueID %>', 'Restore$' + employeeId);
                }
            });

            return false;
        }
    </script>
</asp:Content>
