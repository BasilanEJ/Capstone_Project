<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AllRescheduleBooking.aspx.cs" Inherits="RRCManagementSystem.AllRescheduleBooking" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .container {
            padding: 30px;
            background-color: #f8f9fa;
        }

        .table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
        }

        .table th, .table td {
            padding: 12px;
            border: 1px solid #dee2e6;
            text-align: center;
        }

        .table th {
            background-color: #004085;
            color: white;
        }

        .btn {
            padding: 6px 12px;
            font-size: 14px;
        }
    </style>

    <div class="container">
        <h3>All Rescheduled Operations</h3>
       <asp:GridView ID="gvReschedules" runat="server" AutoGenerateColumns="False" CssClass="table"
    OnRowCommand="gvReschedules_RowCommand" OnRowDataBound="gvReschedules_RowDataBound">

            <Columns>
                <asp:BoundField DataField="BookingID" HeaderText="Booking ID" />
                <asp:BoundField DataField="ClientName" HeaderText="Client Name" />
                <asp:BoundField DataField="ServiceName" HeaderText="Service" />
                <asp:BoundField DataField="OperationNumber" HeaderText="Operation #" />
                <asp:BoundField DataField="ScheduledDate" HeaderText="Scheduled Date" DataFormatString="{0:yyyy-MM-dd}" />
                <asp:TemplateField HeaderText="Status">
                    <ItemTemplate>
                        <asp:DropDownList ID="ddlStatus" runat="server">
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
                       <asp:Button 
    ID="btnUpdate" 
    runat="server" 
    Text="Update" 
    CommandName="UpdateStatus"
    CommandArgument='<%# Eval("ScheduleID") %>' 
    CssClass="btn btn-primary" 
    OnClientClick="return confirm('Are you sure you want to update this schedule?');" />

                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>

        <asp:Label ID="lblMessage" runat="server" ForeColor="Green" />
    </div>
</asp:Content>
