<%@ Page Title="Inspected Inquiries" Language="C#" MasterPageFile="~/Admin.Master"
    AutoEventWireup="true" CodeBehind="InspectedInquiry.aspx.cs"
    Inherits="RRCManagementSystem.InspectedInquiry" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <style>
    /* -------- Page chrome -------- */
    .page-title{
      font-weight:700;color:#0d6efd;margin:0 0 18px;
      font-size:clamp(1.2rem,3.2vw,1.6rem);letter-spacing:.2px
    }

    /* -------- Card wrapper -------- */
    .table-card{
      background:#fff;border:1px solid #e6e9ef;border-radius:12px;
      box-shadow:0 6px 18px rgba(17,24,39,.06);overflow:hidden
    }
    .table-card .card-head{
      display:flex;align-items:center;justify-content:space-between;gap:12px;
      padding:14px 16px;border-bottom:1px solid #edf1f7;
      background:linear-gradient(0deg,#fafbfc,#ffffff)
    }
    .card-head strong{color:#0b3b79;font-weight:700}
    .muted{color:#6c757d;font-size:.925rem}

    /* -------- Responsive table wrapper -------- */
    .table-scroll{width:100%;overflow:auto}

    /* -------- GridView base -------- */
    .gv{width:100%;border-collapse:separate;border-spacing:0; table-layout:fixed}
    .gv thead th{
      position:sticky;top:0;background:#f4f6f9;z-index:1;
      font-weight:700;color:#111827;border-bottom:1px solid #e6e9ef;
      padding:9px 10px;white-space:nowrap;text-align:left
    }
    /* make header sort links look like headers, not blue links */
  /* Make GridView header links look like normal text */
.gv thead a {
  color: inherit;           /* Use the header text color */
  text-decoration: none;    /* Remove underline */
  font-weight: inherit;
}

.gv thead a:hover {
  color: #0d6efd;           /* Optional: highlight on hover */
  text-decoration: none;
}


    .gv tbody td{
      padding:9px 10px;vertical-align:middle;border-bottom:1px solid #f1f3f6
    }
    .gv tbody tr:nth-child(odd){background:#ffffff}
    .gv tbody tr:nth-child(even){background:#fbfcfe}
    .gv tbody tr:hover{background:#eef5ff}

    /* -------- Badges -------- */
    .badge{
      display:inline-block;padding:4px 10px;border-radius:999px;
      font-size:.78rem;font-weight:700;line-height:1; white-space:nowrap
    }
    .badge-pending{background:#fff7ed;color:#9a3412;border:1px solid #fdba74}
    .badge-completed{background:#ecfdf5;color:#065f46;border:1px solid #86efac}

    /* -------- Text utilities -------- */
    .text-break-any{overflow-wrap:anywhere;word-break:break-word}
    .fw-semibold{font-weight:600}
    .small{font-size:.88rem;color:#495057}
    .nowrap{white-space:nowrap}

    /* -------- Button styles -------- */
    .btn-xs{
      padding:7px 12px;font-size:.86rem;border-radius:999px;
      border:1px solid transparent;cursor:pointer;transition:.15s ease-in-out;
      line-height:1; display:inline-flex; align-items:center; gap:6px;
      white-space:nowrap
    }
    .btn-primary{background:#0d6efd;color:#fff;border-color:#0d6efd}
    .btn-primary:hover{filter:brightness(0.96)}
    .btn-primary:active{transform:translateY(1px)}
    .btn-primary:disabled{opacity:.6;cursor:not-allowed}

    /* keep action cell neat */
    .cell-action{ display:flex; justify-content:center; }
    .cell-action .btn-xs{ min-width:118px } /* avoids wrap */

    /* -------- Narrow column widths -------- */
    .col-id{width:80px}
    .col-code{width:160px}
    .col-sched{width:170px}
    .col-status{width:110px; text-align:center}
    .col-action{width:140px; text-align:center}

    /* -------- Pager -------- */
    .gv-pager{
      padding:10px 12px;border-top:1px solid #edf1f7;background:#fafbff
    }
    .gv-pager a, .gv-pager span{
      display:inline-block;margin:0 3px;padding:6px 10px;border-radius:8px;
      text-decoration:none;font-weight:600;border:1px solid transparent
    }
    .gv-pager a{color:#0d6efd}
    .gv-pager a:hover{background:#eef2ff;border-color:#c7d2fe}
    .gv-pager span{background:#0d6efd;color:#fff;border:1px solid #0d6efd}
  </style>

  <div class="container py-4">
    <h2 class="page-title">✅ Inspected Inquiries (Completed w/ Findings)</h2>

    <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="alert alert-info">
      No completed inspections with findings yet.
    </asp:Panel>

    <div class="table-card">
      <div class="card-head">
        <div>
          <strong>Completed Inspections</strong>
          <span class="muted">— records with inspector findings</span>
        </div>
        <asp:Label ID="lblCount" runat="server" CssClass="muted"></asp:Label>
      </div>

      <div class="table-scroll">
        <asp:GridView ID="gvCompleted" runat="server"
          CssClass="gv table table-borderless mb-0"
          AutoGenerateColumns="False"
          DataKeyNames="InspectionID"
          AllowPaging="True" PageSize="10"
          AllowSorting="True"
          OnRowCommand="gvCompleted_RowCommand"
          OnPageIndexChanging="gvCompleted_PageIndexChanging"
          OnSorting="gvCompleted_Sorting">

          <Columns>
      
          <asp:BoundField DataField="InspectionID" HeaderText="Inspection #" SortExpression="InspectionID" Visible="False">
    <HeaderStyle CssClass="col-id" />
    <ItemStyle CssClass="col-id" />
</asp:BoundField>


            <asp:BoundField DataField="InquiryCode" HeaderText="Ref Code" SortExpression="InquiryCode" HtmlEncode="false">
              <HeaderStyle CssClass="col-code" />
              <ItemStyle CssClass="col-code nowrap" />
            </asp:BoundField>

      
            <asp:TemplateField HeaderText="Client">
              <ItemTemplate>
                <div class="fw-semibold text-break-any"><%# Eval("FullName") %></div>
                <div class="small text-break-any"><%# Eval("Email") %></div>
                <div class="small text-break-any"><%# Eval("ContactNumber") %></div>
              </ItemTemplate>
            </asp:TemplateField>

       
            <asp:TemplateField HeaderText="Address">
              <ItemTemplate>
                <div class="text-break-any">
                  <%# Eval("StreetAndUnit") %>, <%# Eval("Barangay") %>, <%# Eval("City") %>, <%# Eval("Region") %>, <%# Eval("Country") %>
                  <%# string.IsNullOrWhiteSpace(Eval("Landmark")?.ToString()) ? "" : " • (Landmark: " + Eval("Landmark") + ")" %>
                </div>
              </ItemTemplate>
            </asp:TemplateField>

       
           <asp:BoundField DataField="ScheduledDate" HeaderText="Scheduled" HtmlEncode="false">
  <HeaderStyle CssClass="col-sched" />
  <ItemStyle CssClass="col-sched nowrap" />
</asp:BoundField>


      
          <asp:TemplateField HeaderText="Status">
  <HeaderStyle CssClass="col-status" />
  <ItemStyle CssClass="col-status" />
  <ItemTemplate>
    <%# (Eval("InspectionStatus")?.ToString() ?? "") == "Completed"
          ? "<span class='badge badge-completed'>Completed</span>"
          : "<span class='badge badge-pending'>Pending</span>" %>
  </ItemTemplate>
</asp:TemplateField>


          
            <asp:TemplateField HeaderText="Remarks">
              <ItemTemplate>
                <div class="text-break-any"><%# Eval("Remarks") %></div>
              </ItemTemplate>
            </asp:TemplateField>

      
            <asp:TemplateField HeaderText="Findings">
              <ItemTemplate>
                <div class="text-break-any"><%#: Eval("Findings") %></div>
              </ItemTemplate>
            </asp:TemplateField>

         
            <asp:TemplateField HeaderText="Action">
              <HeaderStyle CssClass="col-action" />
              <ItemStyle CssClass="col-action" />
              <ItemTemplate>
                <div class="cell-action">
                  <asp:LinkButton ID="btnCreate" runat="server"
                    CssClass="btn-xs btn-primary"
                    CommandName="create"
                    CommandArgument='<%# Eval("InspectionID") %>'>
                 
                    Create Client
                  </asp:LinkButton>
                </div>
              </ItemTemplate>
            </asp:TemplateField>

          </Columns>

          <PagerStyle CssClass="gv-pager" HorizontalAlign="Left" />
          <EmptyDataTemplate>
            <div class="p-3 muted">No completed inspections with findings found.</div>
          </EmptyDataTemplate>
        </asp:GridView>
      </div>
    </div>
  </div>
</asp:Content>
