<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AllEmployee.aspx.cs" Inherits="RRCManagementSystem.AllEmployee" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" />
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container my-4">
        <div class="card shadow-sm">
            <div class="card-header bg-primary text-white text-center fw-bold">
                Active Employees
            </div>
            <div class="card-body">
                <div class="table-responsive">
                    <asp:GridView ID="gvEmployees" runat="server" AutoGenerateColumns="False"
                        CssClass="table table-bordered table-striped text-center align-middle"
                        AllowPaging="True" PageSize="10"
                        DataKeyNames="EmployeeID"
                        OnRowCommand="gvEmployees_RowCommand"
                        OnPageIndexChanging="gvEmployees_PageIndexChanging">
                        <Columns>
                            <asp:BoundField DataField="EmployeeID" HeaderText="ID">
                                <ItemStyle Width="50px" />
                            </asp:BoundField>
                            <asp:BoundField DataField="LastName" HeaderText="Last Name" />
                            <asp:BoundField DataField="FirstName" HeaderText="First Name" />
                            <asp:BoundField DataField="MiddleName" HeaderText="Middle Name" />
                            <asp:BoundField DataField="Email" HeaderText="Email" />
                            <asp:BoundField DataField="Position" HeaderText="Position" />
                            <asp:BoundField DataField="Phone" HeaderText="Phone" />
                            <asp:ImageField DataImageUrlField="ProfileImage" HeaderText="Profile Picture">
                                <ControlStyle CssClass="rounded-circle" Width="50px" Height="50px" />
                            </asp:ImageField>
                            <asp:TemplateField HeaderText="Actions">
                                <ItemTemplate>
                                    <div class="d-flex justify-content-center gap-2">
                                        <asp:LinkButton ID="btnEdit" runat="server"
                                            CommandName="EditEmployee"
                                            CommandArgument='<%# Eval("EmployeeID") %>'
                                            CssClass="btn btn-sm btn-primary">
                                            <i class="fas fa-edit me-1"></i>Edit
                                        </asp:LinkButton>

                                        <asp:LinkButton ID="btnArchive" runat="server"
                                            CssClass="btn btn-sm btn-danger"
                                            OnClientClick='<%# "return confirmArchive(" + Eval("EmployeeID") + ");" %>'>
                                            <i class="fas fa-archive me-1"></i>Archive
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

    <!-- Hidden field and hidden button for archive postback -->
    <asp:HiddenField ID="hfEmployeeToArchive" runat="server" />
    <asp:Button ID="btnHiddenArchive" runat="server" Style="display:none;" OnClick="btnHiddenArchive_Click" />

    <script type="text/javascript">
        function confirmArchive(employeeId) {
            event.preventDefault();

            Swal.fire({
                title: 'Archive Employee?',
                text: 'This employee will be archived and removed from the active list.',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Yes, archive it'
            }).then((result) => {
                if (result.isConfirmed) {
                    document.getElementById('<%= hfEmployeeToArchive.ClientID %>').value = employeeId;
                    document.getElementById('<%= btnHiddenArchive.ClientID %>').click();
                }
            });

            return false;
        }
    </script>
</asp:Content>
