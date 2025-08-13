<%@ Page Title="" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="AddAdmin.aspx.cs" Inherits="RRCManagementSystem.AddAdmin" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container my-5">
        <div class="card shadow mx-auto" style="max-width: 700px;">
            <div class="card-body">
                <h2 class="text-center mb-4 text-primary">Add New Account</h2>

                <asp:Label ID="lblMessage" runat="server" CssClass="text-danger fw-bold d-block text-center mb-3" Visible="false"></asp:Label>

                <div class="alert alert-info small" role="alert">
                    Once you create this account, an email will be sent inviting the user to set their password.
                </div>

                <div class="mb-3">
    <label for="txtName" class="form-label">Name *</label>
    <asp:TextBox ID="txtName" runat="server" CssClass="form-control"
                 placeholder="Enter full name"
                 oninput="sanitizeName(this)" />
    <!-- Required -->
    <asp:RequiredFieldValidator ID="rfvName" runat="server"
        ControlToValidate="txtName" CssClass="text-danger small"
        Display="Dynamic" ErrorMessage="Name is required." />
    <!-- Letters, spaces, hyphen, apostrophe only -->
    <asp:RegularExpressionValidator ID="revName" runat="server"
        ControlToValidate="txtName" CssClass="text-danger small"
        Display="Dynamic"
        ValidationExpression="^[A-Za-zÀ-ÖØ-öø-ÿ\s'\-]+$"
        ErrorMessage="Use letters, spaces, - or ' only." />
</div>

                <div class="mb-3">
    <label for="txtEmail" class="form-label">Email *</label>
    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" 
                 placeholder="Enter email address" TextMode="Email" />

    <!-- Required -->
    <asp:RequiredFieldValidator ID="rfvEmail" runat="server"
        ControlToValidate="txtEmail" CssClass="text-danger small"
        Display="Dynamic" ErrorMessage="Email is required." />

    <!-- Allowed domains: gmail.com, yahoo.com, outlook.com -->
    <asp:RegularExpressionValidator ID="revEmail" runat="server"
        ControlToValidate="txtEmail" CssClass="text-danger small"
        Display="Dynamic"
        ValidationExpression="^[^@\s]+@(gmail\.com|yahoo\.com|outlook\.com)$"
        ErrorMessage="Email must be Gmail, Yahoo, or Outlook." />
</div>


                <div class="mb-3">
                    <label for="ddlRole" class="form-label">Select Role *</label>
                    <asp:DropDownList ID="ddlRole" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlRole_SelectedIndexChanged"></asp:DropDownList>
                </div>

                <asp:Button ID="btnSubmit" runat="server" Style="display:none;" OnClick="btnSubmit_Click" />
                <button type="button" class="btn btn-primary w-100 mb-4" onclick="confirmCreate()">Create Account</button>

                <asp:Repeater ID="rptPermissions" runat="server" Visible="false">
                    <HeaderTemplate>
                        <div class="table-responsive">
                            <table class="table table-bordered text-center">
                                <thead class="table-light">
                                    <tr>
                                        <th>Module</th>
                                        <th>View</th>
                                        <th>Add</th>
                                        <th>Edit</th>
                                        <th>Delete</th>
                                    </tr>
                                </thead>
                                <tbody>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td><%# Eval("ModuleName") %></td>
                            <td><asp:CheckBox ID="chkView" runat="server" Checked='<%# Eval("CanView") %>' CssClass="form-check-input chkView" /></td>
                            <td><asp:CheckBox ID="chkAdd" runat="server" Checked='<%# Eval("CanAdd") %>' CssClass="form-check-input chkAdd" onclick="ensureView(this)" /></td>
                            <td><asp:CheckBox ID="chkEdit" runat="server" Checked='<%# Eval("CanEdit") %>' CssClass="form-check-input chkEdit" onclick="ensureView(this)" /></td>
                            <td><asp:CheckBox ID="chkDelete" runat="server" Checked='<%# Eval("CanDelete") %>' CssClass="form-check-input chkDelete" onclick="ensureView(this)" /></td>
                            <asp:HiddenField ID="hfModuleName" runat="server" Value='<%# Eval("ModuleName") %>' />
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                                </tbody>
                            </table>
                        </div>
                    </FooterTemplate>
                </asp:Repeater>
            </div>
        </div>
    </div>

    <!-- SweetAlert2 -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <script>
        function sanitizeName(el) {
            // allow letters (incl. accents), spaces, hyphen, apostrophe
            el.value = el.value.replace(/[^A-Za-z\u00C0-\u024F\s'\-]/g, '');
        }
    </script>

    <script>
        function confirmCreate() {
            Swal.fire({
                title: 'Create Account?',
                text: "An invitation will be sent to the new user.",
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Yes, create it!',
                cancelButtonText: 'Cancel',
                confirmButtonColor: '#007bff',
                cancelButtonColor: '#d33'
            }).then((result) => {
                if (result.isConfirmed) {
                    document.getElementById('<%= btnSubmit.ClientID %>').click();
                }
            });
        }

        function ensureView(cb) {
            const row = cb.closest('tr');
            if (!row) return;
            const viewCheckbox = row.querySelector('.chkView');
            if (cb.checked && viewCheckbox && !viewCheckbox.checked) {
                viewCheckbox.checked = true;
            }
        }
    </script>
</asp:Content>