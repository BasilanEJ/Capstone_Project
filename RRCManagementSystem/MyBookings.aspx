<%@ Page Title="" Language="C#" MasterPageFile="~/Client.master" AutoEventWireup="true" CodeBehind="MyBookings.aspx.cs" Inherits="RRCManagementSystem.MyBookings" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .container {
            padding: 30px;
            background-color: #ffffff;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
        }
        h3 { color: #004085; margin-bottom: 20px; }
        .table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
        }
        .table th, .table td {
            padding: 12px;
            text-align: center;
            border: 1px solid #dee2e6;
        }
        .table th { background-color: #004085; color: white; }
        .table-striped tbody tr:nth-child(odd) { background-color: #f9f9f9; }
        .message-info {
            background-color: #d4edda;
            color: #155724;
            padding: 10px 15px;
            border-radius: 6px;
            margin-top: 10px;
        }
        .modal-overlay {
            position: fixed;
            top: 0; left: 0;
            width: 100%; height: 100%;
            background: rgba(0,0,0,0.5);
            display: none;
            justify-content: center;
            align-items: center;
            z-index: 9999;
        }
        .modal-box {
            background: #fff;
            padding: 25px;
            border-radius: 10px;
            width: 400px;
            text-align: center;
        }

        /* Responsive layout for tablets and below */
@media (max-width: 768px) {
    .container {
        padding: 15px;
    }

    h3 {
        font-size: 20px;
    }

    .table th, .table td {
        font-size: 13px;
        padding: 8px;
    }

    .btn {
        font-size: 13px;
        padding: 6px 12px;
    }

    .modal-box {
        width: 90%;
        padding: 20px;
    }

    .modal-box h4 {
        font-size: 18px;
        margin-bottom: 15px;
    }

    .form-control {
        font-size: 14px;
        margin-top: 10px;
    }
}

/* Responsive layout for mobile phones */
@media (max-width: 480px) {
    .container {
        padding: 10px;
    }

    h3 {
        font-size: 18px;
    }

    .table th, .table td {
        font-size: 12px;
        padding: 6px;
    }

    .btn {
        font-size: 12px;
        padding: 5px 10px;
    }

    .modal-box {
        width: 95%;
        padding: 15px;
    }

    .modal-box h4 {
        font-size: 16px;
    }

    .form-control {
        font-size: 13px;
        padding: 8px;
    }

    .message-info {
        font-size: 13px;
    }
}

    </style>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>


    <div class="container">
        <h3>My Bookings</h3>
        <asp:Label ID="lblMessage" runat="server" ForeColor="Red" />

        <!-- Bookings Grid -->
        <asp:GridView ID="gvMyBookings" runat="server" AutoGenerateColumns="False" CssClass="table"
            AllowPaging="True" PageSize="10"
            OnPageIndexChanging="gvMyBookings_PageIndexChanging"
            OnRowDataBound="gvMyBookings_RowDataBound">
            <Columns>
                <asp:BoundField DataField="BookingID" HeaderText="Booking ID" />
                <asp:BoundField DataField="ServiceNames" HeaderText="Service" />
                <asp:BoundField DataField="ScheduledDate" HeaderText="Initial Date" DataFormatString="{0:yyyy-MM-dd}" />
                <asp:BoundField DataField="StartTime" HeaderText="Start Time" />
                <asp:BoundField DataField="Status" HeaderText="Status" />
                <asp:BoundField DataField="Notes" HeaderText="Notes" />
                <asp:BoundField DataField="CreatedAt" HeaderText="Date Booked" DataFormatString="{0:yyyy-MM-dd}" />
                <asp:TemplateField HeaderText="Actions">
                    <ItemTemplate>
                        <asp:Button 
                            ID="btnCancel" 
                            runat="server" 
                            Text="Cancel" 
                            CommandName="CancelBooking" 
                            CommandArgument='<%# Eval("BookingID") %>' 
                            CssClass="btn btn-danger btn-sm"
                            Visible='<%# Eval("Status").ToString() == "Pending" %>'
                            OnClientClick="return confirm('Are you sure you want to cancel this booking?');" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>

        <!-- Next Operation Reminder -->
        <asp:Label ID="lblNextOperationNotice" runat="server" CssClass="message-info" Visible="false" />

        <!-- Upcoming Operations -->
        <asp:Panel ID="pnlUpcomingOps" runat="server" Visible="false">
            <h3 style="margin-top:40px;">Upcoming Operations (Next 30 Days)</h3>
            <asp:GridView ID="gvUpcoming" runat="server" AutoGenerateColumns="False" CssClass="table"
                OnRowCommand="gvUpcoming_RowCommand">
                <Columns>
                    <asp:BoundField DataField="ScheduleID" HeaderText="ID" Visible="false" />
                    <asp:BoundField DataField="ScheduledDate" HeaderText="Scheduled Date" DataFormatString="{0:yyyy-MM-dd}" />
                    <asp:BoundField DataField="OperationNumber" HeaderText="Operation" />
                    <asp:BoundField DataField="Status" HeaderText="Status" />
                    <asp:TemplateField HeaderText="Action">
                        <ItemTemplate>
                            <asp:Button ID="btnSetSchedule" runat="server" Text="Set Schedule"
                                CommandName="SetSchedule"
                                CommandArgument='<%# Eval("ScheduleID") + "|" + Eval("ScheduledDate", "{0:yyyy-MM-ddTHH:mm}") %>'
                                CssClass="btn btn-primary btn-sm"
                                Visible='<%# Eval("Status").ToString() != "Completed" %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </asp:Panel>

        <!-- All Scheduled Operations -->
        <asp:Panel ID="pnlAllOps" runat="server" Visible="false">
            <h3 style="margin-top:40px;">All Scheduled Operations</h3>
            <asp:GridView ID="gvAllOps" runat="server" AutoGenerateColumns="False" CssClass="table">
                <Columns>
                    <asp:BoundField DataField="BookingID" HeaderText="Booking ID" />
                    <asp:BoundField DataField="OperationNumber" HeaderText="Operation #" />
                    <asp:BoundField DataField="ScheduledDate" HeaderText="Scheduled Date" DataFormatString="{0:yyyy-MM-dd}" />
                    <asp:BoundField DataField="Status" HeaderText="Status" />
                </Columns>
            </asp:GridView>
        </asp:Panel>

        <!-- Contract Status -->
        <asp:Label ID="lblContractStatus" runat="server" CssClass="message-info" Visible="false" />

        <!-- Modal for Set Schedule -->
        <div id="modalOverlay" class="modal-overlay">
            <div class="modal-box">
                <h4>Set New Schedule</h4>
                <asp:HiddenField ID="hfSelectedScheduleID" runat="server" />
                <asp:TextBox ID="txtNewScheduleDate" runat="server" TextMode="Date" CssClass="form-control" />
                <asp:TextBox ID="txtNewScheduleTime" runat="server" TextMode="Time" CssClass="form-control" />
               <asp:Button ID="btnConfirmSchedule" runat="server" Text="Save" CssClass="btn btn-success"
    OnClientClick="confirmSchedule(); return false;" />

                <asp:Button ID="btnCloseModal" runat="server" Text="Cancel" CssClass="btn btn-secondary"
                    OnClientClick="hideModal(); return false;" />
            </div>
        </div>

        <script type="text/javascript">
            function showModal(scheduleId, currentDateTime) {
                document.getElementById('<%= hfSelectedScheduleID.ClientID %>').value = scheduleId;
                const dateObj = new Date(currentDateTime);
                document.getElementById('<%= txtNewScheduleDate.ClientID %>').value = dateObj.toISOString().split('T')[0];
                document.getElementById('<%= txtNewScheduleTime.ClientID %>').value = dateObj.toTimeString().substring(0, 5);
                document.getElementById('modalOverlay').style.display = 'flex';
            }

            function hideModal() {
                document.getElementById('modalOverlay').style.display = 'none';
            }

            function confirmSchedule() {
                Swal.fire({
                    title: 'Confirm New Schedule?',
                    text: 'Are you sure you want to update this schedule?',
                    icon: 'question',
                    showCancelButton: true,
                    confirmButtonColor: '#1d4ed8',
                    cancelButtonColor: '#d33',
                    confirmButtonText: 'Yes, save it!'
                }).then((result) => {
                    if (result.isConfirmed) {
                        __doPostBack('<%= btnConfirmSchedule.UniqueID %>', '');
                    }
                });
            }

        </script>
    </div>
</asp:Content>