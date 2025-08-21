<%@ Page Title="" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="EditAdmin.aspx.cs" Inherits="RRCManagementSystem.EditAdmin" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<style>
    /* Checkbox Scaling */
    .checkbox-scale input[type="checkbox"] {
        transform: scale(1.3);
        cursor: pointer;
        margin: 5px;
    }
        .checkbox-scale{
        transform: scale(1.3);
        cursor: pointer;
        margin: 5px;
    }

    body {
        background-color: #f3f4f6;
        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
    }

    .container {
        width: 60%;
        margin: 40px auto;
        background-color: #ffffff;
        padding: 30px;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
        border-radius: 10px;
    }

    h2 {
        text-align: center;
        color: #333333;
        margin-bottom: 20px;
    }

    h3 {
        color: #007bff;
        margin-top: 30px;
        margin-bottom: 15px;
    }

    .form-group {
        margin-bottom: 20px;
    }

    label {
        display: block;
        font-weight: 600;
        margin-bottom: 8px;
        color: #333333;
    }

    .form-control {
        width: 100%;
        padding: 12px 15px;
        border: 1px solid #ccc;
        border-radius: 6px;
        font-size: 14px;
    }

    .btn-submit {
        display: block;
        width: 100%;
        padding: 12px 20px;
        background-color: #007bff;
        color: white;
        border: none;
        border-radius: 6px;
        font-size: 16px;
        font-weight: bold;
        cursor: pointer;
        transition: background-color 0.3s, transform 0.2s;
        margin-top: 25px;
    }

    .btn-submit:hover {
        background-color: #0056b3;
        transform: translateY(-2px);
    }

    .alert-message {
        text-align: center;
        font-size: 14px;
        color: #e74c3c;
        margin-bottom: 15px;
    }

    @media (max-width: 768px) {
        .container {
            width: 90%;
            padding: 20px;
        }

        .btn-submit {
            font-size: 14px;
        }
    }
</style>


    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <script>
        function toggleAll(masterCheckbox, className) {
            const checkboxes = document.querySelectorAll(`.${className} input[type="checkbox"]`);
            checkboxes.forEach(cb => {
                cb.checked = masterCheckbox.checked;
                cb.dispatchEvent(new Event('change', { bubbles: true }));
            });
        }

        function ensureView(cb) {
            const row = cb.closest('tr');
            if (!row) return;
            const viewCheckbox = row.querySelector('.chkView input[type="checkbox"]');
            if (cb.checked && viewCheckbox && !viewCheckbox.checked) {
                viewCheckbox.checked = true;
            }
        }

        function confirmSave() {
            Swal.fire({
                title: 'Are you sure?',
                text: "This will save the changes you've made.",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#007bff',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes, save it!',
                cancelButtonText: 'Cancel'
            }).then((result) => {
                if (result.isConfirmed) {
                    document.getElementById('<%= btnSave.ClientID %>').click();
                }
            });
        }
    </script>

    <div class="container">
        <h2>Edit Admin</h2>

        <asp:Label ID="lblMessage" runat="server" CssClass="alert-message" />

        <div class="form-group">
            <label for="txtName">Name</label>
            <asp:TextBox ID="txtName" runat="server" CssClass="form-control" placeholder="Enter full name" />
        </div>

       <div class="form-group">
    <label for="txtEmail">Email</label>
    <asp:TextBox 
        ID="txtEmail" 
        runat="server" 
        CssClass="form-control" 
        placeholder="Enter email address" 
        ReadOnly="true" />
</div>


        <h3>Module Permissions</h3>
        <asp:Repeater ID="rptPermissions" runat="server">
            <HeaderTemplate>
                <table class="table table-bordered" style="width:100%; border-collapse: collapse; text-align:left;">
                    <thead style="background-color:#f0f0f0;">
                        <tr>
                            <th style="padding:10px;">Module</th>
                            <th class="chkView">View <input type="checkbox" class="checkbox-scale" onclick="toggleAll(this, 'chkView')" /></th>
                            <th class="chkAdd">Add <input type="checkbox" class="checkbox-scale" onclick="toggleAll(this, 'chkAdd')" /></th>
                            <th class="chkEdit">Edit <input type="checkbox" class="checkbox-scale" onclick="toggleAll(this, 'chkEdit')" /></th>
                            <th class="chkDelete">Delete <input type="checkbox" class="checkbox-scale" onclick="toggleAll(this, 'chkDelete')" /></th>
                        </tr>
                    </thead>
                    <tbody>
            </HeaderTemplate>
            <ItemTemplate>
              <tr>
                    <td><%# Eval("ModuleName") %>
                        <asp:HiddenField ID="hfModuleName" runat="server" Value='<%# Eval("ModuleName") %>' />
                    </td>
                    <td class="chkView text-center">
                        <asp:CheckBox ID="chkView" runat="server" CssClass="checkbox-scale" Checked='<%# Eval("CanView") %>' />
                    </td>
                    <td class="chkAdd text-center">
                        <asp:CheckBox ID="chkAdd" runat="server" CssClass="checkbox-scale" Checked='<%# Eval("CanAdd") %>' onclick="ensureView(this)" />
                    </td>
                    <td class="chkEdit text-center">
                        <asp:CheckBox ID="chkEdit" runat="server" CssClass="checkbox-scale" Checked='<%# Eval("CanEdit") %>' onclick="ensureView(this)" />
                    </td>
                    <td class="chkDelete text-center">
                        <asp:CheckBox ID="chkDelete" runat="server" CssClass="checkbox-scale" Checked='<%# Eval("CanDelete") %>' onclick="ensureView(this)" />
                    </td>
                </tr>

            </ItemTemplate>
            <FooterTemplate>
                    </tbody>
                </table>
            </FooterTemplate>
        </asp:Repeater>

        <asp:Button ID="btnSave" runat="server" Style="display:none;" OnClick="btnSave_Click" />
        <button type="button" class="btn-submit" onclick="confirmSave()">Save Changes</button>
    </div>
</asp:Content>