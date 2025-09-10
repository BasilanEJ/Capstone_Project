<%@ Page Title="Book Service" Language="C#" MasterPageFile="~/Client.master" AutoEventWireup="true" CodeBehind="BookService.aspx.cs" Inherits="RRCManagementSystem.BookService" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- SweetAlert2 -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <style>
        /* Fluid headings and spacing for all screen sizes */
        .page-title {
            font-weight: 700;
            color: #0d6efd; /* Bootstrap primary */
            font-size: clamp(1.25rem, 3.2vw, 1.75rem);
            line-height: 1.2;
        }
        .page-wrap {
            padding-top: clamp(0.75rem, 2vw, 1.25rem);
            padding-bottom: clamp(0.75rem, 2vw, 1.5rem);
        }

        /* Card polish */
        .booking-card {
            border: none;
            border-radius: 1rem;
            box-shadow: 0 8px 20px rgba(0,0,0,.06);
        }

        /* Labels styled like read-only inputs for consistency */
        .readonly-field {
            background-color: #f8f9fa;
            border: 1px solid #e9ecef;
            border-radius: .5rem;
            padding: .6rem .9rem;
            min-height: 44px; /* comfy tap target */
            display: block;
            width: 100%;
        }

        /* Buttons span full width on mobile, auto on larger screens */
        .responsive-btn {
            width: 100%;
        }
        @media (min-width: 576px) {
            .responsive-btn { width: auto; }
        }

        /* Textarea mobile comfort */
        textarea.form-control {
            min-height: 90px;
        }

        /* Respect reduced motion */
        @media (prefers-reduced-motion: reduce) {
            .modal,
            .swal2-popup { transition: none !important; }
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container page-wrap">
        <div class="card booking-card mx-auto" style="max-width: 720px;">
            <div class="card-body p-3 p-sm-4">
                <h2 class="page-title text-center mb-3">📋 Confirm Your Booking</h2>

                <asp:Label ID="lblMessage" runat="server" CssClass="text-success fw-semibold text-center d-block mb-3" />

                <asp:HiddenField ID="hfIsContract" runat="server" />
                <asp:HiddenField ID="hfQuotationID" runat="server" />

                <div class="mb-3">
    <label class="form-label">Quotation Code</label>
    <asp:Label ID="lblQuotationCode" runat="server" CssClass="readonly-field" />
</div>

                <!-- Services -->
                <div class="mb-3">
                    <label class="form-label">Service(s) to be Provided</label>
                    <asp:Label ID="lblServices" runat="server" CssClass="readonly-field" />
                </div>

                <!-- SQM -->
                <div class="mb-3">
                    <label class="form-label">Square Meters (SQM)</label>
                    <asp:Label ID="lblSQM" runat="server" CssClass="readonly-field" />
                </div>

               <div class="mb-3">
    <label class="form-label">Base Service Price (Based on SQM)</label>
    <asp:Label ID="lblBasePrice" runat="server" CssClass="readonly-field" />
</div>

<!-- Travel Expense -->
<div class="mb-3">
    <label class="form-label">Travel Expense</label>
    <asp:Label ID="lblTravelExpense" runat="server" CssClass="readonly-field" />
</div>

<!-- Miscellaneous -->
<div class="mb-3">
    <label class="form-label">Miscellaneous</label>
    <asp:Label ID="lblMiscellaneous" runat="server" CssClass="readonly-field" />
</div>

<!-- Total Price -->
<div class="mb-3">
    <label class="form-label">Total Price</label>
    <asp:Label ID="lblTotalPrice" runat="server" CssClass="readonly-field" />
</div>

                <!-- Preferred Schedule -->
                <div class="mb-3">
                    <label class="form-label">Preferred Schedule</label>
                    <asp:Label ID="lblSelectedDateTime" runat="server" CssClass="readonly-field" Text="Not selected yet" />
                    <div class="mt-2 d-flex flex-wrap gap-2">
                        <button type="button" class="btn btn-primary responsive-btn" onclick="showDateTimeModal()">
                            Select Date &amp; Time
                        </button>
                    </div>
                    <!-- hidden fields actually posted -->
                    <asp:TextBox ID="txtDate" runat="server" CssClass="d-none" />
                    <asp:TextBox ID="txtTime" runat="server" CssClass="d-none" />
                </div>

                <!-- Notes -->
                <div class="mb-3">
                    <label class="form-label">Additional Notes</label>
                    <asp:TextBox ID="txtNotes" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" />
                </div>

                <!-- Inspector -->
                <div class="mb-4">
                    <label class="form-label">Inspector Who Quoted</label>
                    <asp:Label ID="lblInspector" runat="server" CssClass="readonly-field" />
                </div>

                <!-- Submit -->
                <asp:Button ID="btnBook" runat="server" Text="Book Now" CssClass="btn btn-success w-100" OnClick="btnBook_Click" />
            </div>
        </div>
    </div>

    <!-- Bootstrap Modal for Date & Time -->
    <div class="modal fade" id="dateTimeModal" tabindex="-1" aria-labelledby="dateTimeModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="dateTimeModalLabel">Select Preferred Schedule</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>

                <div class="modal-body">
                    <div class="mb-3">
                        <label class="form-label">📅 Preferred Service Date</label>
                        <asp:TextBox ID="TextBox1" runat="server" CssClass="form-control" TextMode="Date" />
                    </div>
                    <div class="mb-1">
                        <label class="form-label">⏰ Preferred Service Time</label>
                        <asp:TextBox ID="TextBox2" runat="server" CssClass="form-control" TextMode="Time" />
                    </div>
                </div>

                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                    <button type="button" class="btn btn-success" onclick="confirmDateTimeSave()">Save</button>
                </div>
            </div>
        </div>
    </div>

    <!-- Page Scripts -->
    <script>
        // Bootstrap modal instance
        let dateTimeBsModal = null;

        function showDateTimeModal() {
            const modalEl = document.getElementById('dateTimeModal');
            if (!dateTimeBsModal) {
                dateTimeBsModal = new bootstrap.Modal(modalEl, { backdrop: 'static' });
                // On each open, refresh limits and focus date
                modalEl.addEventListener('shown.bs.modal', () => {
                    setDateTimeLimits();
                    const dateInput = document.getElementById('<%= TextBox1.ClientID %>');
                    if (dateInput) dateInput.focus();
                });
            }
            dateTimeBsModal.show();
        }

        function hideDateTimeModal() {
            if (dateTimeBsModal) dateTimeBsModal.hide();
        }

        // Set min/max for date + min for time when date is today
        function setDateTimeLimits() {
            const now = new Date();
            const todayStr = now.toISOString().split("T")[0];

            const modalDateInput = document.getElementById('<%= TextBox1.ClientID %>');
            const modalTimeInput = document.getElementById('<%= TextBox2.ClientID %>');

            if (!modalDateInput || !modalTimeInput) return;

            // Min date = today
            modalDateInput.setAttribute("min", todayStr);

            // Max date = same day, 6 months ahead (adjust for end-of-month)
            const maxDate = new Date(now);
            const originalDay = maxDate.getDate();
            maxDate.setMonth(maxDate.getMonth() + 6);
            if (maxDate.getDate() !== originalDay) {
                // Move to last day of previous month if overflowed
                maxDate.setDate(0);
            }
            const maxDateStr = maxDate.toISOString().split("T")[0];
            modalDateInput.setAttribute("max", maxDateStr);

            // Min time if chosen date is today
            function updateMinTime() {
                const selectedStr = modalDateInput.value;
                if (selectedStr === todayStr) {
                    const hh = String(now.getHours()).padStart(2, '0');
                    const mm = String(now.getMinutes()).padStart(2, '0');
                    modalTimeInput.setAttribute("min", `${hh}:${mm}`);
                } else {
                    modalTimeInput.removeAttribute("min");
                }
            }

            // Bind & trigger once
            modalDateInput.removeEventListener("change", updateMinTime);
            modalDateInput.addEventListener("change", updateMinTime);
            updateMinTime();
        }

        function applyDateTime() {
            const modalDateInput = document.getElementById('<%= TextBox1.ClientID %>');
            const modalTimeInput = document.getElementById('<%= TextBox2.ClientID %>');
            const label = document.getElementById('<%= lblSelectedDateTime.ClientID %>');
            const hiddenDate = document.getElementById('<%= txtDate.ClientID %>');
            const hiddenTime = document.getElementById('<%= txtTime.ClientID %>');

            const selectedDate = modalDateInput?.value || "";
            const selectedTime = modalTimeInput?.value || "";

            if (selectedDate && selectedTime) {
                // Nice readable format (YYYY-MM-DD HH:mm)
                label.innerText = `${selectedDate} at ${selectedTime}`;
                if (hiddenDate) hiddenDate.value = selectedDate;
                if (hiddenTime) hiddenTime.value = selectedTime;
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

        // Show SweetAlert if server set lblMessage
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
