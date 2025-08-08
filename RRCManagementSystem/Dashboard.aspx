<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="RRCManagementSystem.Dashboard" %>
<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Bootstrap 5 CSS -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <!-- Internal CSS -->
    <style>
        .dashboard-container, .sales-overview, .blockchain-transparency {
            padding: 30px 20px;
        }

        .container {
            max-width: 1200px;
            margin: 0 auto;
        }

        .page-title {
            font-size: 28px;
            color: #333;
            text-align: center;
            margin-bottom: 10px;
        }

        .welcome-message {
            text-align: center;
            font-size: 20px;
            color: #4b5563;
            margin-bottom: 30px;
        }

        .metrics-container {
            display: flex;
            flex-wrap: wrap;
            justify-content: center;
            gap: 20px;
        }

        .metric-card {
            background-color: #ffffff;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            width: 250px;
            text-align: center;
            padding: 20px;
            transition: transform 0.3s ease, box-shadow 0.3s ease;
        }

        .metric-card:hover {
            transform: translateY(-5px);
            box-shadow: 0 5px 15px rgba(0,0,0,0.2);
        }

        .metric-card h4 {
            font-size: 18px;
            color: #555;
            margin-bottom: 10px;
        }

        .metric-value {
            font-size: 30px;
            font-weight: bold;
            color: #007bff;
        }

        .btn-sales {
            background-color: #007bff;
            color: white;
            border: none;
            padding: 10px 20px;
            border-radius: 4px;
            cursor: pointer;
            font-weight: bold;
            transition: background-color 0.3s;
        }

        .btn-sales:hover {
            background-color: #0056b3;
        }

        .custom-table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
        }

        .custom-table th, .custom-table td {
            padding: 12px 15px;
            border: 1px solid #ddd;
            text-align: center;
        }

        .custom-table th {
            background-color: #007bff;
            color: #fff;
        }

        .custom-table tbody tr:nth-child(even) {
            background-color: #f9f9f9;
        }

        .custom-table tbody tr:hover {
            background-color: #f1f1f1;
        }

   .weekly-calendar .card {
    min-height: 100px;
    font-size: 15px;
    border: none;
    border-radius: 10px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
    overflow: hidden;
    background-color: #ffffff;
}

.weekly-calendar .card-header {
    font-size: 18px;
    font-weight: 600;
    background-color: #0d6efd;
    color: #ffffff;
    padding: 12px 20px;
    text-align: center;
    border-bottom: 1px solid #ccc;
}

.weekly-calendar .table {
    margin-bottom: 0;
}

.weekly-calendar .table td,
.weekly-calendar .table th {
    width: 14.28%; /* 100% / 7 days = ~14.28% */
    min-width: 160px;
    height: 140px;
    vertical-align: top;
    padding: 10px;
    background-color: #f9f9f9;
    font-size: 14px;
    border: 1px solid #dee2e6;
    cursor: pointer;
}


.weekly-calendar .table td:hover {
    background-color: #e8f0fe;
    transition: background-color 0.2s ease-in-out;
}

    </style>

 <!-- Admin Dashboard Title -->
<section class="dashboard-container">
    <div class="container">
        <h2 class="page-title">Admin Dashboard</h2>

        <!-- Welcome Message -->
        <div class="welcome-message">
            <asp:Label ID="lblWelcome" runat="server" Font-Bold="true"></asp:Label>
        </div>

        <!-- Dashboard Metrics -->
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
            <div class="card-header">
                📅 Weekly Booking Calendar
            </div>
            <div class="table-responsive">
                <asp:Table ID="tblCalendar" runat="server" CssClass="table table-bordered text-center">
                </asp:Table>
            </div>
        </div>
    </div>
</section>

   <!-- Modal -->
<div class="modal fade" id="bookingDetailsModal" tabindex="-1" aria-labelledby="bookingDetailsLabel" aria-hidden="true">
  <div class="modal-dialog modal-dialog-centered">
    <div class="modal-content">
      <div class="modal-header">
        <h5 class="modal-title" id="bookingDetailsLabel">Booking Details</h5>
        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
      </div>
      <div class="modal-body" id="bookingDetailsContent">
        <!-- Injected content here -->
      </div>
    </div>
  </div>
</div>

<script>
    function showBookingDetails(details) {
        document.getElementById('bookingDetailsContent').innerHTML = details;
        var modal = new bootstrap.Modal(document.getElementById('bookingDetailsModal'));
        modal.show();
    }
</script>




    <!-- Sales Overview Section -->
    <section class="sales-overview">
        <div class="container">
            <hr />
            <h3 class="section-title">Sales Overview</h3>

            <div class="sales-controls" style="text-align:center; margin-bottom:20px;">
                <button onclick="loadSalesData('daily')" class="btn-sales">Daily</button>
                <button onclick="loadSalesData('weekly')" class="btn-sales">Weekly</button>
                <button onclick="loadSalesData('monthly')" class="btn-sales">Monthly</button>
                <button onclick="loadSalesData('yearly')" class="btn-sales">Yearly</button>
            </div>

            <div class="chart-container">
                <canvas id="salesChart" style="height: 400px;"></canvas>
            </div>
        </div>
    </section>

    <!-- Blockchain Sales Transparency Section -->
    <section class="blockchain-transparency">
        <div class="container">
            <hr />
            <h3 class="section-title">Blockchain Sales Transparency</h3>

            <!-- Filter + Verify Blockchain -->
            <div style="display:flex; justify-content:center; align-items:center; gap:10px; margin-bottom:20px;">
                <asp:TextBox ID="txtFromDate" runat="server" TextMode="Date" CssClass="form-control" Style="max-width:180px;" />
                <asp:TextBox ID="txtToDate" runat="server" TextMode="Date" CssClass="form-control" Style="max-width:180px;" />
                <asp:Button ID="btnFilterBlockchain" runat="server" Text="📅 Filter" CssClass="btn-sales" OnClick="btnFilterBlockchain_Click" />
                <asp:Button ID="btnVerifyBlockchain" runat="server" Text="🔍 Verify Blockchain" CssClass="btn-sales" OnClick="btnVerifyBlockchain_Click" />
            </div>

            <div style="text-align:center; margin-bottom:30px;">
                <asp:Label ID="lblVerificationResult" runat="server" Style="font-size:18px; font-weight:bold;"></asp:Label>
            </div>

            <!-- Blockchain Logs Table -->
            <div class="table-responsive">
                <table class="custom-table">
                    <thead>
                        <tr>
                            <th>Log ID</th>
                            <th>Transaction ID</th>
                            <th>Sale Hash</th>
                            <th>Timestamp</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptBlockchainLog" runat="server" OnItemCommand="rptBlockchainLog_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td><%# Eval("LogID") %></td>
                                    <td>
                                        <asp:LinkButton ID="lnkTransactionID" runat="server" Text='<%# Eval("TransactionID") %>' CommandName="ViewJson" CommandArgument='<%# Eval("TransactionID") %>' CssClass="btn btn-link" />
                                    </td>
                                    <td><%# Eval("SaleHash") %></td>
                                    <td><%# Eval("Timestamp") %></td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>
        </div>
    </section>

    <!-- SaleDataJson Modal -->
    <div id="jsonModal" style="display:none; position:fixed; top:20%; left:50%; transform:translate(-50%,0); background:white; padding:20px; box-shadow:0 0 15px rgba(0,0,0,0.3); z-index:9999; max-width:600px;">
        <h3>📄 Sale Data Details</h3>
        <pre id="jsonContent" style="white-space:pre-wrap; font-size:14px;"></pre>
        <br />
        <button onclick="closeModal()" class="btn-sales">Close</button>
    </div>



    <script>
        function openModal(content) {
            document.getElementById('jsonContent').innerText = content;
            document.getElementById('jsonModal').style.display = 'block';
        }

        function closeModal() {
            document.getElementById('jsonModal').style.display = 'none';
        }
    </script>

    <input type="hidden" id="salesDataJson" value='<%= salesDataJson %>' />

    <!-- Chart.js CDN -->
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>

    <script>
        var chart;

        function loadSalesData(type) {
            fetch('Dashboard.aspx?type=' + type)
                .then(response => response.json())
                .then(salesData => updateChart(salesData))
                .catch(error => console.error("Error loading sales data:", error));
        }

        function updateChart(salesData) {
            if (chart) chart.destroy();

            var ctx = document.getElementById("salesChart").getContext("2d");
            chart = new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: salesData.labels,
                    datasets: [{
                        label: 'Sales (₱)',
                        data: salesData.data,
                        backgroundColor: 'rgba(54, 162, 235, 0.6)',
                        borderColor: 'rgba(54, 162, 235, 1)',
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: {
                                callback: function (value) { return '₱' + value.toFixed(2); }
                            }
                        }
                    }
                }
            });
        }

        document.addEventListener("DOMContentLoaded", function () {
            loadSalesData('monthly');
        });
    </script>

</asp:Content>