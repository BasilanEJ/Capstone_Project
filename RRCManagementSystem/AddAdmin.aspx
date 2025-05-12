<%@ Page Title="" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="AddAdmin.aspx.cs" Inherits="RRCManagementSystem.AddAdmin" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .form-container {
            width: 60%;
            margin: 0 auto;
            background-color: #ffffff;
            padding: 30px;
            border-radius: 8px;
            box-shadow: 0px 2px 10px rgba(0,0,0,0.1);
        }

        .form-container h2 {
            text-align: center;
            margin-bottom: 30px;
            color: #1f2937;
        }

        .form-group {
            margin-bottom: 20px;
        }

        .form-group label {
            display: block;
            margin-bottom: 8px;
            font-weight: bold;
            color: #374151;
        }

        .form-control {
            width: 100%;
            padding: 10px;
            border: 1px solid #ccc;
            border-radius: 4px;
        }

        .btn-submit {
            background-color: #007bff;
            color: white;
            padding: 12px;
            border: none;
            border-radius: 4px;
            width: 100%;
            cursor: pointer;
            font-weight: bold;
        }

        .btn-submit:hover {
            background-color: #0056b3;
        }

        .note {
            font-size: 14px;
            color: #6b7280;
            margin-bottom: 20px;
        }

        .permissions-section {
            margin-top: 30px;
        }
    </style>

    <div class="form-container">
        <h2>Add New Account</h2>

        <asp:Label ID="lblMessage" runat="server" CssClass="alert-message" ForeColor="Red" Visible="false"></asp:Label>

        <div class="note">
            Once you create this account, an email will be sent inviting the user to set their password.
        </div>

        <div class="form-group">
            <label for="txtName">Name *</label>
            <asp:TextBox ID="txtName" runat="server" CssClass="form-control" placeholder="Enter full name" />
        </div>

        <div class="form-group">
            <label for="txtEmail">Email *</label>
            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" placeholder="Enter email address" TextMode="Email" />
        </div>

   <asp:DropDownList ID="ddlRole" runat="server" CssClass="form-control"
    AutoPostBack="true" OnSelectedIndexChanged="ddlRole_SelectedIndexChanged">
</asp:DropDownList>


        <!-- Hidden ASP.NET Button (triggers postback) -->
        <asp:Button ID="btnSubmit" runat="server" Style="display:none;" OnClick="btnSubmit_Click" />

        <!-- Visible button with SweetAlert -->
        <button type="button" class="btn-submit" onclick="confirmCreate()">Create Account</button>

        <!-- Permissions Section -->
        <div class="permissions-section">
            <asp:Repeater ID="rptPermissions" runat="server" Visible="false">
                <HeaderTemplate>
                    <table class="table table-bordered" style="width:100%; border-collapse: collapse;">
                        <thead style="background-color:#f0f0f0;">
                            <tr>
                                <th style="padding:10px;">Module</th>
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
                        <td style="padding:10px;"><%# Eval("ModuleName") %></td>
                        <td><asp:CheckBox ID="chkView" runat="server" Checked='<%# Eval("CanView") %>' CssClass="chkView" /></td>
                        <td><asp:CheckBox ID="chkAdd" runat="server" Checked='<%# Eval("CanAdd") %>' CssClass="chkAdd" onclick="ensureView(this)" /></td>
                        <td><asp:CheckBox ID="chkEdit" runat="server" Checked='<%# Eval("CanEdit") %>' CssClass="chkEdit" onclick="ensureView(this)" /></td>
                        <td><asp:CheckBox ID="chkDelete" runat="server" Checked='<%# Eval("CanDelete") %>' CssClass="chkDelete" onclick="ensureView(this)" /></td>
                        <asp:HiddenField ID="hfModuleName" runat="server" Value='<%# Eval("ModuleName") %>' />
                    </tr>
                </ItemTemplate>
                <FooterTemplate>
                        </tbody>
                    </table>
                </FooterTemplate>
            </asp:Repeater>
        </div>
    </div>

    <!-- SweetAlert2 Library -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

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