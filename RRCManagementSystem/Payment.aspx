<%@ Page Title="Payment" Language="C#" MasterPageFile="~/Client.master" Async="true"
    AutoEventWireup="true" CodeBehind="Payment.aspx.cs" Inherits="RRCManagementSystem.Payment" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
  <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
  <!-- PayPal SDK (PHP currency) -->
  <script src="https://www.paypal.com/sdk/js?client-id=AXUxUohfga-5TSDyZurxJ07QF4gdpG4uxPWGSn6rqc8Gt3lQSPiYLJyKDGdqOYjhZgRw9vQMWpqHG1Fj&currency=PHP"></script>

  <style>
    :root{
      --brand:#0d6efd; --ink:#111827; --muted:#6b7280;
      --card:#ffffff; --border:#e5e7eb; --bg:#f7f8fb;
    }
    body{ background:var(--bg); }

    .page-wrap{ padding:clamp(16px,2.5vw,28px) 0 calc(110px + env(safe-area-inset-bottom)); }
    .card-shell{ border:1px solid var(--border); border-radius:16px; background:var(--card);
      box-shadow:0 10px 24px rgba(0,0,0,.06); }
    .page-title{ color:var(--brand); font-weight:800; letter-spacing:.2px; display:flex; align-items:center; gap:.5rem; }
    .page-title .emoji{ font-size:1.4em; }

    .meta p{ margin:0 0 .4rem 0; color:var(--ink); }
    .muted{ color:var(--muted); }

    /* KPI cards */
    .kpis{ display:grid; grid-template-columns:repeat(1,minmax(0,1fr)); gap:12px; }
    @media (min-width:576px){ .kpis{ grid-template-columns:repeat(2,1fr); } }
    @media (min-width:992px){ .kpis{ grid-template-columns:repeat(4,1fr); } }
    .kpi{ border:1px solid var(--border); border-radius:14px; padding:14px 16px; background:#fff; }
    .kpi h4{ margin:0 0 6px 0; font-size:.9rem; font-weight:700; color:var(--muted); display:flex; align-items:center; gap:.4rem; }
    .kpi .value{ font-size:clamp(1.25rem,2.2vw,1.6rem); font-weight:800; letter-spacing:.3px; color:var(--ink); }
    .kpi .sub{ font-size:.85rem; color:var(--muted); margin-top:4px; }

    /* Payment sections: ALWAYS vertical stack */
    .pay-wrap{
      display:flex;
      flex-direction:column;       /* <- ensures PayPal is UNDER PayMongo */
      gap:14px;
    }

    #paypal-button-container{ min-height:48px; transition:opacity .18s ease; }
    #paypal-button-container.ppb-loading{ opacity:.2; }

    .btn, .form-select, .form-control{ min-height:44px; }
    .btn-success{ background:#16a34a; border-color:#16a34a; }
    .btn-success:hover{ filter:brightness(.95); }

    /* UpdatePanel fade state */
    .fade-container{ position:relative; transition:opacity .18s ease; }
    .fade-container.updating{ opacity:.35; pointer-events:none; filter:blur(.5px); }
    .fade-container .spinner-overlay{ display:none; position:absolute; inset:0; align-items:center; justify-content:center; z-index:5; }
    .fade-container.updating .spinner-overlay{ display:flex; }
    .spinner{ width:26px; height:26px; border:3px solid #e5e7eb; border-top-color:var(--brand); border-radius:50%; animation:spin .8s linear infinite; }
    @keyframes spin{ to{ transform:rotate(360deg) } }

    /* History table */
    .fixed-grid>table{ min-width:760px; border-collapse:separate; border-spacing:0; }
    .table td, .table th{ vertical-align:middle; white-space:nowrap; }
    .table-responsive{ overflow-x:auto; -webkit-overflow-scrolling:touch; scrollbar-width:thin; }

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

    /* Legacy label makes \n show as lines */
    .preline{ white-space:pre-line; }

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
          if (!url) { Swal.fire('Payment link not ready', 'Please wait a moment or change plan to refresh.', 'info'); return false; }
          window.open(url, '_blank', 'noopener'); return false;
      }

      // ------- PayPal Smart Buttons -------
      window.__ppLastAmount = null; window.__ppButtons = null; window.__ppHooked = false;
      function renderPayPalButtons() {
          var amountEl = document.getElementById('<%= hfPayPalAmount.ClientID %>');
          var container = document.getElementById('paypal-button-container');
          if (!container || !window.paypal) return;

          var raw = amountEl ? (amountEl.value || "") : "";
          var amt = parseFloat(String(raw).replace(/,/g,""));
          if (!amt || isNaN(amt) || amt <= 0) {
            if (window.__ppButtons && window.__ppButtons.close) window.__ppButtons.close();
            container.innerHTML = "<p class='text-success fw-bold mt-3 mb-0'>✅ You have no balance.</p>";
            window.__ppButtons = null; window.__ppLastAmount = null; return;
          }

          var amount = amt.toFixed(2);
          if (window.__ppLastAmount === amount && window.__ppButtons) return;
          window.__ppLastAmount = amount;

          if (window.__ppButtons && window.__ppButtons.close) { try{ window.__ppButtons.close(); }catch(e){} window.__ppButtons=null; }
          container.innerHTML=""; container.classList.add('ppb-loading');

          window.__ppButtons = paypal.Buttons({
            style: { layout:'vertical', label:'paypal' },
            createOrder: function (data, actions) {
              var bookingId = document.getElementById('<%= hfPayPalBookingID.ClientID %>').value;
              return actions.order.create({ purchase_units: [{ amount:{ value:amount }, custom_id: bookingId }] });
            },
            onApprove: function (data, actions) {
              return actions.order.capture().then(function (details) {
                var bookingId = document.getElementById('<%= hfPayPalBookingID.ClientID %>').value;
                var clientId  = document.getElementById('<%= hfPayPalClientID.ClientID %>').value;
                  fetch('/PayPalWebhook.ashx?custom=' + encodeURIComponent(bookingId) + '&amount=' + encodeURIComponent(amount) + '&client=' + encodeURIComponent(clientId))
                      .then(r => r.text())
                      .then(msg => Swal.fire({ icon: 'success', title: 'Payment completed!', html: 'Transaction by ' + (details?.payer?.name?.given_name || 'payer') + '<br/><small>' + msg + '</small>' }).then(() => location.reload()))
                      .catch(err => Swal.fire('✅ Paid, but DB not updated.', err.message, 'warning'));
              });
              },
              onCancel: function () { Swal.fire('Payment canceled', '', 'info'); },
              onError: function (err) { Swal.fire('Payment error', err.message, 'error'); }
          });
          window.__ppButtons.render('#paypal-button-container').finally(function () { container.classList.remove('ppb-loading'); });
      }

      function hookUpdatePanelVisuals() {
          if (!(window.Sys && Sys.WebForms) || window.__ppHooked) return;
          window.__ppHooked = true;
          var prm = Sys.WebForms.PageRequestManager.getInstance();
          prm.add_beginRequest(function () { var panel = document.getElementById('paymentPanelBody'); if (panel) panel.classList.add('updating'); });
          prm.add_endRequest(function () {
              var panel = document.getElementById('paymentPanelBody');
              if (panel) panel.classList.remove('updating');
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
  <div class="container page-wrap">
    <div class="card card-shell p-3 p-sm-4">
      <h2 class="page-title mb-3"><span class="emoji">💳</span> Account Balance Overview</h2>

      <asp:UpdatePanel ID="updPaymentDetails" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
          <div id="paymentPanelBody" class="fade-container">
            <div class="spinner-overlay"><div class="spinner" aria-hidden="true"></div></div>

            <!-- Service meta -->
            <div class="meta mb-3">
              <p><strong>Service Name:</strong> <asp:Label ID="lblServiceName" runat="server" CssClass="ms-1" /></p>
              <p class="muted"><strong>Payment Plan:</strong> <asp:Label ID="lblPaymentPlan" runat="server" CssClass="ms-1" /></p>
            </div>

            <!-- KPI cards -->
            <div class="kpis mb-3">
              <div class="kpi">
                <h4>Next Installment</h4>
                <div class="value"><asp:Literal ID="lblNextInstallment" runat="server" /></div>
                <div class="sub">Due now</div>
              </div>
              <div class="kpi">
                <h4>Total Price</h4>
                <div class="value"><asp:Literal ID="lblTotalPrice" runat="server" /></div>
                <div class="sub">Full contract amount</div>
              </div>
              <div class="kpi">
                <h4>Already Paid</h4>
                <div class="value"><asp:Literal ID="lblAlreadyPaid" runat="server" /></div>
                <div class="sub">Confirmed payments</div>
              </div>
              <div class="kpi">
                <h4>Remaining Balance</h4>
                <div class="value"><asp:Literal ID="lblRemaining" runat="server" /></div>
                <div class="sub">After this installment</div>
              </div>
            </div>

            <!-- Legacy combined label (renders with line breaks via .preline) -->
            <asp:Label ID="lblPrice" runat="server" CssClass="preline d-block mb-3 muted" />

            <!-- Plan selector -->
            <div class="mb-4" id="paymentPlanContainer" runat="server" visible="true">
              <label for="ddlPlanChoice" class="form-label fw-semibold">Preferred Payment Plan</label>
              <asp:DropDownList ID="ddlPlanChoice" runat="server" CssClass="form-select w-auto"
                  AutoPostBack="true" OnSelectedIndexChanged="ddlPlanChoice_SelectedIndexChanged">
                <asp:ListItem Text="50% / 25% / 25%" Value="50-25-25" />
                <asp:ListItem Text="70% / 30%" Value="70-30" />
                <asp:ListItem Text="100% Full Payment" Value="100" />
              </asp:DropDownList>
              <asp:HiddenField ID="hfSelectedPlan" runat="server" />
            </div>

            <!-- Hidden fields for JS -->
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

      <!-- Payment Methods (STACKED) -->
      <div class="pay-wrap mt-3">
        <!-- PayMongo -->
        <div id="paymongo-area">
          <div class="d-flex flex-wrap align-items-center gap-2">
            <button id="btnPayMongo" class="btn btn-success px-4" onclick="return openPayMongoCheckout();" disabled>
              Pay Here
            </button>
            <span id="paymongo-note" class="text-secondary">
             
            </span>
            <span class="badge text-bg-light">GCash · Card · Maya</span>
          </div>
        </div>

        <!-- PayPal (always below PayMongo) -->
        <asp:UpdatePanel ID="updPayPal" runat="server" UpdateMode="Conditional">
          <ContentTemplate>
            <div id="paypalArea" class="pt-1">
              <div id="paypal-warning" class="text-success fw-bold d-none">✅ You have no remaining balance to pay.</div>
              <div id="paypal-button-container" class="mt-2 mb-2"></div>
            </div>
          </ContentTemplate>
        </asp:UpdatePanel>
      </div>

      <!-- History -->
    <h3 class="page-title mt-4"><span class="emoji">📜</span> Payment History</h3>
<div class="table-responsive fixed-grid" id="wrap-gvPaymentHistory">
  <asp:GridView ID="gvPaymentHistory" runat="server"
      AutoGenerateColumns="False"
      CssClass="table table-bordered table-striped align-middle"
      EnableViewState="true"
      OnRowDataBound="gvPaymentHistory_RowDataBound">
    <Columns>
 
      <asp:BoundField DataField="TransactionDate"
                      HeaderText="Date"
                      HtmlEncode="false"
                      DataFormatString="{0:yyyy-MM-dd}"
                      NullDisplayText="—" />

    
      <asp:BoundField DataField="Amount"
                      HeaderText="Amount"
                      HtmlEncode="false"
                      DataFormatString="₱{0:N2}"
                      NullDisplayText="₱0.00" />

     
      <asp:BoundField DataField="PaymentMethod"
                      HeaderText="Method"
                      NullDisplayText="—" />

    
      <asp:BoundField DataField="Remarks"
                      HeaderText="Remarks"
                      NullDisplayText="—" />

      <asp:TemplateField HeaderText="Receipt">
        <ItemTemplate>
          <%# GetReceiptLink(Eval("Receipt")) %>
        </ItemTemplate>
      </asp:TemplateField>
    </Columns>

    <EmptyDataTemplate>
      <div class="text-secondary p-3">No payments recorded yet.</div>
    </EmptyDataTemplate>
  </asp:GridView>
</div>

    </div>
  </div>
</asp:Content>
