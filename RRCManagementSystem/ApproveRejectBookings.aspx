<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ApproveRejectBookings.aspx.cs" Inherits="RRCManagementSystem.ApproveRejectBookings" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <script src="https://cdn.tailwindcss.com"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" />
    <style>
        .table-rounded-corners {
            border-collapse: separate;
            border-spacing: 0;
            border-radius: 0.5rem; /* rounded-lg */
            overflow: hidden;
        }

        .table-rounded-corners thead tr:first-child th:first-child {
            border-top-left-radius: 0.5rem;
        }

        .table-rounded-corners thead tr:first-child th:last-child {
            border-top-right-radius: 0.5rem;
        }
    </style>

    <div class="container mx-auto p-4 md:p-8 bg-white rounded-lg shadow-lg">
        <h3 class="text-2xl font-bold mb-6 text-gray-800 text-center">✅ Approve or Reject Bookings</h3>
        
        <div class="overflow-x-auto">
            <asp:GridView ID="gvBookings" runat="server" AutoGenerateColumns="False"
                CssClass="min-w-full bg-white shadow-md rounded-lg table-rounded-corners"
                DataKeyNames="BookingID" OnRowCommand="gvBookings_RowCommand"
                HeaderStyle-CssClass="bg-blue-600 text-white uppercase text-sm leading-normal"
                RowStyle-CssClass="border-b border-gray-200 hover:bg-gray-100 transition-colors"
                AlternatingRowStyle-CssClass="bg-gray-50 hover:bg-gray-100 transition-colors">

                <Columns>
                    <asp:BoundField DataField="BookingID" HeaderText="Booking ID" Visible="false" />
                    <asp:BoundField DataField="BookingCode" HeaderText="Booking Code" HeaderStyle-CssClass="py-3 px-6 text-center" ItemStyle-CssClass="py-3 px-6 text-center" />
                    <asp:BoundField DataField="ClientName" HeaderText="Client Name" HeaderStyle-CssClass="py-3 px-6 text-center" ItemStyle-CssClass="py-3 px-6 text-center" />
                    <asp:BoundField DataField="ServiceName" HeaderText="Service" HeaderStyle-CssClass="py-3 px-6 text-center" ItemStyle-CssClass="py-3 px-6 text-center" />
                    <asp:BoundField DataField="ScheduledDate" HeaderText="Scheduled Date" DataFormatString="{0:yyyy-MM-dd}" HeaderStyle-CssClass="py-3 px-6 text-center" ItemStyle-CssClass="py-3 px-6 text-center" />
                    
                    <asp:TemplateField HeaderText="Start Time" HeaderStyle-CssClass="py-3 px-6 text-center" ItemStyle-CssClass="py-3 px-6 text-center">
                        <ItemTemplate>
                            <%# Eval("StartTime") != DBNull.Value ? String.Format("{0:hh\\:mm}", Eval("StartTime")) : "—" %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:BoundField DataField="Status" HeaderText="Status" HeaderStyle-CssClass="py-3 px-6 text-center" ItemStyle-CssClass="py-3 px-6 text-center font-semibold" />
                    
                    <asp:TemplateField HeaderText="SQM" HeaderStyle-CssClass="py-3 px-6 text-center" ItemStyle-CssClass="py-3 px-6 text-center">
                        <ItemTemplate>
                            <%# Eval("SQM") != DBNull.Value ? Eval("SQM").ToString() : "0" %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    
                    <asp:TemplateField HeaderText="Price" HeaderStyle-CssClass="py-3 px-6 text-center" ItemStyle-CssClass="py-3 px-6 text-center">
                        <ItemTemplate>
                            <%# Eval("Price") != DBNull.Value ? String.Format("₱{0:N2}", Eval("Price")) : "₱0.00" %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    
                    <asp:TemplateField HeaderText="Actions" HeaderStyle-CssClass="py-3 px-6 text-center" ItemStyle-CssClass="py-3 px-6 text-center space-x-2 whitespace-nowrap">
                        <ItemTemplate>
                          <asp:Button ID="btnApprove" runat="server" Text="Approve"
    CssClass="bg-green-500 hover:bg-green-600 text-white font-bold py-2 px-4 rounded transition-colors duration-200 text-sm"
    CommandName="Approve" CommandArgument='<%# Eval("BookingID") %>'
    OnClientClick="return confirmAction('approve', '<%# ((Control)Container).FindControl("btnApprove").UniqueID %>');" />

<asp:Button ID="btnReject" runat="server" Text="Reject"
    CssClass="bg-red-500 hover:bg-red-600 text-white font-bold py-2 px-4 rounded transition-colors duration-200 text-sm"
    CommandName="Reject" CommandArgument='<%# Eval("BookingID") %>'
    OnClientClick="return confirmAction('reject', '<%# ((Control)Container).FindControl("btnReject").UniqueID %>');" />

                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

        <asp:Label ID="lblMessage" runat="server" CssClass="text-center block mt-4 font-semibold text-green-500" />
    </div>

    <script type="text/javascript">
        function confirmAction(action, uniqueId) {
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
                    __doPostBack(uniqueId, '');
                }
            });
            return false;
        }
    </script>
</asp:Content>
