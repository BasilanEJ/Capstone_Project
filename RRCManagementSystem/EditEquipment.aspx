<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="EditEquipment.aspx.cs" Inherits="RRCManagementSystem.EditEquipment" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        /* Same styling as AddEquipment for consistency */
        .container { margin-top: 40px; padding: 20px; background-color: #f9f9f9; }
        .card { border-radius: 8px; border: 1px solid #ddd; box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1); background-color: #fff; }
        .card-header { background-color: #007bff; color: #fff; font-size: 20px; font-weight: bold; padding: 15px 20px; border-radius: 8px 8px 0 0; text-align: center; }
        .card-body { padding: 25px; }
        .form-group { margin-bottom: 20px; }
        label { font-weight: 600; display: block; margin-bottom: 8px; color: #333; }
        .form-control { width: 100%; padding: 10px 14px; border-radius: 5px; border: 1px solid #ccc; font-size: 14px; }
        .btn { padding: 10px 20px; border-radius: 5px; font-size: 16px; cursor: pointer; font-weight: 600; transition: background-color 0.3s ease, transform 0.2s ease; }
        .btn-primary { background-color: #007bff; color: #fff; border: none; }
        .btn-primary:hover { background-color: #0056b3; transform: translateY(-1px); }
        .btn-secondary { background-color: #6c757d; color: #fff; border: none; }
        .btn-secondary:hover { background-color: #5a6268; transform: translateY(-1px); }
        .error-message { color: red; margin-bottom: 15px; text-align: center; }
        #imagePreview { display: block; width: 120px; height: 120px; object-fit: cover; border-radius: 8px; border: 1px solid #ddd; margin-top: 10px; }
    </style>

    <div class="container">
        <div class="card shadow-sm">
            <div class="card-header">
                <h2 class="page-title">Edit Equipment</h2>
            </div>
            <div class="card-body">
                <asp:Label ID="lblMessage" runat="server" CssClass="error-message" />

                <asp:HiddenField ID="hfEquipmentID" runat="server" />

                <!-- Equipment ID (ReadOnly) -->
                <div class="form-group">
                    <label for="txtEquipmentID">Equipment ID:</label>
                    <asp:TextBox ID="txtEquipmentID" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
                </div>

                <!-- Equipment Name -->
                <div class="form-group">
                    <label for="txtEquipmentName">Equipment Name:</label>
                    <asp:TextBox ID="txtEquipmentName" runat="server" CssClass="form-control"></asp:TextBox>
                </div>

                <!-- Status -->
                <div class="form-group">
                    <label for="ddlStatus">Status:</label>
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-control">
                        <asp:ListItem Text="Select Status" Value="" />
                        <asp:ListItem Text="Available" Value="Available" />
                        <asp:ListItem Text="Unavailable" Value="Unavailable" />
                        <asp:ListItem Text="Under Maintenance" Value="Under Maintenance" />
                    </asp:DropDownList>
                </div>

                <!-- Image Upload -->
                <div class="form-group">
                    <label for="fuEquipmentImage">Change Image (optional):</label>
                    <asp:FileUpload ID="fuEquipmentImage" runat="server" accept="image/*" onchange="validateImage(this); previewImage(event);" />
                    <img id="imagePreview" runat="server" src="#" alt="Equipment Image" />
                </div>

                <!-- Buttons -->
                <div class="form-group text-center">
                    <asp:Button ID="btnUpdate" runat="server" Text="Update Equipment"
                        CssClass="btn btn-primary" UseSubmitBehavior="false"
                        OnClientClick="return showConfirmUpdate();" OnClick="btnUpdate_Click" />

                    <asp:Button ID="btnCancel" runat="server" Text="Cancel"
                        CssClass="btn btn-secondary" UseSubmitBehavior="false"
                        OnClientClick="return showConfirmCancel();" />
                </div>
            </div>
        </div>
    </div>

    <script>
        function previewImage(event) {
            var file = event.target.files[0];
            if (file) {
                var reader = new FileReader();
                reader.onload = function (e) {
                    var img = document.getElementById("imagePreview");
                    img.src = e.target.result;
                    img.style.display = "block";
                };
                reader.readAsDataURL(file);
            }
        }

        function validateImage(input) {
            var filePath = input.value;
            var allowedExtensions = /(\.jpg|\.jpeg|\.png)$/i;
            if (!allowedExtensions.exec(filePath)) {
                Swal.fire({
                    icon: 'error',
                    title: 'Invalid File Type',
                    text: 'Only JPG, JPEG, and PNG files are allowed.'
                });
                input.value = '';
                document.getElementById("imagePreview").style.display = "none";
                return false;
            }
        }

        function showConfirmUpdate() {
            event.preventDefault();
            Swal.fire({
                title: 'Update Equipment?',
                text: 'Are you sure you want to update this equipment?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#007bff',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes, update it'
            }).then((result) => {
                if (result.isConfirmed) {
                    document.getElementById('<%= btnUpdate.ClientID %>').disabled = true;
                    __doPostBack('<%= btnUpdate.UniqueID %>', '');
                }
            });
            return false;
        }

        function showConfirmCancel() {
            event.preventDefault();
            Swal.fire({
                title: 'Cancel?',
                text: 'Are you sure you want to cancel? Unsaved changes will be lost.',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#6c757d',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes, cancel'
            }).then((result) => {
                if (result.isConfirmed) {
                    window.location.href = 'ViewEquipment.aspx';
                }
            });
            return false;
        }
    </script>
</asp:Content>


