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
          var btn = document.getElementById('btnPayMongo');
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

      function openPayMongoCheckout() {
          var url = document.getElementById('<%= hiddenCheckoutURL.ClientID %>').value || '';
          if (!url) {
              Swal.fire('Payment link not ready', 'Please wait a moment or change plan to refresh.', 'info');
              return false;
          }
          window.open(url, '_blank', 'noopener');
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

        var raw = amountEl ? (amountEl.value || "") : "";
        var amt = parseFloat(String(raw).replace(/,/g, ""));
        if (!amt || isNaN(amt) || amt <= 0) {
            if (window.__ppButtons && window.__ppButtons.close) window.__ppButtons.close();
            container.innerHTML = "<p class='text-green-600 font-semibold mt-3 mb-0'>✅ You have no balance.</p>";
            window.__ppButtons = null; window.__ppLastAmount = null; return;
        }

        var amount = amt.toFixed(2);
        if (window.__ppLastAmount === amount && window.__ppButtons) return;
        window.__ppLastAmount = amount;

        if (window.__ppButtons && window.__ppButtons.close) {
            try { window.__ppButtons.close(); } catch (e) { }
            window.__ppButtons = null;
        }
        container.innerHTML = "";
        container.classList.add('opacity-40');

        window.__ppButtons = paypal.Buttons({
            style: { layout: 'vertical', label: 'paypal' },
            createOrder: function (data, actions) {
                var bookingId = document.getElementById('<%= hfPayPalBookingID.ClientID %>').value;
                return actions.order.create({ purchase_units: [{ amount: { value: amount }, custom_id: bookingId }] });
            },
            onApprove: function (data, actions) {
                return actions.order.capture().then(function (details) {
                    var bookingId = document.getElementById('<%= hfPayPalBookingID.ClientID %>').value;
                  var clientId = document.getElementById('<%= hfPayPalClientID.ClientID %>').value;
                  fetch('/PayPalWebhook.ashx?custom=' + encodeURIComponent(bookingId) + '&amount=' + encodeURIComponent(amount) + '&client=' + encodeURIComponent(clientId))
                      .then(r => r.text())
                      .then(msg => Swal.fire({ icon: 'success', title: 'Payment completed!', html: 'Transaction by ' + (details?.payer?.name?.given_name || 'payer') + '<br/><small>' + msg + '</small>' }).then(() => location.reload()))
                      .catch(err => Swal.fire('✅ Paid, but DB not updated.', err.message, 'warning'));
              });
            },
            onCancel: function () { Swal.fire('Payment canceled', '', 'info'); },
            onError: function (err) { Swal.fire('Payment error', err.message, 'error'); }
        });
          window.__ppButtons.render('#paypal-button-container').finally(function () { container.classList.remove('opacity-40'); });
      }

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
              updatePayMongoButton(); renderPayPalButtons();
          });
      }

      document.addEventListener('DOMContentLoaded', function () {
          hookUpdatePanelVisuals(); updatePayMongoButton();
          if (!(window.Sys && Sys.Application && Sys.Application.get_isInitialized && Sys.Application.get_isInitialized())) {
              renderPayPalButtons();
          }
      });
      if (window.Sys && Sys.Application) {
          Sys.Application.add_load(function (sender, args) {
              if (!args.get_isPartialLoad()) { updatePayMongoButton(); renderPayPalButtons(); }
          });
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

            <!-- Dynamic Price Info -->
            <asp:Label ID="lblPrice" runat="server" CssClass="block text-gray-600 whitespace-pre-line mb-4" />

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

            <!-- Hidden Fields -->
            <asp:HiddenField ID="hfSelectedPlan" runat="server" />
            <asp:HiddenField ID="hfPayPalBookingID" runat="server" />
            <asp:HiddenField ID="hfPayPalAmount" runat="server" />
            <asp:HiddenField ID="hfPayPalClientID" runat="server" />
            <asp:HiddenField ID="hiddenCheckoutURL" runat="server" />
            <asp:HiddenField ID="hiddenReference" runat="server" />

            <asp:Label ID="lblMessage" runat="server" CssClass="text-red-600 font-medium block mt-2" />
            <asp:Label ID="lblReminder" runat="server" CssClass="text-yellow-600 font-medium block mt-2" />

          </div>
        </ContentTemplate>
        <Triggers>
          <asp:AsyncPostBackTrigger ControlID="ddlPlanChoice" EventName="SelectedIndexChanged" />
        </Triggers>
      </asp:UpdatePanel>

      <!-- Payment Methods -->
      <div class="space-y-4 mt-6">
        <!-- PayMongo -->
        <div id="paymongo-area" class="hidden">
          <div class="flex flex-wrap items-center gap-3">
            <button id="btnPayMongo" class="bg-green-600 hover:bg-green-700 text-white px-5 py-2 rounded disabled:opacity-50" onclick="return openPayMongoCheckout();" disabled>
              Pay Here
            </button>
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
      <div class="overflow-x-auto">
        <asp:GridView ID="gvPaymentHistory" runat="server"
            AutoGenerateColumns="False"
            CssClass="min-w-full text-sm text-left border border-gray-200"
            GridLines="None"
            OnRowDataBound="gvPaymentHistory_RowDataBound">
          <Columns>
            <asp:BoundField DataField="TransactionDate" HeaderText="Date" DataFormatString="{0:yyyy-MM-dd}" NullDisplayText="—" />
            <asp:BoundField DataField="Amount" HeaderText="Amount" DataFormatString="₱{0:N2}" NullDisplayText="₱0.00" />
            <asp:BoundField DataField="PaymentMethod" HeaderText="Method" NullDisplayText="—" />
            <asp:BoundField DataField="Remarks" HeaderText="Remarks" NullDisplayText="—" />
            <asp:TemplateField HeaderText="Receipt">
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
