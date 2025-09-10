<%@ Page Title="" Language="C#" MasterPageFile="~/Inspector.master" AutoEventWireup="true" CodeBehind="MyInspections.aspx.cs" Inherits="RRCManagementSystem.MyInspections" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <style>
    .page-title{font-weight:700;color:#0d6efd;margin-bottom:1rem;font-size:clamp(1.2rem,3.8vw,1.6rem)}
    .filter-wrap{gap:.75rem}
    .card h5{font-size:clamp(1rem,3.5vw,1.1rem);margin-bottom:.5rem}
    .card p{margin-bottom:.4rem}
    .text-break-any{overflow-wrap:anywhere;word-break:break-word}
    .addr-line{display:block}
    .btn-xs{padding:.3rem .6rem;font-size:.85rem}
    @media (max-width:575.98px){ .container{padding-left:.75rem;padding-right:.75rem} }
  </style>

  <div class="container py-4">
    <h2 class="page-title">🕵️ My Inspections</h2>

    <!-- Status Filter -->
    <div class="row mb-3">
      <div class="col-12 col-sm-8 col-md-5">
        <div class="d-flex filter-wrap">
          <asp:DropDownList ID="ddlStatusFilter" runat="server" AutoPostBack="true"
            OnSelectedIndexChanged="ddlStatusFilter_SelectedIndexChanged"
            CssClass="form-select w-100">
            <asp:ListItem Text="All" Value="All" />
            <asp:ListItem Text="Pending" Value="Pending" />
            <asp:ListItem Text="Completed" Value="Completed" />
          </asp:DropDownList>
        </div>
      </div>
    </div>

    <!-- Inspections List -->
    <div class="row g-3 g-md-4">
      <asp:Repeater ID="rptInspections" runat="server">
        <ItemTemplate>
          <div class="col-12 col-sm-6 col-lg-4">
            <div class="card h-100 border-start border-3 border-primary shadow-sm">
              <div class="card-body">
                <h5 class="card-title text-primary mb-2">
                  Inspection #<%# Eval("InspectionID") %>
                </h5>

                <p class="mb-1"><strong>Inquiry Code:</strong>
                  <span class="text-break-any"><%# Eval("InquiryCode") %></span>
                </p>

                <p class="mb-1"><strong>Name:</strong>
                  <span class="text-break-any"><%# Eval("FullName") %></span>
                </p>

                <p class="mb-1 text-break-any">
                  <strong>Address:</strong>
                  <span class="addr-line">
                    <%# Eval("StreetAndUnit") %>, <%# Eval("Barangay") %>, <%# Eval("City") %>, <%# Eval("Region") %>, <%# Eval("Country") %>
                  </span>
                  <em><%# !string.IsNullOrEmpty(Eval("Landmark")?.ToString()) ? "(Landmark: " + Eval("Landmark") + ")" : "" %></em>
                </p>

                <p class="mb-1">
                  <strong>Scheduled:</strong>
                  <%# Eval("ScheduledDate", "{0:yyyy-MM-dd hh:mm tt}") %>
                </p>

                <p class="mb-1"><strong>Status:</strong> <%# Eval("InspectionStatus") %></p>

                <p class="mb-1 text-break-any"><strong>Remarks:</strong> <%# Eval("Remarks") %></p>

                <p class="mb-1 text-break-any">
                  <strong>Findings:</strong> <%#: Eval("Findings") %>
                </p>

                <p class="mb-3">
                  <strong>Assigned:</strong> <%# Eval("CreatedAt", "{0:yyyy-MM-dd}") %>
                </p>

                <%# Eval("InspectionStatus").ToString() == "Pending"
                    ? "<button type=\"button\" class=\"btn btn-success btn-xs\" onclick=\"markDoneWithFindings('" + Eval("InspectionID") + "')\">Mark Done</button>"
                    : "" %>
              </div>
            </div>
          </div>
        </ItemTemplate>
      </asp:Repeater>
    </div>
  </div>

  <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
  <script>
      function markDoneWithFindings(inspectionId) {
          Swal.fire({
              title: 'Mark as Done?',
              text: 'Please enter your findings for this inspection.',
              icon: 'question',
              input: 'textarea',
              inputLabel: 'Findings',
              inputPlaceholder: 'Describe your findings...',
              inputAttributes: { 'aria-label': 'Findings' },
              inputValidator: (value) => {
                  if (!value || !value.trim()) return 'Findings are required.';
              },
              showCancelButton: true,
              confirmButtonColor: '#198754',
              cancelButtonColor: '#d33',
              confirmButtonText: 'Yes, Mark Done'
          }).then((result) => {
              if (result.isConfirmed) {
                  const findings = encodeURIComponent(result.value.trim());
                  window.location.href = 'MyInspections.aspx?done=' + inspectionId + '&findings=' + findings;
              }
          });
      }
  </script>
</asp:Content>
