<%@ Page Title="Add Supplier" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AddSupplier.aspx.cs" Inherits="RRCManagementSystem.AddSupplier" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Tailwind CSS CDN -->
    <script src="https://cdn.tailwindcss.com"></script>
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <!-- Font Awesome for icons -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0-beta3/css/all.min.css">
    <!-- SweetAlert2 CDN for modern alerts -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <script>
        tailwind.config = {
            theme: {
                extend: {
                    fontFamily: {
                        sans: ['Inter', 'sans-serif'],
                    },
                }
            }
        }
    </script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto p-4 sm:p-6 md:p-8">
        <div class="bg-white rounded-xl shadow-md max-w-2xl mx-auto overflow-hidden">
            <div class="bg-blue-800 text-white p-4 md:p-6 font-bold text-lg md:text-xl">
                <i class="fas fa-truck-moving mr-2"></i> Add New Supplier
            </div>
            <div class="p-4 md:p-6">
                <!-- Supplier Name -->
                <div class="mb-4">
                    <label for="txtName" class="block font-semibold text-gray-700 mb-2">Supplier Name *</label>
                    <asp:TextBox ID="txtName" runat="server" CssClass="form-input w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500" placeholder="Enter supplier name" />
                </div>

                <!-- Address -->
                <div class="mb-4">
                    <label for="txtAddress" class="block font-semibold text-gray-700 mb-2">Address *</label>
                    <asp:TextBox ID="txtAddress" runat="server" CssClass="form-input w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500" placeholder="Enter address" />
                </div>

                <!-- Contact Number -->
                <div class="mb-4">
                    <label for="txtContactNumber" class="block font-semibold text-gray-700 mb-2">Contact Number *</label>
                    <asp:TextBox
                        ID="txtContactNumber"
                        runat="server"
                        CssClass="form-input w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                        placeholder="Enter contact number"
                        onkeypress="return isNumberKey(event);"
                        maxlength="11" onblur="validateContactNumber();" />
                    
                    <!-- Required Field Validator -->
                    <asp:RequiredFieldValidator
                        ID="rfvContactNumber"
                        runat="server"
                        ControlToValidate="txtContactNumber"
                        ForeColor="red"
                        ErrorMessage="Contact number is required." />

                    <!-- Regular Expression Validator -->
                    <asp:RegularExpressionValidator
                        ID="revContactNumber"
                        runat="server"
                        ControlToValidate="txtContactNumber"
                        ForeColor="red"
                        ValidationExpression="^09\d{8,9}$"
                        ErrorMessage="Contact number must start with '09' and be 9 or 11 digits long." Visible="false" />
                </div>

                <script type="text/javascript">
                    function isNumberKey(evt) {
                        var charCode = (evt.which) ? evt.which : evt.keyCode;
                        return (charCode >= 48 && charCode <= 57 || charCode === 8);
                    }
                </script>

                <!-- Email -->
                <div class="mb-4">
                    <label for="txtEmail" class="block font-semibold text-gray-700 mb-2">Email</label>
                    <asp:TextBox
                        ID="txtEmail"
                        runat="server"
                        CssClass="form-input w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                        placeholder="Enter email" onblur="validateEmail();" />
                    
                    <!-- Required Field Validator -->
                    <asp:RequiredFieldValidator
                        ID="rfvEmail"
                        runat="server"
                        ControlToValidate="txtEmail"
                        ForeColor="red"
                        ErrorMessage="Email is required." />

                    <!-- Regular Expression Validator -->
                    <asp:RegularExpressionValidator
                        ID="revEmail"
                        runat="server"
                        ControlToValidate="txtEmail"
                        ForeColor="red"
                        ValidationExpression="^[a-zA-Z0-9._%+-]+@(gmail\.com|yahoo\.com|outlook\.com)$"
                        ErrorMessage="Email must be in the form of 'xxx@gmail.com', 'xxx@yahoo.com', or 'xxx@outlook.com.'" Visible="false" />
                </div>

                <!-- Company Name -->
                <div class="mb-4">
                    <label for="txtCompanyName" class="block font-semibold text-gray-700 mb-2">Company Name</label>
                    <asp:TextBox ID="txtCompanyName" runat="server" CssClass="form-input w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500" placeholder="Enter company name (optional)" />
                </div>

                <!-- Business Type -->
                <div class="mb-4">
                    <label for="ddlBusinessType" class="block font-semibold text-gray-700 mb-2">Business Type</label>
                    <asp:DropDownList ID="ddlBusinessType" runat="server" CssClass="form-select w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500">
                        <asp:ListItem Text="Select Business Type" Value="" />
                        <asp:ListItem Text="Pest Control Products" Value="Pest Control Products" />
                        <asp:ListItem Text="PPE Materials" Value="PPE Materials" />
                        <asp:ListItem Text="Chemicals" Value="Chemicals" />
                        <asp:ListItem Text="Equipment" Value="Equipment" />
                        <asp:ListItem Text="Others" Value="Others" />
                    </asp:DropDownList>
                </div>

                <!-- Status -->
                <div class="mb-6">
                    <label for="ddlStatus" class="block font-semibold text-gray-700 mb-2">Status</label>
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500">
                        <asp:ListItem Text="Select Status" Value="" />
                        <asp:ListItem Text="Active" Value="Active" />
                        <asp:ListItem Text="Inactive" Value="Inactive" />
                    </asp:DropDownList>
                </div>

                <!-- SweetAlert-triggering button -->
                <asp:Button
                    ID="btnSubmit"
                    runat="server"
                    Text="Add Supplier"
                    CssClass="w-full bg-blue-800 hover:bg-blue-900 text-white font-bold py-3 rounded-lg transition-colors duration-200 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
                    UseSubmitBehavior="false"
                    OnClientClick="return confirmAddSupplier(this);"
                    OnClick="btnSubmit_Click" />

                <asp:Label ID="lblMessage" runat="server" CssClass="alert-message mt-4 bg-red-100 text-red-700 border border-red-200 rounded-lg p-3" style="display:none;"></asp:Label>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        function confirmAddSupplier(btn) {
            var isValid = true;
            // You can add additional checks here before the Swal.fire prompt
            if (!validateContactNumber() || !validateEmail()) {
                isValid = false;
            }

            if (!isValid) {
                return false;
            }

            Swal.fire({
                title: 'Are you sure?',
                text: "Do you want to add this supplier?",
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#1D4ED8',
                cancelButtonColor: '#4B5563',
                confirmButtonText: 'Yes, add it!'
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack(btn.name, '');
                }
            });
            return false;
        }

        function validateContactNumber() {
            var contactNumber = document.getElementById('<%= txtContactNumber.ClientID %>').value;
            var errorMessage = "";
            var regex = /^09\d{8,9}$/;
            var messageLabel = document.getElementById('<%= lblMessage.ClientID %>');

            if (!regex.test(contactNumber)) {
                errorMessage = "<i class='fas fa-exclamation-circle mr-2'></i>Contact number must start with '09' and be 9 or 11 digits long.";
            }

            if (errorMessage !== "") {
                messageLabel.innerHTML = errorMessage;
                messageLabel.style.display = 'block';
                return false;
            } else {
                messageLabel.style.display = 'none';
                return true;
            }
        }

        function validateEmail() {
            var email = document.getElementById('<%= txtEmail.ClientID %>').value;
            var errorMessage = "";
            var regex = /^[a-zA-Z0-9._%+-]+@(gmail\.com|yahoo\.com|outlook\.com)$/;
            var messageLabel = document.getElementById('<%= lblMessage.ClientID %>');

            if (!regex.test(email)) {
                errorMessage = "<i class='fas fa-exclamation-circle mr-2'></i>Email must be in the form of 'xxx@gmail.com', 'xxx@yahoo.com', or 'xxx@outlook.com.'";
            }

            if (errorMessage !== "") {
                messageLabel.innerHTML = errorMessage;
                messageLabel.style.display = 'block';
                return false;
            } else {
                messageLabel.style.display = 'none';
                return true;
            }
        }
    </script>
</asp:Content>
