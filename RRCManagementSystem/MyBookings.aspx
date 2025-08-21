<%@ Page Title="My Bookings" Language="C#" MasterPageFile="~/Client.master" AutoEventWireup="true" CodeBehind="MyBookings.aspx.cs" Inherits="RRCManagementSystem.MyBookings" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- SweetAlert2 -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <!-- Bootstrap 5 bundle (includes Modal JS). If already included in Client.master, you can remove this line. -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5/dist/js/bootstrap.bundle.min.js"></script>

    <style>
        .page-title{ color:#0d6efd; font-weight:700; font-size:clamp(1.25rem,3.2vw,1.75rem); line-height:1.2; }
        .page-wrap{ padding-top:clamp(.75rem,2vw,1.25rem); padding-bottom:calc(110px + env(safe-area-inset-bottom)); }
        .card-shell{ border:none;border-radius:1rem;box-shadow:0 8px 20px rgba(0,0,0,.06); }
        .btn{ min-height:44px }

        .table-responsive{ overflow-x:auto; -webkit-overflow-scrolling: touch; scrollbar-width: thin; }
        .fixed-grid > table{ min-width:980px; table-layout:fixed; border-collapse:separate; border-spacing:0; }
        .fixed-grid > table th, .fixed-grid > table td{ vertical-align:middle; white-space:nowrap; word-break:normal; }

        .fixed-grid > table thead th:nth-child(1), .fixed-grid > table tbody td:nth-child(1){ min-width:110px; }
        .fixed-grid > table thead th:nth-child(2), .fixed-grid > table tbody td:nth-child(2){ min-width:140px; }
        .fixed-grid > table thead th:nth-child(3), .fixed-grid > table tbody td:nth-child(3){ min-width:140px; }
        .fixed-grid > table thead th:nth-child(4), .fixed-grid > table tbody td:nth-child(4){ min-width:120px; }
        .fixed-grid > table thead th:nth-child(5), .fixed-grid > table tbody td:nth-child(5){ min-width:120px; }
        .fixed-grid > table thead th:nth-child(6), .fixed-grid > table tbody td:nth-child(6){ min-width:120px; }
        .fixed-grid > table thead th:last-child, .fixed-grid > table tbody td:last-child{ min-width:140px; }

        .fixed-grid#wrap-gvMyBookings > table tbody td:nth-child(6),
        .fixed-grid#wrap-gvMyBookings > table thead th:nth-child(6){ white-space:normal; overflow-wrap:anywhere; }

        .fixed-grid#wrap-gvUpcoming > table tbody td:last-child{ white-space:nowrap; }

        @media (max-width: 576px) {
            .chat-fab { bottom: calc(88px + env(safe-area-inset-bottom)); }
            .fixed-grid{ padding-bottom:.5rem; }
        }
        @media (prefers-reduced-motion: reduce){ .modal,.swal2-popup{transition:none!important} }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <!-- REQUIRED because code-behind uses ScriptManager.RegisterStartupScript -->
    <asp:ScriptManager ID="sm1" runat="server" />

    <div class="container page-wrap">
        <div class="card card-shell p-3 p-sm-4">
            <h3 class="page-title mb-3">My Bookings</h3>

            <asp:Label ID="lblMessage" runat="server" CssClass="text-danger fw-semibold mb-3 d-block" />

            <!-- Bookings Grid -->
            <div class="table-responsive fixed-grid" id="wrap-gvMyBookings">
                <asp:GridView ID="gvMyBookings" runat="server"
                    AutoGenerateColumns="False"
                    CssClass="table table-bordered table-striped text-center align-middle"
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
                                <asp:LinkButton ID="btnCancel"
                                    runat="server"
                                    Text="Cancel"
                                    CommandName="CancelBooking"
                                    CommandArgument='<%# Eval("BookingID") %>'
                                    CssClass="btn btn-danger btn-sm"
                                    CausesValidation="false"
                                    UseSubmitBehavior="false"
                                    OnClientClick="return confirm('Are you sure you want to cancel this booking?');"
                                    Visible='<%# Eval("Status").ToString() == "Pending" %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>

            <!-- Next Operation Reminder -->
            <asp:Label ID="lblNextOperationNotice" runat="server" CssClass="alert alert-success fw-semibold mt-3 d-block" Visible="false" />

            <!-- Upcoming Operations -->
            <asp:Panel ID="pnlUpcomingOps" runat="server" Visible="false">
                <h4 class="text-primary mt-4 mb-2">Upcoming Operations (Next 30 Days)</h4>
                <div class="table-responsive fixed-grid" id="wrap-gvUpcoming">
                    <asp:GridView ID="gvUpcoming" runat="server" AutoGenerateColumns="False"
                        CssClass="table table-bordered table-striped text-center align-middle"
                        OnRowCommand="gvUpcoming_RowCommand">
                        <Columns>
                            <asp:BoundField DataField="ScheduleID" HeaderText="ID" Visible="false" />
                            <asp:BoundField DataField="ScheduledDate" HeaderText="Scheduled Date" DataFormatString="{0:yyyy-MM-dd}" />
                            <asp:BoundField DataField="OperationNumber" HeaderText="Operation" />
                            <asp:BoundField DataField="Status" HeaderText="Status" />
                            <asp:TemplateField HeaderText="Action">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnSetSchedule"
                                        runat="server"
                                        Text="Set Schedule"
                                        CommandName="SetSchedule"
                                        CommandArgument='<%# Eval("ScheduleID") + "|" + Eval("ScheduledDate", "{0:yyyy-MM-ddTHH:mm}") %>'
                                        CssClass="btn btn-primary btn-sm"
                                        CausesValidation="false"
                                        UseSubmitBehavior="false"
                                        Visible='<%# Eval("Status").ToString() != "Completed" %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </asp:Panel>

            <!-- All Scheduled Operations -->
            <asp:Panel ID="pnlAllOps" runat="server" Visible="false">
                <h4 class="text-primary mt-4 mb-2">All Scheduled Operations</h4>
                <div class="table-responsive fixed-grid" id="wrap-gvAllOps">
                    <asp:GridView ID="gvAllOps" runat="server" AutoGenerateColumns="False"
                        CssClass="table table-bordered table-striped text-center align-middle"
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
                                    <asp:LinkButton ID="btnReschedule"
                                        runat="server"
                                        CommandName="Reschedule"
                                        Text="Set New Schedule"
                                        CommandArgument='<%# Eval("ScheduleID") + "|" + Eval("ScheduledDate", "{0:yyyy-MM-ddTHH:mm}") %>'
                                        CssClass="btn btn-warning btn-sm"
                                        CausesValidation="false"
                                        UseSubmitBehavior="false"
                                        Visible='<%# 
                                            Eval("ScheduledDate") != DBNull.Value &&
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
                </div>
            </asp:Panel>

            <!-- Contract Status -->
            <asp:Label ID="lblContractStatus" runat="server" CssClass="alert alert-success fw-semibold mt-3 d-block" Visible="false" />
        </div>
    </div>

    <!-- Bootstrap Modal: Set New Schedule -->
    <div class="modal fade" id="setScheduleModal" tabindex="-1" aria-labelledby="setScheduleModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="setScheduleModalLabel">Set New Schedule</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <asp:HiddenField ID="hfSelectedScheduleID" runat="server" />
                    <div class="mb-3">
                        <label class="form-label">📅 Date</label>
                        <asp:TextBox ID="txtNewScheduleDate" runat="server" TextMode="Date" CssClass="form-control" />
                    </div>
                    <div class="mb-1">
                        <label class="form-label">⏰ Time</label>
                        <asp:TextBox ID="txtNewScheduleTime" runat="server" TextMode="Time" CssClass="form-control" />
                    </div>
                    <div class="form-text">Choose any date/time within your contract window.</div>
                </div>
                <div class="modal-footer">
                    <!-- Server button exists (for postback target) but click is triggered via JS confirm -->
                    <asp:Button ID="btnConfirmSchedule" runat="server" Text="Save" CssClass="btn btn-success"
                        OnClientClick="confirmSchedule(); return false;" OnClick="btnConfirmSchedule_Click" />
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                </div>
            </div>
        </div>
    </div>

    <script>
        let setScheduleBsModal = null;
        function ensureModal() {
            const el = document.getElementById('setScheduleModal');
            if (!setScheduleBsModal) {
                setScheduleBsModal = new bootstrap.Modal(el, { backdrop: 'static' });
            }
            return setScheduleBsModal;
        }

        function showModal(scheduleId, currentDateTime) {
            document.getElementById('<%= hfSelectedScheduleID.ClientID %>').value = scheduleId;

            const dt = currentDateTime ? new Date(currentDateTime) : new Date();
            const yyyy = dt.getFullYear();
            const mm = String(dt.getMonth() + 1).padStart(2, '0');
            const dd = String(dt.getDate()).padStart(2, '0');
            const hh = String(dt.getHours()).padStart(2, '0');
            const min = String(dt.getMinutes()).padStart(2, '0');

            document.getElementById('<%= txtNewScheduleDate.ClientID %>').value = `${yyyy}-${mm}-${dd}`;
            document.getElementById('<%= txtNewScheduleTime.ClientID %>').value = `${hh}:${min}`;

            ensureModal().show();
        }

        function hideModal() {
            if (setScheduleBsModal) setScheduleBsModal.hide();
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
</asp:Content>
