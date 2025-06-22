<%@ Page Title="" Language="C#" MasterPageFile="~/Client.master" Async="true" AutoEventWireup="true" CodeBehind="Payment.aspx.cs" Inherits="RRCManagementSystem.Payment" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
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
                    "<p class='text-success fw-bold mt-3'>✅ You have no balance.</p>";
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
                onCancel: function () {
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
    <div class="container my-5">
        <div class="card shadow p-4">
            <h2 class="mb-4">💳 Account Balance Overview</h2>

            <div class="mb-3">
                <p class="mb-2"><strong>Service Name:</strong> <asp:Label ID="lblServiceName" runat="server" /></p>
                <p class="mb-2"><strong>Current Balance:</strong> <asp:Label ID="lblPrice" runat="server" /></p>
                <p><strong>Payment Plan:</strong><br />
                    <asp:Label ID="lblPaymentPlan" runat="server" />
                </p>
            </div>

            <asp:Label ID="lblMessage" runat="server" CssClass="fw-bold text-danger d-block mt-3" />
            <asp:Label ID="lblReminder" runat="server" CssClass="fw-bold text-warning d-block mt-2" />

            <div class="d-flex flex-wrap gap-3 mt-4">
                <asp:Button ID="btnPayNow" runat="server" CssClass="btn btn-success" Text="Pay Now via PayMongo"
                    OnClientClick="initiatePayMongoPayment(); return false;" />

                <asp:Button ID="btnPayWithPayPal" runat="server" Text="Pay Now via PayPal" CssClass="btn btn-success"
                    OnClick="btnPayWithPayPal_Click" />
            </div>

            <div id="paypal-warning" class="text-danger fw-bold mt-3 d-none">
                ✅ You have no remaining balance to pay.
            </div>

            <div id="paypal-button-container" class="mt-4"></div>

            <asp:HiddenField ID="hfPayPalBookingID" runat="server" />
            <asp:HiddenField ID="hfPayPalAmount" runat="server" />
            <asp:HiddenField ID="hfPayPalClientID" runat="server" />
            <asp:HiddenField ID="hiddenCheckoutURL" runat="server" />
            <asp:HiddenField ID="hiddenReference" runat="server" />

            <p class="mt-4 fw-bold">After completing your payment, click below:</p>
            <asp:Button ID="btnConfirmPayment" runat="server" Text="✅ I Have Paid" CssClass="btn btn-primary mt-2"
                OnClick="btnConfirmPayment_Click" />

            <h3 class="mt-5">📜 Payment History</h3>
            <asp:GridView ID="gvPaymentHistory" runat="server" AutoGenerateColumns="False" CssClass="table table-bordered mt-3">
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
    </div>
</asp:Content>