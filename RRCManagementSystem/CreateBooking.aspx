<%@ Page Title="" Language="C#" MasterPageFile="~/Inspector.master" AutoEventWireup="true" CodeBehind="CreateBooking.aspx.cs" Inherits="RRCManagementSystem.CreateBooking" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <asp:ScriptManager ID="ScriptManager1" runat="server" />

  <style>
    .page-title{font-weight:700;color:#0d6efd;margin-bottom:1rem;text-align:center;font-size:clamp(1.2rem,3.8vw,1.7rem)}
    .card-wrap{max-width:720px}
    .form-label{font-weight:600}
    .form-helper{font-size:.85rem;color:#6c757d}
    .btn-wide{min-width:220px}
    .btn-full-xs{width:auto}
    .gap-12{gap:.75rem}

    /* Services list: full-width, comfy tap targets */
   /* Services list: checkbox on the left, text wraps beside it */
.form-check-list table { width: 100%; border-collapse: separate; border-spacing: 0; }
.form-check-list tr { border-bottom: 1px solid #f1f3f5; }
.form-check-list tr:last-child { border-bottom: 0; }

/* Make each row a horizontal flex container */
.form-check-list td {
  display: flex;
  align-items: flex-start;       /* top-align the checkbox with the first line of text */
  gap: .6rem;
  padding: .5rem 0;
}

/* Bigger tap target on mobile */
.form-check-list input[type="checkbox"] {
  flex: 0 0 auto;
  width: 1.15rem;
  height: 1.15rem;
  margin-top: .15rem;            /* nudges to align nicely with text */
}

/* Label/text wraps but stays on the same line as the checkbox */
.form-check-list label {
  flex: 1 1 auto;
  margin: 0;
  font-weight: 400;
  line-height: 1.35;
  white-space: normal;           /* allow multi-line wrapping */
  word-break: break-word;        /* prevents overflow on very long words */
  user-select: none;
}

.form-check-list table { width: 100%; }
.form-check-list td {
    display: flex;
    align-items: flex-start;
    gap: .6rem;
    padding: .5rem 0;
    border-bottom: 1px solid #eee;
}
.form-check-list td:last-child { border-bottom: none; }

h5.fw-bold {
    font-size: 1.1rem;
    margin-bottom: 0.75rem;
}

    /* AjaxControlToolkit autocomplete dropdown */
    .ajax__autocomplete_container{z-index:2000 !important; max-width:100%}
    .ajax__autocomplete_item{padding:.5rem .75rem; font-size:.95rem}
    .ajax__autocomplete_item:hover{background:#f1f5f9}

    /* Small screens: tighter padding, full-width button */
    @media (max-width:575.98px){
      .container{padding-left:.75rem; padding-right:.75rem}
      .btn-full-xs{width:100%}
    }
  </style>

  <div class="container py-4 py-sm-5">
    <div class="card shadow mx-auto card-wrap">
      <div class="card-body p-3 p-sm-4">
        <h2 class="page-title">🛠️ Create Booking Quotation</h2>

        <asp:Label ID="lblMessage" runat="server" CssClass="text-center d-block fw-semibold text-danger mb-3" />

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

        <!-- Services -->
      <!-- Services -->
<div class="mb-3">
  <label class="form-label">Select Services</label>

  <!-- Termite Control -->
  <div class="mb-4 p-3 border rounded bg-light">
    <h5 class="fw-bold text-primary mb-3">🪲 Termite Control</h5>
    <asp:CheckBoxList
      ID="cblTermite" runat="server"
      RepeatLayout="Table" CssClass="form-check-list"
      DataTextField="Name" DataValueField="ServiceID">
    </asp:CheckBoxList>
  </div>

  <!-- General Pest Control -->
  <div class="p-3 border rounded bg-light">
    <h5 class="fw-bold text-success mb-3">🐜 General Pest Control</h5>
    <asp:CheckBoxList
      ID="cblGeneral" runat="server"
      RepeatLayout="Table" CssClass="form-check-list"
      DataTextField="Name" DataValueField="ServiceID">
    </asp:CheckBoxList>
  </div>
</div>


        <!-- SQM -->
        <div class="mb-3">
          <label for="<%= txtSQM.ClientID %>" class="form-label">Square Meters (SQM)</label>
          <asp:TextBox ID="txtSQM" runat="server" CssClass="form-control" TextMode="Number" />
          <div class="form-helper">Enter total area to be serviced.</div>
        </div>

        <!-- Total Price -->
        <div class="mb-4">
          <label for="<%= txtTotalPrice.ClientID %>" class="form-label">Total Quotation Price (₱)</label>
          <asp:TextBox ID="txtTotalPrice" runat="server" CssClass="form-control" TextMode="Number" />
          <div class="form-helper">Set the total price you discussed with the client.</div>
        </div>

        <!-- Actions -->
        <div class="d-flex justify-content-center gap-12">
          <asp:Button ID="btnSubmit" runat="server" Text="Submit Quotation"
            CssClass="btn btn-success px-4 btn-wide btn-full-xs"
            OnClick="btnSubmit_Click" />
        </div>
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
      }
  </script>
</asp:Content>
