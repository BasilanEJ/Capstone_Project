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
    <asp:HiddenField ID="hfConfirmed" runat="server" Value="false" ClientIDMode="Static" />

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
      <asp:FileUpload ID="fuReceipt" runat="server" CssClass="control" ClientIDMode="Static" />
      <div class="hint">Files are stored securely. Only the filename is saved in the database.</div>
    </div>

    <div class="btn-row">
      <!-- Hidden button that actually submits -->
      <asp:Button ID="btnSaveReal" runat="server" Text="Save Receipt" CssClass="btn btn-primary" 
                  OnClick="btnSave_Click" Style="display:none;" ClientIDMode="Static" />
      
      <!-- Visible button that triggers confirmation -->
      <button type="button" class="btn btn-primary" onclick="confirmUpload()">Save Receipt</button>
      
      <a class="btn btn-light" href="TransactionHistory.aspx">Back to Transaction History</a>
    </div>
  </div>
</div>

<script>
    // ✅ Function to view existing receipt in modal
    function viewReceipt(url, fileName) {
        const ext = fileName.toLowerCase().split('.').pop();
        const isPdf = ext === 'pdf';
        const isMobile = window.innerWidth <= 768;

        let htmlContent = '';

        if (isPdf) {
            htmlContent = `
                <div style="width:100%;max-height:75vh;overflow:auto;display:flex;justify-content:center;background:#f3f4f6;border-radius:8px;padding:15px;">
                    <embed src="${url}" type="application/pdf" style="width:100%;height:70vh;min-height:500px;border:none;border-radius:4px;" />
                </div>
            `;
        } else {
            htmlContent = `
                <div style="width:100%;max-height:75vh;overflow:auto;display:flex;justify-content:center;background:#f3f4f6;border-radius:8px;padding:15px;">
                    <img src="${url}" alt="Receipt" loading="lazy" style="max-width:100%;height:auto;max-height:70vh;object-fit:contain;box-shadow:0 4px 6px rgba(0,0,0,0.1);border-radius:4px;" />
                </div>
            `;
        }

        Swal.fire({
            title: '📄 Current Receipt',
            html: htmlContent,
            width: isMobile ? '98%' : '90%',
            showCloseButton: true,
            showConfirmButton: true,
            confirmButtonText: isMobile ? 'Close' : 'Close Receipt',
            confirmButtonColor: '#2563eb'
        });

        return false;
    }

    // ✅ Function to confirm upload
    function confirmUpload() {
        var fileUpload = document.getElementById('fuReceipt');

        // Check if file selected
        if (!fileUpload.files || fileUpload.files.length === 0) {
            Swal.fire({
                icon: 'warning',
                title: 'No File Selected',
                text: 'Please select a receipt file to upload.',
                confirmButtonColor: '#2563eb'
            });
            return false;
        }

        // Validate file type
        var fileName = fileUpload.files[0].name.toLowerCase();
        var validExtensions = ['.jpg', '.jpeg', '.png', '.pdf'];
        var isValid = validExtensions.some(function (ext) {
            return fileName.endsWith(ext);
        });

        if (!isValid) {
            Swal.fire({
                icon: 'error',
                title: 'Invalid File Type',
                text: 'Only JPG, PNG, or PDF files are allowed.',
                confirmButtonColor: '#2563eb'
            });
            return false;
        }

        // Show confirmation dialog
        Swal.fire({
            title: 'Upload Receipt?',
            text: 'Do you want to upload this receipt?',
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#2563eb',
            cancelButtonColor: '#6b7280',
            confirmButtonText: 'Yes, upload it!',
            cancelButtonText: 'Cancel'
        }).then((result) => {
            if (result.isConfirmed) {
                // Set confirmation flag and trigger postback
                document.getElementById('hfConfirmed').value = 'true';
                document.getElementById('btnSaveReal').click();
            }
        });

        return false;
    }
</script>

</asp:Content>