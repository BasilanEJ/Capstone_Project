<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ClientsProfiles.aspx.cs" Inherits="RRCManagementSystem.ClientsProfiles" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        body {
            font-family: Arial, sans-serif;
        }

        .container {
            max-width: 1100px;
            margin: 30px auto;
            padding: 15px;
        }

        .card {
            border-radius: 10px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
            overflow: hidden;
            background-color: #fff;
            transition: box-shadow 0.3s ease;
        }

        .card:hover {
            box-shadow: 0 6px 18px rgba(0, 0, 0, 0.15);
        }

        .card-header {
            background-color: #0073CF; /* Blue header */
            color: #ffffff;
            padding: 20px;
            font-size: 20px;
            font-weight: 600;
            text-align: center;
        }

        .card-body {
            padding: 20px;
        }

        /* GridView Table */
        .table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 15px;
        }

        .table th, .table td {
            padding: 12px 15px;
            text-align: center;
            vertical-align: middle;
        }

        .table th {
            background-color: #0073CF;
            color: #ffffff;
            font-size: 14px;
        }

        .table-striped tbody tr:nth-of-type(odd) {
            background-color: #f9f9f9;
        }

        .table-bordered {
            border: 1px solid #dee2e6;
        }

        .table-bordered th,
        .table-bordered td {
            border: 1px solid #dee2e6;
        }

        /* View Button Style */
        .btn-view {
            background-color: #007bff;
            color: white;
            padding: 8px 16px;
            border-radius: 5px;
            border: none;
            font-size: 14px;
            cursor: pointer;
            transition: background-color 0.3s ease, transform 0.2s ease;
        }

        .btn-view:hover {
            background-color: #0056b3;
            transform: translateY(-1px);
        }

        /* Responsive adjustments */
        @media (max-width: 768px) {
            .table th, .table td {
                font-size: 12px;
                padding: 8px;
            }

            .btn-view {
                padding: 6px 12px;
                font-size: 12px;
            }

            .card-header {
                font-size: 18px;
            }
        }

        @media (max-width: 576px) {
            .container {
                padding: 10px;
            }

            .card-body {
                padding: 15px;
            }
        }
    </style>

    <div class="container mt-4">
        <div class="card">
            <div class="card-header">Approved Client Profiles</div>
            <div class="card-body">
                <asp:GridView ID="gvClients" runat="server" AutoGenerateColumns="False" CssClass="table table-bordered table-striped"
                    AllowPaging="True" PageSize="10" OnPageIndexChanging="gvClients_PageIndexChanging" OnRowCommand="gvClients_RowCommand">
                    <Columns>
                        <asp:BoundField DataField="ClientID" HeaderText="Client ID" ReadOnly="True" />
                        <asp:BoundField DataField="Name" HeaderText="Name" />
                        <asp:BoundField DataField="Email" HeaderText="Email" />
                        <asp:BoundField DataField="ContactNumber" HeaderText="Contact Number" />
                        <asp:BoundField DataField="City" HeaderText="City" />
                        <asp:BoundField DataField="Country" HeaderText="Country" />
                        <asp:TemplateField HeaderText="Actions">
                            <ItemTemplate>
                                <asp:Button ID="btnView" runat="server" CssClass="btn-view" Text="View Profile"
                                    CommandName="ViewProfile" CommandArgument='<%# Eval("ClientID") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </div>

</asp:Content>