<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="EditServices.aspx.cs" Inherits="RRCManagementSystem.EditServices" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Bootstrap 5 CSS -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <!-- SweetAlert2 -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container my-5" style="max-width: 600px;">
        <div class="card shadow-sm">
            <div class="card-header bg-primary text-white fw-bold fs-5">
                Edit Service
            </div>
            <div class="card-body">

                <asp:Label ID="lblMessage" runat="server" CssClass="alert alert-info d-block" />

                <div class="mb-3">
                    <label for="txtName" class="form-label fw-semibold">Service Name *</label>
                    <asp:TextBox ID="txtName" runat="server" CssClass="form-control" />
                </div>

                <div class="mb-3">
                    <label for="txtDescription" class="form-label fw-semibold">Description</label>
                    <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" />
                </div>

                <div class="mb-3">
                    <label for="txtPrice100" class="form-label fw-semibold">Price for 100 SQM (₱)</label>
                    <asp:TextBox ID="txtPrice100" runat="server" CssClass="form-control" TextMode="Number" />
                </div>

                <div class="mb-3">
                    <label for="txtPrice200" class="form-label fw-semibold">Price for 200 SQM (₱)</label>
                    <asp:TextBox ID="txtPrice200" runat="server" CssClass="form-control" TextMode="Number" />
                </div>

                <div class="mb-4">
                    <label for="txtPriceAbove200" class="form-label fw-semibold">Price for 200 SQM and above (₱)</label>
                    <asp:TextBox ID="txtPriceAbove200" runat="server" CssClass="form-control" TextMode="Number" />
                </div>

                <div class="d-flex flex-wrap gap-2 justify-content-center">
                    <asp:Button ID="btnUpdate" runat="server" Text="Update Service" CssClass="btn btn-primary px-4"
                        OnClientClick="return confirmUpdate();" OnClick="btnUpdate_Click" />

                    <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-secondary px-4"
                        PostBackUrl="~/ADMIN/ViewServices.aspx" />
                </div>
            </div>
        </div>
    </div>

    <script>
        function confirmUpdate() {
            event.preventDefault();
            Swal.fire({
                title: 'Are you sure?',
                text: "Do you want to update this service?",
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
    </script>

    <!-- Bootstrap 5 JS Bundle -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</asp:Content>
