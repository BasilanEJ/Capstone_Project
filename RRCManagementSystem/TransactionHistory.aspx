<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="TransactionHistory.aspx.cs" Inherits="RRCManagementSystem.TransactionHistory" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<!-- Add SweetAlert2 -->
<script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

<style>
    body { font-family: 'Segoe UI', sans-serif; }
    .page-title { text-align:center; color:#1e293b; font-size:26px; font-weight:600; margin:30px 0 20px; }
    .filter-form { display:flex; justify-content:center; gap:15px; margin-bottom:25px; flex-wrap:wrap; }
    .filter-form input[type="date"], .filter-form select {
        padding:8px 10px; border:1px solid #cbd5e1; border-radius:6px; font-size:14px; min-width:160px;
    }
    .filter-form .btn-filter {
        padding:8px 18px; background-color:#2563eb; color:#fff; border:none; border-radius:6px; font-weight:600; cursor:pointer;
    }
    .filter-form .btn-filter:hover { background-color:#1d4ed8; }
    .message-label { text-align:center; color:#ef4444; font-weight:500; margin-bottom:15px; }
    .grid-container { width:95%; margin:0 auto 50px auto; background:#fff; border-radius:10px; box-shadow:0 4px 12px rgba(0,0,0,.05); overflow-x:auto; }
    .table { width:100%; border-collapse:collapse; }
    .table th { background:#1e3a8a; color:#fff; padding:12px; text-align:center; font-weight:600; }
    .table td { padding:10px; text-align:center; font-size:14px; color:#374151; border:1px solid #e5e7eb; }
    .table tr:nth-child(even){ background:#f9fafb; }
    .table tr:hover{ background:#f1f5f9; }

    /* Shared pill base */
    .pill {
        display:inline-block;
        border-radius:999px;
        padding:6px 12px;
        font-size:13px;
        font-weight:600;
        text-decoration:none;
        transition:all 0.2s ease;
        cursor:pointer;
    }

    /* View Receipt = Blue */
    .pill-view {
        background:#e0f2fe;
        color:#075985;
        border:1px solid #7dd3fc;
    }
    .pill-view:hover {
        background:#bae6fd;
        color:#0c4a6e;
    }

    /* Add Receipt = Green */
    .pill-add {
        background:#dcfce7;
        color:#166534;
        border:1px solid #86efac;
    }
    .pill-add:hover {
        background:#bbf7d0;
        color:#14532d;
    }

    .pill-add.disabled {
        background:#f0fdf4;
        color:#6b7280;
        border:1px solid #d1fae5;
        cursor:not-allowed;
    }

    /* Custom styles for SweetAlert receipt modal - Responsive */
    .swal2-popup.receipt-modal {
        width: 95% !important;
        max-width: 1200px !important;
        padding: 1.5rem !important;
    }
    
    .receipt-container {
        width: 100%;
        max-height: 75vh;
        overflow: auto;
        display: flex;
        justify-content: center;
        align-items: flex-start;
        background: #f3f4f6;
        border-radius: 8px;
        padding: 15px;
    }
    
    .receipt-container img {
        max-width: 100%;
        width: auto;
        height: auto;
        max-height: 70vh;
        object-fit: contain;
        box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        border-radius: 4px;
    }
    
    .receipt-container embed {
        width: 100%;
        height: 70vh;
        min-height: 500px;
        border: none;
        border-radius: 4px;
    }

    /* Tablet styles */
    @media (max-width: 1024px) {
        .swal2-popup.receipt-modal {
            width: 96% !important;
            padding: 1.2rem !important;
        }
        
        .receipt-container {
            max-height: 70vh;
            padding: 12px;
        }
        
        .receipt-container img {
            max-height: 65vh;
        }
        
        .receipt-container embed {
            height: 65vh;
            min-height: 400px;
        }
    }

    /* Mobile styles */
    @media (max-width: 768px) {
        .swal2-popup.receipt-modal {
            width: 98% !important;
            padding: 1rem !important;
            margin: 0.5rem !important;
        }
        
        .swal2-title {
            font-size: 1.3rem !important;
            padding: 0.5rem !important;
        }
        
        .receipt-container {
            max-height: 65vh;
            padding: 10px;
        }
        
        .receipt-container img {
            max-height: 60vh;
        }
        
        .receipt-container embed {
            height: 60vh;
            min-height: 350px;
        }
        
        .swal2-confirm {
            font-size: 0.9rem !important;
            padding: 0.6rem 1.5rem !important;
        }
    }

    /* Small mobile devices */
    @media (max-width: 480px) {
        .swal2-popup.receipt-modal {
            width: 98% !important;
            padding: 0.8rem !important;
            margin: 0.3rem !important;
        }
        
        .swal2-title {
            font-size: 1.1rem !important;
            padding: 0.4rem !important;
        }
        
        .receipt-container {
            max-height: 60vh;
            padding: 8px;
        }
        
        .receipt-container img {
            max-height: 55vh;
        }
        
        .receipt-container embed {
            height: 55vh;
            min-height: 300px;
        }
        
        .swal2-confirm {
            font-size: 0.85rem !important;
            padding: 0.5rem 1.2rem !important;
        }
        
        .swal2-close {
            font-size: 1.5rem !important;
        }
    }

    /* Landscape orientation on mobile */
    @media (max-width: 768px) and (orientation: landscape) {
        .receipt-container {
            max-height: 80vh;
        }
        
        .receipt-container img {
            max-height: 75vh;
        }
        
        .receipt-container embed {
            height: 75vh;
        }
    }

</style>

<h2 class="page-title">📄 Transaction History</h2>

<div class="filter-form">
    <asp:TextBox ID="txtFromDate" runat="server" TextMode="Date" />
    <asp:TextBox ID="txtToDate" runat="server" TextMode="Date" />
    <asp:DropDownList ID="ddlPaymentMethod" runat="server"></asp:DropDownList>
    <asp:Button ID="btnFilter" runat="server" Text="Filter" CssClass="btn-filter" OnClick="btnFilter_Click" />
</div>

<asp:Label ID="lblMessage" runat="server" CssClass="message-label" />

<div class="grid-container">
    <asp:GridView ID="gvTransactions" runat="server" AutoGenerateColumns="False"
                  CssClass="table table-striped table-bordered" GridLines="None"
                  EmptyDataText="No transaction records found."
                  OnRowDataBound="gvTransactions_RowDataBound">
        <Columns>
            <asp:BoundField DataField="TransactionID" HeaderText="Transaction ID" />
            <asp:BoundField DataField="SaleID" HeaderText="Sale ID" />
            <asp:BoundField DataField="Amount" HeaderText="Amount" DataFormatString="₱{0:N2}" HtmlEncode="false" />
            <asp:BoundField DataField="PaymentMethod" HeaderText="Payment Method" />
            <asp:BoundField DataField="TransactionDate" HeaderText="Transaction Date" DataFormatString="{0:yyyy-MM-dd HH:mm}" />

            <asp:TemplateField HeaderText="Receipt">
                <ItemTemplate>
                    <asp:Literal ID="litView" runat="server" Mode="PassThrough"
                                 Visible='<%# !string.IsNullOrWhiteSpace(Eval("Receipt") as string) %>'
                                 Text='<%# GetReceiptLink(Eval("Receipt")) %>' />

                    <asp:LinkButton ID="btnAddReceipt" runat="server"
                        CssClass='<%# (ViewState["CanAddReceipt"] != null && !(bool)ViewState["CanAddReceipt"]) ? "pill pill-add disabled" : "pill pill-add" %>'
                        CommandArgument='<%# Eval("TransactionID") %>'
                        Text="Add Receipt"
                        OnClick="btnAddReceipt_Click"
                        Enabled='<%# (ViewState["CanAddReceipt"] != null && (bool)ViewState["CanAddReceipt"]) %>'
                        Visible='<%# string.IsNullOrWhiteSpace(Eval("Receipt") as string) %>' />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>
</div>

<script>
    function viewReceipt(url, fileName) {
        // Determine file type
        const ext = fileName.toLowerCase().split('.').pop();
        const isPdf = ext === 'pdf';

        // Check if mobile device
        const isMobile = window.innerWidth <= 768;

        let htmlContent = '';

        if (isPdf) {
            htmlContent = `
                <div class="receipt-container">
                    <embed src="${url}" type="application/pdf" />
                </div>
            `;
        } else {
            htmlContent = `
                <div class="receipt-container">
                    <img src="${url}" alt="Receipt" loading="lazy" />
                </div>
            `;
        }

        Swal.fire({
            title: '📄 Receipt',
            html: htmlContent,
            width: isMobile ? '98%' : '90%',
            showCloseButton: true,
            showConfirmButton: true,
            confirmButtonText: isMobile ? 'Close' : 'Close Receipt',
            confirmButtonColor: '#2563eb',
            customClass: {
                popup: 'receipt-modal',
                confirmButton: 'swal2-confirm'
            },
            didOpen: () => {
                // Add pinch-to-zoom for mobile images
                if (!isPdf && isMobile) {
                    const img = document.querySelector('.receipt-container img');
                    if (img) {
                        img.style.touchAction = 'pinch-zoom';
                    }
                }
            }
        });

        return false;
    }

    // Handle orientation change
    window.addEventListener('orientationchange', function () {
        // Close modal on orientation change to prevent layout issues
        if (Swal.isVisible()) {
            Swal.close();
        }
    });
</script>

</asp:Content>