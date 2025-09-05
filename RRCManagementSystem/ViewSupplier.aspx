<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ViewSupplier.aspx.cs" Inherits="RRCManagementSystem.ViewSupplier" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
  <!-- Bootstrap (remove if already included in Admin.Master) -->
  <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
  <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
  <!-- SweetAlert2 -->
  <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

  <style>
    .main-content { padding:20px; background:#f4f4f4; min-height:calc(100vh - 100px); }
    .supplier-container { background:#fff; padding:25px; border-radius:8px; box-shadow:0 2px 8px rgba(0,0,0,.1); max-width:1200px; margin:0 auto; }
    .supplier-container h2 { color:#004085; margin-bottom:20px; }
    .table { width:100%; border-collapse:collapse; margin-bottom:20px; font-size:14px;}
    .table th,.table td{ border:1px solid #dee2e6; padding:12px 15px; text-align:left;}
    .table th{ background:#004085; color:#fff; font-weight:bold;}
    .table td{ background:#f8f9fa;}
    #lblMessage{ margin-bottom:15px; display:block; font-size:14px;}
  </style>

  <script type="text/javascript">
      function swalPostBack(el, opts) {
          Swal.fire({
              title: opts.title || 'Are you sure?',
              text: opts.text || 'Proceed?',
              icon: opts.icon || 'question',
              showCancelButton: true,
              confirmButtonText: opts.confirmText || 'Yes',
              cancelButtonText: opts.cancelText || 'Cancel'
          }).then(function (res) {
              if (res.isConfirmed) {
                  // Use the link's original postback script so the correct UniqueID & args are sent
                  var href = el.getAttribute('href');        // e.g. "javascript:__doPostBack('ctl00$MainContent$gvSuppliers$ctl02$btnEdit','')"
                  if (href && href.indexOf('__doPostBack') >= 0) {
                      // safer than eval: call the global function with parsed args
                      var m = href.match(/__doPostBack\('([^']*)','([^']*)'\)/);
                      if (m) { __doPostBack(m[1], m[2]); }
                      else { eval(href); } // fallback
                  } else {
                      // If the link was rendered differently, fall back to eval
                      eval(href);
                  }
              }
          });
          return false; // block default click until confirmed
      }

      function confirmEdit(el) {
          return swalPostBack(el, { title: 'Edit supplier?', icon: 'question', confirmText: 'Edit' });
      }

      function confirmArchive(el) {
          return swalPostBack(el, { title: 'Archive supplier?', text: 'This will move the supplier to archive.', icon: 'warning', confirmText: 'Archive' });
      }

      // unchanged
      function openEmailModal(email, name) {
          var hfEmail = document.getElementById('<%= hfSupplierEmail.ClientID %>');
          var hfName = document.getElementById('<%= hfSupplierName.ClientID %>');
          var lblTo = document.getElementById('<%= lblSendTo.ClientID %>');

          hfEmail.value = email || '';
          hfName.value  = name  || '';
          lblTo.textContent = 'Sending to: ' + (name ? (name + ' <' + email + '>') : email);

          var modal = new bootstrap.Modal(document.getElementById('emailModal'));
          modal.show();
          return false;
      }

      function confirmSendEmail(el) {
          // Call server-side function using __doPostBack after confirmation
          Swal.fire({
              title: 'Send email?',
              icon: 'info',
              confirmButtonText: 'Send',
              showCancelButton: true,
              cancelButtonText: 'Cancel'
          }).then(function (result) {
              if (result.isConfirmed) {
                  // Trigger postback manually
                  __doPostBack('<%= btnSendEmail.UniqueID %>', '');
              }
          });
          return false; // Block default behavior
      }

      function confirmCancelEmail() {
          Swal.fire({
            title: 'Cancel email?',
            text: 'Discard your email content?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Discard',
            cancelButtonText: 'Stay'
          }).then(function (res) {
            if (res.isConfirmed) {
              var modalEl = document.getElementById('emailModal');
              var modal   = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
              modal.hide();
              document.getElementById('<%= txtSubject.ClientID %>').value = '';
              document.getElementById('<%= txtMessageBody.ClientID %>').value = '';
              }
          });
          return false;
      }
  </script>

</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
  <!-- Hidden fields used by JS to pass recipient data to the server -->
  <asp:HiddenField ID="hfSupplierEmail" runat="server" />
  <asp:HiddenField ID="hfSupplierName"  runat="server" />
  <asp:Panel ID="pnlSendEmail" runat="server" Visible="false"></asp:Panel>

  <div class="main-content">
    <div class="supplier-container">
      <h2>Suppliers List</h2>

      <asp:Label ID="lblMessage" runat="server" ForeColor="Red"></asp:Label>

      <asp:GridView ID="gvSuppliers" runat="server" AutoGenerateColumns="False"
        OnRowCommand="gvSuppliers_RowCommand"
        CssClass="table" EmptyDataText="No suppliers found.">
        <Columns>
          <asp:BoundField DataField="SupplierID"   HeaderText="ID" />
          <asp:BoundField DataField="Name"         HeaderText="Name" />
          <asp:BoundField DataField="CompanyName"  HeaderText="Company Name" />
          <asp:BoundField DataField="BusinessType" HeaderText="Business Type" />
          <asp:BoundField DataField="ContactNumber" HeaderText="Contact Number" />
          <asp:BoundField DataField="Email"        HeaderText="Email" />
          <asp:BoundField DataField="Status"       HeaderText="Status" />
          <asp:BoundField DataField="CreatedAt"    HeaderText="Date Added" DataFormatString="{0:yyyy-MM-dd}" />

          <asp:TemplateField HeaderText="Actions">
            <ItemTemplate>
              <!-- Open Email Modal (client-side only) -->
              <a href="#"
                 onclick="return openEmailModal('<%# Eval("Email") %>', '<%# (Eval("Name") ?? "").ToString().Replace("'", "\\'") %>');">
                 Send Email
              </a>
              &nbsp;|&nbsp;

              <!-- Edit (server postback with SweetAlert confirm) -->
              <asp:LinkButton ID="btnEdit" runat="server"
                CommandName="EditSupplier"
                CommandArgument='<%# Eval("SupplierID") %>'
                Text="Edit"
                OnClientClick="return confirmEdit(this);" />
              &nbsp;|&nbsp;

              <!-- Archive (server postback with SweetAlert confirm) -->
              <asp:LinkButton ID="btnArchive" runat="server"
                CommandName="ArchiveSupplier"
                CommandArgument='<%# Eval("SupplierID") %>'
                Text="Archive"
                OnClientClick="return confirmArchive(this);" />
            </ItemTemplate>
          </asp:TemplateField>
        </Columns>
      </asp:GridView>

      <!-- Email Modal (Bootstrap) -->
      <div class="modal fade" id="emailModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-centered">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title">Send Email to Supplier</h5>
              <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>

            <div class="modal-body">
              <asp:Label ID="lblSendTo" runat="server" Text=""></asp:Label>
              <div class="mb-3">
                <asp:TextBox ID="txtSubject" runat="server" CssClass="form-control" placeholder="Subject"></asp:TextBox>
              </div>
              <div class="mb-3">
                <asp:TextBox ID="txtMessageBody" runat="server" TextMode="MultiLine" CssClass="form-control" Rows="8" placeholder="Type your message here..."></asp:TextBox>
              </div>
            </div>

            <div class="modal-footer">
              <!-- IMPORTANT: UseSubmitBehavior=false so __doPostBack works with SweetAlert confirm -->
              <asp:Button ID="btnSendEmail" runat="server" CssClass="btn btn-primary"
                OnClientClick="return confirmSendEmail(this);"
                OnClick="btnSendEmail_Click" Text="Send Email" />
              <button type="button" class="btn btn-secondary" onclick="return confirmCancelEmail();">Cancel</button>
            </div>
          </div>
        </div>
      </div>

    </div>
  </div>
</asp:Content>
