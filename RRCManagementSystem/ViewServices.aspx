<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master"
    AutoEventWireup="true" CodeBehind="ViewServices.aspx.cs"
    Inherits="RRCManagementSystem.ViewServices" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

  <style>
    .table { width:100%; border-collapse:collapse; }
    .table th, .table td { border:1px solid #dee2e6; padding:12px 15px; text-align:left; }
    .table th { background:#0b3f7a; color:#fff; }
    .action-btn { color:#0b3f7a; text-decoration:none; padding:4px 6px; cursor:pointer; }
    .action-btn:hover { text-decoration:underline; }
  </style>

  <asp:GridView ID="gvServices" runat="server"
      AutoGenerateColumns="False"
      CssClass="table"
      DataKeyNames="ServiceID"
      OnRowCommand="gvServices_RowCommand"
      OnRowDataBound="gvServices_RowDataBound"
      EmptyDataText="No services found.">

    <Columns>
      <asp:BoundField DataField="ServiceID" HeaderText="Service ID" ReadOnly="True" />
      <asp:BoundField DataField="Name" HeaderText="Service Name" />
      <asp:BoundField DataField="Description" HeaderText="Description" />
      <asp:BoundField DataField="ServiceType" HeaderText="Service Type" />
  

      <asp:TemplateField HeaderText="Actions">
        <ItemTemplate>
          <asp:LinkButton ID="btnEdit" runat="server"
              CommandName="EditService"
              CommandArgument='<%# Eval("ServiceID") %>'
              CssClass="action-btn" Text="Edit" />

          &nbsp;|&nbsp;

    
          <asp:LinkButton ID="btnDelete" runat="server"
              CommandName="DisableService"
              CommandArgument='<%# Eval("ServiceID") %>'
              CssClass="action-btn"
              CausesValidation="false"
              UseSubmitBehavior="false"
              OnClientClick="return confirmDelete(this, event);"
              Text="Delete" />
        </ItemTemplate>
      </asp:TemplateField>
    </Columns>
  </asp:GridView>

  <!-- SweetAlert2 -->
  <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
  <script type="text/javascript">
      // Important: DO NOT call __doPostBack manually for LinkButton inside GridView.
      // It won't include the internal argument that RowCommand expects.
      // Instead, programmatically click the same LinkButton after confirm.
      function confirmDelete(btn, evt) {
          // If we already confirmed once, allow normal postback
          if (btn.dataset.confirmed === '1') return true;

          if (evt) evt.preventDefault();

          Swal.fire({
              title: 'Delete this service?',
              text: 'This will service will be deleted.',
              icon: 'warning',
              showCancelButton: true,
              confirmButtonColor: '#0b3f7a',
              cancelButtonColor: '#d33',
              confirmButtonText: 'Yes, delete it'
          }).then((result) => {
              if (result.isConfirmed) {
                  // Mark as confirmed to skip re-prompt and trigger the same control click
                  btn.dataset.confirmed = '1';
                  btn.click();
              }
          });

          return false; // always prevent the initial postback
      }
  </script>

</asp:Content>
