<%@ Page Title="Reports" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="Reports.aspx.cs" Inherits="RRCManagementSystem.Reports" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container my-4">
        <div class="card shadow-sm">
            <div class="card-header bg-primary text-white fw-bold text-center">
                Reports Dashboard
            </div>
            <div class="card-body">

                <!-- Feedback Message -->
                <asp:Label ID="lblMessage" runat="server" CssClass="text-danger d-block text-center mb-3" />

                  <asp:Label ID="lblDebug" runat="server" CssClass="text-muted small d-block text-center mb-3" />
                <!-- Filter Section -->
                <div class="row g-3 align-items-end mb-4">
                    <div class="col-md-4">
                        <label for="ddlModule" class="form-label">Select Module</label>
                        <asp:DropDownList ID="ddlModule" runat="server" CssClass="form-select">
                            <asp:ListItem Text="-- Select Module --" Value="" />
                            <asp:ListItem Text="User Accounts" Value="Admins" />
                            <asp:ListItem Text="Archived Admins" Value="ArchivedAdmins" />
                            <asp:ListItem Text="Roles" Value="Roles" />
                            <asp:ListItem Text="Audit Logs" Value="AuditLogs" />
                            <asp:ListItem Text="System Changes" Value="SystemChanges" />
                        </asp:DropDownList>
                    </div>

                    <div class="col-md-3">
                        <label for="txtDateFrom" class="form-label">Date From</label>
                        <asp:TextBox ID="txtDateFrom" runat="server" CssClass="form-control" TextMode="Date" />
                    </div>

                    <div class="col-md-3">
                        <label for="txtDateTo" class="form-label">Date To</label>
                        <asp:TextBox ID="txtDateTo" runat="server" CssClass="form-control" TextMode="Date" />
                    </div>

                    <div class="col-md-2 d-grid">
                        <asp:Button ID="btnGenerate" runat="server" Text="Generate" CssClass="btn btn-success" OnClick="btnGenerate_Click" />
                    </div>
                </div>

                <!-- Grid View -->
                <asp:GridView ID="gvReports" runat="server" CssClass="table table-bordered table-striped text-center" AutoGenerateColumns="True" EmptyDataText="No records found." />
            </div>
        </div>
    </div>
</asp:Content>
