<%@ Page Title="" Language="C#" MasterPageFile="~/Client.master" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="RRCManagementSystem.Profile" %>


<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        body {
            background: linear-gradient(to right, #eef2f3, #8e9eab);
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }

        .profile-card {
            background-color: #fff;
            padding: 40px;
            border-radius: 16px;
            box-shadow: 0 12px 24px rgba(0, 0, 0, 0.1);
            max-width: 650px;
            margin: 60px auto;
        }

        .profile-pic-container {
            text-align: center;
            margin-bottom: 20px;
        }

        .profile-pic {
            width: 150px;
            height: 150px;
            object-fit: cover;
            border-radius: 50%;
            border: 4px solid #007bff;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
        }

        .custom-file-upload {
            display: inline-block;
            padding: 8px 12px;
            cursor: pointer;
            border-radius: 8px;
            color: #007bff;
            background-color: #f9f9f9;
            font-weight: 600;
            margin-top: 10px;
        }

        .custom-file-upload:hover {
            background-color: #007bff;
            color: #fff;
        }

        .profile-field {
            margin-bottom: 20px;
        }

        .profile-label {
            display: block;
            font-weight: 600;
            font-size: 14px;
            color: #555;
            margin-bottom: 8px;
        }

        .profile-value {
            display: block;
            font-size: 15px;
            color: #333;
            padding: 12px 15px;
            background-color: #f9f9f9;
            border-radius: 8px;
            border: 1px solid #e0e0e0;
        }

        .form-control {
            width: 100%;
            height: 45px;
            padding: 10px 15px;
            font-size: 15px;
            border-radius: 8px;
            border: 1px solid #ccc;
        }

        .btn-primary, .btn-secondary {
            display: inline-block;
            padding: 12px 30px;
            font-size: 16px;
            border-radius: 50px;
            cursor: pointer;
            margin: 5px;
            border: none;
        }

        .btn-primary {
            background: linear-gradient(to right, #007bff, #0056b3);
            color: #fff;
        }

        .btn-secondary {
            background-color: #6c757d;
            color: #fff;
        }

        .text-center {
            text-align: center;
            margin-top: 20px;
        }

        .text-danger {
            color: #dc3545;
            text-align: center;
        }

    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="profile-card">
        <h3><i class="fas fa-user-circle"></i> My Profile</h3>

        <asp:Label ID="lblMessage" runat="server" CssClass="text-danger"></asp:Label>

        <div class="profile-pic-container">
            <asp:Image ID="imgProfilePic" runat="server" CssClass="profile-pic" ImageUrl="~/Uploads/default-profile.png" />
        </div>

        <!-- VIEW MODE -->
        <asp:Panel ID="pnlViewMode" runat="server">
       <div class="profile-field">
    <span class="profile-label">First Name</span>
    <asp:TextBox ID="txtFirstName" runat="server" CssClass="form-control" />
</div>
<div class="profile-field">
    <span class="profile-label">Middle Name</span>
    <asp:TextBox ID="txtMiddleName" runat="server" CssClass="form-control" />
</div>
<div class="profile-field">
    <span class="profile-label">Last Name</span>
    <asp:TextBox ID="txtLastName" runat="server" CssClass="form-control" />
</div>


            <div class="profile-field">
                <span class="profile-label">Email</span>
                <asp:Label ID="lblEmail" runat="server" CssClass="profile-value" />
            </div>

            <div class="profile-field">
                <span class="profile-label">Contact Number</span>
                <asp:Label ID="lblContactNumber" runat="server" CssClass="profile-value" />
            </div>

            <div class="profile-field">
                <span class="profile-label">Street and Unit</span>
                <asp:Label ID="lblStreetAndUnit" runat="server" CssClass="profile-value" />
            </div>

            <div class="profile-field">
                <span class="profile-label">Barangay</span>
                <asp:Label ID="lblBarangay" runat="server" CssClass="profile-value" />
            </div>

            <div class="profile-field">
                <span class="profile-label">City</span>
                <asp:Label ID="lblCity" runat="server" CssClass="profile-value" />
            </div>

            <div class="profile-field">
                <span class="profile-label">Region</span>
                <asp:Label ID="lblRegion" runat="server" CssClass="profile-value" />
            </div>

            <div class="profile-field">
                <span class="profile-label">Country</span>
                <asp:Label ID="lblCountry" runat="server" CssClass="profile-value" />
            </div>

            <div class="text-center">
                <asp:Button ID="btnEditProfile" runat="server" CssClass="btn btn-primary" Text="Edit Profile" OnClick="btnEditProfile_Click" />
            </div>
        </asp:Panel>

        <!-- EDIT MODE -->
        <asp:Panel ID="pnlEditMode" runat="server" Visible="false">
            <div class="profile-pic-container">
                <img id="previewImg" src='<%= imgProfilePic.ImageUrl %>' class="profile-pic" />
                <asp:FileUpload ID="fuProfilePic" runat="server" CssClass="form-control" style="display:none;" />
                <label for="fileUpload" class="custom-file-upload">Choose Image</label>
                <input type="file" id="fileUpload" accept="image/*" onchange="previewImage(event)" />
            </div>

            <div class="profile-field">
                <label class="profile-label">Full Name</label>
                <asp:TextBox ID="txtName" runat="server" CssClass="form-control" />
            </div>

            <div class="profile-field">
                <label class="profile-label">Email</label>
                <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" ReadOnly="true" />
            </div>

            <div class="profile-field">
                <label class="profile-label">Contact Number</label>
                <asp:TextBox ID="txtContactNumber" runat="server" CssClass="form-control" />
            </div>

            <div class="profile-field">
                <label class="profile-label">Street and Unit</label>
                <asp:TextBox ID="txtStreetAndUnit" runat="server" CssClass="form-control" />
            </div>

            <div class="profile-field">
                <label class="profile-label">Barangay</label>
                <asp:TextBox ID="txtBarangay" runat="server" CssClass="form-control" />
            </div>

            <div class="profile-field">
                <label class="profile-label">City</label>
                <asp:TextBox ID="txtCity" runat="server" CssClass="form-control" />
            </div>

            <div class="profile-field">
                <label class="profile-label">Region</label>
                <asp:TextBox ID="txtRegion" runat="server" CssClass="form-control" />
            </div>

            <div class="profile-field">
                <label class="profile-label">Country</label>
                <asp:TextBox ID="txtCountry" runat="server" CssClass="form-control" />
            </div>

            <div class="text-center">
                <asp:Button ID="btnSaveProfile" runat="server" CssClass="btn btn-primary" Text="Save Changes" OnClick="btnSaveProfile_Click" />
                <asp:Button ID="btnCancelEdit" runat="server" CssClass="btn btn-secondary" Text="Cancel" OnClick="btnCancelEdit_Click" />
            </div>
        </asp:Panel>
    </div>

    <script type="text/javascript">
        function previewImage(event) {
            var reader = new FileReader();
            reader.onload = function () {
                var output = document.getElementById('previewImg');
                output.src = reader.result;
            };
            reader.readAsDataURL(event.target.files[0]);

            // Hook the ASP.NET FileUpload to the HTML input
            const aspFileUpload = document.getElementById('<%= fuProfilePic.ClientID %>');
            aspFileUpload.files = event.target.files;
        }
    </script>
</asp:Content>

