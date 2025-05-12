<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="EditSupplier.aspx.cs" Inherits="RRCManagementSystem.EditSupplier" %>


<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        /* Main content wrapper */
        .main-content {
            padding: 20px;
            background-color: #f4f4f4;
            min-height: calc(100vh - 100px);
        }

        /* Card container for edit form */
        .card-container {
            background-color: #ffffff;
            padding: 25px;
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
            max-width: 700px;
            margin: 0 auto;
        }

        .card-container h2 {
            color: #004085;
            margin-bottom: 20px;
            font-size: 24px;
        }

        /* Labels */
        #pnlEditSupplier label {
            display: block;
            font-weight: 600;
            margin-bottom: 8px;
            color: #333333;
            margin-top: 15px;
        }

        /* Form control styling */
        .form-control {
            width: 100%;
            padding: 10px 12px;
            font-size: 14px;
            border: 1px solid #ced4da;
            border-radius: 4px;
            transition: border-color 0.3s, box-shadow 0.3s;
        }

        .form-control:focus {
            border-color: #004085;
            box-shadow: 0 0 5px rgba(0, 64, 133, 0.3);
            outline: none;
        }

        /* Buttons */
        .btn {
            display: inline-block;
            padding: 10px 20px;
            margin-top: 20px;
            font-size: 14px;
            border-radius: 4px;
            cursor: pointer;
            transition: background-color 0.3s ease;
            text-decoration: none;
            border: none;
        }

        .btn-primary {
            background-color: #004085;
            color: #ffffff;
        }

        .btn-primary:hover {
            background-color: #003366;
        }

        .btn-secondary {
            background-color: #6c757d;
            color: #ffffff;
        }

        .btn-secondary:hover {
            background-color: #5a6268;
        }

        /* Message label */
        #lblMessage {
            margin-bottom: 15px;
            display: block;
            font-size: 14px;
            color: #dc3545;
        }

        /* Responsive adjustments */
        @media (max-width: 768px) {
            .card-container {
                padding: 15px;
            }

            .btn {
                width: 100%;
                margin-bottom: 10px;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="main-content">
        <div class="card-container">

            <h2>Edit Supplier</h2>

            <asp:Label ID="lblMessage" runat="server" ForeColor="Red"></asp:Label>

            <asp:Panel ID="pnlEditSupplier" runat="server">

                <!-- Supplier Name -->
                <asp:Label ID="lblName" runat="server" Text="Supplier Name:"></asp:Label>
                <asp:TextBox ID="txtName" runat="server" CssClass="form-control"></asp:TextBox>

                <!-- Company Name -->
                <asp:Label ID="lblCompanyName" runat="server" Text="Company Name:"></asp:Label>
                <asp:TextBox ID="txtCompanyName" runat="server" CssClass="form-control"></asp:TextBox>

                <!-- Business Type -->
                <asp:Label ID="lblBusinessType" runat="server" Text="Business Type:"></asp:Label>
                <asp:DropDownList ID="ddlBusinessType" runat="server" CssClass="form-control">
                    <asp:ListItem Text="Select Business Type" Value="" />
                    <asp:ListItem Text="Equipment" Value="Equipment" />
                    <asp:ListItem Text="Chemicals" Value="Chemicals" />
                </asp:DropDownList>

                <!-- Address -->
                <asp:Label ID="lblAddress" runat="server" Text="Address:"></asp:Label>
                <asp:TextBox ID="txtAddress" runat="server" CssClass="form-control"></asp:TextBox>

                <!-- Contact Number -->
                <asp:Label ID="lblContactNumber" runat="server" Text="Contact Number:"></asp:Label>
                <asp:TextBox ID="txtContactNumber" runat="server" CssClass="form-control"></asp:TextBox>

                <!-- Email -->
                <asp:Label ID="lblEmail" runat="server" Text="Email:"></asp:Label>
                <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control"></asp:TextBox>

                <!-- Status -->
                <asp:Label ID="lblStatus" runat="server" Text="Status:"></asp:Label>
                <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-control">
                    <asp:ListItem Text="Select Status" Value="" />
                    <asp:ListItem Text="Active" Value="Active" />
                    <asp:ListItem Text="Inactive" Value="Inactive" />
                </asp:DropDownList>

                <!-- Update Button with Confirmation -->
                <asp:Button ID="btnUpdate" runat="server"
                    Text="Update Supplier"
                    CssClass="btn btn-primary"
                    OnClientClick="return confirm('Are you sure you want to update this supplier?');"
                    OnClick="btnUpdate_Click" />

                &nbsp;

                <!-- Cancel Button with Confirmation -->
                <asp:Button ID="btnCancel" runat="server"
                    Text="Cancel"
                    CssClass="btn btn-secondary"
                    PostBackUrl="~/ADMIN/ViewSupplier.aspx"
                    OnClientClick="return confirm('Are you sure you want to cancel and go back?');" />

            </asp:Panel>
        </div>
    </div>

</asp:Content>