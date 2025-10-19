<%@ Page Title="My Bookings" Language="C#" MasterPageFile="~/Client.master" AutoEventWireup="true" CodeBehind="MyBookings.aspx.cs" Inherits="RRCManagementSystem.MyBookings" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <style>
        /* Minimalist Status badges - Blue & Gray only */
        .status-badge {
            display: inline-block;
            padding: 0.375rem 0.75rem;
            border-radius: 0.5rem;
            font-size: 0.75rem;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.025em;
        }

        .status-pending {
            background-color: #dbeafe;
            color: #1e40af;
            border: 1px solid #93c5fd;
        }

        .status-assigned {
            background-color: #e0e7ff;
            color: #3730a3;
            border: 1px solid #a5b4fc;
        }

        .status-approved {
            background-color: #f3f4f6;
            color: #1f2937;
            border: 1px solid #d1d5db;
        }

        .status-completed {
            background-color: #f9fafb;
            color: #111827;
            border: 1px solid #9ca3af;
        }

        .status-cancelled {
            background-color: #f3f4f6;
            color: #6b7280;
            border: 1px solid #d1d5db;
        }

        /* Table responsive improvements */
        @media (max-width: 768px) {
            .table-container {
                overflow-x: auto;
                -webkit-overflow-scrolling: touch;
            }
            
            .action-btn {
                display: block;
                width: 100%;
                margin-bottom: 0.5rem;
            }
        }

        /* Card hover effect - subtle */
        .booking-card {
            transition: all 0.3s ease;
        }

        .booking-card:hover {
            box-shadow: 0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1);
        }

        /* GridView styling - Blue & Gray only */
        .custom-gridview {
            border-collapse: separate;
            border-spacing: 0;
            width: 100%;
        }

        .custom-gridview th {
            background-color: #1e40af;
            color: white;
            font-weight: 600;
            text-transform: uppercase;
            font-size: 0.75rem;
            letter-spacing: 0.05em;
            padding: 1rem;
            border: none;
            text-align: left;
            vertical-align: middle;
        }

        .custom-gridview td {
            padding: 1rem;
            border-bottom: 1px solid #e5e7eb;
            text-align: left;
            vertical-align: middle;
        }

        .custom-gridview tr:hover td {
            background-color: #f9fafb;
        }

        /* Center specific columns */
        .custom-gridview th:has(+ th):nth-child(6),
        .custom-gridview td:nth-child(6) {
            text-align: center;
        }

        .custom-gridview th:last-child,
        .custom-gridview td:last-child {
            text-align: center;
        }

        /* Progress indicator - Blue & Gray */
        .progress-badge {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.5rem 1rem;
            border-radius: 0.5rem;
            font-size: 0.875rem;
            font-weight: 600;
        }

        .progress-done {
            background-color: #dbeafe;
            color: #1e40af;
            border: 1px solid #93c5fd;
        }

        .progress-pending {
            background-color: #f3f4f6;
            color: #6b7280;
            border: 1px solid #d1d5db;
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="sm1" runat="server" />

    <div class="max-w-7xl mx-auto">
        <!-- Page Header - Minimalist -->
        <div class="mb-8">
            <div class="flex items-center gap-4 mb-3">
                <div class="w-12 h-12 bg-blue-600 rounded-lg flex items-center justify-center">
                    <i class="fas fa-clipboard-list text-white text-xl"></i>
                </div>
                <div>
                    <h1 class="text-3xl md:text-4xl font-bold text-gray-900">My Bookings</h1>
                    <p class="text-gray-600 mt-1">Manage your service bookings and schedules</p>
                </div>
            </div>
        </div>

        <!-- Status Messages -->
        <asp:Label ID="lblMessage" runat="server" CssClass="hidden" />
        <asp:Label ID="lblNextOperationNotice" runat="server" CssClass="hidden" Visible="false" />
        <asp:Label ID="lblContractStatus" runat="server" CssClass="hidden" Visible="false" />

        <!-- Contract/Notice Messages -->
        <div id="noticeContainer" class="mb-6 space-y-3"></div>

        <!-- ===================== Current Bookings ===================== -->
        <div class="booking-card bg-white shadow-md rounded-lg overflow-hidden border border-gray-200 mb-8">
            <!-- Card Header -->
            <div class="bg-blue-600 px-6 py-4 border-b border-blue-700">
                <h2 class="text-lg font-semibold text-white flex items-center">
                    <i class="fas fa-briefcase mr-3"></i>
                    Current Bookings
                </h2>
            </div>

            <!-- Card Body -->
            <div class="p-6">
                <div class="table-container overflow-x-auto">
                    <asp:GridView ID="gvMyBookings" runat="server"
                        AutoGenerateColumns="False"
                        CssClass="custom-gridview w-full"
                        AllowPaging="True" PageSize="10"
                        OnPageIndexChanging="gvMyBookings_PageIndexChanging"
                        OnRowDataBound="gvMyBookings_RowDataBound"
                        GridLines="None">
                        
                        <HeaderStyle CssClass="bg-blue-600 text-white" />
                        <RowStyle CssClass="hover:bg-gray-50 transition" />
                        <AlternatingRowStyle CssClass="bg-gray-50/50" />
                        <PagerStyle CssClass="px-6 py-4 bg-gray-50 text-center" />

                        <Columns>
                            <asp:BoundField DataField="BookingID" HeaderText="BookingID" Visible="false" />
                            
                            <asp:TemplateField HeaderText="Booking Code">
                                <ItemTemplate>
                                    <span class="font-semibold text-blue-600"><%# Eval("BookingCode") %></span>
                                </ItemTemplate>
                                <ItemStyle HorizontalAlign="Left" />
                                <HeaderStyle HorizontalAlign="Left" />
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Service">
                                <ItemTemplate>
                                    <div class="flex items-center">
                                        <i class="fas fa-tools text-gray-600 mr-2"></i>
                                        <span><%# Eval("ServiceNames") %></span>
                                    </div>
                                </ItemTemplate>
                                <ItemStyle HorizontalAlign="Left" />
                                <HeaderStyle HorizontalAlign="Left" />
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Initial Date">
                                <ItemTemplate>
                                    <div class="flex items-center">
                                        <i class="far fa-calendar text-blue-600 mr-2"></i>
                                        <span><%# Eval("ScheduledDate", "{0:MMM dd, yyyy}") %></span>
                                    </div>
                                </ItemTemplate>
                                <ItemStyle HorizontalAlign="Left" />
                                <HeaderStyle HorizontalAlign="Left" />
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Start Time">
                                <ItemTemplate>
                                    <div class="flex items-center">
                                        <i class="far fa-clock text-gray-600 mr-2"></i>
                                        <span><%# Eval("StartTime") %></span>
                                    </div>
                                </ItemTemplate>
                                <ItemStyle HorizontalAlign="Left" />
                                <HeaderStyle HorizontalAlign="Left" />
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Status">
                                <ItemTemplate>
                                    <span class='status-badge status-<%# Eval("Status").ToString().ToLower() %>'>
                                        <%# Eval("Status") %>
                                    </span>
                                </ItemTemplate>
                                <ItemStyle HorizontalAlign="Center" />
                                <HeaderStyle HorizontalAlign="Center" />
                            </asp:TemplateField>

                            <asp:BoundField DataField="Notes" HeaderText="Notes">
                                <ItemStyle HorizontalAlign="Left" />
                                <HeaderStyle HorizontalAlign="Left" />
                            </asp:BoundField>

                            <asp:TemplateField HeaderText="Date Booked">
                                <ItemTemplate>
                                    <%# Eval("CreatedAt", "{0:MMM dd, yyyy}") %>
                                </ItemTemplate>
                                <ItemStyle HorizontalAlign="Left" />
                                <HeaderStyle HorizontalAlign="Left" />
                            </asp:TemplateField>
                            
                            <asp:TemplateField HeaderText="Actions">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnCancel" runat="server"
                                        Text="Cancel"
                                        CssClass="action-btn inline-flex items-center justify-center bg-gray-600 hover:bg-gray-700 text-white px-4 py-2 rounded-lg transition text-sm font-medium"
                                        CausesValidation="false"
                                        UseSubmitBehavior="false"
                                        OnClientClick="return false;"
                                        data-bookingid='<%# Eval("BookingID") %>'
                                        Visible='<%# Eval("Status").ToString() == "Pending" %>'>
                                        <i class="fas fa-times-circle mr-2"></i>
                                        Cancel
                                    </asp:LinkButton>
                                </ItemTemplate>
                                <ItemStyle HorizontalAlign="Center" />
                                <HeaderStyle HorizontalAlign="Center" />
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>

                    <asp:HiddenField ID="hdnCancelBooking" runat="server" />
                </div>
            </div>
        </div>

        <!-- ===================== Upcoming Operations ===================== -->
        <asp:Panel ID="pnlUpcomingOps" runat="server" Visible="false">
            <div class="booking-card bg-white shadow-md rounded-lg overflow-hidden border border-gray-200 mb-8">
                <!-- Card Header -->
                <div class="bg-gray-700 px-6 py-4 border-b border-gray-800">
                    <h2 class="text-lg font-semibold text-white flex items-center">
                        <i class="fas fa-calendar-day mr-3"></i>
                        Upcoming Operations (Next 30 Days)
                    </h2>
                </div>

                <!-- Card Body -->
                <div class="p-6">
                    <div class="table-container overflow-x-auto">
                        <asp:GridView ID="gvUpcoming" runat="server"
                            AutoGenerateColumns="False"
                            CssClass="custom-gridview w-full"
                            GridLines="None"
                            OnRowCommand="gvUpcoming_RowCommand">

                            <HeaderStyle CssClass="bg-gray-700 text-white" />
                            <RowStyle CssClass="hover:bg-gray-50 transition" />
                            <AlternatingRowStyle CssClass="bg-gray-50/50" />

                            <Columns>
                                <asp:TemplateField HeaderText="Booking Code">
                                    <ItemTemplate>
                                        <span class="font-semibold text-blue-600"><%# Eval("BookingCode") %></span>
                                    </ItemTemplate>
                                    <ItemStyle HorizontalAlign="Left" />
                                    <HeaderStyle HorizontalAlign="Left" />
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Scheduled Date">
                                    <ItemTemplate>
                                        <div class="flex items-center">
                                            <i class="far fa-calendar-check text-blue-600 mr-2"></i>
                                            <span><%# Eval("ScheduledDate", "{0:MMM dd, yyyy}") %></span>
                                        </div>
                                    </ItemTemplate>
                                    <ItemStyle HorizontalAlign="Left" />
                                    <HeaderStyle HorizontalAlign="Left" />
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Visit #">
                                    <ItemTemplate>
                                        <span class="inline-flex items-center justify-center w-8 h-8 bg-gray-200 text-gray-800 rounded-full font-bold text-sm">
                                            <%# Eval("OperationNumber") %>
                                        </span>
                                    </ItemTemplate>
                                    <ItemStyle HorizontalAlign="Center" />
                                    <HeaderStyle HorizontalAlign="Center" />
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Status">
                                    <ItemTemplate>
                                        <span class='status-badge status-<%# Eval("Status").ToString().ToLower() %>'>
                                            <%# Eval("Status") %>
                                        </span>
                                    </ItemTemplate>
                                    <ItemStyle HorizontalAlign="Center" />
                                    <HeaderStyle HorizontalAlign="Center" />
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Action">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="btnSetSchedule" runat="server"
                                            Text="Reschedule"
                                            CommandName="SetSchedule"
                                            CommandArgument='<%# Eval("ScheduleID") + "|" + Eval("ScheduledDate", "{0:yyyy-MM-ddTHH:mm}") %>'
                                            CssClass="action-btn inline-flex items-center justify-center bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg transition text-sm font-medium"
                                            CausesValidation="false"
                                            UseSubmitBehavior="false"
                                            Visible='<%# Eval("Status").ToString() == "Pending" && Convert.ToDateTime(Eval("ScheduledDate")) > DateTime.Now.AddDays(2) && Convert.ToDateTime(Eval("ScheduledDate")) <= DateTime.Now.AddDays(30) %>'>
                                            <i class="fas fa-clock mr-2"></i>
                                            Reschedule
                                        </asp:LinkButton>
                                    </ItemTemplate>
                                    <ItemStyle HorizontalAlign="Center" />
                                    <HeaderStyle HorizontalAlign="Center" />
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <!-- ===================== All Scheduled Operations ===================== -->
        <asp:Panel ID="pnlAllOps" runat="server" Visible="false">
            <div class="booking-card bg-white shadow-md rounded-lg overflow-hidden border border-gray-200">
                <!-- Card Header -->
                <div class="bg-gray-800 px-6 py-4 border-b border-gray-900">
                    <h2 class="text-lg font-semibold text-white flex items-center">
                        <i class="fas fa-list-ul mr-3"></i>
                        All Scheduled Operations
                    </h2>
                </div>

                <!-- Card Body -->
                <div class="p-6">
                    <div class="table-container overflow-x-auto">
                        <asp:GridView ID="gvAllOps" runat="server"
                            AutoGenerateColumns="False"
                            CssClass="custom-gridview w-full"
                            GridLines="None"
                            OnRowCommand="gvAllOps_RowCommand"
                            OnRowDataBound="gvAllOps_RowDataBound"
                            DataKeyNames="ScheduleID">

                            <HeaderStyle CssClass="bg-gray-800 text-white" />
                            <RowStyle CssClass="hover:bg-gray-50 transition" />
                            <AlternatingRowStyle CssClass="bg-gray-50/50" />

                            <Columns>
                                <asp:TemplateField HeaderText="Booking Code">
                                    <ItemTemplate>
                                        <span class="font-semibold text-blue-600"><%# Eval("BookingCode") %></span>
                                    </ItemTemplate>
                                    <ItemStyle HorizontalAlign="Left" />
                                    <HeaderStyle HorizontalAlign="Left" />
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Operation #">
                                    <ItemTemplate>
                                        <span class="inline-flex items-center justify-center w-10 h-10 bg-gray-200 text-gray-800 rounded-full font-bold">
                                            <%# Eval("OperationNumber") %>
                                        </span>
                                    </ItemTemplate>
                                    <ItemStyle HorizontalAlign="Center" />
                                    <HeaderStyle HorizontalAlign="Center" />
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Scheduled Date">
                                    <ItemTemplate>
                                        <div class="flex items-center">
                                            <i class="far fa-calendar text-blue-600 mr-2"></i>
                                            <span><%# Eval("ScheduledDate", "{0:MMM dd, yyyy}") %></span>
                                        </div>
                                    </ItemTemplate>
                                    <ItemStyle HorizontalAlign="Left" />
                                    <HeaderStyle HorizontalAlign="Left" />
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Status">
                                    <ItemTemplate>
                                        <span class='status-badge status-<%# Eval("Status").ToString().ToLower() %>'>
                                            <%# Eval("Status") %>
                                        </span>
                                    </ItemTemplate>
                                    <ItemStyle HorizontalAlign="Center" />
                                    <HeaderStyle HorizontalAlign="Center" />
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Action">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="btnReschedule" runat="server"
                                            Text="Reschedule"
                                            CommandName="Reschedule"
                                            CommandArgument='<%# Eval("ScheduleID") + "|" + Eval("ScheduledDate", "{0:yyyy-MM-ddTHH:mm}") %>'
                                            CssClass="action-btn inline-flex items-center justify-center bg-gray-600 hover:bg-gray-700 text-white px-4 py-2 rounded-lg transition text-sm font-medium"
                                            CausesValidation="false"
                                            UseSubmitBehavior="false"
                                            Visible='<%# Eval("Status").ToString() != "Completed" && Convert.ToDateTime(Eval("ScheduledDate")) < DateTime.Now %>'>
                                            <i class="fas fa-calendar-alt mr-2"></i>
                                            Reschedule
                                        </asp:LinkButton>
                                    </ItemTemplate>
                                    <ItemStyle HorizontalAlign="Center" />
                                    <HeaderStyle HorizontalAlign="Center" />
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Progress">
                                    <ItemTemplate>
                                        <asp:Literal ID="ltProgress" runat="server" />
                                    </ItemTemplate>
                                    <ItemStyle HorizontalAlign="Center" />
                                    <HeaderStyle HorizontalAlign="Center" />
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>
            </div>
        </asp:Panel>
    </div>

    <!-- Reschedule Modal -->
    <div id="setScheduleModal" class="hidden fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm">
        <div class="bg-white rounded-lg shadow-2xl w-full max-w-md mx-4 overflow-hidden border border-gray-200">
            <!-- Modal Header -->
            <div class="bg-blue-600 px-6 py-4 border-b border-blue-700">
                <h3 class="text-lg font-semibold text-white flex items-center">
                    <i class="fas fa-calendar-check mr-3"></i>
                    Request Reschedule
                </h3>
            </div>

            <!-- Modal Body -->
            <div class="p-6 space-y-5">
                <asp:HiddenField ID="hfSelectedScheduleID" runat="server" />

                <div>
                    <label class="block text-gray-700 font-medium mb-2 flex items-center">
                        <i class="fas fa-calendar text-blue-600 mr-2"></i>
                        New Preferred Date
                    </label>
                    <asp:TextBox ID="txtNewScheduleDate" runat="server" TextMode="Date"
                        CssClass="w-full rounded-lg border border-gray-300 px-4 py-3 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 transition" />
                </div>

                <div>
                    <label class="block text-gray-700 font-medium mb-2 flex items-center">
                        <i class="fas fa-clock text-blue-600 mr-2"></i>
                        New Preferred Time
                    </label>
                    <asp:TextBox ID="txtNewScheduleTime" runat="server" TextMode="Time"
                        CssClass="w-full rounded-lg border border-gray-300 px-4 py-3 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 transition" />
                </div>

                <div class="bg-blue-50 border-l-4 border-blue-500 p-4 rounded">
                    <p class="text-sm text-gray-700 flex items-start">
                        <i class="fas fa-info-circle mt-0.5 mr-2 text-blue-600"></i>
                        <span>Your reschedule request will be sent for approval. You'll be notified once it's reviewed.</span>
                    </p>
                </div>
            </div>

            <!-- Modal Footer -->
            <div class="bg-gray-50 px-6 py-4 flex justify-end gap-3 border-t border-gray-200">
                <button type="button" onclick="hideModal()" 
                    class="px-5 py-2.5 rounded-lg border border-gray-300 text-gray-700 hover:bg-gray-100 transition font-medium">
                    Cancel
                </button>
                <asp:Button ID="btnConfirmSchedule" runat="server" Text="Send Request"
                    CssClass="px-5 py-2.5 bg-blue-600 hover:bg-blue-700 text-white rounded-lg transition font-medium"
                    OnClientClick="confirmSchedule(); return false;" OnClick="btnConfirmSchedule_Click" />
            </div>
        </div>
    </div>

    <script>
        // Set minimum date/time restrictions
        window.addEventListener('load', function() {
            const dateInput = document.getElementById('<%= txtNewScheduleDate.ClientID %>');
            
            // Set minimum date to today
            const today = new Date().toISOString().split('T')[0];
            dateInput.setAttribute('min', today);
        });

        function showModal(scheduleId, currentDateTime) {
            document.getElementById('<%= hfSelectedScheduleID.ClientID %>').value = scheduleId;

            const dt = currentDateTime ? new Date(currentDateTime) : new Date();
            const dateInput = document.getElementById('<%= txtNewScheduleDate.ClientID %>');
            const timeInput = document.getElementById('<%= txtNewScheduleTime.ClientID %>');
            
            dateInput.value = dt.toISOString().slice(0, 10);
            timeInput.value = dt.toTimeString().slice(0, 5);

            document.getElementById('setScheduleModal').classList.remove('hidden');
            document.body.style.overflow = 'hidden';
        }

        function hideModal() {
            document.getElementById('setScheduleModal').classList.add('hidden');
            document.body.style.overflow = '';
        }

        function confirmSchedule() {
            const date = document.getElementById('<%= txtNewScheduleDate.ClientID %>').value;
            const time = document.getElementById('<%= txtNewScheduleTime.ClientID %>').value;

            if (!date || !time) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Incomplete',
                    text: 'Please select both date and time.',
                    confirmButtonColor: '#2563eb'
                });
                return;
            }

            // Validate: must be at least 1 hour from now
            const selectedDateTime = new Date(date + 'T' + time);
            const now = new Date();
            const oneHourFromNow = new Date(now.getTime() + (60 * 60 * 1000));

            if (selectedDateTime < oneHourFromNow) {
                Swal.fire({
                    icon: 'error',
                    title: 'Invalid Schedule',
                    text: 'Please select a date and time at least 1 hour from now.',
                    confirmButtonColor: '#2563eb'
                });
                return;
            }

            Swal.fire({
                title: 'Send Reschedule Request?',
                text: 'Your request will be sent for approval.',
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#2563eb',
                cancelButtonColor: '#6b7280',
                confirmButtonText: 'Yes, send it!'
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack('<%= btnConfirmSchedule.UniqueID %>', '');
                }
            });
        }

        // Cancel Booking with confirmation
        document.addEventListener('DOMContentLoaded', function () {
            // Display notices
            displayNotices();

            // Cancel button handlers
            document.querySelectorAll('[data-bookingid]').forEach(function (btn) {
                btn.addEventListener('click', function (e) {
                    e.preventDefault();
                    var bookingId = btn.getAttribute('data-bookingid');

                    Swal.fire({
                        title: 'Cancel This Booking?',
                        text: "This action cannot be undone!",
                        icon: 'warning',
                        showCancelButton: true,
                        confirmButtonColor: '#2563eb',
                        cancelButtonColor: '#6b7280',
                        confirmButtonText: 'Yes, cancel it!',
                        cancelButtonText: 'Keep booking'
                    }).then((result) => {
                        if (result.isConfirmed) {
                            document.getElementById('<%= hdnCancelBooking.ClientID %>').value = bookingId;
                            __doPostBack('CancelBooking', '');
                        }
                    });
                });
            });
        });

        // Display notice messages dynamically
        function displayNotices() {
            const container = document.getElementById('noticeContainer');
            const lblMessage = document.getElementById('<%= lblMessage.ClientID %>');
            const lblNextOp = document.getElementById('<%= lblNextOperationNotice.ClientID %>');
            const lblContract = document.getElementById('<%= lblContractStatus.ClientID %>');

            // Clear container
            container.innerHTML = '';

            // Add message notices
            if (lblMessage && lblMessage.innerText.trim()) {
                const isPositive = lblMessage.innerText.includes('✅');
                container.innerHTML += `
                    <div class="flex items-start gap-3 p-4 rounded-lg border ${isPositive ? 'bg-blue-50 border-blue-200' : 'bg-gray-50 border-gray-300'}">
                        <i class="fas ${isPositive ? 'fa-check-circle text-blue-600' : 'fa-exclamation-circle text-gray-600'} text-lg mt-0.5"></i>
                        <span class="${isPositive ? 'text-blue-900' : 'text-gray-800'} font-medium">${lblMessage.innerText}</span>
                    </div>
                `;
            }

            // Add next operation notice
            if (lblNextOp && lblNextOp.innerText.trim() && lblNextOp.style.display !== 'none') {
                container.innerHTML += `
                    <div class="flex items-start gap-3 p-4 rounded-lg border bg-blue-50 border-blue-200">
                        <i class="fas fa-bell text-blue-600 text-lg mt-0.5"></i>
                        <span class="text-blue-900 font-medium">${lblNextOp.innerText}</span>
                    </div>
                `;
            }

            // Add contract status
            if (lblContract && lblContract.innerText.trim() && lblContract.style.display !== 'none') {
                const isCelebration = lblContract.innerText.includes('🎉');
                const isWarning = lblContract.innerText.includes('⚠️');
                const colorClass = isCelebration ? 'bg-blue-50 border-blue-200 text-blue-900' :
                    isWarning ? 'bg-gray-100 border-gray-300 text-gray-800' :
                        'bg-blue-50 border-blue-200 text-blue-900';
                const iconClass = isCelebration ? 'fa-trophy text-blue-600' :
                    isWarning ? 'fa-exclamation-triangle text-gray-600' :
                        'fa-info-circle text-blue-600';

                container.innerHTML += `
                    <div class="flex items-start gap-3 p-4 rounded-lg border ${colorClass}">
                        <i class="fas ${iconClass} text-lg mt-0.5"></i>
                        <span class="font-medium">${lblContract.innerText}</span>
                    </div>
                `;
            }
        }

        // Close modal on Escape key
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                hideModal();
            }
        });

        // Close modal on backdrop click
        document.getElementById('setScheduleModal').addEventListener('click', (e) => {
            if (e.target.id === 'setScheduleModal') {
                hideModal();
            }
        });
    </script>
</asp:Content>