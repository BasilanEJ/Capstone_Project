<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="EditServices.aspx.cs" Inherits="RRCManagementSystem.EditServices" %>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

        <style>
        /* Main Content Wrapper */
        .main-content {
            padding: 20px;
            background-color: #f4f4f4;
            min-height: calc(100vh - 100px);
        }

        /* Card Container */
        .card {
            background-color: #fff;
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
            overflow: hidden;
            max-width: 600px;
            margin: 0 auto;
        }

        /* Card Header */
        .card-header {
            background-color: #004085;
            color: #fff;
            padding: 15px 20px;
            font-size: 18px;
            font-weight: bold;
        }

        /* Card Body */
        .card-body {
            padding: 20px;
        }

        /* Form Group */
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

        /* Alert/Message Styling */
        .alert-message {
            display: block;
            margin-bottom: 15px;
            padding: 10px 15px;
            background-color: #d1ecf1;
            color: #0c5460;
            border: 1px solid #bee5eb;
            border-radius: 5px;
            font-size: 14px;
        }

        /* Buttons */
        .btn-submit {
            background-color: #004085;
            color: #fff;
            border: none;
            padding: 10px 20px;
            border-radius: 4px;
            font-size: 14px;
            cursor: pointer;
            transition: background-color 0.3s ease;
            margin-right: 10px;
        }

        .btn-submit:hover {
            background-color: #003366;
        }

        .btn-cancel {
            background-color: #6c757d;
            color: #fff;
            border: none;
            padding: 10px 20px;
            border-radius: 4px;
            font-size: 14px;
            cursor: pointer;
            transition: background-color 0.3s ease;
        }

        .btn-cancel:hover {
            background-color: #5a6268;
        }

        /* Responsive adjustments */
        @media (max-width: 768px) {
            .card {
                margin: 20px;
            }

            .btn-submit,
            .btn-cancel {
                width: 100%;
                margin-right: 0;
                margin-bottom: 10px;
            }
        }
    </style>
    
<!-- ✅ Main Content -->
<div class="main-content">
    <div class="card">
        <div class="card-header">Edit Service</div>
        <div class="card-body">

            <asp:Label ID="lblMessage" runat="server" CssClass="alert-message" />

            
            <div class="form-group">
                <label for="txtName">Service Name *</label>
                <asp:TextBox ID="txtName" runat="server" CssClass="form-control" />
            </div>

          
            <div class="form-group">
                <label for="txtDescription">Description</label>
                <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" />
            </div>

            <div class="form-group">
                <label for="txtPrice100">Price for 100 SQM (₱)</label>
                <asp:TextBox ID="txtPrice100" runat="server" CssClass="form-control" TextMode="Number" />
            </div>

            <div class="form-group">
                <label for="txtPrice200">Price for 200 SQM (₱)</label>
                <asp:TextBox ID="txtPrice200" runat="server" CssClass="form-control" TextMode="Number" />
            </div>

            <div class="form-group">
                <label for="txtPriceAbove200">Price for 200 SQM and above (₱)</label>
                <asp:TextBox ID="txtPriceAbove200" runat="server" CssClass="form-control" TextMode="Number" />
            </div>

        
            <asp:Button ID="Button1" runat="server" Text="Update Service" CssClass="btn btn-primary"
                OnClick="btnUpdate_Click" OnClientClick="return confirm('Are you sure you want to update this service?');" />

            <asp:Button ID="Button2" runat="server" Text="Cancel" CssClass="btn btn-secondary"
                PostBackUrl="~/ADMIN/ViewServices.aspx" />
        </div>
    </div>
</div>


                <asp:Button ID="btnUpdate" runat="server" Text="Update Service"
                    CssClass="btn-submit" OnClick="btnUpdate_Click" />

                <asp:Button ID="btnCancel" runat="server" Text="Cancel"
                    CssClass="btn-cancel" PostBackUrl="ViewServices.aspx" />
            </div>
        </div>
    </div>

</asp:Content>
