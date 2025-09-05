<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AddSupplier.aspx.cs" Inherits="RRCManagementSystem.AddSupplier" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        /* Main container styling */
        .container {
            padding: 20px;
            background-color: #f4f4f4;
            min-height: calc(100vh - 100px);
        }

        /* Card styling */
        .card {
            background-color: #fff;
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
            max-width: 700px;
            margin: 0 auto;
            overflow: hidden;
        }

        .card-header {
            background-color: #004085;
            color: #fff;
            padding: 15px 20px;
            font-size: 18px;
            font-weight: bold;
        }

        .card-body {
            padding: 20px;
        }

        /* Form group styling */
        .form-group {
            margin: 15px;
            margin-right: 20px;
        }

        .form-group label {
            display: block;
            font-weight: 600;
            margin-bottom: 8px;
            color: #333;
        }

        /* Uniform form control styling */
        .form-control {
            width: 100%;
            padding: 10px 12px;
            font-size: 14px;
            border: 1px solid #ced4da;
            border-radius: 4px;
            transition: border-color 0.3s, box-shadow 0.3s;
        }

        .form-control:focus {
            border-color: #004085;
            box-shadow: 0 0 5px rgba(0, 64, 133, 0.3);
            outline: none;
        }

        /* Submit button styling */
        .btn-submit {
            background-color: #004085;
            color: #fff;
            border: none;
            padding: 10px 20px;
            border-radius: 4px;
            font-size: 14px;
            cursor: pointer;
            transition: background-color 0.3s ease;
            margin-top: 10px;
        }

        .btn-submit:hover {
            background-color: #003366;
        }

        /* Alert message with icon styling */
        .alert-message {
            display: block;
            margin-top: 15px;
            padding: 10px 15px;
            background-color: #f8d7da;
            color: #721c24;
            border: 1px solid #f5c6cb;
            border-radius: 5px;
            font-size: 14px;
        }

        .alert-message i {
            margin-right: 10px;
            font-size: 16px;
        }

        /* Responsive design */
        @media (max-width: 768px) {
            .card {
                margin: 20px;
            }

            .btn-submit {
                width: 100%;
            }
        }
    </style>

    <!-- FontAwesome for icons -->
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.1/css/all.min.css" rel="stylesheet">

    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <script type="text/javascript">
        function confirmAddSupplier(btn) {
            var isValid = true;

            // Validate Contact Number
            if (!validateContactNumber()) {
                isValid = false;
            }

            // Validate Email
            if (!validateEmail()) {
                isValid = false;
            }

            if (!isValid) {
                // Prevent form submission if validation fails
                return false;
            }

            Swal.fire({
                title: 'Are you sure?',
                text: "Do you want to add this supplier?",
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#004085',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Yes, add it!'
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack(btn.name, '');
                }
            });
            return false; // Prevent normal postback
        }

        function validateContactNumber() {
            var contactNumber = document.getElementById('<%= txtContactNumber.ClientID %>').value;
            var errorMessage = "";
            var regex = /^09\d{8,9}$/;  // Starts with 09 and followed by 8 or 9 digits.

            if (!regex.test(contactNumber)) {
                errorMessage = "<i class='fas fa-exclamation-circle'></i>Contact number must start with '09' and be 9 or 11 digits long.";
            }

            if (errorMessage !== "") {
                document.getElementById('<%= lblMessage.ClientID %>').innerHTML = errorMessage;
                document.getElementById('<%= lblMessage.ClientID %>').style.display = 'block'; // Show the error message
                return false; // Validation failed
            } else {
                document.getElementById('<%= lblMessage.ClientID %>').style.display = 'none'; // Hide the error message
                return true; // Validation passed
            }
        }

        function validateEmail() {
            var email = document.getElementById('<%= txtEmail.ClientID %>').value;
            var errorMessage = "";
            var regex = /^[a-zA-Z0-9._%+-]+@(gmail\.com|yahoo\.com|outlook\.com)$/;

            if (!regex.test(email)) {
                errorMessage = "<i class='fas fa-exclamation-circle'></i>Email must be in the form of 'xxx@gmail.com', 'xxx@yahoo.com', or 'xxx@outlook.com'.";
            }

            if (errorMessage !== "") {
                document.getElementById('<%= lblMessage.ClientID %>').innerHTML = errorMessage;
                document.getElementById('<%= lblMessage.ClientID %>').style.display = 'block'; // Show the error message
                return false; // Validation failed
            } else {
                document.getElementById('<%= lblMessage.ClientID %>').style.display = 'none'; // Hide the error message
                return true; // Validation passed
            }
        }
    </script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mt-4">
        <div class="card">
            <div class="card-header">
                <strong>Add New Supplier</strong>
            </div>
            <div class="card-body">
                <!-- Supplier Name -->
                <div class="form-group">
                    <label for="txtName">Supplier Name *</label>
                    <asp:TextBox ID="txtName" runat="server" CssClass="form-control" placeholder="Enter supplier name" />
                </div>

                <!-- Address -->
                <div class="form-group">
                    <label for="txtAddress">Address *</label>
                    <asp:TextBox ID="txtAddress" runat="server" CssClass="form-control" placeholder="Enter address" />
                </div>

                <!-- Contact Number -->
                <div class="form-group">
                    <label for="txtContactNumber">Contact Number *</label>
                    <asp:TextBox 
                        ID="txtContactNumber" 
                        runat="server" 
                        CssClass="form-control" 
                        placeholder="Enter contact number" 
                        onkeypress="return isNumberKey(event);" 
                        maxlength="11" onblur="validateContactNumber();" />

                    <!-- Required Field Validator -->
                    <asp:RequiredFieldValidator 
                        ID="rfvContactNumber" 
                        runat="server" 
                        ControlToValidate="txtContactNumber" 
                        ForeColor="Red" 
                        ErrorMessage="Contact number is required." />

                    <!-- Regular Expression Validator -->
                    <asp:RegularExpressionValidator 
                        ID="revContactNumber" 
                        runat="server" 
                        ControlToValidate="txtContactNumber" 
                        ForeColor="Red" 
                        ValidationExpression="^09\d{8,9}$" 
                        ErrorMessage="Contact number must start with '09' and be 9 or 11 digits long." Visible="false" />
                </div>

                <script type="text/javascript">
                    function isNumberKey(evt) {
                        var charCode = (evt.which) ? evt.which : evt.keyCode;
                        // Only allow numbers (48-57) and backspace (8)
                        return (charCode >= 48 && charCode <= 57 || charCode === 8);
                    }
                </script>

                <!-- Email -->
                <div class="form-group">
                    <label for="txtEmail">Email</label>
                    <asp:TextBox 
                        ID="txtEmail" 
                        runat="server" 
                        CssClass="form-control" 
                        placeholder="Enter email" onblur="validateEmail();" />
                    
                    <!-- Required Field Validator -->
                    <asp:RequiredFieldValidator 
                        ID="rfvEmail" 
                        runat="server" 
                        ControlToValidate="txtEmail" 
                        ForeColor="Red" 
                        ErrorMessage="Email is required." />

                    <!-- Regular Expression Validator -->
                    <asp:RegularExpressionValidator 
                        ID="revEmail" 
                        runat="server" 
                        ControlToValidate="txtEmail" 
                        ForeColor="Red" 
                        ValidationExpression="^[a-zA-Z0-9._%+-]+@(gmail\.com|yahoo\.com|outlook\.com)$" 
                        ErrorMessage="Email must be in the form of 'xxx@gmail.com', 'xxx@yahoo.com', or 'xxx@outlook.com.'" Visible="false" />
                </div>

                <!-- Company Name -->
                <div class="form-group">
                    <label for="txtCompanyName">Company Name</label>
                    <asp:TextBox ID="txtCompanyName" runat="server" CssClass="form-control" placeholder="Enter company name (optional)" />
                </div>

                <!-- Business Type -->
                <div class="form-group">
                    <label for="ddlBusinessType">Business Type</label>
                    <asp:DropDownList ID="ddlBusinessType" runat="server" CssClass="form-control">
                        <asp:ListItem Text="Select Business Type" Value="" />
                        <asp:ListItem Text="Pest Control Products" Value="Pest Control Products" />
                        <asp:ListItem Text="PPE Materials" Value="PPE Materials" />
                        <asp:ListItem Text="Chemicals" Value="Chemicals" />
                        <asp:ListItem Text="Equipment" Value="Equipment" />
                        <asp:ListItem Text="Others" Value="Others" />
                    </asp:DropDownList>
                </div>

                <!-- Status -->
                <div class="form-group">
                    <label for="ddlStatus">Status</label>
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-control">
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
                    CssClass="btn-submit"
                    UseSubmitBehavior="false"
                    OnClientClick="return confirmAddSupplier(this);" 
                    OnClick="btnSubmit_Click" />

                <asp:Label ID="lblMessage" runat="server" CssClass="alert-message" style="display:none;"></asp:Label>
            </div>
        </div>
    </div>
</asp:Content>
