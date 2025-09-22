<%@ Page Title="Payment" Language="C#" MasterPageFile="~/Client.master" Async="true"
AutoEventWireup="true" CodeBehind="Payment.aspx.cs" Inherits="RRCManagementSystem.Payment" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
<!-- Tailwind CSS -->
<script src="https://cdn.tailwindcss.com"></script>
<!-- SweetAlert2 -->
<script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
<!-- PayPal SDK (PHP currency) -->
<script src="https://www.paypal.com/sdk/js?client-id=AXUxUohfga-5TSDyZurxJ07QF4gdpG4uxPWGSn6rqc8Gt3lQSPiYLJyKDGdqOYjhZgRw9vQMWpqHG1Fj&currency=PHP"></script>
<script>
    // ------- PayMongo button toggle & open -------
    function updatePayMongoButton() {
        var url = document.getElementById('<%= hiddenCheckoutURL.ClientID %>').value || '';
        var amountRaw = document.getElementById('<%= hfPayPalAmount.ClientID %>').value || '';
        var amt = parseFloat(String(amountRaw).replace(/,/g, ''));
        var btn = document.getElementById('<%= btnPayHere.ClientID %>');
        var note = document.getElementById('paymongo-note');
        var wrap = document.getElementById('paymongo-area');

        if (!wrap || !btn) return;

        if (!!url && !isNaN(amt) && amt > 0) {
            wrap.classList.remove('hidden');
            btn.disabled = false;
            note.classList.add('hidden');
        } else {
            btn.disabled = true;
            note.classList.remove('hidden');
        }
    }

    // ✅ Open PayMongo checkout in same tab
    function openPayMongoCheckout() {
        var url = document.getElementById('<%= hiddenCheckoutURL.ClientID %>').value || '';
        if (!url) {
            Swal.fire('Payment link not ready', 'Please wait a moment or change plan to refresh.', 'info');
            return false;
        }
        window.location.href = url; // Open in same tab
        return false;
    }

    // ------- PayPal Smart Buttons -------
    window.__ppLastAmount = null;
    window.__ppButtons = null;
    window.__ppHooked = false;

    function renderPayPalButtons() {
        var amountEl = document.getElementById('<%= hfPayPalAmount.ClientID %>');
    var container = document.getElementById('paypal-button-container');

    if (!container || !window.paypal) return;

    // ✅ Always clear before rendering
    container.innerHTML = "";
    if (window.__ppButtons && window.__ppButtons.close) {
        try { window.__ppButtons.close(); } catch (e) { }
    }
    window.__ppButtons = null;
    window.__ppLastAmount = null;

    var raw = amountEl ? (amountEl.value || "") : "";
    var amt = parseFloat(String(raw).replace(/,/g, ""));

    if (!amt || isNaN(amt) || amt <= 0) {
        container.innerHTML = "<p class='text-green-600 font-semibold mt-3 mb-0'>✅ You have no balance.</p>";
        return;
    }

    var amount = amt.toFixed(2);

    // Render PayPal buttons
    window.__ppButtons = paypal.Buttons({
        style: { layout: 'vertical', label: 'paypal' },

        createOrder: function (data, actions) {
            var minRequired = parseFloat(document.getElementById('<%= hfMinRequired.ClientID %>').value) || 0;
            var customValue = document.getElementById('<%= txtCustomAmount.ClientID %>').value.trim();
            var customAmount = customValue === "" ? amt : parseFloat(customValue);

            if (isNaN(customAmount) || customAmount <= 0) {
                Swal.fire('Invalid Amount', 'Please enter a valid payment amount.', 'error');
                return false;
            }

            if (customAmount < minRequired) {
                Swal.fire('Invalid Amount', 'Entered amount must be at least ₱' + minRequired.toFixed(2), 'error');
                return false;
            }

            window.__ppFinalAmount = customAmount;

            return actions.order.create({
                purchase_units: [{
                    amount: { value: customAmount.toFixed(2) },
                    custom_id: document.getElementById('<%= hfPayPalBookingID.ClientID %>').value
                }]
            });
        },

        onApprove: function (data, actions) {
            return actions.order.capture().then(function (details) {
                var bookingId = document.getElementById('<%= hfPayPalBookingID.ClientID %>').value;
                var clientId = document.getElementById('<%= hfPayPalClientID.ClientID %>').value;

                fetch('/PayPalWebhook.ashx?custom=' + encodeURIComponent(bookingId) +
                    '&amount=' + encodeURIComponent(window.__ppFinalAmount) +
                    '&client=' + encodeURIComponent(clientId))
                    .then(r => r.text())
                    .then(msg => Swal.fire({
                        icon: 'success',
                        title: 'Payment completed!',
                        html: 'Transaction by ' + (details?.payer?.name?.given_name || 'payer') +
                            '<br/><small>' + msg + '</small>'
                    }).then(() => location.reload()))
                    .catch(err => Swal.fire('✅ Paid, but DB not updated.', err.message, 'warning'));
            });
        },

        onCancel: function () {
            Swal.fire('Payment canceled', '', 'info');
        },

        onError: function (err) {
            console.error("PayPal Error:", err);
            Swal.fire('Payment error', err.message, 'error');
        }
    });

        // ✅ Finally render the button
        window.__ppButtons.render('#paypal-button-container').finally(function () {
            container.classList.remove('opacity-40');
        });
    }


    // ------- Hook for UpdatePanel Refresh -------
    function hookUpdatePanelVisuals() {
        if (!(window.Sys && Sys.WebForms) || window.__ppHooked) return;
        window.__ppHooked = true;

        var prm = Sys.WebForms.PageRequestManager.getInstance();

        prm.add_beginRequest(function () {
            var panel = document.getElementById('paymentPanelBody');
            if (panel) panel.classList.add('opacity-50');
        });

        prm.add_endRequest(function () {
            var panel = document.getElementById('paymentPanelBody');
            if (panel) panel.classList.remove('opacity-50');

            updatePayMongoButton();
            renderPayPalButtons();
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        hookUpdatePanelVisuals();
        updatePayMongoButton();
        renderPayPalButtons();
    });

    // ------- Validation for Custom Amount -------
    function validateCustomAmount() {
        var minRequired = parseFloat(document.getElementById('<%= hfMinRequired.ClientID %>').value) || 0;
      var inputEl = document.getElementById('<%= txtCustomAmount.ClientID %>');
    var errorLabel = document.getElementById('<%= lblCustomAmountError.ClientID %>');
        var payMongoBtn = document.getElementById('<%= btnPayHere.ClientID %>');
        var paypalContainer = document.getElementById('paypal-button-container');

        var inputValue = inputEl.value.trim();
        var input = inputValue === "" ? 0 : parseFloat(inputValue);

        console.log("hfMinRequired:", minRequired, "Entered:", input);

        // === CASE 1: Empty textbox → revert to default system value ===
        if (inputValue === "") {
            errorLabel.textContent = "";
            inputEl.classList.remove("border-red-500");

            if (payMongoBtn) payMongoBtn.disabled = false;

            // ✅ Enable PayPal again
            if (paypalContainer) {
                paypalContainer.classList.remove("pointer-events-none", "opacity-50");
                paypalContainer.innerHTML = "";
                window.__ppLastAmount = null; // Force PayPal to re-render
            }

            renderPayPalButtons();
            return true;
        }

        // === CASE 2: Amount BELOW minimum ===
        if (input < minRequired) {
            errorLabel.textContent = "Amount cannot be less than ₱" + minRequired.toFixed(2);
            inputEl.classList.add("border-red-500");

            // Disable PayMongo button
            if (payMongoBtn) payMongoBtn.disabled = true;

            // Disable PayPal buttons
            if (paypalContainer) {
                paypalContainer.innerHTML =
                    "<p class='text-red-600 font-semibold mt-3'>Enter at least ₱" +
                    minRequired.toFixed(2) + " to enable PayPal.</p>";

                // Faded style + no click events
                paypalContainer.classList.add("pointer-events-none", "opacity-50");
            }

            return false;
        }

        // === CASE 3: Amount is VALID (>= minimum) ===
        errorLabel.textContent = "";
        inputEl.classList.remove("border-red-500");

        // Enable PayMongo again
        if (payMongoBtn) payMongoBtn.disabled = false;

        // Enable PayPal buttons
        if (paypalContainer) {
            paypalContainer.classList.remove("pointer-events-none", "opacity-50");
            paypalContainer.innerHTML = "";
            window.__ppLastAmount = null; // Force PayPal to render fresh
        }

        renderPayPalButtons();
        return true;
    }


</script>


</asp:Content>

<asp:Content ID="MainContentBlock" ContentPlaceHolderID="MainContent" runat="server">
<asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />

<div class="max-w-6xl mx-auto px-4 py-8">
    <div class="bg-white shadow-lg rounded-xl p-6">
        <h2 class="text-2xl font-bold text-blue-600 flex items-center gap-2 mb-6">
            💳 Account Balance Overview
        </h2>

        <asp:UpdatePanel ID="updPaymentDetails" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div id="paymentPanelBody">

                    <!-- Service Info -->
                    <div class="mb-4">
                        <p class="text-gray-800"><strong>Service Name:</strong> <asp:Label ID="lblServiceName" runat="server" CssClass="ml-1" /></p>
                        <p class="text-gray-500"><strong>Payment Plan:</strong> <asp:Label ID="lblPaymentPlan" runat="server" CssClass="ml-1" /></p>
                    </div>

                    <!-- KPI Cards -->
                    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
                        <div class="border rounded-lg p-4 text-center bg-gray-50">
                            <h4 class="text-gray-500 text-sm font-semibold">Next Installment</h4>
                            <div class="text-xl font-bold text-gray-900"><asp:Literal ID="lblNextInstallment" runat="server" /></div>
                            <div class="text-xs text-gray-500 mt-1"><asp:Literal ID="litNextDue" runat="server" /></div>
                        </div>
                        <div class="border rounded-lg p-4 text-center bg-gray-50">
                            <h4 class="text-gray-500 text-sm font-semibold">Total Price</h4>
                            <div class="text-xl font-bold text-gray-900"><asp:Literal ID="lblTotalPrice" runat="server" /></div>
                            <div class="text-xs text-gray-500 mt-1">Full contract amount</div>
                        </div>
                        <div class="border rounded-lg p-4 text-center bg-gray-50">
                            <h4 class="text-gray-500 text-sm font-semibold">Already Paid</h4>
                            <div class="text-xl font-bold text-gray-900"><asp:Literal ID="lblAlreadyPaid" runat="server" /></div>
                            <div class="text-xs text-gray-500 mt-1">Confirmed payments</div>
                        </div>
                        <div class="border rounded-lg p-4 text-center bg-gray-50">
                            <h4 class="text-gray-500 text-sm font-semibold">Remaining Balance</h4>
                            <div class="text-xl font-bold text-gray-900"><asp:Literal ID="lblRemaining" runat="server" /></div>
                            <div class="text-xs text-gray-500 mt-1">After this installment</div>
                        </div>
                    </div>

                    <!-- Pricing Breakdown -->
                    <div class="mt-8 p-4 bg-white rounded-lg">
                        <h3 class="text-lg font-semibold text-gray-700 mb-4">Pricing Breakdown</h3>
                        <div class="mb-3">
                            <span class="block text-gray-700 font-medium">Base Service Price (Based on SQM):</span>
                            <asp:Label ID="lblBasePrice" runat="server" CssClass="block text-gray-600 text-base" />
                        </div>
                        <div class="mb-3">
                            <span class="block text-gray-700 font-medium">Travel Expense:</span>
                            <asp:Label ID="lblTravelExpense" runat="server" CssClass="block text-gray-600 text-base" />
                        </div>
                        <div class="mb-3">
                            <span class="block text-gray-700 font-medium">Miscellaneous:</span>
                            <asp:Label ID="lblMiscellaneous" runat="server" CssClass="block text-gray-600 text-base" />
                        </div>
                        <div class="mt-4 pt-4 border-t border-gray-300">
                            <span class="block text-gray-700 font-bold">Total Price:</span>
                            <asp:Label ID="Label1" runat="server" CssClass="block text-blue-600 text-lg font-bold" />
                        </div>
                    </div>

                    <!-- Payment Plan Dropdown -->
                    <asp:Panel ID="paymentPlanContainer" runat="server" Visible="true" CssClass="mb-6">
                        <label for="ddlPlanChoice" class="block text-sm font-medium text-gray-700 mb-1">
                            Preferred Payment Plan
                        </label>
                        <asp:DropDownList
                            ID="ddlPlanChoice"
                            runat="server"
                            CssClass="border border-gray-300 rounded-lg px-3 py-2 w-full sm:w-1/3"
                            AutoPostBack="true"
                            OnSelectedIndexChanged="ddlPlanChoice_SelectedIndexChanged">
                            <asp:ListItem Text="50% / 25% / 25%" Value="50-25-25" />
                            <asp:ListItem Text="70% / 30%" Value="70-30" />
                            <asp:ListItem Text="100% Full Payment" Value="100" />
                        </asp:DropDownList>
                    </asp:Panel>

                    <!-- Hidden Fields for Amounts -->
                    <asp:HiddenField ID="hfMinRequired" runat="server" />
                    <%-- The hfCustomAmount field is no longer needed. --%>
                    <asp:HiddenField ID="hfSelectedPlan" runat="server" />
                    <asp:HiddenField ID="hfPayPalBookingID" runat="server" />
                    <asp:HiddenField ID="hfPayPalAmount" runat="server" />
                    <asp:HiddenField ID="hfPayPalClientID" runat="server" />
                    <asp:HiddenField ID="hiddenCheckoutURL" runat="server" />
                    <asp:HiddenField ID="hiddenReference" runat="server" />

                    <!-- Custom Payment Input -->
                    <div class="mt-4">
                        <label for="txtCustomAmount" class="block text-gray-700 font-medium">Enter Payment Amount</label>
                    <asp:TextBox
    ID="txtCustomAmount"
    runat="server"
    AutoPostBack="true"
    OnTextChanged="txtCustomAmount_TextChanged"
    CssClass="mt-1 block w-full border border-gray-300 rounded-md shadow-sm p-2 focus:border-blue-500 focus:ring-blue-500 outline-none"
    placeholder="Enter amount in PHP"
    oninput="validateCustomAmount();" />




                        <small id="customAmountNote" class="text-gray-500 block mt-1">
                            Minimum required: <span id="minRequiredAmount">₱0.00</span>
                        </small>
                        <asp:Label ID="lblCustomAmountError" runat="server" CssClass="text-red-600 text-sm mt-1 block" />
                    </div>

                    <asp:Label ID="lblMessage" runat="server" CssClass="text-red-600 font-medium block mt-2" />
                    <asp:Label ID="lblReminder" runat="server" CssClass="text-yellow-600 font-medium block mt-2" />

                </div>

            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="ddlPlanChoice" EventName="SelectedIndexChanged" />
               <asp:AsyncPostBackTrigger ControlID="txtCustomAmount" EventName="TextChanged" />
            </Triggers>

        </asp:UpdatePanel>

        <!-- Payment Methods -->
        <div class="space-y-4 mt-6">
            <!-- PayMongo -->
            <div id="paymongo-area" class="hidden">
                <div class="flex flex-wrap items-center gap-3">
                    <asp:Button
                        ID="btnPayHere"
                        runat="server"
                        Text="Pay Here"
                        CssClass="bg-green-600 hover:bg-green-700 text-white px-5 py-2 rounded disabled:opacity-50"
                        OnClientClick="return openPayMongoCheckout();"
                        CausesValidation="false" />

                    <span id="paymongo-note" class="text-gray-500">Waiting for checkout link...</span>
                    <span class="text-xs bg-gray-100 text-gray-600 px-2 py-1 rounded">Card · GCash · GrabPay · Maya</span>
                </div>
            </div>

            <!-- PayPal -->
            <asp:UpdatePanel ID="updPayPal" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <div id="paypalArea" class="pt-2">
                        <div id="paypal-warning" class="hidden text-green-600 font-semibold">✅ You have no remaining balance to pay.</div>
                        <div id="paypal-button-container" class="mt-2"></div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>

        <!-- Payment History -->
        <h3 class="text-xl font-bold text-blue-600 flex items-center gap-2 mt-8 mb-4">
            📜 Payment History
        </h3>
        <div class="overflow-x-auto w-full">
            <asp:GridView ID="gvPaymentHistory" runat="server"
                AutoGenerateColumns="False"
                CssClass="w-full text-sm text-left table-auto border-collapse"
                GridLines="None"
                OnRowDataBound="gvPaymentHistory_RowDataBound">
                <HeaderStyle CssClass="bg-gray-200 text-gray-700 font-semibold uppercase tracking-wider" />
                <RowStyle CssClass="bg-white border-t border-gray-200" />
                <Columns>
                    <asp:BoundField DataField="TransactionDate" HeaderText="Date" DataFormatString="{0:yyyy-MM-dd}" NullDisplayText="—" HeaderStyle-CssClass="px-4 py-2 border border-gray-300" ItemStyle-CssClass="px-4 py-2 border border-gray-300 whitespace-nowrap" />
                    <asp:BoundField DataField="Amount" HeaderText="Amount" DataFormatString="₱{0:N2}" NullDisplayText="₱0.00" HeaderStyle-CssClass="px-4 py-2 border border-gray-300" ItemStyle-CssClass="px-4 py-2 border border-gray-300 whitespace-nowrap" />
                    <asp:BoundField DataField="PaymentMethod" HeaderText="Method" NullDisplayText="—" HeaderStyle-CssClass="px-4 py-2 border border-gray-300" ItemStyle-CssClass="px-4 py-2 border border-gray-300 whitespace-nowrap" />
                    <asp:BoundField DataField="Remarks" HeaderText="Remarks" NullDisplayText="—" HeaderStyle-CssClass="px-4 py-2 border border-gray-300" ItemStyle-CssClass="px-4 py-2 border border-gray-300" />
                    <asp:TemplateField HeaderText="Receipt">
                        <HeaderStyle CssClass="px-4 py-2 border border-gray-300" />
                        <ItemStyle CssClass="px-4 py-2 border border-gray-300" />
                        <ItemTemplate>
                            <%# GetReceiptLink(Eval("Receipt")) %>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate>
                    <div class="p-3 text-gray-500">No payments recorded yet.</div>
                </EmptyDataTemplate>
            </asp:GridView>
        </div>
    </div>
</div>

</asp:Content>
