<%@ Page Title="" Language="C#" MasterPageFile="~/RootAdmin.Master" AutoEventWireup="true" CodeBehind="ViewUser.aspx.cs" Inherits="RRCManagementSystem.ViewUser" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mt-4">
        <!-- Header Section -->
        <div class="d-flex justify-content-between align-items-center mb-3">
            <h2 class="mb-0 text-dark">Super Admin Accounts</h2>
            <a href="AddSuperUser.aspx" class="btn btn-primary">
                <i class="fa fa-plus"></i> Add Super Admin
            </a>
        </div>

        <!-- Search Section -->
        <div class="mb-3">
            <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control" placeholder="Search by name or email..." AutoPostBack="true" OnTextChanged="txtSearch_TextChanged" />
        </div>

        <!-- Users Grid -->
        <asp:UpdatePanel ID="updUsers" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="table-responsive">
                    <asp:GridView ID="gvUsers" runat="server" AutoGenerateColumns="False" CssClass="table table-striped table-hover table-bordered"
                        DataKeyNames="UserID" AllowPaging="True" PageSize="10"
                        OnPageIndexChanging="gvUsers_PageIndexChanging">
                        <Columns>
                            <asp:BoundField DataField="Email" HeaderText="Email" SortExpression="Email" />
                            <asp:BoundField DataField="Name" HeaderText="Full Name" SortExpression="Name" />
                            <asp:BoundField DataField="RoleName" HeaderText="Role" SortExpression="RoleName" />
                            <asp:BoundField DataField="CreatedAt" HeaderText="Created At" DataFormatString="{0:yyyy-MM-dd}" SortExpression="CreatedAt" />
                            <asp:TemplateField HeaderText="Actions">
                                <ItemTemplate>
                                    <button type="button" class="btn btn-sm btn-warning me-2" 
                                            onclick='openEditModal(<%# Eval("UserID") %>, "<%# Eval("Name") %>", "<%# Eval("Email") %>")'>
                                        <i class="fa fa-edit"></i> Edit
                                    </button>
                                    <button type="button" class="btn btn-sm btn-danger"
                                            onclick='confirmArchiveManual(<%# Eval("UserID") %>)'>
                                        <i class="fa fa-archive"></i> Archive
                                    </button>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="lnkHiddenArchive" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnSaveEdit" EventName="Click" />
            </Triggers>
        </asp:UpdatePanel>

        <!-- Hidden controls outside UpdatePanel -->
        <asp:HiddenField ID="hfArchiveUserID" runat="server" />
        <asp:LinkButton ID="lnkHiddenArchive" runat="server" OnClick="lnkHiddenArchive_Click" Style="display:none"></asp:LinkButton>
    </div>

    <!-- Edit Modal -->
    <div class="modal fade" id="editModal" tabindex="-1" aria-labelledby="editModalLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header" style="background-color: royalblue; color: white;">
                    <h5 class="modal-title" id="editModalLabel">Edit Super Admin</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <asp:HiddenField ID="hfEditUserID" runat="server" />
                    <div class="mb-3">
                        <label>Email</label>
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" ReadOnly="true" />
                    </div>
                    <div class="mb-3">
                        <label>Full Name</label>
                        <asp:TextBox ID="txtName" runat="server" CssClass="form-control" />
                        <span id="nameError" class="text-danger d-none">Name cannot be empty!</span>
                    </div>
                </div>
                <div class="modal-footer">
                    <asp:Button ID="btnSaveEdit" runat="server" CssClass="btn btn-success" Text="Save Changes"
                        OnClientClick="return validateName();" OnClick="btnSaveEdit_Click" />
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                </div>
            </div>
        </div>
    </div>

    <!-- SweetAlert & JS -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <script>
        function openEditModal(userId, name, email) {
            document.getElementById('<%= hfEditUserID.ClientID %>').value = userId;
            document.getElementById('<%= txtName.ClientID %>').value = name;
            document.getElementById('<%= txtEmail.ClientID %>').value = email;
            document.getElementById("nameError").classList.add("d-none");

            var editModalEl = document.getElementById('editModal');
            var editModal = new bootstrap.Modal(editModalEl);
            editModal.show();

            editModalEl.addEventListener('shown.bs.modal', function () {
                document.getElementById('<%= txtName.ClientID %>').focus();
            }, { once: true });
        }

        function validateName() {
            var nameField = document.getElementById('<%= txtName.ClientID %>');
            if (nameField.value.trim() === "") {
                document.getElementById("nameError").classList.remove("d-none");
                return false;
            }
            return true;
        }

        function confirmArchiveManual(userID) {
            Swal.fire({
                title: 'Are you sure?',
                text: "This user will be archived!",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6',
                confirmButtonText: 'Yes, archive it!'
            }).then((result) => {
                if (result.isConfirmed) {
                    document.getElementById('<%= hfArchiveUserID.ClientID %>').value = userID;
                    document.getElementById('<%= lnkHiddenArchive.ClientID %>').click();
                }
            });
            return false;
        }
    </script>
</asp:Content>
