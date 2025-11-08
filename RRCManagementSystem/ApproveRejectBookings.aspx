<%@ Page Title="Approve/Reject Bookings" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ApproveRejectBookings.aspx.cs" Inherits="RRCManagementSystem.ApproveRejectBookings" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <script src="https://cdn.tailwindcss.com"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" />
    <style>
        .table-rounded-corners {
            border-collapse: separate;
            border-spacing: 0;
            border-radius: 0.5rem;
            overflow: hidden;
        }

        .table-rounded-corners thead tr:first-child th:first-child {
            border-top-left-radius: 0.5rem;
        }

        .table-rounded-corners thead tr:first-child th:last-child {
            border-top-right-radius: 0.5rem;
        }

        .contract-indicator {
            display: inline-flex;
            align-items: center;
            gap: 4px;
            font-size: 0.875rem;
        }

        .payment-badge {
            display: inline-block;
            padding: 4px 12px;
            border-radius: 12px;
            font-size: 0.75rem;
            font-weight: 600;
        }

        .payment-contract {
            background: #fef3c7;
            color: #92400e;
            border: 1px solid #f59e0b;
        }

        .payment-full {
            background: #d1fae5;
            color: #065f46;
            border: 1px solid #10b981;
        }

        /* Service Details Styling */
        .service-item-row {
            display: flex;
            align-items: center;
            gap: 6px;
            margin-bottom: 4px;
        }

        .service-item-row:last-child {
            margin-bottom: 0;
        }

        .sqm-badge {
            display: inline-block;
            padding: 2px 8px;
            background: #f3e8ff;
            color: #6b21a8;
            border-radius: 10px;
            font-size: 0.75rem;
            font-weight: 600;
        }

        .sqm-total {
            margin-top: 8px;
            padding-top: 6px;
            border-top: 1px solid #e5e7eb;
            font-size: 0.75rem;
            color: #6b7280;
        }
    </style>

    <div class="container mx-auto p-4 md:p-8 bg-white rounded-lg shadow-lg">
        <h3 class="text-2xl font-bold mb-6 text-gray-800 text-center flex items-center justify-center gap-3">
            <i class="fas fa-clipboard-check text-blue-600"></i>
            Approve or Reject Bookings
        </h3>
        
        <div class="overflow-x-auto">
            <asp:GridView ID="gvBookings" runat="server" AutoGenerateColumns="False"
                CssClass="min-w-full bg-white shadow-md rounded-lg table-rounded-corners"
                DataKeyNames="BookingID" OnRowCommand="gvBookings_RowCommand" OnRowDataBound="gvBookings_RowDataBound"
                HeaderStyle-CssClass="bg-blue-600 text-white uppercase text-sm leading-normal"
                RowStyle-CssClass="border-b border-gray-200 hover:bg-gray-100 transition-colors"
                AlternatingRowStyle-CssClass="bg-gray-50 hover:bg-gray-100 transition-colors">

                <Columns>
                 
                    <asp:BoundField DataField="BookingID" HeaderText="Booking ID" Visible="false" />
                    <asp:BoundField DataField="ClientID" HeaderText="Client ID" Visible="false" />
                    
                    <asp:TemplateField HeaderText="Booking Code" HeaderStyle-CssClass="py-3 px-6 text-center" ItemStyle-CssClass="py-3 px-6 text-center">
                        <ItemTemplate>
                            <span class="font-semibold text-blue-700"><%# Eval("BookingCode") %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    

                    <asp:BoundField DataField="ClientName" HeaderText="Client Name" 
                        HeaderStyle-CssClass="py-3 px-6 text-center" 
                        ItemStyle-CssClass="py-3 px-6 text-center font-medium" />
                    

                    <asp:TemplateField HeaderText="Service(s)" HeaderStyle-CssClass="py-3 px-6 text-center" ItemStyle-CssClass="py-3 px-6 text-left">
                        <ItemTemplate>
                            <div class="max-w-xs">
                                <%# FormatServiceDetails(Eval("ServiceDetails"), Eval("ServiceName")) %>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                    

                    <asp:TemplateField HeaderText="Scheduled Date" HeaderStyle-CssClass="py-3 px-6 text-center" ItemStyle-CssClass="py-3 px-6 text-center">
                        <ItemTemplate>
                            <i class="far fa-calendar text-blue-500 mr-1"></i>
                            <%# Eval("ScheduledDate") != DBNull.Value ? Convert.ToDateTime(Eval("ScheduledDate")).ToString("MMM dd, yyyy") : "—" %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    

                    <asp:TemplateField HeaderText="Time" HeaderStyle-CssClass="py-3 px-6 text-center" ItemStyle-CssClass="py-3 px-6 text-center">
                        <ItemTemplate>
                            <i class="far fa-clock text-blue-500 mr-1"></i>
                            <%# FormatTime(Eval("StartTime")) %>
                        </ItemTemplate>
                    </asp:TemplateField>


                    <asp:TemplateField HeaderText="Coverage Area" HeaderStyle-CssClass="py-3 px-6 text-center" ItemStyle-CssClass="py-3 px-6 text-left">
                        <ItemTemplate>
                            <div class="max-w-xs">
                                <%# FormatSQMDetails(Eval("ServiceDetails"), Eval("SQM")) %>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                    

                    <asp:TemplateField HeaderText="Total Price" HeaderStyle-CssClass="py-3 px-6 text-center" ItemStyle-CssClass="py-3 px-6 text-center">
                        <ItemTemplate>
                            <span class="font-bold text-green-700">
                                <%# Eval("Price") != DBNull.Value ? String.Format("₱{0:N2}", Eval("Price")) : "₱0.00" %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    

                    <asp:TemplateField HeaderText="Payment" HeaderStyle-CssClass="py-3 px-6 text-center" ItemStyle-CssClass="py-3 px-6 text-center">
                        <ItemTemplate>
                            <%# GetPaymentBadge(Eval("PaymentPlan")?.ToString(), Convert.ToBoolean(Eval("IsContract") ?? false)) %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    

                    <asp:TemplateField HeaderText="Contract" HeaderStyle-CssClass="py-3 px-6 text-center" ItemStyle-CssClass="py-3 px-6 text-center">
                        <ItemTemplate>
                            <%# GetContractIndicator(Convert.ToBoolean(Eval("HasContract") ?? false)) %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    
                    <asp:TemplateField HeaderText="Status" HeaderStyle-CssClass="py-3 px-6 text-center" ItemStyle-CssClass="py-3 px-6 text-center">
                        <ItemTemplate>
                            <span class="px-3 py-1 rounded-full text-xs font-semibold bg-yellow-100 text-yellow-800">
                                <%# Eval("Status") %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    

                    <asp:TemplateField HeaderText="Actions" HeaderStyle-CssClass="py-3 px-6 text-center" 
                        ItemStyle-CssClass="py-3 px-6 text-center space-x-2 whitespace-nowrap">
                        <ItemTemplate>
                            <asp:Button ID="btnApprove" runat="server" Text="✓ Approve"
                                CssClass="bg-green-500 hover:bg-green-600 text-white font-bold py-2 px-4 rounded transition-colors duration-200 text-sm shadow-md"
                                CommandName="Approve" CommandArgument='<%# Eval("BookingID") %>'
                                OnClientClick="confirmAction('approve', this.name); return false;" />

                            <asp:Button ID="btnReject" runat="server" Text="✗ Reject"
                                CssClass="bg-red-500 hover:bg-red-600 text-white font-bold py-2 px-4 rounded transition-colors duration-200 text-sm shadow-md"
                                CommandName="Reject" CommandArgument='<%# Eval("BookingID") %>'
                                OnClientClick="confirmAction('reject', this.name); return false;" />
                        </ItemTemplate>
                    </asp:TemplateField>

                </Columns>
                
                <EmptyDataTemplate>
                    <div class="text-center py-8">
                        <i class="fas fa-inbox text-6xl text-gray-300 mb-4"></i>
                        <p class="text-gray-500 text-lg font-medium">No pending bookings found</p>
                        <p class="text-gray-400 text-sm mt-2">All bookings have been processed</p>
                    </div>
                </EmptyDataTemplate>
            </asp:GridView>
        </div>

        <asp:Label ID="lblMessage" runat="server" CssClass="text-center block mt-4 font-semibold" />
    </div>

    <script type="text/javascript">
        function confirmAction(action, uniqueId) {
            event.preventDefault();

            const actionText = action === 'approve' ? 'Approve' : 'Reject';
            const confirmColor = action === 'approve' ? '#10b981' : '#ef4444';
            const icon = action === 'approve' ? 'question' : 'warning';

            Swal.fire({
                title: actionText + ' Booking?',
                html: `Are you sure you want to <strong>${action}</strong> this booking?`,
                icon: icon,
                showCancelButton: true,
                confirmButtonColor: confirmColor,
                cancelButtonColor: '#6b7280',
                confirmButtonText: '<i class="fas fa-check mr-2"></i>Yes, ' + actionText.toLowerCase(),
                cancelButtonText: '<i class="fas fa-times mr-2"></i>Cancel'
            }).then((result) => {
                if (result.isConfirmed) {
                    // Show loading
                    Swal.fire({
                        title: 'Processing...',
                        text: 'Please wait',
                        allowOutsideClick: false,
                        didOpen: () => {
                            Swal.showLoading();
                        }
                    });

                    // Trigger postback
                    __doPostBack(uniqueId, '');
                }
            });

            return false;
        }
    </script>
</asp:Content>