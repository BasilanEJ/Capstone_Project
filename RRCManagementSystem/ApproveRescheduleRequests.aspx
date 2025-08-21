<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ApproveRescheduleRequests.aspx.cs" Inherits="RRCManagementSystem.ApproveRescheduleRequests" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container py-5">
        <h3 class="text-center fw-bold text-primary mb-4">🔁 Approve Reschedule Requests</h3>

        <!-- 🔎 Filters -->
        <div class="card shadow-sm mb-3">
            <div class="card-body">
                <div class="row g-2 align-items-end">
                    <div class="col-md-4">
                        <label class="form-label">Search (Client / Service)</label>
                        <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control" placeholder="Type a name or service..." />
                    </div>
                    <div class="col-md-2">
                        <label class="form-label">From (Requested)</label>
                        <asp:TextBox ID="txtFrom" runat="server" TextMode="Date" CssClass="form-control" />
                    </div>
                    <div class="col-md-2">
                        <label class="form-label">To (Requested)</label>
                        <asp:TextBox ID="txtTo" runat="server" TextMode="Date" CssClass="form-control" />
                    </div>
                    <div class="col-md-2">
                        <label class="form-label">Status</label>
                        <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select">
                            <asp:ListItem Text="All" Value="All" />
                            <asp:ListItem Text="Pending" Value="Pending" Selected="True" />
                            <asp:ListItem Text="Approved" Value="Approved" />
                            <asp:ListItem Text="Rejected" Value="Rejected" />
                        </asp:DropDownList>
                    </div>
                    <div class="col-md-2 d-grid">
                        <asp:Button ID="btnApply" runat="server" CssClass="btn btn-primary" Text="Apply"
                            OnClick="btnApply_Click" UseSubmitBehavior="false" />
                        <asp:Button ID="btnClear" runat="server" CssClass="btn btn-outline-secondary mt-2" Text="Clear"
                            OnClick="btnClear_Click" UseSubmitBehavior="false" />
                    </div>
                </div>
            </div>
        </div>

        <asp:HiddenField ID="hfRequestID" runat="server" />
        <asp:HiddenField ID="hfRejectReason" runat="server" />

        <asp:GridView ID="gvRescheduleRequests" runat="server" AutoGenerateColumns="False"
            CssClass="table table-bordered table-hover table-striped"
            AllowPaging="true" PageSize="10" ClientIDMode="Static"
            OnPageIndexChanging="gvRescheduleRequests_PageIndexChanging"
            OnRowCommand="gvRescheduleRequests_RowCommand"
            OnRowDataBound="gvRescheduleRequests_RowDataBound">
            <Columns>
                <asp:BoundField DataField="RequestID" HeaderText="Request ID" ReadOnly="true" />
                <asp:BoundField DataField="ClientName" HeaderText="Client" ReadOnly="true" />
                <asp:BoundField DataField="ServiceNames" HeaderText="Service" ReadOnly="true" />
                <asp:BoundField DataField="ScheduledDate" HeaderText="Original Date" DataFormatString="{0:yyyy-MM-dd}" ReadOnly="true" />
                <asp:BoundField DataField="OperationNumber" HeaderText="Operation #" ReadOnly="true" />
                <asp:BoundField DataField="RequestedDate" HeaderText="Requested On" DataFormatString="{0:yyyy-MM-dd}" ReadOnly="true" />
                <asp:BoundField DataField="Status" HeaderText="Status" ReadOnly="true" />
                <asp:TemplateField HeaderText="New Schedule Date">
                    <ItemTemplate>
                        <asp:Label ID="lblNewDate" runat="server" CssClass="form-control bg-light border-0" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Action">
                    <ItemTemplate>
                        <asp:Button ID="btnApprove" runat="server" Text="Approve" CssClass="btn btn-success btn-sm me-2"
                            CommandName="Approve" CommandArgument='<%# Eval("RequestID") %>' UseSubmitBehavior="false" />
                        <asp:Button ID="btnReject" runat="server" Text="Reject" CssClass="btn btn-danger btn-sm"
                            OnClientClick="return openRejectModal(this);" CommandName="Reject"
                            CommandArgument='<%# Eval("RequestID") %>' UseSubmitBehavior="false" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>

        <asp:Label ID="lblMessage" runat="server" CssClass="text-center d-block mt-3 fw-semibold text-success" />
    </div>

    <script type="text/javascript">
        function openRejectModal(button) {
            const requestId = button.getAttribute("commandargument") || button.value;
            Swal.fire({
                title: 'Reject Reschedule Request',
                input: 'text',
                inputLabel: 'Enter rejection reason',
                inputPlaceholder: 'Rejection reason...',
                showCancelButton: true,
                confirmButtonText: 'Reject',
                cancelButtonText: 'Cancel',
                inputValidator: (value) => { if (!value) return 'Please enter a reason'; }
            }).then((result) => {
                if (result.isConfirmed) {
                    document.getElementById('<%= hfRequestID.ClientID %>').value = requestId;
                    document.getElementById('<%= hfRejectReason.ClientID %>').value = result.value;
                    __doPostBack(button.name, '');
                }
            });
            return false;
        }
    </script>
</asp:Content>
