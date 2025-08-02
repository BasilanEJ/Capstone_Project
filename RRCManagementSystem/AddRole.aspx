<%@ Page Title="" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="AddRole.aspx.cs" Inherits="RRCManagementSystem.AddRole" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <script type="text/javascript">
        function showSaveConfirmation() {
            Swal.fire({
                title: 'Add Role?',
                text: 'Are you sure you want to add this role?',
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
            return false; // prevent default postback
        }
    </script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div style="max-width: 500px; margin: 0 auto; background: #ffffff; padding: 30px; border-radius: 10px; box-shadow: 0 4px 12px rgba(0,0,0,0.1);">
        <h2 style="text-align:center; color:#1f2937; margin-bottom:20px;">Add New Role</h2>

        <div class="form-group" style="margin-bottom:15px;">
            <label for="txtRoleName" style="display:block; margin-bottom:5px;">Role Name</label>
            <asp:TextBox ID="txtRoleName" runat="server" CssClass="form-control" placeholder="Enter role name" Style="width:100%; padding:10px; border:1px solid #ccc; border-radius:5px;" />
        </div>

        <div style="text-align:center;">
            <asp:Button ID="btnSave" runat="server" Text="Add Role"
                OnClientClick="return showSaveConfirmation();" UseSubmitBehavior="false"
                Style="padding:10px 20px; background-color:#1f2937; color:white; border:none; border-radius:5px; cursor:pointer;" />
        </div>

        <asp:Button ID="btnSaveHidden" runat="server" OnClick="btnSave_Click" Style="display:none;" />

        <div style="margin-top:15px; text-align:center;">
            <asp:Label ID="lblMessage" runat="server" Text="" ForeColor="Red" />
        </div>
    </div>
</asp:Content>
