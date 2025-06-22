<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="EditItem.aspx.cs" Inherits="RRCManagementSystem.EditItem" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Bootstrap 5 CSS -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <!-- SweetAlert2 -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="container my-5" style="max-width: 800px;">
        <div class="card shadow-sm">
            <div class="card-header bg-primary text-white text-center fs-4 fw-bold">
                Edit Item
            </div>
            <div class="card-body">

                <asp:Label ID="lblMessage" runat="server" CssClass="text-danger fw-semibold mb-3 d-block" />

                <div class="mb-3">
                    <label for="txtItemName" class="form-label fw-semibold">Item Name</label>
                    <asp:TextBox ID="txtItemName" runat="server" CssClass="form-control" placeholder="Enter item name" />
                </div>

                <div class="mb-3">
                    <label for="ddlType" class="form-label fw-semibold">Item Type</label>
                    <asp:DropDownList ID="ddlType" runat="server" CssClass="form-select" AutoPostBack="false">
                        <asp:ListItem Text="Select Type" Value="" />
                        <asp:ListItem Text="Bottled Chemical" Value="Bottled Chemical" />
                        <asp:ListItem Text="Sachet Pack Chemical" Value="Sachet Pack Chemical" />
                        <asp:ListItem Text="Safety Gear" Value="Safety Gear" />
                    </asp:DropDownList>
                </div>

                <div class="mb-3">
                    <label for="txtQuantity" class="form-label fw-semibold">Quantity</label>
                    <asp:TextBox ID="txtQuantity" runat="server" CssClass="form-control" placeholder="Enter quantity"
                        oninput="this.value = this.value.replace(/[^0-9]/g, '')" />
                    <small id="bottleNote" class="form-text text-muted" style="display:none;">
                        💡 Bottled Chemical: 1 box = 10 bottles | 1 bottle = 1L
                    </small>
                    <small id="sachetNote" class="form-text text-muted" style="display:none;">
                        💡 Sachet Pack Chemical: Input quantity in packs
                    </small>
                    <small id="gearNote" class="form-text text-muted" style="display:none;">
                        💡 Safety Gear: Input quantity per item (e.g., gloves, masks)
                    </small>
                </div>

                <div class="mb-3">
                    <label for="txtExpirationDate" class="form-label fw-semibold">Expiration Date</label>
                    <asp:TextBox ID="txtExpirationDate" runat="server" CssClass="form-control" TextMode="Date" />
                </div>

                <div class="mb-3">
                    <label for="fuItemImage" class="form-label fw-semibold">Item Image</label>
                    <asp:FileUpload ID="fuItemImage" runat="server" CssClass="form-control" onchange="previewImage(event)" />
                </div>

                <div class="mb-4 text-center">
                    <asp:Image ID="imgPreview" runat="server" Width="120" Height="120" Style="display:none; object-fit:cover; border-radius:6px; border:1px solid #ccc;" />
                </div>

                <div class="d-flex justify-content-center gap-3 flex-wrap">
                    <asp:Button ID="btnSave" runat="server" Text="Save Changes" CssClass="btn btn-primary px-4"
                        OnClientClick="return confirmSave();" OnClick="btnSave_Click" />

                    <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-secondary px-4"
                        OnClientClick="window.location.href='ViewItem.aspx'; return false;" />
                </div>
            </div>
        </div>
    </div>

    <script>
        // Show notes based on item type selected
        function updateQuantityNote() {
            var ddlType = document.getElementById('<%= ddlType.ClientID %>');
            var typeValue = ddlType.value;

            document.getElementById('bottleNote').style.display = (typeValue === 'Bottled Chemical') ? 'block' : 'none';
            document.getElementById('sachetNote').style.display = (typeValue === 'Sachet Pack Chemical') ? 'block' : 'none';
            document.getElementById('gearNote').style.display = (typeValue === 'Safety Gear') ? 'block' : 'none';
        }

        window.onload = function () {
            updateQuantityNote();
            document.getElementById('<%= ddlType.ClientID %>').addEventListener('change', updateQuantityNote);
        };

        // Image preview
        function previewImage(event) {
            var file = event.target.files[0];
            var reader = new FileReader();

            reader.onload = function (e) {
                var imgPreview = document.getElementById('<%= imgPreview.ClientID %>');
                imgPreview.src = e.target.result;
                imgPreview.style.display = 'inline-block';
            };

            if (file) {
                reader.readAsDataURL(file);
            }
        }

        // SweetAlert confirm on save
        function confirmSave() {
            event.preventDefault();
            Swal.fire({
                title: 'Are you sure?',
                text: "Do you want to save the changes?",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#0d6efd',
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
