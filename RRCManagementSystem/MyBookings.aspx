<%@ Page Title="" Language="C#" MasterPageFile="~/Client.master" AutoEventWireup="true" CodeBehind="MyBookings.aspx.cs" Inherits="RRCManagementSystem.MyBookings" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <div class="container my-5 p-4 bg-white rounded shadow">
        <h3 class="text-primary mb-4">My Bookings</h3>
        <asp:Label ID="lblMessage" runat="server" CssClass="text-danger fw-bold mb-3 d-block" />

        <!-- Bookings Grid -->
        <asp:GridView ID="gvMyBookings" runat="server" AutoGenerateColumns="False" CssClass="table table-bordered table-striped text-center"
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
        <asp:Label ID="lblNextOperationNotice" runat="server" CssClass="alert alert-success fw-bold mt-3 d-block" Visible="false" />

        <!-- Upcoming Operations -->
        <asp:Panel ID="pnlUpcomingOps" runat="server" Visible="false">
            <h3 class="text-primary mt-5">Upcoming Operations (Next 30 Days)</h3>
            <asp:GridView ID="gvUpcoming" runat="server" AutoGenerateColumns="False" CssClass="table table-bordered table-striped text-center"
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
    <h3 class="text-primary mt-5">All Scheduled Operations</h3>
    <asp:GridView ID="gvAllOps" runat="server" AutoGenerateColumns="False"
        CssClass="table table-bordered table-striped text-center"
        OnRowCommand="gvAllOps_RowCommand"
        OnRowDataBound="gvAllOps_RowDataBound"
        DataKeyNames="ScheduleID">
        
        <Columns>
            <asp:BoundField DataField="BookingID" HeaderText="Booking ID" />
            <asp:BoundField DataField="OperationNumber" HeaderText="Operation #" />
            <asp:BoundField DataField="ScheduledDate" HeaderText="Scheduled Date" DataFormatString="{0:yyyy-MM-dd}" />
            <asp:BoundField DataField="Status" HeaderText="Status" />

            <asp:TemplateField HeaderText="Action">
                <ItemTemplate>
                    <asp:Button ID="btnReschedule" runat="server" CommandName="Reschedule" Text="Set New Schedule"
                        CommandArgument='<%# Eval("ScheduleID") + "|" + Eval("ScheduledDate", "{0:yyyy-MM-ddTHH:mm}") %>'
                        CssClass="btn btn-warning btn-sm"
                        Visible='<%# 
                            Convert.ToDateTime(Eval("ScheduledDate")) < DateTime.Now &&
                            Eval("Status").ToString() != "Completed" &&
                            Convert.ToDateTime(Eval("ScheduledDate")) <= Convert.ToDateTime(Eval("CreatedAt")).AddYears(2)
                        %>' />
                </ItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Progress">
                <ItemTemplate>
                    <asp:Literal ID="ltProgress" runat="server" />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>
</asp:Panel>


        <!-- Contract Status -->
        <asp:Label ID="lblContractStatus" runat="server" CssClass="alert alert-success fw-bold mt-4 d-block" Visible="false" />

        <!-- Modal for Set Schedule -->
        <div id="modalOverlay" class="modal position-fixed top-0 start-0 w-100 h-100 bg-dark bg-opacity-50 justify-content-center align-items-center" style="display: none; z-index: 1050;">
            <div class="bg-white p-4 rounded shadow" style="width: 100%; max-width: 400px;">
                <h5 class="mb-3">Set New Schedule</h5>
                <asp:HiddenField ID="hfSelectedScheduleID" runat="server" />
                <div class="mb-3">
                    <asp:TextBox ID="txtNewScheduleDate" runat="server" TextMode="Date" CssClass="form-control" />
                </div>
                <div class="mb-3">
                    <asp:TextBox ID="txtNewScheduleTime" runat="server" TextMode="Time" CssClass="form-control" />
                </div>
                <div class="d-flex justify-content-end gap-2">
                    <asp:Button ID="btnConfirmSchedule" runat="server" Text="Save" CssClass="btn btn-success"
                        OnClientClick="confirmSchedule(); return false;" />
                    <asp:Button ID="btnCloseModal" runat="server" Text="Cancel" CssClass="btn btn-secondary"
                        OnClientClick="hideModal(); return false;" />
                </div>
            </div>
        </div>

        <script type="text/javascript">
            function showModal(scheduleId, currentDateTime) {
                document.getElementById('<%= hfSelectedScheduleID.ClientID %>').value = scheduleId;
                const dateObj = new Date(currentDateTime);
                document.getElementById('<%= txtNewScheduleDate.ClientID %>').value = dateObj.toISOString().split('T')[0];
                document.getElementById('<%= txtNewScheduleTime.ClientID %>').value = dateObj.toTimeString().substring(0, 5);
                document.getElementById('modalOverlay').classList.add('d-flex');
            }

            function hideModal() {
                document.getElementById('modalOverlay').classList.remove('d-flex');
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