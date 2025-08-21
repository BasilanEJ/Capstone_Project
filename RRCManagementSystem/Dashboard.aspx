<%@ Page Title="Dashboard"
    Language="C#"
    MasterPageFile="~/Admin.Master"
    AutoEventWireup="true"
    MaintainScrollPositionOnPostBack="true"
    CodeBehind="Dashboard.aspx.cs"
    Inherits="RRCManagementSystem.Dashboard" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Bootstrap 5 -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .dashboard-container, .sales-overview, .blockchain-transparency { padding: 30px 20px; }
        .container { max-width: 1200px; margin: 0 auto; }
        .page-title { font-size: 28px; color: #333; text-align: center; margin-bottom: 10px; }
        .welcome-message { text-align: center; font-size: 20px; color: #4b5563; margin-bottom: 30px; }
        .metrics-container { display: flex; flex-wrap: wrap; justify-content: center; gap: 20px; }
        .metric-card { background-color: #fff; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,.1); width: 250px; text-align: center; padding: 20px; transition: transform .3s, box-shadow .3s; }
        .metric-card:hover { transform: translateY(-5px); box-shadow: 0 5px 15px rgba(0,0,0,.2); }
        .metric-card h4 { font-size: 18px; color: #555; margin-bottom: 10px; }
        .metric-value { font-size: 30px; font-weight: bold; color: #007bff; }
        .btn-sales { background-color:#007bff; color:#fff; border:none; padding:10px 20px; border-radius:4px; cursor:pointer; font-weight:700; }
        .btn-sales:hover { background-color:#0056b3; }
        .custom-table { width: 100%; border-collapse: collapse; margin-top: 20px; }
        .custom-table th, .custom-table td { padding: 12px 15px; border: 1px solid #ddd; text-align: center; }
        .custom-table th { background-color:#007bff; color:#fff; }
        .custom-table tbody tr:nth-child(even) { background-color:#f9f9f9; }
        .custom-table tbody tr:hover { background-color:#f1f1f1; }

        .weekly-calendar .card { min-height:100px; font-size:15px; border:none; border-radius:10px; box-shadow:0 4px 12px rgba(0,0,0,.1); background:#fff; overflow:hidden; }
        .weekly-calendar .card-header { font-size:18px; font-weight:600; background:#0d6efd; color:#fff; padding:12px 20px; text-align:center; border-bottom:1px solid #ccc; }
        .weekly-calendar .table { margin-bottom:0; }
        .weekly-calendar .table td, .weekly-calendar .table th { width:14.28%; min-width:160px; height:140px; vertical-align:top; padding:10px; background:#f9f9f9; font-size:14px; border:1px solid #dee2e6; cursor:pointer; }
        .weekly-calendar .table td:hover { background:#e8f0fe; transition: background-color .2s; }
        .weekly-calendar .table th { background:#f1f5ff; }
        .weekly-calendar .text-muted em { font-size:13px; }
    </style>

    <!-- Admin Dashboard Title -->
    <section class="dashboard-container">
        <div class="container">
            <h2 class="page-title">Admin Dashboard</h2>
            <div class="welcome-message">
                <asp:Label ID="lblWelcome" runat="server" Font-Bold="true"></asp:Label>
            </div>

            <!-- Metrics -->
            <div class="metrics-container">
                <div class="metric-card">
                    <h4>Total Clients</h4>
                    <asp:Label ID="lblTotalClients" runat="server" CssClass="metric-value"></asp:Label>
                </div>
                <div class="metric-card">
                    <h4>Total Employees</h4>
                    <asp:Label ID="lblTotalWorkers" runat="server" CssClass="metric-value"></asp:Label>
                </div>
                <div class="metric-card">
                    <h4>Today's Sales</h4>
                    <asp:Label ID="lblTodaySales" runat="server" CssClass="metric-value"></asp:Label>
                </div>
                <div class="metric-card">
                    <h4>This Month's Sales</h4>
                    <asp:Label ID="lblMonthSales" runat="server" CssClass="metric-value"></asp:Label>
                </div>
            </div>
        </div>
    </section>

    <!-- Weekly Booking Calendar -->
    <section class="weekly-calendar mb-4">
        <div class="container">
            <div class="card">
                <div class="card-header">📅 Weekly Booking Calendar</div>
                <div class="table-responsive">
                    <asp:Table ID="tblCalendar" runat="server" CssClass="table table-bordered text-center" />
                </div>
            </div>
        </div>
    </section>

    <!-- Booking details modal -->
    <div class="modal fade" id="bookingDetailsModal" tabindex="-1" aria-labelledby="bookingDetailsLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered"><div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="bookingDetailsLabel">Booking Details</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>
            <div class="modal-body" id="bookingDetailsContent"></div>
        </div></div>
    </div>

    <script>
        function showBookingDetails(details) {
            document.getElementById('bookingDetailsContent').innerHTML = details;
            new bootstrap.Modal(document.getElementById('bookingDetailsModal')).show();
        }
    </script>

    <!-- Sales Overview -->
    <section class="sales-overview">
        <div class="container">
            <hr />
            <div style="display:flex;justify-content:space-between;align-items:center;">
                <h3 class="section-title" style="margin:0;">Sales Overview</h3>
                <div>
                    <button type="button" class="btn-sales" onclick="setChartType('bar')">Bar</button>
                    <button type="button" class="btn-sales" onclick="setChartType('line')">Line</button>
                </div>
            </div>

            <div class="sales-controls" style="text-align:center;margin:20px 0;">
                <button type="button" onclick="loadSalesData('daily')"   class="btn-sales">Daily</button>
                <button type="button" onclick="loadSalesData('weekly')"  class="btn-sales">Weekly</button>
                <button type="button" onclick="loadSalesData('monthly')" class="btn-sales">Monthly</button>
                <button type="button" onclick="loadSalesData('yearly')"  class="btn-sales">Yearly</button>
            </div>

            <div class="chart-container">
                <canvas id="salesChart" style="height:400px;"></canvas>
            </div>
        </div>
    </section>

    <!-- ===== BLOCKCHAIN SECTION (stays in place on click) ===== -->
    <!-- Anchor to scroll back after async updates -->
    <span id="blockchainSection"></span>

    <asp:UpdatePanel ID="upBlockchain" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <section class="blockchain-transparency" id="blockchain-transparency">
                <div class="container">
                    <hr />
                    <h3 class="section-title">Blockchain Sales Transparency</h3>

                    <!-- Filter + Verify -->
                    <div style="display:flex;justify-content:center;align-items:center;gap:10px;margin-bottom:20px;">
                        <asp:TextBox ID="txtFromDate" runat="server" TextMode="Date" CssClass="form-control" Style="max-width:180px;" />
                        <asp:TextBox ID="txtToDate"   runat="server" TextMode="Date" CssClass="form-control" Style="max-width:180px;" />
                        <asp:Button ID="btnFilterBlockchain" runat="server" Text="📅 Filter"
                                    CssClass="btn-sales" OnClick="btnFilterBlockchain_Click"
                                    CausesValidation="false" UseSubmitBehavior="false" />
                   <asp:Button ID="btnVerifyBlockchain" runat="server" Text="🔍 Verify Blockchain"
                                    CssClass="btn-sales" OnClick="btnVerifyBlockchain_Click"
                                    CausesValidation="false" UseSubmitBehavior="false" />
                          <!--   <asp:Button ID="btnRecomputeChain"  
            runat="server" 
            Text="🔧 Recompute Chain (Maintenance)" 
            CssClass="btn btn-warning" 
            OnClick="btnRecomputeChain_Click" /> -->

                    </div>

                    <div style="text-align:center;margin-bottom:30px;">
                        <asp:Label ID="lblVerificationResult" runat="server" Style="font-size:18px;font-weight:bold;"></asp:Label>
                    </div>

                    <!-- Blockchain Logs Table -->
                    <div class="table-responsive">
                        <table class="custom-table">
                            <thead>
                                <tr>
                                    <th>Log ID</th>
                                    <th>Transaction ID</th>
                                    <th>Verification Code<br/><small>(Ensures the transaction hasn’t been altered)</small></th> 
                                    <th>Timestamp</th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:Repeater ID="rptBlockchainLog" runat="server" OnItemCommand="rptBlockchainLog_ItemCommand">
                                    <ItemTemplate>
                                        <tr>
                                            <td><%# Eval("LogID") %></td>
                                            <td>
                                                <asp:LinkButton ID="lnkTransactionID" runat="server"
                                                    Text='<%# Eval("TransactionID") %>'
                                                    CommandName="ViewJson"
                                                    CommandArgument='<%# Eval("TransactionID") %>'
                                                    CssClass="btn btn-link"
                                                    CausesValidation="false" />
                                            </td>
                                            <td><%# Eval("SaleHash") %></td>
                                            <td><%# FormatLocalPH(Eval("Timestamp")) %></td>
                                        </tr>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </tbody>
                        </table>
                    </div>
                </div>
            </section>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnFilterBlockchain" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnVerifyBlockchain" EventName="Click" />
        </Triggers>
    </asp:UpdatePanel>

    <!-- JSON modal -->
    <div id="jsonModal" style="display:none; position:fixed; inset:0; background:rgba(0,0,0,.55); z-index:9999; align-items:center; justify-content:center;">
        <div style="background:#fff; width:min(800px,92vw); max-height:80vh; overflow:auto; padding:16px; border-radius:12px;">
            <div style="display:flex; justify-content:space-between; align-items:center; gap:12px; margin-bottom:10px;">
                <h4 style="margin:0;">Transaction JSON</h4>
                <button onclick="closeModal()" class="btn btn-sm btn-secondary">Close</button>
            </div>
            <pre id="jsonModalBody" style="white-space:pre-wrap; word-break:break-word; margin:0;"></pre>
        </div>
    </div>

    <script>
        function openModal(jsonStr) {
            try { jsonStr = JSON.stringify(JSON.parse(jsonStr), null, 2); } catch (e) { }
            document.getElementById('jsonModalBody').textContent = jsonStr;
            document.getElementById('jsonModal').style.display = 'flex';
        }
        function closeModal() { document.getElementById('jsonModal').style.display = 'none'; }
    </script>

    <input type="hidden" id="salesDataJson" value='<%= salesDataJson %>' />

    <!-- Chart.js -->
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <script>
        let chart, currentChartType = 'bar';
        function setChartType(type) { currentChartType = type; if (window._lastRange) loadSalesData(window._lastRange); }
        function loadSalesData(type) {
            window._lastRange = type;
            fetch('Dashboard.aspx/GetSalesData', {
                method: 'POST', headers: { 'Content-Type': 'application/json; charset=utf-8' },
                body: JSON.stringify({ type })
            }).then(r => r.json()).then(res => updateChart(res.d)).catch(console.error);
        }
        function updateChart(salesData) {
            if (!salesData) return;
            const ctx = document.getElementById('salesChart').getContext('2d');
            if (chart) chart.destroy();
            chart = new Chart(ctx, {
                type: currentChartType,
                data: { labels: salesData.labels, datasets: [{ label: 'Sales (₱)', data: salesData.data, tension: currentChartType === 'line' ? 0.3 : 0, fill: false }] },
                options: {
                    responsive: true, maintainAspectRatio: false,
                    scales: { y: { beginAtZero: true, ticks: { callback: v => '₱' + Number(v).toLocaleString('en-PH', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) } } },
                    plugins: { tooltip: { callbacks: { label: (ctx) => 'Sales: ₱' + Number(ctx.parsed.y).toLocaleString('en-PH', { minimumFractionDigits: 2 }) } }, legend: { display: true } },
                    elements: { point: { radius: currentChartType === 'line' ? 3 : 0 } }
                }
            });
        }
        document.addEventListener('DOMContentLoaded', () => loadSalesData('monthly'));

        // Keep the blockchain section in view after any async postback in its UpdatePanel
        if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            var prm = Sys.WebForms.PageRequestManager.getInstance();
            prm.add_endRequest(function () {
                var target = document.getElementById('blockchainSection');
                if (target) target.scrollIntoView({ behavior: 'instant', block: 'start' });
            });
        }
    </script>
</asp:Content>
