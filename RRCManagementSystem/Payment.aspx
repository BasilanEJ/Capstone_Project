<%@ Page Title="Payment" Language="C#" MasterPageFile="~/Client.master" Async="true" AutoEventWireup="true" CodeBehind="Payment.aspx.cs" Inherits="RRCManagementSystem.Payment" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
  <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
  <!-- PayPal SDK (PHP currency) -->
  <script src="https://www.paypal.com/sdk/js?client-id=AXUxUohfga-5TSDyZurxJ07QF4gdpG4uxPWGSn6rqc8Gt3lQSPiYLJyKDGdqOYjhZgRw9vQMWpqHG1Fj&currency=PHP"></script>

  <style>
    .page-title{ color:#0d6efd; font-weight:700; font-size:clamp(1.25rem,3.2vw,1.75rem); line-height:1.2; }
    .page-wrap{ padding-top:clamp(.75rem,2.5vw,1.25rem); padding-bottom:calc(110px + env(safe-area-inset-bottom)); }
    .card-shell{ border:none; border-radius:1rem; box-shadow:0 8px 20px rgba(0,0,0,.06); }

    .fade-container{ position:relative; transition:opacity .18s ease; }
    .fade-container.updating{ opacity:.35; pointer-events:none; filter:blur(.5px); }
    .fade-container .spinner-overlay{ display:none; position:absolute; inset:0; align-items:center; justify-content:center; z-index:5; }
    .fade-container.updating .spinner-overlay{ display:flex; }
    .spinner{ width:26px; height:26px; border:3px solid #e5e7eb; border-top-color:#0d6efd; border-radius:50%; animation:spin .8s linear infinite; }
    @keyframes spin{ to{ transform:rotate(360deg) } }

    #paypal-button-container{ min-height:48px; transition:opacity .18s ease; }
    #paypal-button-container.ppb-loading{ opacity:.2; }

    .btn, .form-select, .form-control{ min-height:44px; }
    .form-select.w-auto{ width:100% !important; }
    @media (min-width:576px){ .form-select.w-auto{ width:auto !important; } }

    .table td, .table th{ vertical-align:middle; white-space:nowrap; word-break:normal; }
    .table-responsive{ overflow-x:auto; -webkit-overflow-scrolling:touch; scrollbar-width:thin; }
    .fixed-grid>table{ min-width:760px; table-layout:auto; border-collapse:separate; border-spacing:0; }
    .fixed-grid>table th, .fixed-grid>table td{ white-space:nowrap !important; overflow:visible; text-wrap:nowrap; }

    #wrap-gvPaymentHistory > table thead th:nth-child(1),
    #wrap-gvPaymentHistory > table tbody td:nth-child(1){ min-width:140px; }
    #wrap-gvPaymentHistory > table thead th:nth-child(2),
    #wrap-gvPaymentHistory > table tbody td:nth-child(2){ min-width:130px; }
    #wrap-gvPaymentHistory > table thead th:nth-child(3),
    #wrap-gvPaymentHistory > table tbody td:nth-child(3){ min-width:140px; }
    #wrap-gvPaymentHistory > table thead th:nth-child(4),
    #wrap-gvPaymentHistory > table tbody td:nth-child(4){ min-width:300px; }
    #wrap-gvPaymentHistory > table thead th:nth-child(5),
    #wrap-gvPaymentHistory > table tbody td:nth-child(5){ min-width:110px; }

    @media (prefers-reduced-motion:reduce){ .fade, .collapse{ transition:none !important; } }

    /* PayMongo UI */
    #paymongo-area .badge{ font-size:.8rem; }
  </style>

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
              wrap.classList.remove('d-none');
              btn.disabled = false;
              note.classList.add('d-none');
          } else {
              btn.disabled = true;
              note.classList.remove('d-none');
          }
      }

      function openPayMongoCheckout() {
          var url = document.getElementById('<%= hiddenCheckoutURL.ClientID %>').value || '';
          if (!url) {
              Swal.fire('Payment link not ready', 'Please wait a moment or change plan to refresh.', 'info');
              return false;
          }
          window.open(url, '_blank', 'noopener');
          return false; // prevent postback
      }

      // ------- PayPal Smart Buttons (unchanged) -------
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
        container.innerHTML = "<p class='text-success fw-bold mt-3 mb-0'>✅ You have no balance.</p>";
        window.__ppButtons = null;
        window.__ppLastAmount = null;
        return;
      }

      var amount = amt.toFixed(2);
      if (window.__ppLastAmount === amount && window.__ppButtons) return;
      window.__ppLastAmount = amount;

      if (window.__ppButtons && window.__ppButtons.close) {
        try { window.__ppButtons.close(); } catch (e) {}
        window.__ppButtons = null;
      }
      container.innerHTML = "";
      container.classList.add('ppb-loading');

      window.__ppButtons = paypal.Buttons({
        style: { layout: 'vertical', label: 'paypal' },
        createOrder: function (data, actions) {
          var bookingId = document.getElementById('<%= hfPayPalBookingID.ClientID %>').value;
          return actions.order.create({
            purchase_units: [{ amount: { value: amount }, custom_id: bookingId }]
          });
        },
        onApprove: function (data, actions) {
          return actions.order.capture().then(function (details) {
            var bookingId = document.getElementById('<%= hfPayPalBookingID.ClientID %>').value;
            var clientId  = document.getElementById('<%= hfPayPalClientID.ClientID %>').value;

              fetch('/PayPalWebhook.ashx?custom=' + encodeURIComponent(bookingId) +
                  '&amount=' + encodeURIComponent(amount) +
                  '&client=' + encodeURIComponent(clientId))
                  .then(r => r.text())
                  .then(msg => Swal.fire({
                      icon: 'success', title: 'Payment completed!',
                      html: 'Transaction by ' + (details?.payer?.name?.given_name || 'payer') +
                          '<br/><small>' + msg + '</small>'
                  }).then(() => location.reload()))
                  .catch(err => Swal.fire('✅ Paid, but DB not updated.', err.message, 'warning'));
          });
          },
          onCancel: function () { Swal.fire('Payment canceled', '', 'info'); },
          onError: function (err) { Swal.fire('Payment error', err.message, 'error'); }
      });

          window.__ppButtons.render('#paypal-button-container').finally(function () {
              container.classList.remove('ppb-loading');
          });
      }

      function hookUpdatePanelVisuals() {
          if (!(window.Sys && Sys.WebForms) || window.__ppHooked) return;
          window.__ppHooked = true;

          var prm = Sys.WebForms.PageRequestManager.getInstance();
          prm.add_beginRequest(function () {
              var panel = document.getElementById('paymentPanelBody');
              if (panel) panel.classList.add('updating');
          });
          prm.add_endRequest(function () {
              var panel = document.getElementById('paymentPanelBody');
              if (panel) panel.classList.remove('updating');
              // Re-check PayMongo readiness + re-render PayPal
              updatePayMongoButton();
              renderPayPalButtons();
          });
      }

      document.addEventListener('DOMContentLoaded', function () {
          hookUpdatePanelVisuals();
          updatePayMongoButton();
          if (!(window.Sys && Sys.Application && Sys.Application.get_isInitialized && Sys.Application.get_isInitialized())) {
              renderPayPalButtons();
          }
      });

      if (window.Sys && Sys.Application) {
          Sys.Application.add_load(function (sender, args) {
              if (!args.get_isPartialLoad()) {
                  updatePayMongoButton();
                  renderPayPalButtons();
              }
          });
      }
  </script>
</asp:Content>

<asp:Content ID="MainContentBlock" ContentPlaceHolderID="MainContent" runat="server">
  <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />
  <div class="container page-wrap">
    <div class="card card-shell p-3 p-sm-4">
      <h2 class="page-title mb-3">💳 Account Balance Overview</h2>


      <asp:UpdatePanel ID="updPaymentDetails" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
          <div id="paymentPanelBody" class="fade-container">
            <div class="spinner-overlay"><div class="spinner" aria-hidden="true"></div></div>

            <div class="mb-4">
              <p class="mb-1"><strong>Service Name:</strong> <asp:Label ID="lblServiceName" runat="server" CssClass="ms-1" /></p>
              <p class="mb-1"><strong>Current Balance:</strong> <asp:Label ID="lblPrice" runat="server" CssClass="ms-1" /></p>
              <p class="mb-0"><strong>Payment Plan:</strong><br /><asp:Label ID="lblPaymentPlan" runat="server" CssClass="ms-1" /></p>
            </div>

            <div class="mb-4" id="paymentPlanContainer" runat="server" visible="true">
              <label for="ddlPlanChoice" class="form-label fw-semibold">Preferred Payment Plan:</label>
              <asp:DropDownList ID="ddlPlanChoice" runat="server" CssClass="form-select w-auto"
                  AutoPostBack="true" OnSelectedIndexChanged="ddlPlanChoice_SelectedIndexChanged">
                <asp:ListItem Text="50% / 25% / 25%" Value="50-25-25" />
                <asp:ListItem Text="70% / 30%" Value="70-30" />
                <asp:ListItem Text="100% Full Payment" Value="100" />
              </asp:DropDownList>
              <asp:HiddenField ID="hfSelectedPlan" runat="server" />
            </div>

            <asp:HiddenField ID="hfPayPalBookingID" runat="server" />
            <asp:HiddenField ID="hfPayPalAmount" runat="server" />
            <asp:HiddenField ID="hfPayPalClientID" runat="server" />
            <asp:HiddenField ID="hiddenCheckoutURL" runat="server" />
            <asp:HiddenField ID="hiddenReference" runat="server" />

      
            <asp:Label ID="lblMessage" runat="server" CssClass="text-danger fw-semibold d-block mt-2" />
            <asp:Label ID="lblReminder" runat="server" CssClass="text-warning fw-semibold d-block" />
          </div>
        </ContentTemplate>
        <Triggers>
          <asp:AsyncPostBackTrigger ControlID="ddlPlanChoice" EventName="SelectedIndexChanged" />
        </Triggers>
      </asp:UpdatePanel>


      <div id="paymongo-area" class="mt-2">
        <div class="d-flex flex-wrap align-items-center gap-2">
          <button id="btnPayMongo" class="btn btn-success px-4" onclick="return openPayMongoCheckout();" disabled>
            Pay via PayMongo
          </button>
          <span id="paymongo-note" class="text-secondary">
            Generating PayMongo link… (it will enable automatically when ready)
          </span>
          <span class="badge text-bg-light">GCash · Card · Maya</span>
        </div>
      </div>


      <asp:UpdatePanel ID="updPayPal" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
          <div class="mt-3" id="paypalArea">
            <div id="paypal-warning" class="text-success fw-bold d-none">✅ You have no remaining balance to pay.</div>
            <div id="paypal-button-container" class="mt-2 mb-4"></div>
          </div>
        </ContentTemplate>
      </asp:UpdatePanel>

      <h3 class="text-primary mb-3">📜 Payment History</h3>
      <div class="table-responsive fixed-grid" id="wrap-gvPaymentHistory">
        <asp:GridView ID="gvPaymentHistory" runat="server" AutoGenerateColumns="False"
          CssClass="table table-bordered table-striped align-middle">
          <Columns>
            <asp:BoundField DataField="TransactionDate" HeaderText="Date" DataFormatString="{0:yyyy-MM-dd}" />
            <asp:BoundField DataField="Amount" HeaderText="Amount" DataFormatString="₱{0:N2}" />
            <asp:BoundField DataField="PaymentMethod" HeaderText="Method" />
            <asp:BoundField DataField="Remarks" HeaderText="Remarks" />
            <asp:TemplateField HeaderText="Receipt">
              <ItemTemplate>
                <%# (Eval("Receipt") != DBNull.Value && Eval("Receipt") != null && Eval("Receipt").ToString() != "")
                    ? ("<a href='DecryptReceipt.aspx?file="
                       + System.IO.Path.GetFileName(Eval("Receipt").ToString())
                       + "' target='_blank' rel='noopener'>View</a>")
                    : "No Receipt" %>
              </ItemTemplate>
            </asp:TemplateField>
          </Columns>
        </asp:GridView>
      </div>
    </div>
  </div>
</asp:Content>
