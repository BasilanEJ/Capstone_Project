<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="EditItem.aspx.cs" Inherits="RRCManagementSystem.EditItem" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        body {
            font-family: Arial, sans-serif;
            background-color: #f4f6f9;
        }

        .container {
            max-width: 800px;
            margin: 40px auto;
            padding: 20px;
        }

        .card {
            background-color: #fff;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
            padding: 30px;
        }

        .card-header {
            background-color: #007bff;
            color: white;
            text-align: center;
            padding: 15px;
            font-size: 24px;
        }

        .form-group {
            margin-bottom: 20px;
        }

        label {
            font-weight: bold;
            display: block;
            margin-bottom: 8px;
        }

        .form-control {
            width: 100%;
            padding: 10px;
            font-size: 14px;
            border: 1px solid #ccc;
            border-radius: 4px;
        }

        .btn {
            padding: 12px 20px;
            border: none;
            border-radius: 4px;
            font-weight: bold;
            cursor: pointer;
        }

        .btn-primary {
            background-color: #007bff;
            color: white;
        }

        .btn-secondary {
            background-color: #6c757d;
            color: white;
        }

        .btn-primary:hover {
            background-color: #0056b3;
        }

        #imagePreview {
            display: block;
            margin: 10px 0;
            width: 120px;
            height: 120px;
            object-fit: cover;
            border: 1px solid #ccc;
            border-radius: 6px;
        }

        .error-message {
            color: red;
            font-weight: bold;
            margin-bottom: 15px;
        }
    </style>

    <div class="container">
        <div class="card">
            <div class="card-header">Edit Item</div>
            <div class="card-body">
                <asp:Label ID="lblMessage" runat="server" CssClass="error-message" />

                <div class="form-group">
                    <label for="txtItemName">Item Name</label>
                    <asp:TextBox ID="txtItemName" runat="server" CssClass="form-control" placeholder="Enter item name" />
                </div>

                <div class="form-group">
                    <label for="ddlType">Item Type</label>
                  <asp:DropDownList ID="ddlType" runat="server" CssClass="form-control">
                    <asp:ListItem Text="Select Type" Value="" />
                    <asp:ListItem Text="Bottled Chemical" Value="Bottled Chemical" />
                    <asp:ListItem Text="Sachet Pack Chemical" Value="Sachet Pack Chemical" />
                    <asp:ListItem Text="Safety Gear" Value="Safety Gear" />
                </asp:DropDownList>

                </div>

                               <!-- Quantity (in bottles) -->
               <!-- Quantity -->
<div class="form-group">
    <label for="txtQuantity">Quantity:</label>
    <asp:TextBox ID="txtQuantity" runat="server" CssClass="form-control"
        placeholder="Enter quantity" required 
        oninput="this.value = this.value.replace(/[^0-9]/g, '')">
    </asp:TextBox>

    <!-- Type-specific notes -->
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

<!-- JavaScript -->
<script type="text/javascript">
    function updateQuantityNote() {
        var ddlType = document.getElementById('<%= ddlType.ClientID %>');
        var typeValue = ddlType.value;

        document.getElementById('bottleNote').style.display = (typeValue === 'Bottled Chemical') ? 'inline' : 'none';
        document.getElementById('sachetNote').style.display = (typeValue === 'Sachet Pack Chemical') ? 'inline' : 'none';
        document.getElementById('gearNote').style.display = (typeValue === 'Safety Gear') ? 'inline' : 'none';
    }

    window.onload = function () {
        updateQuantityNote(); // run on load
        document.getElementById('<%= ddlType.ClientID %>').addEventListener('change', updateQuantityNote);
    };
</script>



                <div class="form-group">
                    <label for="txtExpirationDate">Expiration Date</label>
                    <asp:TextBox ID="txtExpirationDate" runat="server" CssClass="form-control" TextMode="Date" />
                </div>

                <div class="form-group">
                    <label for="fuItemImage">Item Image</label>
                    <asp:FileUpload ID="fuItemImage" runat="server" onchange="previewImage(event)" />
                    <asp:Image ID="imgPreview" runat="server" Width="120px" Height="120px" Style="display:none;" />
                </div>

                <div class="form-group text-center">
                    <asp:Button ID="btnSave" runat="server" Text="Save Changes" CssClass="btn btn-primary" OnClick="btnSave_Click"
                        OnClientClick="return confirm('Are you sure you want to save changes?');" />

                    <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-secondary"
                        OnClientClick="window.location.href='ViewItem.aspx'; return false;" />
                </div>
            </div>
        </div>
    </div>

    <script>
        function previewImage(event) {
            var file = event.target.files[0];
            var reader = new FileReader();

            reader.onload = function (e) {
                var imgPreview = document.getElementById("<%= imgPreview.ClientID %>");
                imgPreview.src = e.target.result;
                imgPreview.style.display = "block";
            }

            if (file) {
                reader.readAsDataURL(file);
            }
        }
    </script>
</asp:Content>
