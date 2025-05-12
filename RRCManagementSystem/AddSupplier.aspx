<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AddSupplier.aspx.cs" Inherits="RRCManagementSystem.AddSupplier" %>


<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        /* Main container styling */
        .container {
            padding: 20px;
            background-color: #f4f4f4;
            min-height: calc(100vh - 100px);
        }

        /* Card styling */
        .card {
            background-color: #fff;
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
            max-width: 700px;
            margin: 0 auto;
            overflow: hidden;
        }

        .card-header {
            background-color: #004085;
            color: #fff;
            padding: 15px 20px;
            font-size: 18px;
            font-weight: bold;
        }

        .card-body {
            padding: 20px;
        }

        /* Form group styling */
        .form-group {
            margin-bottom: 20px;
        }

        .form-group label {
            display: block;
            font-weight: 600;
            margin-bottom: 8px;
            color: #333;
        }

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

        /* Submit button styling */
        .btn-submit {
            background-color: #004085;
            color: #fff;
            border: none;
            padding: 10px 20px;
            border-radius: 4px;
            font-size: 14px;
            cursor: pointer;
            transition: background-color 0.3s ease;
            margin-top: 10px;
        }

        .btn-submit:hover {
            background-color: #003366;
        }

        /* Alert message */
        .alert-message {
            display: block;
            margin-top: 15px;
            padding: 10px 15px;
            background-color: #d1ecf1;
            color: #0c5460;
            border: 1px solid #bee5eb;
            border-radius: 5px;
            font-size: 14px;
        }

        /* Responsive design */
        @media (max-width: 768px) {
            .card {
                margin: 20px;
            }

            .btn-submit {
                width: 100%;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mt-4">
        <div class="card">
            <div class="card-header">
                <strong>Add New Supplier</strong>
            </div>
            <div class="card-body">
                <!-- Supplier Name -->
                <div class="form-group">
                    <label for="txtName">Supplier Name *</label>
                    <asp:TextBox ID="txtName" runat="server" CssClass="form-control" placeholder="Enter supplier name" required="required" />
                </div>

                <!-- Address -->
                <div class="form-group">
                    <label for="txtAddress">Address *</label>
                    <asp:TextBox ID="txtAddress" runat="server" CssClass="form-control" placeholder="Enter address" required="required" />
                </div>

                <!-- Contact Number -->
                <div class="form-group">
                    <label for="txtContactNumber">Contact Number *</label>
                    <asp:TextBox ID="txtContactNumber" runat="server" CssClass="form-control" placeholder="Enter contact number" required="required" />
                </div>

                <!-- Email -->
                <div class="form-group">
                    <label for="txtEmail">Email</label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" placeholder="Enter email (optional)" TextMode="Email" />
                </div>

                <!-- Company Name -->
                <div class="form-group">
                    <label for="txtCompanyName">Company Name</label>
                    <asp:TextBox ID="txtCompanyName" runat="server" CssClass="form-control" placeholder="Enter company name (optional)" />
                </div>

                <!-- Business Type -->
                <div class="form-group">
                    <label for="ddlBusinessType">Business Type</label>
                    <asp:DropDownList ID="ddlBusinessType" runat="server" CssClass="form-control">
                        <asp:ListItem Text="Select Business Type" Value="" />
                        <asp:ListItem Text="Pest Control Products" Value="Pest Control Products" />
                        <asp:ListItem Text="PPE Materials" Value="Construction Materials" />
                        <asp:ListItem Text="Chemicals" Value="Chemicals" />
                        <asp:ListItem Text="Equipment" Value="Equipment" />
                        <asp:ListItem Text="Others" Value="Others" />
                    </asp:DropDownList>
                </div>

                <!-- Status -->
                <div class="form-group">
                    <label for="ddlStatus">Status</label>
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-control">
                        <asp:ListItem Text="Select Status" Value="" />
                        <asp:ListItem Text="Active" Value="Active" />
                        <asp:ListItem Text="Inactive" Value="Inactive" />
                    </asp:DropDownList>
                </div>

                <!-- Submit Button with simple confirmation prompt -->
                <asp:Button 
                    ID="btnSubmit" 
                    runat="server" 
                    Text="Add Supplier" 
                    CssClass="btn-submit" 
                    OnClientClick="return confirm('Are you sure you want to add this item?');" 
                    OnClick="btnSubmit_Click" />

                <!-- ASP.NET Server Message -->
                <asp:Label ID="lblMessage" runat="server" CssClass="alert-message"></asp:Label>
            </div>
        </div>
    </div>
</asp:Content>
