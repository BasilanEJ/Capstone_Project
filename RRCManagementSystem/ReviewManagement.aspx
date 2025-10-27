<%@ Page Title="Review Management" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="ReviewManagement.aspx.cs" Inherits="RRCManagementSystem.ReviewManagement" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css">
    <style>
        .review-management-container {
            padding: 30px;
            background: #f8f9fa;
            min-height: 100vh;
        }

        .page-header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 30px;
            border-radius: 15px;
            margin-bottom: 30px;
            box-shadow: 0 4px 15px rgba(0,0,0,0.1);
        }

        .page-header h1 {
            margin: 0;
            font-size: 32px;
            font-weight: 700;
        }

        .page-header p {
            margin: 10px 0 0 0;
            opacity: 0.9;
        }

        .action-card {
            background: white;
            border-radius: 15px;
            padding: 25px;
            margin-bottom: 25px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.08);
        }

        .action-card h3 {
            color: #667eea;
            margin-bottom: 20px;
            font-weight: 600;
        }

        .form-label {
            font-weight: 600;
            color: #495057;
            margin-bottom: 8px;
        }

        .form-control, .form-select {
            border-radius: 8px;
            border: 2px solid #e0e0e0;
            padding: 10px 15px;
            transition: all 0.3s ease;
        }

        .form-control:focus, .form-select:focus {
            border-color: #667eea;
            box-shadow: 0 0 0 0.2rem rgba(102, 126, 234, 0.25);
        }

        .btn-custom {
            padding: 12px 30px;
            border-radius: 8px;
            font-weight: 600;
            transition: all 0.3s ease;
            border: none;
        }

        .btn-add {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
        }

        .btn-add:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
            color: white;
        }

        .btn-update {
            background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
            color: white;
        }

        .btn-update:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(240, 147, 251, 0.4);
            color: white;
        }

        .gridview-container {
            background: white;
            border-radius: 15px;
            padding: 25px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.08);
            overflow-x: auto;
        }

        .table {
            margin-bottom: 0;
        }

        .table thead {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
        }

        .table thead th {
            border: none;
            padding: 15px;
            font-weight: 600;
            text-transform: uppercase;
            font-size: 13px;
            letter-spacing: 0.5px;
        }

        .table tbody tr {
            transition: all 0.3s ease;
        }

        .table tbody tr:hover {
            background-color: #f8f9ff;
            transform: scale(1.01);
        }

        .table tbody td {
            padding: 15px;
            vertical-align: middle;
            border-bottom: 1px solid #e0e0e0;
        }

        .badge {
            padding: 6px 12px;
            border-radius: 6px;
            font-weight: 600;
            font-size: 12px;
        }

        .badge-active {
            background-color: #10b981;
            color: white;
        }

        .badge-inactive {
            background-color: #ef4444;
            color: white;
        }

        .star-rating {
            color: #fbbf24;
            font-size: 16px;
        }

        .review-text-preview {
            max-width: 300px;
            overflow: hidden;
            text-overflow: ellipsis;
            white-space: nowrap;
        }

        .action-buttons .btn {
            margin: 2px;
            padding: 6px 12px;
            font-size: 13px;
        }

        .char-counter {
            font-size: 12px;
            color: #6c757d;
            float: right;
            margin-top: 5px;
        }

        .form-check-input:checked {
            background-color: #667eea;
            border-color: #667eea;
        }

        @media (max-width: 768px) {
            .review-management-container {
                padding: 15px;
            }

            .page-header {
                padding: 20px;
            }

            .page-header h1 {
                font-size: 24px;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="review-management-container">
        
        <!-- Page Header -->
        <div class="page-header">
            <h1>🌟 Review Management</h1>
            <p>Manage customer reviews that appear on your website</p>
        </div>

        <!-- Add/Edit Review Form -->
        <div class="action-card">
            <h3>
                <asp:Label ID="lblFormTitle" runat="server" Text="📝 Add New Review"></asp:Label>
            </h3>
            
            <div class="row g-3">
                <!-- Hidden Review ID for editing -->
                <asp:HiddenField ID="hfReviewID" runat="server" />

                <!-- Customer Name -->
                <div class="col-md-6">
                    <label class="form-label">Customer Name *</label>
                    <asp:TextBox ID="txtCustomerName" runat="server" 
                        CssClass="form-control" 
                        placeholder="Enter customer name"
                        MaxLength="200" 
                        required="required"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvCustomerName" runat="server"
                        ControlToValidate="txtCustomerName"
                        ErrorMessage="Customer name is required"
                        CssClass="text-danger small"
                        Display="Dynamic"
                        ValidationGroup="ReviewForm" />
                </div>

                <!-- Rating -->
                <div class="col-md-3">
                    <label class="form-label">Rating *</label>
                    <asp:DropDownList ID="ddlRating" runat="server" CssClass="form-select">
                        <asp:ListItem Value="5" Selected="True">⭐⭐⭐⭐⭐ (5 stars)</asp:ListItem>
                        <asp:ListItem Value="4">⭐⭐⭐⭐ (4 stars)</asp:ListItem>
                        <asp:ListItem Value="3">⭐⭐⭐ (3 stars)</asp:ListItem>
                        <asp:ListItem Value="2">⭐⭐ (2 stars)</asp:ListItem>
                        <asp:ListItem Value="1">⭐ (1 star)</asp:ListItem>
                    </asp:DropDownList>
                </div>

                <!-- Display Order -->
                <div class="col-md-3">
                    <label class="form-label">Display Order</label>
                    <asp:TextBox ID="txtDisplayOrder" runat="server" 
                        CssClass="form-control" 
                        TextMode="Number"
                        Text="0"
                        placeholder="0"></asp:TextBox>
                </div>

                <!-- Review Text -->
                <div class="col-12">
                    <label class="form-label">Review Text *</label>
                    <asp:TextBox ID="txtReviewText" runat="server" 
                        CssClass="form-control" 
                        TextMode="MultiLine" 
                        Rows="4"
                        placeholder="Enter customer review..."
                        MaxLength="1000"
                        onkeyup="updateCharCount(this, 1000)"></asp:TextBox>
                    <span id="charCount" class="char-counter">0 / 1000 characters</span>
                    <asp:RequiredFieldValidator ID="rfvReviewText" runat="server"
                        ControlToValidate="txtReviewText"
                        ErrorMessage="Review text is required"
                        CssClass="text-danger small d-block"
                        Display="Dynamic"
                        ValidationGroup="ReviewForm" />
                </div>

                <!-- Checkboxes -->
                <div class="col-md-6">
                    <div class="form-check">
                        <asp:CheckBox ID="chkRecommends" runat="server" 
                            CssClass="form-check-input" 
                            Checked="true" />
                        <label class="form-check-label">
                            ❤️ Customer Recommends
                        </label>
                    </div>
                </div>

                <div class="col-md-6">
                    <div class="form-check">
                        <asp:CheckBox ID="chkIsActive" runat="server" 
                            CssClass="form-check-input" 
                            Checked="true" />
                        <label class="form-check-label">
                            ✅ Active (Display on Website)
                        </label>
                    </div>
                </div>

                <!-- Action Buttons -->
                <div class="col-12">
                    <asp:Button ID="btnAddReview" runat="server" 
                        Text="➕ Add Review" 
                        CssClass="btn btn-custom btn-add me-2" 
                        OnClick="btnAddReview_Click"
                        ValidationGroup="ReviewForm" />
                    
                    <asp:Button ID="btnUpdateReview" runat="server" 
                        Text="💾 Update Review" 
                        CssClass="btn btn-custom btn-update me-2" 
                        OnClick="btnUpdateReview_Click"
                        Visible="false"
                        ValidationGroup="ReviewForm" />
                    
                    <asp:Button ID="btnCancelEdit" runat="server" 
                        Text="❌ Cancel" 
                        CssClass="btn btn-secondary" 
                        OnClick="btnCancelEdit_Click"
                        Visible="false"
                        CausesValidation="false" />
                </div>
            </div>
        </div>

        <!-- Reviews Grid -->
        <div class="gridview-container">
            <h3 style="color: #667eea; margin-bottom: 20px;">📋 All Reviews</h3>
            
            <asp:GridView ID="gvReviews" runat="server" 
                AutoGenerateColumns="False"
                CssClass="table table-hover"
                DataKeyNames="ReviewID"
                OnRowCommand="gvReviews_RowCommand"
                OnRowDataBound="gvReviews_RowDataBound"
                EmptyDataText="No reviews found. Add your first review above!">
                
                <Columns>
                    <asp:BoundField DataField="ReviewID" HeaderText="ID" ItemStyle-Width="50px" />
                    
                    <asp:BoundField DataField="DisplayOrder" HeaderText="Order" ItemStyle-Width="70px" />
                    
                    <asp:TemplateField HeaderText="Customer Name">
                        <ItemTemplate>
                            <strong><%# Eval("CustomerName") %></strong>
                        </ItemTemplate>
                    </asp:TemplateField>
                    
                    <asp:TemplateField HeaderText="Review">
                        <ItemTemplate>
                            <div class="review-text-preview" title='<%# Eval("ReviewText") %>'>
                                <%# Eval("ReviewText") %>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                    
                    <asp:TemplateField HeaderText="Rating" ItemStyle-Width="120px">
                        <ItemTemplate>
                            <span class="star-rating">
                                <%# GetStarRating(Convert.ToInt32(Eval("Rating"))) %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    
                    <asp:TemplateField HeaderText="Recommends" ItemStyle-Width="100px">
                        <ItemTemplate>
                            <%# Convert.ToBoolean(Eval("Recommends")) ? "❤️ Yes" : "No" %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    
                    <asp:TemplateField HeaderText="Status" ItemStyle-Width="90px">
                        <ItemTemplate>
                            <span class='<%# Convert.ToBoolean(Eval("IsActive")) ? "badge badge-active" : "badge badge-inactive" %>'>
                                <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    
                    <asp:TemplateField HeaderText="Actions" ItemStyle-Width="200px">
                        <ItemTemplate>
                            <div class="action-buttons">
                                <asp:Button ID="btnEdit" runat="server" 
                                    Text="✏️ Edit" 
                                    CssClass="btn btn-sm btn-primary" 
                                    CommandName="EditReview" 
                                    CommandArgument='<%# Eval("ReviewID") %>'
                                    CausesValidation="false" />
                                
                                <asp:Button ID="btnToggle" runat="server" 
                                    Text='<%# Convert.ToBoolean(Eval("IsActive")) ? "🔴 Deactivate" : "🟢 Activate" %>' 
                                    CssClass='<%# Convert.ToBoolean(Eval("IsActive")) ? "btn btn-sm btn-warning" : "btn btn-sm btn-success" %>' 
                                    CommandName="ToggleStatus" 
                                    CommandArgument='<%# Eval("ReviewID") %>'
                                    CausesValidation="false" />
                                
                                <asp:Button ID="btnDelete" runat="server" 
                                    Text="🗑️ Delete" 
                                    CssClass="btn btn-sm btn-danger" 
                                    CommandName="DeleteReview" 
                                    CommandArgument='<%# Eval("ReviewID") %>'
                                    OnClientClick="return confirm('Are you sure you want to delete this review?');"
                                    CausesValidation="false" />
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

    </div>

    <!-- SweetAlert2 -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    
    <script>
        // Character counter for textarea
        function updateCharCount(textarea, maxLength) {
            const currentLength = textarea.value.length;
            const counter = document.getElementById('charCount');
            counter.textContent = currentLength + ' / ' + maxLength + ' characters';
            
            if (currentLength >= maxLength) {
                counter.style.color = '#ef4444';
            } else if (currentLength >= maxLength * 0.9) {
                counter.style.color = '#f59e0b';
            } else {
                counter.style.color = '#6c757d';
            }
        }

        // Initialize character count on page load
        window.addEventListener('load', function() {
            const textarea = document.getElementById('<%= txtReviewText.ClientID %>');
            if (textarea) {
                updateCharCount(textarea, 1000);
            }
        });
    </script>
</asp:Content>
