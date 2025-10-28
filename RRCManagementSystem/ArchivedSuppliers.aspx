<%@ Page Title="Archived Suppliers" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ArchivedSuppliers.aspx.cs" Inherits="RRCManagementSystem.ArchivedSuppliers" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<!-- SweetAlert2 -->
<script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

<style>
    .page-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 2rem;
        flex-wrap: wrap;
        gap: 1rem;
    }
    
    .page-title {
        font-size: 1.875rem;
        font-weight: 700;
        color: #1e293b;
    }
    
    .btn-back {
        padding: 0.625rem 1.25rem;
        background: #64748b;
        color: white;
        border: none;
        border-radius: 8px;
        font-weight: 600;
        text-decoration: none;
        transition: all 0.3s ease;
    }
    
    .btn-back:hover {
        background: #475569;
        transform: translateY(-2px);
    }
    
    .grid-container {
        background: white;
        border-radius: 12px;
        box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05);
        overflow: hidden;
    }
    
    .table {
        width: 100%;
        border-collapse: collapse;
    }
    
    .table th {
        background: #1e40af;
        color: white;
        padding: 1rem;
        text-align: left;
        font-weight: 600;
        font-size: 0.875rem;
        text-transform: uppercase;
        letter-spacing: 0.05em;
    }
    
    .table td {
        padding: 1rem;
        border-bottom: 1px solid #e2e8f0;
        font-size: 0.9375rem;
        color: #334155;
    }
    
    .table tr:hover {
        background: #f8fafc;
    }
    
    .action-btn {
        padding: 0.5rem 1rem;
        border: none;
        border-radius: 6px;
        font-weight: 600;
        font-size: 0.875rem;
        cursor: pointer;
        transition: all 0.3s ease;
        margin-right: 0.5rem;
    }
    
    .btn-restore {
        background: #10b981;
        color: white;
    }
    
    .btn-restore:hover {
        background: #059669;
        transform: translateY(-2px);
    }
    
    .btn-delete {
        background: #ef4444;
        color: white;
    }
    
    .btn-delete:hover {
        background: #dc2626;
        transform: translateY(-2px);
    }
    
    .status-badge {
        padding: 0.375rem 0.75rem;
        border-radius: 999px;
        font-size: 0.8125rem;
        font-weight: 600;
        background: #fee2e2;
        color: #991b1b;
    }
    
    .empty-state {
        text-align: center;
        padding: 3rem;
        color: #64748b;
        font-size: 1.125rem;
    }
</style>

<div class="page-header">
    <h2 class="page-title">🗄️ Archived Suppliers</h2>
    <a href="ViewSupplier.aspx" class="btn-back">← Back to Active Suppliers</a>
</div>

<div class="grid-container">
    <asp:GridView ID="gvArchivedSuppliers" runat="server" 
        AutoGenerateColumns="False"
        CssClass="table"
        GridLines="None"
        OnRowCommand="gvArchivedSuppliers_RowCommand"
        EmptyDataText="No archived suppliers found.">
        <Columns>
            <asp:BoundField DataField="SupplierID" HeaderText="ID" />
            <asp:BoundField DataField="Name" HeaderText="Name" />
            <asp:BoundField DataField="CompanyName" HeaderText="Company" />
            <asp:BoundField DataField="BusinessType" HeaderText="Type" />
            <asp:BoundField DataField="ContactNumber" HeaderText="Contact" />
            <asp:BoundField DataField="Email" HeaderText="Email" />
            
            <asp:TemplateField HeaderText="Status">
                <ItemTemplate>
                    <span class="status-badge">Archived</span>
                </ItemTemplate>
            </asp:TemplateField>
            
            <asp:TemplateField HeaderText="Actions">
                <ItemTemplate>
                    <asp:Button ID="btnRestore" runat="server"
                        Text="Restore"
                        CssClass="action-btn btn-restore"
                        CommandName="RestoreSupplier"
                        CommandArgument='<%# Eval("SupplierID") %>'
                        OnClientClick='<%# "return confirmRestore(" + Eval("SupplierID") + ", \"" + Eval("Name") + "\");" %>' />
                    
                    <asp:Button ID="btnDelete" runat="server"
                        Text="Delete"
                        CssClass="action-btn btn-delete"
                        CommandName="DeleteSupplier"
                        CommandArgument='<%# Eval("SupplierID") %>'
                        OnClientClick='<%# "return confirmDelete(" + Eval("SupplierID") + ", \"" + Eval("Name") + "\");" %>' />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <EmptyDataTemplate>
            <div class="empty-state">
                <i class="fas fa-archive" style="font-size: 3rem; margin-bottom: 1rem; color: #cbd5e1;"></i>
                <p>No archived suppliers found.</p>
            </div>
        </EmptyDataTemplate>
    </asp:GridView>
</div>

<script>
    // Helper function for showAlert (if not already defined globally)
    function showAlert(title, text, icon) {
        Swal.fire({
            icon: icon,
            title: title,
            text: text,
            confirmButtonColor: '#2563eb'
        });
    }

    // Confirmation before restoring
    function confirmRestore(supplierId, supplierName) {
        event.preventDefault();

        Swal.fire({
            title: 'Restore Supplier?',
            html: `Do you want to restore <strong>${supplierName}</strong> to the active list?`,
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#10b981',
            cancelButtonColor: '#6b7280',
            confirmButtonText: 'Yes, restore it!',
            cancelButtonText: 'Cancel'
        }).then((result) => {
            if (result.isConfirmed) {
                // Trigger the postback
                __doPostBack('ctl00$MainContent$gvArchivedSuppliers', 'RestoreSupplier$' + supplierId);
            }
        });

        return false;
    }

    // Confirmation before deleting permanently
    function confirmDelete(supplierId, supplierName) {
        event.preventDefault();

        Swal.fire({
            title: 'Permanently Delete?',
            html: `Are you sure you want to <strong>permanently delete</strong> ${supplierName}?<br><br><span style="color: #dc2626;">This action cannot be undone!</span>`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#dc2626',
            cancelButtonColor: '#6b7280',
            confirmButtonText: 'Yes, delete permanently!',
            cancelButtonText: 'Cancel',
            input: 'checkbox',
            inputPlaceholder: 'I understand this cannot be undone'
        }).then((result) => {
            if (result.isConfirmed) {
                if (!result.value) {
                    Swal.fire({
                        icon: 'error',
                        title: 'Confirmation Required',
                        text: 'Please check the confirmation box to proceed.',
                        confirmButtonColor: '#2563eb'
                    });
                } else {
                    // Trigger the postback
                    __doPostBack('ctl00$MainContent$gvArchivedSuppliers', 'DeleteSupplier$' + supplierId);
                }
            }
        });

        return false;
    }
</script>

</asp:Content>