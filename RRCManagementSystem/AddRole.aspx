<%@ Page Title="" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="AddRole.aspx.cs" Inherits="RRCManagementSystem.AddRole" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <script type="text/javascript">
        function showSaveConfirmation() {
            Swal.fire({
                title: 'Add Role?',
                text: 'Are you sure you want to add this role with these permissions?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#1f2937',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes, save it!'
            }).then((result) => {
                if (result.isConfirmed) {
                    document.getElementById('<%= btnSaveHidden.ClientID %>').click();
                }
            });
            return false;
        }

        function ensureView(cb) {
            const row = cb.closest('tr');
            if (!row) return;
            const viewCheckbox = row.querySelector('.chkView');
            if (cb.checked && viewCheckbox && !viewCheckbox.checked) {
                viewCheckbox.checked = true;
            }
        }

        function toggleAllInRow(checkbox, permission) {
            const row = checkbox.closest('tr');
            if (!row) return;

            if (permission === 'view') {
                // If unchecking View, uncheck all others
                if (!checkbox.checked) {
                    row.querySelectorAll('.chkAdd, .chkEdit, .chkDelete').forEach(cb => cb.checked = false);
                }
            }
        }
    </script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container my-5">
        <div class="card shadow mx-auto" style="max-width: 900px;">
            <div class="card-body">
                <h2 class="text-center mb-4 text-primary">Add New Role</h2>

                <asp:Label ID="lblMessage" runat="server" CssClass="text-danger fw-bold d-block text-center mb-3" Visible="false"></asp:Label>

                <div class="mb-4">
                    <label for="txtRoleName" class="form-label fw-bold">Role Name *</label>
                    <asp:TextBox ID="txtRoleName" runat="server" CssClass="form-control" 
                                 placeholder="e.g., Manager, Staff, Supervisor" 
                                 MaxLength="100" />
                    <asp:RequiredFieldValidator ID="rfvRoleName" runat="server"
                        ControlToValidate="txtRoleName" CssClass="text-danger small"
                        Display="Dynamic" ErrorMessage="Role name is required." />
                </div>

                <div class="alert alert-info small mb-3">
                    <strong>Note:</strong> Define the permissions for this role. All users assigned this role will have these permissions.
                </div>

                <h5 class="mb-3">Role Permissions</h5>
                
                <asp:Repeater ID="rptPermissions" runat="server">
                    <HeaderTemplate>
                        <div class="table-responsive">
                            <table class="table table-bordered table-hover text-center">
                                <thead class="table-dark">
                                    <tr>
                                        <th style="width: 30%;">Module</th>
                                        <th style="width: 17.5%;">View</th>
                                        <th style="width: 17.5%;">Add</th>
                                        <th style="width: 17.5%;">Edit</th>
                                        <th style="width: 17.5%;">Delete</th>
                                    </tr>
                                </thead>
                                <tbody>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td class="text-start"><%# Eval("ModuleName") %></td>
                            <td>
                                <asp:CheckBox ID="chkView" runat="server" 
                                    Checked='<%# Eval("CanView") %>' 
                                    CssClass="form-check-input chkView" 
                                    onclick="toggleAllInRow(this, 'view')" />
                            </td>
                            <td>
                                <asp:CheckBox ID="chkAdd" runat="server" 
                                    Checked='<%# Eval("CanAdd") %>' 
                                    CssClass="form-check-input chkAdd" 
                                    onclick="ensureView(this)" />
                            </td>
                            <td>
                                <asp:CheckBox ID="chkEdit" runat="server" 
                                    Checked='<%# Eval("CanEdit") %>' 
                                    CssClass="form-check-input chkEdit" 
                                    onclick="ensureView(this)" />
                            </td>
                            <td>
                                <asp:CheckBox ID="chkDelete" runat="server" 
                                    Checked='<%# Eval("CanDelete") %>' 
                                    CssClass="form-check-input chkDelete" 
                                    onclick="ensureView(this)" />
                            </td>
                            <asp:HiddenField ID="hfModuleName" runat="server" Value='<%# Eval("ModuleName") %>' />
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                                </tbody>
                            </table>
                        </div>
                    </FooterTemplate>
                </asp:Repeater>

                <div class="text-center mt-4">
                    <asp:Button ID="btnSave" runat="server" Text="Add Role" 
                        OnClientClick="return showSaveConfirmation();" 
                        UseSubmitBehavior="false"
                        CssClass="btn btn-primary btn-lg px-5" />
                    
                    <asp:Button ID="btnSaveHidden" runat="server" 
                        OnClick="btnSave_Click" 
                        Style="display:none;" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>