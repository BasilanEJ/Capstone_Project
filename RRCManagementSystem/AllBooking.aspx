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

        .filters {
            margin-bottom: 20px;
        }

        .filters input, .filters select {
            padding: 6px;
            margin-right: 10px;
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

        .btn {
            padding: 4px 8px;
            font-size: 12px;
            border-radius: 4px;
        }

        .btn-complete {
            background-color: #28a745;
            color: white;
            border: none;
        }

        .btn-edit {
            background-color: #007bff;
            color: white;
            border: none;
        }
    </style>

    <div class="container">
        <h3>All Bookings Overview</h3>

        <div class="filters">
            <asp:TextBox ID="txtSearch" runat="server" Placeholder="Search by Client or Service" AutoPostBack="true" OnTextChanged="txtSearch_TextChanged" />
            <asp:DropDownList ID="ddlStatusFilter" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlStatusFilter_SelectedIndexChanged">
                <asp:ListItem Text="All Statuses" Value="" />
                <asp:ListItem Text="Pending" Value="Pending" />
                <asp:ListItem Text="Ongoing" Value="Ongoing" />
                <asp:ListItem Text="Completed" Value="Completed" />
                <asp:ListItem Text="Cancelled" Value="Cancelled" />
            </asp:DropDownList>
        </div>

      <asp:GridView ID="gvBookings" runat="server" AutoGenerateColumns="False"
              CssClass="table table-striped"
              DataKeyNames="BookingID"
              AllowPaging="True" PageSize="10"
              OnPageIndexChanging="gvBookings_PageIndexChanging"
              OnRowCommand="gvBookings_RowCommand"
              OnRowDataBound="gvBookings_RowDataBound">

            <Columns>
                <asp:BoundField DataField="BookingID" HeaderText="Booking ID" />
                <asp:BoundField DataField="ClientName" HeaderText="Client Name" />
                <asp:BoundField DataField="ServiceName" HeaderText="Service" />
                <asp:BoundField DataField="ScheduledDate" HeaderText="Scheduled Date" DataFormatString="{0:yyyy-MM-dd}" />
                <asp:BoundField DataField="StartTime" HeaderText="Start Time" />
                <asp:BoundField DataField="Price" HeaderText="Price" DataFormatString="₱ {0:N2}" HtmlEncode="false" />
                <asp:BoundField DataField="RemainingBalance" HeaderText="Remaining Balance" DataFormatString="₱ {0:N2}" HtmlEncode="false" />
                <asp:BoundField DataField="Status" HeaderText="Booking Status" />
                <asp:BoundField DataField="CreatedAt" HeaderText="Date Booked" DataFormatString="{0:yyyy-MM-dd}" />

                <asp:TemplateField HeaderText="Op1 Status">
                    <ItemTemplate>
                        <%# Eval("Status").ToString() == "Assigned" && Eval("Op1Status") != null
                              ? Eval("Op1Status").ToString()
                              : "—" %>
                    </ItemTemplate>
                </asp:TemplateField>


             <asp:TemplateField HeaderText="Actions">
    <ItemTemplate>
        <!-- Edit button (no change) -->
        <asp:Button ID="btnEdit" runat="server" Text="Edit" CommandName="EditBooking" 
                    CommandArgument='<%# Eval("BookingID") %>' CssClass="btn btn-edit btn-sm" />

        <!-- Visible button for SweetAlert -->
        <asp:Button ID="btnTriggerCompleteOp1" runat="server"
                    Text="Mark Op1 Complete"
                    CssClass="btn btn-complete btn-sm"
                    OnClientClick='<%# "return confirmCompleteOp1(" + Eval("BookingID") + ");" %>'
                    UseSubmitBehavior="false"
                    Visible='<%# Eval("Status").ToString() == "Assigned" && Eval("Op1Status") != null && Eval("Op1Status").ToString() != "Completed" %>' />
    </ItemTemplate>
</asp:TemplateField>

            </Columns>
        </asp:GridView>

        <!-- Hidden field to store BookingID for Op1 completion -->
<asp:HiddenField ID="hfBookingIDToComplete" runat="server" />

<!-- Hidden button to trigger server-side logic -->
<asp:Button ID="btnHiddenCompleteOp1" runat="server"
            Style="display:none;"
            OnClick="btnHiddenCompleteOp1_Click"
            UseSubmitBehavior="false" />

        <asp:Label ID="lblMessage" runat="server" ForeColor="Red" />
    </div>

  <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
<script type="text/javascript">
    function confirmCompleteOp1(bookingId) {
        Swal.fire({
            title: 'Mark Operation 1 as Complete?',
            text: 'This action cannot be undone.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Yes, mark it!',
            cancelButtonText: 'Cancel'  
        }).then((result) => {
            if (result.isConfirmed) {
                document.getElementById('<%= hfBookingIDToComplete.ClientID %>').value = bookingId;
                document.getElementById('<%= btnHiddenCompleteOp1.ClientID %>').click();
            }
        });

        return false; // Prevent default postback
    }
</script>


</asp:Content>
