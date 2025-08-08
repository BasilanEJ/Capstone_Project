    <%@ Page Title="" Language="C#" MasterPageFile="~/Client.master" AutoEventWireup="true" CodeBehind="BookService.aspx.cs" Inherits="RRCManagementSystem.BookService" %>

    <asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

        <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

        <script>
            function showDateTimeModal() {
                document.getElementById("dateTimeModal").style.display = "flex";
                setDateTimeLimits();
            }

            function hideDateTimeModal() {
                document.getElementById("dateTimeModal").style.display = "none";
            }

            function setDateTimeLimits() {
                const now = new Date();
                const todayStr = now.toISOString().split("T")[0];

                const modalDateInput = document.getElementById('<%= TextBox1.ClientID %>');
                const modalTimeInput = document.getElementById('<%= TextBox2.ClientID %>');

                // ✅ Set minimum date (today)
                modalDateInput.setAttribute("min", todayStr);

                // ✅ Calculate maximum date (same day 6 months ahead)
                const maxDate = new Date(now);
                maxDate.setMonth(maxDate.getMonth() + 6);

                // Handle cases where day doesn't exist in new month (e.g. Aug 31 + 6 months)
                if (maxDate.getDate() !== now.getDate()) {
                    // Set to last valid day of the new month
                    maxDate.setDate(0);
                }

                const maxDateStr = maxDate.toISOString().split("T")[0];
                modalDateInput.setAttribute("max", maxDateStr);

                // ⚠ Handle min time only if selected date is today
                function updateMinTime() {
                    const selectedStr = modalDateInput.value;
                    if (selectedStr === todayStr) {
                        const hours = now.getHours().toString().padStart(2, '0');
                        const minutes = now.getMinutes().toString().padStart(2, '0');
                        modalTimeInput.setAttribute("min", `${hours}:${minutes}`);
                    } else {
                        modalTimeInput.removeAttribute("min");
                    }
                }

                modalDateInput.addEventListener("change", updateMinTime);
                modalTimeInput.addEventListener("focus", updateMinTime);
                modalDateInput.dispatchEvent(new Event('change'));
            }



            function applyDateTime() {
                const modalDateInput = document.getElementById('<%= TextBox1.ClientID %>');
                const modalTimeInput = document.getElementById('<%= TextBox2.ClientID %>');
                const label = document.getElementById('<%= lblSelectedDateTime.ClientID %>');
                const hiddenDate = document.getElementById('<%= txtDate.ClientID %>');
                const hiddenTime = document.getElementById('<%= txtTime.ClientID %>');

                const selectedDate = modalDateInput.value;
                const selectedTime = modalTimeInput.value;

                if (selectedDate && selectedTime) {
                    label.innerText = `${selectedDate} at ${selectedTime}`;
                    hiddenDate.value = selectedDate;
                    hiddenTime.value = selectedTime;
                } else {
                    label.innerText = "Not selected yet";
                }

                hideDateTimeModal();
            }

            function confirmDateTimeSave() {
                Swal.fire({
                    title: 'Confirm Schedule?',
                    text: "Are you sure you want to save this date and time?",
                    icon: 'question',
                    showCancelButton: true,
                    confirmButtonColor: '#198754',
                    cancelButtonColor: '#d33',
                    confirmButtonText: 'Yes, save it!',
                    cancelButtonText: 'Cancel'
                }).then((result) => {
                    if (result.isConfirmed) {
                        applyDateTime();
                    }
                });
            }

            window.onload = function () {
                var msgLabel = document.getElementById('<%= lblMessage.ClientID %>');
                if (msgLabel && msgLabel.innerText.trim() !== "") {
                    Swal.fire({
                        icon: 'success',
                        title: 'Success',
                        text: msgLabel.innerText.trim(),
                        confirmButtonColor: '#3085d6'
                    });
                    msgLabel.style.display = 'none';
                }
            };
        </script>

        <div class="container mt-5">
            <div class="card shadow mx-auto" style="max-width: 700px;">
                <div class="card-body">
                    <h2 class="text-center text-primary mb-4">📋 Confirm Your Booking</h2>

                    <asp:Label ID="lblMessage" runat="server" CssClass="text-success fw-bold text-center d-block mb-3" />
                    <asp:HiddenField ID="hfIsContract" runat="server" />
                    <asp:HiddenField ID="hfQuotationID" runat="server" />


                    <div class="mb-3">
                        <label class="form-label">Service(s) to be Provided</label>
                        <asp:Label ID="lblServices" runat="server" CssClass="form-control rounded bg-light px-3 py-2" />
                    </div>

                    <div class="mb-3">
                        <label class="form-label">Square Meters (SQM)</label>
                        <asp:Label ID="lblSQM" runat="server" CssClass="form-control rounded bg-light px-3 py-2" />
                    </div>

                    <div class="mb-3">
                        <label class="form-label">Total Estimated Price</label>
                        <asp:Label ID="lblPrice" runat="server" CssClass="form-control rounded bg-light px-3 py-2" />
                    </div>

                    <div class="mb-3">
                        <label class="form-label">Preferred Schedule</label>
                        <asp:Label ID="lblSelectedDateTime" runat="server" CssClass="form-control rounded bg-light px-3 py-2" Text="Not selected yet" />
                        <button type="button" class="btn btn-primary mt-2" onclick="showDateTimeModal()">Select Date & Time</button>
                        <asp:TextBox ID="txtDate" runat="server" CssClass="d-none" />
                        <asp:TextBox ID="txtTime" runat="server" CssClass="d-none" />
                    </div>

                    <div class="mb-4">
                        <label class="form-label">Additional Notes</label>
                        <asp:TextBox ID="txtNotes" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" />
                    </div>

                    <div class="mb-3">
    <label class="form-label">Inspector Who Quoted</label>
    <asp:Label ID="lblInspector" runat="server" CssClass="form-control rounded bg-light px-3 py-2" />
</div>


                    <asp:Button ID="btnBook" runat="server" Text="Book Now" CssClass="btn btn-success w-100" OnClick="btnBook_Click" />
                </div>
            </div>
        </div>

        <!-- Modal -->
        <div id="dateTimeModal" class="modal position-fixed top-0 start-0 w-100 h-100 bg-dark bg-opacity-50 justify-content-center align-items-center" style="display:none; z-index:1050;">
            <div class="bg-white p-4 rounded shadow" style="max-width: 400px; width: 90%;">
                <h5 class="mb-3">Select Preferred Schedule</h5>
                <div class="mb-3">
                    <label class="form-label">📅 Preferred Service Date</label>
                    <asp:TextBox ID="TextBox1" runat="server" CssClass="form-control" TextMode="Date" />
                </div>
                <div class="mb-3">
                    <label class="form-label">⏰ Preferred Service Time</label>
                    <asp:TextBox ID="TextBox2" runat="server" CssClass="form-control" TextMode="Time" />
                </div>
                <div class="d-flex justify-content-end gap-2">
                    <button type="button" class="btn btn-success" onclick="confirmDateTimeSave()">Save</button>
                    <button type="button" class="btn btn-secondary" onclick="hideDateTimeModal()">Cancel</button>
                </div>
            </div>
        </div>

    </asp:Content>
