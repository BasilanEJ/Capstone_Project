<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ManageInquiry.aspx.cs" Inherits="RRCManagementSystem.ManageInquiry" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .inquiry-header { font-size: 24px; font-weight: bold; margin-bottom: 20px; color: #004085; }
        .inquiry-table { width: 100%; border-collapse: collapse; margin-bottom: 30px; }
        .inquiry-table th, .inquiry-table td { border: 1px solid #dee2e6; padding: 10px; text-align: left; }
        .inquiry-table th { background-color: #e9ecef; color: #333; }
        .btn-primary-sm {
            background-color: #007bff; color: #fff; border: none;
            padding: 6px 12px; font-size: 14px; cursor: pointer;
        }
        .btn-primary-sm:hover { background-color: #0056b3; }
    </style>

    <asp:HiddenField ID="hfSelectedInquiryID" runat="server" />
    <asp:HiddenField ID="hfAssignData" runat="server" />
    <asp:Button ID="btnAssignHidden" runat="server" OnClick="btnAssignHidden_Click" Style="display:none;" />

    <h2 class="inquiry-header">📋 Manage Inquiries</h2>
    <asp:Label ID="lblPermission" runat="server" CssClass="text-danger" />

    <asp:GridView ID="gvInquiries" runat="server" AutoGenerateColumns="False" CssClass="inquiry-table"
        DataKeyNames="InquiryID">
        <Columns>
            <asp:BoundField DataField="InquiryID" HeaderText="Inquiry ID" />
            <asp:BoundField DataField="Email" HeaderText="Client Email" />
            <asp:BoundField DataField="ContactNumber" HeaderText="Contact Number" />
            <asp:BoundField DataField="Message" HeaderText="Message" />
            <asp:BoundField DataField="SubmittedAt" HeaderText="Submitted At" DataFormatString="{0:g}" />
            <asp:TemplateField HeaderText="Action">
                <ItemTemplate>
                    <asp:Button ID="btnAssign" runat="server" Text="Assign Inspector"
                        OnClientClick='<%# "showAssignModal(" + Eval("InquiryID") + "); return false;" %>'
                        CssClass="btn-primary-sm" />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <script>
        function showAssignModal(inquiryId) {
            document.getElementById('<%= hfSelectedInquiryID.ClientID %>').value = inquiryId;

            const now = new Date();
            const pad = (num) => num.toString().padStart(2, '0');
            const yyyy = now.getFullYear(), mm = pad(now.getMonth() + 1), dd = pad(now.getDate()), hh = pad(now.getHours()), min = pad(now.getMinutes());
            const minDateTime = `${yyyy}-${mm}-${dd}T${hh}:${min}`;

            Swal.fire({
                title: 'Assign Inspector',
                width: 800,
                html: `
                    <div style="display: flex; gap: 10px;">
                        <input id="swalName" class="swal2-input" placeholder="Full Name">
                        <input id="swalStreet" class="swal2-input" placeholder="Street & Unit">
                    </div>
                    <div style="display: flex; gap: 10px;">
                        <input id="swalBarangay" class="swal2-input" placeholder="Barangay">
                        <input id="swalCity" class="swal2-input" placeholder="City">
                    </div>
                    <div style="display: flex; gap: 10px;">
                        <input id="swalRegion" class="swal2-input" placeholder="Region">
                        <input id="swalCountry" class="swal2-input" value="Philippines">
                    </div>
                    <label>Schedule:</label>
                    <input type="datetime-local" id="swalSchedule" class="swal2-input" min="${minDateTime}">
                    <label>Inspector:</label>
                    <select id="swalInspector" class="swal2-input"><%= GetInspectorOptions() %></select>
                    <textarea id="swalRemarks" class="swal2-textarea" placeholder="Remarks (optional)"></textarea>
                `,
                showCancelButton: true,
                confirmButtonText: 'Assign',
                preConfirm: () => {
                    const name = document.getElementById('swalName').value.trim();
                    const street = document.getElementById('swalStreet').value.trim();
                    const barangay = document.getElementById('swalBarangay').value.trim();
                    const city = document.getElementById('swalCity').value.trim();
                    const region = document.getElementById('swalRegion').value.trim();
                    const country = document.getElementById('swalCountry').value.trim();
                    const inspector = document.getElementById('swalInspector').value;
                    const schedule = document.getElementById('swalSchedule').value;
                    const remarks = document.getElementById('swalRemarks').value;

                    if (!name || !street || !barangay || !city || !region || !country || !inspector || !schedule) {
                        Swal.showValidationMessage("All fields are required except remarks");
                        return false;
                    }

                    document.getElementById('<%= hfAssignData.ClientID %>').value =
                        `${inspector}|${schedule}|${remarks}|${name}|${street}|${barangay}|${city}|${region}|${country}`;
                    document.getElementById('<%= btnAssignHidden.ClientID %>').click();
                }
            });
        }
    </script>
</asp:Content>
