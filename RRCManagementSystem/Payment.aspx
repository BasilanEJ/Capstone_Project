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
    </script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
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