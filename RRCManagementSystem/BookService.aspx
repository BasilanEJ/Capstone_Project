<%@ Page Title="Book Service" Language="C#" MasterPageFile="~/Client.master" AutoEventWireup="true" CodeBehind="BookService.aspx.cs" Inherits="RRCManagementSystem.BookService" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
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

                <!-- Services -->
                <div class="flex items-start">
                    <div class="flex-shrink-0 w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center mr-4">
                        <i class="fas fa-tools text-green-600"></i>
                    </div>
                    <div class="flex-1">
                        <label class="block text-sm font-medium text-gray-600 mb-1">Service(s) to be Provided</label>
                        <asp:Label ID="lblServices" runat="server"
                            CssClass="text-gray-800 leading-relaxed" />
                    </div>
                </div>

                <!-- SQM -->
                <div class="flex items-start">
                    <div class="flex-shrink-0 w-10 h-10 bg-purple-100 rounded-lg flex items-center justify-center mr-4">
                        <i class="fas fa-ruler-combined text-purple-600"></i>
                    </div>
                    <div class="flex-1">
                        <label class="block text-sm font-medium text-gray-600 mb-1">Square Meters (SQM)</label>
                        <asp:Label ID="lblSQM" runat="server"
                            CssClass="text-gray-800 font-medium" />
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

                    <!-- Miscellaneous -->
                    <div class="flex justify-between items-center">
                        <span class="text-gray-600">Miscellaneous</span>
                        <asp:Label ID="lblMiscellaneous" runat="server"
                            CssClass="font-semibold text-gray-800" />
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
                            Select Date &amp; Time
                        </button>

                        <asp:TextBox ID="txtDate" runat="server" CssClass="hidden" />
                        <asp:TextBox ID="txtTime" runat="server" CssClass="hidden" />
                    </div>
                </div>

                <!-- Additional Notes -->
                <div class="flex items-start">
                    <div class="flex-shrink-0 w-10 h-10 bg-yellow-100 rounded-lg flex items-center justify-center mr-4">
                        <i class="fas fa-sticky-note text-yellow-600"></i>
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
                        CssClass="w-full bg-gradient-to-r from-green-600 to-green-700 hover:from-green-700 hover:to-green-800 text-white font-semibold py-3.5 px-6 rounded-lg shadow-lg hover:shadow-xl transition transform hover:-translate-y-0.5 flex items-center justify-center"
                        OnClick="btnBook_Click" />
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
        <div class="bg-white rounded-2xl shadow-2xl w-full max-w-md mx-4 overflow-hidden">
            <!-- Modal Header -->
            <div class="bg-gradient-to-r from-blue-600 to-blue-700 px-6 py-4">
                <h3 class="text-xl font-semibold text-white flex items-center">
                    <i class="fas fa-calendar-check mr-3"></i>
                    Select Preferred Schedule
                </h3>
            </div>

            <!-- Modal Body -->
            <div class="p-6 space-y-5">
                <div>
                    <label class="block text-gray-700 font-medium mb-2 flex items-center">
                        <i class="fas fa-calendar text-blue-600 mr-2"></i>
                        Preferred Service Date
                    </label>
                    <asp:TextBox ID="TextBox1" runat="server" TextMode="Date"
                        CssClass="w-full rounded-lg border-2 border-gray-300 px-4 py-3 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 transition" />
                </div>

                <div>
                    <label class="block text-gray-700 font-medium mb-2 flex items-center">
                        <i class="fas fa-clock text-blue-600 mr-2"></i>
                        Preferred Service Time
                    </label>
                    <asp:TextBox ID="TextBox2" runat="server" TextMode="Time"
                        CssClass="w-full rounded-lg border-2 border-gray-300 px-4 py-3 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 transition" />
                </div>

                <div class="bg-blue-50 border border-blue-200 rounded-lg p-4">
                    <p class="text-sm text-blue-800 flex items-start">
                        <i class="fas fa-info-circle mt-0.5 mr-2"></i>
                        <span>Please select a date and time at least 1 hour from now to ensure proper scheduling.</span>
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
                    <i class="fas fa-check mr-2"></i>Save
                </button>
            </div>
        </div>
    </div>

    <script>
        const modalEl = document.getElementById('dateTimeModal');

        // Set minimum date and time
        window.addEventListener('load', function () {
            const dateInput = document.getElementById('<%= TextBox1.ClientID %>');
            const timeInput = document.getElementById('<%= TextBox2.ClientID %>');

            // Set minimum date to today
            const today = new Date().toISOString().split('T')[0];
            dateInput.setAttribute('min', today);
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

        function applyDateTime() {
            const date = document.getElementById('<%= TextBox1.ClientID %>').value;
            const time = document.getElementById('<%= TextBox2.ClientID %>').value;
            const label = document.getElementById('<%= lblSelectedDateTime.ClientID %>');
            const hiddenDate = document.getElementById('<%= txtDate.ClientID %>');
            const hiddenTime = document.getElementById('<%= txtTime.ClientID %>');

            if (!date || !time) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Incomplete',
                    text: 'Please select both a date and a time.',
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
                    confirmButtonColor: '#dc2626'
                });
                return;
            }

            // Format for display
            const formattedDate = selectedDateTime.toLocaleDateString('en-US', { 
                month: 'long', day: 'numeric', year: 'numeric' 
            });
            const formattedTime = selectedDateTime.toLocaleTimeString('en-US', {
                hour: '2-digit', minute: '2-digit', hour12: true
            });

            label.innerText = `${formattedDate} at ${formattedTime}`;
            label.classList.remove('text-gray-500', 'italic');
            label.classList.add('text-green-600', 'font-semibold');
            
            hiddenDate.value = date;
            hiddenTime.value = time;
            hideDateTimeModal();
        }

        function confirmDateTimeSave() {
            const date = document.getElementById('<%= TextBox1.ClientID %>').value;
            const time = document.getElementById('<%= TextBox2.ClientID %>').value;

            if (!date || !time) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Incomplete',
                    text: 'Please select both a date and a time.',
                    confirmButtonColor: '#2563eb'
                });
                return;
            }

            Swal.fire({
                title: 'Confirm Schedule?',
                text: 'Are you sure you want to save this date and time?',
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