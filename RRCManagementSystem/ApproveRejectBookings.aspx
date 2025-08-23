<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ApproveRejectBookings.aspx.cs" Inherits="RRCManagementSystem.ApproveRejectBookings" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <div class="container py-5">
        <h3 class="text-center fw-bold text-primary mb-4">✅ Approve or Reject Bookings</h3>

        <asp:GridView ID="gvBookings" runat="server" AutoGenerateColumns="False"
    CssClass="table table-bordered table-hover table-striped"
    DataKeyNames="BookingID" OnRowCommand="gvBookings_RowCommand">

    <Columns>
        <asp:BoundField DataField="BookingID" HeaderText="Booking ID" Visible="false" />

        <asp:BoundField DataField="BookingCode" HeaderText="Booking Code" />

        <asp:BoundField DataField="ClientName" HeaderText="Client Name" />
        <asp:BoundField DataField="ServiceName" HeaderText="Service" />
        <asp:BoundField DataField="ScheduledDate" HeaderText="Scheduled Date" DataFormatString="{0:yyyy-MM-dd}" />
        
        <asp:TemplateField HeaderText="Start Time">
            <ItemTemplate>
                <%# Eval("StartTime") != DBNull.Value ? String.Format("{0:hh\\:mm}", Eval("StartTime")) : "—" %>
            </ItemTemplate>
        </asp:TemplateField>

        <asp:BoundField DataField="Status" HeaderText="Status" />

        <asp:TemplateField HeaderText="SQM">
            <ItemTemplate>
                <%# Eval("SQM") != DBNull.Value ? Eval("SQM").ToString() : "0" %>
            </ItemTemplate>
        </asp:TemplateField>

        <asp:TemplateField HeaderText="Price">
            <ItemTemplate>
                <%# Eval("Price") != DBNull.Value ? String.Format("₱{0:N2}", Eval("Price")) : "₱0.00" %>
            </ItemTemplate>
        </asp:TemplateField>

        <asp:TemplateField HeaderText="Actions">
            <ItemTemplate>
                <asp:Button ID="btnApprove" runat="server" Text="Approve" CssClass="btn btn-success btn-sm"
                    CommandName="Approve" CommandArgument='<%# Eval("BookingID") %>'
                    OnClientClick="return confirmAction('approve', this);" />

                <asp:Button ID="btnReject" runat="server" Text="Reject" CssClass="btn btn-danger btn-sm"
                    CommandName="Reject" CommandArgument='<%# Eval("BookingID") %>'
                    OnClientClick="return confirmAction('reject', this);" />
            </ItemTemplate>
        </asp:TemplateField>
    </Columns>
</asp:GridView>


        <asp:Label ID="lblMessage" runat="server" CssClass="text-center d-block mt-3 fw-semibold text-success" />
    </div>

    <script>
        function confirmAction(action, button) {
            event.preventDefault();
            const actionText = action === 'approve' ? 'Approve' : 'Reject';
            const confirmColor = action === 'approve' ? '#28a745' : '#dc3545';

            Swal.fire({
                title: actionText + ' Booking?',
                text: `Are you sure you want to ${action} this booking?`,
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: confirmColor,
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Yes, ' + actionText.toLowerCase()
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack(button.name, '');
                }
            });
            return false;
        }
    </script>
</asp:Content>
