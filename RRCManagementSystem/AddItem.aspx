<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AddItem.aspx.cs" Inherits="RRCManagementSystem.AddItem" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- ✅ SweetAlert2 CDN -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <!-- ✅ INTERNAL CSS -->
    <style>
        body {
            font-family: Arial, sans-serif;
            background-color: #f4f6f9;
            margin: 0;
            padding: 0;
        }

        .container {
            max-width: 800px;
            margin: 40px auto;
            padding: 20px;
        }

        .card {
            background-color: #ffffff;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
            overflow: hidden;
            transition: all 0.3s ease-in-out;
        }

        .card-header {
            background-color: #007bff;
            color: #ffffff;
            padding: 16px 20px;
            text-align: center;
            font-size: 24px;
            font-weight: bold;
        }

        .card-body {
            padding: 30px;
        }

        .form-group {
            margin-bottom: 20px;
        }

        label {
            font-weight: bold;
            display: block;
            margin-bottom: 8px;
            color: #333333;
        }

        .form-control {
            width: 100%;
            padding: 12px 15px;
            border: 1px solid #ced4da;
            border-radius: 4px;
            font-size: 14px;
        }

        .form-control:focus {
            border-color: #007bff;
            outline: none;
        }

        .btn {
            padding: 12px 24px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            font-weight: bold;
            font-size: 16px;
            transition: background-color 0.3s ease-in-out;
            margin-right: 10px;
        }

        .btn-primary {
            background-color: #007bff;
            color: #ffffff;
        }

        .btn-primary:hover {
            background-color: #0056b3;
        }

        .btn-secondary {
            background-color: #6c757d;
            color: #ffffff;
        }

        .btn-secondary:hover {
            background-color: #5a6268;
        }

        .error-message {
            color: red;
            font-weight: bold;
            margin-bottom: 20px;
        }

        #imagePreview {
            display: none;
            width: 120px;
            height: 120px;
            margin-top: 10px;
            border-radius: 8px;
            border: 1px solid #ccc;
            object-fit: cover;
        }
    </style>

    <!-- ✅ FORM CONTENT -->
    <div class="container">
        <div class="card shadow-sm">
            <div class="card-header">Add New Item</div>
            <div class="card-body">
                <asp:Label ID="lblMessage" runat="server" CssClass="error-message" />

                <!-- Item Name -->
                <div class="form-group">
                    <label for="txtItemName">Item Name:</label>
                    <asp:TextBox ID="txtItemName" runat="server" CssClass="form-control" placeholder="Enter item name" required></asp:TextBox>
                </div>

                <!-- Item Type -->
                <div class="form-group">
                    <label for="ddlType">Item Type:</label>
                    <asp:DropDownList ID="ddlType" runat="server" CssClass="form-control">
                        <asp:ListItem Text="Select Type" Value="" />
                        <asp:ListItem Text="Bottled Chemical" Value="Bottled Chemical" />
                        <asp:ListItem Text="Sachet Pack Chemical" Value="Sachet Pack Chemical" />
                        <asp:ListItem Text="Safety Gear" Value="Safety Gear" />
                    </asp:DropDownList>
                </div>

                <!-- Quantity -->
                <div class="form-group">
                    <label for="txtQuantity">Quantity:</label>
                    <asp:TextBox ID="txtQuantity" runat="server" CssClass="form-control"
                        placeholder="Enter quantity" required 
                        oninput="this.value = this.value.replace(/[^0-9]/g, '')">
                    </asp:TextBox>

                    <small id="bottleNote" style="display:none; color:#666; font-style:italic;">
                        💡 Bottled Chemical: 1 box = 10 bottles | 1 bottle = 1L
                    </small>
                    <small id="sachetNote" style="display:none; color:#666; font-style:italic;">
                        💡 Sachet Pack Chemical: Input quantity in packs
                    </small>
                    <small id="gearNote" style="display:none; color:#666; font-style:italic;">
                        💡 Safety Gear: Input quantity per item (e.g., gloves, masks)
                    </small>
                </div>

                <!-- Excess ML -->
                <div class="form-group">
                    <label for="txtExcessML">Excess Chemical (in mL):</label>
                    <asp:TextBox ID="txtExcessML" runat="server" CssClass="form-control"
                        placeholder="Enter excess chemical in mL (optional)"
                        oninput="this.value = this.value.replace(/[^0-9]/g, '')">
                    </asp:TextBox>
                    <small style="color: #666; font-style: italic;">
                        💡 Example: 150 mL leftover from operation (1L = 1000mL ≈ 3 uses)
                    </small>
                </div>

                <!-- Expiration Date -->
                <div class="form-group">
                    <label for="txtExpirationDate">Expiration Date (if applicable):</label>
                    <asp:TextBox ID="txtExpirationDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                </div>

                <!-- Item Image -->
                <div class="form-group">
                    <label for="fuItemImage">Item Image (JPG, JPEG, PNG only):</label>
                    <asp:FileUpload ID="fuItemImage" runat="server" accept="image/*" onchange="validateImage(this); previewImage(event);" />
                    <img id="imagePreview" src="#" alt="Item Image Preview" />
                </div>

                <!-- Buttons -->
                <div class="form-group text-center">
                    <asp:Button ID="btnSubmit" runat="server" Text="Add Item"
                        CssClass="btn btn-primary"
                        UseSubmitBehavior="false"
                        OnClick="btnSubmit_Click"
                        OnClientClick="return showAddConfirm();" />

                    <asp:Button ID="btnCancel" runat="server" Text="Cancel"
                        CssClass="btn btn-secondary"
                        UseSubmitBehavior="false"
                        OnClientClick="return showCancelConfirm();" />
                </div>
            </div>
        </div>
    </div>

    <!-- ✅ SCRIPT -->
    <script>
        function previewImage(event) {
            var file = event.target.files[0];
            if (file) {
                var reader = new FileReader();
                reader.onload = function (e) {
                    var imgPreview = document.getElementById("imagePreview");
                    imgPreview.src = e.target.result;
                    imgPreview.style.display = "block";
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

        function showAddConfirm() {
            event.preventDefault();
            Swal.fire({
                title: 'Add Item?',
                text: 'Are you sure you want to add this item?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#007bff',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes, add it'
            }).then((result) => {
                if (result.isConfirmed) {
                    document.getElementById('<%= btnSubmit.ClientID %>').disabled = true;
                    __doPostBack('<%= btnSubmit.UniqueID %>', '');
                }
            });
            return false;
        }

        function showCancelConfirm() {
            event.preventDefault();
            Swal.fire({
                title: 'Cancel?',
                text: 'Are you sure you want to cancel? Unsaved data will be lost.',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#6c757d',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes, cancel'
            }).then((result) => {
                if (result.isConfirmed) {
                    window.location.href = 'ViewItem.aspx';
                }
            });
            return false;
        }

        function updateQuantityNote() {
            const type = document.getElementById('<%= ddlType.ClientID %>').value;
            document.getElementById('bottleNote').style.display = (type === 'Bottled Chemical') ? 'inline' : 'none';
            document.getElementById('sachetNote').style.display = (type === 'Sachet Pack Chemical') ? 'inline' : 'none';
            document.getElementById('gearNote').style.display = (type === 'Safety Gear') ? 'inline' : 'none';
        }

        window.onload = function () {
            updateQuantityNote();
            document.getElementById('<%= ddlType.ClientID %>').addEventListener('change', updateQuantityNote);
        };
    </script>

</asp:Content>
