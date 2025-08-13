<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="EditEmployee.aspx.cs" Inherits="RRCManagementSystem.EditEmployee" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Bootstrap 5 CSS -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <!-- SweetAlert2 -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="container my-5" style="max-width:700px;">
        <div class="card shadow-sm">
            <div class="card-header bg-primary text-white text-center fs-4 fw-bold">
                Edit Employee
            </div>
            <div class="card-body">

                <asp:Label ID="lblMessage" runat="server" CssClass="text-danger fw-semibold d-block mb-3" />

              <div class="mb-3">
    <label for="txtLastName" class="form-label fw-semibold">Last Name:</label>
    <asp:TextBox ID="txtLastName" runat="server" CssClass="form-control" 
        placeholder="Enter last name" required
        oninput="this.value = this.value.replace(/[^a-zA-Z\s]/g, '');">
    </asp:TextBox>
</div>

<div class="mb-3">
    <label for="txtFirstName" class="form-label fw-semibold">First Name:</label>
    <asp:TextBox ID="txtFirstName" runat="server" CssClass="form-control" 
        placeholder="Enter first name" required
        oninput="this.value = this.value.replace(/[^a-zA-Z\s]/g, '');">
    </asp:TextBox>
</div>

<div class="mb-3">
    <label for="txtMiddleName" class="form-label fw-semibold">
        Middle Name <span class="text-muted">(optional)</span>:
    </label>
    <asp:TextBox ID="txtMiddleName" runat="server" CssClass="form-control" 
        placeholder="Enter middle name (optional)" 
        oninput="this.value = this.value.replace(/[^a-zA-Z\s]/g, '');">
    </asp:TextBox>
</div>



                <div class="mb-3">
                    <label for="txtEmail" class="form-label fw-semibold">Email:</label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" placeholder="Enter email address" required></asp:TextBox>
                </div>

                <div class="mb-3">
                    <label for="txtPhone" class="form-label fw-semibold">Phone Number:</label>
                    <asp:TextBox ID="txtPhone" runat="server" CssClass="form-control"
                        placeholder="Enter 11-digit Phone Number" required
                        oninput="this.value = this.value.replace(/[^0-9]/g, '').slice(0, 11)">
                    </asp:TextBox>
                </div>

                <div class="mb-3">
                    <label for="ddlPosition" class="form-label fw-semibold">Position:</label>
                    <asp:DropDownList ID="ddlPosition" runat="server" CssClass="form-select">
                        <asp:ListItem Text="Select Position" Value="" />
                        <asp:ListItem Text="IT" Value="IT" />
                        <asp:ListItem Text="Technician" Value="Technician" />
                        <asp:ListItem Text="Inspector" Value="Inspector" />
                    </asp:DropDownList>
                </div>

                <div class="mb-3">
                    <label for="ddlStatus" class="form-label fw-semibold">Status:</label>
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select">
                        <asp:ListItem Text="Available" Value="Available" />
                        <asp:ListItem Text="Unavailable" Value="Unavailable" />
                        <asp:ListItem Text="Resigned" Value="Resigned" />
                    </asp:DropDownList>
                </div>

                <div class="mb-3">
                    <label for="fuProfilePicture" class="form-label fw-semibold">Profile Picture (JPG, JPEG, PNG only):</label>
                    <asp:FileUpload ID="fuProfilePicture" runat="server" accept="image/*" onchange="previewImage();" CssClass="form-control" />
                </div>

                <div class="mb-4 text-center">
                    <asp:Image ID="imgProfilePreview" runat="server" CssClass="rounded-circle img-thumbnail" Width="120" Height="120" Visible="false" />
                </div>

                <div class="d-flex justify-content-center gap-3 flex-wrap">
                    <asp:Button ID="btnSave" runat="server" Text="Save Changes" CssClass="btn btn-success px-4"
                        OnClientClick="return confirmSave();" OnClick="btnSave_Click" />

                    <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-secondary px-4"
                        OnClientClick="window.location.href='AllEmployee.aspx'; return false;" />
                </div>

            </div>
        </div>
    </div>

    <script>
        function previewImage() {
            var fileInput = document.getElementById('<%= fuProfilePicture.ClientID %>');
            var imgPreview = document.getElementById('<%= imgProfilePreview.ClientID %>');

            if (fileInput.files.length > 0) {
                var file = fileInput.files[0];
                var reader = new FileReader();

                reader.onload = function (e) {
                    imgPreview.src = e.target.result;
                    imgPreview.style.display = "inline-block";
                };
                reader.readAsDataURL(file);
            }
        }

        function confirmSave() {
            event.preventDefault();
            Swal.fire({
                title: 'Are you sure?',
                text: "Do you want to save the changes?",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#28a745',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes, save it!',
                cancelButtonText: 'Cancel'
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack('<%= btnSave.UniqueID %>', '');
                }
            });
            return false;
        }
    </script>

    <!-- Bootstrap 5 JS Bundle -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>

</asp:Content>
