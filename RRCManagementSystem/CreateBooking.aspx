<%@ Page Title="" Language="C#" MasterPageFile="~/Inspector.master" AutoEventWireup="true" CodeBehind="CreateBooking.aspx.cs" Inherits="RRCManagementSystem.CreateBooking" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <!-- EnablePageMethods is required for calling static [WebMethod]s via JS -->
  <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />

  <!-- Hidden field to track Inquiry Background visibility -->
  <asp:HiddenField ID="hfInquiryVisible" runat="server" Value="false" />
  <!-- Hidden field to persist Inquiry JSON Data -->
  <asp:HiddenField ID="hfInquiryData" runat="server" Value="" />

  <style>
    .page-title{font-weight:700;color:#0d6efd;margin-bottom:1rem;text-align:center;font-size:clamp(1.2rem,3.8vw,1.7rem)}
    .card-wrap{max-width:720px}
    .form-label{font-weight:600}
    .form-helper{font-size:.85rem;color:#6c757d}
    .btn-wide{min-width:220px}
    .btn-full-xs{width:auto}
    .gap-12{gap:.75rem}

    /* Inquiry Background */
    .form-check-list table { width: 100%; border-collapse: separate; border-spacing: 0; }
    .form-check-list td {
      display: flex;
      align-items: flex-start;
      gap: .6rem;
      padding: .5rem 0;
      border-bottom: 1px solid #eee;
    }
    .form-check-list td:last-child { border-bottom: none; }
    .form-check-list input[type="checkbox"] {
      flex: 0 0 auto;
      width: 1.15rem;
      height: 1.15rem;
      margin-top: .15rem;
    }
    .form-check-list label {
      flex: 1 1 auto;
      margin: 0;
      font-weight: 400;
      line-height: 1.35;
      word-break: break-word;
      user-select: none;
    }
  </style>

  <div class="container py-4 py-sm-5">
    <div class="card shadow mx-auto card-wrap">
      <div class="card-body p-3 p-sm-4">
        <h2 class="page-title">🛠️ Create Booking Quotation</h2>

        <asp:Label ID="lblMessage" runat="server" CssClass="text-center d-block fw-semibold text-danger mb-3" />

        <!-- Wrap dynamic parts with UpdatePanel -->
        <asp:UpdatePanel ID="UpdatePanelMain" runat="server" UpdateMode="Conditional">
          <ContentTemplate>

            <!-- Search Client -->
            <div class="mb-3">
              <label for="<%= txtClientSearch.ClientID %>" class="form-label">Search Client</label>
              <asp:TextBox ID="txtClientSearch" runat="server" CssClass="form-control" placeholder="Type name or email..." />
              <div class="form-helper">Start typing to search, then tap a suggestion.</div>

              <ajaxToolkit:AutoCompleteExtender 
                ID="AutoCompleteExtender1" runat="server"
                TargetControlID="txtClientSearch"
                ServiceMethod="SearchClients"
                MinimumPrefixLength="1"
                CompletionSetCount="10"
                EnableCaching="true"
                FirstRowSelected="true"
                OnClientItemSelected="setClientID" />
              <asp:HiddenField ID="hfClientID" runat="server" />
            </div>

            <!-- Inquiry Background Section -->
            <div id="inquiryBackground" class="mb-4 p-3 border rounded bg-light" style="display:none;">
              <div class="d-flex justify-content-between align-items-center mb-2">
                <h5 class="fw-bold mb-0">Inquiry Background</h5>
                <small class="text-muted" id="ibUpdated"></small>
              </div>

              <div class="mb-2">
                <span class="fw-semibold">Inquiry Code:</span>
                <span id="ibCode" class="text-primary fw-semibold">—</span>
              </div>

              <div>
                <span class="fw-semibold">Recent Findings:</span>
                <ul id="ibFindings" class="mb-0 mt-2" style="padding-left:1.25rem;"></ul>
                <div id="ibEmpty" class="text-muted">No findings found for this client yet.</div>
              </div>
            </div>

            <!-- Service Selection -->
            <div class="mb-3">
              <label class="form-label">Select Service</label>
              <asp:DropDownList ID="ddlServices" runat="server" CssClass="form-select"
                AutoPostBack="true" OnSelectedIndexChanged="ddlServices_SelectedIndexChanged">
              </asp:DropDownList>
            </div>

            <!-- SQM -->
            <div class="mb-3">
              <label class="form-label">Square Meters (SQM)</label>
              <asp:TextBox ID="txtSQM" runat="server" CssClass="form-control" TextMode="Number" 
                           AutoPostBack="true" OnTextChanged="RecalculateTotal" />
            </div>

            <!-- Travel Expense -->
            <div class="mb-3">
              <label class="form-label">Travel Expense (₱)</label>
              <asp:TextBox ID="txtTravelExpense" runat="server" CssClass="form-control" TextMode="Number"
                           AutoPostBack="true" OnTextChanged="RecalculateTotal" />
            </div>

            <!-- Miscellaneous -->
            <div class="mb-3">
              <label class="form-label">Miscellaneous (₱)</label>
              <asp:TextBox ID="txtMiscellaneous" runat="server" CssClass="form-control" TextMode="Number"
                           AutoPostBack="true" OnTextChanged="RecalculateTotal" />
            </div>

            <!-- Total Price -->
            <div class="mb-4">
              <label class="form-label">Total Price (₱)</label>
              <asp:TextBox ID="txtTotalPrice" runat="server" CssClass="form-control" ReadOnly="true" />
            </div>

            <!-- Actions -->
            <div class="d-flex justify-content-center gap-12">
              <asp:Button ID="btnSubmit" runat="server" Text="Submit Quotation"
                CssClass="btn btn-success px-4 btn-wide btn-full-xs"
                OnClick="btnSubmit_Click" />
            </div>

          </ContentTemplate>
        </asp:UpdatePanel>
      </div>
    </div>
  </div>

  <!-- SweetAlert2 -->
  <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

  <!-- Autocomplete selection handler -->
  <script type="text/javascript">
      function setClientID(source, eventArgs) {
          var clientName = eventArgs.get_text();
          var clientID = eventArgs.get_value();
          document.getElementById('<%= txtClientSearch.ClientID %>').value = clientName;
          document.getElementById('<%= hfClientID.ClientID %>').value = clientID;

          // Show Inquiry panel
          document.getElementById('<%= hfInquiryVisible.ClientID %>').value = "true";
          document.getElementById('inquiryBackground').style.display = "block";

          // Fetch Inquiry Data
          PageMethods.GetClientInquirySummary(parseInt(clientID, 10),
              function (res) {
                  var jsonString = JSON.stringify(res);
                  document.getElementById('<%= hfInquiryData.ClientID %>').value = jsonString; // Persist data

                  renderInquiry(res);
              },
              function () {
                  document.getElementById('ibCode').textContent = "—";
                  document.getElementById('ibFindings').innerHTML = "";
                  document.getElementById('ibEmpty').style.display = "";
                  document.getElementById('ibUpdated').textContent = "";
                  document.getElementById('inquiryBackground').style.display = "block";
              });
      }

      // Renders Inquiry Background from object
      function renderInquiry(data) {
          var box = document.getElementById('inquiryBackground');
          var code = document.getElementById('ibCode');
          var list = document.getElementById('ibFindings');
          var empty = document.getElementById('ibEmpty');
          var upd = document.getElementById('ibUpdated');

          list.innerHTML = "";
          empty.style.display = "none";
          upd.textContent = "";

          if (!data) {
              code.textContent = "—";
              empty.style.display = "";
              box.style.display = "block";
              return;
          }

          code.textContent = data.InquiryCode || "—";

          if (data.Findings && data.Findings.length > 0) {
              data.Findings.forEach(function (f) {
                  var li = document.createElement("li");
                  li.className = "small";
                  var when = f.When ? (" (" + f.When + ")") : "";
                  li.textContent = f.Text + when;
                  list.appendChild(li);
              });
              if (data.LastUpdated) upd.textContent = "Updated: " + data.LastUpdated;
          } else {
              empty.style.display = "";
          }

          box.style.display = "block";
      }

      // Reload Inquiry after postback (service selection)
      Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
          var json = document.getElementById('<%= hfInquiryData.ClientID %>').value;
          if (json) {
              renderInquiry(JSON.parse(json));
          }
      });
  </script>
</asp:Content>
