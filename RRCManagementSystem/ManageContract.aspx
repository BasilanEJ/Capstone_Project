<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ManageContract.aspx.cs" Inherits="RRCManagementSystem.ManageContract" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Bootstrap 5 CSS -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <!-- SweetAlert2 CSS -->
    <link href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container py-5">
        <div class="mx-auto" style="max-width: 700px;">
            <h2 class="text-center mb-4 text-primary fw-semibold">Upload New Client Contract</h2>

            <asp:Label ID="lblMessage" runat="server" CssClass="form-text text-center mb-3" />

            <div class="mb-3">
                <label for="<%= ddlClients.ClientID %>" class="form-label fw-semibold">Client</label>
                <asp:DropDownList ID="ddlClients" runat="server" CssClass="form-select" />
            </div>

            <div class="mb-3">
                <label for="<%= fuContract.ClientID %>" class="form-label fw-semibold">Contract File (PDF Only)</label>
                <asp:FileUpload ID="fuContract" runat="server" CssClass="form-control" accept=".pdf" />
            </div>

            <div class="mb-3">
                <label for="<%= txtStartDate.ClientID %>" class="form-label fw-semibold">Start Date</label>
                <asp:TextBox ID="txtStartDate" runat="server" CssClass="form-control" TextMode="Date" />
            </div>

            <div class="mb-3">
                <label for="<%= txtEndDate.ClientID %>" class="form-label fw-semibold">End Date</label>
                <asp:TextBox ID="txtEndDate" runat="server" CssClass="form-control" TextMode="Date" />
            </div>

            <div class="mb-4">
                <label for="<%= txtRemarks.ClientID %>" class="form-label fw-semibold">Remarks</label>
                <asp:TextBox ID="txtRemarks" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" />
            </div>

            <asp:Button ID="btnUpload" runat="server" Text="Upload Contract" CssClass="btn btn-primary w-100"
                OnClick="btnUpload_Click" OnClientClick="return confirmUpload();" />
        </div>
    </div>

    <!-- Bootstrap 5 JS Bundle -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <!-- SweetAlert2 JS -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.all.min.js"></script>

    <script type="text/javascript">
        function confirmUpload() {
            event.preventDefault(); // prevent immediate postback
            Swal.fire({
                title: 'Are you sure?',
                text: "Do you want to upload this contract?",
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Yes, upload it!',
                cancelButtonText: 'Cancel'
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack('<%= btnUpload.UniqueID %>', '');
                }
            });
            return false;
        }
    </script>
</asp:Content>
