<%@ Page Title="Add Equipment" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AddEquipment.aspx.cs" Inherits="RRCManagementSystem.AddEquipment" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Tailwind CSS CDN for styling -->
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
    <style>
        /* Image Modal Styles */
        .image-modal-overlay {
            background-color: rgba(0, 0, 0, 0.85);
            transition: opacity 0.3s ease-in-out;
        }

        .image-modal-close-btn {
            position: absolute;
            top: 15px;
            right: 15px;
            background-color: rgba(255, 255, 255, 0.9);
            border: none;
            cursor: pointer;
            color: #1f2937;
            font-size: 1.5rem;
            line-height: 1;
            padding: 0.5rem;
            border-radius: 50%;
            box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
            transition: background-color 0.2s, transform 0.2s;
            z-index: 60;
        }

        .image-modal-close-btn:hover {
            background-color: #f87171;
            color: white;
            transform: scale(1.05);
        }
    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <!-- Main content container with modern Tailwind styling -->
    <div class="mx-auto max-w-md my-8 p-6 bg-white rounded-xl shadow-lg">
        <div class="rounded-xl border border-gray-200 shadow-md overflow-hidden">
            <!-- Card Header -->
            <div class="bg-blue-600 text-white p-4 text-center">
                <h2 class="text-xl font-bold"><i class="fas fa-tools mr-2"></i> Add New Equipment</h2>
            </div>
            
            <!-- Card Body -->
            <div class="p-6">
                <asp:Label ID="lblMessage" runat="server" CssClass="block text-center text-red-500 font-semibold mb-4"></asp:Label>

                <!-- Equipment ID -->
                <div class="mb-4">
                    <label for="txtEquipmentID" class="block font-semibold text-gray-700 mb-2">Equipment ID:</label>
                    <asp:TextBox ID="txtEquipmentID" runat="server" CssClass="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors" placeholder="Enter Equipment ID" required></asp:TextBox>
                </div>

                <!-- Equipment Name -->
                <div class="mb-4">
                    <label for="txtEquipmentName" class="block font-semibold text-gray-700 mb-2">Equipment Name:</label>
                    <asp:TextBox ID="txtEquipmentName" runat="server" CssClass="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors" placeholder="Enter Equipment Name" required></asp:TextBox>
                </div>

                <!-- Status -->
                <div class="mb-4">
                    <label for="ddlStatus" class="block font-semibold text-gray-700 mb-2">Status:</label>
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors">
                        <asp:ListItem Text="Select Status" Value="" />
                        <asp:ListItem Text="Available" Value="Available" />
                        <asp:ListItem Text="Unavailable" Value="Unavailable" />
                        <asp:ListItem Text="Under Maintenance" Value="Under Maintenance" />
                    </asp:DropDownList>
                </div>

                <!-- Equipment Image -->
                <div class="mb-4">
                    <label for="fuEquipmentImage" class="block font-semibold text-gray-700 mb-2">Equipment Image (JPG, JPEG, PNG only):</label>
                    <asp:FileUpload ID="fuEquipmentImage" runat="server" accept="image/*" onchange="validateImage(this); previewImage(event);" CssClass="block w-full text-sm text-gray-500
                                                                                                        file:mr-4 file:py-2 file:px-4
                                                                                                        file:rounded-full file:border-0
                                                                                                        file:text-sm file:font-semibold
                                                                                                        file:bg-blue-50 file:text-blue-700
                                                                                                        hover:file:bg-blue-100" />
                    
                    <!-- Image Preview with Click to Enlarge -->
                    <div class="mt-4 relative">
                        <img id="imagePreview" src="#" alt="Equipment Image Preview" 
                             class="hidden w-32 h-32 object-cover rounded-lg border border-gray-300 shadow-sm cursor-pointer hover:opacity-80 transition-opacity"
                             onclick="enlargePreview()" />
                        <button type="button" id="btnRemoveImage" onclick="removeImage()" 
                                class="hidden absolute top-0 right-0 bg-red-500 text-white rounded-full p-1 hover:bg-red-600 transition-colors"
                                style="transform: translate(50%, -50%);">
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                <path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd" />
                            </svg>
                        </button>
                    </div>
                </div>

                <!-- Buttons -->
                <div class="flex justify-center space-x-4">
                    <asp:Button ID="btnSubmit" runat="server" Text="Add Equipment"
                        CssClass="w-full md:w-1/2 bg-blue-600 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded-lg transition-colors duration-200 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
                        UseSubmitBehavior="false" OnClientClick="return showConfirmAdd();" OnClick="btnSubmit_Click" />

                    <asp:Button ID="btnCancel" runat="server" Text="Cancel"
                        CssClass="w-full md:w-1/2 bg-gray-500 hover:bg-gray-600 text-white font-bold py-2 px-4 rounded-lg transition-colors duration-200 focus:outline-none focus:ring-2 focus:ring-gray-400 focus:ring-offset-2"
                        UseSubmitBehavior="false" OnClientClick="return showConfirmCancel();" />
                </div>
            </div>
        </div>
    </div>

    <!-- Image Modal for Enlarged Preview -->
    <div id="imageModal" class="fixed inset-0 z-50 flex items-center justify-center image-modal-overlay opacity-0 pointer-events-none" 
         onclick="if (event.target.id === 'imageModal') hideImageModal()">
        
        <div class="relative max-w-4xl max-h-[90vh]" onclick="event.stopPropagation()">
            <button type="button" onclick="hideImageModal()" class="image-modal-close-btn">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                </svg>
            </button>
            
            <img id="modalImage" class="max-w-full max-h-[90vh] mx-auto rounded-lg" alt="Enlarged Equipment Image" />
        </div>
    </div>

    <!-- JAVASCRIPT -->
    <script>
        function previewImage(event) {
            var file = event.target.files[0];
            if (file) {
                var reader = new FileReader();
                reader.onload = function (e) {
                    var imgPreview = document.getElementById("imagePreview");
                    var btnRemove = document.getElementById("btnRemoveImage");
                    imgPreview.src = e.target.result;
                    imgPreview.classList.remove("hidden");
                    imgPreview.classList.add("block");
                    btnRemove.classList.remove("hidden");
                    btnRemove.classList.add("block");
                };
                reader.readAsDataURL(file);
            }
        }

        function removeImage() {
            var fileInput = document.getElementById('<%= fuEquipmentImage.ClientID %>');
            var imgPreview = document.getElementById("imagePreview");
            var btnRemove = document.getElementById("btnRemoveImage");

            fileInput.value = '';
            imgPreview.src = '#';
            imgPreview.classList.add("hidden");
            imgPreview.classList.remove("block");
            btnRemove.classList.add("hidden");
            btnRemove.classList.remove("block");
        }

        function enlargePreview() {
            var imgSrc = document.getElementById("imagePreview").src;
            if (imgSrc && imgSrc !== '#' && imgSrc !== window.location.href + '#') {
                document.getElementById('modalImage').src = imgSrc;
                document.getElementById('imageModal').classList.remove('opacity-0', 'pointer-events-none');
                document.body.style.overflow = 'hidden';
            }
        }

        function hideImageModal() {
            document.getElementById('imageModal').classList.add('opacity-0', 'pointer-events-none');
            document.body.style.overflow = '';
        }

        // Close modal on Escape key
        document.addEventListener('keydown', function (event) {
            const imageModal = document.getElementById('imageModal');
            if (event.key === 'Escape' && !imageModal.classList.contains('opacity-0')) {
                hideImageModal();
            }
        });

        function validateImage(input) {
            var filePath = input.value;
            var allowedExtensions = /(\.jpg|\.jpeg|\.png)$/i;
            if (!allowedExtensions.exec(filePath)) {
                Swal.fire({
                    icon: 'error',
                    title: 'Invalid File Type',
                    text: 'Only JPG, JPEG, and PNG files are allowed.'
                });
                input.value = '';
                removeImage();
                return false;
            }
        }

        function showConfirmAdd() {
            event.preventDefault();
            Swal.fire({
                title: 'Add Equipment?',
                text: 'Are you sure you want to add this equipment?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#007bff',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes, add it'
            }).then((result) => {
                if (result.isConfirmed) {
                    // Check for form validity before postback
                    var form = document.querySelector('form');
                    if (form.checkValidity()) {
                        document.getElementById('<%= btnSubmit.ClientID %>').disabled = true;
                        __doPostBack('<%= btnSubmit.UniqueID %>', '');
                    } else {
                        // Display native browser validation messages
                        form.reportValidity();
                    }
                }
            });
            return false;
        }

        function showConfirmCancel() {
            event.preventDefault();
            Swal.fire({
                title: 'Cancel?',
                text: 'Are you sure you want to cancel? Unsaved data will be lost.',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#6c757d',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes, cancel'
            }).then((result) => {
                if (result.isConfirmed) {
                    window.location.href = 'ViewEquipment.aspx'; // Redirect to equipment list
                }
            });
            return false;
        }
    </script>
</asp:Content>