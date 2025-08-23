<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AllRescheduleBooking.aspx.cs" Inherits="RRCManagementSystem.AllRescheduleBooking" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <div class="container py-5">
        <h3 class="text-center text-primary fw-bold mb-4">🔁 All Rescheduled Operations</h3>

    
        <div class="card shadow-sm mb-3">
            <div class="card-body">
                <div class="row g-2 align-items-end">
                    <div class="col-md-4">
                        <label class="form-label">Search (Code / Client / Service)</label>
                        <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control"
                                     placeholder="e.g. TD0003or Juan Dela Cruz or Termite"></asp:TextBox>
                    </div>
                    <div class="col-md-3">
                        <label class="form-label">Status</label>
                        <asp:DropDownList ID="ddlFilterStatus" runat="server" CssClass="form-select">
                            <asp:ListItem Text="All" Value="" />
                            <asp:ListItem Text="Pending" Value="Pending" />
                            <asp:ListItem Text="Rejected" Value="Rejected" />
                            <asp:ListItem Text="Approved" Value="Approved" />
                            <asp:ListItem Text="In Progress" Value="InProgress" />
                            <asp:ListItem Text="Completed" Value="Completed" />
                            <asp:ListItem Text="Cancelled" Value="Cancelled" />
                        </asp:DropDownList>
                    </div>
                    <div class="col-md-2">
                        <label class="form-label">From</label>
                        <asp:TextBox ID="txtFrom" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                    </div>
                    <div class="col-md-2">
                        <label class="form-label">To</label>
                        <asp:TextBox ID="txtTo" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                    </div>
                    <div class="col-md-1 d-grid">
                        <asp:Button ID="btnFilter" runat="server" CssClass="btn btn-primary" Text="Filter" OnClick="btnFilter_Click" />
                    </div>
                </div>
                <div class="mt-2 text-end">
                    <asp:Button ID="btnReset" runat="server" CssClass="btn btn-outline-secondary btn-sm" Text="Reset" OnClick="btnReset_Click" />
                </div>
            </div>
        </div>

        <asp:GridView ID="gvReschedules" runat="server" AutoGenerateColumns="False"
                      CssClass="table table-bordered table-hover table-striped"
                      AllowPaging="True" PageSize="10"
                      OnPageIndexChanging="gvReschedules_PageIndexChanging"
                      OnRowCommand="gvReschedules_RowCommand"
                      OnRowDataBound="gvReschedules_RowDataBound">

            <Columns>
              
                <asp:BoundField DataField="BookingID" HeaderText="Booking ID" Visible="false" />
              
                <asp:BoundField DataField="BookingCode" HeaderText="Booking Code" />

                <asp:BoundField DataField="ClientName" HeaderText="Client Name" />
                <asp:BoundField DataField="ServiceName" HeaderText="Service" />
                <asp:BoundField DataField="OperationNumber" HeaderText="Operation #" />
                <asp:BoundField DataField="ScheduledDate" HeaderText="Scheduled Date" DataFormatString="{0:yyyy-MM-dd}" />
                <asp:TemplateField HeaderText="Status">
                    <ItemTemplate>
                        <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select">
                            <asp:ListItem Text="Pending" Value="Pending" />
                            <asp:ListItem Text="Rejected" Value="Rejected" />
                            <asp:ListItem Text="Approved" Value="Approved" />
                            <asp:ListItem Text="In Progress" Value="InProgress" />
                            <asp:ListItem Text="Completed" Value="Completed" />
                            <asp:ListItem Text="Cancelled" Value="Cancelled" />
                        </asp:DropDownList>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:Button ID="btnUpdate" runat="server" Text="Update" CommandName="UpdateStatus"
                                     CommandArgument='<%# Eval("ScheduleID") %>' CssClass="btn btn-sm btn-primary"
                                     OnClientClick="return confirmUpdate(this);" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>

        <asp:Label ID="lblMessage" runat="server" CssClass="text-success fw-semibold mt-3 d-block text-center" />
    </div>

    <script>
        function confirmUpdate(button) {
            event.preventDefault();
            Swal.fire({
                title: 'Update Schedule?',
                text: 'Are you sure you want to update this schedule status?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes, update it'
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack(button.name, '');
                }
            });
            return false;
        }
    </script>
</asp:Content>
