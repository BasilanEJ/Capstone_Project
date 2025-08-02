<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AssignBooking.aspx.cs" Inherits="RRCManagementSystem.AssignBooking" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container py-4">
        <div class="card shadow mb-4">
            <div class="card-header bg-primary text-white fw-bold">
                <i class="fas fa-tasks me-2"></i> Assign Team, Equipment, Chemicals & Safety Gear
            </div>
            <div class="card-body">
                <asp:ScriptManager ID="ScriptManager1" runat="server" />

                <div class="mb-3">
                    <label class="form-label fw-semibold">Select Team</label>
                    <asp:DropDownList ID="ddlTeams" runat="server" CssClass="form-select" />
                </div>

                <div class="mt-4">
                    <h5 class="text-primary"><i class="fas fa-tools me-2"></i> Assign Equipment</h5>
                    <div class="table-responsive">
                        <asp:GridView ID="gvEquipments" runat="server" AutoGenerateColumns="False" DataKeyNames="EquipmentID" CssClass="table table-bordered">
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
                </div>

                <div class="mt-4">
                    <h5 class="text-primary"><i class="fas fa-vial me-2"></i> Assign Bottled Chemicals</h5>
                    <div class="table-responsive">
                        <asp:GridView ID="gvChemicals" runat="server" AutoGenerateColumns="False" DataKeyNames="ItemID" CssClass="table table-bordered">
                            <Columns>
                                <asp:BoundField DataField="ItemID" HeaderText="ID" />
                                <asp:BoundField DataField="Name" HeaderText="Chemical Name" />
                                <asp:TemplateField HeaderText="Bottles Available">
                                    <ItemTemplate><asp:Label ID="lblQuantity" runat="server" Text='<%# Eval("Quantity") %>' /></ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Excess mL">
                                    <ItemTemplate><asp:Label ID="lblExcessML" runat="server" Text='<%# Eval("ExcessML") %>' /></ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Bottle Buffer">
                                    <ItemTemplate><asp:TextBox ID="txtBottleBuffer" runat="server" CssClass="form-control" Text="0" /></ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Use This Chemical?">
                                    <ItemTemplate><asp:CheckBox ID="chkUseChemical" runat="server" /></ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>

                <div class="mt-4">
                    <h5 class="text-primary"><i class="fas fa-box me-2"></i> Assign Sachet Pack Chemicals</h5>
                    <div class="table-responsive">
                        <asp:GridView ID="gvSachetChemicals" runat="server" AutoGenerateColumns="False" DataKeyNames="ItemID" CssClass="table table-bordered">
                            <Columns>
                                <asp:BoundField DataField="ItemID" HeaderText="ID" />
                                <asp:BoundField DataField="Name" HeaderText="Sachet Name" />
                                <asp:TemplateField HeaderText="Packs Available">
                                    <ItemTemplate><asp:Label ID="lblSachetQuantity" runat="server" Text='<%# Eval("Quantity") %>' /></ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Assign Quantity">
                                    <ItemTemplate>
                                        <asp:TextBox ID="txtAssignSachet" runat="server" Text="0" CssClass="form-control" onkeypress="return isNumberKey(event);" onblur="setZeroIfEmpty(this);" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>

                <div class="mt-4">
                    <h5 class="text-primary"><i class="fas fa-hard-hat me-2"></i> Assign Safety Gear</h5>
                    <div class="table-responsive">
                        <asp:GridView ID="gvSafetyGears" runat="server" AutoGenerateColumns="False" DataKeyNames="ItemID" CssClass="table table-bordered">
                            <Columns>
                                <asp:BoundField DataField="ItemID" HeaderText="Gear ID" />
                                <asp:BoundField DataField="Name" HeaderText="Gear Name" />
                                <asp:TemplateField HeaderText="Available Quantity">
                                    <ItemTemplate><asp:Label ID="lblSafetyQuantity" runat="server" Text='<%# Eval("Quantity") %>' /></ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Assign Quantity">
                                    <ItemTemplate><asp:TextBox ID="txtAssignSafety" runat="server" Text="0" CssClass="form-control" /></ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>

                <div class="mt-4 text-end">
                    <asp:Button ID="btnAssignAll" runat="server" Text="Assign Booking" CssClass="btn btn-primary px-4" OnClick="btnAssignAll_Click" UseSubmitBehavior="false" />
                </div>
                <asp:Label ID="lblMessage" runat="server" Visible="false" />
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

<script>
    window.onload = function () {
        const params = new URLSearchParams(window.location.search);
        if (params.get('status') === 'assigned') {
            Swal.fire({
                icon: 'success',
                title: 'Success!',
                text: '✅ Booking, chemical, and buffer assignment successful.',
                confirmButtonColor: '#4caf50'
            }).then(() => {
                window.history.replaceState({}, document.title, window.location.pathname);
            });
        }

        const assignBtn = document.getElementById('<%= btnAssignAll.ClientID %>');
        if (assignBtn) {
            assignBtn.onclick = function (e) {
                e.preventDefault();
                Swal.fire({
                    title: 'Assign Booking?',
                    text: "This will assign the team, chemicals, and safety gear.",
                    icon: 'question',
                    showCancelButton: true,
                    confirmButtonColor: '#3085d6',
                    cancelButtonColor: '#d33',
                    confirmButtonText: 'Yes, assign it!'
                }).then((result) => {
                    if (result.isConfirmed) {
                        document.getElementById('<%= btnAssignAll.ClientID %>').click();
                    }
                });
            }
        }
    };
</script>

</asp:Content>