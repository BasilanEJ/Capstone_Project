<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AllEmployee.aspx.cs" Inherits="RRCManagementSystem.AllEmployee" %>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        body {
            font-family: Arial, sans-serif;
            background-color: #f4f6f9;
            margin: 0;
            padding: 0;
        }

        .container {
            max-width: 1200px;
            margin: 40px auto;
            padding: 20px;
        }

        .card {
            background-color: #fff;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
            overflow: hidden;
        }

        .card-header {
            background-color: #007bff;
            color: #fff;
            padding: 16px 20px;
            font-size: 20px;
            font-weight: bold;
        }

        .card-body {
            padding: 20px;
        }

        .table-responsive {
            width: 100%;
            overflow-x: auto;
        }

        .custom-table {
            width: 100%;
            border-collapse: collapse;
            background-color: #fff;
        }

        .custom-table th,
        .custom-table td {
            padding: 12px 15px;
            border: 1px solid #dee2e6;
            text-align: center;
            vertical-align: middle;
        }

        .custom-table th {
            background-color: #343a40;
            color: #fff;
            font-weight: 600;
        }

        .custom-table tr:nth-child(even) {
            background-color: #f9f9f9;
        }

        .custom-table tr:hover {
            background-color: #f1f1f1;
        }

        /* Action Buttons */
        .btn {
            display: inline-block;
            padding: 8px 14px;
            font-size: 14px;
            border-radius: 4px;
            cursor: pointer;
            text-align: center;
            text-decoration: none;
            transition: background-color 0.3s ease;
            border: none;
        }

        .btn-primary {
            background-color: #007bff;
            color: #fff;
        }

        .btn-primary:hover {
            background-color: #0056b3;
        }

        .btn-danger {
            background-color: #dc3545;
            color: #fff;
        }

        .btn-danger:hover {
            background-color: #a71d2a;
        }

        /* Profile Picture Image */
        .custom-table img {
            width: 50px;
            height: 50px;
            border-radius: 50%;
            object-fit: cover;
        }

        /* Responsive adjustments */
        @media (max-width: 768px) {
            .container {
                padding: 10px;
            }

            .card-header {
                font-size: 18px;
                padding: 12px 16px;
            }

            .btn {
                font-size: 12px;
                padding: 6px 10px;
            }

            .custom-table th,
            .custom-table td {
                padding: 8px 10px;
            }

            .custom-table img {
                width: 40px;
                height: 40px;
            }
        }
    </style>

    <div class="container">
        <div class="card">
            <div class="card-header">
                Employee List
            </div>
            <div class="card-body">
                <div class="table-responsive">

                    <asp:GridView ID="gvEmployees" runat="server" CssClass="custom-table"
                        AutoGenerateColumns="False" DataKeyNames="EmployeeID" OnRowCommand="gvEmployees_RowCommand">

                        <Columns>
                            <asp:BoundField DataField="EmployeeID" HeaderText="ID">
                                <ItemStyle Width="50px" />
                            </asp:BoundField>

                            <asp:BoundField DataField="FullName" HeaderText="Full Name">
                                <ItemStyle Width="200px" />
                            </asp:BoundField>

                            <asp:BoundField DataField="Email" HeaderText="Email">
                                <ItemStyle Width="250px" />
                            </asp:BoundField>

                            <asp:BoundField DataField="Position" HeaderText="Position">
                                <ItemStyle Width="150px" />
                            </asp:BoundField>

                            <asp:BoundField DataField="Phone" HeaderText="Phone">
                                <ItemStyle Width="150px" />
                            </asp:BoundField>

                            <asp:ImageField DataImageUrlField="ProfileImage" HeaderText="Profile Picture">
                                <ControlStyle Width="50px" Height="50px" />
                            </asp:ImageField>

                            <asp:TemplateField HeaderText="Actions">
                                <ItemTemplate>
                                    <asp:Button ID="btnEdit" runat="server" CommandName="EditEmployee" CommandArgument='<%# Eval("EmployeeID") %>'
                                        CssClass="btn btn-primary" Text="Edit" />

                                    <asp:Button ID="btnDelete" runat="server" CommandName="DeleteEmployee" CommandArgument='<%# Eval("EmployeeID") %>'
                                        CssClass="btn btn-danger" Text="Archive"
                                        OnClientClick="return confirm('Are you sure you want to archive this employee?');" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>

                    </asp:GridView>

                </div>
            </div>
        </div>
    </div>

</asp:Content>