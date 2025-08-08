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
            DataKeyNames="InquiryID" OnRowDataBound="gvInquiries_RowDataBound">
            <Columns>
                <asp:BoundField DataField="InquiryID" HeaderText="Inquiry ID" />
                <asp:BoundField DataField="Email" HeaderText="Client Email" />
                <asp:BoundField DataField="ContactNumber" HeaderText="Contact Number" />
                <asp:BoundField DataField="Message" HeaderText="Message" />
                <asp:TemplateField HeaderText="Photo">
                    <ItemTemplate>
                        <asp:Image runat="server" ID="imgPhoto" ImageUrl='<%# ResolveUrl(Eval("PhotoPath").ToString()) %>' 
    Width="60" Visible='<%# !string.IsNullOrEmpty(Eval("PhotoPath").ToString()) %>' AlternateText="Pest Photo" />
<%# string.IsNullOrEmpty(Eval("PhotoPath").ToString()) ? "No Photo" : "" %>

                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="SubmittedAt" HeaderText="Submitted At" DataFormatString="{0:g}" />
                <asp:TemplateField HeaderText="Action">
                    <ItemTemplate>
                        <asp:Button ID="btnAssign" runat="server" Text="Assign Inspector"
                            CssClass="btn-primary-sm"
                            CommandName="Assign"
                            CommandArgument='<%# Eval("InquiryID") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>

        <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
       <script type="text/javascript">
           function showAssignModal(inquiryId) {
               document.getElementById('<%= hfSelectedInquiryID.ClientID %>').value = inquiryId;

        const now = new Date();
        const pad = (num) => num.toString().padStart(2, '0');
        const yyyy = now.getFullYear(), mm = pad(now.getMonth() + 1), dd = pad(now.getDate()), hh = pad(now.getHours()), min = pad(now.getMinutes());
        const minDateTime = `${yyyy}-${mm}-${dd}T${hh}:${min}`;

        Swal.fire({
            title: '🛠️ Assign Inspector',
            width: 800,
            html: `
            <div style="display: flex; gap: 10px;">
                <div style="flex:1;">
                    <label>First Name</label>
                    <input id="swalFirstName" class="swal2-input" placeholder="First Name">
                </div>
                <div style="flex:1;">
                    <label>Last Name</label>
                    <input id="swalLastName" class="swal2-input" placeholder="Last Name">
                </div>
            </div>

            <div style="display: flex; gap: 10px;">
                <div style="flex:1;">
                    <label>Middle Name <span style="color:gray;">(optional)</span></label>
                    <input id="swalMiddleName" class="swal2-input" placeholder="Middle Name">
                </div>
                <div style="flex:1;">
                    <label>Country</label>
                    <input id="swalCountry" class="swal2-input" value="Philippines">
                </div>
            </div>

            <div style="display: flex; gap: 10px;">
                <div style="flex:1;">
                    <label>Region</label>
                    <input id="swalRegion" class="swal2-input" placeholder="Region">
                </div>
                <div style="flex:1;">
                    <label>City</label>
                    <input id="swalCity" class="swal2-input" placeholder="City">
                </div>
            </div>

            <div style="display: flex; gap: 10px;">
                <div style="flex:1;">
                    <label>Barangay</label>
                    <input id="swalBarangay" class="swal2-input" placeholder="Barangay">
                </div>
                <div style="flex:1;">
                    <label>Street & Unit</label>
                    <input id="swalStreet" class="swal2-input" placeholder="Street & Unit">
                </div>
            </div>

            <div style="display: flex; gap: 10px;">
                <div style="flex:1;">
                    <label>Landmark <span style="color:gray;">(optional)</span></label>
                    <input id="swalLandmark" class="swal2-input" placeholder="Landmark">
                </div>
                <div style="flex:1;">
                    <label>Schedule</label>
                    <input type="datetime-local" id="swalSchedule" class="swal2-input" min="${minDateTime}">
                </div>
            </div>

<div style="margin-top: 20px; display: flex; flex-direction: column; align-items: center;">
    <label for="swalInspector" style="margin-bottom: 5px; font-weight: 600;">Inspector</label>
    <select id="swalInspector" class="swal2-input" style="font-size: 16px; width: 500px; height: 45px; padding: 8px;">
        <%= GetInspectorOptions() %>
    </select>
</div>



          <div style="margin-top: 20px; display: flex; flex-direction: column; align-items: center;">
    <label for="swalRemarks" style="margin-bottom: 5px; font-weight: 600;">Remarks</label>
    <textarea id="swalRemarks" class="swal2-textarea" placeholder="Remarks (optional)"
              style="width: 600px; font-size: 16px; padding: 8px;"></textarea>
</div>

        `,
            showCancelButton: true,
            confirmButtonText: 'Assign',
            confirmButtonColor: '#004085',
            cancelButtonColor: '#6c757d',
            preConfirm: () => {
                const firstName = document.getElementById('swalFirstName').value.trim();
                const middleName = document.getElementById('swalMiddleName').value.trim();
                const lastName = document.getElementById('swalLastName').value.trim();
                const street = document.getElementById('swalStreet').value.trim();
                const barangay = document.getElementById('swalBarangay').value.trim();
                const city = document.getElementById('swalCity').value.trim();
                const region = document.getElementById('swalRegion').value.trim();
                const country = document.getElementById('swalCountry').value.trim();
                const landmark = document.getElementById('swalLandmark').value.trim();
                const inspector = document.getElementById('swalInspector').value;
                const schedule = document.getElementById('swalSchedule').value;
                const remarks = document.getElementById('swalRemarks').value;

                if (!firstName || !lastName || !street || !barangay || !city || !region || !country || !inspector || !schedule) {
                    Swal.showValidationMessage("All fields are required except middle name, landmark, and remarks");
                    return false;
                }

                document.getElementById('<%= hfAssignData.ClientID %>').value =
                    `${inspector}|${schedule}|${remarks}|${firstName}|${middleName}|${lastName}|${street}|${barangay}|${city}|${region}|${country}|${landmark}`;
                document.getElementById('<%= btnAssignHidden.ClientID %>').click();
            }
        });
           }
       </script>

    </asp:Content>
