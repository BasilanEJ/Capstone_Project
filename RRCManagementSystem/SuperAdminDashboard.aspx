<%@ Page Title="" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="SuperAdminDashboard.aspx.cs" Inherits="RRCManagementSystem.SuperAdminDashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <h1 class="mb-4 fw-bold">Welcome to the System Admin Dashboard</h1>

        <!-- Dashboard Summary Cards -->
        <div class="row g-4">
            <div class="col-md-6 col-lg-4">
                <div class="card text-center shadow-sm h-100">
                    <div class="card-body">
                        <h3 class="card-title display-6 text-dark">
                            <asp:Label ID="lblTotalAdmins" runat="server" Text="0" />
                        </h3>
                        <p class="text-muted mb-0">Total User Accounts</p>
                    </div>
                </div>
            </div>

            <div class="col-md-6 col-lg-4">
                <div class="card text-center shadow-sm h-100">
                    <div class="card-body">
                        <h3 class="card-title display-6 text-dark">
                            <asp:Label ID="lblAuditLogs" runat="server" Text="0" />
                        </h3>
                        <p class="text-muted mb-0">Audit Logs</p>
                    </div>
                </div>
            </div>
        </div>

        <!-- Hidden field to trigger modal -->
        <asp:HiddenField ID="hfShowModal" runat="server" />

        <!-- Recent Logs Section -->
        <div class="mt-5">
            <h2 class="mb-3 fw-semibold">Recent Audit Logs</h2>

            <div class="table-responsive">
                <asp:GridView ID="gvAuditLogs" runat="server" AutoGenerateColumns="False" CssClass="table table-bordered table-hover">
                    <Columns>
                        <asp:BoundField DataField="LogID" HeaderText="Log ID" />
                        <asp:BoundField DataField="AdminName" HeaderText="User Name" />
                        <asp:BoundField DataField="Action" HeaderText="Action" />
                        <asp:BoundField DataField="Timestamp" HeaderText="Timestamp" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </div>

    <!-- SweetAlert2 Script -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <script type="text/javascript">
        window.onload = function () {
            var showModal = document.getElementById('<%= hfShowModal.ClientID %>').value;
            if (showModal === "1") {
                Swal.fire({
                    icon: 'warning',
                    title: 'Unusual Activity Detected',
                    html: 'There have been <strong>50+ failed login attempts</strong> within the last 10 minutes. Please investigate immediately.',
                    confirmButtonColor: '#d33',
                    confirmButtonText: 'Understood'
                });
            }
        };
    </script>
</asp:Content>
