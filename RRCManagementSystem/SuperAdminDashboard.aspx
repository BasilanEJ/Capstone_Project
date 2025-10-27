<%@ Page Title="" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="SuperAdminDashboard.aspx.cs" Inherits="RRCManagementSystem.SuperAdminDashboard" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    
    <div class="container-fluid">
        <h1 class="mb-4 fw-bold text-dark">System Admin Dashboard</h1>
    
        
    <!-- UpdatePanel for Dashboard Summary (Total Admins, Audit Logs) -->
<asp:UpdatePanel ID="upDashboardSummary" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <div class="row g-4 mb-5">
            <div class="col-12 col-sm-6 col-lg-4">
                <div class="card border-0 shadow-sm h-100 rounded-lg">
                    <div class="card-body text-center py-4">
                        <h3 class="display-5 fw-bold text-dark mb-2">
                            <asp:Label ID="lblTotalAdmins" runat="server" Text="0" />
                        </h3>
                        <p class="text-muted mb-0 small">Total User Accounts</p>
                    </div>
                </div>
            </div>
        </div>
    </ContentTemplate>
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="timerRefreshSummary" EventName="Tick" />
    </Triggers>
</asp:UpdatePanel>


        <asp:HiddenField ID="hfShowModal" runat="server" />
        <asp:HiddenField ID="hfUnlockAction" runat="server" />
        <asp:HiddenField ID="hfUnlockID" runat="server" />


        <div class="mb-5">
            <div class="d-flex align-items-center mb-4">
                <div class="me-3">
                    <div class="bg-light rounded-circle d-flex align-items-center justify-content-center" 
                         style="width: 48px; height: 48px;">
                        <i class="bi bi-shield-lock fs-4 text-dark"></i>
                    </div>
                </div>
                <div>
                    <h2 class="mb-0 fw-semibold text-dark">Security Lockout Status</h2>
                    <p class="text-muted mb-0 small">Monitor and manage account security</p>
                </div>
            </div>
        </div>
            

<asp:Timer ID="timerRefreshSummary" runat="server" Interval="30000" OnTick="timerRefreshSummary_Tick" />

<!-- UpdatePanel for secondary summary cards -->
<asp:UpdatePanel ID="upSummaryCards" runat="server">
    <ContentTemplate>

        <div class="row g-3 mb-4">
            <div class="col-6 col-md-6 col-lg-3">
                <div class="card border-0 shadow-sm rounded-lg">
                    <div class="card-body text-center py-3">
                        <div class="d-flex align-items-center justify-content-center mb-2">
                            <i class="bi bi-person-lock text-muted me-2"></i>
                            <h3 class="mb-0 fw-bold text-dark">
                                <asp:Label ID="lblLockedUsers" runat="server" Text="0" />
                            </h3>
                        </div>
                        <small class="text-muted">Locked Users</small>
                    </div>
                </div>
            </div>

            <div class="col-6 col-md-6 col-lg-3">
                <div class="card border-0 shadow-sm rounded-lg">
                    <div class="card-body text-center py-3">
                        <div class="d-flex align-items-center justify-content-center mb-2">
                            <i class="bi bi-laptop text-muted me-2"></i>
                            <h3 class="mb-0 fw-bold text-dark">
                                <asp:Label ID="lblLockedDevices" runat="server" Text="0" />
                            </h3>
                        </div>
                        <small class="text-muted">Locked Devices</small>
                    </div>
                </div>
            </div>

            <div class="col-6 col-md-6 col-lg-3">
                <div class="card border-0 shadow-sm rounded-lg">
                    <div class="card-body text-center py-3">
                        <div class="d-flex align-items-center justify-content-center mb-2">
                            <i class="bi bi-wifi text-muted me-2"></i>
                            <h3 class="mb-0 fw-bold text-dark">
                                <asp:Label ID="lblLockedIPs" runat="server" Text="0" />
                            </h3>
                        </div>
                        <small class="text-muted">Locked IPs</small>
                    </div>
                </div>
            </div>

            <div class="col-6 col-md-6 col-lg-3">
                <div class="card border-0 shadow-sm rounded-lg">
                    <div class="card-body text-center py-3">
                        <div class="d-flex align-items-center justify-content-center mb-2">
                            <i class="bi bi-exclamation-triangle text-muted me-2"></i>
                            <h3 class="mb-0 fw-bold text-dark">
                                <asp:Label ID="lblFailedAttemptsHour" runat="server" Text="0" />
                            </h3>
                        </div>
                        <small class="text-muted">Failed Attempts (1h)</small>
                    </div>
                </div>
            </div>
        </div>

    </ContentTemplate>
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="timerRefreshSummary" EventName="Tick" />
    </Triggers>
</asp:UpdatePanel>


        <!-- Locked Users Table -->
    <asp:UpdatePanel ID="upLockedUsers" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <div class="card border-0 shadow-sm mb-4 rounded-lg">
            <div class="card-header bg-white border-bottom py-3">
                <div class="d-flex justify-content-between align-items-center">
                    <div class="d-flex align-items-center">
                        <i class="bi bi-person-lock text-muted me-2"></i>
                        <h6 class="mb-0 fw-semibold">Currently Locked Users</h6>
                    </div>
                    <asp:Button ID="btnUnlockAllUsers" runat="server" Text="Unlock All"
                        CssClass="btn btn-sm btn-outline-dark"
                        OnClick="btnUnlockAllUsers_Click"
                        OnClientClick="return confirmUnlockAll('users');" />
                </div>
            </div>
            <div class="card-body p-0">
                <div class="table-responsive">
                    <asp:GridView ID="gvLockedUsers" runat="server" AutoGenerateColumns="False"
                        CssClass="table table-hover mb-0"
                        EmptyDataText="No locked users at the moment. ✅"
                        OnRowCommand="gvLockedUsers_RowCommand"
                        GridLines="None">
                        <Columns>
                            <asp:BoundField DataField="Email" HeaderText="Email"
                                HeaderStyle-CssClass="text-muted small fw-normal border-0 bg-light"
                                ItemStyle-CssClass="align-middle" />
                            <asp:BoundField DataField="FailedAttempts" HeaderText="Attempts"
                                ItemStyle-CssClass="text-center align-middle"
                                HeaderStyle-CssClass="text-muted small fw-normal border-0 bg-light text-center" />
                            <asp:BoundField DataField="LockedUntil" HeaderText="Locked Until"
                                DataFormatString="{0:MMM dd, hh:mm tt}"
                                ItemStyle-CssClass="align-middle"
                                HeaderStyle-CssClass="text-muted small fw-normal border-0 bg-light" />
                            <asp:BoundField DataField="MinutesRemaining" HeaderText="Time Left"
                                ItemStyle-CssClass="text-center align-middle"
                                HeaderStyle-CssClass="text-muted small fw-normal border-0 bg-light text-center" />
                            <asp:BoundField DataField="LastIPAddress" HeaderText="Last IP"
                                ItemStyle-CssClass="align-middle"
                                HeaderStyle-CssClass="text-muted small fw-normal border-0 bg-light" />
                            <asp:TemplateField HeaderText="Action"
                                ItemStyle-CssClass="text-center align-middle"
                                HeaderStyle-CssClass="text-muted small fw-normal border-0 bg-light text-center">
                                <ItemTemplate>
                                    <button type="button" class="btn btn-sm btn-outline-success"
                                        onclick="confirmUnlock('user', <%# Eval("UserID") %>); return false;">
                                        Unlock
                                    </button>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <EmptyDataRowStyle CssClass="text-center text-muted py-4" />
                    </asp:GridView>
                </div>
            </div>
        </div>

        <!-- Invisible timer that triggers partial refresh -->
        <asp:Timer ID="TimerLockedUsers" runat="server" Interval="30000" 
                    OnTick="TimerLockedUsers_Tick" Enabled="true" />
    </ContentTemplate>
</asp:UpdatePanel>


        <!-- Locked Devices Table -->
    <asp:UpdatePanel ID="upLockedDevices" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <div class="card border-0 shadow-sm mb-4 rounded-lg">
            <div class="card-header bg-white border-bottom py-3">
                <div class="d-flex justify-content-between align-items-center">
                    <div class="d-flex align-items-center">
                        <i class="bi bi-laptop text-muted me-2"></i>
                        <h6 class="mb-0 fw-semibold">Currently Locked Devices</h6>
                    </div>
                    <asp:Button ID="btnUnlockAllDevices" runat="server" Text="Unlock All" 
                        CssClass="btn btn-sm btn-outline-dark"
                        OnClick="btnUnlockAllDevices_Click"
                        OnClientClick="return confirmUnlockAll('devices');" />
                </div>
            </div>
            <div class="card-body p-0">
                <div class="table-responsive">
                    <asp:GridView ID="gvLockedDevices" runat="server" 
                        AutoGenerateColumns="False" 
                        CssClass="table table-hover mb-0" 
                        EmptyDataText="No locked devices at the moment. ✅"
                        OnRowCommand="gvLockedDevices_RowCommand" 
                        GridLines="None">

                        <Columns>
                            <asp:BoundField DataField="BrowserName" HeaderText="Browser" 
                                HeaderStyle-CssClass="text-muted small fw-normal border-0 bg-light" 
                                ItemStyle-CssClass="align-middle" />

                            <asp:BoundField DataField="Platform" HeaderText="OS" 
                                HeaderStyle-CssClass="text-muted small fw-normal border-0 bg-light" 
                                ItemStyle-CssClass="align-middle" />

                            <asp:BoundField DataField="FailedAttempts" HeaderText="Attempts" 
                                ItemStyle-CssClass="text-center align-middle" 
                                HeaderStyle-CssClass="text-muted small fw-normal border-0 bg-light text-center" />

                            <asp:BoundField DataField="LockedUntil" HeaderText="Locked Until" 
                                DataFormatString="{0:MMM dd, hh:mm tt}" 
                                ItemStyle-CssClass="align-middle" 
                                HeaderStyle-CssClass="text-muted small fw-normal border-0 bg-light" />

                            <asp:BoundField DataField="MinutesRemaining" HeaderText="Time Left" 
                                ItemStyle-CssClass="text-center align-middle" 
                                HeaderStyle-CssClass="text-muted small fw-normal border-0 bg-light text-center" />

                            <asp:BoundField DataField="LastIPAddress" HeaderText="Last IP" 
                                HeaderStyle-CssClass="text-muted small fw-normal border-0 bg-light" 
                                ItemStyle-CssClass="align-middle" />

                            <asp:TemplateField HeaderText="Action" 
                                ItemStyle-CssClass="text-center align-middle" 
                                HeaderStyle-CssClass="text-muted small fw-normal border-0 bg-light text-center">
                                <ItemTemplate>
                                    <button type="button" class="btn btn-sm btn-outline-success" 
                                        onclick="confirmUnlock('device', <%# Eval("DeviceLockoutID") %>); return false;">
                                        Unlock
                                    </button>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>

                        <EmptyDataRowStyle CssClass="text-center text-muted py-4" />
                    </asp:GridView>
                </div>
            </div>
        </div>

        <asp:Timer ID="TimerLockedDevices" runat="server" Interval="30000" 
                    OnTick="TimerLockedDevices_Tick" Enabled="true" />
    </ContentTemplate>
</asp:UpdatePanel>


   <asp:UpdatePanel ID="upRecentFailures" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <div class="card border-0 shadow-sm mb-4 rounded-lg">
            <div class="card-header bg-white border-bottom py-3">
                <div class="d-flex align-items-center">
                    <i class="bi bi-clock-history text-muted me-2"></i>
                    <h6 class="mb-0 fw-semibold">Recent Failed Login Attempts</h6>
                    <span class="badge bg-light text-dark ms-2 small">Last Hour</span>
                </div>
            </div>
            <div class="card-body p-0">
                <div class="table-responsive">
                    <asp:GridView ID="gvRecentFailures" runat="server" AutoGenerateColumns="False"
                        CssClass="table table-hover table-sm mb-0"
                        EmptyDataText="No failed attempts in the last hour. ✅"
                        GridLines="None">

                        <Columns>
                            <asp:BoundField DataField="AttemptTime" HeaderText="Time" DataFormatString="{0:MMM dd, hh:mm tt}"
                                HeaderStyle-CssClass="text-muted small fw-normal border-0 bg-light"
                                ItemStyle-CssClass="align-middle small" />

                            <asp:BoundField DataField="IPAddress" HeaderText="IP Address"
                                HeaderStyle-CssClass="text-muted small fw-normal border-0 bg-light"
                                ItemStyle-CssClass="align-middle font-monospace small" />

                            <asp:TemplateField HeaderText="Email"
                                HeaderStyle-CssClass="text-muted small fw-normal border-0 bg-light">
                                <ItemTemplate>
                                    <div class="d-flex align-items-center">
                                        <span class="truncate-text me-2" data-full-text='<%# Eval("EncryptedEmail") %>'>
                                            <%# Eval("EncryptedEmail").ToString().Length > 20 ? 
                                                Eval("EncryptedEmail").ToString().Substring(0, 20) + "..." : 
                                                Eval("EncryptedEmail") %>
                                        </span>
                                        <%# Eval("EncryptedEmail").ToString().Length > 20 ? 
                                            "<button type='button' class='btn btn-sm btn-outline-secondary btn-view-details' onclick='showDetails(\"Email\", \"" + Eval("EncryptedEmail") + "\"); return false;'><i class=\"bi bi-eye\"></i></button>" : 
                                            "" %>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:BoundField DataField="AccountType" HeaderText="Account Type"
                                HeaderStyle-CssClass="text-muted small fw-normal border-0 bg-light"
                                ItemStyle-CssClass="align-middle small" />

                            <asp:TemplateField HeaderText="Email Hash"
                                HeaderStyle-CssClass="text-muted small fw-normal border-0 bg-light"
                                Visible ="false">
                                <ItemTemplate>
                                    <div class="d-flex align-items-center">
                                        <span class="truncate-text me-2 font-monospace" data-full-text='<%# Eval("EmailHash") %>'>
                                            <%# Eval("EmailHash").ToString().Length > 20 ? 
                                                Eval("EmailHash").ToString().Substring(0, 20) + "..." : 
                                                Eval("EmailHash") %>
                                        </span>
                                        <%# Eval("EmailHash").ToString().Length > 20 ? 
                                            "<button type='button' class='btn btn-sm btn-outline-secondary btn-view-details' onclick='showDetails(\"Email Hash\", \"" + Eval("EmailHash") + "\"); return false;'><i class=\"bi bi-eye\"></i></button>" : 
                                            "" %>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="User Agent"
                                HeaderStyle-CssClass="text-muted small fw-normal border-0 bg-light">
                                <ItemTemplate>
                                    <div class="d-flex align-items-center">
                                        <span class="truncate-text me-2" data-full-text='<%# Eval("UserAgent") %>'>
                                            <%# Eval("UserAgent").ToString().Length > 20 ? 
                                                Eval("UserAgent").ToString().Substring(0, 20) + "..." : 
                                                Eval("UserAgent") %>
                                        </span>
                                        <%# Eval("UserAgent").ToString().Length > 20 ? 
                                            "<button type='button' class='btn btn-sm btn-outline-secondary btn-view-details' onclick='showDetails(\"User Agent\", \"" + Server.HtmlEncode(Eval("UserAgent").ToString()) + "\"); return false;'><i class=\"bi bi-eye\"></i></button>" : 
                                            "" %>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>

                        <EmptyDataRowStyle CssClass="text-center text-muted py-4" />
                    </asp:GridView>
                </div>
            </div>
        </div>

        <!-- Invisible 30-second timer -->
        <asp:Timer ID="TimerRecentFailures" runat="server" Interval="30000"
                    OnTick="TimerRecentFailures_Tick" Enabled="true" />
    </ContentTemplate>
</asp:UpdatePanel>

<div class="mb-5">
    <div class="d-flex align-items-center mb-4">
        <div class="me-3">
            <div class="bg-light rounded-circle d-flex align-items-center justify-content-center" 
                 style="width: 48px; height: 48px;">
                <i class="bi bi-file-text fs-4 text-dark"></i>
            </div>
        </div>
        <div>
            <h2 class="mb-0 fw-semibold text-dark">Recent Audit Logs</h2>
            <p class="text-muted mb-0 small">System activity history</p>
        </div>
    </div>

    <!-- Wrap in UpdatePanel for partial refresh -->
    <asp:UpdatePanel ID="upAuditLogs" runat="server" UpdateMode="Conditional">
        <ContentTemplate>

            <div class="card border-0 shadow-sm rounded-lg">
                <div class="card-body p-0">
                    <div class="table-responsive">
                        <asp:GridView ID="gvAuditLogs" runat="server" AutoGenerateColumns="False" 
                            CssClass="table table-hover mb-0" GridLines="None">
                            <Columns>
                                <asp:BoundField DataField="LogID" HeaderText="ID" HeaderStyle-CssClass="text-muted small fw-normal border-0 bg-light" ItemStyle-CssClass="align-middle" />
                                <asp:BoundField DataField="AdminName" HeaderText="User" HeaderStyle-CssClass="text-muted small fw-normal border-0 bg-light" ItemStyle-CssClass="align-middle" />
                                <asp:BoundField DataField="Action" HeaderText="Action" HeaderStyle-CssClass="text-muted small fw-normal border-0 bg-light" ItemStyle-CssClass="align-middle" />
                                <asp:BoundField DataField="Timestamp" HeaderText="Timestamp" DataFormatString="{0:MMM dd, yyyy hh:mm tt}" HeaderStyle-CssClass="text-muted small fw-normal border-0 bg-light" ItemStyle-CssClass="align-middle" />
                            </Columns>
                            <EmptyDataRowStyle CssClass="text-center text-muted py-4" />
                        </asp:GridView>
                    </div>
                </div>
            </div>

            <!-- Invisible Timer -->
            <asp:Timer ID="TimerAuditLogs" runat="server" Interval="30000" OnTick="TimerAuditLogs_Tick" Enabled="true"></asp:Timer>

        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="TimerAuditLogs" EventName="Tick" />
        </Triggers>
    </asp:UpdatePanel>
</div>
</div>

    <!-- Bootstrap Icons -->
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.10.0/font/bootstrap-icons.css">
    
    <!-- SweetAlert2 -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    
<script type="text/javascript">
    // Custom functions for modal interactions (unchanged)
    window.onload = function () {
        var showModal = document.getElementById('<%= hfShowModal.ClientID %>').value;
        if (showModal === "1") {
            Swal.fire({
                icon: 'warning',
                title: 'Unusual Activity Detected',
                html: 'There have been <strong>50+ failed login attempts</strong> within the last 10 minutes.<br>Please investigate immediately.',
                confirmButtonColor: '#212529',
                confirmButtonText: 'Investigate'
            });
        }
    };

    function confirmUnlock(type, id) {
        const typeName = type === 'user' ? 'user account' : 'device';
        Swal.fire({
            title: 'Confirm Unlock',
            text: 'Are you sure you want to unlock this ' + typeName + '?',
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#198754',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Yes, unlock it',
            cancelButtonText: 'Cancel'
        }).then((result) => {
            if (result.isConfirmed) {
                document.getElementById('<%= hfUnlockAction.ClientID %>').value = type;
            document.getElementById('<%= hfUnlockID.ClientID %>').value = id;
            __doPostBack('', '');
        }
    });
    return false;
}

function confirmUnlockAll(type) {
    const typeName = type === 'users' ? 'all locked users' : 'all locked devices';
    
    Swal.fire({
        title: 'Confirm Unlock All',
        html: 'Are you sure you want to unlock <strong>' + typeName + '</strong>?<br><small class="text-muted">This action will immediately remove all lockouts.</small>',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#198754',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Yes, unlock all',
        cancelButtonText: 'Cancel'
    }).then((result) => {
        if (result.isConfirmed) {
            if (type === 'users') {
                __doPostBack('<%= btnUnlockAllUsers.UniqueID %>', '');
            } else {
                __doPostBack('<%= btnUnlockAllDevices.UniqueID %>', '');
            }
        }
    });
    return false;
}

    function showDetails(fieldName, fullText) {
        Swal.fire({
            title: fieldName + ' Details',
            html: '<div class="text-start"><pre class="bg-light p-3 rounded small" style="word-wrap: break-word; white-space: pre-wrap;">' + fullText + '</pre></div>',
            width: '600px',
            confirmButtonColor: '#212529',
            confirmButtonText: 'Close',
            customClass: {
                htmlContainer: 'text-start'
            }
        });
    }

</script>

    <script type="text/javascript">
        var prm = Sys.WebForms.PageRequestManager.getInstance();

        prm.add_beginRequest(function () {
            window.scrollX_before = window.scrollX;
            window.scrollY_before = window.scrollY;
        });

        prm.add_endRequest(function () {
            window.scrollTo(window.scrollX_before, window.scrollY_before);
        });
    </script>

    <style>
        /* Minimal professional styling */
        .rounded-lg {
            border-radius: 0.5rem !important; /* Consistent rounded corners */
        }
        .card {
            transition: transform 0.2s, box-shadow 0.2s;
        }
        
        .card:hover {
            transform: translateY(-2px);
            box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.1) !important;
        }
        
        .table > :not(caption) > * > * {
            padding: 0.75rem;
        }
        
        .btn-outline-dark:hover {
            background-color: #212529;
            color: white;
        }
        
        .btn-outline-success:hover {
            background-color: #198754;
            color: white;
        }
        
        .font-monospace {
            font-family: 'Courier New', monospace;
        }
        
        /* Truncate text styling */
        .truncate-text {
            display: inline-block;
            max-width: 200px;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
            vertical-align: middle;
        }
        
        /* View details button */
        .btn-view-details {
            padding: 0.25rem 0.5rem;
            font-size: 0.75rem;
            line-height: 1;
            border-radius: 0.25rem;
            transition: all 0.2s;
        }
        
        .btn-view-details:hover {
            background-color: #6c757d;
            border-color: #6c757d;
            color: white;
        }
        
        .btn-view-details i {
            font-size: 0.875rem;
        }
        
        @media (max-width: 992px) {
            .truncate-text {
                max-width: 130px; 
            }
        }
        
        @media (max-width: 768px) {
            .truncate-text {
                max-width: 90px;
            }
            
            .table-sm > :not(caption) > * > * {
                padding: 0.5rem 0.25rem;
            }
            .table > :not(caption) > * > * {
                padding: 0.5rem; 
            }
        }
        
        @media (max-width: 576px) {
            .truncate-text {
                max-width: 70px;
            }
            
            .btn-view-details {
                padding: 0.2rem 0.4rem;
            }
        }

    </style>
</asp:Content>
