<%@ Page Title="" Language="C#" MasterPageFile="~/Client.master" Async="true" AutoEventWireup="true" CodeBehind="Payment.aspx.cs" Inherits="RRCManagementSystem.Payment" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .payment-section {
            margin-top: 50px;
        }
        .payment-info p {
            font-size: 16px;
            margin-bottom: 10px;
        }
        .message-label {
            margin-top: 20px;
            font-weight: bold;
        }
        .history-table {
            margin-top: 30px;
            width: 100%;
            border-collapse: collapse;
        }
        .history-table th, .history-table td {
            padding: 10px;
            text-align: center;
            border-bottom: 1px solid #ddd;
        }
        .history-table th {
            background-color: #004085;
            color: white;
        }
        .btn-paymongo {
            margin-top: 20px;
            background-color: #28a745;
            color: white;
            padding: 10px 20px;
            border: none;
            font-size: 16px;
            cursor: pointer;
            border-radius: 5px;
        }
        .btn-paymongo:hover {
            background-color: #218838;
        }
    </style>

   <script type="text/javascript">
       function initiatePayMongoPayment() {
           var checkoutUrl = document.getElementById('<%= hiddenCheckoutURL.ClientID %>').value;
           if (checkoutUrl) {
               window.open(checkoutUrl, '_blank');
           } else {
               alert("Unable to load payment link. Please try again.");
           }
       }

       document.addEventListener("DOMContentLoaded", function () {
           const amount = document.getElementById('<%= hfPayPalAmount.ClientID %>').value;
        if (!amount || parseFloat(amount) <= 0) {
            document.getElementById('paypal-button-container').innerHTML =
                "<p style='color: red;'>✅ You have fully paid your balance.</p>";
            return;
        }

        paypal.Buttons({
            createOrder: function (data, actions) {
                const bookingId = document.getElementById('<%= hfPayPalBookingID.ClientID %>').value;
                return actions.order.create({
                    purchase_units: [{
                        amount: {
                            value: amount
                        },
                        custom_id: bookingId
                    }]
                });
            },
            onApprove: function (data, actions) {
                return actions.order.capture().then(function (details) {
                    const bookingId = document.getElementById('<%= hfPayPalBookingID.ClientID %>').value;
                    const clientId = document.getElementById('<%= hfPayPalClientID.ClientID %>').value;

                    fetch(`/PayPalWebhook.ashx?custom=${bookingId}&amount=${amount}&client=${clientId}`)
                        .then(res => res.text())
                        .then(msg => {
                            Swal.fire({
                                icon: 'success',
                                title: 'Payment completed!',
                                html: `Transaction by ${details.payer.name.given_name}<br/><small>${msg}</small>`
                            }).then(() => location.reload());
                        })
                        .catch(err => {
                            Swal.fire('✅ Paid, but DB not updated.', err.message, 'warning');
                        });
                });
            },
            onCancel: function (data) {
                Swal.fire('Payment canceled', '', 'info');
            },
            onError: function (err) {
                Swal.fire('Payment error', err.message, 'error');
            }
        }).render('#paypal-button-container');
    });
</script>




    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <script src="https://www.paypal.com/sdk/js?client-id=AXUxUohfga-5TSDyZurxJ07QF4gdpG4uxPWGSn6rqc8Gt3lQSPiYLJyKDGdqOYjhZgRw9vQMWpqHG1Fj&currency=PHP"></script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container payment-section">
        <h2>💳 Account Balance Overview</h2>

        <div class="payment-info">
            <p><strong>Service Name:</strong> <asp:Label ID="lblServiceName" runat="server" /></p>
            <p><strong>Current Balance:</strong> <asp:Label ID="lblPrice" runat="server" /></p>
            <p><strong>Payment Plan:</strong><br />
                <asp:Label ID="lblPaymentPlan" runat="server" />
            </p>
        </div>

        <asp:Label ID="lblMessage" runat="server" CssClass="message-label" ForeColor="Red" />

        <asp:Button ID="btnPayNow" runat="server" CssClass="btn-paymongo" Text="Pay Now via PayMongo"
            OnClientClick="initiatePayMongoPayment(); return false;" />

<asp:Button ID="btnPayWithPayPal" runat="server" Text="Pay Now via PayPal" CssClass="btn-paymongo"
    OnClick="btnPayWithPayPal_Click" />

<!-- Show warning if no balance -->
<div id="paypal-warning" style="color: red; display: none; font-weight: bold; margin-top: 20px;">
    ✅ You have no remaining balance to pay.
</div>

<!-- PayPal button renders here -->
<div id="paypal-button-container" style="margin-top: 20px;"></div>

<!-- Hidden fields -->
<asp:HiddenField ID="hfPayPalBookingID" runat="server" />
<asp:HiddenField ID="hfPayPalAmount" runat="server" />
<asp:HiddenField ID="hfPayPalClientID" runat="server" />
<asp:HiddenField ID="hiddenCheckoutURL" runat="server" />
<asp:HiddenField ID="hiddenReference" runat="server" />

        <p><strong>After completing your payment</strong>, click below:</p>
        <asp:Button ID="btnConfirmPayment" runat="server" Text="✅ I Have Paid" CssClass="btn-paymongo"
            OnClick="btnConfirmPayment_Click" />

        <h3 style="margin-top:40px;">📜 Payment History</h3>
        <asp:GridView ID="gvPaymentHistory" runat="server" AutoGenerateColumns="False" CssClass="table table-bordered">
            <Columns>
                <asp:BoundField DataField="TransactionDate" HeaderText="Transaction Date" DataFormatString="{0:yyyy-MM-dd}" />
                <asp:BoundField DataField="Amount" HeaderText="Amount" DataFormatString="₱{0:N2}" />
                <asp:BoundField DataField="PaymentMethod" HeaderText="Payment Method" />
                <asp:BoundField DataField="Remarks" HeaderText="Remarks" />
                <asp:TemplateField HeaderText="Receipt">
                    <ItemTemplate>
                        <%# Eval("Receipt") != DBNull.Value && Eval("Receipt") != null && Eval("Receipt").ToString() != "" ?
                            "<a href='DecryptReceipt.aspx?path=" + Eval("Receipt").ToString().Replace("~/", "") + "' target='_blank'>View Receipt</a>" :
                            "No Receipt Uploaded" %>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>


</asp:Content>
