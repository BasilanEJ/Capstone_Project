<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="CreateCustomerAccount.aspx.cs" Inherits="RRCManagementSystem.CreateCustomerAccount" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto px-4 py-12">
        <div class="bg-white shadow-lg rounded-xl overflow-hidden max-w-4xl mx-auto">
            
            <div class="bg-blue-600 text-white text-center py-4 px-6">
                <h1 class="text-xl font-bold">Create Customer Account</h1>
            </div>

            <div class="p-8">
                <p class="text-gray-500 text-center mb-6 text-sm">
                    An email will be sent to the client to set their password.
                </p>

                <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
                    <div>
                        <label class="block text-sm font-medium text-gray-700">Last Name *</label>
                        <asp:TextBox ID="txtLastName" runat="server" CssClass="mt-1 block w-full border border-gray-300 rounded-md shadow-sm p-2 focus:border-blue-500 focus:ring-blue-500 outline-none" />
                    </div>
                    <div>
                        <label class="block text-sm font-medium text-gray-700">First Name *</label>
                        <asp:TextBox ID="txtFirstName" runat="server" CssClass="mt-1 block w-full border border-gray-300 rounded-md shadow-sm p-2 focus:border-blue-500 focus:ring-blue-500 outline-none" />
                    </div>
                </div>

                <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-6">
                    <div>
                        <label class="block text-sm font-medium text-gray-700">Middle Name</label>
                        <asp:TextBox ID="txtMiddleName" runat="server" CssClass="mt-1 block w-full border border-gray-300 rounded-md shadow-sm p-2 focus:border-blue-500 focus:ring-blue-500 outline-none" />
                    </div>
                    <div>
                        <label class="block text-sm font-medium text-gray-700">Email *</label>
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="mt-1 block w-full border border-gray-300 rounded-md shadow-sm p-2 focus:border-blue-500 focus:ring-blue-500 outline-none" TextMode="Email" />
                        <asp:RegularExpressionValidator ID="revEmail" runat="server"
                            ControlToValidate="txtEmail"
                            ErrorMessage="Please enter a valid Gmail, Yahoo, Outlook, iCloud, or school/government email address."
                            ForeColor="Red" Display="Dynamic"
                            ValidationGroup="inq"
                            ValidationExpression="^[A-Za-z0-9._%+\-]+@((?:gmail|yahoo|ymail|rocketmail|outlook|hotmail|live|msn|icloud|me|mac|protonmail|proton|zoho|zohomail)\.com|(?:[A-Za-z0-9-]+\.)+edu\.ph|(?:[A-Za-z0-9-]+\.)+gov\.ph)$" />
                    </div>
                    <div>
                        <label class="block text-sm font-medium text-gray-700">Contact Number *</label>
                        <asp:TextBox ID="txtContact" runat="server" CssClass="mt-1 block w-full border border-gray-300 rounded-md shadow-sm p-2 focus:border-blue-500 focus:ring-blue-500 outline-none" />
                    </div>
                </div>

                <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-6">
                    <div>
                        <label class="block text-sm font-medium text-gray-700">Country *</label>
                        <asp:TextBox ID="txtCountry" runat="server" CssClass="mt-1 block w-full border border-gray-300 rounded-md shadow-sm p-2 focus:border-blue-500 focus:ring-blue-500 outline-none" Text="Philippines" />
                    </div>
                    <asp:UpdatePanel ID="UpdatePanel1" runat="server" class="md:col-span-2">
                        <ContentTemplate>
                            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                                <div>
                                    <label class="block text-sm font-medium text-gray-700">Region *</label>
                                    <asp:DropDownList ID="ddlRegion" runat="server" AutoPostBack="true" CssClass="mt-1 block w-full border border-gray-300 rounded-md shadow-sm p-2 focus:border-blue-500 focus:ring-blue-500 outline-none" OnSelectedIndexChanged="ddlRegion_SelectedIndexChanged" />
                                </div>
                                <div>
                                    <label class="block text-sm font-medium text-gray-700">City *</label>
                                    <asp:DropDownList ID="ddlCity" runat="server" CssClass="mt-1 block w-full border border-gray-300 rounded-md shadow-sm p-2 focus:border-blue-500 focus:ring-blue-500 outline-none" />
                                </div>
                            </div>
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="ddlRegion" EventName="SelectedIndexChanged" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>

                <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
                    <div>
                        <label class="block text-sm font-medium text-gray-700">Barangay *</label>
                        <asp:TextBox ID="txtBarangay" runat="server" CssClass="mt-1 block w-full border border-gray-300 rounded-md shadow-sm p-2 focus:border-blue-500 focus:ring-blue-500 outline-none" />
                    </div>
                    <div>
                        <label class="block text-sm font-medium text-gray-700">Street & Unit *</label>
                        <asp:TextBox ID="txtStreet" runat="server" CssClass="mt-1 block w-full border border-gray-300 rounded-md shadow-sm p-2 focus:border-blue-500 focus:ring-blue-500 outline-none" />
                    </div>
                    <div>
                        <label class="block text-sm font-medium text-gray-700">Landmark</label>
                        <asp:TextBox ID="txtLandmark" runat="server" CssClass="mt-1 block w-full border border-gray-300 rounded-md shadow-sm p-2 focus:border-blue-500 focus:ring-blue-500 outline-none" />
                    </div>
                </div>

                <div class="flex justify-end">
                    <asp:Button ID="btnCreate" runat="server"
                        Text="Create Account"
                        CssClass="w-full bg-blue-600 text-white font-bold py-3 px-6 rounded-md hover:bg-blue-700 transition-colors duration-200"
                        OnClick="btnCreate_Click"
                        ValidationGroup="inq"
                        CausesValidation="true" />
                </div>
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
                if (input) {
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
                }
            });

            // Allow only digits in contact number
            if (contactInput) {
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
            }
        };
    </script>
</asp:Content>