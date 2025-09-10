<%@ Page Title="Inspected Inquiries" Language="C#" MasterPageFile="~/Admin.Master" 
    AutoEventWireup="true" CodeBehind="InspectedInquiry.aspx.cs" 
    Inherits="RRCManagementSystem.InspectedInquiry" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <!-- Bootstrap 5 -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />

    <style>
        /* Page Title */
        .inquiry-header {
            font-size: 24px;
            font-weight: bold;
            margin-bottom: 20px;
            color: #004085;
        }

        /* Table styling */
        .inquiry-table {
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 30px;
        }
        .inquiry-table th,
        .inquiry-table td {
            border: 1px solid #dee2e6;
            padding: 10px;
            vertical-align: middle;
        }
        .inquiry-table th {
            background-color: #e9ecef;
            color: #333;
            font-weight: 600;
            white-space: nowrap;
            text-align: center;
        }
        .inquiry-table td {
            text-align: left;
        }
        .inquiry-table tr:hover {
            background-color: #f8f9fa;
            transition: background 0.2s ease-in-out;
        }

        /* Status Badges */
        .badge-pill {
            display: inline-block;
            padding: 4px 10px;
            border-radius: 999px;
            font-size: 12px;
            font-weight: 600;
        }
        .badge-completed {
            background-color: #28a745;
            color: #fff;
        }
        .badge-pending {
            background-color: #ffc107;
            color: #212529;
        }

        /* Action buttons */
        .btn-primary-sm,
        .btn-disabled-sm {
            display: inline-block;
            width: 130px;
            text-align: center;
            padding: 8px 0;
            font-size: 14px;
            font-weight: 600;
            border-radius: 6px;
        }
        .btn-primary-sm {
            background-color: #007bff;
            color: #fff;
            border: none;
            cursor: pointer;
        }
        .btn-primary-sm:hover {
            background-color: #0056b3;
        }
        .btn-disabled-sm {
            background-color: #28a745;
            color: #fff;
            border: none;
            cursor: default;
        }

        /* Action column centering */
        .inquiry-table td.action-cell {
            text-align: center;
            vertical-align: middle;
            width: 160px;
        }

        /* Muted text for small details */
        .muted {
            color: #6c757d;
            font-size: 14px;
        }

        /* Pager styling */
        .gv-pager {
            padding: 10px;
            text-align: center;
            background: #f8f9fa;
            border-top: 1px solid #dee2e6;
        }
        .gv-pager a,
        .gv-pager span {
            margin: 0 3px;
            padding: 6px 10px;
            border-radius: 6px;
            font-weight: 600;
            text-decoration: none;
            font-size: 14px;
        }
        .gv-pager a {
            color: #007bff;
        }
        .gv-pager span {
            background: #007bff;
            color: #fff;
        }
    </style>

    <div class="container-fluid py-4">
        <!-- Page Header -->
        <div class="d-flex justify-content-between align-items-center mb-3">
            <h2 class="inquiry-header">✅ Inspected Inquiries (Completed w/ Findings)</h2>
            <asp:Label ID="lblCount" runat="server" CssClass="text-muted"></asp:Label>
        </div>

        <!-- Empty State Panel -->
        <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="alert alert-info">
            No completed inspections with findings yet.
        </asp:Panel>

        <!-- GridView -->
        <div class="table-responsive">
            <asp:GridView ID="gvCompleted" runat="server"
                CssClass="inquiry-table"
                AutoGenerateColumns="False"
                DataKeyNames="InspectionID"
                AllowPaging="True" PageSize="10"
                AllowSorting="True"
                OnRowCommand="gvCompleted_RowCommand"
                OnPageIndexChanging="gvCompleted_PageIndexChanging"
                OnSorting="gvCompleted_Sorting">

                <Columns>
                  
                    <asp:BoundField DataField="InspectionID" HeaderText="Inspection #" Visible="False" />

                  
                    <asp:BoundField DataField="InquiryCode" HeaderText="Reference Code" SortExpression="InquiryCode">
                        <ItemStyle CssClass="fw-bold text-primary text-center" />
                    </asp:BoundField>

                    <asp:TemplateField HeaderText="Client">
                        <ItemTemplate>
                            <div><strong><%# Eval("FullName") %></strong></div>
                            <div class="muted"><%# Eval("Email") %></div>
                            <div class="muted"><%# Eval("ContactNumber") %></div>
                        </ItemTemplate>
                    </asp:TemplateField>

               
                    <asp:TemplateField HeaderText="Address">
                        <ItemTemplate>
                            <%# Eval("StreetAndUnit") %>, <%# Eval("Barangay") %>, <%# Eval("City") %>, <%# Eval("Region") %>, <%# Eval("Country") %>
                            <%# string.IsNullOrWhiteSpace(Eval("Landmark")?.ToString()) ? "" : " • (Landmark: " + Eval("Landmark") + ")" %>
                        </ItemTemplate>
                    </asp:TemplateField>

                
                    <asp:BoundField DataField="ScheduledDate" HeaderText="Scheduled"
                        DataFormatString="{0:yyyy-MM-dd hh:mm tt}" HtmlEncode="false">
                        <ItemStyle CssClass="text-center nowrap" />
                    </asp:BoundField>

                    
                    <asp:TemplateField HeaderText="Status">
                        <ItemStyle CssClass="text-center" />
                        <ItemTemplate>
                            <span class='<%# Eval("InspectionStatus").ToString() == "Completed" ? "badge-pill badge-completed" : "badge-pill badge-pending" %>'>
                                <%# Eval("InspectionStatus") %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Remarks">
                        <ItemTemplate>
                            (<%# Eval("Remarks") %>)
                        </ItemTemplate>
                    </asp:TemplateField>

            
                    <asp:TemplateField HeaderText="Findings">
                        <ItemTemplate>
                            <%#: Eval("Findings") %>
                        </ItemTemplate>
                    </asp:TemplateField>

                   
                    <asp:TemplateField HeaderText="Action">
                        <ItemStyle CssClass="action-cell" />
                        <ItemTemplate>
                            <asp:LinkButton ID="btnCreate" runat="server"
                                CssClass='<%# (bool)Eval("HasAccount") ? "btn-disabled-sm" : "btn-primary-sm" %>'
                                CommandName="create"
                                CommandArgument='<%# Eval("InspectionID") %>'
                                Enabled='<%# !(bool)Eval("HasAccount") %>'>
                                <%# (bool)Eval("HasAccount") ? "Already Created" : "Create Client" %>
                            </asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>

                <EmptyDataTemplate>
                    <div class="muted">No inspected inquiries found.</div>
                </EmptyDataTemplate>

                <PagerStyle CssClass="gv-pager" />
            </asp:GridView>
        </div>
    </div>

</asp:Content>
