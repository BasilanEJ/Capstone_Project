<%@ Page Title="Book Service" Language="C#" MasterPageFile="~/Client.master" AutoEventWireup="true" CodeBehind="BookService.aspx.cs" Inherits="RRCManagementSystem.BookService" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr/dist/flatpickr.min.css">
    <script src="https://cdn.jsdelivr.net/npm/flatpickr"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <style>
        .service-item {
            padding: 8px 12px;
            background: #f0fdf4;
            border-left: 3px solid #10b981;
            border-radius: 6px;
            margin-bottom: 6px;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }
        
        .service-name {
            font-weight: 500;
            color: #065f46;
        }
        
        .service-sqm {
            font-weight: 600;
            color: #059669;
            background: #d1fae5;
            padding: 2px 10px;
            border-radius: 12px;
            font-size: 0.875rem;
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

        /* ✅ NEW: Payment Plan Styling */
        .payment-plan-option {
            display: block;
            padding: 16px;
            border: 2px solid #e5e7eb;
            border-radius: 12px;
            margin-bottom: 12px;
            cursor: pointer;
            transition: all 0.3s ease;
            background: #f9fafb;
        }

        .payment-plan-option:hover {
            border-color: #3b82f6;
            background: white;
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(59, 130, 246, 0.1);
        }

        .payment-plan-option input[type="radio"]:checked + .payment-plan-content {
            border-color: #3b82f6;
            background: #eff6ff;
        }

        .payment-plan-option input[type="radio"] {
            display: none;
        }

        .payment-plan-content {
            border: 2px solid transparent;
            border-radius: 10px;
            padding: 8px;
            transition: all 0.3s ease;
        }

        .payment-plan-title {
            font-weight: 600;
            font-size: 1.1rem;
            color: #1f2937;
            margin-bottom: 4px;
        }

        .payment-plan-desc {
            font-size: 0.875rem;
            color: #6b7280;
        }

        .payment-plan-badge {
            display: inline-block;
            padding: 4px 12px;
            border-radius: 12px;
            font-size: 0.75rem;
            font-weight: 600;
            margin-top: 6px;
        }

        .badge-full {
            background: #dbeafe;
            color: #1e40af;
        }

        .badge-split {
            background: #fef3c7;
            color: #92400e;
        }

        .badge-flexible {
            background: #d1fae5;
            color: #065f46;
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="max-w-4xl mx-auto">
        <!-- Page Header -->
        <div class="mb-8 text-center">
            <h1 class="text-3xl md:text-4xl font-bold text-gray-800 mb-2">
                Book Your Service
            </h1>
            <p class="text-gray-600">Review quotation details and confirm your booking</p>
        </div>

        <asp:Label ID="lblMessage" runat="server" CssClass="hidden" />
        <asp:HiddenField ID="hfIsContract" runat="server" />
        <asp:HiddenField ID="hfQuotationID" runat="server" />
        <asp:HiddenField ID="hfSelectedDate" runat="server" />
        <asp:HiddenField ID="hfSQM" runat="server" />

        <!-- Main Card -->
        <div class="bg-white rounded-2xl shadow-xl overflow-hidden border border-gray-100">
            <!-- Card Header -->
            <div class="bg-gradient-to-r from-blue-600 to-blue-700 px-6 py-4">
                <h2 class="text-xl font-semibold text-white flex items-center">
                    <i class="fas fa-file-invoice mr-3"></i>
                    Quotation Details
                </h2>
            </div>

            <!-- Card Body -->
            <div class="p-6 md:p-8 space-y-6">
                <!-- Quotation Code -->
                <div class="flex items-start">
                    <div class="flex-shrink-0 w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center mr-4">
                        <i class="fas fa-hashtag text-blue-600"></i>
                    </div>
                    <div class="flex-1">
                        <label class="block text-sm font-medium text-gray-600 mb-1">Quotation Code</label>
                        <asp:Label ID="lblQuotationCode" runat="server" 
                            CssClass="text-lg font-semibold text-gray-800" />
                    </div>
                </div>

                <div class="border-t border-gray-100"></div>

                <!-- Services with Individual SQM -->
                <div class="flex items-start">
                    <div class="flex-shrink-0 w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center mr-4">
                        <i class="fas fa-tools text-green-600"></i>
                    </div>
                    <div class="flex-1">
                        <label class="block text-sm font-medium text-gray-600 mb-3">Service(s) to be Provided</label>
                        <div class="space-y-2">
                            <asp:Literal ID="lblServices" runat="server" />
                        </div>
                    </div>
                </div>

                <!-- Total SQM -->
                <div class="flex items-start">
                    <div class="flex-shrink-0 w-10 h-10 bg-purple-100 rounded-lg flex items-center justify-center mr-4">
                        <i class="fas fa-ruler-combined text-purple-600"></i>
                    </div>
                    <div class="flex-1">
                        <label class="block text-sm font-medium text-gray-600 mb-1">Total Coverage Area</label>
                        <div class="bg-purple-50 border border-purple-200 rounded-lg px-4 py-3 inline-block">
                            <asp:Label ID="lblSQM" runat="server"
                                CssClass="text-xl font-bold text-purple-700" />
                        </div>
                    </div>
                </div>

                <div class="border-t border-gray-100"></div>

                <!-- Pricing Breakdown -->
                <div class="bg-gray-50 rounded-xl p-5 space-y-4">
                    <h3 class="font-semibold text-gray-800 flex items-center mb-3">
                        <i class="fas fa-calculator mr-2 text-blue-600"></i>
                        Pricing Breakdown
                    </h3>

                    <!-- Base Price -->
                    <div class="flex justify-between items-center">
                        <span class="text-gray-600">Base Service Price</span>
                        <asp:Label ID="lblBasePrice" runat="server"
                            CssClass="font-semibold text-gray-800" />
                    </div>

                    <!-- Travel Expense -->
                    <div class="flex justify-between items-center">
                        <span class="text-gray-600">Travel Expense</span>
                        <asp:Label ID="lblTravelExpense" runat="server"
                            CssClass="font-semibold text-gray-800" />
                    </div>

                    <!-- Miscellaneous with Details -->
                    <div class="space-y-2">
                        <div class="flex justify-between items-center">
                            <span class="text-gray-600">Miscellaneous</span>
                            <asp:Label ID="lblMiscellaneous" runat="server"
                                CssClass="font-semibold text-gray-800" />
                        </div>
                        
                        <!-- Miscellaneous Details Breakdown -->
                        <asp:Panel ID="pnlMiscDetails" runat="server" Visible="false" 
                            CssClass="ml-4 pl-4 border-l-2 border-blue-200 space-y-1">
                            <asp:Literal ID="litMiscDetails" runat="server" />
                        </asp:Panel>
                    </div>

                    <div class="border-t border-gray-200 pt-3 mt-3">
                        <div class="flex justify-between items-center">
                            <span class="text-lg font-bold text-gray-800">Total Price</span>
                            <asp:Label ID="lblTotalPrice" runat="server"
                                CssClass="text-2xl font-bold text-blue-600" />
                        </div>
                    </div>
                </div>

                <div class="border-t border-gray-100"></div>

                <!-- ✅ NEW: Payment Plan Selection (Only for Contract Bookings) -->
               <!-- ✅ UPDATED: Payment Plan Selection (Dropdown Version) -->
<asp:Panel ID="pnlPaymentPlan" runat="server" Visible="false">
    <div class="flex items-start">
        <div class="flex-shrink-0 w-10 h-10 bg-yellow-100 rounded-lg flex items-center justify-center mr-4">
            <i class="fas fa-credit-card text-yellow-600"></i>
        </div>
        <div class="flex-1">
            <label class="block text-sm font-medium text-gray-600 mb-3">
                Select Payment Plan <span class="text-red-500">*</span>
            </label>
            
            <asp:DropDownList ID="ddlPaymentPlan" runat="server" 
                CssClass="w-full px-4 py-3 border-2 border-gray-300 rounded-lg focus:border-blue-500 focus:ring-2 focus:ring-blue-200 transition bg-white text-gray-800 font-medium">
                <asp:ListItem Value="100" Text="💰 Full Payment (100%) - Pay entire amount upfront"></asp:ListItem>
                <asp:ListItem Value="70-30" Text="📊 70-30 Plan - 70% now, 30% in 1 month (2 payments)"></asp:ListItem>
                <asp:ListItem Value="50-25-25" Text="📅 50-25-25 Plan - 50% now, 25% monthly x2 (3 payments)" Selected="True"></asp:ListItem>
            </asp:DropDownList>

            <div class="mt-3 p-3 bg-blue-50 border-l-4 border-blue-500 text-sm">
                <i class="fas fa-info-circle text-blue-600 mr-2"></i>
                <span class="text-gray-700">Contract bookings require structured payment plans for your convenience</span>
            </div>
        </div>
    </div>

    <div class="border-t border-gray-100 mt-6"></div>
</asp:Panel>

                <!-- Preferred Schedule -->
                <div class="flex items-start">
                    <div class="flex-shrink-0 w-10 h-10 bg-orange-100 rounded-lg flex items-center justify-center mr-4">
                        <i class="fas fa-calendar-alt text-orange-600"></i>
                    </div>
                    <div class="flex-1">
                        <label class="block text-sm font-medium text-gray-600 mb-2">Preferred Schedule</label>
                        <asp:Label ID="lblSelectedDateTime" runat="server" Text="Not selected yet"
                            CssClass="block text-gray-500 italic mb-3" />

                        <button type="button" onclick="showDateTimeModal()"
                            class="inline-flex items-center bg-blue-600 hover:bg-blue-700 text-white px-6 py-2.5 rounded-lg transition shadow-md hover:shadow-lg">
                            <i class="fas fa-clock mr-2"></i>
                            Select Date &amp; Time Slot
                        </button>

                        <asp:HiddenField ID="txtDate" runat="server" />
                        <asp:HiddenField ID="txtTimeSlot" runat="server" />
                    </div>
                </div>

                <!-- Additional Notes -->
                <div class="flex items-start">
                    <div class="flex-shrink-0 w-10 h-10 bg-gray-100 rounded-lg flex items-center justify-center mr-4">
                        <i class="fas fa-sticky-note text-gray-600"></i>
                    </div>
                    <div class="flex-1">
                        <label class="block text-sm font-medium text-gray-600 mb-2">Additional Notes (Optional)</label>
                        <asp:TextBox ID="txtNotes" runat="server" TextMode="MultiLine" Rows="4"
                            placeholder="Any special instructions or concerns..."
                            CssClass="w-full rounded-lg border border-gray-300 px-4 py-3 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 transition" />
                    </div>
                </div>

                <!-- Inspector Info -->
                <div class="flex items-start">
                    <div class="flex-shrink-0 w-10 h-10 bg-indigo-100 rounded-lg flex items-center justify-center mr-4">
                        <i class="fas fa-user-tie text-indigo-600"></i>
                    </div>
                    <div class="flex-1">
                        <label class="block text-sm font-medium text-gray-600 mb-1">Inspector Who Quoted</label>
                        <asp:Label ID="lblInspector" runat="server"
                            CssClass="text-gray-800 font-medium" />
                    </div>
                </div>

                <div class="border-t border-gray-100 pt-6">
                    <asp:Button ID="btnBook" runat="server" Text="Confirm Booking"
                        CssClass="w-full bg-gradient-to-r from-green-600 to-green-700 hover:from-green-700 hover:to-green-800 text-white font-semibold py-3.5 px-6 rounded-lg shadow-lg hover:shadow-xl transition transform hover:-translate-y-0.5"
                        OnClick="btnBook_Click" OnClientClick="return validateBooking();" />
                </div>
            </div>
        </div>

        <!-- Back Button -->
        <div class="mt-6 text-center">
            <a href="Home.aspx" class="inline-flex items-center text-gray-600 hover:text-blue-600 transition">
                <i class="fas fa-arrow-left mr-2"></i>
                Back to Home
            </a>
        </div>
    </div>

    <!-- Date/Time Modal -->
    <div class="hidden fixed inset-0 z-50 items-center justify-center bg-black/50 backdrop-blur-sm" id="dateTimeModal">
        <div class="bg-white rounded-2xl shadow-2xl w-full max-w-lg mx-4 overflow-hidden">
            <!-- Modal Header -->
            <div class="bg-gradient-to-r from-blue-600 to-blue-700 px-6 py-4">
                <h3 class="text-xl font-semibold text-white flex items-center">
                    <i class="fas fa-calendar-check mr-3"></i>
                    Select Preferred Schedule
                </h3>
            </div>

            <!-- Modal Body -->
            <div class="p-6 space-y-5">
                <!-- Date Picker -->
                <div>
                    <label class="block text-gray-700 font-medium mb-2 flex items-center">
                        <i class="fas fa-calendar text-blue-600 mr-2"></i>
                        Preferred Service Date <span class="text-red-500 ml-1">*</span>
                    </label>
                    <asp:TextBox ID="TextBox1" runat="server" 
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
                                ☀️ 8:00 AM - 12:00 PM<br/>
                                <small class="slot-info">Morning Shift</small>
                            </label>
                        </div>
                        <div class="radio-option">
                            <asp:RadioButton ID="rb12PM4PM" runat="server" GroupName="TimeSlot" />
                            <label for="<%= rb12PM4PM.ClientID %>" class="radio-label">
                                🌤️ 12:00 PM - 4:00 PM<br/>
                                <small class="slot-info">Afternoon Shift</small>
                            </label>
                        </div>
                        <div class="radio-option">
                            <asp:RadioButton ID="rb4PM8PM" runat="server" GroupName="TimeSlot" />
                            <label for="<%= rb4PM8PM.ClientID %>" class="radio-label">
                                🌆 4:00 PM - 8:00 PM<br/>
                                <small class="slot-info">Evening Shift</small>
                            </label>
                        </div>
                        <div class="radio-option">
                            <asp:RadioButton ID="rb8PM12AM" runat="server" GroupName="TimeSlot" />
                            <label for="<%= rb8PM12AM.ClientID %>" class="radio-label">
                                🌃 8:00 PM - 12:00 AM<br/>
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
            <div class="bg-gray-50 px-6 py-4 flex justify-end gap-3">
                <button type="button" onclick="hideDateTimeModal()"
                    class="px-5 py-2.5 rounded-lg border-2 border-gray-300 text-gray-700 hover:bg-gray-100 transition font-medium">
                    Cancel
                </button>
                <button type="button" onclick="confirmDateTimeSave()"
                    class="px-5 py-2.5 rounded-lg bg-green-600 hover:bg-green-700 text-white shadow-md hover:shadow-lg transition font-medium">
                    <i class="fas fa-check mr-2"></i>Save Schedule
                </button>
            </div>
        </div>
    </div>

    <script>
        const modalEl = document.getElementById('dateTimeModal');
        let currentDate = null;
        let currentSQM = 0;

        // ===== Initialize Date Picker =====
        window.addEventListener('load', function () {
            // Get SQM from hidden field
            const sqmField = document.getElementById('<%= hfSQM.ClientID %>');
            if (sqmField && sqmField.value) {
                currentSQM = parseInt(sqmField.value) || 0;
            }

            const dateInput = document.getElementById('<%= TextBox1.ClientID %>');

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
        });

        function showDateTimeModal() {
            modalEl.classList.remove('hidden');
            modalEl.classList.add('flex');
            document.body.style.overflow = 'hidden';
        }

        function hideDateTimeModal() {
            modalEl.classList.add('hidden');
            modalEl.classList.remove('flex');
            document.body.style.overflow = '';
        }

        // ===== Disable past time slots for today =====
        function disablePastTimeSlots(selectedDate) {
            const now = new Date();
            const selected = new Date(selectedDate);
            const isToday =
                now.getFullYear() === selected.getFullYear() &&
                now.getMonth() === selected.getMonth() &&
                now.getDate() === selected.getDate();

            if (!isToday) return; // Only process for today's date

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
                    // Time has passed
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

            fetch('BookService.aspx/CheckDateAvailability', {
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

                    // Only disable if not already disabled by time passing
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
                        }
                    });
                }
            });
        }

        // ===== Check specific time slot availability =====
        function checkTimeSlotAvailability(dateStr, timeSlotId) {
            const lblAvailability = document.getElementById('<%= lblAvailability.ClientID %>');

            fetch('BookService.aspx/CheckTimeSlotAvailability', {
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

        // ===== Save date and time slot =====
        function confirmDateTimeSave() {
            const date = document.getElementById('<%= TextBox1.ClientID %>').value;

            const timeSlotChecked =
                document.getElementById('<%= rb8AM12PM.ClientID %>').checked ||
                document.getElementById('<%= rb12PM4PM.ClientID %>').checked ||
                document.getElementById('<%= rb4PM8PM.ClientID %>').checked ||
                document.getElementById('<%= rb8PM12AM.ClientID %>').checked;

            if (!date) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Missing Date',
                    text: 'Please select a service date.',
                    confirmButtonColor: '#2563eb'
                });
                return;
            }

            if (!timeSlotChecked) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Missing Time Slot',
                    text: 'Please select a time slot.',
                    confirmButtonColor: '#2563eb'
                });
                return;
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
                return;
            }

            Swal.fire({
                title: 'Confirm Schedule?',
                text: 'Are you sure you want to save this schedule?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#16a34a',
                cancelButtonColor: '#6b7280',
                confirmButtonText: 'Yes, save it',
                cancelButtonText: 'Cancel'
            }).then((result) => {
                if (result.isConfirmed) {
                    applyDateTime();
                }
            });
        }

        function applyDateTime() {
            const date = document.getElementById('<%= TextBox1.ClientID %>').value;
            let timeSlotText = '';
            let timeSlotId = 0;

            if (document.getElementById('<%= rb8AM12PM.ClientID %>').checked) {
                timeSlotText = '8:00 AM - 12:00 PM (Morning)';
                timeSlotId = 1;
            } else if (document.getElementById('<%= rb12PM4PM.ClientID %>').checked) {
                timeSlotText = '12:00 PM - 4:00 PM (Afternoon)';
                timeSlotId = 2;
            } else if (document.getElementById('<%= rb4PM8PM.ClientID %>').checked) {
                timeSlotText = '4:00 PM - 8:00 PM (Evening)';
                timeSlotId = 3;
            } else if (document.getElementById('<%= rb8PM12AM.ClientID %>').checked) {
                timeSlotText = '8:00 PM - 12:00 AM (Night)';
                timeSlotId = 4;
            }

            const label = document.getElementById('<%= lblSelectedDateTime.ClientID %>');
            const hiddenDate = document.getElementById('<%= txtDate.ClientID %>');
            const hiddenTimeSlot = document.getElementById('<%= txtTimeSlot.ClientID %>');

            const selectedDate = new Date(date);
            const formattedDate = selectedDate.toLocaleDateString('en-US', {
                month: 'long', day: 'numeric', year: 'numeric', weekday: 'long'
            });

            label.innerText = `${formattedDate} at ${timeSlotText}`;
            label.classList.remove('text-gray-500', 'italic');
            label.classList.add('text-green-600', 'font-semibold');

            hiddenDate.value = date;
            hiddenTimeSlot.value = timeSlotId;

            hideDateTimeModal();
        }

        // ===== Validate booking before submit =====
        function validateBooking() {
            const date = document.getElementById('<%= txtDate.ClientID %>').value;
            const timeSlot = document.getElementById('<%= txtTimeSlot.ClientID %>').value;

            if (!date || !timeSlot) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Missing Schedule',
                    text: 'Please select a date and time slot for your service.',
                    confirmButtonColor: '#2563eb'
                });
                return false;
            }

            return true;
        }

        // Close modal on backdrop click
        modalEl.addEventListener('click', (e) => {
            if (e.target.id === 'dateTimeModal') {
                hideDateTimeModal();
            }
        });

        // Close modal on Escape key
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && !modalEl.classList.contains('hidden')) {
                hideDateTimeModal();
            }
        });
    </script>
</asp:Content>