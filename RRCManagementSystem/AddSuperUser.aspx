<%@ Page Title="" Language="C#" MasterPageFile="~/RootAdmin.Master" AutoEventWireup="true" CodeBehind="AddSuperUser.aspx.cs" Inherits="RRCManagementSystem.AddSuperUser" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mt-5">
        <div class="row justify-content-center">
            <div class="col-lg-6 col-md-8 col-sm-10">
                <div class="card shadow-lg border-0">
                    <div class="card-header text-center bg-primary text-white">
                        <h3>Create System Admin User</h3>
                    </div>
                    <div class="card-body">
                        <!-- Email -->
                        <div class="mb-3">
                            <label for="txtEmail" class="form-label">Email</label>
                            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" ClientIDMode="Static" placeholder="Enter email address" />
                            <asp:RequiredFieldValidator ID="rfvEmail" runat="server"
                                ControlToValidate="txtEmail" Display="Dynamic" CssClass="text-danger"
                                ErrorMessage="Email is required." />
                            <asp:RegularExpressionValidator ID="revEmail" runat="server"
                                ControlToValidate="txtEmail" Display="Dynamic" CssClass="text-danger"
                                ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$"
                                ErrorMessage="Enter a valid email." />
                        </div>

                        <!-- Full Name -->
                        <div class="mb-3">
                            <label for="txtName" class="form-label">Full Name</label>
                            <asp:TextBox ID="txtName" runat="server" CssClass="form-control" ClientIDMode="Static" placeholder="Enter full name" />
                            <asp:RequiredFieldValidator ID="rfvName" runat="server"
                                ControlToValidate="txtName" Display="Dynamic" CssClass="text-danger"
                                ErrorMessage="Full name is required." />
                        </div>

                        <!-- Password -->
                        <div class="mb-3">
                            <label for="txtPassword" class="form-label">Password</label>
                            <div class="input-group">
                                <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password" ClientIDMode="Static" placeholder="Enter password" />
                                <button type="button" class="btn btn-outline-secondary" onclick="togglePassword('txtPassword', this)">
                                    <i class="fa-solid fa-eye"></i>
                                </button>
                            </div>
                            <asp:RequiredFieldValidator ID="rfvPass" runat="server"
                                ControlToValidate="txtPassword" Display="Dynamic" CssClass="text-danger"
                                ErrorMessage="Password is required." />
                            <asp:RegularExpressionValidator ID="revPass" runat="server"
                                ControlToValidate="txtPassword" Display="Dynamic" CssClass="text-danger"
                                ValidationExpression="^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{8,}$"
                                ErrorMessage="Password must be at least 8 chars, include upper, lower, number, and special char." />
                        </div>

                        <!-- Confirm Password -->
                        <div class="mb-3">
                            <label for="txtConfirm" class="form-label">Confirm Password</label>
                            <div class="input-group">
                                <asp:TextBox ID="txtConfirm" runat="server" CssClass="form-control" TextMode="Password" ClientIDMode="Static" placeholder="Confirm your password" />
                                <button type="button" class="btn btn-outline-secondary" onclick="togglePassword('txtConfirm', this)">
                                    <i class="fa-solid fa-eye"></i>
                                </button>
                            </div>
                            <asp:RequiredFieldValidator ID="rfvConfirm" runat="server"
                                ControlToValidate="txtConfirm" Display="Dynamic" CssClass="text-danger"
                                ErrorMessage="Confirm your password." />
                            <asp:CompareValidator ID="cmpPass" runat="server"
                                ControlToValidate="txtConfirm" ControlToCompare="txtPassword"
                                Display="Dynamic" CssClass="text-danger"
                                ErrorMessage="Passwords do not match." />
                        </div>

                        <!-- Error Message -->
                        <asp:Label ID="lblMessage" runat="server" CssClass="text-danger fw-bold"></asp:Label>

                        <!-- Submit -->
                        <div class="mt-4">
                            <asp:Button ID="btnCreate" runat="server" Text="Create Super Admin" CssClass="btn btn-primary w-100" OnClick="btnCreate_Click" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- SweetAlert -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <script>
        function togglePassword(id, btn) {
            var input = document.getElementById(id);
            var icon = btn.querySelector('i');
            if (input.type === "password") {
                input.type = "text";
                icon.classList.remove("fa-eye");
                icon.classList.add("fa-eye-slash");
            } else {
                input.type = "password";
                icon.classList.remove("fa-eye-slash");
                icon.classList.add("fa-eye");
            }
        }

        function showSuccess(message) {
            Swal.fire({
                icon: 'success',
                title: 'Success',
                text: message,
                confirmButtonColor: '#3085d6'
            }).then(() => {
                // Go to the list of super admins instead of dashboard
                window.location.href = 'ViewUser.aspx';
            });
        }

        function showError(message) {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: message,
                confirmButtonColor: '#d33'
            });
        }
    </script>
</asp:Content>
