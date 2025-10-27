<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ManagePayment.aspx.cs" Inherits="RRCManagementSystem.ManagePayment" %>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
  <style>
    /* Layout hygiene */
    *, *::before, *::after { box-sizing: border-box; }
    .payment-form{max-width:980px;margin:0 auto;background:#fff;padding:28px;border-radius:12px;box-shadow:0 6px 16px rgba(0,0,0,.08)}
    .payment-form h2{margin:0 0 18px;color:#0b3b79;text-align:center}
    .grid-2{display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:14px}
    .grid-3{display:grid;grid-template-columns:repeat(3,minmax(0,1fr));gap:14px}
    @media (max-width: 900px){ .grid-3{grid-template-columns:repeat(2,minmax(0,1fr));} }
    @media (max-width: 640px){ .grid-2,.grid-3{grid-template-columns:1fr;} }

    .section{border:1px solid #e5e7eb;border-radius:10px;padding:16px;margin-top:16px}
    .section h4{margin:0 0 10px;color:#0b3b79}
    .form-group{margin-bottom:12px; min-width:0;}
    .form-label{font-weight:600;display:block;margin-bottom:6px}
    .form-control{width:100%;padding:10px;border:1px solid #d1d5db;border-radius:8px;display:block}
    .muted{color:#6b7280}

    .btn{border:none;border-radius:8px;padding:10px 18px;cursor:pointer;font-weight:600}
    .btn-primary{background:#0b3b79;color:#fff}
    .btn-primary:hover{background:#072a57}
    .btn-light{background:#f3f4f6}

    .message{margin-top:12px;text-align:center;font-weight:700}
    .message.success{color:#0f766e}.message.error{color:#b91c1c}

    .table-mini{width:100%;border-collapse:collapse;margin-top:8px;table-layout:fixed}
    .table-mini th,.table-mini td{border:1px solid #e5e7eb;padding:8px;overflow-wrap:anywhere}
    .table-mini th{background:#f9fafb;text-align:left}

    .pill{display:inline-block;background:#eef2ff;color:#3730a3;border-radius:999px;padding:2px 8px;font-size:12px}
    .req{color:#dc2626}

    .preview{max-width:100%;height:auto;border:1px solid #e5e7eb;border-radius:8px;padding:4px;display:none;cursor:pointer}

    /* Subtle reveal for Payment 2 */
    .reveal-wrap{overflow:hidden;transition:max-height .28s ease, opacity .28s ease, margin-top .28s ease}
    .reveal-wrap.collapsed{max-height:0;opacity:0;margin-top:0}
    .reveal-wrap.expanded{max-height:1200px;opacity:1;margin-top:10px}

    .chk-row{display:flex;align-items:center;gap:8px}
  </style>

  <div class="payment-form">
    <h2>Adjust Client Payment</h2>
    <asp:Label ID="lblMessage" runat="server" CssClass="message" />

    <!-- hidden state -->
    <asp:HiddenField ID="hfClientID" runat="server" />
    <asp:HiddenField ID="hfBookingId" runat="server" />

    <!-- SEARCH CLIENT -->
    <div class="section">
      <h4>Search Client</h4>
      <div class="grid-2">
        <div class="form-group">
          <label class="form-label">Name or ClientNumber</label>
          <asp:TextBox ID="txtClientSearch" runat="server" CssClass="form-control" placeholder="e.g. Delara, Trisha or CL-2025" />
        </div>
        <div class="form-group" style="display:flex;align-items:flex-end;gap:8px">
          <asp:Button ID="btnSearchClient" runat="server" Text="Search" CssClass="btn btn-primary" OnClick="btnSearchClient_Click" />
          <asp:Button ID="btnClearClient" runat="server" Text="Clear" CssClass="btn btn-light" OnClick="btnClearClient_Click" />
        </div>
      </div>

      <!-- results -->
      <asp:Panel ID="pnlResults" runat="server" Visible="false">
  <table class="table-mini">
    <thead>
      <tr>
        <th>Client</th>
        <th class="muted">Client No.</th>
        <th></th>
      </tr>
    </thead>
    <tbody>
      <asp:Repeater ID="rpResults" runat="server" OnItemCommand="rpResults_ItemCommand">
        <ItemTemplate>
          <tr>
            <td><%# Eval("DisplayName") %></td>
            <td class="muted">#<%# Eval("ClientNumber") %></td>
            <td>
              <asp:LinkButton runat="server" 
                              CommandName="Pick" 
                              CommandArgument='<%# Eval("ClientID") %>' 
                              CssClass="pill">
                Select
              </asp:LinkButton>
            </td>
          </tr>
        </ItemTemplate>
      </asp:Repeater>
    </tbody>
  </table>
</asp:Panel>


      <!-- selected -->
      <asp:Panel ID="pnlChosen" runat="server" Visible="false" style="margin-top:8px">
        <span class="muted">Selected:</span>
        <asp:Label ID="lblChosen" runat="server" CssClass="pill" />
      </asp:Panel>
    </div>

    <!-- BALANCE -->
    <div class="section">
      <div class="form-group">
        <label class="form-label">Remaining Balance</label>
        <asp:TextBox ID="txtRemainingBalance" runat="server" CssClass="form-control" ReadOnly="true" />
      </div>
    </div>

    <!-- PAYMENT 1 (required) -->
    <div class="section">
      <h4>Payment 1</h4>
      <div class="grid-3">
        <div class="form-group">
          <label class="form-label">Payment Method <span class="req">*</span></label>
          <asp:DropDownList ID="ddlMethod1" runat="server" CssClass="form-control" />
        </div>
       <div class="form-group">
  <label class="form-label">Amount <span class="req">*</span></label>
  <asp:TextBox ID="txtAmount1" runat="server" CssClass="form-control"
               TextMode="Number" step="0.01" />
  <asp:RangeValidator ID="rvAmount1" runat="server"
      ControlToValidate="txtAmount1"
      MinimumValue="500" MaximumValue="9999999"
      Type="Double"
      ErrorMessage="⚠ Amount must be at least 500."
      CssClass="text-danger" Display="Dynamic" />
</div>
        <div class="form-group">
          <label class="form-label">Receipt <span class="req">*</span></label>
          <asp:FileUpload ID="fuReceipt1" runat="server" CssClass="form-control" onchange="preview(this,'imgPrev1')" />
          <img id="imgPrev1" class="preview" alt="Receipt 1" onclick="openImg(this)" />
        </div>
      </div>
      <div class="form-group">
        <label class="form-label">Remarks (optional)</label>
        <asp:TextBox ID="txtRemarks1" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" />
      </div>
    </div>

    <!-- PAYMENT 2 (optional) -->
    <div class="section">
      <h4>Payment 2 (optional)</h4>

      <!-- ASP.NET checkbox (no AutoPostBack) so code-behind can read it, but it won't clear file inputs -->
      <div class="form-group chk-row">
        <asp:CheckBox ID="chkUseSecond" runat="server" ClientIDMode="Static" />
        <label for="chkUseSecond" class="muted">Use a second payment (method, amount, and receipt become required)</label>
      </div>

      <div id="p2Fields" class="reveal-wrap collapsed">
        <div class="grid-3">
          <div class="form-group">
            <label class="form-label">Payment Method 2</label>
            <asp:DropDownList ID="ddlMethod2" runat="server" CssClass="form-control" />
          </div>
       <div class="form-group">
  <label class="form-label">Amount 2</label>
  <asp:TextBox ID="txtAmount2" runat="server" CssClass="form-control"
               TextMode="Number" step="0.01" />
  <asp:RangeValidator ID="rvAmount2" runat="server"
      ControlToValidate="txtAmount2"
      MinimumValue="500" MaximumValue="9999999"
      Type="Double"
      ErrorMessage="⚠ Amount must be at least 500."
      CssClass="text-danger" Display="Dynamic" />
</div>
          <div class="form-group">
            <label class="form-label">Receipt 2</label>
            <asp:FileUpload ID="fuReceipt2" runat="server" CssClass="form-control" onchange="preview(this,'imgPrev2')" />
            <img id="imgPrev2" class="preview" alt="Receipt 2" onclick="openImg(this)" />
          </div>
        </div>
        <div class="form-group">
          <label class="form-label">Remarks 2 (optional)</label>
          <asp:TextBox ID="txtRemarks2" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" />
        </div>
      </div>
    </div>

    <!-- SUMMARY (Total below Remarks 2) -->
    <div class="section">
      <h4>Summary</h4>
      <div class="grid-2">
        <div class="form-group">
          <label class="form-label">Total Amounts</label>
          <asp:TextBox ID="txtTotalPayment" runat="server" CssClass="form-control" ReadOnly="true" />
        </div>
        <div class="form-group"><!-- reserved --></div>
      </div>
      <p class="muted" style="margin-top:6px">
        Total Payment = Payment 1 Amount + Payment 2 Amount (if used).
      </p>
    </div>

    <div style="text-align:center;margin-top:16px">
      <asp:Button ID="btnSaveReal" runat="server" Text="Save (Hidden)" CssClass="btn btn-primary" OnClick="btnSave_Click" Style="display:none" />
      <button type="button" class="btn btn-primary" onclick="confirmSave()">Save Changes</button>
      <asp:Button ID="btnPrintReceipt" runat="server" Text="🖸 Print Last Receipt" CssClass="btn btn-light" Visible="false" OnClick="btnPrintReceipt_Click" />
      <asp:Button ID="btnDownloadReceipt" runat="server" Text="💾 Download Decrypted" CssClass="btn btn-light" Visible="false" OnClick="btnDownloadReceipt_Click" />
    </div>
  </div>

  <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
  <script>
      // ----------- Image preview helpers -----------
      function preview(input, imgId) {
          if (!input.files || !input.files[0]) return;
          var f = input.files[0]; var ok = ["image/jpeg", "image/png"].includes(f.type);
          if (!ok) { Swal.fire("Invalid file", "JPG/PNG only.", "error"); input.value = ""; return; }
          var r = new FileReader();
          r.onload = e => { var img = document.getElementById(imgId); img.src = e.target.result; img.style.display = "block"; }
          r.readAsDataURL(f);
      }
      function openImg(img) { if (img && img.src) window.open(img.src, '_blank'); }

      // ----------- Save confirm -----------
      function confirmSave() {
          Swal.fire({ title: "Save payment?", text: "We will record 1 or 2 transactions.", icon: "warning", showCancelButton: true, confirmButtonText: "Yes, save it", confirmButtonColor: "#0b3b79" })
              .then(r => { if (r.isConfirmed) document.getElementById('<%= btnSaveReal.ClientID %>').click(); });
      }

      // ----------- Payment 2 subtle toggle (no postback) -----------
      function setP2Enabled(on) {
          var wrap = document.getElementById('p2Fields');
          wrap.classList.toggle('expanded', on);
          wrap.classList.toggle('collapsed', !on);
          // enable/disable inputs visually
          ['<%= ddlMethod2.ClientID %>','<%= txtAmount2.ClientID %>','<%= fuReceipt2.ClientID %>','<%= txtRemarks2.ClientID %>']
        .forEach(function(cid){
          var el = document.getElementById(cid);
          if(!el) return;
          el.disabled = !on;
          var fg = el.closest('.form-group'); if (fg) fg.style.opacity = on ? '1' : '0.6';
        });
      recalcTotal();
    }

    document.addEventListener('DOMContentLoaded', function(){
      var chk = document.getElementById('chkUseSecond'); // ClientIDMode="Static"
      chk.addEventListener('change', function(){ setP2Enabled(chk.checked); });

      // start collapsed
      setP2Enabled(chk.checked);

      // Live total wiring
      document.getElementById('<%= txtAmount1.ClientID %>').addEventListener('input', recalcTotal);
      document.getElementById('<%= txtAmount2.ClientID %>').addEventListener('input', recalcTotal);
      recalcTotal();
    });

    // ----------- Live total -----------
    function recalcTotal(){
      var a1 = parseFloat(document.getElementById('<%= txtAmount1.ClientID %>').value) || 0;
      var p2On = document.getElementById('chkUseSecond').checked;
      var a2 = p2On ? (parseFloat(document.getElementById('<%= txtAmount2.ClientID %>').value) || 0) : 0;
      document.getElementById('<%= txtTotalPayment.ClientID %>').value = (a1 + a2).toFixed(2);
      }
  </script>
</asp:Content>
