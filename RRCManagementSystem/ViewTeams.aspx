<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ViewTeams.aspx.cs" Inherits="RRCManagementSystem.ViewTeams" %>
<%@ Import Namespace="System.Data" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container py-4">
        <h3 class="text-primary fw-bold mb-4">Teams and Members Overview</h3>

        <!-- Date Filter -->
        <div class="row mb-4">
            <div class="col-md-4">
                <asp:TextBox ID="txtDate" runat="server" TextMode="Date" CssClass="form-control" />
            </div>
            <div class="col-md-auto">
                <asp:Button ID="btnFilterDate" runat="server" Text="Check Availability" CssClass="btn btn-primary" OnClick="btnFilterDate_Click" />
            </div>
        </div>

        <!-- Teams Repeater -->
        <asp:Repeater ID="rptTeams" runat="server" OnItemDataBound="rptTeams_ItemDataBound">
            <ItemTemplate>
                <div class="card shadow-sm mb-4">
                    <div class="card-header d-flex justify-content-between align-items-center bg-light">
                        <h5 class="mb-0 text-primary">Team: <%# Eval("GroupName") %></h5>
                        <span class='badge <%# Eval("Status").ToString() == "Available" ? "bg-success" : "bg-danger" %>'>
                            <%# Eval("Status") %>
                        </span>
                    </div>
                    <div class="card-body">
                        <ul class="list-group list-group-flush mb-0">
                            <asp:Repeater ID="rptEmployees" runat="server">
                                <ItemTemplate>
                                    <li class="list-group-item">
                                        <%# Eval("LastName") %>, <%# Eval("FirstName") %> <%# Eval("MiddleName") %> (<%# Eval("Department") %>)
                                    </li>
                                </ItemTemplate>
                            </asp:Repeater>

                            <asp:PlaceHolder ID="phNoMembers" runat="server">
                                <li class="list-group-item fst-italic text-muted">No members assigned to this team.</li>
                            </asp:PlaceHolder>
                        </ul>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>

        <asp:Label ID="lblMessage" runat="server" CssClass="fw-bold" />
    </div>
</asp:Content>
