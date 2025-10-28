<%@ Page Title="Add Receipt" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true"
    CodeBehind="AddReceipt.aspx.cs" Inherits="RRCManagementSystem.AddReceipt" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<!-- Add SweetAlert2 -->
<script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

<style>
    body { font-family: 'Segoe UI', sans-serif; }
    .page-title { text-align:center; color:#1e293b; font-size:26px; font-weight:600; margin:30px 0 20px; }
    .message-label { text-align:center; font-weight:600; margin-bottom:15px; }
    .msg-ok { color:#059669; } .msg-err { color:#b91c1c; }

    .grid-container { width:95%; margin:0 auto 50px auto; background:#fff; border-radius:10px; 
        box-shadow:0 4px 12px rgba(0,0,0,.05); overflow:hidden; }
    .inner { padding:20px; }

    .form-grid { display:grid; grid-template-columns:1fr 1fr; gap:16px; }
    @media (max-width: 768px){ .form-grid { grid-template-columns:1fr; } }

    .form-group { margin-bottom:12px; padding: 10px; }
    .label { display:block; font-weight:600; margin-bottom:6px; color:#1f2937; }
    .control, .static { width:100%; padding:10px; border:1px solid #e5e7eb; border-radius:8px; }
    .static { background:#f9fafb; color:#374151; }

    .btn-row { display:flex; gap:10px; justify-content:center; margin-top:14px; }
    .btn { padding:10px 18px; border:none; border-radius:8px; font-weight:700; cursor:pointer; }
    .btn-primary { background:#2563eb; color:#fff; }
    .btn-primary:hover { background:#1d4ed8; }
    .btn-light { background:#f3f4f6; color:#111827; }
    .hint { color:#6b7280; font-size:13px; margin-top:6px; }

    .pill {display:inline-block;border-radius:999px;padding:4px 10px;font-size:12px;font-weight:600;text-decoration:none}
    .pill-view {background:#e0e7ff;color:#1e3a8a;border:1px solid #c7d2fe;}
    .pill-add  {background:#fff7ed;color:#9a3412;border:1px solid #fed7aa;}
</style>

<h2 class="page-title">🧾 Add Receipt</h2>

<asp:Label ID="lblMessage" runat="server" CssClass="message-label"></asp:Label>

<div class="grid-container">
  <div class="inner">

    <asp:HiddenField ID="hfTransactionID" runat="server" />
    <asp:HiddenField ID="hfConfirmed" runat="server" Value="false" />

    <div class="form-grid">
      <div class="form-group">
        <span class="label">Transaction ID</span>
        <asp:TextBox ID="txtTxId" runat="server" CssClass="static" ReadOnly="true" />
      </div>
      <div class="form-group">
        <span class="label">Sale ID</span>
        <asp:TextBox ID="txtSaleId" runat="server" CssClass="static" ReadOnly="true" />
      </div>
      <div class="form-group">
        <span class="label">Amount</span>
        <asp:TextBox ID="txtAmount" runat="server" CssClass="static" ReadOnly="true" />
      </div>
      <div class="form-group">
        <span class="label">Payment Method</span>
        <asp:TextBox ID="txtMethod" runat="server" CssClass="static" ReadOnly="true" />
      </div>
      <div class="form-group">
        <span class="label">Transaction Date</span>
        <asp:TextBox ID="txtDate" runat="server" CssClass="static" ReadOnly="true" />
      </div>
      <div class="form-group">
        <span class="label">Current Receipt</span>
        <asp:PlaceHolder ID="phReceipt" runat="server" />
        <div class="hint">If a receipt already exists, uploading will replace it.</div>
      </div>
    </div>

    <div class="form-group" style="margin-top:8px;">
      <span class="label">Upload Receipt (JPG/PNG/PDF)</span>
      <asp:FileUpload ID="fuReceipt" runat="server" CssClass="control" />
      <div class="hint">Files are stored encrypted on disk. Only the relative path is saved in the database.</div>
    </div>

    <div class="btn-row">
      <asp:Button ID="btnSave" runat="server" Text="Save Receipt" CssClass="btn btn-primary" OnClick="btnSave_Click" OnClientClick="return confirmUpload();" />
      <a class="btn btn-light" href="TransactionHistory.aspx">Back to Transaction History</a>
    </div>
  </div>
</div>

<script>
    function confirmUpload() {
        var fileUpload = document.getElementById('<%= fuReceipt.ClientID %>');
        var confirmed = document.getElementById('<%= hfConfirmed.ClientID %>');
        
        if (!fileUpload.files || fileUpload.files.length === 0) {
            Swal.fire({
                icon: 'warning',
                title: 'No File Selected',
                text: 'Please select a receipt file to upload.',
                confirmButtonColor: '#2563eb'
            });
            return false;
        }
        
        if (confirmed.value === 'true') {
            confirmed.value = 'false';
            return true;
        }
        
        Swal.fire({
            title: 'Upload Receipt?',
            text: 'Do you want to upload this receipt to the client?',
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#2563eb',
            cancelButtonColor: '#6b7280',
            confirmButtonText: 'Yes, upload it!',
            cancelButtonText: 'Cancel'
        }).then((result) => {
            if (result.isConfirmed) {
                confirmed.value = 'true';
                document.getElementById('<%= btnSave.ClientID %>').click();
            }
        });
        
        return false;
    }
</script>

</asp:Content>