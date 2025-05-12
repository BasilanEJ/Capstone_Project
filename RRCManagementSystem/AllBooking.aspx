<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AllBooking.aspx.cs" Inherits="RRCManagementSystem.AllBooking" %>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .container {
            padding: 30px;
            background-color: #f8f9fa;
            min-height: calc(100vh - 100px);
        }

        h3 {
            margin-bottom: 20px;
            color: #004085;
        }

        .table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
        }

        .table th, .table td {
            padding: 12px;
            text-align: center;
            border: 1px solid #dee2e6;
        }

        .table th {
            background-color: #004085;
            color: white;
        }

        .table-striped tbody tr:nth-child(odd) {
            background-color: #f9f9f9;
        }

        .table-striped tbody tr:hover {
            background-color: #e9ecef;
        }

        .status-assigned {
            color: green;
            font-weight: bold;
        }

        .status-pending {
            color: orange;
            font-weight: bold;
        }

        .status-cancelled {
            color: red;
            font-weight: bold;
        }
    </style>

  <div class="container">


        <h3>All Bookings Overview</h3>

    <asp:GridView ID="gvBookings" runat="server" AutoGenerateColumns="False"
    CssClass="table table-striped"
    AllowPaging="True" PageSize="10"
    OnPageIndexChanging="gvBookings_PageIndexChanging"
    OnRowDataBound="gvBookings_RowDataBound"
    OnRowCommand="gvBookings_RowCommand">
    
    <Columns>
        <asp:BoundField DataField="BookingID" HeaderText="Booking ID" />
        <asp:BoundField DataField="ClientName" HeaderText="Client Name" />
        <asp:BoundField DataField="ServiceName" HeaderText="Service" />
        <asp:BoundField DataField="ScheduledDate" HeaderText="Scheduled Date" DataFormatString="{0:yyyy-MM-dd}" />
        <asp:BoundField DataField="StartTime" HeaderText="Start Time" />
          <asp:BoundField DataField="Price" HeaderText="Price" DataFormatString="₱ {0:N2}" HtmlEncode="false" />
        <asp:BoundField DataField="RemainingBalance" HeaderText="Remaining Balance" DataFormatString="₱ {0:N2}" HtmlEncode="false" />
        <asp:BoundField DataField="Status" HeaderText="Status" />
        <asp:BoundField DataField="CreatedAt" HeaderText="Date Booked" DataFormatString="{0:yyyy-MM-dd}" />
        

        <asp:TemplateField HeaderText="Actions">
            <ItemTemplate>
                <asp:Button ID="btnEdit" runat="server" Text="Edit" CommandName="EditBooking" CommandArgument='<%# Eval("BookingID") %>' CssClass="btn btn-primary btn-sm" />
            </ItemTemplate>
        </asp:TemplateField>

    </Columns>
</asp:GridView>


        <asp:Label ID="lblMessage" runat="server" ForeColor="Red" />


    <!--    <div class="section-divider"></div>

        <h3>Client Inquiries Overview</h3>

        <asp:GridView ID="gvInquiries" runat="server" AutoGenerateColumns="False" CssClass="table table-striped"
            AllowPaging="True" PageSize="10"
            OnPageIndexChanging="gvInquiries_PageIndexChanging">

            <Columns>
                <asp:BoundField DataField="InquiryID" HeaderText="Inquiry ID" />
                <asp:BoundField DataField="ClientName" HeaderText="Client Name" />
                <asp:BoundField DataField="Email" HeaderText="Email" />
                <asp:BoundField DataField="ContactNumber" HeaderText="Contact Number" />
                <asp:BoundField DataField="ServiceName" HeaderText="Service" />
                <asp:BoundField DataField="Message" HeaderText="Message" />
                <asp:BoundField DataField="SentAt" HeaderText="Date Sent" DataFormatString="{0:yyyy-MM-dd}" />
            </Columns>

        </asp:GridView>

        <asp:Label ID="lblMessageInquiry" runat="server" ForeColor="Red" />
        -->
    </div>

</asp:Content>