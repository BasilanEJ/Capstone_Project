<%@ Page Title="Create Inquiry"
    Language="C#"
    MasterPageFile="~/Admin.Master"
    AutoEventWireup="true"
    CodeBehind="CreateInquiry.aspx.cs"
    Inherits="RRCManagementSystem.CreateInquiry" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        /* Page */
        .page-title{
            text-align:center;font-size:28px;font-weight:700;color:#0f172a;
            margin:30px 0 18px
        }
        .form-card{
            background:#fff;border:1px solid #e5e7eb;border-radius:14px;
            padding:28px 32px;max-width:940px;margin:0 auto 40px;
            box-shadow:0 8px 24px rgba(2,6,23,.06)
        }
        .section-title{font-weight:700;color:#0d6efd;margin:2px 0 8px}
        .divider{height:1px;background:#eef2f7;margin:8px 0 16px}

        /* Grid & fields */
        .form-grid{display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:18px 24px}
        .full{grid-column:1/-1}
        .form-label{font-weight:600;color:#334155;margin-bottom:6px;display:block}
        .form-control,.form-textarea,.form-file{
            width:100%;padding:10px 12px;border:1px solid #d1d9e6;border-radius:10px;font-size:14px
        }
        .form-textarea{min-height:120px;resize:vertical}
        .help{color:#6b7280;font-size:12px;margin-top:4px}

        /* Buttons */
        .btn-row{display:flex;gap:12px;justify-content:flex-end;margin-top:16px}
        .btn{display:inline-block;padding:10px 16px;border-radius:12px;border:none;cursor:pointer;font-weight:700;font-size:14px}
        .btn-primary{background:#0d6efd;color:#fff}.btn-primary:hover{background:#0b5ed7}
        .btn-secondary{background:#6c757d;color:#fff}.btn-secondary:hover{background:#5a6268}

        /* Validators */
        .text-danger{color:#dc3545}
        .vs-hidden{display:none} /* Keep ValidationSummary for consistency but hidden (SweetAlert shows messages) */
    </style>

    <h2 class="page-title">📝 Create Inquiry</h2>

    <asp:Label ID="lblPermission" runat="server" CssClass="text-danger" />
    <asp:ValidationSummary ID="vs" runat="server" CssClass="vs-hidden" ValidationGroup="inq" />

    <div class="form-card">
     
        <div class="section-title">Contact Info</div>
        <div class="divider"></div>

        <div class="form-grid">
            <div>
                <label for="<%= txtEmail.ClientID %>" class="form-label">Email <span class="text-danger">*</span></label>
                <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" />
                <asp:RequiredFieldValidator runat="server"
                    ControlToValidate="txtEmail"
                    ValidationGroup="inq"
                    ErrorMessage="Email is required"
                    CssClass="text-danger" Display="Dynamic" />
           <asp:RegularExpressionValidator ID="revEmail" runat="server"
    ControlToValidate="txtEmail"
    ErrorMessage="Please enter a valid Gmail, Yahoo, Outlook, iCloud, or school/government email address."
    ForeColor="Red" Display="Dynamic"
    ValidationGroup="inq"
    ValidationExpression="^[A-Za-z0-9._%+\-]+@((?:gmail|yahoo|ymail|rocketmail|outlook|hotmail|live|msn|icloud|me|mac|protonmail|proton|zoho|zohomail)\.com|(?:[A-Za-z0-9-]+\.)+edu\.ph|(?:[A-Za-z0-9-]+\.)+gov\.ph)$" />

            </div>

            <div>
                <label for="<%= txtContact.ClientID %>" class="form-label">Contact Number <span class="text-danger">*</span></label>
                <asp:TextBox ID="txtContact" runat="server" CssClass="form-control" MaxLength="11" />
                <asp:RequiredFieldValidator runat="server"
                    ControlToValidate="txtContact"
                    ValidationGroup="inq"
                    ErrorMessage="Contact Number is required"
                    CssClass="text-danger" Display="Dynamic" />
                <asp:RegularExpressionValidator runat="server"
                    ControlToValidate="txtContact"
                    ValidationGroup="inq"
                    ValidationExpression="^\d{11}$"
                    ErrorMessage="Contact Number must be exactly 11 digits"
                    CssClass="text-danger" Display="Dynamic" />
                <div class="help">Format: 11 digits (e.g., 09xxxxxxxxx)</div>
            </div>

            <div class="full">
    <label for="<%= txtMessage.ClientID %>" class="form-label">Message</label>
    <asp:TextBox ID="txtMessage" runat="server" CssClass="form-control" MaxLength="255" />
    <div class="help">Optional — up to 255 characters</div>
</div>

        </div>

    
        <div class="section-title" style="margin-top:14px;">Optional Info</div>
        <div class="divider"></div>

        <div class="form-grid">
            <div>
                <label for="<%= txtFirstName.ClientID %>" class="form-label">First Name</label>
                <asp:TextBox ID="txtFirstName" runat="server" CssClass="form-control" />
                <asp:RegularExpressionValidator runat="server"
                    ControlToValidate="txtFirstName"
                    ValidationGroup="inq"
                    ValidationExpression="^[A-Za-z\s\-\.'’]*$"
                    ErrorMessage="First Name cannot contain numbers or symbols"
                    CssClass="text-danger" Display="Dynamic" />
            </div>

            <div>
                <label for="<%= txtMiddleName.ClientID %>" class="form-label">Middle Name</label>
                <asp:TextBox ID="txtMiddleName" runat="server" CssClass="form-control" />
                <asp:RegularExpressionValidator runat="server"
                    ControlToValidate="txtMiddleName"
                    ValidationGroup="inq"
                    ValidationExpression="^[A-Za-z\s\-\.'’]*$"
                    ErrorMessage="Middle Name cannot contain numbers or symbols"
                    CssClass="text-danger" Display="Dynamic" />
            </div>

            <div>
                <label for="<%= txtLastName.ClientID %>" class="form-label">Last Name</label>
                <asp:TextBox ID="txtLastName" runat="server" CssClass="form-control" />
                <asp:RegularExpressionValidator runat="server"
                    ControlToValidate="txtLastName"
                    ValidationGroup="inq"
                    ValidationExpression="^[A-Za-z\s\-\.'’]*$"
                    ErrorMessage="Last Name cannot contain numbers or symbols"
                    CssClass="text-danger" Display="Dynamic" />
            </div>

            <div>
                <label for="<%= txtCountry.ClientID %>" class="form-label">Country</label>
                <asp:TextBox ID="txtCountry" runat="server" CssClass="form-control" Text="Philippines" />
            </div>

            <div>
                <label for="<%= txtRegion.ClientID %>" class="form-label">Region</label>
                <asp:TextBox ID="txtRegion" runat="server" CssClass="form-control" />
            </div>

            <div>
                <label for="<%= txtCity.ClientID %>" class="form-label">City</label>
                <asp:TextBox ID="txtCity" runat="server" CssClass="form-control" />
            </div>

            <div>
                <label for="<%= txtBarangay.ClientID %>" class="form-label">Barangay</label>
                <asp:TextBox ID="txtBarangay" runat="server" CssClass="form-control" />
            </div>

            <div>
                <label for="<%= txtStreet.ClientID %>" class="form-label">Street &amp; Unit</label>
                <asp:TextBox ID="txtStreet" runat="server" CssClass="form-control" />
            </div>

            <div class="full">
                <label for="<%= txtLandmark.ClientID %>" class="form-label">Landmark</label>
                <asp:TextBox ID="txtLandmark" runat="server" CssClass="form-control" />
            </div>

            <div class="full">
                <label for="<%= fuPhoto.ClientID %>" class="form-label">Photo (Optional)</label>
                <asp:FileUpload ID="fuPhoto" runat="server" CssClass="form-file" />
                <div class="help">Allowed: .jpg, .jpeg, .png — Max: 5 MB</div>
            </div>
        </div>

      
        <div class="btn-row">
            <a href="AllInquiry.aspx" class="btn btn-secondary">Back to All Inquiries</a>
            <asp:Button ID="btnSave" runat="server" CssClass="btn btn-primary" Text="Save Inquiry"
                OnClick="btnSave_Click"
                ValidationGroup="inq"
                UseSubmitBehavior="false"
                OnClientClick="return validateAndConfirm();" />
        </div>
    </div>

    <!-- Scripts -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
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
            // Trigger WebForms validators for this group
            if (typeof (Page_ClientValidate) === 'function') {
                Page_ClientValidate('inq');
                if (!Page_IsValid) {
                    // Friendly prompt
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

            // Extra guard (trim) — should rarely fire since validators already ran
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

            // Confirm submit
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

            return false; // always stop default submit; we post back from the confirm
        }
    </script>
</asp:Content>
