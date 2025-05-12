<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="EditEmployee.aspx.cs" Inherits="RRCManagementSystem.EditEmployee" %>

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
            max-width: 700px;
            margin: 40px auto;
            padding: 20px;
        }

        .card {
            background-color: #fff;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
            overflow: hidden;
        }

        .card-header {
            background-color: #007bff;
            color: #fff;
            padding: 16px 20px;
            text-align: center;
            font-size: 20px;
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
            margin-bottom: 8px;
            display: block;
            color: #333;
        }

        .form-control {
            width: 100%;
            padding: 12px 15px;
            border: 1px solid #ced4da;
            border-radius: 4px;
            font-size: 14px;
            box-sizing: border-box;
            transition: border-color 0.3s ease-in-out;
        }

        .form-control:focus {
            border-color: #007bff;
            outline: none;
        }

        .btn {
            display: inline-block;
            padding: 12px 20px;
            font-size: 16px;
            border-radius: 6px;
            cursor: pointer;
            text-align: center;
            text-decoration: none;
            border: none;
            transition: background-color 0.3s ease-in-out;
        }

        .btn-success {
            background-color: #28a745;
            color: #fff;
        }

        .btn-success:hover {
            background-color: #218838;
        }

        .btn-secondary {
            background-color: #6c757d;
            color: #fff;
        }

        .btn-secondary:hover {
            background-color: #5a6268;
        }

        .img-thumbnail {
            border-radius: 50%;
            object-fit: cover;
            margin-top: 10px;
        }

        .error-message {
            color: red;
            font-weight: bold;
            margin-bottom: 15px;
            display: block;
        }

        /* Responsive adjustments */
        @media (max-width: 768px) {
            .container {
                padding: 10px;
            }

            .btn {
                width: 100%;
                margin-bottom: 10px;
            }
        }
    </style>

    <!-- ✅ FORM START -->
    <div class="container">
        <div class="card shadow-sm">
            <div class="card-header">
                Edit Employee
            </div>
            <div class="card-body">

                <asp:Label ID="lblMessage" runat="server" CssClass="error-message" />

                <!-- Full Name -->
                <div class="form-group">
                    <label for="txtFullName">Full Name:</label>
                    <asp:TextBox ID="txtFullName" runat="server" CssClass="form-control" placeholder="Enter full name" required></asp:TextBox>
                </div>

                <!-- Email -->
                <div class="form-group">
                    <label for="txtEmail">Email:</label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" placeholder="Enter email address" required></asp:TextBox>
                </div>

                <!-- Phone -->
                <div class="form-group">
                    <label for="txtPhone">Phone Number:</label>
                    <asp:TextBox ID="txtPhone" runat="server" CssClass="form-control"
                        placeholder="Enter 11-digit Phone Number" required 
                        oninput="this.value = this.value.replace(/[^0-9]/g, '').slice(0, 11)">
                    </asp:TextBox>
                </div>

                <!-- Position -->
                <div class="form-group">
                    <label for="ddlPosition">Position:</label>
                    <asp:DropDownList ID="ddlPosition" runat="server" CssClass="form-control">
                        <asp:ListItem Text="Select Position" Value="" />
                        <asp:ListItem Text="IT" Value="IT" />
                        <asp:ListItem Text="Technician" Value="Technician" />
                        <asp:ListItem Text="Inspector" Value="Inspector" />
                    </asp:DropDownList>
                </div>

                <!-- Status -->
                <div class="form-group">
                    <label for="ddlStatus">Status:</label>
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-control">
                        <asp:ListItem Text="Available" Value="Available" />
                        <asp:ListItem Text="Unavailable" Value="Unavailable" />
                        <asp:ListItem Text="Resigned" Value="Resigned" />
                    </asp:DropDownList>
                </div>

                <!-- Profile Picture -->
                <div class="form-group">
                    <label for="fuProfilePicture">Profile Picture (JPG, JPEG, PNG only):</label>
                    <asp:FileUpload ID="fuProfilePicture" runat="server" accept="image/*" onchange="previewImage();" />
                </div>

                <!-- Preview Image -->
                <div class="form-group text-center">
                    <asp:Image ID="imgProfilePreview" runat="server" CssClass="img-thumbnail" Width="100" Height="100" Visible="false" />
                </div>

                <!-- Buttons -->
                <div class="form-group text-center">
                    <asp:Button ID="btnSave" runat="server" Text="Save Changes" CssClass="btn btn-success"
                        OnClientClick="return confirm('Are you sure you want to save these changes?');" 
                        OnClick="btnSave_Click" />

                    <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-secondary"
                        OnClientClick="window.location.href='AllEmployee    .aspx'; return false;" />
                </div>

            </div>
        </div>
    </div>

    <!-- ✅ JAVASCRIPT -->
    <script>
        function previewImage() {
            var fileInput = document.getElementById('<%= fuProfilePicture.ClientID %>');
            var imgPreview = document.getElementById('<%= imgProfilePreview.ClientID %>');

            if (fileInput.files.length > 0) {
                var file = fileInput.files[0];
                var reader = new FileReader();

                reader.onload = function (e) {
                    imgPreview.src = e.target.result;
                    imgPreview.style.display = "block";
                };
                reader.readAsDataURL(file);
            }
        }
    </script>

</asp:Content>

