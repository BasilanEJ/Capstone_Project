<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ApproveRescheduleRequests.aspx.cs" Inherits="RRCManagementSystem.ApproveRescheduleRequests" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <style>
        .main-content {
            padding: 30px;
            background-color: #f8f9fa;
            min-height: calc(100vh - 100px);
        }

        .booking-table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
            font-size: 14px;
        }

        .booking-table th, .booking-table td {
            padding: 12px 15px;
            border: 1px solid #dee2e6;
            text-align: left;
        }

        .booking-table th {
            background-color: #004085;
            color: #ffffff;
            font-weight: 600;
        }

        .btn-success {
            background-color: #28a745;
            color: #fff;
            border: none;
            padding: 8px 12px;
            cursor: pointer;
        }

        .btn-danger {
            background-color: #dc3545;
            color: #fff;
            border: none;
            padding: 8px 12px;
            cursor: pointer;
        }

        .message-label {
            display: block;
            margin-top: 20px;
            font-weight: 600;
            color: #333;
        }
    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="main-content">
        <h3>Approve Reschedule Requests</h3>

        <asp:HiddenField ID="hfRequestID" runat="server" />
        <asp:HiddenField ID="hfRejectReason" runat="server" />

        <asp:GridView ID="gvRescheduleRequests" runat="server" AutoGenerateColumns="False" CssClass="booking-table"
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
                        <asp:Label ID="lblNewDate" runat="server" CssClass="form-control" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Action">
                    <ItemTemplate>
                        <asp:Button ID="btnApprove" runat="server" Text="Approve" CssClass="btn-success"
                            CommandName="Approve" CommandArgument='<%# Eval("RequestID") %>' UseSubmitBehavior="false" />

                        <asp:Button ID="btnReject" runat="server" Text="Reject" CssClass="btn-danger"
                            OnClientClick="return openRejectModal(this);" CommandName="Reject" CommandArgument='<%# Eval("RequestID") %>' UseSubmitBehavior="false" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>

        <asp:Label ID="lblMessage" runat="server" CssClass="message-label" />
    </div>

    <script type="text/javascript">
        function openRejectModal(button) {
            var requestId = button.getAttribute("data-commandargument") || button.getAttribute("value") || button.value;
            if (!requestId) {
                requestId = button.name.split("$")[button.name.split("$").length - 1];
            }

            Swal.fire({
                title: 'Reject Reschedule Request',
                input: 'text',
                inputLabel: 'Enter rejection reason',
                inputPlaceholder: 'Rejection reason...',
                inputAttributes: {
                    'aria-label': 'Rejection reason'
                },
                showCancelButton: true,
                confirmButtonText: 'Reject',
                cancelButtonText: 'Cancel',
                preConfirm: (reason) => {
                    if (!reason) {
                        Swal.showValidationMessage('Please enter a reason');
                    }
                    return reason;
                }
            }).then((result) => {
                if (result.isConfirmed) {
                    document.getElementById('<%= hfRequestID.ClientID %>').value = button.getAttribute("value") || button.getAttribute("commandargument");
                    document.getElementById('<%= hfRejectReason.ClientID %>').value = result.value;
                    __doPostBack(button.name, '');
                }
            });

            return false; // prevent default postback
        }
    </script>
</asp:Content>
