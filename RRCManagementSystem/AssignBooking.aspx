<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AssignBooking.aspx.cs" Inherits="RRCManagementSystem.AssignBooking" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    </asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto py-8 px-4">
        <div class="bg-white rounded-xl shadow-lg overflow-hidden border border-gray-200">
            <div class="bg-blue-800 text-white font-bold p-5 flex items-center justify-between">
                <span>Booking Code</span>
                <span class="bg-white text-blue-800 font-bold py-1 px-3 rounded-full">
                    <asp:Label ID="lblBookingCode" runat="server" />
                </span>
            </div>

            <div class="p-6">
                <h2 class="text-2xl font-bold text-blue-800 mb-6 flex items-center">
                    <i class="fas fa-tasks mr-3"></i> Assign Team, Equipment, Chemicals & Safety Gear
                </h2>

                <div class="mb-6">
                    <label class="block text-gray-700 font-semibold mb-2">Select Team</label>
                    <asp:DropDownList ID="ddlTeams" runat="server" CssClass="block w-full px-4 py-2 text-gray-700 bg-white border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500" />
                </div>

                <div class="mt-8">
                    <h3 class="text-xl font-bold text-blue-800 mb-4 flex items-center">
                        <i class="fas fa-tools mr-3"></i> Assign Equipment
                    </h3>
                    <div class="overflow-x-auto rounded-lg border border-gray-200 shadow-sm">
                        <asp:GridView ID="gvEquipments" runat="server" AutoGenerateColumns="False" DataKeyNames="EquipmentID"
                            CssClass="min-w-full divide-y divide-gray-200">
                            <HeaderStyle CssClass="bg-gray-50" />
                            <RowStyle CssClass="bg-white even:bg-gray-50 hover:bg-gray-100 transition-colors" />
                            <Columns>
                                <asp:BoundField DataField="EquipmentID" HeaderText="ID" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-900" HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" />
                                <asp:BoundField DataField="Name" HeaderText="Name" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-900" HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" />
                                <asp:BoundField DataField="Status" HeaderText="Status" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-900" HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" />
                                <asp:TemplateField HeaderText="Assign?" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500" HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    <ItemTemplate>
                                        <asp:CheckBox ID="chkAssignEquip" runat="server" CssClass="h-4 w-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
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
                confirmButtonColor: '#2563eb', // Tailwind's blue-600
                cancelButtonColor: '#dc2626', // Tailwind's red-600
                confirmButtonText: 'Yes, assign it!'
            }).then((result) => {
                if (result.isConfirmed) {
                    setTimeout(function () {
                        // Set confirmation flag
                        document.getElementById('<%= hfConfirmAssign.ClientID %>').value = "true";
                        // Safely trigger the hidden ASP.NET button
                        document.getElementById('<%= btnHiddenAssign.ClientID %>').click();
                    }, 200);
                }
            });
            return false; // Prevent default postback from visible button
        }
    </script>
    <script type="text/javascript">
        // Check if the status is 'Assigned' in the URL
        const urlParams = new URLSearchParams(window.location.search);
        const status = urlParams.get('status');
        if (status === 'Assigned') {
            Swal.fire({
                icon: 'success',
                title: 'Assigned!',
                text: 'The booking was successfully assigned.',
                showConfirmButton: false,
                timer: 2000
            }).then(() => {
                window.location.href = 'AllBooking.aspx';
            });
        }
    </script>
</asp:Content>