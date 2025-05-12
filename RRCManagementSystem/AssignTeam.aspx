<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AssignTeam.aspx.cs" Inherits="RRCManagementSystem.AssignTeam" %>


<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .main-content {
            padding: 30px;
            background-color: #f8f9fa;
            min-height: calc(100vh - 100px);
        }

        .form-container {
            background-color: #ffffff;
            padding: 20px 25px;
            border-radius: 8px;
            box-shadow: 0 4px 10px rgba(0,0,0,0.1);
            max-width: 900px;
            margin: 0 auto;
        }

        .form-container h3 {
            color: #004085;
            margin-bottom: 20px;
        }

        .btn-save {
            background-color: #28a745;
            color: white;
            padding: 10px 20px;
            border: none;
            border-radius: 4px;
        }

        .btn-cancel {
            background-color: #6c757d;
            color: white;
            padding: 10px 20px;
            border: none;
            border-radius: 4px;
        }

        .gridview-table {
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 20px;
        }

        .gridview-table th, .gridview-table td {
            padding: 10px;
            border: 1px solid #dee2e6;
        }

        .gridview-table th {
            background-color: #004085;
            color: #ffffff;
        }

        .message-label {
            font-weight: 600;
            color: green;
        }
    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="main-content">
        <div class="form-container">
            <h3>Assign Team and Equipment for Booking #<asp:Label ID="lblBookingID" runat="server" /></h3>

            <asp:Label ID="lblMessage" runat="server" CssClass="message-label" />

            <!-- TEAM DROPDOWN -->
            <div class="mb-3">
                <label for="ddlTeams">Assign Team:</label>
                <asp:DropDownList ID="ddlTeams" runat="server" CssClass="form-control"></asp:DropDownList>
            </div>

            <!-- EQUIPMENT GRID -->
            <h5>Assign Equipment</h5>
            <asp:GridView ID="gvEquipments" runat="server" AutoGenerateColumns="False" DataKeyNames="EquipmentID">
    <Columns>
        <asp:TemplateField HeaderText="Select">
            <ItemTemplate>
                <asp:CheckBox ID="chkSelectEquipment" runat="server" />
            </ItemTemplate>
        </asp:TemplateField>
        <asp:BoundField DataField="EquipmentID" HeaderText="ID" />
        <asp:BoundField DataField="Name" HeaderText="Equipment Name" />
        <asp:BoundField DataField="Status" HeaderText="Status" />
    </Columns>
</asp:GridView>


            <!-- CHEMICAL GRID -->
            <h5>Assign Chemicals</h5>
            <asp:GridView ID="gvChemicals" runat="server" AutoGenerateColumns="False" CssClass="gridview-table">
                <Columns>
                    <asp:BoundField DataField="ItemID" HeaderText="Item ID" />
                    <asp:BoundField DataField="Name" HeaderText="Chemical Name" />
                    <asp:BoundField DataField="Quantity" HeaderText="Available Quantity" />
                    <asp:TemplateField HeaderText="Quantity to Assign">
                        <ItemTemplate>
                            <asp:TextBox ID="txtQuantityAssign" runat="server" CssClass="form-control" Text="0" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>

            <asp:Button ID="btnAssign" runat="server" CssClass="btn-save" Text="Save Assignment" OnClick="btnAssign_Click" />
            <asp:Button ID="btnCancel" runat="server" CssClass="btn-cancel" Text="Cancel" PostBackUrl="~/ADMIN/ApproveRejectBookings.aspx" />
        </div>
    </div>
</asp:Content>