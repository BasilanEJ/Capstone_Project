            <%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ManagePayment.aspx.cs" Inherits="RRCManagementSystem.ManagePayment" %>

            <asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
                <style>
                    .payment-form {
                        max-width: 700px;
                        margin: 0 auto;
                        background: #fff;
                        padding: 30px;
                        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
                        border-radius: 10px;
                    }
                    .payment-form h2 { text-align: center; margin-bottom: 25px; color: #004085; }
                    .form-group { margin-bottom: 20px; }
                    .form-label { font-weight: 600; display: block; margin-bottom: 5px; }
                    .form-control { width: 100%; padding: 10px; border-radius: 6px; border: 1px solid #ccc; }
                    .btn-submit { background-color: #004085; color: #fff; padding: 10px 25px; border: none; border-radius: 6px; cursor: pointer; font-weight: bold; margin: 5px; }
                    .btn-submit:hover { background-color: #002f6c; }
                    .message { margin-top: 15px; font-weight: bold; text-align: center; }
                    .message.success { color: green; }
                    .message.error { color: red; }
                </style>

                <div class="payment-form">
                    <h2>Adjust Client Payment</h2>

                    <asp:Label ID="lblMessage" runat="server" CssClass="message" />

                    <!-- Remember which booking we computed balance for -->
                    <asp:HiddenField ID="hfBookingId" runat="server" />

                    <div class="form-group">
                        <label class="form-label">Client</label>
                        <asp:DropDownList ID="ddlClients" runat="server" CssClass="form-control"
                                          AutoPostBack="true"
                                          OnSelectedIndexChanged="ddlClients_SelectedIndexChanged" />
                        <!-- optional validator -->
                        <asp:RequiredFieldValidator ID="rfvClient" runat="server"
                            ControlToValidate="ddlClients"
                            InitialValue=""
                            CssClass="text-danger small"
                            Display="Dynamic"
                            ErrorMessage="Please select a client." />
                    </div>

                        <div class="form-group">
                    <label class="form-label">Remaining Balance</label>
                    <asp:TextBox ID="txtRemainingBalance" runat="server" CssClass="form-control" ReadOnly="true" />
                </div>  

                    <div class="form-group">
                        <label class="form-label">Payment Method</label>
                        <asp:DropDownList ID="ddlPaymentMethod" runat="server" CssClass="form-control">
                            <asp:ListItem Text="-- Select Method --" Value="" />
                            <asp:ListItem Text="Cash" Value="Cash" />
                            <asp:ListItem Text="Bank Transfer" Value="Bank Transfer" />
                            <asp:ListItem Text="Online Payment" Value="Online Payment" />
                        </asp:DropDownList>
                        <!-- optional validator -->
                        <asp:RequiredFieldValidator ID="rfvMethod" runat="server"
                            ControlToValidate="ddlPaymentMethod"
                            InitialValue=""
                            CssClass="text-danger small"
                            Display="Dynamic"
                            ErrorMessage="Please select a payment method." />
                    </div>

    

                    <div class="form-group">
                        <label class="form-label">Amount Paid</label>
                        <asp:TextBox ID="txtNewBalance" runat="server" CssClass="form-control" TextMode="Number" />
                    </div>

                    <div class="form-group">
                        <label class="form-label">Remarks</label>
                        <asp:TextBox ID="txtRemarks" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" />
                    </div>

                    <div class="form-group">
                        <label class="form-label">Upload Receipt <span style="color:red;">*</span></label>
                        <asp:FileUpload ID="fuReceipt" runat="server" CssClass="form-control" onchange="previewReceipt(this)" />
                    </div>

                    <div id="previewContainer" style="margin-top: 20px; text-align: center;">
                        <img id="receiptPreview" src="#" alt="Receipt Preview"
                             style="display:none; max-width: 300px; border: 1px solid #ccc; border-radius: 8px; padding: 5px; cursor: pointer;"
                             onclick="openFullImage()" />
                    </div>

                    <div style="text-align:center; margin-top:20px;">
                        <asp:Button ID="btnSaveReal" runat="server" Text="Save Changes (Hidden)" CssClass="btn-submit"
                                    OnClick="btnSave_Click" Style="display:none;" />
                        <button type="button" class="btn-submit" onclick="showConfirmSwal()">Save Changes</button>

                        <asp:Button ID="btnPrintReceipt" runat="server" Text="🖸️ Print Receipt" CssClass="btn-submit" Visible="false" OnClick="btnPrintReceipt_Click" />
                        <asp:Button ID="btnDownloadReceipt" runat="server" Text="💾 Download Decrypted Receipt" CssClass="btn-submit" Visible="false" OnClick="btnDownloadReceipt_Click" />
                    </div>
                </div>

                <!-- SweetAlert2 -->
                <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

                <script type="text/javascript">
                    function previewReceipt(input) {
                        if (input.files && input.files[0]) {
                            var file = input.files[0];
                            var allowedTypes = ["image/jpeg", "image/png"];

                            if (!allowedTypes.includes(file.type)) {
                                Swal.fire("❌ Invalid File", "Only JPG and PNG images are allowed.", "error");
                                input.value = "";
                                document.getElementById('receiptPreview').style.display = 'none';
                                return;
                            }

                            var reader = new FileReader();
                            reader.onload = function (e) {
                                var preview = document.getElementById('receiptPreview');
                                preview.src = e.target.result;
                                preview.style.display = 'block';
                            };
                            reader.readAsDataURL(file);
                        }
                    }

                    function openFullImage() {
                        // If you want a modal, implement it here
                        var img = document.getElementById('receiptPreview');
                        if (img && img.src && img.style.display !== 'none') {
                            window.open(img.src, '_blank');
                        }
                    }

                    function showConfirmSwal() {
                        Swal.fire({
                            title: 'Are you sure?',
                            text: "Confirm saving this payment.",
                            icon: 'warning',
                            showCancelButton: true,
                            confirmButtonColor: '#004085',
                            cancelButtonColor: '#d33',
                            confirmButtonText: 'Yes, save it!',
                            cancelButtonText: 'Cancel'
                        }).then((result) => {
                            if (result.isConfirmed) {
                                document.getElementById('<%= btnSaveReal.ClientID %>').click();
                            }
                        });
                    }
                </script>
            </asp:Content>
