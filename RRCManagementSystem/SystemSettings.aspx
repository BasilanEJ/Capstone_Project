<%@ Page Title="" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="SystemSettings.aspx.cs" Inherits="RRCManagementSystem.SystemSettings" %>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .settings-container {
            width: 60%;
            margin: 30px auto;
            background: #fff;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
        }

        .settings-container h2 {
            text-align: center;
            margin-bottom: 20px;
        }

        .form-group {
            margin-bottom: 15px;
        }

        label {
            display: block;
            margin-bottom: 5px;
            font-weight: bold;
        }

        .form-control {
            width: 100%;
            padding: 8px;
            border-radius: 4px;
            border: 1px solid #ccc;
        }

        .btn-submit {
            display: block;
            width: 100%;
            padding: 10px;
            background-color: #007bff;
            color: white;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            font-size: 16px;
        }

        .btn-submit:hover {
            background-color: #0056b3;
        }

        .alert-message {
            color: red;
            text-align: center;
        }

        .success-message {
            color: green;
            text-align: center;
        }
    </style>

    <div class="settings-container">
        <h2>System Settings</h2>

        <asp:Label ID="lblMessage" runat="server" CssClass="alert-message"></asp:Label>
        <asp:Label ID="lblSuccess" runat="server" CssClass="success-message"></asp:Label>

        <div class="form-group">
            <label for="txtSystemName">System Name</label>
            <asp:TextBox ID="txtSystemName" runat="server" CssClass="form-control" />
        </div>

        <div class="form-group">
            <label for="txtSenderEmail">Sender Email (SMTP)</label>
            <asp:TextBox ID="txtSenderEmail" runat="server" CssClass="form-control" />
        </div>

        <div class="form-group">
            <label for="ddlMaintenanceMode">Maintenance Mode</label>
            <asp:DropDownList ID="ddlMaintenanceMode" runat="server" CssClass="form-control">
                <asp:ListItem Text="Off" Value="False"></asp:ListItem>
                <asp:ListItem Text="On" Value="True"></asp:ListItem>
            </asp:DropDownList>
        </div>

        <div class="form-group">
            <label for="txtLogRetentionDays">Audit Log Retention (Days)</label>
            <asp:TextBox ID="txtLogRetentionDays" runat="server" CssClass="form-control" TextMode="Number" />
        </div>

        <asp:Button ID="btnSave" runat="server" Text="Save Settings" CssClass="btn-submit" OnClick="btnSave_Click" />
    </div>
</asp:Content>


