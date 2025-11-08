<%@ Page Title="My Bookings" Language="C#" MasterPageFile="~/Client.master" AutoEventWireup="true" CodeBehind="MyBookings.aspx.cs" Inherits="RRCManagementSystem.MyBookings" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr/dist/flatpickr.min.css">
    <script src="https://cdn.jsdelivr.net/npm/flatpickr"></script>
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

        /* Time Slot Styling */
        .radio-group {
            display: flex;
            flex-wrap: wrap;
            gap: 12px;
        }

        .radio-option {
            flex: 1;
            min-width: 200px;
        }

        .radio-option input[type="radio"] {
            display: none;
        }

        .radio-label {
            display: block;
            padding: 14px 16px;
            border: 2px solid #e5e7eb;
            border-radius: 10px;
            cursor: pointer;
            transition: all 0.3s ease;
            text-align: center;
            font-weight: 500;
            background: #f9fafb;
        }

        .radio-option input[type="radio"]:checked + .radio-label {
            border-color: #3b82f6;
            background: #eff6ff;
            color: #1e40af;
        }

        .radio-label:hover:not(.disabled) {
            border-color: #3b82f6;
            background: white;
        }

        .radio-label.disabled {
            opacity: 0.5;
            cursor: not-allowed;
            background: #fee2e2;
            border-color: #fca5a5;
        }

        /* Availability Badge */
        .availability-badge {
            display: inline-flex;
            align-items: center;
            gap: 8px;
            padding: 8px 16px;
            border-radius: 20px;
            font-size: 13px;
            font-weight: 600;
            margin-top: 8px;
        }

        .availability-badge.available {
            background: #d1fae5;
            color: #065f46;
        }

        .availability-badge.limited {
            background: #fef3c7;
            color: #92400e;
        }

        .availability-badge.unavailable {
            background: #fee2e2;
            color: #991b1b;
        }

        .availability-badge.checking {
            background: #dbeafe;
            color: #1e40af;
        }

        /* Loading Spinner */
        .spinner {
            display: inline-block;
            width: 16px;
            height: 16px;
            border: 2px solid rgba(59, 130, 246, 0.3);
            border-radius: 50%;
            border-top-color: #3b82f6;
            animation: spin 0.8s linear infinite;
        }

        @keyframes spin {
            to { transform: rotate(360deg); }
        }

        .slot-info {
            font-size: 0.75rem;
            color: #6b7280;
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

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
        <asp:HiddenField ID="hfSQM" runat="server" />
        <asp:HiddenField ID="hfSelectedDate" runat="server" />

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
                                            CommandArgument='<%# Eval("ScheduleID") + "|" + Eval("ScheduledDate", "{0:yyyy-MM-ddTHH:mm}") + "|" + Eval("BookingID") %>'
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
                                            CommandArgument='<%# Eval("ScheduleID") + "|" + Eval("ScheduledDate", "{0:yyyy-MM-ddTHH:mm}") + "|" + Eval("BookingID") %>'
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
        <div class="bg-white rounded-2xl shadow-2xl w-full max-w-lg mx-4 overflow-hidden border border-gray-200">
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
                <asp:HiddenField ID="hfSelectedTimeSlotID" runat="server" />

                <!-- Date Picker -->
                <div>
                    <label class="block text-gray-700 font-medium mb-2 flex items-center">
                        <i class="fas fa-calendar text-blue-600 mr-2"></i>
                        New Preferred Date <span class="text-red-500 ml-1">*</span>
                    </label>
                    <asp:TextBox ID="txtNewScheduleDate" runat="server"
                        CssClass="w-full rounded-lg border-2 border-gray-300 px-4 py-3 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 transition"
                        placeholder="Click to select date"
                        AutoComplete="off" />
                </div>

                <!-- Availability Indicator -->
                <div id="availabilityIndicator" style="display: none;">
                    <div class="availability-badge checking">
                        <span class="spinner"></span>
                        <span>Checking team availability...</span>
                    </div>
                </div>
                <asp:Label ID="lblAvailability" runat="server" CssClass="availability-badge"
                    Style="display: none;"></asp:Label>

                <!-- Time Slot Selection -->
                <div id="timeSlotSection" style="display: none;">
                    <label class="block text-gray-700 font-medium mb-3 flex items-center">
                        <i class="fas fa-clock text-blue-600 mr-2"></i>
                        Select Time Slot <span class="text-red-500 ml-1">*</span>
                    </label>
                    <div class="radio-group">
                        <div class="radio-option">
                            <asp:RadioButton ID="rb8AM12PM" runat="server" GroupName="TimeSlot" />
                            <label for="<%= rb8AM12PM.ClientID %>" class="radio-label">
                                ☀️ 8:00 AM - 12:00 PM<br />
                                <small class="slot-info">Morning Shift</small>
                            </label>
                        </div>
                        <div class="radio-option">
                            <asp:RadioButton ID="rb12PM4PM" runat="server" GroupName="TimeSlot" />
                            <label for="<%= rb12PM4PM.ClientID %>" class="radio-label">
                                🌤️ 12:00 PM - 4:00 PM<br />
                                <small class="slot-info">Afternoon Shift</small>
                            </label>
                        </div>
                        <div class="radio-option">
                            <asp:RadioButton ID="rb4PM8PM" runat="server" GroupName="TimeSlot" />
                            <label for="<%= rb4PM8PM.ClientID %>" class="radio-label">
                                🌆 4:00 PM - 8:00 PM<br />
                                <small class="slot-info">Evening Shift</small>
                            </label>
                        </div>
                        <div class="radio-option">
                            <asp:RadioButton ID="rb8PM12AM" runat="server" GroupName="TimeSlot" />
                            <label for="<%= rb8PM12AM.ClientID %>" class="radio-label">
                                🌃 8:00 PM - 12:00 AM<br />
                                <small class="slot-info">Night Shift</small>
                            </label>
                        </div>
                    </div>
                </div>

                <div class="bg-blue-50 border border-blue-200 rounded-lg p-4">
                    <p class="text-sm text-blue-800 flex items-start">
                        <i class="fas fa-info-circle mt-0.5 mr-2"></i>
                        <span>Service available 7 days a week. Time slots showing team availability in real-time.</span>
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
                    OnClientClick="return confirmSchedule();" OnClick="btnConfirmSchedule_Click" />
            </div>
        </div>
    </div>

    <script>
        let currentDate = null;
        let currentSQM = 0;

        // ===== Initialize Date Picker =====
        window.addEventListener('load', function () {
            // Get SQM from hidden field
            const sqmField = document.getElementById('<%= hfSQM.ClientID %>');
            if (sqmField && sqmField.value) {
                currentSQM = parseInt(sqmField.value) || 0;
            }

            const dateInput = document.getElementById('<%= txtNewScheduleDate.ClientID %>');

            flatpickr(dateInput, {
                minDate: 'today',
                maxDate: new Date().fp_incr(90), // 90 days from now
                dateFormat: 'Y-m-d',
                onChange: function (selectedDates, dateStr, instance) {
                    if (selectedDates.length > 0) {
                        document.getElementById('<%= hfSelectedDate.ClientID %>').value = dateStr;
                        currentDate = dateStr;
                        checkDateAvailability(dateStr);
                        disablePastTimeSlots(dateStr);
                    }
                }
            });

            // Attach listeners to time slot radio buttons
            attachTimeSlotListeners();

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

        function showModal(scheduleId, currentDateTime, bookingId) {
            document.getElementById('<%= hfSelectedScheduleID.ClientID %>').value = scheduleId;

            // Reset time slots
            resetTimeSlots();

            // Load SQM for this booking
            if (bookingId) {
                loadBookingSQM(bookingId);
            }

            document.getElementById('setScheduleModal').classList.remove('hidden');
            document.body.style.overflow = 'hidden';
        }

        function hideModal() {
            document.getElementById('setScheduleModal').classList.add('hidden');
            document.body.style.overflow = '';

            // Reset form
            document.getElementById('<%= txtNewScheduleDate.ClientID %>').value = '';
            document.getElementById('<%= hfSelectedTimeSlotID.ClientID %>').value = '';
            resetTimeSlots();
        }

        function resetTimeSlots() {
            const timeSlotSection = document.getElementById('timeSlotSection');
            const availabilityIndicator = document.getElementById('availabilityIndicator');
            const lblAvailability = document.getElementById('<%= lblAvailability.ClientID %>');

            timeSlotSection.style.display = 'none';
            availabilityIndicator.style.display = 'none';
            lblAvailability.style.display = 'none';

            // Uncheck all radio buttons and reset labels
            const slotIds = [
                '<%= rb8AM12PM.ClientID %>',
                '<%= rb12PM4PM.ClientID %>',
                '<%= rb4PM8PM.ClientID %>',
                '<%= rb8PM12AM.ClientID %>'
            ];

            const slotLabels = [
                '☀️ 8:00 AM - 12:00 PM<br/><small class="slot-info">Morning Shift</small>',
                '🌤️ 12:00 PM - 4:00 PM<br/><small class="slot-info">Afternoon Shift</small>',
                '🌆 4:00 PM - 8:00 PM<br/><small class="slot-info">Evening Shift</small>',
                '🌃 8:00 PM - 12:00 AM<br/><small class="slot-info">Night Shift</small>'
            ];

            slotIds.forEach((id, index) => {
                const radio = document.getElementById(id);
                const label = document.querySelector(`label[for="${id}"]`);

                if (radio) {
                    radio.checked = false;
                    radio.disabled = false;
                }

                if (label) {
                    label.classList.remove('disabled');
                    label.innerHTML = slotLabels[index];
                }
            });
        }

        function loadBookingSQM(bookingId) {
            // Call server to get SQM for this booking
            fetch('MyBookings.aspx/GetBookingSQM', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ bookingId: bookingId })
            })
                .then(response => response.json())
                .then(data => {
                    if (data.d && data.d > 0) {
                        currentSQM = data.d;
                        document.getElementById('<%= hfSQM.ClientID %>').value = currentSQM;
                    }
                })
                .catch(error => {
                    console.error('Error loading SQM:', error);
                    currentSQM = 100; // Default fallback
                });
        }

        // ===== Disable past time slots for today =====
        function disablePastTimeSlots(selectedDate) {
            const now = new Date();
            const selected = new Date(selectedDate);
            const isToday =
                now.getFullYear() === selected.getFullYear() &&
                now.getMonth() === selected.getMonth() &&
                now.getDate() === selected.getDate();

            if (!isToday) return;

            const currentHour = now.getHours();

            const slotRules = [
                { id: '<%= rb8AM12PM.ClientID %>', start: 8, end: 12, label: '☀️ 8:00 AM - 12:00 PM<br/><small class="slot-info">Morning Shift</small>' },
                { id: '<%= rb12PM4PM.ClientID %>', start: 12, end: 16, label: '🌤️ 12:00 PM - 4:00 PM<br/><small class="slot-info">Afternoon Shift</small>' },
                { id: '<%= rb4PM8PM.ClientID %>', start: 16, end: 20, label: '🌆 4:00 PM - 8:00 PM<br/><small class="slot-info">Evening Shift</small>' },
                { id: '<%= rb8PM12AM.ClientID %>', start: 20, end: 24, label: '🌃 8:00 PM - 12:00 AM<br/><small class="slot-info">Night Shift</small>' }
            ];

            slotRules.forEach(slot => {
                const radio = document.getElementById(slot.id);
                const label = document.querySelector(`label[for="${slot.id}"]`);
                if (!radio || !label) return;

                if (currentHour >= slot.end) {
                    radio.disabled = true;
                    radio.checked = false;
                    label.classList.add('disabled');
                    const originalText = slot.label.split('<br/>')[0];
                    label.innerHTML = originalText + '<br/><small style="color:#dc2626;">⏰ Time Passed</small>';
                }
            });
        }

        // ===== Check date availability for all time slots =====
        function checkDateAvailability(dateStr) {
            const indicator = document.getElementById('availabilityIndicator');
            const lblAvailability = document.getElementById('<%= lblAvailability.ClientID %>');
            const timeSlotSection = document.getElementById('timeSlotSection');

            indicator.style.display = 'block';
            lblAvailability.style.display = 'none';
            timeSlotSection.style.display = 'none';

            fetch('MyBookings.aspx/CheckDateAvailability', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    date: dateStr,
                    sqm: currentSQM
                })
            })
                .then(response => response.json())
                .then(data => {
                    indicator.style.display = 'none';

                    if (data.d && data.d.Success) {
                        updateTimeSlotAvailability(data.d.TimeSlots);
                        disablePastTimeSlots(dateStr);

                        const hasAvailable = data.d.TimeSlots.some(ts => ts.IsAvailable);
                        lblAvailability.style.display = 'inline-flex';

                        if (hasAvailable) {
                            lblAvailability.className = 'availability-badge available';
                            lblAvailability.innerHTML = '<i class="fas fa-check-circle"></i> Time slots available';
                            timeSlotSection.style.display = 'block';
                        } else {
                            lblAvailability.className = 'availability-badge unavailable';
                            lblAvailability.innerHTML = '<i class="fas fa-times-circle"></i> All time slots fully booked';
                        }
                    } else {
                        lblAvailability.style.display = 'inline-flex';
                        lblAvailability.className = 'availability-badge unavailable';
                        lblAvailability.innerHTML = '<i class="fas fa-exclamation-triangle"></i> Error checking availability';
                    }
                })
                .catch(error => {
                    console.error('Date availability check error:', error);
                    indicator.style.display = 'none';
                });
        }

        // ===== Update time slot radio buttons with availability =====
        function updateTimeSlotAvailability(timeSlots) {
            const slotMapping = [
                { id: 1, radioId: '<%= rb8AM12PM.ClientID %>' },
                { id: 2, radioId: '<%= rb12PM4PM.ClientID %>' },
                { id: 3, radioId: '<%= rb4PM8PM.ClientID %>' },
                { id: 4, radioId: '<%= rb8PM12AM.ClientID %>' }
            ];

            slotMapping.forEach(mapping => {
                const timeSlot = timeSlots.find(ts => ts.TimeSlotID === mapping.id);
                const radio = document.getElementById(mapping.radioId);
                const label = radio ? document.querySelector(`label[for="${mapping.radioId}"]`) : null;

                if (radio && label && timeSlot) {
                    const baseDisabled = !timeSlot.IsAvailable || timeSlot.TotalTeams === 0;

                    if (!radio.disabled) {
                        radio.disabled = baseDisabled;
                    }

                    if (baseDisabled && !label.classList.contains('disabled')) {
                        label.classList.add('disabled');

                        const displayText = timeSlot.TotalTeams === 0
                            ? '❌ No Teams Available'
                            : '❌ Fully Booked';

                        label.innerHTML = label.innerHTML.split('<br/>')[0] +
                            `<br/><small style="color: #dc2626;">${displayText}</small>`;
                    } else if (!baseDisabled && !label.classList.contains('disabled')) {
                        label.classList.remove('disabled');

                        const originalText = label.innerHTML.split('<small>')[0];
                        const shiftText = getShiftText(mapping.id);
                        label.innerHTML = originalText +
                            `<small class="slot-info">${shiftText} (${timeSlot.AvailableTeams}/${timeSlot.TotalTeams} teams)</small>`;
                    }
                }
            });
        }

        function getShiftText(slotId) {
            switch (slotId) {
                case 1: return 'Morning Shift';
                case 2: return 'Afternoon Shift';
                case 3: return 'Evening Shift';
                case 4: return 'Night Shift';
                default: return 'Shift';
            }
        }

        // ===== Attach listeners to time slot radio buttons =====
        function attachTimeSlotListeners() {
            const timeSlots = [
                { radio: '<%= rb8AM12PM.ClientID %>', id: 1 },
                { radio: '<%= rb12PM4PM.ClientID %>', id: 2 },
                { radio: '<%= rb4PM8PM.ClientID %>', id: 3 },
                { radio: '<%= rb8PM12AM.ClientID %>', id: 4 }
            ];

            timeSlots.forEach(slot => {
                const radio = document.getElementById(slot.radio);
                if (radio) {
                    radio.addEventListener('change', function () {
                        if (this.checked && currentDate) {
                            checkTimeSlotAvailability(currentDate, slot.id);
                            document.getElementById('<%= hfSelectedTimeSlotID.ClientID %>').value = slot.id;
                        }
                    });
                }
            });
        }

        // ===== Check specific time slot availability =====
        function checkTimeSlotAvailability(dateStr, timeSlotId) {
            const lblAvailability = document.getElementById('<%= lblAvailability.ClientID %>');

            fetch('MyBookings.aspx/CheckTimeSlotAvailability', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    date: dateStr,
                    timeSlotId: timeSlotId,
                    sqm: currentSQM
                })
            })
                .then(response => response.json())
                .then(data => {
                    lblAvailability.style.display = 'inline-flex';

                    const result = data.d;

                    if (result.TotalTeams === 0) {
                        lblAvailability.className = 'availability-badge unavailable';
                        lblAvailability.innerHTML = `<i class="fas fa-times-circle"></i> No teams available for this slot`;
                    } else if (result.IsAvailable) {
                        if (result.AvailableTeams > 1) {
                            lblAvailability.className = 'availability-badge available';
                            lblAvailability.innerHTML = `<i class="fas fa-check-circle"></i> ${result.AvailableTeams} team(s) available`;
                        } else {
                            lblAvailability.className = 'availability-badge limited';
                            lblAvailability.innerHTML = `<i class="fas fa-exclamation-triangle"></i> Only 1 team available`;
                        }
                    } else {
                        lblAvailability.className = 'availability-badge unavailable';
                        lblAvailability.innerHTML = `<i class="fas fa-times-circle"></i> This slot is fully booked`;
                    }
                })
                .catch(error => {
                    console.error('Time slot availability check error:', error);
                });
        }

        // ===== Confirm schedule =====
        function confirmSchedule() {
            const date = document.getElementById('<%= txtNewScheduleDate.ClientID %>').value;
            const timeSlotId = document.getElementById('<%= hfSelectedTimeSlotID.ClientID %>').value;

            if (!date) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Missing Date',
                    text: 'Please select a service date.',
                    confirmButtonColor: '#2563eb'
                });
                return false;
            }

            if (!timeSlotId) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Missing Time Slot',
                    text: 'Please select a time slot.',
                    confirmButtonColor: '#2563eb'
                });
                return false;
            }

            // Check if selected slot is disabled
            const selectedSlotRadio = document.querySelector('input[name$="TimeSlot"]:checked');
            if (selectedSlotRadio && selectedSlotRadio.disabled) {
                Swal.fire({
                    icon: 'error',
                    title: 'Unavailable Time Slot',
                    text: 'This time slot is not available. Please choose another.',
                    confirmButtonColor: '#dc2626'
                });
                return false;
            }

            return true;
        }

        // Display notice messages dynamically
        function displayNotices() {
            const container = document.getElementById('noticeContainer');
            const lblMessage = document.getElementById('<%= lblMessage.ClientID %>');
            const lblNextOp = document.getElementById('<%= lblNextOperationNotice.ClientID %>');
            const lblContract = document.getElementById('<%= lblContractStatus.ClientID %>');

            container.innerHTML = '';

            if (lblMessage && lblMessage.innerText.trim()) {
                const isPositive = lblMessage.innerText.includes('✅');
                container.innerHTML += `
                    <div class="flex items-start gap-3 p-4 rounded-lg border ${isPositive ? 'bg-blue-50 border-blue-200' : 'bg-gray-50 border-gray-300'}">
                        <i class="fas ${isPositive ? 'fa-check-circle text-blue-600' : 'fa-exclamation-circle text-gray-600'} text-lg mt-0.5"></i>
                        <span class="${isPositive ? 'text-blue-900' : 'text-gray-800'} font-medium">${lblMessage.innerText}</span>
                    </div>
                `;
            }

            if (lblNextOp && lblNextOp.innerText.trim() && lblNextOp.style.display !== 'none') {
                container.innerHTML += `
                    <div class="flex items-start gap-3 p-4 rounded-lg border bg-blue-50 border-blue-200">
                        <i class="fas fa-bell text-blue-600 text-lg mt-0.5"></i>
                        <span class="text-blue-900 font-medium">${lblNextOp.innerText}</span>
                    </div>
                `;
            }

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
        const modalEl = document.getElementById('setScheduleModal');
        if (modalEl) {
            modalEl.addEventListener('click', (e) => {
                if (e.target.id === 'setScheduleModal') {
                    hideModal();
                }
            });
        }
    </script>
</asp:Content>