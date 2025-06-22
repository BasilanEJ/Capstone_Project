<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ApproveRescheduleRequests.aspx.cs" Inherits="RRCManagementSystem.ApproveRescheduleRequests" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container py-5">
        <h3 class="text-center fw-bold text-primary mb-4">🔁 Approve Reschedule Requests</h3>

        <asp:HiddenField ID="hfRequestID" runat="server" />
        <asp:HiddenField ID="hfRejectReason" runat="server" />

        <asp:GridView ID="gvRescheduleRequests" runat="server" AutoGenerateColumns="False" CssClass="table table-bordered table-hover table-striped"
            AllowPaging="true" PageSize="10" ClientIDMode="Static"
            OnPageIndexChanging="gvRescheduleRequests_PageIndexChanging"
            OnRowCommand="gvRescheduleRequests_RowCommand"
            OnRowDataBound="gvRescheduleRequests_RowDataBound">
            <Columns>
                <asp:BoundField DataField="RequestID" HeaderText="Request ID" ReadOnly="true" />
                <asp:BoundField DataField="ClientName" HeaderText="Client" ReadOnly="true" />
                <asp:BoundField DataField="ServiceType" HeaderText="Service Type" ReadOnly="true" />
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
                            OnClientClick="return openRejectModal(this);" CommandName="Reject" CommandArgument='<%# Eval("RequestID") %>' UseSubmitBehavior="false" />
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
                inputValidator: (value) => {
                    if (!value) return 'Please enter a reason';
                }
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
