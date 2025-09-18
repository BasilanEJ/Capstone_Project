<%@ Page Title="Book Service" Language="C#" MasterPageFile="~/Client.master" AutoEventWireup="true" CodeBehind="BookService.aspx.cs" Inherits="RRCManagementSystem.BookService" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Tailwind -->
    <script src="https://cdn.tailwindcss.com"></script>
    <!-- SweetAlert2 -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="max-w-3xl mx-auto px-4 py-10">
        <!-- Page Title -->
        <h2 class="text-2xl md:text-3xl font-bold text-center text-blue-700 mb-6">
            📋 Confirm Your Booking
        </h2>

        <!-- Success Message -->
        <asp:Label ID="lblMessage" runat="server"
            CssClass="hidden text-green-600 text-center mb-4 font-medium" />

        <asp:HiddenField ID="hfIsContract" runat="server" />
        <asp:HiddenField ID="hfQuotationID" runat="server" />

        <!-- Booking Card -->
        <div class="bg-white rounded-2xl shadow-lg p-6 space-y-5">
            <!-- Quotation Code -->
            <div>
                <label class="block text-gray-700 font-medium mb-1">Quotation Code</label>
                <asp:Label ID="lblQuotationCode" runat="server" 
                    CssClass="block w-full rounded-lg bg-gray-50 border border-gray-200 px-4 py-2 text-gray-800" />
            </div>

            <!-- Services -->
            <div>
                <label class="block text-gray-700 font-medium mb-1">Service(s) to be Provided</label>
                <asp:Label ID="lblServices" runat="server"
                    CssClass="block w-full rounded-lg bg-gray-50 border border-gray-200 px-4 py-2 text-gray-800" />
            </div>

            <!-- SQM -->
            <div>
                <label class="block text-gray-700 font-medium mb-1">Square Meters (SQM)</label>
                <asp:Label ID="lblSQM" runat="server"
                    CssClass="block w-full rounded-lg bg-gray-50 border border-gray-200 px-4 py-2 text-gray-800" />
            </div>

            <!-- Base Price -->
            <div>
                <label class="block text-gray-700 font-medium mb-1">Base Service Price (Based on SQM)</label>
                <asp:Label ID="lblBasePrice" runat="server"
                    CssClass="block w-full rounded-lg bg-gray-50 border border-gray-200 px-4 py-2 text-gray-800" />
            </div>

            <!-- Travel Expense -->
            <div>
                <label class="block text-gray-700 font-medium mb-1">Travel Expense</label>
                <asp:Label ID="lblTravelExpense" runat="server"
                    CssClass="block w-full rounded-lg bg-gray-50 border border-gray-200 px-4 py-2 text-gray-800" />
            </div>

            <!-- Miscellaneous -->
            <div>
                <label class="block text-gray-700 font-medium mb-1">Miscellaneous</label>
                <asp:Label ID="lblMiscellaneous" runat="server"
                    CssClass="block w-full rounded-lg bg-gray-50 border border-gray-200 px-4 py-2 text-gray-800" />
            </div>

            <!-- Total Price -->
            <div>
                <label class="block text-gray-700 font-semibold mb-1">Total Price</label>
                <asp:Label ID="lblTotalPrice" runat="server"
                    CssClass="block w-full rounded-lg bg-blue-50 border border-blue-200 px-4 py-2 text-blue-800 font-semibold" />
            </div>

            <!-- Preferred Schedule -->
            <div>
                <label class="block text-gray-700 font-medium mb-1">Preferred Schedule</label>
                <asp:Label ID="lblSelectedDateTime" runat="server" Text="Not selected yet"
                    CssClass="block w-full rounded-lg bg-gray-50 border border-gray-200 px-4 py-2 text-gray-800" />

                <div class="mt-3">
                    <button type="button" onclick="showDateTimeModal()"
                        class="w-full md:w-auto bg-blue-600 hover:bg-blue-700 text-white px-5 py-2 rounded-lg transition">
                        Select Date &amp; Time
                    </button>
                </div>

                <!-- Hidden fields for server -->
                <asp:TextBox ID="txtDate" runat="server" CssClass="hidden" />
                <asp:TextBox ID="txtTime" runat="server" CssClass="hidden" />
            </div>

            <!-- Additional Notes -->
            <div>
                <label class="block text-gray-700 font-medium mb-1">Additional Notes</label>
                <asp:TextBox ID="txtNotes" runat="server" TextMode="MultiLine" Rows="3"
                    CssClass="w-full rounded-lg border border-gray-300 px-4 py-2 focus:border-blue-500 focus:ring focus:ring-blue-200" />
            </div>

            <!-- Inspector -->
            <div>
                <label class="block text-gray-700 font-medium mb-1">Inspector Who Quoted</label>
                <asp:Label ID="lblInspector" runat="server"
                    CssClass="block w-full rounded-lg bg-gray-50 border border-gray-200 px-4 py-2 text-gray-800" />
            </div>

            <!-- Submit Button -->
            <asp:Button ID="btnBook" runat="server" Text="Book Now"
                CssClass="w-full bg-green-600 hover:bg-green-700 text-white font-semibold py-2 px-5 rounded-lg shadow transition"
                OnClick="btnBook_Click" />
        </div>
    </div>

    <!-- Date & Time Modal -->
    <div class="hidden fixed inset-0 z-50 items-center justify-center bg-black/50 backdrop-blur-sm" id="dateTimeModal">
        <div class="bg-white rounded-xl shadow-lg w-full max-w-md p-6 relative">
            <h3 class="text-lg font-bold text-blue-700 mb-4">Select Preferred Schedule</h3>

            <!-- Date Picker -->
            <div class="mb-4">
                <label class="block text-gray-700 font-medium mb-1">📅 Preferred Service Date</label>
                <asp:TextBox ID="TextBox1" runat="server" TextMode="Date"
                    CssClass="w-full rounded-lg border border-gray-300 px-4 py-2 focus:border-blue-500 focus:ring focus:ring-blue-200" />
            </div>

            <!-- Time Picker -->
            <div class="mb-4">
                <label class="block text-gray-700 font-medium mb-1">⏰ Preferred Service Time</label>
                <asp:TextBox ID="TextBox2" runat="server" TextMode="Time"
                    CssClass="w-full rounded-lg border border-gray-300 px-4 py-2 focus:border-blue-500 focus:ring focus:ring-blue-200" />
            </div>

            <!-- Modal Actions -->
            <div class="flex justify-end gap-3">
                <button type="button" onclick="hideDateTimeModal()"
                    class="px-4 py-2 rounded-lg border border-gray-300 text-gray-600 hover:bg-gray-100 transition">
                    Cancel
                </button>
                <button type="button" onclick="confirmDateTimeSave()"
                    class="px-4 py-2 rounded-lg bg-green-600 hover:bg-green-700 text-white shadow transition">
                    Save
                </button>
            </div>
        </div>
    </div>

    <script>
        const modalEl = document.getElementById('dateTimeModal');

        function showDateTimeModal() {
            modalEl.classList.remove('hidden', 'opacity-0');
            modalEl.classList.add('flex');
        }

        function hideDateTimeModal() {
            modalEl.classList.add('hidden');
            modalEl.classList.remove('flex');
        }

        // Apply selected date/time
        function applyDateTime() {
            const date = document.getElementById('<%= TextBox1.ClientID %>').value;
            const time = document.getElementById('<%= TextBox2.ClientID %>').value;
            const label = document.getElementById('<%= lblSelectedDateTime.ClientID %>');
            const hiddenDate = document.getElementById('<%= txtDate.ClientID %>');
            const hiddenTime = document.getElementById('<%= txtTime.ClientID %>');

            if (date && time) {
                label.innerText = `${date} at ${time}`;
                hiddenDate.value = date;
                hiddenTime.value = time;
                hideDateTimeModal();
            } else {
                Swal.fire({
                    icon: 'warning',
                    title: 'Incomplete',
                    text: 'Please select both a date and a time.',
                    confirmButtonColor: '#0d6efd'
                });
            }
        }

        function confirmDateTimeSave() {
            Swal.fire({
                title: 'Confirm Schedule?',
                text: 'Are you sure you want to save this date and time?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#198754',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes, save it',
                cancelButtonText: 'Cancel'
            }).then((result) => {
                if (result.isConfirmed) {
                    applyDateTime();
                }
            });
        }

        // Show SweetAlert if message exists
        window.addEventListener('load', function () {
            var msgLabel = document.getElementById('<%= lblMessage.ClientID %>');
            if (msgLabel && msgLabel.innerText.trim() !== "") {
                Swal.fire({
                    icon: 'success',
                    title: 'Success',
                    text: msgLabel.innerText.trim(),
                    confirmButtonColor: '#0d6efd'
                });
                msgLabel.style.display = 'none';
            }
        });
    </script>
</asp:Content>
