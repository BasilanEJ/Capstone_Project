<%@ Page Title="Travel Expense Management" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="TravelExpense.aspx.cs" Inherits="RRCManagementSystem.TravelExpense" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        /* ===================================================================
           GLOBAL STYLES & VARIABLES
           =================================================================== */
        :root {
            --primary-blue: #2563eb;
            --primary-dark: #1e40af;
            --text-dark: #1e293b;
            --text-muted: #64748b;
            --bg-light: #f8fafc;
            --success-color: #10b981;
            --warning-color: #f59e0b;
            --danger-color: #ef4444;
        }

        body {
            background-color: var(--bg-light); 
            font-family: 'Inter', sans-serif;
        }

        .expense-container {
            padding: 30px 15px;
            max-width: 1400px; /* Increased max-width slightly for more table space */
            margin: 0 auto;
        }

        /* ===================================================================
           HEADER & CARD STYLES
           =================================================================== */

        .expense-header {
            background: linear-gradient(135deg, var(--primary-dark) 0%, var(--primary-blue) 100%);
            color: white;
            padding: 30px;
            border-radius: 16px; 
            margin-bottom: 40px;
            box-shadow: 0 8px 30px rgba(30, 64, 175, 0.4); /* Enhanced shadow */
            position: relative;
            overflow: hidden; 
        }

        .expense-header h1 {
            font-size: 32px;
            font-weight: 800; /* Bolder */
            letter-spacing: -0.8px;
        }

        .modern-card {
            background: white;
            border-radius: 16px; 
            padding: 30px;
            margin-bottom: 30px;
            box-shadow: 0 4px 20px rgba(0,0,0,0.08); 
            border: 1px solid #eef1f5; 
            transition: transform 0.3s ease, box-shadow 0.3s ease;
        }

        .modern-card-title {
            font-size: 22px;
            font-weight: 700;
            color: var(--text-dark);
            margin-bottom: 25px;
            padding-bottom: 15px;
            border-bottom: 1px solid #e2e8f0; 
            display: flex;
            align-items: center;
            gap: 12px;
        }

        /* ===================================================================
           FORM CONTROL STYLES
           =================================================================== */

        .form-select, .form-control {
            border-radius: 10px;
            border: 1px solid #cbd5e1;
            padding: 12px 15px;
            transition: border-color 0.3s ease, box-shadow 0.3s ease;
            color: var(--text-dark);
            width: 100%; /* Ensure full width */
        }
        
        .form-select:focus, .form-control:focus {
            outline: none;
            border-color: var(--primary-blue);
            box-shadow: 0 0 0 4px rgba(37, 99, 235, 0.15); 
        }
        
        /* Button Aesthetics */
        .btn-primary, .btn-info, .btn-secondary, .btn-warning, .btn-danger {
            font-weight: 600;
            padding: 12px 25px;
            border-radius: 10px;
            transition: all 0.3s cubic-bezier(.25,.8,.25,1);
            border: none; 
            color: white;
            box-shadow: 0 4px 10px rgba(0,0,0,0.1);
        }

        .btn-primary {
            background: linear-gradient(90deg, #3b82f6, var(--primary-blue));
            box-shadow: 0 4px 15px rgba(37, 99, 235, 0.35);
        }
        
        .btn-warning { background-color: var(--warning-color); }
        .btn-danger { background-color: var(--danger-color); }

        .btn-secondary:hover, .btn-warning:hover, .btn-danger:hover {
            opacity: 0.9;
            transform: translateY(-1px);
        }
        
        /* ===================================================================
           TABLE STYLES & ALIGNMENT FIXES (CRITICAL CHANGES)
           =================================================================== */

        .table-responsive {
            border: none;
            border-radius: 16px; 
            box-shadow: 0 2px 10px rgba(0,0,0,0.05);
            background: white; 
        }
        
        .table {
            border-collapse: separate;
            border-spacing: 0;
            margin-bottom: 0; /* Remove default margin */
        }

        .table > :not(caption) > * > * {
            padding: 18px 20px; /* Slightly more vertical padding */
            vertical-align: middle;
            border-top: none;
        }
        
        .table thead th {
            font-weight: 700;
            font-size: 14px;
            color: #475569; 
            background-color: #f1f5f9; 
            border-bottom: 1px solid #e2e8f0;
            text-align: left; /* Default alignment */
        }
        
        /* Explicit Alignment Classes: Used only when content shouldn't be left-aligned */
        .table thead th.align-center,
        .table tbody td.align-center {
            text-align: center;
        }

        .table thead th.align-right,
        .table tbody td.align-right {
            text-align: right;
        }
        
        /* Specific content styles */
        .badge {
            padding: 0.7em 1.1em; /* Increased padding */
            font-size: 90%;
            font-weight: 700;
            border-radius: 25px; /* More rounded */
            display: inline-flex;
            align-items: center;
            gap: 6px;
        }
        
        .badge.bg-success { background-color: var(--success-color) !important; }
        .badge.bg-primary { background-color: var(--primary-blue) !important; }
        .badge.bg-secondary { background-color: var(--text-muted) !important; }
        
        .table-row-data {
            font-size: 15px;
            color: var(--text-dark);
        }

        .text-muted-small {
            color: #94a3b8 !important; /* Lighter text color for dates */
            font-size: 12px;
            line-height: 1.2;
            display: block; /* Ensure small elements stack cleanly in right-aligned cells */
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="expense-container">
        <!-- Main Header -->
        <div class="expense-header">
            <h1><i class="fas fa-map-marked-alt"></i> Travel Expense Configuration</h1>
            <p>Define standardized travel allowances per region and city for accurate cost quotations. 🚀</p>
        </div>

        <asp:UpdatePanel ID="UpdatePanelMain" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                
                <!-- Expense Form Card -->
                <div class="modern-card">
                    <div class="modern-card-title">
                        <i class="fas fa-plus-circle text-primary"></i>
                        <asp:Label ID="lblFormTitle" runat="server" Text="Add New Travel Expense" />
                    </div>
                    
                    <asp:Label ID="lblMessage" runat="server" CssClass="alert d-block mb-4" Visible="false" />

                    <div class="row g-4">
                        <div class="col-md-4">
                            <label for="<%= ddlRegion.ClientID %>" class="form-label">
                                <i class="fas fa-globe me-1 text-primary"></i>Region <span class="text-danger">*</span>
                            </label>
                            <asp:DropDownList ID="ddlRegion" runat="server" CssClass="form-select" 
                                AutoPostBack="true" OnSelectedIndexChanged="ddlRegion_SelectedIndexChanged">
                            </asp:DropDownList>
                        </div>

                        <div class="col-md-4">
                            <label for="<%= ddlCity.ClientID %>" class="form-label">
                                <i class="fas fa-city me-1 text-success"></i>City/Municipality <span class="text-danger">*</span>
                            </label>
                            <asp:DropDownList ID="ddlCity" runat="server" CssClass="form-select">
                                <asp:ListItem Value="" Text="-- Select Region First --" />
                            </asp:DropDownList>
                        </div>

                        <div class="col-md-4">
                            <label for="<%= txtTravelPrice.ClientID %>" class="form-label">
                                <i class="fas fa-peso-sign me-1 text-warning"></i>Travel Expense (₱) <span class="text-danger">*</span>
                            </label>
                            <asp:TextBox ID="txtTravelPrice" runat="server" CssClass="form-control" 
                                TextMode="Number" step="0.01" min="0" placeholder="e.g., 500.00" />
                            <small class="text-muted ms-1 mt-1 d-block">Enter amount in Philippine Peso</small>
                        </div>
                        
                        <div class="col-12 d-flex justify-content-end align-items-center mt-3">
                            <asp:HiddenField ID="hfTravelExpenseID" runat="server" Value="0" />
                            <asp:HiddenField ID="hfIsActive" runat="server" Value="True" /> 
                            
                            <asp:Button ID="btnCancel" runat="server" Text="Cancel Edit" 
                                CssClass="btn btn-secondary me-3" OnClick="btnCancel_Click" Visible="false" />
                            
                            <asp:Button ID="btnSave" runat="server" Text="Save Expense" 
                                CssClass="btn btn-primary" OnClick="btnSave_Click">
                            </asp:Button>
                        </div>
                    </div>
                </div>
                

                <div class="modern-card">
                    <div class="modern-card-title">
                        <i class="fas fa-filter text-info"></i>Filter & Search Expenses
                    </div>
                    <div class="row g-4 align-items-end">
                        <div class="col-md-5">
                            <label for="<%= ddlFilterRegion.ClientID %>" class="form-label"><i class="fas fa-search-location me-1 text-primary"></i>Filter by Region</label>
                            <asp:DropDownList ID="ddlFilterRegion" runat="server" CssClass="form-select" 
                                AutoPostBack="true" OnSelectedIndexChanged="ApplyFilter">
                            </asp:DropDownList>
                        </div>
                        <div class="col-md-5">
                            <label for="<%= txtSearchCity.ClientID %>" class="form-label"><i class="fas fa-magnifying-glass me-1 text-success"></i>Search City</label>
                            <asp:TextBox ID="txtSearchCity" runat="server" CssClass="form-control" 
                                placeholder="Type city name..." />
                        </div>
                        <div class="col-md-2 d-flex gap-2">
                            <asp:Button ID="btnSearch" runat="server" Text="Apply" 
                                CssClass="btn btn-info w-50" OnClick="ApplyFilter">
                            </asp:Button>
                            <asp:Button ID="btnClearFilter" runat="server" Text="Clear" 
                                CssClass="btn btn-secondary w-50" OnClick="btnClearFilter_Click" />
                        </div>
                    </div>
                </div>

                <!-- Configured Expense List Card -->
                <div class="modern-card p-0">
                    <div class="modern-card-title p-4 pb-3 mb-0 d-flex justify-content-between align-items-center">
                        <span class="m-0">
                             <i class="fas fa-table text-secondary"></i> Configured Expense List
                        </span>
                        <span class="count-badge">
                            Total Records: <asp:Label ID="lblTotalCount" runat="server" Text="0" /> 🗺️
                        </span>
                    </div>
                    <div class="card-body p-0">
                        <div class="table-responsive">
                            <asp:GridView ID="gvTravelExpenses" runat="server" CssClass="table table-hover" 
                                AutoGenerateColumns="false" DataKeyNames="TravelExpenseID" 
                                OnRowCommand="gvTravelExpenses_RowCommand" 
                                EmptyDataText="No travel expenses configured yet. Start by adding one above!"
                                GridLines="None" HeaderStyle-CssClass="table-dark-header">
                                <Columns>
                                    <asp:BoundField DataField="TravelExpenseID" HeaderText="ID" Visible="false" />
                                   
                                    <asp:TemplateField HeaderText="Region">
                                        <ItemTemplate>
                                            <span class="table-row-data">
                                                <i class="fas fa-globe text-primary me-2"></i>
                                                <%# Eval("Region") %>
                                            </span>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="City/Municipality">
                                        <ItemTemplate>
                                            <span class="table-row-data">
                                                <i class="fas fa-city text-success me-2"></i>
                                                <strong><%# Eval("City") %></strong>
                                            </span>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Travel Expense" HeaderStyle-CssClass="align-center" ItemStyle-CssClass="align-center" ItemStyle-Width="150px">
                                        <ItemTemplate>
                                            <span class="badge bg-success fs-6">
                                                <i class="fas fa-money-bill-wave"></i> ₱<%# String.Format("{0:N2}", Eval("TravelPrice")) %>
                                            </span>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Status" HeaderStyle-CssClass="align-center" ItemStyle-CssClass="align-center" ItemStyle-Width="120px">
                                        <ItemTemplate>
                                            <%# Convert.ToBoolean(Eval("IsActive")) 
                                                ? "<span class='badge bg-primary'><i class='fas fa-check-circle'></i> Active</span>" 
                                                : "<span class='badge bg-secondary'><i class='fas fa-minus-circle'></i> Inactive</span>" %>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Last Updated" HeaderStyle-CssClass="align-right" ItemStyle-CssClass="align-right" ItemStyle-Width="180px">
                                        <ItemTemplate>

                                            <span class="text-muted-small">
                                                <i class="fas fa-clock"></i>
                                                <%# Eval("UpdatedAt") != DBNull.Value 
                                                    ? Convert.ToDateTime(Eval("UpdatedAt")).ToString("MMM dd, yyyy hh:mm tt") 
                                                    : Convert.ToDateTime(Eval("CreatedAt")).ToString("MMM dd, yyyy hh:mm tt") %>
                                            </span>
                                        </ItemTemplate>
                                    </asp:TemplateField>


                                    <asp:TemplateField HeaderText="Actions" HeaderStyle-CssClass="align-center" ItemStyle-CssClass="align-center" ItemStyle-Width="180px">
                                        <ItemTemplate>
                                            <asp:LinkButton ID="btnEdit" runat="server" 
                                                CommandName="EditExpense" 
                                                CommandArgument='<%# Eval("TravelExpenseID") %>'
                                                CssClass="btn btn-sm btn-warning me-2" 
                                                ToolTip="Edit">
                                                <i class="fas fa-edit"></i> Edit
                                            </asp:LinkButton>
                                            <asp:LinkButton ID="btnDelete" runat="server" 
                                                CommandName="DeleteExpense" 
                                                CommandArgument='<%# Eval("TravelExpenseID") %>'
                                                CssClass="btn btn-sm btn-danger" 
                                                ToolTip="Delete"
                                                OnClientClick='<%# "return confirmDelete(" + Eval("TravelExpenseID") + ");" %>'>
                                                <i class="fas fa-trash"></i> Delete
                                            </asp:LinkButton>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                                <HeaderStyle CssClass="table-dark-header" />
                            </asp:GridView>
                        </div>
                    </div>
                </div>
                
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>

    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <script src="https://kit.fontawesome.com/a076d05399.js" crossorigin="anonymous"></script> <!-- Ensure FontAwesome is loaded -->
    <script>
        // Use SweetAlert2 for Delete Confirmation
        function confirmDelete(id) {
            Swal.fire({
                title: 'Confirm Deletion',
                text: "This action cannot be undone. Are you sure you want to delete this travel expense record?",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#ef4444',
                cancelButtonColor: '#64748b',
                confirmButtonText: '<i class="fas fa-trash"></i> Yes, Delete It!',
                cancelButtonText: '<i class="fas fa-ban"></i> Cancel',
            }).then((result) => {
                if (result.isConfirmed) {
                    // Manually trigger the postback for the Delete button
                    const deleteButton = document.querySelector(`a[commandname="DeleteExpense"][commandargument="${id}"]`);
                    if (deleteButton) {
                        __doPostBack(deleteButton.name, '');
                    }
                }
            });
            return false;
        }

        // CORRECTED: Script to handle SweetAlert messages after postback (Called by C#)
        function showSwalMessage(title, message, icon) {
            Swal.fire({
                icon: icon,
                title: title,
                text: message,
                showConfirmButton: false,
                timer: 4000,
                toast: true,
                position: 'top-end'
            });
        }
    </script>
</asp:Content>
