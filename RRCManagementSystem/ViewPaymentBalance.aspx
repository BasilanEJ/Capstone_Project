<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ViewPaymentBalance.aspx.cs" Inherits="RRCManagementSystem.ViewPaymentBalance" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" />
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<style>
    body { font-family: 'Segoe UI', sans-serif; }
    .page-title { text-align:center; color:#1e293b; font-size:26px; font-weight:600; margin:30px 0 20px; }
    .filter-form { 
        display:flex; 
        justify-content:center; 
        gap:15px; 
        margin-bottom:25px; 
        flex-wrap:wrap;
        max-width: 600px;
        margin-left: auto;
        margin-right: auto;
    }
    .search-wrapper {
        position: relative;
        flex: 1;
        min-width: 300px;
    }
    .search-icon {
        position: absolute;
        left: 12px;
        top: 50%;
        transform: translateY(-50%);
        color: #94a3b8;
        pointer-events: none;
    }
    .filter-form input[type="text"]{
        padding: 10px 10px 10px 38px;
        border: 1px solid #cbd5e1;
        border-radius: 8px;
        font-size: 14px;
        width: 100%;
        transition: all 0.2s ease;
    }
    .filter-form input[type="text"]:focus {
        outline: none;
        border-color: #2563eb;
        box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
    }
    .search-hint {
        text-align: center;
        font-size: 12px;
        color: #64748b;
        margin-top: -15px;
        margin-bottom: 20px;
    }
    .message-label { text-align:center; color:#ef4444; font-weight:500; margin-bottom:15px; }
    .grid-container { width:95%; margin:0 auto 50px auto; background:#fff; border-radius:10px; box-shadow:0 4px 12px rgba(0,0,0,.05); overflow-x:auto; }
    .table { width:100%; border-collapse:collapse; }
    .table th { background:#1e3a8a; color:#fff; padding:12px; text-align:center; font-weight:600; }
    .table td { padding:10px; text-align:center; font-size:14px; color:#374151; border:1px solid #e5e7eb; }
    .table tr:nth-child(even){ background:#f9fafb; }
    .table tr:hover{ background:#f1f5f9; }
    
    /* Red highlighting for 3+ months overdue */
    .overdue-row {
        background-color: #fee2e2 !important;
    }
    .overdue-row:hover {
        background-color: #fecaca !important;
    }
    .overdue-badge {
        display: inline-block;
        background: #dc2626;
        color: white;
        padding: 2px 8px;
        border-radius: 4px;
        font-size: 11px;
        font-weight: 600;
        margin-left: 8px;
    }
    
    .no-results-message {
        padding: 20px;
        text-align: center;
        color: #64748b;
        background: #fef3c7;
        border-radius: 8px;
        margin: 10px;
        font-size: 14px;
    }
</style>

<h2 class="page-title">💳 View Payment Balances</h2>

<div class="filter-form">
    <div class="search-wrapper">
        <i class="fas fa-search search-icon"></i>
        <asp:TextBox ID="txtClientName" runat="server" 
            placeholder="Search by client name..." 
            onkeyup="filterTable()" />
    </div>
</div>

<p class="search-hint">
    <i class="fas fa-info-circle"></i> Start typing to filter results automatically • Red rows indicate payment overdue
</p>

<asp:Label ID="lblMessage" runat="server" CssClass="message-label" />

<div class="grid-container">
    <asp:GridView ID="gvBalances" runat="server" AutoGenerateColumns="False"
                  CssClass="table table-striped table-bordered" GridLines="None"
                  EmptyDataText="No balances found."
                  OnRowDataBound="gvBalances_RowDataBound">
        <RowStyle CssClass="balance-row" />
        <Columns>
            <asp:BoundField DataField="ClientID" HeaderText="Client ID" Visible="false" />
               <asp:BoundField DataField="ClientNumber" HeaderText="Client Number" ReadOnly="True" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm font-semibold text-blue-600 border-r border-gray-200" />
            <asp:BoundField DataField="ClientName" HeaderText="Client Name" 
                ItemStyle-CssClass="client-name" />
            <asp:BoundField DataField="PaymentPlan" HeaderText="Payment Plan" Visible ="false" />
            <asp:BoundField DataField="TotalAmount" HeaderText="Total Amount" 
                DataFormatString="₱{0:N2}" HtmlEncode="false" />
            <asp:BoundField DataField="AlreadyPaid" HeaderText="Already Paid" 
                DataFormatString="₱{0:N2}" HtmlEncode="false" />
            <asp:BoundField DataField="RemainingBalance" HeaderText="Remaining Balance" 
                DataFormatString="₱{0:N2}" HtmlEncode="false" />
            <asp:BoundField DataField="OldestBookingDate" HeaderText="Oldest Booking" 
                DataFormatString="{0:MMM dd, yyyy}" HtmlEncode="false" />
            <asp:BoundField DataField="MonthsOverdue" HeaderText="Months" 
                ItemStyle-CssClass="months-overdue" />
            <asp:BoundField DataField="IsOverdue" HeaderText="IsOverdue" Visible="false" />
        </Columns>
    </asp:GridView>
</div>

<script type="text/javascript">
    function filterTable() {
        var input = document.getElementById('<%= txtClientName.ClientID %>');
        var filter = input.value.toLowerCase().trim();
        var rows = document.querySelectorAll('.balance-row');
        var visibleCount = 0;

        rows.forEach(function(row) {
            var clientName = row.querySelector('.client-name');

            if (clientName) {
                var name = clientName.textContent.toLowerCase();

                if (name.includes(filter)) {
                    row.style.display = '';
                    visibleCount++;
                } else {
                    row.style.display = 'none';
                }
            }
        });

        // Show message if no results
        var gridContainer = document.querySelector('.grid-container');
        var noResultsMsg = document.getElementById('noResultsMessage');
        
        if (visibleCount === 0 && filter !== '') {
            if (!noResultsMsg) {
                noResultsMsg = document.createElement('div');
                noResultsMsg.id = 'noResultsMessage';
                noResultsMsg.className = 'no-results-message';
                noResultsMsg.innerHTML = '<i class="fas fa-search mr-2"></i>No payment balances found matching your search.';
                gridContainer.appendChild(noResultsMsg);
            }
            noResultsMsg.style.display = 'block';
        } else if (noResultsMsg) {
            noResultsMsg.style.display = 'none';
        }
    }

    // Clear search on page load if needed
    window.addEventListener('load', function() {
        var searchBox = document.getElementById('<%= txtClientName.ClientID %>');
        if (searchBox && searchBox.value === '') {
            filterTable();
        }
    });
</script>
</asp:Content>