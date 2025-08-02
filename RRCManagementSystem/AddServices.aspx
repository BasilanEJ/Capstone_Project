<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AddServices.aspx.cs" Inherits="RRCManagementSystem.AddServices" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .main-content {
            padding: 20px;
            background-color: #f4f4f4;
            min-height: calc(100vh - 100px);
        }

        .card {
            background-color: #fff;
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
            overflow: hidden;
            max-width: 600px;
            margin: 0 auto;
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

        .custom-btn {
            display: inline-block;
            padding: 12px 20px;
            font-size: 16px;
            font-weight: 600;
            color: #fff;
            border: none;
            border-radius: 6px;
            cursor: pointer;
            transition: background-color 0.3s ease, box-shadow 0.2s ease;
            margin-left: 10px;
            box-shadow: 0 2px 6px rgba(0, 0, 0, 0.1);
            min-width: 150px;
        }

        .custom-btn:hover {
            box-shadow: 0 4px 10px rgba(0, 0, 0, 0.15);
        }

        .primary-btn {
            background-color: #007bff;
        }

        .primary-btn:hover {
            background-color: #0056b3;
        }

        .secondary-btn {
            background-color: #6c757d;
        }

        .secondary-btn:hover {
            background-color: #5a6268;
        }

        @media (max-width: 576px) {
            .form-group.text-end {
                text-align: center !important;
            }

            .custom-btn {
                display: block;
                width: 100%;
                margin: 10px 0;
            }
        }

        @media (max-width: 768px) {
            .card {
                margin: 20px;
            }

            .btn-group {
                flex-direction: column;
                gap: 10px;
            }

            .btn-submit,
            .btn-cancel {
                width: 100%;
            }
        }
    </style>

    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <script type="text/javascript">
        function confirmAddService(btn) {
            Swal.fire({
                title: 'Are you sure?',
                text: "Do you want to add this service?",
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#004085',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Yes, add it!'
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack(btn.name, '');
                }
            });
            return false;
        }

        function showSuccessAlert() {
            Swal.fire({
                icon: 'success',
                title: 'Success!',
                text: 'Service added successfully.',
                confirmButtonColor: '#004085'
            });
        }
    </script>

    <div class="main-content">
        <div class="card">
            <div class="card-header">
                Add New Service
            </div>
            <div class="card-body">

                <asp:Label ID="lblMessage" runat="server" CssClass="alert-message" Visible="false" />
                <asp:Literal ID="litScript" runat="server" /> <!-- Script injection -->

                <div class="form-group">
                    <label for="txtName">Service Name *</label>
                    <asp:TextBox ID="txtName" runat="server" CssClass="form-control" MaxLength="100" />
                </div>

                <div class="form-group">
                    <label for="ddlServiceType">Service Type *</label>
                    <asp:DropDownList ID="ddlServiceType" runat="server" CssClass="form-control">
                        <asp:ListItem Text="Select Service Type" Value="" />
                        <asp:ListItem Text="Termite Control" Value="Termite Control" />
                        <asp:ListItem Text="General Pest Control" Value="General Pest Control" />
                    </asp:DropDownList>
                </div>

                <div class="form-group">
                    <label for="txtDescription">Description</label>
                    <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" MaxLength="500" />
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
                    <label for="txtPriceAbove200">Price for 200 SQM and Above (₱)</label>
                    <asp:TextBox ID="txtPriceAbove200" runat="server" CssClass="form-control" TextMode="Number" />
                </div>

                <div class="form-group text-end mt-4">
                    <asp:Button ID="Button1" runat="server" Text="➕ Add Service"
                        CssClass="custom-btn primary-btn"
                        OnClick="btnSubmit_Click"
                        UseSubmitBehavior="false"
                        OnClientClick="return confirmAddService(this);" />

                    <asp:Button ID="Button2" runat="server" Text="Cancel"
                        CssClass="custom-btn secondary-btn"
                        PostBackUrl="~/ADMIN/ViewServices.aspx" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>
