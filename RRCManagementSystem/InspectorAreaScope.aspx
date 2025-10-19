<%@ Page Title="Inspector Area Scope Management" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="InspectorAreaScope.aspx.cs" Inherits="RRCManagementSystem.InspectorAreaScope" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" />

    <script type="text/javascript">
        // SweetAlert function for GridView delete confirmation
        function confirmDelete(areaScopeId) {
            Swal.fire({
                title: 'Are you sure?',
                text: "You won't be able to revert this!",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#dc3545',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Yes, remove it!',
                customClass: {
                    confirmButton: 'btn btn-danger',
                    cancelButton: 'btn btn-secondary'
                },
                buttonsStyling: true
            }).then((result) => {
                if (result.isConfirmed) {
                    // Call the server-side delete method
                    __doPostBack('DeleteAreaScope', areaScopeId);
                }
            });
            return false;
        }
    </script>
    <style>
        /* Custom CSS to enforce vertical alignment and borders for the GridView cells */
        .table-organized td, .table-organized th {
            vertical-align: middle !important;
            border-right: 1px solid #e5e7eb;
        }
        .table-organized tr > *:last-child {
            border-right: none !important; 
        }

        /* Hide default GridView pager */
        .table-organized .default-pager {
            display: none;
        }

        /* Custom Pagination Styles */
        .custom-pagination {
            display: flex;
            justify-content: center;
            align-items: center;
            gap: 0.5rem;
            padding: 1rem;
            background: linear-gradient(to bottom, #f9fafb, #f3f4f6);
            border-top: 2px solid #e5e7eb;
        }

        .pagination-button {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            min-width: 2.5rem;
            height: 2.5rem;
            padding: 0.5rem;
            font-weight: 500;
            font-size: 0.875rem;
            color: #374151;
            background-color: white;
            border: 1px solid #d1d5db;
            border-radius: 0.375rem;
            cursor: pointer;
            transition: all 0.2s;
            text-decoration: none;
        }

        .pagination-button:hover:not(.active):not(:disabled) {
            background-color: #f3f4f6;
            border-color: #9ca3af;
            color: #1f2937;
            transform: translateY(-1px);
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
        }

        .pagination-button.active {
            background: linear-gradient(to bottom, #2563eb, #1d4ed8);
            color: white;
            border-color: #1d4ed8;
            font-weight: 600;
            box-shadow: 0 0 6px rgba(37, 99, 235, 0.4);
        }

        .pagination-button:disabled {
            opacity: 0.5;
            cursor: not-allowed;
        }

        .pagination-info {
            font-size: 0.875rem;
            color: #6b7280;
            font-weight: 500;
            padding: 0 0.75rem;
        }

        .pagination-ellipsis {
            color: #9ca3af;
            padding: 0 0.25rem;
        }

        /* City checkboxes container */
        .city-checkboxes-container {
            height: 192px;
            overflow-y: auto;
            background-color: white;
            border: 1px solid #ced4da;
            border-radius: 0.375rem;
            padding: 0.5rem;
        }

        .city-checkboxes-container .form-check {
            margin-bottom: 0.25rem;
        }

        .select-all-checkbox {
            margin-bottom: 0.5rem;
            font-weight: 600;
            color: #0d6efd;
        }

        /* GridView hover effect */
        .table-organized tbody tr:hover {
            background-color: #f8f9fa;
            transition: background-color 0.3s;
        }

        /* Validation messages */
        .validation-summary {
            color: #dc3545;
            margin-top: 1rem;
            padding: 0.75rem;
            border: 1px solid #f5c2c7;
            border-radius: 0.375rem;
            background-color: #f8d7da;
        }

        /* SweetAlert custom button styling */
.swal-custom-button {
    background-color: #add8e6 !important;
    color: #000000 !important;
    font-weight: bold;
    padding: 10px 24px;
    border-radius: 6px;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    border: none;
    transition: all 0.3s ease;
}

.swal-custom-button:hover {
    background-color: #87ceeb !important;
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.15);
    transform: translateY(-1px);
}

.swal-custom-button:active {
    transform: translateY(0);
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    
    <div class="bg-white rounded shadow-lg mb-4"> 
        <h2 class="fs-3 fw-bold p-4 pt-4 text-secondary text-center border-bottom">🗺️ Inspector Area Scope</h2>

        <div class="p-4">
            <h3 class="fs-5 fw-semibold mb-3 text-secondary">Add New Area Scope</h3>
            
            <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
            
                <div class="row g-3">
                    
                    <div class="col-12 col-lg-4">
                        <label for="<%= ddlInspector.ClientID %>" class="form-label fw-medium text-secondary small">Select Inspector</label>
                        <asp:DropDownList ID="ddlInspector" runat="server" CssClass="form-select">
                            <asp:ListItem Text="-- Select Inspector --" Value=""></asp:ListItem>
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator ID="rfvInspector" runat="server" ControlToValidate="ddlInspector" InitialValue="" ErrorMessage="Inspector is required." CssClass="text-danger small d-block mt-1" Display="Dynamic" ValidationGroup="ScopeGroup" EnableClientScript="False"></asp:RequiredFieldValidator>
                    </div>

                    <div class="col-12 col-lg-4">
                        <label for="<%= ddlRegion.ClientID %>" class="form-label fw-medium text-secondary small">Region</label>
                        <asp:DropDownList ID="ddlRegion" runat="server" 
                            AutoPostBack="true"
                            OnSelectedIndexChanged="ddlRegion_SelectedIndexChanged"
                            CssClass="form-select">
                            <asp:ListItem Text="-- Select Region --" Value=""></asp:ListItem>
                            <asp:ListItem Text="NCR - National Capital Region" Value="NCR"></asp:ListItem>
                            <asp:ListItem Text="Region I - Ilocos Region" Value="Region I"></asp:ListItem>
                            <asp:ListItem Text="Region II - Cagayan Valley" Value="Region II"></asp:ListItem>
                            <asp:ListItem Text="Region III - Central Luzon" Value="Region III"></asp:ListItem>
                            <asp:ListItem Text="Region IV-A - CALABARZON" Value="Region IV-A"></asp:ListItem>
                            <asp:ListItem Text="Region IV-B - MIMAROPA" Value="Region IV-B"></asp:ListItem>
                            <asp:ListItem Text="Region V - Bicol Region" Value="Region V"></asp:ListItem>
                            <asp:ListItem Text="Region VI - Western Visayas" Value="Region VI"></asp:ListItem>
                            <asp:ListItem Text="Region VII - Central Visayas" Value="Region VII"></asp:ListItem>
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator ID="rfvRegion" runat="server" ControlToValidate="ddlRegion" InitialValue="" ErrorMessage="Region is required." CssClass="text-danger small d-block mt-1" Display="Dynamic" ValidationGroup="ScopeGroup" EnableClientScript="False"></asp:RequiredFieldValidator>
                    </div>
                    
                    <div class="col-12 col-lg-4">
                        <label for="<%= pnlCityCheckboxes.ClientID %>" class="form-label fw-medium text-secondary small">Select Cities</label>
                        <div class="city-checkboxes-container">
                             <asp:Panel ID="pnlCityCheckboxes" runat="server">
                                 <asp:Label ID="lblCityPlaceholder" runat="server" Text="Select a Region first." CssClass="text-muted"></asp:Label>
                             </asp:Panel>
                        </div>
                        <asp:CustomValidator ID="cvCityRequired" runat="server" 
                            ErrorMessage="At least one City must be selected." CssClass="text-danger small d-block mt-1" Display="Dynamic" 
                            OnServerValidate="cvCityRequired_ServerValidate" ValidationGroup="ScopeGroup" EnableClientScript="False"></asp:CustomValidator>
                    </div>
                </div>

                <div class="mt-4 text-center">
                    <asp:Button ID="btnAddScope" runat="server" Text="Add Area Scope(s)" OnClick="btnAddScope_Click"
                        CssClass="btn btn-success fw-bold px-4 py-2 shadow" 
                        ValidationGroup="ScopeGroup"/>
                </div>
                
                <asp:ValidationSummary ID="vsAreaScope" runat="server" ShowSummary="true" HeaderText="Please correct the following errors:" CssClass="validation-summary" ValidationGroup="ScopeGroup" EnableClientScript="False" />
                <asp:Literal ID="litMessage" runat="server" EnableViewState="False" />

            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="ddlRegion" EventName="SelectedIndexChanged" />
                <asp:AsyncPostBackTrigger ControlID="btnAddScope" EventName="Click" />
            </Triggers>
            </asp:UpdatePanel>
        </div>

        <div class="p-4 mt-4 border-top">
            <h3 class="fs-5 fw-semibold mb-3 text-secondary">Current Area Scopes</h3>
            <asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="table-responsive shadow rounded border"> 
                    <asp:GridView ID="gvAreaScopes" runat="server" AutoGenerateColumns="False"
                        DataKeyNames="AreaScopeID"
                        AllowPaging="True" PageSize="10" 
                        OnPageIndexChanging="gvAreaScopes_PageIndexChanging"
                        GridLines="None"
                        CssClass="table table-hover table-organized mb-0" 
                        HeaderStyle-CssClass="table-primary text-uppercase small"
                        PagerSettings-Visible="false">
                        <Columns>
                            <asp:BoundField DataField="InspectorName" HeaderText="INSPECTOR NAME" ItemStyle-CssClass="py-3 px-4 text-start" HeaderStyle-CssClass="py-3 px-4 text-start" /> 
                            
                            <asp:BoundField DataField="Region" HeaderText="REGION" ItemStyle-CssClass="py-3 px-4 text-center" HeaderStyle-CssClass="py-3 px-4 text-center" />
                            
                            <asp:BoundField DataField="City" HeaderText="CITY" ItemStyle-CssClass="py-3 px-4 text-center" HeaderStyle-CssClass="py-3 px-4 text-center" />

                            <asp:BoundField DataField="CreatedAt" HeaderText="ASSIGNED DATE" DataFormatString="{0:d}" ItemStyle-CssClass="py-3 px-4 text-center" HeaderStyle-CssClass="py-3 px-4 text-center" />
                            
                            <asp:TemplateField HeaderText="ACTION" ItemStyle-CssClass="py-3 px-4 text-center" HeaderStyle-CssClass="py-3 px-4 text-center">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnDelete" runat="server" 
                                        CssClass="text-danger fw-bold text-decoration-none"
                                        OnClientClick='<%# "return confirmDelete(" + Eval("AreaScopeID") + ");" %>'> 
                                        <i class="fa-solid fa-trash-can"></i> Remove
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <PagerStyle CssClass="default-pager" />
                    </asp:GridView>
                    
                    <asp:Panel ID="pnlCustomPagination" runat="server" CssClass="custom-pagination">
                        <asp:LinkButton ID="btnFirstPage" runat="server" CssClass="pagination-button" OnClick="btnFirstPage_Click" ToolTip="First Page" CausesValidation="false">
                            <i class="fas fa-angle-double-left"></i>
                        </asp:LinkButton>
                        
                        <asp:LinkButton ID="btnPrevPage" runat="server" CssClass="pagination-button" OnClick="btnPrevPage_Click" ToolTip="Previous Page" CausesValidation="false">
                            <i class="fas fa-angle-left"></i>
                        </asp:LinkButton>
                        
                        <span class="pagination-info">
                            <asp:Literal ID="litPageInfo" runat="server"></asp:Literal>
                        </span>
                        
                        <asp:LinkButton ID="btnNextPage" runat="server" CssClass="pagination-button" OnClick="btnNextPage_Click" ToolTip="Next Page" CausesValidation="false">
                            <i class="fas fa-angle-right"></i>
                        </asp:LinkButton>
                        
                        <asp:LinkButton ID="btnLastPage" runat="server" CssClass="pagination-button" OnClick="btnLastPage_Click" ToolTip="Last Page" CausesValidation="false">
                            <i class="fas fa-angle-double-right"></i>
                        </asp:LinkButton>
                    </asp:Panel>
                </div>
            </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>
</asp:Content>