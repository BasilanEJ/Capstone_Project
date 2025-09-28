<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AddEmployees.aspx.cs" Inherits="RRCManagementSystem.AddEmployees" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <style>
        /* Optional: Add a transition for a smoother hover effect */
        .file-input-label {
            transition: color 0.3s ease;
        }

        .file-input-label:hover {
            color: #1d4ed8; /* blue-700 */
        }
    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto py-10 px-4 flex justify-center items-start">
        <div class="w-full max-w-2xl bg-white p-8 rounded-2xl shadow-xl border border-gray-200">
            <h2 class="text-center text-3xl font-extrabold text-blue-800 mb-6">Add New Employee</h2>

            <div class="mb-4">
                <asp:Label ID="lblMessage" runat="server" CssClass="block text-center text-sm font-medium text-red-600" />
            </div>

            <div class="mb-6 flex flex-col items-center">
                <label for="<%= fuProfilePicture.ClientID %>" class="text-sm font-semibold text-gray-700 mb-2 file-input-label cursor-pointer">Profile Picture (JPG, JPEG, PNG only)</label>
                <img id="imagePreview" alt="Profile Preview" class="h-32 w-32 object-cover rounded-full mb-4 border-2 border-gray-300 hidden shadow-md" />
                <asp:FileUpload ID="fuProfilePicture" runat="server" CssClass="block w-full text-sm text-gray-700 file:mr-4 file:py-2 file:px-4 file:rounded-lg file:border-0 file:text-sm file:font-semibold file:bg-blue-500 file:text-white hover:file:bg-blue-600 transition-colors" accept="image/*" onchange="validateFile(); previewImage(event);" />
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
                  <asp:TextBox ID="txtPhone" runat="server"
    CssClass="block w-full px-4 py-2 text-gray-700 bg-gray-50 border border-gray-300 rounded-lg focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-colors"
    placeholder="Enter 11-digit Phone Number"
    required
    maxlength="11"
    pattern="^09\d{9}$"
    title="Phone number must start with 09 and be 11 digits long."
    oninput="this.value=this.value.replace(/[^0-9]/g,'')">
</asp:TextBox>


                    <span id="phoneError" class="block text-xs text-red-500 mt-1 hidden"></span>
                </div>
            </div>
            <div class="mb-6">
                <label for="<%= ddlPosition.ClientID %>" class="block text-sm font-semibold text-gray-700 mb-2">Position</label>
                <asp:DropDownList ID="ddlPosition" runat="server" CssClass="block w-full px-4 py-2 bg-white text-gray-900 border border-gray-300 rounded-lg focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-colors">
                    <asp:ListItem Text="IT" Value="IT" />
                    <asp:ListItem Text="Technician" Value="Technician" />
                </asp:DropDownList>
            </div>

            <div class="text-center">
                <asp:Button ID="btnSubmit" runat="server" Text="Add Employee"
                    CssClass="w-full py-3 px-4 bg-blue-600 text-white font-bold rounded-lg shadow-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors"
                    OnClick="btnSubmit_Click" />
                
                <asp:Button ID="btnHiddenSubmit" runat="server" style="display: none;" OnClick="btnSubmit_Click" />
            </div>
        </div>
    </div>

    <script type="text/javascript">
        // Profile Picture Validation
        function validateFile() {
            var fileInput = document.getElementById('<%= fuProfilePicture.ClientID %>');
            var filePath = fileInput.value;
            var allowedExtensions = /(\.jpg|\.jpeg|\.png)$/i;
            if (!allowedExtensions.exec(filePath)) {
                Swal.fire({
                    icon: 'error',
                    title: 'Invalid File Type',
                    text: 'Only JPG, JPEG, and PNG files are allowed.'
                });
                fileInput.value = '';
                document.getElementById('imagePreview').style.display = 'none';
                return false;
            }
        }

        // Image Preview
        function previewImage(event) {
            var file = event.target.files[0];
            if (file) {
                var reader = new FileReader();
                reader.onload = function (e) {
                    var imgPreview = document.getElementById("imagePreview");
                    imgPreview.src = e.target.result;
                    imgPreview.style.display = "block";
                };
                reader.readAsDataURL(file);
            }
        }

        // Client-side form validation
        function validateForm() {
            var lastName = document.getElementById('<%= txtLastName.ClientID %>').value;
            var firstName = document.getElementById('<%= txtFirstName.ClientID %>').value;
            var email = document.getElementById('<%= txtEmail.ClientID %>').value;
            var phone = document.getElementById('<%= txtPhone.ClientID %>').value;
            var position = document.getElementById('<%= ddlPosition.ClientID %>').value;

            if (lastName.trim() === "" || firstName.trim() === "") {
                Swal.fire({ icon: 'warning', title: 'Incomplete Form', text: 'Last Name and First Name are required fields.' });
                return false;
            }

            var phonePattern = /^\d{11}$/;
            var phoneError = document.getElementById("phoneError");
            if (!phonePattern.test(phone)) {
                phoneError.textContent = 'Phone number must be exactly 11 digits.';
                phoneError.style.display = 'block';
                return false;
            } else {
                phoneError.style.display = 'none';
            }
            
            var emailInput = document.getElementById('<%= txtEmail.ClientID %>').value.trim();
            var emailPattern = /^\w+([\.-]?\w+)*@(gmail|yahoo|outlook)\.com$/i;

            if (emailInput.toLowerCase() !== 'n/a' && !emailPattern.test(emailInput)) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Invalid Email',
                    text: 'Please enter a valid Gmail, Yahoo, or Outlook email address, or type "N/A".'
                });
                return false;
            }

            if (position === "") {
                Swal.fire({ icon: 'warning', title: 'Incomplete Form', text: 'Please select an employee position.' });
                return false;
            }

            return true;
        }

        // We now handle the button click with a client-side event listener.
        document.addEventListener('DOMContentLoaded', function () {
            var mainButton = document.getElementById('<%= btnSubmit.ClientID %>');
            if (mainButton) {
                mainButton.addEventListener('click', function (e) {
                    // Prevent the default postback triggered by the ASP.NET button.
                    e.preventDefault();

                    // Run client-side validation first.
                    if (validateForm()) {
                        // If validation passes, show the SweetAlert confirmation.
                        Swal.fire({
                            title: 'Add Employee?',
                            text: 'Are you sure you want to add this employee?',
                            icon: 'question',
                            showCancelButton: true,
                            confirmButtonColor: '#2563eb', // Tailwind's blue-600
                            cancelButtonColor: '#ef4444', // Tailwind's red-500
                            confirmButtonText: 'Yes, add it!'
                        }).then((result) => {
                            if (result.isConfirmed) {
                                // If confirmed, manually trigger the postback.
                                // We use a hidden button to do this cleanly.
                                document.getElementById('<%= btnHiddenSubmit.ClientID %>').click();
                            }
                        });
                    }
                });
            }
        });
    </script>
</asp:Content>