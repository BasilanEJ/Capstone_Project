<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="CreateCustomerAccount.aspx.cs" Inherits="RRCManagementSystem.CreateCustomerAccount" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f5f7fa;
        }

        .form-container {
            background-color: #ffffff;
            padding: 30px;
            max-width: 600px;
            margin: 40px auto;
            border-radius: 12px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
        }

        .form-container h2 {
            text-align: center;
            margin-bottom: 20px;
            color: #004085;
        }

        .note {
            font-size: 14px;
            color: #6c757d;
            text-align: center;
            margin-bottom: 25px;
        }

        .form-group {
            margin-bottom: 20px;
        }

        .form-group label {
            display: block;
            margin-bottom: 6px;
            font-weight: bold;
            color: #333333;
        }

        .form-control {
            width: 100%;
            padding: 10px 12px;
            border: 1px solid #ced4da;
            border-radius: 6px;
            font-size: 14px;
            box-sizing: border-box;
            transition: border-color 0.3s;
        }

        .form-control:focus {
            border-color: #007bff;
            outline: none;
        }

        .btn-primary {
            background-color: #007bff;
            border: none;
            padding: 12px 20px;
            font-size: 16px;
            color: #ffffff;
            border-radius: 6px;
            cursor: pointer;
            width: 100%;
            transition: background-color 0.3s;
        }

        .btn-primary:hover {
            background-color: #0056b3;
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true" />

    <div class="form-container">
        <h2>Create Customer Account</h2>
        <div class="note">An email will be sent to the client to set their password.</div>

        <div class="form-group">
            <label>Name *</label>
            <asp:TextBox ID="txtName" runat="server" CssClass="form-control" />
        </div>

        <div class="form-group">
            <label>Email *</label>
            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" />
        </div>

        <div class="form-group">
            <label>Contact Number *</label>
            <asp:TextBox ID="txtContact" runat="server" CssClass="form-control" />
        </div>

        <div class="form-group">
            <label>Country *</label>
            <asp:TextBox ID="txtCountry" runat="server" CssClass="form-control" Text="Philippines" />
        </div>

        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <div class="form-group">
                    <label>Region *</label>
                    <asp:DropDownList ID="ddlRegion" runat="server" AutoPostBack="true" CssClass="form-control" OnSelectedIndexChanged="ddlRegion_SelectedIndexChanged" />
                </div>

                <div class="form-group">
                    <label>City *</label>
                    <asp:DropDownList ID="ddlCity" runat="server" CssClass="form-control" />
                </div>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="ddlRegion" EventName="SelectedIndexChanged" />
            </Triggers>
        </asp:UpdatePanel>

        <div class="form-group">
            <label>Barangay *</label>
            <asp:TextBox ID="txtBarangay" runat="server" CssClass="form-control" />
        </div>

        <div class="form-group">
            <label>Street & Unit *</label>
            <asp:TextBox ID="txtStreet" runat="server" CssClass="form-control" />
        </div>

        <div class="form-group">
            <label>Landmark</label>
            <asp:TextBox ID="txtLandmark" runat="server" CssClass="form-control" />
        </div>

        <asp:Button ID="btnCreate" runat="server" Text="Create Account" CssClass="btn btn-primary" OnClick="btnCreate_Click" />
    </div>

    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <asp:Literal ID="ltScript" runat="server" />
</asp:Content>
