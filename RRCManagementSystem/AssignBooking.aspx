<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AssignBooking.aspx.cs" Inherits="RRCManagementSystem.AssignBooking" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto py-8 px-4">
        <div class="bg-white rounded-xl shadow-lg overflow-hidden border border-gray-200">
           <div class="bg-blue-800 text-white font-bold p-5 flex flex-col sm:flex-row sm:items-center sm:justify-between">
    <div>
        <span>Booking Code / Operation #</span><br />
        <asp:Label ID="lblBookingCode" runat="server" CssClass="text-white text-lg font-semibold" />
        <asp:Label ID="lblOperationNumber" runat="server" CssClass="ml-2 text-gray-200 font-normal" Visible="false" />
        <br />
        <asp:Label ID="lblServiceName" runat="server" CssClass="text-yellow-200 text-sm font-medium mt-1 block" />
    </div>
</div>


            <div class="p-6">
                <h2 class="text-2xl font-bold text-blue-800 mb-6 flex items-center">
                    <i class="fas fa-tasks mr-3"></i> Assign Team, Equipment, Chemicals & Safety Gear
                </h2>

                <div class="mb-6">
                    <label for="ddlTeams" class="block text-gray-700 font-semibold mb-2">Select Team</label>
                    <asp:DropDownList 
                        ID="ddlTeams" 
                        runat="server" 
                        AutoPostBack="true" 
                        OnSelectedIndexChanged="ddlTeams_SelectedIndexChanged"
                        CssClass="block w-full px-4 py-2 text-gray-700 bg-white border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500">
                    </asp:DropDownList>
                </div>

                <div class="mt-4">
                    <h3 class="text-lg font-semibold text-gray-700 mb-2">Team's Bookings</h3>
                    <asp:Panel 
                        ID="pnlTeamBookings" 
                        runat="server" 
                        CssClass="p-4 bg-gray-50 border border-gray-200 rounded-lg">
                        <asp:Label 
                            ID="lblTeamBookings" 
                            runat="server" 
                            Text="Select a team to view their bookings." 
                            CssClass="text-gray-600" />
                    </asp:Panel>
                </div>

                <div class="mt-8">
                    <h3 class="text-xl font-bold text-blue-800 mb-4 flex items-center">
                        <i class="fas fa-tools mr-3"></i> Assign Equipment
                    </h3>
                    
                    <asp:Panel ID="pnlNoEquipment" runat="server" Visible="false" 
                        CssClass="p-6 bg-yellow-50 border border-yellow-200 rounded-lg">
                        <div class="flex items-center">
                            <i class="fas fa-exclamation-triangle text-yellow-600 text-2xl mr-3"></i>
                            <div>
                                <h4 class="text-yellow-800 font-semibold">No Equipment Available</h4>
                                <p class="text-yellow-700 text-sm">
                                    All equipment is fully booked for the scheduled date. 
                                    Please select a different date or contact the administrator.
                                </p>
                            </div>
                        </div>
                    </asp:Panel>
                    
                    <asp:Panel ID="pnlEquipmentGrid" runat="server" Visible="true">
                        <div class="overflow-x-auto rounded-lg border border-gray-200 shadow-sm">
                            <asp:GridView ID="gvEquipments" runat="server" AutoGenerateColumns="False" DataKeyNames="EquipmentID"
                                CssClass="min-w-full divide-y divide-gray-200">
                                <HeaderStyle CssClass="bg-gray-50" />
                                <RowStyle CssClass="bg-white even:bg-gray-50 hover:bg-gray-100 transition-colors" />
                                <Columns>
                                    <asp:BoundField DataField="EquipmentID" HeaderText="ID" 
                                        ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-900" 
                                        HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" />
                                    <asp:BoundField DataField="Name" HeaderText="Name" 
                                        ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-900" 
                                        HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" />
                                    <asp:BoundField DataField="Status" HeaderText="Status" 
                                        ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-900" 
                                        HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" />
                                    <asp:TemplateField HeaderText="Assign?" 
                                        ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500" 
                                        HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                        <ItemTemplate>
                                            <asp:CheckBox ID="chkAssignEquip" runat="server" 
                                                CssClass="h-4 w-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </div>
                    </asp:Panel>
                </div>

                <div class="mt-8">
                    <h3 class="text-xl font-bold text-blue-800 mb-4 flex items-center">
                        <i class="fas fa-vial mr-3"></i> Assign Bottled Chemicals
                    </h3>
                    <div class="overflow-x-auto rounded-lg border border-gray-200 shadow-sm">
                        <asp:GridView ID="gvChemicals" runat="server" AutoGenerateColumns="False" DataKeyNames="ItemID"
                            CssClass="min-w-full divide-y divide-gray-200">
                            <HeaderStyle CssClass="bg-gray-50" />
                            <RowStyle CssClass="bg-white even:bg-gray-50 hover:bg-gray-100 transition-colors" />
                            <Columns>
                                <asp:BoundField DataField="ItemID" HeaderText="ID" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-900" HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" />
                                <asp:BoundField DataField="Name" HeaderText="Chemical Name" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-900" HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" />
                                <asp:TemplateField HeaderText="Bottles Available" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-900" HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    <ItemTemplate><asp:Label ID="lblQuantity" runat="server" Text='<%# Eval("Quantity") %>' /></ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Excess mL" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-900" HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    <ItemTemplate><asp:Label ID="lblExcessML" runat="server" Text='<%# Eval("ExcessML") %>' /></ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Bottle Buffer" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-900" HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    <ItemTemplate><asp:TextBox ID="txtBottleBuffer" runat="server" CssClass="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500" Text="0" /></ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Use This Chemical?" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500" HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    <ItemTemplate><asp:CheckBox ID="chkUseChemical" runat="server" CssClass="h-4 w-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500" /></ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>

                <div class="mt-8">
                    <h3 class="text-xl font-bold text-blue-800 mb-4 flex items-center">
                        <i class="fas fa-box mr-3"></i> Assign Sachet Pack Chemicals
                    </h3>
                    <div class="overflow-x-auto rounded-lg border border-gray-200 shadow-sm">
                        <asp:GridView ID="gvSachetChemicals" runat="server" AutoGenerateColumns="False" DataKeyNames="ItemID"
                            CssClass="min-w-full divide-y divide-gray-200">
                            <HeaderStyle CssClass="bg-gray-50" />
                            <RowStyle CssClass="bg-white even:bg-gray-50 hover:bg-gray-100 transition-colors" />
                            <Columns>
                                <asp:BoundField DataField="ItemID" HeaderText="ID" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-900" HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" />
                                <asp:BoundField DataField="Name" HeaderText="Sachet Name" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-900" HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" />
                                <asp:TemplateField HeaderText="Packs Available" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-900" HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    <ItemTemplate><asp:Label ID="lblSachetQuantity" runat="server" Text='<%# Eval("Quantity") %>' /></ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Assign Quantity" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-900" HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    <ItemTemplate>
                                        <asp:TextBox ID="txtAssignSachet" runat="server" Text="0" CssClass="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500" onkeypress="return isNumberKey(event);" onblur="setZeroIfEmpty(this);" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>

                <div class="mt-8">
                    <h3 class="text-xl font-bold text-blue-800 mb-4 flex items-center">
                        <i class="fas fa-hard-hat mr-3"></i> Assign Safety Gear
                    </h3>
                    <div class="overflow-x-auto rounded-lg border border-gray-200 shadow-sm">
                        <asp:GridView ID="gvSafetyGears" runat="server" AutoGenerateColumns="False" DataKeyNames="ItemID"
                            CssClass="min-w-full divide-y divide-gray-200">
                            <HeaderStyle CssClass="bg-gray-50" />
                            <RowStyle CssClass="bg-white even:bg-gray-50 hover:bg-gray-100 transition-colors" />
                            <Columns>
                                <asp:BoundField DataField="ItemID" HeaderText="Gear ID" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-900" HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" />
                                <asp:BoundField DataField="Name" HeaderText="Gear Name" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-900" HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" />
                                <asp:TemplateField HeaderText="Available Quantity" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-900" HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    <ItemTemplate><asp:Label ID="lblSafetyQuantity" runat="server" Text='<%# Eval("Quantity") %>' /></ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Assign Quantity" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-900" HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    <ItemTemplate><asp:TextBox ID="txtAssignSafety" runat="server" Text="0" CssClass="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500" /></ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>

                <div class="mt-8 flex justify-end items-center space-x-4">
                    <asp:Button ID="btnCancel" runat="server"
                        Text="Cancel"
                        CssClass="py-2 px-6 bg-gray-500 text-white font-semibold rounded-md shadow-md hover:bg-gray-600 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-400 transition-colors"
                        OnClientClick="return confirmCancel();"
                        UseSubmitBehavior="false" />
                    
                    <asp:Button ID="btnAssignAll" runat="server"
                        Text="Assign Booking"
                        CssClass="py-2 px-6 bg-blue-600 text-white font-semibold rounded-md shadow-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors"
                        OnClientClick="return confirmAssign();"
                        UseSubmitBehavior="false" />
                </div>
                
                <asp:HiddenField ID="hfConfirmAssign" runat="server" Value="false" />
                <asp:Button ID="btnHiddenAssign" runat="server" Style="display:none;" OnClick="btnAssignAll_Click" UseSubmitBehavior="false" />
                
                <asp:Label ID="lblMessage" runat="server"
                    Visible="false"
                    CssClass="block mt-6 p-4 rounded-lg font-semibold text-center border-l-4 border-blue-500 text-blue-800 bg-blue-100" />
            </div>
        </div>
    </div>

    <script>
        function isNumberKey(evt) {
            var charCode = (evt.which) ? evt.which : evt.keyCode;
            return !(charCode != 8 && charCode != 46 && (charCode < 48 || charCode > 57));
        }

        function setZeroIfEmpty(input) {
            if (input.value.trim() === '') input.value = '0';
        }
    </script>

    <script type="text/javascript">
        function confirmAssign() {
            Swal.fire({
                title: 'Assign Booking?',
                text: "This will assign the team, chemicals, and safety gear.",
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#2563eb',
                cancelButtonColor: '#dc2626',
                confirmButtonText: 'Yes, assign it!'
            }).then((result) => {
                if (result.isConfirmed) {
                    setTimeout(function () {
                        document.getElementById('<%= hfConfirmAssign.ClientID %>').value = "true";
                        document.getElementById('<%= btnHiddenAssign.ClientID %>').click();
                    }, 200);
                }
            });
            return false;
        }

        function confirmCancel() {
            Swal.fire({
                title: 'Cancel Assignment?',
                text: "No changes will be saved. You'll return to the approval page.",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#6b7280',
                cancelButtonColor: '#2563eb',
                confirmButtonText: 'Yes, cancel',
                cancelButtonText: 'Stay here'
            }).then((result) => {
                if (result.isConfirmed) {
                    window.location.href = 'ApproveRejectBookings.aspx';
                }
            });
            return false;
        }
    </script>

    <script type="text/javascript">
        const urlParams = new URLSearchParams(window.location.search);
        const status = urlParams.get('status');
        if (status === 'Assigned') {
            Swal.fire({
                icon: 'success',
                title: 'Assigned!',
                text: 'The booking was successfully assigned.',
                showConfirmButton: false,
                timer: 3000
            }).then(() => {
                window.location.href = 'AllBooking.aspx';
            });
        }
    </script>
</asp:Content>