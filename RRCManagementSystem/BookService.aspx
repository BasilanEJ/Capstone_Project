<%@ Page Title="" Language="C#" MasterPageFile="~/Client.master" AutoEventWireup="true" CodeBehind="BookService.aspx.cs" Inherits="RRCManagementSystem.BookService" %>


<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .card-container {
            max-width: 700px;
            margin: 30px auto;
            background-color: #ffffff;
            padding: 30px;
            border-radius: 12px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
        }

        .card-container h2 {
            text-align: center;
            color: #004085;
            margin-bottom: 25px;
        }

        .form-group {
            margin-bottom: 20px;
        }

        .form-label {
            font-weight: 600;
            display: block;
            margin-bottom: 5px;
        }

        .form-control {
            width: 100%;
            padding: 10px;
            border-radius: 6px;
            border: 1px solid #ccc;
        }

        .btn-submit {
            background-color: #004085;
            color: #fff;
            padding: 10px 25px;
            border: none;
            border-radius: 6px;
            cursor: pointer;
            font-weight: bold;
        }

        .btn-submit:hover {
            background-color: #002f6c;
        }

        .readonly-label {
            background-color: #f9f9f9;
            padding: 10px;
            border-radius: 6px;
            border: 1px solid #ddd;
            display: inline-block;
            min-width: 100%;
        }

        .message {
            text-align: center;
            font-weight: bold;
            color: green;
            margin-top: 10px;
        }

        /* Modal styling */
        .modal-overlay {
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background-color: rgba(0, 0, 0, 0.5);
            display: none;
            z-index: 1000;
            justify-content: center;
            align-items: center;
        }

        .modal-content {
            background: #fff;
            padding: 30px;
            border-radius: 12px;
            box-shadow: 0 8px 24px rgba(0,0,0,0.2);
            max-width: 400px;
            width: 90%;
            animation: fadeIn 0.3s ease-in-out;
        }

        .datetime-group {
            margin-bottom: 20px;
        }

        .datetime-group label {
            font-weight: 600;
            font-size: 14px;
            display: block;
            margin-bottom: 6px;
            color: #2c3e50;
        }

        .modal-buttons {
            display: flex;
            justify-content: flex-end;
            gap: 10px;
            margin-top: 20px;
        }

        @keyframes fadeIn {
            from { opacity: 0; transform: scale(0.95); }
            to { opacity: 1; transform: scale(1); }
        }

        .time-picker {
    appearance: none;
    padding: 12px;
    border-radius: 8px;
    border: 1px solid #ccc;
    background-color: #fdfdfd;
    font-size: 15px;
    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
    box-shadow: inset 0 1px 3px rgba(0, 0, 0, 0.1);
    cursor: pointer;
}

/* Calendar icon styling - safe to keep */
.time-picker::-webkit-calendar-picker-indicator {
    filter: invert(30%);
    cursor: pointer;
}

/* Focus style */
.time-picker:focus {
    outline: none;
    border-color: #004085;
    box-shadow: 0 0 0 2px rgba(0, 64, 133, 0.2);
}

/* Responsive Styles */
@media (max-width: 768px) {
    .card-container {
        padding: 20px;
        margin: 20px;
    }

    .card-container h2 {
        font-size: 20px;
    }

    .form-label {
        font-size: 14px;
    }

    .form-control,
    .readonly-label,
    .btn-submit {
        font-size: 14px;
        padding: 8px;
    }

    .modal-content {
        padding: 20px;
        max-width: 90%;
    }

    .modal-buttons {
        flex-direction: column;
        align-items: stretch;
    }

    .modal-buttons button {
        width: 100%;
        margin-bottom: 10px;
    }
}

@media (max-width: 480px) {
    .card-container {
        padding: 15px;
    }

    .btn-submit {
        padding: 10px;
    }

    .form-group {
        margin-bottom: 15px;
    }

    .modal-content h3 {
        font-size: 18px;
    }
}


    </style>
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

            const dateInput = document.getElementById('<%= txtDate.ClientID %>');
            const timeInput = document.getElementById('<%= txtTime.ClientID %>');

            const modalDateInput = document.getElementById('<%= TextBox1.ClientID %>');
            const modalTimeInput = document.getElementById('<%= TextBox2.ClientID %>');

            dateInput.setAttribute("min", todayStr);
            modalDateInput.setAttribute("min", todayStr);

            function updateMinTime() {
                const selected = new Date(modalDateInput.value);
                const selectedStr = selected.toISOString().split("T")[0];

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


    </script>

    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <div class="card-container">
        <h2>📋 Confirm Your Booking</h2>

        <asp:Label ID="lblMessage" runat="server" CssClass="message" />

        <div class="form-group">
            <label class="form-label">Service(s) to be Provided</label>
            <asp:Label ID="lblServices" runat="server" CssClass="readonly-label" />
        </div>

        <div class="form-group">
            <label class="form-label">Square Meters (SQM)</label>
            <asp:Label ID="lblSQM" runat="server" CssClass="readonly-label" />
        </div>

        <div class="form-group">
            <label class="form-label">Total Estimated Price</label>
            <asp:Label ID="lblPrice" runat="server" CssClass="readonly-label" />
        </div>

        <div class="form-group">
            <label class="form-label">Preferred Schedule</label>
            <asp:Label ID="lblSelectedDateTime" runat="server" CssClass="readonly-label" Text="Not selected yet" />
            <button type="button" class="btn-submit" onclick="showDateTimeModal()">Select Date & Time</button>
            <asp:TextBox ID="txtDate" runat="server" CssClass="form-control" TextMode="Date" Style="display:none;" />
            <asp:TextBox ID="txtTime" runat="server" CssClass="form-control" TextMode="Time" Style="display:none;" />
        </div>

        <div class="form-group">
            <label class="form-label">Additional Notes</label>
            <asp:TextBox ID="txtNotes" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" />
        </div>

        <asp:Button ID="btnBook" runat="server" Text="Book Now" CssClass="btn-submit" OnClick="btnBook_Click" />
    </div>

    <!-- Modal -->
    <div id="dateTimeModal" class="modal-overlay">
        <div class="modal-content">
            <h3>Select Preferred Schedule</h3>
            <div class="datetime-group">
                <label class="form-label">📅 Preferred Service Date</label>
                <asp:TextBox ID="TextBox1" runat="server" CssClass="form-control" TextMode="Date" />
            </div>
         <div class="datetime-group">
    <label class="form-label">⏰ Preferred Service Time</label>
    <asp:TextBox ID="TextBox2" runat="server" CssClass="form-control time-picker" TextMode="Time" />
</div>

            <div class="modal-buttons">
                <button type="button" class="btn-submit" onclick="applyDateTime()">Save</button>
                <button type="button" class="btn-submit" onclick="hideDateTimeModal()">Cancel</button>
            </div>
        </div>
    </div>
</asp:Content>