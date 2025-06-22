<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="CreateCustomerAccount.aspx.cs" Inherits="RRCManagementSystem.CreateCustomerAccount" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true" />

    <div class="container my-5">
        <div class="card shadow-sm">
            <div class="card-header bg-primary text-white text-center fw-bold">
                Create Customer Account
            </div>
            <div class="card-body">
                <p class="text-muted text-center mb-4">An email will be sent to the client to set their password.</p>

                <div class="mb-3">
                    <label class="form-label">Name *</label>
                    <asp:TextBox ID="txtName" runat="server" CssClass="form-control" />
                </div>

                <div class="mb-3">
                    <label class="form-label">Email *</label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" />
                </div>

                <div class="mb-3">
                    <label class="form-label">Contact Number *</label>
                    <asp:TextBox ID="txtContact" runat="server" CssClass="form-control" />
                </div>

                <div class="mb-3">
                    <label class="form-label">Country *</label>
                    <asp:TextBox ID="txtCountry" runat="server" CssClass="form-control" Text="Philippines" />
                </div>

                <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                    <ContentTemplate>
                        <div class="mb-3">
                            <label class="form-label">Region *</label>
                            <asp:DropDownList ID="ddlRegion" runat="server" AutoPostBack="true" CssClass="form-select" OnSelectedIndexChanged="ddlRegion_SelectedIndexChanged" />
                        </div>
                        <div class="mb-3">
                            <label class="form-label">City *</label>
                            <asp:DropDownList ID="ddlCity" runat="server" CssClass="form-select" />
                        </div>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="ddlRegion" EventName="SelectedIndexChanged" />
                    </Triggers>
                </asp:UpdatePanel>

                <div class="mb-3">
                    <label class="form-label">Barangay *</label>
                    <asp:TextBox ID="txtBarangay" runat="server" CssClass="form-control" />
                </div>

                <div class="mb-3">
                    <label class="form-label">Street & Unit *</label>
                    <asp:TextBox ID="txtStreet" runat="server" CssClass="form-control" />
                </div>

                <div class="mb-4">
                    <label class="form-label">Landmark</label>
                    <asp:TextBox ID="txtLandmark" runat="server" CssClass="form-control" />
                </div>

                <asp:Button ID="btnCreate" runat="server" Text="Create Account" CssClass="btn btn-primary w-100" OnClick="btnCreate_Click" />
            </div>
        </div>
    </div>

    <asp:Literal ID="ltScript" runat="server" />

    <script type="text/javascript">
        window.onload = function () {
            const nameInput = document.getElementById('<%= txtName.ClientID %>');
            const contactInput = document.getElementById('<%= txtContact.ClientID %>');

            // Block numbers in name field
            nameInput.addEventListener('keypress', function (e) {
                const charCode = e.which || e.keyCode;
                if (charCode >= 48 && charCode <= 57) {
                    e.preventDefault();
                }
            });

            // Allow only digits in contact number
            contactInput.addEventListener('keypress', function (e) {
                const charCode = e.which || e.keyCode;
                if (charCode < 48 || charCode > 57) {
                    e.preventDefault();
                }
            });

            // Limit to 11 digits
            contactInput.addEventListener('input', function () {
                if (contactInput.value.length > 11) {
                    contactInput.value = contactInput.value.slice(0, 11);
                }
            });

            // Prevent pasting non-numbers
            contactInput.addEventListener('paste', function (e) {
                const paste = (e.clipboardData || window.clipboardData).getData('text');
                if (!/^\d{1,11}$/.test(paste)) {
                    e.preventDefault();
                }
            });
        };
</script>
</asp:Content>
