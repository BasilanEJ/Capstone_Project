<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="EditEmployee.aspx.cs" Inherits="RRCManagementSystem.EditEmployee" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    </asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto py-10 px-4 flex justify-center items-start">
        <div class="w-full max-w-2xl bg-white p-8 rounded-2xl shadow-xl border border-gray-200">
            <h2 class="text-center text-3xl font-extrabold text-blue-800 mb-6">Edit Employee</h2>

            <div class="mb-4">
                <asp:Label ID="lblMessage" runat="server" CssClass="block text-center text-sm font-medium text-red-600" />
            </div>

            <div class="mb-6 flex flex-col items-center">
                <label for="<%= fuProfilePicture.ClientID %>" class="text-sm font-semibold text-gray-700 mb-2">Profile Picture (JPG, JPEG, PNG only)</label>
                <asp:Image ID="imgProfilePreview" runat="server" CssClass="h-32 w-32 object-cover rounded-full mb-4 border-2 border-gray-300 shadow-md" />
                <asp:FileUpload ID="fuProfilePicture" runat="server" CssClass="block w-full text-sm text-gray-700 file:mr-4 file:py-2 file:px-4 file:rounded-lg file:border-0 file:text-sm file:font-semibold file:bg-blue-500 file:text-white hover:file:bg-blue-600 transition-colors" accept="image/*" onchange="previewImage();" />
            </div>

            <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-6">
                <div>
                    <label for="<%= txtLastName.ClientID %>" class="block text-sm font-semibold text-gray-700 mb-2">Last Name</label>
                    <asp:TextBox ID="txtLastName" runat="server" CssClass="block w-full px-4 py-2 text-gray-700 bg-gray-50 border border-gray-300 rounded-lg focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-colors" placeholder="Enter last name" required oninput="this.value=this.value.replace(/[^a-zA-Z\s.-]/g,'')"></asp:TextBox>
                </div>
                <div>
                    <label for="<%= txtFirstName.ClientID %>" class="block text-sm font-semibold text-gray-700 mb-2">First Name</label>
                    <asp:TextBox ID="txtFirstName" runat="server" CssClass="block w-full px-4 py-2 text-gray-700 bg-gray-50 border border-gray-300 rounded-lg focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-colors" placeholder="Enter first name" required oninput="this.value=this.value.replace(/[^a-zA-Z\s.-]/g,'')"></asp:TextBox>
                </div>
                <div>
                    <label for="<%= txtMiddleName.ClientID %>" class="block text-sm font-semibold text-gray-700 mb-2">Middle Name <span class="text-gray-400 font-normal">(optional)</span></label>
                    <asp:TextBox ID="txtMiddleName" runat="server" CssClass="block w-full px-4 py-2 text-gray-700 bg-gray-50 border border-gray-300 rounded-lg focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-colors" placeholder="Enter middle name" oninput="this.value=this.value.replace(/[^a-zA-Z\s.-]/g,'')"></asp:TextBox>
                </div>
            </div>

            <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
                <div>
                    <label for="<%= txtEmail.ClientID %>" class="block text-sm font-semibold text-gray-700 mb-2">Email</label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="block w-full px-4 py-2 text-gray-700 bg-gray-50 border border-gray-300 rounded-lg focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-colors" TextMode="Email" placeholder="Enter email address" required></asp:TextBox>
                </div>
                <div>
                    <label for="<%= txtPhone.ClientID %>" class="block text-sm font-semibold text-gray-700 mb-2">Phone Number</label>
                    <asp:TextBox ID="txtPhone" runat="server" CssClass="block w-full px-4 py-2 text-gray-700 bg-gray-50 border border-gray-300 rounded-lg focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-colors" placeholder="Enter 11-digit Phone Number" required oninput="this.value = this.value.replace(/[^0-9]/g, '').slice(0, 11)"></asp:TextBox>
                </div>
            </div>

            <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
                <div>
                    <label for="<%= ddlPosition.ClientID %>" class="block text-sm font-semibold text-gray-700 mb-2">Position</label>
                    <asp:DropDownList ID="ddlPosition" runat="server" CssClass="block w-full px-4 py-2 text-gray-700 bg-white border border-gray-300 rounded-lg focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-colors">
                        <asp:ListItem Text="Select Position" Value="" />
                        <asp:ListItem Text="IT" Value="IT" />
                        <asp:ListItem Text="Technician" Value="Technician" />
                    </asp:DropDownList>
                </div>
                <div>
                    <label for="<%= ddlStatus.ClientID %>" class="block text-sm font-semibold text-gray-700 mb-2">Status</label>
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="block w-full px-4 py-2 text-gray-700 bg-white border border-gray-300 rounded-lg focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-colors">
                        <asp:ListItem Text="Available" Value="Available" />
                        <asp:ListItem Text="Unavailable" Value="Unavailable" />
                        <asp:ListItem Text="Resigned" Value="Resigned" />
                    </asp:DropDownList>
                </div>
            </div>

            <div class="flex flex-col sm:flex-row justify-center space-y-3 sm:space-y-0 sm:space-x-4">
                <asp:Button ID="btnSave" runat="server" Text="Save Changes"
                    CssClass="w-full sm:w-auto px-6 py-3 bg-green-600 text-white font-bold rounded-lg shadow-md hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-green-500 transition-colors"
                    OnClientClick="return confirmSave();" OnClick="btnSave_Click" />

                <asp:Button ID="btnCancel" runat="server" Text="Cancel"
                    CssClass="w-full sm:w-auto px-6 py-3 bg-gray-600 text-white font-bold rounded-lg shadow-md hover:bg-gray-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500 transition-colors"
                    OnClientClick="window.location.href='AllEmployee.aspx'; return false;" />
            </div>
        </div>
    </div>

    <script type="text/javascript">
        function previewImage() {
            var fileInput = document.getElementById('<%= fuProfilePicture.ClientID %>');
            var imgPreview = document.getElementById('<%= imgProfilePreview.ClientID %>');

            // Client-side validation for file type
            var allowedExtensions = /(\.jpg|\.jpeg|\.png)$/i;
            if (fileInput.files.length > 0 && !allowedExtensions.exec(fileInput.value)) {
                Swal.fire({
                    icon: 'error',
                    title: 'Invalid File Type',
                    text: 'Only JPG, JPEG, and PNG files are allowed.'
                });
                fileInput.value = '';
                // Keep the existing image if no valid new image is selected
                return;
            }

            if (fileInput.files.length > 0) {
                var file = fileInput.files[0];
                var reader = new FileReader();

                reader.onload = function (e) {
                    imgPreview.src = e.target.result;
                    imgPreview.style.display = "inline-block";
                };
                reader.readAsDataURL(file);
            }
        }

        function confirmSave() {
            // Client-side validation for names, phone, and position before showing SweetAlert
            if (!validateForm()) {
                return false;
            }
            
            event.preventDefault();
            Swal.fire({
                title: 'Are you sure?',
                text: "Do you want to save the changes?",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#15803d', // Tailwind's green-700
                cancelButtonColor: '#4b5563', // Tailwind's gray-600
                confirmButtonText: 'Yes, save it!',
                cancelButtonText: 'Cancel'
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack('<%= btnSave.UniqueID %>', '');
                }
            });
            return false;
        }

        function validateForm() {
            var lastName = document.getElementById('<%= txtLastName.ClientID %>').value.trim();
            var firstName = document.getElementById('<%= txtFirstName.ClientID %>').value.trim();
            var phone = document.getElementById('<%= txtPhone.ClientID %>').value.trim();
            var position = document.getElementById('<%= ddlPosition.ClientID %>').value;
            var email = document.getElementById('<%= txtEmail.ClientID %>').value.trim();

            // Check for required fields
            if (lastName === '' || firstName === '') {
                Swal.fire({
                    icon: 'warning',
                    title: 'Incomplete Form',
                    text: 'Last Name and First Name are required fields.'
                });
                return false;
            }

            // Validate Phone number length (exactly 11 digits)
            if (phone.length !== 11) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Invalid Phone Number',
                    text: 'Phone number must be exactly 11 digits.'
                });
                return false;
            }

            // Validate Position selection
            if (position === '') {
                Swal.fire({
                    icon: 'warning',
                    title: 'Incomplete Form',
                    text: 'Please select a position.'
                });
                return false;
            }

            // Validate Email format or N/A
            var emailPattern = /^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/;
            if (email.toLowerCase() !== 'n/a' && !emailPattern.test(email)) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Invalid Email',
                    text: 'Please enter a valid email address or type "N/A".'
                });
                return false;
            }

            return true;
        }
    </script>
</asp:Content>