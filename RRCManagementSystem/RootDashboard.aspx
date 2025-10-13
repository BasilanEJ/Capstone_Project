<%@ Page Title="Root Dashboard" Language="C#" MasterPageFile="~/RootAdmin.Master" AutoEventWireup="true" CodeBehind="RootDashboard.aspx.cs" Inherits="RRCManagementSystem.RootDashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid mt-4">
        <h2 class="mb-4 fw-bold">Dashboard</h2>

        <div class="row g-4">
            <!-- SuperAdmin Count Card -->
            <div class="col-xl-3 col-md-6">
                <div class="card shadow border-0">
                    <div class="card-body text-center">
                        <i class="fas fa-user-shield fa-2x text-primary mb-2"></i>
                        <h6 class="text-muted">Total System Admins</h6>
                        <h3 class="fw-bold text-dark">
                            <asp:Label ID="lblTotalSuperAdmins" runat="server" Text="0"></asp:Label>
                        </h3>
                    </div>
                </div>
            </div>
        </div>

     
        <div class="row mt-4">
            <div class="col-12">
                <div class="card shadow border-0">
                    <div class="card-body">
                        <h6 class="text-muted mb-3">Active SystemAdmins</h6>
                        <asp:UpdatePanel ID="updActiveSuperAdmins" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <div class="table-responsive">
                                    <asp:GridView ID="gvActiveSuperAdmins" runat="server" AutoGenerateColumns="False"
                                        CssClass="table table-striped table-hover table-bordered" DataKeyNames="UserID"
                                        AllowPaging="True" PageSize="10" OnPageIndexChanging="gvActiveSuperAdmins_PageIndexChanging">
                                        <Columns>
                                            <asp:BoundField DataField="Email" HeaderText="Email" SortExpression="Email" />
                                            <asp:BoundField DataField="Name" HeaderText="Full Name" SortExpression="Name" />
                                            <asp:BoundField DataField="CreatedAt" HeaderText="Created At" DataFormatString="{0:yyyy-MM-dd}" SortExpression="CreatedAt" />
                                        </Columns>
                                    </asp:GridView>
                                </div>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
