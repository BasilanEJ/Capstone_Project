<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ApproveRejectBookings.aspx.cs" Inherits="RRCManagementSystem.ApproveRejectBookings" %>


<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
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
        <h3>Approve or Reject Bookings</h3>

        <asp:GridView ID="gvBookings" runat="server" AutoGenerateColumns="False" CssClass="booking-table"
            DataKeyNames="BookingID" OnRowCommand="gvBookings_RowCommand">
            <Columns>
                <asp:BoundField DataField="BookingID" HeaderText="Booking ID" />
                <asp:BoundField DataField="ClientName" HeaderText="Client Name" />
                <asp:BoundField DataField="ServiceName" HeaderText="Service" />
                <asp:BoundField DataField="ScheduledDate" HeaderText="Scheduled Date" DataFormatString="{0:yyyy-MM-dd}" />

                <asp:TemplateField HeaderText="Start Time">
                    <ItemTemplate>
                        <%# Eval("StartTime") != DBNull.Value ? String.Format("{0:hh\\:mm}", Eval("StartTime")) : "—" %>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:BoundField DataField="Status" HeaderText="Status" />

                <asp:TemplateField HeaderText="SQM">
                    <ItemTemplate>
                        <%# Eval("SQM") != DBNull.Value ? Eval("SQM").ToString() : "0" %>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Price">
                    <ItemTemplate>
                        <%# Eval("Price") != DBNull.Value ? String.Format("₱{0:N2}", Eval("Price")) : "₱0.00" %>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Actions">
                    <ItemTemplate>
                        <asp:Button ID="btnApprove" runat="server" Text="Approve"
                            CommandName="Approve"
                            CommandArgument='<%# Eval("BookingID") %>'
                            CssClass="btn-success" />

                        <asp:Button ID="btnReject" runat="server" Text="Reject"
                            CommandName="Reject"
                            CommandArgument='<%# Eval("BookingID") %>'
                            CssClass="btn-danger" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>

        <asp:Label ID="lblMessage" runat="server" CssClass="message-label" />
    </div>
</asp:Content>