<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="EmployeeStatus.aspx.cs" Inherits="RRCManagementSystem.EmployeeStatus" %>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <!-- ✅ INTERNAL CSS STYLES -->
    <style>
        body {
            font-family: Arial, sans-serif;
            background-color: #f8f9fa;
            margin: 0;
            padding: 0;
        }

        .container {
            max-width: 1100px;
            margin: 40px auto;
            padding: 20px;
        }

        .card {
            background-color: #fff;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
            overflow: hidden;
            transition: all 0.3s ease;
        }

        .card-header {
            background-color: #007bff;
            color: #fff;
            padding: 16px 20px;
            text-align: center;
            font-size: 20px;
            font-weight: bold;
        }

        .card-body {
            padding: 30px;
        }

        .form-group {
            margin-bottom: 20px;
        }

        label {
            font-weight: bold;
            display: block;
            margin-bottom: 8px;
            color: #333;
        }

        .form-control {
            width: 100%;
            padding: 12px 15px;
            border: 1px solid #ced4da;
            border-radius: 4px;
            font-size: 14px;
            box-sizing: border-box;
            transition: border-color 0.3s ease-in-out;
        }

        .form-control:focus {
            border-color: #007bff;
            outline: none;
        }

        /* Table Styles */
        .table {
            width: 100%;
            margin-bottom: 1rem;
            color: #212529;
            border-collapse: collapse;
        }

        .table th,
        .table td {
            padding: 12px;
            vertical-align: middle;
            border-top: 1px solid #dee2e6;
            text-align: center;
        }

        .table thead th {
            background-color: #007bff;
            color: #fff;
            border-color: #007bff;
        }

        .table-striped tbody tr:nth-of-type(odd) {
            background-color: rgba(0, 0, 0, 0.05);
        }

        .table-bordered {
            border: 1px solid #dee2e6;
        }

        .table-bordered th,
        .table-bordered td {
            border: 1px solid #dee2e6;
        }

        /* Responsive adjustments */
        @media (max-width: 768px) {
            .container {
                padding: 10px;
            }

            .card-body {
                padding: 15px;
            }

            .form-control {
                padding: 10px;
            }

            .table th,
            .table td {
                padding: 8px;
            }
        }
    </style>

    <!-- ✅ PAGE CONTENT -->
    <div class="container">
        <div class="card shadow-sm">
            <div class="card-header">
                Employee Status
            </div>

            <div class="card-body">
                <!-- Filter Dropdown -->
                <div class="form-group">
                    <label for="ddlStatus">Filter by Status:</label>
                    <asp:DropDownList ID="ddlStatus" runat="server" AutoPostBack="true" CssClass="form-control" OnSelectedIndexChanged="ddlStatus_SelectedIndexChanged">
                        <asp:ListItem Text="All" Value="" Selected="True"></asp:ListItem>
                        <asp:ListItem Text="Available" Value="Available"></asp:ListItem>
                        <asp:ListItem Text="Unavailable" Value="Unavailable"></asp:ListItem>
                        <asp:ListItem Text="Resigned" Value="Resigned"></asp:ListItem>
                    </asp:DropDownList>
                </div>

                <!-- Employee Status Grid -->
                <div class="table-responsive">
                    <asp:GridView ID="gvEmployees" runat="server" CssClass="table table-bordered table-striped"
                        AutoGenerateColumns="False" AllowPaging="True" PageSize="10"
                        OnPageIndexChanging="gvEmployees_PageIndexChanging">
                        <Columns>
                            <asp:BoundField DataField="EmployeeID" HeaderText="ID" ReadOnly="True">
                                <ItemStyle Width="50px" />
                            </asp:BoundField>
                            <asp:BoundField DataField="FullName" HeaderText="Full Name" ReadOnly="True">
                                <ItemStyle Width="200px" />
                            </asp:BoundField>
                            <asp:BoundField DataField="Email" HeaderText="Email" ReadOnly="True">
                                <ItemStyle Width="250px" />
                            </asp:BoundField>
                            <asp:BoundField DataField="Phone" HeaderText="Phone" ReadOnly="True">
                                <ItemStyle Width="150px" />
                            </asp:BoundField>
                            <asp:BoundField DataField="Position" HeaderText="Position" ReadOnly="True">
                                <ItemStyle Width="200px" />
                            </asp:BoundField>
                            <asp:BoundField DataField="Status" HeaderText="Status" ReadOnly="True">
                                <ItemStyle Width="150px" />
                            </asp:BoundField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>

</asp:Content>