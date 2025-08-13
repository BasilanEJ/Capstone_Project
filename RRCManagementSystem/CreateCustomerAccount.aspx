<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="CreateCustomerAccount.aspx.cs" Inherits="RRCManagementSystem.CreateCustomerAccount" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

   <div class="container my-5">
    <div class="card shadow-sm mx-auto" style="max-width: 800px;">
        <div class="card-header bg-primary text-white text-center fw-bold fs-5">
            Create Customer Account
        </div>
        <div class="card-body px-4 py-4">
            <p class="text-muted text-center mb-4">
                An email will be sent to the client to set their password.
            </p>

            <div class="row g-3 mb-3">
                <div class="col-md-6">
                    <label class="form-label">Last Name *</label>
                    <asp:TextBox ID="txtLastName" runat="server" CssClass="form-control" />
                </div>
                <div class="col-md-6">
                    <label class="form-label">First Name *</label>
                    <asp:TextBox ID="txtFirstName" runat="server" CssClass="form-control" />
                </div>
            </div>

          <div class="row mb-3">
    <div class="col-md-4">
        <label class="form-label">Middle Name</label>
        <asp:TextBox ID="txtMiddleName" runat="server" CssClass="form-control" />
    </div>

    <div class="col-md-4">
        <label class="form-label">Email *</label>
        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" />
    </div>

    <div class="col-md-4">
        <label class="form-label">Contact Number *</label>
        <asp:TextBox ID="txtContact" runat="server" CssClass="form-control" />
    </div>
</div>

          <div class="row mb-3">
    <div class="col-md-4">
        <label class="form-label">Country *</label>
        <asp:TextBox ID="txtCountry" runat="server" CssClass="form-control" Text="Philippines" />
    </div>

    <asp:UpdatePanel ID="UpdatePanel1" runat="server" class="col-md-8">
        <ContentTemplate>
            <div class="row g-3">
                <div class="col-md-6">
                    <label class="form-label">Region *</label>
                    <asp:DropDownList ID="ddlRegion" runat="server" AutoPostBack="true" CssClass="form-select" OnSelectedIndexChanged="ddlRegion_SelectedIndexChanged" />
                </div>
                <div class="col-md-6">
                    <label class="form-label">City *</label>
                    <asp:DropDownList ID="ddlCity" runat="server" CssClass="form-select" />
                </div>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="ddlRegion" EventName="SelectedIndexChanged" />
        </Triggers>
    </asp:UpdatePanel>
</div>


           <div class="row mb-4">
    <div class="col-md-4">
        <label class="form-label">Barangay *</label>
        <asp:TextBox ID="txtBarangay" runat="server" CssClass="form-control" />
    </div>

    <div class="col-md-4">
        <label class="form-label">Street & Unit *</label>
        <asp:TextBox ID="txtStreet" runat="server" CssClass="form-control" />
    </div>

    <div class="col-md-4">
        <label class="form-label">Landmark</label>
        <asp:TextBox ID="txtLandmark" runat="server" CssClass="form-control" />
    </div>
</div>


            <asp:Button ID="btnCreate" runat="server" Text="Create Account" CssClass="btn btn-primary w-100" OnClick="btnCreate_Click" />
        </div>
    </div>
</div>


    <asp:Literal ID="ltScript" runat="server" />

 <script type="text/javascript">
     window.onload = function () {
         const lastNameInput = document.getElementById('<%= txtLastName.ClientID %>');
        const firstNameInput = document.getElementById('<%= txtFirstName.ClientID %>');
        const middleNameInput = document.getElementById('<%= txtMiddleName.ClientID %>');
        const contactInput = document.getElementById('<%= txtContact.ClientID %>');

         // Block numbers in LastName, FirstName, and MiddleName
         [lastNameInput, firstNameInput, middleNameInput].forEach(function (input) {
             input.addEventListener('keypress', function (e) {
                 const charCode = e.which || e.keyCode;
                 if (charCode >= 48 && charCode <= 57) { // digits 0–9
                     e.preventDefault();
                 }
             });

             // Prevent pasting numbers into name fields
             input.addEventListener('paste', function (e) {
                 const paste = (e.clipboardData || window.clipboardData).getData('text');
                 if (/\d/.test(paste)) {
                     e.preventDefault();
                 }
             });
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

         // Prevent pasting non-numbers into contact
         contactInput.addEventListener('paste', function (e) {
             const paste = (e.clipboardData || window.clipboardData).getData('text');
             if (!/^\d{1,11}$/.test(paste)) {
                 e.preventDefault();
             }
         });
     };
 </script>

</asp:Content>
