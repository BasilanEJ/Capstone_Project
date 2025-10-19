<%@ Page Title="Payment" Language="C#" MasterPageFile="~/Client.master" Async="true"
AutoEventWireup="true" CodeBehind="Payment.aspx.cs" Inherits="RRCManagementSystem.Payment" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
<!-- SweetAlert2 -->
<script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
<!-- PayPal SDK (PHP currency) -->
<script src="https://www.paypal.com/sdk/js?client-id=AXUxUohfga-5TSDyZurxJ07QF4gdpG4uxPWGSn6rqc8Gt3lQSPiYLJyKDGdqOYjhZgRw9vQMWpqHG1Fj&currency=PHP"></script>

<style>
    /* Card hover effects */
    .stat-card {
        transition: all 0.3s ease;
    }
    
    .stat-card:hover {
        transform: translateY(-4px);
        box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
    }

    /* Payment method cards */
    .payment-card {
        transition: all 0.3s ease;
        border: 2px solid transparent;
    }

    .payment-card:hover {
        border-color: #3b82f6;
        box-shadow: 0 4px 12px rgba(59, 130, 246, 0.2);
    }

    /* Input focus states */
    .custom-input:focus {
        border-color: #3b82f6;
        box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
    }

    /* Smooth transitions */
    .fade-in {
        animation: fadeIn 0.4s ease-in;
    }

    @keyframes fadeIn {
        from { opacity: 0; transform: translateY(10px); }
        to { opacity: 1; transform: translateY(0); }
    }

    /* GridView styling */
    .payment-grid {
        border-radius: 8px;
        overflow: hidden;
    }

    .payment-grid th {
        background: linear-gradient(135deg, #1e40af 0%, #3b82f6 100%);
        color: white;
        font-weight: 600;
        text-transform: uppercase;
        letter-spacing: 0.5px;
        font-size: 0.75rem;
    }

    .payment-grid tr:hover {
        background-color: #f8fafc;
    }

    /* Button animations */
    .btn-pay {
        transition: all 0.3s ease;
        position: relative;
        overflow: hidden;
    }

    .btn-pay:hover {
        transform: translateY(-2px);
        box-shadow: 0 6px 20px rgba(34, 197, 94, 0.3);
    }

    .btn-pay::before {
        content: '';
        position: absolute;
        top: 50%;
        left: 50%;
        width: 0;
        height: 0;
        border-radius: 50%;
        background: rgba(255, 255, 255, 0.2);
        transform: translate(-50%, -50%);
        transition: width 0.6s, height 0.6s;
    }

    .btn-pay:hover::before {
        width: 300px;
        height: 300px;
    }

    /* Loading overlay */
    .loading-overlay {
        background: rgba(255, 255, 255, 0.9);
        backdrop-filter: blur(4px);
    }
</style>

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
        window.location.href = url;
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

        window.__ppButtons = paypal.Buttons({
            style: { layout: 'vertical', label: 'paypal', height: 45 },

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
        
        // Update minimum amount display
        var minReq = document.getElementById('<%= hfMinRequired.ClientID %>').value || '0';
        var minDisplay = document.getElementById('minRequiredAmount');
        if (minDisplay) {
            minDisplay.textContent = '₱' + parseFloat(minReq).toFixed(2);
        }
    });

    var customAmountDebounceTimer = null;

    // ------- Validation for Custom Amount -------
    function validateCustomAmount() {
        var minRequired = parseFloat(document.getElementById('<%= hfMinRequired.ClientID %>').value) || 0;
        var inputEl = document.getElementById('<%= txtCustomAmount.ClientID %>');
        var errorLabel = document.getElementById('<%= lblCustomAmountError.ClientID %>');
        var payMongoBtn = document.getElementById('<%= btnPayHere.ClientID %>');
        var paypalContainer = document.getElementById('paypal-button-container');

        var inputValue = inputEl.value.trim();
        var input = inputValue === "" ? 0 : parseFloat(inputValue);

        if (customAmountDebounceTimer) {
            clearTimeout(customAmountDebounceTimer);
        }

        if (inputValue === "") {
            errorLabel.textContent = "";
            inputEl.classList.remove("border-red-500");

            if (payMongoBtn) payMongoBtn.disabled = false;

            if (paypalContainer) {
                paypalContainer.classList.remove("pointer-events-none", "opacity-50");
                paypalContainer.innerHTML = "";
                window.__ppLastAmount = null;
            }

            renderPayPalButtons();
            
            customAmountDebounceTimer = setTimeout(function() {
                triggerCustomAmountUpdate();
            }, 1000);
            
            return true;
        }

        if (input < minRequired) {
            errorLabel.textContent = "Amount cannot be less than ₱" + minRequired.toFixed(2);
            inputEl.classList.add("border-red-500");

            if (payMongoBtn) payMongoBtn.disabled = true;

            if (paypalContainer) {
                paypalContainer.innerHTML =
                    "<p class='text-red-600 font-semibold mt-3'>Enter at least ₱" +
                    minRequired.toFixed(2) + " to enable PayPal.</p>";
                paypalContainer.classList.add("pointer-events-none", "opacity-50");
            }

            return false;
        }

        errorLabel.textContent = "";
        inputEl.classList.remove("border-red-500");

        if (payMongoBtn) payMongoBtn.disabled = false;

        if (paypalContainer) {
            paypalContainer.classList.remove("pointer-events-none", "opacity-50");
            paypalContainer.innerHTML = "";
            window.__ppLastAmount = null;
        }

        renderPayPalButtons();

        customAmountDebounceTimer = setTimeout(function() {
            triggerCustomAmountUpdate();
        }, 1000);

        return true;
    }

    function triggerCustomAmountUpdate() {
        var inputEl = document.getElementById('<%= txtCustomAmount.ClientID %>');
        var minRequired = parseFloat(document.getElementById('<%= hfMinRequired.ClientID %>').value) || 0;
        var inputValue = inputEl.value.trim();
        var input = inputValue === "" ? minRequired : parseFloat(inputValue);

        if (inputValue === "" || (input >= minRequired && !isNaN(input))) {
            __doPostBack('<%= txtCustomAmount.UniqueID %>', '');
        }
    }
</script>
</asp:Content>

<asp:Content ID="MainContentBlock" ContentPlaceHolderID="MainContent" runat="server">
<asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />

<div class="fade-in">
    <!-- Page Header -->
    <div class="mb-6">
        <h1 class="text-3xl font-bold text-gray-800 flex items-center gap-3">
            <i class="fas fa-credit-card text-blue-600"></i>
            Payment Management
        </h1>
        <p class="text-gray-600 mt-2">Manage your payments and view transaction history</p>
    </div>

    <asp:UpdatePanel ID="updPaymentDetails" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div id="paymentPanelBody" class="transition-opacity duration-300">
                
                <!-- Service Information Card -->
                <div class="bg-white rounded-xl shadow-md p-6 mb-6 border-l-4 border-blue-600">
                    <h2 class="text-xl font-semibold text-gray-800 mb-4 flex items-center gap-2">
                        <i class="fas fa-info-circle text-blue-600"></i>
                        Service Details
                    </h2>
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div>
                            <label class="text-sm font-medium text-gray-600">Service Name</label>
                            <asp:Label ID="lblServiceName" runat="server" CssClass="block text-gray-900 font-semibold text-lg" />
                        </div>
                        <div>
                            <label class="text-sm font-medium text-gray-600">Payment Plan</label>
                            <asp:Label ID="lblPaymentPlan" runat="server" CssClass="block text-gray-900 font-semibold text-lg" />
                        </div>
                    </div>
                </div>

                <!-- KPI Cards -->
                <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
                    <div class="stat-card bg-white rounded-xl shadow-md p-5 border-t-4 border-blue-600">
                        <div class="flex items-center justify-between mb-2">
                            <h4 class="text-gray-600 text-sm font-semibold uppercase tracking-wide">Next Installment</h4>
                            <i class="fas fa-calendar-check text-blue-600 text-xl"></i>
                        </div>
                        <div class="text-2xl font-bold text-gray-900"><asp:Literal ID="lblNextInstallment" runat="server" /></div>
                        <div class="text-xs text-gray-500 mt-2"><asp:Literal ID="litNextDue" runat="server" /></div>
                    </div>

                    <div class="stat-card bg-white rounded-xl shadow-md p-5 border-t-4 border-indigo-600">
                        <div class="flex items-center justify-between mb-2">
                            <h4 class="text-gray-600 text-sm font-semibold uppercase tracking-wide">Total Price</h4>
                            <i class="fas fa-file-invoice-dollar text-indigo-600 text-xl"></i>
                        </div>
                        <div class="text-2xl font-bold text-gray-900"><asp:Literal ID="lblTotalPrice" runat="server" /></div>
                        <div class="text-xs text-gray-500 mt-2">Full contract amount</div>
                    </div>

                    <div class="stat-card bg-white rounded-xl shadow-md p-5 border-t-4 border-green-600">
                        <div class="flex items-center justify-between mb-2">
                            <h4 class="text-gray-600 text-sm font-semibold uppercase tracking-wide">Already Paid</h4>
                            <i class="fas fa-check-circle text-green-600 text-xl"></i>
                        </div>
                        <div class="text-2xl font-bold text-gray-900"><asp:Literal ID="lblAlreadyPaid" runat="server" /></div>
                        <div class="text-xs text-gray-500 mt-2">Confirmed payments</div>
                    </div>

                    <div class="stat-card bg-white rounded-xl shadow-md p-5 border-t-4 border-orange-600">
                        <div class="flex items-center justify-between mb-2">
                            <h4 class="text-gray-600 text-sm font-semibold uppercase tracking-wide">Remaining</h4>
                            <i class="fas fa-wallet text-orange-600 text-xl"></i>
                        </div>
                        <div class="text-2xl font-bold text-gray-900"><asp:Literal ID="lblRemaining" runat="server" /></div>
                        <div class="text-xs text-gray-500 mt-2">After this installment</div>
                    </div>
                </div>

                <!-- Pricing Breakdown Card -->
                <div class="bg-white rounded-xl shadow-md p-6 mb-6">
                    <h3 class="text-xl font-semibold text-gray-800 mb-4 flex items-center gap-2">
                        <i class="fas fa-calculator text-blue-600"></i>
                        Pricing Breakdown
                    </h3>
                    <div class="space-y-3">
                        <div class="flex justify-between items-center py-3 border-b border-gray-100">
                            <span class="text-gray-700 font-medium">Base Service Price (Based on SQM)</span>
                            <asp:Label ID="lblBasePrice" runat="server" CssClass="text-gray-900 font-semibold text-lg" />
                        </div>
                        <div class="flex justify-between items-center py-3 border-b border-gray-100">
                            <span class="text-gray-700 font-medium">Travel Expense</span>
                            <asp:Label ID="lblTravelExpense" runat="server" CssClass="text-gray-900 font-semibold text-lg" />
                        </div>
                        <div class="flex justify-between items-center py-3 border-b border-gray-100">
                            <span class="text-gray-700 font-medium">Miscellaneous</span>
                            <asp:Label ID="lblMiscellaneous" runat="server" CssClass="text-gray-900 font-semibold text-lg" />
                        </div>
                        <div class="flex justify-between items-center py-4 bg-blue-50 rounded-lg px-4 mt-4">
                            <span class="text-gray-900 font-bold text-lg">Total Price</span>
                            <asp:Label ID="Label1" runat="server" CssClass="text-blue-600 font-bold text-2xl" />
                        </div>
                    </div>
                </div>

                <!-- Payment Plan Selection -->
                <asp:Panel ID="paymentPlanContainer" runat="server" Visible="true" CssClass="bg-white rounded-xl shadow-md p-6 mb-6">
                    <h3 class="text-xl font-semibold text-gray-800 mb-4 flex items-center gap-2">
                        <i class="fas fa-sliders-h text-blue-600"></i>
                        Payment Plan Selection
                    </h3>
                    <div class="max-w-md">
                        <label for="ddlPlanChoice" class="block text-sm font-medium text-gray-700 mb-2">
                            Choose Your Preferred Payment Plan
                        </label>
                        <asp:DropDownList
                            ID="ddlPlanChoice"
                            runat="server"
                            CssClass="block w-full border-2 border-gray-300 rounded-lg px-4 py-3 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 outline-none transition-all"
                            AutoPostBack="true"
                            OnSelectedIndexChanged="ddlPlanChoice_SelectedIndexChanged">
                            <asp:ListItem Text="50% / 25% / 25%" Value="50-25-25" />
                            <asp:ListItem Text="70% / 30%" Value="70-30" />
                            <asp:ListItem Text="100% Full Payment" Value="100" />
                        </asp:DropDownList>
                    </div>
                </asp:Panel>

                <!-- Custom Amount Input -->
                <div class="bg-white rounded-xl shadow-md p-6 mb-6">
                    <h3 class="text-xl font-semibold text-gray-800 mb-4 flex items-center gap-2">
                        <i class="fas fa-edit text-blue-600"></i>
                        Custom Payment Amount
                    </h3>
                    <div class="max-w-md">
                        <label for="txtCustomAmount" class="block text-sm font-medium text-gray-700 mb-2">
                            Enter Your Payment Amount
                        </label>
                        <div class="relative">
                            <span class="absolute left-4 top-1/2 -translate-y-1/2 text-gray-500 font-semibold">₱</span>
                            <asp:TextBox
                                ID="txtCustomAmount"
                                runat="server"
                                CssClass="custom-input block w-full border-2 border-gray-300 rounded-lg pl-10 pr-4 py-3 outline-none transition-all"
                                placeholder="0.00"
                                oninput="validateCustomAmount();" />
                        </div>
                        <p class="text-sm text-gray-600 mt-2 flex items-center gap-2">
                            <i class="fas fa-info-circle text-blue-600"></i>
                            Minimum required: <span id="minRequiredAmount" class="font-semibold text-gray-900">₱0.00</span>
                        </p>
                        <asp:Label ID="lblCustomAmountError" runat="server" CssClass="text-red-600 text-sm mt-2 block font-medium" />
                    </div>

                    <asp:Label ID="lblMessage" runat="server" CssClass="text-red-600 font-medium block mt-4 p-3 bg-red-50 rounded-lg" />
                    <asp:Label ID="lblReminder" runat="server" CssClass="text-yellow-700 font-medium block mt-4 p-3 bg-yellow-50 rounded-lg" />
                </div>

                <!-- Hidden Fields -->
                <asp:HiddenField ID="hfMinRequired" runat="server" />
                <asp:HiddenField ID="hfSelectedPlan" runat="server" />
                <asp:HiddenField ID="hfPayPalBookingID" runat="server" />
                <asp:HiddenField ID="hfPayPalAmount" runat="server" />
                <asp:HiddenField ID="hfPayPalClientID" runat="server" />
                <asp:HiddenField ID="hiddenCheckoutURL" runat="server" />
                <asp:HiddenField ID="hiddenReference" runat="server" />

            </div>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="ddlPlanChoice" EventName="SelectedIndexChanged" />
            <asp:AsyncPostBackTrigger ControlID="txtCustomAmount" EventName="TextChanged" />
        </Triggers>
    </asp:UpdatePanel>

    <!-- Payment Methods Section -->
    <div class="bg-white rounded-xl shadow-md p-6 mb-6">
        <h3 class="text-xl font-semibold text-gray-800 mb-4 flex items-center gap-2">
            <i class="fas fa-money-check-alt text-blue-600"></i>
            Payment Methods
        </h3>
        
        <div class="space-y-4">
            <!-- PayMongo Card -->
            <div id="paymongo-area" class="payment-card hidden bg-gradient-to-r from-green-50 to-emerald-50 rounded-lg p-5">
                <div class="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
                    <div class="flex-1">
                        <h4 class="font-semibold text-gray-800 mb-2 flex items-center gap-2">
                            <i class="fas fa-credit-card text-green-600"></i>
                            PayMongo - Multiple Options
                        </h4>
                        <div class="flex flex-wrap gap-2 mb-3">
                            <span class="text-xs bg-white text-gray-700 px-3 py-1 rounded-full border border-gray-200">💳 Card</span>
                            <span class="text-xs bg-white text-gray-700 px-3 py-1 rounded-full border border-gray-200">📱 GCash</span>
                            <span class="text-xs bg-white text-gray-700 px-3 py-1 rounded-full border border-gray-200">🚗 GrabPay</span>
                            <span class="text-xs bg-white text-gray-700 px-3 py-1 rounded-full border border-gray-200">💎 Maya</span>
                        </div>
                        <p id="paymongo-note" class="text-sm text-gray-600">
                            <i class="fas fa-spinner fa-spin mr-2"></i>Preparing checkout link...
                        </p>
                    </div>
                    <asp:Button
                        ID="btnPayHere"
                        runat="server"
                        Text="Pay with PayMongo"
                        CssClass="btn-pay bg-green-600 hover:bg-green-700 text-white px-6 py-3 rounded-lg font-semibold disabled:opacity-50 disabled:cursor-not-allowed whitespace-nowrap shadow-md"
                        OnClientClick="return openPayMongoCheckout();"
                        CausesValidation="false" />
                </div>
            </div>

            <!-- PayPal Card -->
            <asp:UpdatePanel ID="updPayPal" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="payment-card bg-gradient-to-r from-blue-50 to-indigo-50 rounded-lg p-5">
                        <h4 class="font-semibold text-gray-800 mb-3 flex items-center gap-2">
                            <i class="fab fa-paypal text-blue-600 text-xl"></i>
                            PayPal
                        </h4>
                        <div id="paypal-warning" class="hidden text-green-600 font-semibold p-3 bg-green-50 rounded-lg">
                            <i class="fas fa-check-circle mr-2"></i>You have no remaining balance to pay.
                        </div>
                        <div id="paypal-button-container" class="mt-2 max-w-sm"></div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>

    <!-- Payment History Section -->
    <div class="bg-white rounded-xl shadow-md p-6">
        <h3 class="text-xl font-semibold text-gray-800 mb-4 flex items-center gap-2">
            <i class="fas fa-history text-blue-600"></i>
            Payment History
        </h3>
        
        <div class="overflow-x-auto rounded-lg border border-gray-200">
            <asp:GridView ID="gvPaymentHistory" runat="server"
                AutoGenerateColumns="False"
                CssClass="payment-grid w-full text-sm"
                GridLines="None"
                OnRowDataBound="gvPaymentHistory_RowDataBound">
                <HeaderStyle CssClass="bg-gradient-to-r from-blue-600 to-blue-700" />
                <RowStyle CssClass="bg-white hover:bg-gray-50 transition-colors" />
                <AlternatingRowStyle CssClass="bg-gray-50 hover:bg-gray-100 transition-colors" />
                <Columns>
                    <asp:BoundField DataField="TransactionDate" HeaderText="Date" DataFormatString="{0:MMM dd, yyyy}" NullDisplayText="—" 
                        HeaderStyle-CssClass="px-6 py-4 text-left text-white" 
                        ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-gray-800" />
                    
                    <asp:BoundField DataField="Amount" HeaderText="Amount" DataFormatString="₱{0:N2}" NullDisplayText="₱0.00" 
                        HeaderStyle-CssClass="px-6 py-4 text-left text-white" 
                        ItemStyle-CssClass="px-6 py-4 whitespace-nowrap font-semibold text-gray-900" />
                    
                    <asp:BoundField DataField="PaymentMethod" HeaderText="Method" NullDisplayText="—" 
                        HeaderStyle-CssClass="px-6 py-4 text-left text-white" 
                        ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-gray-800" />
                    
                    <asp:BoundField DataField="Remarks" HeaderText="Remarks" NullDisplayText="—" 
                        HeaderStyle-CssClass="px-6 py-4 text-left text-white" 
                        ItemStyle-CssClass="px-6 py-4 text-gray-700" />
                    
                    <asp:TemplateField HeaderText="Receipt">
                        <HeaderStyle CssClass="px-6 py-4 text-left text-white" />
                        <ItemStyle CssClass="px-6 py-4 whitespace-nowrap" />
                        <ItemTemplate>
                            <%# GetReceiptLink(Eval("Receipt")) %>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate>
                    <div class="p-8 text-center">
                        <i class="fas fa-inbox text-4xl text-gray-300 mb-3"></i>
                        <p class="text-gray-500 font-medium">No payment records found</p>
                        <p class="text-gray-400 text-sm mt-1">Your payment history will appear here</p>
                    </div>
                </EmptyDataTemplate>
            </asp:GridView>
        </div>
    </div>
</div>

</asp:Content>