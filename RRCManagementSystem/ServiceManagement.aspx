<%@ Page Title="Service Management" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="ServiceManagement.aspx.cs" Inherits="RRCManagementSystem.ServiceManagement" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        /* ============ Service Management Styles ============ */
        .service-container {
            padding: 30px;
            max-width: 1400px;
            margin: 0 auto;
        }

        .service-header {
            background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%);
            color: white;
            padding: 25px;
            border-radius: 12px;
            margin-bottom: 30px;
            box-shadow: 0 4px 12px rgba(37, 99, 235, 0.2);
        }

        .service-header h1 {
            margin: 0;
            font-size: 28px;
            font-weight: 600;
        }

        .service-header p {
            margin: 8px 0 0;
            opacity: 0.9;
            font-size: 14px;
        }

        /* ============ Add Service Section ============ */
        .add-service-card {
            background: white;
            border-radius: 12px;
            padding: 30px;
            margin-bottom: 30px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.08);
            border: 1px solid #e2e8f0;
        }

        .add-service-title {
            font-size: 20px;
            font-weight: 600;
            color: #1e293b;
            margin-bottom: 20px;
            display: flex;
            align-items: center;
            gap: 10px;
        }

        .form-grid {
            display: grid;
            grid-template-columns: repeat(2, 1fr);
            gap: 20px;
            margin-bottom: 20px;
        }

        .form-grid-full {
            grid-column: 1 / -1;
        }

        .form-group {
            margin-bottom: 0;
        }

        .form-label {
            display: block;
            font-weight: 600;
            color: #334155;
            margin-bottom: 8px;
            font-size: 14px;
        }

        .form-control {
            width: 100%;
            padding: 10px 14px;
            border: 1px solid #cbd5e1;
            border-radius: 8px;
            font-size: 14px;
            transition: all 0.3s ease;
            box-sizing: border-box;
        }

        .form-control:focus {
            outline: none;
            border-color: #2563eb;
            box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
        }

        textarea.form-control {
            resize: vertical;
            min-height: 100px;
        }

        /* ============ Custom File Upload ============ */
        .custom-file-upload {
            position: relative;
            overflow: hidden;
            display: inline-block;
            padding: 10px 20px;
            background: #f1f5f9;
            border: 2px dashed #cbd5e1;
            border-radius: 8px;
            cursor: pointer;
            transition: all 0.3s ease;
            text-align: center;
            width: 100%;
        }

        .custom-file-upload:hover {
            background: #e2e8f0;
            border-color: #2563eb;
        }

        .custom-file-upload i {
            margin-right: 8px;
            color: #2563eb;
        }

        .custom-file-upload input[type="file"] {
            position: absolute;
            left: 0;
            top: 0;
            width: 100%;
            height: 100%;
            opacity: 0;
            cursor: pointer;
            font-size: 100px;
        }

        /* ============ Service Type Radio Buttons ============ */
        .service-type-group {
            display: flex;
            flex-wrap: wrap;
            gap: 15px;
            margin-top: 8px;
        }

        .service-type-group > span {
            display: flex;
            align-items: center;
            gap: 8px;
            padding: 10px 16px;
            border: 2px solid #e2e8f0;
            border-radius: 8px;
            cursor: pointer;
            transition: all 0.3s ease;
            flex-grow: 1;
        }

        .service-type-group > span:hover {
            border-color: #2563eb;
            background: #eff6ff;
        }

        .service-type-group input[type="radio"] {
            width: 18px;
            height: 18px;
            cursor: pointer;
            accent-color: #2563eb;
        }

        .service-type-group > span.selected {
            border-color: #2563eb;
            background: #eff6ff;
        }

        .service-type-group label {
            margin: 0;
            cursor: pointer;
            font-weight: 500;
            color: #334155;
        }

        /* ============ Action Buttons ============ */
        .btn-primary-custom {
            background: linear-gradient(135deg, #2563eb, #1e40af);
            color: white;
            border: none;
            padding: 12px 24px;
            border-radius: 8px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s ease;
            display: inline-flex;
            align-items: center;
            gap: 8px;
        }

        .btn-primary-custom:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(37, 99, 235, 0.3);
        }

        .btn-secondary-custom {
            background: #f1f5f9;
            color: #475569;
            border: none;
            padding: 12px 24px;
            border-radius: 8px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s ease;
            margin-left: 10px;
        }

        .btn-secondary-custom:hover {
            background: #e2e8f0;
        }

        /* ============ Service List Section ============ */
        .service-list-card {
            background: white;
            border-radius: 12px;
            padding: 30px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.08);
            border: 1px solid #e2e8f0;
        }

        .service-list-title {
            font-size: 20px;
            font-weight: 600;
            color: #1e293b;
            margin-bottom: 20px;
            display: flex;
            align-items: center;
            gap: 10px;
        }

        /* ============ Service Type Tabs ============ */
        .service-tabs {
            display: flex;
            gap: 10px;
            margin-bottom: 20px;
            border-bottom: 2px solid #e2e8f0;
            padding-bottom: 0;
        }

        .tab-btn {
            padding: 12px 24px;
            background: transparent;
            border: none;
            border-bottom: 3px solid transparent;
            cursor: pointer;
            font-weight: 600;
            color: #64748b;
            transition: all 0.3s ease;
            position: relative;
            bottom: -2px;
        }

        .tab-btn:hover {
            color: #2563eb;
            background: #eff6ff;
            border-radius: 8px 8px 0 0;
        }

        .tab-btn.active {
            color: #2563eb;
            border-bottom-color: #2563eb;
        }

        /* ============ GridView Styles ============ */
        .service-grid {
            width: 100%;
            border-collapse: separate;
            border-spacing: 0 12px;
        }

        .service-grid th {
            background: linear-gradient(135deg, #2563eb, #1e40af);
            color: white;
            padding: 14px;
            text-align: left;
            font-weight: 600;
            font-size: 14px;
            border: none;
        }

        .service-grid th:first-child {
            border-radius: 8px 0 0 8px;
        }

        .service-grid th:last-child {
            border-radius: 0 8px 8px 0;
        }

        .service-grid tr {
            background: white;
            transition: all 0.3s ease;
        }

        .service-grid td {
            padding: 16px;
            border: 1px solid #e2e8f0;
            background: white;
            border-top: 1px solid #e2e8f0;
            border-bottom: 1px solid #e2e8f0;
            border-left: none;
            border-right: none;
        }
        
        .service-grid tr td:first-child {
            border-left: 1px solid #e2e8f0;
        }
        .service-grid tr td:last-child {
            border-right: 1px solid #e2e8f0;
        }

        .service-grid tr:hover td {
            background: #f8fafc;
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(0,0,0,0.08);
        }

        .service-grid tr td:first-child {
            border-radius: 8px 0 0 8px;
        }

        .service-grid tr td:last-child {
            border-radius: 0 8px 8px 0;
        }

        /* ============ Service Image Preview ============ */
        .service-image-preview {
            width: 80px;
            height: 80px;
            object-fit: cover;
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
        }

        /* ============ Service Type Badge ============ */
        .service-type-badge {
            display: inline-block;
            padding: 4px 12px;
            border-radius: 6px;
            font-size: 12px;
            font-weight: 600;
        }

        .badge-termite {
            background: #dbeafe;
            color: #1e40af;
        }

        .badge-pest {
            background: #dcfce7;
            color: #166534;
        }

        /* ============ Action Buttons in Grid ============ */
        .btn-grid-action {
            padding: 6px 12px;
            border-radius: 6px;
            font-size: 13px;
            border: none;
            cursor: pointer;
            transition: all 0.3s ease;
            margin: 2px;
        }

        .btn-move {
            background: #f1f5f9;
            color: #475569;
        }

        .btn-move:hover {
            background: #e2e8f0;
            transform: scale(1.1);
        }

        .btn-edit {
            background: #2563eb;
            color: white;
        }

        .btn-edit:hover {
            background: #1e40af;
        }

        .btn-delete {
            background: #ef4444;
            color: white;
        }

        .btn-delete:hover {
            background: #dc2626;
        }

        .btn-status {
            padding: 6px 14px;
            border-radius: 6px;
            font-size: 12px;
            font-weight: 600;
            border: none;
            cursor: pointer;
        }

        .btn-status.active {
            background: #10b981;
            color: white;
        }

        .btn-status.inactive {
            background: #6b7280;
            color: white;
        }

        /* ============ Empty State ============ */
        .empty-state {
            text-align: center;
            padding: 60px 20px;
            color: #64748b;
        }

        .empty-state i {
            font-size: 64px;
            margin-bottom: 20px;
            opacity: 0.5;
        }

        .empty-state h3 {
            font-size: 20px;
            margin-bottom: 10px;
        }

        .empty-state p {
            font-size: 14px;
        }

        /* ============ Edit Modal Styles ============ */
        #editModal {
            visibility: hidden;
            opacity: 0;
            transition: opacity 0.3s ease, visibility 0.3s ease;
        }
        #editModal.modal-show {
            visibility: visible;
            opacity: 1;
        }
        #modalBackdrop {
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0, 0, 0, 0.5);
            z-index: 1040;
        }
        #modalContainer {
            position: fixed;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            background: white;
            border-radius: 12px;
            box-shadow: 0 5px 15px rgba(0,0,0,0.3);
            z-index: 1050;
            width: 90%;
            max-width: 600px;
            max-height: 90vh;
            display: flex;
            flex-direction: column;
        }
        #modalHeader {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 16px 24px;
            border-bottom: 1px solid #e2e8f0;
        }
        #modalHeader h3 {
            margin: 0;
            font-size: 18px;
            font-weight: 600;
            color: #1e293b;
        }
        #modalCloseBtn {
            background: transparent;
            border: none;
            font-size: 24px;
            font-weight: 300;
            color: #64748b;
            cursor: pointer;
            padding: 0;
            line-height: 1;
        }
        #modalBody {
            padding: 24px;
            overflow-y: auto;
        }
        #modalBody .form-group {
            margin-bottom: 15px;
        }
        #modalBody .form-group .form-label {
            font-weight: 600;
        }
        #modalBody .form-control {
            width: 100%;
        }
        #modalBody .service-image-preview {
            display: block;
            margin-bottom: 10px;
        }
        #modalFooter {
            padding: 16px 24px;
            border-top: 1px solid #e2e8f0;
            background: #f8fafc;
            border-radius: 0 0 12px 12px;
            display: flex;
            justify-content: flex-end;
            gap: 10px;
        }

        /* ============ Responsive Section ============ */
        @media (max-width: 768px) {
            .form-grid {
                grid-template-columns: 1fr;
            }

            .service-tabs {
                overflow-x: auto;
            }

            .tab-btn {
                white-space: nowrap;
            }

            .service-grid {
                border-spacing: 0;
            }
            .service-grid thead {
                display: none;
            }
            .service-grid tr {
                display: block;
                margin-bottom: 20px;
                border-radius: 12px;
                box-shadow: 0 2px 8px rgba(0,0,0,0.08);
                border: 1px solid #e2e8f0;
                overflow: hidden; 
            }
            .service-grid tr:hover td {
                transform: none;
                box-shadow: none;
                background: white;
            }
            .service-grid td {
                display: block;
                padding: 12px 16px;
                border: none;
                border-bottom: 1px solid #f1f5f9;
                position: relative;
                text-align: right;
                min-height: 48px;
                padding-left: 130px;
            }
            .service-grid td:last-child {
                border-bottom: none;
            }
            .service-grid tr td:first-child, .service-grid tr td:last-child {
                border-radius: 0;
                border-left: none;
                border-right: none;
            }
            .service-grid td::before {
                content: attr(data-label);
                position: absolute;
                left: 16px;
                top: 50%;
                transform: translateY(-50%);
                font-weight: 600;
                color: #334155;
                text-align: left;
                white-space: nowrap;
            }
            
            .service-grid td[data-label="Image"],
            .service-grid td[data-label="Status"],
            .service-grid td[data-label="Actions"],
            .service-grid td[data-label="Order"] {
                text-align: center;
                padding-left: 16px;
            }

            .service-grid td[data-label="Description"] {
                text-align: left;
                padding-left: 16px;
                padding-top: 40px;
            }
             .service-grid td[data-label="Description"]::before {
                 top: 12px;
                 transform: none;
             }
             .service-grid td[data-label="Description"] div {
                 max-width: 100%;
             }
            
            .service-grid td:has(.form-control) {
                display: none;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="service-container">
        <div class="service-header">
            <h1><i class="fas fa-cogs"></i> Service Management</h1>
            <p>Add, edit, and manage your termite control and pest control services</p>
        </div>

        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>

                <div class="add-service-card">
                    <div class="add-service-title">
                        <i class="fas fa-plus-circle"></i>
                        Add New Service
                    </div>

                    <div class="form-grid">
                        
                        <div class="form-group form-grid-full">
                            <label class="form-label">
                                <i class="fas fa-tag"></i> Service Type *
                            </label>
                            <asp:RadioButtonList ID="rblServiceType" runat="server" RepeatDirection="Horizontal" 
                                CssClass="service-type-group" RepeatLayout="Flow">
                                <asp:ListItem Value="Termite Control" Selected="True">
                                    🐜 Termite Control
                                </asp:ListItem>
                                <asp:ListItem Value="General Pest Control">
                                    🐛 General Pest Control
                                </asp:ListItem>
                            </asp:RadioButtonList>
                        </div>

                        <div class="form-group">
                            <label class="form-label">
                                <i class="fas fa-heading"></i> Service Title *
                            </label>
                            <asp:TextBox ID="txtServiceTitle" runat="server" CssClass="form-control" 
                                placeholder="e.g., Baiting System" MaxLength="200" />
                        </div>

                        <div class="form-group">
                            <label class="form-label">
                                <i class="fas fa-image"></i> Service Image * (Recommended: 400x300px)
                            </label>
                            <div class="custom-file-upload">
                                <asp:FileUpload ID="fuServiceImage" runat="server" />
                                <i class="fas fa-upload"></i>
                                <span>Choose image file...</span>
                            </div>
                        </div>

                        <div class="form-group form-grid-full">
                            <label class="form-label">
                                <i class="fas fa-align-left"></i> Service Description *
                            </label>
                            <asp:TextBox ID="txtServiceDescription" runat="server" TextMode="MultiLine" 
                                Rows="4" CssClass="form-control" 
                                placeholder="Enter detailed description of the service..." />
                        </div>

                        <div class="form-group form-grid-full">
                            <label class="form-label">
                                <i class="fas fa-list-ul"></i> Key Features/Benefits (one per line)
                            </label>
                            <asp:TextBox ID="txtBulletPoints" runat="server" TextMode="MultiLine" 
                                Rows="5" CssClass="form-control" 
                                placeholder="Enter key features or benefits (one per line)&#13;&#10;Example:&#13;&#10;- Safe and eco-friendly&#13;&#10;- Long-lasting protection&#13;&#10;- Professional service" />
                        </div>
                    </div>

                    <div style="margin-top: 20px;">
                        <asp:Button ID="btnAddService" runat="server" Text="Add Service" 
                            CssClass="btn-primary-custom" OnClick="btnAddService_Click" 
                            OnClientClick="return validateServiceForm();"
                            CausesValidation="false" />
                        <asp:Button ID="btnClearForm" runat="server" Text="Clear Form" 
                            CssClass="btn-secondary-custom" OnClick="btnClearForm_Click" 
                            CausesValidation="false" />
                    </div>
                </div>

                <div class="service-list-card">
                    <div class="service-list-title">
                        <i class="fas fa-list"></i>
                        Service List
                    </div>

                    <div class="service-tabs">
                        <asp:Button ID="btnTabAll" runat="server" Text="All Services" 
                            CssClass="tab-btn active" OnClick="btnTabAll_Click" 
                            CausesValidation="false" OnClientClick="setActiveTab(this)" />
                        <asp:Button ID="btnTabTermite" runat="server" Text="🐜 Termite Control" 
                            CssClass="tab-btn" OnClick="btnTabTermite_Click" 
                            CausesValidation="false" OnClientClick="setActiveTab(this)" />
                        <asp:Button ID="btnTabPest" runat="server" Text="🐛 General Pest Control" 
                            CssClass="tab-btn" OnClick="btnTabPest_Click" 
                            CausesValidation="false" OnClientClick="setActiveTab(this)" />
                    </div>

                    <asp:GridView ID="gvServices" runat="server" AutoGenerateColumns="False" 
                        CssClass="service-grid" DataKeyNames="ServiceID"
                        OnRowCommand="gvServices_RowCommand" 
                        OnRowEditing="gvServices_RowEditing"
                        OnRowCancelingEdit="gvServices_RowCancelingEdit"
                        OnRowUpdating="gvServices_RowUpdating"
                        OnRowDeleting="gvServices_RowDeleting"
                        GridLines="None" ShowHeaderWhenEmpty="True">
                        
                        <Columns>

                            <asp:TemplateField HeaderText="Order" ItemStyle-Width="80px" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnMoveUp" runat="server" 
                                        CommandName="MoveUp" 
                                        CommandArgument='<%# Eval("ServiceID") %>'
                                        CssClass="btn-grid-action btn-move"
                                        ToolTip="Move Up"
                                        CausesValidation="false">
                                        <i class="fas fa-arrow-up"></i>
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="btnMoveDown" runat="server" 
                                        CommandName="MoveDown" 
                                        CommandArgument='<%# Eval("ServiceID") %>'
                                        CssClass="btn-grid-action btn-move"
                                        ToolTip="Move Down"
                                        CausesValidation="false">
                                        <i class="fas fa-arrow-down"></i>
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Image" ItemStyle-Width="100px" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <asp:Image ID="imgService" runat="server" 
                                        ImageUrl='<%# Eval("ImagePath") %>' 
                                        CssClass="service-image-preview" 
                                        AlternateText="Service Image" />
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:Image ID="imgServiceEdit" runat="server" 
                                        ImageUrl='<%# Eval("ImagePath") %>' 
                                        CssClass="service-image-preview" 
                                        AlternateText="Service Image" />
                                    <br />
                                    <asp:FileUpload ID="fuEditImage" runat="server" 
                                        CssClass="form-control" style="margin-top: 10px; font-size: 12px;" />
                                    <small style="color: #64748b;">Leave empty to keep current image</small>
                                </EditItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Type" ItemStyle-Width="150px">
                                <ItemTemplate>
                                    <span class='<%# Eval("ServiceType").ToString() == "Termite Control" ? "service-type-badge badge-termite" : "service-type-badge badge-pest" %>'>
                                        <%# Eval("ServiceType") %>
                                    </span>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:DropDownList ID="ddlEditServiceType" runat="server" 
                                        CssClass="form-control" SelectedValue='<%# Bind("ServiceType") %>'>
                                        <asp:ListItem Value="Termite Control">Termite Control</asp:ListItem>
                                        <asp:ListItem Value="General Pest Control">General Pest Control</asp:ListItem>
                                    </asp:DropDownList>
                                </EditItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Service Title">
                                <ItemTemplate>
                                    <strong style="color: #1e293b; font-size: 15px;">
                                        <%# Eval("ServiceTitle") %>
                                    </strong>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:TextBox ID="txtEditTitle" runat="server" 
                                        Text='<%# Bind("ServiceTitle") %>'
                                        CssClass="form-control" MaxLength="200" />
                                </EditItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Description">
                                <ItemTemplate>
                                    <div style="max-width: 300px; font-size: 13px; color: #64748b;">
                                        <%# Eval("ServiceDescription").ToString().Length > 100 
                                            ? Eval("ServiceDescription").ToString().Substring(0, 100) + "..." 
                                            : Eval("ServiceDescription") %>
                                    </div>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:TextBox ID="txtEditDescription" runat="server" 
                                        Text='<%# Bind("ServiceDescription") %>'
                                        TextMode="MultiLine" Rows="3"
                                        CssClass="form-control" />
                                </EditItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Status" ItemStyle-Width="100px" ItemStyle-HorizontalAlign="Center" Visible ="false">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnToggleStatus" runat="server"
                                        CommandName="ToggleStatus"
                                        CommandArgument='<%# Eval("ServiceID") %>'
                                        CssClass='<%# Convert.ToBoolean(Eval("IsActive")) ? "btn-status active" : "btn-status inactive" %>'
                                        CausesValidation="false">
                                        <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Actions" ItemStyle-Width="150px" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnEdit" runat="server" 
                                        CommandName="Edit"
                                        CssClass="btn-grid-action btn-edit"
                                        ToolTip="Edit Service"
                                        CausesValidation="false">
                                        <i class="fas fa-edit"></i>
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="btnDelete" runat="server" 
                                        CommandName="Delete"
                                        CommandArgument='<%# Eval("ServiceID") %>'
                                        CssClass="btn-grid-action btn-delete"
                                        OnClientClick="return confirmDelete(this);"
                                        ToolTip="Delete Service"
                                        CausesValidation="false">
                                        <i class="fas fa-trash"></i>
                                    </asp:LinkButton>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:LinkButton ID="btnUpdate" runat="server" 
                                        CommandName="Update"
                                        CssClass="btn-grid-action"
                                        style="background: #10b981; color: white;"
                                        ToolTip="Save Changes">
                                        <i class="fas fa-check"></i> Save
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="btnCancel" runat="server" 
                                        CommandName="Cancel"
                                        CssClass="btn-grid-action"
                                        style="background: #6b7280; color: white;"
                                        ToolTip="Cancel"
                                        CausesValidation="false">
                                        <i class="fas fa-times"></i> Cancel
                                    </asp:LinkButton>
                                </EditItemTemplate>
                            </asp:TemplateField>
                        </Columns>

                        <EmptyDataTemplate>
                            <div class="empty-state">
                                <i class="fas fa-box-open"></i>
                                <h3>No Services Found</h3>
                                <p>Start by adding your first service using the form above!</p>
                            </div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>

                <div id="editModal">
                    <div id="modalBackdrop"></div>
                    <div id="modalContainer">
                        <div id="modalHeader">
                            <h3><i class="fas fa-edit"></i> Edit Service</h3>
                            <button type="button" id="modalCloseBtn">&times;</button>
                        </div>
                        <div id="modalBody">
                        </div>
                        <div id="modalFooter">
                        </div>
                    </div>
                </div>

                <%-- Hidden field and button for delete confirmation --%>
                <asp:HiddenField ID="hdnDeleteServiceID" runat="server" />
                <asp:Button ID="btnHiddenDelete" runat="server" Style="display:none;" OnClick="btnHiddenDelete_Click" />

            </ContentTemplate>
            <Triggers>
                <asp:PostBackTrigger ControlID="btnAddService" />
                <asp:PostBackTrigger ControlID="gvServices" />
            </Triggers>
        </asp:UpdatePanel>
    </div>

    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <script>
        // ============ SweetAlert Validation Functions ============
        
        // Validate Add Service Form
        function validateServiceForm() {
            const title = document.getElementById('<%= txtServiceTitle.ClientID %>').value.trim();
            const description = document.getElementById('<%= txtServiceDescription.ClientID %>').value.trim();
            const fileUpload = document.getElementById('<%= fuServiceImage.ClientID %>');
            
            if (title === '') {
                Swal.fire({
                    title: 'Validation Error',
                    text: 'Service title is required!',
                    icon: 'warning',
                    confirmButtonColor: '#2563eb'
                });
                return false;
            }
            
            if (description === '') {
                Swal.fire({
                    title: 'Validation Error',
                    text: 'Service description is required!',
                    icon: 'warning',
                    confirmButtonColor: '#2563eb'
                });
                return false;
            }
            
            if (!fileUpload.files || fileUpload.files.length === 0) {
                Swal.fire({
                    title: 'Validation Error',
                    text: 'Please select an image for the service!',
                    icon: 'warning',
                    confirmButtonColor: '#2563eb'
                });
                return false;
            }
            
            // Validate file type
            const file = fileUpload.files[0];
            const allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif|\.webp)$/i;
            if (!allowedExtensions.exec(file.name)) {
                Swal.fire({
                    title: 'Invalid File Type',
                    text: 'Only image files (JPG, PNG, GIF, WEBP) are allowed!',
                    icon: 'error',
                    confirmButtonColor: '#2563eb'
                });
                return false;
            }
            
            // Validate file size (5MB)
            if (file.size > 5 * 1024 * 1024) {
                Swal.fire({
                    title: 'File Too Large',
                    text: 'Image size must be less than 5MB!',
                    icon: 'error',
                    confirmButtonColor: '#2563eb'
                });
                return false;
            }
            
            return true;
        }
        
        // Confirm Delete with SweetAlert
        function confirmDelete(deleteButton) {
            event.preventDefault();
            
            // Find the row and get ServiceID from the row's cells
            const row = deleteButton.closest('tr');
            let serviceId = null;
            
            // Try to extract from the href attribute (ASP.NET generates this)
            const href = deleteButton.href || '';
            const match = href.match(/'Delete','(\d+)'/);
            if (match && match[1]) {
                serviceId = match[1];
            }
            
            // Show confirmation dialog
            Swal.fire({
                title: 'Are you sure?',
                text: "Do you want to delete this service? This action cannot be undone!",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#ef4444',
                cancelButtonColor: '#6b7280',
                confirmButtonText: 'Yes, delete it!',
                cancelButtonText: 'Cancel'
            }).then((result) => {
                if (result.isConfirmed) {
                    if (serviceId) {
                        // Set the ServiceID in hidden field
                        document.getElementById('<%= hdnDeleteServiceID.ClientID %>').value = serviceId;
                        // Trigger the hidden delete button
                        document.getElementById('<%= btnHiddenDelete.ClientID %>').click();
                    } else {
                        // Fallback: trigger the original button's postback
                        eval(deleteButton.href);
                    }
                }
            });
            
            return false;
        }
        
        // Clear form function for callback
        function clearForm() {
            document.getElementById('<%= txtServiceTitle.ClientID %>').value = '';
            document.getElementById('<%= txtServiceDescription.ClientID %>').value = '';
            document.getElementById('<%= txtBulletPoints.ClientID %>').value = '';
            const fileSpan = document.querySelector('.custom-file-upload span');
            if (fileSpan) {
                fileSpan.innerText = 'Choose image file...';
            }
        }

        // ============ Other Functions ============

        function setActiveTab(tabButton) {
            document.querySelectorAll('.tab-btn').forEach(btn => {
                btn.classList.remove('active');
            });
            if (tabButton) {
                tabButton.classList.add('active');
            }
        }

        function applyResponsiveTableHeaders() {
            const grid = document.querySelector('.service-grid');
            if (!grid) return;

            const headers = [];
            const headerRow = grid.querySelector('thead tr') || grid.querySelector('tr');
            if (!headerRow) return;

            headerRow.querySelectorAll('th').forEach(th => {
                headers.push(th.innerText.trim());
            });

            if (headers.length === 0) return;

            grid.querySelectorAll('tbody tr').forEach(row => {
                row.querySelectorAll('td').forEach((td, index) => {
                    if (headers[index]) {
                        td.setAttribute('data-label', headers[index]);
                    }
                });
            });
        }

        function styleRadioButtons() {
            const radioGroups = document.querySelectorAll('.service-type-group');
            radioGroups.forEach(group => {
                const radios = Array.from(group.querySelectorAll('input[type="radio"]'));

                function updateSelection() {
                    radios.forEach(radio => {
                        const parentSpan = radio.closest('span');
                        if (parentSpan) {
                            if (radio.checked) {
                                parentSpan.classList.add('selected');
                            } else {
                                parentSpan.classList.remove('selected');
                            }
                        }
                    });
                }

                updateSelection();

                radios.forEach(radio => {
                    const parentSpan = radio.closest('span');
                    if (!parentSpan) return;

                    radio.addEventListener('change', updateSelection);

                    if (!parentSpan.hasAttribute('data-radio-listener')) {
                        parentSpan.addEventListener('click', (e) => {
                            if (e.target.tagName !== 'INPUT') {
                                if (!radio.checked) {
                                    radio.checked = true;
                                    updateSelection();
                                }
                            }
                        });
                        parentSpan.setAttribute('data-radio-listener', 'true');
                    }
                });
            });
        }

        function handleFileUploadUI() {
            document.querySelectorAll('.custom-file-upload input[type="file"]').forEach(input => {
                const span = input.parentElement.querySelector('span');
                if (span && !input.hasAttribute('data-file-listener')) {
                    const defaultText = span.innerText;
                    input.addEventListener('change', () => {
                        if (input.files && input.files.length > 0) {
                            span.innerText = input.files[0].name;
                        } else {
                            span.innerText = defaultText;
                        }
                    });
                    input.setAttribute('data-file-listener', 'true');
                }
            });
        }

        function handleEditModal() {
            const modal = document.getElementById('editModal');
            const modalBody = document.getElementById('modalBody');
            const modalFooter = document.getElementById('modalFooter');
            const modalCloseBtn = document.getElementById('modalCloseBtn');
            const modalBackdrop = document.getElementById('modalBackdrop');
            if (!modal) return;

            const updateButton = document.querySelector('a[id*="btnUpdate"]');

            if (updateButton) {
                const editRow = updateButton.closest('tr');
                const cancelButton = editRow.querySelector('a[id*="btnCancel"]');

                modalBody.innerHTML = '';
                modalFooter.innerHTML = '';

                editRow.querySelectorAll('td').forEach(td => {
                    const label = td.getAttribute('data-label');
                    if (!label || label === 'Order' || label === 'Status' || label === 'Actions') {
                        return;
                    }

                    const formGroup = document.createElement('div');
                    formGroup.className = 'form-group';
                    formGroup.style.marginBottom = '15px';

                    const labelEl = document.createElement('label');
                    labelEl.className = 'form-label';
                    labelEl.innerText = label;
                    formGroup.appendChild(labelEl);

                    while (td.firstChild) {
                        formGroup.appendChild(td.firstChild);
                    }

                    modalBody.appendChild(formGroup);
                });

                modalFooter.appendChild(updateButton);
                modalFooter.appendChild(cancelButton);

                editRow.style.display = 'none';

                modal.classList.add('modal-show');

                const cancelClick = (e) => {
                    e.preventDefault();
                    if (cancelButton) cancelButton.click();
                };
                modalCloseBtn.onclick = cancelClick;
                modalBackdrop.onclick = cancelClick;

            } else {
                modal.classList.remove('modal-show');
            }
        }

        function runAllClientScripts() {
            applyResponsiveTableHeaders();
            styleRadioButtons();
            handleFileUploadUI();
            handleEditModal();
        }

        document.addEventListener('DOMContentLoaded', runAllClientScripts);

        if (typeof Sys !== 'undefined') {
            try {
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(runAllClientScripts);
            } catch (e) {
                console.error("Error attaching to PageRequestManager:", e);
            }
        }
    </script>
</asp:Content>