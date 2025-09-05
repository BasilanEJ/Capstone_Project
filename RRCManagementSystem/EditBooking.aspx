<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="EditBooking.aspx.cs" Inherits="RRCManagementSystem.EditBooking" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Bootstrap 5 CSS -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <!-- SweetAlert2 -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="container my-5" style="max-width:700px;">
        <div class="card shadow-sm">
            <div class="card-header bg-primary text-white text-center fs-4 fw-bold">
                Edit Booking Status
            </div>
            <div class="card-body">

                <asp:Label ID="lblMessage" runat="server" CssClass="text-danger fw-semibold d-block mb-3" />

                <!-- Booking ID -->
                <div class="mb-3">
                    <label class="form-label fw-semibold">Booking ID:</label>
                    <asp:Label ID="lblBookingID" runat="server" CssClass="form-control-plaintext" />
                </div>

                <!-- Client Name -->
                <div class="mb-3">
                    <label class="form-label fw-semibold">Client Name:</label>
                    <asp:TextBox ID="txtClientName" runat="server" CssClass="form-control" ReadOnly="true" />
                </div>

                <!-- Service Name -->
                <div class="mb-3">
                    <label class="form-label fw-semibold">Service Name:</label>
                    <asp:TextBox ID="txtServiceName" runat="server" CssClass="form-control" ReadOnly="true" />
                </div>

                <!-- Scheduled Date -->
                <div class="mb-3">
                    <label class="form-label fw-semibold">Scheduled Date:</label>
                    <asp:TextBox ID="txtScheduledDate" runat="server" CssClass="form-control" ReadOnly="true" />
                </div>

                <!-- Start Time -->
                <div class="mb-3">
                    <label class="form-label fw-semibold">Start Time:</label>
                    <asp:TextBox ID="txtStartTime" runat="server" CssClass="form-control" ReadOnly="true" />
                </div>

                <!-- Status Dropdown -->
             <div class="mb-3">
    <label class="form-label fw-semibold">Status:</label>
    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select">
        <asp:ListItem Text="Pending" Value="Pending" />
        <asp:ListItem Text="Rejected" Value="Rejected" />
        <asp:ListItem Text="Approved" Value="Approved" />
        <asp:ListItem Text="Assigned" Value="Assigned" />
        <asp:ListItem Text="Completed" Value="Completed" />
    </asp:DropDownList>
</div>

                <div class="mb-4">
                    <label class="form-label fw-semibold">Notes:</label>
                    <asp:TextBox ID="txtNotes" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" ReadOnly="true" />
                </div>

                <!-- Buttons -->
                <div class="d-flex justify-content-center gap-3">
                    <asp:Button ID="btnSave" runat="server" Text="Save Status" CssClass="btn btn-success px-4"
                        OnClientClick="return confirmSave();" OnClick="btnSave_Click" />

                    <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-secondary px-4"
                        OnClientClick="window.location.href='AllBooking.aspx'; return false;" />
                </div>

            </div>
        </div>
    </div>

    <script>
        function confirmSave() {
            event.preventDefault(); // prevent form submit
            Swal.fire({
                title: 'Are you sure?',
                text: "Do you want to save the changes?",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#28a745',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes, save it!',
                cancelButtonText: 'Cancel'
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack('<%= btnSave.UniqueID %>', '');
                }
            });
            return false;
        }
    </script>

    <!-- Bootstrap 5 JS Bundle -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>

</asp:Content>
