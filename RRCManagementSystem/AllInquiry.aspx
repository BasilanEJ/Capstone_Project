<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AllInquiry.aspx.cs" Inherits="RRCManagementSystem.AllInquiry" %>

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
        .btn-danger-sm {
            background-color: #dc3545; color:#fff; border:none;
            padding:6px 12px; font-size:14px; cursor:pointer;
        }
        .btn-danger-sm:hover { background-color:#bb2d3b; }
    </style>

    <asp:HiddenField ID="hfSelectedInquiryID" runat="server" />
    <asp:HiddenField ID="hfAssignData" runat="server" />
    <asp:Button ID="btnAssignHidden" runat="server" OnClick="btnAssignHidden_Click" Style="display:none;" />

    <!-- Hidden controls for deletion -->
    <asp:HiddenField ID="hfDeleteInquiryID" runat="server" />
    <asp:Button ID="btnDeleteHidden" runat="server" OnClick="btnDeleteHidden_Click" Style="display:none;" />

    <!-- NEW: hidden source dropdown (server-bound) to feed Swal select -->
    <asp:DropDownList ID="ddlInspectorSource" runat="server" Style="display:none;"></asp:DropDownList>

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

            <asp:TemplateField HeaderText="Delete">
                <ItemTemplate>
                    <button type="button" class="btn-danger-sm"
                            onclick='confirmDelete(<%# Eval("InquiryID") %>)'>
                        Delete
                    </button>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

<script type="text/javascript">
    function showAssignModal(el, inquiryId) {
        // keep the selected ID for server side
        document.getElementById('<%= hfSelectedInquiryID.ClientID %>').value = inquiryId;

      // read values from data-* attributes placed on the Assign button
      const d = el && el.dataset ? el.dataset : {};
      const val = (s) => (s || '').replace(/"/g, '&quot;'); // avoid breaking HTML attributes

      // min datetime for schedule
      const now = new Date();
      const pad = (n) => String(n).padStart(2, '0');
      const minDateTime = `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}T${pad(now.getHours())}:${pad(now.getMinutes())}`;

      Swal.fire({
          title: '🛠️ Assign Inspector',
          width: 800,
          html: `
          <div style="display: flex; gap: 10px;">
              <div style="flex:1;">
                  <label></label>
                  <input id="swalFirstName" class="swal2-input" placeholder="First Name" value="${val(d.fn)}">
              </div>
              <div style="flex:1;">
                  <label></label>
                  <input id="swalLastName" class="swal2-input" placeholder="Last Name" value="${val(d.ln)}">
              </div>
          </div>

          <div style="display: flex; gap: 10px;">
              <div style="flex:1;">
                  <label></label>
                  <input id="swalMiddleName" class="swal2-input" placeholder="Middle Name" value="${val(d.mn)}">
              </div>
              <div style="flex:1;">
                  <label></label>
                  <input id="swalCountry" class="swal2-input" value="${val(d.ctry || 'Philippines')}">
              </div>
          </div>

          <div style="display: flex; gap: 10px;">
              <div style="flex:1;">
                  <label></label>
                  <input id="swalRegion" class="swal2-input" placeholder="Region" value="${val(d.reg)}">
              </div>
              <div style="flex:1;">
                  <label></label>
                  <input id="swalCity" class="swal2-input" placeholder="City" value="${val(d.city)}">
              </div>
          </div>

          <div style="display: flex; gap: 10px;">
              <div style="flex:1;">
                  <label></label>
                  <input id="swalBarangay" class="swal2-input" placeholder="Barangay" value="${val(d.brgy)}">
              </div>
              <div style="flex:1;">
                  <label></label>
                  <input id="swalStreet" class="swal2-input" placeholder="Street & Unit" value="${val(d.str)}">
              </div>
          </div>

          <div style="display: flex; gap: 10px;">
              <div style="flex:1;">
                  <label></label>
                  <input id="swalLandmark" class="swal2-input" placeholder="Landmark" value="${val(d.lmk)}">
              </div>
              <div style="flex:1;">
                  <label>Schedule</label>
                  <input type="datetime-local" id="swalSchedule" class="swal2-input" min="${minDateTime}">
              </div>
          </div>

          <div style="margin-top: 20px; display: flex; flex-direction: column; align-items: center;">
              <label for="swalInspector" style="margin-bottom: 5px; font-weight: 600;">Inspector</label>
              <select id="swalInspector" class="swal2-input" style="font-size: 16px; width: 500px; height: 45px; padding: 8px;">
                  <!-- options filled in didOpen -->
              </select>
          </div>

          <div style="margin-top: 20px; display: flex; flex-direction: column; align-items: center;">
              <label for="swalRemarks" style="margin-bottom: 5px; font-weight: 600;">Remarks</label>
              <textarea id="swalRemarks" class="swal2-textarea" placeholder="Remarks (optional)"
                        style="width: 600px; font-size: 16px; padding: 8px;"></textarea>
          </div>
          `,
          didOpen: () => {
              // copy inspector options from hidden server-bound dropdown
              const source = document.getElementById('<%= ddlInspectorSource.ClientID %>');
              const target = document.getElementById('swalInspector');
              if (source && target) target.innerHTML = source.innerHTML;
          },
          showCancelButton: true,
          confirmButtonText: 'Assign',
          confirmButtonColor: '#004085',
          cancelButtonColor: '#6c757d',
          preConfirm: () => {
              const firstName = document.getElementById('swalFirstName').value.trim();
              const middleName = document.getElementById('swalMiddleName').value.trim();
              const lastName  = document.getElementById('swalLastName').value.trim();
              const street    = document.getElementById('swalStreet').value.trim();
              const barangay  = document.getElementById('swalBarangay').value.trim();
              const city      = document.getElementById('swalCity').value.trim();
              const region    = document.getElementById('swalRegion').value.trim();
              const country   = document.getElementById('swalCountry').value.trim();
              const landmark  = document.getElementById('swalLandmark').value.trim();
              const inspector = document.getElementById('swalInspector').value;
              const schedule  = document.getElementById('swalSchedule').value;
              const remarks   = document.getElementById('swalRemarks').value;

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

        function confirmDelete(inquiryId) {
            Swal.fire({
                title: 'Delete this inquiry?',
                text: "This action cannot be undone.",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Yes, delete it',
                cancelButtonText: 'Cancel',
                confirmButtonColor: '#dc3545',
                cancelButtonColor: '#6c757d'
            }).then((result) => {
                if (result.isConfirmed) {
                    document.getElementById('<%= hfDeleteInquiryID.ClientID %>').value = inquiryId;
                    document.getElementById('<%= btnDeleteHidden.ClientID %>').click();
                }
            });
        }
</script>
</asp:Content>
