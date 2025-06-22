<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="BookingHistory.aspx.cs" Inherits="RRCManagementSystem.BookingHistory" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container py-4">
        <h2 class="text-center text-primary fw-bold mb-4">Booking History</h2>

        <div class="bg-light rounded shadow p-4 mb-4">
            <div class="row g-3 align-items-end">
                <div class="col-md-3">
                    <label class="form-label">Start Date:</label>
                    <asp:TextBox ID="txtStartDate" runat="server" TextMode="Date" CssClass="form-control" />
                </div>
                <div class="col-md-3">
                    <label class="form-label">End Date:</label>
                    <asp:TextBox ID="txtEndDate" runat="server" TextMode="Date" CssClass="form-control" />
                </div>
                <div class="col-md-3">
                    <label class="form-label">Status:</label>
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select">
                        <asp:ListItem Text="All" Value="" />
                        <asp:ListItem Text="Pending" Value="Pending" />
                        <asp:ListItem Text="Approved" Value="Approved" />
                        <asp:ListItem Text="Assigned" Value="Assigned" />
                        <asp:ListItem Text="Completed" Value="Completed" />
                        <asp:ListItem Text="Cancelled" Value="Cancelled" />
                        <asp:ListItem Text="Rejected" Value="Rejected" />
                    </asp:DropDownList>
                </div>
                <div class="col-md-3 text-end">
                    <asp:Button ID="btnFilter" runat="server" Text="Filter" CssClass="btn btn-primary w-100" OnClick="btnFilter_Click" />
                </div>
            </div>
        </div>

        <div class="table-responsive shadow rounded">
            <asp:GridView ID="gvBookingHistory" runat="server" AutoGenerateColumns="False" CssClass="table table-bordered table-hover" AllowPaging="true" PageSize="10" OnPageIndexChanging="gvBookingHistory_PageIndexChanging">
                <Columns>
                    <asp:BoundField DataField="BookingID" HeaderText="Booking ID" />
                    <asp:BoundField DataField="ClientName" HeaderText="Client Name" />
                    <asp:BoundField DataField="ServiceNames" HeaderText="Services" />
                    <asp:BoundField DataField="ScheduledDate" HeaderText="Scheduled Date" DataFormatString="{0:yyyy-MM-dd}" />
                    <asp:BoundField DataField="StartTime" HeaderText="Start Time" />
                    <asp:BoundField DataField="Status" HeaderText="Status" />
                    <asp:BoundField DataField="Price" HeaderText="Price (₱)" DataFormatString="{0:N2}" />
                </Columns>
            </asp:GridView>
        </div>

        <asp:Label ID="lblMessage" runat="server" CssClass="d-block text-center text-danger fw-semibold mt-3" />
    </div>
</asp:Content>
