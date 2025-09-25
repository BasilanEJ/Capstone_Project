<%@ Page Title="My Bookings" Language="C#" MasterPageFile="~/Client.master" AutoEventWireup="true" CodeBehind="MyBookings.aspx.cs" Inherits="RRCManagementSystem.MyBookings" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Tailwind CSS -->
    <script src="https://cdn.tailwindcss.com"></script>
    <!-- SweetAlert2 -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <style>
        /* ======== Mobile improvements ======== */
        @media (max-width: 640px) {
            .grid-btn {
                display: block;
                width: 100%;
                margin-top: 0.5rem;
            }
        }

        .badge-success {
            background-color: #22c55e;
            color: white;
            padding: 0.25rem 0.5rem;
            border-radius: 0.375rem;
            font-size: 0.75rem;
            font-weight: 500;
        }

        .badge-secondary {
            background-color: #9ca3af;
            color: white;
            padding: 0.25rem 0.5rem;
            border-radius: 0.375rem;
            font-size: 0.75rem;
            font-weight: 500;
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="sm1" runat="server" />

    <div class="max-w-7xl mx-auto px-4 py-8 space-y-10">
        <!-- Page Header -->
        <div class="flex items-center gap-3">
            <div class="bg-blue-100 text-blue-600 rounded-full p-3">
                <i class="fas fa-clipboard-list text-xl"></i>
            </div>
            <h1 class="text-3xl font-bold text-gray-900">My Bookings</h1>
        </div>

        <!-- Status / Notification Message -->
        <asp:Label ID="lblMessage" runat="server"
            CssClass="block p-3 rounded-lg text-sm font-medium bg-green-50 border border-green-200 text-green-700 mb-4 hidden" />

        <!-- ===================== Current Bookings ===================== -->
        <div class="bg-white shadow-md rounded-xl p-6">
            <h2 class="text-lg font-semibold text-gray-800 mb-4 flex items-center gap-2">
                <i class="fas fa-briefcase text-blue-600"></i> Current Bookings
            </h2>
            <div class="overflow-x-auto">
                <asp:GridView ID="gvMyBookings" runat="server"
                    AutoGenerateColumns="False"
                    CssClass="min-w-full border border-gray-200 text-sm text-gray-800"
                    AllowPaging="True" PageSize="10"
                    OnPageIndexChanging="gvMyBookings_PageIndexChanging"
                    OnRowDataBound="gvMyBookings_RowDataBound"
                    GridLines="None">
                    
                    <HeaderStyle CssClass="bg-gray-100 text-gray-800 font-semibold uppercase tracking-wide text-sm text-center" />
                    <RowStyle CssClass="hover:bg-gray-50 transition text-center" />
                    <AlternatingRowStyle CssClass="bg-gray-50 text-center" />

                    <Columns>
                        <asp:BoundField DataField="BookingID" HeaderText="BookingID" Visible="false" />
                        <asp:BoundField DataField="BookingCode" HeaderText="Booking Code" ItemStyle-CssClass="px-4 py-2" />
                        <asp:BoundField DataField="ServiceNames" HeaderText="Service" ItemStyle-CssClass="px-4 py-2" />
                        <asp:BoundField DataField="ScheduledDate" HeaderText="Initial Date" DataFormatString="{0:yyyy-MM-dd}" ItemStyle-CssClass="px-4 py-2" />
                        <asp:BoundField DataField="StartTime" HeaderText="Start Time" ItemStyle-CssClass="px-4 py-2" />
                        <asp:BoundField DataField="Status" HeaderText="Status" ItemStyle-CssClass="px-4 py-2" />
                        <asp:BoundField DataField="Notes" HeaderText="Notes" ItemStyle-CssClass="px-4 py-2" />
                        <asp:BoundField DataField="CreatedAt" HeaderText="Date Booked" DataFormatString="{0:yyyy-MM-dd}" ItemStyle-CssClass="px-4 py-2" />
                        
                        <asp:TemplateField HeaderText="Actions">
                            <ItemTemplate>
                             <asp:LinkButton ID="btnCancel" runat="server"
    Text="Cancel"
    CssClass="cancel-btn bg-red-500 hover:bg-red-600 text-white px-3 py-1 rounded shadow text-xs transition grid-btn"
    CausesValidation="false"
    UseSubmitBehavior="false"
    OnClientClick="return false;"
    data-bookingid='<%# Eval("BookingID") %>'
    Visible='<%# Eval("Status").ToString() == "Pending" %>' />

                            </ItemTemplate>
                            <ItemStyle CssClass="px-4 py-2" />
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>

                <asp:HiddenField ID="hdnCancelBooking" runat="server" />
            </div>
        </div>

        <!-- Label for Next Operation Notice -->
        <asp:Label ID="lblNextOperationNotice" runat="server"
            CssClass="block mt-4 p-3 rounded-lg bg-green-50 border border-green-200 text-green-700 font-medium hidden"
            Visible="false" />

        <!-- ===================== Upcoming Operations ===================== -->
        <asp:Panel ID="pnlUpcomingOps" runat="server" Visible="false">
            <div class="bg-white shadow-md rounded-xl p-6">
                <h2 class="text-lg font-semibold text-gray-800 mb-4 flex items-center gap-2">
                    <i class="fas fa-calendar-day text-blue-600"></i> Upcoming Operations (Next 30 Days)
                </h2>
                <div class="overflow-x-auto">
                    <asp:GridView ID="gvUpcoming" runat="server"
                        AutoGenerateColumns="False"
                        CssClass="min-w-full border border-gray-200 text-sm text-gray-800"
                        GridLines="None"
                        OnRowCommand="gvUpcoming_RowCommand">

                        <HeaderStyle CssClass="bg-gray-100 text-gray-800 font-semibold uppercase tracking-wide text-sm text-center" />
                        <RowStyle CssClass="hover:bg-gray-50 transition text-center" />
                        <AlternatingRowStyle CssClass="bg-gray-50 text-center" />

                        <Columns>
                            <asp:BoundField DataField="BookingCode" HeaderText="Booking Code" ItemStyle-CssClass="px-4 py-2" />
                            <asp:BoundField DataField="ScheduledDate" HeaderText="Scheduled Date" DataFormatString="{0:yyyy-MM-dd}" ItemStyle-CssClass="px-4 py-2" />
                            <asp:BoundField DataField="OperationNumber" HeaderText="Visit" ItemStyle-CssClass="px-4 py-2" />
                            <asp:BoundField DataField="Status" HeaderText="Status" ItemStyle-CssClass="px-4 py-2" />

                            <asp:TemplateField HeaderText="Action">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnSetSchedule" runat="server"
                                        Text="Set New Schedule"
                                        CommandName="SetSchedule"
                                        CommandArgument='<%# Eval("ScheduleID") + "|" + Eval("ScheduledDate", "{0:yyyy-MM-ddTHH:mm}") %>'
                                        CssClass="bg-blue-600 hover:bg-blue-700 text-white px-3 py-1 rounded text-xs shadow transition grid-btn"
                                        CausesValidation="false"
                                        UseSubmitBehavior="false"
                                        Visible='<%# Eval("Status").ToString() == "Pending" && Convert.ToDateTime(Eval("ScheduledDate")) > DateTime.Now.AddDays(2) && Convert.ToDateTime(Eval("ScheduledDate")) <= DateTime.Now.AddDays(30) %>' />
                                </ItemTemplate>
                                <ItemStyle CssClass="px-4 py-2" />
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </asp:Panel>

        <!-- ===================== All Scheduled Operations ===================== -->
        <asp:Panel ID="pnlAllOps" runat="server" Visible="false">
            <div class="bg-white shadow-md rounded-xl p-6">
                <h2 class="text-lg font-semibold text-gray-800 mb-4 flex items-center gap-2">
                    <i class="fas fa-list-ul text-blue-600"></i> All Scheduled Operations
                </h2>
                <div class="overflow-x-auto">
                    <asp:GridView ID="gvAllOps" runat="server"
                        AutoGenerateColumns="False"
                        CssClass="min-w-full border border-gray-200 text-sm text-gray-800"
                        GridLines="None"
                        OnRowCommand="gvAllOps_RowCommand"
                        OnRowDataBound="gvAllOps_RowDataBound"
                        DataKeyNames="ScheduleID">

                        <HeaderStyle CssClass="bg-gray-100 text-gray-800 font-semibold uppercase tracking-wide text-sm text-center" />
                        <RowStyle CssClass="hover:bg-gray-50 transition text-center" />
                        <AlternatingRowStyle CssClass="bg-gray-50 text-center" />

                        <Columns>
                            <asp:BoundField DataField="BookingCode" HeaderText="Booking Code" ItemStyle-CssClass="px-4 py-2" />
                            <asp:BoundField DataField="OperationNumber" HeaderText="Operation #" ItemStyle-CssClass="px-4 py-2" />
                            <asp:BoundField DataField="ScheduledDate" HeaderText="Scheduled Date" DataFormatString="{0:yyyy-MM-dd}" ItemStyle-CssClass="px-4 py-2" />
                            <asp:BoundField DataField="Status" HeaderText="Status" ItemStyle-CssClass="px-4 py-2" />

                     
                            <asp:TemplateField HeaderText="Action">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnReschedule" runat="server"
                                        Text="Set New Schedule"
                                        CommandName="Reschedule"
                                        CommandArgument='<%# Eval("ScheduleID") + "|" + Eval("ScheduledDate", "{0:yyyy-MM-ddTHH:mm}") %>'
                                        CssClass="bg-yellow-500 hover:bg-yellow-600 text-white px-3 py-1 rounded text-xs shadow transition grid-btn"
                                        CausesValidation="false"
                                        UseSubmitBehavior="false"
                                        Visible='<%# Eval("Status").ToString() != "Completed" && Convert.ToDateTime(Eval("ScheduledDate")) < DateTime.Now %>' />
                                </ItemTemplate>
                                <ItemStyle CssClass="px-4 py-2" />
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Progress">
                                <ItemTemplate>
                                    <asp:Literal ID="ltProgress" runat="server" />
                                </ItemTemplate>
                                <ItemStyle CssClass="px-4 py-2" />
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </asp:Panel>

        <!-- Contract Status -->
        <asp:Label ID="lblContractStatus" runat="server"
            CssClass="hidden mt-4 p-3 rounded-lg border text-sm font-medium bg-blue-50 border-blue-200 text-blue-800"
            Visible="false" />
    </div>

    <!-- Modal for Reschedule -->
    <div id="setScheduleModal" class="hidden fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm">
        <div class="bg-white rounded-xl shadow-xl w-full max-w-md p-6">
            <h5 class="text-lg font-bold text-blue-700 mb-4">Set New Schedule</h5>
            <asp:HiddenField ID="hfSelectedScheduleID" runat="server" />

            <div class="mb-4">
                <label class="block text-gray-700 font-medium mb-1">📅 Date</label>
                <asp:TextBox ID="txtNewScheduleDate" runat="server" TextMode="Date"
                    CssClass="w-full rounded-lg border border-gray-300 px-4 py-2 focus:border-blue-500 focus:ring focus:ring-blue-200" />
            </div>
            <div class="mb-4">
                <label class="block text-gray-700 font-medium mb-1">⏰ Time</label>
                <asp:TextBox ID="txtNewScheduleTime" runat="server" TextMode="Time"
                    CssClass="w-full rounded-lg border border-gray-300 px-4 py-2 focus:border-blue-500 focus:ring focus:ring-blue-200" />
            </div>
            <p class="text-xs text-gray-500 mb-4">Your new preferred date/time will be sent for approval.</p>

            <div class="flex justify-end gap-3">
                <button type="button" onclick="hideModal()" 
                    class="px-4 py-2 rounded-lg border border-gray-300 text-gray-600 hover:bg-gray-100">
                    Cancel
                </button>
                <asp:Button ID="btnConfirmSchedule" runat="server" Text="Send Request"
                    CssClass="px-4 py-2 bg-green-600 hover:bg-green-700 text-white rounded-lg shadow transition"
                    OnClientClick="confirmSchedule(); return false;" OnClick="btnConfirmSchedule_Click" />
            </div>
        </div>
    </div>

    <!-- JavaScript for Modal & SweetAlert -->
    <script>
        function showModal(scheduleId, currentDateTime) {
            document.getElementById('<%= hfSelectedScheduleID.ClientID %>').value = scheduleId;

            const dt = currentDateTime ? new Date(currentDateTime) : new Date();
            document.getElementById('<%= txtNewScheduleDate.ClientID %>').value = dt.toISOString().slice(0, 10);
            document.getElementById('<%= txtNewScheduleTime.ClientID %>').value = dt.toTimeString().slice(0, 5);

            document.getElementById('setScheduleModal').classList.remove('hidden');
        }

        function hideModal() {
            document.getElementById('setScheduleModal').classList.add('hidden');
        }

        function confirmSchedule() {
            Swal.fire({
                title: 'Send reschedule request?',
                text: 'We will notify you once it is approved.',
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#1d4ed8',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes, send it!'
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack('<%= btnConfirmSchedule.UniqueID %>', '');
                }
            });
        }

        // Cancel Booking with confirmation
        document.addEventListener('DOMContentLoaded', function () {
            document.querySelectorAll('.cancel-btn').forEach(function (btn) {
                btn.addEventListener('click', function (e) {
                    e.preventDefault();
                    var bookingId = btn.getAttribute('data-bookingid');

                    Swal.fire({
                        title: 'Are you sure?',
                        text: "You won't be able to revert this action!",
                        icon: 'warning',
                        showCancelButton: true,
                        confirmButtonColor: '#d33',
                        cancelButtonColor: '#3085d6',
                        confirmButtonText: 'Yes, cancel it!'
                    }).then((result) => {
                        if (result.isConfirmed) {
                            document.getElementById('<%= hdnCancelBooking.ClientID %>').value = bookingId;
                    __doPostBack('CancelBooking', '');
                }
            });
        });
    });
     });

    </script>
</asp:Content>
