<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="EditSupplier.aspx.cs" Inherits="RRCManagementSystem.EditSupplier" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Bootstrap 5 CSS -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <!-- SweetAlert2 -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container my-5" style="max-width: 700px;">
        <div class="card shadow-sm">
            <div class="card-header bg-primary text-white fs-4 fw-semibold text-center">
                Edit Supplier
            </div>
            <div class="card-body">

                <asp:Label ID="lblMessage" runat="server" CssClass="text-danger fw-semibold mb-3 d-block" />

                <asp:Panel ID="pnlEditSupplier" runat="server">
                    <div class="mb-3">
                        <label for="txtName" class="form-label fw-semibold">Supplier Name</label>
                        <asp:TextBox ID="txtName" runat="server" CssClass="form-control" />
                    </div>

                    <div class="mb-3">
                        <label for="txtCompanyName" class="form-label fw-semibold">Company Name</label>
                        <asp:TextBox ID="txtCompanyName" runat="server" CssClass="form-control" />
                    </div>

                    <div class="mb-3">
                        <label for="ddlBusinessType" class="form-label fw-semibold">Business Type</label>
                        <asp:DropDownList ID="ddlBusinessType" runat="server" CssClass="form-select">
                            <asp:ListItem Text="Select Business Type" Value="" />
                            <asp:ListItem Text="Equipment" Value="Equipment" />
                            <asp:ListItem Text="Chemicals" Value="Chemicals" />
                        </asp:DropDownList>
                    </div>

                    <div class="mb-3">
                        <label for="txtAddress" class="form-label fw-semibold">Address</label>
                        <asp:TextBox ID="txtAddress" runat="server" CssClass="form-control" />
                    </div>

                    <div class="mb-3">
                        <label for="txtContactNumber" class="form-label fw-semibold">Contact Number</label>
                        <asp:TextBox ID="txtContactNumber" runat="server" CssClass="form-control" />
                    </div>

                    <div class="mb-3">
                        <label for="txtEmail" class="form-label fw-semibold">Email</label>
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" />
                    </div>

                    <div class="mb-4">
                        <label for="ddlStatus" class="form-label fw-semibold">Status</label>
                        <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select">
                            <asp:ListItem Text="Select Status" Value="" />
                            <asp:ListItem Text="Active" Value="Active" />
                            <asp:ListItem Text="Inactive" Value="Inactive" />
                        </asp:DropDownList>
                    </div>

                    <div class="d-flex gap-2 flex-wrap justify-content-center">
                        <asp:Button ID="btnUpdate" runat="server" Text="Update Supplier" CssClass="btn btn-primary px-4"
                            OnClientClick="return confirmUpdate();" OnClick="btnUpdate_Click" />

                        <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-secondary px-4"
                            PostBackUrl="~/ADMIN/ViewSupplier.aspx" OnClientClick="return confirmCancel();" />
                    </div>
                </asp:Panel>
            </div>
        </div>
    </div>

    <script>
        function confirmUpdate() {
            event.preventDefault();
            Swal.fire({
                title: 'Update Supplier?',
                text: "Are you sure you want to update this supplier's information?",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#0d6efd',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes, update it!',
                cancelButtonText: 'Cancel'
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack('<%= btnUpdate.UniqueID %>', '');
                }
            });
            return false;
        }

        function confirmCancel() {
            event.preventDefault();
            Swal.fire({
                title: 'Cancel Editing?',
                text: "Your changes will not be saved. Continue?",
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#6c757d',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes, cancel',
                cancelButtonText: 'Continue Editing'
            }).then((result) => {
                if (result.isConfirmed) {
                    window.location.href = '<%= ResolveUrl("~/ADMIN/ViewSupplier.aspx") %>';
                }
            });
            return false;
        }
    </script>

    <!-- Bootstrap 5 JS Bundle -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</asp:Content>
