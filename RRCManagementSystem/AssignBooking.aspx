    <%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AssignBooking.aspx.cs" Inherits="RRCManagementSystem.AssignBooking" %>

   <asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
        <style>
            /* Main content inherits padding/margin from master */
            .assign-section {
                background-color: #ffffff;
                padding: 25px;
                border-radius: 10px;
                box-shadow: 0 2px 8px rgba(0,0,0,0.1);
                margin-bottom: 30px;
            }

            .assign-section h3 {
                color: #004085;
                margin-bottom: 15px;
            }

            .form-group {
                margin-bottom: 20px;
            }

            .form-group label {
                font-weight: bold;
                margin-bottom: 5px;
                display: block;
            }

            .form-group select, 
            .form-group input[type="text"], 
            .form-group input[type="number"] {
                width: 100%;
                padding: 10px;
                border: 1px solid #ced4da;
                border-radius: 5px;
            }

            .btn-primary {
                background-color: #004085;
                color: #ffffff;
                padding: 10px 20px;
                border: none;
                border-radius: 5px;
                font-size: 14px;
                cursor: pointer;
            }

            .btn-primary:hover {
                background-color: #002752;
            }

            .message-label {
                margin-top: 20px;
                font-weight: bold;
            }

            .gridview-container {
                margin-top: 10px;
            }

            .section-title {
                display: flex;
                align-items: center;
                font-size: 18px;
                color: #004085;
                margin-bottom: 15px;
            }

            .section-title i {
                margin-right: 10px;
            }

            /* GridView headers */
            .gridview-container table {
                width: 100%;
                border-collapse: collapse;
            }

            .gridview-container th {
                background-color: #004085;
                color: white;
                padding: 10px;
            }

            .gridview-container td {
                padding: 10px;
                border: 1px solid #dee2e6;
            }

            .gridview-container input[type="text"] {
                width: 60px;
                text-align: center;
            }
        </style>
    </asp:Content>

    <asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="assign-section">
        <h2><i class="fas fa-tasks"></i> Assign Team, Equipment, Chemicals & Safety Gear</h2>

        <div class="form-group">
            <label><i class="fas fa-users"></i> Select Team</label>
            <asp:DropDownList ID="ddlTeams" runat="server" CssClass="form-control" />
        </div>

        <div class="gridview-container">
            <div class="section-title"><i class="fas fa-tools"></i> Assign Equipment</div>
            <asp:GridView ID="gvEquipments" runat="server" AutoGenerateColumns="False" DataKeyNames="EquipmentID" CssClass="table">
                <Columns>
                    <asp:BoundField DataField="EquipmentID" HeaderText="Equipment ID" />
                    <asp:BoundField DataField="Name" HeaderText="Equipment Name" />
                    <asp:BoundField DataField="Status" HeaderText="Status" />
                    <asp:TemplateField HeaderText="Assign?">
                        <ItemTemplate>
                            <asp:CheckBox ID="chkAssignEquip" runat="server" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

        <div class="gridview-container">
            <asp:GridView ID="gvChemicals" runat="server" AutoGenerateColumns="False" DataKeyNames="ItemID" CssClass="table">
                <Columns>
                    <asp:BoundField DataField="ItemID" HeaderText="ID" />
                    <asp:BoundField DataField="Name" HeaderText="Chemical Name" />
                    <asp:TemplateField HeaderText="Bottles Available">
                        <ItemTemplate>
                            <asp:Label ID="lblQuantity" runat="server" Text='<%# Eval("Quantity") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Excess mL">
                        <ItemTemplate>
                            <asp:Label ID="lblExcessML" runat="server" Text='<%# Eval("ExcessML") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Bottle Buffer">
                        <ItemTemplate>
                            <asp:TextBox ID="txtBottleBuffer" runat="server" CssClass="form-control" Text="0" Width="80px" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Use This Chemical?">
                        <ItemTemplate>
                            <asp:CheckBox ID="chkUseChemical" runat="server" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

        <div class="gridview-container">
            <div class="section-title"><i class="fas fa-box"></i> Assign Sachet Pack Chemicals</div>
            <asp:GridView ID="gvSachetChemicals" runat="server" AutoGenerateColumns="False" DataKeyNames="ItemID" CssClass="table">
                <Columns>
                    <asp:BoundField DataField="ItemID" HeaderText="ID" />
                    <asp:BoundField DataField="Name" HeaderText="Sachet Name" />
                    <asp:TemplateField HeaderText="Packs Available">
                        <ItemTemplate>
                            <asp:Label ID="lblSachetQuantity" runat="server" Text='<%# Eval("Quantity") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Assign Quantity">
                        <ItemTemplate>
                            <asp:TextBox ID="txtAssignSachet" runat="server" Text="0" Width="60px"
                                onkeypress="return isNumberKey(event);" onblur="setZeroIfEmpty(this);" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

        <div class="gridview-container">
            <div class="section-title"><i class="fas fa-hard-hat"></i> Assign Safety Gear</div>
            <asp:GridView ID="gvSafetyGears" runat="server" AutoGenerateColumns="False" DataKeyNames="ItemID" CssClass="table">
                <Columns>
                    <asp:BoundField DataField="ItemID" HeaderText="Gear ID" />
                    <asp:BoundField DataField="Name" HeaderText="Gear Name" />
                    <asp:BoundField DataField="Quantity" HeaderText="Available Quantity" />
                    <asp:TemplateField HeaderText="Assign Quantity">
                        <ItemTemplate>
                            <asp:TextBox ID="txtGearQuantityAssign" runat="server" Text="0" CssClass="form-control" Width="60px" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

        <div style="margin-top: 20px;">
            <asp:Button ID="btnAssignAll" runat="server" Text="Assign Booking" CssClass="btn-primary" OnClick="btnAssignAll_Click" />
        </div>

        <asp:Label ID="lblMessage" runat="server" CssClass="message-label" />
    </div>

    <script type="text/javascript">
        function isNumberKey(evt) {
            var charCode = (evt.which) ? evt.which : evt.keyCode;
            if (charCode != 8 && charCode != 46 && (charCode < 48 || charCode > 57)) return false;
            return true;
        }

        function setZeroIfEmpty(input) {
            if (input.value.trim() === '') input.value = '0';
        }
    </script>
</asp:Content>