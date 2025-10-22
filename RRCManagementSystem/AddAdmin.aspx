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
                    <asp:RequiredFieldValidator ID="rfvName" runat="server"
                        ControlToValidate="txtName" CssClass="text-danger small"
                        Display="Dynamic" ErrorMessage="Name is required." />
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
                    <asp:RequiredFieldValidator ID="rfvEmail" runat="server"
                        ControlToValidate="txtEmail" CssClass="text-danger small"
                        Display="Dynamic" ErrorMessage="Email is required." />
                    <asp:RegularExpressionValidator ID="revEmail" runat="server"
                        ControlToValidate="txtEmail" CssClass="text-danger small"
                        Display="Dynamic"
                        ValidationExpression="^[^@\s]+@(gmail\.com|yahoo\.com|outlook\.com)$"
                        ErrorMessage="Email must be Gmail, Yahoo, or Outlook." />
                </div>

                <div class="mb-3">
                    <label for="ddlRole" class="form-label">Select Role *</label>
                    <asp:DropDownList ID="ddlRole" runat="server" CssClass="form-select"></asp:DropDownList>
                    <small class="text-muted">User will inherit permissions assigned to this role.</small>
                </div>

                <asp:Button ID="btnSubmit" runat="server" Style="display:none;" OnClick="btnSubmit_Click" />
                <button type="button" class="btn btn-primary w-100 mb-2" onclick="confirmCreate()">Create Account</button>
            </div>
        </div>
    </div>

    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <script>
        function sanitizeName(el) {
            el.value = el.value.replace(/[^A-Za-z\u00C0-\u024F\s'\-]/g, '');
        }

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
    </script>
</asp:Content>