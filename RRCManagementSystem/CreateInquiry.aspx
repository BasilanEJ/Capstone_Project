<%@ Page Title="Create Inquiry"
    Language="C#"
    MasterPageFile="~/Admin.Master"
    AutoEventWireup="true"
    CodeBehind="CreateInquiry.aspx.cs"
    Inherits="RRCManagementSystem.CreateInquiry" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <script src="https://cdn.tailwindcss.com"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" />

    <div class="container mx-auto p-4 md:p-8">
        <h2 class="text-center text-3xl font-bold text-slate-900 mt-8 mb-4">📝 Create Inquiry</h2>

        <asp:Label ID="lblPermission" runat="server" CssClass="text-red-500 font-semibold text-center block" />
        <asp:ValidationSummary ID="vs" runat="server" CssClass="hidden" ValidationGroup="inq" />

        <div class="bg-white border border-gray-200 rounded-xl p-6 md:p-8 max-w-4xl mx-auto shadow-lg mt-8 mb-10">

            <!-- Contact Info Section -->
            <div class="font-bold text-blue-600 mt-4 mb-2">Contact Info</div>
            <div class="h-px bg-slate-200 my-2"></div>

            <div class="grid grid-cols-1 md:grid-cols-2 gap-4 md:gap-6">
                <div>
                    <label for="<%= txtEmail.ClientID %>" class="block font-semibold text-slate-700 mb-1">Email <span class="text-red-500">*</span></label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="w-full px-4 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500" />
                    <asp:RequiredFieldValidator runat="server"
                        ControlToValidate="txtEmail"
                        ValidationGroup="inq"
                        ErrorMessage="Email is required"
                        CssClass="text-red-500 text-sm mt-1 block" Display="Dynamic" />
                    <asp:RegularExpressionValidator ID="revEmail" runat="server"
                        ControlToValidate="txtEmail"
                        ErrorMessage="Please enter a valid Gmail, Yahoo, Outlook, iCloud, or school/government email address."
                        ForeColor="Red" Display="Dynamic"
                        ValidationGroup="inq"
                        ValidationExpression="^[A-Za-z0-9._%+\-]+@((?:gmail|yahoo|ymail|rocketmail|outlook|hotmail|live|msn|icloud|me|mac|protonmail|proton|zoho|zohomail)\.com|(?:[A-Za-z0-9-]+\.)+edu\.ph|(?:[A-Za-z0-9-]+\.)+gov\.ph)$" />
                </div>

                <div>
                    <label for="<%= txtContact.ClientID %>" class="block font-semibold text-slate-700 mb-1">Contact Number <span class="text-red-500">*</span></label>
                    <asp:TextBox ID="txtContact" runat="server" CssClass="w-full px-4 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500" MaxLength="11" />
                    <asp:RequiredFieldValidator runat="server"
                        ControlToValidate="txtContact"
                        ValidationGroup="inq"
                        ErrorMessage="Contact Number is required"
                        CssClass="text-red-500 text-sm mt-1 block" Display="Dynamic" />
                    <asp:RegularExpressionValidator runat="server"
                        ControlToValidate="txtContact"
                        ValidationGroup="inq"
                        ValidationExpression="^\d{11}$"
                        ErrorMessage="Contact Number must be exactly 11 digits"
                        CssClass="text-red-500 text-sm mt-1 block" Display="Dynamic" />
                    <div class="text-slate-500 text-xs mt-1">Format: 11 digits (e.g., 09xxxxxxxxx)</div>
                </div>

                <div class="col-span-1 md:col-span-2">
                    <label for="<%= txtMessage.ClientID %>" class="block font-semibold text-slate-700 mb-1">Message</label>
                    <asp:TextBox ID="txtMessage" runat="server" CssClass="w-full px-4 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500" MaxLength="255" />
                    <div class="text-slate-500 text-xs mt-1">Optional — up to 255 characters</div>
                </div>
            </div>

            <!-- Optional Info Section -->
            <div class="font-bold text-blue-600 mt-6 mb-2">Optional Info</div>
            <div class="h-px bg-slate-200 my-2"></div>

            <div class="grid grid-cols-1 md:grid-cols-2 gap-4 md:gap-6">
                <div>
                    <label for="<%= txtFirstName.ClientID %>" class="block font-semibold text-slate-700 mb-1">First Name</label>
                    <asp:TextBox ID="txtFirstName" runat="server" CssClass="w-full px-4 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500" />
                    <asp:RegularExpressionValidator runat="server"
                        ControlToValidate="txtFirstName"
                        ValidationGroup="inq"
                        ValidationExpression="^[A-Za-z\s\-\.'’]*$"
                        ErrorMessage="First Name cannot contain numbers or symbols"
                        CssClass="text-red-500 text-sm mt-1 block" Display="Dynamic" />
                </div>

                <div>
                    <label for="<%= txtMiddleName.ClientID %>" class="block font-semibold text-slate-700 mb-1">Middle Name</label>
                    <asp:TextBox ID="txtMiddleName" runat="server" CssClass="w-full px-4 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500" />
                    <asp:RegularExpressionValidator runat="server"
                        ControlToValidate="txtMiddleName"
                        ValidationGroup="inq"
                        ValidationExpression="^[A-Za-z\s\-\.'’]*$"
                        ErrorMessage="Middle Name cannot contain numbers or symbols"
                        CssClass="text-red-500 text-sm mt-1 block" Display="Dynamic" />
                </div>

                <div>
                    <label for="<%= txtLastName.ClientID %>" class="block font-semibold text-slate-700 mb-1">Last Name</label>
                    <asp:TextBox ID="txtLastName" runat="server" CssClass="w-full px-4 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500" />
                    <asp:RegularExpressionValidator runat="server"
                        ControlToValidate="txtLastName"
                        ValidationGroup="inq"
                        ValidationExpression="^[A-Za-z\s\-\.'’]*$"
                        ErrorMessage="Last Name cannot contain numbers or symbols"
                        CssClass="text-red-500 text-sm mt-1 block" Display="Dynamic" />
                </div>

                <div>
                    <label for="<%= txtCountry.ClientID %>" class="block font-semibold text-slate-700 mb-1">Country</label>
                    <asp:TextBox ID="txtCountry" runat="server" CssClass="w-full px-4 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500" Text="Philippines" />
                </div>

                <div>
                    <label for="<%= txtRegion.ClientID %>" class="block font-semibold text-slate-700 mb-1">Region</label>
                    <asp:TextBox ID="txtRegion" runat="server" CssClass="w-full px-4 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500" />
                </div>

                <div>
                    <label for="<%= txtCity.ClientID %>" class="block font-semibold text-slate-700 mb-1">City</label>
                    <asp:TextBox ID="txtCity" runat="server" CssClass="w-full px-4 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500" />
                </div>

                <div>
                    <label for="<%= txtBarangay.ClientID %>" class="block font-semibold text-slate-700 mb-1">Barangay</label>
                    <asp:TextBox ID="txtBarangay" runat="server" CssClass="w-full px-4 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500" />
                </div>

                <div>
                    <label for="<%= txtStreet.ClientID %>" class="block font-semibold text-slate-700 mb-1">Street &amp; Unit</label>
                    <asp:TextBox ID="txtStreet" runat="server" CssClass="w-full px-4 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500" />
                </div>

                <div class="col-span-1 md:col-span-2">
                    <label for="<%= txtLandmark.ClientID %>" class="block font-semibold text-slate-700 mb-1">Landmark</label>
                    <asp:TextBox ID="txtLandmark" runat="server" CssClass="w-full px-4 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500" />
                </div>

                <div class="col-span-1 md:col-span-2">
                    <label for="<%= fuPhoto.ClientID %>" class="block font-semibold text-slate-700 mb-1">Photo (Optional)</label>
                    <asp:FileUpload ID="fuPhoto" runat="server" CssClass="w-full px-4 py-2 border border-slate-300 rounded-lg file:bg-blue-600 file:text-white file:px-4 file:py-2 file:rounded-md file:border-0 file:cursor-pointer file:hover:bg-blue-700 file:transition-colors file:mr-4" />
                    <div class="text-slate-500 text-xs mt-1">Allowed: .jpg, .jpeg, .png — Max: 5 MB</div>
                </div>
            </div>

            <div class="flex flex-col sm:flex-row-reverse gap-3 sm:gap-4 justify-end mt-6">
                <a href="AllInquiry.aspx" class="bg-gray-500 text-white px-6 py-3 rounded-xl font-bold text-sm cursor-pointer hover:bg-gray-600 transition-colors text-center w-full sm:w-auto">Back to All Inquiries</a>
                <asp:Button ID="btnSave" runat="server" CssClass="bg-blue-600 text-white px-6 py-3 rounded-xl font-bold text-sm cursor-pointer hover:bg-blue-700 transition-colors w-full sm:w-auto" Text="Save Inquiry"
                    OnClick="btnSave_Click"
                    ValidationGroup="inq"
                    UseSubmitBehavior="false"
                    OnClientClick="return validateAndConfirm();" />
            </div>
        </div>
    </div>

    <!-- Scripts -->
    <script>
        (function () {
            const contact = document.getElementById('<%= txtContact.ClientID %>');
            if (contact) {
                contact.setAttribute('inputmode', 'numeric');
                contact.addEventListener('input', function () {
                    this.value = this.value.replace(/\D/g, '').slice(0, 11);
                });
            }
            ['<%= txtFirstName.ClientID %>', '<%= txtMiddleName.ClientID %>', '<%= txtLastName.ClientID %>']
                .forEach(id => {
                    const el = document.getElementById(id);
                    if (!el) return;
                    el.addEventListener('input', function () {
                        this.value = this.value.replace(/[^A-Za-z\s\-\.'’]/g, '');
                    });
                });
        })();

        function validateAndConfirm() {
            if (typeof (Page_ClientValidate) === 'function') {
                Page_ClientValidate('inq');
                if (!Page_IsValid) {
                    const emailVal = document.getElementById('<%= txtEmail.ClientID %>').value.trim();
                    const contactVal = document.getElementById('<%= txtContact.ClientID %>').value.trim();
                    const errs = [];
                    if (!emailVal) errs.push('Email');
                    else if (!/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(emailVal)) errs.push('Valid Email');
                    if (!contactVal) errs.push('Contact Number');
                    else if (!/^\d{11}$/.test(contactVal)) errs.push('11-digit Contact Number');

                    Swal.fire({
                        icon: 'warning',
                        title: 'Missing / Invalid Fields',
                        text: 'Please fix: ' + (errs.length ? errs.join(', ') : 'the highlighted fields') + '.'
                    });
                    return false;
                }
            }

            const email = document.getElementById('<%= txtEmail.ClientID %>').value.trim();
            const contact = document.getElementById('<%= txtContact.ClientID %>').value.trim();
            if (!/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(email) || !/^\d{11}$/.test(contact)) {
                Swal.fire({
                    icon: 'error',
                    title: 'Invalid input',
                    text: !/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(email)
                        ? 'Please enter a valid email address.'
                        : 'Contact Number must be exactly 11 digits.'
                });
                return false;
            }

            Swal.fire({
                title: 'Save this inquiry?',
                text: 'You can review details on the next page.',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Yes, save it',
                cancelButtonText: 'Cancel'
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack('<%= btnSave.UniqueID %>', '');
                }
            });

            return false;
        }
    </script>
</asp:Content>
