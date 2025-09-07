    <%@ Page Title="" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="SuperAdminProfile.aspx.cs" Inherits="RRCManagementSystem.SuperAdminProfile" %>


    <asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
        <style>
            .profile-container {
                width: 50%;
                margin: 30px auto;
                background-color: #fff;
                padding: 20px;
                border-radius: 6px;
                box-shadow: 0 0 10px rgba(0,0,0,0.1);
            }

            .profile-container h2 {
                text-align: center;
                margin-bottom: 20px;
            }

            .form-group {
                margin-bottom: 15px;
            }

            label {
                display: block;
                font-weight: bold;
                margin-bottom: 5px;
            }

            .form-control {
                width: 100%;
                padding: 8px;
                font-size: 14px;
                border-radius: 4px;
                border: 1px solid #ccc;
            }

            .btn-save {
                background-color: #007bff;
                color: #fff;
                border: none;
                padding: 10px 20px;
                cursor: pointer;
                border-radius: 4px;
            }

            .btn-save:hover {
                background-color: #0056b3;
            }

            .alert {
                margin-top: 15px;
                text-align: center;
                color: red;
            }

            .success {
                color: green;
            }
        </style>

        <div class="profile-container">
            <h2>System Admin Profile</h2>

            <asp:Label ID="lblMessage" runat="server" CssClass="alert"></asp:Label>

            <div class="form-group">
                <label for="txtName">Name</label>
                <asp:TextBox ID="txtName" runat="server" CssClass="form-control" />
            </div>

         <div class="form-group">
    <label for="txtEmail">Email</label>
    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" ReadOnly="true" />
</div>


            <div class="form-group">
                <label for="txtNewPassword">New Password (optional)</label>
                <asp:TextBox ID="txtNewPassword" runat="server" CssClass="form-control" TextMode="Password" />
            </div>

            <asp:Button ID="btnSaveProfile" runat="server" Text="Save Changes" CssClass="btn-save" OnClick="btnSaveProfile_Click" />
        </div>
    </asp:Content>