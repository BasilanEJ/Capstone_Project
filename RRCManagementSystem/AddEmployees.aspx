<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AddEmployees.aspx.cs" Inherits="RRCManagementSystem.AddEmployees" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- SweetAlert2 CDN -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        body {
            font-family: Arial, sans-serif;
            background-color: #f3f4f6;
            margin: 0;
            padding: 0;
        }

        .container {
            max-width: 600px;
            margin: 40px auto;
            padding: 20px;
        }

        .card {
            background-color: #ffffff;
            border-radius: 10px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
            overflow: hidden;
            padding: 20px;
        }

        .card-header {
            padding-bottom: 10px;
            border-bottom: 1px solid #eee;
            margin-bottom: 20px;
        }

        .page-title {
            font-size: 24px;
            color: #333;
            text-align: center;
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
            padding: 12px;
            border: 1px solid #ccc;
            border-radius: 6px;
            font-size: 14px;
            transition: border-color 0.3s ease;
        }

        .form-control:focus {
            border-color: #007bff;
            outline: none;
        }

        .error-message {
            color: #dc3545;
            font-size: 14px;
            margin-bottom: 10px;
            text-align: center;
        }

        .btn {
            display: inline-block;
            padding: 12px 20px;
            background-color: #007bff;
            color: #fff;
            font-size: 16px;
            border: none;
            border-radius: 6px;
            cursor: pointer;
            width: 100%;
            transition: background-color 0.3s ease;
        }

        .btn:hover {
            background-color: #0056b3;
        }

        .text-center {
            text-align: center;
        }

        #imagePreview {
            display: none;
            width: 120px;
            height: 120px;
            margin: 10px auto;
            object-fit: cover;
            border-radius: 50%;
            border: 2px solid #ddd;
        }

        @media (max-width: 768px) {
            .container {
                padding: 10px;
            }

            .page-title {
                font-size: 20px;
            }

            .btn {
                font-size: 14px;
            }
        }
    </style>

    <div class="container">
        <div class="card shadow-sm">
            <div class="card-header">
                <h2 class="page-title">Add New Employee</h2>
            </div>

            <div class="card-body">
                <asp:Label ID="lblMessage" runat="server" CssClass="error-message"></asp:Label>

             <div class="form-group">
    <label for="txtLastName">Last Name:</label>
    <asp:TextBox ID="txtLastName" runat="server" CssClass="form-control" placeholder="Enter last name" required></asp:TextBox>
</div>

<div class="form-group">
    <label for="txtFirstName">First Name:</label>
    <asp:TextBox ID="txtFirstName" runat="server" CssClass="form-control" placeholder="Enter first name" required></asp:TextBox>
</div>

<div class="form-group">
    <label for="txtMiddleName">Middle Name <span class="text-muted">(optional)</span>:</label>
    <asp:TextBox ID="txtMiddleName" runat="server" CssClass="form-control" placeholder="Enter middle name (optional)"></asp:TextBox>
</div>


                <div class="form-group">
                    <label for="txtEmail">Email:</label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" placeholder="Enter email address" required></asp:TextBox>
                </div>

                <div class="form-group">
                    <label for="txtPhone">Phone Number</label>
                    <asp:TextBox ID="txtPhone" runat="server" CssClass="form-control"
                        placeholder="Enter 11-digit Phone Number" required
                        oninput="this.value = this.value.replace(/[^0-9]/g, '').slice(0, 11)">
                    </asp:TextBox>
                    <small id="phoneError" style="color: red; display: none;">Phone number must start with 09 and be 11 digits long.</small>
                </div>

                <div class="form-group">
                    <label for="ddlPosition">Position:</label>
                    <asp:DropDownList ID="ddlPosition" runat="server" CssClass="form-control">
                        <asp:ListItem Text="Select Position" Value="" />
                        <asp:ListItem Text="IT" Value="IT" />
                        <asp:ListItem Text="Technician" Value="Technician" />
                        <asp:ListItem Text="Inspector" Value="Inspector" />
                    </asp:DropDownList>
                </div>

                <div class="form-group">
                    <label for="fuProfilePicture">Profile Picture (JPG, JPEG, PNG only):</label>
                    <asp:FileUpload ID="fuProfilePicture" runat="server" accept="image/*" onchange="validateFile(); previewImage(event);" />
                    <img id="imagePreview" alt="Profile Preview" />
                </div>

                <!-- SweetAlert Confirm Button -->
                <div class="form-group text-center">
               <asp:Button ID="btnSubmit" runat="server" Text="Add Employee"
    CssClass="btn"
    OnClick="btnSubmit_Click"
    OnClientClick="return showConfirm(this);" 
    UseSubmitBehavior="false" />



                </div>
            </div>
        </div>
    </div>

    <script>
        // Profile Picture Validation
        function validateFile() {
            var fileInput = document.getElementById('<%= fuProfilePicture.ClientID %>');
            var filePath = fileInput.value;
            var allowedExtensions = /(\.jpg|\.jpeg|\.png)$/i;

            if (!allowedExtensions.exec(filePath)) {
                Swal.fire({
                    icon: 'error',
                    title: 'Invalid File Type',
                    text: 'Only JPG, JPEG, and PNG files are allowed.'
                });
                fileInput.value = '';
                return false;
            }
        }

        // Image Preview
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

        // SweetAlert Confirmation before submit
        function showConfirm(button) {
            if (window.event) {
                window.event.preventDefault();
            }

            Swal.fire({
                title: 'Add Employee?',
                text: 'Are you sure you want to add this employee?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#007bff',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes, add it!'
            }).then((result) => {
                if (result.isConfirmed) {
                    button.disabled = true;
                    // ✅ Explicitly trigger the correct ASP.NET postback
                    __doPostBack('<%= btnSubmit.UniqueID %>', '');
        }
    });

            return false;
        }


    // Attach the event properly on page load
    document.addEventListener("DOMContentLoaded", function () {
        var btn = document.getElementById('<%= btnSubmit.ClientID %>');
        btn.addEventListener('click', showConfirm);
    });


        // Phone Number Validation
        document.addEventListener("DOMContentLoaded", function () {
            var phoneInput = document.getElementById('<%= txtPhone.ClientID %>');
            var errorLabel = document.getElementById("phoneError");

            phoneInput.addEventListener("input", function () {
                var phonePattern = /^09\d{9}$/;

                if (!phonePattern.test(phoneInput.value)) {
                    errorLabel.style.display = "block";
                } else {
                    errorLabel.style.display = "none";
                }
            });
        });
    </script>

</asp:Content>
