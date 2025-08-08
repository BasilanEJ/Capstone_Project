    <%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AddStocks.aspx.cs" Inherits="RRCManagementSystem.AddStocks" %>

    <asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
        <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    </asp:Content>

    <asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
        <style>
            .container { max-width: 800px; margin: 40px auto; padding: 20px; }
            .card { background: #fff; border-radius: 8px; box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1); }
            .card-header { background-color: #007bff; color: white; padding: 16px; font-size: 24px; text-align: center; font-weight: bold; }
            .card-body { padding: 30px; }
            .form-group { margin-bottom: 20px; }
            .form-control { width: 100%; padding: 12px; font-size: 14px; }
            .btn { padding: 12px 24px; font-weight: bold; border: none; border-radius: 4px; font-size: 16px; margin-right: 10px; }
            .btn-primary { background-color: #007bff; color: white; }
            .btn-primary:hover { background-color: #0056b3; }
            .btn-secondary { background-color: #6c757d; color: white; }
            .btn-secondary:hover { background-color: #5a6268; }
            img { width: 120px; height: 120px; object-fit: cover; border-radius: 8px; border: 1px solid #ccc; }
        </style>

        <div class="container">
            <div class="card">
                <div class="card-header">Add Stock</div>
                <div class="card-body">
                    <asp:Label ID="lblMessage" runat="server" CssClass="text-danger fw-semibold mb-3 d-block" />

                    <div class="form-group">
                        <label>Item Name:</label>
                        <asp:Label ID="lblItemName" runat="server" CssClass="form-control" />
                    </div>

                    <div class="form-group">
                        <label>Item Type:</label>
                        <asp:Label ID="lblItemType" runat="server" CssClass="form-control" />
                    </div>

                    <div class="form-group text-center">
                        <asp:Image ID="imgItem" runat="server" />
                    </div>

                    <div class="form-group">
                        <label for="txtAddQty">Quantity to Add:</label>
                        <asp:TextBox ID="txtAddQty" runat="server" CssClass="form-control" placeholder="Enter quantity" oninput="this.value = this.value.replace(/[^0-9]/g, '')" />
                    </div>

                    <div class="form-group text-center">
                        <asp:Button ID="btnSubmit" runat="server" Text="Add Stock" CssClass="btn btn-primary" UseSubmitBehavior="false" OnClick="btnSubmit_Click" OnClientClick="return confirmAdd();" />
                        <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-secondary" OnClientClick="window.location.href='ViewItem.aspx'; return false;" />
                    </div>
                </div>
            </div>
        </div>

        <script>
            function confirmAdd() {
                event.preventDefault();
                Swal.fire({
                    title: 'Confirm Stock Addition',
                    text: 'Are you sure you want to add this quantity?',
                    icon: 'question',
                    showCancelButton: true,
                    confirmButtonColor: '#007bff',
                    cancelButtonColor: '#d33',
                    confirmButtonText: 'Yes, add it!'
                }).then((result) => {
                    if (result.isConfirmed) {
                        __doPostBack('<%= btnSubmit.UniqueID %>', '');
                    }
                });
                return false;
            }
        </script>
    </asp:Content>