<%@ Page Title="My Profile" Language="C#" MasterPageFile="~/Client.master" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="RRCManagementSystem.Profile" %>

<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" />
    <style>
        body{background:linear-gradient(to right,#eef2f3,#8e9eab);font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif}
        .profile-card{background:#fff;padding:40px;border-radius:16px;box-shadow:0 12px 24px rgba(0,0,0,.1);max-width:650px;margin:60px auto}
        .profile-pic-container{text-align:center;margin-bottom:20px}
        .profile-pic{width:150px;height:150px;object-fit:cover;border-radius:50%;border:4px solid #007bff;box-shadow:0 4px 12px rgba(0,0,0,.1)}
        .avatar-wrap{position:relative;display:inline-block}
        .edit-badge{position:absolute;right:6px;bottom:6px;width:44px;height:44px;border-radius:50%;border:none;background:#fff;box-shadow:0 4px 10px rgba(0,0,0,.15);display:flex;align-items:center;justify-content:center;cursor:pointer}
        .edit-badge i{font-size:18px;color:#007bff}
        .edit-badge:hover{background:#007bff}.edit-badge:hover i{color:#fff}

        .profile-field{margin-bottom:20px}
        .profile-label{display:block;font-weight:600;font-size:14px;color:#555;margin-bottom:8px}
        .profile-value{display:block;font-size:15px;color:#333;padding:12px 15px;background:#f9f9f9;border-radius:8px;border:1px solid #e0e0e0}
        .form-control{width:100%;height:45px;padding:10px 15px;font-size:15px;border-radius:8px;border:1px solid #ccc}
        .form-control.ro{background:#f5f5f5;border-color:#e3e3e3;color:#666;cursor:not-allowed}
        .form-control.ro:focus{outline:none;box-shadow:none}

        .btn-primary{display:inline-block;padding:12px 30px;font-size:16px;border-radius:50px;cursor:pointer;margin:5px;border:none;background:linear-gradient(to right,#007bff,#0056b3);color:#fff}
        .btn-secondary{display:inline-block;padding:12px 30px;font-size:16px;border-radius:50px;cursor:pointer;margin:5px;border:none;background:#6c757d;color:#fff}
        .text-center{text-align:center;margin-top:20px}
        .text-danger{color:#dc3545;text-align:center}
        .sr-only{display:none}
    </style>
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <div class="profile-card">
        <h3><i class="fas fa-user-circle"></i> My Profile</h3>
        <asp:Label ID="lblMessage" runat="server" CssClass="text-danger"></asp:Label>

        <!-- Keep view panel hidden so code-behind can still set its labels -->
        <asp:Panel ID="pnlViewMode" runat="server" Visible="false" CssClass="sr-only">
            <div class="profile-field"><span class="profile-label">Email</span><asp:Label ID="lblEmail" runat="server" CssClass="profile-value" /></div>
            <div class="profile-field"><span class="profile-label">Contact Number</span><asp:Label ID="lblContactNumber" runat="server" CssClass="profile-value" /></div>
            <div class="profile-field"><span class="profile-label">Street and Unit</span><asp:Label ID="lblStreetAndUnit" runat="server" CssClass="profile-value" /></div>
            <div class="profile-field"><span class="profile-label">Barangay</span><asp:Label ID="lblBarangay" runat="server" CssClass="profile-value" /></div>
            <div class="profile-field"><span class="profile-label">City</span><asp:Label ID="lblCity" runat="server" CssClass="profile-value" /></div>
            <div class="profile-field"><span class="profile-label">Region</span><asp:Label ID="lblRegion" runat="server" CssClass="profile-value" /></div>
            <div class="profile-field"><span class="profile-label">Country</span><asp:Label ID="lblCountry" runat="server" CssClass="profile-value" /></div>
        </asp:Panel>

        <!-- Single visible layout (read-only textboxes + change photo) -->
        <asp:Panel ID="pnlEditMode" runat="server" Visible="true">
            <div class="profile-pic-container">
                <div class="avatar-wrap">
                    <asp:Image ID="imgProfilePic" runat="server" CssClass="profile-pic" ImageUrl="~/Uploads/default-profile.png" />
                    <button type="button" class="edit-badge" onclick="openPicker()">
                        <i class="fas fa-pencil-alt"></i>
                    </button>
                </div>
                <!-- Hidden ASP.NET FileUpload; we click it from JS -->
               <asp:FileUpload ID="fuProfilePic" runat="server" Style="display:none;" accept=".jpg,.jpeg,.png" />
                <div><small>Click the pencil to change photo</small></div>
            </div>

            <div class="profile-field">
                <label class="profile-label">First Name</label>
                <asp:TextBox ID="txtFirstName" runat="server" CssClass="form-control ro" ReadOnly="true" />
            </div>
            <div class="profile-field">
                <label class="profile-label">Middle Name</label>
                <asp:TextBox ID="txtMiddleName" runat="server" CssClass="form-control ro" ReadOnly="true" />
            </div>
            <div class="profile-field">
                <label class="profile-label">Last Name</label>
                <asp:TextBox ID="txtLastName" runat="server" CssClass="form-control ro" ReadOnly="true" />
            </div>

            <div class="profile-field">
                <label class="profile-label">Full Name</label>
                <asp:TextBox ID="txtName" runat="server" CssClass="form-control ro" ReadOnly="true" />
            </div>
            <div class="profile-field">
                <label class="profile-label">Email</label>
                <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control ro" ReadOnly="true" />
            </div>
            <div class="profile-field">
                <label class="profile-label">Contact Number</label>
                <asp:TextBox ID="txtContactNumber" runat="server" CssClass="form-control ro" ReadOnly="true" />
            </div>
            <div class="profile-field">
                <label class="profile-label">Street and Unit</label>
                <asp:TextBox ID="txtStreetAndUnit" runat="server" CssClass="form-control ro" ReadOnly="true" />
            </div>
            <div class="profile-field">
                <label class="profile-label">Barangay</label>
                <asp:TextBox ID="txtBarangay" runat="server" CssClass="form-control ro" ReadOnly="true" />
            </div>
            <div class="profile-field">
                <label class="profile-label">City</label>
                <asp:TextBox ID="txtCity" runat="server" CssClass="form-control ro" ReadOnly="true" />
            </div>
            <div class="profile-field">
                <label class="profile-label">Region</label>
                <asp:TextBox ID="txtRegion" runat="server" CssClass="form-control ro" ReadOnly="true" />
            </div>
            <div class="profile-field">
                <label class="profile-label">Country</label>
                <asp:TextBox ID="txtCountry" runat="server" CssClass="form-control ro" ReadOnly="true" />
            </div>

            <div class="text-center">
                <asp:Button ID="btnSaveProfile" runat="server" CssClass="btn-primary" Text="Save Photo" OnClick="btnSaveProfile_Click" />
                <asp:Button ID="btnCancelEdit" runat="server" CssClass="btn-secondary" Text="Cancel" OnClick="btnCancelEdit_Click" Visible="false" />
            </div>
        </asp:Panel>
    </div>

    <script type="text/javascript">
        function openPicker() {
            document.getElementById('<%= fuProfilePic.ClientID %>').click();
        }
        // preview selected image on the <asp:Image>
        document.addEventListener('change', function (e) {
            if (e.target && e.target.id === '<%= fuProfilePic.ClientID %>') {
                const file = e.target.files && e.target.files[0];
                if (!file) return;
                const reader = new FileReader();
                reader.onload = function () {
                    document.getElementById('<%= imgProfilePic.ClientID %>').src = reader.result;
                };
                reader.readAsDataURL(file);
            }
        });
    </script>
</asp:Content>
